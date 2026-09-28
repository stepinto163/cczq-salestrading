using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.OHLC;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.OHLC;
using STC.Common.Utils;
using STC.Common.ViewModel.Card;
using STC.Common.ViewModel.Data;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static STC.Common.Utils.HomePageDataHandler;

namespace STC.Common.ViewModel
{
    /// <summary>
    /// TradeBookView的数据内容
    /// </summary>
    public class QuotationListViewModel : ViewModelBase
    {
        WindowManager m_windowManager = WindowManager.Instance;//new WindowManager();
        public ICommand CloseCommand { get { return new ComRelayCommand(OnCloseClick); } }
        //public ICommand TopCommand { get { return new RelayCommand(OnTopClick); } }

        public event Action<QuotationEventArgs> OnListChangeEvent;
        public event Action<ExecutionReport> OnYunEvent;
        
        public Window myWindow { get; set; }
        #region define
        private PriceModel _priceModel = new PriceModel();
        
        public PriceModel PriceModel { get { return _priceModel; } set { _priceModel = value; this.OnPropertyChanged(nameof(PriceModel)); } }
        /// <summary>
        /// 报价权限
        /// (即若“勾选”则有点价或报价界面，若“不勾选”则无报价界面)
        /// </summary>
        private readonly string MODULEID = "1003";

        /// <summary>
        /// 是否是收藏债券， 0：不是 ； 1：是
        /// </summary>
        private string _isFavorite = "0";
        public string IsFavorite
        {
            get
            {
                return _isFavorite;
            }

            set
            {
                if (_isFavorite == value)
                {
                    return;
                }
                _isFavorite = value;
                this.OnPropertyChanged(nameof(IsFavorite));
            }
        }


        /// <summary>
        /// 债券代码
        /// </summary>
        private string _SecurityID = string.Empty;

        public string SecurityID
        {
            get
            {
                return this._SecurityID;
            }

            set
            {
                if (this._SecurityID != value)
                {
                    this._SecurityID = string.Empty;
                    this.ClearData(true);
                    // 切换债券
                    this._SecurityID = value;
                    this.OnPropertyChanged();

                    ShowImcompletePanel = Visibility.Collapsed;
                    ShowSum = Visibility.Collapsed;

                    InitNewSecurityID(value);
                }
            }
        }

        private async Task InitNewSecurityID(string securityid)
        {
            await Task.Run(() =>
            {

                if (!string.IsNullOrEmpty(securityid))
                {
                    this.Init();
                }

                if (!string.IsNullOrEmpty(securityid))
                {
                    GetSymbolInfo();
                }

                Security = securityid;
            });
        }

        private string _headerAmount = string.Empty;

        public string HeaderAmount
        {
            get
            {
                return this._headerAmount;
            }

            set
            {
                if ((this._headerAmount != value))
                {
                    this._headerAmount = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private async Task GetIsFavorite(string securityId)
        {
            await Task.Run(() =>
            {
                IsFavorite = HomePageDataHandler.IsFavorite(securityId);
            });
        }

        /// <summary>
        /// 债券名称
        /// </summary>
        private string _Symbol = string.Empty;

        public string Symbol
        {
            get
            {
                return this._Symbol;
            }

            set
            {
                if (this._Symbol != value)
                {
                    this._Symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 债券全称
        /// </summary>
        private string _FullName = string.Empty;

        public string FullName
        {
            get
            {
                return this._FullName;
            }

            set
            {
                if (this._FullName != value)
                {
                    this._FullName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _security;
        public string Security
        {
            get { return _security; }
            set 
            { 
                _security = value;

                if (!string.IsNullOrEmpty(value))
                    IsSheBond = CommonUtils.IsSheBond(value) ? "1" : "0";
                else
                    IsSheBond = "0";

                this.OnPropertyChanged(); 
            }
        }


        private string _symbolName;
        public string SymbolName
        {
            get { return _symbolName; }
            set { _symbolName = value; this.OnPropertyChanged(); }
        }


        // 显示搜索
        private string showtext = string.Empty;
        public string ShowText
        {
            get
            {
                return showtext;
            }

            set
            {
                if (value != showtext)
                {
                    showtext = value;
                    this.OnPropertyChanged(nameof(ShowText));
                }
            }
        }

        private string _cn_short_name;
        /// <summary>
        /// 发行人
        /// </summary>
        public string CN_Short_Name
        {
            get { return _cn_short_name; }
            set { _cn_short_name = value; this.OnPropertyChanged(nameof(CN_Short_Name)); }
        }

        private string _coupon_rate_current;
        /// <summary>
        /// 票面
        /// </summary>
        public string Coupon_Rate_Current
        {
            get { return _coupon_rate_current; }
            set { _coupon_rate_current = value; this.OnPropertyChanged(nameof(Coupon_Rate_Current)); }
        }

        private string _ccdcvaluationshow;
        /// <summary>
        /// 用于显示的中债估值
        /// </summary>
        public string CCDCValuationShow
        {
            get { return _ccdcvaluationshow; }
            set 
            { 
                _ccdcvaluationshow = value; 
                this.OnPropertyChanged(nameof(CCDCValuationShow)); 
            }
        }

        private string _ccdcvaluadate;
        /// <summary>
        /// 中债估值日期
        /// </summary>
        public string CCDCValuaDate
        {
            get { return _ccdcvaluadate; }
            set
            {
                _ccdcvaluadate = value;
                this.OnPropertyChanged(nameof(CCDCValuaDate));
            }
        }

        private string _showlastworkday;
        /// <summary>
        /// 中债估值日期警告标记 !
        /// </summary>
        public string ShowLastWorkDay
        {
            get { return _showlastworkday; }
            set
            {
                _showlastworkday = value;
                ShowLastWorkDayVisible = value == "!" ? Visibility.Visible : Visibility.Collapsed;
                this.OnPropertyChanged();
            }
        }

        private Visibility _showlastworkdayVisible = Visibility.Collapsed;
        public Visibility ShowLastWorkDayVisible
        {
            get { return _showlastworkdayVisible; }
            set
            {
                _showlastworkdayVisible = value;
                this.OnPropertyChanged();

            }
        }

        private Visibility _ShowExeRadioVisible = Visibility.Collapsed;
        public Visibility ShowExeRadioVisible
        {
            get { return _ShowExeRadioVisible; }
            set
            {
                _ShowExeRadioVisible = value;
                this.OnPropertyChanged();
                if (_ShowExeRadioVisible == Visibility.Visible && !CheckExeRadio)
                {
                    CheckExeRadio = true;
                    CheckMarRadio = false;
                }
                else 
                {
                    CheckExeRadio = false;
                    CheckMarRadio = true;
                }
            }
        }

        private bool _CheckExeRadio = false;
        public bool CheckExeRadio
        {
            get { return _CheckExeRadio; }
            set 
            { 
                _CheckExeRadio = value;
                this.OnPropertyChanged();
            }
        }

        private bool _CheckMarRadio = false;
        public bool CheckMarRadio
        {
            get { return _CheckMarRadio; }
            set
            {
                _CheckMarRadio = value;
                this.OnPropertyChanged();
            }
        }

        private string _lastworkday;
        /// <summary>
        /// 上一个工作日
        /// </summary>
        public string LastWorkDay
        {
            get { return _lastworkday; }
            set
            {
                _lastworkday = value;
                this.OnPropertyChanged(nameof(LastWorkDay));
            }
        }

        private string _defaultpricetype;
        public string DefaultPriceType
        {
            get { return _defaultpricetype; }
            set 
            {
                _defaultpricetype = value;
                //DefaultPriceType 1 - 净价 3 - 收益率 ;  0-原价
                if (value == "3") //3-	收益率
                {
                    IsShowNetPrice = false;
                }
                else if (value == "1")
                {
                    IsShowNetPrice = true;
                }
                else
                {
                    IsShowNetPrice = false;
                }
                BidSort();
                ofrSort();

                this.OnPropertyChanged(nameof(DefaultPriceType)); 
            }
        }
        
        /// <summary>
        /// UI上用户自选择索引
        /// </summary>
        private int _PriceTypeIndex;
        public int PriceTypeIndex
        {
            get { return _PriceTypeIndex; }
            set 
            {
                _PriceTypeIndex = value;
                if (value == 0)
                {
                    DefaultPriceType = "3";
                    //IsYieldType = "true";
                }
                else if (value == 1)
                {
                    DefaultPriceType = "1";
                    //IsYieldType = "false";
                }
                else
                {
                    DefaultPriceType = "0";
                }
                //if (value == 1)
                //{
                //    IsShowNetPrice = false;
                //}
                //else
                //{
                //    IsShowNetPrice = true;
                //}
                this.OnPropertyChanged(nameof(PriceTypeIndex)); 
            }
        }
        private string _IsYieldType;

        public string IsYieldType
        {
            get
            {
                return _IsYieldType;
            }

            set
            {
                _IsYieldType = value;
                this.OnPropertyChanged();

            }
        }
        private Visibility _pirceTypeVisible = Visibility.Visible;

        public Visibility PirceTypeVisible
        { 
            get { return _pirceTypeVisible; }
            set
            {
                _pirceTypeVisible = value;
                this.OnPropertyChanged(nameof(PirceTypeVisible));
            }
        }


        private string _marketIndicator;
        public string MarketIndicator
        { 
            get { return _marketIndicator; }
            set 
            {
                _marketIndicator = value;
                this.OnPropertyChanged(nameof(MarketIndicator)); 
            }
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
        private string _lastExecNetPrice = CommonConstants.NO_PRICE;
        public string LastExecNetPrice
        {
            get { return _lastExecNetPrice; }
            set { _lastExecNetPrice = value; this.OnPropertyChanged(); }
        }

        private string _lastExecNetPriceSpread;
        public string LastExecNetPriceSpread
        {
            get { return _lastExecNetPriceSpread; }
            set { _lastExecNetPriceSpread = value; this.OnPropertyChanged(); }
        }
        private string _lastExecSideShow;
        public string LastExecSideShow
        {
            get { return _lastExecSideShow; }
            set { _lastExecSideShow = value; this.OnPropertyChanged(); }
        }

        private string _lastExecSource;
        public string LastExecSource
        {
            get { return _lastExecSource; }
            set { _lastExecSource = value; this.OnPropertyChanged(nameof(LastExecSource)); }
        }

        private Brush _lastExecBrush;
        public Brush LastExecBrush
        {
            get { return _lastExecBrush; }
            set { _lastExecBrush = value; this.OnPropertyChanged(); }
        }

        private string _lastExecTime;
        public string LastExecTime
        {
            get { return _lastExecTime; }
            set { _lastExecTime = value; this.OnPropertyChanged(nameof(LastExecTime)); }
        }
        /// <summary>
        /// 是否显示净价
        /// </summary>
        private volatile bool _isShowNetPrice = false;
        public bool IsShowNetPrice
        {
            get
            {
                return _isShowNetPrice;
            }

            set
            {
                if (_isShowNetPrice == value)
                {
                    return;
                }
                _isShowNetPrice = value;
                //string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                //string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                //if (_isShowNetPrice)
                //{
                //    bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD} desc,{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                //    ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                //}

                ////BaseDV.Sort = bidSort;
                //BidQuotations.Sort = bidSort;
                //OfrQuotations.Sort = ofrSort;
                //BidQuotations_Frozen.Sort = bidSort;
                //OfrQuotations_Frozen.Sort = ofrSort;
                //BidIncompleteQuotations.Sort = bidSort;
                //OfrIncompleteQuotations.Sort = ofrSort;
                //AdjustData(this.BidQuotations, this.BidQuotations_Frozen, bidLockObj, bidLockObj_Frozen);
                //AdjustData(this.OfrQuotations, this.OfrQuotations_Frozen, ofrLockObj, ofrLockObj_Frozen);

                //EntryPxColumnVisiblity = _isShowNetPrice == true ? Visibility.Collapsed : Visibility.Visible;
                //NetPriceColumnVisibility = _isShowNetPrice == true ? Visibility.Visible : Visibility.Collapsed;
                //this.OnPropertyChanged();
                //this.AdjustData(this.BidQuotations);
                //this.AdjustData(this.OfrQuotations);
            }
        }

        private Visibility _EntryPxColumnVisiblity = Visibility.Visible;
        public Visibility EntryPxColumnVisiblity
        {
            get
            {
                return _EntryPxColumnVisiblity;
            }
            set
            {
                if (_EntryPxColumnVisiblity == value)
                {
                    return;
                }
                _EntryPxColumnVisiblity = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _NetPriceColumnVisibility = Visibility.Visible;
        public Visibility NetPriceColumnVisibility
        {
            get
            {
                return _NetPriceColumnVisibility;
            }
            set
            {
                if (_NetPriceColumnVisibility == value)
                {
                    return;
                }
                _NetPriceColumnVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ShowImcompletePanel = Visibility.Collapsed;
        public Visibility ShowImcompletePanel
        {
            get
            {
                return _ShowImcompletePanel;
            }
            set
            {
                if (_ShowImcompletePanel != value)
                {
                    _ShowImcompletePanel = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ShowSum= Visibility.Collapsed;
        public Visibility ShowSum
        {
            get
            {
                return _ShowSum;
            }
            set
            {
                if (_ShowSum != value)
                {
                    _ShowSum = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ShowOrderBtn = Visibility.Collapsed;

        public Visibility ShowOrderBtn
        {
            get
            {
                return _ShowOrderBtn;
            }
            set
            {
                if (_ShowOrderBtn != value)
                {
                    _ShowOrderBtn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility showccdc = Visibility.Visible;

        public Visibility ShowCCDC
        {
            get
            {
                return showccdc;
            }
            set
            {
                if (showccdc != value)
                {
                    showccdc = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private Visibility _showFilterVisisble = Visibility.Visible;
        /// <summary>
        /// 是否显示渠道过滤，清算速度过滤
        /// </summary>
        public Visibility ShowFilterVisisble
        {
            get
            {
                return _showFilterVisisble;
            }
            set
            {
                _showFilterVisisble = value;
                this.OnPropertyChanged();
            }
        }

        private string _IsSheBond = "0";
        /// <summary>
        /// 是否是交易所债券
        /// </summary>
        public string IsSheBond
        {
            get { return _IsSheBond; }
            set
            {
                _IsSheBond = value;
                this.OnPropertyChanged(nameof(IsSheBond));
            }
        }

        private object baseLockObj = new object();
        private object bidLockObj = new object();
        private object ofrLockObj = new object();
        private object bidLockObj_Frozen = new object();
        private object ofrLockObj_Frozen = new object();
        private object bidIncompleteLockObj = new object();
        private object ofrIncompleteLockObj = new object();

        private DataView _BaseDV;
        private DataView BaseDV
        {
            get
            {
                lock (baseLockObj)
                {
                    if (_BaseDV == null)
                    {
                        _BaseDV = new DataTable().DefaultView;
                        _BaseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISINCOMPLETE_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string)); 
                        _BaseDV.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
                        _BaseDV.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));

                        _BaseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                        _BaseDV.Table.PrimaryKey = new DataColumn[] { _BaseDV.Table.Columns[CTPriceControler.KEY_FIELD] };
                        this.OnPropertyChanged();
                    }
                    return this._BaseDV;
                }
            }
        }

        private DataView _BidQuotations;
        public DataView BidQuotations
        {
            get
            {
                lock (bidLockObj)
                {
                    if (_BidQuotations == null)
                    {
                        _BidQuotations = new DataTable().DefaultView;
                        _BidQuotations.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(decimal));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.HIDE_PX_FLAG_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.HIDE_NETPRICE_FLAG_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));

                        if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _BidQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc, {CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                        {
                            _BidQuotations.Sort = $"{CTPriceControler.YieldNum_Field},{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }
                        _BidQuotations.Table.PrimaryKey = new DataColumn[] { _BidQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        _BidQuotations.Table.TableName = "BidQuotations";
                        //_BidQuotations.ListChanged += _BidQuotations_ListChanged;
                        this.OnPropertyChanged();
                    }
                    return this._BidQuotations;
                }
            }
        }

        private DataView _OfrQuotations;
        public DataView OfrQuotations
        {
            get
            {
                lock (ofrLockObj)
                {
                    if (_OfrQuotations == null)
                    {
                        _OfrQuotations = new DataTable().DefaultView;
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(decimal));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.HIDE_PX_FLAG_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.HIDE_NETPRICE_FLAG_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));

                        if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _OfrQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                        {
                            _OfrQuotations.Sort = $"{CTPriceControler.YieldNum_Field} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }

                        _OfrQuotations.Table.PrimaryKey = new DataColumn[] { _OfrQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        _OfrQuotations.Table.TableName = "OfrQuotations";
                        //_OfrQuotations.ListChanged += _OfrQuotations_ListChanged;
                        this.OnPropertyChanged();
                    }
                    return this._OfrQuotations;
                }
            }
        }

        private void BidSort() 
        {
            lock (bidLockObj)
            {
                if (_BidQuotations != null)
                {

                    if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        _BidQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc, {CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                    else
                    {
                        if (PriceTypeIndex == 1)
                        {
                            _BidQuotations.Sort = $"{CTPriceControler.NET_PRICE_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }
                        else 
                        {
                            _BidQuotations.Sort = $"{CTPriceControler.YieldNum_Field},{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }
                    }
                    this.OnPropertyChanged();
                }
            }
            AdjustData(this.BidQuotations, this.BidQuotations_Frozen, bidLockObj, bidLockObj_Frozen);
        }
        private void ofrSort()
        {
            lock (ofrLockObj)
            {
                if (_OfrQuotations != null)
                {

                    if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        _OfrQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                    else
                    {
                        if (PriceTypeIndex == 1)
                        {
                            _OfrQuotations.Sort = $"{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }
                        else
                        {
                            _OfrQuotations.Sort = $"{CTPriceControler.YieldNum_Field} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc";
                        }
                    }
                    this.OnPropertyChanged();
                }
            }
            AdjustData(this.OfrQuotations, this.OfrQuotations_Frozen, ofrLockObj, ofrLockObj_Frozen);
        }

        private DataView _BidQuotations_Frozen;
        public DataView BidQuotations_Frozen
        {
            get
            {
                lock (bidLockObj_Frozen)
                {
                    if (_BidQuotations_Frozen == null)
                    {
                        _BidQuotations_Frozen = BidQuotations.Table.Clone().DefaultView;

                        if(LocalEnv.ConfInfo.IsUseVenueSort == "1")
                             _BidQuotations_Frozen.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc, {CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _BidQuotations_Frozen.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD} desc,{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                        this.OnPropertyChanged();
                    }
                    return this._BidQuotations_Frozen;
                }
            }
        }

        private DataView _OfrQuotations_Frozen;
        public DataView OfrQuotations_Frozen
        {
            get
            {
                lock (ofrLockObj_Frozen)
                {
                    if (_OfrQuotations_Frozen == null)
                    {
                        _OfrQuotations_Frozen = OfrQuotations.Table.Clone().DefaultView;

                        if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _OfrQuotations_Frozen.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _OfrQuotations_Frozen.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

                        this.OnPropertyChanged();
                    }
                    return this._OfrQuotations_Frozen;
                }
            }
        }

        private DataView _BidIncompleteQuotations;
        public DataView BidIncompleteQuotations
        {
            get
            {
                lock (bidIncompleteLockObj)
                {
                    if (_BidIncompleteQuotations == null)
                    {
                        _BidIncompleteQuotations = new DataTable().DefaultView;
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));

                        //if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        //    _BidIncompleteQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        //else
                        //_BidIncompleteQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                        _BidIncompleteQuotations.Sort = $"{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.QTY_FIELD} desc";

                        _BidIncompleteQuotations.Table.PrimaryKey = new DataColumn[] { _BidIncompleteQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        this.OnPropertyChanged();
                    }
                    return this._BidIncompleteQuotations;
                }
            }
        }

        private DataView _OfrIncompleteQuotations;
        public DataView OfrIncompleteQuotations
        {
            get
            {
                lock (ofrIncompleteLockObj)
                {
                    if (_OfrIncompleteQuotations == null)
                    {
                        _OfrIncompleteQuotations = new DataTable().DefaultView;
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(decimal));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(decimal));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.YieldToExecution_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.ExerciseFlag_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.BOND_TYPE_FIELD, typeof(string));
                        //if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        //    _OfrIncompleteQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        //else
                        //    _OfrIncompleteQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                        _OfrIncompleteQuotations.Sort = $"{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.QTY_FIELD} desc";
                        _OfrIncompleteQuotations.Table.PrimaryKey = new DataColumn[] { _OfrIncompleteQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        this.OnPropertyChanged();
                    }
                    return this._OfrIncompleteQuotations;
                }
            }
        }

        private string _rowFilterStr = string.Empty;

        private Thread CalcDataThread = null;
        private Thread RefreshDataThread = null;

        public string SettleFilter = "全";
        #endregion

        public QuotationListViewModel()
        {
            InitQ();
        }

        private async void InitQ()
        {
            await InitQLVM();
        }

        private async Task InitQLVM()
        {
            await Task.Run(() =>
            {
                if (CalcDataThread == null)
                {
                    CalcDataThread = new Thread(RefreshAdjustFlagType);//CalcData);
                    CalcDataThread.IsBackground = true;
                    CalcDataThread.Start();
                }
                if (RefreshDataThread == null)
                {
                    RefreshDataThread = new Thread(RefreshQueue);//RefreshData);
                    RefreshDataThread.IsBackground = true;
                    RefreshDataThread.Start();
                }

                BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);
                BaseHandler.Instance.Init(Instance_OnClear);

                #region 收MarketExec消息
                //LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnOwnExecutions;
                //ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew += Instance_OnOwnExecutions;
                #endregion
                // 报价权限问题
                SetPermission();

                //订阅最后成交处理完成事件，当获取历史记录完成后更新界面的最后成交
                BaseHandler.Instance.SubscribeProctMarketExecutionsStatus(OnLoadtMarketExecutionsStatus);

                Init();
            });
        }

        private void OnLoadtMarketExecutionsStatus(bool isok)
        {
            if (isok)
            {
                LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnOwnExecutions;
                //ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew += Instance_OnOwnExecutions;
                //获取最优价
                if (!string.IsNullOrEmpty(this.SecurityID) && this.SecurityID.ToLower() != "tradebook")
                    LastExecutionProcesser.Processer.AddGetLastExecutionQueue(this.SecurityID);
            }
        }

        ////private void Instance_OnLastNewMarketExecutions(MarketExecutionsItem obj)
        ////{
        ////    if (obj.SecurityID == this.Security)
        ////    {
        ////        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        ////        {
        ////            ExecutionReport temOrder = new ExecutionReport();
        ////            temOrder.TransactTime = obj.TimeStamp;
        ////            temOrder.SecurityID = obj.SecurityID;
        ////            temOrder.Side = obj.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : obj.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : obj.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : obj.Side;
        ////            temOrder.LastPx = obj.ExecutePricePercent.ToString();
        ////            temOrder.Venues = obj.SourceTrader;
        ////            temOrder.Yield = temOrder.LastPx;
        ////            temOrder.NetPrice = obj.NetPrice.ToString();
        ////            if (string.IsNullOrEmpty(LastExecPrice))
        ////            {
        ////                string tmppric = CommonUtils.FormatNum(temOrder.LastPx);
        ////                LastExecPrice = tmppric == "0.00" ? "" : tmppric;
        ////                LastExecSideShow = ComboSourceUtil.Instance.FindEnum(temOrder.Side, "side2"); ;
        ////                LastExecSource = ComboSourceUtil.Instance.FindEnum(temOrder.Venues, "venues");
        ////            }
        ////            if (string.IsNullOrEmpty(LastExecNetPrice))
        ////            {
        ////                string tmppric = CommonUtils.FormatNum(temOrder.NetPrice);
        ////                LastExecNetPrice = tmppric == "0.00" ? "" : tmppric;
        ////            }
        ////            this.OnLastExec(temOrder);
        ////        }));
        ////    }
        ////}

        private void _BidQuotations_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (this.OnListChangeEvent != null)
            {
                QuotationEventArgs arg = new QuotationEventArgs();
                arg.tableName = "BidQuotations";
                arg.oldIndex = e.OldIndex;
                arg.newIndex = e.NewIndex;
                arg.listChangedType = e.ListChangedType.ToString().Replace("System.ComponentModel.ListChangedType.","");
                DataTable dc = BidQuotations.ToTable();
                arg.jsonData = dc;

                OnListChangeEvent?.Invoke(arg);
            }
        }


        private void _OfrQuotations_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (this.OnListChangeEvent != null)
            {
                QuotationEventArgs arg = new QuotationEventArgs();
                arg.tableName = "OfrQuotations";
                arg.oldIndex = e.OldIndex;
                arg.newIndex = e.NewIndex;
                arg.listChangedType = e.ListChangedType.ToString().Replace("System.ComponentModel.ListChangedType.", "");
                DataTable dc = OfrQuotations.ToTable();
                arg.jsonData = dc;

                OnListChangeEvent?.Invoke(arg);
            }
        }


        private async Task GetSymbolInfo()
        {
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(this._SecurityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                    PriceHandler.Instance.RegisterCTPrice(this._SecurityID, Instance_OnCTPrice);


                    if (!string.IsNullOrEmpty(SecurityID))
                    {
                        Security = SecurityID;
                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.SecurityID);
                        if (bondsInfoPo != null)
                        {
                            SymbolName = bondsInfoPo.Short_Name;
                            RemainingTime = bondsInfoPo.RemaingingTime;
                            Symbol = bondsInfoPo.Short_Name;
                            MarketIndicator = bondsInfoPo.MarketIndicator;

                            if (CommonUtils.IsFutureByMarketIndicator(MarketIndicator))
                            {
                                //如果是期货，则隐藏显示选择项，由于值保存在收益率所以显示收益率字段  '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  0 - 显示原价
                                //DefaultPriceType = "0";
                                PriceTypeIndex = 0;
                                PirceTypeVisible = Visibility.Collapsed;
                                CN_Short_Name = string.Empty;
                                Coupon_Rate_Current = string.Empty;
                                CCDCValuationShow = string.Empty;
                                CCDCValuaDate = string.Empty;
                                ShowLastWorkDay = "";
                                LastWorkDay = "";
                                ShowSum = Visibility.Collapsed;
                                ShowFilterVisisble = Visibility.Collapsed;
                                //ShowText = $"{this.SecurityID}  {SymbolName}";
                            }
                            else if (MarketIndicator == "4")
                            {
                                string tmpDefaultPriceType = string.IsNullOrEmpty(bondsInfoPo.PriceType) ? "3" : bondsInfoPo.PriceType != "3" ? bondsInfoPo.PriceType : "3";
                                PriceTypeIndex = tmpDefaultPriceType == "1" || SecurityID.EndsWith(".SH") || SecurityID.EndsWith(".SZ") ? 1 : 0;
                                PirceTypeVisible = Visibility.Visible;
                                ShowSum = Visibility.Visible;
                                CN_Short_Name = string.IsNullOrEmpty(bondsInfoPo.CN_Short_Name) ? "" : bondsInfoPo.CN_Short_Name.Trim();
                                if (!string.IsNullOrEmpty(bondsInfoPo.Coupon_Rate_Current))
                                {
                                    Coupon_Rate_Current = bondsInfoPo.Coupon_Rate_Current.ToString() == "0" ? "" : $" 票面: {CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current.ToString())}%";
                                }
                                else
                                {
                                    Coupon_Rate_Current = "票面:--";
                                }

                                CCDCValuationShow = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.CCDCValuationShow.ToString();
                                CCDCValuaDate = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.CCDCValuaDate == null ? "" : bondsInfoPo.CCDCValua.CCDCValuaDate.ToString();
                                LastWorkDay = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.LastWorkDay == null ? "" : bondsInfoPo.CCDCValua.LastWorkDay.Replace("-", "").Replace("/", "");
                                if (!string.IsNullOrEmpty(CCDCValuaDate))
                                {
                                    if (!string.IsNullOrEmpty(LastWorkDay))
                                    {
                                        if (LastWorkDay != CCDCValuaDate.Replace("-", "").Replace("/", ""))
                                        {
                                            ShowLastWorkDay = "!";
                                        }
                                    }
                                }

                                PirceTypeVisible = Visibility.Visible;
                                ShowSum = Visibility.Visible;
                                ShowFilterVisisble = Visibility.Visible;
                                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                {
                                    ShowExeRadioVisible = Visibility.Visible;
                                }
                                else
                                {
                                    ShowExeRadioVisible = Visibility.Collapsed;
                                }
                                //ShowText = $"{this.SecurityID}  {SymbolName}  {RemainingTime}  {Coupon_Rate_Current}  {CCDCValuationShow}  {CN_Short_Name}";
                            }
                            else
                            {
                                //IRS
                                PirceTypeVisible = Visibility.Collapsed;
                                ShowSum = Visibility.Collapsed;
                                ShowFilterVisisble = Visibility.Collapsed;
                                ShowCCDC = Visibility.Collapsed;
                                PirceTypeVisible = Visibility.Collapsed;
                                PriceTypeIndex = 2;
                            }

                            //获取最优价
                            LastExecutionProcesser.Processer.AddGetLastExecutionQueue(SecurityID);
                        }
                    }
                }));
            });

            await GetIsFavorite(this.SecurityID);
        }

        private void onloadData(bool flag)
        {
            if (this.Symbol == null)
            { 
                return; 
            }
            if (this.Symbol.Length > 0)
            {
                return;
            }
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.SecurityID);
            Security = this.SecurityID;
            if (bondsInfoPo != null)
            {
                Symbol = bondsInfoPo.Short_Name;
                Security = bondsInfoPo.Bond_ID;
                SymbolName = bondsInfoPo.Short_Name;
                RemainingTime = bondsInfoPo.RemaingingTime;
                MarketIndicator = bondsInfoPo.MarketIndicator;

                if (CommonUtils.IsFutureByMarketIndicator(MarketIndicator))
                {
                    CN_Short_Name = string.Empty;
                    Coupon_Rate_Current = string.Empty;
                    CCDCValuationShow = string.Empty;
                    CCDCValuaDate = string.Empty;
                    ShowLastWorkDay = "";
                    LastWorkDay = "";
                    ShowFilterVisisble = Visibility.Collapsed;
                    ShowCCDC = Visibility.Collapsed;
                    //ShowText = $"{this.SecurityID}  {SymbolName}";
                }
                else if (MarketIndicator == "4")
                {
                    CN_Short_Name = string.IsNullOrEmpty(bondsInfoPo.CN_Short_Name) ? "" : bondsInfoPo.CN_Short_Name.Trim();
                    if (!string.IsNullOrEmpty(bondsInfoPo.Coupon_Rate_Current))
                    {
                        Coupon_Rate_Current = bondsInfoPo.Coupon_Rate_Current.ToString() == "0" ? "" : $" 票面: {CommonUtils.FormatNum(bondsInfoPo.Coupon_Rate_Current.ToString())}%";
                    }
                    else
                    {
                        Coupon_Rate_Current = "票面:--";
                    }
                    CCDCValuationShow = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.CCDCValuationShow.ToString();
                    CCDCValuaDate = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.CCDCValuaDate == null ? "" : bondsInfoPo.CCDCValua.CCDCValuaDate.ToString();
                    //ShowText = $"{this.SecurityID}  {SymbolName}  {RemainingTime}  {Coupon_Rate_Current}  {CCDCValuationShow}  {CN_Short_Name}";
                    LastWorkDay = bondsInfoPo.CCDCValua == null ? "" : bondsInfoPo.CCDCValua.LastWorkDay == null ? "" : bondsInfoPo.CCDCValua.LastWorkDay.Replace("-", "").Replace("/", "");
                    if (!string.IsNullOrEmpty(CCDCValuaDate))
                    {
                        if (!string.IsNullOrEmpty(LastWorkDay))
                        {
                            if (LastWorkDay != CCDCValuaDate.Replace("-", "").Replace("/", ""))
                            {
                                ShowLastWorkDay = "!";
                            }
                        }
                    }

                    PirceTypeVisible = Visibility.Visible;
                    ShowSum = Visibility.Visible;
                    ShowFilterVisisble = Visibility.Visible;
                }
                else 
                {
                    //IRS
                    PirceTypeVisible = Visibility.Collapsed;
                    ShowSum = Visibility.Collapsed;
                    ShowFilterVisisble = Visibility.Collapsed;
                    ShowCCDC = Visibility.Collapsed;
                    PirceTypeVisible = Visibility.Collapsed;
                    PriceTypeIndex = 2;
                }
            }

            // 重刷行情，为了解决初始行情是灰色问题
            if (!string.IsNullOrEmpty(Security))
            {
                PriceHandler.Instance.UnregisterCTPrice(SecurityID, Instance_OnCTPrice);
                PriceHandler.Instance.RegisterCTPrice(SecurityID, Instance_OnCTPrice);
                
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("QuotationListViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearData(false);
                    this.Init();

                    if (string.IsNullOrEmpty(this.SecurityID) == false)
                    {
                        PriceHandler.Instance.RegisterCTPrice(this.SecurityID, Instance_OnCTPrice);
                    }
                }));
            }
        }

        ~QuotationListViewModel()
        {
            LogHelper.Debug("析构QuotationListViewModel ");
            Dispose();
        }

        new public void Dispose()
        {
            LastExecutionProcesser.Processer.OnLastExecutions -= Instance_OnOwnExecutions;
            //ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew -= Instance_OnOwnExecutions;
            StopRefreshQueue = true;

            if (RefreshDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose RefreshDataThread:" + this._SecurityID);
                _autoResetEvent.Set();

                if (RefreshDataThread.IsAlive)
                {
                    RefreshDataThread.Abort();
                }

                RefreshDataThread = null;
            }

            if (CalcDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose CalcDataThread:" + this._SecurityID);
                
                if (CalcDataThread.IsAlive)
                {
                    CalcDataThread.Abort();
                }

                CalcDataThread.Abort();
                CalcDataThread = null;
            }


            BindingOperations.DisableCollectionSynchronization(BidQuotations);

            BindingOperations.DisableCollectionSynchronization(OfrQuotations);

            BindingOperations.DisableCollectionSynchronization(BidQuotations_Frozen);

            BindingOperations.DisableCollectionSynchronization(OfrQuotations_Frozen);

            BindingOperations.DisableCollectionSynchronization(BidIncompleteQuotations);

            BindingOperations.DisableCollectionSynchronization(OfrIncompleteQuotations);

            _autoResetEvent.Set();

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
            LogHelper.Info("QuotationListViewModel Dispose:" + this._SecurityID);
            this.ClearData(true);

            base.Dispose(); 
        }

        private volatile bool _isDetailDtChange = false;

        private void DetailDT_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            lock (_adjustFlagTypeLock)
            {
                _isDetailDtChange = true;
                // AdjustFlagType();
            }
        }

        private void DetailDT_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            lock (_adjustFlagTypeLock)
            {
                _isDetailDtChange = true;
                //AdjustFlagType();
            }
        }

        private void AdjustFlagType()
        {
            try
            {
                // 角标标志
                lock (this.bidLockObj)
                {
                    foreach (DataRowView drv in this.BidQuotations)
                    {
                        string orginalFlag = drv[CTPriceControler.FLAG_TYPE_FIELD] == null ? string.Empty : drv[CTPriceControler.FLAG_TYPE_FIELD].ToString();
                        string currentFlag = GenerateFlagType(drv);
                        if (orginalFlag != currentFlag)
                        {
                            drv[CTPriceControler.FLAG_TYPE_FIELD] = currentFlag;
                        }
                    }
                }
                lock (this.ofrLockObj)
                {
                    foreach (DataRowView drv in this.OfrQuotations)
                    {
                        string orginalFlag = drv[CTPriceControler.FLAG_TYPE_FIELD] == null ? string.Empty : drv[CTPriceControler.FLAG_TYPE_FIELD].ToString();
                        string currentFlag = GenerateFlagType(drv);
                        if (orginalFlag != currentFlag)
                        {
                            drv[CTPriceControler.FLAG_TYPE_FIELD] = currentFlag;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                LogHandler.Error(e);
            }
        }

        /// <summary>
        /// 清空界面数据
        /// </summary>
        /// 
        public void ClearData(bool clearSymbol = false)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(this._SecurityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                LogHelper.Debug("QuotationListViewModel ClearData UnregisterCTPrice:" + this._SecurityID);
                //if (!string.IsNullOrEmpty(this.SecurityID))
                try
                {
                    PriceHandler.Instance.UnregisterCTPrice(this.SecurityID, Instance_OnCTPrice);
                }
                catch (Exception e)
                {

                    LogHelper.Error(e);
                }
                    
                if (clearSymbol)
                {
                    Symbol = string.Empty;
                    //_SecurityID = FullName = string.Empty;
                    PriceModel = new PriceModel();
                    SymbolName = string.Empty;
                    RemainingTime = string.Empty;
                    
                    CN_Short_Name = string.Empty;
                    Coupon_Rate_Current = string.Empty;
                    CCDCValuationShow = string.Empty;
                    CCDCValuaDate = string.Empty;
                    LastWorkDay = string.Empty;
                    ShowLastWorkDay = string.Empty;
                    ShowText = string.Empty;
                    ShowCCDC = Visibility.Visible;
                }

                InitPriceModel();

                #region  Release Object

                ShowImcompletePanel = Visibility.Collapsed;
				ShowSum = Visibility.Collapsed;
                // to do ...
                //HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
                EmsOrderProcesser.Processer.DetailDT.TableNewRow -= DetailDT_TableNewRow;
                EmsOrderProcesser.Processer.DetailDT.RowDeleted -= DetailDT_RowDeleted;

            
                 //_autoResetEvent.Set();

                lock (this.baseLockObj)
                {
                    if (_BaseDV != null)
                    {
                        _BaseDV.Table.Rows.Clear();
                    }
                }

                lock (this.bidLockObj)
                {
                    if (_BidQuotations != null)
                    {
                        _BidQuotations.Table.Rows.Clear();
                    }
                }
                lock (this.ofrLockObj)
                {
                    if (_OfrQuotations != null)
                    {
                        _OfrQuotations.Table.Rows.Clear();
                    }
                }

                lock (this.bidLockObj_Frozen)
                {
                    if (_BidQuotations_Frozen != null)
                    {
                        _BidQuotations_Frozen.Table.Rows.Clear();
                    }
                }
                lock (this.ofrLockObj_Frozen)
                {
                    if (_OfrQuotations_Frozen != null)
                    {
                        _OfrQuotations_Frozen.Table.Rows.Clear();
                    }
                }

                lock (this.bidIncompleteLockObj)
                {
                    if (_BidIncompleteQuotations != null)
                    {
                        _BidIncompleteQuotations.Table.Rows.Clear();
                    }
                }

                lock (this.ofrIncompleteLockObj)
                {
                    if (_OfrIncompleteQuotations != null)
                    {
                        _OfrIncompleteQuotations.Table.Rows.Clear();
                    }
                }

                #endregion
                // to do ...

                if (!clearSymbol)
                {
                    PriceModel = new PriceModel();
                }
            }));
        }

        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, null);
            if (isAllow)
            {
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                if (isNotAllow)
                {
                    ShowOrderBtn = Visibility.Collapsed;
                }
                else
                {
                    ShowOrderBtn = Visibility.Visible;
                }
            }
            else
            {
                ShowOrderBtn = Visibility.Collapsed;
            }
        }


        public void Init()
        {
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            BindingOperations.EnableCollectionSynchronization(BidQuotations, bidLockObj);

            BindingOperations.EnableCollectionSynchronization(OfrQuotations, ofrLockObj);

            BindingOperations.EnableCollectionSynchronization(BidQuotations_Frozen, bidLockObj_Frozen);

            BindingOperations.EnableCollectionSynchronization(OfrQuotations_Frozen, ofrLockObj_Frozen);

            BindingOperations.EnableCollectionSynchronization(BidIncompleteQuotations, bidIncompleteLockObj);

            BindingOperations.EnableCollectionSynchronization(OfrIncompleteQuotations, ofrIncompleteLockObj);

            EmsOrderProcesser.Processer.DetailDT.TableNewRow += DetailDT_TableNewRow;
            EmsOrderProcesser.Processer.DetailDT.RowChanged += DetailDT_RowDeleted;
            EmsOrderProcesser.Processer.DetailDT.RowDeleted += DetailDT_RowDeleted;

            //#region 测试数据
            //GenerateTestData();
            //return;
            //#endregion
            InitPriceModel();
        }

        private void InitPriceModel()
        {
            Application.Current.Dispatcher.Invoke((Action)(() => 
            {
                // Bid 背景色
                _priceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                _priceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
                //_priceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                // Ofr 背景色
                _priceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                _priceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;
                //_priceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                // Bp 背景色
                _priceModel.UpSideBdBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121A19"));

                // Bid 左上角
                _priceModel.BidArrowVisibilly = Visibility.Collapsed;
                _priceModel.BidArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30FF7D00"));
                _priceModel.BidArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                _priceModel.BidArrowToolTxt = "";

                // Ofr 右上角
                _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                _priceModel.OfrArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30009AFF"));
                _priceModel.OfrArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                _priceModel.OfrArrowToolTxt = "";

                _priceModel.BestBidAmount = "";
                _priceModel.BestBid = "";
                _priceModel.BestBidNetPrice = "";
                _priceModel.BestOfrAmount = "";
                _priceModel.BestOfr = "";
                _priceModel.BestOfrNetPrice = "";
                _priceModel.BestPriceBP = "";
                _priceModel.BestPriceDisplayBP = "";
                _priceModel.BestNetPriceBP = "";
                _priceModel.BestNetPriceDisplayBP = "";
                _priceModel.BestBidSettler = "";
                _priceModel.BestOfrSettler = "";
                _priceModel.BestBidSettlerYuan = "";
                _priceModel.BestOfrSettlerYuan = "";

                LastExecPrice = "";
                LastExecPriceBP = "";
                LastExecSideShow = "";
                LastExecSource = "";
                LastExecNetPrice = "";
                LastExecNetPriceSpread = "";

                ShowImcompletePanel = Visibility.Collapsed;
            }));
        }


        private void HomePageDataHandler_OnFavoritesChanged(FavoriteTypeEnum favoriteTypeEnum, string groupID, string securityID)
        {
            if (favoriteTypeEnum == FavoriteTypeEnum.AddBond
                || favoriteTypeEnum == FavoriteTypeEnum.DelBond)
            {
                if (this.SecurityID == securityID)
                {
                    IsFavorite = HomePageDataHandler.IsFavorite(this.SecurityID);
                }
            }
            else if (favoriteTypeEnum == FavoriteTypeEnum.DelGroup
                || favoriteTypeEnum == FavoriteTypeEnum.BatchImport)
            {
                IsFavorite = HomePageDataHandler.IsFavorite(this.SecurityID);
            }
        }

        private ConcurrentStack<MarketDataSnapshotFullRefresh> _ctPriceStack = new ConcurrentStack<MarketDataSnapshotFullRefresh>();
        private ManualResetEventSlim _autoResetEvent = new ManualResetEventSlim(false);
        //private object _queueLock = new object();
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            //string content = Ats.Foundation.Message.MsgHelper.Serializer(obj);
            //LogHelper.Info($"QuotationListViewModel onCTPrice:{content}");
            try
            {
                if (obj != null && obj.SecurityID == this.SecurityID)
                {
                    if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                    {
                        LogHelper.Debug($"QuotationListViewModel onCTPrice:{obj.SecurityID}");
                    }

                    //lock (_queueLock)
                    //{
                        _ctPriceStack.Push(obj);
                        _autoResetEvent.Set();
                    //}
                    //lock (cacheStackLock)
                    //{
                    //    if (_CacheStack != null)
                    //    {
                    //        _CacheStack.Push(obj);
                    //    }
                    //}
                }
            }
            catch(Exception e)
            {
                LogHandler.Info($"ERROR Json:{JsonConvert.SerializeObject(obj)}");
                LogHandler.Error(e);
            }
        }



        private void UpdatePrice2(
            DataTable bidNormalDt, DataTable bidCDHDt, DataTable bidIncompleteDt,
            DataTable ofrNormalDt, DataTable ofrCDHDt, DataTable ofrIncompleteDt,
            DataTable bidSORDt, DataTable ofrSORDt)
        {
            try
            {
                // 构建bid数据
                lock (bidLockObj)
                {
                    ////开始Stopwatch
                    //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    //stopwatch.Start();

                    UpdateBidOfrView_Normal("0", bidNormalDt);
                    
                    UpdateBidOfrView_CDH("0", bidCDHDt, CTPriceControler.CDH);

                    UpdateBidOfrView_CDH("0", bidSORDt, CTPriceControler.SOR);

                    ////停止Stopwatch
                    //stopwatch.Stop();
                    //// 输出所消耗的时间，以毫秒为单位
                    //LogHelper.Info($"程序执行 Bid  UpdateBidOfrView_Normal UpdateBidOfrView_CDH 时间: {stopwatch.ElapsedMilliseconds} 毫秒");
                }

                // 构建ofr数据
                lock (ofrLockObj)
                {
                    ////开始Stopwatch
                    //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    //stopwatch.Start();
                    UpdateBidOfrView_Normal("1", ofrNormalDt);
                    UpdateBidOfrView_CDH("1", ofrCDHDt, CTPriceControler.CDH);
                    UpdateBidOfrView_CDH("1", ofrSORDt, CTPriceControler.SOR);
                    ////停止Stopwatch
                    //stopwatch.Stop();
                    //输出所消耗的时间，以毫秒为单位
                    //LogHelper.Info($"程序执行 Ofr UpdateBidOfrView_Normal UpdateBidOfrView_CDH 时间: {stopwatch.ElapsedMilliseconds} 毫秒");
                }

                // 构建bid不完整数据
                lock (bidIncompleteLockObj)
                {
                    UpdateBidOfrView_Incomplete("0", bidIncompleteDt);
                }

                // 构建ofr不完整数据
                lock (ofrIncompleteLockObj)
                {
                    UpdateBidOfrView_Incomplete("1", ofrIncompleteDt);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                ////开始Stopwatch
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();
                //// 模拟需要测量的程序运行时间

                AdjustData(this.BidQuotations, this.BidQuotations_Frozen, bidLockObj, bidLockObj_Frozen);
                AdjustData(this.OfrQuotations, this.OfrQuotations_Frozen, ofrLockObj, ofrLockObj_Frozen);

                ////停止Stopwatch
                //stopwatch.Stop();
                //// 输出所消耗的时间，以毫秒为单位
                //LogHelper.Info($"程序执行 AdjustData 时间: {stopwatch.ElapsedMilliseconds} 毫秒");

                if (this.BidIncompleteQuotations.Count > 0 ||
                    this.OfrIncompleteQuotations.Count > 0)
                {
                    ShowImcompletePanel = Visibility.Visible;
                }
                else
                {
                    ShowImcompletePanel = Visibility.Collapsed;
                }
            }
        }


        private void UpdateData(MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {

            try
            //if (marketDataSnapshotFullRefresh != null)// || _NeedUpdate == true)
            {
                DataTable tempBaseDt = null;
                lock (this.BaseDV.Table)
                {
                    try
                    {

                        if (marketDataSnapshotFullRefresh != null)
                        {
                            ////开始Stopwatch
                            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                            //stopwatch.Start();

                            CTPriceControler.UpdateDataToBaseDV(this.BaseDV, marketDataSnapshotFullRefresh);

                            ////停止Stopwatch
                            //stopwatch.Stop();
                            //// 输出所消耗的时间，以毫秒为单位
                            //LogHelper.Info($"程序执行 UpdateDataToBaseDV 时间: {stopwatch.ElapsedMilliseconds} 毫秒");

                            if (string.IsNullOrEmpty(this.CCDCValuationShow))
                                UpdateCCDCValua(marketDataSnapshotFullRefresh);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Info($"ERROR Json:{JsonConvert.SerializeObject(marketDataSnapshotFullRefresh)}");
                        LogHandler.Error(ex);
                    }


                    try
                    {
                        DataTable tempdt = this.BaseDV.ToTable();
                        tempBaseDt = tempdt.Copy();
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }

                }

                if (tempBaseDt == null)
                {
                    LogHandler.Info("tempBaseDt == null");
                    return;
                }

                DataTable bidNormalDt = null;
                DataTable bidCDHDt = null;
                DataTable bidSORDt = null;
                DataTable ofrNormalDt = null;
                DataTable ofrCDHDt = null;
                DataTable ofrSORDt = null;
                DataTable bidIncompleteDt = null;
                DataTable ofrIncompleteDt = null;

                #region DataTable复制
                string filter = string.Empty;
                // 添加和更新(非CDH数据)-bid QDM2
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}','{CTPriceControler.SOR}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}','{CTPriceControler.SOR}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidNormalDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(CDH数据)-bid
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidCDHDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(SOR数据)-bid
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.SOR}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.SOR}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidSORDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(非CDH数据)-ofr
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}','{CTPriceControler.SOR}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}','{CTPriceControler.SOR}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrNormalDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(CDH数据)-ofr
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrCDHDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(CDH数据)-ofr
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.SOR}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.SOR}')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrSORDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(不完整数据)-bid
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.ISINCOMPLETE_FIELD} <> '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '0' and ({CTPriceControler.ISINCOMPLETE_FIELD} <> '0' and {CTPriceControler.ISINNER_FIELD}='0')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidIncompleteDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(不完整数据)-ofr
                if (LocalEnv.ConfInfo.IsShowDRP == "0")
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.ISINCOMPLETE_FIELD} <> '0'";
                else
                    filter = $"{CTPriceControler.SIDE_FIELD} = '1' and ({CTPriceControler.ISINCOMPLETE_FIELD} <> '0' and {CTPriceControler.ISINNER_FIELD}='0')";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrIncompleteDt = CTPriceControler.getDataTable(tempBaseDt, filter);
                #endregion

                try
                {
                    if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                    {
                        LogHelper.Info("Into UpdateData, security:" + (marketDataSnapshotFullRefresh == null ? string.Empty : marketDataSnapshotFullRefresh.SecurityID));
                    }


                    UpdatePrice2(bidNormalDt, bidCDHDt, bidIncompleteDt,
                                ofrNormalDt, ofrCDHDt, ofrIncompleteDt,
                                bidSORDt, ofrSORDt);

                    //CommonUtils.WriteConvertObjectToJsonToText(marketDataSnapshotFullRefresh.NoMDEntries, $"{marketDataSnapshotFullRefresh.SecurityID}-NoMDEntries.txt");
                    //CommonUtils.WriteConvertObjectToJsonToText(tempBaseDt, "tempBaseDt.txt");
                    //CommonUtils.WriteConvertObjectToJsonToText(BidQuotations.ToTable(), "BidQuotations.txt");
                    //CommonUtils.WriteConvertObjectToJsonToText(OfrQuotations.ToTable(), "OfrQuotations.txt");
                    if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                    {
                        LogHelper.Info("UpdateData end, security:" + (marketDataSnapshotFullRefresh == null ? string.Empty : marketDataSnapshotFullRefresh.SecurityID));
                    }


                    //if (_isDetailDtChange)
                    //{
                    //    AdjustFlagType();
                    //    _isDetailDtChange = false;
                    //}

                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                finally
                {
                    _UIProcessing = false;
                }
            }
            catch (Exception e)
            {
                _UIProcessing = false;
            }
        }

        /// <summary>
        ///  更新中债估值
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateCCDCValua(MarketDataSnapshotFullRefresh obj)
        {
            Task.Run(() => {
                // 更新中债估值
                Model.Base.CCDCValuaModel srcccdc = CommonUtils.GetCCDCValua(obj.SecurityID);
                if (srcccdc == null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        srcccdc = CommonUtils.GenerateCCDCValue(obj);
                        this.CCDCValuaDate = srcccdc.CCDCValuaDate;
                        if (!string.IsNullOrEmpty(srcccdc.CCDCValuaDate))
                        {
                            this.CCDCValuationShow = srcccdc.CCDCValuationShow;
                            this.LastWorkDay = srcccdc.LastWorkDay;
                            if (!string.IsNullOrEmpty(LastWorkDay))
                            {
                                if (LastWorkDay.Replace("-", "").Replace("/", "") != CCDCValuaDate.Replace("-", "").Replace("/", ""))
                                {
                                    ShowLastWorkDay = "!";
                                }
                            }

                            CommonUtils.SetCCDCValua(obj.SecurityID, srcccdc);
                        }
                    }));
                }
                else
                {
                    //要判断中债估值的日期，尤其是隔日 to do...
                }
            });
        }


        public bool StopRefreshQueue = false;

        private void RefreshQueue()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshQueue)
                    {
                        break;
                    }

                    MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh = null;

                    _autoResetEvent.Wait();

                    if (_ctPriceStack.TryPop(out marketDataSnapshotFullRefresh))
                    {
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHelper.Info("Into refreshQueue, security:" + marketDataSnapshotFullRefresh.SecurityID);
                        }

                        if (marketDataSnapshotFullRefresh == null) // 生产结束的标记
                            return;

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            // 处理行情数据
                            UpdateData(marketDataSnapshotFullRefresh);
                        }));

                        if (!_ctPriceStack.IsEmpty)
                        {
                            LogHelper.Info("Into refreshQueue clear _ctPriceQueue, security:" + marketDataSnapshotFullRefresh.SecurityID);
                            _ctPriceStack.Clear();
                        }
                    }

                    // 重置事件
                    _autoResetEvent.Reset();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshQueue Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshQueue exception:", ex);
                }
            }
        }

        private object _adjustFlagTypeLock = new object();
        private void RefreshAdjustFlagType()
        {
            while (true)
            {
                if (StopRefreshQueue)
                {
                    break;
                }

                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {
                            lock (_adjustFlagTypeLock)
                            {
                                if (_isDetailDtChange)
                                {
                                    AdjustFlagType();
                                    _isDetailDtChange = false;
                                }
                            }
                        }
                        catch (ThreadAbortException ex)
                        {
                            LogHelper.Debug("RefreshAdjustFlagType Normal Abort:", ex);
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }
                    }));
                }
                try
                {
                    Thread.Sleep(200);
                }
                catch (Exception ex)
                { }
            }
        }
        
        private DateTime LastUpdateTime = DateTime.MinValue;
        private volatile bool _UIProcessing = false;

        /// <summary>
        /// 使用基础数据更新的bidQuotations和ofrQuotations(非CDH数据)
        /// </summary>
        /// <param name="side">0:bid,1:ofr</param>
        private void UpdateBidOfrView_Normal(string side, DataTable tempDt)
        {

            // 选择数据源
            DataView dv = null;

            if (side == "1")
            {
                dv = this.OfrQuotations;
            }
            else
            {
                dv = this.BidQuotations;
            }

            dv.Table.BeginLoadData();
            //设置删除标志
            DataRow[] drs = dv.Table.Select($"{CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}', '{CTPriceControler.SOR}')", $"{CTPriceControler.VENUE_TYPE_GW_FIELD}");
            foreach (DataRow dr in drs)
            {
                dr[CTPriceControler.DELETE_FLAG_FIELD] = "1";
            }


            if (tempDt != null && tempDt.Rows.Count > 0)
            {
                foreach (DataRow dr in tempDt.Rows)
                {
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);

                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    price = CommonUtils.FormatPrice(price);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                    if (IsSheBond == "1")
                        netPrice = CommonUtils.ConvertToDecimal(CommonUtils.formatString(CommonUtils.ConvertToString(netPrice), 3));

                    decimal qty = CommonUtils.ConvertToDecimal(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settlType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 询问
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();

                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();

                    string traderName = dr[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : dr[CTPriceControler.TRADERNAME_FIELD].ToString();

                    string transactTime = dr[CTPriceControler.TRANSACT_TIME_FILED] == null ? string.Empty : dr[CTPriceControler.TRANSACT_TIME_FILED].ToString();
                    string deliveryType = dr[CTPriceControler.DELIVERY_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
                    string clearingMethod = dr[CTPriceControler.CLEARING_METHOD_FILED] == null ? string.Empty : dr[CTPriceControler.CLEARING_METHOD_FILED].ToString();
                    string quoteType = dr[CTPriceControler.QUOTE_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.QUOTE_TYPE_FILED].ToString();

                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();
                    string quoteStatus = dr[CTPriceControler.QUOTE_STATUS_FIELD] == null ? string.Empty : dr[CTPriceControler.QUOTE_STATUS_FIELD].ToString();

                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];

                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();

                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();

                    //2-收益率反算净价  4-净价反算收益率  0-原始值 3-全价反算收益率和净价 5-利差反算收益率和净价
                    if (string.IsNullOrEmpty(calFlag))
                    {
                        calFlag = "0";
                    }

                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);
                    yield = CommonUtils.FormatPrice(yield);

                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    string isinnerincomplete = dr[CTPriceControler.IsInnerInComplete_Field].ToString();
                    string exerciseflag = dr[CTPriceControler.ExerciseFlag_FIELD].ToString() ?? "1"; // 0=行权收益率,1或空=到期收益率 
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    string orginalPrice = string.Empty;
                    if (marketIndicator != "5")
                    {
                        orginalPrice = price;

                        // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                        // exerciseFlag 0=行权收益率,1或空=到期收益率
                        //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价

                        if (calFlag == "2")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (calFlag == "4")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            orginalPrice = CommonUtils.ConvertToString(netPrice);
                        }

                        if (calFlag == "3")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }

                        if (priceType == "1")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            calFlag = "4";
                            orginalPrice = CommonUtils.ConvertToString(netPrice);
                        }

                        if (priceType == "3")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (priceType == "2")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }

                        if (calFlag == "6")
                        {
                            calTypeFieldTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                            calTypeNetPriceTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                        }
                    }
                    else
                    {
                        calFlag = "0";
                        priceType = "3";
                    }
                    string yieldToExecution = dr[CTPriceControler.YieldToExecution_Field] == null ? "" : dr[CTPriceControler.YieldToExecution_Field] + "";
                    string bondtype = dr[CTPriceControler.BOND_TYPE_FIELD] == null ? "" : dr[CTPriceControler.BOND_TYPE_FIELD] + "";
                    CTPriceControler.FeedDataNew(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settlType_yuan, firmFlag,
                            text, price, netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader,
                            isMe, isinnerincomplete, traderName, marketIndicator, transactTime, deliveryType, clearingMethod, quoteType, partyName, quoteStatus, isInner, partyID, calFlag, yield, priceType,
                            calTypeFieldTip, calTypeNetPriceTip, "", "", exerciseflag, yieldToExecution, orginalPrice, bondtype);
                }
            }

            // 删除DeleteFlag为0的记录
            drs = dv.Table.Select($"{CTPriceControler.DELETE_FLAG_FIELD} = '1'", $"{CTPriceControler.DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }

            dv.Table.EndLoadData();
            dv.Table.AcceptChanges();
        }

        /// <summary>
        /// 使用基础数据更新的bidIncompleteQuotations和ofrIncompleteQuotations(不完整数据)
        /// </summary>
        /// <param name="side">0:bid,1:ofr</param>
        private void UpdateBidOfrView_Incomplete(string side, DataTable tempDt)
        {
            // 选择数据源
            DataView dv = this.BidIncompleteQuotations;
            if (side == "1")
            {
                dv = this.OfrIncompleteQuotations;
            }

            dv.Table.BeginLoadData();
            //设置删除标志
            foreach (DataRow dr in dv.Table.Rows)
            {
                dr[CTPriceControler.DELETE_FLAG_FIELD] = "1";
            }


            if (tempDt != null && tempDt.Rows.Count > 0)
            {
                //int index = tempDt.Rows.Count > 2 ? 2 : tempDt.Rows.Count;
                int index = tempDt.Rows.Count;
                for (int i = 0; i < index; i++)
                {
                    DataRow dr = tempDt.Rows[i];
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    // 主键
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    if (venueTypeGW == "CDH" || venueTypeGW == "SOR")
                    {
                        //CDH订单需要合并数据
                        key = $"{venueTypeGW}#{venues}#{price}";
                    }

                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();

                    price = CommonUtils.FormatPrice(price);
                    string netPrice = CommonUtils.ConvertToString(dr[CTPriceControler.NET_PRICE_FIELD]);
                    netPrice = CommonUtils.FormatPrice(netPrice);
                    decimal qty = CommonUtils.ConvertToDecimal(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settleType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();

                    string partySubName = dr[CTPriceControler.CUSTOMER_SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.CUSTOMER_SOURCE_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();

                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();

                    string traderName = dr[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : dr[CTPriceControler.TRADERNAME_FIELD].ToString();

                    string transactTime = dr[CTPriceControler.TRANSACT_TIME_FILED] == null ? string.Empty : dr[CTPriceControler.TRANSACT_TIME_FILED].ToString();

                    string deliveryType = dr[CTPriceControler.DELIVERY_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
                    string clearingMethod = dr[CTPriceControler.CLEARING_METHOD_FILED] == null ? string.Empty : dr[CTPriceControler.CLEARING_METHOD_FILED].ToString();
                    string quoteType = dr[CTPriceControler.QUOTE_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.QUOTE_TYPE_FILED].ToString();

                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];

                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();
                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);
                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    string exerciseflag = dr[CTPriceControler.ExerciseFlag_FIELD].ToString() ?? "1"; // 0=行权收益率,1或空=到期收益率 
                    // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    // exerciseFlag 0=行权收益率,1或空=到期收益率
                    //calFlag 2-收益率反算净价  4-净价反算收益率 0-原始值 3-全价反算收益率和净价 5-利差反算收益率和净价
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    string orginalPrice = price;
                    if (marketIndicator != "5")
                    {
                        // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                        // exerciseFlag 0=行权收益率,1或空=到期收益率
                        //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价

                        if (calFlag == "2")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (calFlag == "4")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            orginalPrice = netPrice;
                        }

                        if (calFlag == "3")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }

                        if (priceType == "1")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            calFlag = "4";
                            orginalPrice = netPrice;
                        }

                        if (priceType == "3")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (priceType == "2")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }
                    }

                    string yieldToExecution = dr[CTPriceControler.YieldToExecution_Field] == null ? "" : dr[CTPriceControler.YieldToExecution_Field] + "";
                    string bondtype = dr[CTPriceControler.BOND_TYPE_FIELD] == null ? "" : dr[CTPriceControler.BOND_TYPE_FIELD] + "";
                    CTPriceControler.FeedData_CDH_Incomplete(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price, netPrice, qty, time,
                       mdPriceLevel, bargainFlag, wholeFlag, institutionName, showTrader, isMe, orginalPrice, partyName, partySubName, isInner, partyID, calFlag, yield, priceType, calTypeFieldTip, calTypeNetPriceTip, exerciseflag,
                       yieldToExecution, bondtype);
                }

                tempDt.Clear();
                tempDt.Dispose();
                tempDt = null;
            }

            // 删除DeleteFlag为0的记录
            DataRow[] drs = dv.Table.Select($"{CTPriceControler.DELETE_FLAG_FIELD} = '1'", $"{CTPriceControler.DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }

            dv.Table.EndLoadData();
            dv.Table.AcceptChanges();
        }

        /// <summary>
        /// 使用基础数据更新的bidQuotations和ofrQuotations(CDH数据)
        /// </summary>
        /// <param name="side">0:bid,1:ofr</param>
        private void UpdateBidOfrView_CDH(string side, DataTable tempDt, string venue)
        {
            // 选择数据源
            DataView dv = this.BidQuotations;
            if (side == "1")
            {
                dv = this.OfrQuotations;
            }

            dv.Table.BeginLoadData();

            //设置删除标志
            DataRow[] drs = dv.Table.Select($"{CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{venue}')", $"{CTPriceControler.VENUE_TYPE_GW_FIELD}");
            foreach (DataRow dr in drs)
            {
                dr[CTPriceControler.DELETE_FLAG_FIELD] = "1";
            }

            if (tempDt != null && tempDt.Rows.Count > 0)
            {
                foreach (DataRow dr in tempDt.Rows)
                {
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());

                    if (IsSheBond == "1")
                        netPrice = CommonUtils.ConvertToDecimal(CommonUtils.formatString(CommonUtils.ConvertToString(netPrice), 3));
                   
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.CUSTOMER_SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.CUSTOMER_SOURCE_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 主键
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    if (venueTypeGW == "CDH" || venueTypeGW == "SOR")
                    {
                        //CDH订单需要合并数据
                        key = $"{venueTypeGW}#{venues}#{price}";
                    }
                    // 变动字段
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    decimal qty = CommonUtils.ConvertToDecimal(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settleType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();
                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();
                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];
                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();

                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();
                    //2-收益率反算净价  4-净价反算收益率  0-原始值 3-全价反算收益率和净价 5-利差反算收益率和净价
                    if (string.IsNullOrEmpty(calFlag))
                    {
                        calFlag = "0";
                    }

                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);

                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    string exerciseflag = dr[CTPriceControler.ExerciseFlag_FIELD].ToString() ?? "1"; // 0=行权收益率,1或空=到期收益率 
                    // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    // exerciseFlag 0=行权收益率,1或空=到期收益率
                    //calFlag 2-收益率反算净价  4-净价反算收益率 0-原始值  3-全价反算收益率和净价 5-利差反算收益率和净价
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    string orginalPrice = price;
                    if (marketIndicator != "5")
                    {
                        // priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                        // exerciseFlag 0=行权收益率,1或空=到期收益率
                        //calFlag 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价

                        if (calFlag == "2")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (calFlag == "4")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            orginalPrice = netPrice.ToString();
                        }

                        if (calFlag == "3")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }

                        if (priceType == "1")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 净价 {netPrice}";
                            calFlag = "4";
                            orginalPrice = netPrice.ToString();
                        }

                        if (priceType == "3")
                        {
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 收益率 {yield}%";
                        }

                        if (priceType == "2")
                        {
                            calTypeFieldTip = $"换算仅供参考 原始行情: 全价 {price}";
                            calTypeNetPriceTip = $"换算仅供参考 原始行情: 全价 {price}";
                        }
                        if (calFlag == "6")
                        {
                            calTypeFieldTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                            calTypeNetPriceTip = $"换算仅供参考，原始行情: 行权收益率 {price}";
                        }
                    }
                    else
                    {
                        calFlag = "0";
                        priceType = "3";
                    }
                    string yieldToExecution = dr[CTPriceControler.YieldToExecution_Field] == null ? "" : dr[CTPriceControler.YieldToExecution_Field] + "";
                    string bondtype = dr[CTPriceControler.BOND_TYPE_FIELD] == null ? "" : dr[CTPriceControler.BOND_TYPE_FIELD] + "";
                    CTPriceControler.FeedData_CDH_FilterIncomplete(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price, netPrice, qty, time, 
                        mdPriceLevel, bargainFlag, wholeFlag, institutionName, showTrader, isMe, orginalPrice, partyName, partySubName, isInner, partyID, calFlag, yield, priceType, calTypeFieldTip, calTypeNetPriceTip, exerciseflag, 
                        yieldToExecution, bondtype);
                }
            }

            // 删除DeleteFlag为0的记录
            drs = dv.Table.Select($"{CTPriceControler.DELETE_FLAG_FIELD} = '1'", $"{CTPriceControler.DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }

            dv.Table.EndLoadData();
            dv.Table.AcceptChanges();
        }

        /// <summary>
        /// 过滤数据
        /// </summary>
        /// <param name="filterDic">firmFlag, Venues, SettlType</param>
        public void FilterData(Dictionary<string, IList> filterDic)
        {
            _priceModel.BestBidAmount = "";
            _priceModel.BestBid = "";
            _priceModel.BestBidNetPrice = "";
            _priceModel.BestOfrAmount = "";
            _priceModel.BestOfr = "";
            _priceModel.BestOfrNetPrice = "";
            _priceModel.BestPriceBP = "";
            _priceModel.BestNetPriceBP = "";
            _priceModel.BestBidSettler = "";
            _priceModel.BestOfrSettler = "";
            _priceModel.UpSideBdBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121A19"));
            _priceModel.UpSideBgBrush = new SolidColorBrush(Colors.Transparent);
            // Bid 背景色
            _priceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
            _priceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
            // Ofr 背景色
            _priceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
            _priceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;

            StringBuilder filterBuffer = new StringBuilder();

            StringBuilder firmFlagFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("FirmFlag"))
            {
                IList firmTypeList = filterDic["FirmFlag"];
                if (firmTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem firmType in firmTypeList)
                    {
                        if (firmFlagFilterBuf.Length > 0)
                        {
                            firmFlagFilterBuf.Append(" or ");
                        }

                        if (firmType.Content.ToString() == "firm")
                        {
                            firmFlagFilterBuf.Append($"{CTPriceControler.FIRM_TYPE_FIELD} like '%1%'");
                        }
                        else if (firmType.Content.ToString() == "nonfirm")
                        {
                            firmFlagFilterBuf.Append($"{CTPriceControler.FIRM_TYPE_FIELD} like '%0%'");
                        }
                    }
                }
            }

            StringBuilder venuesFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("Venues"))
            {
                IList venuesList = filterDic["Venues"];
                if (venuesList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem venues in venuesList)
                    {
                        if (venuesFilterBuf.Length > 0)
                        {
                            venuesFilterBuf.Append(" or ");
                        }

                        switch (venues.Content.ToString())
                        {
                            case "IDB":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'IDB'");
                                break;
                            case "量化":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QUANT'");
                                break;
                            case "人工":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'MANUAL'");
                                break;
                            case "内部":
                                venuesFilterBuf.Append($"({CTPriceControler.SOURCE_FIELD} = 'IDB' or { CTPriceControler.SOURCE_FIELD} = 'QUANT' or {CTPriceControler.SOURCE_FIELD} = 'MANUAL')");
                                break;
                            case "X-Bond":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'ODM'");
                                break;
                            case "双边":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QDMESP'");
                                break;

                            case CommonConstants.SSE:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSE'");
                                break;
                            case CommonConstants.SSF:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSF'");
                                break;
                            case CommonConstants.SZE:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SZE'");
                                break;
                            case CommonConstants.SZF:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SZF'");
                                break;

                            case "经纪商":
                                venuesFilterBuf.Append($"({CTPriceControler.SOURCE_FIELD} = 'CNEX' or {CTPriceControler.SOURCE_FIELD} = 'TPSC' or {CTPriceControler.SOURCE_FIELD} = 'CCTB' or {CTPriceControler.SOURCE_FIELD} = 'CTTJ' or {CTPriceControler.SOURCE_FIELD} = 'UEDA' or {CTPriceControler.SOURCE_FIELD} = 'PTCN')");
                                break;
                            case CommonConstants.CNEX:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CNEX'");
                                break;
                            case CommonConstants.TPSC:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'TPSC'");
                                break;
                            case CommonConstants.CCTB:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CCTB'");
                                break;
                            case CommonConstants.CTTJ:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'CTTJ'");
                                break;
                            case CommonConstants.PTCN:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'PTCN'");
                                break;
                            case CommonConstants.UEDA:
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'UEDA'");
                                break;
                            case "QDMESP":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QDMESP'");
                                break;
                            case "ODM":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'ODM'");
                                break;
                            case "SZE":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SZE'");
                                break;
                            case "SZF":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SZF'");
                                break;
                            case "SSE":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSE'");
                                break;
                            case "SSF":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'SSF'");
                                break;
                        }
                    }
                }
            }

            StringBuilder settlTypeFilterBuf = new StringBuilder();
            if (filterDic != null && filterDic.ContainsKey("Settle"))
            {
                IList settlTypeList = filterDic["Settle"];
                if (settlTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                    {
                        if (settlTypeFilterBuf.Length > 0)
                        {
                            settlTypeFilterBuf.Append(" or ");
                        }

                        if (settlType.Content.ToString() == "+1")
                        {
                            settlTypeFilterBuf.Append($"({CTPriceControler.SETTL_TYPE_FIELD} like '%2%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%3%')");
                            SettleFilter = "+1";
                        }
                        else if (settlType.Content.ToString() == "+0")
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%1%'");
                            SettleFilter = "+0";
                        }
                        else if (settlType.Content.ToString() == "全")
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%'");
                            SettleFilter = "全";
                        }
                        else
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                            SettleFilter = "远";
                        }
                    }
                }
            }
            if (!filterDic.ContainsKey("Settle"))
            {
                SettleFilter = "全";
                settlTypeFilterBuf = new StringBuilder();
                if (filterDic != null && filterDic.ContainsKey("SettlType"))
                {
                    IList settlTypeList = filterDic["SettlType"];
                    if (settlTypeList.Count > 0)
                    {
                        foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                        {
                            if (settlTypeFilterBuf.Length > 0)
                            {
                                settlTypeFilterBuf.Append(" or ");
                            }

                            if (settlType.Content.ToString() == "T+1")
                            {
                                settlTypeFilterBuf.Append($"({CTPriceControler.SETTL_TYPE_FIELD} like '%2%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%3%')");
                                SettleFilter = "+1";
                            }
                            else if (settlType.Content.ToString() == "T+0")
                            {
                                settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%1%'");
                                SettleFilter = "+0";
                            }
                            else
                            {
                                settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                                SettleFilter = "远";
                            }
                        }
                    }
                }
            }

            if (firmFlagFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({firmFlagFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({firmFlagFilterBuf})");
                }
            }


            if (venuesFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({venuesFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({venuesFilterBuf})");
                }
            }

            if (settlTypeFilterBuf.Length > 0)
            {
                if (filterBuffer.Length > 0)
                {
                    filterBuffer.Append($" and ({settlTypeFilterBuf})");
                }
                else
                {
                    filterBuffer.Append($"({settlTypeFilterBuf})");
                }
            }

            this._rowFilterStr = filterBuffer.ToString();

            this.UpdateData(null);
        }

        private void AdjustData(DataView dv, DataView dv_Frozen, object normalLock, object frozenLock)
        {
            object[] itemArr = null;
            lock (normalLock)
            {
                int index = 0;
                decimal totalQty = 0;
                decimal px_last = decimal.MinValue;
                decimal netPrice_last = decimal.MinValue;
                string firmflag = string.Empty;
                if (dv.Count > 0)
                {

                    foreach (DataRowView drv in dv)
                    {
                        string firmflag_last = CommonUtils.ConvertToString(drv[CTPriceControler.FIRM_TYPE_FIELD].ToString());
                        if (this.IsShowNetPrice)
                        {
                            decimal netPrice = CommonUtils.ConvertToDecimal(drv[CTPriceControler.NET_PRICE_FIELD].ToString());
                            drv[CTPriceControler.HIDE_NETPRICE_FLAG_FIELD] = (netPrice != netPrice_last || firmflag != firmflag_last) ? "0" : "1";
                            netPrice_last = netPrice;
                        }
                        else
                        {
                            //如果是价量齐全订单
                            if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                            {
                                decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                drv[CTPriceControler.HIDE_PX_FLAG_FIELD] = (px != px_last || firmflag != firmflag_last) ? "0" : "1";
                                px_last = px;
                            }
                        }
                        firmflag = firmflag_last;

                        // 设置是否是第一行
                        if (index == 0)
                        {
                            drv[CTPriceControler.IS_BEST_FIELD] = true;
                        }
                        else
                        {
                            drv[CTPriceControler.IS_BEST_FIELD] = false;
                        }
                        index++;

                        // 处理多结算方式, +0,+1
                        if (this.SettleFilter == "+1")
                        {
                            string srcsetter = drv[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                            if (srcsetter.Contains("1,"))
                            {
                                drv[CTPriceControler.SETTL_TYPE_FIELD] = srcsetter.Replace("1,","");
                            }
                        }

                        // 统计总数 
                        if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                        {
                            decimal qty = CommonUtils.ConvertToDecimal(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                            totalQty += qty;
                            drv[CTPriceControler.TOTAL_QTY_FIELD] = totalQty;
                        }

                        // 角标标志
                        string orginalFlag = drv[CTPriceControler.FLAG_TYPE_FIELD] == null ? string.Empty : drv[CTPriceControler.FLAG_TYPE_FIELD].ToString();
                        string currentFlag = GenerateFlagType(drv);
                        if (orginalFlag != currentFlag)
                        {
                            drv[CTPriceControler.FLAG_TYPE_FIELD] = currentFlag;
                        }
                    }
                }


                if (dv.Count > 0)
                {
                    DataRowView drv = dv[0];
                    itemArr = drv.Row.ItemArray;
                }
            }

            // 更新固定行数据
            lock (frozenLock)
            {
                if (itemArr != null)
                {
                    if (dv_Frozen.Count == 0)
                    {
                        dv_Frozen.Table.Rows.Add(itemArr);
                    }
                    else
                    {
                        dv_Frozen[0].Row.ItemArray = itemArr;
                    }
                }
                else
                {
                    dv_Frozen.Table.Rows.Clear();
                }
            }

            // 表名
            string tn= dv.Table.TableName; //BidQuotations,OfrQuotations
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(tn))
                {
                    string _BestBidAmount = string.Empty;
                    string _BestOfrAmount = string.Empty;
                    string _BestBid = string.Empty;
                    string _BestOfr = string.Empty;
                    string _BestPriceBP = string.Empty;
                    string _BestBidSetter = string.Empty;
                    string _BestOfrSetter = string.Empty;
                    decimal _totalBid = 0;
                    decimal _totalOfr = 0;
                    // Bid 背景色
                    PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                    PriceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
                    // Ofr 背景色
                    PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                    PriceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;

                    if (tn == "BidQuotations")
                    {
                        lock (normalLock)
                        {
                            _priceModel.BestBidAmount = "";
                            _priceModel.BestBid = "--";
                            _priceModel.BestBidNetPrice = "--";
                            _priceModel.BidArrowToolTxt = "本人有报价";
                            _priceModel.BestBidSettler = "";
                            _priceModel.BestBidSettlerYuan = "";
                            _priceModel.BidArrowVisibilly = Visibility.Collapsed;
                            _priceModel.BidIsPermission = "false";
                            _priceModel.BestBidDataRowView = null;
                            
                            // 现券
                            decimal bestBIDp = 0;
                            // 远期
                            decimal bestBIDf = 0;
                            if (dv.Count > 0)
                            {
                                bool isFound = false;
                                bool isHaveMeBestPrice = false;
                                bool isHaveMePrice = false;
                                bool isHaveOurPrice = false;
                                bool isHaveAddZeroAndOne = false;
                                List<string> lstsettle = new List<string>();
                                foreach (DataRowView drv in dv)
                                {
                                    if (drv["IsBest"].ToString() == "True")
                                    {
                                        if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                        {
                                            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                            _BestBid = px.ToString();
                                            _priceModel.BestBid = _BestBid;
                                            _priceModel.BestBidNetPrice = CommonUtils.ConvertToDecimal(drv[CTPriceControler.NET_PRICE_FIELD].ToString()).ToString();
                                        }

                                        isFound = true;


                                        string settleryuan = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                                        _priceModel.BestBidSettlerYuan = settleryuan;
                                        // _priceModel.BestBidSettler = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                        _priceModel.BestBidDataRowView = drv;
                                        string tmpsettle0 = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                        if (!string.IsNullOrEmpty(tmpsettle0))
                                        {
                                            if (!lstsettle.Contains(tmpsettle0))
                                            {
                                                lstsettle.Add(tmpsettle0);
                                            }
                                        }

                                        if (drv["IsMe"].ToString() == "True")
                                        {
                                            isHaveMeBestPrice = true;
                                        }

                                        if (drv[CTPriceControler.IS_PERMISSION_FIELD].ToString() == "True")
                                        {
                                            _priceModel.BidIsPermission = "true";
                                        }
                                    }
                                    else
                                    {
                                        if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                        {
                                            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                            _BestBid = px.ToString();
                                            if (_priceModel.BestBid == _BestBid)
                                            {
                                                string settleryuan = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                                                string tmpsettle = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                                if (!string.IsNullOrEmpty(tmpsettle))
                                                {
                                                    if (!lstsettle.Contains(tmpsettle))
                                                    {
                                                        lstsettle.Add(tmpsettle);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //小红点 如果是远期, 计算远的最小值
                                    decimal tmpDp = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                    if (!string.IsNullOrEmpty(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString()))
                                    {
                                        if (dv.Count > 1)
                                            bestBIDf = bestBIDf == 0 ? tmpDp : Math.Min(tmpDp, bestBIDf);
                                    }
                                    else
                                    {
                                        // 如果是现券, 计算现券的最小值
                                        bestBIDp = bestBIDp == 0 ? tmpDp : Math.Min(tmpDp, bestBIDp);
                                        isHaveAddZeroAndOne = true;
                                    }

                                    if (drv["IsMe"].ToString() == "True")
                                    {
                                        isHaveMePrice = true;
                                    }

                                    if ((drv["Source"].ToString() == "IDB") || (drv["Source"].ToString() == "QUANT") || (drv["Source"].ToString() == "MANUAL"))
                                    {
                                        isHaveOurPrice = true;
                                    }

                                    if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                    {
                                        if (CommonUtils.ConvertToDecimal(drv[CTPriceControler.Yield_Field].ToString()) == CommonUtils.ConvertToDecimal(_priceModel.BestBid.ToString()))
                                        {
                                            decimal qty = CommonUtils.ConvertToDecimal(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                                            _totalBid += qty;
                                        }
                                    }
                                }

                                // 显示小红点
                                if (this.OnYunEvent != null)
                                {
                                    if ((bestBIDf > 0) && isHaveAddZeroAndOne)
                                    {
                                        if (bestBIDf < bestBIDp)
                                        {
                                            ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "1", Info10 = "BidQuotations" };
                                            OnYunEvent?.Invoke(obj);
                                        }
                                        else
                                        {
                                            ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "0", Info10 = "BidQuotations" };
                                            OnYunEvent?.Invoke(obj);
                                        }
                                    }
                                    else
                                    {
                                        ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "0", Info10 = "BidQuotations" };
                                        OnYunEvent?.Invoke(obj);
                                    }
                                }




                                if (lstsettle.Count == 1)
                                {
                                    if (lstsettle.Contains("+0"))
                                    {
                                        _priceModel.BestBidSettler = "+0";
                                    }

                                    if (lstsettle.Contains("远"))
                                    {
                                        _priceModel.BestBidSettler = "远";
                                    }

                                    if (lstsettle.Contains("多"))
                                    {
                                        _priceModel.BestBidSettler = "多";
                                    }
                                }
                                else if (lstsettle.Count > 1)
                                {
                                    if (lstsettle.Contains("+0") && (!lstsettle.Contains("远")))
                                    {
                                        _priceModel.BestBidSettler = "+0";
                                    }

                                    if (lstsettle.Contains("+0") && lstsettle.Contains("远"))
                                    {
                                        _priceModel.BestBidSettler = "多";
                                    }
                                }
                                else
                                {
                                    _priceModel.BestBidSettler = "";
                                }


                                if (isHaveMeBestPrice)
                                {
                                    if (_priceModel.BestBid != "--")
                                    {
                                        _priceModel.BidArrowToolTxt = "本人有报价，且为市场最优";
                                        _priceModel.BidArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFF7D00"));
                                        _priceModel.BidArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                                        _priceModel.BidArrowVisibilly = Visibility.Visible;
                                    }
                                }
                                else
                                {
                                    // 如果没有本人报价，但有本机构报价
                                    if (isHaveOurPrice)
                                    {
                                        _priceModel.BidArrowToolTxt = "本机构有报价";
                                        _priceModel.BidArrowStroke = Application.Current.MainWindow.FindResource("TextBox.Focus.Border") as SolidColorBrush;//new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
                                        _priceModel.BidArrowFill = _priceModel.BidArrowStroke;
                                        _priceModel.BidArrowVisibilly = Visibility.Visible;
                                    }

                                    if (isHaveMePrice)
                                    {
                                        _priceModel.BidArrowToolTxt = "本人有报价";
                                        _priceModel.BidArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                                        _priceModel.BidArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                                        _priceModel.BidArrowVisibilly = Visibility.Visible;
                                    }
                                }

                                if (MarketIndicator != "5")
                                {
                                    if (_totalBid > 0)
                                    {
                                        _totalBid = _totalBid / 1000;
                                        _priceModel.BestBidAmount = $"{_totalBid.ToString()} K";
                                    }
                                }
                                else
                                {
                                    _priceModel.BestBidAmount = $"{_totalBid.ToString()}";
                                }
                                

                                if (!isFound)
                                {
                                    _priceModel.BestBid = "--";
                                    _priceModel.BestBidNetPrice = "--";
                                    _priceModel.BestBidAmount = "";
                                    _priceModel.BestBidSettler = "";
                                    _priceModel.BidArrowVisibilly = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    else if (tn == "OfrQuotations")
                    {
                        lock (normalLock)
                        {
                            _priceModel.BestOfrAmount = "";
                            _priceModel.BestOfr = "--";
                            _priceModel.BestOfrNetPrice = "--";
                            _priceModel.OfrArrowToolTxt = "本人有报价";
                            _priceModel.BestOfrSettler = "";
                            _priceModel.BestOfrSettlerYuan = "";
                            _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                            _priceModel.OfrIsPermission = "false";
                            _priceModel.BestOfrDataRowView = null;

                            // 现券
                            decimal bestOFRp = 0;
                            // 远期
                            decimal bestOFRf = 0;

                            if (dv.Count > 0)
                            {
                                bool isFound = false;
                                bool isHaveMeBestPrice = false;
                                bool isHaveMePrice = false;
                                bool isHaveOurPrice = false;
                                bool isHaveAddZeroAndOne = false;
                                List<string> lstsettle = new List<string>();
                                foreach (DataRowView drv in dv)
                                {
                                    if (drv["IsBest"].ToString() == "True")
                                    {
                                        if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                        {
                                            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                            _BestOfr = px.ToString();
                                            _priceModel.BestOfr = _BestOfr;
                                            _priceModel.BestOfrNetPrice = CommonUtils.ConvertToDecimal(drv[CTPriceControler.NET_PRICE_FIELD].ToString()).ToString();
                                        }

                                        isFound = true;
                                        string settleryuan = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                                        _priceModel.BestOfrSettlerYuan = settleryuan;
                                        //_priceModel.BestOfrSettler = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                        _priceModel.BestOfrDataRowView = drv;

                                        string tmpsettle0 = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                        if (!string.IsNullOrEmpty(tmpsettle0))
                                        {
                                            if (!lstsettle.Contains(tmpsettle0))
                                            {
                                                lstsettle.Add(tmpsettle0);
                                            }
                                        }

                                        if (drv["IsMe"].ToString() == "True")
                                        {
                                            isHaveMeBestPrice = true;
                                        }

                                        // to do ... 累加时是否要判断是否有权限？
                                        if (drv[CTPriceControler.IS_PERMISSION_FIELD].ToString() == "True")
                                        {
                                            _priceModel.OfrIsPermission = "true";
                                        }
                                    }
                                    else
                                    {
                                        if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                        {
                                            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                            _BestOfr = px.ToString();
                                            if (_priceModel.BestOfr == _BestOfr)
                                            {
                                                string settleryuan = drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                                                string tmpsettle = GetSettle(drv["SettlType"] == null ? "" : drv["SettlType"].ToString(), settleryuan);
                                                if (!string.IsNullOrEmpty(tmpsettle))
                                                {
                                                    if (!lstsettle.Contains(tmpsettle))
                                                    {
                                                        lstsettle.Add(tmpsettle);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (drv[CTPriceControler.IsInnerInComplete_Field].ToString() == "0")
                                    {
                                        if (CommonUtils.ConvertToDecimal(drv[CTPriceControler.Yield_Field].ToString()) == CommonUtils.ConvertToDecimal(_priceModel.BestOfr.ToString()))
                                        {
                                            decimal qty = CommonUtils.ConvertToDecimal(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                                            _totalOfr += qty;
                                        }
                                    }

                                    //小红点 如果是远期, 计算远的最大值
                                    decimal tmpDp = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                    if (!string.IsNullOrEmpty(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString()))
                                    {
                                        if (dv.Count > 1)
                                            bestOFRf = bestOFRf == 0 ? tmpDp : Math.Max(tmpDp, bestOFRf);
                                    }
                                    else
                                    {
                                        // 如果是现券, 计算现券的最大值
                                        bestOFRp = bestOFRp == 0 ? tmpDp : Math.Max(tmpDp, bestOFRp);
                                        isHaveAddZeroAndOne = true;
                                    }

                                    if (drv["IsMe"].ToString() == "True")
                                    {
                                        isHaveMePrice = true;
                                    }

                                    if ((drv["Source"].ToString() == "IDB") || (drv["Source"].ToString() == "QUANT") || (drv["Source"].ToString() == "MANUAL"))
                                    {
                                        isHaveOurPrice = true;
                                    }
                                }

                                // 显示小红点
                                if (this.OnYunEvent != null)
                                {
                                    if (bestOFRf > 0 && isHaveAddZeroAndOne)
                                    {
                                        if (bestOFRf > bestOFRp)
                                        {
                                            ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "1", Info10 = "OfrQuotations" };
                                            OnYunEvent?.Invoke(obj);
                                        }
                                        else
                                        {
                                            ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "0", Info10 = "OfrQuotations" };
                                            OnYunEvent?.Invoke(obj);
                                        }
                                    }
                                    else
                                    {
                                        ExecutionReport obj = new ExecutionReport() { SecurityID = this.SecurityID, Terminal = "0", Info10 = "OfrQuotations" };
                                        OnYunEvent?.Invoke(obj);
                                    }
                                }

                                if (lstsettle.Count == 1)
                                {
                                    if (lstsettle.Contains("+0"))
                                    {
                                        _priceModel.BestOfrSettler = "+0";
                                    }

                                    if (lstsettle.Contains("远"))
                                    {
                                        _priceModel.BestOfrSettler = "远";
                                    }

                                    if (lstsettle.Contains("多"))
                                    {
                                        _priceModel.BestOfrSettler = "多";
                                    }
                                }
                                else if (lstsettle.Count > 1)
                                {
                                    if (lstsettle.Contains("+0") && (!lstsettle.Contains("远")))
                                    {
                                        _priceModel.BestOfrSettler = "+0";
                                    }
                                    if (lstsettle.Contains("+0") && lstsettle.Contains("远"))
                                    {
                                        _priceModel.BestOfrSettler = "多";
                                    }
                                }
                                else
                                {
                                    _priceModel.BestOfrSettler = "";
                                }

                                if (isHaveMeBestPrice)
                                {
                                    if (_priceModel.BestBid != "--")
                                    {
                                        _priceModel.OfrArrowToolTxt = "本人有报价，且为市场最优";
                                        _priceModel.OfrArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C009AFF"));
                                        _priceModel.OfrArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                                        _priceModel.OfrArrowVisibilly = Visibility.Visible;
                                    }
                                }
                                else
                                {
                                    // 如果没有本人报价，但有本机构报价
                                    if (isHaveOurPrice)
                                    {
                                        _priceModel.OfrArrowToolTxt = "本机构有报价";
                                        _priceModel.OfrArrowStroke = Application.Current.MainWindow.FindResource("TextBox.Focus.Border") as SolidColorBrush; //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
                                        _priceModel.OfrArrowFill = _priceModel.OfrArrowStroke;
                                        _priceModel.OfrArrowVisibilly = Visibility.Visible;
                                    }

                                    if (isHaveMePrice)
                                    {
                                        _priceModel.OfrArrowToolTxt = "本人有报价";
                                        _priceModel.OfrArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                                        _priceModel.OfrArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                                        _priceModel.OfrArrowVisibilly = Visibility.Visible;
                                    }
                                }

                                if (!isFound)
                                {
                                    _priceModel.BestOfr = "--";
                                    _priceModel.BestOfrNetPrice = "--";
                                    _priceModel.BestOfrAmount = "";
                                    _priceModel.BestOfrSettler = "";
                                    _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                                }

                                if (MarketIndicator != "5")
                                {
                                    if (_totalOfr>0)
                                    {
                                        _totalOfr = _totalOfr / 1000;
                                        _priceModel.BestOfrAmount = $"{_totalOfr.ToString()} K";
                                    }
                                }
                                else
                                {
                                    _priceModel.BestOfrAmount = $"{_totalOfr.ToString()}";
                                }

                                
                            }
                        }
                    }

                    if ((string.IsNullOrEmpty(_priceModel.BestBid)) | (string.IsNullOrEmpty(_priceModel.BestOfr)))
                    {
                        _priceModel.BestPriceBP = "--";
                        _priceModel.BestPriceDisplayBP = "--"; 
                        _priceModel.BestNetPriceBP = "--";
                        _priceModel.BestNetPriceDisplayBP = "--";
                    }

                    if ((_priceModel.BestBid=="--") | (_priceModel.BestOfr == "--"))
                    {
                        _priceModel.BestPriceBP = "--";
                        _priceModel.BestPriceDisplayBP = "--";
                        _priceModel.BestNetPriceBP = "--";
                        _priceModel.BestNetPriceDisplayBP = "--";
                    }

                    ReCalcBestPriceColor();
                }
            });
        }

        /// <summary>
        /// 动态 Bid Ofr 背景色
        /// </summary>
        public void ReCalcBestPriceColor()
        {
            // 是否显示倒挂
            bool isProcUpDownResverse = false;
            // Bid 
            if ((PriceModel.BestBid != "--") && (PriceModel.BidIsPermission == "true"))
            {
                PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                PriceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
            }
            else
            {
                PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                //PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                //PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));

            }

            // Ofr 
            if ((PriceModel.BestOfr != "--") && (PriceModel.OfrIsPermission == "true"))
            {
                PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                PriceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;
            }
            else
            {
                PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                //PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                //PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
            }

            if ((PriceModel.BestBid != "--") && (PriceModel.BestOfr != "--"))
            {
                decimal bidD = CommonUtils.ConvertToDecimal(PriceModel.BestBid);
                decimal ofrD = CommonUtils.ConvertToDecimal(PriceModel.BestOfr);
                decimal bpd = (bidD - ofrD) * 100;

                if (MarketIndicator == "2")
                {
                    bpd = (ofrD - bidD) * 100;
                }


                if (string.IsNullOrEmpty(PriceModel.BestBid) || string.IsNullOrEmpty(PriceModel.BestOfr))
                {
                    PriceModel.BestPriceBP = CommonConstants.NO_PRICE;
                    PriceModel.BestPriceDisplayBP = CommonConstants.NO_PRICE;
                }
                else
                {
                    PriceModel.BestPriceBP = PriceCalHelper.Instance.RemoveZero(CommonUtils.FormatNum(bpd.ToString())) + " bp";
                    PriceModel.BestPriceDisplayBP = CommonUtils.FormatLimitBp(bpd);
                }
                decimal bidNet = CommonUtils.ConvertToDecimal(PriceModel.BestBidNetPrice);
                decimal ofrNet = CommonUtils.ConvertToDecimal(PriceModel.BestOfrNetPrice);
                decimal bpNet = (ofrNet - bidNet);
                if (string.IsNullOrEmpty(PriceModel.BestBidNetPrice) || string.IsNullOrEmpty(PriceModel.BestOfrNetPrice))
                {
                    PriceModel.BestNetPriceBP = CommonConstants.NO_PRICE;
                    PriceModel.BestNetPriceDisplayBP = CommonConstants.NO_PRICE;
                }
                else
                {
                    PriceModel.BestNetPriceBP = PriceCalHelper.Instance.RemoveZero(CommonUtils.FormatNum(bpNet.ToString()));
                    PriceModel.BestNetPriceDisplayBP = CommonUtils.FormatLimitNetPx(bpNet);
                }
                if (MarketIndicator == "2")
                {
                    //IRS
                    // 处理倒挂颜色
                    if (bpd > 0)
                    {
                        isProcUpDownResverse = true;

                        // 如果 倒挂 Bid 有效
                        if (PriceModel.BidIsPermission == "true")
                        {
                            // 倒挂 Bid 背景色,需要动态变化，最后定格为 #16FF3C3C
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370F11"));
                            PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                        }
                        else
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                            //PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }

                        // 如果 倒挂 Ofr 有效
                        if (PriceModel.OfrIsPermission == "true")
                        {
                            // 倒挂 Ofr 背景色,需要动态变化，最后定格为 #16FF3C3C
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370F11"));
                            PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                        }
                        else
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideOfrBorderBrush = PriceModel.UpSideOfrBgBrush;
                            //PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }
                    }
                    else
                    {
                        isProcUpDownResverse = false;
                        // 如果 Bid 有效
                        if (PriceModel.BidIsPermission == "true")
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                            PriceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
                        }
                        else
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                            //PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }

                        // 如果 Ofr 有效
                        if (PriceModel.OfrIsPermission == "true")
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                            PriceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;
                        }
                        else
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideOfrBorderBrush = PriceModel.UpSideOfrBgBrush;
                            //PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }
                    }
                }
                else
                {
                    // 处理倒挂颜色
                    if (bpd <= 0)
                    {
                        isProcUpDownResverse = true;

                        // 如果 倒挂 Bid 有效
                        if (PriceModel.BidIsPermission == "true")
                        {
                            // 倒挂 Bid 背景色,需要动态变化，最后定格为 #16FF3C3C
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370F11"));
                            PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                        }
                        else
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                            //PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }

                        // 如果 倒挂 Ofr 有效
                        if (PriceModel.OfrIsPermission == "true")
                        {
                            // 倒挂 Ofr 背景色,需要动态变化，最后定格为 #16FF3C3C
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370F11"));
                            PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                        }
                        else
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideOfrBorderBrush = PriceModel.UpSideOfrBgBrush;
                            //PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }
                    }
                    else
                    {
                        isProcUpDownResverse = false;
                        // 如果 Bid 有效
                        if (PriceModel.BidIsPermission == "true")
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
                            PriceModel.UpSideBidBorderBrush = _priceModel.UpSideBidBgBrush;
                        }
                        else
                        {
                            PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideBidBorderBrush = PriceModel.UpSideBidBgBrush;
                            //PriceModel.UpSideBidBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideBidBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }

                        // 如果 Ofr 有效
                        if (PriceModel.OfrIsPermission == "true")
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
                            PriceModel.UpSideOfrBorderBrush = _priceModel.UpSideOfrBgBrush;
                        }
                        else
                        {
                            PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414"));
                            PriceModel.UpSideOfrBorderBrush = PriceModel.UpSideOfrBgBrush;
                            //PriceModel.UpSideOfrBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                            //PriceModel.UpSideOfrBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303030"));
                        }
                    }
                }
            }

            // 倒挂 Bp 背景色和边框色处理
            if (isProcUpDownResverse)
            {
                // 倒挂 Bp 边框背景色， 倒挂  #90FF3C3C  正常 #121A19
                PriceModel.UpSideBdBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                // 倒挂 Bp 背景色
                PriceModel.UpSideBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2125"));
                PriceModel.UpSideForgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370F11"));
            }
            else
            {
                PriceModel.UpSideBdBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121A19"));
                PriceModel.UpSideBgBrush = new SolidColorBrush(Colors.Transparent);
                if (_priceModel.BestPriceBP != "--")
                {
                    PriceModel.UpSideForgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                }
                else
                {
                    PriceModel.UpSideForgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4D4C"));
                }
            }
        }
        /// <summary>
        /// 获取显示用的清算速度 +1 为空
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="settleryuan"></param>
        /// <returns></returns>
        private string GetSettle(string settle, string settleryuan)
        {
            string res = "";
            if (settle.Contains("2") || settle.Contains("3") || settle.Contains("2,3") || settle.Contains("3,2"))
            {
                res = "";
            }
            
            if (settle.Contains("1"))
            {
                res = "+0";
            }

            if (!string.IsNullOrEmpty(settleryuan))
            {
                res = "远";
            }

            if ((settle.Contains("1")) && (!string.IsNullOrEmpty(settleryuan)))
            {
                res = "多";
            }
            
            return res;
        }


        /// <summary>
        /// 生成角标，规则如下
        /// 1.text和ems订单都没有，标记为空
        /// 2.只有text，标记为0； 
        /// 3.ems订单（且不是inner或者是idb的报价），标记为1
        /// </summary>
        /// <param name="drv"></param>
        /// <returns>角标标记</returns>
        private string GenerateFlagType(DataRowView drv)
        {

            string text = CommonUtils.ConvertToString(drv[CTPriceControler.TEXT_FIELD]);
            string venueTypeGw = CommonUtils.ConvertToString(drv[CTPriceControler.VENUE_TYPE_GW_FIELD]);
            string venues = CommonUtils.ConvertToString(drv[CTPriceControler.SOURCE_FIELD]);
            string securtityID = CommonUtils.ConvertToString(drv[CTPriceControler.SECURITY_ID_FIELD]);
            string side_str = CommonUtils.ConvertToString(drv[CTPriceControler.SIDE_FIELD]);
            string side = side_str == "0" ? "1" : "2";
            string settleType = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_FIELD]);
            string settleType_Yuan = CommonUtils.ConvertToString(drv[CTPriceControler.SETTL_TYPE_YUAN_FIELD]);
            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD] == null ? "0" : drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
            decimal yieldExe = CommonUtils.ConvertToDecimal(drv[CTPriceControler.YieldToExecution_Field] == null ? "0" : drv[CTPriceControler.YieldToExecution_Field].ToString());
            //if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(venueTypeGw) == false && venueTypeGw != "IDB" && venueTypeGw != "INNER")
            //{
            //    string qty_text = CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]);
            //    text = qty_text;
            //    drv[CTPriceControler.TEXT_FIELD] = qty_text;
            //}
            string partname = CommonUtils.ConvertToString(drv[CTPriceControler.PARTY_NAME_FIELD]);
            string c = CommonUtils.ConvertToString(drv[CTPriceControler.CUSTOMER_SOURCE_FIELD]);


            // 角标标志
            string currentFlag = string.Empty;
            int count = 0;
            if (string.IsNullOrEmpty(venueTypeGw) == false && venueTypeGw != "IDB" && venueTypeGw != "INNER")
            {
                string[] settlTypeArr = null;
                if (settleType.Equals("2,3"))
                    settlTypeArr = new string[] { "2,3" };
                else
                    settlTypeArr = settleType.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);

                foreach (string settleTypeItem in settlTypeArr)
                {
                    if (venues.Equals("QDMESP"))
                    {
                        count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeItem, venues, venueTypeGw, px, side, partname,yieldExe);
                    }
                    else
                    {
                        count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeItem, venues, venueTypeGw, px, side);
                    }
                }

                string[] settlType_yuan_Arr = settleType_Yuan.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string settleTypeYuanItem in settlType_yuan_Arr)
                {
                    if (venues.Equals("QDMESP"))
                    {
                        count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeYuanItem, venues, venueTypeGw, px, side, partname, yieldExe);
                    }
                    else
                    {
                        count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeYuanItem, venues, venueTypeGw, px, side);
                    }
                }
            }
            if (count > 0)
            {
                currentFlag = "1";
            }
            else
            {
                currentFlag = string.IsNullOrEmpty(text) ? string.Empty : "0";
            }

            return currentFlag;
        }

        #region Cancel Button
        private void OnCloseClick()
        {
            this.Dispose();
            myWindow.Close();
            GC.Collect();
        }

        
        
        #endregion

        #region 成交单

        public void OnLastExec(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //lock (_lockObj)
                {
                    UpdateLastExec(obj);
                }
            }));
        }


        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj != null)
            {
                if ((obj.SecurityID ?? "") == this.Security)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        UpdateLastExec(obj);

                    }));
                }
            }
        }

        private bool IsFwd(string settle)
        {
            return !string.IsNullOrEmpty(settle) && !settle.Equals(CommonConstants.T1_ONE) && !CommonConstants.T2_T3.Contains(settle) && settle.Contains("+");
        }

        /// <summary>
        /// 小红点
        /// </summary>
        /// <param name="obj"></param>
        public bool CheckPwd(string securityid)
        {
            bool result = false;
            lock (LastExecutionProcesser.Processer.lockObj)
            {
                IEnumerable<ExecutionReport> res = ExecutionsHandler.Instance.GetLastExecutionBySecurityID(securityid).Where(orderItem => IsFwd(orderItem.SettlType));
                if (res != null)
                {
                    if (res.ToList().Count > 0)
                        result = true;
                }
            }
            return result;
        }

        private void UpdateLastExec(ExecutionReport obj)
        {
            //更新最新成交
            if (string.IsNullOrEmpty(obj.Yield))
                return;


            MarketExecutionsItem newExeOrder = new MarketExecutionsItem();
            LastExecutionProcesser.Processer.ConvertExecutionReportToMarketExecutionsItem(obj, ref newExeOrder);

            if (LocalEnv.ConfInfo.IsUseOHLCSvr == "1")
            {
                OhlcMsg lastExeItemohlc = OhlcHandler.Instance.GetLastOhlc(obj.SecurityID);
                if (lastExeItemohlc != null)
                {
                    decimal dNew = 0;
                    decimal dClose = 0;

                    if (!string.IsNullOrWhiteSpace(newExeOrder.ExecutePricePercent.ToString()))
                    {
                        if (decimal.TryParse(newExeOrder.ExecutePricePercent.ToString(), out dNew) && decimal.TryParse(lastExeItemohlc.inf2, out dClose) && dClose != 0 && dNew != 0)
                        {
                            decimal bp = dNew - dClose;
                            newExeOrder.Bp = bp == 0 ? string.Empty : (bp * 100).ToString();
                            //newExeOrder.UpDown = bp > 0 ? CommonConstants.SIDE_BID_ONE : bp < 0 ? CommonConstants.SIDE_OFR_TWO : CommonConstants.SIDE_TRD_THREE;
                        }
                    }
                    else
                    {
                        newExeOrder.Bp = CommonConstants.NO_PRICE;
                    }
                    decimal dNewNet = 0;
                    decimal dCloseNet = 0;
                    if (!string.IsNullOrWhiteSpace(newExeOrder.NetPrice.ToString()))
                    {
                        if (decimal.TryParse(newExeOrder.NetPrice.ToString(), out dNewNet) && decimal.TryParse(lastExeItemohlc.inf3, out dCloseNet) && dCloseNet != 0 && dNewNet != 0)
                        {
                            decimal bp = dNewNet - dCloseNet;
                            newExeOrder.NetPriceSpread = bp == 0 ? string.Empty : (bp).ToString();
                        }
                    }
                    else
                    {
                        newExeOrder.NetPriceSpread = CommonConstants.NO_PRICE;
                    }
                }
            }

            decimal bpd = 0;
            if (decimal.TryParse(newExeOrder.Bp, out bpd))
            {
                LastExecPriceBP = (bpd > 0 ? "+" : "-") + PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString())+" bp";
            }
            else
                LastExecPriceBP = "";

            // 行权收益率可能因后台 null/SQLite DECIMAL 列归一为 "0"，此时视作无值，回退到 Yield
            bool hasYieldToExecution = !string.IsNullOrEmpty(obj.YieldToExecution) && CommonUtils.ConvertToDecimal(obj.YieldToExecution) != 0;
            string tmppric = hasYieldToExecution ? CommonUtils.FormatNum(obj.YieldToExecution) : CommonUtils.FormatNum(obj.Yield);
            LastExecPrice = tmppric == "0.00" ? "" : tmppric;

            LastExecSideShow = ComboSourceUtil.Instance.FindEnum(obj.Side, "side2"); ;

            LastExecSource = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venues");

            decimal spread = 0;
            if (decimal.TryParse((newExeOrder.NetPriceSpread??"").Replace("+","").Replace("-", "").Replace(CommonConstants.NO_PRICE,"").Replace(CommonConstants.NO_UP_DOWN,""), out spread))
            {
                LastExecNetPriceSpread = (spread > 0 ? "+" : "-") + PriceCalHelper.Instance.RemoveZero(Math.Abs(spread).ToString());
            }
            else
                LastExecNetPriceSpread = "";

            string netPrice = CommonUtils.FormatNum(obj.NetPrice);
            LastExecNetPrice = netPrice == "0.00" ? "" : netPrice;
            if (bpd > 0)
            {
                LastExecBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21DF1F"));
            }
            else if (bpd < 0)
            {
                LastExecBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C"));
            }
            else
            {
                LastExecBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            }
        }

        public void GetLastExecutionsItem(string secid)
        {
            //获取最优价
            //LastExecutionProcesser.Processer.AddGetLastExecutionQueue(secid);
            //OhlcMsg lastExeItemohlc = OhlcHandler.Instance.GetLastOhlc(secid);
            //if (lastExeItemohlc == null)
            //{
            //    OhlcHandler.Instance.RequestOhlc(secid);
            //}

            //MarketExecutionsItem lastExeItem = ExecutionsHandler.Instance.GetLastExecutionsItem(secid);
            //if (lastExeItem != null)
            //{
            //    ExecutionReport temOrder = new ExecutionReport();
            //    temOrder.TransactTime = lastExeItem.TimeStamp;
            //    temOrder.SecurityID = secid;
            //    temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
            //    temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
            //    temOrder.Venues = lastExeItem.SourceTrader;
            //    temOrder.Yield = temOrder.LastPx;
            //    if (string.IsNullOrEmpty(LastExecPrice))
            //    {
            //        string tmppric = CommonUtils.FormatNum(temOrder.LastPx);
            //        LastExecPrice = tmppric == "0.00" ? "" : tmppric;
            //        LastExecSideShow = ComboSourceUtil.Instance.FindEnum(temOrder.Side, "side2"); ;
            //        LastExecSource = ComboSourceUtil.Instance.FindEnum(temOrder.Venues, "venues");
            //    }

            //    this.OnLastExec(temOrder);
            //}
        }

        /// <summary>
        /// 小红点
        /// </summary>
        /// <param name="marketDataList"></param>
        private void UpdateFwdFlag(List<MarketDataSnapshotFullRefresh> marketDataList)
        {
            //ObservableCollection<MarketExecutionsItem> objs  = LastExecutionProcesser.Processer.GetDatasBySecurityID(SecurityID);
            //List<MarketExecutionsItem> om = ((ObservableCollection<MarketExecutionsItem>)objs).OrderByDescending(o => o.Time).ToList();
            //bool showFwd = PriceCalHelper.Instance.CheckFwd(om);
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    PriceAndOrderView paov = (myWindow as View.PopupNoBorder).MainFrame.Content as PriceAndOrderView;

            //    SettleFilterView settleFilterView = paov.FindName("FilterPanel") as SettleFilterView;
            //    settleFilterView.SetEllipseVisibility(Visibility.Visible);
            //}));
        }

        #endregion

        #region 生成测试数据
        //private bool startTestFlag = false;
        //private int i = 0;
        //public void GenerateTestData()
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        while (true)
        //        {
        //            if (this.startTestFlag)
        //            {
        //                if (string.IsNullOrEmpty(this.SecurityID) == true)
        //                {
        //                    //Thread.Sleep(1000);
        //                    //continue;
        //                }

        //                // MarketDataSnapshotFullRefresh ctPrice = STC.XBond.Plugin.Test.TestDataGenerator.GetCTPrice(this.SecurityID);
        //                MarketDataSnapshotFullRefresh ctPrice = new MarketDataSnapshotFullRefresh();
        //                if (i % 2 == 0)
        //                {
        //                    ctPrice.CCDCValuation = "3.2";
        //                    ctPrice.ClOrdID = "1623117498307";
        //                    ctPrice.MDBookType = "401";
        //                    ctPrice.MarketIndicator = "4";
        //                    ctPrice.SecurityID = "150210.IB";
        //                    ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
        //                    ctPrice.Symbol = "15国开10";
        //                    ctPrice.VenueTypeGW = "ct";

        //                    GNoMDEntries data1 = new GNoMDEntries();
        //                    data1.FirmFlag = "1";
        //                    data1.InstitutionID = string.Empty;
        //                    data1.InstitutionName = string.Empty;
        //                    data1.InternalFlag = "1";
        //                    data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
        //                    data1.MDEntryID = "s851761298516574208";
        //                    data1.MDEntryPx = "4";
        //                    data1.MDEntrySize = "2000";
        //                    data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
        //                    data1.MDEntryType = "0";
        //                    data1.MDPriceLevel = "1";
        //                    data1.MDReqID = string.Empty;
        //                    data1.NetPrice = "100.7239";
        //                    data1.PartySubID = "193";
        //                    data1.PartySubName = "严晓莉";
        //                    data1.RoleName = "管理员";
        //                    data1.SettlType = "2";
        //                    data1.TraderID = string.Empty;
        //                    data1.TraderName = string.Empty;
        //                    data1.VenueTypeGW = "INNER";
        //                    data1.Venues = "NANUAL";
        //                    data1.WholeFlag = "0";
        //                    data1.Yield = "4";

        //                    GNoMDEntries data2 = new GNoMDEntries();
        //                    data2.FirmFlag = "1";
        //                    data2.InstitutionID = string.Empty;
        //                    data2.InstitutionName = string.Empty;
        //                    data2.InternalFlag = "1";
        //                    data2.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
        //                    data2.MDEntryID = "s851762052480466944";
        //                    data2.MDEntryPx = "5";
        //                    data2.MDEntrySize = "2000";
        //                    data2.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
        //                    data2.MDEntryType = "0";
        //                    data2.MDPriceLevel = "2";
        //                    data2.MDReqID = string.Empty;
        //                    data2.NetPrice = "97.2847";
        //                    data2.PartySubID = "193";
        //                    data2.PartySubName = "严晓莉";
        //                    data2.RoleName = "管理员";
        //                    data2.SettlType = "2";
        //                    data2.TraderID = string.Empty;
        //                    data2.TraderName = string.Empty;
        //                    data2.VenueTypeGW = "INNER";
        //                    data2.Venues = "NANUAL";
        //                    data2.WholeFlag = "0";
        //                    data2.Yield = "5";

        //                    List<GNoMDEntries> list = new List<GNoMDEntries>();
        //                    list.Add(data1);
        //                    list.Add(data2);
        //                    ctPrice.NoMDEntries = list;
                            
        //                }
        //                else
        //                {
        //                    ctPrice.CCDCValuation = "3.2";
        //                    ctPrice.ClOrdID = "1623117498307";
        //                    ctPrice.MDBookType = "401";
        //                    ctPrice.MarketIndicator = "4";
        //                    ctPrice.SecurityID = "150210.IB";
        //                    ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
        //                    ctPrice.Symbol = "15国开10";
        //                    ctPrice.VenueTypeGW = "ct";

        //                    GNoMDEntries data1 = new GNoMDEntries();
        //                    data1.FirmFlag = "1";
        //                    data1.InstitutionID = string.Empty;
        //                    data1.InstitutionName = string.Empty;
        //                    data1.InternalFlag = "1";
        //                    data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
        //                    data1.MDEntryID = "s851761298516574208";
        //                    data1.MDEntryPx = "4";
        //                    data1.MDEntrySize = "2000";
        //                    data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
        //                    data1.MDEntryType = "0";
        //                    data1.MDPriceLevel = "1";
        //                    data1.MDReqID = string.Empty;
        //                    data1.NetPrice = "100.7239";
        //                    data1.PartySubID = "193";
        //                    data1.PartySubName = "严晓莉";
        //                    data1.RoleName = "管理员";
        //                    data1.SettlType = "2";
        //                    data1.TraderID = string.Empty;
        //                    data1.TraderName = string.Empty;
        //                    data1.VenueTypeGW = "INNER";
        //                    data1.Venues = "NANUAL";
        //                    data1.WholeFlag = "0";
        //                    data1.Yield = "4";

        //                    GNoMDEntries data2 = new GNoMDEntries();
        //                    data2.FirmFlag = "1";
        //                    data2.InstitutionID = string.Empty;
        //                    data2.InstitutionName = string.Empty;
        //                    data2.InternalFlag = "1";
        //                    data2.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
        //                    data2.MDEntryID = "s851762052480466944";
        //                    data2.MDEntryPx = "5";
        //                    data2.MDEntrySize = "2000";
        //                    data2.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
        //                    data2.MDEntryType = "0";
        //                    data2.MDPriceLevel = "2";
        //                    data2.MDReqID = string.Empty;
        //                    data2.NetPrice = "97.2847";
        //                    data2.PartySubID = "193";
        //                    data2.PartySubName = "严晓莉";
        //                    data2.RoleName = "管理员";
        //                    data2.SettlType = "2";
        //                    data2.TraderID = string.Empty;
        //                    data2.TraderName = string.Empty;
        //                    data2.VenueTypeGW = "INNER";
        //                    data2.Venues = "NANUAL";
        //                    data2.WholeFlag = "0";
        //                    data2.Yield = "5";

        //                    List<GNoMDEntries> list = new List<GNoMDEntries>();
        //                    list.Add(data1);
        //                    list.Add(data2);
        //                    ctPrice.NoMDEntries = list;
        //                }
                        
        //                Instance_OnCTPrice(ctPrice);
        //                i = i % 2;
        //                i++;
        //            }

        //            Thread.Sleep(1000);
        //        }
        //    });
        //}
        #endregion
        private class CTPriceClass
        {
            public DateTime updateTime = DateTime.MinValue;

            public DataTable[] datas = null;
        }
    }
}
