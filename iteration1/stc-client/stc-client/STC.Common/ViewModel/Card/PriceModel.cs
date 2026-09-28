using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Common.ViewModel.Card
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
        private SolidColorBrush _upSideBdBrush;
        public SolidColorBrush UpSideBdBrush
        {
            get { return _upSideBdBrush; }
            set 
            {
                _upSideBdBrush = value;
                // 如果倒挂，则更改背景色
                Color tarcolor = (Color)ColorConverter.ConvertFromString("#9B2125");
                this.OnPropertyChanged();
                if (_upSideBdBrush.Color == tarcolor)
                {
                    ChangeBgColor();
                }
                else
                {
                    ReSetBgColor();
                }
            }
        }

        //倒挂背景Brush
        private Brush _upSideBgBrush;
        public Brush UpSideBgBrush
        {
            get { return _upSideBgBrush; }
            set { _upSideBgBrush = value; this.OnPropertyChanged(); }
        }

        //倒挂前景色Brush
        private Brush _upSideForgroundBrush = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFEBC8"));
        public Brush UpSideForgroundBrush
        {
            get { return _upSideForgroundBrush; }
            set { _upSideForgroundBrush = value; this.OnPropertyChanged(); }
        }


        private string _security;
        public string Security
        {
            get { return _security; }
            set { _security = value; this.OnPropertyChanged(); }
        }


        private string _symbolName;
        public string SymbolName
        {
            get { return _symbolName; }
            set { _symbolName = value; this.OnPropertyChanged(); }
        }

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
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

        private string _bestBidNetPrice = CommonConstants.NO_PRICE;
        public string BestBidNetPrice
        {
            get { return _bestBidNetPrice; }
            set { _bestBidNetPrice = value; this.OnPropertyChanged(); }
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

        private string _bestOfrNetPrice = CommonConstants.NO_PRICE;
        public string BestOfrNetPrice
        {
            get { return _bestOfrNetPrice; }
            set { _bestOfrNetPrice = value; this.OnPropertyChanged(); }
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
        private string _bestNetPriceBP;
        public string BestNetPriceBP
        {
            get { return _bestNetPriceBP; }
            set { _bestNetPriceBP = value; this.OnPropertyChanged(); }
        }
        private string _bestPriceDisplayBP;
        public string BestPriceDisplayBP
        {
            get { return _bestPriceDisplayBP; }
            set { _bestPriceDisplayBP = value; this.OnPropertyChanged(); }
        }
        private string _bestNetPriceDisplayBP;
        public string BestNetPriceDisplayBP
        {
            get { return _bestNetPriceDisplayBP; }
            set { _bestNetPriceDisplayBP = value; this.OnPropertyChanged(); }
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


        /// <summary>
        /// Bid 背景色
        /// </summary>
        private SolidColorBrush _upSideBidBgBrush;
        public SolidColorBrush UpSideBidBgBrush
        {
            get { return _upSideBidBgBrush; }
            set { _upSideBidBgBrush = value; this.OnPropertyChanged(nameof(UpSideBidBgBrush)); }
        }

        /// <summary>
        /// Bid 边框背景色
        /// </summary>
        private Brush _upSideBidBorderBrush;
        public Brush UpSideBidBorderBrush
        {
            get { return _upSideBidBorderBrush; }
            set { _upSideBidBorderBrush = value; this.OnPropertyChanged(nameof(UpSideBidBorderBrush)); }
        }

        /// <summary>
        /// Ofr 背景色
        /// </summary>
        private SolidColorBrush _upSideOfrBgBrush;
        public SolidColorBrush UpSideOfrBgBrush
        {
            get { return _upSideOfrBgBrush; }
            set { _upSideOfrBgBrush = value; this.OnPropertyChanged(nameof(UpSideOfrBgBrush)); }
        }

        /// <summary>
        /// Ofr 边框背景色
        /// </summary>
        private Brush _upSideOfrBorderBrush;
        public Brush UpSideOfrBorderBrush
        {
            get { return _upSideOfrBorderBrush; }
            set { _upSideOfrBorderBrush = value; this.OnPropertyChanged(nameof(UpSideOfrBorderBrush)); }
        }

        



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

        private string _bestbidsettler;
        public string BestBidSettler { get { return _bestbidsettler; } set { _bestbidsettler = value; this.OnPropertyChanged(); } }

        private string _bestbidsettleryuan;
        public string BestBidSettlerYuan { get { return _bestbidsettleryuan; } set { _bestbidsettleryuan = value; this.OnPropertyChanged(); } }


        private string _bestofrsettler;
        public string BestOfrSettler { get { return _bestofrsettler; } set { _bestofrsettler = value; this.OnPropertyChanged(); } }

        private string _bestofrsettleryuan;
        public string BestOfrSettlerYuan { get { return _bestofrsettleryuan; } set { _bestofrsettleryuan = value; this.OnPropertyChanged(); } }

        private DataRowView _bestOfrDataRowView;
        public DataRowView BestOfrDataRowView { get { return _bestOfrDataRowView; } set { _bestOfrDataRowView = value; this.OnPropertyChanged(); } }

        private DataRowView _bestBidDataRowView;
        public DataRowView BestBidDataRowView { get { return _bestBidDataRowView; } set { _bestBidDataRowView = value; this.OnPropertyChanged(); } }

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


        private string _bidispermission;
        public string BidIsPermission { get { return _bidispermission; } set { _bidispermission = value; this.OnPropertyChanged(); } }

        private string _ofrispermission;
        public string OfrIsPermission { get { return _ofrispermission; } set { _ofrispermission = value; this.OnPropertyChanged(); } }
        #endregion


        public void OnClear()
        {
            this.BestBid = CommonConstants.NO_PRICE;
            this.BestBidNetPrice = CommonConstants.NO_PRICE;
            this.BestBidAmount = CommonConstants.NO_PRICE;
            this.BestOfr = CommonConstants.NO_PRICE;
            this.BestOfrNetPrice = CommonConstants.NO_PRICE;
            this.BestOfrAmount = CommonConstants.NO_PRICE;
            this.BestPriceBP = CommonConstants.NO_PRICE; 
            this.BestNetPriceBP = CommonConstants.NO_PRICE;
            this.BidArrowVisibilly = Visibility.Collapsed;
            this.BidArrowToolTxt = string.Empty;
            this.BidBrokerBargainFlag = string.Empty;
            this.BidCornerMarketVisibility = Visibility.Collapsed;
            this.BidPrice = CommonConstants.NO_PRICE;
            this.BidTxt = string.Empty;
            this.LastExecPrice = CommonConstants.NO_PRICE;
            this.LastExecPriceBP = string.Empty;
            this.LastExecSide = string.Empty;
            this.LastExecSource = string.Empty;
            this.OfrArrowToolTxt = string.Empty;
            this.OfrArrowVisibilly = Visibility.Collapsed;
            this.OfrBrokerBargainFlag = string.Empty;
            this.OfrCornerMarket = string.Empty;
            this.OfrCornerMarketText = string.Empty;
            this.OfrCornerMarketVisibility = Visibility.Collapsed;
            this.OfrPrice = CommonConstants.NO_PRICE;
            this.OfrTxt = string.Empty;
            this.BidIsPermission = "false";
            this.OfrIsPermission = "false";
            this.BestOfrDataRowView = null;
            this.BestBidDataRowView = null;
        }

        /// <summary>
        /// 动画倒挂背景颜色
        /// </summary>
        public void ChangeBgColor()
        {
            SolidColorBrush ys = new SolidColorBrush();
            ColorAnimation ks = new ColorAnimation(); //动画效果
            ks.From = (Color)ColorConverter.ConvertFromString("#4D370F11");
            ks.To = (Color)ColorConverter.ConvertFromString("#909B2125");
            ks.AutoReverse = false; // 正常播放
            ks.RepeatBehavior = new RepeatBehavior(3);
            ks.Completed += Ks_Completed;
            ks.Duration = new Duration(TimeSpan.FromSeconds(1)); // 动画一次所用时间
            // 如果 倒挂 Bid 有效
            if (this.BidIsPermission == "true")
            {
                // 倒挂 Bid 背景色,需要动态变化，最后定格为 #16FF3C3C
                UpSideBidBgBrush = ys; //使用动画绘制
            }
            else
            {
                UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
            }
            // 如果 倒挂 Ofr 有效
            if (this.OfrIsPermission == "true")
            {
                // 倒挂 Ofr 背景色,需要动态变化，最后定格为 #16FF3C3C
                UpSideOfrBgBrush = ys; //使用动画绘制
            }
            else
            {
                UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
            }

            ys.BeginAnimation(SolidColorBrush.ColorProperty, ks); // 颜色绘制使用动画绘制
        }

        public void ReSetBgColor()
        {
            // Bid 
            if ((BestBid != "--") && (BidIsPermission == "true"))
            {
                UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                UpSideBidBorderBrush = UpSideBidBgBrush;
            }
            else
            {
                UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                //UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                UpSideBidBorderBrush = UpSideBidBgBrush;
            }

            // Ofr 
            if ((BestOfr != "--") && (OfrIsPermission == "true"))
            {
                UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                UpSideOfrBorderBrush = UpSideOfrBgBrush;
            }
            else
            {
                UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                //UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                UpSideOfrBorderBrush = UpSideOfrBgBrush;
            }

            if ((BestBid != "--") && (BestOfr != "--"))
            {
                decimal bidD = CommonUtils.ConvertToDecimal(BestBid);
                decimal ofrD = CommonUtils.ConvertToDecimal(BestOfr);
                decimal bpd = (bidD - ofrD) * 100;
                if (string.IsNullOrEmpty(BestBid) || string.IsNullOrEmpty(BestOfr))
                {
                    BestPriceBP = CommonConstants.NO_PRICE;
                    BestPriceDisplayBP = CommonConstants.NO_PRICE;
                }
                else
                {
                    BestPriceBP = PriceCalHelper.Instance.RemoveZero(CommonUtils.FormatNum(bpd.ToString())) + " bp";
                    BestPriceDisplayBP = CommonUtils.FormatLimitBp(bpd);
                }
                decimal bidNet = CommonUtils.ConvertToDecimal(BestBidNetPrice);
                decimal ofrNet = CommonUtils.ConvertToDecimal(BestOfrNetPrice);
                decimal bpNet = (ofrNet-bidNet);
                if (string.IsNullOrEmpty(BestBidNetPrice) || string.IsNullOrEmpty(BestOfrNetPrice))
                {
                    BestNetPriceBP = CommonConstants.NO_PRICE;
                    BestNetPriceDisplayBP = CommonConstants.NO_PRICE;
                }
                else
                {
                    BestNetPriceBP = PriceCalHelper.Instance.RemoveZero(CommonUtils.FormatNum(bpNet.ToString()));
                    BestNetPriceDisplayBP = CommonUtils.FormatLimitNetPx(bpNet);
                }
                // 处理倒挂颜色
                if (bpd <= 0)
                {
                    // 如果 倒挂 Bid 有效
                    if (BidIsPermission == "true")
                    {
                        // 倒挂 Bid 背景色,需要动态变化，最后定格为 #16FF3C3C
                        UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF370F11"));
                        UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                    }
                    else
                    {
                        UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                        UpSideBidBorderBrush = UpSideBidBgBrush;
                        // UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                    }

                    // 如果 倒挂 Ofr 有效
                    if (OfrIsPermission == "true")
                    {
                        // 倒挂 Ofr 背景色,需要动态变化，最后定格为 #16FF3C3C
                        UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF370F11"));
                        UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                    }
                    else
                    {
                        UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                        UpSideOfrBorderBrush = UpSideOfrBgBrush;
                        //UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                    }
                }
                else
                {
                    // 如果 Bid 有效
                    if (BidIsPermission == "true")
                    {
                        UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                        UpSideBidBorderBrush = UpSideBidBgBrush;
                    }
                    else
                    {
                        UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                        UpSideBidBorderBrush = UpSideBidBgBrush;
                        //UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                    }

                    // 如果 Ofr 有效
                    if (OfrIsPermission == "true")
                    {
                        UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                        UpSideOfrBorderBrush = UpSideOfrBgBrush;
                    }
                    else
                    {
                        UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                        UpSideOfrBorderBrush = UpSideOfrBgBrush;
                        //UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                    }
                }
            }
        }

        private void Ks_Completed(object sender, EventArgs e)
        {
            ReSetBgColor();
        }
    }
}

