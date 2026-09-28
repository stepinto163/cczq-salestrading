using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.IDCEnum;
using com.gateway.log;
using STC.Common.Model.CRM;
using STC.Common.Utils;

namespace STC.Common.Handler.CRM
{
    public class CRMDataHandler : ICRMDataHandler
    {

        private static ICRMDataHandler crmDataSearchImpl = new CRMDataHandler();
        public static ICRMDataHandler Instance { get { return crmDataSearchImpl; } }

        private CRMDataHandler() { }

        public IForm Application { get; set; }

        #region Bond

        public List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            if (string.IsNullOrWhiteSpace(condition))
            {
                return result;
            }
            int retryCount = 100;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    //20200227 wym
                    string key = GetPluginShareKey(EPluginShareKey.Bond);
                    if (string.IsNullOrWhiteSpace(key) || Application == null)
                    {
                        return result;
                    }

                    List<BondsInfoPo> bonds = new List<BondsInfoPo>();
                    List<BondsInfoPo> bonds2 = new List<BondsInfoPo>();
                    List<Dictionary<string, BondsInfoPo>> dicBonds = new List<Dictionary<string, BondsInfoPo>>();

                    if (condition.Length >= 3)
                    {
                        for (int strLength = 3; strLength <= condition.Length; strLength++)
                        {
                            string strCondition = condition.Substring(strLength - 3, 3);
                            Dictionary<string, BondsInfoPo> list;
                            if (ConstIndex.BondIndex.TryGetValue(strCondition.ToLower(), out list))
                            {
                                if (strLength > 3)
                                {
                                    dicBonds.Add(list);
                                }
                                else
                                {
                                    dicBonds.Add(list);
                                    foreach (KeyValuePair<string, BondsInfoPo> item in list)
                                    {
                                        bonds.Add(item.Value);
                                    }
                                }
                            }
                        }
                        if (condition.Length > 3)
                        {
                            if (dicBonds.Count > 0)
                            {
                                Dictionary<string, BondsInfoPo> valuePairs = dicBonds[0];
                                ///找出最短的字典
                                for (int j = 0; j < dicBonds.Count; j++)
                                {
                                    if (j == 0)
                                    {
                                        valuePairs = dicBonds[0];
                                    }
                                    else
                                    {
                                        if (valuePairs.Count > dicBonds[j].Count)
                                        {
                                            valuePairs = dicBonds[j];
                                        }
                                    }
                                }

                                foreach (var item in valuePairs.Values)
                                {
                                    bool ischeked = true;
                                    foreach (var item2 in dicBonds)
                                    {
                                        BondsInfoPo po;
                                        if (!item2.TryGetValue(item.Bond_ID, out po))
                                        {
                                            ischeked = false;
                                            break;
                                        }
                                    }
                                    if (ischeked && (item.Bond_ID.Contains(condition)
                                        || item.Short_Name.Contains(condition)
                                        || item.pinYin.Contains(condition)
                                        || item.pinyin_full.Contains(condition)))
                                    {
                                        bonds2.Add(item);
                                    }
                                }
                            }
                            if (bonds2.Count > 0)
                            {
                                if (resultNum > 0)
                                {
                                    result = bonds2.OrderBy(k => k.Bond_ID.Length).OrderBy(k => k.listed_Market).Take(resultNum).ToList();
                                }
                                else
                                {
                                    result = bonds2.OrderBy(k => k.Bond_ID.Length).OrderBy(k => k.listed_Market).ToList();
                                }

                            }
                        }
                        else
                        {
                            if (bonds.Count > 0)
                            {
                                if (resultNum > 0)
                                {
                                    result = bonds.OrderBy(k => k.Bond_ID.Length).OrderBy(k => k.listed_Market).Take(resultNum).ToList();
                                }
                                else
                                {
                                    result = bonds.OrderBy(k => k.Bond_ID.Length).OrderBy(k => k.listed_Market).ToList();
                                }


                            }
                        }

                    }
                    else
                    {
                        // 和产品确认过，前两位不需要排序
                        string keyBond = GetPluginShareKey(EPluginShareKey.BondByCode);
                        ConcurrentDictionary<string, BondsInfoPo> dic = (ConcurrentDictionary<string, BondsInfoPo>)Application.GetObject(keyBond);
                        if (dic != null)
                        {
                            int count = 0;
                            foreach (var item in dic)
                            {
                                if (count > resultNum)
                                {
                                    break;
                                }
                                if (item.Value.Bond_ID.Contains(condition)
                                    || item.Value.Short_Name.Contains(condition)
                                    || (item.Value.pinYin != null && item.Value.pinYin.Contains(condition))
                                    || (item.Value.pinyin_full != null && item.Value.pinyin_full.Contains(condition)))
                                {
                                    count++;
                                    result.Add(item.Value);
                                }
                            }
                        }
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(20);
                    LogHelper.Debug($"查询重试{i + 1}次");
                    if (i == retryCount - 1)
                    {
                        LogHelper.Error(ex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 债券搜索，当输入条件为三位的时候，进行债券代码末尾匹配
        /// 列入;输入003，匹配201003.IB，211003.SH。
        /// 年限大的排在前面： 200210.IB>140210.IB
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public List<BondsInfoPo> GetBondsInfoPo3(string condition, int resultNum)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            if (string.IsNullOrEmpty(condition))
                return result;
            if (condition.Count() != 3)
                return result;
            string key = GetPluginShareKey(EPluginShareKey.BondByCode);
            if (string.IsNullOrWhiteSpace(key) || Application == null)
            {
                return result;
            }
            int retryCount = 100;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    if (LocalEnv.ConfInfo.CacheDigitLength >= 3)
                    {
                        var dic3 = ConstIndex.BondIndexThree;
                        Dictionary<string, BondsInfoPo> list;
                        if (!dic3.TryGetValue(condition, out list))
                        {
                            return result;
                        }

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var v = list.Values.OrderBy(k => k.Bond_ID.Length).ThenBy(k => k.listed_Market).ThenByDescending(k => k.Bond_ID);
                        if (v != null && v.Count() > 0)
                        {
                            int tempNum = 0;
                            foreach (var kv in v)
                            {
                                if (tempNum < resultNum)
                                    result.Add(kv);
                                else
                                    break;
                                tempNum++;
                            }
                        }

                        stopwatch.Stop();
                        LogHelper.Info($"ConstIndex.BondIndex search3 key={condition};listcount ={list.Count}; findCount={result.Count} time ={stopwatch.ElapsedMilliseconds}");
                    }
                    else
                    {
                        ConcurrentDictionary<string, BondsInfoPo> dic = (ConcurrentDictionary<string, BondsInfoPo>)Application.GetObject(key);

                        if (dic == null)
                        {
                            return result;
                        }

                        var v = dic.Where(d => getBondCodeNO(d.Key).EndsWith(condition)).OrderBy(k => k.Value.Bond_ID.Length).ThenBy(k => k.Value.listed_Market).ThenByDescending(k => k.Value.Bond_ID);
                        if (v != null && v.Count() > 0)
                        {
                            int tempNum = 0;
                            foreach (KeyValuePair<string, BondsInfoPo> kv in v)
                            {
                                if (tempNum < resultNum)
                                    result.Add(kv.Value);
                                else
                                    break;
                                tempNum++;
                            }
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(20);
                    LogHelper.Debug($"查询重试{i + 1}次");
                    if (i == retryCount - 1)
                    {
                        LogHelper.Error(ex);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取债券的纯数字代码 
        /// 例如: 20210.IB => 20210
        /// </summary>
        /// <param name="bondCode"></param>
        /// <returns></returns>
        private string getBondCodeNO(string bondCode)
        {
            string BondCodeNO = string.Empty;
            if (string.IsNullOrEmpty(bondCode))
                return BondCodeNO;
            string[] strs = bondCode.Split(new char[] { '.' });
            return strs[0];
        }
        /// <summary>
        /// 新版债券搜索算法
        /// 条件是3位的时候，优先使用3位末尾匹配算法，后面再追加老板算法结果
        /// 非3位条件的时候，使用老算法去搜索匹配债券。
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public List<BondsInfoPo> GetBondsInfoPoNew(string condition, int resultNum)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            if (string.IsNullOrEmpty(condition))
                return result;
            if (condition.Count() == 3)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                result = GetBondsInfoPo3(condition, resultNum);
                stopwatch.Stop();
                LogHelper.Info("===========333费时间：" + stopwatch.ElapsedMilliseconds);
                if (result.Count < resultNum)
                {
                    //Stopwatch stopwatch1 = new Stopwatch();
                    stopwatch.Start();
                    List<BondsInfoPo> result1 = GetBondsInfoPo(condition, resultNum);
                    stopwatch.Stop();
                    LogHelper.Info("===========1111费时间：" + stopwatch.ElapsedMilliseconds);
                    foreach (BondsInfoPo bond in result1)
                    {
                        if (result.Count < resultNum)
                            result.Add(bond);
                        else
                            break;
                    }
                }
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                result = GetBondsInfoPo(condition, resultNum);
                stopwatch.Stop();
                LogHelper.Info("===========2222费时间：" + stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        /// <summary>
        /// 根据bondid获取唯一的债券
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public BondsInfoPo GetBondsInfoPoOne(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return null;
            BondsInfoPo result = null;
            string key = GetPluginShareKey(EPluginShareKey.Bond);
            while (Application.GetObject(key) == null || ((Dictionary<string, BondsInfoPo>)Application.GetObject(key)).Count == 0)
            {
                Thread.Sleep(100);
            }
            Dictionary<string, BondsInfoPo> dic = (Dictionary<string, BondsInfoPo>)Application.GetObject(key);
            var v = dic.Where(d => d.Key.ToLower().Contains(condition.ToLower()) && d.Value.Bond_ID == condition).ToList();
            if (v != null && v.Count() > 0)
            {
                result = v.First().Value;
            }
            return result;
        }

        public BondsInfoPo GetBondsInfoByCode(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return null;
            BondsInfoPo result = null;
            string key = GetPluginShareKey(EPluginShareKey.BondByCode);
            int i = 0;
            while (Application.GetObject(key) == null || ((ConcurrentDictionary<string, BondsInfoPo>)Application.GetObject(key)).Count == 0)
            {
                if (i > 30)
                {
                    break;
                }
                Thread.Sleep(20);
                i++;
            }
            ConcurrentDictionary<string, BondsInfoPo> dic = (ConcurrentDictionary<string, BondsInfoPo>)Application.GetObject(key);
            if (dic != null && dic.Count > 0)
            {
                BondsInfoPo value;
                if (dic.TryGetValue(condition, out value))
                {
                    return value;
                }
            }
            return result;
        }

        public List<BondsInfoPo> GetBondsInfoPoByCon(string condition, int resultNum, out int flag)
        {
            flag = 0;
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            string key = GetPluginShareKey(EPluginShareKey.Bond);
            Dictionary<string, BondsInfoPo> dic = (Dictionary<string, BondsInfoPo>)Application.GetObject(key);
            IEnumerable<KeyValuePair<string, BondsInfoPo>> dicBonds = null;

            lock (dic)
            {
                //1.字符短的排在前面：说明匹配程度最高,2.按照交易所排序
                //var v = (from d in dic where d.Key.ToLower().Contains(condition.ToLower()) select d).OrderBy(k=>k.Value.BondID.Length).OrderBy(k=>k.Value.ListedMarket);
                //一、本意是先按照1.字符短的排在前面，2.长度相等的情况下再按照交易所排序
                //二、目前看下来1.先按交易所排序，2.再按字符长度排序同样达到上面【一】一样的效果 20200424 wym
                //先使用精准匹配,如果精准匹配到就不在匹配

                IEnumerable<KeyValuePair<string, BondsInfoPo>> dicBondsByID = dic.Where(
                o => o.Value.Bond_ID == condition
                || (!string.IsNullOrEmpty(o.Value.Bond_ID) ? (o.Value.Bond_ID.LastIndexOf('.') != -1 ? o.Value.Bond_ID.Substring(0, o.Value.Bond_ID.LastIndexOf('.')) : o.Value.Bond_ID) : "") == condition)
                    .OrderBy(k => k.Value.listed_Market).OrderBy(k => k.Value.Bond_ID.Length);
                if (dicBondsByID != null && dicBondsByID.Any())
                {
                    flag = 1;
                    dicBonds = dicBondsByID;
                }

                if (flag == 0)
                {
                    IEnumerable<KeyValuePair<string, BondsInfoPo>> dicBondsInfoPo = dic.Where(o => o.Value.Short_Name == condition
                    || (o.Value.pinyin_full != null && o.Value.pinYin == condition)
                    || (o.Value.pinyin_full != null && o.Value.pinyin_full == condition)).OrderBy(k => k.Value.listed_Market).OrderBy(k => k.Value.Bond_ID.Length);
                    if (dicBondsInfoPo != null && dicBondsInfoPo.Any())
                    {
                        flag = 2;
                        dicBonds = dicBondsInfoPo;
                    }
                }
                if (flag == 0)
                {
                    flag = 3;
                    dicBonds = dic.Where(d => d.Value.Bond_ID.ToLower().Contains(condition.ToLower())).OrderBy(k => k.Value.listed_Market).OrderBy(k => k.Value.Bond_ID.Length);
                }

                if (flag == 0)
                {
                    flag = 4;
                    dicBonds = dic.Where(d => d.Value.Short_Name.ToLower().Contains(condition.ToLower())
                    || d.Value.pinYin.ToLower().Contains(condition.ToLower())
                    || d.Value.pinyin_full.ToLower().Contains(condition.ToLower())).OrderBy(k => k.Value.listed_Market).OrderBy(k => k.Value.Bond_ID.Length);
                }

                //if (flag == 0)
                //{
                //    dicBonds = dic.Where(d => d.Key.ToLower().Contains(condition.ToLower())).OrderBy(k => k.Value.ListedMarket).OrderBy(k => k.Value.BondID.Length);
                //}

                int i = 0;
                foreach (KeyValuePair<string, BondsInfoPo> kv in dicBonds)
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

        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="pluginKey">pluginKey是怎么定义的</param>
        /// <param name="name">tbleName</param>
        /// <returns></returns>
        private DataTable GetDataTable(int pluginKey, string name)
        {
            string key = GetPluginShareKey(113, name);
            DataTable dtc = (DataTable)Application.GetObject(key);
            return dtc;
        }
        /// <summary>
        /// 根据pluginShareKey获取数据
        /// </summary>
        /// <param name="pluginKey"></param>
        /// <param name="name"></param>
        /// <remarks>替换DataTable GetDataTable(int pluginKey, string name)方法</remarks>
        /// <returns></returns>
        private DataTable GetDataTable(EPluginShareKey pluginShareKey)
        {
            string key = GetPluginShareKey(pluginShareKey);
            DataTable dtc = new DataTable();
            if (Application != null)
            {
                dtc = (DataTable)Application.GetObject(key);
            }
            return dtc;
        }

        /// <summary>
        /// 作废
        /// </summary>
        private string GetPluginShareKey(int pluginKey, string name)
        {
            return string.Format("{0}_{1}", pluginKey, name);
        }
        /// <summary>
        /// PluginShareKey
        /// 20200227 wym
        /// </summary>
        /// <remarks>替换GetPluginShareKey(int pluginKey, string name)方法</remarks>
        /// <returns></returns>
        private string GetPluginShareKey(EPluginShareKey pluginShareKey)
        {
            return string.Format("{0}_{1}", Math.Floor((int)pluginShareKey / 10.0), pluginShareKey.ToString());
        }


        public BondsInfoPo GetBondsInfoPoByCode(string bondCode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Contact

        public HashSet<InstTraderInfo> GetInstIdsByBrokerId(string brokerId)
        {
            HashSet<InstTraderInfo> instIds = new HashSet<InstTraderInfo>();
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            lock (dtc.Rows.SyncRoot)
            {
                foreach (DataRow dr in dtc.Rows)
                {
                    var bId = dr["managers"];
                    if (bId != null && bId.ToString().Contains(brokerId))
                    {
                        instIds.Add(new InstTraderInfo
                        {
                            InstId = dr["instid"].ToString(),
                            TraderId = dr["id"].ToString()
                        });
                    }
                }
            }
            return instIds;
        }

        /// <summary>
        /// 根据机构id和交易员名称获取Contact
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="instId"></param>
        /// <param name="contactName"></param>
        public DataTable GetContactByInstIdAndContactName(string instId, string contactName)
        {
            var sql = $"instid = '{instId}' and namecn = '{contactName}'";
            return SearchContactBySql(sql, 1);
        }


        public DataTable SearchContactBySql(string sql, int resultNum)
        {
            DataTable dtc = GetDataTable(1, "CRM_Contact");
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(sql).OrderBy(o => o["namecn"].ToString().Length).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("biztype like '%BNC%' and (namecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%')", condition)).Take(num);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("biztype like '%BNC%' and instid in ({0})", condition)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("biztype like '%BNC%' and namecn like '%{0}%'", condition)).Take(10);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("biztype like '%BNC%' and instid = '{0}'", condition)).Take(10);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = resultNum >= 0 ? dtc.Select(sql).OrderBy(_ => _.Field<string>("shortnamecn").Length).Take(resultNum) : dtc.Select(sql);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchInst(string condition)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or instcode like '%{0}%')", condition)).Take(20);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }


        public DataTable SearchInst2(string condition, int resultNum)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }

        public DataTable SearchInst3(string condition)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%' or pinyinfull like '%{0}%' or instbncshortname like '%{0}%')", condition)).Take(20);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn = '{0}'  or pinyin = '{0}')", condition)).Take(resultNum);
                    if (drs.Count() == 0)
                        drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%'  or pinyin like '%{0}%')", condition)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
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
        /// <summary>
        /// 根据机构id获取机构数据
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataRow SearchInstByInstId(string instid)
        {
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataRow result = null;

            // 等待数据初始化
            if (dtc == null || !dtc.Columns.Contains("idbkey"))
            {
                Thread.Sleep(3000);
            }
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("idbkey = '{0}'", instid));
                if (drs.Count() > 0)
                    result = drs[0];
            }

            return result;
        }
        /// <summary>
        /// 根据机构id和机构简称获取机构数据
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public DataRow SearchInstByInstIdAndInstName(string instid, string instname)
        {
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataRow result = null;
            lock (dtc.Rows.SyncRoot)
            {
                var drs = dtc.Select(string.Format("idbkey = '{0}'and shortnamecn = '{1}'", instid, instname));
                if (drs.Count() > 0)
                    result = drs[0];
            }

            return result;
        }
        public DataTable SearchQuoteInst(string condition)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and (shortnamecn like '%{0}%')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }
        public DataTable SearchQuoteByIdbkey(string condition)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%BOND%' and idbkey = '{0}'", condition)).Take(10);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }

            return result;
        }


        public DataTable SearchBillInst(string condition)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Inst");
            //20200227 wym
            if (!string.IsNullOrWhiteSpace(condition))
            {
                condition = condition.Replace("'", "").Replace("*", "");
            }
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Inst);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs = null;
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%bill%' and  billshortnamelist like '%{0}%' ", condition));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                if (drs != null && drs.Count() > 0)
                {
                    foreach (var dr in drs)
                    {
                        result.ImportRow(dr);
                    }
                }

                result.DefaultView.Sort = "shortnamecn asc";
                result = result.DefaultView.ToTable();
                DataTable result2 = dtc.Clone();
                try
                {
                    drs = dtc.Select(string.Format("taglist like '%bill%' and (shortnamecn like '%{0}%'or pinyin like '%{0}%' or instcode like '%{0}%')", condition));// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result2;
                }
                foreach (var dr in drs)
                {
                    result2.ImportRow(dr);
                }
                result2.DefaultView.Sort = "shortnamecn asc";
                result2 = result2.DefaultView.ToTable();

                result.Merge(result2);

            }

            return result;
        }



        #endregion

        #region Broker
        /// <summary>
        /// 通过SQL语句获取Broker信息
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="resultNum">返回数量</param>
        /// <returns></returns>
        public DataTable SearchBrokerBySql(string sql, int resultNum)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    if (resultNum > 0)
                    {
                        drs = dtc.Select(sql).Take(resultNum);
                    }
                    else
                    {
                        drs = dtc.Select(sql);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBroker(string matchName, string filterName)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {//过滤单引号：20200610 wxp
                matchName = matchName.Replace("'", "");
                filterName = filterName.Replace("'", "");
                matchName = matchName.Replace("'", "");
                filterName = filterName.Replace("'", "");
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
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {//过滤单引号：20200610 wxp
                matchName = matchName.Replace("'", "");
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }

        public DataTable SearchBroker3(string matchName, int resultNum)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {//过滤单引号：20200610 wxp
                matchName = matchName.Replace("'", "");
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account = '{0}' or AccountName = '{0}' or PinYin = '{0}')", matchName)).Take(resultNum);
                    if (drs.Count() == 0)
                        drs = dtc.Select(string.Format("Term_ids like '%2,%' and (Account like '%{0}%' or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchQuoteBroker(string matchName)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {//过滤单引号：20200610 wxp
                matchName = matchName.Replace("'", "");
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%2,%' and AccountName like '%{0}%'", matchName)).Take(10);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchQuoteBrokerByAccount(string account)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%2,%' and Account = '{0}'", account)).Take(1);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchBillBroker(string matchName, int resultNum)
        {
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                //解决sql中有单引号的问题 20200610 wym
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%Bill%' and (Account like '%{0}%'or AccountName like '%{0}%' or PinYin like '%{0}%')", matchName)).Take(resultNum);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }
        public DataTable SearchBillBrokerByName(string matchName)
        {
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {
                //解决sql中有单引号的问题 20200610 wym
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%Bill%' and (Account = '{0}'or AccountName = '{0}' or PinYin = '{0}')", matchName));// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
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
                    IEnumerable<DataRow> drs;
                    try
                    {
                        drs = dtc.Select(string.Format("taglist like '%bill%' and (idbkey = '{0}')", condition)).Take(10);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                        return result;
                    }
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
            //DataTable dtc = GetDataTable(1, "CRM_Account");
            //20200227 wym
            DataTable dtc = GetDataTable(EPluginShareKey.CRM_Account);
            DataTable result = dtc.Clone();
            lock (dtc.Rows.SyncRoot)
            {//过滤单引号：20200610 wxp
                matchName = matchName.Replace("'", "");
                matchName = matchName.Replace("'", "");
                IEnumerable<DataRow> drs;
                try
                {
                    drs = dtc.Select(string.Format("Term_ids like '%Bill%' and (Idb_key = '{0}')", matchName)).Take(resultNum);// or shortnamecn like '%{0}%' or insttypevalue like '%{0}%'
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    return result;
                }
                foreach (var dr in drs)
                {
                    result.ImportRow(dr);
                }
            }
            return result;
        }


        /// <summary>
        /// 根据idbKey获取机构信息
        /// </summary>
        /// <param name="idbKey"></param>
        /// <returns></returns>
        public DataGridDataPo GetInstByIdbKey(string idbKey)
        {
            string sql = string.Format("IdbKey='{0}'", idbKey);
            DataTable dt = Instance.SearchInstBySql(sql, 1);
            DataGridDataPo dg = new DataGridDataPo();
            foreach (DataRow row in dt.Rows)
            {
                dg.InstName = row["shortnamecn"] + ""; //GetInstName(row, null);
                dg.InstIdbKey = row["idbkey"] + "";
                dg.instcode = row["instcode"] + "";
                dg.PinYin = row["pinyin"] + "";
                dg.PinYinFull = row["pinyinfull"] + "";
                break;
            }
            return dg;
        }


        #endregion
    }
}
