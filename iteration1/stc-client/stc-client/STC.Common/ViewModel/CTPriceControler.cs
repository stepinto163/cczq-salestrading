using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using STC.Common.Model.Base;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Common.ViewModel
{
    /// <summary>
    /// 深度行情处理器
    /// </summary>
    public class CTPriceControler
    {
        public const string CDH = "CDH";
        public const string XSWAP = "XSWAP";
        public const string SOR = "SOR";

        #region 表格字段
        public const string KEY_FIELD = "Key";
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string TOTAL_QTY_FIELD = "TotalQty";
        public const string TIME_FIELD = "Time";
        public const string SOURCE_FIELD = "Source";
        public const string QTY_FIELD = "Qty";
        public const string ENTRY_PX_FIELD = "EntryPx";   //用户输入的原始价(特殊：由于修改了MD，当收益率为空只有净价时EntryPx显示为收益率)
        public const string NET_PRICE_FIELD = "NetPrice"; //净价
        public const string Yield_Field = "Yield"; //收益率
        public const string YieldNum_Field = "YieldNum"; //收益率
        public const string YieldToExecution_Field = "YieldToExecution"; //行权收益率
        public const string PriceType_Field = "PriceType"; //类型  0-意向；1-净价；2-全价；3-收益率；4-利差；8-经纪商未指定 如果没有输入则为 3-收益率
        public const string SETTL_TYPE_FIELD = "SettlType";
        public const string SETTL_TYPE_YUAN_FIELD = "SettlType_yuan";
        public const string SIDE_FIELD = "Side";
        public const string IS_BEST_FIELD = "IsBest";
        public const string FLAG_TYPE_FIELD = "FlagType";//角标，重新计算
        public const string FIRM_TYPE_FIELD = "FirmFlag";//意向型， 重新计算
        public const string DELETE_FLAG_FIELD = "DeleteFlag";
        public const string VENUE_TYPE_GW_FIELD = "VenueTypeGW";
        public const string TEXT_FIELD = "Text";// 角标内容
        public const string MD_PRICE_LEVEL_FIELD = "MDPriceLevel"; // 优先顺序
        public const string PARTY_SUB_NAME_FIELD = "PartySubName"; // 本方交易员名
        public const string BARGAIN_FLAG_FIELD = "BargainFlag"; // 询问标记
        public const string HIDE_PX_FLAG_FIELD = "HidePxFlag"; // 是否隐藏价格，0：不隐藏 1：隐藏(此字段只有主表使用)
        public const string HIDE_NETPRICE_FLAG_FIELD = "HideNetPriceFlag"; // 是否隐藏净价，0：不隐藏 1：隐藏(此字段只有主表使用)
        public const string WHOLE_FLAG_FIELD = "WholeFlag"; // 是否是整量
        public const string INSTITUTION_NAME_FIELD = "InstitutionName"; // 机构名称
        public const string SHOWTRADER_FIELD = "ShowTrader"; // 是否显示机构名称图标
        public const string ISME_FIELD = "IsMe"; // 是否是我的
        public const string ISINCOMPLETE_FIELD = "IsIncomplete";  // 是否是缺损报价

        public const string TRADERNAME_FIELD = "TraderName";//交易员名称

        public const string TRANSACT_TIME_FILED = "TransactTime";//报价时间 20210714 16:56:20.103

        public const string CUSTOMER_SOURCE_FIELD = "CustomerSource";//自定义来源
        public const string PARTY_NAME_FIELD = "PartyName";//机构名称
        public const string PARTY_ID_FIELD = "PartyID";//机构名称

        public const string DELIVERY_TYPE_FILED = "DeliveryType";
        public const string CLEARING_METHOD_FILED = "ClearingMethod";
        public const string QUOTE_TYPE_FILED = "QuoteType";

        public const string IS_PERMISSION_FIELD = "IsPermission";//是否有权限

        public const string IS_MORE_FIELD = "IsMore";//是否显示多结算速度
        public const string QUOTE_STATUS_FIELD = "QuoteStatus";//报价状态

        public const string VENUE_SORT_FIELD = "VenueSort";//个人设置自定义Venue顺序
        public const string ISINNER_FIELD = "IsInner";//是否是内部

        public const string MARKETINDICATOR_FIELD = "MarketIndicator";//是否是内部 5期货 4债券
        public const string CalFlag_Field = "CalFlag";//试算标记 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0-原始 
        public const string CalFlagFieldTip_Field = "CalFlagYieldTip"; //ToolTip
        public const string CalFlagNetPriceTip_Field = "CalFlagNetPriceTip"; //ToolTip
		public const string INFO3_FIELD = "Info3";//双边我的报价Bid深度挡位
        public const string INFO4_FIELD = "Info4";//双边我的报价Ofr深度挡位

        public const string STRATEGY_BUY_FIELD = "StrategyBuy";  //策略买
        public const string MANUAL_BUY_FIELD = "ManualBuy";      //手工买
        public const string BUY_QUANT_FIELD = "BuyQty";          //买量
        public const string STRATEGY_SELL_FIELD = "StrategySell";//策略卖
        public const string MANUAL_SELL_FIELD = "ManualSell"; //手工卖
        public const string SELL_QUANT_FIELD = "SellQty";        //卖量

        public const string MIN_CHANGE_FIELD = "MinChange";        //价格最小变动单位
        public const string IsInnerInComplete_Field = "IsInnerInComplete";  //是否是内部无价无量订单
        public const string ExerciseFlag_FIELD = "ExerciseFlag"; //0=行权收益率,1或空=到期收益率

        //public const string PRICETYPE_FIELD = "PriceType";  // 价格类型 0-意向: 1-净价;2-全价;3-收益率;4-利差;8-经纪商未指定
        public const string DIVIDED_FIELD = "Divided";// 1:量可部分执行
        public const string BOND_TYPE_FIELD = "BondType";

        #endregion
        public static object MDLock = new object();

        public static void UpdateDataToBaseDV(DataView dv, MarketDataSnapshotFullRefresh obj)
        {

            dv.Table.BeginLoadData();
            //设置删除标志
            foreach (DataRow dr in dv.Table.Rows)
            {
                dr[DELETE_FLAG_FIELD] = "1";
            }


            string min_change = CommonUtils.GetMinChangeBySecurityID(obj.SecurityID);

            if (obj.NoMDEntries == null)
            {
                dv.Table.EndLoadData();
                return;
            }

            // 添加和更新数据(普通)
            foreach (GNoMDEntries entry in obj.NoMDEntries)
            {
                //if (obj.SecurityID.StartsWith("240210"))
                //    CommonUtils.WriteConvertObjectToJsonToText(obj.NoMDEntries, "1.txt");
                 //string key = $"{obj.SecurityID}#{entry.MDEntryType}#{entry.SettlType}#{entry.MDEntryPx}#{GenerateSourceByVenues(entry.Venues, entry.PartySubName)}";
                string key = string.IsNullOrEmpty(entry.MDEntryID) ? Guid.NewGuid().ToString("N") + entry.MDEntryType : entry.MDEntryID + entry.MDEntryType;
                string securityID = obj.SecurityID;
                string venues = entry.Venues;
                string venueTypeGW = entry.VenueTypeGW;
                string time = entry.MDEntryTime;
                string firmFlag = entry.FirmFlag;
                string text = entry.Text;
                string price = string.Empty;

                //if (string.IsNullOrEmpty(entry.PriceType))
                //{
                //    price = CommonUtils.formatString(CommonUtils.ConvertToString(entry.MDEntryPx), 4);
                //}
                //else
                //{
                //    if (entry.PriceType.ToString() == "1")
                //    {
                //        price = CommonUtils.formatString(CommonUtils.ConvertToString(entry.Yield), 4);
                //    }
                //    else
                //    {

                //根据最小价格变动单位决定价格显示位数
                switch (min_change)
                {
                    case Consts.PRICE_MIN_CHANGE_1:
                    case Consts.PRICE_MIN_CHANGE_5:
                    case Consts.PRICE_MIN_CHANGE_10:
                        price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 0);
                        break;
                    case Consts.PRICE_MIN_CHANGE_05:
                    case Consts.PRICE_MIN_CHANGE_01:
                        price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 1);
                        break;
                    case Consts.PRICE_MIN_CHANGE_005:
                        price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 2);
                        break;
                    case Consts.PRICE_MIN_CHANGE_0005:
                        price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 3);
                        break;
                    default:
                        price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 4);
                        break;
                }
                //    }
                //}

                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);
                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                string partySubName = GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);
                string side = entry.MDEntryType;
                string settlType = string.Empty;
                string settleType_yuan = string.Empty;

                if (entry.SettlType.Contains(".") || entry.SettlType.Contains('4'))
                {
                    settleType_yuan = entry.SettlType;
                }
                else
                {
                    settlType = entry.SettlType;
                }

                int mdPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                string bargainFlag = entry.BargainFlag;

                string wholeFlag = entry.WholeFlag;
                string institutionName = entry.InstitutionName;
                string showTrader = entry.ShowTrader;
                string traderName = entry.TraderName;

                string transactTime = string.Format("{0} {1}", entry.MDEntryDate, entry.MDEntryTime);
                string deliveryType = entry.DeliveryType;
                string clearingMethod = entry.ClearingMethod;
                string quoteType = entry.QuoteTransType;
                string partyName = entry.PartyName;
                
                string partyID = entry.PartyID ?? null;

                //if (IsContainsProperties(entry, CTPriceControler.PARTY_ID_FIELD))
                //{
                //    partyID = entry.PartyID;
                //}
                
                string quoteStatus = entry.QuoteStatus;

                string isIncomplete = IsIncomplete(entry, obj);
                string isInner = entry.VenueTypeGW == "INNER" ? "1" : "0";

                ////内部无价无量订单放在正常订单
                //if (LocalEnv.ConfInfo.IsShowDRP == "1")
                //{
                //    if (isIncomplete == "1" && isInner == "1")
                //    {
                //        isIncomplete = "0";
                //    }
                //}

                string info3 = entry.Info3;
                string info4 = entry.Info4;

                string marketIndicator = obj.MarketIndicator;

                //新增处理显示字段
                string yield = string.Empty;
                string calTypeFieldTip = string.Empty; // 
                string calTypeNetPriceTip = string.Empty; // 
                string priceType = string.Empty;
                string calFlag = string.Empty;

                string orginalPrice = string.Empty;

                if (marketIndicator != "5")
                {
                    calFlag = entry.CalFlag ?? "0";
                    if (string.IsNullOrEmpty(calFlag))
                        calFlag = "0";

                    priceType = entry.PriceType ?? "3";

                    //设置PriceType
                    if (!string.IsNullOrEmpty(priceType))
                    {
                        if (string.IsNullOrEmpty(CommonUtils.GetPriceType(obj.SecurityID)))
                        {
                            CommonUtils.SetPriceType(obj.SecurityID, priceType);
                        }
                    }

                    //收益率
                    decimal outd = decimal.Zero;
                    bool isnumber = decimal.TryParse(entry.Yield, out outd);
                    if (isnumber)
                        yield = CommonUtils.formatString(entry.Yield, 4);
                    else
                        yield = entry.Yield;
                    priceType = entry.PriceType;
                    orginalPrice = price;

                    // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    // exerciseFlag 0=行权收益率,1或空=到期收益率
                    //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0 null

                    if (calFlag == "2")
                    {
                        calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                    }

                    if (calFlag == "4")
                    {
                        calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {price}";
                        orginalPrice = netPrice;
                    }

                    if (calFlag == "3")
                    {
                        calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                        calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                    }

                    if (priceType == "1")
                    {
                        calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {price}";
                        calFlag = "4";
                        orginalPrice = netPrice;
                    }

                    if (priceType == "3")
                    {
                        calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                    }

                    if (priceType == "2")
                    {
                        calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                        calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                    }
                    if (calFlag == "6")
                    {
                        calTypeFieldTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                        calTypeNetPriceTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                    }
                }
                else if (marketIndicator == "5" && string.IsNullOrEmpty(yield))
                {
                    yield = entry.MDEntryPx;
                    calFlag = "0";
                    priceType = "3";
                }

                string exerciseflag = entry.ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率

                string bondtype = obj.BondType;

                string yieldToExecution = entry.YieldToExecution;
                FeedDataNew(dv, key, securityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price,
                netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader,
                isMe, isIncomplete, traderName, marketIndicator, transactTime, deliveryType, clearingMethod, quoteType, partyName, 
                quoteStatus, isInner, partyID, calFlag, yield, priceType, calTypeFieldTip, calTypeNetPriceTip, info3, info4, 
                exerciseflag,yieldToExecution, orginalPrice, bondtype);
            }

            // 删除DeleteFlag为0的记录
            DataRow[] drs = dv.Table.Select($"{DELETE_FLAG_FIELD} = '1'", $"{DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }

            dv.Table.EndLoadData();
            dv.Table.AcceptChanges();
        }

        public static void UpdateDataToBaseDV_BDB(DataView dv, MarketDataSnapshotFullRefresh obj)
        {
            //设置删除标志
            foreach (DataRow dr in dv.Table.Rows)
            {
                dr[DELETE_FLAG_FIELD] = "1";
            }

            // 添加和更新数据(普通)
            foreach (GNoMDEntries entry in obj.NoMDEntries)
            {
                //if (entry.QuoteStatus != "16")
                //{
                //    continue;
                //}

                //string key = $"{obj.SecurityID}#{entry.MDEntryType}#{entry.SettlType}#{entry.MDEntryPx}#{GenerateSourceByVenues(entry.Venues, entry.PartySubName)}";
                //string key = entry.MDEntryID;
                string key = $"{ obj.SecurityID}#{obj.Venues}#{entry.MDEntryType}#{entry.SettlType}#{entry.MDEntryPx}#{entry.MDPriceLevel}";
                string securityID = obj.SecurityID;
                string venues = entry.Venues;//obj.Venues;
                string venueTypeGW = obj.VenueTypeGW;//entry.VenueTypeGW;
                string time = entry.MDEntryTime;
                string firmFlag = entry.FirmFlag;
                string text = entry.Text;
                string price = CommonUtils.ConvertToString(entry.MDEntryPx);
                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);
                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                string partySubName = GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);
                string side = entry.MDEntryType;
                string settlType = string.Empty;
                string settleType_yuan = string.Empty;

                //if (entry.SettlType == "1" || entry.SettlType == "2" || entry.SettlType == "3" || entry.SettlType == "2,3")
                {
                    settlType = entry.SettlType;
                }
                //else
                {
                    settleType_yuan = CommonUtils.GetFwdSettle(entry.SettlType); //entry.SettlType;
                }
                int mdPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                string bargainFlag = entry.BargainFlag;

                string wholeFlag = entry.WholeFlag;
                string institutionName = entry.InstitutionName;
                string showTrader = entry.ShowTrader;
                string traderName = entry.TraderName;

                string priceType = entry.PriceType;
                string isIncomplete = IsIncomplete(entry);
                string divided = entry.Divided;
                string bondtype = obj.BondType;

                FeedData_BDB(dv, key, securityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price,
                    netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader,
                    isMe, isIncomplete, traderName, priceType, divided, bondtype);
            }

            // 删除DeleteFlag为0的记录
            DataRow[] drs = dv.Table.Select($"{DELETE_FLAG_FIELD} = '1'", $"{DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }
            dv.Table.AcceptChanges();
        }
        public static string IsIncomplete(GNoMDEntries obj)
        {
            if (obj != null && (obj.MDEntrySize == null || obj.MDEntrySize == null
                || CommonUtils.IsNumeric((obj.MDEntryPx + "").Trim()) == false
                 || CommonUtils.ConvertToLong((obj.MDEntrySize + "").Trim()) == 0))
            {
                return "1";
            }
            return "0";
        }

        public static void FeedData_BDB(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
        string text, object price, object netPrice, decimal qty, string time, int mdPirceLevel, string partySubName, string bargainFlag, string wholeFlag,
        string institutionName, string showTrader, bool isMe, string IsIncomplete, string traderName, string priceType, string divided, string bondtype)
        {
            DataRow findDr = dv.Table.Rows.Find(key);
            if (findDr == null)
            {
                findDr = dv.Table.NewRow();
                findDr[KEY_FIELD] = key;
                findDr[SECURITY_ID_FIELD] = securityID;
                findDr[PARTY_SUB_NAME_FIELD] = partySubName;
                findDr[SOURCE_FIELD] = source;
                findDr[VENUE_TYPE_GW_FIELD] = venueTypeGW;
                findDr[SIDE_FIELD] = side;
                findDr[DELETE_FLAG_FIELD] = "1";
                findDr[ISME_FIELD] = isMe;
                dv.Table.Rows.Add(findDr);
            }

            // 清算速度
            findDr[SETTL_TYPE_FIELD] = settlType;
            findDr[SETTL_TYPE_YUAN_FIELD] = settleType_yuan;

            // 可点击成交 和 意向型
            findDr[FIRM_TYPE_FIELD] = firmFlag;

            // 询问符号
            findDr[BARGAIN_FLAG_FIELD] = bargainFlag;

            // 角标内容
            findDr[TEXT_FIELD] = text;
            //// 角标标志
            //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
            //string currentFlag = string.IsNullOrEmpty(text) ? string.Empty : "0";
            //if (orginalFlag != currentFlag)
            //{
            //    findDr[FLAG_TYPE_FIELD] = currentFlag;
            //}

            findDr[MD_PRICE_LEVEL_FIELD] = mdPirceLevel;
            findDr[ENTRY_PX_FIELD] = price;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[QTY_FIELD] = qty;
            findDr[TIME_FIELD] = time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[WHOLE_FLAG_FIELD] = wholeFlag;

            findDr[PriceType_Field] = priceType;

            if (!string.IsNullOrEmpty(source) && source.ToUpper() == "IDB")
            {
                findDr[INSTITUTION_NAME_FIELD] = institutionName;
                findDr[SHOWTRADER_FIELD] = showTrader;
                findDr[TRADERNAME_FIELD] = traderName;
            }
            if (dv.Table.Columns.Contains(ISINCOMPLETE_FIELD))
            {
                findDr[ISINCOMPLETE_FIELD] = IsIncomplete;
            }

            if (dv.Table.Columns.Contains(BOND_TYPE_FIELD))
            {
                findDr[BOND_TYPE_FIELD] = bondtype;
            }

            findDr[DIVIDED_FIELD] = divided;
        }

        public static void FeedDataNew(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
        string text, object price, object netPrice, decimal qty, string time, int mdPirceLevel, string partySubName, string bargainFlag, string wholeFlag,
        string institutionName, string showTrader, bool isMe, string IsIncomplete, string traderName, string marketIndicator,string transactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
        string partyName = null, string quoteStatus = null, string isInner = "0", string partyID = "", string calFlag = "", string yield = "" , string priceType = "", string calTypeFieldTip = "", string calTypeNetPriceTip="", string info3="", string info4="", 
        string exerciseflag = "1", string yieldToExecution = "", string orginalPrice = "", string bondType = "")
        {
            FeedData(dv, key, securityID, source, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price, netPrice, qty, time, mdPirceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader, isMe, IsIncomplete, traderName, transactTime, deliveryType, clearingMethod, quoteType
                , partyName, quoteStatus, isInner, partyID, calFlag, yield, priceType, calTypeFieldTip, calTypeNetPriceTip, info3, info4, exerciseflag, yieldToExecution, orginalPrice, marketIndicator, bondType);
        }

        public static void FeedData(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
        string text, object price, object netPrice, decimal qty, string time, int mdPirceLevel, string partySubName, string bargainFlag, string wholeFlag,
        string institutionName, string showTrader, bool isMe, string IsIncomplete, string traderName, string transactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
        string partyName = null, string quoteStatus = null, string isInner = "0" ,string partyID = "", string calFlag = "", string yield = "", string priceType = "", string calTypeFieldTip = "", string calTypeNetPriceTip = "", string info3 = "", string info4 = "", 
        string exerciseflag = "1",string yieldToExecution = "", string orginalPrice = "", string marketIndicator = "4", string bondType = "")
        {
            DataRow findDr = dv.Table.Rows.Find(key);
            if (findDr == null)
            {
                findDr = dv.Table.NewRow();
                findDr[KEY_FIELD] = key;
                findDr[SECURITY_ID_FIELD] = securityID;
                findDr[PARTY_SUB_NAME_FIELD] = partySubName;
               
                findDr[VENUE_TYPE_GW_FIELD] = venueTypeGW;
                findDr[SIDE_FIELD] = side;
                findDr[DELETE_FLAG_FIELD] = "1";
                findDr[ISME_FIELD] = isMe;
                findDr[ISINNER_FIELD] = isInner;
                dv.Table.Rows.Add(findDr);
            }
            findDr[SOURCE_FIELD] = source;

            findDr[PARTY_NAME_FIELD] = partyName.ConvertToString();

            findDr[MARKETINDICATOR_FIELD] = marketIndicator;

            if (dv.Table.Columns.Contains(PARTY_ID_FIELD))
                findDr[PARTY_ID_FIELD] = partyID.ConvertToString();

            if (!string.IsNullOrEmpty(partyName) && (venueTypeGW == "QDMESP" || source == "SSF" || source == "SZF"))
            {
                findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), partyName);
            }
            else
            {
                if (string.IsNullOrEmpty(partyName) && (source == "SZF"))
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), "匿名");
                }
                else
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = ComboSourceUtil.Instance.FindEnum(partySubName, "venues");
                }
            }

            if (!string.IsNullOrEmpty(transactTime))
                findDr[TRANSACT_TIME_FILED] = transactTime;

            if (!string.IsNullOrEmpty(deliveryType))
                findDr[DELIVERY_TYPE_FILED] = deliveryType;

            if (!string.IsNullOrEmpty(clearingMethod))
                findDr[CLEARING_METHOD_FILED] = clearingMethod;

            if (!string.IsNullOrEmpty(quoteType))
                findDr[QUOTE_TYPE_FILED] = quoteType;

            if (dv.Table.Columns.Contains(QUOTE_STATUS_FIELD))
            {
                if (!string.IsNullOrEmpty(quoteStatus))
                    findDr[QUOTE_STATUS_FIELD] = quoteStatus;
            }
               

            // 清算速度
            findDr[SETTL_TYPE_FIELD] = settlType;
            findDr[SETTL_TYPE_YUAN_FIELD] = settleType_yuan;

            // 可点击成交 和 意向型
            findDr[FIRM_TYPE_FIELD] = firmFlag;

            // 询问符号
            findDr[BARGAIN_FLAG_FIELD] = bargainFlag;

            // 角标内容
            findDr[TEXT_FIELD] = text;
            //// 角标标志
            //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
            //string currentFlag = string.IsNullOrEmpty(text) ? string.Empty : "0";
            //if (orginalFlag != currentFlag)
            //{
            //    findDr[FLAG_TYPE_FIELD] = currentFlag;
            //}

            findDr[MD_PRICE_LEVEL_FIELD] = mdPirceLevel;
            findDr[ENTRY_PX_FIELD] = orginalPrice;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[QTY_FIELD] = qty;
            findDr[TIME_FIELD] = CommonUtils.ConvertTime(time);//time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[WHOLE_FLAG_FIELD] = wholeFlag;

            findDr[IS_PERMISSION_FIELD] = ComPermissionHandler.Instance.CheckVenuePermission(source, securityID);

            findDr[VENUE_SORT_FIELD] = SystemParamHandler.Instance.GetVenueSort(source);
            
            if (!string.IsNullOrEmpty(quoteStatus) && quoteStatus == "9")
                findDr[IS_PERMISSION_FIELD] = false;

            if (source.ToUpper() == "IDB")
            {
                findDr[INSTITUTION_NAME_FIELD] = institutionName;
                findDr[SHOWTRADER_FIELD] = showTrader;
                findDr[TRADERNAME_FIELD] = traderName;
            }
            if (dv.Table.Columns.Contains(ISINCOMPLETE_FIELD))
            {
                findDr[ISINCOMPLETE_FIELD] = IsIncomplete;
            }

            findDr[ISINNER_FIELD] = isInner;

            if (dv.Table.Columns.Contains(INFO3_FIELD))
            {
                findDr[INFO3_FIELD] = info3;
            }
			
            if (dv.Table.Columns.Contains(INFO4_FIELD))
            {
                findDr[INFO4_FIELD] = info4;
            }

            if (dv.Table.Columns.Contains(CalFlag_Field))
                findDr[CalFlag_Field] = calFlag;

            if (dv.Table.Columns.Contains(Yield_Field))
            {
                findDr[Yield_Field] = yield;
            }
            if (dv.Table.Columns.Contains(YieldNum_Field))
            {
                if (CommonUtils.IsNumeric(yield))
                {
                    findDr[YieldNum_Field] = CommonUtils.ConvertToDecimal(yield);
                }
                else
                {
                    if (side == "1")
                    {
                        findDr[YieldNum_Field] = decimal.MinValue;
                    }
                    else
                    {
                        findDr[YieldNum_Field] = decimal.MaxValue;
                    }
                }
            }

            if (dv.Table.Columns.Contains(YieldToExecution_Field))
                findDr[YieldToExecution_Field] = yieldToExecution;

            if (dv.Table.Columns.Contains(PriceType_Field))
                findDr[PriceType_Field] = priceType;
            
            if (dv.Table.Columns.Contains(CalFlagFieldTip_Field))
                findDr[CalFlagFieldTip_Field] = calTypeFieldTip;

            if (dv.Table.Columns.Contains(CalFlagNetPriceTip_Field))
                findDr[CalFlagNetPriceTip_Field] = calTypeNetPriceTip;

            if (dv.Table.Columns.Contains(ExerciseFlag_FIELD))
                findDr[ExerciseFlag_FIELD] = exerciseflag;

            if (dv.Table.Columns.Contains(PriceType_Field))
                findDr[PriceType_Field] = priceType;

            if (dv.Table.Columns.Contains(IsInnerInComplete_Field))
            {
                //CDH无价无量也不显示在行情里
                if (findDr[VENUE_TYPE_GW_FIELD].ToString().ToUpper() == "CDH"
                    || findDr[VENUE_TYPE_GW_FIELD].ToString().ToUpper() == "SOR")
                {
                    if (qty == 0 || yield == "--" || string.IsNullOrEmpty(yield))
                    {
                        findDr[IsInnerInComplete_Field] = "1";
                        return;
                    }
                }

                if (findDr[VENUE_TYPE_GW_FIELD].ToString().ToUpper() == "QDM2")
                {
                    //指示性报价
                    findDr[ISINNER_FIELD] = "1";
                    findDr[IsInnerInComplete_Field] = "1";
                }
                else
                {
                    if ((qty == 0) && (yield ?? "") == "--")
                    {
                        findDr[IsInnerInComplete_Field] = "3";
                    }
                    else
                    {
                        if (qty == 0 && (yield ?? "") != "--")
                        {
                            findDr[IsInnerInComplete_Field] = "0";
                        }
                        else if ((yield??"").ToString() == "--")
                        {
                            findDr[IsInnerInComplete_Field] = "2";
                        }
                        else
                        {
                            findDr[IsInnerInComplete_Field] = "0";
                        }
                    }
                }
            }

            if (dv.Table.Columns.Contains(MIN_CHANGE_FIELD))
                findDr[MIN_CHANGE_FIELD] = CommonUtils.GetMinChangeBySecurityID(securityID);

            if (dv.Table.Columns.Contains(BOND_TYPE_FIELD))
            {
                findDr[BOND_TYPE_FIELD] = bondType;
            }

        }

        public static void FeedData_CDH_FilterIncomplete(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
           string text, string price, decimal netPrice, decimal qty, string time, int mdPriceLevel, string barginFlag, string wholeFlag, string institutionName, string showTrader, bool isMe, string orginalPrice,
           string partyName = null, string partySubName = null, string isInner = "0", string partyID = "", string calFlag = "", string yield = "", string priceType = "", string calTypeFieldTip = "", string calTypeNetPriceTip = "",
           string exerciseflag = "1", string yieldToExecution = "", string bondType = "")
        {
            //CDH无价无量也不显示在行情里
            if (venueTypeGW == "CDH" || venueTypeGW == "SOR")
            {
                bool ishaveyield = !(string.IsNullOrEmpty(yield) || "--".Equals(yield));
                bool noprice = !(CommonUtils.ConvertToDecimal(price) > 0 || ishaveyield || netPrice > 0);

                if (qty == 0 || noprice)
                {
                    return;
                }
            }

            FeedData_CDH(dv, key, securityID, source, venueTypeGW, side, settlType, settleType_yuan, firmFlag,
               text, price, netPrice, qty, time, mdPriceLevel, barginFlag, wholeFlag, institutionName, showTrader, isMe, orginalPrice,
               partyName, partySubName, isInner, partyID, calFlag, yield, priceType, calTypeFieldTip, calTypeNetPriceTip,
               exerciseflag, yieldToExecution, bondType);
        }

        public static void FeedData_CDH(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
           string text, string price, decimal netPrice, decimal qty, string time, int mdPriceLevel, string barginFlag, string wholeFlag, string institutionName, string showTrader, bool isMe, string orginalPrice,
           string partyName = null, string partySubName = null, string isInner = "0", string partyID = "", string calFlag = "", string yield = "", string priceType = "", string calTypeFieldTip = "", string calTypeNetPriceTip = "", 
           string exerciseflag = "1",string yieldToExecution="", string bondType = "")
        {
            DataRow findDr = dv.Table.Rows.Find(key);

            if (findDr == null)
            {
                findDr = dv.Table.NewRow();
                findDr[KEY_FIELD] = key;
                findDr[SECURITY_ID_FIELD] = securityID;
                findDr[SOURCE_FIELD] = source;
                findDr[PARTY_SUB_NAME_FIELD] = source;
                findDr[VENUE_TYPE_GW_FIELD] = venueTypeGW;
                findDr[SIDE_FIELD] = side;
                findDr[DELETE_FLAG_FIELD] = "1";
                findDr[ISME_FIELD] = false;
                dv.Table.Rows.Add(findDr);
            }

            string deleteFlag = findDr[DELETE_FLAG_FIELD] == null ? "1" : findDr[DELETE_FLAG_FIELD].ToString();

            if (deleteFlag == "1")
            {
                #region 替换数据
                // 清算速度
                findDr[SETTL_TYPE_FIELD] = settlType;
                findDr[SETTL_TYPE_YUAN_FIELD] = settleType_yuan;

                // 可点击成交 和 意向型
                findDr[FIRM_TYPE_FIELD] = firmFlag;

                // 询问符号
                findDr[BARGAIN_FLAG_FIELD] = barginFlag;

                // 角标内容
                findDr[TEXT_FIELD] = text;
                //// 角标标志
                //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
                //string currentFlag = string.IsNullOrEmpty(findDr[TEXT_FIELD].ToString()) ? string.Empty : "0";
                //if (orginalFlag != currentFlag)
                //{
                //    findDr[FLAG_TYPE_FIELD] = currentFlag;
                //}
                // 数量
                findDr[QTY_FIELD] = qty;

                findDr[WHOLE_FLAG_FIELD] = wholeFlag;

                findDr[INSTITUTION_NAME_FIELD] = string.Empty;
                findDr[SHOWTRADER_FIELD] = "0";

                if (isMe)
                {
                    findDr[ISME_FIELD] = true;
                }
                #endregion
            }
            else
            {
                #region 累计数据
                // 清算速度
                string localSettlType = findDr[SETTL_TYPE_FIELD].ToString();
                string localSettlType_YUAN = findDr[SETTL_TYPE_YUAN_FIELD].ToString();
                if (string.IsNullOrEmpty(settlType) == false)
                {
                    if (string.IsNullOrEmpty(localSettlType) == true)
                    {
                        localSettlType = settlType;
                    }
                    else
                    {
                        if (localSettlType.Contains(settlType) == false)
                        {
                            localSettlType = $"{localSettlType},{settlType}";
                        }
                    }
                }
                if (string.IsNullOrEmpty(settleType_yuan) == false)
                {
                    if (string.IsNullOrEmpty(localSettlType_YUAN) == true)
                    {
                        localSettlType_YUAN = settleType_yuan;
                    }
                    else
                    {
                        if (localSettlType_YUAN.Contains(settleType_yuan) == false)
                        {
                            localSettlType_YUAN = $"{localSettlType_YUAN},{settleType_yuan}";
                        }
                    }
                }

                findDr[SETTL_TYPE_FIELD] = localSettlType;
                findDr[SETTL_TYPE_YUAN_FIELD] = localSettlType_YUAN;

                // 可点击成交 和 意向型
                string localFirmFlag = findDr[FIRM_TYPE_FIELD].ToString();
                if (localFirmFlag.Contains(firmFlag) == false && string.IsNullOrEmpty(firmFlag) == false)
                {
                    if (string.IsNullOrEmpty(localFirmFlag) == true)
                    {
                        localFirmFlag = firmFlag;
                    }
                    else
                    {
                        localFirmFlag = $"{localFirmFlag},{firmFlag}";
                    }
                }
                findDr[FIRM_TYPE_FIELD] = localFirmFlag;

                // 询问符号
                string localBargainFlag = CommonUtils.ConvertToString(findDr[BARGAIN_FLAG_FIELD]);
                if (barginFlag != null && barginFlag.CompareTo(localBargainFlag) > 0)
                {
                    findDr[BARGAIN_FLAG_FIELD] = barginFlag;
                }

                // 角标内容
                string localText = findDr[TEXT_FIELD].ToString();
                if (string.IsNullOrEmpty(text) == false)
                {
                    if (string.IsNullOrEmpty(localText) == true)
                    {
                        localText = text;
                    }
                    else
                    {
                        localText = $"{localText}+{text}";
                    }
                }
                findDr[TEXT_FIELD] = localText;
                //// 角标标志
                //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
                //string currentFlag = string.IsNullOrEmpty(findDr[TEXT_FIELD].ToString()) ? string.Empty : "0";
                //if (orginalFlag != currentFlag)
                //{
                //    findDr[FLAG_TYPE_FIELD] = currentFlag;
                //}
                // 数量
                decimal localQty = CommonUtils.ConvertToDecimal(findDr[QTY_FIELD].ToString());
                findDr[QTY_FIELD] = localQty + qty;

                // 整量标记
                if (wholeFlag == "1")
                {
                    findDr[WHOLE_FLAG_FIELD] = wholeFlag;
                }

                findDr[INSTITUTION_NAME_FIELD] = string.Empty;
                findDr[SHOWTRADER_FIELD] = "0";
                if (isMe)
                {
                    findDr[ISME_FIELD] = true;
                }
                #endregion
            }

            findDr[MD_PRICE_LEVEL_FIELD] = mdPriceLevel;
            findDr[ENTRY_PX_FIELD] = orginalPrice;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[TIME_FIELD] = CommonUtils.ConvertTime(time);//time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[IS_PERMISSION_FIELD] = ComPermissionHandler.Instance.CheckVenuePermission(source, securityID);

            findDr[VENUE_SORT_FIELD] = SystemParamHandler.Instance.GetVenueSort(source);

            if (dv.Table.Columns.Contains("PartyId"))
            {
                findDr[PARTY_ID_FIELD] = partyID;
            }

            findDr[PARTY_NAME_FIELD] = partyName;
            if (!string.IsNullOrEmpty(partyName) && (venueTypeGW == "QDMESP" || source == "SSF" || source == "SZF"))
            {
                findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), partyName);
            }
            else
            {
                if (string.IsNullOrEmpty(partyName) && (source == "SZF"))
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), "匿名");
                }
                else
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = partySubName;
                } 
            }

            if (dv.Table.Columns.Contains(ISINNER_FIELD))
                findDr[ISINNER_FIELD] = isInner;

            if (dv.Table.Columns.Contains(CalFlag_Field))
                findDr[CalFlag_Field] = calFlag;

            if (dv.Table.Columns.Contains(Yield_Field))
            {
                findDr[Yield_Field] = yield;
            }
            if (dv.Table.Columns.Contains(YieldNum_Field))
            {
                if (CommonUtils.IsNumeric(yield))
                {
                    findDr[YieldNum_Field] = CommonUtils.ConvertToDecimal(yield);
                }
                else
                {
                    if (side == "1")
                    {
                        findDr[YieldNum_Field] = decimal.MinValue;
                    }
                    else
                    {
                        findDr[YieldNum_Field] = decimal.MaxValue;
                    }
                }
            }

            if (dv.Table.Columns.Contains(YieldToExecution_Field))
                findDr[YieldToExecution_Field] = yieldToExecution;

            if (dv.Table.Columns.Contains(PriceType_Field))
                findDr[PriceType_Field] = priceType;

            if (dv.Table.Columns.Contains(CalFlagFieldTip_Field))
                findDr[CalFlagFieldTip_Field] = calTypeFieldTip;

            if (dv.Table.Columns.Contains(CalFlagNetPriceTip_Field))
                findDr[CalFlagNetPriceTip_Field] = calTypeNetPriceTip;

            if (dv.Table.Columns.Contains(ExerciseFlag_FIELD))
                findDr[ExerciseFlag_FIELD] = exerciseflag;

            if (findDr[VENUE_TYPE_GW_FIELD].ToString().ToUpper() == "QDM2")
            {
                //指示性报价
                findDr[ISINNER_FIELD] = "1";
                findDr[IsInnerInComplete_Field] = "1";
            }
            else
            {
                if ((qty == 0) && yield.ToString() == "--")
                {
                    findDr[IsInnerInComplete_Field] = "3";
                }
                else
                {
                    if (qty == 0 && yield.ToString() != "--")
                    {
                        findDr[IsInnerInComplete_Field] = "0";
                    }
                    else if (yield.ToString() == "--")
                    {
                        findDr[IsInnerInComplete_Field] = "2";
                    }
                    else
                    {
                        findDr[IsInnerInComplete_Field] = "0";
                    }
                }
            }

            if (dv.Table.Columns.Contains(BOND_TYPE_FIELD))
                findDr[BOND_TYPE_FIELD] = bondType;
            //Console.WriteLine("------>>>>>>>>>>>>************-----------:" + findDr[CUSTOMER_SOURCE_FIELD] + ", partySubName:" + findDr[PARTY_SUB_NAME_FIELD]);
        }

        /// <summary>
        /// CDH无价无量也要汇总
        /// </summary>
        public static void FeedData_CDH_Incomplete(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
           string text, string price, string netPrice, decimal qty, string time, int mdPriceLevel, string barginFlag, string wholeFlag, string institutionName, string showTrader, bool isMe, string orginalPrice,
           string partyName = null, string partySubName = null, string isInner = "0", string partyID = "", string calFlag = "", string yield = "", string priceType = "", string calTypeFieldTip = "", string calTypeNetPriceTip = "",
           string exerciseflag = "1", string yieldToExecution = "", string bondType = "")
        {
            DataRow findDr = dv.Table.Rows.Find(key);
            if (findDr == null)
            {
                findDr = dv.Table.NewRow();
                findDr[KEY_FIELD] = key;
                findDr[SECURITY_ID_FIELD] = securityID;
                findDr[SOURCE_FIELD] = source;
                findDr[PARTY_SUB_NAME_FIELD] = source;
                findDr[VENUE_TYPE_GW_FIELD] = venueTypeGW;
                findDr[SIDE_FIELD] = side;
                findDr[DELETE_FLAG_FIELD] = "1";
                findDr[ISME_FIELD] = false;
                dv.Table.Rows.Add(findDr);
            }

            string deleteFlag = findDr[DELETE_FLAG_FIELD] == null ? "1" : findDr[DELETE_FLAG_FIELD].ToString();

            if (deleteFlag == "1")
            {
                #region 替换数据
                // 清算速度
                findDr[SETTL_TYPE_FIELD] = settlType;
                findDr[SETTL_TYPE_YUAN_FIELD] = settleType_yuan;

                // 可点击成交 和 意向型
                findDr[FIRM_TYPE_FIELD] = firmFlag;

                // 询问符号
                findDr[BARGAIN_FLAG_FIELD] = barginFlag;

                // 角标内容
                findDr[TEXT_FIELD] = text;
                //// 角标标志
                //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
                //string currentFlag = string.IsNullOrEmpty(findDr[TEXT_FIELD].ToString()) ? string.Empty : "0";
                //if (orginalFlag != currentFlag)
                //{
                //    findDr[FLAG_TYPE_FIELD] = currentFlag;
                //}
                // 数量
                findDr[QTY_FIELD] = qty;

                findDr[WHOLE_FLAG_FIELD] = wholeFlag;

                findDr[INSTITUTION_NAME_FIELD] = string.Empty;
                findDr[SHOWTRADER_FIELD] = "0";

                if (isMe)
                {
                    findDr[ISME_FIELD] = true;
                }
                #endregion
            }
            else
            {
                #region 累计数据
                // 清算速度
                string localSettlType = findDr[SETTL_TYPE_FIELD].ToString();
                string localSettlType_YUAN = findDr[SETTL_TYPE_YUAN_FIELD].ToString();
                if (string.IsNullOrEmpty(settlType) == false)
                {
                    if (string.IsNullOrEmpty(localSettlType) == true)
                    {
                        localSettlType = settlType;
                    }
                    else
                    {
                        if (localSettlType.Contains(settlType) == false)
                        {
                            localSettlType = $"{localSettlType},{settlType}";
                        }
                    }
                }
                if (string.IsNullOrEmpty(settleType_yuan) == false)
                {
                    if (string.IsNullOrEmpty(localSettlType_YUAN) == true)
                    {
                        localSettlType_YUAN = settleType_yuan;
                    }
                    else
                    {
                        if (localSettlType_YUAN.Contains(settleType_yuan) == false)
                        {
                            localSettlType_YUAN = $"{localSettlType_YUAN},{settleType_yuan}";
                        }
                    }
                }

                findDr[SETTL_TYPE_FIELD] = localSettlType;
                findDr[SETTL_TYPE_YUAN_FIELD] = localSettlType_YUAN;

                // 可点击成交 和 意向型
                string localFirmFlag = findDr[FIRM_TYPE_FIELD].ToString();
                if (localFirmFlag.Contains(firmFlag) == false && string.IsNullOrEmpty(firmFlag) == false)
                {
                    if (string.IsNullOrEmpty(localFirmFlag) == true)
                    {
                        localFirmFlag = firmFlag;
                    }
                    else
                    {
                        localFirmFlag = $"{localFirmFlag},{firmFlag}";
                    }
                }
                findDr[FIRM_TYPE_FIELD] = localFirmFlag;

                // 询问符号
                string localBargainFlag = CommonUtils.ConvertToString(findDr[BARGAIN_FLAG_FIELD]);
                if (barginFlag != null && barginFlag.CompareTo(localBargainFlag) > 0)
                {
                    findDr[BARGAIN_FLAG_FIELD] = barginFlag;
                }

                // 角标内容
                string localText = findDr[TEXT_FIELD].ToString();
                if (string.IsNullOrEmpty(text) == false)
                {
                    if (string.IsNullOrEmpty(localText) == true)
                    {
                        localText = text;
                    }
                    else
                    {
                        localText = $"{localText}+{text}";
                    }
                }
                findDr[TEXT_FIELD] = localText;
                //// 角标标志
                //string orginalFlag = findDr[FLAG_TYPE_FIELD] == null ? string.Empty : findDr[FLAG_TYPE_FIELD].ToString();
                //string currentFlag = string.IsNullOrEmpty(findDr[TEXT_FIELD].ToString()) ? string.Empty : "0";
                //if (orginalFlag != currentFlag)
                //{
                //    findDr[FLAG_TYPE_FIELD] = currentFlag;
                //}
                // 数量
                decimal localQty = CommonUtils.ConvertToDecimal(findDr[QTY_FIELD].ToString());
                findDr[QTY_FIELD] = localQty + qty;

                // 整量标记
                if (wholeFlag == "1")
                {
                    findDr[WHOLE_FLAG_FIELD] = wholeFlag;
                }

                findDr[INSTITUTION_NAME_FIELD] = string.Empty;
                findDr[SHOWTRADER_FIELD] = "0";
                if (isMe)
                {
                    findDr[ISME_FIELD] = true;
                }
                #endregion
            }

            findDr[MD_PRICE_LEVEL_FIELD] = mdPriceLevel;
            findDr[ENTRY_PX_FIELD] = orginalPrice;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[TIME_FIELD] = CommonUtils.ConvertTime(time);//time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[IS_PERMISSION_FIELD] = ComPermissionHandler.Instance.CheckVenuePermission(source, securityID);

            findDr[VENUE_SORT_FIELD] = SystemParamHandler.Instance.GetVenueSort(source);

            if (dv.Table.Columns.Contains("PartyId"))
            {
                findDr[PARTY_ID_FIELD] = partyID;
            }

            findDr[PARTY_NAME_FIELD] = partyName;
            if (!string.IsNullOrEmpty(partyName) && (venueTypeGW == "QDMESP" || venueTypeGW == "SSF" || source == "SZF"))
            {
                findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), partyName);
            }
            else
            {
                if (string.IsNullOrEmpty(partyName) && (source == "SZF"))
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), "匿名");
                }
                else
                {
                    findDr[CUSTOMER_SOURCE_FIELD] = partySubName;
                }
            }

            if (dv.Table.Columns.Contains(ISINNER_FIELD))
                findDr[ISINNER_FIELD] = isInner;

            if (dv.Table.Columns.Contains(CalFlag_Field))
                findDr[CalFlag_Field] = calFlag;

            if (dv.Table.Columns.Contains(Yield_Field))
            {
                findDr[Yield_Field] = yield; 
            }
            if (dv.Table.Columns.Contains(YieldNum_Field))
            {
                if (CommonUtils.IsNumeric(yield))
                {
                    findDr[YieldNum_Field] = CommonUtils.ConvertToDecimal(yield);
                }
                else
                {
                    if (side == "1")
                    {
                        findDr[YieldNum_Field] = decimal.MinValue;
                    }
                    else
                    {
                        findDr[YieldNum_Field] = decimal.MaxValue;
                    }
                }
            }


            if (dv.Table.Columns.Contains(YieldToExecution_Field))
                findDr[YieldToExecution_Field] = yieldToExecution;

            if (dv.Table.Columns.Contains(PriceType_Field))
                findDr[PriceType_Field] = priceType;

            if (dv.Table.Columns.Contains(CalFlagFieldTip_Field))
                findDr[CalFlagFieldTip_Field] = calTypeFieldTip;

            if (dv.Table.Columns.Contains(CalFlagNetPriceTip_Field))
                findDr[CalFlagNetPriceTip_Field] = calTypeNetPriceTip;

            if (dv.Table.Columns.Contains(ExerciseFlag_FIELD))
                findDr[ExerciseFlag_FIELD] = exerciseflag;

            if (findDr[VENUE_TYPE_GW_FIELD].ToString().ToUpper() == "QDM2")
            {
                //指示性报价
                findDr[ISINNER_FIELD] = "1";
                findDr[IsInnerInComplete_Field] = "1";
            }
            else
            {
                if ((qty == 0) && yield.ToString() == "--")
                {
                    findDr[IsInnerInComplete_Field] = "3";
                }
                else
                {
                    if (qty == 0 && yield.ToString() != "--")
                    {
                        findDr[IsInnerInComplete_Field] = "0";
                    }
                    else if (yield.ToString() == "--")
                    {
                        findDr[IsInnerInComplete_Field] = "2";
                    }
                    else
                    {
                        findDr[IsInnerInComplete_Field] = "0";
                    }
                }
            }

            if (dv.Table.Columns.Contains(BOND_TYPE_FIELD))
                findDr[BOND_TYPE_FIELD] = bondType;
            //Console.WriteLine("------>>>>>>>>>>>>************-----------:" + findDr[CUSTOMER_SOURCE_FIELD] + ", partySubName:" + findDr[PARTY_SUB_NAME_FIELD]);
        }

        public static string GenerateSourceByVenues(string venues, string partSubName, string roleName)
        {
            if (string.IsNullOrEmpty(venues) == false)
            {
                if (venues.ToUpper() == "INNER"
                    || venues.ToUpper() == "QUANT"
                    || venues.ToUpper() == "MANUAL"
                    || venues.ToUpper() == "IDB")
                {
                    return $"{roleName}-{partSubName}";
                }
            }
            return venues;
        }

        public static string IsIncomplete(GNoMDEntries obj, MarketDataSnapshotFullRefresh fullObj)
        {
            if (obj.MDEntrySize == null || obj.MDEntrySize == null || obj.MDEntryPx == null
                || CommonUtils.IsNumeric(obj.MDEntryPx.Trim()) == false
                 || (CommonUtils.ConvertToLong(obj.MDEntrySize.Trim()) == 0 && CommonUtils.IsBrokerSecurity(fullObj.SecurityID))
                 || (CommonUtils.ConvertToDecimal(obj.MDEntrySize.Trim()) == 0 && !CommonUtils.IsBrokerSecurity(fullObj.SecurityID)))
            {
                return "1";
            }
            return "0";
        }

        public static DataTable getDataTable(DataTable dt, string filter)
        {
            dt.DefaultView.RowFilter = filter;
            DataTable tempDt = dt.DefaultView.ToTable();
            return tempDt;
        }

        public static DataTable getDataTable(DataView dv, string filter)
        {
            dv.RowFilter = filter;
            DataTable tempDt = dv.ToTable();
            return tempDt;
        }

        
        public static string GetAloneSettleFilter(string settle)
        {
            StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();

            if (settle == STCConstants.ALL_PARTY)
            {
            }
            else if (settle == STCConstants.NOW)
            {
                bestCacheSettleFitlerBuf.Append($"(({CTPriceControler.SETTL_TYPE_FIELD} = '1' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1' or {CTPriceControler.SETTL_TYPE_FIELD} like '1,%') or ({CTPriceControler.SETTL_TYPE_FIELD} = '2' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2') or ({CTPriceControler.SETTL_TYPE_FIELD} = '3' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3'))");

            }
            else if (settle == "+1" || settle == "2" || settle == "3" || settle == "2,3")
            {
                bestCacheSettleFitlerBuf.Append($"(({CTPriceControler.SETTL_TYPE_FIELD} = '2' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2') or ({CTPriceControler.SETTL_TYPE_FIELD} = '3' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3'))");

            }
            else if (settle == "+0" || settle == "1")
            {
                bestCacheSettleFitlerBuf.Append($"({CTPriceControler.SETTL_TYPE_FIELD} = '1' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1' or {CTPriceControler.SETTL_TYPE_FIELD} like '1,%')");
            }
            else
            {
                bestCacheSettleFitlerBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                //bestCacheSettleFitlerBuf.Append($"{SETTL_TYPE_FIELD} <> '1' and {SETTL_TYPE_FIELD} <> '2' and {SETTL_TYPE_FIELD} <> '3' and {SETTL_TYPE_FIELD} <> '2,3'");
            }

            return bestCacheSettleFitlerBuf.ToString();
        }

        public static DataView CreateBaseDataView()
        {
            DataView baseDV = new DataTable().DefaultView;
            baseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
            baseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(DateTime));
            baseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
            baseDV.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
            baseDV.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));
            baseDV.Table.Columns.Add(CTPriceControler.ISINCOMPLETE_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DIVIDED_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

            baseDV.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));
            baseDV.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
            baseDV.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
            //baseDV.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));

            //_BaseDV.Sort = $"{VENUE_TYPE_GW_FIELD},{SOURCE_FIELD},{ENTRY_PX_FIELD},{MD_PRICE_LEVEL_FIELD}";
            baseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            baseDV.Table.PrimaryKey = new DataColumn[] { baseDV.Table.Columns[CTPriceControler.KEY_FIELD] };

            return baseDV;
        }


    }
}
