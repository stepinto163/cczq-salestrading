using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace STC.WorkTable.Plugin.Utils.Data.CRM.Handler
{
    public class DataHandler
    {
        private bool IsSearching = false;
        private int PageSize = 2000;
        private int maxCount = 0;
        private int pageNum = 0;
        private bool IsStart = true;
       
        //broker
        private readonly DataTable dt = new DataTable();
        //trader
        private readonly SortedDictionary<string, CrmInst> instDic = new SortedDictionary<string, CrmInst>();//字典（key 为id,value为CrmInst）
        private readonly DataTable dtContact = new DataTable();
        //inst
        private readonly SortedDictionary<string, CrmContact> contactDic = new SortedDictionary<string, CrmContact>();//字典（key 为id,value为CrmContact）
        private readonly DataTable dtInst = new DataTable();
        //交易员和机构数据混合表
        private readonly DataTable dtContactInst = new DataTable();

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
                    InitData();

                    InitInstData();

                    InitContactData();

                    while (IsStart && LocalEnv.ConfInfo.CanGoodsQuickSearch)
                    {
                        SearchBroker();

                        SearchInsts();

                        SearchContact1();

                        Thread.Sleep(10 * 60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void ClearDataTable(DataTable dt)
        {
            try
            {
                dt.Rows.Clear();
            }
            catch (Exception e)
            {
                LogHelper.Error("ClearDataTable exception:" + e.Message);
            }
        }

        #region contact 交易员

        public SortedDictionary<string, CrmContact> GetAllContactData()
        {
            return contactDic;
        }

        private void InitContactData()
        {
            try
            {
                dtContact.Columns.Add(new DataColumn("instcode", typeof(string)));
                dtContact.Columns.Add(new DataColumn("idbkey", typeof(string)));
                dtContact.Columns.Add(new DataColumn("crmid", typeof(string)));
                dtContact.Columns.Add(new DataColumn("id", typeof(string)));
                dtContact.Columns.Add(new DataColumn("namecn", typeof(string)));
                dtContact.Columns.Add(new DataColumn("instid", typeof(string)));
                dtContact.Columns.Add(new DataColumn("instname", typeof(string)));
                dtContact.Columns.Add(new DataColumn("insttypevalue", typeof(string)));
                dtContact.Columns.Add(new DataColumn("createtime", typeof(string)));
                dtContact.Columns.Add(new DataColumn("updatetime", typeof(string)));
                dtContact.Columns.Add(new DataColumn("pinyin", typeof(string)));
                dtContact.Columns.Add(new DataColumn("pinyinfull", typeof(string)));
                dtContact.Columns.Add(new DataColumn("biztype", typeof(string)));
                dtContact.Columns.Add(new DataColumn("contactqbid", typeof(string)));
                dtContact.Columns.Add(new DataColumn("isntqbid", typeof(string)));
                dtContact.PrimaryKey = new DataColumn[] { dtContact.Columns["idbkey"] };
                dtContact.DefaultView.Sort = "idbkey ASC";

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
            lock (contactDic)
            {
                if (!contactDic.ContainsKey(i.idbkey))
                    contactDic.Add(i.idbkey, i);
                else
                    contactDic[i.idbkey] = i;
            }
            return result;
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
                LogHelper.Debug("MaxCount:" + maxCount + ", dtContact.Rows:" + dtContact.Rows.Count);
                if (maxCount > 0 && maxCount != dtContact.Rows.Count)
                {
                    LogHelper.Debug("Clear Contact datatable.");
                    ClearDataTable(dtContact);

                    LogHelper.Debug("Clear Contact db.");
                    SqliteManager.Instance.DeleteContact();

                    LogHelper.Debug("Start sync contact: reply count:" + maxCount + ", db count:" + dtContact.Rows.Count);
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


        public DataTable SearchContact(string condition, int num)
        {
            DataTable dtc = dtContact.Clone();
            lock (dtContact.Rows.SyncRoot)
            {
                var drs = dtContact.Select(string.Format("biztype like '%BNC%' and (namecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%')", condition)).Take(num);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        public DataTable SearchContactByinstid(string condition, int resultNum)
        {
            DataTable dtc = dtContact.Clone();
            lock (dtContact.Rows.SyncRoot)
            {
                var drs = dtContact.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        public DataTable SearchContactByIn(string condition, int resultNum)
        {
            DataTable dtc = dtContact.Clone();
            lock (dtContact.Rows.SyncRoot)
            {
                var drs = dtContact.Select(string.Format("biztype like '%BNC%' and instid in ({0})", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        public DataTable SearchQuoteContact(string condition)
        {
            DataTable dtc = dtContact.Clone();
            lock (dtContact.Rows.SyncRoot)
            {
                var drs = dtContact.Select(string.Format("biztype like '%BNC%' and namecn like '%{0}%'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }
        public DataTable SearchQuoteContactByinstid(string condition)
        {
            DataTable dtc = dtContact.Clone();
            lock (dtContact.Rows.SyncRoot)
            {
                var drs = dtContact.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        #endregion

        #region Inst 机构

        public SortedDictionary<string, CrmInst> GetAllInstData()
        {
            return instDic;
        }

        private void InitInstData()
        {
            try
            {
                dtInst.Columns.Add(new DataColumn("idbkey", typeof(string)));
                dtInst.Columns.Add(new DataColumn("crmid", typeof(string)));
                dtInst.Columns.Add(new DataColumn("id", typeof(string)));
                dtInst.Columns.Add(new DataColumn("shortnamecn", typeof(string)));
                dtInst.Columns.Add(new DataColumn("shortnameen", typeof(string)));
                dtInst.Columns.Add(new DataColumn("instcode", typeof(string)));
                dtInst.Columns.Add(new DataColumn("cityvalue", typeof(string)));
                dtInst.Columns.Add(new DataColumn("insttypevalue", typeof(string)));
                dtInst.Columns.Add(new DataColumn("createtime", typeof(string)));
                dtInst.Columns.Add(new DataColumn("updatetime", typeof(string)));
                dtInst.Columns.Add(new DataColumn("issuer_code", typeof(string)));
                dtInst.Columns.Add(new DataColumn("citycode", typeof(string)));
                dtInst.Columns.Add(new DataColumn("companyid", typeof(string)));
                dtInst.Columns.Add(new DataColumn("pinyin", typeof(string))); 
                dtInst.Columns.Add(new DataColumn("pinyinfull", typeof(string)));
                dtInst.Columns.Add(new DataColumn("taglist", typeof(string)));
                dtInst.Columns.Add(new DataColumn("bncshortnamelist", typeof(string)));
                dtInst.Columns.Add(new DataColumn("instbncshortname", typeof(string)));
                dtInst.Columns.Add(new DataColumn("bcoshortnamelist", typeof(string))); 
                dtInst.Columns.Add(new DataColumn("ncdshortnamelist", typeof(string)));
                dtInst.Columns.Add(new DataColumn("irsshortnamelist", typeof(string)));
                dtInst.PrimaryKey = new DataColumn[] { dtInst.Columns["idbkey"] };
                dtInst.DefaultView.Sort = "idbkey ASC";

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
            lock (instDic)
            {
                if (!instDic.ContainsKey(i.idbkey))
                    instDic.Add(i.idbkey, i);
                else
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
                LogHelper.Debug("MaxCount:" + maxCount + ", dtInst.Rows:" + dtInst.Rows.Count);
                if (maxCount > 0 && maxCount != dtInst.Rows.Count)
                {
                    LogHelper.Debug("Clear Insts datatable.");
                    ClearDataTable(dtInst);

                    LogHelper.Debug("Clear Insts db.");
                    SqliteManager.Instance.DeleteInst();

                    LogHelper.Debug("Start sync insts: reply count:" + maxCount + ", db count:" + dtInst.Rows.Count);
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

        public DataTable SearchInst(string condition)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or instcode like '%{0}%')", condition)).Take(20);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

       
        public DataTable SearchInst2(string condition, int resultNum)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        public DataTable SearchInst3(string condition)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or instbncshortname like '%{0}%')", condition)).Take(20);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        /// <summary>
        /// 先唯一匹配，再模糊匹配
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataTable SearchInst3(string condition, int resultNum)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn = '{0}'  or pinyin = '{0}')", condition)).Take(resultNum);
                if (drs.Count() == 0)
                    drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }

        /// <summary>
        /// 过滤带 资管 名字的机构,并且匹配bncshortnamelist
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataTable SearchInst4(string condition, int resultNum)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (bncshortnamelist = '{0}' or shortnamecn = '{0}'  or pinyin = '{0}' or pinyinfull = '{0}')", condition)).Take(resultNum);
                if (drs.Count() == 0)
                {
                    drs = dtInst.Select(string.Format("taglist like '%BOND%' and (bncshortnamelist like '%{0}%' or shortnamecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%')", condition));

                    if (drs.Count() > 0)
                    {
                        int index = 0;
                        foreach (DataRow dr in drs)
                        {
                            string shortname = dr["shortnamecn"] + "";
                            if (index < resultNum && !string.IsNullOrEmpty(shortname) && shortname.IndexOf("资管") < 0)
                            {
                                dtc.ImportRow(dr);
                                index++;
                            }
                            if (index == resultNum)
                                break;
                        }
                        if (index == 0)
                        {
                            drs = drs.Take(resultNum);
                            foreach (var dr in drs)
                            {
                                dtc.ImportRow(dr);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var dr in drs)
                    {
                        dtc.ImportRow(dr);
                    }
                }
            }

            return dtc;
        }

        public DataTable SearchQuoteInst(string condition)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }
        public DataTable SearchQuoteByIdbkey(string condition)
        {
            DataTable dtc = dtInst.Clone();
            lock (dtInst.Rows.SyncRoot)
            {
                var drs = dtInst.Select(string.Format("taglist like '%BOND%' and idbkey = '{0}'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }

            return dtc;
        }
        #endregion


        #region broker 经纪人

        public DataTable GetAllBrokerData()
        {
            return dt;
        }

        private void InitData()
        {
            try
            {
                dt.Columns.Add(new DataColumn("Idb_key", typeof(string)));
                dt.Columns.Add(new DataColumn("AccountId", typeof(string)));
                dt.Columns.Add(new DataColumn("Account", typeof(string)));
                dt.Columns.Add(new DataColumn("AccountName", typeof(string)));
                dt.Columns.Add(new DataColumn("UpdateTime", typeof(string)));
                dt.Columns.Add(new DataColumn("PinYin", typeof(string)));
                dt.Columns.Add(new DataColumn("Term_ids", typeof(string)));

                dt.PrimaryKey = new DataColumn[] { dt.Columns["Idb_key"] };
                dt.DefaultView.Sort = "Idb_key ASC";

                List<BrokerInfoPo> list = SqliteManager.Instance.GetAllBroker();
                if (list != null && list.Count > 0)
                {
                    foreach (BrokerInfoPo i in list)
                    {
                        AddRow(i);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }

        private bool AddRow(BrokerInfoPo i)
        {
            bool isExit = false;
            lock (dt.Rows.SyncRoot)
            {
                DataRow dr = dt.NewRow();
                if (dt.Rows.Contains(i.Idb_key))
                {
                    isExit = true;
                    dr = dt.Rows.Find(i.Idb_key);
                }
                dr[0] = i.Idb_key;
                dr[1] = i.AccountId;
                dr[2] = i.Account;
                dr[3] = i.AccountName;
                dr[4] = i.UpdateTime;
                dr[5] = i.PinYin;
                //转化Term_ids为BCO,NCD显示，方便LIke查询
                dr[6] = TermIdsToName(i.Term_ids);//i.Term_ids;
                if (!dt.Rows.Contains(i.Idb_key))
                    dt.Rows.Add(dr);
            }
            return isExit;
        }

        private string TermIdsToName(string termIds)
        {
            string name = string.Empty;

            string[] goodstypeArr = termIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            
            foreach (string str in goodstypeArr)
            {
                if (str == "1")
                    name += "BCO,";
                else if (str == "2")
                    name += "BNC,";
                else if (str == "11")
                    name += "ABS,";
                else if (str == "12")
                    name += "NCD,";
                else if (str == "33")
                    name += "Bill,";
            }
            if (!string.IsNullOrEmpty(name))
                name = name.Substring(0, name.Length - 1);

            return name;
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


        private void SearchBroker1()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;

            var pre = SqliteManager.Instance.GetBrokerMaxDate();
            if (pre != null)
            {
                request.UpdateTime = pre.UpdateTime;
            }

            var reply = ApiManager.Instance.GetDefaultApi().Request<BrokerSearchReplyPo>(ApiConsts.CRMSvr, ApiConsts.TOPIC_BROKER_SEARCH, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all broker data.");
                    SyncBroker();

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

                    
                    var reply1 = ApiManager.Instance.GetDefaultApi().Request<BrokerSearchReplyPo>(ApiConsts.CRMSvr, ApiConsts.TOPIC_BROKER_SEARCH, request);
                    if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                    {
                        SqliteManager.Instance.InsertBroker(reply1.Datas);

                        foreach (var a in reply1.Datas)
                        {
                            AddRow(a);
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// 每日检查Broker数据是否同步
        /// </summary>
        public void SyncBroker()
        {
            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 1;
            request.PageSize = 1;
            
            var reply = ApiManager.Instance.GetDefaultApi().Request<BrokerSearchReplyPo>(ApiConsts.CRMSvr, ApiConsts.TOPIC_BROKER_SEARCH, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.TotalSize;
                LogHelper.Debug("MaxCount:" + maxCount + ", dt.Rows:" + dt.Rows.Count);
                if (maxCount > 0 && maxCount != dt.Rows.Count)
                {
                    LogHelper.Debug("Clear Broker datatable.");
                    ClearDataTable(dt);

                    LogHelper.Debug("Clear Broker db.");
                    SqliteManager.Instance.DeleteBroker();

                    LogHelper.Debug("Start sync broker: reply count:" + maxCount + ", db count:" + dt.Rows.Count);
                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    for (int i = 0; i < pageNum; i++)
                    {
                        request.PageIndex = i + 1;
                        request.PageSize = PageSize;

                        var reply1 = ApiManager.Instance.GetDefaultApi().Request<BrokerSearchReplyPo>(ApiConsts.CRMSvr, ApiConsts.TOPIC_BROKER_SEARCH, request);
                        if (reply1 != null && reply1.Datas != null && reply1.Datas.Count > 0)
                        {
                            foreach (var a in reply1.Datas)
                            {
                                AddRow(a);
                            }

                            SqliteManager.Instance.InsertBroker(reply1.Datas);

                        }
                    }
                }
            }
        }

        public DataTable SearchBroker(string matchName, string filterName)
        {
            DataTable dtc = dt.Clone();
            lock (dt.Rows.SyncRoot)
            {
                var drs = dt.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%'  or PinYin like '%{0}%') and AccountName <> '{1}'", matchName, filterName)).Take(5);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }
            return dtc;
        }

        public DataTable SearchBroker2(string matchName, int resultNum)
        {
            DataTable dtc = dt.Clone();
            lock (dt.Rows.SyncRoot)
            {
                var drs = dt.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }
            return dtc;
        }

        public DataTable SearchBroker3(string matchName, int resultNum)
        {
            DataTable dtc = dt.Clone();
            lock (dt.Rows.SyncRoot)
            {
                var drs = dt.Select(string.Format("Term_ids like '%2,%' and (Account = '{0}' or AccountName = '{0}' or PinYin = '{0}')", matchName)).Take(resultNum);
                if (drs.Count() == 0)
                    drs = dt.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }
            return dtc;
        }
        public DataTable SearchQuoteBroker(string matchName)
        {
            DataTable dtc = dt.Clone();
            lock (dt.Rows.SyncRoot)
            {
                var drs = dt.Select(string.Format("Term_ids like '%2,%' and AccountName like '%{0}%'", matchName)).Take(10);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }
            return dtc;
        }
        public DataTable SearchQuoteBrokerByAccount(string account)
        {
            DataTable dtc = dt.Clone();
            lock (dt.Rows.SyncRoot)
            {
                var drs = dt.Select(string.Format("Term_ids like '%2,%' and Account = '{0}'", account)).Take(1);
                foreach (var dr in drs)
                {
                    dtc.ImportRow(dr);
                }
            }
            return dtc;
        }

        #endregion

     
    }
}
