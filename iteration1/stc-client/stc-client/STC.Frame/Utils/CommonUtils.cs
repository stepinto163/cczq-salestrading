using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin.WindowsPlugin;
using STC.Frame.Utils;
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

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class CommonUtils
    {
        //public static IForm Application;


        private const string BondKey = "2_Bond";

        /// <summary>
        /// 获取债券基本信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum, string listed_market = null)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            //string key = GetPluginShareKey(2, "Bond");
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                IEnumerable<KeyValuePair<string, BondsInfoPo>> v = null;
                if (listed_market != null)
                    v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) && d.Value.listed_Market == listed_market && string.IsNullOrEmpty(d.Value.Security_Type_Id) == false select d);
                else
                    v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) select d);

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

        /// <summary>
        /// 缓存用户使用过的债券基本信息
        /// </summary>
        static ConcurrentDictionary<string, BondsInfoPo> bondsInfoWithSecurityIDDic = new ConcurrentDictionary<string, BondsInfoPo>();
        public static BondsInfoPo GetBondsInfoPoBySecurityID(string securityID)
        {
            if (bondsInfoWithSecurityIDDic.ContainsKey(securityID))
            {
                return bondsInfoWithSecurityIDDic[securityID];
            }
            else
            {
                //string key = GetPluginShareKey(2, "Bond");
                SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);
                if (dic != null)
                {
                    lock (dic)
                    {
                        if (dic.ContainsKey(securityID))
                        {
                            BondsInfoPo bondsInfoPo = dic[securityID];
                            bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                            return bondsInfoPo;
                        }
                    }
                }
            }
            return null;
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
        /// 判断是否是双边债券
        /// </summary>
        /// <param name="bondsInfo"></param>
        /// <returns></returns>
        public static bool IsCIB(BondsInfoPo item)
        {
            return item != null && item.listed_Market == "CIB" && string.IsNullOrEmpty(item.Security_Type_Id) == false;
        }

        /// <summary>
        /// 得到所有债券基本信息
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<string, BondsInfoPo> GetAllBaseBondsInfo()
        {
            //string key = GetPluginShareKey(pluginKey, name);
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);
            return dic;
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
                return ((List<bool>)AppUtils.Application.GetObject(IsEndOfLoadFromDBKey))[0];
            }
            return false;
        }

        //private static string GetPluginShareKey(int pluginKey, string name)
        //{
        //    return string.Format("{0}_{1}", pluginKey, name);
        //}

        private static string InstKey = "2_Inst";

        public static InstInfoPo GetInstByPARTYCODE6(string PARTY_CODE6)
        {
            lock(string.Intern(InstKey))
            {
                Dictionary<string, InstInfoPo> dic = (Dictionary<string, InstInfoPo>)AppUtils.Application.GetObject(InstKey);
                if (string.IsNullOrEmpty(PARTY_CODE6) == false && dic.ContainsKey(PARTY_CODE6))
                {
                    return dic[PARTY_CODE6];
                }
            }
            
            return null;
        }

       /// <summary>
       /// 对象转字符串
       /// </summary>
       /// <param name="obj"></param>
       /// <returns></returns>
        public static string ConvertToString(object obj)
        {
            return obj == null ?  string.Empty : obj.ToString();
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
            try
            {
                long temp = Convert.ToInt64(Convert.ToDouble(obj));
                return temp;
            }
            catch(Exception ex)
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
            catch(Exception e)
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
            catch(Exception e)
            {

            }
            return DateTimeEx.ServerTime;
        }

        public static string ConvertTime(string time)
        {
            try
            {
                string[] arr = time.Split(new string[] { " ", "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    time = time.Contains("/")? arr[1] : arr[0];

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
        public static List<BondsInfoPo> GetODMBondsInfoPo(string condition, int resultNum)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            //string key = GetPluginShareKey(2, "Bond");
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                var v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) select d);

                int i = 0;
                foreach (KeyValuePair<string, BondsInfoPo> kv in v)
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
                return $"{ tradeTime.ToString("MM.dd") }+0";
            }
            else if (fastSettlType == "2" || fastSettlType == "4")
            {
                return $"{ tradeTime.ToString("MM.dd") }+1";
            }
            return fastSettlType;
        }

        /// <summary>
        /// settleType是mm.dd+0,mm.dd+1
        /// </summary>
        /// <param name="settleType"></param>
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
                if (requst != null && result.Data != null)
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
                List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(goodcode, 1);
                if (bondsInfoPos != null && bondsInfoPos.Count > 0)
                {
                    goodsType = bondsInfoPos[0].SECURITY_TYPE;

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
    }
}
