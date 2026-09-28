using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mmshe.Plugin.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Mmshe.Plugin.ViewModel
{
    public class PriceCalHelper
    {
        private static PriceCalHelper proc = new PriceCalHelper();

        public static PriceCalHelper Instance
        {
            get
            {
                return proc;
            }
        }

        private PriceCalHelper()
        { }

        ////public void InitTableDataByBroker(DataView dv, string broker)
        ////{
        ////    for (int i = 0; i < 3; i++)
        ////    {
        ////        string showSettle = string.Empty;
        ////        string key = string.Empty;
        ////        if (i == 0)
        ////        {
        ////            key = string.Format("{0}{1}", broker, Constants.T1_EN);
        ////            showSettle = Constants.T1_EN;
        ////        }
        ////        else if (i == 1)
        ////        {
        ////            key = string.Format("{0}{1}", broker, Constants.T2_EN);
        ////            showSettle = Constants.T2_EN;
        ////        }
        ////        else if (i == 2)
        ////        {
        ////            key = string.Format("{0}{1}", broker, Constants.FWD);
        ////            showSettle = Constants.FWD;
        ////        }

        ////        DataRow findDr = dv.Table.NewRow();
        ////        findDr[PriceViewModel.KEY_FIELD] = key;
        ////        findDr[PriceViewModel.SHOW_SETTLE_TYPE_FILED] = showSettle;
        ////        findDr[PriceViewModel.SHOW_SETTLE_COLUMN_FLAG_FILED] = "1";
        ////        findDr[PriceViewModel.SOURCE_FIELD] = broker;

        ////        findDr[PriceViewModel.TRADE_PERMISSION_FLAG_FIELD] = PermisionHandler.Instance.CheckTradePermission(broker) ? "1" : string.Empty;

        ////        findDr[PriceViewModel.BID_FIELD] = CommonConstants.NO_PRICE;
        ////        findDr[PriceViewModel.BID_AMOUNT_FIELD] = CommonConstants.NO_PRICE;

        ////        findDr[PriceViewModel.OFR_FIELD] = CommonConstants.NO_PRICE;
        ////        findDr[PriceViewModel.OFR_AMOUNT_FIELD] = CommonConstants.NO_PRICE;

        ////        dv.Table.Rows.Add(findDr);
        ////    }
        ////}


        ////public void InitTableDataByPermission(DataView dv, List<string> filterBroker)
        ////{
        ////    try
        ////    {
        ////        List<string> sortList = PermisionHandler.Instance.GetBroker();

        ////        int sortIndex = 1;
        ////        foreach (string str in sortList)
        ////        {
        ////            string venue = ComboSourceUtil.Instance.FindEnum(str, "venues_en");
        ////            if (filterBroker == null || (filterBroker != null && filterBroker.Contains(venue)))
        ////            {
        ////                //lb.Content = ComboSourceUtil.Instance.FindEnum(str, "venues_cn");

        ////                string key = venue;
        ////                DataRow findDr = dv.Table.NewRow();
        ////                findDr[PriceViewModel.KEY_FIELD] = key;

        ////                findDr[PriceViewModel.SOURCE_FIELD] = venue;

        ////                findDr[PriceViewModel.TRADE_PERMISSION_FLAG_FIELD] = PermisionHandler.Instance.CheckTradePermission(venue) ? "1" : string.Empty;



        ////                findDr[PriceViewModel.SORT_FIELD] = sortIndex;//ComboSourceUtil.Instance.FindEnum(str, "venues_en_tonumber");
        ////                sortIndex++;

        ////                findDr[PriceViewModel.BID_FIELD] = CommonConstants.NO_PRICE;
        ////                findDr[PriceViewModel.BID_AMOUNT_FIELD] = CommonConstants.NO_PRICE;

        ////                findDr[PriceViewModel.OFR_FIELD] = CommonConstants.NO_PRICE;
        ////                findDr[PriceViewModel.OFR_AMOUNT_FIELD] = CommonConstants.NO_PRICE;


        ////                dv.Table.Rows.Add(findDr);
        ////            }
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        LogHelper.Error(ex);
        ////    }
        ////}

        ////public BilateralBestPrice CalBestPrice(List<BilateralBestPrice> bilateralBestList)
        ////{
        ////    BilateralBestPrice result = new BilateralBestPrice();

        ////    BestPrice1 bidBestPrice = null;
        ////    BestPrice1 ofrBestPrice = null;

        ////    decimal newD = 0;
        ////    decimal oldD = 0;
        ////    foreach (BilateralBestPrice bestPrice in bilateralBestList)
        ////    {
        ////        if ((bidBestPrice == null && decimal.TryParse(bestPrice.BidBest.bestPx, out newD))
        ////                || (bidBestPrice != null && decimal.TryParse(bestPrice.BidBest.bestPx, out newD) && decimal.TryParse(bidBestPrice.bestPx, out oldD) && newD < oldD))
        ////            bidBestPrice = bestPrice.BidBest;

        ////        if ((ofrBestPrice == null && decimal.TryParse(bestPrice.OfrBest.bestPx, out newD))
        ////                || (ofrBestPrice != null && decimal.TryParse(bestPrice.OfrBest.bestPx, out newD) && decimal.TryParse(ofrBestPrice.bestPx, out oldD) && newD > oldD))
        ////            ofrBestPrice = bestPrice.OfrBest;
        ////    }

        ////    int bidQty = 0;
        ////    int ofrQty = 0;
        ////    foreach (BilateralBestPrice temPrice in bilateralBestList)
        ////    {
        ////        if (temPrice.BidBest != null && bidBestPrice != null && bidBestPrice.bestPx == temPrice.BidBest.bestPx)
        ////            bidQty += temPrice.BidBest.bestQty;

        ////        if (temPrice.OfrBest != null && ofrBestPrice != null && ofrBestPrice.bestPx == temPrice.OfrBest.bestPx)
        ////            ofrQty += temPrice.OfrBest.bestQty;
        ////    }

        ////    if (bidBestPrice != null)
        ////        bidBestPrice.bestQty = bidQty;
        ////    if (ofrBestPrice != null)
        ////        ofrBestPrice.bestQty = ofrQty;

        ////    result.BidBest = bidBestPrice;
        ////    result.OfrBest = ofrBestPrice;
            

        ////    return result;
        ////}


        /////// <summary>
        /////// 是否倒挂
        /////// </summary>
        /////// <returns></returns>
        ////public bool IsUpSide(BilateralBestPrice bestPrice)
        ////{
        ////    //计算倒挂
        ////    if (bestPrice != null)
        ////    {
        ////        if (bestPrice.BidBest != null && bestPrice.OfrBest != null && bestPrice.BidBest.bestPx != null && bestPrice.OfrBest.bestPx != null
        ////            && CommonUtils.IsNumeric(bestPrice.BidBest.bestPx) && CommonUtils.IsNumeric(bestPrice.OfrBest.bestPx))
        ////        {
        ////            decimal bestBidPx = CommonUtils.ConvertToDecimal(bestPrice.BidBest.bestPx);
        ////            decimal bestOfrPx = CommonUtils.ConvertToDecimal(bestPrice.OfrBest.bestPx);

        ////            if (bestBidPx <= bestOfrPx)
        ////            {
        ////                return true;
        ////            }
        ////            else
        ////            {
        ////                return false;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            return false;
        ////        }
        ////    }
        ////    else
        ////        return false;
        ////}


        public bool CheckFwd(List<MarketDataSnapshotFullRefresh> marketData)
        {
            bool result = false;

            foreach (MarketDataSnapshotFullRefresh md in marketData)
            {
                foreach (GNoMDEntries noMDEntries in md.NoMDEntries)
                {
                    if (IsFwd(noMDEntries.SettlType))
                    {
                        result = true;

                        break;
                    }
                }
            }

            return result;
        }

        public string GetKey(string venues, string price)
        {
            return string.Format("{0}-{1}", venues, price);
        }

        public bool IsFwd(string settle)
        {
            return !string.IsNullOrEmpty(settle) && !settle.Equals(CommonConstants.T1_ONE) && !CommonConstants.T2_T3.Contains(settle) && settle.Contains("+");
        }

        public string GetShowSettle(IList settlTypeList)
        {
            string showSettle = string.Empty;
            int index = 0;
            foreach (ListBoxItem settlType in settlTypeList)
            {
                string tem = settlType.Content.ToString();

                if (tem == CommonConstants.ALL_PARTY)
                {
                    showSettle = tem;
                    break;
                }
                else if (tem == MMsheConstants.T1_EN)
                {
                    showSettle = tem;
                    index++;
                }
                else if (tem == MMsheConstants.T2_EN)
                {
                    showSettle = tem;
                    index++;
                }
                else if (tem == MMsheConstants.FWD)
                {
                    showSettle = tem;
                    index++;
                }
            }

            if (index == 3)
                showSettle = CommonConstants.ALL_PARTY;

            return showSettle;
        }

        public string RemoveZero(string num)
        {
            string result = num;

            if (result.Contains(".") && result.EndsWith("0"))
            {
                result = result.Substring(0, result.Length - 1);

                result = RemoveZero(result);
            }

            if (result.EndsWith("."))
                result = result.Substring(0, result.Length - 1);

            return result;
        }

        public string FormatBestBP(decimal bestBP)
        {
            string result = string.Format("{0}", CommonUtils.formatString(bestBP.ToString(), 2));

            if (result.Contains("."))
            {
                result = RemoveZero(result);
            }
            result = string.Format("{0}bp", result);
            if (result.Length > 6)
            {
                result = result.Substring(0, result.Length - 2);
            }

            if (result.Length > 6)
            {
                decimal d = 0;
                if (decimal.TryParse(result, out d))
                {
                    result = d > 9999 ? "9999" : d < -9999 ? "-9999" : result;
                }
            }

            return result;
        }
        

        public string CalSurplusQty(string qty, string dealQty)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(dealQty))
            {
                result = qty;
            }
            else
            {
                decimal qtyD = 0;
                decimal dealQtyD = 0;
                if (decimal.TryParse(qty, out qtyD) && decimal.TryParse(dealQty, out dealQtyD))
                    result = (qtyD - dealQtyD).ToString();
            }

            return result;
        }

        public Dictionary<string, IList> CopyDictionary(Dictionary<string, IList> oldDic)
        {
            Dictionary<string, IList> copyDic = new Dictionary<string, IList>();
            if (oldDic == null)
                return copyDic;

            foreach (KeyValuePair<string, IList> kv in oldDic)
            {
                string key = kv.Key;

                copyDic.Add(key, CopyList(kv.Value));
            }

            return copyDic;
        }

        public IList CopyList(IList list)
        {
            List<object> result = new List<object>();

            foreach (object obj in list)
            {
                result.Add(obj);
            }

            return result;
        }

    }
}
