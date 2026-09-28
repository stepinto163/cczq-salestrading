using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace STC.Common.ViewModel.Data
{
    /// <summary>
    /// 下单界面数据
    /// </summary>
    public class SendPriceItem : ViewModelBase
    {
        private string _customQty;
        public string CustomQty
        {
            get
            {
                return string.IsNullOrEmpty(_customQty) ? "" : $"{_customQty} Kw";
            }
            set
            {
                if (value != null)
                {
                    _customQty = value;

                    this.OnPropertyChanged();
                }
            }
        }

        private string _marketQty;
        public string MarketQty
        {
            get { return string.IsNullOrEmpty(_marketQty) ? "" : $"{_marketQty} KW"; }
            set
            {
                if (value != null)
                {
                    _marketQty = value;

                    this.OnPropertyChanged();
                }
            }
        }



        private string security;
        public string Security { get { return security; }
            set
            {
                if (value != null)
                {
                    security = value;

                    this.OnPropertyChanged();
                }
            }
        }

        private string symbolShortname;
        public string SymbolShortname
        {
            get { return symbolShortname; }
            set
            {
                if (value != null)
                {
                    symbolShortname = value;

                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        private decimal? bestBid;
        public decimal? BestBid
        {
            get { return bestBid; }
            set
            {
                bestBid = value;
                this.OnPropertyChanged();
            }
        }

        private string bestBidStr;
        public string BestBidStr
        {
            get
            {
                //if (bestBid.HasValue)
                //{
                //    bestBidStr = bestBid.Value.ToString("F" + Digit);
                //}
                //else
                //{
                //    bestBidStr = decimal.Zero.ToString("F" + Digit);
                //}               
                return bestBidStr;
            }
            set
            {
                bestBidStr = value;
                this.OnPropertyChanged();
            }
        }

        private string bestBidInt;
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        public string BestBidInt
        {
            get { return bestBidInt; }
            set
            {
                bestBidInt = value;
                this.OnPropertyChanged();
            }
        }

        private string bestBidFloat;
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        public string BestBidFloat
        {
            get { return bestBidFloat; }
            set
            {
                bestBidFloat = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? bestOfr;
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        public decimal? BestOfr
        {
            get { return bestOfr; }
            set
            {
                bestOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrStr;
        public string BestOfrStr
        {
            get
            {
                //if (bestOfr.HasValue)
                //{
                //    bestOfrStr = bestOfr.Value.ToString("F" + Digit);
                //}
                //else
                //{
                //    bestOfrStr = decimal.Zero.ToString("F" + Digit);
                //}
                return bestOfrStr;
            }
            set
            {
                bestOfrStr = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrInt;
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        public string BestOfrInt
        {
            get { return bestOfrInt; }
            set
            {
                bestOfrInt = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrFloat;
        /// <summary>
        /// 市场最优价 去掉INNER
        /// </summary>
        public string BestOfrFloat
        {
            get { return bestOfrFloat; }
            set
            {
                bestOfrFloat = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? bestBrokerOfr;
        public decimal? BestBrokerOfr
        {
            get { return bestBrokerOfr; }
            set
            {
                bestBrokerOfr = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? bestBrokerBid;
        public decimal? BestBrokerBid
        {
            get { return bestBrokerBid; }
            set
            {
                bestBrokerBid = value;
                this.OnPropertyChanged();
            }
        }

        #region
        private string firmMouseEnterIS;
        public string FirmMouseEnterIS
        {
            get { return firmMouseEnterIS; }
            set
            {
                if (value != null)
                {
                    firmMouseEnterIS = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string firmCheckIS;
        public string FirmCheckIS
        {
            get { return firmCheckIS; }
            set
            {
                if (value != null)
                {
                    firmCheckIS = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        private bool defaultItemEnable = false;
        public bool DefaultItemEnable
        {
            get { return defaultItemEnable; }
            set
            {
                defaultItemEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool directionItemEnable = false;
        public bool DirectionItemEnable
        {
            get { return directionItemEnable; }
            set
            {
                directionItemEnable = value;
                this.OnPropertyChanged();
            }
        }


        #region UI显示Brush

        private string bgImageSource;
        public string BgImageSource
        {
            get
            {
                return bgImageSource;
            }

            set
            {
                bgImageSource = value;

                this.OnPropertyChanged();

            }
        }


        private Brush bidBG;
        public Brush BidBG
        {
            get { return bidBG; }
            set
            {
                if (value != null)
                {
                    bidBG = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush wholeQuantityBG;
        public Brush WholeQuantityBG
        {
            get { return wholeQuantityBG; }
            set
            {
                if (value != null)
                {
                    wholeQuantityBG = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush ofrBG;
        public Brush OfrBG
        {
            get { return ofrBG; }
            set
            {
                if (value != null)
                {
                    ofrBG = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Effect showBidEffect;
        public Effect ShowBidEffect
        {
            get { return showBidEffect; }
            set
            {
                //if (value != null)
                {
                    showBidEffect = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Effect showOfrEffect;
        public Effect ShowOfrEffect
        {
            get { return showOfrEffect; }
            set
            {
                //if (value != null)
                {
                    showOfrEffect = value;
                    this.OnPropertyChanged();
                }
            }
        }
        

        private Brush borderBH;
        public Brush BorderBH
        {
            get { return borderBH; }
            set
            {
                if (value != null)
                {
                    borderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush qtyBorderBH;
        public Brush QtyBorderBH
        {
            get { return qtyBorderBH; }
            set
            {
                if (value != null)
                {
                    qtyBorderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush borderBG;
        public Brush BorderBG
        {
            get { return borderBG; }
            set
            {
                //if (value != null)
                {
                    borderBG = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush chatbuttonbg;
        public Brush ChatButtonBG
        {
            get { return chatbuttonbg; }
            set
            {
                chatbuttonbg = value;
                this.OnPropertyChanged();
            }
        }

        private Brush chatbuttonforeground;
        public Brush ChatButtonForeground
        {
            get { return chatbuttonforeground; }
            set
            {
                chatbuttonforeground = value;
                this.OnPropertyChanged();
            }
        }
        

        #endregion

        #region 字段展示 

        private Visibility _calWarnVisibility = Visibility.Collapsed;
        public Visibility CalWarnVisibility
        {
            get { return _calWarnVisibility; }
            set
            {
                _calWarnVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility netPriceVisible = Visibility.Collapsed;
        public Visibility NetPriceVisible
        {
            get { return netPriceVisible; }
            set
            {
                netPriceVisible = value;
                this.OnPropertyChanged();
            }
        }

        public string ShowSecurity
        {
            get { return string.Format("{0} {1}", security, symbolShortname); }
        }
        

        private Visibility firmVisibility = Visibility.Collapsed;
        public Visibility FirmVisibility
        {
            get { return firmVisibility; }
            set
            {
                firmVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility dateVisible = Visibility.Collapsed;
        public Visibility DateVisible
        {
            get { return dateVisible; }
            set
            {
                dateVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility settleTypeVisible = Visibility.Visible;
        public Visibility SettleTypeVisible
        {
            get { return dateVisible; }
            set
            {
                settleTypeVisible = value;
                this.OnPropertyChanged();
            }
        }
        
        private string randomNum;
        public string RandomNum
        {
            get { return randomNum; }
            set
            {
                if (value != null)
                {
                    randomNum = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string amount;
        public string Amount
        {
            get { return amount; }
            set
            {
                if (value != null)
                {
                    amount = value;
                    this.OnPropertyChanged(nameof(Amount));
                }
            }
        }

        private string customPrice;
        public string CustomPrice
        {
            get { return customPrice; }
            set
            {
                if (value != null)
                {
                    customPrice = value;
                    this.OnPropertyChanged(nameof(CustomPrice));
                }
            }
        }

        

        private string overTime;
        public string OverTime
        {
            get { return overTime; }
            set
            {
                if (value != null)
                {
                    overTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string customSettleDate;
        public string CustomSettleDate
        {
            get { return customSettleDate; }
            set
            {
                if (value != null)
                {
                    customSettleDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool wholeQuantitySwitch = false;

        public bool WholeQuantitySwitch
        {
            get
            {
                return wholeQuantitySwitch;
            }

            set
            {
                wholeQuantitySwitch = value;
                this.OnPropertyChanged();
            }
        }

        private bool appoint = false;

        public bool Appoint
        {
            get
            {
                return appoint;
            }

            set
            {
                appoint = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region listbox选中索引

        private int amountBoxIndex = 0;

        public int AmountBoxIndex
        {
            get
            {
                return amountBoxIndex;
            }

            set
            {
                amountBoxIndex = value;
                this.OnPropertyChanged();
            }
        }

        private string settleType;

        public string SettleType
        {
            get
            {
                return settleType;
            }

            set
            {
                settleType = value;
                this.OnPropertyChanged();
            }
        }

        private int channelIndex = 0;

        public int ChannelIndex
        {
            get
            {
                return channelIndex;
            }

            set
            {
                channelIndex = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private string nonfirmTime;

        public string NonfirmTime
        {
            get
            {
                return nonfirmTime;
            }

            set
            {
                nonfirmTime = value;
                this.OnPropertyChanged();
            }
        }

        //国债期货行情字段
        //最新价
        private decimal? lastPrice;
        public decimal? LastPrice
        {
            get { return lastPrice; }
            set
            {
                lastPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string lastPriceStr;
        public string LastPriceStr
        {
            get { return lastPriceStr; }
            set
            {
                lastPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //最新价涨跌
        private decimal? deltaLastPrice;
        public decimal? DeltaLastPrice
        {
            get { return deltaLastPrice; }
            set
            {
                deltaLastPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string deltaLastPriceStr;
        public string DeltaLastPriceStr
        {
            get { return deltaLastPriceStr; }
            set
            {
                deltaLastPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //一档Bid数量
        private int bestBidAmount;
        public int BestBidAmount
        {
            get { return bestBidAmount; }
            set
            {
                bestBidAmount = value;
                this.OnPropertyChanged();
            }
        }

        //一档Ofr数量
        private int bestOfrAmount;
        public int BestOfrAmount
        {
            get { return bestOfrAmount; }
            set
            {
                bestOfrAmount = value;
                this.OnPropertyChanged();
            }
        }

        //涨停价
        private decimal? upperLimitPrice;
        public decimal? UpperLimitPrice
        {
            get { return upperLimitPrice; }
            set
            {
                upperLimitPrice = value;
                this.OnPropertyChanged();
            }
        }

        //涨停价
        private string upperLimitPriceStr;
        public string UpperLimitPriceStr
        {
            get { return upperLimitPriceStr; }
            set
            {
                upperLimitPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //跌停价
        private decimal? lowerLimitPrice;
        public decimal? LowerLimitPrice
        {
            get { return lowerLimitPrice; }
            set
            {
                lowerLimitPrice = value;
                this.OnPropertyChanged();
            }
        }

        //跌停价
        private string lowerLimitPriceStr;
        public string LowerLimitPriceStr
        {
            get { return lowerLimitPriceStr; }
            set
            {
                lowerLimitPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //跟盘价CheckBox
        private bool followingPrice;
        public bool FollowingPrice
        {
            get { return followingPrice; }
            set
            {
                followingPrice = value;
                this.OnPropertyChanged();
            }
        }

        //跟盘价类型 1=Bid对价，2=Bid最新价，3=Bid市价,4=Ofr对价，5=Ofr最新价，6=Ofr市价
        private int priceType;
        public int PriceType
        {
            get { return priceType; }
            set
            {
                priceType = value;
                this.OnPropertyChanged();
            }
        }

        //持仓均价买
        private decimal? longAvgPrice;
        public decimal? LongAvgPrice
        {
            get { return longAvgPrice; }
            set
            {
                longAvgPrice = value;
                this.OnPropertyChanged();
            }
        }

        //持仓均价买
        private string longAvgPriceStr;
        public string LongAvgPriceStr
        {
            get { return longAvgPriceStr; }
            set
            {
                longAvgPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //持仓均价卖
        private decimal? shortAvgPrice;
        public decimal? ShortAvgPrice
        {
            get { return shortAvgPrice; }
            set
            {
                shortAvgPrice = value;
                this.OnPropertyChanged();
            }
        }

        //持仓均价卖
        private string shortAvgPriceStr;
        public string ShortAvgPriceStr
        {
            get { return shortAvgPriceStr; }
            set
            {
                shortAvgPriceStr = value;
                this.OnPropertyChanged();
            }
        }

        //行情买五价/买十价(最下面的价)
        private decimal? bestBid5;
        public decimal? BestBid5
        {
            get { return bestBid5; }
            set
            {
                bestBid5 = value;
                this.OnPropertyChanged();
            }
        }

        private string bestBid5Str;
        public string BestBid5Str
        {
            get
            {
                if (bestBid5.HasValue)
                {
                    bestBid5Str = bestBid5.Value.ToString("F" + Digit);
                }
                else
                {
                    bestBid5Str = decimal.Zero.ToString("F" + Digit);
                }
                return bestBid5Str;
            }
            set
            {
                bestBid5Str = value;
                this.OnPropertyChanged();
            }
        }

        //行情卖五价/卖十价(最上面的价)
        private decimal? bestOfr5;
        public decimal? BestOfr5
        {
            get { return bestOfr5; }
            set
            {
                bestOfr5 = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfr5Str;
        public string BestOfr5Str
        {
            get
            {
                if (bestOfr5.HasValue)
                {
                    bestOfr5Str = bestOfr5.Value.ToString("F" + Digit);
                }
                else
                {
                    bestOfr5Str = decimal.Zero.ToString("F" + Digit);
                }
                return bestOfr5Str;
            }
            set
            {
                bestOfr5Str = value;
                this.OnPropertyChanged();
            }
        }

        //价格最小位数
        private int _digit = 3;
        public int Digit
        {
            get
            {
                return _digit;
            }

            set
            {
                _digit = value;
                this.OnPropertyChanged(nameof(Digit));
            }
        }


        private string bestBidShort;
        /// <summary>
        /// 市场最优（含INNER)
        /// </summary>
        public string BestBidShort
        {
            get { return bestBidShort; }
            set
            {
                bestBidShort = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrShort;
        /// <summary>
        /// 市场最优
        /// </summary>
        public string BestOfrShort
        {
            get { return bestOfrShort; }
            set
            {
                bestOfrShort = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Bid数量
        /// </summary>
        private string bestBidShowAmount;
        public string BestBidShowAmount
        {
            get { return bestBidShowAmount; }
            set
            {
                bestBidShowAmount = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ofr数量
        /// </summary>
        private string bestOfrShowAmount;
        public string BestOfrShowAmount
        {
            get { return bestOfrShowAmount; }
            set
            {
                bestOfrShowAmount = value;
                this.OnPropertyChanged();
            }
        }


        private string bestBidShortOuter;
        /// <summary>
        /// 外部最优（不含INNER)
        /// </summary>
        public string BestBidShortOuter
        {
            get { return bestBidShortOuter; }
            set
            {
                bestBidShortOuter = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrShortOuter;
        /// <summary>
        /// 外部最优 （不含INNER)
        /// </summary>
        public string BestOfrShortOuter
        {
            get { return bestOfrShortOuter; }
            set
            {
                bestOfrShortOuter = value;
                this.OnPropertyChanged();
            }
        }
    }
}
