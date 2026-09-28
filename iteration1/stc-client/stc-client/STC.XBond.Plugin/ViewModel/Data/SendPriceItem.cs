using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace STC.XBond.Plugin.ViewModel.Data
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

        private string bestBidInt;
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
        public decimal? BestOfr
        {
            get { return bestOfr; }
            set
            {
                bestOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string bestOfrInt;
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
                if (value != null)
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
                if (value != null)
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

        private Brush borderBG;
        public Brush BorderBG
        {
            get { return borderBG; }
            set
            {
                if (value != null)
                {
                    borderBG = value;
                    this.OnPropertyChanged();
                }
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

        private Visibility settleTypeVisible = Visibility.Collapsed;
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
    }
}
