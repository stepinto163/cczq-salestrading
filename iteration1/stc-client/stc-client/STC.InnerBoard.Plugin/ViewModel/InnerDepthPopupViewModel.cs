using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.InnerBoard.Plugin.ViewModel
{
    public class InnerDepthPopupViewModel : ViewModelBase
    {
        private object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private ConcurrentDictionary<string,CustomCollection<InnerOrderData>> _depthOrdersDic = new ConcurrentDictionary<string,CustomCollection<InnerOrderData>>();

        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set {
                if (_securityID != value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        PriceHandler.Instance.RegisterCTPrice(value, UpdateCtPrice);
                    }
                    _securityID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private void UpdateCtPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh price)
        {
            GetBestPrice(price);


            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (_depthOrdersDic.ContainsKey(price.SecurityID))
                    {

                        CustomCollection<InnerOrderData> datas = _depthOrdersDic[price.SecurityID];
                        foreach (InnerOrderData data in datas)
                        {
                            data.Best_Bid = _bestPriceDic[price.SecurityID][0];
                            data.Best_Ofr = _bestPriceDic[price.SecurityID][1];
                            if (data.Price_InnerBid.HasValue && (data.Best_Bid == null || data.Best_Bid >= data.Price_InnerBid) && datas[0].Price_InnerBid == data.Price_InnerBid)
                            {
                                data.IsMarketBest_InnerBid = "1"; // 市场最优，大拇指
                                }
                            else
                            {
                                data.IsMarketBest_InnerBid = "0";
                            }

                            if (data.Price_InnerOfr.HasValue && (data.Best_Ofr == null || data.Best_Ofr <= data.Price_InnerOfr) && datas[0].Price_InnerOfr == data.Price_InnerOfr)
                            {
                                data.IsMarketBest_InnerOfr = "1"; // 市场最优，大拇指
                                }
                            else
                            {
                                data.IsMarketBest_InnerOfr = "0";
                            }
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// key:securityid; value:bestPrices 0:bestBid 1:bestOfr 2:sencodBestBid 3:sencondBestOfr
        /// </summary>
        ConcurrentDictionary<string, decimal?[]> _bestPriceDic = new ConcurrentDictionary<string, decimal?[]>();

        private void GetBestPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh price)
        {
            if (!_bestPriceDic.ContainsKey(price.SecurityID))
            {
                _bestPriceDic[price.SecurityID] = new decimal?[2];
            }
            else
            {
                _bestPriceDic[price.SecurityID][0] = null;
                _bestPriceDic[price.SecurityID][1] = null;
            }

            // 剔除
            var bestPrices_best_bid = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "0").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_bid != null && bestPrices_best_bid.Count() > 0)
            {
                _bestPriceDic[price.SecurityID][0] = bestPrices_best_bid.First().MDEntryPx.ConvertToDecimal();
            }

            var bestPrices_best_ofr = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "1").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_ofr != null && bestPrices_best_ofr.Count() > 0)
            {
                _bestPriceDic[price.SecurityID][1] = bestPrices_best_ofr.First().MDEntryPx.ConvertToDecimal();
            }

            //var bestPrices = price.NoMDEntries?.Where(item => item.MDPriceLevel == "1");
            //foreach (GNoMDEntries entry in bestPrices)
            //{
            //    if (entry.MDEntryType == "0") // bid
            //    {
            //        _bestPriceDic[price.SecurityID][0] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //    else if (entry.MDEntryType == "1") // ofr
            //    {
            //        _bestPriceDic[price.SecurityID][1] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //}
        }


        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set { 
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private InnerOrderData _selectedDepthData;

        public InnerOrderData SelectedDepthData
        {
            get { return _selectedDepthData; }
            set {
                _selectedDepthData = value;
                this.OnPropertyChanged();
            }
        }
       
        /// <summary>
        /// 最小：2行 32 + 32 + 4 = 100
        /// 最大：7行 32 + 224 + 4 = 260
        /// </summary>
        private int _viewHeight = 260;

        public int ViewHeight
        {
            get { return _viewHeight; }
            set 
            {
                if (_viewHeight != value)
                {
                    _viewHeight = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public InnerDepthPopupViewModel()
        {
        }

        ~InnerDepthPopupViewModel()
        {
        }

        internal void UnfilterData()
        {
            ViewSource.Source = null;
            this.SecurityID = null;
            this.Symbol = null;
        }

        internal void FilterData()
        {
            if (_depthOrdersDic.ContainsKey(SecurityID))
            {
                if (ViewSource.Source != _depthOrdersDic[SecurityID])
                {
                    ViewSource.Source = _depthOrdersDic[SecurityID];

                    int count = 0;
                    if (_depthOrdersDic[SecurityID] != null)
                    {
                        count = _depthOrdersDic[SecurityID].Count;
                    }

                    if (count <= 2)
                    {
                        this.ViewHeight = 100;
                    }
                    else if (count < 7)
                    {
                        this.ViewHeight = 32 + 32 * count + 4;
                    }
                    else
                    {
                        this.ViewHeight = 260;
                    }
                }
                else
                {
                    int count = 0;
                    if (_depthOrdersDic[SecurityID] != null)
                    {
                        count = _depthOrdersDic[SecurityID].Count;
                    }

                    if (count <= 2)
                    {
                        this.ViewHeight = 100;
                    }
                    else if (count < 7)
                    {
                        this.ViewHeight = 32 + 32 * count + 4;
                    }
                    else
                    {
                        this.ViewHeight = 260;
                    }
                }
            }
            else
            {
                ViewSource.Source = null;
                this.ViewHeight = 68;
            }
        }

        internal void OnInnerDepthCallback(string securityID, IEnumerable<InnerOrderData> bid_InnerOrdersList, IEnumerable<InnerOrderData> ofr_InnerOrdersList)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_OnInnerDepthCallback(securityID, bid_InnerOrdersList, ofr_InnerOrdersList);
                    if (this.SecurityID == securityID)
                    {
                        FilterData();
                    }
                }));
            }
        }

        private void Sub_OnInnerDepthCallback(string securityID, IEnumerable<InnerOrderData> bid_InnerOrdersList, IEnumerable<InnerOrderData> ofr_InnerOrdersList)
        {
            int bidCount = 0; 
            if (bid_InnerOrdersList != null)
            {
                bidCount = bid_InnerOrdersList.Count();
            }

            int ofrCount = 0;
            if (ofr_InnerOrdersList != null)
            {
                ofrCount = ofr_InnerOrdersList.Count();
            }

            if (bidCount == 0 && ofrCount == 0)
            {
                if (_depthOrdersDic.ContainsKey(securityID))
                {
                    _depthOrdersDic[securityID].Clear();
                }
            }
            else
            {
                if (!_depthOrdersDic.ContainsKey(securityID))
                {
                    _depthOrdersDic[securityID] = new CustomCollection<InnerOrderData>();
                }

                CustomCollection<InnerOrderData> collection = _depthOrdersDic[securityID];

                int oldCount = collection.Count;

                int maxCount = Math.Max(bidCount, ofrCount);
                if (oldCount > maxCount)
                {
                    for (int j = oldCount - 1; j > maxCount - 1; j--)
                    {
                        collection.RemoveAt(j);
                    }
                }
                else if (oldCount < maxCount)
                {
                    for (int j = 0; j < maxCount - oldCount; j++)
                    {
                        collection.Add(new InnerOrderData());
                    }
                }


                for (int i = 0; i < maxCount; i++)
                {
                    if (i < bidCount)
                    {
                        InnerOrderData bidData = bid_InnerOrdersList.ElementAt(i);

                        collection[i].SecurityID = securityID;
                        collection[i].Count_InnerBid = 1;
                        collection[i].FirmFlag_InnerBid = bidData.FirmFlag;
                        collection[i].IntentionId_InnerBid = bidData.IntentionID;
                        //collection[i].IsBest_InnerBid = string.Empty;
                        collection[i].IsMe_InnerBid = bidData.IsMe;
                        collection[i].NetPrice_InnerBid = bidData.NetPrice;
                        collection[i].FullPrice_InnerBid = bidData.FullPrice;
                        collection[i].PartySubName_InnerBid = bidData.PartySubName;
                        collection[i].PartySubId_InnerBid = bidData.PartySubId;
                        collection[i].UserName_InnerBid = bidData.UserName;
                        collection[i].Client_InnerBid = bidData.Client;
                        //collection[i].PartySubName_Tooltip_InnerBid = string.Empty;
                        collection[i].Price_InnerBid = bidData.Price;
                        collection[i].Qty_InnerBid = bidData.Qty;
                        collection[i].CopyInfo_InnerBid = bidData.CopyInfo;
                        //collection[i].Qty_Tooltip_InnerBid = string.Empty;
                        collection[i].StrategyOrderId_innerBid = bidData.StrategyOrderId;
                        collection[i].RoleId_InnerBid = bidData.RoleId;
                        collection[i].RoleName_InnerBid = bidData.RoleName;
                        collection[i].SettleType_InnerBid = bidData.SettleType;
                        collection[i].WholeFlag_InnerBid = bidData.WholeFlag;
                        collection[i].DQType_InnerBid = bidData.DQType;
                        collection[i].Best_Bid = bidData.Best_Bid;
                        collection[i].UpdateTime_InnerBid = bidData.UpdateTime;
                        collection[i].SecurityType = bidData.SecurityType;
                        collection[i].BondSubType = bidData.BondSubType;
                        collection[i].RemainingTime = bidData.RemainingTime;
                        collection[i].DayOffStr = bidData.DayOffStr;
                        collection[i].CouponRateCurrent = bidData.CouponRateCurrent;
                        collection[i].ChinaBondValuation = bidData.ChinaBondValuation;
                        collection[i].ChinaBondValuationFull = bidData.ChinaBondValuationFull;
                        collection[i].ChinaBondExercise = bidData.ChinaBondExercise;
                        collection[i].ChinaBondDue = bidData.ChinaBondDue;
                        collection[i].CsiBondValuation = bidData.CsiBondValuation;
                        collection[i].CsiBondValuationFull = bidData.CsiBondValuationFull;
                        collection[i].CsiBondValuationExercise = bidData.CsiBondValuationExercise;
                        collection[i].CsiBondValuationDue = bidData.CsiBondValuationDue;
                        collection[i].MaturtityDate = bidData.MaturtityDate;
                        collection[i].Rating = bidData.Rating;
                        collection[i].Cdc_Implited_Rating = bidData.Cdc_Implited_Rating;
                        collection[i].IssueType = bidData.IssueType;
                        collection[i].BondCategory = bidData.BondCategory;
                        collection[i].If_Guarantee = bidData.If_Guarantee;
                        collection[i].WarranterName = bidData.WarranterName;
                        collection[i].EnterpriseType = bidData.EnterpriseType;
                        collection[i].RegionType = bidData.RegionType;
                        collection[i].Level = bidData.Level;
                        collection[i].IssuerName = bidData.IssuerName;
                        collection[i].IssueAmount = bidData.IssueAmount;

                        if (bidData.Price.HasValue && (bidData.Price_InnerBid == null || bidData.Price_InnerBid >= bidData.Price) && collection[0].Price_InnerBid == bidData.Price)
                        {
                            collection[i].IsMarketBest_InnerBid = "1"; // 市场最优，大拇指
                        }
                        else
                        {
                            collection[i].IsMarketBest_InnerBid = "0";
                        }
                        collection[i].HurryFlag_InnerBid = bidData.HurryFlag;
                        collection[i].SuggestFlag_InnerBid = bidData.SuggestFlag;
                        collection[i].FireFlag_InnerBid = bidData.FireFlag;
                        collection[i].FloorPrice_InnerBid = bidData.FloorPrice;
                        collection[i].OCOFlag_InnerBid = bidData.OCOFlag;
                        collection[i].PublicRemarks_InnerBid = bidData.PublicRemarks;
                        collection[i].PersonalRemarks_InnerBid = bidData.IsMe ? bidData.PersonalRemarks : string.Empty;
                        collection[i].FromQQ_Bid = bidData.FromQQ;
                        collection[i].Channel_InnerBid = bidData.Channel;
                        collection[i].Info2_InnerBid = bidData.Info2;
                        collection[i].Info1_InnerBid = bidData.Info1;
                        collection[i].PriceType_InnerBid = bidData.PriceType;
                        collection[i].InstitutionID_InnerBid = bidData.InstitutionID;
                        collection[i].InstitutionName_InnerBid = bidData.InstitutionName;
                        collection[i].TraderID_InnerBid = bidData.TraderID;
                        collection[i].TraderName_InnerBid = bidData.TraderName;
                        if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                        {
                            collection[i].RiskInfo = bidData.RiskInfo;
                            collection[i].IsRisk = string.IsNullOrEmpty(bidData.RiskInfo) ? false : true;
                        }

                        if (bidData.PriceType == "1")
                        {
                            collection[i].Price_Display_InnerBid = bidData.NetPrice;
                        }
                        else if (bidData.PriceType == "2")
                        {
                            collection[i].Price_Display_InnerBid = bidData.FullPrice;
                        }
                        else 
                        {
                            collection[i].Price_Display_InnerBid = bidData.Price;
                        }
                    }
                    else
                    {
                        collection[i].SecurityID = securityID;
                        collection[i].Count_InnerBid = 0;
                        collection[i].FirmFlag_InnerBid = string.Empty;
                        collection[i].IntentionId_InnerBid = string.Empty;
                        //collection[i].IsBest_InnerBid = string.Empty;
                        collection[i].IsMe_InnerBid = false;
                        collection[i].NetPrice_InnerBid = null;
                        collection[i].FullPrice_InnerBid = null;
                        collection[i].PartySubName_InnerBid = string.Empty;
                        collection[i].PartySubId_InnerBid = string.Empty;
                        collection[i].UserName_InnerBid = string.Empty;
                        collection[i].Client_InnerBid = string.Empty;
                        //collection[i].PartySubName_Tooltip_InnerBid = string.Empty;
                        collection[i].Price_InnerBid = null;
                        collection[i].Qty_InnerBid = null;
                        collection[i].CopyInfo = null;
                        //collection[i].Qty_Tooltip_InnerBid = string.Empty;
                        collection[i].StrategyOrderId_innerBid = string.Empty;
                        collection[i].RoleId_InnerBid = string.Empty;
                        collection[i].RoleName_InnerBid = string.Empty;
                        collection[i].SettleType_InnerBid = string.Empty;
                        collection[i].WholeFlag_InnerBid = string.Empty;
                        collection[i].DQType_InnerBid = string.Empty;
                        collection[i].IsMarketBest_InnerBid = "0";
                        collection[i].Best_Bid = null;
                        collection[i].UpdateTime_InnerBid = string.Empty;
                        collection[i].HurryFlag_InnerBid = string.Empty;
                        collection[i].SuggestFlag_InnerBid = string.Empty;
                        collection[i].FireFlag_InnerBid = string.Empty;
                        collection[i].FloorPrice_InnerBid = string.Empty;
                        collection[i].OCOFlag_InnerBid = string.Empty;
                        collection[i].PublicRemarks_InnerBid = string.Empty;
                        collection[i].PersonalRemarks_InnerBid = string.Empty;
                        collection[i].FromQQ_Bid = string.Empty;
                        collection[i].Channel_InnerBid = string.Empty;
                        collection[i].Info2_InnerBid = string.Empty;
                        collection[i].Info1_InnerBid = string.Empty;
                        collection[i].PriceType_InnerBid = string.Empty;
                        collection[i].InstitutionID_InnerBid = string.Empty;
                        collection[i].InstitutionName_InnerBid = string.Empty;
                        collection[i].TraderID_InnerBid = string.Empty;
                        collection[i].TraderName_InnerBid = string.Empty;
                        collection[i].RiskInfo = string.Empty;
                        collection[i].IsRisk = false;
                        collection[i].Price_Display_InnerBid = null;
                    }

                    if (i < ofrCount)
                    {
                        InnerOrderData ofrData = ofr_InnerOrdersList.ElementAt(i);

                        collection[i].SecurityID = securityID;
                        collection[i].Count_InnerOfr = 1;
                        collection[i].FirmFlag_InnerOfr = ofrData.FirmFlag;
                        collection[i].IntentionId_InnerOfr = ofrData.IntentionID;
                        //collection[i].IsBest_InnerOfr = string.Empty;
                        collection[i].IsMe_InnerOfr = ofrData.IsMe;
                        collection[i].NetPrice_InnerOfr = ofrData.NetPrice;
                        collection[i].FullPrice_InnerOfr = ofrData.FullPrice;
                        collection[i].PartySubName_InnerOfr = ofrData.PartySubName;
                        collection[i].PartySubId_InnerOfr = ofrData.PartySubId;
                        collection[i].UserName_InnerOfr = ofrData.UserName;
                        collection[i].Client_InnerOfr = ofrData.Client;
                        //collection[i].PartySubName_Tooltip_InnerOfr = string.Empty;
                        collection[i].Price_InnerOfr = ofrData.Price;
                        collection[i].Qty_InnerOfr = ofrData.Qty;
                        collection[i].CopyInfo_InnerOfr = ofrData.CopyInfo;
                        //collection[i].Qty_Tooltip_InnerOfr = string.Empty;
                        collection[i].StrategyOrderId_InnerOfr = ofrData.StrategyOrderId;
                        collection[i].RoleId_InnerOfr = ofrData.RoleId;
                        collection[i].RoleName_InnerOfr = ofrData.RoleName;
                        collection[i].SettleType_InnerOfr = ofrData.SettleType;
                        collection[i].WholeFlag_InnerOfr = ofrData.WholeFlag;
                        collection[i].DQType_InnerOfr = ofrData.DQType;
                        collection[i].Best_Ofr = ofrData.Best_Ofr;
                        collection[i].UpdateTime_InnerOfr = ofrData.UpdateTime;
                        collection[i].SecurityType = ofrData.SecurityType;
                        collection[i].BondSubType = ofrData.BondSubType;
                        collection[i].RemainingTime = ofrData.RemainingTime;
                        collection[i].DayOffStr = ofrData.DayOffStr;
                        collection[i].CouponRateCurrent = ofrData.CouponRateCurrent;
                        collection[i].ChinaBondValuation = ofrData.ChinaBondValuation;
                        collection[i].ChinaBondValuationFull = ofrData.ChinaBondValuationFull;
                        collection[i].ChinaBondExercise = ofrData.ChinaBondExercise;
                        collection[i].ChinaBondDue = ofrData.ChinaBondDue;
                        collection[i].CsiBondValuation = ofrData.CsiBondValuation;
                        collection[i].CsiBondValuationFull = ofrData.CsiBondValuationFull;
                        collection[i].CsiBondValuationExercise = ofrData.CsiBondValuationExercise;
                        collection[i].CsiBondValuationDue = ofrData.CsiBondValuationDue;
                        collection[i].MaturtityDate = ofrData.MaturtityDate;
                        collection[i].Rating = ofrData.Rating;
                        collection[i].Cdc_Implited_Rating = ofrData.Cdc_Implited_Rating;
                        collection[i].IssueType = ofrData.IssueType;
                        collection[i].BondCategory = ofrData.BondCategory;
                        collection[i].If_Guarantee = ofrData.If_Guarantee;
                        collection[i].WarranterName = ofrData.WarranterName;
                        collection[i].EnterpriseType = ofrData.EnterpriseType;
                        collection[i].RegionType = ofrData.RegionType;
                        collection[i].Level = ofrData.Level;
                        collection[i].IssuerName = ofrData.IssuerName;
                        collection[i].IssueAmount = ofrData.IssueAmount;

                        if (ofrData.Price.HasValue && (collection[0].Best_Ofr == null || collection[0].Best_Ofr <= ofrData.Price) && collection[0].Price_InnerOfr == ofrData.Price)
                        {
                            collection[i].IsMarketBest_InnerOfr = "1"; // 市场最优，大拇指
                        }
                        else
                        {
                            collection[i].IsMarketBest_InnerOfr = "0";
                        }

                        collection[i].HurryFlag_InnerOfr = ofrData.HurryFlag;
                        collection[i].SuggestFlag_InnerOfr = ofrData.SuggestFlag;
                        collection[i].FireFlag_InnerOfr = ofrData.FireFlag;
                        collection[i].FloorPrice_InnerOfr = ofrData.FloorPrice;
                        collection[i].OCOFlag_InnerOfr = ofrData.OCOFlag;
                        collection[i].PublicRemarks_InnerOfr = ofrData.PublicRemarks;
                        collection[i].PersonalRemarks_InnerOfr = ofrData.IsMe ? ofrData.PersonalRemarks : string.Empty;
                        collection[i].FromQQ_Ofr = ofrData.FromQQ;
                        collection[i].Channel_InnerOfr = ofrData.Channel;
                        collection[i].Info2_InnerOfr = ofrData.Info2;
                        collection[i].Info1_InnerOfr = ofrData.Info1;
                        collection[i].PriceType_InnerOfr = ofrData.PriceType;
                        collection[i].InstitutionID_InnerOfr = ofrData.InstitutionID;
                        collection[i].InstitutionName_InnerOfr = ofrData.InstitutionName;
                        collection[i].TraderID_InnerOfr = ofrData.TraderID;
                        collection[i].TraderName_InnerOfr = ofrData.TraderName;

                        if (ofrData.PriceType == "1")
                        {
                            collection[i].Price_Display_InnerOfr = ofrData.NetPrice;
                        }
                        else if (ofrData.PriceType == "2")
                        {
                            collection[i].Price_Display_InnerOfr = ofrData.FullPrice;
                        }
                        else
                        {
                            collection[i].Price_Display_InnerOfr = ofrData.Price;
                        }
                    }
                    else
                    {
                        collection[i].SecurityID = securityID;
                        collection[i].Count_InnerOfr = 0;
                        collection[i].FirmFlag_InnerOfr = string.Empty;
                        collection[i].IntentionId_InnerOfr = string.Empty;
                        //collection[i].IsBest_InnerOfr = string.Empty;
                        collection[i].IsMe_InnerOfr = false;
                        collection[i].NetPrice_InnerOfr = null;
                        collection[i].FullPrice_InnerOfr = null;
                        collection[i].PartySubName_InnerOfr = string.Empty;
                        collection[i].PartySubId_InnerOfr = string.Empty;
                        collection[i].UserName_InnerOfr = string.Empty;
                        collection[i].Client_InnerOfr = string.Empty;
                        //collection[i].PartySubName_Tooltip_InnerOfr = string.Empty;
                        collection[i].Price_InnerOfr = null;
                        collection[i].Qty_InnerOfr = null;
                        collection[i].CopyInfo_InnerOfr = null;
                        //collection[i].Qty_Tooltip_InnerOfr = string.Empty;
                        collection[i].StrategyOrderId_InnerOfr = string.Empty;
                        collection[i].RoleId_InnerOfr = string.Empty;
                        collection[i].RoleName_InnerOfr = string.Empty;
                        collection[i].SettleType_InnerOfr = string.Empty;
                        collection[i].WholeFlag_InnerOfr = string.Empty;
                        collection[i].DQType_InnerOfr = string.Empty;
                        collection[i].IsMarketBest_InnerOfr = "0";
                        collection[i].Best_Ofr = null;
                        collection[i].UpdateTime_InnerOfr = string.Empty;

                        collection[i].HurryFlag_InnerOfr = string.Empty;
                        collection[i].SuggestFlag_InnerOfr = string.Empty;
                        collection[i].FireFlag_InnerOfr = string.Empty;
                        collection[i].FloorPrice_InnerOfr = string.Empty;
                        collection[i].OCOFlag_InnerOfr = string.Empty;
                        collection[i].PublicRemarks_InnerOfr = string.Empty;
                        collection[i].PersonalRemarks_InnerOfr = string.Empty;
                        collection[i].FromQQ_Ofr = string.Empty;
                        collection[i].Channel_InnerOfr = string.Empty;
                        collection[i].Info2_InnerOfr = string.Empty;
                        collection[i].Info1_InnerOfr = string.Empty;
                        collection[i].PriceType_InnerOfr = string.Empty;
                        collection[i].InstitutionID_InnerOfr = string.Empty;
                        collection[i].InstitutionName_InnerOfr = string.Empty;
                        collection[i].TraderID_InnerOfr = string.Empty;
                        collection[i].TraderName_InnerOfr = string.Empty;
                        collection[i].Price_Display_InnerOfr = null;
                    }
                }
            }
        }

        internal async void DealDelete()
        {
            List<InnerOrderData> refList = new List<InnerOrderData>();
            if (_depthOrdersDic.ContainsKey(this.SecurityID))
            {
                CustomCollection<InnerOrderData> collection = _depthOrdersDic[this.SecurityID];
                refList.AddRange(collection.Where(item => item.IsMe_InnerBid || item.IsMe_InnerOfr));
            }
            
            if (refList.Count == 0)
            {
                CommonUtils.Prompt2Second("REF", "无可撤销订单");
                return;
            }

            //LogHandler.Info($"DealDelete {orderId}");
            bool result = CustomMessageBox.Question("是否撤销本人在本券上的所有报价？", "REF");

            if (result == true)
            {
                StringBuilder errMsg = new StringBuilder();
                foreach (InnerOrderData innerOrder in refList)
                {
                    if (innerOrder.IsMe_InnerBid)
                    {
                        string msg = await CancelOne(innerOrder.StrategyOrderId_innerBid);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            errMsg.AppendLine(msg);
                        }
                    }
                       
                    if (innerOrder.IsMe_InnerOfr)
                    {
                        string msg = await CancelOne(innerOrder.StrategyOrderId_InnerOfr);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            errMsg.AppendLine(msg);
                        }
                    }
                }

                if (errMsg.Length > 0)
                {
                    CommonUtils.Prompt2Second("REF", errMsg.ToString());
                }
                else
                {
                    CommonUtils.Prompt2Second("REF", "REF成功");
                }
            }
        }

        private async Task<string> CancelOne(string orderid)
        {
            OrderCancelRequest cancelPo = new OrderCancelRequest();
            DataRow dr = MyOrderProcesser.Processer.GetOrderById(orderid);
            if (dr != null)
            {
                cancelPo.AlgoName = CommonUtils.ConvertToString(dr[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
            }

            cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
            cancelPo.OrderID = orderid;
            TBookCancelReplyPo reply = await CancelOrder(cancelPo);
            if (reply == null || !reply.IsSuccess())
            {
                return string.IsNullOrEmpty(reply.Msg) ? $"[{orderid}]撤销失败" : reply.Msg;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }
    }
}
