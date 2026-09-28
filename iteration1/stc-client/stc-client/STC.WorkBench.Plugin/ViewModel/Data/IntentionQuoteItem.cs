using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WtUtils = STC.WorkBench.Plugin.Utils.CommonUtils;

namespace STC.WorkBench.Plugin.ViewModel.Data
{
    public class IntentionQuoteItem : ViewModelBase
    {
        private bool _isChecked = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShow = false;

        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                this.OnPropertyChanged();
            }
        }

        private string _intentionId;

        public string IntentionId
        {
            get { return _intentionId; }
            set {
                _intentionId = value;
                this.OnPropertyChanged();
            }
        }

        private bool isSysAuto_Institution = false;

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                if (_selectedInstitution_Edit != value)
                {
                    _selectedInstitution_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _institutionName_Edit;

        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_institutionName_Edit) && string.IsNullOrEmpty(value)) && _institutionName_Edit != value)
                {
                    //if (isSysAuto_Institution == false
                    //&& _institutionName_Edit != value)
                    //{
                    //    isSysAuto_Trader = true;
                    //    SelectedTrader_Edit = null;
                    //    TraderName_Edit = string.Empty;
                    //}
                    //isSysAuto_Institution = false;

                    _institutionName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        //private string _institutionName_Text;

        //public string InstitutionName_Text
        //{
        //    get { return _institutionName_Text; }
        //    set
        //    {
        //        _institutionName_Text = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        private string _institutionId;

        public string InstitutionId
        {
            get { return _institutionId; }
            set
            {
                _institutionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName;

        public string InstitutionName
        {
            get { return _institutionName; }
            set
            {
                _institutionName = value;
                this.OnPropertyChanged();
            }
        }

        private bool isSysAuto_Trader = false;

        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {

                if (_selectedTrader_Edit != value)
                {
                    // 只有crmContact中才有机构信息，可以联动显示
                    if (value != null)
                    {
                        if (SelectedInstintuion_Edit == null || SelectedInstintuion_Edit.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                isSysAuto_Institution = true;
                                isSysAuto_Trader = true;
                                SelectedInstintuion_Edit = results[0];
                                InstitutionName_Edit = results[0].shortnamecn;
                            }
                        }

                        if (SelectedInstintuion_Edit != null && SelectedInstintuion_Edit.idbkey == value.instid)
                        {
                            isSysAuto_Institution = false;
                            isSysAuto_Trader = true;
                            SelectedInstintuion_Edit = SelectedInstintuion_Edit;
                            InstitutionName_Edit = InstitutionName_Edit;
                        }
                    }

                    _selectedTrader_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_traderName_Edit) && string.IsNullOrEmpty(value)) && _traderName_Edit != value)
                {
                    //if (isSysAuto_Trader == false
                    //    && _traderName_Edit != value)
                    //{
                    //    isSysAuto_Institution = true;
                    //    SelectedInstintuion_Edit = null;
                    //    InstitutionName_Edit = string.Empty;
                    //}
                    isSysAuto_Trader = false;

                    _traderName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderId;

        public string TraderId
        {
            get { return _traderId; }
            set
            {
                _traderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName;

        public string TraderName
        {
            get { return _traderName; }
            set
            {
                _traderName = value;
                this.OnPropertyChanged();
            }
        }

        //private string _traderName_Text;

        //public string TraderName_Text
        //{
        //    get { return _traderName_Text; }
        //    set
        //    {
        //        _traderName_Text = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        private string _qqNumber;

        public string QqNumber
        {
            get { return _qqNumber; }
            set
            {
                _qqNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _side_Edit;

        public string Side_Edit
        {
            get { return _side_Edit; }
            set
            {
                _side_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _side;

        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityId;

        public string SecurityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _cleanPrice;

        public decimal? CleanPrice
        {
            get { return _cleanPrice; }
            set
            {
                _cleanPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _fullPrice;

        public decimal? FullPrice
        {
            get { return _fullPrice; }
            set
            {
                _fullPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _CustomerPrice;

        public decimal? CustomerPrice
        {
            get { return _CustomerPrice; }
            set
            {
                _CustomerPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _CustomerPrice_Edit;

        public decimal? CustomerPrice_Edit
        {
            get { return _CustomerPrice_Edit; }
            set
            {
                _CustomerPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _BP;

        public decimal? BP
        {
            get { return _BP; }
            set
            {
                _BP = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _BP_Edit;

        public decimal? BP_Edit
        {
            get { return _BP_Edit; }
            set
            {
                _BP_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price_Edit;

        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price;

        public decimal? Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    if (!value.HasValue)
                    {
                        this.CCDCValuation_BP_InnerBid = null;
                        this.CCDCValuation_BP_InnerOfr = null;
                        this.CsiValuation_BP_InnerBid = null;
                        this.CsiValuation_BP_InnerOfr = null;
                    }
                    else
                    {
                        if (value >= -100 && value < 9999)
                        {
                            if (this.ChinaBondExercise.HasValue && value.HasValue)
                            {
                                if (this.Side == "1")
                                    this.CCDCValuation_BP_InnerBid = (value - this.ChinaBondExercise) * 100;
                                else if (this.Side == "2")
                                    this.CCDCValuation_BP_InnerOfr = (this.ChinaBondExercise - value) * 100;
                            }
                            else if (this.ChinaBondDue.HasValue && value.HasValue)
                            {
                                if (this.Side == "1")
                                    this.CCDCValuation_BP_InnerBid = (value - this.ChinaBondDue) * 100;
                                else if (this.Side == "2")
                                    this.CCDCValuation_BP_InnerOfr = (this.ChinaBondDue - value) * 100;
                            }
                            if (this.CsiBondValuationExercise.HasValue && value.HasValue)
                            {
                                if (this.Side == "1")
                                    this.CsiValuation_BP_InnerBid = (value - this.CsiBondValuationExercise) * 100;
                                else if (this.Side == "2")
                                    this.CsiValuation_BP_InnerOfr = (this.CsiBondValuationExercise - value) * 100;
                            }
                            else if (this.CsiBondValuationDue.HasValue && value.HasValue)
                            {
                                if (this.Side == "1")
                                    this.CsiValuation_BP_InnerBid = (value - this.CsiBondValuationDue) * 100;
                                else if (this.Side == "2")
                                    this.CsiValuation_BP_InnerOfr = (this.CsiBondValuationDue - value) * 100;
                            }
                        }
                    }

                    _price = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _price_display;

        public decimal? Price_Display
        {
            get { return _price_display; }
            set
            {
                _price_display = value;
                this.OnPropertyChanged();
            }
        }

        private int _deciamlLength;

        public int DeciamlLength
        {
            get { return _deciamlLength; }
            set
            {
                _deciamlLength = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _rangeValue;
        public decimal RangeValue
        {
            get { return _rangeValue; }
            set
            {
                _rangeValue = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _qty_Edit;

        public decimal? Qty_Edit
        {
            get { return _qty_Edit; }
            set
            {
                _qty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty;

        public decimal? Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _executionqty;

        public decimal? ExecutionQty
        {
            get { return _executionqty; }
            set
            {
                _executionqty = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType;

        public string SettlType
        {
            get { return _settlType; }
            set
            {
                _settlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType_edit;

        public string SettlType_Edit
        {
            get { return _settlType_edit; }
            set
            {
                _settlType_edit = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _edit_settltype_date;

        public DateTime edit_settltype_date
        {
            get { return _edit_settltype_date; }
            set
            {
                _edit_settltype_date = value;
                OnPropertyChanged();
            }
        }

        private int _edit_settletype = int.MinValue;

        public int edit_settletype
        {
            get { return _edit_settletype; }
            set
            {
                _edit_settletype = value;
                this.OnPropertyChanged();
            }
        }

        private string _brokerId = LocalEnv.UserInfo.UserName;

        public string BrokerId
        {
            get { return _brokerId; }
            set
            {
                _brokerId = value;
                this.OnPropertyChanged();
            }
        }

        private string _brokerName = LocalEnv.UserInfo.FullName;

        public string BrokerName
        {
            get { return _brokerName; }
            set
            {
                _brokerName = value;
                this.OnPropertyChanged();
            }
        }

        private string _roleId;

        public string RoleId
        {
            get { return _roleId; }
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _roleName;

        public string RoleName
        {
            get { return _roleName; }
            set
            {
                if (_roleName != value)
                {
                    _roleName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private StcTbookUsers _selectedBroker_Edit;

        public StcTbookUsers SelectedBroker_Edit
        {
            get { return _selectedBroker_Edit; }
            set
            {
                if (_selectedBroker_Edit != value)
                {
                    _selectedBroker_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _brokerName_Edit;

        public string BrokerName_Edit
        {
            get { return _brokerName_Edit; }
            set
            {
                _brokerName_Edit = value;
                this.OnPropertyChanged();
            }
        }


        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                this.OnPropertyChanged();
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnPropertyChanged();

                if (value == "0" || value == "1"
                    || value == "81" || value == "80")
                {
                    Enabled_Inquiry = true;
                }
                else
                {
                    Enabled_Inquiry = false;
                }
            }
        }

        private bool _enabled_Inquiry;

        public bool Enabled_Inquiry
        {
            get { return _enabled_Inquiry; }
            set
            {
                _enabled_Inquiry = value;
                this.OnPropertyChanged();
            }
        }

        private string _inquiryStatus;

        public string InquiryStatus
        {
            get { return _inquiryStatus; }
            set
            {
                _inquiryStatus = value;
                this.OnPropertyChanged();
            }
        }

        public string _status_display;

        public string Status_Display
        {
            get { return _status_display; }
            set
            {
                this._status_display = value;
                this.OnPropertyChanged();
            }
        }

        public string _status_tooltip_display;

        public string Status_Tooltip_Display
        {
            get { return _status_tooltip_display; }
            set
            {
                this._status_tooltip_display = value;
                this.OnPropertyChanged();
            }
        }

        private string _type = string.Empty;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set
            {
                _createTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _closeBy;

        public string CloseBy
        {
            get { return _closeBy; }
            set
            {
                _closeBy = value;
                this.OnPropertyChanged();
            }
        }

        private string _closeByName;

        public string CloseByName
        {
            get { return _closeByName;  }
            set
            {
                _closeByName = value;
                this.OnPropertyChanged();
            }
        }

        private string _orderID;

        public string OrderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
                this.OnPropertyChanged();
            }
        }


        private string _isEdit = "2"; // 0:显示 2：不显示

        public string IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                _isEdit = value;
                this.OnPropertyChanged();
            }
        }

        // 序号
        private int? _no;

        public int? No
        {
            get
            {
                return _no;
            }
            set
            {
                _no = value;
                this.OnPropertyChanged();
            }
        }

        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                this.OnPropertyChanged();
            }
        }

        public STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest UpdateIntentionRequest { get; set; }

        public STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest Request { get; set; }

        public IntentionInquiryRequest InquiryRequest { get; set; }

        public Common.Model.Fix.Message.NewOrderSingle SingleOrder { get; set; }

        private bool _isConfirmed;
        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                _isConfirmed = value;
                this.OnPropertyChanged();
            }
        }


        private string _institutionInit;

        public string InstitutionInit
        {
            get
            {
                return _institutionInit;
            }
            set
            {
                _institutionInit = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderInit;

        public string TraderInit
        {
            get
            {
                return _traderInit;
            }
            set
            {
                _traderInit = value;
                this.OnPropertyChanged();
            }
        }


        private string _wholeFlag_Edit;

        public string WholeFlag_Edit
        {
            get
            {
                return _wholeFlag_Edit;
            }
            set
            {
                _wholeFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlag;

        public string WholeFlag
        {
            get
            {
                return _wholeFlag;
            }
            set
            {
                _wholeFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private decimal _remaingingTime_convert;
        public decimal RemaingingTime_convert
        {
            get { return _remaingingTime_convert; }
            set { _remaingingTime_convert = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 中债
        /// </summary>
        private decimal? _ccdcvaluation;

        public decimal? CCDCValuation
        {
            get
            {
                return _ccdcvaluation;
            }
            set
            {
                if (_ccdcvaluation != value)
                {
                    _ccdcvaluation = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债估值偏离
        /// </summary>
        private decimal? _ccdcvaluation_bp;

        public decimal? CCDCValuation_BP
        {
            get
            {
                return _ccdcvaluation_bp;
            }
            set
            {
                if (_ccdcvaluation_bp != value)
                {
                    _ccdcvaluation_bp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债估值偏离（InnerBid）
        /// </summary>
        private decimal? _ccdcvaluation_bp_InnerBid;

        public decimal? CCDCValuation_BP_InnerBid
        {
            get
            {
                return _ccdcvaluation_bp_InnerBid;
            }
            set
            {
                if (_ccdcvaluation_bp_InnerBid != value)
                {
                    _ccdcvaluation_bp_InnerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债估值偏离（InnerOfr）
        /// </summary>
        private decimal? _ccdcvaluation_bp_InnerOfr;

        public decimal? CCDCValuation_BP_InnerOfr
        {
            get
            {
                return _ccdcvaluation_bp_InnerOfr;
            }
            set
            {
                if (_ccdcvaluation_bp_InnerOfr != value)
                {
                    _ccdcvaluation_bp_InnerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 中证估值偏离
        /// </summary>
        private decimal? _csiValuation_bp;

        public decimal? CsiValuation_BP
        {
            get
            {
                return _csiValuation_bp;
            }
            set
            {
                if (_csiValuation_bp != value)
                {
                    _csiValuation_bp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中证估值偏离（InnerBid）
        /// </summary>
        private decimal? _csiValuation_bp_InnerBid;

        public decimal? CsiValuation_BP_InnerBid
        {
            get
            {
                return _csiValuation_bp_InnerBid;
            }
            set
            {
                if (_csiValuation_bp_InnerBid != value)
                {
                    _csiValuation_bp_InnerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中证估值偏离（InnerOfr）
        /// </summary>
        private decimal? _csiValuation_bp_InnerOfr;

        public decimal? CsiValuation_BP_InnerOfr
        {
            get
            {
                return _csiValuation_bp_InnerOfr;
            }
            set
            {
                if (_csiValuation_bp_InnerOfr != value)
                {
                    _csiValuation_bp_InnerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 中债日期
        /// </summary>
        private string _ccdcvaluation_date;

        public string CCDCValuation_Date
        {
            get
            {
                return _ccdcvaluation_date;
            }
            set
            {
                if (_ccdcvaluation_date != value)
                {
                    _ccdcvaluation_date = value;
                    this.OnPropertyChanged();
                }
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
                this.OnPropertyChanged();
            }
        }

        private decimal? _best_bid;

        public decimal? Best_Bid
        {
            get
            {
                return _best_bid;
            }
            set
            {
                if (_best_bid != value)
                {
                    _best_bid = value;
                    this.OnPropertyChanged();
                    //BestBid_FC.BeginAnimation(SolidColorBrush.ColorProperty, BID_CA); // 颜色绘制使用动画绘制
                }
            }
        }


        private decimal? _best_ofr;

        public decimal? Best_Ofr
        {
            get
            {
                return _best_ofr;
            }
            set
            {
                if (_best_ofr != value)
                {
                    _best_ofr = value;
                    this.OnPropertyChanged();
                    //BestOfr_FC.BeginAnimation(SolidColorBrush.ColorProperty, OFR_CA); // 颜色绘制使用动画绘制
                }
            }
        }

        private string _Best_Bid_PriceType;
        public string Best_Bid_PriceType
        {
            get
            {
                return _Best_Bid_PriceType;
            }
            set
            {
                if (_Best_Bid_PriceType != value)
                {
                    _Best_Bid_PriceType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Best_Ofr_PriceType;

        public string Best_Ofr_PriceType
        {
            get 
            { 
                return _Best_Ofr_PriceType; 
            }
            set
            {
                if (_Best_Ofr_PriceType != value)
                {
                    _Best_Ofr_PriceType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        //private static ColorAnimation BID_CA = new ColorAnimation()
        //{
        //    From = (Color)ColorConverter.ConvertFromString("#4CFB943C"),
        //    To = (Color)ColorConverter.ConvertFromString("#FB943C"),
        //    AutoReverse = false, // 正常播放
        //    RepeatBehavior = new RepeatBehavior(3),
        //    Duration = new Duration(TimeSpan.FromSeconds(1)) // 动画一次所用时间
        //};//动画效果
        //private SolidColorBrush _bestBid_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB943C"));

        //public SolidColorBrush BestBid_FC
        //{
        //    get
        //    {
        //        return _bestBid_FC;
        //    }
        //    set
        //    {
        //        if (_bestBid_FC != value)
        //        {
        //            _bestBid_FC = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        //private static ColorAnimation OFR_CA = new ColorAnimation()
        //{
        //    From = (Color)ColorConverter.ConvertFromString("#4C58B3FF"),
        //    To = (Color)ColorConverter.ConvertFromString("#58B3FF"),
        //    AutoReverse = false, // 正常播放
        //    RepeatBehavior = new RepeatBehavior(3),
        //    Duration = new Duration(TimeSpan.FromSeconds(1)), // 动画一次所用时间
        //}; //动画效果
        //private SolidColorBrush _bestOfr_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58B3FF"));

        //public SolidColorBrush BestOfr_FC
        //{
        //    get
        //    {
        //        return _bestOfr_FC;
        //    }
        //    set
        //    {
        //        if (_bestOfr_FC != value)
        //        {
        //            _bestOfr_FC = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        private string _accountID;

        public string AccountID
        {
            get { return _accountID; }
            set 
            {   _accountID = value;
                this.OnPropertyChanged();
            }
        }

        private string _accountName;

        public string AccountName
        {
            get { return _accountName; }
            set { _accountName = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isOpposite = true;

        public bool IsOpposite
        {
            get { return _isOpposite; }
            set 
            { 
                _isOpposite = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isSame = false;

        public bool IsSame
        {
            get { return _isSame; }
            set
            {
                _isSame = value;
                this.OnPropertyChanged();
            }
        }

        public string vipCode;//"0001"（勾选）、"0010"(不勾选）
        public string orgType;//"0001"（银行配置户）、"0010"（券商）、"0011"（基金）、"0100"（资管）、"0101"（保险）、"0110"（其他）、"0111"（银行交易户）、"1000"（理财子公司）、"1001"（期货机构）、"1010"（信托）
        public string ifSubBond;//"BUY"(同向）,"SELL"（反向）,"ALL"（全选）
        public string validUntilTime;
        public string Info1; // 与validUnitlTime配合使用
        public string Info2;
        public string Info5;

        //IRS原文类别  IRS
        public string ParseType;

        private string _clordid;

        /// <summary>
        /// 客户订单编号
        /// </summary>
        public string ClOrdID
        {
            get
            {
                return _clordid;
            }
            set
            {
                _clordid = value;
                this.OnPropertyChanged();
            }
        }

        //到期时间
        private string _timeToMaturity;

        public string TimeToMaturity
        {
            get { return _timeToMaturity; }
            set
            {
                _timeToMaturity = value;
                this.OnPropertyChanged();
            }
        }

        //到期时间_转换
        private decimal _timeToMaturity_Convert;

        public decimal TimeToMaturity_Convert
        {
            get { return _timeToMaturity_Convert; }
            set
            {
                _timeToMaturity_Convert = value;
                this.OnPropertyChanged();
            }
        }

        //到期日距离下个工作日天数
        private string _timeToMaturityToWorkDay;

        public string TimeToMaturityToWorkDay
        {
            get { return _timeToMaturityToWorkDay; }
            set
            {
                _timeToMaturityToWorkDay = value;
                this.OnPropertyChanged();
            }
        }

        // 价格类型
        private string _priceType;

        public string PriceType
        {
            get { return _priceType; }
            set
            {
                _priceType = value;
                this.OnPropertyChanged();
            }
        }

        private string _priceType_Edit;

        public string PriceType_Edit
        {
            get { return _priceType_Edit; }
            set
            {
                _priceType_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<KeyValuePair<string, string>> _priceType_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeDic
        {
            get
            {
                if (_priceType_Dic.Count == 0)
                {
                    _priceType_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_Dic;
            }
        }

        private string _firmFlag;

        public string FirmFlag
        {
            get { return _firmFlag; }
            set
            {
                _firmFlag = value;
                this.OnPropertyChanged();
            }
        }

        #region 标签
        // 急
        private string _HurryFlag_Edit;

        public string HurryFlag_Edit
        {
            get
            {
                return _HurryFlag_Edit;
            }
            set
            {
                _HurryFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _HurryFlag;

        public string HurryFlag
        {
            get
            {
                return _HurryFlag;
            }
            set
            {
                _HurryFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 荐
        private string _SuggestFlag_Edit;

        public string SuggestFlag_Edit
        {
            get
            {
                return _SuggestFlag_Edit;
            }
            set
            {
                _SuggestFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _SuggestFlag;

        public string SuggestFlag
        {
            get
            {
                return _SuggestFlag;
            }
            set
            {
                _SuggestFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 火
        private string _FireFlag_Edit;

        public string FireFlag_Edit
        {
            get
            {
                return _FireFlag_Edit;
            }
            set
            {
                _FireFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _FireFlag;

        public string FireFlag
        {
            get
            {
                return _FireFlag;
            }
            set
            {
                _FireFlag = value;
                this.OnPropertyChanged();
            }
        }

        // 底价
        private string _FloorPrice_Edit;

        public string FloorPrice_Edit
        {
            get
            {
                return _FloorPrice_Edit;
            }
            set
            {
                _FloorPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _FloorPrice;

        public string FloorPrice
        {
            get
            {
                return _FloorPrice;
            }
            set
            {
                _FloorPrice = value;
                this.OnPropertyChanged();
            }
        }

        // oco, 打包

        private string _OCOFlag_Edit;

        public string OCOFlag_Edit
        {
            get
            {
                return _OCOFlag_Edit;
            }
            set
            {
                _OCOFlag_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _OCOFlag;

        public string OCOFlag
        {
            get
            {
                return _OCOFlag;
            }
            set
            {
                _OCOFlag = value;
                this.OnPropertyChanged();
            }
        }

        //备注
        private string _PublicRemarks_Edit;

        public string PublicRemarks_Edit
        {
            get
            {
                return _PublicRemarks_Edit;
            }
            set
            {
                _PublicRemarks_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _PublicRemarks;

        public string PublicRemarks
        {
            get
            {
                return _PublicRemarks;
            }
            set
            {
                _PublicRemarks = value;
                this.OnPropertyChanged();
            }
        }

        //本人备注
        private string _PersonalRemarks_Edit;

        public string PersonalRemarks_Edit
        {
            get
            {
                return _PersonalRemarks_Edit;
            }
            set
            {
                _PersonalRemarks_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _PersonalRemarks;

        public string PersonalRemarks
        {
            get
            {
                return _PersonalRemarks;
            }
            set
            {
                _PersonalRemarks = value;
                this.OnPropertyChanged();
            }
        }

        private string _Channel;

        public string Channel
        {
            get
            {
                return _Channel;
            }
            set
            {
                _Channel = value;
                this.OnPropertyChanged();
            }
        }

        private string _IM;

        public string IM
        {
            get
            {
                return _IM;
            }
            set
            {
                _IM = value;
                this.OnPropertyChanged();
            }
        }

        private string _AddVisible;

        public string AddVisible
        {
            get
            {
                return _AddVisible;
            }
            set
            {
                _AddVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _ToGroupName;

        public string ToGroupName
        {
            get
            {
                return _ToGroupName;
            }
            set
            {
                _ToGroupName = value;
                this.OnPropertyChanged();
            }
        }

        private TagData _FQInfo;

        public TagData FQInfo
        {
            get
            {
                return _FQInfo;
            }
            set
            {
                _FQInfo = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isMe;

        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                if (_isMe != value)
                {
                    _isMe = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _securityType;
        public string SecurityType
        {
            get { return _securityType; }
            set { _securityType = value; this.OnPropertyChanged(); }
        }

        private string _bondSubType;

        public string BondSubType
        {
            get { return _bondSubType; }
            set 
            { 
                _bondSubType = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _termDays;
        public decimal? TermDays
        {
            get { return _termDays; }
            set { _termDays = value; this.OnPropertyChanged(); }
        }

        private decimal? _couponRateCurrent;

        public decimal? CouponRateCurrent
        {
            get { return _couponRateCurrent; }
            set { _couponRateCurrent = value; this.OnPropertyChanged(); }
        }



        private string _issuerCode;
        public string IssuerCode
        {
            get { return _issuerCode; }
            set { _issuerCode = value; this.OnPropertyChanged(); }
        }

        private string _issuerName;
        public string IssuerName
        {
            get { return _issuerName; }
            set { _issuerName = value; this.OnPropertyChanged(); }
        }

        private string _warranterCode;

        public string WarranterCode
        {
            get { return _warranterCode; }
            set { _warranterCode = value; this.OnPropertyChanged(); }
        }

        private string _warranterName;

        public string WarranterName
        {
            get { return _warranterName; }
            set { _warranterName = value; this.OnPropertyChanged(); }
        }

        // 修正久期
        private decimal? _modifiedDuration;

        public decimal? ModifiedDuration
        {
            get { return _modifiedDuration; }
            set { _modifiedDuration = value; this.OnPropertyChanged(); }
        }


        // 中债估值
        private decimal? _chinaBondValuation;

        public decimal? ChinaBondValuation
        {
            get { return _chinaBondValuation; }
            set { _chinaBondValuation = value; this.OnPropertyChanged(); }
        }

        // 中债估值(复制)
        private string _chinaBondValuationFull;

        public string ChinaBondValuationFull
        {
            get { return _chinaBondValuationFull; }
            set { _chinaBondValuationFull = value; this.OnPropertyChanged(); }
        }

        // 中债行权
        private decimal? _chinaBondExercise;

        public decimal? ChinaBondExercise
        {
            get { return _chinaBondExercise; }
            set { _chinaBondExercise = value; this.OnPropertyChanged(); }
        }


        // 中债到期
        private decimal? _chinaBondDue;

        public decimal? ChinaBondDue
        {
            get { return _chinaBondDue; }
            set { _chinaBondDue = value; this.OnPropertyChanged(); }
        }

        // 中证估值
        private decimal? _csiBondValuation;

        public decimal? CsiBondValuation
        {
            get { return _csiBondValuation; }
            set { _csiBondValuation = value; this.OnPropertyChanged(); }
        }

        // 中证估值(复制)
        private string _csiBondValuationFull;

        public string CsiBondValuationFull
        {
            get { return _csiBondValuationFull; }
            set { _csiBondValuationFull = value; this.OnPropertyChanged(); }
        }

        // 中证行权
        private decimal? _csiBondValuationExercise;

        public decimal? CsiBondValuationExercise
        {
            get { return _csiBondValuationExercise; }
            set { _csiBondValuationExercise = value; this.OnPropertyChanged(); }
        }

        // 中证到期
        private decimal? _csiBondValuationDue;

        public decimal? CsiBondValuationDue
        {
            get { return _csiBondValuationDue; }
            set { _csiBondValuationDue = value; this.OnPropertyChanged(); }
        }

        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, QuoteTag> quoteTagDic = Common.Handler.Base.BaseHandler.Instance.GetQuoteTagDic();
            string a = CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }

        private long _sign;

        public long Sign
        {
            get { return _sign; }
            set
            {
                if (_sign != value || _signDic == null)
                {
                    _sign = value;
                    _signDic = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic;

        public Dictionary<string, string> SignDic
        {
            get { return _signDic; }
            set
            {
                if (_signDic != value)
                {
                    _signDic = value;
                }
            }
        }

        private long _sign1;

        public long Sign1
        {
            get { return _sign1; }
            set
            {
                if (_sign1 != value || _signDic1 == null)
                {
                    _sign1 = value;
                    _signDic1 = ParseSign("sign1", value);
                }
            }
        }

        private Dictionary<string, string> _signDic1;

        public Dictionary<string, string> SignDic1
        {
            get { return _signDic1; }
            set
            {
                if (_signDic1 != value)
                {
                    _signDic1 = value;
                }
            }
        }

        private long _sign2;

        public long Sign2
        {
            get { return _sign2; }
            set
            {
                if (_sign2 != value || _signDic2 == null)
                {
                    _sign2 = value;
                    _signDic2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic2;

        public Dictionary<string, string> SignDic2
        {
            get { return _signDic2; }
            set
            {
                if (_signDic2 != value)
                {
                    _signDic2 = value;
                }
            }
        }

        private long _sign3;

        public long Sign3
        {
            get { return _sign3; }
            set
            {
                if (_sign3 != value || _signDic3 == null)
                {
                    _sign3 = value;
                    _signDic3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic3;

        public Dictionary<string, string> SignDic3
        {
            get { return _signDic3; }
            set
            {
                if (_signDic3 != value)
                {
                    _signDic3 = value;
                }
            }
        }

        private long _sign4;

        public long Sign4
        {
            get { return _sign4; }
            set
            {
                if (_sign4 != value || _signDic4 == null)
                {
                    _sign4 = value;
                    _signDic4 = ParseSign("sign4", value);
                }
            }
        }

        private Dictionary<string, string> _signDic4;

        public Dictionary<string, string> SignDic4
        {
            get { return _signDic4; }
            set
            {
                if (_signDic4!= value)
                {
                    _signDic4 = value;
                }
            }
        }

        //到期日距离下个工作日天数（休n/x）
        private string _dayOffStr;

        public string DayOffStr
        {
            get { return _dayOffStr; }
            set
            {
                _dayOffStr = value;
                this.OnPropertyChanged();
            }
        }

        private string _copyInfo;

        public string CopyInfo
        {
            get
            {
                return _copyInfo;
            }
            set
            {
                if (_copyInfo != value)
                {
                    _copyInfo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _bondCategory;
        public string BondCategory
        {
            get { return _bondCategory; }
            set { _bondCategory = value; this.OnPropertyChanged(); }
        }

        private string _Rating;

        public string Rating
        {
            get { return _Rating; }
            set
            {
                _Rating = value;
                this.OnPropertyChanged();
            }
        }

        private string _cdc_Implited_Rating;

        public string Cdc_Implited_Rating
        {
            get { return _cdc_Implited_Rating; }
            set
            {
                _cdc_Implited_Rating = value;
                this.OnPropertyChanged();
            }
        }

        private string _maturityDate;

        public string MaturtityDate
        {
            get { return _maturityDate; }
            set { _maturityDate = value; this.OnPropertyChanged(); }
        }
        private string _level;

        public string Level
        {
            get { return _level; }
            set { _level = value; this.OnPropertyChanged(); }
        }


        private string _regionType;

        public string RegionType
        {
            get { return _regionType; }
            set { _regionType = value; this.OnPropertyChanged(); }
        }


        private string _enterpriseType;

        public string EnterpriseType
        {
            get { return _enterpriseType; }
            set { _enterpriseType = value; this.OnPropertyChanged(); }
        }


        private string _issueType;

        public string IssueType
        {
            get { return _issueType; }
            set { _issueType = value; this.OnPropertyChanged(); }
        }

        private bool if_guarantee;

        public bool If_Guarantee
        {
            get { return if_guarantee; }
            set
            {
                if_guarantee = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _issueAmount;

        public decimal? IssueAmount
        {
            get { return _issueAmount; }
            set
            {
                _issueAmount = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _outstanding_Amount;

        public decimal? Outstanding_Amount
        {
            get { return _outstanding_Amount; }
            set
            {
                _outstanding_Amount = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 还本条款
        /// "01", "到期一次性还本"
        /// "10", "提前还本"
        /// </summary>
        private string if_repay;

        public string If_Repay
        {
            get { return if_repay; }
            set
            {
                if_repay = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private string _quotetype;

        public string QuoteType
        {
            get { return _quotetype; }
            set
            {
                _quotetype = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _lastMarketPx;

        public decimal? LastMarketPx
        {
            get { return _lastMarketPx; }
            set
            {
                if (_lastMarketPx != value)
                {
                    _lastMarketPx = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _validInquiryConnt = 0;

        public int ValidInquiryCount
        {
            get { return _validInquiryConnt; }
            set
            {
                _validInquiryConnt = value;
                this.OnPropertyChanged();
            }
        }

        private string _commandNote;

        public string CommandNote
        {
            get { return _commandNote; }
            set
            {
                _commandNote = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 对手方机构ID
        /// </summary>
        private string _counterPartyID;

        public string CounterPartyID
        {
            get { return _counterPartyID; }
            set
            {
                _counterPartyID = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 对手方机构名称
        /// </summary>
        private string _counterPartyName;

        public string CounterPartyName
        {
            get { return _counterPartyName; }
            set 
            { 
                _counterPartyName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 投资经理 ID（STC userName)
        /// </summary>
        private string _investor;

        public string Investor
        {
            get { return _investor; }
            set 
            { 
                _investor = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 投资经理名称 (中文名称)
        /// </summary>
        private string _investorName;

        public string InvestorName
        {
            get { return _investorName; }
            set 
            { 
                _investorName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易员 ID（STC userName)
        /// </summary>
        private string _trader_buyer;

        public string Trader_Buyer
        {
            get { return _trader_buyer; }
            set
            {
                _trader_buyer = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易员名称 (中文名称)
        /// </summary>
        private string _traderName_buyer;

        public string TraderName_Buyer
        {
            get { return _traderName_buyer; }
            set
            {
                _traderName_buyer = value;
                this.OnPropertyChanged();
            }
        }

        private StcTbookUsers _selectedTrader_Buy_Edit;

        public StcTbookUsers SelectedTrader_Buy_Edit
        {
            get { return _selectedTrader_Buy_Edit; }
            set
            {
                _selectedTrader_Buy_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _trader_Buy_Edit;

        public string Trader_Buy_Edit
        {
            get { return _trader_Buy_Edit; }
            set
            {
                _trader_Buy_Edit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 删除标记
        /// </summary>
        private string _delFlag;

        public string DelFlag
        {
            get { return _delFlag; }
            set
            {
                _delFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 阅读标记,Json格式，如{"userId1":"1","userId2":"0"}，表示userId1需要阅读，userId2不需要阅读
        /// 1 显示 ， 0 不显示
        /// </summary>
        private string _readFlag;

        public string ReadFlag
        {
            get { return _readFlag; }
            set
            {
                _readFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 子状态
        /// </summary>
        private string _subStatus;

        public string SubStatus
        {
            get { return _subStatus; }
            set 
            { 
                _subStatus = value;
                this.OnPropertyChanged();
            }
        }

        private string _roletype;

        public string RoleType
        {
            get { return _roletype; }
            set
            {
                _roletype = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 历史操作类型
        /// </summary>
        private string _operation;

        public string Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 1：要素变更 – 报价
        /// 2：要素变更 – 报量
        /// 3：要素变更 – 方向
        /// 4：要素变更 – 清算速度
        /// 5：要素变更 – 指令备注
        /// 6：状态变更
        /// 7：匹配状态变更
        /// 存在多个用逗号分割
        /// </summary>
        private string _diff;

        public string Diff
        {
            get { return _diff; }
            set 
            {
                _diff = value;
                this.OnPropertyChanged();
            }
        }

        private string _matchStatus;

        public string MatchStatus
        {
            get { return _matchStatus; }
            set
            {
                _matchStatus = value;
                this.OnPropertyChanged();
            }
        }

        private string _SelectedTradeAccountId = null;
        /// <summary>
        /// 选中的交易账户代码
        /// </summary>
        public string SelectedTradeAccountId
        {
            get { return _SelectedTradeAccountId; }
            set
            {
                if (_SelectedTradeAccountId != value)
                {
                    _SelectedTradeAccountId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SelectedTradeAccountCode = null;
        /// <summary>
        /// 选中的交易账户代码/组合代码
        /// </summary>
        public string SelectedTradeAccountCode
        {
            get { return _SelectedTradeAccountCode; }
            set
            {
                if (_SelectedTradeAccountCode != value)
                {
                    _SelectedTradeAccountCode = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SelectedTradeAccountName = null;
        /// <summary>
        /// 选中的交易账户名称
        /// </summary>
        public string SelectedTradeAccountName
        {
            get { return _SelectedTradeAccountName; }
            set
            {
                if (_SelectedTradeAccountName != value)
                {
                    _SelectedTradeAccountName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _displayAccountName = null;
        /// <summary>
        /// 选中的交易账户名称
        /// </summary>
        public string DisplayAccountName
        {
            get { return _displayAccountName; }
            set
            {
                if (_displayAccountName != value)
                {
                    _displayAccountName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private List<string> _SelectedGroupItem = new List<string>();
        /// <summary>
        /// 选中的组合代码
        /// </summary>
        public List<string> SelectedGroupItem
        {
            get { return _SelectedGroupItem; }
            set
            {
                if (_SelectedGroupItem != value)
                {
                    _SelectedGroupItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private List<string> _SelectedGroupText = new List<string>();
        /// <summary>
        /// 选中的组合
        /// </summary>
        public List<string> SelectedGroupText
        {
            get { return _SelectedGroupText; }
            set
            {
                if (_SelectedGroupText != value)
                {
                    _SelectedGroupText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public Dictionary<string, Dictionary<string, object>> _selectedGroupDict = new Dictionary<string, Dictionary<string, object>>();
        /// <summary>
        /// 选中的组合字典
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> SelectedGroupDict
        {
            get { return _selectedGroupDict; }
            set
            {
                if (_selectedGroupDict != value)
                {
                    _selectedGroupDict = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SelectedProductId = null;
        /// <summary>
        /// 选中恒生编号，以,隔开
        /// </summary>
        public string SelectedProductId
        {
            get { return _SelectedProductId; }
            set
            {
                if (_SelectedProductId != value)
                {
                    _SelectedProductId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 组合id
        /// </summary>
        private string _bookId;

        public string BookId
        {
            get { return _bookId; }
            set
            {
                _bookId = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合
        /// </summary>
        private string _bookName;

        public string BookName
        {
            get { return _bookName; }
            set
            {
                _bookName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合交易员id
        /// </summary>
        private string _bookTraderId;

        public string BookTraderId
        {
            get { return _bookTraderId; }
            set
            {
                _bookTraderId = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合交易员名称
        /// </summary>
        private string _bookTraderName;

        public string BookTraderName
        {
            get { return _bookTraderName; }
            set
            {
                _bookTraderName = value;
                this.OnPropertyChanged();
            }
        }

        private string _riskNote;

        public string RiskNote
        {
            get { return _riskNote; }
            set
            {
                _riskNote = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// optim总持仓量(万)
        /// </summary>
        private decimal? _totalQty;

        public decimal? TotalQty
        {
            get { return _totalQty; }
            set 
            {
                _totalQty = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 可用持仓量(万) optim
        /// </summary>
        private decimal? _availablePosition;

        public decimal? AvailablePosition
        {
            get { return _availablePosition; }
            set
            {
                _availablePosition = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// stc可用持仓量(万)
        /// </summary>
        private decimal? _stcAvailablePosition;

        public decimal? StcAvailablePosition
        {
            get { return _stcAvailablePosition; }
            set
            {
                _stcAvailablePosition = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// STC在途卖量(万)
        /// </summary>
        private decimal? _stcInSalesVolume;

        public decimal? StcInSalesVolume
        {
            get { return _stcInSalesVolume; }
            set
            {
                _stcInSalesVolume = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// STC在途买量(万)
        /// </summary>
        private decimal? _stcInBuyVolume;

        public decimal? StcInBuyVolume
        {
            get { return _stcInBuyVolume; }
            set
            {
                _stcInBuyVolume = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// stc可用额度(万)
        /// </summary>
        private decimal? _stcAvailableLimit;

        public decimal? StcAvailableLimit
        {
            get { return _stcAvailableLimit; }
            set 
            { 
                _stcAvailableLimit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 内部主体评级
        /// </summary>
        private string _rate;
        
        public string Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 内部债券评级
        /// </summary>
        private string _bonddrating;

        public string Bonddrating
        {
            get { return _bonddrating; }
            set
            {
                _bonddrating = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 主体额度
        /// </summary>
        private decimal? _quotalimit;

        public decimal? Quotalimit
        {
            get { return _quotalimit; }
            set
            {
                _quotalimit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 可投比例
        /// </summary>
        private string _investmentratio;

        public string Investmentratio
        {
            get { return _investmentratio; }
            set
            {
                _investmentratio = value;
                this.OnPropertyChanged();
            }
        }

        // 匹配成交量
        private long? _matchedQty;

        public long? MatchedQty
        {
            get { return _matchedQty; }
            set
            {
                _matchedQty = value;
                this.OnPropertyChanged();
            }
        }

        // 匹配状态
        private string _execStatus;

        public string ExecStatus
        {
            get { return _execStatus; }
            set
            {
                _execStatus = value;
                this.OnPropertyChanged();
            }
        }

        private string _relatedUsers;

        public string RelatedUsers
        {
            get { return _relatedUsers; }
            set
            {
                _relatedUsers = value;
                this.OnPropertyChanged();
            }
        }

        private string _relatedUsers_display;

        public string RelatedUsers_Display
        {
            get { return _relatedUsers_display; }
            set
            {
                _relatedUsers_display = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 分组详细信息
        /// </summary>
        private CustomCollection<IntentionQuoteItem> detailIntentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> DetailIntentionQuoteInfo
        {
            get { return detailIntentionQuoteInfo; }
            set { detailIntentionQuoteInfo = value; }
        }

        private string _sortKey;

        public string SortKey
        {
            get { return _sortKey; }
            set
            {
                _sortKey = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupKey;

        public string GroupKey
        {
            get { return _groupKey; }
            set
            {
                _groupKey = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// PAYL的临时排序字段
        /// </summary>
        private object _tempSort;

        public object TempSort
        {
            get { return _tempSort; }
            set
            {
                _tempSort = value;
                this.OnPropertyChanged();
            }
        }


        private string _hsProductId;

        public string HsProductId
        {
            get { return _hsProductId; }
            set
            {
                _hsProductId = value;
                this.OnPropertyChanged();
            }
        }

        private string _strategyOrderId;

        public string StrategyOrderId
        {
            get
            {
                return _strategyOrderId;
            }
            set
            {
                _strategyOrderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _exchange;

        public string Exchange
        {
            get { return _exchange; }
            set
            {
                _exchange = value;
                this.OnPropertyChanged();
            }
        }

        private bool _hasInquiry;

        public bool HasInquiry
        {
            get { return _hasInquiry; }
            set 
            { 
                _hasInquiry = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsFlashing;

        public bool IsFlashing
        {
            get { return _IsFlashing; }
            set
            {
                _IsFlashing = value;
                this.OnPropertyChanged();
            }
        }

        private string _Tax_Free;

        public string Tax_Free
        {
            get { return _Tax_Free; }
            set
            {
                _Tax_Free = value;
                this.OnPropertyChanged();
            }
        }
    }
}
