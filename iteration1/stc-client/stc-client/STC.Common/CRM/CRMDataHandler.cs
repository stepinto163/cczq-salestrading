using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using STC.Frame.Plugin.WindowsPlugin;
using STC.Common.Model.CRM;
using STC.Common.Model.Base;

namespace STC.Common.CRM
{
    public class CRMDataHandler : ICRMDataHandler
    {

        private static ICRMDataHandler crmDataSearchImpl = new CRMDataHandler();
        public static ICRMDataHandler Instance { get { return crmDataSearchImpl; } }

        private CRMDataHandler()
        { }

        public IForm Application { get; set; }

        #region Bond

        public List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            string key = GetPluginShareKey(2, "Bond");
            Dictionary<string, BondsInfoPo> dic = (Dictionary<string, BondsInfoPo>)Application.GetObject(key);

            lock (dic)
            {
                var v = (from d in dic where d.Key.Contains(condition) select d);

                int i = 0;
                foreach (KeyValuePair<string, BondsInfoPo> kv in v)
                {
                    if (i < resultNum)
                        result.Add(kv.Value);
                    else
                        break;

                    i++;
                }
            }

            return result;
        }

        private DataTable GetDataTable(int pluginKey, string name)
        {
            string key = GetPluginShareKey(pluginKey, name);
            DataTable dtc = (DataTable)Application.GetObject(key);
            return dtc;
        }

       

        //public DataTable SearchBondBySql(string sql, int resultNum)
        //{
        //    DataTable dtc = GetDataTable(2, "Bond");
        //    DataTable result = dtc.Clone();
        //    lock (dtc.Rows.SyncRoot)
        //    {
        //        var drs = dtc.Select(sql).Take(resultNum);
        //        foreach (var dr in drs)
        //        {
        //            result.ImportRow(dr);
        //        }
        //    }
        //    return result;
        //}

        private string GetPluginShareKey(int pluginKey, string name)
        {
            return string.Format("{0}_{1}", pluginKey, name);
        }

        public DataTable SearchBond2(string condition)
        {
            DataTable dtc = GetDataTable(2, "Bond");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("BondID like '%{0}%' or ShortName like '%{0}%' or pinYin like '%{0}%' or pinYinFull like '%{0}%'", condition)).Take(20);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchBond3(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(2, "Bond");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                //var drs = dtc.Select(string.Format("BondID = '{0}' or ShortName = '{0}' or pinYin = '{0}' or pinYinFull = '{0}'", condition)).Take(resultNum);
                //if (drs.Count() == 0)
                    var drs = dtc.Select(string.Format("BondID like '%{0}%' or ShortName like '%{0}%' or pinYin like '%{0}%' or pinYinFull like '%{0}%'", condition)).Take(resultNum);

                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        #endregion

        #region Contact

        public DataTable SearchContactBySql(string sql, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(sql).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }


        public DataTable SearchContact(string condition, int num)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("biztype like '%BNC%' and (namecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%')", condition)).Take(num);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchContactByinstid(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchContactByIn(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("biztype like '%BNC%' and instid in ({0})", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchQuoteContact(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("biztype like '%BNC%' and namecn like '%{0}%'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        public DataTable SearchQuoteContactByinstid(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public List<DataGridDataPo> GetContactByContactName(string contactName, int resultNum)
        {
            contactName = contactName.Replace("'", "");
            DataTable dt = SearchContact(contactName, resultNum);
            List<DataGridDataPo> dataList = new List<DataGridDataPo>(dt.Rows.Count);
            foreach (DataRow dr in dt.Rows)
            {
                DataGridDataPo dg = new DataGridDataPo()
                {
                    InstTraderName = string.Format("{0}({1})", dr["instname"], (dr["namecn"] + "").Trim()),
                    IdbKey = dr["idbkey"] + "",
                    InstIdbKey = dr["instid"] + "",
                    TraderName = dr["namecn"] + "",
                    InstName = dr["instname"] + ""

                };
                dataList.Add(dg);
            }

            return dataList;
        }

        public List<DataGridDataPo> GetContactByInstName(string instName, int resultNum)
        {
            instName = instName.Replace("'", "");
            DataTable dt = SearchInst4(instName, resultNum);
            List<DataGridDataPo> dataList = new List<DataGridDataPo>();

            if (dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                Dictionary<string, DataRow> instDic = new Dictionary<string, DataRow>();
                foreach (DataRow dr in dt.Rows)
                {
                    string instkey = dr["idbkey"] + "";
                    sb.Append(string.Format("'{0}',", instkey));
                    if (!instDic.ContainsKey(instkey))
                        instDic.Add(instkey, dr);
                }
                string condition = sb.ToString().Substring(0, sb.ToString().Length - 1);

                DataTable dtContact = SearchContactByIn(condition, resultNum);
                if (dtContact.Rows.Count > 0)
                {
                    foreach (DataRow drCon in dtContact.Rows)
                    {
                        string instid = drCon["instid"] + "";
                        if (instDic.ContainsKey(instid))
                        {
                            DataGridDataPo dg = SetDataGridDataPo(GetInstName(instDic[instid]), drCon["namecn"] + "", drCon["idbkey"] + "", drCon["instid"] + "");
                            if (dataList.Count >= resultNum)
                                return dataList;
                            dataList.Add(dg);
                        }
                    }
                }

                if (dataList.Count > resultNum)
                    dataList = dataList.Take(resultNum).ToList();
            }
            return dataList;
        }

        private string GetInstName(DataRow dr)
        {
            string result = string.Empty;

            if (dr == null)
                return result;

            result = dr["bncshortnamelist"] + "";
            if (string.IsNullOrEmpty(result))
                result = dr["shortnamecn"] + "";

            return result;
        }

        private DataGridDataPo SetDataGridDataPo(string instname, string traderName, string traderIdbkey, string instid)
        {
            return new DataGridDataPo()
            {
                InstTraderName = string.Format("{0}({1})", instname, traderName.Trim()),
                IdbKey = traderIdbkey,
                InstIdbKey = instid,
                TraderName = traderName,
                InstName = instname
            };
        }
        #endregion

        #region Inst

        public DataTable SearchInstBySql(string sql, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = resultNum >=0? dtc.Select(sql).Take(resultNum): dtc.Select(sql);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        
        public DataTable SearchInst(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or instcode like '%{0}%')", condition)).Take(20);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }


        public DataTable SearchInst2(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchInst3(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%' or instbncshortname like '%{0}%')", condition)).Take(20);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        /// <summary>
        /// 先唯一匹配，再模糊匹配
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataTable SearchInst3(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn = '{0}'  or pinyin = '{0}')", condition)).Take(resultNum);
                if (drs.Count() == 0)
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        /// <summary>
        /// 过滤带 资管 名字的机构,并且匹配bncshortnamelist
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataTable SearchInst4(string condition, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (bncshortnamelist = '{0}' or shortnamecn = '{0}'  or pinyin = '{0}' or pinyinfull = '{0}')", condition)).Take(resultNum);
                if (drs.Count() == 0)
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (bncshortnamelist like '%{0}%' or shortnamecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%')", condition));

                    if (drs.Count() > 0)
                    {
                        int index = 0;
                        foreach (DataRow dr in drs)
                        {
                            string shortname = dr["shortnamecn"] + "";
                            if (index < resultNum && !string.IsNullOrEmpty(shortname) && shortname.IndexOf("资管") < 0)
                            {
                                result.ImportRow(dr);
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
                                result.ImportRow(dr);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var dr in drs)
                    {
                        result.ImportRow(dr);
                    }
                }
            }

            return result;
        }

        public DataTable SearchQuoteInst(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        public DataTable SearchQuoteByIdbkey(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%BOND%' and idbkey = '{0}'", condition)).Take(10);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        public DataTable SearchBillInst(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("taglist like '%bill%' and (shortnamecn like '%{0}%'or pinyin like '%{0}%' or instcode like '%{0}%')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        #endregion

        #region Broker

        public DataTable SearchBrokerBySql(string sql, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(sql).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBroker(string matchName, string filterName)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%'  or PinYin like '%{0}%') and AccountName <> '{1}'", matchName, filterName)).Take(5);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBroker2(string matchName, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBroker3(string matchName, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account = '{0}' or AccountName = '{0}' or PinYin = '{0}')", matchName)).Take(resultNum);
                if (drs.Count() == 0)
                    drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchQuoteBroker(string matchName)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%2,%' and AccountName like '%{0}%'", matchName)).Take(10);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchQuoteBrokerByAccount(string account)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%2,%' and Account = '{0}'", account)).Take(1);
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchBillBroker(string matchName, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%Bill%' and (Account like '%{0}%'or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBillInstByKey(string condition)
        {
            DataTable dtc = GetDataTable(1, "CRM_Inst");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                if (dtc.Rows != null && dtc.Rows.Count > 0)
                {
                    var drs = dtc.Select(string.Format("taglist like '%bill%' and (idbkey = '{0}')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                    foreach (var dr in drs)
                    {
                        result.ImportRow(dr);
                    }
                }
            }

            return result;
        }

        public DataTable SearchBillBrokerByKey(string matchName, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Account");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("Term_ids like '%Bill%' and (Idb_key = '{0}')", matchName)).Take(resultNum);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        #endregion
    }
}
