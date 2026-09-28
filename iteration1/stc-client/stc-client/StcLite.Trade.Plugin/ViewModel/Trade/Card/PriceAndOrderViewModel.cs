using com.gateway.log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STC.Common.Model;
using STC.Common.View.Component;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using SettleFilterView = BDB.Trade.Plugin.Component.SettleFilterView;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;

using BDB.Price.Plugin.Model;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Handler;
using BDB.Price.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.Model;
using BDB.Price.Plugin.Cache;
using BDB.Price.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.Component;
using BDB.Price.Plugin.View.Trade;
using BDB.Price.Plugin.ViewModel.Trade;
using BDB.Trade.Plugin.View.Trade;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler.DataProcesser;
using STC.Common.Model.Common;
using STC.Common.ViewModel.Data;
using BDB.Price.Plugin.Handler;
using STC.Common.Utils;
using BDB.Common.Model.OHLC;
using Newtonsoft.Json.Linq;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class PriceAndOrderViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;


        private CardShowType cardShowType = CardShowType.InCardPage;

        public CardShowType CardShowType
        {
            get { return cardShowType; }
            set
            {
                cardShowType = value;
            }
        }

        private TabShowModel showModel = TabShowModel.Card;

        public TabShowModel ShowModel
        {
            get { return showModel; }
            set
            {
                showModel = value;
            }
        }


        public FrameworkElement OwnUI { get { return _ownUI; } set { _ownUI = value; } }


        private object _lockObj = new object();
        private object _ohlcLockObj = new object();

        private Dictionary<string, List<string>> _filterDic = new Dictionary<string, List<string>>();

        private bool isFuture = false;

        private int rowIndex;
        public int RowIndex
        {
            get { return rowIndex; }
            set { rowIndex = value; }
        }
        private int colIndex;
        public int ColIndex
        {
            get { return colIndex; }
            set { colIndex = value; }
        }

        private int pageIndex;
        public int PageIndex
        {
            get { return pageIndex; }
            set { pageIndex = value; }
        }

        #region 关联属性
        private string _id = string.Empty;

        public string Id { get { return _id; } set { _id = value; this.OnPropertyChanged(); } }

        private string _groupId = string.Empty;

        public string GroupId { get { return _groupId; } set { _groupId = value; this.OnPropertyChanged(); } }

        private decimal _customSort;
        public decimal CustomSort { get { return _customSort; } set { _customSort = value; this.OnPropertyChanged(); } }

        private string _bondType;
        public string BondType { get { return _bondType; } set { _bondType = value; this.OnPropertyChanged(); } }

        private string _bondCategory;
        public string BondCategory { get { return _bondCategory; } set { _bondCategory = value; this.OnPropertyChanged(); } }

        private int _remaingingTimeConvert;
        public int RemaingingTimeConvert { get { return _remaingingTimeConvert; } set { _remaingingTimeConvert = value; this.OnPropertyChanged(); } }

        private string updatetime;
        public string UpdateTime { get { return updatetime; } set { updatetime = value; this.OnPropertyChanged(); } }


        private PriceModel _priceModel = new PriceModel();

        public PriceModel PriceModel { get { return _priceModel; } set { _priceModel = value; } }

        private int _settleIndex = 0;

        public int SettleIndex
        {
            get { return _settleIndex; }
            set
            {
                int oldVla = _settleIndex;
                _settleIndex = value;

                if (oldVla != _settleIndex)
                {

                    this.OnPropertyChanged();
                }
            }
        }


        private Dictionary<string, IList> _settleFilterDic;
        public Dictionary<string, IList> SettleFilterDic
        {
            get { return _settleFilterDic; }
            set
            {
                Dictionary<string, IList> oldVal = _settleFilterDic;
                _settleFilterDic = PriceCalHelper.Instance.CopyDictionary(value);

                if ((value == null || value.Count == 0) && (oldVal == null || oldVal.Count == 0))
                    return;

                //this.OnPropertyChanged();

                this.ChangeBrokerShow();

                if (_currentOperationType == OperationType.None && this.cardShowType == CardShowType.InCardPage)
                {
                    //BondChangeHandler.Instance.OnMainSettleChange(OperationType.CardToMain, _groupId, null);
                    BondChangeHandler.Instance.OnOneSettleChanged(OperationType.CardToTable, _groupId, _id, ConditionHelper.GetMainSettle(value));
                }


            }
        }

        private int _brokerIndex = 0;

        public int BrokerIndex
        {
            get { return _brokerIndex; }
            set
            {
                int oldVal = _brokerIndex;
                _brokerIndex = value;

                if (oldVal != _brokerIndex)
                {
                    ChangeBrokerShow();
                    this.OnPropertyChanged();

                    //BondChangeHandler.Instance.OnOneBrokerChanged(OperationType.CardToMain, string.Empty);
                }
            }
        }

        private MsgSettingItem _msgSettingItem;

        public MsgSettingItem MsgSettingItem
        {
            get { return _msgSettingItem; }
            set
            {
                if (_msgSettingItem != value)
                {

                    if (_msgSettingItem != null)
                    {
                        _msgSettingItem.PropertyChanged -= Value_PropertyChanged;
                    }
                    _msgSettingItem = value;
                    if (value != null)
                    {
                        _priceModel.BellStatus = value.Status;
                        value.PropertyChanged += Value_PropertyChanged;
                    }
                    else
                    {
                        _priceModel.BellStatus = string.Empty;
                    }
                    this.OnPropertyChanged();
                }
                
            }
        }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                MsgSettingItem msi = sender as MsgSettingItem;
                if (msi != null)
                {
                    _priceModel.BellStatus = msi.Status;
                }
            }
        }

        private BrokerPriceViewModel _oneBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel OneBrokerPriceViewModel { get { return _oneBrokerPriceViewModel; } set { _oneBrokerPriceViewModel = value; } }

        private BrokerPriceViewModel _twoBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel TwoBrokerPriceViewModel { get { return _twoBrokerPriceViewModel; } set { _twoBrokerPriceViewModel = value; } }

        private BrokerPriceViewModel _threeBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel ThreeBrokerPriceViewModel { get { return _threeBrokerPriceViewModel; } set { _threeBrokerPriceViewModel = value; } }

        private BrokerPriceViewModel _fourBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel FourBrokerPriceViewModel { get { return _fourBrokerPriceViewModel; } set { _fourBrokerPriceViewModel = value; } }

        private BrokerPriceViewModel _fiveBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel FiveBrokerPriceViewModel { get { return _fiveBrokerPriceViewModel; } set { _fiveBrokerPriceViewModel = value; } }
        private BrokerPriceViewModel _sixBrokerPriceViewModel = new BrokerPriceViewModel();
        public BrokerPriceViewModel SixBrokerPriceViewModel { get { return _sixBrokerPriceViewModel; } set { _sixBrokerPriceViewModel = value; } }

        public ICommand DelteCommand { get { return new RelayCommand(OnDelteClick); } }

        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }

        public ICommand PopCommand { get { return new RelayCommand(OnPopClick); } }

        public ICommand EditCommand { get { return new RelayCommand(OnEditClick); } }

        public ICommand MsgCommand { get { return new RelayCommand(OnMsgClick); } }

        #endregion

        public void OnAddClick()
        {
            PriceModel.AddBondVisibility = Visibility.Collapsed;
            //PriceModel.BondGridVisibility = Visibility.Collapsed;
            PriceModel.SearchVisibility = Visibility.Visible;
        }


        private void OnDelteClick()
        {
            if (this.CardShowType == CardShowType.InCardPage)
            {
                string text = string.Format("是否确定删除该债券({0} {1})？", this.PriceModel.Security, this.PriceModel.SymbolName);
                string title = "删除债券";
                Window w = Window.GetWindow(this._ownUI);
                if (CustomMessageBox.Question(text, title, w) == true)
                {
                    HomePageDataHandler.Instance.DelBond(DisplayMode.Card, _groupId, _id, this.PriceModel.Security);


                }
            }
            else
            {
                Window window = CommonUtils.GetParentObject<Window>(this._ownUI) as Window;
                if (window != null)
                {
                    window.Close();
                }
            }
        }
        

        private void OnPopClick()
        {
            object obj = CardWinCache.Instance.GetBySecurity(_priceModel.Security);
            if (obj == null)
            {
                CardWindow cardWindow = new CardWindow();
                cardWindow.SecurityId = _priceModel.Security;
                cardWindow.Show();

                CardWinCache.Instance.AddOrUpdate(_priceModel.Security, cardWindow);
            }
            else
            {
                CardWindow cardWindow = obj as CardWindow;
                cardWindow.Activate();
            }
        }

        private void OnEditClick()
        {
            object obj = CardWinCache.Instance.GetBySecurity(_priceModel.Security);

            if (this.cardShowType == CardShowType.AloneWindow)
            {
                PriceModel.AddBondVisibility = Visibility.Collapsed;
                //PriceModel.BondGridVisibility = Visibility.Collapsed;
                PriceModel.SearchVisibility = Visibility.Visible;
                PriceModel.EditVisibility = Visibility.Collapsed;

                SearchEditUI seSearch = _ownUI.FindName("seSearch") as SearchEditUI;

                seSearch.SearchBox.Text = PriceModel.Security;

                seSearch.SetFocus();
            }
        }

        private void OnMsgClick()
        {
            string winKey = $"MsgSettingWin-{this.PriceModel.Security}";
            if (!WindowManager.Instance.IsExist(winKey))
            {
                MsgSettingWindow msgSettingWin = new MsgSettingWindow();
                if (msgSettingWin.DataContext is MsgSettingViewModel) 
                {
                    MsgSettingViewModel vm = msgSettingWin.DataContext as MsgSettingViewModel;
                    Price.Plugin.ViewModel.Data.MsgSettingItem msgSettingItem = MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(this.PriceModel.Security, true);

                    msgSettingItem.IsBidLimit_Edit = msgSettingItem.IsBidLimit;
                    msgSettingItem.BestBidLimit_Edit = msgSettingItem.BestBidLimit;
                    msgSettingItem.IsOfrLimit_Edit = msgSettingItem.IsOfrLimit;
                    msgSettingItem.BestOfrLimit_Edit = msgSettingItem.BestOfrLimit;
                    msgSettingItem.IsBestBp_Edit = msgSettingItem.IsBestBp;
                    msgSettingItem.BestBp_Edit = msgSettingItem.BestBp;
                    msgSettingItem.IsBidPrice_Edit = msgSettingItem.IsBidPrice;
                    msgSettingItem.IsOfrPrice_Edit = msgSettingItem.IsOfrPrice;
                    msgSettingItem.IsExecPrice_Edit = msgSettingItem.IsExecPrice;

                    this.MsgSettingItem = msgSettingItem;
                    this.PriceModel.BellStatus = msgSettingItem.Status;
                    vm.Item = msgSettingItem;
                    vm.Item.SecurityID = this.PriceModel.Security;
                    vm.SymbolName = this.PriceModel.SymbolName;
                }
                WindowManager.Instance.OpenOnlyOneWindow(winKey, msgSettingWin);
            }

        }


        public void SetAloneWindowModel()
        {
            if (this.cardShowType == CardShowType.AloneWindow)
            {
                StackPanel stackPanel = _ownUI.FindName("spTitle") as StackPanel;
                if (stackPanel != null)
                {
                    stackPanel.Margin = new Thickness(16, 0, 0, 0);
                }

                CustomBrokerFilterView brokerFilter = _ownUI.FindName("brokerFilter") as CustomBrokerFilterView;
                if (brokerFilter != null)
                {
                    brokerFilter.CardShowType = CardShowType.AloneWindow;
                }

                PriceModel.EditVisibility = Visibility.Visible;
            }
        }

        public MarketOrderViewModel GetMarketOrderViewModel()
        {
            MarketOrderView marketOrderView = _ownUI.FindName("marketView") as MarketOrderView;
            MarketOrderViewModel marketCardOrderViewModel = marketOrderView.DataContext as MarketOrderViewModel;

            return marketCardOrderViewModel;
        }

        private MarketOrderView GetMarketOrderView()
        {
            MarketOrderView marketOrderView = _ownUI.FindName("marketView") as MarketOrderView;

            return marketOrderView;
        }


        private MarketCardOrderView GetMarketCardOrderView()
        {
            MarketCardOrderView marketCardOrderView = _ownUI.FindName("marketView") as MarketCardOrderView;

            return marketCardOrderView;
        }

        private SettleFilterView GetSettleFilterView()
        {
            SettleFilterView settleFilterView = _ownUI.FindName("settleFilter") as SettleFilterView;

            return settleFilterView;
        }

        private CustomBrokerFilterView GetBrokerFilterView()
        {
            CustomBrokerFilterView brokerFilter = _ownUI.FindName("brokerFilter") as CustomBrokerFilterView;

            return brokerFilter;
        }

        public void OnDelete()
        {
            lock (_lockObj)
            {
                Init(_ownUI, string.Empty, string.Empty, _groupId);
                OnClear();

                if (string.IsNullOrEmpty(_priceModel.Security))
                {
                    CustomBrokerFilterView customBrokerFilterView = _ownUI.FindName("brokerFilter") as CustomBrokerFilterView;
                    customBrokerFilterView.SetIndex(0);
                    BrokerIndex = 0;
                }
            }

            GetMarketOrderViewModel().ClearData(false);

        }

        public void InitByPageChange(FrameworkElement ownUI, string securityId, string id, string groupId)
        {
            Init(ownUI, securityId, id, groupId);
            // card 深度行情 ui 初始化
            OnClear();
            // card 成交数据 ui 初始化
            GetMarketOrderViewModel().ClearData(false);

        }


        public void Init(FrameworkElement ownUI, string security, string id, string groupId)
        {
            if (ownUI != null)
                _ownUI = ownUI;

            _id = id;
            _groupId = groupId;

            _priceModel.Security = security.ToLower() == "tradebook" ? "" : security;

            
            this.MsgSettingItem = Price.Plugin.Handler.MsgSettingHandler.Instance.GetMsgSettingItemBySecurtiyID(security);
            if (this.MsgSettingItem != null)
                this.PriceModel.BellStatus = this.MsgSettingItem.Status;
            else
                this.PriceModel.BellStatus = string.Empty;

            string emptyStr = GetEmptyStr();

            _oneBrokerPriceViewModel.Security = security;
            _oneBrokerPriceViewModel.BrokerIndex = 0;
            _oneBrokerPriceViewModel.Clear(emptyStr);

            string brokerEN = PermisionHandler.Instance.GetBrokerByIndex(1);
            _oneBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _oneBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _oneBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _oneBrokerPriceViewModel.PriceModel = this.PriceModel;



            _twoBrokerPriceViewModel.Security = security;
            _twoBrokerPriceViewModel.BrokerIndex = 1;
            brokerEN = PermisionHandler.Instance.GetBrokerByIndex(2);
            _twoBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _twoBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _twoBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _twoBrokerPriceViewModel.Clear(emptyStr);
            _twoBrokerPriceViewModel.PriceModel = this.PriceModel;

            _threeBrokerPriceViewModel.Security = security;
            _threeBrokerPriceViewModel.BrokerIndex = 2;
            brokerEN = PermisionHandler.Instance.GetBrokerByIndex(3);
            _threeBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _threeBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _threeBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _threeBrokerPriceViewModel.Clear(emptyStr);
            _threeBrokerPriceViewModel.PriceModel = this.PriceModel;

            _fourBrokerPriceViewModel.Security = security;
            _fourBrokerPriceViewModel.BrokerIndex = 3;
            brokerEN = PermisionHandler.Instance.GetBrokerByIndex(4);
            _fourBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _fourBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _fourBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _fourBrokerPriceViewModel.Clear(emptyStr);
            _fourBrokerPriceViewModel.PriceModel = this.PriceModel;

            _fiveBrokerPriceViewModel.Security = security;
            _fiveBrokerPriceViewModel.BrokerIndex = 4;
            brokerEN = PermisionHandler.Instance.GetBrokerByIndex(5);
            _fiveBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _fiveBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _fiveBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _fiveBrokerPriceViewModel.Clear(emptyStr);
            _fiveBrokerPriceViewModel.PriceModel = this.PriceModel;

            _sixBrokerPriceViewModel.Security = security;
            _sixBrokerPriceViewModel.BrokerIndex = 5;
            brokerEN = PermisionHandler.Instance.GetBrokerByIndex(6);
            _sixBrokerPriceViewModel.BrokerBlockBG = GetBrushByBroker(brokerEN);
            _sixBrokerPriceViewModel.BrokerBlockFG = GetFBBrushByBroker(brokerEN);
            _sixBrokerPriceViewModel.BrokerCN = ComboSourceUtil.Instance.FindEnum(brokerEN, "venues");
            _sixBrokerPriceViewModel.Clear(emptyStr);
            _sixBrokerPriceViewModel.PriceModel = this.PriceModel;

            if (!string.IsNullOrEmpty(security))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(security);
                if (bondsInfoPo != null)
                {
                    _priceModel.Security = bondsInfoPo.Bond_ID;
                    _priceModel.SymbolName = bondsInfoPo.Short_Name;
                    _priceModel.RemainingTime = bondsInfoPo.RemaingingTime;
                    _priceModel.MarketIndicator = bondsInfoPo.MarketIndicator;
                    if (bondsInfoPo.MarketIndicator == "5")//国债期货
                    {
                        isFuture = true;

                        _priceModel.BrokerConditionHeight = 0;
                        _priceModel.BrokerPriceShowHeight = 0;

                        _priceModel.IndexVisibility = Visibility.Visible;
                        _priceModel.SettleVisibility = Visibility.Collapsed;

                        _priceModel.FutureRealDataVisibility = Visibility.Visible;
                        _priceModel.OHLCVisibility = Visibility.Visible;

                        _priceModel.IndexTxt = "昨";
                        _priceModel.IndexTxt2 = "开";

                        _priceModel.SymbolNameVisibilly = Visibility.Collapsed;

                        _priceModel.CodeColWith = "60*";
                        _priceModel.SymbolNameColWith = "90*";
                        _priceModel.MaturityColWith = "Auto";

                        _priceModel.FutureVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        isFuture = false;

                        _priceModel.BrokerConditionHeight = 24;
                        _priceModel.BrokerPriceShowHeight = 156;

                        _priceModel.IndexVisibility = Visibility.Collapsed;
                        _priceModel.SettleVisibility = Visibility.Visible;

                        _priceModel.FutureRealDataVisibility = Visibility.Collapsed;
                        _priceModel.OHLCVisibility = Visibility.Visible;

                        _priceModel.IndexTxt = "估";
                        _priceModel.IndexTxt2 = "昨";

                        _priceModel.SymbolNameVisibilly = Visibility.Visible;

                        _priceModel.CodeColWith = "120*";
                        _priceModel.SymbolNameColWith = "90*";
                        _priceModel.MaturityColWith = "70*";

                        _priceModel.FutureVisibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (security.StartsWith("T") && !security.Contains("."))//国债期货
                    {
                        isFuture = true;

                        _priceModel.BrokerConditionHeight = 0;
                        _priceModel.BrokerPriceShowHeight = 0;

                        _priceModel.IndexVisibility = Visibility.Visible;
                        _priceModel.SettleVisibility = Visibility.Collapsed;

                        _priceModel.FutureRealDataVisibility = Visibility.Visible;
                        _priceModel.OHLCVisibility = Visibility.Collapsed;

                        _priceModel.IndexTxt = "昨";
                        _priceModel.IndexTxt2 = "开";

                        _priceModel.SymbolNameVisibilly = Visibility.Collapsed;

                        _priceModel.CodeColWith = "60*";
                        _priceModel.SymbolNameColWith = "90*";
                        _priceModel.MaturityColWith = "Auto";
                    }
                    else
                    {
                        isFuture = false;

                        _priceModel.BrokerConditionHeight = 24;
                        _priceModel.BrokerPriceShowHeight = 156;

                        _priceModel.IndexVisibility = Visibility.Collapsed;
                        _priceModel.SettleVisibility = Visibility.Visible;

                        _priceModel.FutureRealDataVisibility = Visibility.Collapsed;
                        _priceModel.OHLCVisibility = Visibility.Collapsed;

                        _priceModel.IndexTxt = "估";
                        _priceModel.IndexTxt2 = "昨";

                        _priceModel.SymbolNameVisibilly = Visibility.Visible;

                        _priceModel.CodeColWith = "120*";
                        _priceModel.SymbolNameColWith = "90*";
                        _priceModel.MaturityColWith = "70*";
                    }
                }
                _priceModel.SearchVisibility = Visibility.Collapsed;
                _priceModel.AddBondVisibility = Visibility.Collapsed;
                _priceModel.BondGridVisibility = Visibility.Visible;
            }
            else
            {
                _priceModel.SymbolName = string.Empty;
                _priceModel.RemainingTime = string.Empty;
                _priceModel.MarketIndicator = string.Empty;

                isFuture = false;

                _priceModel.SearchVisibility = Visibility.Collapsed;
                _priceModel.BondGridVisibility = Visibility.Collapsed;
                _priceModel.AddBondVisibility = Visibility.Visible;

                _priceModel.IndexVisibility = Visibility.Collapsed;
                _priceModel.SettleVisibility = Visibility.Collapsed;

                _priceModel.FutureRealDataVisibility = Visibility.Collapsed;
                _priceModel.OHLCVisibility = Visibility.Collapsed;
            }

            _priceModel.CCDCValuation = CommonConstants.NO_PRICE;

        }

        public void AdapterFuture(string security)
        {
            if (!string.IsNullOrEmpty(security))
            {
                //BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(security);
                bool isFutureBond = CommonUtils.IsFutureBond(security);
                //if ((bondsInfoPo != null && bondsInfoPo.MarketIndicator == "5") || (security.StartsWith("T") && !security.Contains(".")))//国债期货
                if (isFutureBond)
                {
                    isFuture = true;

                    _priceModel.BrokerConditionHeight = 0;
                    _priceModel.BrokerPriceShowHeight = 0;

                    _priceModel.IndexVisibility = Visibility.Visible;
                    _priceModel.SettleVisibility = Visibility.Collapsed;

                    _priceModel.FutureRealDataVisibility = Visibility.Visible;
                    _priceModel.OHLCVisibility = Visibility.Visible;

                    _priceModel.IndexTxt = "昨";
                    _priceModel.IndexTxt2 = "开";

                    _priceModel.SymbolNameVisibilly = Visibility.Collapsed;

                    _priceModel.UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                    _priceModel.UpSideBgBrush = (Brush)_ownUI.FindResource("Black-1");


                    _priceModel.CodeColWith = "60*";
                    _priceModel.SymbolNameColWith = "90*";
                    _priceModel.MaturityColWith = "Auto";
                }
                else
                {
                    isFuture = false;

                    _priceModel.BrokerConditionHeight = 24;
                    _priceModel.BrokerPriceShowHeight = 156;

                    _priceModel.CodeColWith = "120*";
                    _priceModel.SymbolNameColWith = "90*";
                    _priceModel.MaturityColWith = "70*";

                    _priceModel.IndexVisibility = Visibility.Collapsed;
                    _priceModel.SettleVisibility = Visibility.Visible;

                    _priceModel.FutureRealDataVisibility = Visibility.Collapsed;
                    _priceModel.OHLCVisibility = Visibility.Visible;
                    _priceModel.IndexTxt = "估";
                    _priceModel.IndexTxt2 = "昨";

                    ChangeBrokerShow();
                }

            }
        }

        private Brush GetBrushByBroker(string broker)
        {
            Brush result = null;
            if (OwnUI != null)
            {
                if (broker == CommonConstants.TPSC_EN)
                {
                    result = this.OwnUI.FindResource("TPSC.BorderBrush") as Brush;
                }
                else if (broker == CommonConstants.CNEX_EN)
                {
                    result = this.OwnUI.FindResource("CNEX.BorderBrush") as Brush;
                }
                else if (broker == CommonConstants.CCTB_EN)
                {
                    result = this.OwnUI.FindResource("CCTB.BorderBrush") as Brush;
                }
                else if (broker == CommonConstants.PTCN_EN)
                {
                    result = this.OwnUI.FindResource("PTCN.BorderBrush") as Brush;
                }
                else if (broker == CommonConstants.CTTJ_EN)
                {
                    result = this.OwnUI.FindResource("CTTJ.BorderBrush") as Brush;
                }
                else if (broker == CommonConstants.UEDA_EN)
                {
                    result = this.OwnUI.FindResource("UEDA.BorderBrush") as Brush;
                }
            }
            return result;
        }

        private Brush GetFBBrushByBroker(string broker)
        {
            Brush result = null;
            if (OwnUI != null)
            {
                if (broker == CommonConstants.TPSC_EN)
                {
                    result = this.OwnUI.FindResource("White-1") as Brush;
                }
                else if (broker == CommonConstants.CNEX_EN)
                {
                    result = this.OwnUI.FindResource("White-1") as Brush;
                }
                else if (broker == CommonConstants.CCTB_EN)
                {
                    result = this.OwnUI.FindResource("White-1") as Brush;
                }
                else if (broker == CommonConstants.PTCN_EN)
                {
                    result = this.OwnUI.FindResource("White-1") as Brush;
                }
                else if (broker == CommonConstants.CTTJ_EN)
                {
                    result = this.OwnUI.FindResource("White-1") as Brush;
                }
                else if (broker == CommonConstants.UEDA_EN)
                {
                    result = this.OwnUI.FindResource("Black-1") as Brush;
                }
            }
            return result;
        }

        private void InitCondition()
        {
            object obj = MainConditionCache.Instance.GetConditionById(GroupId);
            Dictionary<string, IList> dic = obj as Dictionary<string, IList>;
            if (dic != null)
            {
                if (dic.ContainsKey(CommonConstants.SETTLE))
                {
                    Dictionary<string, IList> temDic = new Dictionary<string, IList>();

                    temDic.Add(CommonConstants.SETTLE, dic[CommonConstants.SETTLE]);

                    this.SetSettle(OperationType.Card, temDic);

                }

                if (dic.ContainsKey(CommonConstants.BROKER))
                {
                    Dictionary<string, IList> temDic = new Dictionary<string, IList>();

                    temDic.Add(CommonConstants.BROKER, dic[CommonConstants.BROKER]);

                    this.SetBroker(OperationType.Card, temDic);
                }

                if (dic.ContainsKey(CommonConstants.EXECTYPE))
                {
                    Dictionary<string, IList> temDic = new Dictionary<string, IList>();

                    temDic.Add(CommonConstants.EXECTYPE, dic[CommonConstants.EXECTYPE]);

                    this.SetExecType(OperationType.Card, temDic);
                }
            }
        }

        public void InitData()
        {
            //if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(PriceModel.Security))
            {
                //string cacheSettle = SettleCache.Instance.GetSettleById(GroupId, Id);
                //if (!string.IsNullOrEmpty(cacheSettle))
                //{
                //    List<string> list = new List<string>();
                //    list.Add(cacheSettle);
                //    SetSettle(OperationType.RemerberToCard, ConditionHelper.SettleListToDic(list));
                //}


                InitCondition();


                MarketDataSnapshotFullRefresh refreshSignal = new MarketDataSnapshotFullRefresh();
                refreshSignal.SecurityID = PriceModel.Security;

                OnPrice(refreshSignal, false);


                MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(PriceModel.Security);
                if (lastExeItem != null)
                {
                    ExecutionReport temOrder = new ExecutionReport();
                    temOrder.SecurityID = PriceModel.Security;
                    temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                    temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                    temOrder.Venues = lastExeItem.SourceTrader;

                    OnLastExec(temOrder);
                }

                //MarketOrderView marketOrderView = GetMarketOrderView();
                //if (marketOrderView != null)
                //{
                //    marketOrderView.Init(ViewModel.Data.TabShowModel.Card, PriceModel.Security);
                //}
            }
        }

        public void SetMarketShowModel(TabShowModel showModel)
        {
            MarketOrderView marketOrderView = GetMarketOrderView();
            if (marketOrderView != null)
            {
                marketOrderView.SetShowModel(showModel);
            }
        }

        public void InitMarketData()
        {
            MarketOrderView marketOrderView = GetMarketOrderView();
            if (marketOrderView != null)
            {
                marketOrderView.Init(ViewModel.Data.TabShowModel.Card, PriceModel.Security);
            }
        }

        public void InitMarketDataByCardWind()
        {
            MarketOrderView marketOrderView = GetMarketOrderView();
            if (marketOrderView != null)
            {
                marketOrderView.Init(ViewModel.Data.TabShowModel.Card, PriceModel.Security, this.cardShowType);
            }
        }


        public void SetBroker(OperationType operationType, object broker)
        {
            bool isFuture = CommonUtils.IsFutureBond(_priceModel.Security);

            if (!isFuture && broker != null)
            {
                Dictionary<string, IList> dic = broker as Dictionary<string, IList>;
                if (dic != null)
                {
                    SetOperationType(operationType);

                    if (dic.ContainsKey(CommonConstants.BROKER))
                    {
                        IList brokerList = dic[CommonConstants.BROKER];
                        if (brokerList != null && brokerList.Count > 0)
                        {
                            GetBrokerFilterView().SetIndexByContent((brokerList[0] as ListBoxItem).Content.ToString());
                        }
                    }
                    SetOperationType(OperationType.None);
                }
            }
        }


        public void SetSettle(OperationType operationType, object settle)
        {
            bool isFuture = CommonUtils.IsFutureBond(_priceModel.Security);

            if (!isFuture && settle != null)
            {
                Dictionary<string, IList> dic = settle as Dictionary<string, IList>;
                if (dic != null)
                {
                    SetOperationType(operationType);

                    if (dic.ContainsKey(CommonConstants.SETTLE))
                    {
                        GetSettleFilterView().SetItemSelectedStatus(dic);
                    }
                    SetOperationType(OperationType.None);
                }
            }
        }

        public void SetExecType(OperationType operationType, object execType)
        {
            if (execType != null)
            {
                Dictionary<string, IList> dic = execType as Dictionary<string, IList>;
                if (dic != null)
                {

                    SetOperationType(operationType);

                    if (dic.ContainsKey(CommonConstants.EXECTYPE))
                    {
                        IList list = dic[CommonConstants.EXECTYPE];
                        int type = 0;
                        if (list != null && list.Count > 0)
                        {
                            type = (int)list[0];
                        }
                        GetMarketOrderView().SetExecType(type);
                    }

                }
                SetOperationType(OperationType.None);
            }
        }

        public void OnClear()
        {
            string emptyStr = GetEmptyStr();

            OneBrokerPriceViewModel.OnClear(emptyStr);
            TwoBrokerPriceViewModel.OnClear(emptyStr);
            ThreeBrokerPriceViewModel.OnClear(emptyStr);
            FourBrokerPriceViewModel.OnClear(emptyStr);
            FiveBrokerPriceViewModel.OnClear(emptyStr);
            SixBrokerPriceViewModel.OnClear(emptyStr);

            PriceModel.OnClear(emptyStr);

            _priceModel.UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
            _priceModel.UpSideBgBrush = (Brush)_ownUI.FindResource("NormalBrush");
            _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("TextBox.Static.Foreground");

            AnimationHelper.Instance.StopAndClear();
        }

        private string GetEmptyStr()
        {
            string emptyStr = CommonConstants.NO_PRICE;
            if (string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(PriceModel.Security))
            {
                emptyStr = string.Empty;
            }

            return emptyStr;
        }


        #region 过滤条件设置

        private void ChangeBrokerShow()
        {
            lock (_lockObj)
            {
                if (BrokerIndex == 0)
                {
                    string emptyStr = GetEmptyStr();

                    PriceModel.BrokerVisibility = Visibility.Collapsed;
                    PriceModel.BrokerHeight = 26;
                    PriceModel.AllBrokerHeight = 26;
                    PriceModel.LastBrokerHeight = 26;
                    PriceModel.BestPriceHeight = 40;

                    bool isFutureInner = CommonUtils.IsFutureBond(PriceModel.Security);

                    _priceModel.BrokerPriceShowHeight = isFutureInner ? 0 : 156;

                    OneBrokerPriceViewModel.Clear(emptyStr);
                    TwoBrokerPriceViewModel.Clear(emptyStr);
                    ThreeBrokerPriceViewModel.Clear(emptyStr);
                    FourBrokerPriceViewModel.Clear(emptyStr);
                    FiveBrokerPriceViewModel.Clear(emptyStr);
                    SixBrokerPriceViewModel.Clear(emptyStr);
                }
                else
                {
                    _priceModel.BrokerVisibility = Visibility.Visible;
                    PriceModel.BrokerHeight = 32;

                    _priceModel.BrokerPriceShowHeight = 196;

                    string emptyStr = GetEmptyStr();

                    PriceModel.BestPriceHeight = PriceModel.LastBrokerHeight = PriceModel.AllBrokerHeight = 0;
                    OneBrokerPriceViewModel.Clear(emptyStr);

                    TwoBrokerPriceViewModel.Clear(emptyStr);

                    ThreeBrokerPriceViewModel.Clear(emptyStr);
                    ThreeBrokerPriceViewModel.ShowSettle(CommonConstants.T1_EN);

                    FourBrokerPriceViewModel.Clear(emptyStr);
                    FourBrokerPriceViewModel.ShowSettle(CommonConstants.T2_EN);

                    FiveBrokerPriceViewModel.Clear(emptyStr);
                    FiveBrokerPriceViewModel.ShowSettle(CommonConstants.FWD);
                    SixBrokerPriceViewModel.Clear(emptyStr);

                }

                MarketDataSnapshotFullRefresh refreshSignal = new MarketDataSnapshotFullRefresh();
                refreshSignal.SecurityID = this.PriceModel.Security;

                OnPrice(refreshSignal, false);
            }
        }

        #endregion

        #region 成交单

        public void OnLastExec(ExecutionReport obj)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (_lockObj)
            {
                if (!string.IsNullOrEmpty(_priceModel.Security) && obj.SecurityID == _priceModel.Security)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(_priceModel.Security) && obj.SecurityID == _priceModel.Security)
                        {
                            UpdateLastExec(obj);
                        }
                    }));
                }
                else if (string.IsNullOrEmpty(_priceModel.Security))
                {

                }
            }
        }

        private void UpdateLastExec(ExecutionReport obj)
        {
            //更新最新成交
            LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(obj);


            //LastExecDirection = lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE ? CommonConstants.UP : lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO ? CommonConstants.DOWN : CommonConstants.NO_UP_DOWN;
            decimal bpd = 0;
            bool isFuture = CommonUtils.IsFutureBond(obj.SecurityID);
            if (decimal.TryParse(lastPriceParam.Bp, out bpd))
            {
                _priceModel.LastExecPriceBP = _priceModel.LastExecPriceBP_ToolTip = (isFuture ? string.Empty : (bpd > 0 ? "+" : "-")) + PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString()) + (isFuture ? string.Empty : "bp");

                if (isFuture)
                {
                    if (lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE)
                    {
                        _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("Red-3");
                    }
                    else if (lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO)
                    {
                        _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("Green-2");
                    }
                    else
                    {
                        _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("TextBox.Static.Foreground");
                    }
                }
                else
                {
                    if (bpd > 0)
                    {
                        _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("Green-2");
                    }
                    else
                    {
                        _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("Red-3");
                    }
                }
            }
            else
            {
                if (lastPriceParam.Bp == CommonConstants.NO_PERMISSION)
                {
                    _priceModel.LastExecPriceBP = CommonConstants.NO_PERMISSION;
                    _priceModel.LastExecPriceBP_ToolTip = "无中债估值权限";
                }
                else
                {
                    _priceModel.LastExecPriceBP = _priceModel.LastExecPriceBP_ToolTip = CommonConstants.NO_UP_DOWN;//string.Empty;
                }
                _priceModel.LastExecBpBrush = (Brush)_ownUI.FindResource("TextBox.Static.Foreground");
            }
            //LastExecPriceBP = lastPriceParam.Bp;

            if (isFuture)
            {
                _priceModel.LastExecPrice = CommonUtils.FormatNumFuture(obj.LastPx);
            }
            else
            {
                _priceModel.LastExecPrice = CommonUtils.FormatNum(obj.LastPx); //PriceCalHelper.Instance.RemoveZero(obj.LastPx);//CommonUtils.formatString(obj.LastPx, 4);
            }

            Brush showlastExecPriceSideBrush = null;
            if (obj.Side == CommonConstants.SIDE_OFR_TWO || obj.Side == CommonConstants.SIDE_X)
            {
                _priceModel.LastExecSideShow = CommonConstants.TKN;
                showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("ColorBrush.Blue_firm");

            }
            else if (obj.Side == CommonConstants.SIDE_BID_ONE || obj.Side == CommonConstants.SIDE_Y)
            {
                showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("Orange-1");

                _priceModel.LastExecSideShow = CommonConstants.GVN;
            }
            else if (obj.Side == CommonConstants.SIDE_TRD_THREE || obj.Side == CommonConstants.SIDE_Z)
            {
                showlastExecPriceSideBrush = (Brush)_ownUI.FindResource("RFQ.ColorBrush");

                _priceModel.LastExecSideShow = CommonConstants.TRD;
            }

            _priceModel.LastExecSource = ComboSourceUtil.Instance.FindEnum(lastPriceParam.Source, "venues");

            _priceModel.LastExecTime = obj.TransactTime;

            decimal bp = 0;


            Brush showlastExecPriceBrush = null;

            if (lastPriceParam.UpDown == CommonConstants.SIDE_OFR_TWO)
            {
                showlastExecPriceBrush = (Brush)_ownUI.FindResource("Red-3");
            }
            else if (lastPriceParam.UpDown == CommonConstants.SIDE_BID_ONE)
            {
                showlastExecPriceBrush = (Brush)_ownUI.FindResource("Green-2");
            }
            else if (bp == 0)
            {
                showlastExecPriceBrush = (Brush)_ownUI.FindResource("TextBox.Static.Foreground");
            }

            _priceModel.LastExecSideBrush = showlastExecPriceSideBrush;

        }

        #endregion

        #region 我的订单
        public void OnOrder(ExecutionReport obj)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    lock (this._lockObj)
            //    {
            //        CalOrder(obj);
            //    }
            //}));
        }

        private void CalOrder(ExecutionReport obj)
        {
            // 我的订单
            string filterStr = ConditionHelper.GetCardOrderCondition(BrokerIndex, SettleFilterDic);
            DataRow bidOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(obj.SecurityID, CommonConstants.SIDE_BID_ONE, "0", filterStr);//this._rowFilterStr);//item[SETTLE_TYPE_FIELD] as string);
            if (bidOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestBidPrice = CommonUtils.ConvertToDecimal(_priceModel.BestBid);
                string myLstBidPrice = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myBidOrderPrice = CommonUtils.ConvertToDecimal(myLstBidPrice);
                string isPoint = CommonUtils.ConvertToString(bidOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                {
                    if (CommonUtils.IsNumeric(_priceModel.BestBid) && CommonUtils.IsNumeric(myLstBidPrice))
                    {
                        _priceModel.BidArrowVisibilly = Visibility.Visible;
                        if (bestBidPrice == myBidOrderPrice)
                        {
                            _priceModel.BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground-30%") as Brush;
                            _priceModel.BidArrowStroke = _ownUI.FindResource("Bid.textblock.foreground") as Brush;

                            _priceModel.BidArrowToolTxt = "本人有报价，且为市场最优";
                        }
                        else
                        {
                            _priceModel.BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                            _priceModel.BidArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;

                            _priceModel.BidArrowToolTxt = "本人有报价";
                        }

                    }
                }
                else
                {
                    _priceModel.BidArrowVisibilly = Visibility.Collapsed;
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
                        _priceModel.BidArrowVisibilly = Visibility.Visible;
                    else
                        _priceModel.BidArrowVisibilly = Visibility.Collapsed;


                    _priceModel.BidArrowFill = _ownUI.FindResource("Bid.textblock.foreground") as Brush;
                    _priceModel.BidArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;
                }
                else
                {
                    _priceModel.BidArrowVisibilly = Visibility.Collapsed;
                }

            }

            DataRow ofrOrder = MyOrderProcesser.Processer.GetMyBestOrderBySecurityId(obj.SecurityID, CommonConstants.SIDE_OFR_TWO, "0", filterStr);//this._rowFilterStr);
            if (ofrOrder != null)
            {
                // 最优价格 我的订单 比较， 是否相同
                decimal bestOfrPrice = CommonUtils.ConvertToDecimal(_priceModel.BestOfr);
                string myLstOfrPrice = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.PRICE_ORDER_FIELD]);
                decimal myOfrOrderPrice = CommonUtils.ConvertToDecimal(myLstOfrPrice);
                string isPoint = CommonUtils.ConvertToString(ofrOrder[MyOrderProcesser.ISPOINT_FILED].ToString());

                if (string.IsNullOrEmpty(isPoint) || isPoint == "0")
                {
                    if (CommonUtils.IsNumeric(_priceModel.BestOfr) && CommonUtils.IsNumeric(myLstOfrPrice))
                    {
                        _priceModel.OfrArrowVisibilly = Visibility.Visible;
                        if (bestOfrPrice == myOfrOrderPrice)
                        {
                            _priceModel.OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush-30%") as Brush;
                            _priceModel.OfrArrowStroke = _ownUI.FindResource("ColorBrush.Blue_firm") as Brush;

                            _priceModel.OfrArrowToolTxt = "本人有报价，且为市场最优";
                        }
                        else
                        {
                            _priceModel.OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush") as Brush;
                            _priceModel.OfrArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;

                            _priceModel.OfrArrowToolTxt = "本人有报价";
                        }
                    }
                }
                else
                {
                    _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
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
                        _priceModel.OfrArrowVisibilly = Visibility.Visible;
                    else
                        _priceModel.OfrArrowVisibilly = Visibility.Collapsed;


                    _priceModel.OfrArrowFill = _ownUI.FindResource("Ofr.Selected.BorderBrush") as Brush;
                    _priceModel.OfrArrowStroke = _ownUI.FindResource("NormalBrush") as Brush;
                }
                else
                {
                    _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                }
            }
        }

        #endregion


        #region OHLC

        public void onOHLC(OhlcMsg ohlcMsg)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (this._ohlcLockObj)
            {
                if (!string.IsNullOrEmpty(ohlcMsg.securityid) && ohlcMsg.securityid.ToLower() == PriceModel.Security.ToLower())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!string.IsNullOrEmpty(ohlcMsg.securityid) && ohlcMsg.securityid.ToLower() == PriceModel.Security.ToLower())
                        {
                            string closePrice = string.IsNullOrEmpty(ohlcMsg.inf2) ? CommonConstants.NO_PRICE : ohlcMsg.inf2;//昨

                            if (CommonUtils.IsNumeric(closePrice))
                            {
                                PriceModel.PreClose = CommonUtils.FormatNum(closePrice.ConvertToDecimal().ToString("0.00##"));// CommonUtils.RemoveZero(closePrice);
                            }
                            else
                            {
                                PriceModel.PreClose = closePrice;
                            }

                            if (ohlcMsg.open != 0)
                                PriceModel.OpenPrice = CommonUtils.FormatNum(ohlcMsg.open.ToString("0.00##"));//ohlcMsg.open.ToString();
                            else
                                PriceModel.OpenPrice = CommonConstants.NO_PRICE;

                            if (ohlcMsg.high != 0)
                                PriceModel.High = CommonUtils.FormatNum(ohlcMsg.high.ToString("0.00##"));//ohlcMsg.high.ToString();
                            else
                            {
                                PriceModel.High = CommonConstants.NO_PRICE;
                            }

                            if (ohlcMsg.low != 0)
                                PriceModel.Low = CommonUtils.FormatNum(ohlcMsg.low.ToString("0.00##"));
                            else
                            {
                                PriceModel.Low = CommonConstants.NO_PRICE;
                            }
                        }
                    }));
                }
            }
        }
        

        #endregion

        #region 报价
        public void OnPrice(MarketDataSnapshotFullRefresh data, bool needRefreshColor)
        {
            //CommonUtils._ChangeTab_ManualResetEvent.WaitOne();
            lock (this._lockObj)
            {
                try
                {
                    string securityId = data.SecurityID;
                    if (data.ContainsKey("isCache"))
                    {
                        CalBestPrice(securityId, false);
                    }
                    else
                    {
                        CalBestPrice(securityId, needRefreshColor);
                    }
                    List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(data.Venues, securityId);
                    if (marketDataList.Count > 0)
                    {
                        UpdatePrice2(data.Venues);

                        //ExecutionReport obj = new ExecutionReport();
                        //obj.SecurityID = securityId;
                        //OnOrder(obj);
                    }

                    UpdateFuture(data);

                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private void UpdateFuture(MarketDataSnapshotFullRefresh data)
        {
            bool isFutureInner = CommonUtils.IsFutureBond(data.SecurityID);
            if (isFutureInner)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this._lockObj)
                    {
                        List<MarketDataSnapshotFullRefresh> marketDataList = STC.Common.Utils.PriceHandler.Instance.GetAllMarketData(data.SecurityID);
                        if (marketDataList.Count > 0)
                        {
                            data = marketDataList[0];
                        }

                        //_priceModel.PreSettlePrice = CommonUtils.formatString(data.PreSettlePrice, 3);
                        //_priceModel.OpenPrice = CommonUtils.formatString(data.OpenPrice, 3);
                        //_priceModel.OpenInterest = CommonUtils.formatString(data.OpenInterest, 0);
                        //_priceModel.Turnover = CommonUtils.formatQtyToBillion(!string.IsNullOrEmpty(data.Turnover) ? data.Turnover : string.Empty);
                        //_priceModel.PreOpenInterest = CommonUtils.formatString(data.PreOpenInterest, 0);


                        double number = 0;
                        if (double.TryParse(data.PreClosePrice, out number))
                            PriceModel.PreClose = CommonUtils.FormatNum(number.ToString("0.00##"));// string.Format("{0:0.000}", number);
                        else
                        {
                            PriceModel.PreClose = CommonConstants.NO_PRICE;
                        }

                        if (double.TryParse(data.HighPrice, out number))
                            PriceModel.High = CommonUtils.FormatNum(number.ToString("0.00##"));//string.Format("{0:0.000}", number);
                        else
                        {
                            PriceModel.High = CommonConstants.NO_PRICE;
                        }

                        if (double.TryParse(data.LowPrice, out number))
                            PriceModel.Low = CommonUtils.FormatNum(number.ToString("0.00##"));//string.Format("{0:0.000}", number);
                        else
                        {
                            PriceModel.Low = CommonConstants.NO_PRICE;
                        }

                        if (double.TryParse(data.OpenPrice, out number))
                            PriceModel.OpenPrice = CommonUtils.FormatNum(number.ToString("0.00##"));//string.Format("{0:0.000}", number);
                        else
                        {
                            PriceModel.OpenPrice = CommonConstants.NO_PRICE;
                        }

                    }
                }));
            }
        }

        private bool FilterData(GNoMDEntries mdEntrys, string broker)
        {
            bool result = true;

            if (BrokerIndex > 1)
            {
                string selectBroker = PermisionHandler.Instance.GetBrokerByIndex(BrokerIndex - 1);
                result = selectBroker == broker;
            }

            if (result)
            {
                if (SettleFilterDic != null && SettleFilterDic.ContainsKey(CommonConstants.SETTLE))
                {
                    result = ConditionHelper.CheckSettleCondition(SettleFilterDic, mdEntrys.SettlType);
                }
            }

            return result;
        }

        private bool NeedFilterVenus(string venus)
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

        private void UpdatePrice2(string updateVenue)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this._lockObj)
                {
                    try
                    {
                        List<MarketDataSnapshotFullRefresh> marketDataList = PriceHandler.Instance.GetAllMarketData(updateVenue, _priceModel.Security);
                        if (marketDataList.Count > 0)
                        {
                            List<GNoMDEntries> bidNoMDEntryList = new List<GNoMDEntries>();
                            List<GNoMDEntries> ofrNoMDEntryList = new List<GNoMDEntries>();
                            List<string> bidVenusList = new List<string>();
                            List<string> ofrVenusList = new List<string>();

                            string cdcValue = string.Empty;
                            string cdcValue2 = string.Empty;
                            string oricdcValue = string.Empty;
                            string sciValue = string.Empty;
                            foreach (MarketDataSnapshotFullRefresh mds in marketDataList)
                            {
                                UpdateTime = mds.SendingTime;

                                oricdcValue = mds.CCDCValuation;

                                if (JsonSplit.IsJson(mds.Info1))
                                {
                                    JObject jsonObj = JObject.Parse(mds.Info1);
                                    if (jsonObj.ContainsKey("csiBondValuationFull"))
                                    {
                                        sciValue = jsonObj["csiBondValuationFull"].ToString();
                                        int index = sciValue.IndexOf("|");
                                        if (index > 0)
                                        {
                                            sciValue = sciValue.Substring(index);
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(PriceModel.CCDCValuation) || PriceModel.CCDCValuation == CommonConstants.NO_PRICE)
                                {
                                    cdcValue = mds.CCDCValuation;
                                    string tmpccdc = CommonUtils.formatPriceString(CommonUtils.ConvertToString(mds.CCDCValuation), 4);
                                    string tmpccdc2 = CommonUtils.formatPriceString(CommonUtils.ConvertToString(mds.CCDCValuation2), 4);

                                    cdcValue = string.IsNullOrEmpty(tmpccdc) ? "--" : tmpccdc;
                                    cdcValue2 = string.IsNullOrEmpty(tmpccdc2) ? "--" : tmpccdc2.Replace("|", "/");
                                    if (cdcValue == cdcValue2)
                                    {
                                        cdcValue2 = "--";
                                    }
                                    if (cdcValue != cdcValue2 && !string.IsNullOrEmpty(cdcValue) && !string.IsNullOrEmpty(cdcValue2) && cdcValue2 != "--")
                                    {
                                        cdcValue = "--";
                                        cdcValue2 = $"{cdcValue2}";
                                    }
                                    if (cdcValue == "--")
                                    {
                                        cdcValue = cdcValue2;
                                    }

                                    PriceModel.CCDCValuation = cdcValue;
                                }

                                List<GNoMDEntries> temList = mds.NoMDEntries.Where(delegate (GNoMDEntries noMDEntry)
                                {
                                    if (noMDEntry.MDEntryType == CommonConstants.SIDE_BID && FilterData(noMDEntry, noMDEntry.Venues))
                                    {
                                        if (!string.IsNullOrEmpty(mds.Venues))
                                            noMDEntry.Venues = mds.Venues;

                                        return true;
                                    }
                                    else
                                        return false;
                                }).ToList();

                                bidNoMDEntryList.AddRange(temList);

                                if (temList.Count > 0)// && NeedFilterVenus(mds.Venues))
                                {
                                    foreach (GNoMDEntries entry in temList)
                                    {
                                        if (!bidVenusList.Contains(entry.Venues))
                                            bidVenusList.Add(entry.Venues);
                                    }

                                }

                                temList.Clear();
                                temList = mds.NoMDEntries.Where(delegate (GNoMDEntries noMDEntry)
                                {
                                    if (noMDEntry.MDEntryType == CommonConstants.SIDE_OFR && FilterData(noMDEntry, noMDEntry.Venues))
                                    {
                                        if (!string.IsNullOrEmpty(mds.Venues))
                                            noMDEntry.Venues = mds.Venues;

                                        return true;
                                    }
                                    else
                                        return false;
                                }).ToList();

                                ofrNoMDEntryList.AddRange(temList);
                                if (temList.Count > 0)// && NeedFilterVenus(mds.Venues))
                                {
                                    foreach (GNoMDEntries entry in temList)
                                    {
                                        if (!ofrVenusList.Contains(entry.Venues))
                                            ofrVenusList.Add(entry.Venues);
                                    }
                                }
                            }
                            string securityID = marketDataList[0].SecurityID;
                            string venues = marketDataList[0].Venues;

                            if (BrokerIndex == 0)//全部
                            {
                                UpdateAllBrokerPrice(CommonConstants.SIDE_BID, bidVenusList, bidNoMDEntryList);
                                UpdateAllBrokerPrice(CommonConstants.SIDE_OFR, ofrVenusList, ofrNoMDEntryList);
                            }
                            else if (BrokerIndex == 1)//最优
                            {
                                RefreshBest();
                            }
                            else
                            {
                                string broker = PermisionHandler.Instance.GetBrokerByIndex(BrokerIndex - 1);

                                RefreshBrokerData(broker, bidNoMDEntryList, ofrNoMDEntryList);
                            }

                            if (CommonUtils.HasCDCAuth(securityID))
                            {
                                PriceModel.CDCValue = string.IsNullOrEmpty(oricdcValue) ? CommonConstants.NO_PRICE : CommonUtils.FormatNum(oricdcValue.ConvertToDecimal().ToString("0.00##"));
                                PriceModel.CsiValue = string.IsNullOrEmpty(sciValue) ? CommonConstants.NO_PRICE : CommonUtils.FormatNum(sciValue.ConvertToDecimal().ToString("0.00##"));
                            }
                            else
                            {
                                PriceModel.CDCValue = CommonConstants.NO_PERMISSION;
                                PriceModel.CsiValue = CommonConstants.NO_PERMISSION;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("UpdatePrice2 exception:", ex);
                    }
                }
            }));
        }

        #region 更新最优panel

        private void RefreshBest()
        {
            RefreshCornerMarket(ThreeBrokerPriceViewModel);
            RefreshCornerMarket(FourBrokerPriceViewModel);
            RefreshCornerMarket(FiveBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, ThreeBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, ThreeBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, FourBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, FourBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, FiveBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, FiveBrokerPriceViewModel);

            _priceModel.BidBrokerBargainFlag = string.Empty;
            _priceModel.OfrBrokerBargainFlag = string.Empty;

            RefreshBestDataBySide(CommonConstants.SIDE_BID_ONE);
            RefreshBestDataBySide(CommonConstants.SIDE_OFR_TWO);
        }

        private void RefreshBestDataBySide(string side)
        {
            Dictionary<string, IList> dic = MainConditionCache.Instance.GetConditionById(_groupId) as Dictionary<string, IList>;
            string filterStr = ConditionHelper.GetNewPriceViewCondition(_settleFilterDic, dic);

            bool isAll = ConditionHelper.GetSettleType(_settleFilterDic, CommonConstants.ALL_PARTY);
            bool isT1 = ConditionHelper.GetSettleType(_settleFilterDic, CommonConstants.T1);
            bool isT2 = ConditionHelper.GetSettleType(_settleFilterDic, CommonConstants.T2);
            bool isFwd = ConditionHelper.GetSettleType(_settleFilterDic, CommonConstants.FWD);

            BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(PriceModel.Security, filterStr, string.Empty);



            BilateralBestPrice bestPriceT1 = null;
            Dictionary<string, IList> settleDic = null;

            BilateralBestPrice bestPriceT2 = null;
            BilateralBestPrice bestPriceFWD = null;

            if (isAll || isT1)
            {
                settleDic = ConditionHelper.GetDicBySettle(CommonConstants.T1);
                filterStr = ConditionHelper.GetNewPriceViewCondition(settleDic, dic);
                bestPriceT1 = PriceHandler.Instance.CalBestPrice(PriceModel.Security, filterStr, string.Empty);
            }
            
            if (isAll || isT2)
            {
                settleDic = ConditionHelper.GetDicBySettle(CommonConstants.T2);
                filterStr = ConditionHelper.GetNewPriceViewCondition(settleDic, dic);
                bestPriceT2 = PriceHandler.Instance.CalBestPrice(PriceModel.Security, filterStr, string.Empty);
            }

            if (isAll || isFwd)
            {
                settleDic = ConditionHelper.GetDicBySettle(CommonConstants.FWD);
                filterStr = ConditionHelper.GetNewPriceViewCondition(settleDic, dic);
                bestPriceFWD = PriceHandler.Instance.CalBestPrice(PriceModel.Security, filterStr, string.Empty);
            }


            if (side == CommonConstants.SIDE_BID_ONE)
            {
                if (bestPrice.BidBest != null)
                {
                    _priceModel.BidPrice = CommonUtils.FormatNum(bestPrice.BidBest.bestPx);
                    _priceModel.BidTxt = bestPrice.BidBest.text;
                    _priceModel.BidBrokerBargainFlag = bestPrice.BidBest.bargain;
                }
                else
                {
                    string emptyStr = GetEmptyStr();
                    _priceModel.BidPrice = emptyStr;
                    _priceModel.BidTxt = emptyStr;
                    _priceModel.BidBrokerBargainFlag = string.Empty;
                }
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                if (bestPrice.OfrBest != null)
                {
                    _priceModel.OfrPrice = CommonUtils.FormatNum(bestPrice.OfrBest.bestPx);
                    _priceModel.OfrTxt = bestPrice.OfrBest.text;
                    _priceModel.OfrBrokerBargainFlag = bestPrice.OfrBest.bargain;
                }
                else
                {
                    string emptyStr = GetEmptyStr();
                    _priceModel.OfrPrice = emptyStr;
                    _priceModel.OfrTxt = emptyStr;
                    _priceModel.OfrBrokerBargainFlag = string.Empty;
                }
            }

            this.UpdateBestByBroker(ThreeBrokerPriceViewModel, bestPriceT1, side);
            this.UpdateBestByBroker(FourBrokerPriceViewModel, bestPriceT2, side);
            this.UpdateBestByBroker(FiveBrokerPriceViewModel, bestPriceFWD, side);

        }

        private void UpdateBestByBroker(BrokerPriceViewModel brokerPriceViewModel, BilateralBestPrice bestPrice, string side)
        {
            if (side == CommonConstants.SIDE_BID_ONE)
            {
                if (bestPrice != null && bestPrice.BidBest != null)
                {
                    brokerPriceViewModel.Bid = CommonUtils.FormatNum(bestPrice.BidBest.bestPx);
                    brokerPriceViewModel.BidAmount = bestPrice.BidBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.BidBest.bestQty.ToString();
                    brokerPriceViewModel.BidBargainFlag = bestPrice.BidBest.bargain;
                    brokerPriceViewModel.BidCornerMarketText = bestPrice.BidBest.text;
                    brokerPriceViewModel.BidCornerMarket = !string.IsNullOrEmpty(bestPrice.BidBest.text) ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.BidPCornerMarket = IsQtyStar(bestPrice.BidBest.text) ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.BidStarVisibility = bestPrice.BidBest.text.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.BidPStarVisibility = bestPrice.BidBest.text.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                    if (brokerPriceViewModel.BidStarVisibility == Visibility.Visible || brokerPriceViewModel.BidPStarVisibility == Visibility.Visible)
                    {
                        brokerPriceViewModel.BidLineVisibility = Visibility.Visible;
                    }
                    else
                    {
                        brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    string emptyStr = GetEmptyStr();
                    brokerPriceViewModel.Bid = emptyStr;
                    brokerPriceViewModel.BidAmount = emptyStr;
                    brokerPriceViewModel.BidBargainFlag = string.Empty;
                    brokerPriceViewModel.BidCornerMarketText = string.Empty;
                    brokerPriceViewModel.BidCornerMarket = Visibility.Collapsed;
                    brokerPriceViewModel.BidPCornerMarket = Visibility.Collapsed;
                    brokerPriceViewModel.BidStarVisibility = Visibility.Collapsed;
                    brokerPriceViewModel.BidPStarVisibility = Visibility.Collapsed;
                    brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;
                }
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                if (bestPrice != null && bestPrice.OfrBest != null)
                {
                    brokerPriceViewModel.Ofr = CommonUtils.FormatNum(bestPrice.OfrBest.bestPx);
                    brokerPriceViewModel.OfrAmount = bestPrice.OfrBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.OfrBest.bestQty.ToString();
                    brokerPriceViewModel.OfrBargainFlag = bestPrice.OfrBest.bargain;
                    brokerPriceViewModel.OfrCornerMarketText = bestPrice.OfrBest.text;
                    brokerPriceViewModel.OfrCornerMarket = !string.IsNullOrEmpty(bestPrice.OfrBest.text) ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.OfrPCornerMarket = IsQtyStar(bestPrice.OfrBest.text) ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.OfrStarVisibility = bestPrice.OfrBest.text.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                    brokerPriceViewModel.OfrPStarVisibility = bestPrice.OfrBest.text.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                    if (brokerPriceViewModel.OfrStarVisibility == Visibility.Visible || brokerPriceViewModel.OfrPStarVisibility == Visibility.Visible)
                    {
                        brokerPriceViewModel.OfrLineVisibility = Visibility.Visible;
                    }
                    else
                    {
                        brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    string emptyStr = GetEmptyStr();
                    brokerPriceViewModel.OfrAmount = emptyStr;
                    brokerPriceViewModel.OfrBargainFlag = string.Empty;
                    brokerPriceViewModel.OfrCornerMarketText = string.Empty;
                    brokerPriceViewModel.OfrCornerMarket = Visibility.Collapsed;
                    brokerPriceViewModel.OfrPCornerMarket = Visibility.Collapsed;
                    brokerPriceViewModel.OfrStarVisibility = Visibility.Collapsed;
                    brokerPriceViewModel.OfrPStarVisibility = Visibility.Collapsed;
                    brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
                }
            }
        }

        #endregion


        #region 更新指定Broker

        private void RefreshCornerMarket(BrokerPriceViewModel brokerPriceViewModel)
        {
            brokerPriceViewModel.BidCornerMarket = Visibility.Collapsed;
            brokerPriceViewModel.BidCornerMarketText = string.Empty;
            brokerPriceViewModel.BidPCornerMarket = Visibility.Collapsed;
            brokerPriceViewModel.BidStarVisibility = Visibility.Collapsed;
            brokerPriceViewModel.BidPStarVisibility = Visibility.Collapsed;
            brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;

            brokerPriceViewModel.OfrCornerMarket = Visibility.Collapsed;
            brokerPriceViewModel.OfrCornerMarketText = string.Empty;
            brokerPriceViewModel.OfrPCornerMarket = Visibility.Collapsed;
            brokerPriceViewModel.OfrStarVisibility = Visibility.Collapsed;
            brokerPriceViewModel.OfrPStarVisibility = Visibility.Collapsed;
            brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
        }

        private void RefreshBrokerData(string broker, List<GNoMDEntries> bidMDEntryList, List<GNoMDEntries> ofrMDEntryList)
        {
            RefreshCornerMarket(ThreeBrokerPriceViewModel);
            RefreshCornerMarket(FourBrokerPriceViewModel);
            RefreshCornerMarket(FiveBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, ThreeBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, ThreeBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, FourBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, FourBrokerPriceViewModel);

            UpdateBrokerPrice(CommonConstants.SIDE_BID, FiveBrokerPriceViewModel);
            UpdateBrokerPrice(CommonConstants.SIDE_OFR, FiveBrokerPriceViewModel);

            _priceModel.BidBrokerBargainFlag = string.Empty;
            _priceModel.OfrBrokerBargainFlag = string.Empty;

            if (PermisionHandler.Instance.CheckBrokerPermissionByEN(broker))
            {
                RefreshDataBySide(CommonConstants.SIDE_BID_ONE, broker, bidMDEntryList);
                RefreshDataBySide(CommonConstants.SIDE_OFR_TWO, broker, ofrMDEntryList);
            }
            else
            {
                ThreeBrokerPriceViewModel.Bid = ThreeBrokerPriceViewModel.Ofr = ThreeBrokerPriceViewModel.BidAmount = ThreeBrokerPriceViewModel.OfrAmount = CommonConstants.NO_BROKER;
                FourBrokerPriceViewModel.Bid = FourBrokerPriceViewModel.Ofr = FourBrokerPriceViewModel.BidAmount = FourBrokerPriceViewModel.OfrAmount = CommonConstants.NO_BROKER;
                FiveBrokerPriceViewModel.Bid = FiveBrokerPriceViewModel.Ofr = FiveBrokerPriceViewModel.BidAmount = FiveBrokerPriceViewModel.OfrAmount = CommonConstants.NO_BROKER;

                _priceModel.BidPrice = _priceModel.OfrPrice = _priceModel.BidTxt = _priceModel.OfrTxt = CommonConstants.NO_BROKER;
            }
        }


        private void RefreshDataBySide(string side, string broker, List<GNoMDEntries> noMDEntryList)
        {
            StringBuilder sb = new StringBuilder();

            decimal fwdQty = decimal.Zero;
            decimal spotQty = decimal.Zero;
            decimal addOneQty = decimal.Zero;
            string price = string.Empty;
            StringBuilder sbFwd = new StringBuilder();

            List<string> barZero = new List<string>();
            List<string> barOne = new List<string>();
            List<string> barFwd = new List<string>();

            List<string> allBar = new List<string>();

            bool isPermission = PermisionHandler.Instance.CheckTradePermission(broker);

            bool updateAll = ConditionHelper.ContainSettle(_settleFilterDic, CommonConstants.ALL_PARTY);
            bool updateZero = updateAll || ConditionHelper.ContainSettle(_settleFilterDic, CommonConstants.T1_EN);
            bool updateOne = updateAll || ConditionHelper.ContainSettle(_settleFilterDic, CommonConstants.T2_EN);
            bool updateFwd = updateAll || ConditionHelper.ContainSettle(_settleFilterDic, CommonConstants.FWD);

            foreach (GNoMDEntries originMdEntry in noMDEntryList)
            {
                if (sb.Length > 0)
                    sb.Append("+");

                if (!string.IsNullOrEmpty(originMdEntry.Text))
                    sb.Append(string.Format("{0}", originMdEntry.Text));//string.Format("{0}({1})", mdEntry.MDEntrySize, mdEntry.Text));
                else
                    sb.Append(originMdEntry.MDEntrySize);

                price = CommonUtils.FormatNum(CommonUtils.GetShowPrice(originMdEntry));//originMdEntry.MDEntryPx;

                string[] settleArr = originMdEntry.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string settle in settleArr)
                {
                    GNoMDEntries mdEntry = CommonUtils.DeserializeObject<GNoMDEntries>(CommonUtils.SerializeObject(originMdEntry));
                    mdEntry.SettlType = settle;


                    decimal d = 0;

                    BrokerPriceViewModel brokerPriceViewModel = null;

                    allBar.Add(mdEntry.BargainFlag);

                    if (mdEntry.SettlType == CommonConstants.T1_ONE && updateZero)
                    {
                        brokerPriceViewModel = ThreeBrokerPriceViewModel;
                        brokerPriceViewModel.SettleType = CommonConstants.T1_ONE;

                        if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                        {
                            addOneQty += d;
                        }

                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            brokerPriceViewModel.Bid = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                            //decimal total = CalBrokerQty(brokerPriceViewModel.BidAmount, mdEntry);
                            //brokerPriceViewModel.BidAmount = total == 0 ? CommonConstants.NO_PRICE : total.ToString();
                            //string.IsNullOrEmpty(mdEntry.MDEntrySize) ? CommonConstants.NO_PRICE : mdEntry.MDEntrySize;
                            //brokerPriceViewModel.BidBargainFlag = CalBar(brokerPriceViewModel.BidBargainFlag, mdEntry);//mdEntry.BargainFlag;

                            if (isPermission && brokerPriceViewModel.Bid != CommonConstants.NO_PRICE && brokerPriceViewModel.BidAmount != CommonConstants.NO_PRICE)
                            {
                                brokerPriceViewModel.BidBgBrush = (Brush)_ownUI.FindResource("Bid.Selected.BorderBrush-30%");
                                brokerPriceViewModel.IsBidPermission = true;
                            }

                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            brokerPriceViewModel.Ofr = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                            //decimal total = CalBrokerQty(brokerPriceViewModel.OfrAmount, mdEntry);
                            //brokerPriceViewModel.OfrAmount = total == 0 ? CommonConstants.NO_PRICE : total.ToString();
                            //brokerPriceViewModel.OfrBargainFlag = CalBar(brokerPriceViewModel.BidBargainFlag, mdEntry);//mdEntry.BargainFlag;

                            if (isPermission && brokerPriceViewModel.Ofr != CommonConstants.NO_PRICE && brokerPriceViewModel.OfrAmount != CommonConstants.NO_PRICE)
                            {
                                brokerPriceViewModel.BidBgBrush = (Brush)_ownUI.FindResource("Ofr.Selected.BorderBrush-30%");
                                brokerPriceViewModel.IsBidPermission = true;
                            }
                        }
                        barZero.Add(mdEntry.BargainFlag);
                    }
                    else if (CommonConstants.T2_T3.Contains(mdEntry.SettlType) && updateOne)
                    {
                        brokerPriceViewModel = FourBrokerPriceViewModel;

                        if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                            spotQty += d;

                        brokerPriceViewModel.SettleType = CommonConstants.T2_T3;
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            brokerPriceViewModel.Bid = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            brokerPriceViewModel.Ofr = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                        }

                        barOne.Add(mdEntry.BargainFlag);

                    }
                    else if (PriceCalHelper.Instance.IsFwd(mdEntry.SettlType) && updateFwd)
                    {
                        brokerPriceViewModel = FiveBrokerPriceViewModel;

                        if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                            fwdQty += d;

                        sbFwd.Append(string.Format("{0}:", mdEntry.SettlType));
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            brokerPriceViewModel.Bid = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            brokerPriceViewModel.Ofr = CommonUtils.FormatNum(CommonUtils.GetShowPrice(mdEntry));//string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;

                        }

                        barFwd.Add(mdEntry.BargainFlag);

                    }

                    if (brokerPriceViewModel != null)
                    {
                        long amount = 0;
                        long.TryParse(mdEntry.MDEntrySize, out amount);
                        //更新角标
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            string result = CalCornerMarket(brokerPriceViewModel.BidCornerMarketText, amount, mdEntry.Text);
                            brokerPriceViewModel.BidCornerMarketText = result;
                            brokerPriceViewModel.BidCornerMarket = !string.IsNullOrEmpty(result) ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.BidPCornerMarket = IsQtyStar(result) ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.BidStarVisibility = result.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.BidPStarVisibility = result.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                            if (brokerPriceViewModel.BidStarVisibility == Visibility.Visible || brokerPriceViewModel.BidPStarVisibility == Visibility.Visible)
                            {
                                brokerPriceViewModel.BidLineVisibility = Visibility.Visible;
                            }
                            else
                            {
                                brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;
                            }
                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            string result = CalCornerMarket(brokerPriceViewModel.OfrCornerMarketText, amount, mdEntry.Text);
                            brokerPriceViewModel.OfrCornerMarketText = result;
                            brokerPriceViewModel.OfrCornerMarket = !string.IsNullOrEmpty(result) ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.OfrPCornerMarket = IsQtyStar(result) ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.OfrStarVisibility = result.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                            brokerPriceViewModel.OfrPStarVisibility = result.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                            if (brokerPriceViewModel.OfrStarVisibility == Visibility.Visible || brokerPriceViewModel.OfrPStarVisibility == Visibility.Visible)
                            {
                                brokerPriceViewModel.OfrLineVisibility = Visibility.Visible;
                            }
                            else
                            {
                                brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
                            }
                        }
                    }
                }

            }

            BrokerPriceViewModel temBrokerPriceModel = ThreeBrokerPriceViewModel;

            //更新+0
            if (addOneQty > 0)
            {
                UpdateAmountAndStar(temBrokerPriceModel, side, addOneQty.ToString(), CalBar(barZero));
            }
            else
            {
                string emptyStr = GetEmptyStr();
                UpdateAmountAndStar(temBrokerPriceModel, side, emptyStr, CalBar(barZero));
            }

            temBrokerPriceModel = FourBrokerPriceViewModel;
            //更新+1
            if (spotQty > 0)
            {
                UpdateAmountAndStar(temBrokerPriceModel, side, spotQty.ToString(), CalBar(barOne));

            }
            else
            {
                string emptyStr = GetEmptyStr();
                UpdateAmountAndStar(temBrokerPriceModel, side, emptyStr, CalBar(barOne));
            }

            UpdateBg(FourBrokerPriceViewModel, side, isPermission);

            //更新Bid Ofr合计的bargain
            if (side == CommonConstants.SIDE_BID_ONE)
            {
                _priceModel.BidBrokerBargainFlag = CalBar(allBar);
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                _priceModel.OfrBrokerBargainFlag = CalBar(allBar);
            }

            temBrokerPriceModel = FiveBrokerPriceViewModel;
            //更新远期
            if (fwdQty > 0)
            {

                UpdateAmountAndStar(temBrokerPriceModel, side, fwdQty.ToString(), CalBar(barFwd));

                string fwdSettle = sbFwd.ToString();
                temBrokerPriceModel.SettleType = fwdSettle.Length > 0 ? fwdSettle.Substring(0, fwdSettle.Length - 1) : string.Empty;

            }
            else
            {
                UpdateAmountAndStar(temBrokerPriceModel, side, CommonConstants.NO_PRICE, CalBar(barFwd));

                temBrokerPriceModel.SettleType = string.Empty;
            }

            UpdateBg(FiveBrokerPriceViewModel, side, isPermission);
            
            if (side == CommonConstants.SIDE_BID_ONE)
            {
                _priceModel.BidTxt = sb.ToString();
                _priceModel.BidPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                _priceModel.OfrTxt = sb.ToString();
                _priceModel.OfrPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
        }

        private void UpdateBg(BrokerPriceViewModel temBrokerPriceModel, string side, bool isPermission)
        {
            if (side == CommonConstants.SIDE_BID_ONE)
            {
                if (isPermission && temBrokerPriceModel.Bid != CommonConstants.NO_PRICE && temBrokerPriceModel.BidAmount != CommonConstants.NO_PRICE)
                {
                    temBrokerPriceModel.BidBgBrush = (Brush)_ownUI.FindResource("Bid.Selected.BorderBrush-30%");
                    temBrokerPriceModel.IsBidPermission = true;
                }
                else
                {
                    temBrokerPriceModel.BidBgBrush = new SolidColorBrush(Colors.Transparent);
                    temBrokerPriceModel.IsBidPermission = false;
                }
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                if (isPermission && temBrokerPriceModel.Ofr != CommonConstants.NO_PRICE && temBrokerPriceModel.OfrAmount != CommonConstants.NO_PRICE)
                {
                    temBrokerPriceModel.OfrBgBrush = (Brush)_ownUI.FindResource("Ofr.Selected.BorderBrush-30%");
                    temBrokerPriceModel.IsOfrPermission = true;
                }
                else
                {
                    temBrokerPriceModel.OfrBgBrush = new SolidColorBrush(Colors.Transparent);
                    temBrokerPriceModel.IsOfrPermission = false;
                }
            }
        }

        private void UpdateAmountAndStar(BrokerPriceViewModel brokerPriceViewModel, string side, string qty, string bargain)
        {
            if (brokerPriceViewModel != null)
            {
                if (side == CommonConstants.SIDE_BID_ONE)
                {
                    brokerPriceViewModel.BidAmount = qty;
                    brokerPriceViewModel.BidBargainFlag = bargain;

                }
                else if (side == CommonConstants.SIDE_OFR_TWO)
                {
                    brokerPriceViewModel.OfrAmount = qty;
                    brokerPriceViewModel.OfrBargainFlag = bargain;
                }
            }
        }

        #endregion

        private BrokerPriceViewModel GetBrokerPriceViewModel(string venues)
        {
            int index = PermisionHandler.Instance.GetBrokerIndex(venues);
            BrokerPriceViewModel result = GetBrokerPriceViewModelByIndex(index);
            return result;
        }

        public BrokerPriceViewModel GetBrokerPriceViewModelByIndex(int index)
        {
            BrokerPriceViewModel result = null;
            switch (index)
            {
                case 0:
                    result = OneBrokerPriceViewModel;
                    break;
                case 1:
                    result = TwoBrokerPriceViewModel;
                    break;
                case 2:
                    result = ThreeBrokerPriceViewModel;
                    break;
                case 3:
                    result = FourBrokerPriceViewModel;
                    break;
                case 4:
                    result = FiveBrokerPriceViewModel;
                    break;
                case 5:
                    result = SixBrokerPriceViewModel;
                    break;
            }
            return result;
        }

        private void SetPermission()
        {
            List<string> brokerList = PermisionHandler.Instance.GetAllBrokerList();
            foreach (string key in brokerList)
            {
                BrokerPriceViewModel brokerPriceViewModel = GetBrokerPriceViewModel(key);
                if (brokerPriceViewModel != null)
                {
                    if (!PermisionHandler.Instance.CheckBrokerPermissionByEN(key))
                    {
                        brokerPriceViewModel.Bid = brokerPriceViewModel.Ofr = brokerPriceViewModel.BidAmount = brokerPriceViewModel.OfrAmount = CommonConstants.NO_BROKER;
                    }
                }
            }
        }

        private void SetShowSettle(Dictionary<string, string> allBrokerSettleTipDic, string settleType, string dicKey)
        {
            string[] settleArr = settleType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            bool isContainZero = false;
            bool isCOntainFwd = false;
            foreach (string settle in settleArr)
            {
                if (CommonUtils.IsSettleZero(settle))
                    isContainZero = true;
                if (CommonUtils.IsFwd(settle))
                    isCOntainFwd = true;
            }
            if (isContainZero || isCOntainFwd)
            {
                bool isAll = false;
                bool isZero = false;
                bool isOne = false;
                bool isFwd = false;
                if (SettleFilterDic != null && SettleFilterDic.ContainsKey(CommonConstants.SETTLE))
                {
                    isAll = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.ALL_PARTY);
                    isZero = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.T1_EN);
                    isOne = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.T2_EN);
                    isFwd = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.FWD);
                }
                else
                {
                    isAll = true;
                }

                string setShowSettle = CommonUtils.GetShowSettle(isContainZero, isCOntainFwd);
                if (!allBrokerSettleTipDic.ContainsKey(dicKey))
                {
                    if (setShowSettle == "多")
                    {
                        if (isAll)
                            allBrokerSettleTipDic[dicKey] = setShowSettle;
                        else if (!isAll && isZero && !isFwd)
                            allBrokerSettleTipDic[dicKey] = "+0";
                        else if (!isAll && !isZero && isFwd)
                            allBrokerSettleTipDic[dicKey] = "远";
                    }
                    else if ((setShowSettle == "+0" && (isAll || isZero)) || (setShowSettle == "远" && (isAll || isFwd)))
                    {
                        allBrokerSettleTipDic[dicKey] = setShowSettle;
                    }
                }
                else
                {
                    string localShowSettle = allBrokerSettleTipDic[dicKey];
                    if (setShowSettle == "多" || localShowSettle != setShowSettle)
                    {
                        if (isAll || (isZero && isFwd))
                        {
                            allBrokerSettleTipDic[dicKey] = "多";
                        }
                        else if (isZero && !isFwd)
                        {
                            allBrokerSettleTipDic[dicKey] = "+0";
                        }
                        else if (!isZero && isFwd)
                        {
                            allBrokerSettleTipDic[dicKey] = "远";
                        }
                        else
                        {
                            allBrokerSettleTipDic[dicKey] = setShowSettle;
                        }
                    }
                    else if (localShowSettle == setShowSettle)
                    {
                        if ((setShowSettle == "+0" && !isAll && !isZero) || (setShowSettle == "远" && !isAll && !isFwd))
                            allBrokerSettleTipDic[dicKey] = string.Empty;
                    }
                }
            }
        }

        private void UpdateAllBrokerPrice(string side, List<string> updateVenues, List<GNoMDEntries> noMDEntryList)
        {
            UpdateBrokerPrice(side, OneBrokerPriceViewModel);
            OneBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                OneBrokerPriceViewModel.BidMD.Clear();
            else
                OneBrokerPriceViewModel.OfrMD.Clear();

            UpdateBrokerPrice(side, TwoBrokerPriceViewModel);
            TwoBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                TwoBrokerPriceViewModel.BidMD.Clear();
            else
                TwoBrokerPriceViewModel.OfrMD.Clear();

            UpdateBrokerPrice(side, ThreeBrokerPriceViewModel);
            ThreeBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                ThreeBrokerPriceViewModel.BidMD.Clear();
            else
                ThreeBrokerPriceViewModel.OfrMD.Clear();

            UpdateBrokerPrice(side, FourBrokerPriceViewModel);
            FourBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                FourBrokerPriceViewModel.BidMD.Clear();
            else
                FourBrokerPriceViewModel.OfrMD.Clear();

            UpdateBrokerPrice(side, FiveBrokerPriceViewModel);
            FiveBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                FiveBrokerPriceViewModel.BidMD.Clear();
            else
                FiveBrokerPriceViewModel.OfrMD.Clear();

            UpdateBrokerPrice(side, SixBrokerPriceViewModel);
            SixBrokerPriceViewModel.BrokerBlockWidth = 26;
            if (side == CommonConstants.SIDE_BID)
                SixBrokerPriceViewModel.BidMD.Clear();
            else
                SixBrokerPriceViewModel.OfrMD.Clear();
            SetPermission();

            Dictionary<string, List<string>> allBrokerBarDic = new Dictionary<string, List<string>>();

            Dictionary<string, StringBuilder> allSettleDic = new Dictionary<string, StringBuilder>();

            Dictionary<string, string> allBrokerSettleTipDic = new Dictionary<string, string>();


            // 添加和更新数据(普通)
            foreach (GNoMDEntries entry in noMDEntryList)
            {
                //string[] settlArr = originMDEntries.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (string settle in settlArr)
                {
                    //GNoMDEntries entry = originMDEntries.Clone();
                    //entry.SettlType = settle;

                    string key = entry.Venues;//PriceCalHelper.Instance.GetKey(entry.Venues, entry.MDEntryPx);//entry.MDEntryID;

                    string time = entry.MDEntryTime;
                    string text = entry.Text;
                    string divided = entry.Divided;
                    string price = CommonUtils.FormatNum(CommonUtils.GetShowPrice(entry));//CommonUtils.ConvertToString(entry.MDEntryPx);
                    decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);

                    string settlType = entry.SettlType;


                    if (PermisionHandler.Instance.CheckBrokerPermissionByEN(key))
                    {
                        //string dicKey = string.Format("{0}-{1}", key, side);
                        string dicKey = string.Format("{0}", key);
                        //if (!allBrokerBarDic.ContainsKey(dicKey))
                        //    allBrokerBarDic[dicKey] = entry.BargainFlag;
                        //else
                        //{
                        //    allBrokerBarDic[dicKey] = CalBar(allBrokerBarDic[dicKey], entry);
                        //}
                        if (!allBrokerBarDic.ContainsKey(dicKey))
                        {
                            allBrokerBarDic[dicKey] = new List<string>();
                        }
                        allBrokerBarDic[dicKey].Add(entry.BargainFlag);

                        this.SetShowSettle(allBrokerSettleTipDic, entry.SettlType, dicKey);
                    }

                    BrokerPriceViewModel brokerPriceViewModel = GetBrokerPriceViewModel(key);


                    if (brokerPriceViewModel != null)
                    {
                        if (PermisionHandler.Instance.CheckBrokerPermissionByEN(key))
                        {
                            // 清算速度
                            brokerPriceViewModel.SettleType = settlType;

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
                                brokerPriceViewModel.SettleFwdType = settlType;

                            string[] settleArr = settlType.Split(',');

                            bool isPermission = PermisionHandler.Instance.CheckTradePermission(entry.Venues);
                            if (side == CommonConstants.SIDE_BID)
                            {
                                //findDr[BID_SETTLE_TYPE_FIELD] = settlType;
                                if (settleArr.Contains("2") || settleArr.Contains("3"))
                                {
                                    brokerPriceViewModel.BidT1Qty += qty;
                                }

                                string result = CalCornerMarket(brokerPriceViewModel.BidCornerMarketText, qty, text);
                                brokerPriceViewModel.BidCornerMarketText = result;
                                brokerPriceViewModel.BidCornerMarket = !string.IsNullOrEmpty(result) ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.BidPCornerMarket = IsQtyStar(result) ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.BidStarVisibility = result.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.BidPStarVisibility = result.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                                if (brokerPriceViewModel.BidStarVisibility == Visibility.Visible || brokerPriceViewModel.BidPStarVisibility == Visibility.Visible)
                                {
                                    brokerPriceViewModel.BidLineVisibility = Visibility.Visible;
                                }
                                else
                                {
                                    brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;
                                }

                                decimal localQty = CommonUtils.ConvertToDecimal(brokerPriceViewModel.BidAmount);
                                qty = localQty + qty;
                                brokerPriceViewModel.Bid = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;

                                string emptyStr = GetEmptyStr();

                                brokerPriceViewModel.BidAmount = qty == 0 ? emptyStr : qty.ToString();

                                if ((brokerPriceViewModel.Bid != CommonConstants.NO_PRICE) || (brokerPriceViewModel.BidAmount != CommonConstants.NO_PRICE))
                                {
                                    if (isPermission)
                                    {
                                        brokerPriceViewModel.BidBgBrush = (Brush)_ownUI.FindResource("Bid.Selected.BorderBrush-30%");
                                        brokerPriceViewModel.IsBidPermission = true;
                                    }
                                    else
                                    {
                                        brokerPriceViewModel.BidBgBrush = new SolidColorBrush(Colors.Transparent);
                                        brokerPriceViewModel.IsBidPermission = false;
                                    }
                                }
                                else
                                {
                                    brokerPriceViewModel.BidBgBrush = new SolidColorBrush(Colors.Transparent);
                                    brokerPriceViewModel.IsBidPermission = false;
                                }

                                brokerPriceViewModel.BidMDEntryID = entry.MDEntryID;
                                brokerPriceViewModel.BidPriceType = entry.PriceType;
                                brokerPriceViewModel.BidMD.Add(entry);
                            }
                            else if (side == CommonConstants.SIDE_OFR)
                            {
                                if (settleArr.Contains("2") || settleArr.Contains("3"))
                                {
                                    brokerPriceViewModel.OfrT1Qty += qty;
                                }

                                string result = CalCornerMarket(brokerPriceViewModel.OfrCornerMarketText, qty, text);
                                brokerPriceViewModel.OfrCornerMarketText = result;
                                brokerPriceViewModel.OfrCornerMarket = !string.IsNullOrEmpty(result) ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.OfrPCornerMarket = IsQtyStar(result) ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.OfrStarVisibility = result.Replace("P*", string.Empty).Contains("*") ? Visibility.Visible : Visibility.Collapsed;
                                brokerPriceViewModel.OfrPStarVisibility = result.Contains("P*") ? Visibility.Visible : Visibility.Collapsed;
                                if (brokerPriceViewModel.OfrStarVisibility == Visibility.Visible || brokerPriceViewModel.OfrPStarVisibility == Visibility.Visible)
                                {
                                    brokerPriceViewModel.OfrLineVisibility = Visibility.Visible;
                                }
                                else
                                {
                                    brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
                                }

                                decimal localQty = CommonUtils.ConvertToDecimal(brokerPriceViewModel.OfrAmount);
                                qty = localQty + qty;
                                brokerPriceViewModel.Ofr = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;

                                string emptyStr = GetEmptyStr();
                                brokerPriceViewModel.OfrAmount = qty == 0 ? emptyStr : qty.ToString();


                                if ((brokerPriceViewModel.Ofr != CommonConstants.NO_PRICE) || (brokerPriceViewModel.OfrAmount != CommonConstants.NO_PRICE))
                                {
                                    if (isPermission)
                                    {
                                        brokerPriceViewModel.OfrBgBrush = (Brush)_ownUI.FindResource("Ofr.Selected.BorderBrush-30%");
                                        brokerPriceViewModel.IsOfrPermission = true;
                                    }
                                    else
                                    {
                                        brokerPriceViewModel.OfrBgBrush = new SolidColorBrush(Colors.Transparent);
                                        brokerPriceViewModel.IsOfrPermission = false;
                                    }
                                }
                                else
                                {
                                    brokerPriceViewModel.OfrBgBrush = new SolidColorBrush(Colors.Transparent);
                                    brokerPriceViewModel.IsOfrPermission = false;
                                }

                                brokerPriceViewModel.OfrMDEntryID = entry.MDEntryID;
                                brokerPriceViewModel.OfrPriceType = entry.PriceType;
                                brokerPriceViewModel.OfrMD.Add(entry);
                            }
                        }
                        else
                        {
                            brokerPriceViewModel.Bid = brokerPriceViewModel.Ofr = brokerPriceViewModel.BidAmount = brokerPriceViewModel.OfrAmount = CommonConstants.NO_BROKER;
                            brokerPriceViewModel.BidMDEntryID = brokerPriceViewModel.OfrMDEntryID = null;
                            brokerPriceViewModel.BidPriceType = brokerPriceViewModel.OfrPriceType = string.Empty;
                        }

                        brokerPriceViewModel.UpdateTime = time;
                    }
                }

            }

            foreach (KeyValuePair<string, StringBuilder> kv in allSettleDic)
            {
                BrokerPriceViewModel brokerPriceViewModel = GetBrokerPriceViewModel(kv.Key);
                string allSettle = kv.Value.ToString();
                allSettle = allSettle.Length > 0 ? allSettle.Substring(0, allSettle.Length - 1) : allSettle;

                if (side == CommonConstants.SIDE_BID)
                {
                    brokerPriceViewModel.BidSettleType = allSettle;
                }
                else if (side == CommonConstants.SIDE_OFR)
                {
                    brokerPriceViewModel.OfrSettleType = allSettle;
                }
            }



            //刷新星标
            foreach (KeyValuePair<string, List<string>> kv in allBrokerBarDic)
            {
                string key = kv.Key;//.Substring(0, kv.Key.IndexOf("-"));
                BrokerPriceViewModel brokerPriceViewModel = GetBrokerPriceViewModel(kv.Key);

                if (brokerPriceViewModel != null)
                {
                    if (side == CommonConstants.SIDE_BID)
                    {
                        brokerPriceViewModel.BidBargainFlag = CalBar(kv.Value);
                    }
                    else if (side == CommonConstants.SIDE_OFR)
                    {
                        brokerPriceViewModel.OfrBargainFlag = CalBar(kv.Value);
                    }
                }
            }


            //刷新+0、远、多标签
            UpdateSettleFlag(side, allBrokerSettleTipDic);
        }

        private void UpdateSettleFlag(string side, Dictionary<string, string> allBrokerSettleTipDic)
        {
            foreach (KeyValuePair<string, string> kv in allBrokerSettleTipDic)
            {
                string key = kv.Key;//.Substring(0, kv.Key.IndexOf("-"));
                BrokerPriceViewModel brokerPriceViewModel = GetBrokerPriceViewModel(kv.Key);

                if (brokerPriceViewModel != null)
                {
                    if (side == CommonConstants.SIDE_BID)
                    {
                        brokerPriceViewModel.BidSettleVisibility = Visibility.Visible;
                        brokerPriceViewModel.BidSettleTip = kv.Value;
                    }
                    else if (side == CommonConstants.SIDE_OFR)
                    {
                        brokerPriceViewModel.OfrSettleVisibility = Visibility.Visible;
                        brokerPriceViewModel.OfrSettleTip = kv.Value;
                    }
                }
            }
        }

        private string CalBar(List<string> bargainFlagList)
        {
            //string result = bargainFlag;

            //if (string.IsNullOrEmpty(bargainFlag))
            //    result = mdEntry.BargainFlag;
            //else if (bargainFlag != mdEntry.BargainFlag && mdEntry.BargainFlag == "2")
            //    result = "2";

            string result = "0";

            if (bargainFlagList.Any(item => item == "1"))
            {
                result = "1";
            }
            else if (bargainFlagList.Any(item => item == "2"))
            {
                result = "2";
            }

            return result;
        }

        private string CalCornerMarket(string oldTxt, decimal qty, string text)
        {
            string newText = qty.ToString();
            if (!string.IsNullOrEmpty(text))
                newText = string.Format("{0}", text);//string.Format("{0}({1})", newText, text);

            string oldText = oldTxt;

            if (!string.IsNullOrEmpty(oldText))
            {
                if (!string.IsNullOrEmpty(newText))
                    newText = string.Format("{0}+{1}", oldText, newText);
                else
                    newText = oldText;
            }

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(oldText))
                return string.Empty;

            return newText;

        }

        private bool IsQtyStar(string text)
        {
            bool isAllP = false;
            if (string.IsNullOrEmpty(text) == false)
            {
                string[] arr = text.Split(CommonConstants.PLUS_CHAR);
                isAllP = true;
                foreach (string subText in arr)
                {
                    if (!subText.Contains("P*"))
                    {
                        isAllP = false;
                        break;
                    }
                }
            }
            return isAllP;
        }

        private void UpdateBrokerPrice(string side, BrokerPriceViewModel brokerPriceViewModel)
        {
            string emptyStr = GetEmptyStr();

            if (side == CommonConstants.SIDE_BID)
            {
                brokerPriceViewModel.BidAmount = emptyStr;
                brokerPriceViewModel.BidCornerMarket = Visibility.Collapsed;
                brokerPriceViewModel.BidCornerMarketText = string.Empty;
                brokerPriceViewModel.BidPCornerMarket = Visibility.Collapsed;
                brokerPriceViewModel.BidStarVisibility = Visibility.Collapsed;
                brokerPriceViewModel.BidPStarVisibility = Visibility.Collapsed;
                brokerPriceViewModel.BidLineVisibility = Visibility.Collapsed;
                brokerPriceViewModel.Bid = emptyStr;
                brokerPriceViewModel.BidBargainFlag = "0";

                brokerPriceViewModel.BidBgBrush = new SolidColorBrush(Colors.Transparent);
                brokerPriceViewModel.IsBidPermission = false;
                brokerPriceViewModel.BidSettleType = string.Empty;
                brokerPriceViewModel.BidPriceType = string.Empty;
                brokerPriceViewModel.BidSettleTip = string.Empty;
                brokerPriceViewModel.BidSettleVisibility = Visibility.Collapsed;

                brokerPriceViewModel.BidT1Qty = 0;
            }
            else if (side == CommonConstants.SIDE_OFR)
            {
                brokerPriceViewModel.OfrAmount = emptyStr;
                brokerPriceViewModel.OfrCornerMarket = Visibility.Collapsed;
                brokerPriceViewModel.OfrCornerMarketText = string.Empty;
                brokerPriceViewModel.OfrPCornerMarket = Visibility.Collapsed;
                brokerPriceViewModel.OfrStarVisibility = Visibility.Collapsed;
                brokerPriceViewModel.OfrPStarVisibility = Visibility.Collapsed;
                brokerPriceViewModel.OfrLineVisibility = Visibility.Collapsed;
                brokerPriceViewModel.Ofr = emptyStr;
                brokerPriceViewModel.OfrBargainFlag = "0";

                brokerPriceViewModel.OfrBgBrush = new SolidColorBrush(Colors.Transparent);
                brokerPriceViewModel.IsOfrPermission = false;
                brokerPriceViewModel.OfrSettleType = string.Empty;
                brokerPriceViewModel.OfrPriceType = string.Empty;
                brokerPriceViewModel.OfrSettleTip = string.Empty;
                brokerPriceViewModel.OfrSettleVisibility = Visibility.Collapsed;

                brokerPriceViewModel.OfrT1Qty = 0;
            }
        }

        #endregion


        #region 最优价
        private void CalBestPrice(string securityID, bool needRefreshColor)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this._lockObj)
                {
                    try
                    {
                        if (securityID != _priceModel.Security)
                        {
                            return;
                        }
                        Dictionary<string, IList> dic = MainConditionCache.Instance.GetConditionById(_groupId) as Dictionary<string, IList>;

                        string filterStr = string.Empty;
                        bool isFuture = CommonUtils.IsFutureBond(securityID);

                        if (!isFuture)
                            filterStr = ConditionHelper.GetNewPriceViewCondition(_settleFilterDic, dic);

                        BilateralBestPrice bestPrice = PriceHandler.Instance.CalBestPrice(securityID, filterStr, string.Empty);//PriceCalHelper.Instance.CalBestPrice(bilateralBestList);

                        _priceModel.BidSettleTypeCopy = bestPrice.BidSettleTypes;
                        _priceModel.OfrSettleTypeCopy = bestPrice.OfrSettleTypes;

                        


                        bool isAll = false;
                        bool isZero = false;
                        bool isOne = false;
                        bool isFwd = false;
                        if (SettleFilterDic != null && SettleFilterDic.ContainsKey(CommonConstants.SETTLE))
                        {
                            isAll = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.ALL_PARTY);
                            isZero = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.T1_EN);
                            isOne = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.T2_EN);
                            isFwd = ConditionHelper.CheckSettleConditionBySelf(SettleFilterDic, CommonConstants.FWD);
                        }
                        else
                        {
                            isAll = true;
                        }




                        if (bestPrice.BidBest != null)
                        {
                            _priceModel.BestBid = isFuture ? CommonUtils.FormatNumFuture(bestPrice.BidBest.bestPx) : CommonUtils.FormatNum(bestPrice.BidBest.bestPx);
                            _priceModel.BestBidAmount = CommonUtils.formatQtyToK(bestPrice.BidBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.BidBest.bestQty.ToString());

                            if (LocalEnv.ConfInfo.ShowBestSource == "1")
                                _priceModel.BestBidVenue = ComboSourceUtil.Instance.FindEnum(bestPrice.BidBest.venue, "venues");
                            else
                                _priceModel.BestBidVenue = string.Empty;


                            if ((bestPrice.BidBest.bestT0 == "1" || bestPrice.BidBest.bestYuan == "1") && !isOne)
                            {
                                _priceModel.AllBidSettleVisibility = Visibility.Visible;

                                string setShowTip = CommonUtils.GetShowSettle(bestPrice.BidBest.bestT0 == "1", bestPrice.BidBest.bestYuan == "1");
                                if ((setShowTip == "+0" && (isAll || isZero)) || (setShowTip == "远" && (isAll || isFwd)))
                                    _priceModel.AllBidSettleTip = setShowTip;
                                else if (setShowTip == "多")
                                {
                                    if (isAll || (isZero && isFwd))
                                        _priceModel.AllBidSettleTip = setShowTip;
                                    else if (!isAll && isZero && !isFwd)
                                        _priceModel.AllBidSettleTip = "+0";
                                    else if (!isAll && !isZero && isFwd)
                                        _priceModel.AllBidSettleTip = "远";
                                }
                            }
                            else
                            {
                                _priceModel.AllBidSettleTip = string.Empty;
                                _priceModel.AllBidSettleVisibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            string emptyStr = GetEmptyStr();
                            _priceModel.BestBid = _priceModel.BestBidAmount = emptyStr;
                            _priceModel.BestBidVenue = string.Empty;


                            _priceModel.AllBidSettleTip = string.Empty;
                            _priceModel.AllBidSettleVisibility = Visibility.Collapsed;
                        }

                        if (bestPrice.OfrBest != null)
                        {
                            _priceModel.BestOfr = isFuture ? CommonUtils.FormatNumFuture(bestPrice.OfrBest.bestPx) : CommonUtils.FormatNum(bestPrice.OfrBest.bestPx);
                            _priceModel.BestOfrAmount = CommonUtils.formatQtyToK(bestPrice.OfrBest.bestQty == 0 ? CommonConstants.NO_PRICE : bestPrice.OfrBest.bestQty.ToString());

                            if (LocalEnv.ConfInfo.ShowBestSource == "1")
                                _priceModel.BestOfrVenue = ComboSourceUtil.Instance.FindEnum(bestPrice.OfrBest.venue, "venues");
                            else
                                _priceModel.BestOfrVenue = string.Empty;



                            if ((bestPrice.OfrBest.bestT0 == "1" || bestPrice.OfrBest.bestYuan == "1") && !isOne)
                            {
                                _priceModel.AllOfrSettleVisibility = Visibility.Visible;

                                //_priceModel.AllOfrSettleTip = CommonUtils.GetShowSettle(bestPrice.OfrBest.bestT0 == "1", bestPrice.OfrBest.bestYuan == "1");


                                string setShowTip = CommonUtils.GetShowSettle(bestPrice.OfrBest.bestT0 == "1", bestPrice.OfrBest.bestYuan == "1");
                                if ((setShowTip == "+0" && (isAll || isZero)) || (setShowTip == "远" && (isAll || isFwd)))
                                    _priceModel.AllOfrSettleTip = setShowTip;
                                else if (setShowTip == "多")
                                {
                                    if (isAll || (isZero && isFwd))
                                        _priceModel.AllOfrSettleTip = setShowTip;
                                    else if (!isAll && isZero && !isFwd)
                                        _priceModel.AllOfrSettleTip = "+0";
                                    else if (!isAll && !isZero && isFwd)
                                        _priceModel.AllOfrSettleTip = "远";
                                }

                            }
                            else
                            {
                                _priceModel.AllOfrSettleTip = string.Empty;
                                _priceModel.AllOfrSettleVisibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            string emptyStr = GetEmptyStr();
                            _priceModel.BestOfr = _priceModel.BestOfrAmount = emptyStr;
                            _priceModel.BestOfrVenue = string.Empty;

                            _priceModel.AllOfrSettleTip = string.Empty;
                            _priceModel.AllOfrSettleVisibility = Visibility.Collapsed;
                        }

                        decimal bidD = 0;
                        decimal ofrD = 0;

                        string isUpSide = string.Empty;
                        if (bestPrice.BidBest != null && bestPrice.OfrBest != null && decimal.TryParse(bestPrice.BidBest.bestPx, out bidD) && decimal.TryParse(bestPrice.OfrBest.bestPx, out ofrD)
                            && bidD > 0 && ofrD > 0)
                        {
                            //this.BestPriceBP = string.Format("{0}bp", CommonUtils.formatString((00).ToString(), 0));

                            decimal bpd = decimal.Zero;
                            bool isNetPrice = false;
                            if (bestPrice.OfrBest.priceType == PriceType.NetPrice && bestPrice.BidBest.priceType == PriceType.NetPrice)
                            {
                                bpd = (bidD - ofrD);
                                isNetPrice = true;
                            }
                            else if (bestPrice.OfrBest.priceType == bestPrice.BidBest.priceType && bestPrice.OfrBest.priceType == PriceType.Yield)
                            {
                                bpd = (bidD - ofrD) * 100;
                            }
                            else if (isFuture)
                            {
                                bpd = (bidD - ofrD);
                            }

                            bpd = (isFuture || isNetPrice) ? Math.Abs(bpd) : bpd;
                            _priceModel.BestPriceBP = isFuture ? bpd.ToString() : PriceCalHelper.Instance.FormatBestBP(bpd); //bpd == 0 ? CommonConstants.NO_PRICE : PriceCalHelper.Instance.FormatBestBP(bpd);//string.Format("{0}bp", CommonUtils.formatString(bpd.ToString(), 2));

                            if (isNetPrice || isFuture)
                            {
                                _priceModel.BestPriceBP = _priceModel.BestPriceBP.Replace("bp", "");
                            }

                            if (PriceCalHelper.Instance.IsUpSide(bestPrice) && !isFuture)//国债期货没有倒挂
                            {
                                _priceModel.UpSideBdBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Red-60%");
                                _priceModel.UpSideBgBrush = (Brush)_ownUI.FindResource("Textblock.Foreground.Red-16%");

                                isUpSide = "1";
                            }
                            else
                            {
                                _priceModel.UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                                _priceModel.UpSideBgBrush = (Brush)_ownUI.FindResource("Black-1");
                            }
                        }
                        else
                        {
                            bool isEmpty = string.IsNullOrEmpty(Id) && string.IsNullOrEmpty(PriceModel.Security);

                            _priceModel.BestPriceBP = isEmpty ? string.Empty : CommonConstants.NO_UP_DOWN;
                            _priceModel.UpSideBdBrush = (Brush)_ownUI.FindResource("Border.Background");
                            _priceModel.UpSideBgBrush = (Brush)_ownUI.FindResource("Black-1");
                        }



                        if (bestPrice.BidBest != null && bestPrice.OfrBest != null && needRefreshColor)
                        {
                            Border bdBid = _ownUI.FindName("bdBid") as Border;

                            if (bdBid != null)
                                AnimationHelper.Instance.StartAnimation(bdBid, "bdBid", isUpSide, true);

                            Border bdOfr = _ownUI.FindName("bdOfr") as Border;

                            if (bdOfr != null)
                                AnimationHelper.Instance.StartAnimation(bdOfr, "bdOfr", isUpSide, true);

                            Border bdBp = _ownUI.FindName("bdBp") as Border;

                            if (bdBp != null)
                                AnimationHelper.Instance.StartAnimation(bdBp, "bdBp", isUpSide, true);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("CalBestPrice exception:", ex);
                    }
                }
            }));

        }
        #endregion


        #region 实时结算方式改变

        private OperationType _currentOperationType = OperationType.None;
        public void SetOperationType(OperationType operationType)
        {
            _currentOperationType = operationType;
        }

        public void OnSettleChange(OperationType operationType, object settleParam)
        {
            bool isFuture = CommonUtils.IsFutureBond(_priceModel.Security);

            if (!isFuture && settleParam != null)
            {
                _currentOperationType = operationType;
                Dictionary<string, IList> dic = settleParam as Dictionary<string, IList>;

                this.SettleFilterDic = dic;

                _currentOperationType = OperationType.None;
            }
        }

        #endregion
    }
}
