using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.ViewModel
{
    /// <summary>
    /// 深度行情处理器
    /// </summary>
    public class CTPriceControler
    {
        public const string CDH = "CDH";
        public const string XSWAP = "XSWAP";

        #region 表格字段
        public const string KEY_FIELD = "Key";
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string TOTAL_QTY_FIELD = "TotalQty";
        public const string TIME_FIELD = "Time";
        public const string SOURCE_FIELD = "Source";
        public const string QTY_FIELD = "Qty";
        public const string ENTRY_PX_FIELD = "EntryPx";
        public const string NET_PRICE_FIELD = "NetPrice";
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

        public const string DELIVERY_TYPE_FILED = "DeliveryType";
        public const string CLEARING_METHOD_FILED = "ClearingMethod";
        public const string QUOTE_TYPE_FILED = "QuoteType";

        public const string IS_PERMISSION_FIELD = "IsPermission";//是否有权限

        public const string IS_MORE_FIELD = "IsMore";//是否显示多结算速度
        public const string QUOTE_STATUS_FIELD = "QuoteStatus";//报价状态

        public const string VENUE_SORT_FIELD = "VenueSort";//个人设置自定义Venue顺序
        public const string ISINNER_FIELD = "IsInner";//是否是内部
        public const string INFO3_FIELD = "Info3";//双边我的报价Bid深度挡位
        public const string INFO4_FIELD = "Info4";//双边我的报价Ofr深度挡位
        #endregion

        public static void UpdateDataToBaseDV(DataView dv, MarketDataSnapshotFullRefresh obj)
        {
            //设置删除标志
            foreach (DataRow dr in dv.Table.Rows)
            {
                dr[DELETE_FLAG_FIELD] = "1";
            }

            // 添加和更新数据(普通)
            foreach (MarketDataSnapshotFullRefresh.GNoMDEntries entry in obj.NoMDEntries)
            {
                //if (entry.QuoteStatus != "16")
                //{
                //    continue;
                //}

                //string key = $"{obj.SecurityID}#{entry.MDEntryType}#{entry.SettlType}#{entry.MDEntryPx}#{GenerateSourceByVenues(entry.Venues, entry.PartySubName)}";
                string key = entry.MDEntryID + entry.MDEntryType;
                string securityID = obj.SecurityID;
                string venues = entry.Venues;
                string venueTypeGW = entry.VenueTypeGW;
                string time = entry.MDEntryTime;
                string firmFlag = entry.FirmFlag;
                string text = entry.Text;
                string price = CommonUtils.ConvertToString(entry.MDEntryPx);
                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                long qty = CommonUtils.ConvertToLong(entry.MDEntrySize);
                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                string partySubName = GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);
                string side = entry.MDEntryType;
                string settlType = string.Empty;
                string settleType_yuan = string.Empty;

                if ((entry.SettlType.Contains(".")) || entry.SettlType.Contains('4'))
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

                string quoteStatus = entry.QuoteStatus;

                string isIncomplete = IsIncomplete(entry);
                string isInner = entry.VenueTypeGW == "INNER" ? "1" : "0";
                string info3 = entry.Info3;
                string info4 = entry.Info4;
                FeedData(dv, key, securityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price,
                    netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader, 
                    isMe, isIncomplete, traderName, transactTime, deliveryType, clearingMethod, quoteType, partyName, quoteStatus,isInner,info3, info4);
            }

            // 删除DeleteFlag为0的记录
            DataRow[] drs = dv.Table.Select($"{DELETE_FLAG_FIELD} = '1'", $"{DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }
            dv.Table.AcceptChanges();
        }

        public static void FeedData(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
        string text, object price, object netPrice, long qty, string time, int mdPirceLevel, string partySubName, string bargainFlag, string wholeFlag,
        string institutionName, string showTrader, bool isMe, string IsIncomplete, string traderName, string transactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
        string partyName = null, string quoteStatus = null, string isInner = "0" ,string info3 = null, string info4 = null)
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
                findDr[ISINNER_FIELD] = isInner;
                dv.Table.Rows.Add(findDr);
            }

            findDr[PARTY_NAME_FIELD] = partyName;
            if (!string.IsNullOrEmpty(partyName) && venueTypeGW == "QDMESP")
            {
                findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), partyName);
            }
            else
                findDr[CUSTOMER_SOURCE_FIELD] = ComboSourceUtil.Instance.FindEnum(partySubName, "venues");


            //Console.WriteLine("----------------:" + findDr[CUSTOMER_SOURCE_FIELD] + ", partySubName:" + findDr[PARTY_SUB_NAME_FIELD]);

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
            findDr[ENTRY_PX_FIELD] = price;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[QTY_FIELD] = qty;
            findDr[TIME_FIELD] = CommonUtils.ConvertTime(time);//time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[WHOLE_FLAG_FIELD] = wholeFlag;

            findDr[IS_PERMISSION_FIELD] = PermissionHandler.Instance.CheckVenuePermission(source, securityID);

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
            findDr[INFO3_FIELD] = info3;
            findDr[INFO4_FIELD] = info4;
        }

        public static void FeedData_CDH(DataView dv, string key,string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
           string text, decimal price, decimal netPrice, long qty, string time, int mdPriceLevel, string barginFlag, string wholeFlag, string institutionName, string showTrader, bool isMe,
           string partyName = null, string partySubName = null, string isInner = "0")
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
                long localQty = CommonUtils.ConvertToLong(findDr[QTY_FIELD].ToString());
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
            findDr[ENTRY_PX_FIELD] = price;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[TIME_FIELD] = CommonUtils.ConvertTime(time);//time;
            findDr[DELETE_FLAG_FIELD] = "0";

            findDr[IS_PERMISSION_FIELD] = PermissionHandler.Instance.CheckVenuePermission(source, securityID);

            findDr[VENUE_SORT_FIELD] = SystemParamHandler.Instance.GetVenueSort(source);

            findDr[PARTY_NAME_FIELD] = partyName;
            if (!string.IsNullOrEmpty(partyName) && venueTypeGW == "QDMESP")
            {
                findDr[CUSTOMER_SOURCE_FIELD] = string.Format("{0}-{1}", ComboSourceUtil.Instance.FindEnum(partySubName, "venues"), partyName);
            }
            else
                findDr[CUSTOMER_SOURCE_FIELD] = ComboSourceUtil.Instance.FindEnum(partySubName, "venues");

            findDr[ISINNER_FIELD] = isInner;
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

        public static string IsIncomplete(MarketDataSnapshotFullRefresh.GNoMDEntries obj)
        {
            if (obj.MDEntrySize == null || obj.MDEntrySize == null
                || CommonUtils.IsNumeric(obj.MDEntryPx.Trim()) == false
                 || CommonUtils.ConvertToLong(obj.MDEntrySize.Trim()) == 0)
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
    }
}
