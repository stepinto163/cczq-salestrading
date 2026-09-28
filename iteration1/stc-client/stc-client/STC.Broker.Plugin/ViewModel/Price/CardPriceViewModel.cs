using com.gateway.log;
using STC.Broker.Plugin.Handler;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.Order;
using STC.Broker.Plugin.View.Price;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.GW;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Frame.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;

namespace STC.Broker.Plugin.ViewModel.Price
{
    public class CardPriceViewModel : BaseViewModel
    {
        #region 定义
        public CardPriceView mywin = null;
        private object lockbidObj = new object();
        private object lockofrObj = new object();
        public const string TXT_KW = "kw";
        public const string TXT_IRS = "百万";

        #region 债券基本信息
        //private HomeGroupSecurityIDsInfo _HomeGroupInfo;
        //public HomeGroupSecurityIDsInfo HomeGroupInfo
        //{
        //    get { return _HomeGroupInfo; }
        //    set { _HomeGroupInfo = value; this.OnPropertyChanged(); }
        //}


        private CardItem _curcarditem;
        public CardItem CurCardItem
        {
            get { return _curcarditem; }
            set { _curcarditem = value; this.OnPropertyChanged(); }
        }

        private string _SecurityID;
        private string oldSecurityID;
        public string SecurityID
        {
            get { return _SecurityID; }
            set
            {
                oldSecurityID = _SecurityID;
                _SecurityID = value;

                if (!string.IsNullOrEmpty(_SecurityID) && isLoadDataOK)
                {
                    GetSymbolInfo();
                    //SubscribeCtpPrice(_SecurityID);
                }

                if (!string.IsNullOrEmpty(_SecurityID) && isLoadDataOK && _SecurityID != oldSecurityID)
                {
                    if (CurCardItem != null)
                    {
                        //通知Broker系统
                        QuoteConfigHandler.Instance.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, CurCardItem.GroupId, _SecurityID, CurCardItem);
                    }

                    SubscribeCtpPrice(_SecurityID);
                    oldSecurityID = _SecurityID;
                }
                else
                {
                    SymbolName = RemainingTime = string.Empty;
                    ////if (!string.IsNullOrEmpty(oldSecurityID))
                    ////{
                    ////    if (!CurCardItem.SecurityidList.Contains(_SecurityID))
                    ////    {
                    ////        InstsertOrDeleteBonds(oldSecurityID, "3");

                    ////        if (CurCardItem.SecurityidList.Contains(oldSecurityID))
                    ////            CurCardItem.SecurityidList.Remove(oldSecurityID);
                    ////    }
                    ////}
                }

                this.OnPropertyChanged();
            }
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

        private int _remaingingtimeconvert;
        public int RemaingingTimeConvert
        {
            get { return _remaingingtimeconvert; }
            set { _remaingingtimeconvert = value; this.OnPropertyChanged(); }
        }

        private string _bondcategory;
        public string BondCategory
        {
            get { return _bondcategory; }
            set { _bondcategory = value; this.OnPropertyChanged(); }
        }
        

        private string _marketIndicator = "4";
        public string MarketIndicator
        {
            get { return _marketIndicator; }
            set { _marketIndicator = value; this.OnPropertyChanged(); }
        }

        

        private Visibility _IsAllControlVisible = Visibility.Visible;
        /// <summary>
        /// 所有控件是否可见
        /// </summary>
        public Visibility IsAllControlVisible
        {
            get
            {
                return _IsAllControlVisible;
            }
            set
            {
                _IsAllControlVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _IsAddSecurityIDVisible = Visibility.Collapsed;
        /// <summary>
        /// 添加债券是否可见
        /// </summary>
        public Visibility IsAddSecurityIDVisible
        {
            get
            {
                return _IsAddSecurityIDVisible;
            }
            set
            {
                _IsAddSecurityIDVisible = value; 
                this.OnPropertyChanged();
            }
        }

        private Visibility _IsSearchSecurityIDVisible = Visibility.Visible;
        /// <summary>
        /// 添加债券是否可见
        /// </summary>
        public Visibility IsSearchSecurityIDVisible
        {
            get
            {
                return _IsSearchSecurityIDVisible;
            }
            set
            {
                _IsSearchSecurityIDVisible = value; this.OnPropertyChanged();
            }
        }

        private bool _IsAddSecurityIDEnable = true;
        public bool IsAddSecurityIDEnable
        {
            get
            {
                return _IsAddSecurityIDEnable;
            }
            set
            {
                _IsAddSecurityIDEnable = value; this.OnPropertyChanged();
            }
        }

        #endregion

        #region 最后成交信息
        private string _lastdisplaymarketpx = CommonConstants.NO_PRICE;
        public string LastDisplayMarketPx
        {
            get { return _lastdisplaymarketpx; }
            set { _lastdisplaymarketpx = value; this.OnPropertyChanged(); }
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

        private string _lastExecSide;
        public string LastExecSide
        {
            get { return _lastExecSide; }
            set { _lastExecSide = value; this.OnPropertyChanged(); }
        }

        private string _lastmarketpxvenues;
        public string LastMarketPxVenues
        {
            get { return _lastmarketpxvenues; }
            set { _lastmarketpxvenues = value; this.OnPropertyChanged(); }
        }

        #endregion

        #region 数据

        public ObservableCollection<GoodsVolumeVoModel> _bidquotations = new ObservableCollection<GoodsVolumeVoModel>();

        public ObservableCollection<GoodsVolumeVoModel> BidQuotations
        {
            get
            {
                return _bidquotations;
            }
            set
            {
                _bidquotations = value;
                this.OnPropertyChanged(nameof(BidQuotations));
            }
        }


        public ObservableCollection<GoodsVolumeVoModel> _ofrquotations = new ObservableCollection<GoodsVolumeVoModel>();

        public ObservableCollection<GoodsVolumeVoModel> OfrQuotations
        {
            get
            {
                return _ofrquotations;
            }

            set { _ofrquotations = value; this.OnPropertyChanged(nameof(OfrQuotations)); }
        }

        private GoodsVolumeVoModel selbidquotations;
        public GoodsVolumeVoModel SelBidQuotations
        {
            get
            {
                return selbidquotations;
            }

            set 
            { 
                selbidquotations = value; 
                this.OnPropertyChanged(); 
            }
        }

        private GoodsVolumeVoModel selOfrquotations;
        public GoodsVolumeVoModel SelOfrQuotations
        {
            get
            {
                return selOfrquotations;
            }

            set 
            { 
                selOfrquotations = value; 
                this.OnPropertyChanged(); 
            }
        }

        private CollectionViewSource ofrquotationsmv = new CollectionViewSource();
        public CollectionViewSource OfrQuotationsMv
        {
            get 
            { 
                return ofrquotationsmv; 
            }
            set
            {
                ofrquotationsmv = value;
                this.OnPropertyChanged(nameof(OfrQuotationsMv));
            }
        }

        private CollectionViewSource bidquotationsmv = new CollectionViewSource();
        public CollectionViewSource BidQuotationsMv
        {
            get { return bidquotationsmv; }
            set
            {
                bidquotationsmv = value;
                this.OnPropertyChanged(nameof(BidQuotationsMv));
            }
        }


        public ObservableCollection<GoodsVolumeVoModel> _selfquotations = new ObservableCollection<GoodsVolumeVoModel>();

        public ObservableCollection<GoodsVolumeVoModel> SelfQuotations
        {
            get
            {
                return _selfquotations;
            }
            set
            {
                _selfquotations = value;
                this.OnPropertyChanged(nameof(SelfQuotations));
            }
        }

        #endregion



        private Visibility _PopupCloseButtonVisible = Visibility.Collapsed;
        /// <summary>
        /// 关闭按钮是否可见
        /// </summary>
        public Visibility PopupCloseButtonVisible
        {
            get
            {
                return _PopupCloseButtonVisible;
            }
            set
            {
                _PopupCloseButtonVisible = value;
                if (value == Visibility.Visible)
                {
                    CardGridHeight = new GridLength(1, GridUnitType.Star);
                }
                
                this.OnPropertyChanged();
            }
        }

        private Visibility _MoreMenuButtonVisible = Visibility.Visible;
        /// <summary>
        /// More ... 按钮是否可见
        /// </summary>
        public Visibility MoreMenuButtonVisible
        {
            get
            {
                return _MoreMenuButtonVisible;
            }
            set
            {
                _MoreMenuButtonVisible = value; this.OnPropertyChanged();
            }
        }

        /// <summary>
        ///  myWindow 
        /// </summary>
        public NoBorderWin _myWindow = null;
        public NoBorderWin myWindow
        {
            get { return _myWindow; }
            set { _myWindow = value; this.OnPropertyChanged(); }
        }

        private GridLength _CardGridHeight = new GridLength(286);
        public GridLength CardGridHeight
        {
            get { return _CardGridHeight; }
            set { _CardGridHeight = value; this.OnPropertyChanged(); }
        }

        private bool _IsGVNCommandEnable = true;
        /// <summary>
        /// GVN是否可用
        /// </summary>
        public bool IsGVNCommandEnable
        {
            get
            {
                return _IsGVNCommandEnable;
            }
            set
            {
                _IsGVNCommandEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsTKNCommandEnable = true;
        /// <summary>
        /// TKN是否可用
        /// </summary>
        public bool IsTKNCommandEnable
        {
            get
            {
                return _IsTKNCommandEnable;
            }
            set
            {
                _IsTKNCommandEnable = value; 
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Command
        public ICommand GVNCommand { get; set; }
        public ICommand TKNCommand { get; set; }
        

        public ICommand BestBidCommand { get; set; }
        public ICommand BestOfrCommand { get; set; }

        /// <summary>
        /// 添加债券
        /// </summary>
        public ICommand AddBoundCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        #endregion

        #region 最优集合

        #region 最优 Bid
        private decimal? _BestBidPrice;

        /// <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public decimal? BestBidPrice
        {
            get
            {
                return _BestBidPrice;
            }
            set
            {
                _BestBidPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _BestBidshowprice;

        /// <summary>
        /// 价格
        /// </summary>
        public string BestBidShowprice
        {
            get
            {
                return _BestBidshowprice;
            }
            set
            {
                this._BestBidshowprice = value;
                base.OnPropertyChanged("BestBidShowprice");
                this.BestBidPriceFormat = (this.BestBidPrice == null) ? "-.-" : CommonUtils.FormatNum(this.BestBidPrice.Value.ToString());
                string bi = this.BestBidPriceFormat.Substring(0, this.BestBidPriceFormat.IndexOf('.') + 1);
                this.BestBidPriceInt = bi == "-." ? "--" : bi; 
                string bd = this.BestBidPriceFormat.Substring(this.BestBidPriceFormat.IndexOf('.') + 1);
                this.BestBidPriceDecimal = bd =="-" ? "" : bd;
            }
        }

        private string _BestBidPriceFormat;
        public string BestBidPriceFormat
        {
            get
            {
                return this._BestBidPriceFormat;
            }
            set
            {
                this._BestBidPriceFormat = value;
                base.OnPropertyChanged("BestBidPriceFormat");
            }
        }

        private string _BestBidPriceInt;
        public string BestBidPriceInt
        {
            get
            {
                return this._BestBidPriceInt;
            }
            set
            {
                this._BestBidPriceInt = value;
                base.OnPropertyChanged("BestBidPriceInt");
            }
        }

        private string _BestBidPriceDecimal;
        public string BestBidPriceDecimal
        {
            get
            {
                return this._BestBidPriceDecimal;
            }
            set
            {
                this._BestBidPriceDecimal = value;
                base.OnPropertyChanged("BestBidPriceDecimal");
            }
        }

        private decimal _BestBidSumQty;

        /// <summary>
        ///  最优 Bid Qty
        /// </summary>
        public decimal BestBidSumQty
        {
            get
            {
                return _BestBidSumQty;
            }
            set
            {
                _BestBidSumQty = value;

                if (MarketIndicator == "2")
                {
                    QtyTxt = TXT_IRS;
                }
                else
                {
                    QtyTxt = TXT_KW;
                }

                if (value > 0)
                {
                    if (QtyTxt == TXT_IRS)
                    {
                        BestBidShowQty = (value / 100).ToString("#");
                    }
                    else
                    {
                        BestBidShowQty = (value / 1000).ToString("0.#");
                    }
                }
                else
                {
                    BestBidShowQty = "--";
                }

                this.OnPropertyChanged();
            }
        }


        private string _BestBidShowQty;

        /// <summary>
        /// 显示订单量
        /// </summary>
        public string BestBidShowQty
        {
            get
            {
                return _BestBidShowQty;
            }
            set
            {
                _BestBidShowQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _QtyTxt;

        /// <summary>
        /// 显示订单量单位 kw 百万
        /// </summary>
        public string QtyTxt
        {
            get
            {
                return _QtyTxt;
            }
            set
            {
                _QtyTxt = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region 最优 ofr
        private decimal? _BestOfrPrice;

        /// <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public decimal? BestOfrPrice
        {
            get
            {
                return _BestOfrPrice;
            }
            set
            {
                _BestOfrPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _BestOfrshowprice;

        /// <summary>
        /// 价格
        /// </summary>
        public string BestOfrShowprice
        {
            get
            {
                return _BestOfrshowprice;
            }
            set
            {
                _BestOfrshowprice = value;
                this.BestOfrPriceFormat = (this.BestOfrPrice == null) ? "-.-" : CommonUtils.FormatNum(this.BestOfrPrice.Value.ToString());

                string bi = this.BestOfrPriceFormat.Substring(0, this.BestOfrPriceFormat.IndexOf('.') + 1);
                this.BestOfrPriceInt = bi == "-." ? "--" : bi;
                string bd = this.BestOfrPriceFormat.Substring(this.BestOfrPriceFormat.IndexOf('.') + 1);
                this.BestOfrPriceDecimal = bd == "-" ? "" : bd;
            }
        }

        private string _BestOfrPriceFormat;
        public string BestOfrPriceFormat
        {
            get
            {
                return this._BestOfrPriceFormat;
            }
            set
            {
                this._BestOfrPriceFormat = value;
                base.OnPropertyChanged("BestOfrPriceFormat");
            }
        }

        private string _BestOfrPriceInt;
        public string BestOfrPriceInt
        {
            get
            {
                return this._BestOfrPriceInt;
            }
            set
            {
                this._BestOfrPriceInt = value;
                base.OnPropertyChanged("BestOfrPriceInt");
            }
        }

        private string _BestOfrPriceDecimal;
        public string BestOfrPriceDecimal
        {
            get
            {
                return this._BestOfrPriceDecimal;
            }
            set
            {
                this._BestOfrPriceDecimal = value;
                base.OnPropertyChanged("BestOfrPriceDecimal");
            }
        }


        private decimal _BestOfrSumQty;

        /// <summary>
        ///  最优 Ofr Qty
        /// </summary>
        public decimal BestOfrSumQty
        {
            get
            {
                return _BestOfrSumQty;
            }
            set
            {
                _BestOfrSumQty = value;
                this.OnPropertyChanged();

                if (MarketIndicator == "2")
                {
                    QtyTxt = TXT_IRS;
                }
                else
                {
                    QtyTxt = TXT_KW;
                }

                if (value > 0)
                {
                    if (QtyTxt == TXT_IRS)
                    {
                        BestOfrShowQty = (value / 100).ToString("#");
                    }
                    else
                    {
                        BestOfrShowQty = (value / 1000).ToString("0.#");
                    }
                }
                else
                {
                    BestOfrShowQty = "--";
                }

                base.OnPropertyChanged();
            }

        }

        private string _BestOfrShowQty;

        /// <summary>
        /// 显示订单量
        /// </summary>
        public string BestOfrShowQty
        {
            get
            {
                return _BestOfrShowQty;
            }
            set
            {
                _BestOfrShowQty = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private ObservableCollection<GoodsVolumeVoModel> _BestBidQuotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// Bid 最优价列表
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BestBidQuotations
        {
            get
            {
                return _BestBidQuotations;
            }
            set
            {
                _BestBidQuotations = value;
                this.OnPropertyChanged();
            }
        }


        private ObservableCollection<GoodsVolumeVoModel> _BestOfrQuotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// Ofr 最优价列表
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BestOfrQuotations
        {
            get
            {
                return _BestOfrQuotations;
            }
            set
            {
                _BestOfrQuotations = value;
                this.OnPropertyChanged(nameof(BestOfrQuotations));
            }
        }

        private CollectionViewSource bestidquotationsmv = new CollectionViewSource();
        public CollectionViewSource BestBidQuotationsMv
        {
            get { return bestidquotationsmv; }
            set
            {
                bestidquotationsmv = value;
                this.OnPropertyChanged(nameof(BestBidQuotationsMv));
            }
        }


        private CollectionViewSource bestofrquotationsmv = new CollectionViewSource();
        public CollectionViewSource BestOfrQuotationsMv
        {
            get 
            { 
                return bestofrquotationsmv; 
            }
            set
            {
                bestofrquotationsmv = value;
                this.OnPropertyChanged(nameof(BestOfrQuotationsMv));
            }
        }

        bool isLoadDataOK = false;

        private ObservableCollection<MultiBestQuoteModel> _MultiBidBestQuotations = new ObservableCollection<MultiBestQuoteModel>();
        public ObservableCollection<MultiBestQuoteModel> MultiBidBestQuotations
        {
            get
            {
                return this._MultiBidBestQuotations;
            }
            set
            {
                this._MultiBidBestQuotations = value;
                this.OnPropertyChanged(nameof(MultiBidBestQuotations));
            }
        }

        private CollectionViewSource _multiBidBestQuotationsmv = new CollectionViewSource();
        public CollectionViewSource MultiBidBestQuotationsMv
        {
            get 
            {
                return this ._multiBidBestQuotationsmv;
            }
            set
            {
                this._multiBidBestQuotationsmv = value;
                this.OnPropertyChanged(nameof(MultiBidBestQuotationsMv));
            }
        }

        private ObservableCollection<MultiBestQuoteModel> _MultiOfrBestQuotations = new ObservableCollection<MultiBestQuoteModel>();
        public ObservableCollection<MultiBestQuoteModel> MultiOfrBestQuotations
        {
            get 
            {
                return this ._MultiOfrBestQuotations;
            }
            set
            {
                this._MultiOfrBestQuotations = value;
                this.OnPropertyChanged(nameof(MultiOfrBestQuotations));
            }
        }

        private CollectionViewSource _multiOfrBestQuotationsmv = new CollectionViewSource();
        public CollectionViewSource MultiOfrBestQuotationsMv
        {
            get 
            {
                return this._multiOfrBestQuotationsmv;
            }
            set
            {
                this._multiOfrBestQuotationsmv = value;
                this.OnPropertyChanged(nameof(MultiOfrBestQuotationsMv));
            }
        }

        private MultiBestQuoteModel _selMultiBidBestQuotations;
        public MultiBestQuoteModel SelMultiBidBestQuotations
        {
            get 
            {
                return this ._selMultiBidBestQuotations;
            }
            set
            {
                this._selMultiBidBestQuotations = value;
                this.OnPropertyChanged("SelMultiBidBestQuotations");
            }
        }

        private MultiBestQuoteModel _selMultiOfrBestQuotations;
        public MultiBestQuoteModel SelMultiOfrBestQuotations
        {
            get 
            {
                return this._selMultiOfrBestQuotations;
            }
            set
            {
                this._selMultiOfrBestQuotations = value;
                this.OnPropertyChanged("SelMultiOfrBestQuotations");
            }
        }

        private int _curPageIndex = 0;
        public int CurPageIndex
        {
            get
            {
                return  this._curPageIndex;
            }
            set
            {
                this._curPageIndex = value;
                this.OnPropertyChanged("CurPageIndex");
            }
        }


        private ObservableCollection<SignalData> _ListMenu = new ObservableCollection<SignalData>();
        /// <summary>
        /// 下拉菜单
        /// </summary>
        public ObservableCollection<SignalData> ListMenu
        {
            get
            {
                return _ListMenu;
            }
            set
            {
                _ListMenu = value;
                this.OnPropertyChanged(nameof(ListMenu));
            }
        }


        #endregion


        public CardPriceViewModel()
        {
            GVNCommand = new DelegateCommand(new Action<object>(OnGVNCommand));
            TKNCommand = new DelegateCommand(new Action<object>(OnTKNCommand));

            BestBidCommand = new DelegateCommand(new Action<object>(OnBestBidCommand));
            BestOfrCommand = new DelegateCommand(new Action<object>(OnBestOfrCommand));
            AddBoundCommand = new DelegateCommand(new Action<object>(OnAddBoundCommand));

            CloseCommand = new DelegateCommand(new Action<object>(OnCloseCommand));

            //全部 BID INNER
            this.BidQuotationsMv.Source = BidQuotations;
            this.BidQuotationsMv.View.CollectionChanged += BidQuotationsMv_CollectionChanged;
            this.BidQuotationsMv.IsLiveFilteringRequested = true;
            this.BidQuotationsMv.IsLiveSortingRequested = true;

            this.BidQuotationsMv.SortDescriptions.Add(new SortDescription("MDPriceLevel", ListSortDirection.Ascending));
            this.BidQuotationsMv.SortDescriptions.Add(new SortDescription("FirmFlag", ListSortDirection.Descending));

            //全部 OFR INNER
            this.OfrQuotationsMv.Source = OfrQuotations;
            this.OfrQuotationsMv.View.CollectionChanged += OfrQuotationsMv_CollectionChanged;
            this.OfrQuotationsMv.IsLiveFilteringRequested = true;
            this.OfrQuotationsMv.IsLiveSortingRequested = true;

            this.OfrQuotationsMv.SortDescriptions.Add(new SortDescription("MDPriceLevel", ListSortDirection.Ascending));
            this.OfrQuotationsMv.SortDescriptions.Add(new SortDescription("FirmFlag", ListSortDirection.Descending));

            SetBidQuotationsMvFilter();
            SetOfrQuotationsMvFilter();


            //Bid 最优价列表 BID INNER
            this.BestBidQuotationsMv.Source = BestBidQuotations;
            this.BestBidQuotationsMv.IsLiveFilteringRequested = true;
            this.BestBidQuotationsMv.IsLiveSortingRequested = true;

            this.BestBidQuotationsMv.SortDescriptions.Add(new SortDescription("MDPriceLevel", ListSortDirection.Ascending));
            this.BestBidQuotationsMv.SortDescriptions.Add(new SortDescription("FirmFlag", ListSortDirection.Descending));

            //Ofr 最优价列表 OFR INNER
            this.BestOfrQuotationsMv.Source = BestOfrQuotations;
            this.BestOfrQuotationsMv.IsLiveFilteringRequested = true;
            this.BestOfrQuotationsMv.IsLiveSortingRequested = true;

            this.BestOfrQuotationsMv.SortDescriptions.Add(new SortDescription("MDPriceLevel", ListSortDirection.Ascending));
            this.BestOfrQuotationsMv.SortDescriptions.Add(new SortDescription("FirmFlag", ListSortDirection.Descending));

            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);

            this.MultiBidBestQuotationsMv.Source = this.MultiBidBestQuotations;
            this.MultiBidBestQuotationsMv.IsLiveFilteringRequested = true;
            this.MultiBidBestQuotationsMv.IsLiveSortingRequested = true;
            this.MultiBidBestQuotationsMv.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending));

            this.MultiOfrBestQuotationsMv.Source = this.MultiOfrBestQuotations;
            this.MultiOfrBestQuotationsMv.IsLiveFilteringRequested = true;
            this.MultiOfrBestQuotationsMv.IsLiveSortingRequested = true;
            this.MultiOfrBestQuotationsMv.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending));

            //初始化显示数据
            InitBestBidOfrQuotationData();

            ListMenu.Add(new SignalData() { Content = "弹出", Topic="Popup" });
            ListMenu.Add(new SignalData() { Content = "删除", Topic = "DelBond" });
            
            //响应设置改变功能
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
        }

        private void Instance_OnSettingChange(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                if (obj is GrLayoutSettingModel)
                {
                    CardPriceBidSelector templatebidSelector = new CardPriceBidSelector();
                    CardPriceOfrSelector templateofrSelector = new CardPriceOfrSelector();
                    (mywin as CardPriceView).DgBidQuotation.ItemTemplateSelector = templatebidSelector;
                    (mywin as CardPriceView).DgOfrQuotation.ItemTemplateSelector = templateofrSelector;
                }
            }));
        }

        #region InitUiData
        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                isLoadDataOK = true;

                if (!string.IsNullOrEmpty(_SecurityID))
                {
                    GetSymbolInfo();
                    //SubscribeCtpPrice(_SecurityID);
                }
            }
        }

        /// <summary>
        /// 初始化显示数据
        /// </summary>
        private void InitBestBidOfrQuotationData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BestBidQuotations.Clear();
                BestBidPrice = null;
                BestBidShowprice = "--";
                BestBidSumQty = 0;
                for (int i = 0; i < 8; i++)
                {
                    BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                }
                

                BestOfrQuotations.Clear();
                BestOfrPrice = null;
                BestOfrShowprice = "--";
                BestOfrSumQty = 0;
                for (int i = 0; i < 8; i++)
                {
                    BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                }
                
                SelfQuotations.Clear();

                this.MultiBidBestQuotations.Clear();
                
                this.MultiBidBestQuotations.Add(new MultiBestQuoteModel() { id = "1", Side = "1", SettlType = "全", qty = 0, MDPriceLevel = 1, IsVisible = true });
                this.MultiBidBestQuotations.Add(new MultiBestQuoteModel() { id = "2", Side = "1", SettlType = "+1", qty = 0, MDPriceLevel = 2, IsVisible = true });
                this.MultiBidBestQuotations.Add(new MultiBestQuoteModel() { id = "3", Side = "1", SettlType = "+0", qty = 0, MDPriceLevel = 3, IsVisible = true });
                this.MultiBidBestQuotations.Add(new MultiBestQuoteModel() { id = "4", Side = "1", SettlType = "远", qty = 0, MDPriceLevel = 4, IsVisible = true });
                this.MultiBidBestQuotations.Add(new MultiBestQuoteModel() { id = "5", Side = "1", SettlType = "不限", qty = 0, MDPriceLevel = 5, IsVisible = false });


                this.MultiOfrBestQuotations.Clear();

                this.MultiOfrBestQuotations.Add(new MultiBestQuoteModel() { id = "1", Side = "2", SettlType = "全", qty = 0, MDPriceLevel = 1, IsVisible = true });
                this.MultiOfrBestQuotations.Add(new MultiBestQuoteModel() { id = "2", Side = "2", SettlType = "+1", qty = 0, MDPriceLevel = 2, IsVisible = true });
                this.MultiOfrBestQuotations.Add(new MultiBestQuoteModel() { id = "3", Side = "2", SettlType = "+0", qty = 0, MDPriceLevel = 3, IsVisible = true });
                this.MultiOfrBestQuotations.Add(new MultiBestQuoteModel() { id = "4", Side = "2", SettlType = "远", qty = 0, MDPriceLevel = 4, IsVisible = true });
                this.MultiOfrBestQuotations.Add(new MultiBestQuoteModel() { id = "5", Side = "2", SettlType = "不限", qty = 0, MDPriceLevel = 5, IsVisible = false });
            });
        }

        #endregion

        #region 设置过滤条件
        private Dictionary<string, IList> _SettleFilterDic = new Dictionary<string, IList>();
        public Dictionary<string, IList> SettleFilterDic
        {
            get { return _SettleFilterDic; }
            set
            {
                _SettleFilterDic = value;

                StringBuilder settlTypeFilterBuf = new StringBuilder();
                if (_SettleFilterDic != null && _SettleFilterDic.ContainsKey("Settle"))
                {
                    IList settlTypeList = _SettleFilterDic["Settle"];
                    if (settlTypeList.Count > 0)
                    {
                        foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                        {
                            if (settlType.Content.ToString() == "+1")
                            {
                                SettleFilter = "+1";
                            }
                            else if (settlType.Content.ToString() == "+0")
                            {
                                SettleFilter = "+0";
                            }
                            else if (settlType.Content.ToString() == "多")
                            {
                                SettleFilter = "多";
                            }
                            else
                            {
                                SettleFilter = "远";
                            }
                        }
                    }
                    else
                    {
                        SettleFilter = "多";
                    }
                }

                this.OnPropertyChanged(nameof(SettleFilterDic));
            }
        }

        private string _SettleFilter = "+1";

        public string SettleFilter
        {
            get { return _SettleFilter; }
            set
            {
                _SettleFilter = value;
            }
        }

        private int _settleindex = 1;
        /// <summary>
        /// 清算速度 索引 默认选择 1,当多选时  0,1,2,3 全,+1,+0,远
        /// </summary>
        public int SettleIndex
        {
            get
            {
                return _settleindex;
            }
            set
            {
                _settleindex = value;
                if (CurCardItem != null)
                {
                    CurCardItem.SettleIndex = value.ToString();
                    //通知Broker系统
                    QuoteConfigHandler.Instance.Notify(HomePageDataHandler.FavoriteTypeEnum.UpdBond, CurCardItem.GroupId, SecurityID, CurCardItem);
                }

                this.OnPropertyChanged();
            }
        }

 
        public void FilterData(Dictionary<string, IList> filterDic)
        {
            StringBuilder settlTypeFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("Settle"))
            {
                IList settlTypeList = filterDic["Settle"];
                if (settlTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                    {
                        if (settlType.Content.ToString() == "+1")
                        {
                            SettleFilter = "+1";
                            SettleIndex = 1;
                        }
                        else if (settlType.Content.ToString() == "+0")
                        {
                            SettleFilter = "+0";
                            SettleIndex = 2;
                        }
                        else if (settlType.Content.ToString() == "多")
                        {
                            SettleFilter = "多";
                            SettleIndex = 0;
                        }
                        else
                        {
                            SettleFilter = "远";
                            SettleIndex = 3;
                        }
                    }

                }
                else
                {
                    SettleFilter = "多";
                    SettleIndex = 3;
                }

                //选择页面
                this.CurPageIndex = (this.SettleFilter != "多") ? 0 : 1;

                IsGVNCommandEnable = IsTKNCommandEnable = SettleFilter == "远" ? false : true;
            }

            SetBidQuotationsMvFilter();
            SetOfrQuotationsMvFilter();

        }

        private void SetBidQuotationsMvFilter()
        {
            this.BidQuotationsMv.View.Filter = x =>
            {
                GoodsVolumeVoModel data = x as GoodsVolumeVoModel;
                switch (SettleFilter)
                {
                    case "+1":
                        data.IsInfo9Visible = "0";
                        return data.DictSettleType.ContainsKey("2,3") || data.DictSettleType.ContainsKey("2") || data.DictSettleType.ContainsKey("3");
                    case "+0":
                        data.IsInfo9Visible = "0";
                        return data.DictSettleType.ContainsKey("1");
                    case "远":
                        data.IsInfo9Visible = "1";
                        return data.settlType.Contains("+");
                    case "多":
                        data.IsInfo9Visible = "0";
                        return true;
                }

                bool result = true;
                return result;
            };

        }

        private void SetOfrQuotationsMvFilter()
        {
            this.OfrQuotationsMv.View.Filter = x =>
            {
                GoodsVolumeVoModel data = x as GoodsVolumeVoModel;
                switch (SettleFilter)
                {
                    case "+1":
                        data.IsInfo9Visible = "0";
                        return data.DictSettleType.ContainsKey("2,3") || data.DictSettleType.ContainsKey("2") || data.DictSettleType.ContainsKey("3");
                    case "+0":
                        data.IsInfo9Visible = "0";
                        return data.DictSettleType.ContainsKey("1");
                    case "远":
                        data.IsInfo9Visible = "1";
                        return data.settlType.Contains("+");
                    case "多":
                        data.IsInfo9Visible = "0";
                        return true;
                }

                bool result = true;
                return result;
            };

        }
        #endregion

        #region 数据更新时更新界面数据
        private void BidQuotationsMv_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Count() > 0)
                {
                    GoodsVolumeVoModel list = BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).OrderBy(o => o.MDPriceLevel).First();
                    if (list != null)
                    {
                        BestBidPrice = list.price;
                        BestBidShowprice = list.showprice;

                        BestBidQuotations.Clear();
                        List<GoodsVolumeVoModel> tmpList = BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.price == list.price && p.MDPriceLevel != 999).ToList();
                        foreach (var x in tmpList)
                        {
                            BestBidQuotations.Add(x);
                        }

                        if (BestBidQuotations.Count < 8)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                            }
                        }

                        BestBidQuotationsMv.View.Refresh();
                        BestBidSumQty = tmpList.Sum(p => p.qty);

                    }
                }
                else
                {
                    BestBidQuotations.Clear();
                    this.BestBidPrice = null;
                    this.BestBidShowprice = "--";
                    this.BestBidSumQty = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                    }
                }
            });
        }

        private void OfrQuotationsMv_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Count() > 0)
                {
                    GoodsVolumeVoModel list = OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).OrderBy(o => o.MDPriceLevel).First();
                    if (list != null)
                    {
                        BestOfrPrice = list.price;
                        BestOfrShowprice = list.showprice;

                        BestOfrQuotations.Clear();
                        List<GoodsVolumeVoModel> tmpList = OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.price == list.price && p.MDPriceLevel != 999).ToList();
                        foreach (var x in tmpList)
                        {
                            BestOfrQuotations.Add(x);
                        }

                        if (BestOfrQuotations.Count < 8)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                            }
                        }

                        BestOfrQuotationsMv.View.Refresh();
                        BestOfrSumQty = tmpList.Sum(p => p.qty);
                    }
                }
                else
                {
                    BestOfrQuotations.Clear();
                    BestOfrPrice = null;
                    BestOfrShowprice = "--";
                    BestOfrSumQty = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                    }
                }
            });
        }


        private static void SyncCollections(ObservableCollection<GoodsVolumeVoModel> collectionA, ObservableCollection<GoodsVolumeVoModel> collectionB)
        {
            // 删除在 A 中而不在 B 中的项
            // 删除在 A 中而不在 B 中的项
            var itemsToRemove = collectionA.Where(a => !collectionB.Any(b => b.Key == a.Key)).ToList();
            foreach (var item in itemsToRemove)
            {
                collectionA.Remove(item);
            }

            // 更新 A 中存在于 B 中的项
            foreach (var itemB in collectionB)
            {
                var itemA = collectionA.FirstOrDefault(a => a.Key == itemB.Key);
                if (itemA != null)
                {
                    itemA = itemB;
                }
                else
                {
                    // 如果 A 中没有 B 的项，则添加到 A 中
                    collectionA.Add(itemB);
                }
            }
        }
        /// <summary>
        /// 更新 多 数据
        /// </summary>
        public void UpdateMulitBestQuotations()
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                lock (lockbidObj)
                {
                    if (this.BidQuotations.Count<GoodsVolumeVoModel>() > 0)
                    {
                        //全
                        List<GoodsVolumeVoModel> fullmultirec = this.BidQuotations.Where(p => p.MDPriceLevel != 999).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel fullmodel = this.MultiBidBestQuotations.Where(p => p.id == "1").FirstOrDefault();
                        if (fullmultirec.Count > 0)
                        {
                            fullmodel.BestPrice = new decimal?(fullmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = fullmultirec.Where(p => p.price == fullmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            fullmodel.BestShowprice = fullmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(fullmodel.BestPrice), 4) : "-.-";
                            fullmodel.BestSumQty = tmpList.Sum( p=>p.qty);
                            //fullmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    fullmodel.ObsQuoteModel.Add(gvvm);
                            //}

                            SyncCollections(fullmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            fullmodel.BestPrice = null;
                            fullmodel.BestSumQty = 0M;
                            fullmodel.BestShowprice = "-.-";
                            fullmodel.ObsQuoteModel.Clear();
                        }

                        //+1
                        List<GoodsVolumeVoModel> onemultirec = this.BidQuotations.Where(p => p.MDPriceLevel != 999 && (p.DictSettleType.ContainsKey("2,3") || p.DictSettleType.ContainsKey("2")) || p.DictSettleType.ContainsKey("3")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel onemodel = this.MultiBidBestQuotations.Where(p => p.id == "2").FirstOrDefault();
                        if (onemultirec.Count > 0)
                        {
                            onemodel.BestPrice = new decimal?(onemultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = onemultirec.Where(p => p.price == onemodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            onemodel.BestShowprice = onemodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(onemodel.BestPrice), 4) : "-.-";
                            onemodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //onemodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    onemodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(onemodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            onemodel.BestPrice = null;
                            onemodel.BestSumQty = 0M;
                            onemodel.BestShowprice = "-.-";
                            onemodel.ObsQuoteModel.Clear();
                        }

                        //+0
                        List<GoodsVolumeVoModel> zeromultirec = this.BidQuotations.Where(p => p.MDPriceLevel != 999 && p.DictSettleType.ContainsKey("1")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel zeromodel = this.MultiBidBestQuotations.Where(p => p.id == "3").FirstOrDefault();
                        if (zeromultirec.Count > 0)
                        {
                            zeromodel.BestPrice = new decimal?(zeromultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = zeromultirec.Where(p => p.price == zeromodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            zeromodel.BestShowprice = zeromodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(zeromodel.BestPrice), 4) : "-.-";
                            zeromodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //zeromodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    zeromodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(zeromodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            zeromodel.BestPrice = null;
                            zeromodel.BestSumQty = 0M;
                            zeromodel.BestShowprice = "-.-";
                            zeromodel.ObsQuoteModel.Clear();
                        }

                        //远
                        List<GoodsVolumeVoModel> farmultirec = this.BidQuotations.Where(p => p.MDPriceLevel != 999 && p.settlType.Contains("+")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel farmodel = this.MultiBidBestQuotations.Where(p => p.id == "4").FirstOrDefault();
                        if (farmultirec.Count > 0)
                        {
                            farmodel.BestPrice = new decimal?(farmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = farmultirec.Where(p => p.price == farmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            farmodel.BestShowprice = farmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(farmodel.BestPrice), 4) : "-.-";
                            farmodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //farmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    farmodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(farmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            farmodel.BestPrice = null;
                            farmodel.BestSumQty = 0M;
                            farmodel.BestShowprice = "-.-";
                            farmodel.ObsQuoteModel.Clear();
                        }


                        //不限 +0 +1
                        List<GoodsVolumeVoModel> nolimitmultirec = this.BidQuotations.Where(p => p.MDPriceLevel != 999 && (p.DictSettleType.ContainsKey("2,3") || p.DictSettleType.ContainsKey("2")) || p.DictSettleType.ContainsKey("3") || p.DictSettleType.ContainsKey("1")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel nolimitmodel = this.MultiBidBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                        if (nolimitmultirec.Count > 0)
                        {
                            nolimitmodel.BestPrice = new decimal?(nolimitmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = nolimitmultirec.Where(p => p.price == nolimitmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            nolimitmodel.BestShowprice = nolimitmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(nolimitmodel.BestPrice), 4) : "-.-";
                            nolimitmodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //nolimitmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    nolimitmodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(nolimitmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            nolimitmodel.BestPrice = null;
                            nolimitmodel.BestSumQty = 0M;
                            nolimitmodel.BestShowprice = "-.-";
                            nolimitmodel.ObsQuoteModel.Clear();
                        }

                        //发布消息
                        //STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "MultiBestQuotationsUpdate", Info1 = CommonUtils.SerializeObject(MultiBidBestQuotations), Info2 = "1" });
                    }
                    else
                    {
                        //全
                        MultiBestQuoteModel fullmodel = this.MultiBidBestQuotations.Where(p => p.id == "1").FirstOrDefault();
                        fullmodel.BestPrice = null;
                        fullmodel.BestSumQty = 0M;
                        fullmodel.BestShowprice = "-.-";
                        fullmodel.ObsQuoteModel.Clear();
                        //+1
                        MultiBestQuoteModel onemodel = this.MultiBidBestQuotations.Where(p => p.id == "2").FirstOrDefault();
                        onemodel.BestPrice = null;
                        onemodel.BestSumQty = 0M;
                        onemodel.BestShowprice = "-.-";
                        onemodel.ObsQuoteModel.Clear();
                        //+0
                        MultiBestQuoteModel zeromodel = this.MultiBidBestQuotations.Where(p => p.id == "3").FirstOrDefault();
                        zeromodel.BestPrice = null;
                        zeromodel.BestSumQty = 0M;
                        zeromodel.BestShowprice = "-.-";
                        zeromodel.ObsQuoteModel.Clear();
                        //远
                        MultiBestQuoteModel farmodel = this.MultiBidBestQuotations.Where(p => p.id == "4").FirstOrDefault();
                        farmodel.BestPrice = null;
                        farmodel.BestSumQty = 0M;
                        farmodel.BestShowprice = "-.-";
                        farmodel.ObsQuoteModel.Clear();

                        //+0 +1
                        MultiBestQuoteModel nolimitmodel = this.MultiBidBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                        nolimitmodel.BestPrice = null;
                        nolimitmodel.BestSumQty = 0M;
                        nolimitmodel.BestShowprice = "-.-";
                        nolimitmodel.ObsQuoteModel.Clear();
                        //发布消息
                        //STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "MultiBestQuotationsUpdate", Info1 = CommonUtils.SerializeObject(MultiBidBestQuotations), Info2 = "1" });
                    }
                }
            }));

            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                lock (lockofrObj)
                {
                    if (this.OfrQuotations.Count<GoodsVolumeVoModel>() > 0)
                    {
                        //全
                        List<GoodsVolumeVoModel> fullmultirec = this.OfrQuotations.Where(p => p.MDPriceLevel != 999).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel fullmodel = this.MultiOfrBestQuotations.Where(p => p.id == "1").FirstOrDefault();
                        if (fullmultirec.Count > 0)
                        {
                            fullmodel.BestPrice = new decimal?(fullmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = fullmultirec.Where(p => p.price == fullmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            fullmodel.BestShowprice = fullmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(fullmodel.BestPrice), 4) : "-.-";
                            fullmodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //fullmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    fullmodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(fullmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            fullmodel.BestPrice = null;
                            fullmodel.BestSumQty = 0M;
                            fullmodel.BestShowprice = "-.-";
                            fullmodel.ObsQuoteModel.Clear();
                        }
                        //+1
                        List<GoodsVolumeVoModel> onemultirec = this.OfrQuotations.Where(p => p.MDPriceLevel != 999 && (p.DictSettleType.ContainsKey("2,3") || p.DictSettleType.ContainsKey("2")) || p.DictSettleType.ContainsKey("3")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel onemodel = this.MultiOfrBestQuotations.Where(p => p.id == "2").FirstOrDefault();
                        if (onemultirec.Count > 0)
                        {
                            onemodel.BestPrice = new decimal?(onemultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = onemultirec.Where(p => p.price == onemodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            onemodel.BestShowprice = onemodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(onemodel.BestPrice), 4) : "-.-";
                            onemodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //onemodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    onemodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(onemodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            onemodel.BestPrice = null;
                            onemodel.BestSumQty = 0M;
                            onemodel.BestShowprice = "-.-";
                            onemodel.ObsQuoteModel.Clear();
                        }

                        //+0
                        List<GoodsVolumeVoModel> zeromultirec = this.OfrQuotations.Where(p => p.MDPriceLevel != 999 && p.DictSettleType.ContainsKey("1")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel zeromodel = this.MultiOfrBestQuotations.Where(p => p.id == "3").FirstOrDefault();
                        if (zeromultirec.Count > 0)
                        {
                            zeromodel.BestPrice = new decimal?(zeromultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = zeromultirec.Where(p => p.price == zeromodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            zeromodel.BestShowprice = zeromodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(zeromodel.BestPrice), 4) : "-.-";
                            zeromodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //zeromodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    zeromodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(zeromodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            zeromodel.BestPrice = null;
                            zeromodel.BestSumQty = 0M;
                            zeromodel.BestShowprice = "-.-";
                            zeromodel.ObsQuoteModel.Clear();
                        }

                        //远
                        List<GoodsVolumeVoModel> farmultirec = this.OfrQuotations.Where(p => p.MDPriceLevel != 999 && p.settlType.Contains("+")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel farmodel = this.MultiOfrBestQuotations.Where(p => p.id == "4").FirstOrDefault();
                        if (farmultirec.Count > 0)
                        {
                            farmodel.BestPrice = new decimal?(farmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = farmultirec.Where(p => p.price == farmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            farmodel.BestShowprice = farmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(farmodel.BestPrice), 4) : "-.-";
                            farmodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //farmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    farmodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(farmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            farmodel.BestPrice = null;
                            farmodel.BestSumQty = 0M;
                            farmodel.BestShowprice = "-.-";
                            farmodel.ObsQuoteModel.Clear();
                        }


                        //不限 +0 +1
                        List<GoodsVolumeVoModel> nolimitmultirec = this.OfrQuotations.Where(p => p.MDPriceLevel != 999 && (p.DictSettleType.ContainsKey("2,3") || p.DictSettleType.ContainsKey("2")) || p.DictSettleType.ContainsKey("3") || p.DictSettleType.ContainsKey("1")).OrderBy(p => p.MDPriceLevel).ToList();
                        MultiBestQuoteModel nolimitmodel = this.MultiOfrBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                        if (nolimitmultirec.Count > 0)
                        {
                            nolimitmodel.BestPrice = new decimal?(nolimitmultirec.First<GoodsVolumeVoModel>().price);
                            List<GoodsVolumeVoModel> tmpList = nolimitmultirec.Where(p => p.price == nolimitmodel.BestPrice).OrderBy(p => p.MDPriceLevel).ToList();
                            nolimitmodel.BestShowprice = nolimitmodel.BestPrice.HasValue ? CommonUtils.formatPriceString(CommonUtils.ConvertToString(nolimitmodel.BestPrice), 4) : "-.-";
                            nolimitmodel.BestSumQty = tmpList.Sum(p => p.qty);
                            //nolimitmodel.ObsQuoteModel.Clear();
                            //foreach (GoodsVolumeVoModel gvvm in tmpList)
                            //{
                            //    nolimitmodel.ObsQuoteModel.Add(gvvm);
                            //}
                            SyncCollections(nolimitmodel.ObsQuoteModel, new ObservableCollection<GoodsVolumeVoModel>(tmpList));
                        }
                        else
                        {
                            nolimitmodel.BestPrice = null;
                            nolimitmodel.BestSumQty = 0M;
                            nolimitmodel.BestShowprice = "-.-";
                            nolimitmodel.ObsQuoteModel.Clear();
                        }

                        //发布消息
                        //STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "MultiBestQuotationsUpdate", Info1 = CommonUtils.SerializeObject(MultiOfrBestQuotations) , Info2 = "0"});
                    }
                    else
                    {
                        //全
                        MultiBestQuoteModel fullmodel = this.MultiOfrBestQuotations.Where(p => p.id == "1").FirstOrDefault();
                        fullmodel.BestPrice = null;
                        fullmodel.BestSumQty = 0M;
                        fullmodel.BestShowprice = "-.-";
                        fullmodel.ObsQuoteModel.Clear();
                        //+1
                        MultiBestQuoteModel onemodel = this.MultiOfrBestQuotations.Where(p => p.id == "2").FirstOrDefault();
                        onemodel.BestPrice = null;
                        onemodel.BestSumQty = 0M;
                        onemodel.BestShowprice = "-.-";
                        onemodel.ObsQuoteModel.Clear();
                        //+0
                        MultiBestQuoteModel zeromodel = this.MultiOfrBestQuotations.Where(p => p.id == "3").FirstOrDefault();
                        zeromodel.BestPrice = null;
                        zeromodel.BestSumQty = 0M;
                        zeromodel.BestShowprice = "-.-";
                        zeromodel.ObsQuoteModel.Clear();
                        //远
                        MultiBestQuoteModel farmodel = this.MultiOfrBestQuotations.Where(p => p.id == "4").FirstOrDefault();
                        farmodel.BestPrice = null;
                        farmodel.BestSumQty = 0M;
                        farmodel.BestShowprice = "-.-";
                        farmodel.ObsQuoteModel.Clear();

                        //+0 +1
                        MultiBestQuoteModel nolimitmodel = this.MultiOfrBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                        nolimitmodel.BestPrice = null;
                        nolimitmodel.BestSumQty = 0M;
                        nolimitmodel.BestShowprice = "-.-";
                        nolimitmodel.ObsQuoteModel.Clear();

                        //发布消息
                        //STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "MultiBestQuotationsUpdate", Info1 = CommonUtils.SerializeObject(MultiOfrBestQuotations), Info2 = "0" });
                    }
                }
            }));
        }

        /// <summary>
        /// 更新 最优价 数据
        /// </summary>
        public void UpdateBestQuotations()
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                lock (lockbidObj)
                {
                    if (BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Count() > 0)
                    {
                        GoodsVolumeVoModel list = BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).OrderBy(p => p.MDPriceLevel).First();
                        if (list != null)
                        {
                            BestBidPrice = list.price;
                            BestBidShowprice = list.showprice;

                            BestBidQuotations.Clear();
                            List<GoodsVolumeVoModel> tmpList = BidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.price == list.price && p.MDPriceLevel != 999).ToList();
                            foreach (var x in tmpList)
                            {
                                BestBidQuotations.Add(x);
                            }

                            if (BestBidQuotations.Count < 20)
                            {
                                for (int i = 0; i < 20; i++)
                                {
                                    BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                                }
                            }

                            BestBidQuotationsMv.View.Refresh();
                            BestBidSumQty = tmpList.Sum(p => p.qty);

                            //V2.4 如果同一机构，同一交易员有两个报价，一个明盘，一个暗盘，如果两个价格相同，此时点价，只能显示一个量。
                            var lst = tmpList.Where(p => p.HandlInst == "1").ToList();
                            if (lst.Count() > 0)
                            {
                                foreach (GoodsVolumeVoModel obj in lst)
                                {
                                    //BidQuotations
                                    foreach (GoodsVolumeVoModel x in BidQuotations)
                                    {
                                        //HandlInst":"2" 2明盘 1是暗盘
                                        if (x.institutionID == obj.institutionID && x.traderID == obj.traderID && x.price == obj.price && x.HandlInst == "2")
                                        {
                                            x.quoteInternalFlag = "3";
                                        }
                                    }
                                }

                                QuoteConfigHandler.Instance.TriggerBaseDataUpdated("");
                            }

                        }
                    }
                    else
                    {
                        BestBidQuotations.Clear();
                        this.BestBidPrice = null;
                        this.BestBidShowprice = "--";
                        this.BestBidSumQty = 0;
                        for (int i = 0; i < 20; i++)
                        {
                            BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                        }
                    }
                }
            }));

            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                lock (lockofrObj)
                {
                    if (OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Count() > 0)
                    {
                        GoodsVolumeVoModel list = OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).OrderBy(p => p.MDPriceLevel).First();
                        if (list != null)
                        {
                            BestOfrPrice = list.price;
                            BestOfrShowprice = list.showprice;

                            BestOfrQuotations.Clear();
                            List<GoodsVolumeVoModel> tmpList = OfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.price == list.price && p.MDPriceLevel != 999).ToList();
                            foreach (var x in tmpList)
                            {
                                BestOfrQuotations.Add(x);
                            }

                            if (BestOfrQuotations.Count < 20)
                            {
                                for (int i = 0; i < 20; i++)
                                {
                                    BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                                }
                            }

                            BestOfrQuotationsMv.View.Refresh();
                            BestOfrSumQty = tmpList.Sum(p => p.qty);

                            //V2.4 如果同一机构，同一交易员有两个报价，一个明盘，一个暗盘，如果两个价格相同，此时点价，只能显示一个量。
                            var lst = tmpList.Where(p => p.HandlInst == "1").ToList();
                            if (lst.Count() > 0)
                            {
                                foreach (GoodsVolumeVoModel obj in lst)
                                {
                                    //BidQuotations
                                    foreach (GoodsVolumeVoModel x in OfrQuotations)
                                    {
                                        //HandlInst":"2" 2明盘 1是暗盘
                                        if (x.institutionID == obj.institutionID && x.traderID == obj.traderID && x.price == obj.price && x.HandlInst == "2")
                                        {
                                            x.quoteInternalFlag = "3";
                                        }
                                    }
                                }

                                QuoteConfigHandler.Instance.TriggerBaseDataUpdated("");
                            }
                        }
                    }
                    else
                    {
                        BestOfrQuotations.Clear();
                        BestOfrPrice = null;
                        BestOfrShowprice = "--";
                        BestOfrSumQty = 0;
                        for (int i = 0; i < 20; i++)
                        {
                            BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                        }
                    }
                }
            }));
        }

        #endregion

        #region 撤单信息
        /// <summary>
        /// 获取撤单列表
        /// </summary>
        public void GetSelfQuotations()
        {

            List<GoodsVolumeVoModel> tmpBidList = BidQuotations.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999 && Utils.SystemPowerHandler.Instance.IsCanEditOrder(p.brokerID)).ToList();
            List<GoodsVolumeVoModel> tmpOfrList = OfrQuotations.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999 && Utils.SystemPowerHandler.Instance.IsCanEditOrder(p.brokerID)).ToList();
            SelfQuotations.Clear();
            foreach (GoodsVolumeVoModel gv in tmpBidList)
            {
                SelfQuotations.Add(gv);
            }
            foreach (GoodsVolumeVoModel gv in tmpOfrList)
            {
                SelfQuotations.Add(gv);
            }
        }

        /// <summary>
        /// 撤单
        /// </summary>
        public void UnRefQuotations(object inparam)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GoodsVolumeVoModel bidcur = SelfQuotations.Where(p => p.id == (string)inparam).FirstOrDefault();
                if (bidcur != null)
                {
                    if (bidcur.MDPriceLevel != 999)
                    {
                        OrderCancelRequest cancelPo = new OrderCancelRequest();
                        cancelPo.AlgoName = "BROKER";
                        cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                        cancelPo.OrderID = bidcur.id;
                        TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
                        if (reply != null)
                        {
                            if (reply.Code == "0000")
                            {
                            }
                            else
                            {
                                Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                            }
                        }
                    }
                }
            });
        }
        #endregion

        #region Command Proc
        public void OnBestBidCommand(object inparam)
        {
            if (this.MarketIndicator == "5")
                return;

            if (inparam != null)
            {
                OnGVNCommand(inparam);
            }
            else
            {
                return;
            }
        }

        public void OnBestOfrCommand(object inparam)
        {

            if (this.MarketIndicator == "5")
                return;

            if (inparam != null)
            {
                 OnTKNCommand(inparam);
            }
            else
            {
                return;
            }
        }


        /// <summary>
        /// GVN Button Command
        /// </summary>
        /// <param name="inparam"></param>
        public void OnGVNCommand(object inparam)
        {
            if (inparam != null)
            {
                //如果是双击点价
                GoodsVolumeVoModel cur = BestBidQuotations.Where(p => p.id == (string)inparam && p.MDPriceLevel != 999).FirstOrDefault();
                if (cur != null)
                {
                    this.SelBidQuotations = cur;
                    string settle = this.SelBidQuotations.settlType;
                    if (this.SelBidQuotations.DictSettleType.Count > 1)
                    {
                        foreach (string key in this.SelBidQuotations.DictSettleType.Keys)
                        {
                            if (key == "1")
                            {
                                settle = key;
                                break;
                            }

                            if (key == "2" || key == "3" || key == "2,3")
                            {
                                settle = "2,3";
                                break;
                            }
                        }

                        if (SettleFilter == "远")
                        {
                            settle = this.SelBidQuotations.DictSettleType.Keys.ToList().Where(p => p.Contains("+")).FirstOrDefault().ToString();
                        }
                    }

                    SendBorkerPriceCommand(GetGoodPo(), BidQuotations, true, this.SelBidQuotations.price, this.SelBidQuotations.id, settle);
                }
                else
                {
                    return;
                }
            }
            else
            {

                //点击GVN按钮
                string settle = string.Empty;
                //清算速度= 【多】
                if (SettleFilter == "多")
                {
                    //点击TKN/GVN按钮。点价窗口中，筛选器中清算速度默认为<+1><明天+0>，支持切换为<不限><+0>。此时不展示远期按钮。
                    settle = "2,3";
                    MultiBestQuoteModel onemodel = this.MultiBidBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                    if (onemodel.BestPrice.HasValue)
                    {
                        SendBorkerPriceCommand(GetGoodPo(), onemodel.ObsQuoteModel, true, onemodel.BestPrice.Value, null, settle, "5");
                    }
                }
                else
                {
                    //报价板清算速度 ≠【多】 "远" 不可用
                    ObservableCollection<GoodsVolumeVoModel> lst = new ObservableCollection<GoodsVolumeVoModel>(BestBidQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).ToList());
                    if (lst.Count > 0)
                    {
                        switch (SettleFilter)
                        {
                            case "+1":
                                settle = "2,3";
                                break;
                            case "+0":
                                settle = "1";
                                break;
                        }

                        SendBorkerPriceCommand(GetGoodPo(), BidQuotations, true, lst[0].price, null, settle);
                    }
                }
            }

        }


        /// <summary>
        /// TKN Button Command
        /// </summary>
        /// <param name="inparam"></param>
        public void OnTKNCommand(object inparam)
        {
            if (inparam != null)
            {
                //如果是双击点价
                GoodsVolumeVoModel cur = BestOfrQuotations.Where(p => p.id == (string)inparam && p.MDPriceLevel != 999).FirstOrDefault();
                if (cur != null)
                {
                    this.SelOfrQuotations = cur;
                    string settle = this.SelOfrQuotations.settlType;
                    if (this.SelOfrQuotations.DictSettleType.Count > 1)
                    {
                        foreach (string key in this.SelOfrQuotations.DictSettleType.Keys)
                        {
                            if (key == "1")
                            {
                                settle = key;
                                break;
                            }

                            if (key == "2" || key == "3" || key == "2,3")
                            {
                                settle = "2,3";
                                break;
                            }
                        }
                    }

                    if (SettleFilter == "远")
                    {
                        settle = this.SelOfrQuotations.DictSettleType.Keys.ToList().Where(p => p.Contains("+")).FirstOrDefault().ToString();
                    }

                    //SendBorkerPriceCommand(GetGoodPo(), new ObservableCollection<GoodsVolumeVoModel>(OfrQuotations.Where(p => p.MDPriceLevel != 999 && p.price == this.SelOfrQuotations.price).ToList()), false, this.SelOfrQuotations.price, this.SelOfrQuotations.id, this.SelOfrQuotations.settlType);
                    SendBorkerPriceCommand(GetGoodPo(), OfrQuotations, false, this.SelOfrQuotations.price, this.SelOfrQuotations.id, settle);
                }
            }
            else
            {
                //点击TKN按钮
                string settle = string.Empty;
                //清算速度= 【多】
                if (SettleFilter == "多")
                {
                    //点击TKN/GVN按钮。点价窗口中，筛选器中清算速度默认为<+1><明天+0>，支持切换为<不限><+0>。此时不展示远期按钮。
                    settle = "2,3";
                    MultiBestQuoteModel onemodel = this.MultiOfrBestQuotations.Where(p => p.id == "5").FirstOrDefault();
                    if (onemodel.BestPrice.HasValue)
                    {
                        SendBorkerPriceCommand(GetGoodPo(), onemodel.ObsQuoteModel, false, onemodel.BestPrice.Value, null, settle, "5");
                    }
                }
                else
                {
                    //报价板清算速度 ≠【多】 "远" 不可用
                    ObservableCollection<GoodsVolumeVoModel> lst = new ObservableCollection<GoodsVolumeVoModel>(BestOfrQuotationsMv.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999).ToList());
                    if (lst.Count > 0)
                    {
                        switch (SettleFilter)
                        {
                            case "+1":
                                settle = "2,3";
                                break;
                            case "+0":
                                settle = "1";
                                break;
                        }

                        SendBorkerPriceCommand(GetGoodPo(), OfrQuotations, false, lst[0].price, null, settle);
                    }
                }
            }
        }


        private void SendBorkerPriceCommand(GoodsPo gp, ObservableCollection<GoodsVolumeVoModel> lstgoodsvolume, bool isBid, decimal price, string quoteid, string selsettle, string id = null)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke((Action)(async () =>
                {
                    SendBrokerPriceView uinew = new SendBrokerPriceView();
                    uinew.Init(gp, lstgoodsvolume, isBid, price, quoteid, selsettle, id);
                    uinew.ShowActivated = true;
                    STC.Common.Utils.WindowManager.Instance.OpenOnlyOneWindow($"SendBrokerPriceView-{isBid.ToString()}-{quoteid}{id??String.Empty}", uinew);
                    //uinew.ShowActivated = true;
                    //uinew.Show();
                }),DispatcherPriority.Send);
            });
        }

        /// <summary>
        /// 多 Bid  双击点价
        /// </summary>
        /// <param name="inparam"></param>
        public void OnMultiBestBidCommand(object inparam)
        {
            if (this.MarketIndicator == "5")
                return;

            //如果是双击点价
            MultiBestQuoteModel cur = this.MultiBidBestQuotations.Where(p => p.id == (string)inparam).FirstOrDefault();
            if (cur != null)
            {
                if (cur.BestPrice.HasValue)
                {
                    string settle = string.Empty;
                    switch (cur.SettlType)
                    {
                        case "全":
                            settle = "1,2,3";
                            break;
                        case "+1":
                            settle = "2,3";
                            break;
                        case "+0":
                            settle = "1";
                            break;
                        case "远":
                            settle = "+";

                            if (cur.ObsQuoteModel.Count > 1)
                            {
                                CommonUtils.Prompt2Second("提醒", "远期有多个报价\r\n不支持在<多>进行直接点价。\r\n需切换到<远>后才能点价!");
                                return;
                            }
                            else
                            {
                                settle = cur.ObsQuoteModel[0].settlType;
                            }
                            break;
                    }

                    SendBorkerPriceCommand(GetGoodPo(), cur.ObsQuoteModel, true, cur.BestPrice.Value, null, settle, cur.id);
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 多 Ofr  双击点价
        /// </summary>
        public void OnMultiBestOfrCommand(object inparam)
        {
            if (this.MarketIndicator == "5")
                return;

            MultiBestQuoteModel cur = this.MultiOfrBestQuotations.Where(p => p.id == (string)inparam).FirstOrDefault();
            if (cur != null)
            {
                if (cur.BestPrice.HasValue)
                {
                    string settle = string.Empty;
                    switch (cur.SettlType)
                    {
                        case "全":
                            settle = "1,2,3";
                            break;
                        case "+1":
                            settle = "2,3";
                            break;
                        case "+0":
                            settle = "1";
                            break;
                        case "远":
                            settle = "+";
                            if (cur.ObsQuoteModel.Count > 1)
                            {
                                CommonUtils.Prompt2Second("提醒", "远期有多个报价\r\n不支持在<多>进行直接点价。\r\n需切换到<远>后才能点价!");
                                return;
                            }
                            else
                            {
                                settle = cur.ObsQuoteModel[0].settlType;
                            }
                            break;
                    }

                    SendBorkerPriceCommand(GetGoodPo(), cur.ObsQuoteModel, false, cur.BestPrice.Value, null, settle, cur.id);
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="inparam"></param>
        public void OnCloseCommand(object inparam)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (myWindow != null)
                {
                    myWindow.Close();
                    GC.Collect();
                }
            });
        }

        private GoodsPo GetGoodPo()
        {
            GoodsPo gp = new GoodsPo();
            ////if (this.HomeGroupInfo != null)
            ////{
            ////    gp.GoodsCode = this.HomeGroupInfo.securityid;
            ////    gp.GoodsShortname = this.HomeGroupInfo.symbol;
            ////    gp.MarketIndicator = this.MarketIndicator;
            ////}
            ////else
            {
                gp.GoodsCode = SecurityID;
                gp.MarketIndicator = this.MarketIndicator;
                gp.GoodsShortname = this.SymbolName;
            }

            return gp;
        }

        /// <summary>
        /// 删除债券
        /// </summary>
        public void DelSecurityID()
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                string oldsec = oldSecurityID;
                SecurityID = string.Empty;
                IsSearchSecurityIDVisible = Visibility.Collapsed;
                IsAllControlVisible = Visibility.Hidden;
                MoreMenuButtonVisible = Visibility.Collapsed;
                IsAddSecurityIDVisible = Visibility.Visible;
                InitBestBidOfrQuotationData();
                //通知Broker系统
                InstsertOrDeleteBonds(oldsec, "3");
                //QuoteConfigHandler.Instance.Notify(HomePageDataHandler.FavoriteTypeEnum.DelBond, this.CurCardItem.GroupId, oldsec, CurCardItem);
            }));
        }


        /// <summary>
        /// 直接点击打开小窗，则自动置顶，如果是接受消息打开也自动置顶
        /// </summary>
        public void PopupSecurityID()
        {
            //发布消息
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBorkerQuotesMiniWindows", Info1 = SecurityID });
        }
  
        public void OnAddBoundCommand(object inparam)
        {
            IsAddSecurityIDVisible = Visibility.Collapsed;
            IsAllControlVisible = Visibility.Hidden;
            IsSearchSecurityIDVisible = Visibility.Visible;
            MoreMenuButtonVisible = Visibility.Collapsed;
            CustomBrokerSearchTextBox searchEditUI = this.mywin.FindName("SerachText") as CustomBrokerSearchTextBox;
            searchEditUI.SetFocus();
        }

        #endregion

        #region 回调
        public void SubscribeCtpPrice(string securityid)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    PricingHandler.Instance.SubscribeCTPriceInfo(securityid, Instance_OnCTPrice);
                }
                catch (Exception e)
                {
                    LogHelper.Error("RegisterCTPrice", e);
                }
            });
        }

        /// <summary>
        /// 深度行情回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (obj != null)
                        {
                            if (obj.NoMDEntries == null)
                            {
                                return;
                            }

                            if (obj.SecurityID != this.SecurityID)
                                return;

                            //MDEntryType 0 bid 1 ofr
                            foreach (var item in BidQuotations)
                            {
                                // 设置 DelFlag 属性为 1
                                item.DeleteFlag = "1";
                            }

                            foreach (var item in OfrQuotations)
                            {
                                // 设置 DelFlag 属性为 1
                                item.DeleteFlag = "1";
                            }
                            
                            // 添加和更新数据(普通)
                            foreach (GNoMDEntries entry in obj.NoMDEntries)
                            {

                                if (entry.VenueTypeGW.ToUpper() != "INNER")
                                    continue;

                                if (entry.MDEntryPx == "--")
                                    continue;

                                string key = entry.MDEntryID + entry.MDEntryType;
                                string securityID = obj.SecurityID;
                                string venues = entry.Venues;
                                string venueTypeGW = entry.VenueTypeGW;
                                string time = entry.MDEntryTime;
                                string text = entry.Text;
                                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                                decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);
                                //去掉无量的报价
                                if (string.IsNullOrEmpty(entry.MDEntrySize))
                                {
                                    continue;
                                }
                                else if (entry.MDEntrySize == "--")
                                {
                                    continue;
                                }
                                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                                //string partySubName = GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);
                                string side = entry.MDEntryType;
                                string settlType = entry.SettlType;
                                List<string> uniqeset = settlType.Split(',').ToList<string>();
                                Dictionary<string, string> dictuniqeset = new Dictionary<string, string>();
                                for (int i = 0; i < uniqeset.Count; i++)
                                {
                                    if (!dictuniqeset.ContainsKey(uniqeset[i]))
                                        dictuniqeset.Add(uniqeset[i], uniqeset[i]);
                                }

                                int mdPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                                string bargainFlag = entry.BargainFlag;

                                string wholeFlag = entry.WholeFlag;
                                string traderName = entry.TraderName;

                                string showTrader = entry.ShowTrader;
                                
                                string transactTime = string.Format("{0} {1}", entry.MDEntryDate, entry.MDEntryTime);
                                string deliveryType = entry.DeliveryType;
                                string clearingMethod = entry.ClearingMethod;
                                string quoteType = entry.QuoteTransType;
                                string partyName = entry.PartyName;
                                string partyID = string.Empty;

                                string quoteStatus = entry.QuoteStatus;

                                //string isIncomplete = IsIncomplete(entry, obj);
                                string isInner = entry.VenueTypeGW == "INNER" ? "1" : "0";

                                string info3 = entry.Info3;
                                string info4 = entry.Info4;

                                string marketIndicator = obj.MarketIndicator;

                                //新增处理显示字段
                                string yield = string.Empty;
                                string calTypeFieldTip = string.Empty; // 
                                string calTypeNetPriceTip = string.Empty; // 
                                string priceType = string.Empty;
                                string calFlag = string.Empty;

                                if (marketIndicator != "5")
                                {
                                    //设置PriceType
                                    if (!string.IsNullOrEmpty(priceType))
                                    {
                                        if (string.IsNullOrEmpty(CommonUtils.GetPriceType(obj.SecurityID)))
                                        {
                                            CommonUtils.SetPriceType(obj.SecurityID, priceType);
                                        }
                                    }

                                    //收益率
                                    yield = entry.Yield == "--" ? "--" : CommonUtils.formatString(CommonUtils.ConvertToString(entry.Yield), 4);
                                    priceType = entry.PriceType;

                                    // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                                    // exerciseFlag 0=行权收益率,1或空=到期收益率
                                    //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0 null
                                }
                                else if (marketIndicator == "5" && string.IsNullOrEmpty(yield))
                                {
                                    yield = CommonUtils.ConvertToString(entry.MDEntryPx);
                                }

                                string exerciseflag = entry.ExerciseFlag ?? "1"; // 0=行权收益率,1或空=到期收益率

                                GoodsVolumeVoModel findDr = null;
                                bool isfound = false;
                                if (side == CommonConstants.SIDE_OFR)
                                {
                                    findDr = OfrQuotations.FirstOrDefault(a => a.Key == key);
                                    if (findDr == null)
                                    {
                                        isfound = false;
                                        findDr = new GoodsVolumeVoModel();
                                        findDr.Key = key;
                                        findDr.IsBid = false;
                                        findDr.goodCode = securityID;
                                        findDr.Source = venues;
                                        findDr.VenueTypeGW = venueTypeGW;
                                        findDr.Side = side;
                                        findDr.FlashCount = 4;
                                        findDr.DeleteFlag = "0";
                                        findDr.quoteInternalFlag = entry.HandlInst;
                                        findDr.HandlInst = entry.HandlInst;
                                    }
                                    else
                                    {
                                        findDr.brokerID = entry.PartySubID;
                                        findDr.brokerName = entry.PartySubName;
                                        findDr.traderID = entry.TraderID;
                                        findDr.traderName = entry.TraderName;
                                        findDr.institutionID = entry.InstitutionID;
                                        findDr.institutionName = entry.InstitutionName;
                                        if (string.IsNullOrEmpty(entry.InstitutionName))
                                        {
                                            findDr.institutionName = CommonUtils.GetCrmInstInstiid(entry.InstitutionID)?.shortnamecn;
                                        }

                                        findDr.DeleteFlag = "0";
                                        if (findDr.qty != qty || findDr.settlType != settlType || findDr.institutionID != entry.InstitutionID || findDr.institutionName!=entry.InstitutionName || findDr.traderID!=entry.TraderID || findDr.traderName!=entry.TraderName || findDr.description != entry.Text || findDr.HandlInst != entry.HandlInst)
                                        {
                                            findDr.FlashCount = 4;
                                        }
                                        
                                        isfound = true;
                                    }
                                }
                                else if (side == CommonConstants.SIDE_BID)
                                {
                                    findDr = BidQuotations.FirstOrDefault(a => a.Key == key);
                                    if (findDr == null)
                                    {
                                        isfound = false;
                                        findDr = new GoodsVolumeVoModel();
                                        findDr.Key = key;
                                        findDr.IsBid = true;
                                        findDr.goodCode = securityID;
                                        findDr.Source = venues;
                                        findDr.VenueTypeGW = venueTypeGW;
                                        findDr.Side = side;
                                        findDr.FlashCount = 4;
                                    }
                                    else
                                    {
                                        findDr.DeleteFlag = "0";
                                        if (findDr.qty != qty || findDr.settlType != settlType || findDr.institutionID != entry.InstitutionID || findDr.institutionName != entry.InstitutionName || findDr.traderID != entry.TraderID || findDr.traderName != entry.TraderName || findDr.description != entry.Text || findDr.HandlInst != entry.HandlInst)
                                        {
                                            findDr.FlashCount = 4;
                                        }
                                        isfound = true;
                                    }
                                }

                                //换券标识 true: 换券; false 不换券
                                findDr.ExchangeSignFlag = string.IsNullOrEmpty(entry.ExchangeSignFlag) ? false : entry.ExchangeSignFlag == "0" ? false : true;
                                //议价标识 0:无星; 1: 1星; 2: 2星
                                findDr.bargainFlag = entry.BargainFlag;
                                //OCO 打包 1:oco 2:打包
                                findDr.ocoFlag = entry.OCOFlag ?? "";
                                findDr.brokerID = entry.PartySubID;
                                findDr.brokerName = entry.PartySubName;
                                findDr.traderID = entry.TraderID;
                                findDr.traderName = entry.TraderName;
                                findDr.institutionID = entry.InstitutionID;
                                findDr.institutionName = entry.InstitutionName;
                                if (string.IsNullOrEmpty(entry.InstitutionName))
                                {
                                    findDr.institutionName = CommonUtils.GetCrmInstInstiid(entry.InstitutionID)?.shortnamecn;
                                }

                                /// 内部报价标识
                                /// 2：内部报价，1：正常报价 0:待填写
                                //HandlInst":"2" 2明盘 1是暗盘
                                findDr.quoteInternalFlag = entry.HandlInst;
                                findDr.HandlInst = entry.HandlInst;

                                //市场标识 废弃
                                findDr.bondKeyListedMarket = "";

                                findDr.flagUrgent = entry.FirmFlag == "1" ? true : false;
                                findDr.FirmFlag = entry.FirmFlag;
                                //报价类型，2线上，1线下
                                findDr.onLine = entry.OnLine;

                                findDr.qty = qty;

                                string price = string.Empty;

                                price = CommonUtils.formatPriceString(CommonUtils.ConvertToString(entry.MDEntryPx), 4);
                                findDr.price = price == "--" ? 0 : CommonUtils.ConvertToDecimal(price);

                                findDr.showprice = price;
                                findDr.time = time;
                                findDr.settlementType = uniqeset.ToArray();
                                findDr.settlType = settlType;
                                findDr.DictSettleType = dictuniqeset;
                                string memoInfo9 = entry.Info9 ?? settlType;
                                findDr.Info9 = memoInfo9;
                                findDr.id = entry.MDEntryID;
                                findDr.ExerciseFlag = exerciseflag;
                                findDr.description = entry.Text;
                                findDr.FanDian = entry.FanDian;
                                findDr.FanDianFlag = entry.FanDianFlag ?? "0";
                                findDr.PriceType = priceType;
                                findDr.MDPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                                findDr.TransactTime = transactTime;
                                findDr.MDEntryDate = entry.MDEntryDate;
                                findDr.wholeFlag = entry.WholeFlag;
                                /// 内部报价标识
                                /// 2：内部报价，1：正常报价 
                                //HandlInst":"2" 2明盘 1是暗盘
                                findDr.quoteInternalFlag = entry.HandlInst ?? "2";
                                findDr.HandlInst = entry.HandlInst ?? "2";
                                // TradeVolume RoleName QuoteStatus PriceType Yield  NetPrice VipCode PartDealFlag MDReqID  MDEntrySize MDEntryDate InternalFlag Info2 Handllnst FanDianFlag
                                if (!isfound)
                                {
                                    if (side == "1")
                                    {
                                        OfrQuotations.Add(findDr);
                                    }
                                    else
                                    {
                                        BidQuotations.Add(findDr);
                                    }
                                }
                            }

                            //更新数据后处理流程
                            var bidToRemove = BidQuotations.Where(person => person.DeleteFlag == "1").ToList();
                            foreach (var person in bidToRemove)
                            {
                                BidQuotations.Remove(person);
                            }
                            var ofrToRemove = OfrQuotations.Where(person => person.DeleteFlag == "1").ToList();
                            foreach (var person in ofrToRemove)
                            {
                                OfrQuotations.Remove(person);
                            }

                            if (obj.SecurityID == "101404.SH")
                            {
                                //string ss = CommonUtils.SerializeObject(obj.NoMDEntries);
                            }

                            if (BidQuotations.Count == 0)
                            {
                                //如果已经没有记录，则清空最优价格
                                BestBidQuotations.Clear();
                                this.BestBidPrice = null;
                                this.BestBidShowprice = "--";
                                this.BestBidSumQty = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    BestBidQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                                }
                            }

                            if (OfrQuotations.Count == 0)
                            {
                                //如果已经没有记录，则清空最优价格
                                BestOfrQuotations.Clear();
                                BestOfrPrice = null;
                                BestOfrShowprice = "--";
                                BestOfrSumQty = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    BestOfrQuotations.Add(new GoodsVolumeVoModel() { id = "", qty = 0, MDPriceLevel = 999 });
                                }
                            }

                            this.UpdateMulitBestQuotations();
                            this.UpdateBestQuotations();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                });
            });
        }

        #endregion

        #region 查询，删除债券信息
        /// <summary>
        /// 获取债券信息
        /// </summary>
        private void GetSymbolInfo()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {

                    if (!string.IsNullOrEmpty(SecurityID))
                    {
                        _SecurityID = SecurityID;
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.SecurityID);
                        if (bondsInfoPo != null)
                        {
                            SymbolName = bondsInfoPo.Short_Name;
                            RemainingTime = bondsInfoPo.RemaingingTime;

                            RemaingingTimeConvert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                            BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                            MarketIndicator = bondsInfoPo.MarketIndicator;

                            if (CurCardItem == null)
                            {
                                CurCardItem = new CardItem();
                            }
                            CurCardItem.SecurityID = SecurityID;
                            CurCardItem.SymbolName = SymbolName;
                            CurCardItem.BondCategory = BondCategory;
                            CurCardItem.BondType = bondsInfoPo.Bond_Subtype;
                            CurCardItem.RemaingingTime = bondsInfoPo.RemaingingTime;
                            CurCardItem.RemaingingTimeConvert = RemaingingTimeConvert;


                            IsAddSecurityIDVisible = Visibility.Collapsed;
                            IsAllControlVisible = Visibility.Visible;
                            if (this.PopupCloseButtonVisible == Visibility.Visible)
                                MoreMenuButtonVisible = Visibility.Collapsed;
                            else
                                MoreMenuButtonVisible = Visibility.Visible;

                            //if (this.HomeGroupInfo != null)
                            if (!string.IsNullOrEmpty(this.SecurityID))
                            {
                                ////HomeGroupInfo.securityid = SecurityID;
                                ////HomeGroupInfo.symbol = SymbolName;
                                
                                if (CurCardItem == null)
                                {
                                    CurCardItem = new CardItem();
                                }

                                CurCardItem.SecurityID = SecurityID;
                                CurCardItem.SymbolName = SymbolName;
                                CurCardItem.BondCategory = BondCategory;
                                CurCardItem.BondType = bondsInfoPo.Bond_Subtype;
                                CurCardItem.RemaingingTime = bondsInfoPo.RemaingingTime;
                                CurCardItem.RemaingingTimeConvert = RemaingingTimeConvert;

                                //插入债券
                                InstsertOrDeleteBonds(this.SecurityID);

                                //通知Broker系统
                                QuoteConfigHandler.Instance.Notify(HomePageDataHandler.FavoriteTypeEnum.UpdBond, CurCardItem.GroupId, _SecurityID, CurCardItem);
                            }
                        }
                        else
                        {
                            IsSearchSecurityIDVisible = Visibility.Visible;
                            IsAllControlVisible = Visibility.Hidden;
                            IsAddSecurityIDVisible = Visibility.Collapsed;

                            CurCardItem.SecurityID = "";
                            CurCardItem.SymbolName = "";
                            CurCardItem.BondCategory = "";
                            CurCardItem.BondType = "";
                            CurCardItem.RemaingingTime = "";
                            CurCardItem.RemaingingTimeConvert = 0;
                        }
                    }
                    else
                    {
                        IsSearchSecurityIDVisible = Visibility.Visible;
                        IsAllControlVisible = Visibility.Hidden;
                        IsAddSecurityIDVisible = Visibility.Collapsed;
                        CurCardItem.SecurityID = "";
                        CurCardItem.SymbolName = "";
                        CurCardItem.BondCategory = "";
                        CurCardItem.BondType = "";
                        CurCardItem.RemaingingTime = "";
                        CurCardItem.RemaingingTimeConvert = 0;
                    }
                }));
            });
        }


        #region 删除债券
        private void InstsertOrDeleteBonds(string securityid, string action = "1")
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                {
                    HomeGroupSecurityidsUpdateReq req = new HomeGroupSecurityidsUpdateReq();
                    if (action == "1")
                    {
                        req.securityid = this.SecurityID;
                        req.symbol = this.SymbolName;
                        req.action = action; //"1"; 添加
                        req.ordersn = CurCardItem.CustomSort.ToString();
                        req.groupid = CurCardItem.GroupId;
                        req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    }
                    else
                    {
                        req.groupid = CurCardItem.GroupId;
                        req.id = CurCardItem.ParentId;
                        req.action = action; // "3"; //删除;
                    }
                    
                    try
                    {
                        //PResult result2 = await HomePageDataHandler.AsyncUpdateSecurityids(req);
                        //if (result2 != null && result2.Code == "0000")
                        //{
                        //}
                        //else
                        //{
                        //    LogHandler.Debug(result2.Msg);
                        //}

                        //通知Broker系统
                        QuoteConfigHandler.Instance.Notify(action == "1" ? HomePageDataHandler.FavoriteTypeEnum.AddBond : HomePageDataHandler.FavoriteTypeEnum.DelBond, CurCardItem.GroupId, req.securityid, CurCardItem);

                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }));
            });
        }
        #endregion

        #endregion
    }
}
