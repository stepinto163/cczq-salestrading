using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class CTPriceControler
    {
        public const string CDH = "CDH";

        #region 表格字段
        public const string KEY_FIELD = "Key";
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string TOTAL_QTY_FIELD = "TotalQty";
        public const string TIME_FIELD = "Time";
        public const string SOURCE_FIELD ="Source"; //"Venues";
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

        public const string PRICETYPE_FIELD = "PriceType";  // 价格类型 0-意向: 1-净价;2-全价;3-收益率;4-利差;8-经纪商未指定
        public const string DIVIDED_FIELD = "Divided";// 1:量可部分执行

        #endregion

        public static void UpdateDataToBaseDV(DataView dv, MarketDataSnapshotFullRefresh obj)
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

                FeedData(dv, key, securityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price,
                    netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader,
                    isMe, isIncomplete, traderName, priceType, divided);
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
        string text, object price, object netPrice, decimal qty, string time, int mdPirceLevel, string partySubName, string bargainFlag, string wholeFlag,
        string institutionName, string showTrader, bool isMe, string IsIncomplete, string traderName, string priceType, string divided)
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

            findDr[PRICETYPE_FIELD] = priceType;

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

            findDr[DIVIDED_FIELD] = divided;
        }

        public static void FeedData_CDH(DataView dv, string key, string securityID, string source, string venueTypeGW, string side, string settlType, string settleType_yuan, string firmFlag,
           string text, decimal price, decimal netPrice, long qty, string time, int mdPriceLevel, string barginFlag, string wholeFlag, string institutionName, string showTrader, bool isMe)
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
            findDr[ENTRY_PX_FIELD] = price;
            findDr[NET_PRICE_FIELD] = netPrice;
            findDr[TIME_FIELD] = time;
            findDr[DELETE_FLAG_FIELD] = "0";
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
            baseDV.Table.Columns.Add(CTPriceControler.PRICETYPE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.DIVIDED_FIELD, typeof(string));

            //_BaseDV.Sort = $"{VENUE_TYPE_GW_FIELD},{SOURCE_FIELD},{ENTRY_PX_FIELD},{MD_PRICE_LEVEL_FIELD}";
            baseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            baseDV.Table.PrimaryKey = new DataColumn[] { baseDV.Table.Columns[CTPriceControler.KEY_FIELD] };

            return baseDV;
        }



        public static string GetAloneSettleFilter(string settle)
        {
            StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();

            if (settle == CommonConstants.ALL_PARTY)
            {
            }
            else if (settle == CommonConstants.NOW)
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
    }
}
