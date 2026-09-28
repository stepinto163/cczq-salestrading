using com.gateway.log;
using netapi.Api;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.Frame.Utils;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.DataProcesser;
using STC.Mm.Plugin.View;
using STC.Mm.Plugin.View.Card;
using STC.Mm.Plugin.View.Component;
using STC.Mm.Plugin.ViewModel.Card;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Mm.Plugin.ViewModel
{
    /// <summary>
    /// TradeBookView的数据内容
    /// </summary>
    public class QuotationListViewModel : ViewModelBase
    {
        WindowManager m_windowManager = WindowManager.Instance;//new WindowManager();
        public ICommand CloseCommand { get { return new RelayCommand(OnCloseClick); } }
        //public ICommand TopCommand { get { return new RelayCommand(OnTopClick); } }

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
                this.OnPropertyChanged();
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
                if ((this._SecurityID != value))
                {
                    this.ClearData(true);

                    if (!string.IsNullOrEmpty(_SecurityID))
                    {
                        this.Init();
                    }
                    // 切换债券
                    this._SecurityID = value;
                    this.OnPropertyChanged();

                    ShowImcompletePanel = Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(_SecurityID))
                    {
                        GetSymbolInfo();
                        EmsOrdersHandler.Instance.RegistEMSOrdersBySecurityID(value, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
                        PriceHandler.Instance.RegisterCTPrice(value, Instance_OnCTPrice);
                    }
                }
                
            }
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
                string bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                string ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                if (_isShowNetPrice)
                {
                    bidSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD} desc,{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                    ofrSort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.NET_PRICE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                }

                BaseDV.Sort = bidSort;
                BidQuotations.Sort = bidSort;
                OfrQuotations.Sort = ofrSort;
                BidQuotations_Frozen.Sort = bidSort;
                OfrQuotations_Frozen.Sort = ofrSort;
                BidIncompleteQuotations.Sort = bidSort;
                OfrIncompleteQuotations.Sort = ofrSort;
                AdjustData(this.BidQuotations, this.BidQuotations_Frozen, bidLockObj, bidLockObj_Frozen);
                AdjustData(this.OfrQuotations, this.OfrQuotations_Frozen, ofrLockObj, ofrLockObj_Frozen);

                EntryPxColumnVisiblity = _isShowNetPrice == true ? Visibility.Collapsed : Visibility.Visible;
                NetPriceColumnVisibility = _isShowNetPrice == true ? Visibility.Visible : Visibility.Collapsed;
                this.OnPropertyChanged();
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
                        _BaseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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

                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BaseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string)); 

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
                        _BidQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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

                        _BidQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BidQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BidQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        if (Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _BidQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc, {CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _BidQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

                        _BidQuotations.Table.PrimaryKey = new DataColumn[] { _BidQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        _BidQuotations.Table.TableName = "BidQuotations";
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
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _OfrQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _OfrQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        if (Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _OfrQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _OfrQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

                        _OfrQuotations.Table.PrimaryKey = new DataColumn[] { _OfrQuotations.Table.Columns[CTPriceControler.KEY_FIELD] };
                        _OfrQuotations.Table.TableName = "OfrQuotations";
                        this.OnPropertyChanged();
                    }
                    return this._OfrQuotations;
                }
            }
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

                        if(Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                             _BidQuotations_Frozen.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc, {CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _BidQuotations_Frozen.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
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

                        if (Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _OfrQuotations_Frozen.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _OfrQuotations_Frozen.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

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
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BidIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        if (Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _BidIncompleteQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD},{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _BidIncompleteQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

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
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(long));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
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

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));

                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _OfrIncompleteQuotations.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));

                        if (Frame.Utils.LocalEnv.ConfInfo.IsUseVenueSort == "1")
                            _OfrIncompleteQuotations.Sort = $"{CTPriceControler.ENTRY_PX_FIELD} desc,{CTPriceControler.FIRM_TYPE_FIELD} desc,{CTPriceControler.ISINNER_FIELD} desc,{CTPriceControler.VENUE_SORT_FIELD},{CTPriceControler.TRANSACT_TIME_FILED}";
                        else
                            _OfrIncompleteQuotations.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";

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
        #endregion

        public QuotationListViewModel()
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
            ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew += Instance_OnMarketExecutions;
            ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew += Instance_OnMarketExecutions;
            #endregion
            // 报价权限问题
            SetPermission();

            Init();
        }


        private void GetSymbolInfo()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(SecurityID))
                {
                    Security = SecurityID;
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.SecurityID);
                    if (bondsInfoPo != null)
                    {
                        SymbolName = bondsInfoPo.Short_Name;
                        RemainingTime = bondsInfoPo.RemaingingTime;
                        Symbol = bondsInfoPo.Short_Name;
                    }
                }
            }));
        }

        private void onloadData(bool flag)
        {
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
            #region Not Use

            BindingOperations.DisableCollectionSynchronization(BidQuotations);

            BindingOperations.DisableCollectionSynchronization(OfrQuotations);

            BindingOperations.DisableCollectionSynchronization(BidQuotations_Frozen);

            BindingOperations.DisableCollectionSynchronization(OfrQuotations_Frozen);

            BindingOperations.DisableCollectionSynchronization(BidIncompleteQuotations);

            BindingOperations.DisableCollectionSynchronization(OfrIncompleteQuotations);

            ExecutionsHandler.Instance.OnMarketExecutionsOnlyNew -= Instance_OnMarketExecutions;
            ExecutionsHandler.Instance.OnOwnExecutionsOnlyNew -= Instance_OnMarketExecutions;

            if (RefreshDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose RefreshDataThread:" + this._SecurityID);
                RefreshDataThread.Abort();
                RefreshDataThread = null;
            }

            if (CalcDataThread != null)
            {
                LogHelper.Info("QuotationListViewModel Dispose CalcDataThread:" + this._SecurityID);
                CalcDataThread.Abort();
                CalcDataThread = null;
            }

            _autoResetEvent.Set();
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
            LogHelper.Info("QuotationListViewModel Dispose:" + this._SecurityID);
            this.ClearData(true);

            lock (stackLock)
            {
                if (_Stack != null)
                {
                    _Stack.Clear();
                    _Stack = null;
                }
            }

            lock (cacheStackLock)
            {
                if (_CacheStack != null)
                {
                    _CacheStack.Clear();
                    _CacheStack = null;
                }
            }

            lock (this.baseLockObj)
            {
                if (_BaseDV != null)
                {
                    _BaseDV.Table.Clear();
                    _BaseDV.Dispose();
                    _BaseDV = null;
                }
            }

            lock (this.bidLockObj)
            {
                if (_BidQuotations != null)
                {
                    _BidQuotations.Table.Clear();
                    _BidQuotations.Dispose();
                    _BidQuotations = null;
                }
            }
            lock (this.ofrLockObj)
            {
                if (_OfrQuotations != null)
                {
                    _OfrQuotations.Table.Clear();
                    _OfrQuotations.Dispose();
                    _OfrQuotations = null;
                }
            }

            lock (this.bidLockObj_Frozen)
            {
                if (_BidQuotations_Frozen != null)
                {
                    _BidQuotations_Frozen.Table.Clear();
                    _BidQuotations_Frozen.Dispose();
                    _BidQuotations_Frozen = null;
                }
            }
            lock (this.ofrLockObj_Frozen)
            {
                if (_OfrQuotations_Frozen != null)
                {
                    _OfrQuotations_Frozen.Table.Clear();
                    _OfrQuotations_Frozen.Dispose();
                    _OfrQuotations_Frozen = null;
                }
            }

            lock (this.bidIncompleteLockObj)
            {
                if (_BidIncompleteQuotations != null)
                {
                    _BidIncompleteQuotations.Table.Clear();
                    _BidIncompleteQuotations.Dispose();
                    _BidIncompleteQuotations = null;
                }
            }

            lock (this.ofrIncompleteLockObj)
            {
                if (_OfrIncompleteQuotations != null)
                {
                    _OfrIncompleteQuotations.Table.Clear();
                    _OfrIncompleteQuotations.Dispose();
                    _OfrIncompleteQuotations = null;
                }
            }
            
            #endregion
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
        public void ClearData(bool clearSymbol = false)
        {
            EmsOrdersHandler.Instance.UnregistEMSOrderBySecurityID(this._SecurityID, EmsOrderProcesser.Processer.Instance_OnEmsOrders);
            LogHelper.Info("QuotationListViewModel ClearData UnregisterCTPrice:" + this._SecurityID);
            PriceHandler.Instance.UnregisterCTPrice(this.SecurityID, Instance_OnCTPrice);
            if (clearSymbol)
            {
                Symbol = string.Empty;
                _SecurityID = FullName = string.Empty;
                PriceModel = new PriceModel();
                this.OnPropertyChanged("SecurityID");
                Security = SecurityID;
                SymbolName = string.Empty;
                RemainingTime = string.Empty;
            }


            InitPriceModel();

            #region  Release Object

            ShowImcompletePanel = Visibility.Collapsed;

            EmsOrderProcesser.Processer.DetailDT.TableNewRow -= DetailDT_TableNewRow;
            EmsOrderProcesser.Processer.DetailDT.RowDeleted -= DetailDT_RowDeleted;

          
            //_autoResetEvent.Set();

            lock (stackLock)
            {
                if (_Stack != null)
                {
                    _Stack.Clear();
                }
            }

            lock (cacheStackLock)
            {
                if (_CacheStack != null)
                {
                    _CacheStack.Clear();
                }
            }

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
        }

        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, null);
            if (isAllow)
            {
                ShowOrderBtn = Visibility.Visible;
            }
            else
            {
                ShowOrderBtn = Visibility.Collapsed;
            }
        }


        public void Init()
        {
            BindingOperations.EnableCollectionSynchronization(BidQuotations, bidLockObj);

            BindingOperations.EnableCollectionSynchronization(OfrQuotations, ofrLockObj);

            BindingOperations.EnableCollectionSynchronization(BidQuotations_Frozen, bidLockObj_Frozen);

            BindingOperations.EnableCollectionSynchronization(OfrQuotations_Frozen, ofrLockObj_Frozen);

            BindingOperations.EnableCollectionSynchronization(BidIncompleteQuotations, bidIncompleteLockObj);

            BindingOperations.EnableCollectionSynchronization(OfrIncompleteQuotations, ofrIncompleteLockObj);

            EmsOrderProcesser.Processer.DetailDT.TableNewRow += DetailDT_TableNewRow;
            //EmsOrderProcesser.Processer.DetailDT.RowChanged += DetailDT_RowChanged;
            EmsOrderProcesser.Processer.DetailDT.RowDeleted += DetailDT_RowDeleted;

            //#region 测试数据
            //GenerateTestData();
            //return;
            //#endregion

            InitPriceModel();
        }

        private void InitPriceModel()
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
            _priceModel.BestOfrAmount = "";
            _priceModel.BestOfr = "";
            _priceModel.BestPriceBP = "";
            _priceModel.BestPriceDisplayBP = "";
            _priceModel.BestBidSettler = "";
            _priceModel.BestOfrSettler = "";
            _priceModel.BestBidSettlerYuan = "";
            _priceModel.BestOfrSettlerYuan = "";

            LastExecPrice = "";
            LastExecPriceBP = "";
            LastExecSideShow = "";
            LastExecSource = "";
        }
        
        private ConcurrentQueue<MarketDataSnapshotFullRefresh> _ctPriceQueue = new ConcurrentQueue<MarketDataSnapshotFullRefresh>();
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private object _queueLock = new object();
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
                    {
                        _ctPriceQueue.Enqueue(obj);
                        _autoResetEvent.Set();
                    }
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
                LogHandler.Error(e);
            }
        }



        private void UpdatePrice2(
            DataTable bidNormalDt, DataTable bidCDHDt, DataTable bidIncompleteDt,
            DataTable ofrNormalDt, DataTable ofrCDHDt, DataTable ofrIncompleteDt)
        {
            try
            {
                // 构建bid数据
                lock (bidLockObj)
                {
                    UpdateBidOfrView_Normal("0", bidNormalDt);
                    
                    UpdateBidOfrView_CDH("0", bidCDHDt);
                }

                // 构建ofr数据
                lock (ofrLockObj)
                {
                    UpdateBidOfrView_Normal("1", ofrNormalDt);
                    UpdateBidOfrView_CDH("1", ofrCDHDt);
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
                AdjustData(this.BidQuotations, this.BidQuotations_Frozen,bidLockObj, bidLockObj_Frozen);
                AdjustData(this.OfrQuotations, this.OfrQuotations_Frozen,ofrLockObj, ofrLockObj_Frozen);

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

      

        private volatile bool _NeedUpdate = false;

        private  object cacheStackLock= new object();
        private Stack<MarketDataSnapshotFullRefresh> _CacheStack = new Stack<MarketDataSnapshotFullRefresh>();

        private object stackLock = new object();
        private Stack<CTPriceClass> _Stack = new Stack<CTPriceClass>();


        private void UpdateData(MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {

            //if (marketDataSnapshotFullRefresh != null)// || _NeedUpdate == true)
            {
                if (marketDataSnapshotFullRefresh != null)
                {
                    CTPriceControler.UpdateDataToBaseDV(this.BaseDV, marketDataSnapshotFullRefresh);
                }

                _NeedUpdate = false;
                //_isDetailDtChange = false;

                DataTable tempBaseDt = null;
                lock (this.baseLockObj)
                {
                    try
                    {
                        DataTable tempdt = this.BaseDV.ToTable();
                        tempBaseDt = tempdt.Copy();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                }

                if (tempBaseDt == null)
                {
                    LogHandler.Info("tempBaseDt == null");
                    return;
                }


                DataTable bidNormalDt = null;
                DataTable bidCDHDt = null;
                DataTable ofrNormalDt = null;
                DataTable ofrCDHDt = null;
                DataTable bidIncompleteDt = null;
                DataTable ofrIncompleteDt = null;

                #region DataTable复制
                // 添加和更新(非CDH数据)-bid
                string filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidNormalDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(CDH数据)-bid
                filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidCDHDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(非CDH数据)-ofr
                filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrNormalDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(CDH数据)-ofr
                filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrCDHDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(不完整数据)-bid
                filter = $"{CTPriceControler.SIDE_FIELD} = '0' and {CTPriceControler.ISINCOMPLETE_FIELD} = '1'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                bidIncompleteDt = CTPriceControler.getDataTable(tempBaseDt, filter);

                // 添加和更新(不完整数据)-ofr
                filter = $"{CTPriceControler.SIDE_FIELD} = '1' and {CTPriceControler.ISINCOMPLETE_FIELD} = '1'";
                if (this._rowFilterStr.Length > 0)
                {
                    filter = $"{filter} and ({this._rowFilterStr})";
                }
                ofrIncompleteDt = CTPriceControler.getDataTable(tempBaseDt, filter);
                #endregion

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHelper.Info("Into UpdateData, security:" + (marketDataSnapshotFullRefresh == null ? string.Empty : marketDataSnapshotFullRefresh.SecurityID));
                        }

                        
                        UpdatePrice2(bidNormalDt, bidCDHDt, bidIncompleteDt,
                                    ofrNormalDt, ofrCDHDt, ofrIncompleteDt);

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

                }));
            }
        }

        private void RefreshQueue()
        {
            while (true)
            {
                try
                {
                    MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh = null;
                    if (_ctPriceQueue.TryDequeue(out marketDataSnapshotFullRefresh))
                    {
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHelper.Info("Into refreshQueue, security:" + marketDataSnapshotFullRefresh.SecurityID);
                        }

                        
                        UpdateData(marketDataSnapshotFullRefresh);

                       
                    }
                    else
                        _autoResetEvent.WaitOne();
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

        private void RefreshData()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if ((_Stack.Count > 0 || _isDetailDtChange) && _UIProcessing == false)
                        {
                            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                            {
                                LogHelper.Debug($"QuotationListViewModel RefreshData: _Stack.Count > 0 {_Stack.Count > 0}  _isDetailDtChange {_isDetailDtChange}  _UIProcessing  {_UIProcessing}");
                            }
                                
                            _UIProcessing = true;
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                try
                                {
                                    CTPriceClass ctPriceCls = null;
                                    lock (stackLock)
                                    {
                                        if (_Stack != null && _Stack.Count > 0)
                                        {
                                            CTPriceClass peek = _Stack.Peek();
                                            if (peek != null && peek.updateTime.CompareTo(this.LastUpdateTime) > 0)
                                            {
                                                this.LastUpdateTime = peek.updateTime;
                                                ctPriceCls = _Stack.Pop();
                                                _Stack.Clear();
                                            }
                                        }
                                    }

                                    if (ctPriceCls != null)
                                    {
                                        DataTable bidNormalDt = ctPriceCls.datas[0];
                                        DataTable bidCDHDt = ctPriceCls.datas[1];
                                        DataTable bidIncompleteDt = ctPriceCls.datas[2];
                                        DataTable ofrNormalDt = ctPriceCls.datas[3];
                                        DataTable ofrCDHDt = ctPriceCls.datas[4];
                                        DataTable ofrIncompleteDt = ctPriceCls.datas[5];

                                        UpdatePrice2(bidNormalDt, bidCDHDt, bidIncompleteDt,
                                                    ofrNormalDt, ofrCDHDt, ofrIncompleteDt);
                                    }
                                    else if (_isDetailDtChange)
                                    {
                                        AdjustFlagType();
                                        _isDetailDtChange = false;
                                    }

                                }
                                catch(Exception e)
                                {
                                    LogHandler.Error(e);
                                }                           
                                finally
                                {
                                    _UIProcessing = false;
                                }

                            }));
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                    catch(Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                LogHandler.Error(e);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        /// <summary>
        /// 使用基础数据更新的bidQuotations和ofrQuotations(非CDH数据)
        /// </summary>
        /// <param name="side">0:bid,1:ofr</param>
        private void UpdateBidOfrView_Normal(string side, DataTable tempDt)
        {

                // 选择数据源
                DataView dv = this.BidQuotations;
                if (side == "1")
                {
                    dv = this.OfrQuotations;
                }

                //设置删除标志
                DataRow[] drs = dv.Table.Select($"{CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}')", $"{CTPriceControler.VENUE_TYPE_GW_FIELD}");
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
                        string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                        string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                        string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                        string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                        string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                        string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                        long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
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

                    string quoteStatus = dr[CTPriceControler.QUOTE_STATUS_FIELD] == null ? string.Empty : dr[CTPriceControler.QUOTE_STATUS_FIELD].ToString();

                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];

                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    CTPriceControler.FeedData(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settlType_yuan, firmFlag, 
                            text, price, netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader, 
                            isMe, null, traderName, transactTime, deliveryType, clearingMethod, quoteType, partyName, quoteStatus, isInner);
                    }
                    tempDt.Clear();
                    tempDt.Dispose();
                    tempDt = null;
                }
                

                // 删除DeleteFlag为0的记录
                drs = dv.Table.Select($"{CTPriceControler.DELETE_FLAG_FIELD} = '1'", $"{CTPriceControler.DELETE_FLAG_FIELD}");
                foreach (DataRow dr in drs)
                {
                    dv.Table.Rows.Remove(dr);
                }
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

            //设置删除标志
            foreach (DataRow dr in dv.Table.Rows)
            {
                dr[CTPriceControler.DELETE_FLAG_FIELD] = "1";
            }


            if (tempDt != null && tempDt.Rows.Count > 0)
            {
                int index = tempDt.Rows.Count > 2 ? 2 : tempDt.Rows.Count;

                for (int i = 0; i < index; i++)
                {
                    DataRow dr = tempDt.Rows[i];
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    price = CommonUtils.FormatPrice(price);
                    string netPrice = CommonUtils.ConvertToString(dr[CTPriceControler.NET_PRICE_FIELD]);
                    netPrice = CommonUtils.FormatPrice(netPrice);
                    long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settlType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
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

                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];

                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    CTPriceControler.FeedData(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settlType_yuan, firmFlag, text, price,
                        netPrice, qty, time, mdPriceLevel, partySubName, bargainFlag, wholeFlag, institutionName, showTrader,
                        isMe, null, traderName, transactTime, deliveryType, clearingMethod, quoteType, partyName, isInner);
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
            dv.Table.AcceptChanges();
        }

        /// <summary>
        /// 使用基础数据更新的bidQuotations和ofrQuotations(CDH数据)
        /// </summary>
        /// <param name="side">0:bid,1:ofr</param>
        private void UpdateBidOfrView_CDH(string side, DataTable tempDt)
        {
            // 选择数据源
            DataView dv = this.BidQuotations;
            if (side == "1")
            {
                dv = this.OfrQuotations;
            }

            //设置删除标志
            DataRow[] drs = dv.Table.Select($"{CTPriceControler.VENUE_TYPE_GW_FIELD} in ('{CTPriceControler.CDH}')", $"{CTPriceControler.VENUE_TYPE_GW_FIELD}");
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
                    decimal price = CommonUtils.ConvertToDecimal(dr[CTPriceControler.ENTRY_PX_FIELD] == null ? string.Empty : dr[CTPriceControler.ENTRY_PX_FIELD].ToString());
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 主键
                    string key = $"{venueTypeGW}#{venues}#{price}";
                    // 变动字段
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settleType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();
                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();
                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];
                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";

                    CTPriceControler.FeedData_CDH(dv, key, this.SecurityID, venues, venueTypeGW, side, settlType, settleType_yuan, firmFlag, text, price, netPrice, qty, time, 
                        mdPriceLevel, bargainFlag, wholeFlag, institutionName, showTrader, isMe, partyName, partySubName, isInner);
                }
                tempDt.Clear();
                tempDt.Dispose();
                tempDt = null;
            }

            // 删除DeleteFlag为0的记录
            drs = dv.Table.Select($"{CTPriceControler.DELETE_FLAG_FIELD} = '1'", $"{CTPriceControler.DELETE_FLAG_FIELD}");
            foreach (DataRow dr in drs)
            {
                dv.Table.Rows.Remove(dr);
            }
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
            _priceModel.BestOfrAmount = "";
            _priceModel.BestOfr = "";
            _priceModel.BestPriceBP = "";
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
                            case "ODM":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'ODM'");
                                break;
                            case CommonConstants.AllBroker:
                                venuesFilterBuf.Append($"({CTPriceControler.SOURCE_FIELD} = 'CNEX' or {CTPriceControler.SOURCE_FIELD} = 'TPSC' or {CTPriceControler.SOURCE_FIELD} = 'CCTB' or {CTPriceControler.SOURCE_FIELD} = 'CTTJ' or {CTPriceControler.SOURCE_FIELD} = 'PTCN')");
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
                            case "QDMESP":
                                venuesFilterBuf.Append($"{CTPriceControler.SOURCE_FIELD} = 'QDMESP'");
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
                        }
                        else if (settlType.Content.ToString() == "+0")
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%1%'");
                        }
                        else if (settlType.Content.ToString() == "全")
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%'");
                        }
                        else
                        {
                            settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                        }
                    }
                }
            }
            if (!filterDic.ContainsKey("Settle"))
            {
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
                            }
                            else if (settlType.Content.ToString() == "T+0")
                            {
                                settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} like '%1%'");
                            }
                            else
                            {
                                settlTypeFilterBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
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
            _NeedUpdate = true;

            this.UpdateData(null);
        }

        private void AdjustData(DataView dv, DataView dv_Frozen, object normalLock, object frozenLock)
        {
            object[] itemArr = null;
            lock (normalLock)
            {
                int index = 0;
                long totalQty = 0;
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
                            decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                            drv[CTPriceControler.HIDE_PX_FLAG_FIELD] = (px != px_last || firmflag != firmflag_last) ? "0" : "1";
                            px_last = px;
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

                        // 统计总数
                        long qty = CommonUtils.ConvertToLong(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                        totalQty += qty;
                        drv[CTPriceControler.TOTAL_QTY_FIELD] = totalQty;

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
                    long _totalBid = 0;
                    long _totalOfr = 0;
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
                            _priceModel.BidArrowToolTxt = "本人有报价";
                            _priceModel.BestBidSettler = "";
                            _priceModel.BestBidSettlerYuan = "";
                            _priceModel.BidArrowVisibilly = Visibility.Collapsed;
                            _priceModel.BidIsPermission = "false";
                            _priceModel.BestBidDataRowView = null;
                            if (dv.Count > 0)
                            {
                                bool isFound = false;
                                bool isHaveMeBestPrice = false;
                                bool isHaveMePrice = false;
                                bool isHaveOurPrice = false;
                                List<string> lstsettle = new List<string>();
                                foreach (DataRowView drv in dv)
                                {
                                    if (drv["IsBest"].ToString() == "True")
                                    {
                                        decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                        _BestBid = px.ToString();
                                        _priceModel.BestBid = _BestBid;
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


                                    if (drv["IsMe"].ToString() == "True")
                                    {
                                        isHaveMePrice = true;
                                    }

                                    if ((drv["Source"].ToString() == "IDB") || (drv["Source"].ToString() == "QUANT") || (drv["Source"].ToString() == "MANUAL"))
                                    {
                                        isHaveOurPrice = true;
                                    }

                                    if (CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString()) == CommonUtils.ConvertToDecimal(_priceModel.BestBid.ToString()))
                                    {
                                        long qty = CommonUtils.ConvertToLong(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                                        _totalBid += qty;
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
                                    _priceModel.BidArrowToolTxt = "本人有报价，且为市场最优";
                                    _priceModel.BidArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFF7D00"));
                                    _priceModel.BidArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7D00"));
                                    _priceModel.BidArrowVisibilly = Visibility.Visible;
                                }
                                else
                                {
                                    // 如果没有本人报价，但有本机构报价
                                    if (isHaveOurPrice)
                                    {
                                        _priceModel.BidArrowToolTxt = "本机构有报价";
                                        _priceModel.BidArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
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

                                _totalBid = _totalBid / 1000;
                                _priceModel.BestBidAmount = $"{_totalBid.ToString()} K";

                                if (!isFound)
                                {
                                    _priceModel.BestBid = "--";
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
                            _priceModel.OfrArrowToolTxt = "本人有报价";
                            _priceModel.BestOfrSettler = "";
                            _priceModel.BestOfrSettlerYuan = "";
                            _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                            _priceModel.OfrIsPermission = "false";
                            _priceModel.BestOfrDataRowView = null;
                            if (dv.Count > 0)
                            {
                                bool isFound = false;
                                bool isHaveMeBestPrice = false;
                                bool isHaveMePrice = false;
                                bool isHaveOurPrice = false;
                                List<string> lstsettle = new List<string>();
                                foreach (DataRowView drv in dv)
                                {
                                    if (drv["IsBest"].ToString() == "True")
                                    {
                                        decimal px = CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString());
                                        _BestOfr = px.ToString();
                                        _priceModel.BestOfr = _BestOfr;
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

                                    if (CommonUtils.ConvertToDecimal(drv[CTPriceControler.ENTRY_PX_FIELD].ToString()) == CommonUtils.ConvertToDecimal(_priceModel.BestOfr.ToString()))
                                    {
                                        long qty = CommonUtils.ConvertToLong(drv[CTPriceControler.QTY_FIELD] == null ? string.Empty : drv[CTPriceControler.QTY_FIELD].ToString());
                                        _totalOfr += qty;
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
                                        _priceModel.BestBidSettler = "多";
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
                                    _priceModel.OfrArrowToolTxt = "本人有报价，且为市场最优";
                                    _priceModel.OfrArrowFill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C009AFF"));
                                    _priceModel.OfrArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
                                    _priceModel.OfrArrowVisibilly = Visibility.Visible;
                                }
                                else
                                {
                                    // 如果没有本人报价，但有本机构报价
                                    if (isHaveOurPrice)
                                    {
                                        _priceModel.OfrArrowToolTxt = "本机构有报价";
                                        _priceModel.OfrArrowStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
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
                                    _priceModel.BestOfrAmount = "";
                                    _priceModel.BestOfrSettler = "";
                                    _priceModel.OfrArrowVisibilly = Visibility.Collapsed;
                                }

                                _totalOfr = _totalOfr / 1000;
                                _priceModel.BestOfrAmount = $"{_totalOfr.ToString()} K";
                            }
                        }
                    }

                    if ((string.IsNullOrEmpty(_priceModel.BestBid)) | (string.IsNullOrEmpty(_priceModel.BestOfr)))
                    {
                        _priceModel.BestPriceBP = "--";
                        _priceModel.BestPriceDisplayBP = "--";
                    }

                    if ((_priceModel.BestBid=="--") | (_priceModel.BestOfr == "--"))
                    {
                        _priceModel.BestPriceBP = "--";
                        _priceModel.BestPriceDisplayBP = "--";
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
            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(venueTypeGw) == false && venueTypeGw != "IDB" && venueTypeGw != "INNER")
            {
                string qty_text = CommonUtils.ConvertToString(drv[CTPriceControler.QTY_FIELD]);
                text = qty_text;
                drv[CTPriceControler.TEXT_FIELD] = qty_text;
            }

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
                    count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeItem, venues, venueTypeGw, px, side);
                }

                string[] settlType_yuan_Arr = settleType_Yuan.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (string settleTypeYuanItem in settlType_yuan_Arr)
                {
                    count += EmsOrderProcesser.Processer.GetCacheEmsOrders(securtityID, settleTypeYuanItem, venues, venueTypeGw, px, side);
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
        private void Instance_OnMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj.SecurityID == this.Security)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                     UpdateLastExec(obj);

                    lock (_queueLock)
                    {
                        bool showFwd = CheckPwd(obj.SecurityID);
                        if (showFwd)
                        {
                            if (this.OnYunEvent != null)
                            {
                                OnYunEvent?.Invoke(obj);
                            }
                        }
                    }
                }));
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
            lock (_queueLock)
            {
                IEnumerable<MarketExecutionsItem> res = LastExecutionProcesser.Processer.GetDatasBySecurityID(securityid).Where(orderItem => IsFwd(orderItem.SettlType));
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
            if (string.IsNullOrEmpty(obj.LastPx))
                return;

            LastPriceParam lastPriceParam = LastExecutionProcesser.Processer.CalLastExe(obj);
            if (string.IsNullOrEmpty(LastExecTime))
            {
                LastExecTime = obj.TransactTime;
            }

            if (!string.IsNullOrEmpty(LastExecTime) || !string.IsNullOrEmpty(obj.TransactTime))
            {
                if (LastExecTime.CompareTo(obj.TransactTime) > 0)
                {
                    return;
                }
            }

            decimal bpd = 0;
            if (decimal.TryParse(lastPriceParam.Bp, out bpd))
            {
                LastExecPriceBP = (bpd > 0 ? "+" : "-") + PriceCalHelper.Instance.RemoveZero(Math.Abs(bpd).ToString())+" bp";
            }
            else
                LastExecPriceBP = "";

            string tmppric = CommonUtils.FormatNum(obj.LastPx);
            LastExecPrice = tmppric == "0.00" ? "" : tmppric;

            LastExecSideShow = ComboSourceUtil.Instance.FindEnum(obj.Side, "side2"); ;

            LastExecSource = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venues");

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
            MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(secid);
            if (lastExeItem != null)
            {
                ExecutionReport temOrder = new ExecutionReport();
                temOrder.TransactTime = lastExeItem.TimeStamp;
                temOrder.SecurityID = secid;
                temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                temOrder.Venues = lastExeItem.SourceTrader;

                if (string.IsNullOrEmpty(LastExecPrice))
                {
                    string tmppric = CommonUtils.FormatNum(temOrder.LastPx);
                    LastExecPrice = tmppric == "0.00" ? "" : tmppric;
                    LastExecSideShow = ComboSourceUtil.Instance.FindEnum(temOrder.Side, "side2"); ;
                    LastExecSource = ComboSourceUtil.Instance.FindEnum(temOrder.Venues, "venues");
                }
                this.OnLastExec(temOrder);
            }
        }

        public void ClearLastExecutionsItem(string secid)
        {
            LastExecutionProcesser.Processer.ClearLastExecutionsItem(secid);
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
        private bool startTestFlag = false;
        private int i = 0;
        public void GenerateTestData()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    if (this.startTestFlag)
                    {
                        if (string.IsNullOrEmpty(this.SecurityID) == true)
                        {
                            //Thread.Sleep(1000);
                            //continue;
                        }

                        // MarketDataSnapshotFullRefresh ctPrice = STC.XBond.Plugin.Test.TestDataGenerator.GetCTPrice(this.SecurityID);
                        MarketDataSnapshotFullRefresh ctPrice = new MarketDataSnapshotFullRefresh();
                        if (i % 2 == 0)
                        {
                            ctPrice.CCDCValuation = "3.2";
                            ctPrice.ClOrdID = "1623117498307";
                            ctPrice.MDBookType = "401";
                            ctPrice.MarketIndicator = "4";
                            ctPrice.SecurityID = "150210.IB";
                            ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                            ctPrice.Symbol = "15国开10";
                            ctPrice.VenueTypeGW = "ct";

                            MarketDataSnapshotFullRefresh.GNoMDEntries data1 = new MarketDataSnapshotFullRefresh.GNoMDEntries();
                            data1.FirmFlag = "1";
                            data1.InstitutionID = string.Empty;
                            data1.InstitutionName = string.Empty;
                            data1.InternalFlag = "1";
                            data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                            data1.MDEntryID = "s851761298516574208";
                            data1.MDEntryPx = "4";
                            data1.MDEntrySize = "2000";
                            data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                            data1.MDEntryType = "0";
                            data1.MDPriceLevel = "1";
                            data1.MDReqID = string.Empty;
                            data1.NetPrice = "100.7239";
                            data1.PartySubID = "193";
                            data1.PartySubName = "严晓莉";
                            data1.RoleName = "管理员";
                            data1.SettlType = "2";
                            data1.TraderID = string.Empty;
                            data1.TraderName = string.Empty;
                            data1.VenueTypeGW = "INNER";
                            data1.Venues = "NANUAL";
                            data1.WholeFlag = "0";
                            data1.Yield = "4";

                            MarketDataSnapshotFullRefresh.GNoMDEntries data2 = new MarketDataSnapshotFullRefresh.GNoMDEntries();
                            data2.FirmFlag = "1";
                            data2.InstitutionID = string.Empty;
                            data2.InstitutionName = string.Empty;
                            data2.InternalFlag = "1";
                            data2.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                            data2.MDEntryID = "s851762052480466944";
                            data2.MDEntryPx = "5";
                            data2.MDEntrySize = "2000";
                            data2.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                            data2.MDEntryType = "0";
                            data2.MDPriceLevel = "2";
                            data2.MDReqID = string.Empty;
                            data2.NetPrice = "97.2847";
                            data2.PartySubID = "193";
                            data2.PartySubName = "严晓莉";
                            data2.RoleName = "管理员";
                            data2.SettlType = "2";
                            data2.TraderID = string.Empty;
                            data2.TraderName = string.Empty;
                            data2.VenueTypeGW = "INNER";
                            data2.Venues = "NANUAL";
                            data2.WholeFlag = "0";
                            data2.Yield = "5";

                            List<MarketDataSnapshotFullRefresh.GNoMDEntries> list = new List<MarketDataSnapshotFullRefresh.GNoMDEntries>();
                            list.Add(data1);
                            list.Add(data2);
                            ctPrice.NoMDEntries = list;
                            
                        }
                        else
                        {
                            ctPrice.CCDCValuation = "3.2";
                            ctPrice.ClOrdID = "1623117498307";
                            ctPrice.MDBookType = "401";
                            ctPrice.MarketIndicator = "4";
                            ctPrice.SecurityID = "150210.IB";
                            ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                            ctPrice.Symbol = "15国开10";
                            ctPrice.VenueTypeGW = "ct";

                            MarketDataSnapshotFullRefresh.GNoMDEntries data1 = new MarketDataSnapshotFullRefresh.GNoMDEntries();
                            data1.FirmFlag = "1";
                            data1.InstitutionID = string.Empty;
                            data1.InstitutionName = string.Empty;
                            data1.InternalFlag = "1";
                            data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                            data1.MDEntryID = "s851761298516574208";
                            data1.MDEntryPx = "4";
                            data1.MDEntrySize = "2000";
                            data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                            data1.MDEntryType = "0";
                            data1.MDPriceLevel = "1";
                            data1.MDReqID = string.Empty;
                            data1.NetPrice = "100.7239";
                            data1.PartySubID = "193";
                            data1.PartySubName = "严晓莉";
                            data1.RoleName = "管理员";
                            data1.SettlType = "2";
                            data1.TraderID = string.Empty;
                            data1.TraderName = string.Empty;
                            data1.VenueTypeGW = "INNER";
                            data1.Venues = "NANUAL";
                            data1.WholeFlag = "0";
                            data1.Yield = "4";

                            MarketDataSnapshotFullRefresh.GNoMDEntries data2 = new MarketDataSnapshotFullRefresh.GNoMDEntries();
                            data2.FirmFlag = "1";
                            data2.InstitutionID = string.Empty;
                            data2.InstitutionName = string.Empty;
                            data2.InternalFlag = "1";
                            data2.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                            data2.MDEntryID = "s851762052480466944";
                            data2.MDEntryPx = "5";
                            data2.MDEntrySize = "2000";
                            data2.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                            data2.MDEntryType = "0";
                            data2.MDPriceLevel = "2";
                            data2.MDReqID = string.Empty;
                            data2.NetPrice = "97.2847";
                            data2.PartySubID = "193";
                            data2.PartySubName = "严晓莉";
                            data2.RoleName = "管理员";
                            data2.SettlType = "2";
                            data2.TraderID = string.Empty;
                            data2.TraderName = string.Empty;
                            data2.VenueTypeGW = "INNER";
                            data2.Venues = "NANUAL";
                            data2.WholeFlag = "0";
                            data2.Yield = "5";

                            List<MarketDataSnapshotFullRefresh.GNoMDEntries> list = new List<MarketDataSnapshotFullRefresh.GNoMDEntries>();
                            list.Add(data1);
                            list.Add(data2);
                            ctPrice.NoMDEntries = list;
                        }
                        
                        Instance_OnCTPrice(ctPrice);
                        i = i % 2;
                        i++;
                    }

                    Thread.Sleep(1000);
                }
            });
        }
        #endregion
        private class CTPriceClass
        {
            public DateTime updateTime = DateTime.MinValue;

            public DataTable[] datas = null;
        }
    }
}
