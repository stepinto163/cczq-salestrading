using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.OHLC;
using STC.Common.Model.Admin;
using STC.Common.Model.Chart;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.OHLC;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    /// <summary>
    /// 内部成交
    /// </summary>
    public class ExecutionsHandler
    {
        // 清零
        private readonly Object m_OnClearEventLock = new Object();
        private Action<Common.Model.Base.ClearPo> _OnClear;
        public event Action<Common.Model.Base.ClearPo> OnClear
        {
            add
            {
                lock (m_OnClearEventLock)
                {
                    if (_OnClear == null || !_OnClear.GetInvocationList().Contains(value))
                        _OnClear += value;
                }
            }
            remove
            {
                lock (m_OnClearEventLock)
                {
                    _OnClear -= value;
                }
            }
        }

        // 订阅我的成交
        private readonly Object m_OnOwnExecutionsEventLock = new Object();
        private Action<ExecutionReport> _OnOwnExecutions;
        /// <summary>
        /// 我的成交
        /// </summary>
        public event Action<ExecutionReport> OnOwnExecutions
        {
            add
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    if (_OnOwnExecutions == null || !_OnOwnExecutions.GetInvocationList().Contains(value))
                        _OnOwnExecutions += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheOwnExecutions)
                    {
                        _OnOwnExecutions?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    _OnOwnExecutions -= value;
                }
            }
        }

        public event Action<ExecutionReport> OnOwnExecutionsOnlyNew
        {
            add
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    if (_OnOwnExecutions == null || !_OnOwnExecutions.GetInvocationList().Contains(value))
                        _OnOwnExecutions += value;
                }
            }
            remove
            {
                lock (m_OnOwnExecutionsEventLock)
                {
                    _OnOwnExecutions -= value;
                }
            }
        }


        // 订阅市场成交
        private readonly Object m_OnMarketExecutionsEventLock = new Object();
        private Action<ExecutionReport> _OnMarketExecutions;

        /// <summary>
        /// 最新成交
        /// </summary>
        public event Action<ExecutionReport> OnMarketExecutions
        {
            add
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions += value;
                }

                ////System.Threading.ThreadPool.QueueUserWorkItem(o =>
                ////{

                ////    //List<ExecutionReport> tmp = _CacheMarketExecutions.Values.OrderBy(p => p.UpdateTime).ToList();
                ////    List<ExecutionReport> tmp = STC.Common.Data.STCRocksDbHandler.Instance.GetAllCache(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions)?.OrderBy(p => p.UpdateTime).ToList();
                ////    foreach (var obj in tmp)
                ////    {
                ////        _OnMarketExecutions?.Invoke(obj);
                ////    }
                ////});
            }
            remove
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions -= value;
                }
            }
        }

        public event Action<ExecutionReport> OnMarketExecutionsOnlyNew
        {
            add
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions += value;
                }
            }
            remove
            {
                lock (m_OnMarketExecutionsEventLock)
                {
                    _OnMarketExecutions -= value;
                }
            }
        }

        /// <summary>
        /// 缓存我的成交原始报文, Key = OrderID
        /// </summary>
        public ConcurrentDictionary<string, ExecutionReport> _CacheOwnExecutions = new ConcurrentDictionary<string, ExecutionReport>();


        //最后更新时间
        private DateTime LastUpdateTime;

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


        /// <summary>
        /// 最新的队列池
        /// </summary>
        private static BlockingCollection<Common.Model.Fix.Message.ExecutionReport> UpdateQueue = new BlockingCollection<Common.Model.Fix.Message.ExecutionReport>();


        /// <summary>
        /// 获得securityId的本地成交含市场成交原始数据
        /// </summary>
        /// <param name="securityId">债券代码</param>
        /// <returns>市场成交数据</returns>
        public List<ExecutionReport> GetLocalExecutionsBySecurityID(string securityId, bool isContainsOwn = true, int maxcount = -1)
        {
            if (!string.IsNullOrEmpty(securityId))
            {
                List<ExecutionReport> temp = STC.Common.Data.STCDbHandler.Instance.GetAllCacheByKey(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, securityId, false, maxcount);

                if (isContainsOwn)
                {
                    List<ExecutionReport> owntemp = _CacheOwnExecutions.Values.Where(p => p.SecurityID == securityId).ToList();
                    if (temp == null)
                        temp = new List<ExecutionReport>();
                    if (owntemp == null)
                        owntemp = new List<ExecutionReport>();

                    List<ExecutionReport> newunion = temp.Union(owntemp).ToList();
                    if (newunion.Count > 0)
                    {
                        return new List<ExecutionReport>(newunion.OrderBy(p => p.UpdateTime).ToList());
                    }
                }
                else
                {
                    return temp.OrderBy(p => p.UpdateTime).ToList();
                }
            }
            else
            {
                return new List<ExecutionReport>();
            }

            return new List<ExecutionReport>();
        }

        /// <summary>
        /// 获得securityID获取最后的有效成交价,从市场成交和内部成交中获取
        /// </summary>
        /// <param name="securityId">债券代码</param>
        /// <returns>市场成交数据</returns>
        public List<ExecutionReport> GetLastExecutionBySecurityID(string securityId)
        {
            if (!string.IsNullOrEmpty(securityId))
            {

                List<ExecutionReport> temp = STC.Common.Data.STCDbHandler.Instance.GetAllCacheByKey(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, securityId, false, -1);

                List<ExecutionReport> owntemp = _CacheOwnExecutions.Values.Where(p => p.SecurityID == securityId).ToList();
                if (temp == null)
                    temp = new List<ExecutionReport>();
                if (owntemp == null)
                    owntemp = new List<ExecutionReport>();

                List<ExecutionReport> newunion = temp.Union(owntemp).OrderByDescending(p => p.TransactTime).ThenByDescending(p => p.UpdateTime).ToList();
                if (newunion.Count > 0)
                {
                    return newunion;
                }
            }
            else
            {
                return new List<ExecutionReport>();
            }

            return new List<ExecutionReport>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private ExecutionsHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

        }

        private object initlock = new object();

        private volatile bool isInit = false;

        private object lockObj = new object();

        private bool IsClear = false;

        public bool EndInit()
        {
            return isInit;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (CommonUtils.IsBDBAppName())
            {
                return;
            }

            lock (initlock)
            {
                if (isInit == true)
                {
                    return;
                }

                isInit = true;
            }

            //内部成交
            if (!IsClear)
            {
                RequestOwnExecutions();
            }

            //如果是Clear信号
            if (IsClear)
            {
                IsClear = false;
                Thread thread = new Thread(() =>
                {
                    int retrytime = 0;
                    //等待RocksDb创建好
                    if (STCDbHandler.Instance.IsNewDay())
                    {
                        while (STCDbHandler.IsBbusy && retrytime < 30)
                        {
                            Task.Delay(1000);
                            retrytime++;
                        }
                    }

                    RequestOwnExecutions();

                    string lastres = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, "LastUpdateTime");
                    //一次性通知所有程序最后成交
                    ProcDbLastExecution();

                    if (!string.IsNullOrEmpty(lastres))
                    {
                        //局部刷新
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(lastres);

                        if (!string.IsNullOrEmpty(lastres))
                        {
                            //2. 查询处理更新记录
                            LogHelper.Info($"查询市场成交最新更新....");
                            RequestMarketExecutionsSortExecid(orgdatetime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }
                    }
                    else
                    {
                        //全部刷新
                        RequestMarketExecutions();
                    }
                });

                thread.Priority = ThreadPriority.Normal; // 设置线程的优先级为最高
                thread.Start();
            }
            else
            {
                string lastres = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, "LastUpdateTime");

                //首先启动处理订阅市场成交的线程
                Thread thread = new Thread(() =>
                {
                    //处理市场成交数据
                    DoProcMeQueue(cancellationTokenSource.Token);
                });

                thread.Priority = ThreadPriority.Normal; // 设置线程的优先级为最高
                thread.Start();

                Task.Factory.StartNew(() =>
                {
                    //订阅最新市场成交
                    MarketExecHandler.Instance.Init(Instance_OnExecOrder);

                    //一次性通知所有程序最后成交
                    ProcDbLastExecution();

                    if (!string.IsNullOrEmpty(lastres))
                    {
                        //局部刷新
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(lastres);

                        LogHelper.Info($"{LocalEnv.ConfInfo.LocalTableType}'s ExecutionReport Records are approximately equal to :{STC.Common.Data.STCDbHandler.Instance.GetRecordCount(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions)}");
                        //LogHelper.Info($"{LocalEnv.ConfInfo.LocalTableType}'s MarketExecution Records are approximately equal to :{STC.Common.Data.STCDbHandler.Instance.GetRecordCount(STC.Common.Data.STCDbHandler.DbMarketExecution)}");
                        LogHelper.Info($"{LocalEnv.ConfInfo.LocalTableType}'s LastExecution Records are approximately equal to :{STC.Common.Data.STCDbHandler.Instance.GetRecordCount(STC.Common.Data.STCDbHandler.DbLastExecution)}");
                        LogHelper.Info($"{LocalEnv.ConfInfo.LocalTableType} ExecutionReport LastUpdateTime is:{lastres}");


                        if (!string.IsNullOrEmpty(lastres))
                        {
                            //2. 查询处理更新记录
                            LogHelper.Info($"查询市场成交最新更新....");
                            RequestMarketExecutionsSortExecid(orgdatetime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }
                    }
                    else
                    {
                        //全部刷新
                        RequestMarketExecutions();
                    }
                });


                //处理查询的历史市场成交
                Task.Factory.StartNew(() => DoProcHistoryMarketExecutionsQueue(cancelhistorylationTokenSource.Token), TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
            }
        }

        #region 废弃 最大线程处理任务

        private SemaphoreSlim _semaphore; // 控制最大线程数
        private int _maxThreads = 100;
        private List<Task> _processingTasks; // 数据处理任务
        private CancellationTokenSource _cancellationTokenSource; // 用于取消任务

        public void ProcDbLastExecution()
        {
            LogHelper.Info($"Prepare to load last transaction from {LocalEnv.ConfInfo.LocalTableType}'s data...");
            _processingTasks = new List<Task>();
            _semaphore = new SemaphoreSlim(_maxThreads);
            _cancellationTokenSource = new CancellationTokenSource();

            List<ExecutionReport> tmp = STC.Common.Data.STCDbHandler.Instance.GetAllCache(STC.Common.Data.STCDbHandler.DbLastExecution)?.OrderBy(p => p.UpdateTime).ToList();
            if (tmp != null && tmp.Count > 0)
            {
                LogHelper.Info($"{LocalEnv.ConfInfo.LocalTableType}'s Last transaction data count:{tmp.Count}");
                StartProcessing(tmp);
            }

            tmp?.Clear();
        }

        // 启动数据处理
        private void StartProcessing(List<ExecutionReport> tmplist)
        {
            for (int i = 0; i < tmplist.Count; i++) //有数据数据需要处理
            {
                int dataIndex = i; // 防止闭包问题

                string primaryKey = netapi.Api.ApiManager.Instance.GetDefaultApi().getTopic(Common.Api.ApiConsts.Topic_stc_md_prefix, ApiConsts.CT,LocalEnv.UserInfo.Termids, tmplist[dataIndex].SecurityID, string.Empty, string.Empty);
                if (!STC.Common.Utils.PriceHandler.Instance._UpdateCTCallbackDic.ContainsKey(primaryKey))
                {
                    continue;
                }

                _processingTasks.Add(Task.Run(async () =>
                {
                    await _semaphore.WaitAsync(); // 控制最大并发线程数量
                    try
                    {
                        await ProcessDataTask(dataIndex, tmplist[dataIndex]);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }));
            }

            // 等待所有数据处理任务完成后取消发送数据任务
            Task.WhenAll(_processingTasks).ContinueWith(t =>
            {
                _cancellationTokenSource.Cancel(); // 取消数据任务
                LogHelper.Info($"Processing of the last transaction from {LocalEnv.ConfInfo.LocalTableType}'s data completed");
            });
        }

        // 模拟数据处理任务
        private async Task ProcessDataTask(int index, ExecutionReport obj)
        {
            //更新最新成交字典
            //Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj);
            Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(obj);
            await Task.Delay(1);
        }

        #endregion


        /// <summary>
        /// 处理市场成交数据
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcMeQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Common.Model.Fix.Message.ExecutionReport obj = null;
                   
                    obj = UpdateQueue.Take(cancellationToken);

                    //更新最新时间
                    if (obj != null && IsEndProcHistoryMarketExecutions)
                        UpdateLastUpdateTime(obj.UpdateTime);

                    try
                    {
                        if (obj != null)
                        {
                            //首先保存原始报文到RocksDb
                            STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, $"{obj.SecurityID}#{obj.ExecID}", obj);

                            //内部成交
                            if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSF.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZF.ToLower())))
                            {
                                lock (string.Intern(obj.OrderID))
                                {
                                    _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                }

                                _OnOwnExecutions?.Invoke(obj);
                            }

                            if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                            {
                                continue;
                            }

                            //市场成交数据
                            UpdateMarketExecutions(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (lockObj)
                {
                    string key = obj.OrderID;
                    if (obj.VenueTypeGW.ToUpper().Equals(Common.Api.ApiConsts.CDH.ToUpper()))
                    {
                        key = obj.ExecID;
                    }

                    string tmp = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}");

                    if (string.IsNullOrEmpty(tmp))
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            return;
                        }

                        // 添加新数据
                        //AddRecord(obj);

                        MarketExecutionsItem marketExe = new MarketExecutionsItem();
                        Common.DataProcesser.LastExecutionProcesser.Processer.ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
                        STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{marketExe.SecurityID}#{key}", marketExe);

                        //更新最后成交字典  由 STC.XBond.Plugin MarketExecutionProcesser 处理
                        //Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj);
                    }
                    else
                    {
                        //查找 "TimeStamp" 的位置
                        string[] parts = tmp.Split(new string[] { "\"TimeStamp\":" }, StringSplitOptions.None);

                        if (parts.Length > 1)
                        {
                            string transactionTime = parts[1].Split(',')[0];
                            DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(transactionTime.Replace("\"", ""));
                            DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.UpdateTime);
                            if (DateTime.Compare(curUpdateTime, orgdatetime) < 0)
                            {
                                return;
                            }

                            if (obj.OrdStatus == "4")
                            {
                                //删除原始ER报文和处理后的市场成交
                                STCDbHandler.Instance.Delete(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, $"{obj.SecurityID}#{key}");

                                //更新最新成交字典，获取上一个有效的成交作为最后成交  由 STC.XBond.Plugin MarketExecutionProcesser 处理
                                //Common.DataProcesser.LastExecutionProcesser.Processer.DeleteLastERAndGetPrevER(obj);

                                string srcmarket = string.Empty;

                                // 删除
                                //DeleteRecord(obj);

                                srcmarket = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}");
                                if (!string.IsNullOrEmpty(srcmarket))
                                    STCDbHandler.Instance.Delete(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}");

                                return;
                            }

                            // 更新数据
                            //UpdateRecord(obj);

                            //更新记录
                            MarketExecutionsItem marketExe2 = new MarketExecutionsItem();
                            Common.DataProcesser.LastExecutionProcesser.Processer.ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe2);
                            STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}", marketExe2);

                            //更新最后成交字典 由 STC.XBond.Plugin MarketExecutionProcesser 处理
                            //Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }


        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("ExecutionsHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                //_CacheMarketExecutions.Clear();
                _CacheOwnExecutions.Clear();
                _OnClear?.Invoke(obj);
                isInit = false;

                //MyExecutionProcesser.Processer.ResetClear();
                //LastExecutionProcesser.Processer.ResetClear();

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        IsClear = true;
                        init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("RequestOrders", e);
                    }
                });
            }
        }

        /// <summary>
        /// 实时成交数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        public void Instance_OnExecOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj == null)
            {
                // 可能会有ems.c的消息
                LogHandler.Info("Instance_OnExecOrder obj == null");
                return;
            }

            ////更新最新时间
            //if (obj != null)
            //    UpdateLastUpdateTime(obj.UpdateTime);

            //内部成交
            if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSF.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZF.ToLower())))
            {
                lock (string.Intern(obj.OrderID))
                {
                    _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                }

                _OnOwnExecutions?.Invoke(obj);
            }

            //插入待处理的队列
            UpdateQueue.Add(obj);

            _OnMarketExecutions?.Invoke(obj);
        }

        /// <summary>
        /// ExecutionsHandler实例
        /// </summary>
        private static ExecutionsHandler server = new ExecutionsHandler();
        public static ExecutionsHandler Instance { get { return server; } }

        /// <summary>
        /// 请求当日历史成交数据（所有本方ems成交数据 + cdh数据）
        /// </summary>
        public void RequestMarketExecutions()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    //request.securityid = "180210.IB";
                    //request.venuetypegw = "CDH,CMDS,SDN,INNER";
                    request.venues = "CCTB,CNEX,PTCN,CTTJ,TPSC,UEDA,ODM,QDMESP,MANUAL,SSE,SSF,SZE,SZF";
                    //request.venues = "SSEMD,SSFMD,SZEMD";
                    //request.venuetypegw = "CMDS,INNER";
                    //request.venues = "QDMESP,MANUAL";
                    //request.venuetypegw = "CDH";
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    int index = 0;
                    request.pageSize = 3000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        
                        ///获取成交
                        var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            LogHelper.Info($"没有查询到历史市场成交记录 !");
                            IsEndProcHistoryMarketExecutions = true;
                            //发布消息
                            Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            if (!IsStartProcHistoryMarketExecutions)
                            {
                                IsStartProcHistoryMarketExecutions = true;
                                LogHelper.Info($"开始处理历史市场成交记录 ! 记录总数:{reply.totalSize}");
                                if (reply.totalSize == 0)
                                    Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);
                            }

                            LogHelper.Info($"正在处理历史市场成交记录 ! 当前页码:{index} 。");

                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                            {
                                //市场成交
                                HistoryQueue.Add(obj);

                                #region 废弃
                                //STC.Common.Data.STCRocksDbHandler.Instance.Put(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions, $"{obj.SecurityID}#{obj.UpdateTime}", CommonUtils.SerializeObject(obj));
                                //STC.Common.Data.STCRocksDbHandler.Instance.Put(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions, obj.ExecID, CommonUtils.SerializeObject(obj));


                                //////内部成交
                                ////if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSF.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZF.ToLower())))
                                ////{
                                ////    lock (string.Intern(obj.OrderID))
                                ////    {
                                ////        _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                ////    }

                                ////    _OnOwnExecutions?.Invoke(obj);
                                ////}


                                ////if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                                ////{
                                ////    continue;
                                ////}

                                //市场成交
                                ////更新最新时间
                                //UpdateLastUpdateTime(obj.UpdateTime);

                                #endregion
                            }
                            if (reply.datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }

                        reply?.datas.Clear();
                    }

                    IsLastPage = true;
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
                finally
                {

                }
            });
        }


        /// <summary>
        /// 请求当日历史成交数据是时间点前或后的数据
        /// </summary>
        public void RequestMarketExecutionsSortExecid(string lastupdtime)
        {
            List<ExecutionReport> res = new List<ExecutionReport>();
            try
            {
                QueryOrderReq request = new QueryOrderReq();
                string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                request.dealTimeBegin = today;
                request.dealTimeEnd = today;
                //request.securityid = "180210.IB";
                //request.venuetypegw = "CDH,CMDS,SDN,INNER";
                request.venues = "CCTB,CNEX,PTCN,CTTJ,TPSC,UEDA,ODM,QDMESP,MANUAL,SSE,SSF,SZE,SZF";
                //request.venues = "SSEMD,SSFMD,SZEMD";
                //request.venuetypegw = "CMDS,INNER";
                //request.venues = "QDMESP,MANUAL";
                //request.venuetypegw = "CDH";
                //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                request.orderStatus = "1,2";
                request.transactTime = lastupdtime;
                request.sortRule = "1";
                int index = 0;
                request.pageSize = 2000;
                request.pageNum = index;

                bool flag = true;
                while (flag)
                {
                    request.pageNum = index;
                    TBookOrder reply = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_execOrderQuery, request);
                    if (reply == null || reply.datas == null)
                    {
                        flag = false;
                        LogHelper.Info($"没有查询到历史市场成交记录 !");
                        IsEndProcHistoryMarketExecutions = true;
                        //发布消息
                        Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);
                        break;
                    }
                    ///获取
                    if (reply != null && reply.datas != null)
                    {
                        if (!IsStartProcHistoryMarketExecutions)
                        {
                            IsStartProcHistoryMarketExecutions = true;
                            LogHelper.Info($"开始处理历史市场成交记录 ! 记录总数:{reply.totalSize}");
                            if (reply.totalSize == 0)
                                Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);
                        }

                        List<ExecutionReport> replysort = reply.datas.OrderBy(p => p.UpdateTime).ToList();

                        foreach (Common.Model.Fix.Message.ExecutionReport obj in replysort)
                        {
                            //市场成交
                            HistoryQueue.Add(obj);

                            //STC.Common.Data.STCRocksDbHandler.Instance.Put(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions, $"{obj.SecurityID}#{obj.UpdateTime}", CommonUtils.SerializeObject(obj));
                            //STC.Common.Data.STCRocksDbHandler.Instance.Put(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions, obj.ExecID, CommonUtils.SerializeObject(obj));

                            ////内部成交
                            //if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSF.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZF.ToLower())))
                            //{
                            //    lock (string.Intern(obj.OrderID))
                            //    {
                            //        _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                            //    }

                            //    _OnOwnExecutions?.Invoke(obj);
                            //}

                            //if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                            //{
                            //    continue;
                            //}
                        }

                        if (reply.datas.Count < request.pageSize)
                        {
                            flag = false;
                        }
                        else
                        {
                            index++;
                        }
                    }

                    reply?.datas.Clear();
                }

                IsLastPage = true;
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExecutions", e);
            }
            finally
            {
                ////发布消息
                //Common.Handler.Base.BaseHandler.Instance.SetLoadtMarketExecutionsStatus(true);
            }
        }

        /// <summary>
        /// 请求当日历史我的成交数据
        /// </summary>
        public void RequestOwnExecutions()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QueryOrderReq request = new QueryOrderReq();
                    string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
                    request.dealTimeBegin = today;
                    request.dealTimeEnd = today;
                    request.userName = LocalEnv.UserInfo.UserName;
                    //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                    //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                    request.orderStatus = "1,2";
                    int index = 0;
                    request.pageSize = 2000;

                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取成交
                        var reply = MarketExecHandler.Instance.QueryExecOrderForEMS(request);

                        if (reply == null || reply.datas == null)
                        {
                            flag = false;
                            LogHelper.Info($"没有查询到历史市场成交记录 !");
                            IsEndProcHistoryMarketExecutions = true;
                            //发布消息
                            Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);
                            break;
                        }

                        if (reply != null && reply.datas != null)
                        {
                            foreach (Common.Model.Fix.Message.ExecutionReport obj in reply.datas)
                            {
                                obj.Vs = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";

                                if (obj.VenueTypeGW != null && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()))
                                {
                                    lock (string.Intern(obj.OrderID))
                                    {
                                        _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                    }

                                    _OnOwnExecutions?.Invoke(obj);
                                }
                                else
                                {
                                    //// 过滤掉 4：成交无效（删除/撤销等）
                                    //if (obj.OrdStatus == "4")
                                    //{
                                    //    continue;
                                    //}
                                    /*lock (string.Intern(obj.ExecID))
                                    {
                                        if (_CacheMarketExecutions.ContainsKey(obj.ExecID)
                                            && _CacheMarketExecutions[obj.ExecID].TransactTime.CompareTo(obj.TransactTime) > 0)
                                        {
                                            continue;
                                        }
                                        _CacheMarketExecutions.AddOrUpdate(obj.ExecID, obj, (key, value) => obj);
                                    }
                                    _OnMarketExecutions?.Invoke(obj);*/
                                }
                            }
                            if (reply.datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }

                        reply?.datas.Clear();
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestExecutions", e);
                }
            });
        }


        /// <summary>
        /// 更新最新更新时间
        /// </summary>
        public void UpdateLastUpdateTime(string updatetime)
        {
            if (LastUpdateTime == DateTime.MinValue)
            {
                LastUpdateTime = DateTimeEx.ConvertStringToDateTime(updatetime);
                STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, "LastUpdateTime", LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            else
            {
                string lastres = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, "LastUpdateTime");
                if (!string.IsNullOrEmpty(lastres))
                {
                    DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(lastres);
                    DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(updatetime);
                    if (DateTime.Compare(curUpdateTime, orgdatetime) > 0)
                    {
                        LastUpdateTime = curUpdateTime;
                        STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, "LastUpdateTime", curUpdateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                }
            }
        }


        #region 处理历史市场成交线程
        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancelhistorylationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// 历史数据队列池
        /// </summary>
        private static BlockingCollection<Common.Model.Fix.Message.ExecutionReport> HistoryQueue = new BlockingCollection<Common.Model.Fix.Message.ExecutionReport>();
        // 使用 SemaphoreSlim 限制并发线程数量为 50
        private static SemaphoreSlim semaphore = new SemaphoreSlim(50);
        private bool IsLastPage = false;
        /// <summary>
        /// 开始处理历史市场成交
        /// </summary>
        private bool IsStartProcHistoryMarketExecutions = false;
        /// <summary>
        /// 已经处理完历史市场成交
        /// </summary>
        private bool IsEndProcHistoryMarketExecutions = false;
        /// <summary>
        /// 处理查询得到的市场成交
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcHistoryMarketExecutionsQueue(CancellationToken cancelhistorylationTokenSource)
        {
            try
            {
                Common.Model.Fix.Message.ExecutionReport obj = null;
                while (!cancelhistorylationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        semaphore.Wait(); // 控制最多只能有 50 个并发线程

                        if (IsLastPage)
                        {
                            if (HistoryQueue.Count == 0)
                            {
                                LogHelper.Info($"历史市场成交记录处理完成 !");
                                IsEndProcHistoryMarketExecutions = true;
                                //发布消息
                                Common.Handler.Base.BaseHandler.Instance.SetProctMarketExecutionsStatus(true);

                                //一次性通知所有程序最后成交
                                ProcDbLastExecution();
                            }
                        }
                        
                        // 尝试从队列中取数据，等待10ms避免阻塞主线程
                        obj = HistoryQueue.Take(cancelhistorylationTokenSource);
                        //obj.Vs = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";

                        if (!string.IsNullOrEmpty(obj.SecurityID ?? ""))
                        {
                            //首先保存到RocksDb
                            STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbCacheMarketExecutions, $"{obj.SecurityID}#{obj.ExecID}", obj);

                            //内部成交
                            if (obj.VenueTypeGW != null && (!obj.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.CMDS.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SSF.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZE.ToLower()) && !obj.VenueTypeGW.ToLower().Equals(ApiConsts.SZF.ToLower())))
                            {
                                lock (string.Intern(obj.OrderID))
                                {
                                    _CacheOwnExecutions.AddOrUpdate(obj.OrderID, obj, (key, value) => obj);
                                }
                            }

                            if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                            {
                                semaphore.Release(); // 不符合要求的数据，直接释放信号量
                            }
                            else
                            {
                                // 创建一个任务来处理数据
                                Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        SubProcHistoryMarketExecutionsNew(obj);
                                    }
                                    finally
                                    {
                                        semaphore.Release(); // 处理完数据后，释放信号量
                                        //LogHelper.Error($"正常释放信号量,当前个数 {semaphore.CurrentCount}");
                                    }
                                });
                            }
                        }
                        else
                        {
                            semaphore.Release(); // 不符合要求的数据，直接释放信号量
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            finally
            {

            }
        }

        /// <summary>
        /// 处理查询得到的市场成交子线程
        /// </summary>
        /// <param name="obj"></param>
        private void SubProcHistoryMarketExecutionsNew(ExecutionReport obj)
        {
            try
            {
                string key = obj.OrderID;
                if (obj.VenueTypeGW.ToUpper().Equals(Common.Api.ApiConsts.CDH.ToUpper()))
                {
                    key = obj.ExecID;
                }

                string src = string.Empty;

                //更新记录市场成交
                src = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}");

                if (string.IsNullOrEmpty(src))
                {
                    MarketExecutionsItem marketExe2 = new MarketExecutionsItem();
                    Common.DataProcesser.LastExecutionProcesser.Processer.ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe2);
                    STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbMarketExecution, $"{obj.SecurityID}#{key}", marketExe2);
                }

                //更新最新成交
                string lastres = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbLastExecution, obj.SecurityID);

                if (string.IsNullOrEmpty(lastres))
                {
                    //更新最新成交
                    Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(obj.SecurityID, obj);
                }
                else
                {

                    //查找 "UpdateTime" 的位置
                    string[] parts = lastres.Split(new string[] { "\"TransactTime\":" }, StringSplitOptions.None);

                    if (parts.Length > 1)
                    {
                        string transactionTime = parts[1].Split(',')[0];
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(transactionTime.Replace("\"", ""));
                        DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.TransactTime);
                        if (DateTime.Compare(curUpdateTime, orgdatetime) < 0)
                        {
                            return;
                        }

                        //更新最新成交
                        Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(obj.SecurityID, obj);
                    }
                }


            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

        }

        #endregion
    }
}
