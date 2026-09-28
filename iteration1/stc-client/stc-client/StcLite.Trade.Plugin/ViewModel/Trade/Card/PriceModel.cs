using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class PriceModel : BaseViewModel
    {
        
        private Visibility _addBondVisibility = Visibility.Collapsed;
        public Visibility AddBondVisibility
        {
            get { return _addBondVisibility; }
            set { _addBondVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _searchVisibility = Visibility.Collapsed;
        public Visibility SearchVisibility
        {
            get { return _searchVisibility; }
            set { _searchVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _bondGridVisibility = Visibility.Collapsed;
        public Visibility BondGridVisibility
        {
            get { return _bondGridVisibility; }
            set { _bondGridVisibility = value; this.OnPropertyChanged(); }
        }


        private Brush _amountBrush;
        public Brush AmountBrush
        {
            get { return _amountBrush; }
            set { _amountBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _bidBrush;
        public Brush BidBrush
        {
            get { return _bidBrush; }
            set { _bidBrush = value; this.OnPropertyChanged(); }
        }

        private Brush _ofrBrush;
        public Brush OfrBrush
        {
            get { return _ofrBrush; }
            set { _ofrBrush = value; this.OnPropertyChanged(); }
        }


        //倒挂边框Brush
        private Brush _upSideBdBrush;
        public Brush UpSideBdBrush
        {
            get { return _upSideBdBrush; }
            set { _upSideBdBrush = value; this.OnPropertyChanged(); }
        }

        //倒挂背景Brush
        private Brush _upSideBgBrush;
        public Brush UpSideBgBrush
        {
            get { return _upSideBgBrush; }
            set { _upSideBgBrush = value; this.OnPropertyChanged(); }
        }

        private string _ToolTipTxt;
        public string ToolTipTxt
        {
            get { return _ToolTipTxt; }
            set { _ToolTipTxt = value; this.OnPropertyChanged(); }
        }

        private string _security;
        public string Security
        {
            get { return _security; }
            set 
            { 
                _security = value; this.OnPropertyChanged();
                ToolTipTxt = $"{this.Security}  {this.SymbolName}  {this.RemainingTime}";
            }
        }

        private string _bellStatus;

        public string BellStatus
        {
            get { return _bellStatus; }
            set
            {
                _bellStatus = value;
                this.OnPropertyChanged();
            }
        }

        private string marketIndicator;
        public string MarketIndicator
        {
            get { return marketIndicator; }
            set { marketIndicator = value; 
                this.OnPropertyChanged();
                if (value == "4")
                {
                    MarketIndicatorVisibility = Visibility.Visible;
                }
                else
                {
                    MarketIndicatorVisibility = Visibility.Collapsed;
                }
            }
        }

        private Visibility _marketIndicatorVisibility = Visibility.Collapsed;

        public Visibility MarketIndicatorVisibility
        {
            get { return _marketIndicatorVisibility; }
            set {
                _marketIndicatorVisibility = value;
                this.OnPropertyChanged();
            }
        }


        private string _symbolName;
        public string SymbolName
        {
            get { return _symbolName; }
            set 
            { 
                _symbolName = value; this.OnPropertyChanged();
                ToolTipTxt = $"{this.Security}  {this.SymbolName}  {this.RemainingTime}";
            }
        }

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set 
            { 
                _remainingTime = value; this.OnPropertyChanged();
                ToolTipTxt = $"{this.Security}  {this.SymbolName}  {this.RemainingTime}";
            }
        }

        private string _lastExecPrice = CommonConstants.NO_PRICE;
        public string LastExecPrice
        {
            get { return _lastExecPrice; }
            set { _lastExecPrice = value; this.OnPropertyChanged(); }
        }

        private string _lastExecPriceBP;
        public string LastExecPriceBP
        {
            get { return _lastExecPriceBP; }
            set { _lastExecPriceBP = value; this.OnPropertyChanged(); }
        }

        private string _lastExecPriceBP_ToolTip;
        public string LastExecPriceBP_ToolTip
        {
            get { return _lastExecPriceBP_ToolTip; }
            set { _lastExecPriceBP_ToolTip = value; this.OnPropertyChanged(); }
        }

        private Brush _lastExecBpBrush;
        public Brush LastExecBpBrush
        {
            get { return _lastExecBpBrush; }
            set { _lastExecBpBrush = value; this.OnPropertyChanged(); }
        }

        private string _lastExecSide;
        public string LastExecSide
        {
            get { return _lastExecSide; }
            set { _lastExecSide = value; this.OnPropertyChanged(); }
        }

        private string _lastExecSideShow;
        public string LastExecSideShow
        {
            get { return _lastExecSideShow; }
            set { _lastExecSideShow = value; this.OnPropertyChanged(); }
        }

        private Brush _lastExecSideBrush;
        public Brush LastExecSideBrush
        {
            get { return _lastExecSideBrush; }
            set { _lastExecSideBrush = value; this.OnPropertyChanged(); }
        }

        private string _lastExecSource;
        public string LastExecSource
        {
            get { return _lastExecSource; }
            set { _lastExecSource = value; this.OnPropertyChanged(); }
        }

        private string _lastExecTime;

        public string LastExecTime
        {
            get { return _lastExecTime; }
            set
            {
                _lastExecTime = value;
                this.OnPropertyChanged();
            }
        }

        private bool _showFwdFlag;
        public bool ShowFwdFlag
        {
            get { return _showFwdFlag; }
            set { _showFwdFlag = value; this.OnPropertyChanged(); }
        }

        private string _selectSettle;
        public string SelectSettle
        {
            get { return _selectSettle; }
            set { _selectSettle = value; this.OnPropertyChanged(); }
        }


        private string _bestBid = CommonConstants.NO_PRICE;
        public string BestBid
        {
            get { return _bestBid; }
            set { _bestBid = value; this.OnPropertyChanged(); }
        }

        private string _bestBidAmount;
        public string BestBidAmount
        {
            get { return _bestBidAmount; }
            set { _bestBidAmount = value; this.OnPropertyChanged(); }
        }

        private string _bestOfr = CommonConstants.NO_PRICE;
        public string BestOfr
        {
            get { return _bestOfr; }
            set { _bestOfr = value; this.OnPropertyChanged(); }
        }

        private string _bestOfrAmount;
        public string BestOfrAmount
        {
            get { return _bestOfrAmount; }
            set { _bestOfrAmount = value; this.OnPropertyChanged(); }
        }

        private string _bestPriceBP;
        public string BestPriceBP
        {
            get { return _bestPriceBP; }
            set { _bestPriceBP = value; this.OnPropertyChanged(); }
        }

        private Visibility _brokerVisibility = Visibility.Collapsed;
        public Visibility BrokerVisibility
        {
            get { return _brokerVisibility; }
            set { _brokerVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _allVisibility = Visibility.Visible;
        public Visibility AllVisibility
        {
            get { return _allVisibility; }
            set { _allVisibility = value; this.OnPropertyChanged(); }
        }

        private string _bidPrice;
        public string BidPrice { get { return _bidPrice; } set { _bidPrice = value; this.OnPropertyChanged(); } }

        private string _bidBrokerBargainFlag;
        public string BidBrokerBargainFlag { get { return _bidBrokerBargainFlag; } set { _bidBrokerBargainFlag = value; this.OnPropertyChanged(); } }

        private string _bidTxt;
        public string BidTxt { get { return _bidTxt; } set { _bidTxt = value; this.OnPropertyChanged(); } }

        private string _ofrPrice;
        public string OfrPrice { get { return _ofrPrice; } set { _ofrPrice = value; this.OnPropertyChanged(); } }

        private string _ofrBrokerBargainFlag;
        public string OfrBrokerBargainFlag { get { return _ofrBrokerBargainFlag; } set { _ofrBrokerBargainFlag = value; this.OnPropertyChanged(); } }


        private string _ofrTxt;
        public string OfrTxt { get { return _ofrTxt; } set { _ofrTxt = value; this.OnPropertyChanged(); } }

        private string _bidCornerMarket;
        public string BidCornerMarket { get { return _bidCornerMarket; } set { _bidCornerMarket = value; this.OnPropertyChanged(); } }

        private string _ofrCornerMarket;
        public string OfrCornerMarket { get { return _ofrCornerMarket; } set { _ofrCornerMarket = value; this.OnPropertyChanged(); } }

        private int _brokerConditionHeight = 24;

        public int BrokerConditionHeight { get { return _brokerConditionHeight; } set { _brokerConditionHeight = value; this.OnPropertyChanged(); } }
        private int _brokerPriceShowHeight = 156;

        public int BrokerPriceShowHeight { get { return _brokerPriceShowHeight; } set { _brokerPriceShowHeight = value; this.OnPropertyChanged(); } }


        private string bestBidVenue;
        public string BestBidVenue { get { return bestBidVenue; } set { bestBidVenue = value; this.OnPropertyChanged(); } }

        private string bestOfrVenue;
        public string BestOfrVenue { get { return bestOfrVenue; } set { bestOfrVenue = value; this.OnPropertyChanged(); } }


        private int _lastBrokerHeight = 26;

        public int LastBrokerHeight { get { return _lastBrokerHeight; } set { _lastBrokerHeight = value; this.OnPropertyChanged(); } }


        private int _brokerHeight = 26;

        public int BrokerHeight { get { return _brokerHeight; } set { _brokerHeight = value; this.OnPropertyChanged(); } }

        private int _allBrokerHeight = 26;

        public int AllBrokerHeight { get { return _allBrokerHeight; } set { _allBrokerHeight = value; this.OnPropertyChanged(); } }

        private int _bestPriceHeight = 40;
        public int BestPriceHeight { get { return _bestPriceHeight; } set { _bestPriceHeight = value; this.OnPropertyChanged(); } }
        


        #region 最优价角标显示

        private Brush _bidArrowStroke;
        public Brush BidArrowStroke
        {
            get { return _bidArrowStroke; }
            set { _bidArrowStroke = value; this.OnPropertyChanged(); }
        }

        private Brush _bidArrowFill;
        public Brush BidArrowFill
        {
            get { return _bidArrowFill; }
            set { _bidArrowFill = value; this.OnPropertyChanged(); }
        }

        private Visibility _bidArrowVisibilly = Visibility.Collapsed;
        public Visibility BidArrowVisibilly
        {
            get { return _bidArrowVisibilly; }
            set { _bidArrowVisibilly = value; this.OnPropertyChanged(); }
        }

        private string _bidArrowToolTxt;
        public string BidArrowToolTxt { get { return _bidArrowToolTxt; } set { _bidArrowToolTxt = value; this.OnPropertyChanged(); } }



        private Brush _ofrArrowStroke;
        public Brush OfrArrowStroke
        {
            get { return _ofrArrowStroke; }
            set { _ofrArrowStroke = value; this.OnPropertyChanged(); }
        }

        private Brush _ofrArrowFill;
        public Brush OfrArrowFill
        {
            get { return _ofrArrowFill; }
            set { _ofrArrowFill = value; this.OnPropertyChanged(); }
        }

        private Visibility _ofrArrowVisibilly = Visibility.Collapsed;
        public Visibility OfrArrowVisibilly
        {
            get { return _ofrArrowVisibilly; }
            set { _ofrArrowVisibilly = value; this.OnPropertyChanged(); }
        }


        private string _ofrArrowToolTxt;
        public string OfrArrowToolTxt { get { return _ofrArrowToolTxt; } set { _ofrArrowToolTxt = value; this.OnPropertyChanged(); } }


        #endregion

        #region Broker报价角标

        private Visibility _bidCornerMarketVisibility = Visibility.Collapsed;
        public Visibility BidCornerMarketVisibility
        {
            get { return _bidCornerMarketVisibility; }
            set { _bidCornerMarketVisibility = value; this.OnPropertyChanged(); }
        }

        private string _bidCornerMarketText;
        public string BidCornerMarketText { get { return _bidCornerMarketText; } set { _bidCornerMarketText = value; this.OnPropertyChanged(); } }

        private Visibility _ofrCornerMarketVisibility = Visibility.Collapsed;
        public Visibility OfrCornerMarketVisibility
        {
            get { return _ofrCornerMarketVisibility; }
            set { _ofrCornerMarketVisibility = value; this.OnPropertyChanged(); }
        }

        private string _ofrCornerMarketText;
        public string OfrCornerMarketText { get { return _ofrCornerMarketText; } set { _ofrCornerMarketText = value; this.OnPropertyChanged(); } }

        #endregion


        private Visibility _settleVisibility = Visibility.Visible;
        public Visibility SettleVisibility
        {
            get { return _settleVisibility; }
            set { _settleVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _popVisibility = Visibility.Visible;
        public Visibility PopVisibility
        {
            get { return _popVisibility; }
            set { _popVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility _editVisibility = Visibility.Collapsed;
        public Visibility EditVisibility
        {
            get { return _editVisibility; }
            set { _editVisibility = value; this.OnPropertyChanged(); }
        }


        private Visibility _allBidSettleVisibility = Visibility.Collapsed;
        public Visibility AllBidSettleVisibility
        {
            get { return _allBidSettleVisibility; }
            set { _allBidSettleVisibility = value; this.OnPropertyChanged(); }
        }

        private string _allBidSettleTip;
        public string AllBidSettleTip { get { return _allBidSettleTip; } set { _allBidSettleTip = value; this.OnPropertyChanged(); } }

        private Visibility _allOfrSettleVisibility = Visibility.Collapsed;
        public Visibility AllOfrSettleVisibility
        {
            get { return _allOfrSettleVisibility; }
            set { _allOfrSettleVisibility = value; this.OnPropertyChanged(); }
        }

        private string _allOfrSettleTip;
        public string AllOfrSettleTip { get { return _allOfrSettleTip; } set { _allOfrSettleTip = value; this.OnPropertyChanged(); } }


        private string bidSettleTypeCopy;

        public string BidSettleTypeCopy
        {
            get { return bidSettleTypeCopy; }
            set
            {
                if (bidSettleTypeCopy != value)
                {
                    bidSettleTypeCopy = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string ofrSettleTypeCopy;

        public string OfrSettleTypeCopy
        {
            get { return ofrSettleTypeCopy; }
            set
            {
                if (ofrSettleTypeCopy != value)
                {
                    ofrSettleTypeCopy = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string indexTxt;
        public string IndexTxt { get { return indexTxt; } set { indexTxt = value; this.OnPropertyChanged(); } }

        private string indexTxt2;
        public string IndexTxt2 { get { return indexTxt2; } set { indexTxt2 = value; this.OnPropertyChanged(); } }

        #region 债券OHLC

        private string cdcValue = CommonConstants.NO_PRICE;
        public string CDCValue { get { return cdcValue; } set { cdcValue = value; this.OnPropertyChanged(); } }

        private string high = CommonConstants.NO_PRICE;
        public string High { get { return high; } set { high = value; this.OnPropertyChanged(); } }

        private string preClose = CommonConstants.NO_PRICE;
        public string PreClose { get { return preClose; } set { preClose = value; this.OnPropertyChanged(); } }

        private string low = CommonConstants.NO_PRICE;
        public string Low { get { return low; } set { low = value; this.OnPropertyChanged(); } }

        private string csiValue = CommonConstants.NO_PRICE;
        public string CsiValue { get { return csiValue; } set { csiValue = value; this.OnPropertyChanged(); } }

        private Visibility futureVisibility = Visibility.Hidden;

        public Visibility FutureVisibility
        {
            get { return futureVisibility; }
            set { futureVisibility = value; this.OnPropertyChanged(); }
        }

        private Visibility ohlcVisibility = Visibility.Visible;
        public Visibility OHLCVisibility
        {
            get { return ohlcVisibility; }
            set { ohlcVisibility = value; this.OnPropertyChanged(); }
        }

        #endregion

        private Visibility futureRealDataVisibility = Visibility.Collapsed;
        public Visibility FutureRealDataVisibility
        {
            get { return futureRealDataVisibility; }
            set { futureRealDataVisibility = value; this.OnPropertyChanged(); }
        }


        #region 国债期货

        private Visibility _symbolNameVisibilly = Visibility.Visible;
        public Visibility SymbolNameVisibilly
        {
            get { return _symbolNameVisibilly; }
            set { _symbolNameVisibilly = value; this.OnPropertyChanged(); }
        }

        //今开
        private string openPrice;
        public string OpenPrice { get { return openPrice; } set { openPrice = value; this.OnPropertyChanged(); } }
        //今持
        private string openInterest;
        public string OpenInterest { get { return openInterest; } set { openInterest = value; this.OnPropertyChanged(); } }

        //金额
        private string turnover;
        public string Turnover { get { return turnover; } set { turnover = value; this.OnPropertyChanged(); } }

        //昨持
        private string preOpenInterest;
        public string PreOpenInterest { get { return preOpenInterest; } set { preOpenInterest = value; this.OnPropertyChanged(); } }

        //昨结
        private string preSettlePrice;
        public string PreSettlePrice { get { return preSettlePrice; } set { preSettlePrice = value; this.OnPropertyChanged(); } }

        //累计成交
        private string sumExec;
        public string SumExec { get { return sumExec; } set { sumExec = value; this.OnPropertyChanged(); } }


        private Visibility _indexVisibility = Visibility.Collapsed;
        public Visibility IndexVisibility
        {
            get { return _indexVisibility; }
            set { _indexVisibility = value; this.OnPropertyChanged(); }
        }


        private string codeColWith = "120*";
        public string CodeColWith { get { return codeColWith; } set { codeColWith = value; this.OnPropertyChanged(); } }

        private string symbolNameColWith = "90*";
        public string SymbolNameColWith { get { return symbolNameColWith; } set { symbolNameColWith = value; this.OnPropertyChanged(); } }

        private string maturityColWith = "70*";
        public string MaturityColWith { get { return maturityColWith; } set { maturityColWith = value; this.OnPropertyChanged(); } }
        #endregion


        private string ccdcvaluation = CommonConstants.NO_PRICE;
        public string CCDCValuation { get { return ccdcvaluation; } set { ccdcvaluation = value; this.OnPropertyChanged(); } }

        public void OnClear(string emptyStr)
        {
            this.BestBid = emptyStr;
            this.BestBidAmount = emptyStr;
            this.BestOfr = emptyStr;
            this.BestOfrAmount = emptyStr;
            this.BestPriceBP = emptyStr;
            this.BidArrowVisibilly = Visibility.Collapsed;
            this.BidArrowToolTxt = string.Empty;
            this.BidBrokerBargainFlag = string.Empty;
            this.BidCornerMarketVisibility = Visibility.Collapsed;
            this.BidPrice = emptyStr;
            this.BidTxt = string.Empty;
            this.LastExecPrice = emptyStr;
            this.LastExecPriceBP = string.Empty;
            this.LastExecPriceBP_ToolTip = string.Empty;
            this.LastExecSide = string.Empty;
            this.LastExecSource = string.Empty;
            this.LastExecTime = string.Empty;
            this.OfrArrowToolTxt = string.Empty;
            this.OfrArrowVisibilly = Visibility.Collapsed;
            this.OfrBrokerBargainFlag = string.Empty;
            this.OfrCornerMarket = string.Empty;
            this.OfrCornerMarketText = string.Empty;
            this.OfrCornerMarketVisibility = Visibility.Collapsed;
            this.OfrPrice = emptyStr;
            this.OfrTxt = string.Empty;

            this.BestBidVenue = string.Empty;
            this.BestOfrVenue = string.Empty;


            this.AllBidSettleTip = string.Empty;
            this.AllOfrSettleTip = string.Empty;

            this.AllBidSettleVisibility = Visibility.Collapsed;
            this.AllOfrSettleVisibility = Visibility.Collapsed;

            this.LastExecSideShow = string.Empty;



            this.CDCValue = CommonConstants.NO_PRICE;
            this.High = CommonConstants.NO_PRICE;
            this.Low = CommonConstants.NO_PRICE;
            this.PreClose = CommonConstants.NO_PRICE;
            this.CCDCValuation = "";
        }

    }
}
