using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using STC.Common.View.Component;
using STC.Common.Model.Fix.Message;
using System.Threading;
using System.Diagnostics;
using STC.Common.Model.CRM;
using STC.Common.Model.Table;
using STC.Common.Handler.Base;
using STC.Common.Data.DataModel;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Utils.SqlLite;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.ViewModel.Data;
using STC.Common.Model.FinQ;
using STC.Common.Handler.FinQ;

namespace STC.Common.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class CommonUtils
    {
        //public static ManualResetEvent _ChangeTab_ManualResetEvent = new ManualResetEvent(true);

        static ConcurrentDictionary<string, ChildTag> IssuerDic = new ConcurrentDictionary<string, ChildTag>();
        public static ChildTag GetIssuer(string TagValue)
        {
            ChildTag result = null;

            if (!string.IsNullOrEmpty(TagValue))
            {
                if (IssuerDic.ContainsKey(TagValue))
                {
                    result = IssuerDic[TagValue];
                }
                else
                {
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where childTagValue='{TagValue}'";
                    List<ChildTag> resultList = TagsSqliteManager.Instance.GetAllIssuers(sql);
                    if (resultList != null && resultList.Count > 0)
                    {
                        result = resultList[0];
                        IssuerDic[TagValue] = result;
                    }
                }
            }
            

            return result;

        }

        static ConcurrentDictionary<string, ChildTag> GuarantorDic = new ConcurrentDictionary<string, ChildTag>();
        public static ChildTag GetGuarantor(string TagValue)
        {
            ChildTag result = null;

            if (!string.IsNullOrEmpty(TagValue))
            {
                if (GuarantorDic.ContainsKey(TagValue))
                {
                    result = GuarantorDic[TagValue];
                }
                else
                {
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor where childTagValue='{TagValue}'";
                    List<ChildTag> resultList = TagsSqliteManager.Instance.GetAllGuarantors(sql);
                    if (resultList != null && resultList.Count > 0)
                    {
                        result = resultList[0];
                        GuarantorDic[TagValue] = result;
                    }
                }
            }


            return result;
        }
        /*
        //public static IForm Application;
        private const string IssuerKey = "115_Issuer";

        public static ChildTag GetIssuer(string TagValue)
        {
            ChildTag result = null;
            ConcurrentDictionary<string, ChildTag> dic = (ConcurrentDictionary<string, ChildTag>)AppUtils.Application.GetObject(IssuerKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                if (dic.ContainsKey(TagValue))
                {
                    result = dic[TagValue];
                }

                return result;

            }
        }

        public static List<ChildTag> GetIssuers(string condition, int resultNum = 2000)
        {
            List<ChildTag> result = new List<ChildTag>();
            ConcurrentDictionary<string, ChildTag> dic = (ConcurrentDictionary<string, ChildTag>)AppUtils.Application.GetObject(IssuerKey);

            if (dic == null)
            {
                return result;
            }

            lock (dic)
            {
                result = dic.Where(d => (d.Value.childTagName != null && d.Value.childTagName.Contains(condition))
                                          || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition)))
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }

            return result;
        }

        private const string GuarantorKey = "115_Guarantor";

        public static ChildTag GetGuarantor(string TagValue)
        {
            ChildTag result = null;
            ConcurrentDictionary<string, ChildTag> dic = (ConcurrentDictionary<string, ChildTag>)AppUtils.Application.GetObject(GuarantorKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                if (dic.ContainsKey(TagValue))
                {
                    result = dic[TagValue];
                }

                return result;

            }
        }

        public static List<ChildTag> GetGuarantors(string condition, int resultNum = 2000)
        {
            List<ChildTag> result = new List<ChildTag>();
            ConcurrentDictionary<string, ChildTag> dic = (ConcurrentDictionary<string, ChildTag>)AppUtils.Application.GetObject(GuarantorKey);

            if (dic == null)
            {
                return result;
            }

            lock (dic)
            {
                result = dic.Where(d => (d.Value.childTagName != null && d.Value.childTagName.Contains(condition))
                                          || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition)))
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }

            return result;
        }
        */
        private const string BondKey = "2_Bond";

        //public static HashSet<BondsInfoPo> HashSetBondsInfoPo = null;
        /// <summary>
        /// 获取债券基本信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<SimpleBondsInfoPo> GetBondsInfoPo(string condition, int resultNum, string listed_market = null, string marketindicatorfilter = null)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            //if (resultNum == 1)
            //{
            //    result = Common.Data.STCRocksDbHandler.Instance.GetBondsInfoPo(condition);
            //    return result;
            //}

            //string key = GetPluginShareKey(2, "Bond");
            SortedDictionary<string, SimpleBondsInfoPo> DictBondsInfoPo = (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (DictBondsInfoPo == null)
                return result;

            if (resultNum == 1 && listed_market == null)
            {
                if (DictBondsInfoPo.ContainsKey(condition))
                    result.Add(DictBondsInfoPo[condition]);
                return result;
            }

            lock (DictBondsInfoPo)
            {
                IEnumerable<KeyValuePair<string, SimpleBondsInfoPo>> v = null;
                if (listed_market != null)
                {
                    if (listed_market == "SSE" || listed_market == "SSZ")
                        v = (from d in DictBondsInfoPo
                             where (d.Value.Bond_ID.Contains(condition) || d.Value.Short_Name.Contains(condition) || (!string.IsNullOrWhiteSpace(d.Value.pinYin) && d.Value.pinYin.Contains(condition)) || (!string.IsNullOrWhiteSpace(d.Value.pinyin_full) && d.Value.pinyin_full.Contains(condition))) && d.Value.listed_Market == listed_market
                             select d);
                    else
                        v = (from d in DictBondsInfoPo where (d.Value.Bond_ID.Contains(condition) || d.Value.Short_Name.Contains(condition) || (!string.IsNullOrWhiteSpace(d.Value.pinYin) && d.Value.pinYin.Contains(condition)) || (!string.IsNullOrWhiteSpace(d.Value.pinyin_full) && d.Value.pinyin_full.Contains(condition))) && d.Value.listed_Market == listed_market && string.IsNullOrEmpty(d.Value.Security_Type_Id) == false select d);
                }
                else
                {
                    if (marketindicatorfilter == null)
                        v = (from d in DictBondsInfoPo where d.Value.Bond_ID.Contains(condition) || d.Value.Short_Name.Contains(condition) || (!string.IsNullOrWhiteSpace(d.Value.pinYin) && d.Value.pinYin.Contains(condition)) || (!string.IsNullOrWhiteSpace(d.Value.pinyin_full) && d.Value.pinyin_full.Contains(condition)) select d);
                    else
                        v = (from d in DictBondsInfoPo where (d.Value.Bond_ID.Contains(condition) || d.Value.Short_Name.Contains(condition) || (!string.IsNullOrWhiteSpace(d.Value.pinYin) && d.Value.pinYin.Contains(condition)) || (!string.IsNullOrWhiteSpace(d.Value.pinyin_full) && d.Value.pinyin_full.Contains(condition))) && d.Value.MarketIndicator == marketindicatorfilter select d);
                }
                int i = 0;
                foreach (KeyValuePair<string, SimpleBondsInfoPo> kv in v)
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
        /// 缓存用户使用过的债券基本信息
        /// </summary>
        static ConcurrentDictionary<string, BondsInfoPo> bondsInfoWithSecurityIDDic = new ConcurrentDictionary<string, BondsInfoPo>();
        public static BondsInfoPo GetBondsInfoPoBySecurityID(string securityID)
        {
            if (!string.IsNullOrEmpty(securityID))
            {
                if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
                {
                    return bondsInfoWithSecurityIDDic[securityID];
                }
                else
                {
                    //string str = STCRocksDb4BasicDataHandler.Instance.Get(STCRocksDb4BasicDataHandler.DbBondInfo, securityID);
                    //if (!string.IsNullOrEmpty(str))
                    //{
                    //    BondsInfoPo bondsInfoPo = CommonUtils.DeserializeObject<BondsInfoPo>(str);
                    //    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                    //    return bondsInfoPo;
                    //}
                    BondsInfoPo bondsInfoPo = BondsInfoSqliteManager.Instance.GetBondsInfoBySecurityID(securityID);
                    if (bondsInfoPo != null)
                    {
                        bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                        return bondsInfoPo;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// 是否是IRS
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool IsIRS(string securityID)
        {
            bool res = false;
            BondsInfoPo x = GetBondsInfoPoBySecurityID(securityID);
            if (x != null)
            {
                res = x.MarketIndicator.ToString().Trim() == "2";
            }

            return res;
        }

        /// <summary>
        /// 是否是交易所债券
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool IsSheBond(string securityID)
        {
            return securityID.EndsWith(".SH") || securityID.EndsWith(".SZ");
        }
        /// <summary>
        /// 设置PirceType
        /// </summary>
        public static void SetPriceType(string securityID, string priceType)
        {
            if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
            {
                bondsInfoWithSecurityIDDic[securityID].PriceType = priceType;
            }
            else
            {
                //string str = STCRocksDb4BasicDataHandler.Instance.Get(STCRocksDb4BasicDataHandler.DbBondInfo, securityID);
                //if (!string.IsNullOrEmpty(str))
                //{
                //    BondsInfoPo bondsInfoPo = CommonUtils.DeserializeObject<BondsInfoPo>(str);
                //    bondsInfoPo.PriceType = priceType;
                //    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                //}
                BondsInfoPo bondsInfoPo = BondsInfoSqliteManager.Instance.GetBondsInfoBySecurityID(securityID);
                if (bondsInfoPo != null)
                {
                    bondsInfoPo.PriceType = priceType;
                    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                }
            }
        }

        /// <summary>
        /// 获取PirceType
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static string GetPriceType(string securityID)
        {
            string outstr = string.Empty;
            if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
            {
                outstr = bondsInfoWithSecurityIDDic[securityID].PriceType;
            }
            else
            {
                //string str = STCRocksDb4BasicDataHandler.Instance.Get(STCRocksDb4BasicDataHandler.DbBondInfo, securityID);
                //if (!string.IsNullOrEmpty(str))
                //{
                //    BondsInfoPo bondsInfoPo = CommonUtils.DeserializeObject<BondsInfoPo>(str);
                //    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                //    outstr = bondsInfoPo.PriceType;
                //}
                BondsInfoPo bondsInfoPo = BondsInfoSqliteManager.Instance.GetBondsInfoBySecurityID(securityID);
                if (bondsInfoPo != null)
                {
                    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                    outstr = bondsInfoPo.PriceType;
                }
            }
            return outstr;
        }

        public static CCDCValuaModel GenerateCCDCValue(MarketDataSnapshotFullRefresh obj)
        {
            CCDCValuaModel srcccdc = new CCDCValuaModel();
            string ccdcValueDate = obj.CCDCValuaDate == null ? "" : obj.CCDCValuaDate;
            if (!string.IsNullOrEmpty(ccdcValueDate) && ccdcValueDate.Length == 8)
            {
                ccdcValueDate = $"{obj.CCDCValuaDate.Substring(0, 4)}/{obj.CCDCValuaDate.Substring(4, 2)}/{obj.CCDCValuaDate.Substring(6, 2)}";
            }
            srcccdc.SecurityID = obj.SecurityID;
            srcccdc.CCDCValuaDate = ccdcValueDate;
            if ("1".Equals(obj.CCDCValuaType))
            {
                srcccdc.CCDCValuation = obj.CCDCValuation2 == null ? "" : obj.CCDCValuation2;
                srcccdc.CCDCValuationShow = string.IsNullOrEmpty(srcccdc.CCDCValuation) ? "" : $"行权: {srcccdc.CCDCValuation.Split('|')[0]}";
            }
            else
            {
                srcccdc.CCDCValuation = obj.CCDCValuation == null ? "" : obj.CCDCValuation;
                srcccdc.CCDCValuationShow = string.IsNullOrEmpty(srcccdc.CCDCValuation) ? "" : $"中债: {srcccdc.CCDCValuation}";
            }
            srcccdc.CCDCValuaType = string.IsNullOrEmpty(obj.CCDCValuaType) ? "2" : obj.CCDCValuaType;
            srcccdc.LastWorkDay = string.IsNullOrEmpty(obj.LastWorkDay) ? "" : obj.LastWorkDay;
            srcccdc.ShowLastWorkDay = string.Empty;
            if (!string.IsNullOrEmpty(srcccdc.LastWorkDay))
            {
                if (srcccdc.LastWorkDay.Replace("-", "").Replace("/", "") != srcccdc.CCDCValuaDate.Replace("-", "").Replace("/", ""))
                {
                    srcccdc.ShowLastWorkDay = "!";
                }
            }
            srcccdc.OrderCount = obj.Volume;
            return srcccdc;
        }

        /// <summary>
        /// 设置中债估值
        /// </summary>
        public static void SetCCDCValua(string securityID, CCDCValuaModel ccdcvalua)
        {
            ccdcvalua.SecurityID = securityID;

            if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
            {
                bondsInfoWithSecurityIDDic[securityID].CCDCValua = ccdcvalua;
            }
            else
            {
                //string str = STCRocksDb4BasicDataHandler.Instance.Get(STCRocksDb4BasicDataHandler.DbBondInfo, securityID);
                //if (!string.IsNullOrEmpty(str))
                //{
                //    BondsInfoPo bondsInfoPo = CommonUtils.DeserializeObject<BondsInfoPo>(str);
                //    bondsInfoPo.CCDCValua = ccdcvalua;
                //    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                //}
                BondsInfoPo bondsInfoPo = BondsInfoSqliteManager.Instance.GetBondsInfoBySecurityID(securityID);
                if (bondsInfoPo != null)
                {
                    bondsInfoPo.CCDCValua = ccdcvalua;
                    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                }
            }
        }
        /// <summary>
        /// 是否有缓存债券信息
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool HasCacheBondInfo(string securityID)
        {
            return bondsInfoWithSecurityIDDic.ContainsKey(securityID);
        }

        /// <summary>
        /// 获取 中债估值
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static CCDCValuaModel GetCCDCValua(string securityID)
        {
            CCDCValuaModel outstr = null;
            if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
            {
                outstr = bondsInfoWithSecurityIDDic[securityID].CCDCValua;
            }
            else
            {
                //string str = STCRocksDb4BasicDataHandler.Instance.Get(STCRocksDb4BasicDataHandler.DbBondInfo, securityID);
                //if (!string.IsNullOrEmpty(str))
                //{
                //    BondsInfoPo bondsInfoPo = CommonUtils.DeserializeObject<BondsInfoPo>(str);
                //    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                //    outstr = bondsInfoPo.CCDCValua;
                //}
                BondsInfoPo bondsInfoPo = BondsInfoSqliteManager.Instance.GetBondsInfoBySecurityID(securityID);
                if (bondsInfoPo != null)
                {
                    bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                    outstr = bondsInfoPo.CCDCValua;
                }
                
            }
            return outstr;
        }

        /// <summary>
        /// 判断是否是双边债券
        /// </summary>
        /// <param name="bondsInfo"></param>
        /// <returns></returns>
        public static bool IsCIB(BondsInfoPo item)
        {
            return item != null && item.listed_Market == "CIB" && string.IsNullOrEmpty(item.Security_Type_Id) == false;
        }
        public static bool checkBondVenue(string Venue, BondsInfoPo bonds) 
        {
            bool status = true;
            if (("SSE".Equals(Venue) || "SSF".Equals(Venue)) && !bonds.Bond_ID.EndsWith(".SH"))
            {
                status = false;
            }
            if (("SZF".Equals(Venue) || "SZE".Equals(Venue)) && !bonds.Bond_ID.EndsWith(".SZ"))
            {
                status = false;
            }
            return status;
        }
        public static bool checkMarket(BondsInfoPo item,string venue)
        {
            return item != null && item.listed_Market == ("SSE".Equals(venue) || "SSF".Equals(venue) ? "SSE" : "");
        }
        /// <summary>
        /// 得到所有债券基本信息
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<string, SimpleBondsInfoPo> GetAllBaseBondsInfo()
        {
            //string key = GetPluginShareKey(pluginKey, name);
            return (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey); ;
        }

        private static string IsEndOfLoadFromDBKey = "2_IsEndOfLoadFromDB";

        /// <summary>
        /// 判断债券基本信息是否已经加载完毕
        /// </summary>
        /// <returns></returns>
        public static bool IsEndofLoadFromDB()
        {
            if (IsEndOfLoadFromDBKey != null)
            {
                if (AppUtils.Application.GetObject(IsEndOfLoadFromDBKey) != null)
                {
                    return ((List<bool>)AppUtils.Application.GetObject(IsEndOfLoadFromDBKey))[0];
                }
            }
            return false;
        }

        public static string AdjustSecurityID(string securityID)
        {
            string temp = securityID;
            if (securityID.IndexOf(".") < 0)
            {
                temp = securityID + ".IB";
                if (CommonUtils.GetBondsInfoPoBySecurityID(temp) == null)
                {
                    temp = securityID + ".SH";
                    if (CommonUtils.GetBondsInfoPoBySecurityID(temp) == null)
                    {
                        temp = securityID + ".SZ";
                        if (CommonUtils.GetBondsInfoPoBySecurityID(temp) != null)
                        {
                            return temp;
                        }
                    }
                    else
                    {
                        return temp;
                    }
                }
                else
                {
                    return temp;
                }
            }
            return securityID;
        }

        //private static string GetPluginShareKey(int pluginKey, string name)
        //{
        //    return string.Format("{0}_{1}", pluginKey, name);
        //}

        /// <summary>
        /// 对象转字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertToString(object obj)
        {
            return obj == null ? string.Empty : obj.ToString();
        }

        /// <summary>
        /// 对象转字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertToString(object obj, string defaultValue)
        {
            return (obj == null || obj.ToString().Length == 0) ? defaultValue : obj.ToString();
        }

        /// <summary>
        /// 字符串转整型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int ConvertToInt(string obj)
        {
            try
            {
                int temp = Convert.ToInt32(Convert.ToDouble(obj));
                return temp;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        /// <summary>
        /// 字符串转长整型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long ConvertToLong(string obj)
        {
            if (obj == "--")
            {
                return 0;
            }
            try
            {
                long temp = Convert.ToInt64(Convert.ToDouble(obj));
                return temp;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        /// <summary>
        /// 字符串转Decimal, 默认是Null
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal? ConvertToDecimalOrNull(string obj)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(obj, out temp))
                    return temp;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        /// <summary>
        /// 对象转Decimal， 默认是0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(string obj)
        {
            try
            {
                decimal temp = decimal.Zero;
                decimal.TryParse(obj, out temp);
                return temp;
            }
            catch (Exception e)
            {

            }
            return decimal.Zero;
        }

        /// <summary>
        /// 数字格式化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="num">小数点位数</param>
        /// <returns></returns>
        public static string formatString(string obj, int num)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(obj, out temp))
                    return temp.ToString("N" + num);

                return obj;
            }
            catch (Exception e)
            {

            }
            return obj;
        }

        /// <summary>
        /// 价格数字格式化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="num">小数点位数</param>
        /// <returns></returns>
        public static string formatPriceString(string obj, int num)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(obj, out temp))
                    return temp.ToString("F" + num);

                return obj;
            }
            catch (Exception e)
            {

            }
            return obj;
        }

        /// <summary>
        /// 字符串转双精度浮点型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ConvertToDouble(string obj)
        {
            try
            {
                double temp = 0;
                double.TryParse(obj, out temp);
                return temp;
            }
            catch (Exception e)
            {

            }
            return 0;
        }

        /// <summary>
        /// 字符串转DateTime， 默认是服务器时间
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string obj)
        {
            try
            {
                DateTime dateTime = DateTime.Parse(obj);
                return dateTime;
            }
            catch (Exception e)
            {

            }
            return DateTimeEx.GetNow();
        }

        public static string ConvertTime(string time)
        {
            try
            {
                string[] arr = time.Split(new string[] { " ", "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    time = time.Contains("/") ? arr[1] : arr[0];

                    arr = time.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 2)
                    {
                        if (arr[0].Length == 1)
                        {
                            time = string.Format("0{0}:{1}:{2}", arr[0], arr[1], arr[2]);
                        }
                    }

                }
            }
            catch (Exception e)
            {

            }

            return time;
        }

        /// <summary>
        /// 字符串转DateTime格式， 默认是 DateTime.MinValue
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string year, string month, string day)
        {
            try
            {
                int y = 0;
                int m = 0;
                int d = 0;
                DateTime dateTime = DateTime.MinValue;
                if (int.TryParse(year, out y) && int.TryParse(month, out m) && int.TryParse(day, out d))
                    dateTime = new DateTime(y, m, d);


                return dateTime;
            }
            catch (Exception e)
            {

            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// 把yyyy-MM-dd转成yyyyMMdd
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ParseTimeString(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 10)
                return str.Substring(0, 10).Replace("-", "");

            return str;
        }

        /// <summary>
        /// 字符串转换为DateTime
        /// </summary>
        /// <param name="dateStr"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime2(string dateStr)
        {
            try
            {
                if (dateStr.Length == 8)
                {
                    string year = dateStr.Substring(0, 4);
                    string month = dateStr.Substring(4, 2);
                    string day = dateStr.Substring(6, 2);

                    DateTime dt = ConvertToDateTime(year, month, day);
                    return dt;
                }
            }
            catch
            {
                return DateTimeEx.ServerTime;
            }
            return DateTimeEx.ServerTime;
        }

        public static string ConvertTimeFormat(string time)
        {
            string result = time;

            if (time.Length > 19)
                result = time.Substring(0, time.Length - 4);

            return result;
        }

        /// <summary>
        /// 带筛选条件获得子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : Visual
        {
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject v = (DependencyObject)VisualTreeHelper.GetChild(parent, i);
                T child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v, predicate);
                    if (child != null)
                    {
                        return child;
                    }
                }
                else
                {
                    if (predicate(child))
                    {
                        return child;
                    }
                    else
                    {
                        return GetVisualChild<T>(child, predicate);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获得子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualChild<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// 查找父控件
        /// </summary>
        /// <typeparam name="T">父控件的类型</typeparam>
        /// <param name="obj">要找的是obj的父控件</param>
        /// <param name="name">想找的父控件的Name属性</param>
        /// <returns>目标父控件</returns>
        public static T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }

                // 在上一级父控件中没有找到指定名字的控件，就再往上一级找
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// 查找父控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetParentObject<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T)
                {
                    return (T)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }



        /// <summary>
        /// Get a bool value indicate whether is the VerticalScrollBar at buttom
        /// </summary>
        /// <returns>A bool value indicate whether is the VerticalScrollBar at buttom</returns>
        public static bool IsVerticalScrollBarAtButtom(ScrollViewer scrollViewer)
        {

            bool isAtButtom = false;

            // get the vertical scroll position
            double dVer = scrollViewer.VerticalOffset;

            //get the vertical size of the scrollable content area
            double dViewport = scrollViewer.ViewportHeight;

            //get the vertical size of the visible content area
            double dExtent = scrollViewer.ExtentHeight;

            if (dVer != 0)
            {
                if (dVer + dViewport == dExtent)
                {
                    isAtButtom = true;
                }
                else
                {
                    isAtButtom = false;
                }
            }
            else
            {
                isAtButtom = false;
            }

            if (scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled
                || scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
            {
                isAtButtom = true;
            }

            return isAtButtom;
        }

        /// <summary>
        /// 获取bond基本信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<SimpleBondsInfoPo> GetODMBondsInfoPo(string condition, int resultNum)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            //string key = GetPluginShareKey(2, "Bond");
            SortedDictionary<string, SimpleBondsInfoPo> dic = (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                var v = from d in dic where d.Value.Bond_ID.ToLower().Contains(condition.ToLower()) select d;

                int i = 0;
                foreach (KeyValuePair<string, SimpleBondsInfoPo> kv in v)
                {
                    if ("Y".Equals(kv.Value.flag))
                    {
                        if (i < resultNum)
                            result.Add(kv.Value);
                        else
                            break;

                        i++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 判断是否是数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
            }
            return false;
        }

        //// <summary>  
        /// 判断一个字符串是否为合法数字(指定整数位数和小数位数)  
        /// </summary>  
        /// <param name="s">字符串</param>  
        /// <param name="precision">整数位数</param>  
        /// <param name="scale">小数位数</param>  
        /// <returns></returns>  
        public static bool IsNumber(string s, int precision, int scale)
        {
            if ((precision == 0) && (scale == 0))
            {
                return false;
            }
            string pattern = @"(^\d{1," + precision + "}";
            if (scale > 0)
            {
                pattern += @"\.\d{0," + scale + "}$)|" + pattern;
            }
            pattern += "$)";
            return Regex.IsMatch(s, pattern);
        }

        //// <summary>  
        /// 判断一个字符串是否为合法负数(指定整数位数和小数位数)  
        /// </summary>  
        /// <param name="s">字符串</param>  
        /// <param name="precision">整数位数</param>  
        /// <param name="scale">小数位数</param>  
        /// <returns></returns>  
        public static bool IsNegative(string s, int precision, int scale)
        {
            if ((precision == 0) && (scale == 0))
            {
                return false;
            }
            string pattern = @"(^-\d{1," + precision + "}";
            if (scale > 0)
            {
                pattern += @"\.\d{0," + scale + "}$)|" + pattern;
            }
            pattern += "$)";
            return Regex.IsMatch(s, pattern);
        }

        /// <summary>
        /// 价格格式化
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public static string FormatPrice(string price)
        {
            if (CommonUtils.IsNumeric(price))
            {
                price = CommonUtils.ConvertToDecimal(price).ToString("N4");
            }
            else
            {
                if (string.IsNullOrEmpty(price))
                {
                    price = "--";
                }
            }
            return price;
        }

        /// <summary>
        /// 价格格式化
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public static string FormatPriceN3(string price)
        {
            if (CommonUtils.IsNumeric(price))
            {
                price = CommonUtils.ConvertToDecimal(price).ToString("N3");
            }
            else
            {
                if (string.IsNullOrEmpty(price))
                {
                    price = "--";
                }
            }
            return price;
        }
        /// <summary>
        /// 判断是否是利率债， 如果是BNC:利率债
        /// 如果是BCO，ABS,  NCD 都属于信用债
        /// </summary>
        /// <returns></returns>
        public static bool IsRate(string goodsType)
        {
            return "BNC" == goodsType;
        }

        ///// <summary>
        ///// 如果是BCO，ABS,  NCD 都属于信用债
        ///// </summary>
        ///// <param name="goodsType"></param>
        ///// <returns></returns>
        //public static bool IsSecurity(string goodsType)
        //{
        //    return "BCO" == goodsType || "ABS" == goodsType || "NCD" == goodsType;
        //}


        /// <summary>
        /// 根据bond_subType和 ent_cor获得债券类型名称
        /// </summary>
        /// <param name="bond_subType"></param>
        /// <param name="ent_cor"></param>
        /// <returns></returns>
        public static string GetBondCategoryName(string bond_subType, string ent_cor)
        {
            if (string.IsNullOrEmpty(bond_subType) == false)
            {
                string bond_subType_upper = bond_subType.ToUpper();
                switch (bond_subType_upper)
                {
                    // 【利率债】 
                    case "BGB":
                    case "SGB":
                    case "EGB":
                        return "国债";
                    case "SCB":
                        return "央票";
                    case "PBB":
                        return "金融债";
                    case "TLB":
                    case "LLB":
                        return "地方政府债";
                    // 【信用债】
                    case "CCP":
                    case "CSP":
                    case "LCP":
                    case "LSP":
                        return "短融";
                    case "CMN":
                    case "LMN":
                        return "中票";
                    case "CEB":
                    case "LEB":
                        if (ent_cor == "ENT")
                            return "企业债";
                        else if (ent_cor == "COP")
                            return "公司债";
                        else
                            return string.Empty;
                    case "PPN":
                        if (ent_cor == "COP")
                            return "公司债";
                        else if (ent_cor == null)
                            return "PPN";
                        else
                            return string.Empty;
                    case "AMP":
                    case "LBS":
                    case "CBS":
                    case "MBS":
                        return "ABS";//资产支持证券
                    case "PSB":
                    case "CBB":
                    case "CSB":
                    case "CXB":
                    case "NCB":
                    case "SFB":
                    case "SEB":
                    case "SSB":
                    case "SES":
                    case "HJB":
                        return "金融机构债";
                    case "CCB":
                    case "COB":
                        return "集合债";
                    case "SCV":
                    case "CVB":
                        return "可转债";
                    case "MCD":
                    case "SPD":
                    case "SHD":
                    case "CCD":
                    case "RRD":
                    case "RTD":
                    case "FRD":
                    case "OTD":
                        return "NCD";
                    case "CRM":
                    case "CLN":
                        return "CRM";
                    case "RAB":
                    case "TET":
                    case "INT":
                    case "PDB":
                        return "其他";
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 自定义债券归类， 便于筛选
        /// A: 国债
        /// B: 央票
        /// C: 金融债
        /// D: 地方政府债
        /// E: 短融
        /// F: 中票
        /// G: 企业债
        /// H: 公司债
        /// I:   NCD
        /// J:PPN
        /// K:ABS
        /// L:CRM
        /// </summary>
        /// <param name="bond_subType"></param>
        /// <param name="ent_cor"></param>
        /// <returns></returns>
        public static string GetBondCategory(string bond_subType, string ent_cor)
        {
            string bondCategoryName = CommonUtils.GetBondCategoryName(bond_subType, ent_cor);
            switch (bondCategoryName)
            {
                case "国债":
                    return bondCategoryName;
                case "央票":
                    return bondCategoryName;
                case "金融债":
                    return bondCategoryName;
                case "地方政府债":
                    return bondCategoryName;
                case "短融":
                    return bondCategoryName;
                case "中票":
                    return bondCategoryName;
                case "企业债":
                    return bondCategoryName;
                case "公司债":
                    return bondCategoryName;
                case "NCD":
                    return bondCategoryName;
                case "PPN":
                    return bondCategoryName;
                case "ABS":
                    return bondCategoryName;
                case "CRM":
                    return bondCategoryName;
                default:
                    return bondCategoryName;
            }
        }


        //selectSettle:T+0:0, T+1:1
        public static string IsSpotSettle(DateTime createTime, DateTime? selectDate, int selectSettleIndex)
        {
            if (selectDate != null && selectDate.Value != null)
            {
                DateTime dtNow = new DateTime(createTime.Year, createTime.Month, createTime.Day);
                DateTime dtSelect = new DateTime(selectDate.Value.Year, selectDate.Value.Month, selectDate.Value.Day);

                int days = (dtSelect - dtNow).Days;

                if (days == 0 && selectSettleIndex == 0)
                    return "1";//T+0
                else if (days == 0 && selectSettleIndex == 1)
                    return "2";// T+1
                else if (days == 1 && selectSettleIndex == 0)
                    return "3";// 明天+0
            }

            return string.Empty;
        }

        /// <summary>
        /// 判断是否是明天
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="selectDate"></param>
        /// <param name="selectSettleIndex"></param>
        /// <returns></returns>
        public static bool IsTomorrow(DateTime createTime, DateTime? selectDate, int selectSettleIndex)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string secondday = createTime.AddDays(1).ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                string firstday = createTime.ToString("yyyyMMdd");

                //明天+0=T+1
                if ((secondday.CompareTo(select) == 0 && selectSettleIndex == 0) || (firstday.CompareTo(select) == 0 && selectSettleIndex == 1))
                    result = true;

            }

            return result;
        }

        /// <summary>
        /// 根据settyltype和tradeData， 计算界面显示的selectDateTime和selectIndex
        /// </summary>
        /// <param name="settlType"></param>
        /// <param name="tradeDate"></param>
        /// <param name="selectDateTime"></param>
        /// <param name="selectIndex"></param>
        public static void SetSettleType(string settlType, string tradeDate, out DateTime selectDateTime, out int selectIndex)
        {
            DateTime tradeTime = CommonUtils.ConvertToDateTime2(tradeDate);

            selectDateTime = tradeTime;
            selectIndex = 0;
            if (settlType == "1")
            {
                selectDateTime = tradeTime;
                selectIndex = 0;
            }
            else if (settlType == "2")
            {
                selectDateTime = tradeTime;
                selectIndex = 1;
            }
            else if (settlType == "3")
            {
                selectDateTime = tradeTime;
                selectIndex = 0;
            }
            else if (settlType == "4")
            {
                selectDateTime = tradeTime;
                selectIndex = 1;
            }
            else if (settlType != null && settlType.Contains("+"))
            {

                string[] arr = settlType.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                int month = 0;
                int day = 0;
                if (arr != null && arr.Length > 1)
                {
                    string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (dtStr != null && dtStr.Length > 1
                                && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                    {
                        DateTime dt = new DateTime(DateTimeEx.ServerTime.Year, month, day);
                        try
                        {
                            selectDateTime = dt;
                        }
                        catch (Exception e) { }
                    }

                    if (arr[1].Equals("0"))
                    {
                        selectIndex = 0;

                    }
                    else if (arr[1].Equals("1"))
                    {
                        selectIndex = 1;
                    }
                }
            }
        }

        /// <summary>
        /// 格式化SettleType
        /// </summary>
        /// <param name="tradedate"></param>
        /// <param name="fastSettlType"></param>
        /// <returns></returns>
        public static string FormatSettlType(string tradedate, string fastSettlType)
        {
            DateTime tradeTime = CommonUtils.ConvertToDateTime2(tradedate);
            if (fastSettlType == "1" || fastSettlType == "3")
            {
                return $"{tradeTime.ToString("MM.dd")}+0";
            }
            else if (fastSettlType == "2" || fastSettlType == "4")
            {
                return $"{tradeTime.ToString("MM.dd")}+1";
            }
            return fastSettlType;
        }

        /// <summary>
        /// newSettleType 转换成标准清算速度格式(1,2,3,4,mm.dd+0,mm.dd+1)
        /// </summary>
        /// <param name="settleType">交割日期 yyyyMMdd</param>
        /// <returns></returns>
        public static string CalSettlDate(string baseDate, string settleType, out string newSettleType)
        {
            newSettleType = settleType;
            try
            {
                CalWeekDateReq requst = new CalWeekDateReq();
                requst.settlementType = settleType;// "1";
                if (!string.IsNullOrEmpty(baseDate))
                    requst.baseDate = baseDate;

                CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                if (result != null && result.Data != null)
                {
                    newSettleType = result.Data.settlementType2;
                    return result.Data.settlementDate;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return string.Empty;
        }

        /// <summary>
        /// 计算交易日期
        /// </summary>
        /// <param name="settleType"></param>
        /// <returns></returns>
        public static string CalTradeDate(string settleType)
        {
            try
            {
                if (!string.IsNullOrEmpty(settleType))
                {

                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null && result.Data != null)
                    {
                        return result.Data.dealDate;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return string.Empty;
        }


        public static string GetOrderId(string id)
        {
            return !string.IsNullOrEmpty(id) ? id.Substring(0, id.Length - 1) : id;
        }


        /// <summary>
        /// 设置债券类型
        /// </summary>
        /// <param name="goodcode"></param>
        /// <returns></returns>
        public static string SetGoodsShortname(string goodcode)
        {
            string goodsType = string.Empty;

            if (!string.IsNullOrEmpty(goodcode))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(goodcode);
                if (bondsInfoPo != null)
                {
                    goodsType = bondsInfoPo.SECURITY_TYPE;

                    if (!string.IsNullOrEmpty(goodsType) && (goodsType.ToUpper().Equals("ABS") || goodsType.ToUpper().Equals("NCD")))
                        goodsType = "BCO";
                }
            }
            else
                goodsType = "BNC";//默认利率

            return goodsType;
        }

        public static string FormatNum(string num)
        {
            string result = num;

            if (!string.IsNullOrEmpty(num) && result.Contains("."))
            {
                int index = result.IndexOf(".");
                int differ = num.Length - index - 1;
                if (result.EndsWith("0"))
                {
                    if (differ > 2)
                    {
                        result = result.Substring(0, result.Length - 1);
                        result = FormatNum(result);
                    }
                }
                else if (differ < 2)
                {
                    result += differ == 0 ? "00" : differ == 1 ? "0" : string.Empty;
                }
            }
            else if (num == "--")
            {
                result = "0.00";
            }
            else if (num == "Bid" || num == "Ofr" 
                || num == CommonConstants.NO_PRICE 
                || num == CommonConstants.NO_PERMISSION 
                || num == CommonConstants.NO_BROKER)
            {

            }
            else
            {
                result += ".00";
            }

            return result;
        }

        public static string FormatNum3(string num)
        {
            string result = num;

            if (!string.IsNullOrEmpty(num) && result.Contains("."))
            {
                int index = result.IndexOf(".");
                int differ = num.Length - index - 1;
                if (result.EndsWith("0"))
                {
                    if (differ > 3)
                    {
                        result = result.Substring(0, result.Length - 1);
                        result = FormatNum3(result);
                    }
                }
                else if (differ < 2)
                {
                    result += differ == 0 ? "00" : differ == 1 ? "0" : string.Empty;
                }
            }
            else if (num == "--")
            {
                result = "0.00";
            }
            else
            {
                result += ".00";
            }

            return result;
        }

        /// <summary>
        /// 格式化BP，显示范围 -9999 ~ 9999 ，去无效 0 ，最多展示2位小数，去掉末0后整体长度仍大于6位，再省略bp
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public static string FormatLimitBp(decimal dp)
        {
            string str = dp.ToString("0.##");
            decimal newbp = Convert.ToDecimal(str);
            if (Math.Abs(newbp) > 9999)
            {
                newbp = newbp > 0 ? 9999 : -9999;
            }

            str = newbp.ToString().Length >= 5 ? newbp.ToString() : newbp.ToString() + "bp";
            if (str.Length > 6)
            {
                str = dp.ToString("0");
            }

            return str;
        }
        public static string FormatLimitNetPx(decimal dp)
        {
            string str = dp.ToString("0.###");
            decimal newbp = Convert.ToDecimal(str);
            if (Math.Abs(newbp) > 9999)
            {
                newbp = newbp > 0 ? 9999 : -9999;
            }

            str = newbp.ToString().Length >= 5 ? newbp.ToString() : newbp.ToString();
            if (str.Length > 6)
            {
                str = dp.ToString("0");
            }

            return str;
        }
        /// <summary>
        /// 转换 Linux时间戳为日期时间
        /// </summary>
        /// <param name="timesSamp"></param>
        /// <returns></returns>
        public static DateTime ConvertLinuxTimeToDatetime(long timesSamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            DateTime dt = startTime.AddMilliseconds(timesSamp);
            return dt;
        }

        public static void Prompt2Second(string title, string text, Window win = null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text, win);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text, win);
                }));
            }
        }


        private static void Sub_Prompt2Second(string title, string text, Window win = null)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.Topmost = true;
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            //prompt.Owner = win == null ? Application.Current.MainWindow : win;
            prompt.Show();
        }

        /// <summary>
        /// 通过合约编号和MarketIndicator判断期货类型
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns>0=非期货，1=国债期货， 2=商品期货</returns>
        public static int GetFutureType(string securityID, string marketindicator)
        {
            if (!Consts.MARKETINDICATOR_FUTURE.Equals(marketindicator))
            {
                return 0;
            }
            else
            {
                if (Regex.IsMatch(securityID, @"^T[0-9]{4}") || Regex.IsMatch(securityID, @"^TF[0-9]{4}") || Regex.IsMatch(securityID, @"^TS[0-9]{4}"))
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        /// <summary>
        /// 判断国债期货
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool IsTreasuryFuture(string securityID)
        {
            if (!string.IsNullOrEmpty(securityID))
            {
                return Regex.IsMatch(securityID, @"^T[0-9]{4}") || Regex.IsMatch(securityID, @"^TF[0-9]{4}") || Regex.IsMatch(securityID, @"^TS[0-9]{4}");
            }
            return false;
        }

        /// <summary>
        /// 判断是否期货
        /// </summary>
        /// <param name="marketindicator"></param>
        /// <returns></returns>
        public static bool IsFutureByMarketIndicator(string marketindicator)
        {
            if (string.Equals(marketindicator, Consts.MARKETINDICATOR_FUTURE))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据securityTypeID获得债券类型名称
        /// </summary>
        /// <param name="securityTypeID"></param>
        /// <returns>债券类型名称</returns>
        public static string GetBondCategoryName(string securityTypeID)
        {
            if (string.IsNullOrEmpty(securityTypeID) == false)
            {
                switch (securityTypeID)
                {
                    // 【利率债】 
                    case "100001":
                        return "国债";
                    case "100003":
                        return "政策性金融债";
                    case "100011":
                        return "地方政府债";
                    case "100002":
                        return "央票";
                    // 【信用债】
                    case "100027":
                    case "100052":
                        return "政府支持机构债";
                    case "100004":
                    case "100006":
                    case "100010":
                    case "100029":
                    case "100050":
                    case "100055":
                    case "100057":
                    case "100073":
                    case "100025":
                        return "公司债";
                    case "100005":
                    case "100007":
                    case "100009":
                    case "100022":
                    case "100023":
                    case "100024":
                    case "100026":
                    case "100028":
                    case "100053":
                    case "100054":
                    case "100056":
                    case "100082":
                    case "100083":
                        return "金融债";
                    case "100041":
                        return "NCD";
                    case "100071":
                    case "100072":
                        return "ABS";
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据securityId获得债券类型名称
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns>债券类型名称</returns>
        public static string GetBondCategoryNameBySecurityId(string securityId)
        {
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                if (!string.IsNullOrEmpty(bondsInfo.Security_Type_Id))
                {
                    return GetBondCategoryName(bondsInfo.Security_Type_Id);
                }
                else
                {
                    return GetBondCategory(bondsInfo.Bond_Subtype, bondsInfo.Ent_Cor);
                }

            }
            return string.Empty;
        }

        //开平仓标志显示
        public static string GetOffsetFlagDisplay(string code)
        {
            switch (code)
            {
                case Consts.OPEN_CODE:
                    return Consts.OPEN;
                case Consts.CLOSE_CODE:
                    return Consts.CLOSE;
                case Consts.CLOSE_TODAY_CODE:
                    return Consts.CLOSE_TODAY;
                case Consts.CLOSE_YESTERDAY_CODE:
                    return Consts.CLOSE_YESTERDAY;
                default:
                    return string.Empty;
            }
        }

        public static int GetDigit(decimal minchange)
        {
            switch (CommonUtils.ConvertToString(minchange))
            {
                case Consts.PRICE_MIN_CHANGE_0002:
                case Consts.PRICE_MIN_CHANGE_0005:
                    return 3;
                case Consts.PRICE_MIN_CHANGE_002:
                case Consts.PRICE_MIN_CHANGE_005:
                    return 2;
                case Consts.PRICE_MIN_CHANGE_01:
                case Consts.PRICE_MIN_CHANGE_02:
                case Consts.PRICE_MIN_CHANGE_05:
                    return 1;
                case Consts.PRICE_MIN_CHANGE_1:
                case Consts.PRICE_MIN_CHANGE_2:
                case Consts.PRICE_MIN_CHANGE_5:
                case Consts.PRICE_MIN_CHANGE_10:
                    return 0;
                case Consts.PRICE_MIN_CHANGE_00001:
                default:
                    return 4;
            }
        }

        public static int GetDigit(string minchange)
        {
            switch (minchange)
            {
                case Consts.PRICE_MIN_CHANGE_0002:
                case Consts.PRICE_MIN_CHANGE_0005:
                    return 3;
                case Consts.PRICE_MIN_CHANGE_002:
                case Consts.PRICE_MIN_CHANGE_005:
                    return 2;
                case Consts.PRICE_MIN_CHANGE_01:
                case Consts.PRICE_MIN_CHANGE_02:
                case Consts.PRICE_MIN_CHANGE_05:
                    return 1;
                case Consts.PRICE_MIN_CHANGE_1:
                case Consts.PRICE_MIN_CHANGE_2:
                case Consts.PRICE_MIN_CHANGE_5:
                case Consts.PRICE_MIN_CHANGE_10:
                    return 0;
                case Consts.PRICE_MIN_CHANGE_00001:
                default:
                    return 4;
            }
        }

        //通过合约编号获取期货合约的最小变动价格
        public static string GetMinChangeBySecurityID(string securityID)
        {
            //STOPWATCH.Restart();

            //默认0.0001
            string minchange = Consts.PRICE_MIN_CHANGE_00001;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (bondsInfo != null)
            {
                if (!string.IsNullOrEmpty(bondsInfo.Min_change))
                {
                    minchange = bondsInfo.Min_change;
                }
            }
            //else
            //{
            //    int i = 0;
            //    //合约缓存未加载时，取不到合约信息，等到取到合约缓存。(取不到10次之后返回)
            //    while (bondsInfo == null && i<10)
            //    {
            //        bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //        Thread.Sleep(2000);
            //        i++;
            //    }
            //}

            //STOPWATCH.Stop();
            //LogHandler.Debug("GetMinChangeBySecurityID({0}) 耗时{1}ms", securityID, STOPWATCH.ElapsedMilliseconds);
            return minchange;
        }

        public static Stopwatch STOPWATCH = Stopwatch.StartNew();



        public static string[] exSecurityArr = new string[] { "SH", "SZ", "sh", "sz" };
        /// <summary>
        /// 是否是Broker债券
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns></returns>
        public static bool IsBrokerSecurity(string securityID)
        {
            bool isBrokerSecurity = true;
            foreach (string str in exSecurityArr)
            {
                if (securityID.ToUpper().EndsWith(str))
                {
                    isBrokerSecurity = false;
                    break;
                }
            }

            return isBrokerSecurity;
        }


        #region 获取机构信息

        private const string InstitutionKey = "113_CRM_Inst"; //机构表
        private const string ContactKey = "113_CRM_Contact"; //交易员表
        /// <summary>
        /// 获取机构信息
        /// </summary>
        /// <param name="condition">关键字</param>
        /// <param name="resultNum">数量</param>
        /// <returns></returns>
        public static List<CrmInst> GetInstitutionsInfoPo(string condition, int resultNum, List<string> instidList = null)
        {
            List<CrmInst> result = new List<CrmInst>();
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dic == null)
            {
                return result;
            }

            //int i = 0;
            //var v = (from d in dic where d.Key.ToLower().Contains(condition) select d);

            if (instidList == null || instidList.Count == 0)
            {
                result = dic.Where(d => string.IsNullOrEmpty(condition)
                                      || (d.Value.shortnamecn != null && d.Value.shortnamecn.ToUpper().Contains(condition.ToUpper()))
                                      || (d.Value.pinyin != null && d.Value.pinyin.ToUpper().Contains(condition.ToUpper()))
                                      || (d.Value.pinyinfull != null && d.Value.pinyinfull.ToUpper().Contains(condition.ToUpper()))
                                      || (d.Value.bncshortnamelist != null && d.Value.bncshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                      || (d.Value.bcoshortnamelist != null && d.Value.bcoshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                      || (d.Value.ncdshortnamelist != null && d.Value.ncdshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                          )
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }
            else
            {
                result = dic.Where(d => (string.IsNullOrEmpty(condition)
                                          || (d.Value.shortnamecn != null && d.Value.shortnamecn.ToUpper().Contains(condition))
                                          || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition))
                                          || (d.Value.pinyinfull != null && d.Value.pinyinfull.Contains(condition)))
                                          && instidList.Contains(d.Value.idbkey))
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }

            return result;
        }


        /// <summary>
        /// 根据机构id 获取机构名称
        ///  @ requirement 1.8.1.:zs51
        ///@author sz:lihuan
        ///@data 2021/12/16 10:08
        /// </summary>
        /// <param name="insid">机构id</param>
        /// <returns></returns>
        public static List<CrmInst> GetInstitutionsIDInfoPo(string insid)
        {
            List<CrmInst> result = new List<CrmInst>();

            ConcurrentDictionary<string, CrmInst> dicContant = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dicContant == null)
            {
                return null;
            }

            result = dicContant.Where(d => d.Value.idbkey != null && d.Value.idbkey == insid)
                    .Select(d => d.Value)
                        .ToList();
            return result;
        }


        public static List<CrmInst> GetInstitutionsInfoPo(List<string> instidList = null)
        {
            List<CrmInst> result;
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (instidList == null || instidList.Count == 0)
            {
                result = dic.Select(d => d.Value).ToList();
            }
            else
            {
                result = dic.Where(d => instidList.Contains(d.Value.idbkey))
                           .Select(d => d.Value)
                           .ToList();
            }
            return result;
        }


        public static List<CrmContact> GetContactInfoPo(string condition, string instid)
        {
            List<CrmContact> result = new List<CrmContact>();
            ConcurrentDictionary<string, CrmContact> dic = (ConcurrentDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {

                result = dic.Where(d => (string.IsNullOrEmpty(condition) || (!string.IsNullOrEmpty(d.Value.namecn) && (d.Value.namecn.Contains(condition) || d.Value.idbkey == condition)))
                                            && (string.IsNullOrEmpty(instid) || (!string.IsNullOrEmpty(d.Value.instid) && d.Value.instid == instid)))
                       .Select(d => d.Value)
                       .ToList();//多条件筛选，新功能增加

                return result;

            }
        }

        #endregion


        #region 记录常用机构和交易员
        // 缓存常用交易员信息
        private static List<CrmContact> _MyContactList = new List<CrmContact>();
        private static List<CrmInst> _MyInstList = new List<CrmInst>();
        private static string MY_CONTACT_KEY = "MyTradeList";
        private static string MY_INST_KEY = "MyInstList";
        static object userCrmLock = new object();
        static bool isUserCrmInit = false;

        public static void LoadUserCrm()
        {
            lock (userCrmLock)
            {
                if (isUserCrmInit == true)
                {
                    return;
                }
                isUserCrmInit = true;

                string instStr = IniFileHelper.ReadValue($"Work_{LocalEnv.UserInfo.UserName}", MY_INST_KEY);
                string[] instArr = instStr.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                for (int i = instArr.Length - 1; i >= 0; i--)
                {
                    AddInst(instArr[i]);
                }

                string traderStr = IniFileHelper.ReadValue($"Work_{LocalEnv.UserInfo.UserName}", MY_CONTACT_KEY);
                string[] traderArr = traderStr.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                for (int i = traderArr.Length - 1; i >= 0; i--)
                {
                    AddContract(traderArr[i]);
                }
            }
        }

        public static void SaveContract()
        {
            StringBuilder traderSB = new StringBuilder();

            foreach (CrmContact item in _MyContactList)
            {
                if (traderSB.Length > 0)
                {
                    traderSB.Append(",");
                }
                traderSB.Append(item.idbkey);
            }

            IniFileHelper.WriteValue($"Work_{LocalEnv.UserInfo.UserName}", MY_CONTACT_KEY, traderSB.ToString());
        }

        public static void AddContract(string traderIdbkey)
        {
            if (string.IsNullOrEmpty(traderIdbkey))
            {
                return;
            }

            List<CrmContact> traderList = null;
            if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
            {
                traderList = GetContactInfoPo(traderIdbkey, string.Empty);
            }
            //else
            //{
            //    traderList = GetDBContactInfoPo(null, null, traderIdbkey);
            //}

            if (traderList != null)
            {
                foreach (CrmContact contract in traderList)
                {
                    if (_MyContactList.Any(item => item.idbkey == contract.idbkey))
                    {
                        _MyContactList.RemoveAll(item => item.idbkey == contract.idbkey);
                    }
                    else
                    {
                        if (_MyContactList.Count == 10)
                        {
                            _MyContactList.RemoveAt(9);
                        }
                    }
                    _MyContactList.Insert(0, contract);
                }
            }
        }

        public static List<CrmContact> GetMyContract()
        {
            return _MyContactList;
        }

        public static void SaveInst()
        {
            StringBuilder instSB = new StringBuilder();
            foreach (CrmInst item in _MyInstList)
            {
                if (instSB.Length > 0)
                {
                    instSB.Append(",");
                }
                instSB.Append(item.idbkey);
            }
            IniFileHelper.WriteValue($"Work_{LocalEnv.UserInfo.UserName}", MY_INST_KEY, instSB.ToString());
        }

        public static void AddInst(string instIdbKey)
        {
            if (string.IsNullOrEmpty(instIdbKey))
            {
                return;
            }

            List<CrmInst> crmInstList = GetInstitutionsIDInfoPo(instIdbKey);

            if (crmInstList != null)
            {
                foreach (CrmInst inst in crmInstList)
                {
                    if (_MyInstList.Any(item => item.idbkey == inst.idbkey))
                    {
                        _MyInstList.RemoveAll(item => item.idbkey == inst.idbkey);
                    }
                    else
                    {
                        if (_MyInstList.Count == 10)
                        {
                            _MyInstList.RemoveAt(9);
                        }
                    }
                    _MyInstList.Insert(0, inst);
                }
            }
        }

        public static List<CrmInst> GetMyInstList()
        {
            return _MyInstList;
        }

        #endregion

        public static long GetDays(string text)
        {
            long days = 0;

            if (!string.IsNullOrEmpty(text))
            {
                string temp = text.Replace(" ", string.Empty);
                temp = temp.ToUpper();

                char[] charArr = temp.ToCharArray();

                int start = 0;
                int end = 0;
                for (int i = 0; i < charArr.Length; i++)
                {
                    end = i + 1;
                    if (charArr[i] == 'Y')
                    {
                        string y = temp.Substring(start, end - start - 1);
                        days += (long)(y.ConvertToDecimal() * 365);
                        start = end;
                    }
                    else if (charArr[i] == 'D')
                    {
                        string d = temp.Substring(start, end - start - 1);
                        days += (long)d.ConvertToDecimal();
                        start = end;
                    }
                }

                if (start != end)
                {
                    string y = temp.Substring(start, end - start);
                    days += (long)(y.ConvertToDecimal() * 365);
                }
            }


            return days;
        }

        /// <summary>
        /// 设置提示窗口图标
        /// </summary>
        /// <param name="obj"></param>
        public static void SetIcon(Image obj)
        {
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "dmart":
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/dmart_windows_ico_32.ico"));
                    obj.Height = 18;
                    break;
                case "gtja":
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/gtja_windows_ico_64.ico"));
                    obj.Height = 18;
                    break;
                case "idbpro":
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/idbpro_windows_icon_64.ico"));
                    obj.Height = 18;
                    break;
                case "bdb":
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/bdb_windows_icon_128.ico"));
                    obj.Height = 18;
                    break;
                case "idcclient":
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/bdb_windows_icon_128.ico"));
                    obj.Height = 18;
                    break;
                default:
                    obj.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_win_logo.png"));
                    obj.Height = 12;
                    break;
            }
        }


        /// <summary>
        /// 是否是Spread 或 Basic
        /// </summary>
        /// <param name="securityid"></param>
        /// <returns></returns>
        public static bool IsSpreadOrBasic(string securityid)
        {
            return !string.IsNullOrEmpty(securityid) && (securityid.Contains("*") || securityid.Contains("S3M/R07"));
        }

        public static string SerializeObject(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);

        }

        public static T DeserializeObject<T>(string tmp)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(tmp);
        }

        public static char[] PLUS_CHAR = new char[] { '+' };
        /// <summary>
        /// 把基础剩余时间转换成时间段
        /// 如：2Y+5D -> 2 * 365
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        public static int ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            int convertTime = 0;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split(PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    string firstTime_str = splitArr[0].Trim();
                    if (firstTime_str.EndsWith("Y"))
                    {
                        string yearStr = firstTime_str.Replace("Y", "");
                        decimal year = CommonUtils.ConvertToDecimal(yearStr);
                        convertTime = (int)(365 * year);
                    }
                    else if (firstTime_str.EndsWith("D"))
                    {
                        string dayStr = firstTime_str.Replace("D", "");
                        int day = CommonUtils.ConvertToInt(dayStr);
                        convertTime = day;
                    }
                }
            }

            return convertTime;
        }

        public static bool IsPointInScreen(Point point, System.Drawing.Rectangle screenBounds)
        {
            return point.X >= screenBounds.Left &&
                   point.X <= screenBounds.Right &&
                   point.Y >= screenBounds.Top &&
                   point.Y <= screenBounds.Bottom;
        }

        #region GR 债券信息特殊算法
        /// <summary>
        /// 新的GR获取债券信息功能
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<SimpleBondsInfoPo> GetBondGridData(string condition, int resultNum)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();

            if (condition.Length == 3)
            {
                result = GetBondsInfoPo3(condition, resultNum);

                if (result.Count < resultNum)
                {
                    List<SimpleBondsInfoPo> result1 = GetBondsInfoPoNew(condition, resultNum);

                    foreach (SimpleBondsInfoPo bond in result1)
                    {
                        if (result.Count < resultNum)
                        {
                            List<string> lstkey = result.Select(o => o.Bond_ID).ToList();
                            if (!lstkey.Contains(bond.Bond_ID))
                                result.Add(bond);
                        }
                        else
                            break;
                    }
                }

            }
            else
            {
                result = GetBondsInfoPoNew(condition, resultNum);
            }
            //债券类型:1信用,2利率,11 ABS,12 NCD

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
        public static List<SimpleBondsInfoPo> GetBondsInfoPo3(string condition, int resultNum)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            SortedDictionary<string, SimpleBondsInfoPo> DictBondsInfoPo = (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (DictBondsInfoPo == null)
            {
                return result;
            }

            if (string.IsNullOrEmpty(condition))
                return result;

            if (condition.Length != 3)
                return result;

            List<SimpleBondsInfoPo> valuesList = new List<SimpleBondsInfoPo>();
            
            if (LocalEnv.UserInfo.TermIds.Count == 0)
            {
                valuesList = DictBondsInfoPo.Values.Where(p => getBondCodeNO(p.Bond_ID).EndsWith(condition.ToLower())).ToList();
            }
            else
            {
                valuesList = DictBondsInfoPo.Values.Where(p => getBondCodeNO(p.Bond_ID).EndsWith(condition.ToLower()) && LocalEnv.UserInfo.TermIds.Contains(p.symbolType)).ToList();
            }

            valuesList = DictBondsInfoPo.Values.Where(p => getBondCodeNO(p.Bond_ID).EndsWith(condition.ToLower())).ToList();

            //修改排序
            foreach (var stock in valuesList.Where(s => s.Bond_ID.Contains(".")))
            {
                if (!stock.Bond_ID.EndsWith(".BJ"))
                    stock.Full_Name_EN = stock.Bond_ID.Substring(stock.Bond_ID.IndexOf('.') + 1);
                else
                    stock.Full_Name_EN = "ZBJ";
            }

            if (resultNum > 0)
            {
                result = valuesList.OrderBy(k => k.Full_Name_EN).ThenBy(k => k.Bond_ID.Length).ThenByDescending(k => k.Bond_ID).Take(resultNum).ToList();
            }
            else
            {
                result = valuesList.OrderBy(k => k.Full_Name_EN).ThenBy(k => k.Bond_ID.Length).ThenByDescending(k => k.Bond_ID).ToList();
            }

            return result;
        }

        /// <summary>
        /// 获取债券的纯数字代码 
        /// 例如: 20210.IB => 20210
        /// </summary>
        /// <param name="bondCode"></param>
        /// <returns></returns>
        private static string getBondCodeNO(string bondCode)
        {
            string BondCodeNO = string.Empty;
            if (string.IsNullOrEmpty(bondCode))
                return BondCodeNO;
            string[] strs = bondCode.Split(new char[] { '.' });
            return strs[0];
        }

        /// <summary>
        /// <summary>
        /// 新版债券搜索算法
        /// 条件是3位的时候，优先使用3位末尾匹配算法，后面再追加老板算法结果
        /// 非3位条件的时候，使用老算法去搜索匹配债券。
        ///1.字符短的排在前面：说明匹配程度最高,2.按照交易所排序
        ///var v = (from d in dic where d.Key.ToLower().Contains(condition.ToLower()) select d).OrderBy(k=>k.Value.BondID.Length).OrderBy(k=>k.Value.ListedMarket);
        ///一、本意是先按照1.字符短的排在前面，2.长度相等的情况下再按照交易所排序
        ///二、目前看下来1.先按交易所排序，2.再按字符长度排序同样达到上面【一】一样的效果 20200424 wym
        ///先使用精准匹配,如果精准匹配到就不在匹配
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <param name="listed_market"></param>
        /// <param name="marketindicatorfilter"></param>
        /// <returns></returns>
        public static List<SimpleBondsInfoPo> GetBondsInfoPoNew(string condition, int resultNum = int.MaxValue, string listed_market = null, string marketindicatorfilter = null)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            if (string.IsNullOrWhiteSpace(condition))
            {
                return result;
            }

            SortedDictionary<string, SimpleBondsInfoPo> DictBondsInfoPo = (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (DictBondsInfoPo == null)
            {
                return result;
            }

            lock (DictBondsInfoPo)
            {
                List<SimpleBondsInfoPo> valuesList = new List<SimpleBondsInfoPo>();
                
                valuesList = DictBondsInfoPo.Values.Where(p => p.Bond_ID.Contains(condition) || p.Short_Name.Contains(condition) || (!string.IsNullOrWhiteSpace(p.pinYin) && p.pinYin.Contains(condition)) || (!string.IsNullOrWhiteSpace(p.pinyin_full) && p.pinyin_full.Contains(condition))).ToList();

                if (LocalEnv.UserInfo.TermIds.Count >0)
                {
                    valuesList = valuesList.Where(p => LocalEnv.UserInfo.TermIds.Contains(p.symbolType)).ToList();
                }
                
                //修改排序
                foreach (var stock in valuesList.Where(s => s.Bond_ID.Contains(".")))
                {
                    if (!stock.Bond_ID.EndsWith(".BJ"))
                        stock.Full_Name_EN = stock.Bond_ID.Substring(stock.Bond_ID.IndexOf('.') + 1);
                    else
                        stock.Full_Name_EN = "ZBJ";
                }

                if (valuesList.Count > 0)
                {
                    if (condition.ToLower().Length > 2)
                    {
                        if (resultNum > 0)
                        {
                            //result = valuesList.OrderBy(k => k.Bond_ID.Length).ThenBy(k => k.listed_Market).ThenByDescending(k => k.Bond_ID).Take(resultNum).ToList();
                            result = valuesList.OrderBy(k => k.Bond_ID.Length).ThenBy(k => k.Full_Name_EN).Take(resultNum).ToList();
                        }
                        else
                        {
                            result = valuesList.OrderBy(k => k.Bond_ID.Length).ThenBy(k => k.Full_Name_EN).ToList();
                        }
                    }
                    else
                    {
                        if (resultNum > 0)
                        {
                            result = valuesList.Take(resultNum).ToList();
                        }
                        else
                        {
                            result = valuesList;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 台子类型转换
        /// BCO——信用 1
        /// BNC——利率 2
        /// ABS——ABS  11
        /// NCD——NCD  12
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static string BizTypeConvert(string ids)
        {
            string result = string.Empty;
            string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string id in idArr)
            {
                if (id == "1")
                    //result += string.IsNullOrEmpty(result) ? "BCO" : ",BCO";
                    stringBuilder.Append(",BCO");
                else if (id == "2")
                    //result += string.IsNullOrEmpty(result) ? "BNC" : ",BNC";
                    stringBuilder.Append(",BNC");
                else if (id == "11")
                    //result += string.IsNullOrEmpty(result) ? "ABS" : ",ABS";
                    stringBuilder.Append(",ABS");
                else if (id == "12")
                    //result += string.IsNullOrEmpty(result) ? "NCD" : ",NCD";
                    stringBuilder.Append(",NCD");
                else if (id == "14")
                    //result += string.IsNullOrEmpty(result) ? "IRS" : ",IRS";
                    stringBuilder.Append(",IRS");
            }
            result = stringBuilder.ToString().Trim(',');

            return result;
        }

        /// <summary>
        /// 初始化用户可用的查询数据集合
        /// </summary>
        public static void InitDictBondsInfoPoAndHashSetBondsInfoPo()
        {
            ////SortedDictionary<string, BondsInfoPo> DictBondsInfoPo = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            ////lock (DictBondsInfoPo)
            ////{
            ////    HashSet<BondsInfoPo> Tmphash = new HashSet<BondsInfoPo>(DictBondsInfoPo.Values);
            ////    List<BondsInfoPo> valuesList = new List<BondsInfoPo>();
            ////    string name = BizTypeConvert(LocalEnv.UserInfo.Termids);
            ////    List<string> termIds = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            ////    if (termIds.Count == 0)
            ////    {
            ////        valuesList = Tmphash.ToList();
            ////    }
            ////    else if (termIds.Count > 0)
            ////    {
            ////        valuesList = Tmphash.Where(p => termIds.Contains(p.symbolType)).ToList();
            ////    }

            ////    //债券类型:/// BCO——信用 1    /// BNC——利率 2  /// ABS——ABS  11  /// NCD——NCD  12
            ////    //使用 LINQ 更新 ListMarket 属性

            ////    foreach (var stock in valuesList.Where(s => s.Bond_ID.Contains(".")))
            ////    {
            ////        if (!stock.Bond_ID.EndsWith(".BJ"))
            ////            stock.Full_Name_EN = stock.Bond_ID.Substring(stock.Bond_ID.IndexOf('.') + 1);
            ////        else
            ////            stock.Full_Name_EN = "ZBJ";
            ////    }

            ////    HashSetBondsInfoPo = new HashSet<BondsInfoPo>(valuesList);
            ////}
        }

        //private static List<string> _Termids = null;
        //private static bool _isQuery = false;
        //public static List<string> AccountPower()
        //{
        //    if (!_isQuery)
        //    {
        //        string name = BizTypeConvert(LocalEnv.UserInfo.Termids);
        //        _Termids = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //        return _Termids;
        //    }
        //    else
        //    {
        //        return _Termids;
        //    }
        //}

        #endregion

        #region GR 弹窗清算速度转换
        /// <summary>
        /// 准备废弃
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string ConvertSettlType(object value, object parameter)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string instr = (value.ToString());
            string[] arrstr = instr.Split(',');
            if (arrstr.Length == 0)
            {
                return string.Empty;
            }
            if ("popup".Equals(parameter))
            {
                if (arrstr.Length == 2 && arrstr.Contains("2") && arrstr.Contains("3"))
                {
                    return "+1";
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (string settl in arrstr)
            {
                if (settl.Equals("1"))
                {
                    sb.AppendLine("+0");
                }
                else if (settl.Equals("2"))
                {
                    sb.AppendLine("+1");
                }
                else if (settl.Equals("3"))
                {
                    sb.AppendLine("明+0");
                }
                else
                {
                    sb.AppendLine(settl);
                }
            }

            string[] lines = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string newster = string.Join(",", lines);
            if (newster.EndsWith(","))
            {
                newster = newster.Substring(0, newster.Length - 1);
            }
            return newster;

        }



        #region GR特殊的清算速度处理
        public static bool IsDealHistDay(ExecutionReport er)
        {
            DateTime dt;
            if (DateTime.TryParseExact(er.TradeTime, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 成交记录，结算类型显示（历史数据）
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public static string GetHistoryDisplaySettle(ExecutionReport er)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非错期
            if (!string.IsNullOrEmpty(er.SettlType2) && er.ExecType.Equals(er.SettlType2))
            {
                ///有备注
                if (!string.IsNullOrEmpty(er.Remark))
                {
                    dtDeal = GetDate(er.TradeDate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + er.Remark;
                }
                else
                {
                    dtDeal = GetDate(er.TradeDate);
                    DateTime dtSettle = GetDate(er.SettlDate);
                    DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    var time = dtSettle - dtDeal;
                    if (time.Days == 0)
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+0";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(er.SettlType2);
                    }
                    else
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+1";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(er.SettlType2);
                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(er.SettlType2) && !string.IsNullOrEmpty(er.SettlType))
                {
                    //ofr
                    ofrResult = GetHistorySettle(er, true);
                    //bid
                    bidResult = GetHistorySettle(er, false);

                    if (ofrResult.Equals(bidResult))
                        result = ofrResult;
                    else
                        result = string.Format("{0}/{1}", ofrResult, bidResult);
                }
                else if (!string.IsNullOrEmpty(er.SettlType) && string.IsNullOrEmpty(er.SettlType2))
                {
                    result = GetHistorySettle(er, false);
                }
                else if (string.IsNullOrEmpty(er.SettlType) && !string.IsNullOrEmpty(er.SettlType2))
                {
                    //ofr
                    result = GetHistorySettle(er, true);
                }
            }

            return result;
        }

        public static string GetHistorySettle(ExecutionReport orderDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? orderDetailPo.SettlDate2 : orderDetailPo.SettlDate;
            string dealTime = isOfr ? orderDetailPo.TradeDate2 : orderDetailPo.TradeDate;
            string settle = isOfr ? orderDetailPo.SettlType2 : orderDetailPo.SettlType;

            string result = string.Empty;

            DateTime dtDeal, dtSettle;
            dtSettle = GetDate(settleTime);
            dtDeal = GetDate(dealTime);

            var time = dtSettle - dtDeal;
            //成交和结算在同一天
            if (time.Days == 0)
            {
                time = dtDeal - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (time.Days == 0)
                    ///今天成交
                    result = "+0";
                else
                    ///远期
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                        + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+0";

            }
            else if (time.Days != 0) //+1或者跨节假日 //周周六+0的时候 ，交易日和结算日不是同一天
            {
                time = dtDeal - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (time.Days == 0)
                {
                    result = GetDefautSettle(settle);
                }
                else
                {
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "." + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(settle);
                }
            }
            return result;
        }

        /// <summary>
        /// 成交记录，结算类型显示
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public static string GetDisplaySettle(ExecutionReport er)
        {
            string DefaultSettle1 = "+1,明天+0";
            string DefaultSettle2 = "明天+0,+1";
            string SettleDisplay2 = "";
            string SettleDisplay = "";
            if (DefaultSettle2.Equals(er.SettlType2) && er.SettlType2.Contains(DefaultSettle2))
            {
                SettleDisplay2 = "";
            }
            else
            {
                SettleDisplay2 = GetSettleDisplay(er.SettlType2, er.TradeDate2);
            }

            if (DefaultSettle2.Equals(er.SettlType) && er.SettlType.Contains(DefaultSettle2))
            {
                SettleDisplay = "";
            }
            else
            {
                SettleDisplay = GetSettleDisplay(er.SettlType, er.TradeDate);
            }

            string result = string.Empty;
            if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(er.Remark))
                {
                    DateTime dtDeal = GetDate(er.TradeDate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + er.Remark;
                }
                else
                    result = SettleDisplay;
            }
            else
            {
                if (!string.IsNullOrEmpty(er.SettlType2) && !string.IsNullOrEmpty(er.SettlType))
                    result = string.Format("{0}/{1}", SettleDisplay2, SettleDisplay);
                else if (!string.IsNullOrEmpty(er.SettlType) && string.IsNullOrEmpty(er.SettlType2))
                    result = SettleDisplay;
                else if (string.IsNullOrEmpty(er.SettlType) && !string.IsNullOrEmpty(er.SettlType2))
                    result = SettleDisplay2;
            }
            return result;
        }

        /// <summary>
        /// 获取默认结算 
        /// 周三+1 =》 +1
        /// 明天+1 =》 +1
        /// 10.11+0 =》 +0
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public static string GetDefautSettle(string settle)
        {
            if (string.IsNullOrEmpty(settle))
            {
                return string.Empty;
            }
            if (settle.Contains("+0"))
                return "+0";
            if (settle.Contains("+1"))
                return "+1";
            return string.Empty;
        }

        /// <summary>
        /// 获取显示的结算类型
        /// 成交日期不在显示明天或者周几，除了+0和+1以外，全部显示日期，展示为10.14+0
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="dealDate"></param>
        /// <returns></returns>
        public static string GetSettleDisplay(string settle, string dealDate)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(settle))
            {
                return result;
            }
            string settleStr = settle;
            switch (settle)
            {
                case "1":
                    settleStr = "+0";
                    break;
                case "2":
                    settleStr = "+1";
                    break;
                case "3":
                    settleStr = "明天+0";
                    break;

            }
            if (IsDefautSettle(settleStr))
                result = settleStr;
            else
            {
                if (string.IsNullOrEmpty(dealDate))
                {
                    result = settleStr;
                }
                else
                {
                    DateTime dtDeal = GetDate(dealDate);
                    string dealStr = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString());
                    result = dealStr + GetDefautSettle(settleStr);
                }
            }
            return result;
        }

        public static DateTime GetDate(string time)
        {
            DateTime result;
            DateTime.TryParseExact(time, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);

            return result;
        }

        /// <summary>
        /// 是否是默认结算
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public static bool IsDefautSettle(string settle)
        {
            return settle + "" == "+0" || settle + "" == "+1";
        }
        #endregion

        #endregion

        public static string ToBinary(long x)
        {
            char[] buff = new char[64];

            for (int i = 63; i >= 0; i--)
            {
                buff[i] = x % 2 == 1 ? '1' : '0';
                x = x / 2;
            }

            return new string(buff);
        }

        public static string CalBond(string securityID, string calType, string settleType, string val)
        {
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = securityID;
                requst.CalType = calType;//"4";
                requst.SettlType = settleType;// "1";
                requst.Value = val;// customPrice;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (requst != null && result.Data != null)
                {
                    if (calType.Equals("4"))
                    {
                        return result.Data.CleanPrice.ToString("N4");
                    }
                    else if (calType.Equals("2"))
                    {
                        return (result.Data.Yield * 100).ToString("N4");
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return "";
        }

        /// <summary>
        /// 根据PriceType计算其他值
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="settleType"></param>
        /// <param name="val">对应pricetype相同类型的值</param>
        /// <param name="priceType">1：净价；2： 全价；3： 到期收益率；7：行权收益率</param>
        /// <returns>如果是收益率或行权收益率要乘100</returns>
        public static CalResult CalBond_PriceType(string securityID, string settleType, string val, string priceType)
        {
            //Accrued("1"),CleanPrice("2"),FullPrice("3"),Yield("4"),Spread("5"),SpreadAndPredictRate("6"),yieldToExecution("7");
            string calType = string.Empty;
            if (priceType == "1")// 原始为净价
            {
                calType = "2";
            }
            else if (priceType == "2")//原始为全价
            {
                calType = "3";
            }
            else if (priceType == "7")//原始为行权收益率
            {
                calType = "7";
            }
            else if (priceType == "3")//原始为到期收益率
            {
                calType = "4";
            }

            if (!string.IsNullOrEmpty(calType))
            {
                // 计算收益率， 净价
                CalReq requst = new CalReq();
                requst.SecurityID = securityID;
                requst.CalType = calType;
                requst.SettlType = settleType;
                if (calType == "4" || calType == "7")
                {
                    // 如果是行权收益率，到期收益率 
                    requst.Value = (val.ConvertToDecimal() / 100).ToString();
                }
                else
                {
                    requst.Value = val;
                }

                CalResult result = CalHandler.Instance.CalBond(requst);
                return result;
                //if (result != null && result.Data != null)
                //{
                //    request.Yield = (result.Data.Yield * 100).ToString("N4");
                //    request.NetPrice = result.Data.CleanPrice.ToString("N4");
                //    request.YieldToExecution = (result.Data.YieldToExecution * 100).ToString("N4");
                //}
            }
            return null;
        }

        #region CRM机构
        public static CrmInst GetCrmInstInstiid(string instid)
        {
            ConcurrentDictionary<string, CrmInst> DictCrmInst = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject($"113_CRM_Inst");

            if (DictCrmInst != null)
            {
                if (DictCrmInst.ContainsKey(instid))
                {
                    return DictCrmInst[instid];
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        #endregion


        public static void CopyDataRow(DataRow old, DataRow newDataRow)
        {
            foreach (DataColumn dc in old.Table.Columns)
            {
                newDataRow[dc.ColumnName] = old[dc.ColumnName];
            }
        }

        public static bool IsFutureBond(string securityId)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(securityId) && securityId.StartsWith("T") && !securityId.Contains("."))//国债期货
            {
                result = true;
            }

            return result;
        }

        public static string FormatNumFuture(string num)
        {
            string result = num;

            decimal d = 0;
            if (decimal.TryParse(num, out d))
            {
                if (!string.IsNullOrEmpty(num) && result.Contains("."))
                {
                    int index = result.IndexOf(".");
                    int differ = num.Length - index - 1;
                    if (result.EndsWith("0"))
                    {
                        if (differ > 2)
                        {
                            result = result.Substring(0, result.Length - 1);
                            result = FormatNumFuture(result);
                        }
                    }
                    else if (differ < 3)
                    {
                        result += differ == 0 ? "000" : differ == 2 ? "0" : differ == 1 ? "00" : string.Empty;
                    }
                }
                else
                {
                    result += ".000";
                }
            }

            return result;
        }

        public static string formatQtyToK(string qty)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(qty, out temp) && temp >= 1000)
                {
                    return Math.Round(temp / 1000, 3) + "K";
                }

                return qty;
            }
            catch (Exception e)
            {

            }
            return qty;
        }

        public static string formatQtyToBillion(string qty)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(qty, out temp))
                    return (temp / 100000000).ToString("N1") + "亿";

                return qty;
            }
            catch (Exception e)
            {

            }
            return qty;
        }

        public static string GetShowPrice(GNoMDEntries mdEntry)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(mdEntry.MDEntryPx))
            {
                result = STCConstants.NO_PRICE;
            }
            else if (mdEntry.PriceType == "0")//意向
            {
                result = mdEntry.MDEntryType == "0" ? "Bid" : "Ofr";
            }
            else
            {
                result = mdEntry.MDEntryPx;
            }

            return result;
        }

        public static string GetFwdSettle(string settle)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(settle))
            {
                string[] settleArr = settle.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string str in settleArr)
                {
                    if (!(str == "1" || str == "2" || str == "3"))
                    {
                        result += str + ",";
                    }
                }
            }
            return result.Length > 0 ? result.Substring(0, result.Length - 1) : string.Empty;
        }

        public static string RemoveZero(string str)
        {
            string result = str;

            if (CommonUtils.IsNumeric(str) && str.Contains("."))
            {
                string[] arr = str.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 1)
                {
                    if (arr[1].Length > 4)
                    {
                        result = CommonUtils.formatString(str, 4);
                    }
                    else if (arr[1].EndsWith("0"))
                    {
                        result = str.Substring(0, str.Length - 1);

                        result = RemoveZero(result);
                    }
                }
            }

            return result;
        }

        public static bool IsSettleZero(string settle)
        {
            return !string.IsNullOrEmpty(settle) && (settle == "+0" || settle == "1");
        }

        public static bool IsFwd(string settle)
        {
            return !string.IsNullOrEmpty(settle) && (settle.Contains("."));
        }

        public static string GetShowSettle(bool isContainZero, bool isContainFwd)
        {
            string result = string.Empty;

            if (isContainZero && isContainFwd)
            {
                return "多";
            }
            else if (isContainZero && !isContainFwd)
            {
                return "+0";
            }
            else if (!isContainZero && isContainFwd)
            {
                return "远";
            }


            return result;
        }


        public static string FormatPriceZeroEqualsBar(string price)
        {
            if (CommonUtils.IsNumeric(price))
            {
                price = CommonUtils.ConvertToDecimal(price).ToString();
                if (CommonUtils.ConvertToDecimal(price) == CommonUtils.ConvertToDecimal(price.Split('.')[0].ToString()))
                {
                    price = CommonUtils.ConvertToDecimal(price.Split('.')[0].ToString()).ToString();
                }

                if (price == "0")
                {
                    price = "--";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(price))
                {
                    price = "--";
                }
            }
            return price;
        }

        public static string GetTagNameByCode(string code, string tagType)
        {
            if ("CUM_TYPE" == tagType && code == "11")
            {
                return "待行权";
            }
            else if ("PRODUCT" == tagType)
            {
                if (code == "0001")
                {
                    return "债券";
                }
                else if (code == "0010")
                {
                    return "国债期货";
                }
                else if (code == "0011")
                {
                    return "IRS";
                }
            }

            Dictionary<string, QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey(tagType) && dic[tagType].childTags != null)
            {
                if (BaseHandler.Instance.TagsDic != null
                    && BaseHandler.Instance.TagsDic.ContainsKey(tagType)
                    && BaseHandler.Instance.TagsDic[tagType] != null
                    && BaseHandler.Instance.TagsDic[tagType].ContainsKey(code))
                {
                    return BaseHandler.Instance.TagsDic[tagType][code];
                }
            }
            else if (tagType == Consts.ISSUER)
            {
                ChildTag ct = Common.Utils.CommonUtils.GetIssuer(code);
                if (ct != null)
                {
                    return ct.childTagName;
                }
            }
            else if (tagType == Consts.GUARANTOR)
            {
                ChildTag ct = Common.Utils.CommonUtils.GetGuarantor(code);
                if (ct != null)
                {
                    return ct.childTagName;
                }
            }

            return string.Empty;
        }

        public static string ConvertDes(string key, string temp)
        {
            if (temp.StartsWith("其他"))
            {
                if (key == Consts.BOND_SUB_TYPE)
                {
                    temp = "其他类型";
                }
                else if (key == Consts.ENTERPRISE_TYPE)
                {
                    temp = "其他企业";
                }
                else if (key == Consts.NCD_TYPE)
                {
                    temp = "其他NCD";
                }
                else if (key == Consts.ADMIN_LEVEL)
                {
                    temp = "其他行政级别";
                }
                else if (key == Consts.RANKING_TYPE)
                {
                    temp = "其他";
                }
            }
            else if (temp == "买入")
            {
                temp = "Bid";
            }
            else if (temp == "卖出")
            {
                temp = "Ofr";
            }

            return temp;
        }


        private List<BondsInfoPo> SetFilterDatas1(Dictionary<string, List<string>> _focusTagDic)
        {
            //this.ViewSource.View.Filter = x => {
            //    InnerOrderData data = x as InnerOrderData;

            //    if ((this.DisplayMode == "BNC" && data.SecurityType != "BNC" && data.BondSubType != "PSB")
            //        || (string.IsNullOrEmpty(this.DisplayMode) == false && this.DisplayMode != "BNC" && data.SecurityType == "BNC" && data.BondSubType != "PSB")
            //        || (this.DisplayMode == "BNC" && data.BondSubType == "PSB"))
            //        return false;

            //    if (string.IsNullOrEmpty(this.SearchText) == false
            //        && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
            //        && (data.Symbol == null || !data.Symbol.Contains(this.SearchText))
            //        && (data.RoleName == null || !data.RoleName.Contains(this.SearchText))
            //        && (data.PartySubName == null || !data.PartySubName.Contains(this.SearchText))
            //        && (data.IssuerName == null || !data.IssuerName.Contains(this.SearchText))
            //        && (data.WarranterName == null || !data.WarranterName.Contains(this.SearchText))
            //        && (data.TraderName == null || !data.TraderName.Contains(this.SearchText))
            //        && (data.InstitutionName == null || !data.InstitutionName.Contains(this.SearchText)))
            //        return false;

            //    if (IsMe == true && data.IsMe == false)
            //    {
            //        return false;
            //    }

            //    if (IsFuzzy == true && data.SecurityID != "99999")
            //    {
            //        return false;
            //    }

            //    if (!string.IsNullOrEmpty(this.StartTermDY) && data.TermDays < CommonUtils.GetDays(this.StartTermDY))
            //    {
            //        return false;
            //    }
            //    if (!string.IsNullOrEmpty(this.EndTermDY) && data.TermDays > CommonUtils.GetDays(this.EndTermDY))
            //    {
            //        return false;
            //    }

            //    if (this.IsCoupon)
            //    {
            //        if (!string.IsNullOrEmpty(this.StartCoupon) && (data.CouponRateCurrent == null || data.CouponRateCurrent < this.StartCoupon.ConvertToDecimal()))
            //        {
            //            return false;
            //        }
            //        if (!string.IsNullOrEmpty(this.EndCoupon) && (data.CouponRateCurrent == null || data.CouponRateCurrent > this.EndCoupon.ConvertToDecimal()))
            //        {
            //            return false;
            //        }
            //    }

            //    {
            //        if (!string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
            //        {
            //            decimal startPrice = this.StartPrice.ConvertToDecimal();
            //            decimal endPrice = this.EndPrice.ConvertToDecimal();
            //            if (!(data.Price >= startPrice && data.Price <= endPrice))
            //            {
            //                return false;
            //            }
            //        }
            //        else if (!string.IsNullOrEmpty(this.StartPrice) && string.IsNullOrEmpty(this.EndPrice))
            //        {
            //            decimal startPrice = this.StartPrice.ConvertToDecimal();
            //            if (!(data.Price >= startPrice))
            //            {
            //                return false;
            //            }
            //        }
            //        else if (string.IsNullOrEmpty(this.StartPrice) && !string.IsNullOrEmpty(this.EndPrice))
            //        {
            //            decimal endPrice = this.EndPrice.ConvertToDecimal();
            //            if (!(data.Price <= endPrice))
            //            {
            //                return false;
            //            }
            //        }
            //    }

            //    {
            //        decimal? chinaBondValue = data.ChinaBondValuation;
            //        if (data.ChinaBondExercise.HasValue)
            //        {
            //            chinaBondValue = data.ChinaBondExercise;
            //        }

            //        if (!string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
            //        {
            //            decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
            //            decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
            //            if (!(chinaBondValue >= startChinaBondValue && chinaBondValue <= endChinaBondValue))
            //            {
            //                return false;
            //            }
            //        }
            //        else if (!string.IsNullOrEmpty(this.StartChinaBondValue) && string.IsNullOrEmpty(this.EndChinaBondValue))
            //        {
            //            decimal startChinaBondValue = this.StartChinaBondValue.ConvertToDecimal();
            //            if (!(chinaBondValue >= startChinaBondValue))
            //            {
            //                return false;
            //            }
            //        }
            //        else if (string.IsNullOrEmpty(this.StartChinaBondValue) && !string.IsNullOrEmpty(this.EndChinaBondValue))
            //        {
            //            decimal endChinaBondValue = this.EndChinaBondValue.ConvertToDecimal();
            //            if (!(chinaBondValue <= endChinaBondValue))
            //            {
            //                return false;
            //            }
            //        }
            //    }

            //bool result = true;

            //if (data.Sign != 0 && _focusTagDic != null)
            //{
            //    foreach (KeyValuePair<string, string> pair in data.SignDic)
            //    {
            //        CheckSign(data, pair.Key, pair.Value, ref result, _focusTagDic);

            //        if (!result)
            //            return result;
            //    }
            //}

            //if (data.Sign1 != 0 && _focusTagDic != null)
            //{
            //    foreach (KeyValuePair<string, string> pair in data.SignDic1)
            //    {
            //        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

            //        if (!result)
            //            return result;
            //    }
            //}

            //if (data.Sign2 != 0 && this._focusTagDic != null)
            //{
            //    foreach (KeyValuePair<string, string> pair in data.SignDic2)
            //    {
            //        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

            //        if (!result)
            //            return result;
            //    }
            //}

            //if (data.Sign3 != 0 && this._focusTagDic != null)
            //{
            //    foreach (KeyValuePair<string, string> pair in data.SignDic3)
            //    {
            //        CheckSign(data, pair.Key, pair.Value, ref result, this._focusTagDic);

            //        if (!result)
            //            return result;
            //    }
            //}
            //else if (result && data.Sign3 == 0 && "BNC".Equals(data.SecurityType) && this._focusTagDic != null)
            //{
            //    foreach (KeyValuePair<string, string> pair in data.SignDic3)
            //    {
            //        string key = pair.Key;
            //        string value = pair.Value;
            //        if (key == Consts.RANKING_TYPE)
            //        {
            //            result = result
            //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
            //               || (_focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //        }
            //        else if (key == Consts.RATING_CURRENT)
            //        {
            //            result = result
            //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
            //               || (_focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //        }
            //        else if (key == Consts.CDC_IMPLITED_RATING)
            //        {
            //            result = result
            //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
            //               || (_focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //        }
            //        else if (key == Consts.ISSUER_RATING_CURRENT_CBR
            //            || key == Consts.CSI_IMPLIED_RATING)
            //        {
            //            result = result
            //               && (!_focusTagDic.ContainsKey(key) || _focusTagDic[key].Count == 0) || _focusTagDic[key].Contains(value)
            //               || (_focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.SecurityType)); // 00000也算其他
            //        }
            //    }
            //}

            //return result;
            //};
            List<BondsInfoPo> resultList = new List<BondsInfoPo>();
            //SortedDictionary<string, SimpleBondsInfoPo> dic = (SortedDictionary<string, SimpleBondsInfoPo>)AppUtils.Application.GetObject(BondKey);
            //bool result = true;
            //foreach (BondsInfoPo data in dic.Values)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (data.SignList.ElementAt(i) != "0" && _focusTagDic != null)
            //        {
            //            foreach (KeyValuePair<string, string> pair in data.SignDicList.ElementAt(i))
            //            {
            //                CheckSign(data, pair.Key, pair.Value, ref result, _focusTagDic);

            //                if (result)
            //                    resultList.Add(data);
            //            }
            //        }
            //    }
            //}

            return resultList;
        }

        private void CheckSign(BondsInfoPo data, string key, string value, ref bool result, Dictionary<string, List<string>> focusTagDic, string type = null)
        {
            //if (key == Utils.Constants.ISSUER)
            //{
            //    List<string> binaryList = new List<string>();
            //    string[] arr = null;
            //    if (!string.IsNullOrEmpty(data.IssuerCode))
            //    {
            //        arr = data.IssuerCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
            //    }

            //    if (focusTagDic.ContainsKey(key))
            //    {
            //        foreach (string ic in focusTagDic[key])
            //        {
            //            ChildTag ct = CommonUtils.GetIssuer(ic);
            //            if (ct != null)
            //            {
            //                binaryList.Add(ct.childTag);
            //            }

            //        }
            //    }

            //    result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            //}
            //else if (key == Utils.Constants.GUARANTOR)
            //{
            //    string[] arr = null;
            //    List<string> binaryList = new List<string>();
            //    if (!string.IsNullOrEmpty(data.WarranterCode))
            //    {
            //        arr = data.WarranterCode.Split(CommonConstants.BAR_CHAR, StringSplitOptions.RemoveEmptyEntries);
            //    }

            //    if (focusTagDic.ContainsKey(key))
            //    {
            //        foreach (string ic in focusTagDic[key])
            //        {
            //            ChildTag ct = CommonUtils.GetGuarantor(ic);
            //            if (ct != null)
            //            {
            //                binaryList.Add(ct.childTag);
            //            }
            //        }
            //    }

            //    result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || binaryList.Any(item => arr != null && arr.Contains(item)));
            //}
            //else if (key == Utils.Constants.DQTYPE)
            //{
            //    string tempDqtype = value;

            //    bool isme = false;
            //    if (type == "1")
            //    {
            //        isme = data.IsMe_InnerBid;
            //    }
            //    else if (type == "2")
            //    {
            //        isme = data.IsMe_InnerOfr;
            //    }
            //    else
            //    {
            //        isme = data.IsMe;
            //    }

            //    if (_innerType == "2" && isme == false)
            //    {
            //        tempDqtype = "0000";
            //    }
            //    result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(tempDqtype));
            //}
            //else if (key == Utils.Constants.CUM_TYPE)
            //{
            //    string cumType = "00";

            //    if (type == "1")
            //    {
            //        if (data.SignDic_InnerBid1 != null && data.SignDic_InnerBid1.ContainsKey(Utils.Constants.CUM_TYPE))
            //            cumType = data.SignDic_InnerBid1[Utils.Constants.CUM_TYPE];
            //    }
            //    else if (type == "2")
            //    {
            //        if (data.SignDic_InnerOfr1 != null && data.SignDic_InnerOfr1.ContainsKey(Utils.Constants.CUM_TYPE))
            //            cumType = data.SignDic_InnerOfr1[Utils.Constants.CUM_TYPE];
            //    }
            //    else
            //    {
            //        if (data.SignDic1 != null && data.SignDic1.ContainsKey(Utils.Constants.CUM_TYPE))
            //            cumType = data.SignDic1[Utils.Constants.CUM_TYPE];
            //    }

            //    string ifExe = "00";
            //    if (type == "1")
            //    {
            //        if (data.SignDic_InnerBid3 != null && data.SignDic_InnerBid3.ContainsKey(Utils.Constants.IF_EXERCISE))
            //            ifExe = data.SignDic_InnerBid3[Utils.Constants.IF_EXERCISE];
            //    }
            //    else if (type == "2")
            //    {
            //        if (data.SignDic_InnerOfr3 != null && data.SignDic_InnerOfr3.ContainsKey(Utils.Constants.IF_EXERCISE))
            //            ifExe = data.SignDic_InnerOfr3[Utils.Constants.IF_EXERCISE];
            //    }
            //    else
            //    {
            //        if (data.SignDic3 != null && data.SignDic3.ContainsKey(Utils.Constants.IF_EXERCISE))
            //            ifExe = data.SignDic3[Utils.Constants.IF_EXERCISE];
            //    }

            //    result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(cumType) || (focusTagDic[key].Contains("11") && ifExe == "10"));
            //}
            //else if (key == Utils.Constants.ORGTYPE)
            //{
            //    result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value) || (focusTagDic[key].Contains("0000000000000001") && value == "0000000000000000"));
            //}
            //else if (key == Utils.Constants.BOND_SUB_TYPE)
            //{
            //    int bondSubTypeCount = 0;
            //    if (focusTagDic.ContainsKey(key))
            //    {
            //        List<string> subBondTypeList = focusTagDic[key].ToList();
            //        subBondTypeList.RemoveAll(item => item == "1011" || item == "1100" || item == "1101" || item == "1110");
            //        bondSubTypeCount = subBondTypeList.Count;
            //    }

            //    result = result
            //        && (!focusTagDic.ContainsKey(key) || bondSubTypeCount == 0 || bondSubTypeCount == focusTagDic[key].Count)
            //        && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value))
            //        && (!focusTagDic.ContainsKey(Utils.Constants.BONDTYPE) || focusTagDic[Utils.Constants.BONDTYPE].Count == 0 || !focusTagDic.ContainsKey(key) || focusTagDic[Utils.Constants.BONDTYPE].Count == focusTagDic[key].Count);
            //}
            //else if (key == Utils.Constants.RANKING_TYPE)
            //{
            //    result = result
            //       && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
            //       || (focusTagDic[key].Contains("xx") && !rankingtype_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //}
            //else if (key == Utils.Constants.RATING_CURRENT)
            //{
            //    result = result
            //       && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
            //       || (focusTagDic[key].Contains("xx") && !ratingCurrent_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //}
            //else if (key == Utils.Constants.CDC_IMPLITED_RATING)
            //{
            //    result = result
            //       && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
            //       || (focusTagDic[key].Contains("xx") && !cdcImplitedRating_arr.Contains(value) && !"BNC".Equals(data.SecurityType));
            //}
            //else if (key == Utils.Constants.ISSUER_RATING_CURRENT_CBR
            //    || key == Utils.Constants.CSI_IMPLIED_RATING)
            //{
            //    result = result
            //       && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0) || focusTagDic[key].Contains(value)
            //       || (focusTagDic[key].Contains("01000") && value == "00000" && !"BNC".Equals(data.SecurityType)); // 00000也算其他
            //}
            //else
            //{
            //result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(InnerBoardFilterPanel_New.ALL) || focusTagDic[key].Contains(value));
            result = result && (!focusTagDic.ContainsKey(key) || focusTagDic[key].Count == 0 || focusTagDic[key].Contains(value));
            //}
        }

        public const string PARAM_KEY_TRADEBOOKHEADER = "STC_TradeBookHeader_Setting";

        private static string _TradeBookHeader = string.Empty;

        public static string TradeBookHeader
        {
            get { return _TradeBookHeader; }
            set
            {
                if (_TradeBookHeader != value)
                {
                    _TradeBookHeader = value;
                }
            }
        }

        public const string PARAM_KEY_COPY = "STC_InnerBoard_Copy_Setting";

        private static Dictionary<string, IList<CopyColumnItem>> _copyDic;

        public static Dictionary<string, IList<CopyColumnItem>> CopyDic
        {
            get
            {
                if (_copyDic == null)
                {
                    _copyDic = new Dictionary<string, IList<CopyColumnItem>>();
                }
                return _copyDic;
            }
        }

        private static string _CopyInfoJson = string.Empty;

        public static string CopyInfoJson
        {
            get { return _CopyInfoJson; }
            set
            {
                if (_CopyInfoJson != value)
                {
                    _CopyInfoJson = value;

                    try
                    {
                        if (string.IsNullOrEmpty(value) == false)
                            _copyDic = JsonConvert.DeserializeObject<Dictionary<string, IList<CopyColumnItem>>>(value);
                        else
                            _copyDic = new Dictionary<string, IList<CopyColumnItem>>();
                    }
                    catch
                    {
                        _copyDic = new Dictionary<string, IList<CopyColumnItem>>();
                    }
                }
            }
        }

        public const string STC_SALE_INTENTION = "STC_SALE_INTENTION";

        private static Dictionary<string, string> _SaleDic;

        public static Dictionary<string, string> SaleDic
        {
            get
            {
                if (_SaleDic == null)
                {
                    _SaleDic = new Dictionary<string, string>();
                }
                return _SaleDic;
            }
        }

        private static string _SaleInfoJson = string.Empty;

        public static string SaleInfoJson
        {
            get { return _SaleInfoJson; }
            set
            {
                if (_SaleInfoJson != value)
                {
                    _SaleInfoJson = value;

                    try
                    {
                        if (string.IsNullOrEmpty(value) == false)
                            _SaleDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                        else
                            _SaleDic = new Dictionary<string, string>();
                    }
                    catch (Exception)
                    {
                        _SaleDic = new Dictionary<string, string>();
                    }
                }
            }
        }

        private static Dictionary<string, BrokerWhiteList> _BrokerDic;

        public static Dictionary<string, BrokerWhiteList> BrokerDic
        {
            get { return _BrokerDic; }
            set
            {
                _BrokerDic = value;
            }
        }

        public static void loadInnerSetingBroker()
        {
            BrokerWhiteList request = new BrokerWhiteList();
            request.action = 0;
            request.userId = LocalEnv.UserInfo.UserId;
            request.username = LocalEnv.UserInfo.UserName;
            BrokerWhiteListReplyPo result = TableHandler.Instance.brokerWhiteList(request);
            CommonUtils.BrokerDic = new Dictionary<string, BrokerWhiteList>();
            if (result != null && result.Datas != null && result.Datas.Count > 0)
            {
                foreach (BrokerWhiteList obj in result.Datas)
                {
                    if (!string.IsNullOrEmpty(obj.brokerId) && !CommonUtils.BrokerDic.ContainsKey(obj.brokerId))
                        CommonUtils.BrokerDic.Add(obj.brokerId, obj);
                }
            }
        }

        public static void OpenSmallBondWin(string securityid)
        {
            //Window parentWindow = Window.GetWindow(obj);
            Process p = Process.GetProcessesByName("qeubee").FirstOrDefault();
            if (p != null)
            {
                IntPtr parentWindowHandle = p.MainWindowHandle;
                // 检查父窗口是否存在
                if (parentWindowHandle != IntPtr.Zero)
                {
                    //// 获取父窗口的句柄
                    //IntPtr parentWindowHandle = new WindowInteropHelper(parentWindow).Handle;

                    QBShowBondUtil.SendMessage(securityid, parentWindowHandle);
                }
            }
        }

        private static List<string> LstBdbAppNameList = new List<string>() { "bdb", "idcclient" };
        private static List<string> LstIDBAppNameList = new List<string>() { "idbpro" };
        /// <summary>
        /// 是否是BDB
        /// </summary>
        /// <returns></returns>
        public static bool IsBDBAppName()
        {
            return LstBdbAppNameList.Contains(LocalEnv.ServerInfo.AppName.ToLower());
        }

        /// <summary>
        /// 是否是IDB
        /// </summary>
        /// <returns></returns>
        public static bool IsIDBAppName()
        {
            return LstIDBAppNameList.Contains(LocalEnv.ServerInfo.AppName.ToLower());
        }

        /// <summary>
        /// 是否包含XBound插件
        /// </summary>
        /// <returns></returns>
        public static bool IsContainsXBoundPlugin()
        {
            string files1 = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"plugin\BDB.XBond.Plugin.dll");
            string files2 = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"plugin\STC.XBond.Plugin.dll");

            bool ishaveplugin = STC.Common.Utils.FileHelper.FileIsExists(files1) || STC.Common.Utils.FileHelper.FileIsExists(files2);

            return ishaveplugin;
        }

        public static bool HasCDCAuth(string securtiyId)
        {
            if (IsBDBAppName())
            {
                BondsInfoPo bondInfo = GetBondsInfoPoBySecurityID(securtiyId);
                if (bondInfo != null)
                {
                    string cdcAuth = bondInfo.cdcAuth;
                    if (string.IsNullOrEmpty(cdcAuth) || (!string.IsNullOrEmpty(LocalEnv.UserInfo.CDCPermissions) && LocalEnv.UserInfo.CDCPermissions.Contains(cdcAuth)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将 Object 转换为Json,写入 data 目录的 文本文件中
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fname"></param>
        /// <example>CommonUtils.WriteConvertObjectToJsonToText(lstlastOwnOrder, "1.txt");</example>
        public static void WriteConvertObjectToJsonToText(object obj, string fname)
        {
           FileHelper.WriteFullText(System.IO.Path.Combine(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data"), fname), CommonUtils.SerializeObject(obj));
        }

        //private static ConcurrentDictionary<string, string> _QQDic = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 使用usename搜索, 
        /// </summary>
        /// <param name="username">usename 或者 userid</param>
        /// <returns></returns>
        public static string GetQQNumber(string username)
        {
            string qqNumber = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                //if (_QQDic.ContainsKey(username))
                //{
                //    qqNumber = _QQDic[username];
                //}
                if (string.IsNullOrEmpty(qqNumber))
                {
                    if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                    {
                        if (AdminHandler.Instance.GetCachestcUserQuery().Datas.Any(item => item.Username == username || item.Userid == username))
                        {
                            StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.First(item => item.Username == username || item.Userid == username);
                            qqNumber = currentUser.inf3;
                        }
                    }
                }

            }
            return qqNumber;
        }

        static List<StcTbookUsers> investors = null;
        public static List<StcTbookUsers> GetInvestors()
        {
            if (investors == null)
            {
                if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                {
                    investors = AdminHandler.Instance.GetCachestcUserQuery().Datas.Where(a => (a.Biztypeid == LocalEnv.UserInfo.Termids || (LocalEnv.UserInfo.mdgroupids != null && LocalEnv.UserInfo.mdgroupids.Contains(a.Biztypeid))) && (a.ActionIds.Contains(OWN) || a.ActionIds.Contains(ALL))).ToList<StcTbookUsers>();
                }
            }

            return investors;
        }

        static List<StcTbookUsers> traders = null;

        public static List<StcTbookUsers> GetTraders()
        {
            if (traders == null)
            {
                if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                {
                    traders = AdminHandler.Instance.GetCachestcUserQuery().Datas.Where(a => (a.Biztypeid == LocalEnv.UserInfo.Termids || (LocalEnv.UserInfo.mdgroupids != null && LocalEnv.UserInfo.mdgroupids.Contains(a.Biztypeid))) && !(a.ActionIds.Contains(OWN) || a.ActionIds.Contains(ALL))).ToList<StcTbookUsers>();
                }
            }
            return traders;
        }

        static ConcurrentDictionary<string, StcTbookUsers> _UserInfoDic = new ConcurrentDictionary<string, StcTbookUsers>();
        public static StcTbookUsers GetUsersInfoByUserName(string userName)
        {
            if (_UserInfoDic.ContainsKey(userName))
            {
                return _UserInfoDic[userName];
            }
            else
            {
                if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                {
                    StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.FirstOrDefault(item => item.Username == userName);
                    return currentUser;
                }
                return null;
            }
        }

        static List<StcTbookUsers> allTbookUsers = null;
        public static List<StcTbookUsers> GetUsersInfoByToqq(string toqq)
        {
            if (allTbookUsers == null)
            {
                if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                {
                    allTbookUsers = AdminHandler.Instance.GetCachestcUserQuery().Datas;
                    if (!string.IsNullOrEmpty(toqq))
                    {
                        List<StcTbookUsers> currentUser = allTbookUsers.FindAll(item => item.Inf3 == toqq);
                        if (currentUser.Count > 0)
                            return currentUser;
                        else
                            return allTbookUsers;
                    }
                    else {
                        return allTbookUsers;
                    }
                }
                return null;
            }
            else 
            {
                if (!string.IsNullOrEmpty(toqq))
                {
                    List<StcTbookUsers> currentUser = allTbookUsers.FindAll(item => item.Inf3 == toqq);
                    if (currentUser.Count > 0)
                        return currentUser;
                    else
                        return allTbookUsers;
                }
                else
                {
                    return allTbookUsers;
                }
            }
        }

        //执行权限
        private readonly static string MODULEID_INTENTION = "4008";//需求工作台
        private readonly static string OWN_INQUIRY = "400801";//仅自己(有询价),交易员
        private readonly static string OWN = "400802";//仅自己(无询价),投资经理
        private readonly static string ALL_INQUIRY = "400803";//所有(有询价),交易员
        private readonly static string ALL = "400804";//所有(无询价).投资经理

        public static bool IsInvestor()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, OWN) || AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, ALL);
        }

        public static bool HasInquiry()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, OWN_INQUIRY) || AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, ALL_INQUIRY);
        }

        public static bool IsOwn()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, OWN) || AdminHandler.Instance.CheckPermission(MODULEID_INTENTION, OWN_INQUIRY);
        }

        /// <summary>
        /// 获得biztypeid， biztype
        /// </summary>
        /// <param name="username">username 或者 Userid</param>
        /// <param name="biztypeid">组id</param>
        /// <param name="biztype"> 组名</param>
        public static void GetBizInfo(string username, out string biztypeid, out string biztype)
        {
            biztypeid = string.Empty;
            biztype = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                {
                    if (AdminHandler.Instance.GetCachestcUserQuery().Datas.Any(item => item.Username == username || item.Userid == username))
                    {
                        StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.First(item => item.Username == username || item.Userid == username);
                        biztypeid = currentUser.Biztypeid;
                        biztype = currentUser.Biztypestr;
                    }
                }
            }
        }

        //public static string AutoSendQQParam = string.Empty;

        //public static void QueryAutoSendQQParam()
        //{
        //    QueryUnrefParamReq req = new QueryUnrefParamReq();
        //    req.param_key = "STC_AutoSendQQParam";
        //    req.username = LocalEnv.UserInfo.UserName;

        //    QueryUnrefParamReplyPo reply = TableHandler.Instance.QueryUnrefParam(req);
        //    if (reply != null && reply.IsSuccess())
        //    {
        //        if (reply.Datas != null)
        //        {
        //            TTbookSystemParameter item = reply.Datas;
        //            AutoSendQQParam = item.param_value;
        //        }
        //    }
        //}

        public static void ProcessInf4(string inf2, string inf1, FilterGroupItem item)
        {
            item.FocusGroupId_Edit = item.FocusGroupId = inf2;
        }

        public static void ProcessInf3(string inf3, string inf1, FilterGroupItem item)
        {
            bool isUpdateTerm = false;
            bool isUpdateCoupon = false;
            bool isUpdatePrice = false;
            bool isUpdateChinaBond = false;
            bool isUpdateBidBp = false;
            bool isUpdateOfrBp = false;
            if (!string.IsNullOrEmpty(inf3) && inf1 == "DYNAMIC_PARAM")
            {
                Dictionary<string, string> inf3Dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(inf3);
                if (inf3Dic.ContainsKey(Consts.TERM_INF3))
                {
                    string[] termStrArr = inf3Dic[Consts.TERM_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (termStrArr.Length == 2)
                    {
                        item.IsAppTerm = true;
                        item.StartTermDY_Edit = item.StartTermDY = termStrArr[0]?.Trim();
                        item.EndTermDY_Edit = item.EndTermDY = termStrArr[1]?.Trim();
                        isUpdateTerm = true;
                    }
                }

                if (inf3Dic.ContainsKey(Consts.COUPON_INF3))
                {
                    string[] termStrArr = inf3Dic[Consts.COUPON_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (termStrArr.Length == 2)
                    {
                        item.IsCoupon_Edit = item.IsCoupon = true;
                        item.StartCoupon_Edit = item.StartCoupon = termStrArr[0]?.Trim();
                        item.EndCoupon_Edit = item.EndCoupon = termStrArr[1]?.Trim();
                        isUpdateCoupon = true;
                    }
                }

                if (inf3Dic.ContainsKey(Consts.PRICE_INF3))
                {
                    string[] priceStrArr = inf3Dic[Consts.PRICE_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (priceStrArr.Length == 2)
                    {
                        item.StartPrice_Edit = item.StartPrice = priceStrArr[0]?.Trim();
                        item.EndPrice_Edit = item.EndPrice = priceStrArr[1]?.Trim();
                        isUpdatePrice = true;
                    }
                }

                if (inf3Dic.ContainsKey(Consts.CHINABONDVALUE_INF3))
                {
                    string[] chinaStrArr = inf3Dic[Consts.CHINABONDVALUE_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (chinaStrArr.Length == 2)
                    {
                        item.StartChinaBond_Edit = item.StartChinaBond = chinaStrArr[0]?.Trim();
                        item.EndChinaBond_Edit = item.EndChinaBond = chinaStrArr[1]?.Trim();
                        isUpdateChinaBond = true;
                    }
                }

                if (inf3Dic.ContainsKey(Consts.BID_SUB_VAL_INF3))
                {
                    string[] bidBpArr = inf3Dic[Consts.BID_SUB_VAL_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (bidBpArr.Length == 2)
                    {
                        item.StartBidBp_Edit = item.StartBidBp = bidBpArr[0]?.Trim();
                        item.EndBidBp_Edit = item.EndBidBp = bidBpArr[1]?.Trim();
                        isUpdateBidBp = true;
                    }
                }

                if (inf3Dic.ContainsKey(Consts.VAL_SUB_OFR_INF3))
                {
                    string[] ofrBpArr = inf3Dic[Consts.VAL_SUB_OFR_INF3].Split(CommonConstants.COMMA_CHAR);
                    if (ofrBpArr.Length == 2)
                    {
                        item.StartOfrBp_Edit = item.StartOfrBp = ofrBpArr[0]?.Trim();
                        item.EndOfrBp_Edit = item.EndOfrBp = ofrBpArr[1]?.Trim();
                        isUpdateOfrBp = true;
                    }
                }
            }

            if (!isUpdateTerm)
            {
                item.StartTermDY_Edit = item.StartTermDY = string.Empty;
                item.EndTermDY_Edit = item.EndTermDY = string.Empty;
            }

            if (!isUpdateCoupon)
            {
                item.IsCoupon_Edit = item.IsCoupon = false;
                item.StartCoupon_Edit = item.StartCoupon = string.Empty;
                item.EndCoupon_Edit = item.EndCoupon = string.Empty;
            }

            if (!isUpdatePrice)
            {
                item.StartPrice_Edit = item.StartPrice = string.Empty;
                item.EndPrice_Edit = item.EndPrice = string.Empty;
            }

            if (!isUpdateChinaBond)
            {
                item.StartChinaBond_Edit = item.StartChinaBond = string.Empty;
                item.EndChinaBond_Edit = item.EndChinaBond = string.Empty;
            }

            if (!isUpdateBidBp)
            {
                item.StartBidBp_Edit = item.StartBidBp = string.Empty;
                item.EndBidBp_Edit = item.EndBidBp = string.Empty;
            }

            if (!isUpdateOfrBp)
            {
                item.StartOfrBp_Edit = item.StartOfrBp = string.Empty;
                item.EndOfrBp_Edit = item.EndOfrBp = string.Empty;
            }
        }


        public const string MODULEID_BONDSUBTYPE = "5005";
        public const string RATE = "500502";
        public const string CREDIT = "500503";
        public static bool isOnlyRate = false;
        public static bool isOnlyCredit = false;


        public static SendIntentBrokers GetBrokerQq(string venues, string securityType)
        {
            if (broker_ToOption_Dic.ContainsKey(venues))
            {
                return broker_ToOption_Dic[venues].FirstOrDefault<SendIntentBrokers>(e => e.securityType == securityType);
            }
            return null;
        }

        static ConcurrentDictionary<string, List<SendIntentBrokers>> broker_ToOption_Dic = new ConcurrentDictionary<string, List<SendIntentBrokers>>();
        public static void LoadFQBrokerConfig()
        {
            if (broker_ToOption_Dic.Count == 0)
            {
                GetUserConfigFinqRequest req = new GetUserConfigFinqRequest();
                req.Username = LocalEnv.UserInfo.UserName;
                req.sender = CommonUtils.GetQQNumber(LocalEnv.UserInfo.UserName);
                GetUserConfigFinqReplyPo reply = FinQHandler.Instance.GetUserConfig(req);

                if (reply != null && reply.IsSuccess() && reply.data != null)
                {
                    // broker;
                    broker_ToOption_Dic.Clear();
                    if (reply.data.sendIntentBrokers != null && reply.data.sendIntentBrokers.Count > 0)
                    {
                        foreach (KeyValuePair<string, List<SendIntentBrokers>> pair in reply.data.sendIntentBrokers)
                        {
                            if (pair.Value == null || pair.Value.Count == 0)
                                continue;

                            string brokerName = GetBrokerName(pair.Key);

                            if (!broker_ToOption_Dic.ContainsKey(brokerName))
                            {
                                broker_ToOption_Dic[brokerName] = pair.Value;
                            }
                        }
                    }
                }
            }
        }

        private static string GetBrokerName(string key)
        {
            if (key == "1")
            {
                return "TPSC";// 国利
            }
            else if (key == "2")
            {
                return "CNEX";// 国际
            }
            else if (key == "3")
            {
                return "CCTB";//中诚
            }
            else if (key == "4")
            {
                return "PTCN";//平安
            }
            else if (key == "5")
            {
                return "CTTJ";//信唐
            }
            else if (key == "6")
            {
                return "UEDA"; //上田
            }
            return key;
        }

        /// <summary>
        /// 批量识别报价精度
        /// 1:2位 2:4位 
        /// </summary>
        public static string RateDP = "2";
        public static string CreditDP = "1";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securityID"></param>
        /// <returns>ir:利率, cfp：商金二永, ib:银行间 cp:公司债</returns>
        public static string ConvertToPAType(string securityID)
        {
            BondsInfoPo bondInfo = GetBondsInfoPoBySecurityID(securityID);
            string securityID_Upper = securityID.ToUpper();
            if (bondInfo != null)
            {
                if (bondInfo.SECURITY_TYPE == "ABS" || bondInfo.SECURITY_TYPE == "PPN")
                {
                    if (securityID_Upper.EndsWith(".SH") || securityID_Upper.EndsWith(".SZ") || securityID_Upper.EndsWith(".BJ"))
                    {
                        return Consts.CP;
                    }
                    else if (securityID_Upper.EndsWith(".IB"))
                    {
                        return Consts.IB;
                    }
                }
                else if (bondInfo.Bond_Subtype == "BIL" 
                    || bondInfo.Bond_Subtype == "SCB"
                    || bondInfo.Bond_Subtype == "CCP"
                    || bondInfo.Bond_Subtype == "CMN"
                    || bondInfo.Bond_Subtype == "COB"
                    || bondInfo.Bond_Subtype == "CSP"
                    || bondInfo.Bond_Subtype == "LCP"
                    || bondInfo.Bond_Subtype == "LMN"
                    || bondInfo.Bond_Subtype == "LSP"
                    || bondInfo.Bond_Subtype == "RAB"
                    || bondInfo.Bond_Subtype == "LCP"
                    || bondInfo.Bond_Subtype == "CLN"
                    || bondInfo.Bond_Subtype == "CRM"
                    || bondInfo.Bond_Subtype == "NCB"
                    || bondInfo.Bond_Subtype == "NCB"
                    || bondInfo.Bond_Subtype == "SSB"
                    || bondInfo.Bond_Subtype == "CCP"
                    || bondInfo.Bond_Subtype == "CMN"
                    || bondInfo.Bond_Subtype == "HJB"
                    || bondInfo.Bond_Subtype == "CCD"
                    || bondInfo.Bond_Subtype == "FRD"
                    || bondInfo.Bond_Subtype == "MCD"
                    || bondInfo.Bond_Subtype == "OTD"
                    || bondInfo.Bond_Subtype == "RRD"
                    || bondInfo.Bond_Subtype == "RTD"
                    || bondInfo.Bond_Subtype == "SHD"
                    || bondInfo.Bond_Subtype == "SPD"
                    || bondInfo.Bond_Subtype == "PDB")
                {
                    return Consts.IB;
                }
                else if (bondInfo.Bond_Subtype == "CCB"
                    || bondInfo.Bond_Subtype == "CEB"
                    || bondInfo.Bond_Subtype == "CVB"
                    || bondInfo.Bond_Subtype == "LEB"
                    || bondInfo.Bond_Subtype == "SCV"
                    || bondInfo.Bond_Subtype == "WBS"
                    || bondInfo.Bond_Subtype == "BIL"
                    || bondInfo.Bond_Subtype == "SEB"
                    || bondInfo.Bond_Subtype == "SES")
                {
                    return Consts.CP;
                }
                else if (bondInfo.Bond_Subtype == "CBB"
                    || bondInfo.Bond_Subtype == "CSB"
                    || bondInfo.Bond_Subtype == "CXB"
                    || bondInfo.Bond_Subtype == "PSB"
                    || bondInfo.Bond_Subtype == "TET"
                    || bondInfo.Bond_Subtype == "TLC"
                    || bondInfo.Bond_Subtype == "INT")
                {
                    return Consts.CTP;
                }
                else if (bondInfo.Bond_Subtype == "PBB"
                    || bondInfo.Bond_Subtype == "BGB"
                    || bondInfo.Bond_Subtype == "LLB"
                    || bondInfo.Bond_Subtype == "TLB")
                {
                    return Consts.IR;
                }
            }
            return null;
        }

        /// <summary>
        /// todo 如何区分
        /// </summary>
        /// <param name="bondSubType"></param>
        /// <param name="securityType"></param>
        /// <param name="bondCategory"></param>
        /// <param name="symbolType"></param>
        /// <returns>1:信用;2:利率债(其他);3：NCD;21：地方债</returns>
        public static string GetQQSecurityType(string bondSubType_tag, string bondSubType_base)
        {
            //3：NCD   BOND_SUB_TYPE   1001   
            //21：地方债  BOND_SUB_TYPE  1110
            //2:利率债(其他) bondsubtype SCB/PBB/PSB/BGB
            //1:信用  
            string SecurityType = string.Empty;
            // NCD
            if (bondSubType_tag == "1001")
            {
                SecurityType = "3";
            }
            // 利率债(地方债)
            else if (bondSubType_tag == "1110")
            {
                SecurityType = "21";
            }
            // 利率债(其他)
            else if (bondSubType_base == "SCB"
                || bondSubType_base == "PBB"
                || bondSubType_base == "PSB"
                || bondSubType_base == "BGB")
            {
                SecurityType = "2";
            }
            // 信用
            else
            {
                SecurityType = "1";
            }
           
           
            return SecurityType;
        }
    }
}
