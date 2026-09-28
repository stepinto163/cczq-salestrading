
using com.gateway.log;
using StcLite.Common.Common;
using StcLite.Common.Handler.Base;
using StcLite.Common.Handler.Cal;
using StcLite.Common.Model.Base;
using StcLite.Common.Model.Cal;
using StcLite.Common.Utils;
using StcLite.Frame.Utils;
using StcLite.Trade.Plugin.Command;
using StcLite.Trade.Plugin.Component;
using StcLite.Trade.Plugin.ViewModel.Trade;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StcLite.Trade.Plugin.Utils
{
    public class CommonUtils
    {
        //public static IForm Application;
        public static PopupWindowCommand PopupWindowCommand = new PopupWindowCommand();

        private const string BondKey = "2_Bond";
        public static List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum)
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
                var v = (from d in dic where d.Key.ToLower().Contains(condition.ToLower()) select d);

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

        static Dictionary<string, BondsInfoPo> bondsInfoWithSecurityIDDic = new Dictionary<string, BondsInfoPo>();
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
                lock (dic)
                {
                    IEnumerable<KeyValuePair<string, BondsInfoPo>> findList = dic.Where(item => item.Key.ToLower().Contains(securityID.ToLower()));
                    if (findList != null && findList.ToArray().Count() > 0)
                    {
                        BondsInfoPo bondsInfoPo = findList.First().Value;
                        bondsInfoWithSecurityIDDic[securityID] = bondsInfoPo;
                        return bondsInfoPo;
                    }
                }
            }
            return null;
        }

        public static SortedDictionary<string, BondsInfoPo> GetAllBaseBondsInfo()
        {
            //string key = GetPluginShareKey(pluginKey, name);
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);
            return dic;
        }

        private static string IsEndOfLoadFromDBKey = "2_IsEndOfLoadFromDB";
        public static bool IsEndofLoadFromDB()
        {

            return BaseHandler.Instance.LoadDataStatus();
            //if (IsEndOfLoadFromDBKey != null)
            //{
            //    return ((List<bool>)AppUtils.Application.GetObject(IsEndOfLoadFromDBKey))[0];
            //}
            //return false;
        }

        //private static string GetPluginShareKey(int pluginKey, string name)
        //{
        //    return string.Format("{0}_{1}", pluginKey, name);
        //}

        public static string ConvertToString(object obj)
        {
            return obj == null ?  string.Empty : obj.ToString();
        }

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

        public static DateTime ConvertToDateTime(string obj)
        {
            try
            {
                string s = (string)obj;
                if (s.Length == 8)
                {
                    obj = $"{s.Substring(0,4)}-{s.Substring(4,2)}-{s.Substring(6,2)}";
                }
                DateTime dateTime = DateTime.Parse(obj);
                return dateTime;
            }
            catch(Exception e)
            {

            }
            return DateTimeEx.ServerTime;
        }


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

        public static string ParseTimeString(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 10)
                return str.Substring(0, 10).Replace("-", "");

            return str;
        }

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

        public static T FindVisualChildItem<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            if (null != obj)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is T && ((T)child).Name.Equals(name))
                    {
                        return (T)child;
                    }
                    else
                    {
                        T childOfChild = FindVisualChildItem<T>(child, name);
                        if (childOfChild != null && childOfChild is T && ((T)childOfChild).Name.Equals(name))
                        {
                            return childOfChild;
                        }
                    }
                }
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
                var v = (from d in dic where d.Key.ToLower().Contains(condition.ToLower()) select d);

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

        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
            }
            return false;
        }

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
                if (string.IsNullOrEmpty(price) )
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
                string[] splitArr = remainingTime.Split(Constants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
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
            string bondCategoryName = GetBondCategoryName(bond_subType, ent_cor);
            switch (bondCategoryName)
            {
                case "国债":
                    return "A";
                case "央票":
                    return "B";
                case "金融债":
                    return "C";
                case "地方政府债":
                    return "D";
                case "短融":
                    return "E";
                case "中票":
                    return "F";
                case "企业债":
                    return "G";
                case "公司债":
                    return "H";
                case "NCD":
                    return "I";
                case "PPN":
                    return "J";
                case "ABS":
                    return "K";
                case "CRM":
                    return "L";
                default:
                    return string.Empty;
            }
        }


        public static void CopyDataRow(DataRow old, DataRow newDataRow)
        {
            foreach (DataColumn dc in old.Table.Columns)
            {
                newDataRow[dc.ColumnName] = old[dc.ColumnName];
            }
        }

        public static void CopyCardItem(CardItem old, CardItem newItem)
        {
            newItem.Id = old.Id;
            newItem.HomeGroupSecurityIDsInfo = old.HomeGroupSecurityIDsInfo;
            newItem.Security = old.Security;
            newItem.GroupId = old.GroupId;
            newItem.BondCategory = old.BondCategory;
            newItem.BondType = old.BondType;
            newItem.RemaingingTimeConvert = old.RemaingingTimeConvert;
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
            else
            {
                result += ".00";
            }

            return result;
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
            prompt.Owner = win == null ? Application.Current.MainWindow : win;
            prompt.Show();
        }

        public static string formatQtyToK(string qty)
        {
            try
            {
                decimal temp = decimal.Zero;
                if (decimal.TryParse(qty, out temp) && temp > 1000)
                {
                    return Math.Round(temp / 1000, 1) + "K";
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

        public static bool IsFutureBond(string securityId)
        {
            bool result = false;

            //BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            //if (bondsInfoPo != null)
            //{
            //    if (bondsInfoPo.MarketIndicator == "5")//国债期货
            //    {
            //        result = true;
            //    }

            //}
            //else
            {
                if (!string.IsNullOrEmpty(securityId) && securityId.StartsWith("T") && !securityId.Contains("."))//国债期货
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
