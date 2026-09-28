using com.gateway.log;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.View.Trade;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using STC.Common.Model.Fix.Message;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.View.Component;
using SettleFilterView = BDB.Trade.Plugin.Component.SettleFilterView;
using STC.Common.ViewModel.Data;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class PriceViewModel : BaseViewModel
    {
        private object _lockObj = new object();

        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessMyOrdersThreadQueue;
        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        //存储最新的CT行情， key: secutirtyID; Value: MarketDataSnapshotFullRefresh
        private Dictionary<string, MarketDataSnapshotFullRefresh> _LastCTPriceDic = new Dictionary<string, MarketDataSnapshotFullRefresh>();

        private FrameworkElement _ownUI = null;
        //更新行情数据
        private Thread _RefreshDataThread = null;
        private string _rowFilterStr = string.Empty;
        private string _rowFilterSettleStr = string.Empty;
        private Dictionary<string, List<string>> _filterDic = new Dictionary<string, List<string>>();
        //key:SecurityID
        private ConcurrentDictionary<string, bool> _needUpdateDic = new ConcurrentDictionary<string, bool>();

        private CardItem _currentCardItem;

        public CardItem CurrentCardItem { get { return _currentCardItem; } set { _currentCardItem = value; } }

        private bool _isUpdateAll = true;
        private TabShowModel _tabShowModel = TabShowModel.Table;

        public TabShowModel GetShowModel()
        {
            return _tabShowModel;
        }

        public void SetTabShowModel(TabShowModel tabShowModel)
        {
            this._tabShowModel = tabShowModel;
        }

        #region 关联属性
        
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


        #region 角标显示

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

        private GridLength _OneColumnWidth = new GridLength(0, GridUnitType.Pixel);
        public GridLength OneColumnWidth { get { return _OneColumnWidth; } set { _OneColumnWidth = value; this.OnPropertyChanged(); } }


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


        private DataGridHeadersVisibility _headVisibility = DataGridHeadersVisibility.Column;
        public DataGridHeadersVisibility HeadVisibility
        {
            get { return _headVisibility; }
            set { _headVisibility = value; this.OnPropertyChanged(); }
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

        private string _lastExecDirection;
        public string LastExecDirection
        {
            get { return _lastExecDirection; }
            set { _lastExecDirection = value; this.OnPropertyChanged(); }
        }

        private Brush _lastExecBrush;
        public Brush LastExecBrush
        {
            get { return _lastExecBrush; }
            set { _lastExecBrush = value; this.OnPropertyChanged(); }
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

        #endregion



        #region 表格字段
        public const string KEY_FIELD = "Key";//Source + price
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string SETTLE_TYPE_FIELD = "SettleType";

        public const string BID_SETTLE_TYPE_FIELD = "BidSettleType";
        public const string OFR_SETTLE_TYPE_FIELD = "OfrSettleType";

        public const string SETTLE_FWD_TYPE_FIELD = "SettleFwdType";
        public const string SHOW_SETTLE_TYPE_FILED = "ShowSettleType";
        public const string BID_FIELD = "Bid";
        public const string BID_AMOUNT_FIELD = "BidAmount";
        public const string OFR_FIELD = "Ofr";
        public const string OFR_AMOUNT_FIELD = "OfrAmount";
        public const string SOURCE_FIELD = "Venues";//"Source";
        public const string UPDATE_TIME_FIELD = "UpdateTime";
        public const string DELETE_FLAG_FIELD = "DeleteFlag";

        public const string TRADE_PERMISSION_FLAG_FIELD = "TradePermissionFlag";

        public const string SHOW_SETTLE_COLUMN_FLAG_FILED = "ShowSettleColumnFlag";

        public const string BID_CORNER_MARKET_FIELD = "BidCornerMarket";
        public const string OFR_CORNER_MARKET_FIELD = "OfrCornerMarket";

        public const string SORT_FIELD = "BrokerSort";

        public const string BID_CORNER_MARKET_TEXT_FIELD = "BidCornerMarketText";
        public const string OFR_CORNER_MARKET_TEXT_FIELD = "OfrCornerMarketText";

        public const string BID_BARGAIN_FLAG_FIELD = "BidBargainFlag";
        public const string OFR_BARGAIN_FLAG_FIELD = "OfrBargainFlag";
        public const string ALL_IS_NULL_FLAG_FIELD = "AllIsNullFlag";//标识securityId为空时的显示标志



        private DataView _bidTableView;
        public DataView BidTableView
        {
            get
            {
                lock (_lockObj)
                {
                    if (_bidTableView == null)
                    {
                        _bidTableView = new DataTable().DefaultView;
                        _bidTableView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(BID_SETTLE_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_SETTLE_TYPE_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(SORT_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SHOW_SETTLE_TYPE_FILED, typeof(string));
                        _bidTableView.Table.Columns.Add(SETTLE_FWD_TYPE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_AMOUNT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(TRADE_PERMISSION_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(SHOW_SETTLE_COLUMN_FLAG_FILED, typeof(string));

                        _bidTableView.Table.Columns.Add(SOURCE_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(DELETE_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_CORNER_MARKET_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(BID_CORNER_MARKET_TEXT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(OFR_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_AMOUNT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(OFR_CORNER_MARKET_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_CORNER_MARKET_TEXT_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(BID_BARGAIN_FLAG_FIELD, typeof(string));
                        _bidTableView.Table.Columns.Add(OFR_BARGAIN_FLAG_FIELD, typeof(string));

                        _bidTableView.Table.Columns.Add(ALL_IS_NULL_FLAG_FIELD, typeof(string));
                        
                        _bidTableView.Sort = $"{SORT_FIELD} asc";
                        _bidTableView.Table.PrimaryKey = new DataColumn[] {
                            _bidTableView.Table.Columns[KEY_FIELD],
                        };
                        this.OnPropertyChanged();
                    }
                    return this._bidTableView;
                }
            }
        }



        #endregion

        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }

        private void OnAddClick()
        {
            BondGridVisibility = Visibility.Collapsed;
            SearchVisibility = Visibility.Visible;
            AddBondVisibility = Visibility.Collapsed;

            SearchEditUI searchEditUI = this._ownUI.FindName("seSearch") as SearchEditUI;
            searchEditUI.SetFocus();
        }

        ~PriceViewModel()
        {
            this.Dispose();
        }

        private void ClearLastExe()
        {
            LastExecPriceBP = LastExecSide = LastExecSource = LastExecDirection = string.Empty;
            LastExecPrice = string.Empty;//CommonConstants.NA;
            LastExecBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.HOLC.Gray");
            this.LastExecSideShow = string.Empty;
        }

        public void ClearParty(bool isFromBroker)
        {
            if (!isFromBroker)
            {
                //LastExecPriceBP = LastExecSide = LastExecSource = LastExecDirection = string.Empty;

                BidArrowVisibilly = OfrArrowVisibilly = Visibility.Collapsed;

                //this.LastExecSideShow = string.Empty;

                UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                UpSideBgBrush = (Brush)_ownUI.FindResource("NormalBrush");

                BestBid = BestOfr = BestBidAmount = BestOfrAmount = CommonConstants.NO_PRICE;
                //LastExecPrice = CommonConstants.NA;
                //LastExecBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.HOLC.Gray");
                BestPriceBP = CommonConstants.NO_UP_DOWN;
            }


            lock (this._lockObj)
            {
                if (_bidTableView != null)
                {
                    _bidTableView.Table.Rows.Clear();
                }
            }


        }

        public void Clear()
        {
            PriceHandler.Instance.UnregisterCTPrice(Security, Instance_OnMDPrice);

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                SettleFilterView settleFilterView = _ownUI.FindName("settleFilter") as SettleFilterView;
                settleFilterView.SetEllipseVisibility(Visibility.Collapsed);
            }));

            this.Security = this.SymbolName = this.RemainingTime = string.Empty;


            ClearParty(false);

            ClearLastExe();

            if (_LastCTPriceDic != null)
            {
                lock (_LastCTPriceDic)
                {
                    _LastCTPriceDic.Clear();
                }
            }

            if (_needUpdateDic != null)
            {
                _needUpdateDic.Clear();
            }

            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Clear();
            }

            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Clear();
            }
        }

        new public void Dispose()
        {
            Clear();

            if (_RefreshDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose RefreshDataThread:");
                _RefreshDataThread.Abort();
                _RefreshDataThread = null;
            }

            lock (this._lockObj)
            {
                if (BidTableView != null)
                {
                    BidTableView.Table.Clear();
                    BidTableView.Dispose();
                    _bidTableView = null;
                }
            }

            lock (_LastCTPriceDic)
            {
                if (_LastCTPriceDic != null)
                {
                    _LastCTPriceDic.Clear();
                    //_LastCTPriceDic = null;
                }
            }
            


            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Clear();
                _ProcessMyOrdersThreadQueue.Dispose();
                _ProcessMyOrdersThreadQueue = null;
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Clear();
                _ProcessCtPriceThreadQueue.Dispose();
                _ProcessCtPriceThreadQueue = null;
            }
            
            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Clear();
                _ProcessLastExecutionsThreadQueue.Dispose();
                _ProcessLastExecutionsThreadQueue = null;
            }

            if (_tabShowModel == TabShowModel.Table)
                BondChangeHandler.Instance.OnSelectBondChange -= Instance_OnSelectBondChange;
            else
                BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnSelectSettleChange;

            BondChangeHandler.Instance.OnOneSettleChange -= Instance_OnOneSettleChange;

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrders -= Instance_OnOrder;

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
            BondChangeHandler.Instance.OnSelectBondChange -= Instance_OnSelectBondChange;
            base.Dispose();
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("PriceViewModel Clear, security:{0}", this.Security));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        lock (this._lockObj)
                        {
                            ClearLastExe();
                            ClearParty(false);
                            SettleFilterView settleFilterView = _ownUI.FindName("settleFilter") as SettleFilterView;
                            settleFilterView.SetEllipseVisibility(Visibility.Collapsed);

                            List<string> filterVenues = _filterDic.ContainsKey(CommonConstants.BROKER) ? _filterDic[CommonConstants.BROKER] : null;

                            if (filterVenues == null || filterVenues.Count == 0 || filterVenues.Count == PermisionHandler.Instance.GetBrokerCount())
                            {
                                PriceCalHelper.Instance.InitTableDataByPermission(BidTableView, filterVenues);

                            }
                            else
                            {
                                PriceCalHelper.Instance.InitTableDataByBroker(BidTableView, filterVenues[0]);
                            }

                            if (string.IsNullOrEmpty(this.Security))
                            {
                                foreach (DataRow dr in BidTableView.Table.Rows)
                                {
                                    dr[ALL_IS_NULL_FLAG_FIELD] = "1";
                                }
                            }

                            LogHelper.Info(string.Format("PriceViewModel Clear end, security:{0}, model:{1}", this.Security, this._tabShowModel.ToString()));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(string.Format("PriceViewModel Clear end, security:{0}, model:{1}, exception:{2}", this.Security, this._tabShowModel.ToString(), ex.Message));
                    }
                }));
            }
        }


        public void Init(FrameworkElement ownUI, CardItem cardItem)
        {
            _currentCardItem = cardItem;

            BaseHandler.Instance.Init(Instance_OnClear);

            _ownUI = ownUI;
            LastExecBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.HOLC.Gray");// ("TextBox.Static.Foreground");

            UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
            UpSideBgBrush = (Brush)_ownUI.FindResource("NormalBrush");
            
            BindingOperations.EnableCollectionSynchronization(this.BidTableView, _lockObj);

            if (_tabShowModel == TabShowModel.Table)
                BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;
            else
                BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnSelectSettleChange;

            BondChangeHandler.Instance.OnOneSettleChange += Instance_OnOneSettleChange;

            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrders += Instance_OnOrder;

            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateMyOrdersAsync);
            }

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }

            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }

            PriceCalHelper.Instance.InitTableDataByPermission(BidTableView, null);

            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
            //订阅price
            //PriceHandler.Instance.RegisterH0Price(obj.securityid, Instance_OnMDPrice);
            if (!string.IsNullOrEmpty(this.Security))
            {
                PriceHandler.Instance.RegisterCTPrice(this.Security, Instance_OnMDPrice);

                UpdateSecurityInfo(Security);

                BondGridVisibility = Visibility.Visible;
                AddBondVisibility = Visibility.Collapsed;
                SearchVisibility = Visibility.Collapsed;

                foreach (DataRow dr in BidTableView.Table.Rows)
                {
                    dr[ALL_IS_NULL_FLAG_FIELD] = string.Empty;
                }

                OfrBrush = _ownUI.FindResource("ColorBrush.Blue_firm") as Brush;
                BidBrush = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                AmountBrush = _ownUI.FindResource("TextBox.Static.Foreground") as Brush;

                HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;
            }
            else
            {
                BondGridVisibility = Visibility.Collapsed;

                if (_tabShowModel == TabShowModel.Card)
                    AddBondVisibility = Visibility.Visible;
                else
                    AddBondVisibility = Visibility.Collapsed;

                SearchVisibility = Visibility.Collapsed;

                AmountBrush = BidBrush = OfrBrush = _ownUI.FindResource("Separator.ColorBrush") as Brush;

                foreach (DataRow dr in BidTableView.Table.Rows)
                {
                    dr[ALL_IS_NULL_FLAG_FIELD] = "1";
                }

                HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;

            }

        }


        public event Action<string> OnAddBondEnd;

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum typeEnum, string groupID, string securityID, string bondId)
        {
            if (typeEnum == FavoriteTypeEnum.AddBondFromCard)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (_currentCardItem != null && !string.IsNullOrEmpty(_currentCardItem.GroupId) && string.IsNullOrEmpty(this.Security) && _currentCardItem.GroupId == groupID)
                    {
                        _currentCardItem.Id = bondId;
                        _currentCardItem.Security = securityID;

                        _currentCardItem.HomeGroupSecurityIDsInfo = HomePageDataHandler.Instance.GetHomeGroupSecurityIDsInfo(groupID, bondId);

                        AddBondVisibility = Visibility.Collapsed ;
                        BondGridVisibility = Visibility.Visible;
                        SearchVisibility = Visibility.Collapsed;

                        Instance_OnSelectBondChange(BondUpdateType.ChangeFocusRow, groupID, bondId, securityID, CommonConstants.ALL_PARTY, true);

                        HomePageDataHandler.Instance.OnFavoritesChanged -= Instance_OnFavoritesChanged;

                        OnAddBondEnd?.Invoke(securityID);
                    }
                }));
            }
        }

        private void Instance_OnOneSettleChange(OperationType operationType, string groupId, string id, List<string> settle)
        {
            if (operationType == OperationType.CardToTable || operationType == OperationType.TableToCard)
            {
                if  (_currentCardItem != null && !string.IsNullOrEmpty(_currentCardItem.GroupId) && _currentCardItem.GroupId == groupId && !string.IsNullOrEmpty(_currentCardItem.Id) && _currentCardItem.Id == id
                    && settle != null && settle.Count > 0)
                {
                    if (_currentFilterDic != null)
                    {
                        if (_currentFilterDic.ContainsKey(CommonConstants.SETTLE))
                        {
                            bool isContain = ConditionHelper.IsContainSettle(settle[0], _currentFilterDic[CommonConstants.SETTLE]);
                            if (isContain)
                                return;

                            _currentFilterDic.Remove(CommonConstants.SETTLE);
                        }
                    }
                    else
                    {
                        _currentFilterDic = new Dictionary<string, IList>();
                    }
                    _currentFilterDic.Add(CommonConstants.SETTLE, ConditionHelper.GetMainSettle(settle));
                    this.Instance_OnSelectSettleChange(operationType, groupId, _currentFilterDic);
                }
            }
        }

        #region 处理CT报价更新

        private void UpdateMDData(MarketDataSnapshotFullRefresh data)
        {
            lock (this._lockObj)
            {
                try
                {
                    string securityId = data.SecurityID;
                    CalBestPrice(securityId);
                    List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(string.Empty, securityId);
                    if (marketDataList.Count > 0)
                    {
                        UpdatePrice2(marketDataList);

                        UpdateFwdFlag(marketDataList);

                        UpdateMyOrdersAsync(securityId);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }
        

        private void UpdateFwdFlag(List<MarketDataSnapshotFullRefresh> marketDataList)
        {
            bool showFwd = PriceCalHelper.Instance.CheckFwd(marketDataList);

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                SettleFilterView settleFilterView = _ownUI.FindName("settleFilter") as SettleFilterView;
                settleFilterView.SetEllipseVisibility(showFwd ? Visibility.Visible : Visibility.Collapsed);
            }));
        }

        private bool FilterData(GNoMDEntries mdEntrys, string broker)
        {
            bool result = true;

            if (_filterDic != null && _filterDic.Keys.Count > 0)
            {
                if (_filterDic.ContainsKey(CommonConstants.BROKER))
                {
                    //过滤Broker
                    result = _filterDic[CommonConstants.BROKER].Contains(broker);
                }

                if (result)
                {
                    if (_filterDic.ContainsKey(CommonConstants.SETTLE))
                    {
                        //过滤settle
                        result = _filterDic[CommonConstants.SETTLE].Contains(mdEntrys.SettlType);

                        //处理远期
                        if (!result && _filterDic[CommonConstants.SETTLE].Contains(CommonConstants.FWD) && !string.IsNullOrEmpty(mdEntrys.SettlType) && mdEntrys.SettlType.Contains("+"))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private void UpdatePrice2(List<MarketDataSnapshotFullRefresh> marketDataList)
        {
            List<GNoMDEntries> bidNoMDEntryList = new List<GNoMDEntries>();
            List<GNoMDEntries> ofrNoMDEntryList = new List<GNoMDEntries>();
            List<string> bidVenusList = new List<string>();
            List<string> ofrVenusList = new List<string>();
            foreach (MarketDataSnapshotFullRefresh mds in marketDataList)
            {
                List<GNoMDEntries> temList = mds.NoMDEntries.Where(delegate (GNoMDEntries noMDEntry)
                {
                    if (noMDEntry.MDEntryType == CommonConstants.SIDE_BID && FilterData(noMDEntry, noMDEntry.Venues))
                    {
                        //if (!string.IsNullOrEmpty(mds.Venues))
                        //    noMDEntry.Venues = mds.Venues;

                        bidVenusList.Add(noMDEntry.Venues);

                        return true;
                    }
                    else
                        return false;
                }).ToList();
                
                bidNoMDEntryList.AddRange(temList);

                //if (temList.Count > 0 && !string.IsNullOrEmpty(mds.Venues) && NeedFilterVenus(mds.Venues))
                //    bidVenusList.Add(mds.Venues);


                temList.Clear();

                //temList = mds.NoMDEntries.Where((o) => { return o.MDEntryType == CommonConstants.SIDE_BID; }).ToList();

                //if (temList == null || temList.Count == 0)
                //    bidVenusList.Add(mds.Venues);

                //temList.Clear();






                temList = mds.NoMDEntries.Where(delegate (GNoMDEntries noMDEntry)
                {
                    if (noMDEntry.MDEntryType == CommonConstants.SIDE_OFR && FilterData(noMDEntry, noMDEntry.Venues))
                    {
                        //if (!string.IsNullOrEmpty(mds.Venues))
                        //    noMDEntry.Venues = mds.Venues;

                        ofrVenusList.Add(noMDEntry.Venues);

                        return true;
                    }
                    else
                        return false;
                }).ToList();

                ofrNoMDEntryList.AddRange(temList);
                //if (temList.Count > 0 && !string.IsNullOrEmpty(mds.Venues) && NeedFilterVenus(mds.Venues))
                //    ofrVenusList.Add(mds.Venues);

                //temList.Clear();
                //temList = mds.NoMDEntries.Where((o) => { return o.MDEntryType == CommonConstants.SIDE_OFR; }).ToList();

                //if (temList == null || temList.Count == 0)
                //    ofrVenusList.Add(mds.Venues);

                temList.Clear();

            }

            string securityID = marketDataList[0].SecurityID;
            string venues = marketDataList[0].Venues;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this._lockObj)
                {
                    if (_isUpdateAll)
                    {
                        UpdatePriceTable2(CommonConstants.SIDE_BID, bidVenusList, bidNoMDEntryList);
                        UpdatePriceTable2(CommonConstants.SIDE_OFR, ofrVenusList, ofrNoMDEntryList);

                    }
                    else
                    {
                        string broker = string.Empty;
                        if (_filterDic != null && _filterDic.Keys.Count > 0)
                        {
                            if (_filterDic.ContainsKey(CommonConstants.BROKER))
                            {
                                //过滤Broker
                                broker = _filterDic[CommonConstants.BROKER].Count > 0 ? _filterDic[CommonConstants.BROKER][0] : string.Empty;
                            }
                        }

                        RefreshData(broker, bidNoMDEntryList, ofrNoMDEntryList);
                    }
                }
            }));
        }


        #region 更新指定broker

        public void RefreshData(string broker, List<GNoMDEntries> bidMDEntryList, List<GNoMDEntries> ofrMDEntryList)
        {
            lock (this._lockObj)
            {
                foreach (DataRow dr in BidTableView.Table.Rows)
                {
                    dr[BID_CORNER_MARKET_TEXT_FIELD] = dr[BID_CORNER_MARKET_FIELD] = string.Empty;
                    dr[OFR_CORNER_MARKET_TEXT_FIELD] = dr[OFR_CORNER_MARKET_FIELD] = string.Empty;

                    dr[BID_FIELD] = dr[BID_AMOUNT_FIELD] = CommonConstants.NO_PRICE;
                    dr[OFR_FIELD] = dr[OFR_AMOUNT_FIELD] = CommonConstants.NO_PRICE;

                    dr[BID_BARGAIN_FLAG_FIELD] = string.Empty;
                    dr[OFR_BARGAIN_FLAG_FIELD] = string.Empty;
                }

                RefreshDataBySide(CommonConstants.SIDE_BID_ONE, broker, bidMDEntryList);
                RefreshDataBySide(CommonConstants.SIDE_OFR_TWO, broker, ofrMDEntryList);
            }
        }

        private void RefreshDataBySide(string side, string broker, List<GNoMDEntries> noMDEntryList)
        {
            StringBuilder sb = new StringBuilder();

            decimal fwdQty = decimal.Zero;
            decimal spotQty = decimal.Zero;
            string spotKey = string.Empty;
            string fwdKey = string.Empty;
            string price = string.Empty;
            StringBuilder sbFwd = new StringBuilder();

            string barOne = string.Empty;
            string barFwd = string.Empty;

            string allBar = string.Empty;
            
            foreach (GNoMDEntries mdEntry in noMDEntryList)
            {
                if (sb.Length > 0)
                    sb.Append("+");

                if (!string.IsNullOrEmpty(mdEntry.Text))
                    sb.Append(string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                else
                    sb.Append(mdEntry.MDEntrySize);

                price = mdEntry.MDEntryPx;

                decimal d = 0;
                string key = string.Empty;

                DataRow[] drs = null;

                allBar = CalBar(allBar, mdEntry);

                if (mdEntry.SettlType == CommonConstants.T1_ONE)
                {
                    key = string.Format("{0}{1}", broker, CommonConstants.T1_EN);
                    drs = BidTableView.Table.Select($"{KEY_FIELD} = '{key}'");

                    if (drs.Length > 0)
                    {
                        drs[0][SETTLE_TYPE_FIELD] = CommonConstants.T1_ONE;
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                            drs[0][BID_AMOUNT_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntrySize) ? CommonConstants.NO_PRICE : mdEntry.MDEntrySize;
                            drs[0][BID_BARGAIN_FLAG_FIELD] = mdEntry.BargainFlag;
                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            drs[0][OFR_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                            drs[0][OFR_AMOUNT_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntrySize) ? CommonConstants.NO_PRICE : mdEntry.MDEntrySize;
                            drs[0][OFR_BARGAIN_FLAG_FIELD] = mdEntry.BargainFlag;
                        }
                    }
                }
                else if (CommonConstants.T2_T3.Contains(mdEntry.SettlType))
                {
                    spotKey = key = string.Format("{0}{1}", broker, CommonConstants.T2_EN);

                    if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                        spotQty += d;
                    drs = BidTableView.Table.Select($"{KEY_FIELD} = '{key}'");

                    if (drs.Length > 0)
                    {
                        drs[0][SETTLE_TYPE_FIELD] = CommonConstants.T2_T3;
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            drs[0][OFR_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                        }

                        barOne = CalBar(barOne, mdEntry);
                    }
                }
                else if (PriceCalHelper.Instance.IsFwd(mdEntry.SettlType))
                {
                    fwdKey = key = string.Format("{0}{1}", broker, CommonConstants.FWD);

                    if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                        fwdQty += d;

                    drs = BidTableView.Table.Select($"{KEY_FIELD} = '{key}'");


                    if (drs != null && drs.Length > 0)
                    {
                        sbFwd.Append(string.Format("{0}:", mdEntry.SettlType));
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            drs[0][OFR_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;


                        }

                        barFwd = CalBar(barFwd, mdEntry);
                    }
                }

                if (drs != null && drs.Length > 0)
                {
                    long amount = 0;
                    long.TryParse(mdEntry.MDEntrySize, out amount);
                    //更新角标
                    if (side == CommonConstants.SIDE_BID_ONE)
                    {

                        string result = CalCornerMarket(drs[0], BID_CORNER_MARKET_TEXT_FIELD, amount, mdEntry.Text);

                        drs[0][BID_CORNER_MARKET_FIELD] = !string.IsNullOrEmpty(result) ? "1" : string.Empty;
                    }
                    else if (side == CommonConstants.SIDE_OFR_TWO)
                    {
                        string result = CalCornerMarket(drs[0], OFR_CORNER_MARKET_TEXT_FIELD, amount, mdEntry.Text);

                        drs[0][OFR_CORNER_MARKET_FIELD] = !string.IsNullOrEmpty(result) ? "1" : string.Empty;
                    }
                }

                //if (drs != null && drs.Length > 0)
                //{
                //    drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                //}
            }

            //更新+1
            if (spotQty > 0 && !string.IsNullOrEmpty(spotKey))
            {
                UpdateAmountAndStar(spotKey, side, spotQty.ToString(), barOne);
            }
            else
            {
                spotKey = string.Format("{0}{1}", broker, CommonConstants.T2_EN);
                UpdateAmountAndStar(spotKey, side, CommonConstants.NO_PRICE, barOne);
            }

            //更新Bid Ofr合计的bargain
            if (side == CommonConstants.SIDE_BID_ONE)
            {
                BidBrokerBargainFlag = allBar;
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                OfrBrokerBargainFlag = allBar;
            }

            //更新远期
            if (fwdQty > 0 && !string.IsNullOrEmpty(fwdKey))
            {
                UpdateAmountAndStar(fwdKey, side, fwdQty.ToString(), barFwd);

                DataRow[] drs = this.BidTableView.Table.Select($"{KEY_FIELD} = '{fwdKey}'");

                if (drs != null && drs.Length > 0)
                {
                    string fwdSettle = sbFwd.ToString();
                    drs[0][SETTLE_TYPE_FIELD] = fwdSettle.Length > 0 ? fwdSettle.Substring(0, fwdSettle.Length - 1) : string.Empty;
                }
            }
            else
            {
                fwdKey = string.Format("{0}{1}", broker, CommonConstants.FWD);
                UpdateAmountAndStar(fwdKey, side, CommonConstants.NO_PRICE, barOne);

                DataRow[] drs = this.BidTableView.Table.Select($"{KEY_FIELD} = '{fwdKey}'");

                if (drs != null && drs.Length > 0)
                {
                    drs[0][SETTLE_TYPE_FIELD] = string.Empty;
                }
            }

            if (side == CommonConstants.SIDE_BID_ONE)
            {
                this.BidTxt = sb.ToString();
                this.BidPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                this.OfrTxt = sb.ToString();
                this.OfrPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
        }

        private string CalBar(string bargainFlag, GNoMDEntries mdEntry)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(bargainFlag))
                result = mdEntry.BargainFlag;
            else if (bargainFlag != mdEntry.BargainFlag && mdEntry.BargainFlag == "2")
                result = "2";


            return result;
        }

        private void UpdateAmountAndStar(string key, string side, string qty, string bargain)
        {
            DataRow[] drs = this.BidTableView.Table.Select($"{KEY_FIELD} = '{key}'");

            if (drs != null && drs.Length > 0)
            {
                if (side == CommonConstants.SIDE_BID_ONE)
                {
                    drs[0][BID_AMOUNT_FIELD] = qty;
                    drs[0][BID_BARGAIN_FLAG_FIELD] = bargain;
                }
                else if (side == CommonConstants.SIDE_OFR_TWO)
                {
                    drs[0][OFR_AMOUNT_FIELD] = qty;
                    drs[0][OFR_BARGAIN_FLAG_FIELD] = bargain;
                }
            }
        }

        #endregion

        public bool NeedFilterVenus(string venus)
        {
            if (_filterDic != null && _filterDic.Keys.Count > 0 && _filterDic.ContainsKey(CommonConstants.BROKER))
            {
                List<string> filterBroker = _filterDic[CommonConstants.BROKER];
                if (filterBroker.Contains(venus))
                    return true;
                else
                    return false;
            }

            return true;
        }

        private void UpdatePriceTable2(string side, List<string> updateVenues, List<GNoMDEntries> noMDEntryList)
        {
            if (_bidTableView == null)
                return;

            //重置总量、星标、报价
            foreach (string str in updateVenues)
            {
                DataRow temDr = _bidTableView.Table.Rows.Find(str);
                if (temDr != null)
                {
                    if (side == CommonConstants.SIDE_BID)
                    {
                        temDr[BID_FIELD] = CommonConstants.NO_PRICE;
                        temDr[BID_AMOUNT_FIELD] = CommonConstants.NO_PRICE;
                        temDr[BID_CORNER_MARKET_TEXT_FIELD] = string.Empty;
                        temDr[BID_CORNER_MARKET_FIELD] = string.Empty;
                        temDr[BID_SETTLE_TYPE_FIELD] = string.Empty;
                        temDr[BID_BARGAIN_FLAG_FIELD] = string.Empty;
                    }
                    else if (side == CommonConstants.SIDE_OFR)
                    {
                        temDr[OFR_FIELD] = CommonConstants.NO_PRICE;
                        temDr[OFR_AMOUNT_FIELD] = CommonConstants.NO_PRICE;
                        temDr[OFR_CORNER_MARKET_TEXT_FIELD] = string.Empty;
                        temDr[OFR_CORNER_MARKET_FIELD] = string.Empty;
                        temDr[OFR_SETTLE_TYPE_FIELD] = string.Empty;
                        temDr[OFR_BARGAIN_FLAG_FIELD] = string.Empty;
                    }
                }
            }

            Dictionary<string, string> allBrokerBarDic = new Dictionary<string, string>();

            Dictionary<string, StringBuilder> allSettleDic = new Dictionary<string, StringBuilder>();
            

            // 添加和更新数据(普通)
            foreach (GNoMDEntries entry in noMDEntryList)
            {
                string key = entry.Venues;//PriceCalHelper.Instance.GetKey(entry.Venues, entry.MDEntryPx);//entry.MDEntryID;

                string venueTypeGW = entry.VenueTypeGW;
                string time = entry.MDEntryTime;
                string firmFlag = entry.FirmFlag;
                string text = entry.Text;
                string price = CommonUtils.ConvertToString(entry.MDEntryPx);
                string netPrice = CommonUtils.ConvertToString(entry.NetPrice);
                long qty = CommonUtils.ConvertToLong(entry.MDEntrySize);
                //string source = GenerateSourceByVenues(venues, entry.PartySubName);
                bool isMe = LocalEnv.UserInfo.UserId == entry.PartySubID;
                //string partySubName = CTPriceControler.GenerateSourceByVenues(venues, entry.PartySubName, entry.RoleName);


                string settlType = string.Empty;
                string settleType_yuan = string.Empty;

                settlType = entry.SettlType;

                string dicKey = string.Format("{0}-{1}", key, side);
                if (!allBrokerBarDic.ContainsKey(dicKey))
                    allBrokerBarDic[dicKey] = entry.BargainFlag;
                else
                {
                    allBrokerBarDic[dicKey] = CalBar(allBrokerBarDic[dicKey], entry);
                }

                int mdPriceLevel = CommonUtils.ConvertToInt(entry.MDPriceLevel);
                string bargainFlag = entry.BargainFlag;

                string wholeFlag = entry.WholeFlag;
                string institutionName = entry.InstitutionName;
                string showTrader = entry.ShowTrader;
                string traderName = entry.TraderName;


                DataRow findDr = _bidTableView.Table.Rows.Find(key);
                

                if (findDr != null)
                {
                    // 清算速度
                    findDr[SETTLE_TYPE_FIELD] = settlType;

                    string temSettl = string.Format("{0}:", settlType);
                    if (!allSettleDic.ContainsKey(key))
                    {
                        StringBuilder sbSettle = new StringBuilder();
                        sbSettle.Append(temSettl);

                        allSettleDic.Add(key, sbSettle);
                    }
                    else
                    {
                        allSettleDic[key].Append(temSettl);
                    }

                    if (!string.IsNullOrEmpty(settlType) && settlType != "1" && settlType != "2" && settlType != "3")
                        findDr[SETTLE_FWD_TYPE_FIELD] = settlType;


                    if (side == CommonConstants.SIDE_BID)
                    {
                        //findDr[BID_SETTLE_TYPE_FIELD] = settlType;
                        string result = CalCornerMarket(findDr, BID_CORNER_MARKET_TEXT_FIELD, qty, text);

                        findDr[BID_CORNER_MARKET_FIELD] = !string.IsNullOrEmpty(result) ? "1" : string.Empty;
                        long localQty = CommonUtils.ConvertToLong(findDr[BID_AMOUNT_FIELD].ToString());
                        qty = localQty + qty;
                        findDr[BID_FIELD] = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : CommonUtils.formatString(price, 4);
                        findDr[BID_AMOUNT_FIELD] = qty == 0 ? CommonConstants.NO_PRICE : qty.ToString();
                    }
                    else if (side == CommonConstants.SIDE_OFR)
                    {
                        string result = CalCornerMarket(findDr, OFR_CORNER_MARKET_TEXT_FIELD, qty, text);

                        //findDr[OFR_SETTLE_TYPE_FIELD] = settlType;
                        findDr[OFR_CORNER_MARKET_FIELD] = !string.IsNullOrEmpty(result) ? "1" : string.Empty;
                        long localQty = CommonUtils.ConvertToLong(findDr[OFR_AMOUNT_FIELD].ToString());
                        qty = localQty + qty;
                        findDr[OFR_FIELD] = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : CommonUtils.formatString(price, 4);
                        findDr[OFR_AMOUNT_FIELD] = qty == 0 ? CommonConstants.NO_PRICE : qty.ToString();
                    }

                    findDr[UPDATE_TIME_FIELD] = time;
                }

            }

            foreach (KeyValuePair<string, StringBuilder> kv in allSettleDic)
            {
                DataRow findDr = _bidTableView.Table.Rows.Find(kv.Key);
                string allSettle = kv.Value.ToString();
                allSettle = allSettle.Length > 0 ? allSettle.Substring(0, allSettle.Length - 1) : allSettle;
                
                if (side == CommonConstants.SIDE_BID)
                {
                    findDr[BID_SETTLE_TYPE_FIELD] = allSettle;
                }
                else if (side == CommonConstants.SIDE_OFR)
                {
                    findDr[OFR_SETTLE_TYPE_FIELD] = allSettle;
                }
            }



            //刷新星标
            foreach (KeyValuePair<string, string> kv in allBrokerBarDic)
            {
                string key = kv.Key.Substring(0, kv.Key.IndexOf("-"));
                DataRow findDr = _bidTableView.Table.Rows.Find(key);

                if (findDr != null)
                {
                    if (side == CommonConstants.SIDE_BID)
                    {
                        findDr[BID_BARGAIN_FLAG_FIELD] = kv.Value;
                    }
                    else if (side == CommonConstants.SIDE_OFR)
                    {
                        findDr[OFR_BARGAIN_FLAG_FIELD] = kv.Value;
                    }
                }
            }

            List<DataRow> removeRow = new List<DataRow>();
            foreach (DataRow dr in _bidTableView.Table.Rows)
            {
                string key = dr[KEY_FIELD].ToString();
                if (!updateVenues.Contains(key))
                {
                    DataRow findDr = _bidTableView.Table.Rows.Find(key);

                    removeRow.Add(findDr);
                }
            }

            if (removeRow.Count > 0)
            {
                foreach (DataRow temDr in removeRow)
                {
                    if (temDr != null)
                    {
                        if (side == CommonConstants.SIDE_BID)
                        {
                            temDr[BID_FIELD] = CommonConstants.NO_PRICE;
                            temDr[BID_AMOUNT_FIELD] = CommonConstants.NO_PRICE;
                            temDr[BID_CORNER_MARKET_TEXT_FIELD] = string.Empty;
                            temDr[BID_CORNER_MARKET_FIELD] = string.Empty;
                            temDr[BID_SETTLE_TYPE_FIELD] = string.Empty;
                            temDr[BID_BARGAIN_FLAG_FIELD] = string.Empty;
                        }
                        else if (side == CommonConstants.SIDE_OFR)
                        {
                            temDr[OFR_FIELD] = CommonConstants.NO_PRICE;
                            temDr[OFR_AMOUNT_FIELD] = CommonConstants.NO_PRICE;
                            temDr[OFR_CORNER_MARKET_TEXT_FIELD] = string.Empty;
                            temDr[OFR_CORNER_MARKET_FIELD] = string.Empty;
                            temDr[OFR_SETTLE_TYPE_FIELD] = string.Empty;
                            temDr[OFR_BARGAIN_FLAG_FIELD] = string.Empty;
                        }
                    }
                }
            }

            _bidTableView.Table.AcceptChanges();

        }



        private string CalCornerMarket(DataRow findDr, string sideField, long qty, string text)
        {
            string newText = qty.ToString();
            if (!string.IsNullOrEmpty(text))
                newText = string.Format("{0}({1})", newText, text);

            string oldText = CommonUtils.ConvertToString(findDr[sideField]);

            if (!string.IsNullOrEmpty(oldText))
            {
                if (!string.IsNullOrEmpty(newText))
                    newText = string.Format("{0}+{1}", oldText, newText);
                else
                    newText = oldText;
            }

            findDr[sideField] = newText;

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(oldText))
                return string.Empty;

            return newText;

        }

        private void CalBestPrice(string securityID)
        {
            string filterStr = this._rowFilterSettleStr;
            
            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityID, filterStr, string.Empty);//PriceCalHelper.Instance.CalBestPrice(bilateralBestList);

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this._lockObj)
                {
                    if (bestPrice.BidBest != null)
                    {
                        this.BestBid = CommonUtils.formatString(bestPrice.BidBest.bestPx, 4);
                        this.BestBidAmount = bestPrice.BidBest.bestQty == 0 ? string.Empty : bestPrice.BidBest.bestQty.ToString();
                    }
                    else
                    {
                        BestBid = BestBidAmount = CommonConstants.NO_PRICE;
                    }

                    if (bestPrice.OfrBest != null)
                    {
                        this.BestOfr = CommonUtils.formatString(bestPrice.OfrBest.bestPx, 4);
                        this.BestOfrAmount = bestPrice.OfrBest.bestQty == 0 ? string.Empty : bestPrice.OfrBest.bestQty.ToString();
                    }
                    else
                    {
                        BestOfr = BestOfrAmount = CommonConstants.NO_PRICE;
                    }

                    decimal bidD = 0;
                    decimal ofrD = 0;


                    if (bestPrice.BidBest != null && bestPrice.OfrBest != null && decimal.TryParse(bestPrice.BidBest.bestPx, out bidD) && decimal.TryParse(bestPrice.OfrBest.bestPx, out ofrD)
                        && bidD > 0 && ofrD > 0)
                    {
                        //this.BestPriceBP = string.Format("{0}bp", CommonUtils.formatString((00).ToString(), 0));

                        decimal bpd = (bidD - ofrD) * 100;
                        //bpd = Math.Abs(bpd);
                        this.BestPriceBP = PriceCalHelper.Instance.FormatBestBP(bpd);//string.Format("{0}bp", CommonUtils.formatString(bpd.ToString(), 2));

                        if (PriceCalHelper.Instance.IsUpSide(bestPrice))
                        {
                            UpSideBdBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Red-60%");
                            UpSideBgBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Red-16%");
                        }
                        else
                        {
                            UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                            UpSideBgBrush = (Brush)_ownUI.FindResource("NormalBrush");
                        }
                    }
                    else
                    {
                        this.BestPriceBP = CommonConstants.NO_UP_DOWN;
                        UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                        UpSideBgBrush = (Brush)_ownUI.FindResource("NormalBrush");
                    }
                }
            }));

        }

        private string GetKey(string security, string venues)
        {
            return string.Format("{0}{1}", security, venues);
        }

        /// <summary>
        /// 做市报价（SOR）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);
            
        }


        private DataView CreateBaseDataView()
        {
            DataView baseDV = new DataTable().DefaultView;
            baseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
            baseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(DateTime));
            baseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
            baseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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
            baseDV.Table.Columns.Add(CTPriceControler.DIVIDED_FIELD, typeof(string));

            baseDV.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));

            //_BaseDV.Sort = $"{VENUE_TYPE_GW_FIELD},{SOURCE_FIELD},{ENTRY_PX_FIELD},{MD_PRICE_LEVEL_FIELD}";
            baseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
            baseDV.Table.PrimaryKey = new DataColumn[] { baseDV.Table.Columns[CTPriceControler.KEY_FIELD] };

            return baseDV;
        }

        #endregion

        #region 最新成交

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            if (obj == null)
            {
                return;
            }

            if (_ProcessLastExecutionsThreadQueue != null && !string.IsNullOrEmpty(obj.SecurityID) && obj.SecurityID == this.Security)
            {
                _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            }
        }


        private void UpdateLastExecutionsAsync(ExecutionReport obj)
        {
            // 市场最新成交
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateLastExecutions(obj);
            }));
        }


        private void UpdateLastExecutions(ExecutionReport obj)
        {
            lock (_lockObj)
            {
                string side = obj.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : obj.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : obj.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_TRD_THREE : obj.Side;
                string security = obj.SecurityID;

                //更新最新成交
                LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(obj); //LastExecutionProcesser.Processer.CalLastExe(security, side, _settleList);

                LastExecDirection = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? CommonConstants.UP : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;
                decimal bpd = 0;
                if (decimal.TryParse(lastPriceParam.Bp, out bpd))
                    LastExecPriceBP = PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString()); //CommonUtils.formatString(Math.Abs(bpd).ToString(), 2);
                else
                    LastExecPriceBP = string.Empty;
                //LastExecPriceBP = lastPriceParam.Bp;
                LastExecPrice = CommonUtils.formatString(obj.LastPx, 4);

                Brush showlastExecPriceSideBrush = null;
                if (obj.Side == CommonConstants.SIDE_BID_ONE || obj.Side == CommonConstants.SIDE_Y)
                {
                    LastExecSideShow = CommonConstants.TKN;
                    showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("ColorBrush.Blue");

                }
                else if (obj.Side == CommonConstants.SIDE_OFR_TWO || obj.Side == CommonConstants.SIDE_X)
                {
                    showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("Bid.textblock.foreground");

                    LastExecSideShow = CommonConstants.GVN;
                }
                else if (obj.Side == CommonConstants.SIDE_TRD_THREE || obj.Side == CommonConstants.SIDE_Z)
                {
                    showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("RFQ.ColorBrush");

                    LastExecSideShow = CommonConstants.TRD;
                }

                LastExecSource = ComboSourceUtil.Instance.FindEnum(lastPriceParam.Source, "venues");

                decimal bp = 0;


                Brush showlastExecPriceBrush = null;

                if (lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO)
                {
                    showlastExecPriceBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Red");
                }
                else if (lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE)
                {
                    showlastExecPriceBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Green");
                }
                else if (bp == 0)
                {
                    showlastExecPriceBrush = (Brush)_ownUI.FindResource("TextBox.Static.Foreground");
                }


                LastExecBrush = showlastExecPriceBrush;
                LastExecSideBrush = showlastExecPriceSideBrush;

            }
        }

        #endregion

        public void ChangeOweSettle(List<string> settleList)
        {
            List<ListBoxItem> itemList = new List<ListBoxItem>();

            if (settleList != null)
            {
                foreach (string settle in settleList)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Content = settle;
                    itemList.Add(listBoxItem);
                }
            }
            else
            {
                ListBoxItem allItem = new ListBoxItem();
                allItem.Content = CommonConstants.ALL_PARTY;

                itemList.Add(allItem);
            }

            Dictionary<string, IList> filterDic = new Dictionary<string, IList>();
            filterDic[CommonConstants.SETTLE] = itemList;
            
            SettleFilterView settleFilter = _ownUI.FindName("settleFilter") as SettleFilterView;
            settleFilter.SetItemSelectedStatus(filterDic);
        }

        private OperationType _currentOperationType = OperationType.None;
        public OperationType CurrentOperationType { get { return _currentOperationType; } set { _currentOperationType = value; } }

        //当前全局过滤条件
        private Dictionary<string, IList> _currentFilterDic = null;


        public void Instance_OnSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if (operationType == OperationType.Card || operationType == OperationType.Table || operationType == OperationType.CardToTable || operationType == OperationType.TableToCard)
            {
                _currentOperationType = operationType;

                _currentFilterDic = PriceCalHelper.Instance.CopyDictionary(obj as Dictionary<string, IList>);

                lock (this._lockObj)
                {
                    List<string> settleList = ConditionHelper.GetMainSettle(_currentFilterDic);
                    if (settleList != null && settleList.Count > 0)
                        ChangeOweSettle(settleList);
                    else
                    {
                        _rowFilterSettleStr = ConditionHelper.GetPriceViewCondition(_filterDic, _currentFilterDic);
                    }

                    CalBestPrice(this.Security);

                }
            }
            else if (operationType == OperationType.RemerberToCard && groupId == _currentCardItem.GroupId)
            {
                if (!string.IsNullOrEmpty(Security))
                {
                    _currentOperationType = OperationType.RemerberToCard;
                    if (obj != null)
                        _currentFilterDic = PriceCalHelper.Instance.CopyDictionary(obj as Dictionary<string, IList>);
                    else
                        _currentFilterDic = new Dictionary<string, IList>();

                    string settle = SettleCache.Instance.GetSettleById(_currentCardItem.GroupId, _currentCardItem.Id);
                    
                    if (!string.IsNullOrEmpty(settle))
                    {
                        List<string> settleList = new List<string>();
                        settleList.Add(settle);

                        if (_currentFilterDic.ContainsKey(CommonConstants.SETTLE))
                        {
                            bool isContain = ConditionHelper.IsContainSettle(settle, _currentFilterDic[CommonConstants.SETTLE]);
                            if (isContain)
                                return;

                            _currentFilterDic.Remove(CommonConstants.SETTLE);

                        }
                        _currentFilterDic.Add(CommonConstants.SETTLE, ConditionHelper.GetMainSettle(settleList));
                        
                        ChangeOweSettle(settleList);
                        //lock (this._lockObj)
                        //{
                        //    CalBestPrice(this.Security);

                        //}
                    }
                    else
                    {
                        _rowFilterSettleStr = ConditionHelper.GetPriceViewCondition(_filterDic, _currentFilterDic);

                        lock (this._lockObj)
                        {
                            CalBestPrice(this.Security);

                            if (string.IsNullOrEmpty(LastExecPrice) || LastExecPrice == CommonConstants.NA || LastExecPrice == CommonConstants.NO_PRICE)
                                RefreshLastExe(this.Security);
                        }
                    }
                }
            }
        }

        public void Instance_OnSelectBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_currentCardItem == null)
                {
                    _currentCardItem = new CardItem();
                }

                string currentId = _currentCardItem.Id;

                _currentCardItem.GroupId = groupId;
                _currentCardItem.Id = id;
                _currentCardItem.Security = security;


                if (actionFrom != BondUpdateType.ChangeRowSettleSignal)
                {
                    //if (this.Security == security)
                    if (currentId == id && !needRefresh)
                        return;
                }


                if (!needRefresh)
                {
                    if (_tabShowModel == TabShowModel.Card)
                    {
                        return;
                    }
                }

                if (actionFrom == BondUpdateType.ChangeRowSettle)
                {
                    List<string> settleList = new List<string>();
                    settleList.Add(settle);
                    ChangeOweSettle(settleList);
                }
                else
                {
                    _currentOperationType = OperationType.TableChangeFocusRow;

                    if (actionFrom != BondUpdateType.ChangeRowSettleSignal)
                    {

                        this.Clear();

                        this.Security = security;

                        lock (_LastCTPriceDic)
                        {
                            _needUpdateDic[security] = false;
                        }

                        if (!string.IsNullOrEmpty(this.Security))
                        {
                            BondGridVisibility = Visibility.Visible;
                            AddBondVisibility = Visibility.Collapsed;
                            SearchVisibility = Visibility.Collapsed;

                            OfrBrush = _ownUI.FindResource("ColorBrush.Blue_firm") as Brush;
                            BidBrush = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                            AmountBrush = _ownUI.FindResource("TextBox.Static.Foreground") as Brush;

                        }
                        else
                        {
                            BondGridVisibility = Visibility.Collapsed;
                            if (_tabShowModel == TabShowModel.Card)
                                AddBondVisibility = Visibility.Visible;
                            else
                                AddBondVisibility = Visibility.Collapsed;

                            SearchVisibility = Visibility.Collapsed;
                        }

                        if (string.IsNullOrEmpty(security))
                            return;

                        List<string> filterVenues = _filterDic.ContainsKey(CommonConstants.BROKER) ? _filterDic[CommonConstants.BROKER] : null;

                        if (filterVenues == null || filterVenues.Count == 0 || filterVenues.Count == PermisionHandler.Instance.GetBrokerCount())
                        {
                            PriceCalHelper.Instance.InitTableDataByPermission(BidTableView, filterVenues);

                        }
                        else
                        {
                            PriceCalHelper.Instance.InitTableDataByBroker(BidTableView, filterVenues[0]);
                        }

                        foreach (DataRow dr in BidTableView.Table.Rows)
                        {
                            dr[ALL_IS_NULL_FLAG_FIELD] = string.Empty;
                        }

                        //订阅price
                        //PriceHandler.Instance.RegisterCTPrice(this.Security, Instance_OnMDPrice);

                        UpdateSecurityInfo(security);
                    }
                    else
                    {
                        _currentOperationType = OperationType.TableChangeFocusRowSettle;
                    }

                    List<string> settleList = new List<string>();
                    settleList.Add(settle);
                    ChangeOweSettle(settleList);

                    if (actionFrom == BondUpdateType.ChangeFocusRow)
                    {
                        PriceHandler.Instance.RegisterCTPrice(this.Security, Instance_OnMDPrice);
                    }

                }
            }));
        }

        private void UpdateSecurityInfo(string security)
        {
            List<SimpleBondsInfoPo> result = CommonUtils.GetBondsInfoPo(security, 20);

            BondGridDataEquality _BondGridDataEq = new BondGridDataEquality();
            result = result.Distinct(_BondGridDataEq).ToList();

            if (result != null && result.Count > 0)
            {
                SimpleBondsInfoPo obj = result[0];
                this.SymbolName = obj.Short_Name;
                this.Security = obj.Bond_ID;
                this.RemainingTime = obj.RemaingingTime;
            }
        }

        #region Main settle filter

        public void OnSelectSettleChange(object obj)
        {
        }

        #endregion


        #region Filter
        
        List<string> _settleList = new List<string>();
        /// <summary>
        /// 处理我的订单的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessFilterCondition(Dictionary<string, IList> filterDic)
        {
            lock (_LastCTPriceDic)
            {
                bool isFromBroker = true;
                StringBuilder filterBuffer = new StringBuilder();
                filterDic = PriceCalHelper.Instance.CopyDictionary(filterDic);
                if (filterDic != null)
                {
                    if (!filterDic.ContainsKey(CommonConstants.BROKER))
                        isFromBroker = false;
                    
                    if (filterDic.ContainsKey(CommonConstants.SETTLE))
                    {
                        _settleList.Clear();
                        
                        StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                        IList settlTypeList = filterDic[CommonConstants.SETTLE];
                        bestCacheSettleFitlerBuf = ConditionHelper.GetSettleCondition(settlTypeList);
                        
                        _filterDic.Remove(CommonConstants.SETTLE);

                        if (settlTypeList.Count > 0)
                        {

                            _filterDic.Add(CommonConstants.SETTLE, _settleList);

                            foreach (ListBoxItem settlType in settlTypeList)
                            {
                                if (settlType.Content.ToString() == CommonConstants.ALL_PARTY)
                                {
                                    _settleList.Add(CommonConstants.T1_ONE);
                                    _settleList.Add(CommonConstants.T2_TWO);
                                    _settleList.Add(CommonConstants.T3_THREE);
                                    _settleList.Add(CommonConstants.T4_FOUR);
                                    _settleList.Add(CommonConstants.FWD);
                                    _settleList.Add(CommonConstants.FWD_EN);


                                    break;
                                }
                                else if (settlType.Content.ToString() == "+1")
                                {
                                    _settleList.Add(CommonConstants.T2_TWO);
                                    _settleList.Add(CommonConstants.T3_THREE);
                                    _settleList.Add(CommonConstants.T2_T3);
                                }
                                else if (settlType.Content.ToString() == "+0")
                                {
                                    _settleList.Add(CommonConstants.T1_ONE);
                                }
                                else
                                {
                                    _settleList.Add(CommonConstants.FWD);
                                    _settleList.Add(CommonConstants.FWD_EN);

                                }
                            }
                        }

                        if (bestCacheSettleFitlerBuf.Length > 0)
                        {
                            if (filterBuffer.Length > 0)
                            {
                                filterBuffer.Append($" and ({bestCacheSettleFitlerBuf})");
                            }
                            else
                            {
                                filterBuffer.Append($"({bestCacheSettleFitlerBuf})");
                            }
                        }
                    }

                    //设置全局结算过滤条件
                    this._rowFilterSettleStr = filterBuffer.ToString();

                    //全局Broker条件
                    if (_currentFilterDic != null && _currentFilterDic.ContainsKey(CommonConstants.BROKER))
                    {
                        IList mainVenueList = _currentFilterDic[CommonConstants.BROKER];
                        StringBuilder sbBroker = ConditionHelper.GetBrokerCondition(mainVenueList);
                        if (sbBroker.Length > 0)
                        {
                            _rowFilterSettleStr = !string.IsNullOrEmpty(_rowFilterSettleStr) ? string.Format("{0} and ({1})", _rowFilterSettleStr, sbBroker) : sbBroker.ToString();
                        }
                    }


                    if (filterDic.ContainsKey(CommonConstants.BROKER))
                    {
                        StringBuilder inFilterSB_venues = new StringBuilder();
                        // Broker
                        IList venuesList = filterDic[CommonConstants.BROKER];
                        _filterDic.Remove(CommonConstants.BROKER);

                        if (venuesList.Count > 0)
                        {
                            List<string> broker = new List<string>();
                            _filterDic.Add(CommonConstants.BROKER, broker);

                            bool isAll = false;

                            foreach (System.Windows.Controls.ListBoxItem venues in venuesList)
                            {
                                if (inFilterSB_venues.Length > 0)
                                {
                                    inFilterSB_venues.Append(" or ");
                                }

                                if (venues.Content.ToString() == CommonConstants.ALL_EN)
                                {
                                    inFilterSB_venues.Clear();

                                    isAll = true;

                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.TPSC_EN))
                                        broker.Add(CommonConstants.TPSC_EN);
                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.CCTB_EN))
                                        broker.Add(CommonConstants.CCTB_EN);
                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.CNEX_EN))
                                        broker.Add(CommonConstants.CNEX_EN);
                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.CTTJ_EN))
                                        broker.Add(CommonConstants.CTTJ_EN);
                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.PTCN_EN))
                                        broker.Add(CommonConstants.PTCN_EN);
                                    if (PermisionHandler.Instance.CheckBrokerPosition(CommonConstants.UEDA_EN))
                                        broker.Add(CommonConstants.UEDA_EN);

                                    inFilterSB_venues.Append($"({SOURCE_FIELD} = '{CommonConstants.CNEX_EN}' or {SOURCE_FIELD} = '{CommonConstants.TPSC_EN}' or {SOURCE_FIELD} = '{CommonConstants.CCTB_EN}' or {SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}' or {SOURCE_FIELD} = '{CommonConstants.PTCN_EN}'  or {SOURCE_FIELD} = '{CommonConstants.UEDA_EN}' or {SOURCE_FIELD} = '')");
                                    break;
                                }

                                switch (venues.Content.ToString())
                                {
                                    case CommonConstants.AllBroker:
                                        inFilterSB_venues.Append($"({SOURCE_FIELD} = '{CommonConstants.CNEX_EN}' or {SOURCE_FIELD} = '{CommonConstants.TPSC_EN}' or {SOURCE_FIELD} = '{CommonConstants.CCTB_EN}' or {SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}' or {SOURCE_FIELD} = '{CommonConstants.PTCN_EN}' or {SOURCE_FIELD} = '{CommonConstants.UEDA_EN}' or {SOURCE_FIELD} = '')");
                                        break;
                                    case CommonConstants.CNEX:
                                        broker.Add(CommonConstants.CNEX_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.CNEX_EN}'");
                                        break;
                                    case CommonConstants.TPSC:
                                        broker.Add(CommonConstants.TPSC_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.TPSC_EN}'");
                                        break;
                                    case CommonConstants.CCTB:
                                        broker.Add(CommonConstants.CCTB_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.CCTB_EN}'");
                                        break;
                                    case CommonConstants.CTTJ:
                                        broker.Add(CommonConstants.CTTJ_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}'");
                                        break;
                                    case CommonConstants.PTCN:
                                        broker.Add(CommonConstants.PTCN_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.PTCN_EN}'");
                                        break;
                                    case CommonConstants.UEDA:
                                        broker.Add(CommonConstants.UEDA_EN);
                                        inFilterSB_venues.Append($"{SOURCE_FIELD} = '{CommonConstants.UEDA_EN}'");
                                        break;
                                }
                            }

                            DataGrid dgBid = _ownUI.FindName("dgBid") as DataGrid;
                            if (isAll)
                                dgBid.RowHeight = 26;
                            else
                                dgBid.RowHeight = 30;

                        }

                        if (inFilterSB_venues.Length > 0 && isFromBroker)
                        {
                            if (filterBuffer.Length > 0)
                            {
                                filterBuffer.Append($" and ({inFilterSB_venues})");
                            }
                            else
                            {
                                filterBuffer.Append($"({inFilterSB_venues})");
                            }
                        }
                    }

                }

                this._rowFilterStr = filterBuffer.ToString();


                ClearParty(isFromBroker);

                List<string> filterVenues = _filterDic.ContainsKey(CommonConstants.BROKER) ? _filterDic[CommonConstants.BROKER] : null;

                if (filterVenues == null || filterVenues.Count == 0 || filterVenues.Count == PermisionHandler.Instance.GetBrokerCount())
                {
                    BrokerVisibility = Visibility.Collapsed;
                    AllVisibility = Visibility.Visible;

                    HeadVisibility = DataGridHeadersVisibility.Column;
                    
                    _isUpdateAll = true;

                    PriceCalHelper.Instance.InitTableDataByPermission(BidTableView, filterVenues);

                }
                else
                {
                    _isUpdateAll = false;
                    
                    BrokerVisibility = Visibility.Visible;

                    HeadVisibility = DataGridHeadersVisibility.None;

                    PriceCalHelper.Instance.InitTableDataByBroker(BidTableView, filterVenues[0]);
                }

                if (!string.IsNullOrEmpty(this.Security))
                {
                    foreach (DataRow dr in BidTableView.Table.Rows)
                    {
                        dr[ALL_IS_NULL_FLAG_FIELD] = string.Empty;
                    }
                }
                else
                {
                    foreach (DataRow dr in BidTableView.Table.Rows)
                    {
                        dr[ALL_IS_NULL_FLAG_FIELD] = "1";
                    }
                }


                MarketDataSnapshotFullRefresh refreshSignal = new MarketDataSnapshotFullRefresh();
                refreshSignal.SecurityID = this.Security;

                this.Instance_OnMDPrice(refreshSignal);

                if (string.IsNullOrEmpty(LastExecPrice) || LastExecPrice == CommonConstants.NA || LastExecPrice == CommonConstants.NO_PRICE)
                    RefreshLastExe(this.Security);
                
            }
            
        }

        private object GetUIByName(string name)
        {
            return _ownUI.FindName(name);
        }

        #endregion


        //手动修改过滤条件时触发
        private void RefreshLastExe(string security)
        {
            // 市场最新成交
            MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(security);
            if (lastExeItem != null)
            {


                ExecutionReport temOrder = new ExecutionReport();
                temOrder.SecurityID = security;
                temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                temOrder.Venues = lastExeItem.SourceTrader;
                this.Processer_OnLastExecutions(temOrder);
                
            }
        }


        public void DataGridMouseDoubleClick()
        {
            lock (_LastCTPriceDic)
            {
                DataGrid dgBid = _ownUI.FindName("dgBid") as DataGrid;
                int index = dgBid.Columns.IndexOf(dgBid.CurrentColumn);
                DataRowView dr = dgBid.CurrentItem as DataRowView;
                if (dr != null)
                {
                    string tradePermission = dr[PriceViewModel.TRADE_PERMISSION_FLAG_FIELD] as string;

                    if (!string.IsNullOrEmpty(tradePermission))
                    {
                        string source = dr[PriceViewModel.SOURCE_FIELD] as string;
                        //source = ComboSourceUtil.Instance.FindEnum(source, "tovenues");
                        string security = Security;
                        string price = string.Empty;
                        string qty = string.Empty;
                        bool isBid = false;
                        if (index == 0)
                        {
                            isBid = true;
                            price = dr[PriceViewModel.BID_FIELD] as string;
                            qty = dr[PriceViewModel.BID_AMOUNT_FIELD] as string;
                        }
                        else if (index == 1)
                        {
                            price = dr[PriceViewModel.OFR_FIELD] as string;
                            qty = dr[PriceViewModel.OFR_AMOUNT_FIELD] as string;
                        }

                        if (!string.IsNullOrEmpty(price))
                        {
                            decimal tmpprice = 0;
                            if (!decimal.TryParse(price, out tmpprice))
                            {
                                return;
                            }
                            string settle = string.Empty;

                            if (this._isUpdateAll)
                            {
                                settle = isBid ? (dr[BID_SETTLE_TYPE_FIELD] as string) : (dr[OFR_SETTLE_TYPE_FIELD] as string);
                            }
                            else
                                settle = dr[PriceViewModel.SETTLE_TYPE_FIELD] as string;

                            string orderinfotip = isBid ? dr[PriceViewModel.BID_CORNER_MARKET_TEXT_FIELD] as string : dr[PriceViewModel.OFR_CORNER_MARKET_TEXT_FIELD] as string;
                            DataTable s = dr.DataView.ToTable();

                            SendPointPriceViewLite sendPointPriceViewLite = new SendPointPriceViewLite(security, isBid, price, qty, settle, string.Empty, source, string.Empty, true, orderinfotip);
                            sendPointPriceViewLite.Show();
                        }
                    }
                }
            }
        }


        #region 角标处理


        /// <summary>
        /// 我的订单回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(ExecutionReport obj)
        {
            //UpdateMyOrders(obj);
            if (_ProcessMyOrdersThreadQueue != null)
            {
                _ProcessMyOrdersThreadQueue.Enqueue(obj);
            }
        }


        private void UpdateMyOrdersAsync(ExecutionReport obj)
        {
            if (string.IsNullOrEmpty(this.Security))
                return;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                
                    UpdateMyOrders(obj);
            }));

        }
        

        private void UpdateMyOrdersAsync(string securityId)
        {
            if (Application.Current != null)
            {
                ExecutionReport obj = new ExecutionReport();
                obj.SecurityID = securityId;

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateMyOrders(obj);
                }));
            }
        }

        /// <summary>
        /// 更新我的订单
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMyOrders(ExecutionReport obj)
        {
            lock (this._lockObj)
            {
                // 我的订单
                string filterStr = _rowFilterStr;
                DataRow bidOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(obj.SecurityID, CommonConstants.SIDE_BID_ONE, "0", filterStr);//this._rowFilterStr);//item[SETTLE_TYPE_FIELD] as string);
                if (bidOrder != null)
                {
                    // 最优价格 我的订单 比较， 是否相同
                    decimal bestBidPrice = CommonUtils.ConvertToDecimal(this.BestBid);
                    string myLstBidPrice = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                    decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);
                    string isPoint = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                    if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                    {
                        if (CommonUtils.IsNumeric(BestBid) && CommonUtils.IsNumeric(myLstBidPrice))
                        {
                            BidArrowVisibilly = Visibility.Visible;
                            if (bestBidPrice == myBidOrderPrice)
                            {
                                BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground-30%") as Brush;
                                BidArrowStroke = _ownUI.FindResource("Bid.textblock.foreground") as Brush;

                                BidArrowToolTxt = "本人有报价，且为市场最优";
                            }
                            else
                            {
                                BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                                BidArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;

                                BidArrowToolTxt = "本人有报价";
                            }

                        }
                    }
                    else
                    {
                        BidArrowVisibilly = Visibility.Collapsed;
                    }
                }
                else
                {
                    //是否包含我的订单
                    bidOrder = MyOrderProcesser.Processer.GetMyOrderByCondition(obj.SecurityID, CommonConstants.SIDE_BID_ONE, filterStr, string.Empty);//this._rowFilterStr);

                    if (bidOrder != null)
                    {
                        DataRow temDr = MyOrderProcesser.Processer.GetMyOrderByCondition(obj.SecurityID, CommonConstants.SIDE_BID_ONE, filterStr, "0");
                        if (temDr != null)
                            BidArrowVisibilly = Visibility.Visible;
                        else
                            BidArrowVisibilly = Visibility.Collapsed;

                        
                        BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                        BidArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;
                    }
                    else
                    {
                        BidArrowVisibilly = Visibility.Collapsed;
                    }

                }

                DataRow ofrOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(obj.SecurityID, CommonConstants.SIDE_OFR_TWO, "0", filterStr);//this._rowFilterStr);
                if (ofrOrder != null)
                {
                    // 最优价格 我的订单 比较， 是否相同
                    decimal bestOfrPrice = CommonUtils.ConvertToDecimal(BestOfr);
                    string myLstOfrPrice = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                    decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);
                    string isPoint = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                    if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                    {
                        if (CommonUtils.IsNumeric(BestOfr) && CommonUtils.IsNumeric(myLstOfrPrice))
                        {
                            OfrArrowVisibilly = Visibility.Visible;
                            if (bestOfrPrice == myOfrOrderPrice)
                            {
                                OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush-30%") as Brush;
                                OfrArrowStroke = _ownUI.FindResource("ColorBrush.Blue_firm") as Brush;

                                OfrArrowToolTxt = "本人有报价，且为市场最优";
                            }
                            else
                            {
                                OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush") as Brush;
                                OfrArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;

                                OfrArrowToolTxt = "本人有报价";
                            }
                        }
                    }
                    else
                    {
                        OfrArrowVisibilly = Visibility.Collapsed;
                    }
                }
                else
                {
                    //是否包含我的订单
                    ofrOrder = MyOrderProcesser.Processer.GetMyOrderByCondition(obj.SecurityID, CommonConstants.SIDE_OFR_TWO, filterStr, string.Empty);//this._rowFilterStr);

                    if (ofrOrder != null)
                    {
                        DataRow temDr = MyOrderProcesser.Processer.GetMyOrderByCondition(obj.SecurityID, CommonConstants.SIDE_OFR_TWO, filterStr, "0");
                        if (temDr != null)
                            OfrArrowVisibilly = Visibility.Visible;
                        else
                            OfrArrowVisibilly = Visibility.Collapsed;

                        
                        OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush") as Brush;
                        OfrArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;
                    }
                    else
                    {
                        OfrArrowVisibilly = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion
    }
}
