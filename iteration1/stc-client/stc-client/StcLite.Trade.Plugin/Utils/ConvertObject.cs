using BDB.Trade.Plugin.ViewModel.Trade.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Utils
{
    public class ConvertObject
    {
        public static void CopyPriceAndOrderViewModel(PriceAndOrderViewModel oldModel, PriceAndOrderViewModel newModel)
        {
            newModel.PriceModel.AllBrokerHeight = oldModel.PriceModel.AllBrokerHeight;
            newModel.BondCategory = oldModel.BondCategory;
            newModel.BondType = oldModel.BondType;
            newModel.PriceModel.BrokerHeight = oldModel.PriceModel.BrokerHeight;
            newModel.BrokerIndex = oldModel.BrokerIndex;
            newModel.CustomSort = oldModel.CustomSort;
            newModel.FiveBrokerPriceViewModel = CopyBrokerPriceViewModel(oldModel.FiveBrokerPriceViewModel);
            newModel.FourBrokerPriceViewModel = CopyBrokerPriceViewModel(oldModel.FourBrokerPriceViewModel);
            newModel.GroupId = oldModel.GroupId;
            newModel.Id = oldModel.Id;
            newModel.OneBrokerPriceViewModel = CopyBrokerPriceViewModel(oldModel.OneBrokerPriceViewModel);
            newModel.PriceModel = CopyPriceModel(oldModel.PriceModel);
            newModel.RemaingingTimeConvert = oldModel.RemaingingTimeConvert;
            newModel.SettleFilterDic = oldModel.SettleFilterDic;
            newModel.ThreeBrokerPriceViewModel = CopyBrokerPriceViewModel(oldModel.ThreeBrokerPriceViewModel);
            newModel.TwoBrokerPriceViewModel = CopyBrokerPriceViewModel(oldModel.TwoBrokerPriceViewModel);


            newModel.OwnUI = oldModel.OwnUI;
            
        }

        public static BrokerPriceViewModel CopyBrokerPriceViewModel(BrokerPriceViewModel oldView)
        {
            BrokerPriceViewModel result = new BrokerPriceViewModel();
            result.Bid = oldView.Bid;
            result.BidAmount = oldView.BidAmount;
            result.BidBargainFlag = oldView.BidBargainFlag;
            result.BidBgBrush = oldView.BidBgBrush;
            result.IsBidPermission = oldView.IsBidPermission;
            result.IsOfrPermission = oldView.IsOfrPermission;
            result.BidCornerMarket = oldView.BidCornerMarket;
            result.BidPCornerMarket = oldView.BidPCornerMarket;
            result.BidCornerMarketText = oldView.BidCornerMarketText;
            result.BidStarVisibility = oldView.BidStarVisibility;
            result.BidPStarVisibility = oldView.BidPStarVisibility;
            result.BidLineVisibility = oldView.BidLineVisibility;
            result.BidSettleType = oldView.BidSettleType;
            result.BidPriceType = oldView.BidPriceType;
            result.HeadVisibility = oldView.HeadVisibility;
            result.Ofr = oldView.Ofr;
            result.OfrAmount = oldView.OfrAmount;
            result.OfrBargainFlag = oldView.OfrBargainFlag;
            result.OfrBgBrush = oldView.OfrBgBrush;
            result.OfrCornerMarket = oldView.OfrCornerMarket;
            result.OfrPCornerMarket = oldView.OfrPCornerMarket;
            result.OfrCornerMarketText = oldView.OfrCornerMarketText;
            result.OfrStarVisibility = oldView.OfrStarVisibility;
            result.OfrPStarVisibility = oldView.OfrPStarVisibility;
            result.OfrLineVisibility = oldView.OfrLineVisibility;
            result.OfrSettleType = oldView.OfrSettleType;
            result.OfrPriceType = oldView.OfrPriceType;
            result.SettleColWidth = oldView.SettleColWidth;
            result.SettleFwdType = oldView.SettleFwdType;
            result.SettleType = oldView.SettleType;
            result.ShowSettleType = oldView.ShowSettleType;

            return result;
        }

        public static PriceModel CopyPriceModel(PriceModel priceModel)
        {
            PriceModel result = new PriceModel();

            result.AddBondVisibility = priceModel.AddBondVisibility;
            result.AllVisibility = priceModel.AllVisibility;
            result.AmountBrush = priceModel.AmountBrush;
            result.BestBid = priceModel.BestBid;
            result.BestBidAmount = priceModel.BestBidAmount;
            result.BestOfr = priceModel.BestOfr;
            result.BestOfrAmount = priceModel.BestOfrAmount;
            result.BestPriceBP = priceModel.BestPriceBP;
            result.BidArrowFill = priceModel.BidArrowFill;
            result.BidArrowStroke = priceModel.BidArrowStroke;
            result.BidArrowToolTxt = priceModel.BidArrowToolTxt;
            result.BidArrowVisibilly = priceModel.BidArrowVisibilly;
            result.BidBrokerBargainFlag = priceModel.BidBrokerBargainFlag;
            result.BidBrush = priceModel.BidBrush;
            result.BidCornerMarket = priceModel.BidCornerMarket;
            result.BidPrice = priceModel.BidPrice;
            result.BidTxt = priceModel.BidTxt;
            result.BondGridVisibility = priceModel.BondGridVisibility;
            result.BrokerVisibility = priceModel.BrokerVisibility;
            result.LastExecBpBrush = priceModel.LastExecBpBrush;
            result.LastExecPrice = priceModel.LastExecPrice;
            result.LastExecPriceBP = priceModel.LastExecPriceBP;
            result.LastExecPriceBP_ToolTip = priceModel.LastExecPriceBP_ToolTip;
            result.LastExecSide = priceModel.LastExecSide;
            result.LastExecSideBrush = priceModel.LastExecSideBrush;
            result.LastExecSideShow = priceModel.LastExecSideShow;
            result.LastExecSource = priceModel.LastExecSource;
            result.LastExecTime = priceModel.LastExecTime;
            result.OfrArrowFill = priceModel.OfrArrowFill;
            result.OfrArrowStroke = priceModel.OfrArrowStroke;
            result.OfrArrowToolTxt = priceModel.OfrArrowToolTxt;
            result.OfrArrowVisibilly = priceModel.OfrArrowVisibilly;
            result.OfrBrokerBargainFlag = priceModel.OfrBrokerBargainFlag;
            result.OfrBrush = priceModel.OfrBrush;
            result.OfrCornerMarket = priceModel.OfrCornerMarket;
            result.OfrPrice = priceModel.OfrPrice;
            result.OfrTxt = priceModel.OfrTxt;
            result.RemainingTime = priceModel.RemainingTime;
            result.SearchVisibility = priceModel.SearchVisibility;
            result.Security = priceModel.Security;
            result.SymbolName = priceModel.SymbolName;
            result.MarketIndicator = priceModel.MarketIndicator;
            result.SelectSettle = priceModel.SelectSettle;
            result.ShowFwdFlag = priceModel.ShowFwdFlag;
            result.SymbolName = priceModel.SymbolName;
            result.UpSideBdBrush = priceModel.UpSideBdBrush;
            result.UpSideBgBrush = priceModel.UpSideBgBrush;
            result.BellStatus = priceModel.BellStatus;


            return result;
        }
    }
}
