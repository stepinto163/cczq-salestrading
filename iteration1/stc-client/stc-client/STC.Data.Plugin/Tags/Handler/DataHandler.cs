using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.Utils.SqlLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Tags.Handler
{
    class DataHandler
    {
        private const int PageSize = 2000;
        private volatile bool IsStart = true;
        private volatile int IsSearching = 0;  // 0: not searching, 1: searching
        private volatile int isLoadCrm = 0;     // 0: not loading, 1: loading

        private static DataHandler brokerHandler = new DataHandler();
        public static DataHandler Instance { get { return brokerHandler; } }

        public Action OnIssuerComplate;

        public Action OnGuarantorComplate;

        private readonly ReaderWriterLockSlim issuerLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim guarantorLock = new ReaderWriterLockSlim();

        // 追踪后台任务
        private volatile int activeDatabaseTasks = 0;

        /// <summary>
        /// 优雅关闭 DataHandler，释放所有资源
        /// 等待所有后台数据库操作完成，然后才释放资源
        /// </summary>
        public void Stop()
        {
            try
            {
                //LogHelper.Info("DataHandler.Stop() called, shutting down gracefully");
                
                // 1. 设置停止标志，让后台线程退出循环
                IsStart = false;
                
                // 2. 等待所有正在进行的数据库操作完成（最多等待 10 秒）
                int waitCount = 0;
                while (activeDatabaseTasks > 0 && waitCount < 100)  // 100 * 100ms = 10 秒
                {
                    System.Threading.Thread.Sleep(100);
                    waitCount++;
                }
                
                if (activeDatabaseTasks > 0)
                {
                    //LogHelper.Warn(string.Format("DataHandler.Stop() timeout waiting for {0} database tasks to complete", activeDatabaseTasks));
                }
                else
                {
                    //LogHelper.Info("All database tasks completed successfully");
                }
                
                // 3. 重置状态标志
                IsSearching = 0;
                isLoadCrm = 0;
                
                // 4. 现在才释放读写锁资源（此时没有任务在使用）
                if (issuerLock != null)
                {
                    try
                    {
                        issuerLock.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("Error disposing issuerLock: " + ex.Message);
                    }
                }
                
                if (guarantorLock != null)
                {
                    try
                    {
                        guarantorLock.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("Error disposing guarantorLock: " + ex.Message);
                    }
                }
                
                LogHelper.Info("DataHandler.Stop() completed successfully");
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in DataHandler.Stop():", ex);
            }
        }

        private static List<ChildTag> ListChildTag = new List<ChildTag>();
        private static List<ChildTag> ListGuarantor = new List<ChildTag>();

        private DataHandler()
        {
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    // 启动写日志
                    WriteDBLogHelper.Instance.Init();

                    InitGuarantorData();

                    if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                    {
                        InitIssuerData();
                    }

                    // 每2小时执行一次搜索（120分钟）
                    const int searchIntervalMs = 120 * 60 * 1000;
                    while (IsStart && LocalEnv.ConfInfo.CanGoodsQuickSearch)
                    {
                        try
                        {
                            SearchGuarantors();

                            if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                            {
                                SearchIssuers();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error("Error during search cycle:", ex);
                        }

                        Thread.Sleep(searchIntervalMs);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("Fatal error in DataHandler.Start:", e);
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            });
        }

        #region contact 交易员


        private void InitIssuerData()
        {
            try
            {

                BaseHandler.Instance.SetLoadDataStatus_Issuer(true);
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }

        private void SearchIssuers()
        {
            // 使用原子操作替代简单的标志位检查
            if (Interlocked.CompareExchange(ref isLoadCrm, 1, 0) != 0)
                return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    SyncIssuer1();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Search Issuer failed!", ex);
                }
                finally
                {
                    Interlocked.Exchange(ref isLoadCrm, 0);
                }
            });
        }

        public void SyncIssuer()
        {
            if (Interlocked.CompareExchange(ref isLoadCrm, 1, 0) != 0)
                return;

            try
            {
                SyncIssuer1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Sync Issuer failed!", ex);
            }
            finally
            {
                Interlocked.Exchange(ref isLoadCrm, 0);
            }
        }

        /// <summary>
        /// 每日同步发行人
        /// </summary>
        public void SyncIssuer1()
        {
            QueryTagsCountRequest requestCount = new QueryTagsCountRequest();
            requestCount.typeCode = "ISSUER";
            requestCount.maxTagValue = "0";

            var reply = BaseHandler.Instance.QueryTagsCount(requestCount);

            int maxCount = 0;
            if (reply != null && reply.IsSuccess())
            {
                maxCount = reply.Datas.ConvertToInt();
            }

            if (maxCount > 0 && maxCount - TagsSqliteManager.Instance.GetIssuersCount() >= 10)
            {
                int pageNum = (maxCount + PageSize - 1) / PageSize;  // 更高效的分页计算
                LogHelper.Info($"Start Sync Issuer...  reply count: {maxCount}");
                
                try
                {
                    var newIssuerList = LoadIssuerDataInParallel(pageNum);
                    
                    issuerLock.EnterWriteLock();
                    try
                    {
                        ListChildTag = newIssuerList;
                    }
                    finally
                    {
                        issuerLock.ExitWriteLock();
                    }

                    LogHelper.Info("End Sync Issuer !");
                    BaseHandler.Instance.SetLoadDataStatus_Issuer(true);
                    WriteLocalDb_Issuer();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Error syncing issuers:", ex);
                }
            }
        }

        /// <summary>
        /// 并行加载发行人数据
        /// </summary>
        private List<ChildTag> LoadIssuerDataInParallel(int pageNum)
        {
            var issuerBag = new ConcurrentBag<ChildTag>();
            
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, pageNum, parallelOptions, i =>
            {
                try
                {
                    QueryTagsLoadRequest request = new QueryTagsLoadRequest();
                    request.typeCode = "ISSUER";
                    request.maxTagValue = "0";
                    request.pageNum = i + 1;
                    request.pageSize = PageSize;

                    var reply = BaseHandler.Instance.QueryTagsLoad(request);
                    if (reply?.Datas != null && reply.Datas.Count > 0 && reply.Datas[0].childTags?.Count > 0)
                    {
                        foreach (var tag in reply.Datas[0].childTags)
                        {
                            issuerBag.Add(tag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Error loading issuer page {i + 1}:", ex);
                }
            });

            return issuerBag.ToList();
        }

        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb_Issuer()
        {
            Task.Factory.StartNew(() =>
            {
                // 增加活跃任务计数
                Interlocked.Increment(ref activeDatabaseTasks);
                
                try
                {
                    issuerLock.EnterReadLock();
                    try
                    {
                        TagsSqliteManager.Instance.DeleteContact();
                        LogHelper.Info("Start Write Issuer db: db count:" + ListChildTag.Count);
                        TagsSqliteManager.Instance.InsertIssuer(ListChildTag);
                        LogHelper.Info("Sync Issuer db succeed!");
                        // 数据写入成功后，清空缓存，使新数据立即可用
                        TagsSqliteManager.Instance.ClearIssuerCache();
                    }
                    finally
                    {
                        issuerLock.ExitReadLock();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync Issuer db exception.");
                    LogHelper.Error(ex);
                }
                finally
                {
                    // 减少活跃任务计数
                    Interlocked.Decrement(ref activeDatabaseTasks);
                }
            });
        }

       
        #endregion

        #region GUARANTOR 担保人

        private void InitGuarantorData()
        {
            try
            {
                BaseHandler.Instance.SetLoadDataStatus_Guarantor(true);
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }


        private void SearchGuarantors()
        {
            if (Interlocked.CompareExchange(ref IsSearching, 1, 0) != 0)
                return;

            Task.Factory.StartNew(() => 
            { 
                try
                {
                    SyncGuarantors1();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Search Guarantor failed!", ex);
                }
                finally
                {
                    Interlocked.Exchange(ref IsSearching, 0);
                }
            });
        }

        public void SyncGuarantors()
        {
            if (Interlocked.CompareExchange(ref IsSearching, 1, 0) != 0)
                return;

            try
            {
                SyncGuarantors1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Sync Guarantor failed!", ex);
            }
            finally
            {
                Interlocked.Exchange(ref IsSearching, 0);
            }
        }

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncGuarantors1()
        {
            QueryTagsCountRequest requestCount = new QueryTagsCountRequest();
            requestCount.typeCode = "GUARANTOR";
            requestCount.maxTagValue = "0";

            var reply = BaseHandler.Instance.QueryTagsCount(requestCount);
            int maxCount = 0;
            if (reply != null)
            {
                maxCount = reply.Datas.ConvertToInt();
            }

            if (maxCount > 0 && maxCount - TagsSqliteManager.Instance.GetGuarantorsCount() >= 10)
            {
                int pageNum = (maxCount + PageSize - 1) / PageSize;  // 更高效的分页计算
                LogHelper.Info($"Start Sync Guarantors...  reply count: {maxCount}");

                try
                {
                    var newGuarantorList = LoadGuarantorDataInParallel(pageNum);

                    guarantorLock.EnterWriteLock();
                    try
                    {
                        ListGuarantor = newGuarantorList;
                    }
                    finally
                    {
                        guarantorLock.ExitWriteLock();
                    }

                    LogHelper.Info("End Sync Guarantors !");
                    BaseHandler.Instance.SetLoadDataStatus_Guarantor(true);
                    WriteLocalDb_Guarantor();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Error syncing guarantors:", ex);
                }
            }
        }

        /// <summary>
        /// 并行加载担保人数据
        /// </summary>
        private List<ChildTag> LoadGuarantorDataInParallel(int pageNum)
        {
            var guarantorBag = new ConcurrentBag<ChildTag>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, pageNum, parallelOptions, i =>
            {
                try
                {
                    QueryTagsLoadRequest request = new QueryTagsLoadRequest();
                    request.typeCode = "GUARANTOR";
                    request.maxTagValue = "0";
                    request.pageNum = i + 1;
                    request.pageSize = PageSize;

                    var reply = BaseHandler.Instance.QueryTagsLoad(request);
                    if (reply?.Datas != null && reply.Datas.Count > 0 && reply.Datas[0].childTags?.Count > 0)
                    {
                        foreach (var tag in reply.Datas[0].childTags)
                        {
                            guarantorBag.Add(tag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Error loading guarantor page {i + 1}:", ex);
                }
            });

            return guarantorBag.ToList();
        }

        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb_Guarantor()
        {
            Task.Factory.StartNew(() =>
            {
                // 增加活跃任务计数
                Interlocked.Increment(ref activeDatabaseTasks);
                
                try
                {
                    guarantorLock.EnterReadLock();
                    try
                    {
                        TagsSqliteManager.Instance.DeleteGuarantor();
                        LogHelper.Info("Start sync Guarantors db: db count:" + ListGuarantor.Count);
                        TagsSqliteManager.Instance.InsertGuarantor(ListGuarantor);
                        LogHelper.Info("Sync Guarantors db succeed!");
                        // 数据写入成功后，清空缓存，使新数据立即可用
                        TagsSqliteManager.Instance.ClearGuarantorCache();
                    }
                    finally
                    {
                        guarantorLock.ExitReadLock();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync Guarantors db exception.");
                    LogHelper.Error(ex);
                }
                finally
                {
                    // 减少活跃任务计数
                    Interlocked.Decrement(ref activeDatabaseTasks);
                }
            });
        }
        #endregion

    }
}
