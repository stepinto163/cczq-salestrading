using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Price;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Setting;
using STC.InnerBoard.Plugin.ViewModel;
using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace STC.InnerBoard.Plugin.View.Component
{
    public class CustomInnerDataGrid : ExDataGrid
    {
        private string tabStr = "    ";

        private ConcurrentDictionary<string, bool> confDic = new ConcurrentDictionary<string, bool>();

        public CustomInnerDataGrid()
        {
            InnerSettingViewModel_InnerCopyInfoChange();
            InnerSettingViewModel.InnerCopyInfoChange += InnerSettingViewModel_InnerCopyInfoChange;
        }

        private void InnerSettingViewModel_InnerCopyInfoChange()
        {
            confDic["票面_R"] = IsContain("RATE", "票面");
            confDic["中债估值_R"] = IsContain("RATE", "中债估值");
            confDic["中证估值_R"] = IsContain("RATE", "中证估值");
            confDic["到期日_R"] = IsContain("RATE", "到期日");
            confDic["中债久期_R"] = IsContain("RATE", "中债久期");
            confDic["债券余额_R"] = IsContain("RATE", "债券余额");
            confDic["发行量_R"] = IsContain("RATE", "发行量");
            confDic["是否提还_R"] = IsContain("RATE", "是否提还");
            confDic["是否免税_R"] = IsContain("RATE", "是否免税");

            confDic["票面_C"] = IsContain("CREDIT", "票面");
            confDic["评级"] = IsContain("CREDIT", "评级");
            confDic["中债隐含评级"] = IsContain("CREDIT", "中债隐含评级");
            confDic["担保方"] = IsContain("CREDIT", "担保方");
            confDic["中债估值_C"] = IsContain("CREDIT", "中债估值");
            confDic["中证估值_C"] = IsContain("CREDIT", "中证估值");
            confDic["募集类型"] = IsContain("CREDIT", "募集类型");
            confDic["企业类型"] = IsContain("CREDIT", "企业类型");
            confDic["地区"] = IsContain("CREDIT", "地区");
            confDic["城投级别"] = IsContain("CREDIT", "城投级别");
            confDic["发行人"] = IsContain("CREDIT", "发行人");
            confDic["债券余额_C"] = IsContain("CREDIT", "债券余额");
            confDic["发行量"] = IsContain("CREDIT", "发行量");
            confDic["到期日_C"] = IsContain("CREDIT", "到期日");
            confDic["是否免税_C"] = IsContain("CREDIT", "是否免税");

            confDic["按剩余期限排序"] = IsContain("COMMON", "按剩余期限排序");

            if (IsContain("TAB", "空格"))
            {
                tabStr = " ";
            }
            else if (IsContain("TAB", "Tab制表符"))
            {
                tabStr = "\t";
            } 
            else
            {
                tabStr = "    ";
            }
        }

        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                string copyMode = "1";
                if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
                {
                    copyMode = "2";
                }
                else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                && CommonUtils.CopyDic["COPYMODE"].Count > 0
                && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
                {
                    copyMode = "3";
                }

                if (this.Name == "InnerBoard_dataGrid")
                {
                    CopyData_Single(true, copyMode);
                }
                else if (this.Name == "InnerBoard_Best_dataGrid")
                {
                    CopyData_Best(true, copyMode);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("CustomInnerDataGrid OnExecutedCopy", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="needPrompt"></param>
        /// <param name="copyMode">1:内部最优 2：经纪商最优</param>
        public void Sub_CopyData_Single(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }

            foreach (InnerOrderData data in list)
            {
                string side = data.Side;
                string remainingTime = string.IsNullOrEmpty(data.RemainingTime) ? string.Empty : data.RemainingTime.Replace(" ", "");
                if (!string.IsNullOrEmpty(data.DayOffStr))
                {
                    remainingTime += $"({data.DayOffStr})";
                }
                //int index = remainingTime.IndexOf("+");
                //if (index >= 0)
                //{
                //    remainingTime = remainingTime.Substring(0, index);
                //}

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string px = data.Price.HasValue ? data.Price?.ToString("0.00##") : (side == "1" ? "Bid" : "Ofr");
                string px_Str = string.Empty;
                //string qty = data.Qty.HasValue ? data.Qty?.ToString("0") : "--";
                string qty = data.CopyInfo;
                string qty_Str = string.Empty;
                string star = data.FirmFlag == "0" ? "*" : string.Empty;

                if (copyMode == "2")
                {
                    string tempBidPx = null;
                    decimal tempBidSize = 0;
                    string tempBidTime = null;
                    string tempBidVenueTypeGW = null;
                    string tempBidVenue = null;
                    string tempBidFirmFlag = null;

                    string tempOfrPx = null;
                    decimal tempOfrSize = 0;
                    string tempOfrTime = null;
                    string tempOfrVenueTypeGW = null;
                    string tempOfrVenue = null;
                    string tempOfrFirmFlag = null;
                    string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityId, string.Empty, string.Empty);
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                    if (ctPrice != null && ctPrice.NoMDEntries != null && ctPrice.NoMDEntries.Count > 0)
                    {
                        var bidBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "0");
                        if (bidBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in bidBest)
                            {
                                if (index == 0)
                                {
                                    tempBidPx = entry.MDEntryPx;
                                    tempBidTime = entry.MDEntryTime;
                                    tempBidSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempBidFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempBidPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempBidTime) > 0)
                                    {
                                        tempBidVenue = entry.Venues;
                                        tempBidVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempBidVenueTypeGW == "CDH" || tempBidVenueTypeGW == "SOR")
                            {
                                tempBidSize = 0;
                                foreach (GNoMDEntries entry in bidBest)
                                {
                                    if (entry.VenueTypeGW == tempBidVenueTypeGW && entry.Venues == tempBidVenue && entry.MDEntryPx == tempBidPx)
                                    {
                                        tempBidSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }

                        var ofrBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "1");
                        if (ofrBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in ofrBest)
                            {
                                if (index == 0)
                                {
                                    tempOfrPx = entry.MDEntryPx;
                                    tempOfrTime = entry.MDEntryTime;
                                    tempOfrSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempOfrFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempOfrPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempOfrTime) > 0)
                                    {
                                        tempOfrVenue = entry.Venues;
                                        tempOfrVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempOfrVenueTypeGW == "CDH" || tempOfrVenueTypeGW == "SOR")
                            {
                                tempOfrSize = 0;
                                foreach (GNoMDEntries entry in ofrBest)
                                {
                                    if (entry.VenueTypeGW == tempOfrVenueTypeGW && entry.Venues == tempOfrVenue && entry.MDEntryPx == tempOfrPx)
                                    {
                                        tempOfrSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }

                    }
                    
                    px_Str = (tempBidPx == null ? "--" : tempBidPx) + tempBidFirmFlag + "/" + (tempOfrPx == null ? "--" : tempOfrPx) + tempOfrFirmFlag;
                    qty_Str = (tempBidSize == 0 ? "--" : tempBidSize.ToString("#")) + tempBidFirmFlag + "/" + (tempOfrSize == 0 ? "--" : tempOfrSize.ToString("#")) + tempOfrFirmFlag;
                }
                else if (copyMode == "3")
                {

                }
                else
                {
                    if (side == "1")
                    {
                        px_Str = $"{px}{star}/--";
                        qty_Str = $"{qty}/--";
                    }
                    else
                    {
                        px_Str = $"--/{px}{star}";
                        qty_Str = $"--/{qty}";
                    }
                }

                if (px_Str.Length > 0)
                {
                    px_Str = $"{tabStr}{px_Str}";
                }
                if (qty_Str.Length > 0)
                {
                    qty_Str = $"{tabStr}{qty_Str}";
                }

                //string CCDCValuation = data.CCDCValuation?.ToString("0.00##");

                //if (CCDCValuation == null)
                //{
                //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(securityId);
                //    if (ccdcValueModel != null)
                //    {
                //        CCDCValuation = ccdcValueModel.CCDCValuation;
                //    }
                //}

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                string local_gov_bond_type = string.Empty;
                if (!string.IsNullOrEmpty(data.BondCategory))
                {
                    if (data.BondCategory.IndexOf("(一般)") > 0)
                        local_gov_bond_type = $"{tabStr}一般债";
                    else if (data.BondCategory.IndexOf("专项") > 0)
                        local_gov_bond_type = $"{tabStr}专项债";
                }

                string ccdc = string.Empty;
                if ((confDic["中债估值_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["中债估值_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    //ccdc = (!data.ChinaBondValuation.HasValue || data.ChinaBondValuation == decimal.Zero) ? $"{tabStr}估值:--" : $"{tabStr}估值:{data.ChinaBondValuation?.ToString("0.00##")}";
                    ccdc = $"{tabStr}中债估值:--";
                    
                    if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero)
                    {
                        ccdc = $"{tabStr}中债估值:{data.ChinaBondExercise?.ToString("0.00##")}(权)";
                    }
                    else if (data.ChinaBondDue.HasValue && data.ChinaBondDue != decimal.Zero)
                    {
                        ccdc = $"{tabStr}中债估值:{data.ChinaBondDue?.ToString("0.00##")}";
                    }
                }

                string csi = string.Empty;
                if ((confDic["中证估值_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["中证估值_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    //ccdc = (!data.ChinaBondValuation.HasValue || data.ChinaBondValuation == decimal.Zero) ? $"{tabStr}估值:--" : $"{tabStr}估值:{data.ChinaBondValuation?.ToString("0.00##")}";
                    csi = $"{tabStr}中证估值:--";

                    if (data.CsiBondValuationExercise.HasValue && data.CsiBondValuationExercise != decimal.Zero)
                    {
                        csi = $"{tabStr}中证估值:{data.CsiBondValuationExercise?.ToString("0.00##")}(权)";
                    }
                    else if (data.CsiBondValuationDue.HasValue && data.CsiBondValuationDue != decimal.Zero)
                    {
                        csi = $"{tabStr}中证估值:{data.CsiBondValuationDue?.ToString("0.00##")}";
                    }
                }

                string coupon = string.Empty;
                if ((confDic["票面_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["票面_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    coupon = !data.CouponRateCurrent.HasValue ? $"{tabStr}票面:--" : $"{tabStr}票面:{data.CouponRateCurrent?.ToString("0.00##")}";
                }

                string rating = string.Empty;
                if (confDic["评级"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    rating = string.IsNullOrEmpty(data.Rating) ? $"{tabStr}主/债:--/--" : $"{tabStr}主/债:{data.Rating}";
                }

                string cdc_implited_rating = string.Empty;
                if (confDic["中债隐含评级"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    cdc_implited_rating = string.IsNullOrEmpty(data.Cdc_Implited_Rating) ? $"{tabStr}中债隐含:--" : $"{tabStr}中债隐含:{data.Cdc_Implited_Rating}";
                }

                string duration = string.Empty;
                if (confDic["中债久期_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                {
                    duration = !data.ModifiedDuration.HasValue ? $"{tabStr}中债久期:--" : $"{tabStr}中债久期:{data.ModifiedDuration?.ToString("0.00##")}";
                }

                string maturityDate = string.Empty;
                if ((confDic["到期日_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["到期日_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    string temp = ConvertToDateTime(data.MaturtityDate);
                    maturityDate = string.IsNullOrEmpty(data.MaturtityDate) ? $"{tabStr}到期日:--" : $"{tabStr}到期日:{temp}";
                }

                string guarantee = string.Empty;
                if (confDic["担保方"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    string warranterName = string.IsNullOrEmpty(data.WarranterName) ? "--" : data.WarranterName;
                    guarantee = !data.If_Guarantee ? string.Empty : $"{tabStr}有担保({warranterName})";
                }
                string issueType = string.Empty;
                if (confDic["募集类型"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.IssueType))
                {
                    issueType = $"{tabStr}{data.IssueType}";
                }

                string enterprise_type = string.Empty;
                if (confDic["企业类型"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.EnterpriseType) && !data.EnterpriseType.StartsWith("其他"))
                {
                    enterprise_type = $"{tabStr}{data.EnterpriseType}";
                }

                string issuer = string.Empty;
                if (confDic["发行人"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.IssuerName))
                {
                    issuer = $"{tabStr}发行人:{data.IssuerName}";
                }

                string region = string.Empty;
                if (confDic["地区"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.RegionType))
                {
                    region = $"{tabStr}{data.RegionType}";
                }

                string level = string.Empty;
                if (confDic["城投级别"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.Level) && !data.Level.StartsWith("其他"))
                {
                    level = $"{tabStr}{data.Level}";
                }

                string outstandingAmount = string.Empty;
                if (((confDic["债券余额_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["债券余额_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                    && data.Outstanding_Amount.HasValue)
                {
                    outstandingAmount = data.Outstanding_Amount?.ToString("0.00");
                    outstandingAmount = $"{tabStr}债券余额:{outstandingAmount}亿";
                }

                string issueAmount = string.Empty;
                if (((confDic["发行量"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["发行量_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                    && data.IssueAmount.HasValue)
                {
                    issueAmount = data.IssueAmount?.ToString("0.00");
                    issueAmount = $"{tabStr}发行量:{issueAmount}亿";
                }

                string If_Repay_displayText = string.Empty;
                if (confDic["是否提还_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                {
                    If_Repay_displayText = tabStr + ComboSourceUtil.Instance.FindEnum(data.If_Repay, "ifrepay");
                }

                string isTaxFree = string.Empty;
                if ((confDic["是否免税_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                    || (confDic["是否免税_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB")))
                {
                    if (data.SignDic4.ContainsKey(Consts.IF_TAX_FREE) && data.SignDic4[Consts.IF_TAX_FREE] == "01")
                    {
                        isTaxFree = "(免)";
                    }
                }

                plainTxt.Append($"{remainingTime}{tabStr}{symbolName}{isTaxFree}{tabStr}{securityId}{px_Str}{qty_Str}{local_gov_bond_type}{coupon}{rating}{cdc_implited_rating}{guarantee}{ccdc}{duration}{csi}{issueType}{enterprise_type}{region}{level}{issuer}{outstandingAmount}{issueAmount}{maturityDate}{If_Repay_displayText}");
            }

            //复制,不加try会导致程序无法退出
            Common.View.FQ.FQSendWindow.CopyStr = string.Empty;
            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.View.FQ.FQSendWindow.CopyStr = plainTxt.ToString();
                    ClipboardHelper.CopyToClipboard(Common.View.FQ.FQSendWindow.CopyStr, true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="needPrompt"></param>
        /// <param name="copyMode">1:内部最优 2：经纪商最优</param>
        public void Sub_CopyData_Single_YC(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();
            foreach (InnerOrderData data in datas)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                string side = data.Side;
                string remainingTime = string.IsNullOrEmpty(data.RemainingTime) ? string.Empty : data.RemainingTime.Replace(" ", "");
                if (!string.IsNullOrEmpty(data.DayOffStr))
                {
                    remainingTime += $"({data.DayOffStr})";
                }

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string star = data.FirmFlag == "0" ? "*" : string.Empty;
                string px = data.Price.HasValue ? data.Price?.ToString("0.00##") + star : (side == "1" ? "Bid" : "Ofr");
                string px_Str;
                string qty = data.Qty.HasValue ? data.Qty?.ToString("0") + star : "--";
                string qty_Str;
               

                if (side == "1")
                {
                    px_Str = $"{px}/--";
                    qty_Str = $"{qty}/--";
                }
                else
                {
                    px_Str = $"--/{px}";
                    qty_Str = $"--/{qty}";
                }

                string ccdc = string.Empty;
                if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero)
                {
                    ccdc = $" 中债估值{data.ChinaBondExercise?.ToString("0.0000")}权";
                }
                else if (data.ChinaBondDue.HasValue && data.ChinaBondDue != decimal.Zero)
                {
                    ccdc = $" 中债估值{data.ChinaBondDue?.ToString("0.0000")}";
                }

                string csi = string.Empty;
                if (data.CsiBondValuationExercise.HasValue && data.CsiBondValuationExercise != decimal.Zero)
                {
                    csi = $" 中证估值{data.CsiBondValuationExercise?.ToString("0.0000")}权";
                }
                else if (data.CsiBondValuationDue.HasValue && data.CsiBondValuationDue != decimal.Zero)
                {
                    csi = $" 中证估值{data.CsiBondValuationDue?.ToString("0.0000")}";
                }

                string coupon = !data.CouponRateCurrent.HasValue ? string.Empty : $" 票面{data.CouponRateCurrent?.ToString("0.00##")}";

                string region = string.IsNullOrEmpty(data.RegionType) ? string.Empty : $" {data.RegionType}";

                string issueAmount = !data.IssueAmount.HasValue ? string.Empty : $" {data.IssueAmount?.ToString("0.00")}亿";

                string rating = string.IsNullOrEmpty(data.Rating) ? string.Empty : $" 主/债:{data.Rating}";

                string warranterName = string.IsNullOrEmpty(data.WarranterName) ? "--" : data.WarranterName;
                string guarantee = !data.If_Guarantee ? string.Empty : $" {warranterName}担保";

                string level = string.IsNullOrEmpty(data.Level) || data.Level.StartsWith("其他") ? string.Empty : $" {data.Level}";

                string issueType = string.IsNullOrEmpty(data.IssueType) ? string.Empty : $" {data.IssueType}";

                string clientType = CommonUtils.GetTagNameByCode(data.SignDic1[Utils.Constants.BOND_SUB_TYPE], Utils.Constants.BOND_SUB_TYPE);
                if (clientType == "国债" || clientType == "金融债" || clientType == "央票")
                {
                    plainTxt.Append($"{remainingTime} {securityId} {symbolName} {px_Str} {qty_Str}{ccdc}{coupon}{region}{issueAmount}");
                }
                else if (clientType == "地方债")
                {
                    plainTxt.Append($"{remainingTime} {securityId} {symbolName} {px_Str} {qty_Str}{ccdc}{coupon}{issueAmount}");
                }
                else // if (data.SecurityType != "BNC" || data.BondSubType == "PSB") 信用
                {
                    if (securityId.ToUpper().EndsWith(".SH") || securityId.ToUpper().EndsWith(".SZ"))
                    {
                        plainTxt.Append($"{remainingTime} {securityId} {symbolName}{rating} {px_Str} {qty_Str}{ccdc}{csi}{coupon}{guarantee}{region}{level}{issueType}");
                    }
                    else
                    {
                        plainTxt.Append($"{remainingTime} {securityId} {symbolName}{rating} {px_Str} {qty_Str}{ccdc}{csi}{coupon}{guarantee}{region}{level}");
                    }
                }
            }

            //复制,不加try会导致程序无法退出
            Common.View.FQ.FQSendWindow.CopyStr_YC = string.Empty;
            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.View.FQ.FQSendWindow.CopyStr_YC = plainTxt.ToString();
                    //ClipboardHelper.CopyToClipboard(Common.View.FQ.FQSendWindow.CopyStr, true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }


        /// <summary>
        /// "剩余期限（包括含权期限，是否休假）"、"债券代码（.IB .SH .SZ）"、"债券简称"、 "价格（bid/ofr，--/ofr）（"全""净"）"、"券面（万为单位，如 3000，3000/3000）"、"中债估值 （含权债默认为含权估值"权"）"
        /// </summary>
        /// <param name="dg"></param>
        public void CopyData_Single(bool needPrompt = true, string copyMode = "1")
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                copyMode = "2";
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                copyMode = "3";
            }
            else
            {
                copyMode = "1";
            }

            Sub_CopyData_Single(this.SelectedItems as IList, needPrompt, copyMode);
            Sub_CopyData_Single_YC(this.SelectedItems as IList, needPrompt, copyMode);
        }

        public void Sub_CopyData_SecurityID(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }

            foreach (InnerOrderData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append(data.SecurityID);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_SecurityID(bool needPrompt = true, string copyMode = "1")
        {
            Sub_CopyData_SecurityID(this.SelectedItems as IList, needPrompt, copyMode);
        }

        public void Sub_CopyData_Symbol(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }

            foreach (InnerOrderData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append(data.Symbol);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_Symbol(bool needPrompt = true, string copyMode = "1")
        {
            Sub_CopyData_Symbol(this.SelectedItems as IList, needPrompt, copyMode);
        }

        public void Sub_CopyData_Single_Price(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }
            

            foreach (InnerOrderData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string px = data.Price.HasValue ? data.Price?.ToString("0.00##") : (data.Side == "1" ? "Bid" : "Ofr");
                string px_Str;
                //string qty = data.Qty.HasValue ? data.Qty?.ToString("0") : "--";
                string qty = data.CopyInfo;
                string qty_Str;
                string star = data.FirmFlag == "0" ? "*" : string.Empty;

                if (copyMode == "2")
                {
                    string tempBidPx = null;
                    decimal tempBidSize = 0;
                    string tempBidTime = null;
                    string tempBidVenueTypeGW = null;
                    string tempBidVenue = null;
                    string tempBidFirmFlag = null;

                    string tempOfrPx = null;
                    decimal tempOfrSize = 0;
                    string tempOfrTime = null;
                    string tempOfrVenueTypeGW = null;
                    string tempOfrVenue = null;
                    string tempOfrFirmFlag = null;
                    string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityId, string.Empty, string.Empty);
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                    if (ctPrice != null && ctPrice.NoMDEntries != null && ctPrice.NoMDEntries.Count > 0)
                    {
                        var bidBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "0");
                        if (bidBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in bidBest)
                            {
                                if (index == 0)
                                {
                                    tempBidPx = entry.MDEntryPx;
                                    tempBidTime = entry.MDEntryTime;
                                    tempBidSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempBidFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempBidPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempBidTime) > 0)
                                    {
                                        tempBidVenue = entry.Venues;
                                        tempBidVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempBidVenueTypeGW == "CDH" || tempBidVenueTypeGW == "SOR")
                            {
                                tempBidSize = 0;
                                foreach (GNoMDEntries entry in bidBest)
                                {
                                    if (entry.VenueTypeGW == tempBidVenueTypeGW && entry.Venues == tempBidVenue && entry.MDEntryPx == tempBidPx)
                                    {
                                        tempBidSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }

                        var ofrBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "1");
                        if (ofrBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in ofrBest)
                            {
                                if (index == 0)
                                {
                                    tempOfrPx = entry.MDEntryPx;
                                    tempOfrTime = entry.MDEntryTime;
                                    tempOfrSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempOfrFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempOfrPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempOfrTime) > 0)
                                    {
                                        tempOfrVenue = entry.Venues;
                                        tempOfrVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempOfrVenueTypeGW == "CDH" || tempOfrVenueTypeGW == "SOR")
                            {
                                tempOfrSize = 0;
                                foreach (GNoMDEntries entry in ofrBest)
                                {
                                    if (entry.VenueTypeGW == tempOfrVenueTypeGW && entry.Venues == tempOfrVenue && entry.MDEntryPx == tempOfrPx)
                                    {
                                        tempOfrSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }
                    }
                    px_Str = (tempBidPx == null ? "--" : tempBidPx) + tempBidFirmFlag + "/" + (tempOfrPx == null ? "--" : tempOfrPx) + tempOfrFirmFlag;
                    qty_Str = (tempBidSize == 0 ? "--" : tempBidSize.ToString("#")) + tempBidFirmFlag + "/" + (tempOfrSize == 0 ? "--" : tempOfrSize.ToString("#")) + tempOfrFirmFlag;
                }
                else
                {
                    if (data.Side == "1")
                    {
                        px_Str = $"{px}{star}/--";
                        qty_Str = $"{qty}/--";
                    }
                    else
                    {
                        px_Str = $"--/{px}{star}";
                        qty_Str = $"--/{qty}";
                    }
                }

                plainTxt.Append(px_Str);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_Single_Price(bool needPrompt = true, string copyMode = "1")
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                copyMode = "2";
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                copyMode = "3";
            }
            else
            {
                copyMode = "1";
            }


            Sub_CopyData_Single_Price(this.SelectedItems as IList, needPrompt, copyMode);
        }

        public void Sub_CopyData_Best_Price(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }


            foreach (InnerOrderData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string star_bid = string.Empty;
                string star_ofr = string.Empty;
                string px_Str;
                string qty_Str;

                if (copyMode == "2")
                {
                    string tempBidPx = null;
                    decimal tempBidSize = 0;
                    string tempBidTime = null;
                    string tempBidVenueTypeGW = null;
                    string tempBidVenue = null;
                    string tempBidFirmFlag = null;

                    string tempOfrPx = null;
                    decimal tempOfrSize = 0;
                    string tempOfrTime = null;
                    string tempOfrVenueTypeGW = null;
                    string tempOfrVenue = null;
                    string tempOfrFirmFlag = null;

                    string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityId, string.Empty, string.Empty);
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                    if (ctPrice != null && ctPrice.NoMDEntries != null && ctPrice.NoMDEntries.Count > 0)
                    {
                        var bidBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "0");
                        if (bidBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in bidBest)
                            {
                                if (index == 0)
                                {
                                    tempBidPx = entry.MDEntryPx;
                                    tempBidTime = entry.MDEntryTime;
                                    tempBidSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempBidFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempBidPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempBidTime) > 0)
                                    {
                                        tempBidVenue = entry.Venues;
                                        tempBidVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempBidVenueTypeGW == "CDH" || tempBidVenueTypeGW == "SOR")
                            {
                                tempBidSize = 0;
                                foreach (GNoMDEntries entry in bidBest)
                                {
                                    if (entry.VenueTypeGW == tempBidVenueTypeGW && entry.Venues == tempBidVenue && entry.MDEntryPx == tempBidPx)
                                    {
                                        tempBidSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }

                        var ofrBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "1");
                        if (ofrBest != null)
                        {
                            int index = 0;
                            foreach (GNoMDEntries entry in ofrBest)
                            {
                                if (index == 0)
                                {
                                    tempOfrPx = entry.MDEntryPx;
                                    tempOfrTime = entry.MDEntryTime;
                                    tempOfrSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempOfrFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempOfrPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempOfrTime) > 0)
                                    {
                                        tempOfrVenue = entry.Venues;
                                        tempOfrVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempOfrVenueTypeGW == "CDH" || tempOfrVenueTypeGW == "SOR")
                            {
                                tempOfrSize = 0;
                                foreach (GNoMDEntries entry in ofrBest)
                                {
                                    if (entry.VenueTypeGW == tempOfrVenueTypeGW && entry.Venues == tempOfrVenue && entry.MDEntryPx == tempOfrPx)
                                    {
                                        tempOfrSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }
                    }

                    px_Str = (tempBidPx == null ? "--" : tempBidPx) + tempBidFirmFlag + "/" + (tempOfrPx == null ? "--" : tempOfrPx) + tempOfrFirmFlag;
                    qty_Str = (tempBidSize == 0 ? "--" : tempBidSize.ToString()) + tempBidFirmFlag + "/" + (tempOfrSize == 0 ? "--" : tempOfrSize.ToString()) + tempOfrFirmFlag;
                }
                else
                {
                    star_bid = data.FirmFlag_InnerBid == "0" ? "*" : string.Empty;
                    star_ofr = data.FirmFlag_InnerOfr == "0" ? "*" : string.Empty;
                    px_Str = (data.Price_InnerBid.HasValue ? data.Price_InnerBid?.ToString("0.00##") + star_bid : (data.Count_InnerBid > 0 ? "Bid" : "--")) + "/" + (data.Price_InnerOfr.HasValue ? data.Price_InnerOfr?.ToString("0.00##") + star_ofr : (data.Count_InnerOfr > 0 ? "Ofr" : "--"));
                    qty_Str = (string.IsNullOrEmpty(data.CopyInfo_InnerBid) ? "--" : data.CopyInfo_InnerBid) + "/" + (string.IsNullOrEmpty(data.CopyInfo_InnerOfr) ? "--" : data.CopyInfo_InnerOfr);
                }

                plainTxt.Append(px_Str);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_Best_Price(bool needPrompt = true, string copyMode = "1")
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                copyMode = "2";
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                copyMode = "3";
            }
            else
            {
                copyMode = "1";
            }

            Sub_CopyData_Best_Price(this.SelectedItems as IList, needPrompt, copyMode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="needPrompt"></param>
        /// <param name="copyMode">1:内部最优 2：经纪商最优</param>
        public void Sub_CopyData_Best(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();


            IEnumerable<InnerOrderData> list;
            if (confDic["按剩余期限排序"])
            {
                list = datas.Cast<InnerOrderData>().OrderBy(item => item.TermDays);
            }
            else
            {
                list = datas.Cast<InnerOrderData>();
            }

            foreach (InnerOrderData data in list)
            {
                string side = data.Side;
                string remainingTime = string.IsNullOrEmpty(data.RemainingTime) ? string.Empty : data.RemainingTime.Replace(" ", "");
                if (!string.IsNullOrEmpty(data.DayOffStr))
                {
                    remainingTime += $"({data.DayOffStr})";
                }
                //int index = remainingTime.IndexOf("+");
                //if (index >= 0)
                //{
                //    remainingTime = remainingTime.Substring(0, index);
                //}
                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string star_bid = string.Empty;
                string star_ofr = string.Empty;
                string px_Str = string.Empty;
                string qty_Str = string.Empty;
                if (copyMode == "2")
                {
                    string tempBidPx = null;
                    decimal tempBidSize = 0;
                    string tempBidTime = null;
                    string tempBidVenueTypeGW = null;
                    string tempBidVenue = null;
                    string tempBidFirmFlag = null;

                    string tempOfrPx = null;
                    decimal tempOfrSize = 0;
                    string tempOfrTime = null;
                    string tempOfrVenueTypeGW = null;
                    string tempOfrVenue = null;
                    string tempOfrFirmFlag = null;

                    string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityId, string.Empty, string.Empty);
                    MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
                    if (ctPrice != null && ctPrice.NoMDEntries != null && ctPrice.NoMDEntries.Count > 0)
                    {
                        var bidBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "0");
                        if (bidBest != null)
                        {
                            int index = 0;
                            foreach(GNoMDEntries entry in bidBest)
                            {
                                if (index == 0)
                                {
                                    tempBidPx = entry.MDEntryPx;
                                    tempBidTime = entry.MDEntryTime;
                                    tempBidSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempBidFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempBidPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempBidTime) > 0)
                                    {
                                        tempBidVenue = entry.Venues;
                                        tempBidVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempBidVenueTypeGW == "CDH" || tempBidVenueTypeGW == "SOR")
                            {
                                tempBidSize = 0;
                                foreach (GNoMDEntries entry in bidBest)
                                {
                                    if (entry.VenueTypeGW == tempBidVenueTypeGW && entry.Venues == tempBidVenue && entry.MDEntryPx == tempBidPx)
                                    {
                                        tempBidSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }

                        var ofrBest = ctPrice.NoMDEntries?.Where(entry => entry.MDEntryType == "1");
                        if (ofrBest != null)
                        {
                            int index = 0;
                            foreach(GNoMDEntries entry in ofrBest)
                            {
                                if (index == 0)
                                {
                                    tempOfrPx = entry.MDEntryPx;
                                    tempOfrTime = entry.MDEntryTime;
                                    tempOfrSize = entry.MDEntrySize.ConvertToDecimal();
                                    tempOfrFirmFlag = entry.FirmFlag == "0" ? "*" : string.Empty;
                                }

                                if (entry.MDEntryPx == tempOfrPx)
                                {
                                    if (entry.MDEntryTime.CompareTo(tempOfrTime) > 0)
                                    {
                                        tempOfrVenue = entry.Venues;
                                        tempOfrVenueTypeGW = entry.VenueTypeGW;
                                    }
                                }
                                index++;
                            }
                            if (tempOfrVenueTypeGW == "CDH" || tempOfrVenueTypeGW == "SOR")
                            {
                                tempOfrSize = 0;
                                foreach (GNoMDEntries entry in ofrBest)
                                {
                                    if (entry.VenueTypeGW == tempOfrVenueTypeGW && entry.Venues == tempOfrVenue && entry.MDEntryPx == tempOfrPx)
                                    {
                                        tempOfrSize += entry.MDEntrySize.ConvertToDecimal();
                                    }
                                }
                            }
                        }
                    }
                    px_Str = (tempBidPx == null ? "--" : tempBidPx) + tempBidFirmFlag + "/" + (tempOfrPx == null ? "--" : tempOfrPx) + tempOfrFirmFlag;
                    qty_Str = (tempBidSize == 0 ? "--" : tempBidSize.ToString()) + tempBidFirmFlag + "/" + (tempOfrSize == 0 ? "--" : tempOfrSize.ToString()) + tempOfrFirmFlag;
                }
                else if (copyMode == "3")
                {

                }
                else
                {
                    star_bid = data.FirmFlag_InnerBid == "0" ? "*" : string.Empty;
                    star_ofr = data.FirmFlag_InnerOfr == "0" ? "*" : string.Empty;
                    px_Str = (data.Price_InnerBid.HasValue ? data.Price_InnerBid?.ToString("0.00##") + star_bid : (data.Count_InnerBid > 0 ? "Bid" : "--")) + "/" + (data.Price_InnerOfr.HasValue ? data.Price_InnerOfr?.ToString("0.00##") + star_ofr : (data.Count_InnerOfr > 0 ? "Ofr" : "--"));
                    qty_Str = (string.IsNullOrEmpty(data.CopyInfo_InnerBid) ? "--" : data.CopyInfo_InnerBid) + "/" + (string.IsNullOrEmpty(data.CopyInfo_InnerOfr) ? "--" : data.CopyInfo_InnerOfr);
                }

                if (px_Str.Length > 0)
                {
                    px_Str = $"{tabStr}{px_Str}";
                }
                if (qty_Str.Length > 0)
                {
                    qty_Str = $"{tabStr}{qty_Str}";
                }

                //string CCDCValuation = data.CCDCValuation?.ToString("0.00##");

                //if (CCDCValuation == null)
                //{
                //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(securityId);
                //    if (ccdcValueModel != null)
                //    {
                //        CCDCValuation = ccdcValueModel.CCDCValuation;
                //    }
                //}

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }


                string local_gov_bond_type = string.Empty;
                if (!string.IsNullOrEmpty(data.BondCategory))
                {
                    if (data.BondCategory.IndexOf("(一般)") > 0)
                        local_gov_bond_type = $"{tabStr}一般债";
                    else if (data.BondCategory.IndexOf("专项") > 0)
                        local_gov_bond_type = $"{tabStr}专项债";
                }

                string ccdc = string.Empty;
                if ((confDic["中债估值_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                   || (confDic["中债估值_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    //ccdc = (!data.ChinaBondValuation.HasValue || data.ChinaBondValuation == decimal.Zero) ? $"{tabStr}估值:--" : $"{tabStr}估值:{data.ChinaBondValuation?.ToString("0.00##")}";
                    ccdc = $"{tabStr}中债估值:--";

                    if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero)
                    {
                        ccdc = $"{tabStr}中债估值:{data.ChinaBondExercise}(权)";
                    }
                    else if (data.ChinaBondDue.HasValue && data.ChinaBondDue != decimal.Zero)
                    {
                        ccdc = $"{tabStr}中债估值:{data.ChinaBondDue}";
                    }
                }

                string csi = string.Empty;
                if ((confDic["中证估值_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["中证估值_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    //ccdc = (!data.ChinaBondValuation.HasValue || data.ChinaBondValuation == decimal.Zero) ? $"{tabStr}估值:--" : $"{tabStr}估值:{data.ChinaBondValuation?.ToString("0.00##")}";
                    csi = $"{tabStr}中证估值:--";

                    if (data.CsiBondValuationExercise.HasValue && data.CsiBondValuationExercise != decimal.Zero)
                    {
                        csi = $"{tabStr}中证估值:{data.CsiBondValuationExercise}(权)";
                    }
                    else if (data.CsiBondValuationDue.HasValue && data.CsiBondValuationDue != decimal.Zero)
                    {
                        csi = $"{tabStr}中证估值:{data.CsiBondValuationDue}";
                    }
                }

                string coupon = string.Empty;
                if ((confDic["票面_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["票面_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    coupon = !data.CouponRateCurrent.HasValue ? $"{tabStr}票面:--" : $"{tabStr}票面:{data.CouponRateCurrent?.ToString("0.00##")}";
                }

                string rating = string.Empty;
                if (confDic["评级"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    rating = string.IsNullOrEmpty(data.Rating) ? $"{tabStr}主/债:--/--" : $"{tabStr}主/债:{data.Rating}";
                }

                string cdc_implited_rating = string.Empty;
                if (confDic["中债隐含评级"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    cdc_implited_rating = string.IsNullOrEmpty(data.Cdc_Implited_Rating) ? $"{tabStr}中债隐含:--" : $"{tabStr}中债隐含:{data.Cdc_Implited_Rating}";
                }

                string duration = string.Empty;
                if (confDic["中债久期_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                {
                    duration = !data.ModifiedDuration.HasValue ? $"{tabStr}中债久期:--" : $"{tabStr}中债久期:{data.ModifiedDuration?.ToString("0.00##")}";
                }

                string maturityDate = string.Empty;
                if ((confDic["到期日_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["到期日_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                {
                    string temp = ConvertToDateTime(data.MaturtityDate);
                    maturityDate = string.IsNullOrEmpty(data.MaturtityDate) ? $"{tabStr}到期日:--" : $"{tabStr}到期日:{temp}";
                }

                string guarantee = string.Empty;
                if (confDic["担保方"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                {
                    string warranterName = string.IsNullOrEmpty(data.WarranterName) ? "--" : data.WarranterName;
                    guarantee = !data.If_Guarantee ? string.Empty : $"{tabStr}有担保({warranterName})";
                }

                string issueType = string.Empty;
                if (confDic["募集类型"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.IssueType))
                {
                    issueType = $"{tabStr}{data.IssueType}";
                }

                string enterprise_type = string.Empty;
                if (confDic["企业类型"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.EnterpriseType) && !data.EnterpriseType.StartsWith("其他"))
                {
                    enterprise_type = $"{tabStr}{data.EnterpriseType}";
                }

                string issuer = string.Empty;
                if (confDic["发行人"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.IssuerName))
                {
                    issuer = $"{tabStr}发行人:{data.IssuerName}";
                }

                string region = string.Empty;
                if (confDic["地区"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.RegionType))
                {
                    region = $"{tabStr}{data.RegionType}";
                }

                string level = string.Empty;
                if (confDic["城投级别"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB") && !string.IsNullOrEmpty(data.Level) && !data.Level.StartsWith("其他"))
                {
                    level = $"{tabStr}{data.Level}";
                }

                string outstandingAmount = string.Empty;
                if (((confDic["债券余额_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["债券余额_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                    && data.Outstanding_Amount.HasValue)
                {
                    outstandingAmount = data.Outstanding_Amount?.ToString("0.00");
                    outstandingAmount = $"{tabStr}债券余额:{outstandingAmount}亿";
                }

                string issueAmount = string.Empty;
                if (((confDic["发行量"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB"))
                    || (confDic["发行量_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB"))
                    && data.IssueAmount.HasValue)
                {
                    issueAmount = data.IssueAmount?.ToString("0.00");
                    issueAmount = $"{tabStr}发行量:{issueAmount}亿";
                }

                string If_Repay_displayText = string.Empty;
                if (confDic["是否提还_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                {
                    If_Repay_displayText = tabStr + ComboSourceUtil.Instance.FindEnum(data.If_Repay, "ifrepay");
                }

                string isTaxFree = string.Empty;
                if ((confDic["是否免税_R"] && data.SecurityType == "BNC" && data.BondSubType != "PSB")
                    || (confDic["是否免税_C"] && (data.SecurityType != "BNC" || data.BondSubType == "PSB")))
                {
                    if ((data.SignDic4 != null && data.SignDic4.ContainsKey(Consts.IF_TAX_FREE) && data.SignDic4[Consts.IF_TAX_FREE] == "01")
                        || (data.SignDic_InnerBid4 != null && data.SignDic_InnerBid4.ContainsKey(Consts.IF_TAX_FREE) && data.SignDic_InnerBid4[Consts.IF_TAX_FREE] == "01")
                        || (data.SignDic_InnerOfr4 != null && data.SignDic_InnerOfr4.ContainsKey(Consts.IF_TAX_FREE) && data.SignDic_InnerOfr4[Consts.IF_TAX_FREE] == "01"))
                    {
                        isTaxFree = "(免)";
                    }
                }

                plainTxt.Append($"{remainingTime}{tabStr}{symbolName}{isTaxFree}{tabStr}{securityId}{px_Str}{qty_Str}{local_gov_bond_type}{coupon}{rating}{cdc_implited_rating}{guarantee}{ccdc}{duration}{csi}{issueType}{enterprise_type}{region}{level}{issuer}{outstandingAmount}{issueAmount}{maturityDate}{If_Repay_displayText}");
            }
            //复制
            //复制,不加try会导致程序无法退出
            Common.View.FQ.FQSendWindow.CopyStr = string.Empty;
            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.View.FQ.FQSendWindow.CopyStr = plainTxt.ToString();
                    ClipboardHelper.CopyToClipboard(Common.View.FQ.FQSendWindow.CopyStr, true);
                    
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void Sub_CopyData_Best_YC(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            foreach (InnerOrderData data in datas)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                string side = data.Side;
                string remainingTime = string.IsNullOrEmpty(data.RemainingTime) ? string.Empty : data.RemainingTime.Replace(" ", "");
                if (!string.IsNullOrEmpty(data.DayOffStr))
                {
                    remainingTime += $"({data.DayOffStr})";
                }

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string star_bid = data.FirmFlag_InnerBid == "0" ? "*" : string.Empty;
                string star_ofr = data.FirmFlag_InnerOfr == "0" ? "*" : string.Empty;
                string px_Str = (data.Price_InnerBid.HasValue ? data.Price_InnerBid?.ToString("0.00##") + star_bid : (data.Count_InnerBid > 0 ? "Bid" : "--")) + "/" + (data.Price_InnerOfr.HasValue ? data.Price_InnerOfr?.ToString("0.00##") + star_ofr : (data.Count_InnerOfr > 0 ? "Ofr" : "--"));
                string qty_Str = (!data.Qty_InnerBid.HasValue ? "--" : data.Qty_InnerBid.ToString() + star_bid) + "/" + (!data.Qty_InnerOfr.HasValue ? "--" : data.Qty_InnerOfr.ToString() + star_ofr);
             

                string ccdc = string.Empty;
                if (data.ChinaBondExercise.HasValue && data.ChinaBondExercise != decimal.Zero)
                {
                    ccdc = $" 中债估值{data.ChinaBondExercise?.ToString("0.0000")}权";
                }
                else if (data.ChinaBondDue.HasValue && data.ChinaBondDue != decimal.Zero)
                {
                    ccdc = $" 中债估值{data.ChinaBondDue?.ToString("0.0000")}";
                }

                string csi = string.Empty;
                if (data.CsiBondValuationExercise.HasValue && data.CsiBondValuationExercise != decimal.Zero)
                {
                    csi = $" 中证估值{data.CsiBondValuationExercise?.ToString("0.0000")}权";
                }
                else if (data.CsiBondValuationDue.HasValue && data.CsiBondValuationDue != decimal.Zero)
                {
                    csi = $" 中证估值{data.CsiBondValuationDue?.ToString("0.0000")}";
                }

                string coupon = !data.CouponRateCurrent.HasValue ? string.Empty : $" 票面{data.CouponRateCurrent?.ToString("0.00##")}";

                string region = string.IsNullOrEmpty(data.RegionType) ? string.Empty : $" {data.RegionType}";

                string issueAmount = !data.IssueAmount.HasValue ? string.Empty : $" {data.IssueAmount?.ToString("0.00")}亿";

                string rating = string.IsNullOrEmpty(data.Rating) ? string.Empty : $" 主/债:{data.Rating}";

                string warranterName = string.IsNullOrEmpty(data.WarranterName) ? "--" : data.WarranterName;
                string guarantee = !data.If_Guarantee ? string.Empty : $" {warranterName}担保";

                string level = string.IsNullOrEmpty(data.Level) || data.Level.StartsWith("其他") ? string.Empty : $" {data.Level}";

                string issueType = string.IsNullOrEmpty(data.IssueType) ? string.Empty : $" {data.IssueType}";

                Dictionary<string, string> signDic1 = data.SignDic_InnerBid1;
                if (signDic1 == null)
                {
                    signDic1 = data.SignDic_InnerOfr1;
                }
                string clientType = CommonUtils.GetTagNameByCode(signDic1[Utils.Constants.BOND_SUB_TYPE], Utils.Constants.BOND_SUB_TYPE);
                if (clientType == "国债" || clientType == "金融债" || clientType == "央票")
                {
                    plainTxt.Append($"{remainingTime} {securityId} {symbolName} {px_Str} {qty_Str}{ccdc}{coupon}{region}{issueAmount}");
                }
                else if (clientType == "地方债")
                {
                    plainTxt.Append($"{remainingTime} {securityId} {symbolName} {px_Str} {qty_Str}{ccdc}{coupon}{issueAmount}");
                }
                else // if (data.SecurityType != "BNC" || data.BondSubType == "PSB") 信用
                {
                    if (securityId.ToUpper().EndsWith(".SH") || securityId.ToUpper().EndsWith(".SZ"))
                    {
                        plainTxt.Append($"{remainingTime} {securityId} {symbolName}{rating} {px_Str} {qty_Str}{ccdc}{csi}{coupon}{guarantee}{region}{level}{issueType}");
                    }
                    else
                    {
                        plainTxt.Append($"{remainingTime} {securityId} {symbolName}{rating} {px_Str} {qty_Str}{ccdc}{csi}{coupon}{guarantee}{region}{level}");
                    }
                }

            }
            //复制
            //复制,不加try会导致程序无法退出
            Common.View.FQ.FQSendWindow.CopyStr_YC = string.Empty;
            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.View.FQ.FQSendWindow.CopyStr_YC = plainTxt.ToString();
                   // ClipboardHelper.CopyToClipboard(Common.View.FQ.FQSendWindow.CopyStr, true);

                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        /// <summary>
        /// "剩余期限（包括含权期限，是否休假）"、"债券代码（.IB .SH .SZ）"、"债券简称"、 "价格（bid/ofr，--/ofr）（"全""净"）"、"券面（万为单位，如 3000，3000/3000）"、"中债估值 （含权债默认为含权估值"权"）"
        /// </summary>
        /// <param name="dg"></param>
        public void CopyData_Best(bool needPrompt = true, string copyMode = "1")
        {
            if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
            {
                copyMode = "2";
            }
            else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
              && CommonUtils.CopyDic["COPYMODE"].Count > 0
              && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
            {
                copyMode = "3";
            }
            else
            {
                copyMode = "1";
            }

            Sub_CopyData_Best(this.SelectedItems as IList, needPrompt, copyMode);
            Sub_CopyData_Best_YC(this.SelectedItems as IList, needPrompt, copyMode);
        }

        private bool IsContain(string type, string columnName)
        {
            if (CommonUtils.CopyDic.ContainsKey(type)
                && CommonUtils.CopyDic[type].Any(item => item.Name == columnName))
            {
                return CommonUtils.CopyDic[type].First(item => item.Name == columnName).IsChecked;
            }
            else
            {
                if (columnName == "中债估值"
                    || columnName == "地区"
                    || columnName == "票面")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string ConvertToDateTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr) == false && dateStr.Length == 8)
            {
                string year = dateStr.Substring(0, 4);
                string month = dateStr.Substring(4, 2);
                string day = dateStr.Substring(6, 2);
                return $"{year}-{month}-{day}";
            }
            return string.Empty;
        }
    }
}
