using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.CRM;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Data.Plugin.CRM.Handler
{
    public class DataHandler
    {
        private bool IsSearching = false;
        private int PageSize = 2000;
        private int maxCount = 0;
        private int pageNum = 0;
        private bool IsStart = true;
       
        //trader
        private readonly ConcurrentDictionary<string, CrmInst> instDic = new ConcurrentDictionary<string, CrmInst>();//字典（key 为id,value为CrmInst）
        //inst
        private readonly ConcurrentDictionary<string, CrmContact> contactDic = new ConcurrentDictionary<string, CrmContact>();//字典（key 为id,value为CrmContact）
        //                                                                                                                      //inst
        private readonly ConcurrentDictionary<string, BrokerInfoPo> brokerDic = new ConcurrentDictionary<string, BrokerInfoPo>();//字典（key 为id,value为BrokerInfoPo）

        private static DataHandler brokerHandler = new DataHandler();
        public static DataHandler Instance { get { return brokerHandler; } }

        private DataHandler()
        {
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    InitInstData();
                    InitBrokerData();
                    if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                    {
                        InitContactData();
                    }

                    while (IsStart && LocalEnv.ConfInfo.CanGoodsQuickSearch)
                    {
                        SearchInsts();
                        SearchBroker();
                        if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                        {
                            SearchContact();
                        }

                        CommonUtils.LoadUserCrm();

                        Thread.Sleep(10 * 60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                    Task.Factory.StartNew(() =>
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    });
                }
            });

        }

        #region 处理订阅信息
        /// <summary>
        /// 订阅Crm
        /// </summary>
        public void SubscribeCrmInfo()
        {
            STC.Common.Handler.CRM.CRMHandler.Instance.SubscribeCrmContact(OnSubscribeCrmContact);
            STC.Common.Handler.CRM.CRMHandler.Instance.SubscribeCrmInstMsg(OnSubscribeCrmInstMsg);
            STC.Common.Handler.CRM.CRMHandler.Instance.SubscribeOnCrmAccountMsg(OnSubscribeOnCrmAccountMsg);
        }

        /// <summary>
        /// Crm账号
        /// </summary>
        /// <param name="rs"></param>
        private void OnSubscribeOnCrmAccountMsg(CrmAccountReplyModel rs)
        {
            if (!rs.success)
                return;

            //BrokerInfoPo r = brokerDic[bip.Idb_key];
            Task.Run(() =>
            {
                try
                {
                    BrokerInfoPo bip = rs.body;
                    AddBrokerRow(bip);
                    SqliteManager.Instance.InsertBroker(new List<BrokerInfoPo>() { bip });
                    LogHelper.Info($"Subscribe And Update Crm Account Ok.");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Subscribe And Update Crm Account exception: {ex.Message}");
                }
            });

        }

        /// <summary>
        /// Crm交易员
        /// </summary>
        /// <param name="rs"></param>
        private void OnSubscribeCrmContact(CrmContactReplyModel rs)
        {
            if (!rs.success)
                return;
            
            Task.Run(() =>
            {
                try
                {
                    //List<string> termids = CommonUtils.AccountPower();
                    CrmContact bip = rs.body;
                    //if (!(termids != null && termids.Count > 0 && !termids.Contains(bip.biztype)))
                    //{
                    //    AddContactRow(bip);
                    //    SqliteManager.Instance.InsertConyact(new List<CrmContact>() { bip });
                    //    LogHelper.Info($"Subscribe And Update Crm Contact Ok.");
                    //}
                    //else {
                    //    LogHelper.Info($"交易员权限不符.");
                    //}
                    AddContactRow(bip);
                    SqliteManager.Instance.InsertConyact(new List<CrmContact>() { bip });
                    LogHelper.Info($"Subscribe And Update Crm Contact Ok.");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Subscribe And Update Crm Contact exception: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Crm机构
        /// </summary>
        /// <param name="rs"></param>
        private void OnSubscribeCrmInstMsg(CrmInstReplyModel rs)
        {
            if (!rs.success)
                return;

            Task.Run(() =>
            {
                try
                {
                    //List<string> termids = CommonUtils.AccountPower();
                    CrmInst bip = rs.body;
                    //if (!(termids != null && termids.Count > 0 && !termids.Contains(bip.taglist)))
                    //{
                        AddInstRow(bip);
                        SqliteManager.Instance.InsertInst(new List<CrmInst>() { bip });
                        LogHelper.Info($"Subscribe And Update Crm Inst Ok.");
                    //}
                    //else {
                    //    LogHelper.Info($"机构权限不符.");
                    //}
                   
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Subscribe And Update Crm Inst exception: {ex.Message}");
                }
            });
        }

        #endregion


        #region contact 交易员

        public ConcurrentDictionary<string, CrmContact> GetAllContactData()
        {
            return contactDic;
        }

        private void InitContactData()
        {
            try
            {
                //List<string> termids = CommonUtils.AccountPower();
                var list = SqliteManager.Instance.GetAllContacts();
                if (list != null && list.Count > 0)
                {
                    foreach (var i in list)
                    {
                        //if (!(termids != null && termids.Count > 0 && !termids.Contains(i.biztype)))
                        //{
                            AddContactRow(i);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }

        private bool AddContactRow(CrmContact i)
        {
            bool result = false;
            if (string.IsNullOrEmpty(i.crmid))
            {
                CrmContact removeCrmContract = null;
                contactDic.TryRemove(i.idbkey, out removeCrmContract);
            }
            else
            {
                contactDic[i.idbkey] = i;
            }
            
            return result;
        }

        private void SearchContact()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SearchContact1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Search Contact failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        public void SyncContact()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SyncContact1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Sync Contact failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncContact1()
        {
            //List<string> termids = CommonUtils.AccountPower();
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var reply = CRMHandler.Instance.QueryContact(request);
                //ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.totalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dtContact.Rows:" + contactDic.Count);
                if (maxCount > 0 && maxCount != contactDic.Count)
                {
                    LogHelper.Debug("Clear Contact datatable.");
                    contactDic.Clear();

                    LogHelper.Debug("Start sync contact: reply count:" + maxCount + ", db count:" + contactDic.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    Task[] lstTask = new Task[pageNum];
                    object uplock = new object();
                    for (int i = 0; i < pageNum; i++)
                    {
                        CRMRequestParamPo request1 = new CRMRequestParamPo();
                        request1.PageIndex = i + 1;
                        request1.PageSize = PageSize;
                        lstTask[i] = new Task(() =>
                        {
                            var reply1 = CRMHandler.Instance.QueryContact(request1);
                            //ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
                            if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                            {
                                foreach (var a in reply1.Datas)
                                {
                                    lock(uplock)
                                    {
                                        //if (!(termids!=null&& termids.Count>0&& !termids.Contains(a.biztype))) {
                                            AddContactRow(a);
                                        //}
                                    }
                                }

                                //SqliteManager.Instance.InsertConyact(reply1.Datas);
                            }
                        });
                        lstTask[i].Start();
                    }
                    Task.WaitAll(lstTask);
                    WriteLocalDb_contact();
                }
            }
        }

        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb_contact()
        {
            Task.Run(() =>
            {
                try
                {
                    LogHelper.Debug("Clear Contact db.");
                    SqliteManager.Instance.DeleteContact();
                    LogHelper.Debug("Start sync contact: db count:" + contactDic.Count);
                    List<CrmContact> datas = this.contactDic.Values.ToList();
                    SqliteManager.Instance.InsertConyact(datas);
                    LogHelper.Info("Sync crm contact db succeed!");
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync crm contact exception.");
                    LogHelper.Error(ex);
                }
            });
        }

        private void SearchContact1()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;
            // re.updatetime = "2018-10-06 00:00:00";
            var pre = SqliteManager.Instance.GetConyactMaxDate();
            if (pre != null)
            {
                request.UpdateTime = pre.updatetime;
            }
            var reply = CRMHandler.Instance.QueryContact(request);
            //ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all contact data.");
                    SyncContact1();

                    return;
                }

                int maxCount = reply.totalSize;
                int pageNum = 0;
                if (maxCount > 0)
                {
                    pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                }
                else
                    pageNum = 0;

                for (int i = 0; i < pageNum; i++)
                {
                    request.PageIndex = i + 1;
                    request.PageSize = PageSize;

                    var reply1 = CRMHandler.Instance.QueryContact(request);
                    //ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
                    if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                    {
                        SqliteManager.Instance.InsertConyact(reply1.Datas);
                        foreach (var a in reply1.Datas)
                        {
                            AddContactRow(a);
                        }
                    }
                }
            }
        }
        #endregion

        #region Inst 机构

        public ConcurrentDictionary<string, CrmInst> GetAllInstData()
        {
            return instDic;
        }

        private void InitInstData()
        {
            try
            {
                var list = SqliteManager.Instance.GetAllInsts();
                if (list != null && list.Count > 0)
                {
                    foreach (var i in list)
                    {
                        AddInstRow(i);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }

        private bool AddInstRow(CrmInst i)
        {
            bool result = false;
            if (string.IsNullOrEmpty(i.crmid))
            {
                CrmInst remmoveCrmIns = null;
                instDic.TryRemove(i.idbkey, out remmoveCrmIns);
            }
            else
            {
                instDic[i.idbkey] = i;
            }
            return result;
        }

        private void SearchInsts()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SearchInsts1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Search Inst failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        public void SyncInsts()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SyncInsts1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Sync Inst failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncInsts1()
        {
            //List<string> termids = CommonUtils.AccountPower();
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var reply = CRMHandler.Instance.QueryInst(request);
                //ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.totalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dtInst.Rows:" + instDic.Count);
                if (maxCount > 0 && maxCount != instDic.Count)
                {
                    LogHelper.Debug("Clear Insts datatable.");
                    instDic.Clear();

                    LogHelper.Debug("Start sync insts: reply count:" + maxCount + ", db count:" + instDic.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    Task[] lstTask = new Task[pageNum];
                    object uplock = new object();
                    for (int i = 0; i < pageNum; i++)
                    {
                        CRMRequestParamPo request1 = new CRMRequestParamPo();
                        request1.PageIndex = i + 1;
                        request1.PageSize = PageSize;

                        lstTask[i] = new Task(() =>
                        {
                            var reply1 = CRMHandler.Instance.QueryInst(request1);
                            //ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
                            if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                            {
                                foreach (var a in reply1.Datas)
                                {
                                    lock (uplock)
                                    {
                                        AddInstRow(a);
                                        //if (!(termids != null && termids.Count > 0 && !termids.Contains(a.taglist)))
                                        //{
                                        //    AddInstRow(a);
                                        //}
                                    }
                                }

                                //SqliteManager.Instance.InsertInst(reply1.Datas);
                            }
                        });
                        lstTask[i].Start();  
                    }
                    Task.WaitAll(lstTask); 
                    WriteLocalDb_Insts();
                }
               
            }
        }

        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb_Insts()
        {
            Task.Run(() =>
            {
                try
                {
                    LogHelper.Debug("Clear Insts db.");
                    SqliteManager.Instance.DeleteInst();
                    LogHelper.Debug("Start sync insts: db count:" + instDic.Count);
                    List<CrmInst> datas = this.instDic.Values.ToList();
                    SqliteManager.Instance.InsertInst(datas);
                    LogHelper.Info("Sync crm Insts db succeed!");
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync crm Insts exception.");
                    LogHelper.Error(ex);
                }
            });
        }

        private void SearchInsts1()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;
            // re.updatetime = "2018-10-06 00:00:00";
            var pre = SqliteManager.Instance.GetInstMaxDate();
            if (pre != null)
            {
                request.UpdateTime = pre.updatetime;
            }
            var reply = CRMHandler.Instance.QueryInst(request); 
            //ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all isnt data.");
                    SyncInsts1();

                    return;
                }


                int maxCount = reply.totalSize;
                int pageNum = 0;
                if (maxCount > 0)
                {
                    pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                }
                else
                    pageNum = 0;

                for (int i = 0; i < pageNum; i++)
                {
                    request.PageIndex = i + 1;
                    request.PageSize = PageSize;

                    var reply1 = CRMHandler.Instance.QueryInst(request); 
                    //ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
                    if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                    {
                        SqliteManager.Instance.InsertInst(reply1.Datas);

                        foreach (var a in reply1.Datas)
                        {
                            AddInstRow(a);
                        }
                    }
                }
            }
        }


        #endregion

        #region broker 账号

        public ConcurrentDictionary<string, BrokerInfoPo> GetAllBrokerData()
        {
            return brokerDic;
        }

        private void InitBrokerData()
        {
            try
            {
                var list = SqliteManager.Instance.GetAllBroker();
                if (list != null && list.Count > 0)
                {
                    foreach (var i in list)
                    {
                        AddBrokerRow(i);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }

        private bool AddBrokerRow(BrokerInfoPo i)
        {
            bool result = false;
            if (string.IsNullOrEmpty(i.Crm_id))
            {
                BrokerInfoPo removeBrokerInfo = null;
                brokerDic.TryRemove(i.Idb_key, out removeBrokerInfo);
            }
            else
            {
                brokerDic[i.Idb_key] = i;
            }

            return result;
        }

        private void SearchBroker()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SearchBroker1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Search Broker failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        public void SyncBroker()
        {
            if (IsSearching)
                return;
            IsSearching = true;
            try
            {
                SyncBroker1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Sync Broker failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncBroker1()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var reply = CRMHandler.Instance.QueryBroker(request);
            //ApiManager.Instance.GetDefaultApi().Request<BrokerInfoPoReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.TotalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dtBroker.Rows:" + brokerDic.Count);
                if (maxCount > 0 && maxCount != contactDic.Count)
                {
                    LogHelper.Debug("Clear Broker datatable.");
                    brokerDic.Clear();

                    LogHelper.Debug("Clear Broker db.");
                    SqliteManager.Instance.DeleteBroker();

                    LogHelper.Debug("Start sync broker: reply count:" + maxCount + ", db count:" + brokerDic.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    Task[] lstTask = new Task[pageNum];
                    object uplock = new object();
                    for (int i = 0; i < pageNum; i++)
                    {
                        CRMRequestParamPo request1 = new CRMRequestParamPo();
                        request1.PageIndex = i + 1;
                        request1.PageSize = PageSize;

                        lstTask[i] = new Task(() => {
                            var reply1 = CRMHandler.Instance.QueryBroker(request1);
                            //ApiManager.Instance.GetDefaultApi().Request<BrokerInfoPoReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
                            if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                            {
                                foreach (var a in reply1.Datas)
                                {
                                    AddBrokerRow(a);

                                }

                                //SqliteManager.Instance.InsertBroker(reply1.Datas);
                            }
                        });
                        lstTask[i].Start();
                    }
                    Task.WaitAll(lstTask);
                    WriteLocalDb_Broker();
                }
            }
        }


        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb_Broker()
        {
            Task.Run(() =>
            {
                try
                {
                    LogHelper.Debug("Clear Broker db.");
                    SqliteManager.Instance.DeleteBroker();
                    LogHelper.Debug("Start sync broker: db count:" + brokerDic.Count);
                    List<BrokerInfoPo> datas = this.brokerDic.Values.ToList();
                    SqliteManager.Instance.InsertBroker(datas);
                    LogHelper.Info("Sync crm Broker db succeed!");
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync crm Broker exception.");
                    LogHelper.Error(ex);
                }
            });
        }

        private void SearchBroker1()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;
            // re.updatetime = "2018-10-06 00:00:00";
            var pre = SqliteManager.Instance.GetBrokerMaxDate();
            if (pre != null)
            {
                request.UpdateTime = pre.UpdateTime;
            }
            var reply = CRMHandler.Instance.QueryBroker(request);
            //ApiManager.Instance.GetDefaultApi().Request<BrokerInfoPoReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all broker data.");
                    SyncBroker1();

                    return;
                }

                int maxCount = reply.TotalSize;
                int pageNum = 0;
                if (maxCount > 0)
                {
                    pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                }
                else
                    pageNum = 0;

                for (int i = 0; i < pageNum; i++)
                {
                    request.PageIndex = i + 1;
                    request.PageSize = PageSize;

                    var reply1 = CRMHandler.Instance.QueryBroker(request);
                    //ApiManager.Instance.GetDefaultApi().Request<BrokerInfoPoReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
                    if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                    {
                        SqliteManager.Instance.InsertBroker(reply1.Datas);
                        foreach (var a in reply1.Datas)
                        {
                            AddBrokerRow(a);
                        }
                    }
                }
            }
        }
        #endregion



    }
}
