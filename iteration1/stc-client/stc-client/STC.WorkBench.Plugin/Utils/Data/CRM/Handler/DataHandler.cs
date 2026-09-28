using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace STC.WorkBench.Plugin.Utils.Data.CRM.Handler
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

                    if(LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                    {
                        InitContactData();
                    }

                    while (IsStart && LocalEnv.ConfInfo.CanGoodsQuickSearch)
                    {
                        SearchInsts();

                        if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                        {
                            SearchContact();
                        }

                        Thread.Sleep(10 * 60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        #region contact 交易员

        public ConcurrentDictionary<string, CrmContact> GetAllContactData()
        {
            return contactDic;
        }

        private void InitContactData()
        {
            try
            {
                var list = SqliteManager.Instance.GetAllContacts();
                if (list != null && list.Count > 0)
                {
                    foreach (var i in list)
                    {
                        AddContactRow(i);
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
            contactDic[i.idbkey] = i;
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

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncContact()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var reply = ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.totalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dtContact.Rows:" + contactDic.Count);
                if (maxCount > 0 && maxCount != contactDic.Count)
                {
                    LogHelper.Debug("Clear Contact datatable.");
                    contactDic.Clear();

                    LogHelper.Debug("Clear Contact db.");
                    SqliteManager.Instance.DeleteContact();

                    LogHelper.Debug("Start sync contact: reply count:" + maxCount + ", db count:" + contactDic.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    for (int i = 0; i < pageNum; i++)
                    {
                        request.PageIndex = i + 1;
                        request.PageSize = PageSize;
                        var reply1 = ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
                        if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                        {
                            foreach (var a in reply1.Datas)
                            {
                                AddContactRow(a);

                            }

                            SqliteManager.Instance.InsertConyact(reply1.Datas);
                        }
                    }
                }

            }
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
            var reply = ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all contact data.");
                    SyncContact();

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

                    var reply1 = ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACT, request);
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
            instDic[i.idbkey] = i;
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

        /// <summary>
        /// 每日同步机构数据
        /// </summary>
        public void SyncInsts()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var reply = ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.totalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dtInst.Rows:" + instDic.Count);
                if (maxCount > 0 && maxCount != instDic.Count)
                {
                    LogHelper.Debug("Clear Insts datatable.");
                    instDic.Clear();

                    LogHelper.Debug("Clear Insts db.");
                    SqliteManager.Instance.DeleteInst();

                    LogHelper.Debug("Start sync insts: reply count:" + maxCount + ", db count:" + instDic.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    for (int i = 0; i < pageNum; i++)
                    {
                        request.PageIndex = i + 1;
                        request.PageSize = PageSize;
                        var reply1 = ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
                        if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                        {
                            foreach (var a in reply1.Datas)
                            {
                                AddInstRow(a);
                            }

                            SqliteManager.Instance.InsertInst(reply1.Datas);

                        }
                    }
                }
               
            }
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
            var reply = ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all isnt data.");
                    SyncInsts();

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

                    var reply1 = ApiManager.Instance.GetDefaultApi().Request<CrmInstReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_INST, request);
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

    }
}
