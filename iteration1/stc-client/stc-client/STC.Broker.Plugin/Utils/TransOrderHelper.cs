using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public class TransOrderHelper
    {
        private static TransOrderHelper instance = null;
        private static readonly object padlock = new object();
        private static Dictionary<string, string> todayDic = new Dictionary<string, string>();

        private string _teamTag;
        public string TeamTag
        {
            get
            {
                return _teamTag;
            }
        }

        private string _nextDate;
        public string NextDate
        {
            get
            {
                return _nextDate;
            }
            set
            {
                _nextDate = value;
            }
        }

        private TransOrderHelper()
        {
            GetNextDate();
        }

        /// <summary>
        /// 合并展示机构
        /// </summary>
        public Dictionary<string, AccountInfo> dicSet = new Dictionary<string, AccountInfo>();
        public static TransOrderHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        instance = new TransOrderHelper();
                    }
                }
                return instance;
            }
        }


        public string GetBondCodeShow(ExecutionReport er)
        {
            string result = string.Empty;

            if (er != null && !string.IsNullOrEmpty(er.SecurityType))
            {
                if (er.SecurityType.Equals("2") || er.SecurityType.Equals("BNC"))
                {
                    if (!string.IsNullOrEmpty(er.BondSubtype) && (er.BondSubtype.Equals("LLB") || er.BondSubtype.Equals("TLB")))
                        result = er.Symbol;
                    else
                        result = er.SecurityID;

                }
                else if (er.SecurityType.Equals("1") || er.SecurityType.Equals("11") || er.SecurityType.Equals("12") ||
                    er.SecurityType.Equals("BCO") || er.SecurityType.Equals("ABS") || er.SecurityType.Equals("NCD"))
                {
                    result = er.Symbol;
                }

            }
            if (string.IsNullOrEmpty(result))
            {
                result = er.SecurityID;
            }
            return result;
        }



        public bool IsDealHistDay(TransactionOrdermModel er)
        {
            DateTime dt;
            if (DateTime.TryParseExact(er.CreateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }


        public bool IsDealHistDay(ExecutionReport er)
        {
            DateTime dt;
            if (DateTime.TryParseExact(er.TransactTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }

        public bool IsDealHistDay(string CreateTime)
        {
            DateTime dt;
            if (DateTime.TryParseExact(CreateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
        public string GetHistoryDisplaySettle(TransactionOrdermModel er)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非错期
            if (!string.IsNullOrEmpty(er.SettleDisplay2) && er.SettleDisplay.Equals(er.SettleDisplay2))
            {
                ///有备注
                if (!string.IsNullOrEmpty(er.Remarks))
                {
                    dtDeal = GetDate(er.Dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + er.Remarks;
                }
                else
                {
                    dtDeal = GetDate(er.Dealdate);
                    DateTime dtSettle = GetDate(er.Settlementdate);
                    DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    var time = dtSettle - dtDeal;
                    if (time.Days == 0)
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+0";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(er.Settlementtype2);
                    }
                    else
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+1";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(er.Settlementtype2);
                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(er.Settlementtype2) && !string.IsNullOrEmpty(er.SettlementType))
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
                else if (!string.IsNullOrEmpty(er.SettlementType) && string.IsNullOrEmpty(er.Settlementtype2))
                {
                    result = GetHistorySettle(er, false);
                }
                else if (string.IsNullOrEmpty(er.SettlementType) && !string.IsNullOrEmpty(er.Settlementtype2))
                {
                    //ofr
                    result = GetHistorySettle(er, true);
                }
            }

            return result;
        }


        /// <summary>
        /// 成交记录，结算类型显示（历史数据）
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public string GetHistoryDisplaySettle(ExecutionReport er)
        {
            string result = string.Empty;
            DateTime dtDeal;

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
                SettleDisplay2 = TransOrderHelper.Instance.GetSettleDisplay(er.SettlType2, er.TradeDate2);
            }

            if (DefaultSettle2.Equals(er.SettlType) && er.SettlType.Contains(DefaultSettle2))
            {
                SettleDisplay = "";
            }
            else
            {
                SettleDisplay = TransOrderHelper.Instance.GetSettleDisplay(er.SettlType, er.TradeDate);
            }
            ///非错期
            if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
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
                if (!string.IsNullOrEmpty(er.SettlType2) && !string.IsNullOrEmpty(er.SettlType2))
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


        public string GetDisplaySettleForMain(TransactionOrdermModel er)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(er.SettleDisplayForMain2) && er.SettleDisplayForMain.Equals(er.SettleDisplayForMain2))
            {
                //备注
                if (!string.IsNullOrEmpty(er.Remarks))
                {
                    DateTime dtDeal = GetDate(er.Dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + er.Remarks;
                }
                else
                    result = er.SettleDisplayForMain;
            }
            else
            //错期
            {
                if (!string.IsNullOrEmpty(er.Settlementtype2) && !string.IsNullOrEmpty(er.SettlementType))
                    result = string.Format("{0}/{1}", er.SettleDisplayForMain2, er.SettleDisplayForMain);
                else if (!string.IsNullOrEmpty(er.SettlementType) && string.IsNullOrEmpty(er.Settlementdate2))
                    result = er.SettleDisplayForMain;
                else if (string.IsNullOrEmpty(er.SettlementType) && !string.IsNullOrEmpty(er.Settlementdate2))
                    result = er.SettleDisplayForMain2;
            }
            return result;
        }


        /// <summary>
        /// 历史单结类型 错期使用,新版本，考虑到节假日的情况，解决 日期+0 会显示成 日期+1
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="isOfr"></param>
        /// <returns></returns>
        public string GetHistorySettle(TransactionOrdermModel orderDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? orderDetailPo.Settlementdate2 : orderDetailPo.Settlementdate;
            string dealTime = isOfr ? orderDetailPo.Dealdate2 : orderDetailPo.Dealdate;
            string settle = isOfr ? orderDetailPo.Settlementtype2 : orderDetailPo.SettlementType;

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

            settle = settleStr;
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

        public string GetHistorySettle(ExecutionReport orderDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? orderDetailPo.SettlDate2 : orderDetailPo.SettlDate;
            string dealTime = isOfr ? orderDetailPo.TradeDate2 : orderDetailPo.TradeDate;
            string settle = isOfr ? orderDetailPo.SettlType2 : orderDetailPo.SettlType;
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
            settle = settleStr;
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
        /// 获取清算速度标准值
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public string GetSettleValue(string settle)
        {
            string settleDisplay = settle;
            switch (settle)
            {
                case "+0":
                    settleDisplay = "1";
                    break;
                case "+1":
                    settleDisplay = "2";
                    break;
                case "明天+0":
                    settleDisplay = "3";
                    break;
            }
            return settleDisplay;
        }


        /// <summary>
        /// 成交记录，结算类型显示
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public string GetDisplaySettle(ExecutionReport er)
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
                SettleDisplay2 = TransOrderHelper.Instance.GetSettleDisplay(er.SettlType2, er.TradeDate2);
            }

            if (DefaultSettle2.Equals(er.SettlType) && er.SettlType.Contains(DefaultSettle2))
            {
                SettleDisplay = "";
            }
            else
            {
                SettleDisplay = TransOrderHelper.Instance.GetSettleDisplay(er.SettlType, er.TradeDate);
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
        /// 成交记录，结算类型显示
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public string GetDisplaySettle(TransactionOrdermModel er)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(er.SettleDisplay2) && er.SettleDisplay.Equals(er.SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(er.Remarks))
                {
                    DateTime dtDeal = GetDate(er.Dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + er.Remarks;
                }
                else
                    result = er.SettleDisplay;
            }
            else
            {
                if (!string.IsNullOrEmpty(er.Settlementtype2) && !string.IsNullOrEmpty(er.SettlementType))
                    result = string.Format("{0}/{1}", er.SettleDisplay2, er.SettleDisplay);
                else if (!string.IsNullOrEmpty(er.SettlementType) && string.IsNullOrEmpty(er.Settlementtype2))
                    result = er.SettleDisplay;
                else if (string.IsNullOrEmpty(er.SettlementType) && !string.IsNullOrEmpty(er.Settlementtype2))
                    result = er.SettleDisplay2;
            }

            return result;
        }

        /// <summary>
        /// 是否是默认结算
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public bool IsDefautSettle(string settle)
        {
            return settle + "" == "+0" || settle + "" == "+1";
        }

        public DateTime GetDate(string time)
        {
            DateTime result;
            DateTime.TryParseExact(time, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

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

            if (settle.IndexOf("/") > -1)
            {
                string[] sett = settle.Split('/');
                if (sett.Length == 2)
                {
                    string settleStr1 = sett[0];
                    string settleStr2 = sett[1];
                    if (!string.IsNullOrWhiteSpace(sett[0]))
                    {
                        if (sett[0].Contains("+0"))
                        {
                            settleStr1 = "+0";
                        }
                        else if (sett[0].Contains("+1"))
                        {
                            settleStr1 = "+1";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(sett[1]))
                    {

                        if (sett[1].Contains("+0"))
                        {
                            settleStr2 = "+0";
                        }
                        else if (sett[1].Contains("+1"))
                        {
                            settleStr2 = "+1";
                        }
                    }
                    return settleStr1 + "/" + settleStr2;
                }
            }
            else
            {
                if (settle.Contains("+0"))
                    return "+0";
                if (settle.Contains("+1"))
                    return "+1";
            }


            return string.Empty;
        }
        /// <summary>
        /// 获取显示的结算类型
        /// 成交日期不在显示明天或者周几，除了+0和+1以外，全部显示日期，展示为10.14+0
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="dealDate"></param>
        /// <returns></returns>
        public string GetSettleShow(string settle, string dealDate)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(settle))
            {
                return result;
            }

            if (IsDefautSettle(settle))
                result = settle;
            else
            {
                if (string.IsNullOrEmpty(dealDate))
                {
                    result = settle;
                }
                else
                {
                    DateTime dtDeal = GetDate(dealDate);
                    string dealStr = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString());
                    result = dealStr + GetDefautSettle(settle);
                }
            }
            return result;
        }


        /// <summary>
        /// 获取显示的结算类型
        /// 成交日期不在显示明天或者周几，除了+0和+1以外，全部显示日期，展示为10.14+0
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="dealDate"></param>
        /// <returns></returns>
        public string GetSettleDisplay(string settle, string dealDate)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(settle))
            {
                return result;
            }
            string settleStr = settle;
            string key = "/";
            if (settle.IndexOf(key) > -1)
            {
                string[] sett = settle.Split('/');
                if (sett.Length == 2)
                {
                    string settleStr1 = sett[0];
                    string settleStr2 = sett[1];
                    if (!string.IsNullOrWhiteSpace(sett[0]))
                    {
                        switch (sett[0])
                        {
                            case "1":
                                settleStr1 = "+0";
                                break;
                            case "2":
                                settleStr1 = "+1";
                                break;
                            case "3":
                                settleStr1 = "明天+0";
                                break;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(sett[1]))
                    {

                        switch (sett[1])
                        {
                            case "1":
                                settleStr2 = "+0";
                                break;
                            case "2":
                                settleStr2 = "+1";
                                break;
                            case "3":
                                settleStr2 = "明天+0";
                                break;
                        }
                    }
                    settleStr = settleStr1 + "/" + settleStr2;
                }
            }
            else
            {
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
                    if (settleStr.StartsWith("/"))
                    {
                        settleStr = settleStr.TrimStart('/');
                        DateTime dtDeal = GetDate(dealDate);
                        string dealStr = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString());
                        result = "/"+dealStr + GetDefautSettle(settleStr);
                    }
                    else {
                        DateTime dtDeal = GetDate(dealDate);
                        string dealStr = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString());
                        result = dealStr + GetDefautSettle(settleStr);
                    }
                    
                }
            }
            return result;
        }

        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public string GetOperateType(ExecutionReport er)
        {
            string result = string.Empty;

            if ("0".Equals(er.OrdStatus)
                || "1".Equals(er.OrdStatus)
                || "2".Equals(er.OrdStatus))
            {
                result = "新意向";

            }
            else if ("E".Equals(er.OrdStatus))
            {
                if (er.BrokerBConfirmQty != er.MatchQTY)
                {
                    result = "部分确认";

                }
                else
                {
                    result = "全部确认";
                }
            }
            else if ("F".Equals(er.OrdStatus))
            {
                result = "拒绝";
            }
            else if ("H".Equals(er.OrdStatus))
            {
                result = "拒绝";
            }
            else if ("C".Equals(er.OrdStatus))
            {
                result = "交易撤销";
            }
            else if ("I".Equals(er.OrdStatus))
            {
                result = "确认";
            }
            //客户端自定义状态显示
            else if ("a".Equals(er.OrdStatus))
                result = "修改债券";
            else if ("b".Equals(er.OrdStatus))
                result = "修改价格";
            else if ("c".Equals(er.OrdStatus))
                result = "修改数量";
            else if ("d".Equals(er.OrdStatus))
                result = "修改结算";
            else if ("e".Equals(er.OrdStatus))
                result = "修改交易员";
            else if ("f".Equals(er.OrdStatus))
                result = "修改Broker";
            else if ("g".Equals(er.OrdStatus))
                result = "修改发给对象";
            else if ("h".Equals(er.OrdStatus))
                result = "新增桥";
            else if ("i".Equals(er.OrdStatus))
                result = "修改桥";
            else if ("j".Equals(er.OrdStatus))
                result = "删除桥";
            else if ("k".Equals(er.OrdStatus))
                result = "修改价格/量";
            else if ("l".Equals(er.OrdStatus))
                result = "修改价格描述";
            else if ("m".Equals(er.OrdStatus))
                result = "DCS";
            else if ("n".Equals(er.OrdStatus))
                result = "修改桥状态";
            else if ("G".Equals(er.OrdStatus))
            {
                //if ((IsChangeAmount || IsChangePrice) && string.IsNullOrEmpty(dcsflag))
                //{
                //    result = "修改";
                //}
                //if (string.IsNullOrEmpty(brokerunread) || (!brokerunread.Contains(brokerIdA + "") && !brokerunread.Contains(brokerIdB + "")))
                if (string.IsNullOrEmpty(er.BrokerUnread) || (!ContainsUserId(er.BrokerUnread, er.PartyID) && !ContainsUserId(er.BrokerUnread, er.CounterPartySubID)))
                {
                    result = "确认";
                }
            }

            return result;
        }

        public bool ContainsUserId(string self, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
            string parent = string.Format(",{0},", self);
            string child = string.Format(",{0},", userId);
            if (parent.Contains(child))
                return true;
            return false;
        }


        /// <summary>
        /// 获取交易员A展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public string GetTraderAInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.PartyCode) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                    return string.Format("{0}({1})", orderDetail.InstitutionShowName, orderDetail.TraderName.Trim());
                else
                    return string.Format("{0}({1})", orderDetail.InstitutionName, orderDetail.TraderName.Trim());
            }

            if (string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                    return orderDetail.InstitutionShowName;
                else
                    return orderDetail.InstitutionName;
            }
            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                return orderDetail.TraderName;

            return "";
        }

        /// <summary>
        /// 获取交易员B展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public string GetTraderBInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                    return string.Format("{0}({1})", orderDetail.CounterPartyInstitutionShowName, orderDetail.CounterPartyTraderName.Trim());
                else
                    return string.Format("{0}({1})", orderDetail.CounterPartyInstitutionName, orderDetail.CounterPartyTraderName.Trim());
            }


            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                    return orderDetail.CounterPartyInstitutionShowName;
                else
                    return orderDetail.CounterPartyInstitutionName;
            }

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                return orderDetail.CounterPartyTraderName;

            return "";
        }

        #region 通用
        public string GetDisplaySettle(string SettleDisplay, string SettleDisplay2, string Remarks, string Dealdate, string SettlementType, string Settlementtype2)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(Remarks))
                {
                    DateTime dtDeal = GetDate(Dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + Remarks;
                }
                else
                    result = SettleDisplay;
            }
            else
            {
                if (!string.IsNullOrEmpty(Settlementtype2) && !string.IsNullOrEmpty(SettlementType))
                    result = string.Format("{0}/{1}", SettleDisplay2, SettleDisplay);
                else if (!string.IsNullOrEmpty(SettlementType) && string.IsNullOrEmpty(Settlementtype2))
                    result = SettleDisplay;
                else if (string.IsNullOrEmpty(SettlementType) && !string.IsNullOrEmpty(Settlementtype2))
                    result = SettleDisplay2;
            }

            return result;
        }

        public string GetHistoryDisplaySettle(string SettleDisplay, string SettleDisplay2, string Remarks, string Dealdate, string Dealdate2, string SettlementType, string Settlementtype2, string Settlementdate, string Settlementdate2)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非错期
            if (!string.IsNullOrEmpty(SettleDisplay2) && SettleDisplay.Equals(SettleDisplay2))
            {
                ///有备注
                if (!string.IsNullOrEmpty(Remarks))
                {
                    dtDeal = GetDate(Dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + Remarks;
                }
                else
                {
                    dtDeal = GetDate(Dealdate);
                    DateTime dtSettle = GetDate(Settlementdate);
                    DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    var time = dtSettle - dtDeal;
                    if (time.Days == 0)
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+0";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(Settlementtype2);
                    }
                    else
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+1";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(Settlementtype2);
                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(Settlementtype2) && !string.IsNullOrEmpty(SettlementType))
                {
                    //ofr
                    ofrResult = GetHistorySettle(Settlementdate, Settlementdate2, Dealdate, Dealdate2, SettlementType, Settlementtype2, true);
                    //bid
                    bidResult = GetHistorySettle(Settlementdate, Settlementdate2, Dealdate, Dealdate2, SettlementType, Settlementtype2, false);

                    if (ofrResult.Equals(bidResult))
                        result = ofrResult;
                    else
                        result = string.Format("{0}/{1}", ofrResult, bidResult);
                }
                else if (!string.IsNullOrEmpty(SettlementType) && string.IsNullOrEmpty(Settlementtype2))
                {
                    result = GetHistorySettle(Settlementdate, Settlementdate2, Dealdate, Dealdate2, SettlementType, Settlementtype2, false);
                }
                else if (string.IsNullOrEmpty(SettlementType) && !string.IsNullOrEmpty(Settlementtype2))
                {
                    //ofr
                    result = GetHistorySettle(Settlementdate, Settlementdate2, Dealdate, Dealdate2, SettlementType, Settlementtype2, true);
                }
            }

            return result;
        }

        public string GetHistorySettle(string Settlementdate, string Settlementdate2, string Dealdate, string Dealdate2, string SettlementType, string Settlementtype2, bool isOfr)
        {
            string settleTime = isOfr ? Settlementdate2 : Settlementdate;
            string dealTime = isOfr ? Dealdate2 : Dealdate;
            string settle = isOfr ? Settlementtype2 : SettlementType;

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
            settle = settleStr;
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
        #endregion

        #region 交易处理订单使用
        public TransOrderTxt ConvertOrderDataToTxt(TransOrderServerData er, bool isHist, string traderSide, string assistBrokerId = null)
        {
            string Code = "";
            string Type = "";
            string insta = "";
            string instb = "";
            string volume = "";
            TransOrderTxt transOrderTxt = new TransOrderTxt();
            transOrderTxt.NextDate = NextDate;
            //Type = OrderHelper.GetDisplaySettle(order);
            Type = isHist ? GetHistoryDisplaySettle(er) : GetDisplaySettle(er);
            #region 方向判断
            insta = GetInstName(er, 0);
            instb = GetInstName(er, 1);
            if (string.IsNullOrEmpty(insta))
                insta = "机构待定";
            if (string.IsNullOrEmpty(instb))
                instb = "机构待定";
            if (!string.IsNullOrWhiteSpace(er.LastQty))
            {
                decimal qty = decimal.Zero;
                if (decimal.TryParse(er.LastQty, out qty))
                {
                    if (qty / 10000 >= 1)
                    {
                        volume = string.Format("{0}E", qty / 10000);
                    }
                    else
                    {
                        volume = string.Format("{0}", qty);
                    }
                }
            }
            string bankAgentNameA = (string.IsNullOrWhiteSpace(insta) || dicSet.ContainsKey(insta)) ? string.Format(" {0}", er.TraderName) : "";
            string bankAgentNameB = (string.IsNullOrWhiteSpace(instb) || dicSet.ContainsKey(instb)) ? string.Format(" {0}", er.CounterPartyTraderName) : "";
            string goodsCode = er.SecurityID.Replace(".IB", "");
            string goodsName = (GRSettingsHandler.GlobSystemParameter != null && "true".Equals(GRSettingsHandler.GlobSystemParameter.formatShortNameCheck)) ? string.Format("{0}  ", er.Symbol) : "";

            if (string.IsNullOrEmpty(goodsName) && BondHelper.IsLGBBond(er.SecurityID) && GRSettingsHandler.AppSettingIsTrue(Constant.WithShortNameLGB))
            {
                goodsName = string.Format("{0} ", er.Symbol);
            }

            if (GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype))
            {
                BondsInfoPo bond = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.SecurityID);
                if (bond != null && !string.IsNullOrWhiteSpace(bond.Asset_Status) && bond.Asset_Status == "PRI")
                {
                    goodsName = goodsName + " " + "私募债" + "  ";
                }
            }

            //if (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatDebtTypeCheck))
            //{
            //    string fundObjectivesCategoryStr = CopyDetailHelper.GetFundObjectivesCategory(er.goodsCode);
            //    if (!string.IsNullOrWhiteSpace(fundObjectivesCategoryStr))
            //    {
            //        goodsName = goodsName + " " + fundObjectivesCategoryStr;
            //    }
            //}


            if (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatDebtTypeCheck))
            {
                string fundObjectivesCategoryStr = CopyDetailHelper.GetFundObjectivesCategory(er.SecurityID);
                if (!string.IsNullOrWhiteSpace(fundObjectivesCategoryStr))
                {
                    goodsName = goodsName + " " + fundObjectivesCategoryStr;
                }
            }
            string tenor = (GRSettingsHandler.GlobSystemParameter != null && "true".Equals(GRSettingsHandler.GlobSystemParameter.formatTenorCheck)) ? string.Format("{0}  ", er.TimeToMaturity) : "";

            //string tenor = $"{ er.TimeToMaturity}  ";
            bool isDetail = true;
            string tsento = "";
            string side = "";
            decimal d;
            string fandian = string.IsNullOrEmpty(er.FanDian) ? string.Empty : decimal.TryParse(er.FanDian, out d) ? "F" + er.FanDian : er.FanDian;
            string price = string.Format("{0}{1}", er.Price, fandian);
            //string price = string.IsNullOrWhiteSpace(order.fandian) ? string.Format("{0}{1}", order.PriceFormat, order.fandian) : string.Format("{0}F{1}", order.PriceFormat, order.fandian);

            var comparedName = string.IsNullOrEmpty(assistBrokerId) ? LocalEnv.UserInfo.UserId : assistBrokerId;

            string bidBridgeName = "";
            string ofrBridgeName = "";
            //发给文本
            string sendToTxt = string.Empty;

            if (string.IsNullOrWhiteSpace(traderSide))
            {
                if (!string.IsNullOrWhiteSpace(er.PartySubID)
                    && !string.IsNullOrWhiteSpace(er.CounterPartySubID)
                    && er.PartySubID == er.CounterPartySubID)
                {
                    traderSide = er.Side;
                }

            }
            traderSide = er.Side;
            if (er.Side == "2")
            {
                if (comparedName.Equals(er.PartySubID) && comparedName.Equals(er.CounterPartySubID))
                {
                    if (traderSide == "2")
                    {
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                         Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb
                         );
                        tsento = GetDsendtoTxt(er, true);
                        side = "Ofr";
                        sendToTxt = tsento;
                    }
                    else if (traderSide == "2")
                    {
                        transOrderTxt.TradeDate = er.TradeDate;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, false);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} 出给 {7}{8}", tenor,
                           Code, goodsName, price, volume, Type, insta, instb, bankAgentNameB
                        );
                        tsento = GetDsendtoTxt(er, false);
                        side = "Bid";
                        sendToTxt = tsento;
                    }
                    else
                    {
                        //brokerA=brokerB=自己 显示在今日无机构成交单，按ofr方显示
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                         Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb
                         );
                        tsento = GetDsendtoTxt(er, true);
                        side = "Ofr";
                        sendToTxt = tsento;
                    }
                    if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                    {
                        //过桥没有加桥（只有发送对象）
                        transOrderTxt.isShowColor = true;
                        if ((string.IsNullOrWhiteSpace(tsento))) { }

                        else
                        {
                            transOrderTxt.Text += ", " + tsento;
                            transOrderTxt.WithTraderText += ", " + tsento;
                            transOrderTxt.HideSendQQMsgText += ", " + tsento;
                            sendToTxt = ", " + tsento;
                        }
                        transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                    }
                    else
                    {
                        string sendToStr = GetSendTo(er, isDetail, "O", transOrderTxt, side, null, comparedName);
                        //o.Text += GetSendTo(order, isDetail, "O", o, side, null, comparedName);
                        transOrderTxt.Text += sendToStr;
                        transOrderTxt.WithTraderText += sendToStr;
                        transOrderTxt.HideSendQQMsgText += sendToStr;
                        sendToTxt = sendToStr;
                        //隐藏对手方文本
                        if (IsHaseBride(er, transOrderTxt))
                        {
                            switch (side)
                            {
                                case "Ofr":
                                    bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId, er.SecurityType);
                                    transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} {bankAgentNameA} 出给 {bidBridgeName}";
                                    transOrderTxt.HideCounterpartyText += sendToStr;
                                    break;
                                case "Bid":
                                    bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId2, er.SecurityType);
                                    transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {bidBridgeName} 出给 {instb}{bankAgentNameB}";
                                    transOrderTxt.HideCounterpartyText += sendToStr;
                                    break;
                                default:
                                    transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                                    break;
                            }
                        }
                        else
                        {
                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                    }
                }
                else
                {
                    if (comparedName.Equals(er.PartySubID))
                    {
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                          Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb
                        );
                        side = "Ofr";
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        { //过桥没有加桥（只有发送对象）
                            string sendStr = GetDsendtoTxt(er, true);
                            transOrderTxt.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(sendStr))) { }
                            else
                            {
                                transOrderTxt.Text += ", " + sendStr;
                                sendToTxt = ", " + sendStr;
                            }


                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(er, isDetail, "O", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToStr;
                            sendToTxt = sendToStr;
                            //隐藏对手方文本
                            if (IsHaseBride(er, transOrderTxt))
                            {
                                switch (side)
                                {
                                    case "Ofr":
                                        bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId, er.SecurityType);
                                        transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} {bankAgentNameA} 出给 {bidBridgeName}";
                                        transOrderTxt.HideCounterpartyText += sendToStr;
                                        break;
                                    case "Bid":
                                        bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId2, er.SecurityType);
                                        transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {bidBridgeName} 出给 {instb}{bankAgentNameB}";
                                        transOrderTxt.HideCounterpartyText += sendToStr;
                                        break;
                                    default:
                                        transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                                        break;
                                }
                            }
                            else
                            {
                                transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                            }

                        }

                    }
                    else if (comparedName.Equals(er.CounterPartySubID))
                    {
                        transOrderTxt.TradeDate = er.TradeDate;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, false);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} 出给 {7}{8}", tenor,
                           Code, goodsName, price, volume, Type, insta, instb, bankAgentNameB
                          );
                        side = "Bid";
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        { //过桥没有加桥（只有发送对象）
                            transOrderTxt.isShowColor = true;
                            string sendStr = GetDsendtoTxt(er, false);
                            if ((string.IsNullOrWhiteSpace(sendStr))) { }
                            else
                            {
                                transOrderTxt.Text += ", " + sendStr;
                                sendToTxt = ", " + sendStr;
                            }
                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(er, isDetail, "B", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToStr;
                            transOrderTxt.WithTraderText += sendToStr;
                            transOrderTxt.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (IsHaseBride(er, transOrderTxt))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId2, er.SecurityType);
                                transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {bidBridgeName} 出给 {instb}{bankAgentNameB}";
                                transOrderTxt.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                            }
                        }
                    }
                    else
                    {
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} {7} 出给 {8} {9}", tenor,
                             goodsCode, goodsName, price, volume, Type, insta, er.TraderName, instb, er.CounterPartyTraderName
                            );
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        {//过桥没有加桥（只有发送对象）
                            transOrderTxt.isShowColor = true;
                            string sendStr = GetDsendtoTxt(er, false);
                            string sendStr2 = GetDsendtoTxt(er, true);
                            if (!string.IsNullOrWhiteSpace(sendStr) && !string.IsNullOrWhiteSpace(sendStr2))
                            {
                                transOrderTxt.Text += ",ofr" + sendStr2 + ";bid" + sendStr;
                                sendToTxt = ",ofr" + sendStr2 + ";bid" + sendStr;
                            }
                            else
                            {
                                //if ("2".Equals(order.bridgeflag))

                                if (!string.IsNullOrWhiteSpace(sendStr2))
                                {
                                    transOrderTxt.Text += ",ofr" + sendStr2;
                                    sendToTxt = ",ofr" + sendStr2;
                                }
                                else if (!string.IsNullOrWhiteSpace(sendStr))
                                {
                                    transOrderTxt.Text += ",bid" + sendStr;
                                    sendToTxt = ",bid" + sendStr;
                                }

                            }
                        }
                        else
                        {
                            string sendToContent = GetSendTo(er, isDetail, "OB", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToContent;
                            sendToTxt = sendToContent;
                        }
                        transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                    }
                }
                string[] traderInfo = new string[] {
                    insta,bankAgentNameA, instb, bankAgentNameB
                };

                string num = string.Empty;

                string[] copyText = new string[] {
                    num,tenor, Code, goodsName,price,volume, Type, sendToTxt
                };
                SetHideBridgeInfo(er, traderInfo, copyText, side, transOrderTxt, true, true);
            }
            else
            {
                if (comparedName.Equals(er.PartySubID) && comparedName.Equals(er.CounterPartySubID))
                {
                    if (traderSide == "2")
                    {
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                           Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta
                           );
                        tsento = GetDsendtoTxt(er, true);
                        side = "Ofr";
                        sendToTxt = tsento;
                    }
                    else if (traderSide == "1")
                    {
                        transOrderTxt.TradeDate = er.TradeDate;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, false);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} 出给 {7}{8}", tenor,
                             Code, goodsName, price, volume, Type, instb, insta, bankAgentNameA
                             );
                        tsento = GetDsendtoTxt(er, false);
                        side = "Bid";
                        sendToTxt = tsento;
                    }
                    else
                    {
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                           Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta
                           );
                        tsento = GetDsendtoTxt(er, true);
                        side = "Ofr";
                        sendToTxt = tsento;
                    }
                    if ("2".Equals(er.BridgeFlag) && BridgeIsAllConfirmed(er))
                    { //过桥没有加桥（只有发送对象）
                        transOrderTxt.isShowColor = true;
                        if (!(string.IsNullOrWhiteSpace(tsento)))
                        {
                            transOrderTxt.Text += ", " + tsento;
                            sendToTxt = ", " + tsento;
                        }
                        transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                    }
                    else
                    {
                        string sendToStr = GetSendTo(er, isDetail, "O", transOrderTxt, side, null, comparedName);
                        transOrderTxt.Text += sendToStr;
                        transOrderTxt.WithTraderText += sendToStr;
                        transOrderTxt.HideSendQQMsgText += sendToStr;
                        sendToTxt = sendToStr;
                        //隐藏对手方文本
                        if (IsHaseBride(er, transOrderTxt))
                        {
                            switch (side)
                            {
                                case "Ofr":
                                    bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId, er.SecurityType);
                                    transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} {bankAgentNameB} 出给 {bidBridgeName}";
                                    transOrderTxt.HideCounterpartyText += sendToStr;
                                    break;
                                case "Bid":
                                    bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId2, er.SecurityType);
                                    transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {bidBridgeName} 出给 {insta}{bankAgentNameA}";
                                    transOrderTxt.HideCounterpartyText += sendToStr;
                                    break;
                                default:
                                    transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                                    break;
                            }
                        }
                        else
                        {
                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                    }
                }
                else
                {
                    if (comparedName.Equals(er.CounterPartySubID))
                    {
                        transOrderTxt.TradeDate = er.TradeDate2;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, true);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6}{7} 出给 {8}", tenor,
                         Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta
                         );
                        side = "Ofr";
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        { //过桥没有加桥（只有发送对象）
                            transOrderTxt.isShowColor = true;
                            string sendStr = GetDsendtoTxt(er, false);
                            string sendStr2 = GetDsendtoTxt(er, true);
                            if ((string.IsNullOrWhiteSpace(sendStr2))) { }
                            else
                            {
                                transOrderTxt.Text += ", " + sendStr2;
                                sendToTxt = ", " + sendStr2;
                            }
                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(er, isDetail, "O", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToStr;
                            transOrderTxt.WithTraderText += sendToStr;
                            transOrderTxt.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (IsHaseBride(er, transOrderTxt))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId, er.SecurityType);
                                transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} {bankAgentNameB} 出给 {bidBridgeName}";
                                transOrderTxt.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                            }
                        }
                    }
                    else if (comparedName.Equals(er.PartySubID))
                    {
                        string sendStr = GetDsendtoTxt(er, false);
                        string sendStr2 = GetDsendtoTxt(er, true);
                        transOrderTxt.TradeDate = er.TradeDate;
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = GetBondCodeTxt(er, false);
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} 出给 {7}{8}", tenor,
                          Code, goodsName, price, volume, Type, instb, insta, bankAgentNameA
                           );
                        side = "Bid";
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        { //过桥没有加桥（只有发送对象）
                            transOrderTxt.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(sendStr))) { }
                            else
                            {
                                transOrderTxt.Text += ", " + sendStr;
                                sendToTxt = ", " + sendStr;
                            }
                            transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(er, isDetail, "B", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToStr;
                            transOrderTxt.WithTraderText += sendToStr;
                            transOrderTxt.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (IsHaseBride(er, transOrderTxt))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(er.BridgeInstitutionId2, er.SecurityType);
                                transOrderTxt.HideCounterpartyText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {bidBridgeName} 出给 {insta}{bankAgentNameA}";
                                transOrderTxt.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                            }
                        }
                    }
                    else
                    {
                        transOrderTxt.Text = string.Format("{0}{1}  {2}{3}  {4}  {5} {6} {7} 出给 {8} {9}", tenor,
                              goodsCode, goodsName, price, volume, Type, instb, er.CounterPartyTraderName, insta, er.TraderName
                              );
                        string sendStr = GetDsendtoTxt(er, false);
                        string sendStr2 = GetDsendtoTxt(er, true);
                        if ("2".Equals(er.BridgeFlag) && !BridgeIsAllConfirmed(er))
                        {//过桥没有加桥（只有发送对象）
                            transOrderTxt.isShowColor = true;
                            if (!string.IsNullOrWhiteSpace(sendStr) && !string.IsNullOrWhiteSpace(sendStr2))
                            {
                                transOrderTxt.Text += ",ofr" + sendStr2 + ";bid" + sendStr;
                                sendToTxt = ",ofr" + sendStr2 + ";bid" + sendStr;
                            }
                            else
                            {
                                //if ("2".Equals(order.bridgeflag))

                                if (!string.IsNullOrWhiteSpace(sendStr2))
                                {
                                    transOrderTxt.Text += ",ofr" + sendStr2;
                                    sendToTxt = ",ofr" + sendStr2;
                                }
                                else if (!string.IsNullOrWhiteSpace(sendStr))
                                {
                                    transOrderTxt.Text += ",bid" + sendStr;
                                    sendToTxt = ",bid" + sendStr;
                                }
                            }
                        }
                        else
                        {
                            string sendToContent = GetSendTo(er, isDetail, "OB", transOrderTxt, side, null, comparedName);
                            transOrderTxt.Text += sendToContent;
                            sendToTxt = sendToContent;
                        }
                        transOrderTxt.HideCounterpartyText = transOrderTxt.Text;
                    }
                }

                transOrderTxt.AssistantAText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} 买入";
                transOrderTxt.AssistantBText = $"{tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} 卖出";

                string[] traderInfo = new string[] {
                    insta,bankAgentNameA, instb, bankAgentNameB
                };
                string num = string.Empty;
                string[] copyText = new string[] {
                    num,tenor, Code, goodsName,price,volume, Type, sendToTxt
                };
                SetHideBridgeInfo(er, traderInfo, copyText, side, transOrderTxt, true, true);
            }
            #endregion
            return transOrderTxt;
        }


        /// <summary>
        /// 拼接关联机构文本字符串
        /// </summary>
        /// <param name="order">订单</param>
        /// <param name="traderInfo">交易员信息</param>
        /// <param name="copyText">文本要素</param>
        /// <param name="side">订单方向</param>
        /// <param name="o">文本对象</param>
        /// <param name="isRecordDetail">是否是主订单列表文本</param>
        private void SetHideBridgeInfo(TransOrderServerData order, string[] traderInfo, string[] copyText, string side, TransOrderTxt o, bool isRecordDetail, bool isbroker)
        {
            //机构信息
            string counterpartyA = string.Empty;
            string counterpartyB = string.Empty;
            //机构信息带交易员
            string counterpartyTraderA = string.Empty;
            string counterpartyTraderB = string.Empty;
            if (order.IsGVN)
            {
                counterpartyA = $"{traderInfo[0]}{traderInfo[1]}";
                counterpartyB = $"{traderInfo[2]}{traderInfo[3]}";

                counterpartyTraderA = $"{traderInfo[0]} {order.PartySubName}";
                counterpartyTraderB = $"{traderInfo[2]} {order.CounterPartySubName}";
            }
            else
            {
                counterpartyA = $"{traderInfo[2]}{traderInfo[3]}";
                counterpartyB = $"{traderInfo[0]}{traderInfo[1]}";

                counterpartyTraderA = $"{traderInfo[2]} {order.CounterPartySubName}";
                counterpartyTraderB = $"{traderInfo[0]} {order.PartySubName}";
            }
            if (order.AssociatedFlag == "1")
            {
                if ((SystemPowerHandler.Instance.IsCanEditOrder(order.PartySubID)
                    || SystemPowerHandler.Instance.IsCanEditOrder(order.CounterPartySubID))
                    && !SystemPowerHandler.Instance.IsBridge)
                {
                    if (IsHaseBride(order, o))
                    {
                        switch (side)
                        {
                            case "Ofr":
                                counterpartyB = CommonUtils.GetInstName2(order.BridgeInstitutionId, order.SecurityType);
                                counterpartyTraderB = counterpartyB;
                                break;
                            case "Bid":
                                counterpartyA = CommonUtils.GetInstName2(order.BridgeInstitutionId2, order.SecurityType);
                                counterpartyTraderA = counterpartyA;
                                break;
                            default:
                                break;
                        }
                        if (isRecordDetail)
                        {
                            o.CopyText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {copyText[7]}";
                            o.WithTraderText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";

                        }
                        else
                        {
                            if (isbroker)
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {copyText[7]}";
                            }
                            else
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";
                            }

                            o.WithTraderText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";
                        }
                    }
                    else
                    {
                        switch (side)
                        {
                            case "Ofr":
                                counterpartyB = "机构待定";
                                counterpartyTraderB = counterpartyB;
                                break;
                            case "Bid":
                                counterpartyA = "机构待定";
                                counterpartyTraderA = counterpartyA;
                                break;
                            default:
                                break;
                        }
                        //获取备注代付费
                        string remarks = GetRemarkContent(order, true, o);
                        if (isRecordDetail)
                        {
                            o.CopyText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {remarks}";
                            o.WithTraderText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                        }
                        else
                        {
                            if (isbroker)
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {remarks}";
                            }
                            else
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                            }

                            o.WithTraderText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                        }
                        o.HideCounterpartyText = o.CopyText;
                    }
                }
                else
                {
                    o.CopyText = o.Text;
                }
            }
            else
            {
                o.CopyText = o.Text;
            }
        }


        /// <summary>
        /// 复制标准化订单的备注内容
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isDetail"></param>
        /// <param name="bridgeType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public string GetRemarkContent(TransOrderServerData order, bool isDetail, TransOrderTxt o)
        {
            string send = "";
            if (isDetail)
            {
                if (o.isShowColor)
                    return send;
                if (!"2".Equals(order.BridgeFlag) && string.IsNullOrWhiteSpace(order.BridgeType))
                {
                    //发给标准化
                    if (!string.IsNullOrWhiteSpace(order.IssueState) && order.IssueState == "1")
                    {
                        string staSend = "";
                        if (!string.IsNullOrWhiteSpace(order.ProxyFee))
                        {
                            staSend = "," + order.ProxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.IssueRemark))
                        {
                            staSend = staSend + "," + order.IssueRemark;
                        }
                        send = staSend;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(order.SendTo))
                            send = "," + order.SendTo;
                        if (!string.IsNullOrWhiteSpace(order.ProxyFee))
                        {
                            send = "," + order.ProxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.IssueRemark))
                            send += "," + order.IssueRemark;
                    }

                }
            }
            return send;
        }

        /// <summary>
        /// 获取订单的指定机构
        /// </summary>
        /// <param name="order"></param>
        /// <param name="Ab"></param>
        /// <returns></returns>
        public string GetInstName(ExecutionReport order, int Ab)
        {
            string key = "";
            switch (Ab)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(order.InstitutionShowName))
                        key = order.InstitutionShowName;
                    else
                        key = order.InstitutionName;
                    break;
                case 1:
                    if (!string.IsNullOrWhiteSpace(order.CounterPartyInstitutionShowName))
                        key = order.CounterPartyInstitutionShowName;
                    else
                        key = order.CounterPartyInstitutionName;
                    break;
            }
            return key;
        }
        /// <summary>
        /// 给明细用
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public string GetBondCodeTxt(ExecutionReport er, bool isGvn)
        {

            if (string.IsNullOrEmpty(er.SecurityID))
                return string.Empty;
            if (isGvn)
            {
                if (IsShowDate(er.SettlDate2, ConvertTextHelper.NextDate))
                {
                    string TradeDate = string.Empty;
                    if (!string.IsNullOrEmpty(er.TradeDate2) && er.TradeDate2.Length == 8)
                    {
                        TradeDate = er.TradeDate2.Insert(4, "-").Insert(7, "-");
                    }
                    string[] s = TradeDate.Split('-');
                    if (s.Length == 3)
                        TradeDate = string.Format("{0}.{1}", s[1], s[2]);

                    return string.Format("{0}({1} 远)", er.SecurityID.Replace(".IB", ""), TradeDate);
                }
                else
                {
                    return er.SecurityID.Replace(".IB", "");
                }
            }
            else
            {
                if (IsShowDate(er.SettlDate, ConvertTextHelper.NextDate))
                {
                    string TradeDate = string.Empty;
                    if (!string.IsNullOrEmpty(er.TradeDate) && er.TradeDate.Length == 8)
                    {
                        TradeDate = er.TradeDate.Insert(4, "-").Insert(7, "-");
                    }
                    string[] s = TradeDate.Split('-');
                    if (s.Length == 3)
                        TradeDate = string.Format("{0}.{1}", s[1], s[2]);

                    return string.Format("{0}({1} 远)", er.SecurityID.Replace(".IB", ""), TradeDate);
                }
                else
                {
                    return er.SecurityID.Replace(".IB", "");
                }
            }
        }

        public string GetDsendtoTxt(ExecutionReport er, bool isGvn)
        {
            if (isGvn)
            {
                return ERemarkShow.不显示.EqualsToValue(er.BridgeRemarkShow) ? string.IsNullOrEmpty(er.BridgeRemark) ? er.SendTo2 : (er.SendTo2 + "").Replace(", " + er.BridgeRemark + "", "") : er.SendTo2;
            }
            else
            {
                return ERemarkShow.不显示.EqualsToValue(er.BridgeRemarkShow2) ? string.IsNullOrEmpty(er.BridgeRemark2) ? er.SendTo : (er.SendTo + "").Replace(", " + er.BridgeRemark2 + "", "") : er.SendTo;
            }

        }

        /// <summary>
        /// 结算日大于NextDate显示日期
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public bool IsShowDate(string settleDate, string NextDate)
        {
            string Date;
            bool result = false;
            if (string.IsNullOrWhiteSpace(settleDate) || string.IsNullOrWhiteSpace(NextDate))
                return result;
            try
            {

                if (settleDate.Length == 8)
                {
                    settleDate = settleDate.Insert(4, "-").Insert(7, "-");
                }
                if (NextDate.Length == 8)
                {
                    NextDate = NextDate.Insert(4, "-").Insert(7, "-");
                }
                DateTime t1 = Convert.ToDateTime(settleDate);
                DateTime t2 = Convert.ToDateTime(NextDate);
                int compNum = DateTime.Compare(t1, t2);
                if (compNum > 0)
                {
                    result = true;
                    string[] s = settleDate.Split('-');
                    if (s.Length == 3)
                        Date = string.Format("{0}.{1}", s[1], s[2]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return result;
        }


        /// <summary>
        /// 判断单子是否成功添加所有的桥
        /// 根据bridgeorder的状态判断
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool BridgeIsAllConfirmed(TransOrderServerData order)
        {
            bool flag = true;
            //不需要加桥
            if (!(order.BridgeFlag + "").Equals("2"))
            {
                flag = true;
            }
            //需要加桥
            else
            {   //无桥
                if (string.IsNullOrEmpty(order.BridgeType))
                {
                    flag = false;
                }
                //单桥
                else if (!string.IsNullOrEmpty(order.BridgeType) && order.BridgeType == "B,O")
                {
                    //老单子bridgeOrder为空，默认确认
                    if (order.BridgeOrder != null)
                    {
                        if (order.BridgeOrder != null
                           && IsConfirmed(order.BridgeOrderModel.BidbridgeStateEnum)
                           && IsConfirmed(order.BridgeOrderModel.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = true;

                }
                //双桥
                else
                {
                    if (order.BridgeOrderModel != null)
                    {
                        if (order.BridgeOrderModel != null
                             && IsConfirmed(order.BridgeOrderModel.BidbridgeStateEnum)
                             && IsConfirmed(order.BridgeOrderModel.OfrbridgeStateEnum)
                             && order.BridgeOrderModel2 != null
                             && IsConfirmed(order.BridgeOrderModel2.BidbridgeStateEnum)
                             && IsConfirmed(order.BridgeOrderModel2.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;

                    }
                    else
                        flag = true;
                }
            }
            return flag;
        }

        /// <summary>
        /// 是否确认状态
        /// </summary>
        /// <param name="bridgeState"></param>
        /// <returns></returns>
        public bool IsConfirmed(BridgeState bridgeState)
        {
            bool isConfirmed = false;
            if (bridgeState == BridgeState.Confirmed ||
                bridgeState == BridgeState.Finished ||
                bridgeState == BridgeState.Transferred ||
                bridgeState == BridgeState.RequestReturned ||
                bridgeState == BridgeState.IssuedOrder)
            {
                isConfirmed = true;
            }

            return isConfirmed;
        }

        /// <summary>
        /// 明细有桥时候的发给内容
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isDetail"></param>
        /// <param name="bridgeType"></param>
        /// <param name="o"></param>
        /// <param name="side"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetSendTo(TransOrderServerData order, bool isDetail, string bridgeType, TransOrderTxt o, string side, string key = null, string assistBrokerId = null)
        {
            string send = "";
            if (isDetail)
            {
                if (o.isShowColor)
                    return send;
                if (!"2".Equals(order.BridgeFlag) && string.IsNullOrWhiteSpace(order.BridgeType))
                {
                    string strChannel = "";
                    foreach (ComboxBaseModel item in EnumHelper.Instance.ChannelComboxModel)
                    {
                        if (item.ValueMemberPath == order.IssueChannel)
                        {
                            switch (order.IssueChannel)
                            {
                                case "1":
                                    if (order.SecurityID.EndsWith(".ib") || order.SecurityID.EndsWith(".IB"))
                                    {
                                        strChannel = "";

                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                                case "2":
                                    if (order.SecurityID.EndsWith(".ib") || order.SecurityID.EndsWith(".IB"))
                                    {
                                        strChannel = "【" + item.DisplayMemberPath + "】";
                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                                case "3":
                                case "4":
                                case "5":
                                case "6":
                                case "7":
                                case "8":
                                case "11":
                                    strChannel = item.DisplayMemberPath;
                                    break;
                                case "9":
                                case "10":
                                    //意向(9)和匿名(10)两种渠道，拼接规则参看 SetIssueShowName => issueDesc 位于渠道之前
                                    //strChannel还要和下面的issueShowName再次拼接，因此这个地方strChannel置为空字符串
                                    //当债券是SZ，SH类型的时候，要赋值为渠道名称
                                    if (order.SecurityID.EndsWith(".ib") || order.SecurityID.EndsWith(".IB"))
                                    {
                                        strChannel = "";
                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                            }
                            break;
                        }
                    }

                    string issueShowName = SetIssueShowName(order);

                    //发给标准化
                    if (!string.IsNullOrWhiteSpace(order.IssueState) && order.IssueState == "1")
                    {
                        string staSend = "";
                        if (!string.IsNullOrWhiteSpace(order.IssueShowType) && order.IssueShowType == "1")
                        {
                            if (!string.IsNullOrWhiteSpace(order.ProxyFee))
                            {
                                staSend = strChannel + issueShowName + "," + order.ProxyFee + "厘";
                            }
                            else
                            {
                                staSend = strChannel + issueShowName;
                            }

                            if (!string.IsNullOrWhiteSpace(order.IssueRemark))
                            {
                                staSend = staSend + "," + order.IssueRemark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(order.ProxyFee))
                            {
                                staSend = strChannel + "," + order.ProxyFee + "厘";
                            }
                            else
                            {
                                staSend = strChannel;
                            }

                            if (!string.IsNullOrWhiteSpace(order.IssueRemark))
                            {
                                staSend = staSend + "," + order.IssueRemark;
                            }
                        }
                        //过桥并有加桥
                        if (SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId)
                            && order.BridgeFlag == "2"
                            && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == LocalEnv.UserInfo.UserId))
                        {
                            if (!string.IsNullOrWhiteSpace(order.SendTo2) && !string.IsNullOrWhiteSpace(order.SendTo))
                                send = ",ofr" + staSend + ";bid" + staSend;
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(order.SendTo2))
                                    send = ",ofr" + staSend;
                                if (!string.IsNullOrWhiteSpace(order.SendTo))
                                    send = ",bid" + staSend;
                            }
                        }
                        else
                        {
                            var comparedName = string.IsNullOrEmpty(assistBrokerId) ? LocalEnv.UserInfo.UserId : assistBrokerId;

                            //发给根据[机构 + trade]去判断（原来根据机构来判断）
                            bool isSelf = true;
                            if (!comparedName.Equals(order.PartySubID) && !comparedName.Equals(order.CounterPartySubID))
                            {
                                isSelf = false;
                            }

                            switch (side)
                            {
                                case "Ofr":
                                    if (isSelf)
                                        send = "," + staSend;
                                    else
                                        send = ",ofr" + staSend;
                                    break;
                                case "Bid":
                                    if (isSelf)
                                        send = "," + staSend;
                                    else
                                        send = ",bid" + staSend;
                                    break;
                                default:
                                    if (isSelf)
                                        send = "," + staSend + ";" + staSend;
                                    else
                                        send = ",ofr" + staSend + ";bid" + staSend;
                                    break;
                            }

                        }
                    }
                    else
                    {
                        string sendtoStr = GetDsendtoTxt(order, true);
                        if (!string.IsNullOrWhiteSpace(sendtoStr))
                            send = "," + sendtoStr;
                        if (!string.IsNullOrWhiteSpace(order.ProxyFee))
                        {
                            send = "," + order.ProxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.IssueRemark))
                            send += "," + order.IssueRemark;
                    }

                }
                else
                {

                    //过桥并有加桥
                    if (SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId) && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == LocalEnv.UserInfo.UserId))
                    {
                        if (!string.IsNullOrWhiteSpace(order.SendTo2) && !string.IsNullOrWhiteSpace(order.SendTo))
                            send = ",ofr" + order.SendTo2 + ";bid" + order.SendTo;
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(order.SendTo2))
                                send = ",ofr" + order.SendTo2;
                            if (!string.IsNullOrWhiteSpace(order.SendTo))
                                send = ",bid" + order.SendTo;
                        }
                    }
                    else
                    {
                        var comparedName = string.IsNullOrEmpty(assistBrokerId) ? LocalEnv.UserInfo.UserId : assistBrokerId;

                        //发给根据[机构 + trade]去判断（原来根据机构来判断）
                        bool isSelf = true;
                        if (!comparedName.Equals(order.PartySubID) && !comparedName.Equals(order.CounterPartySubID))
                        {
                            isSelf = false;
                        }
                        string sendtoStr = GetDsendtoTxt(order, false);
                        string sendtoStr2 = GetDsendtoTxt(order, true);
                        switch (side)
                        {
                            case "Ofr":
                                if (isSelf)
                                    send = "," + sendtoStr2;
                                else
                                    send = ",ofr" + sendtoStr2;
                                break;
                            case "Bid":
                                if (isSelf)
                                    send = "," + sendtoStr;
                                else
                                    send = ",bid" + sendtoStr;
                                break;
                            default:
                                if (isSelf)
                                    send = "," + sendtoStr2 + ";" + sendtoStr;
                                else
                                    send = ",ofr" + sendtoStr2 + ";bid" + sendtoStr;
                                break;
                        }

                    }
                }
            }
            return send;
        }


        /// <summary>
        ///发给展示
        /// </summary>
        /// <param name="operationOrderDetail"></param>
        private string SetIssueShowName(TransOrderServerData operationOrderDetail)
        {
            string issueInstName = operationOrderDetail.IssueInstNameShort;
            if (string.IsNullOrWhiteSpace(issueInstName))
            {
                issueInstName = operationOrderDetail.IssueInstName;
            }


            string idesc = "";
            if (!string.IsNullOrWhiteSpace(operationOrderDetail.SecurityID))
            {
                if (operationOrderDetail.SecurityID.EndsWith(".ib") || operationOrderDetail.SecurityID.EndsWith(".IB"))
                {
                    if (string.IsNullOrWhiteSpace(operationOrderDetail.IssueChannel))
                    {
                        return idesc;
                    }
                    ChannelEnum channelEnum = (ChannelEnum)System.Enum.Parse(typeof(ChannelEnum), operationOrderDetail.IssueChannel, true);
                    switch (channelEnum)
                    {
                        case ChannelEnum.Request:
                            idesc = operationOrderDetail.IssueDesc + issueInstName;
                            break;
                        case ChannelEnum.FixedIncome:
                        case ChannelEnum.Bulk:
                        case ChannelEnum.Bidding:
                        case ChannelEnum.Dialogue:
                        case ChannelEnum.Bilateral:
                        case ChannelEnum.xbond:
                        case ChannelEnum.iDeal:
                        case ChannelEnum.Xbargain:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueTraderName))
                            {
                                idesc = operationOrderDetail.IssueDesc + issueInstName + " " + operationOrderDetail.IssueTraderName;
                            }
                            else
                            {
                                idesc = operationOrderDetail.IssueDesc + issueInstName;
                            }
                            break;
                        case ChannelEnum.Intention:
                        case ChannelEnum.Anonymity:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueTraderName))
                            {
                                idesc = operationOrderDetail.IssueDesc + channelEnum.GetDescriptionN() + "," + issueInstName + " " + operationOrderDetail.IssueTraderName;
                                //idesc = channelEnum.GetDescription() + "," + issueInstName + " " + operationOrderDetail.IssueTraderName;
                            }
                            else
                            {
                                //idesc = channelEnum.GetDescription() + "," + issueInstName;
                                idesc = operationOrderDetail.IssueDesc + channelEnum.GetDescriptionN() + "," + issueInstName;
                            }
                            break;
                    }
                }
                else
                {
                    return "";
                }

            }
            else
            {
                idesc = operationOrderDetail.IssueDesc + " " + issueInstName;
                idesc = issueInstName;
            }

            return idesc;
        }

        /// <summary>
        /// 是否有桥,非过桥专员
        /// </summary>
        /// <returns></returns>
        private bool IsHaseBride(TransOrderServerData order, TransOrderTxt o)
        {
            bool haseBridge = false;
            if ("2".Equals(order.BridgeFlag)
                               && !string.IsNullOrWhiteSpace(order.BridgeType)
                               && !o.isShowColor)
            //&& !SystemParamHandler.IsBridgeRole())
            {
                haseBridge = true;
            }
            return haseBridge;
        }

        public bool IsHaseBride(string BridgeFlag, string BridgeType, bool isShowColor)
        {
            bool haseBridge = false;
            if ("2".Equals(BridgeFlag)
                               && !string.IsNullOrWhiteSpace(BridgeType)
                               && !isShowColor)
            //&& !SystemParamHandler.IsBridgeRole())
            {
                haseBridge = true;
            }
            return haseBridge;
        }

        /// <summary>
        /// 订单是否是当前登录的broker 的
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public bool IsLoginBrokerOrder(TransOrderServerData orderDetailPo)
        {
            return LocalEnv.UserInfo.IsMyself(orderDetailPo.PartySubID) || LocalEnv.UserInfo.IsMyself(orderDetailPo.CounterPartySubID);
        }


        public string GetBeAssistedId(TransOrderServerData order,bool isAssist)
        {
            string result = "";
            if (isAssist)
            {
                if (SystemPowerHandler.Instance.AssistantPermissions != null && SystemPowerHandler.Instance.AssistantPermissions.Any(_ => _.userId == order.PartySubID))
                {
                    result = order.PartySubID;
                }
                if (SystemPowerHandler.Instance.AssistantPermissions != null && SystemPowerHandler.Instance.AssistantPermissions.Any(_ => _.userId == order.CounterPartySubID))
                {
                    result = order.CounterPartySubID;
                }
            }
            else {
                if (SystemPowerHandler.Instance.AssistantPermissions != null && SystemPowerHandler.Instance.AssistantPermissions.Any(_ => _.userId == order.PartySubID))
                {
                    return order.PartySubID;
                }
                if (SystemPowerHandler.Instance.AssistantPermissions != null && SystemPowerHandler.Instance.AssistantPermissions.Any(_ => _.userId == order.CounterPartySubID))
                {
                    return order.CounterPartySubID;
                }

                if (SystemPowerHandler.Instance.LookPermissions != null && SystemPowerHandler.Instance.LookPermissions.Any(_ => _.userId == order.PartySubID))
                {
                    result = order.PartySubID;
                }
                if (SystemPowerHandler.Instance.LookPermissions != null && SystemPowerHandler.Instance.LookPermissions.Any(_ => _.userId == order.CounterPartySubID))
                {
                    result = order.CounterPartySubID;
                }
            }
            
            return result;
        }


        public void ConvertBaseDataToTransData(List<TransOrderServerData> newDatas, List<ExecutionReport> oldDatas)
        {
            if (oldDatas != null && oldDatas.Count > 0)
            {
                foreach (var item in oldDatas)
                {
                    TransOrderServerData trans = new TransOrderServerData();
                    trans.CopyParentData(item);
                    newDatas.Add(trans);
                }
            }
            //newDatas = oldDatas=new;
        }

        public void GetNextDate()
        {
            string today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");

            if (!todayDic.ContainsKey(today))
            {
                todayDic.Clear();
                CalWeekDateReq requst = new CalWeekDateReq();
                requst.settlementType = "2";
                todayDic[today] = CalSettleDate(requst);
                LogHelper.Info(string.Format("......................NextDate:{0}", todayDic[today]));
            }
            NextDate = ConvertTextHelper.NextDate = todayDic[today];

        }

        public string CalSettleDate(CalWeekDateReq request)
        {
            string settlementDate = "";
            //ThreadPool.QueueUserWorkItem((o) =>
            //{

            CalWeekDateResult result = CalHandler.Instance.calSettlementDate(request);
            if (result != null && result.Data != null)
            {
                settlementDate = result.Data.settlementDate;
            }
            return settlementDate;
        }
        #endregion

    }
}