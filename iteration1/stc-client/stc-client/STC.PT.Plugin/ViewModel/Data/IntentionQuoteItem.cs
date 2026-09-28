using STC.Common.Model.CRM;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.PT.Plugin.ViewModel.Data
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
            set
            {
                _intentionId = value;
                this.OnPropertyChanged();
            }
        }

        private bool isSysAuto_Institution = false;

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
        private string _cleanPrice_Edit;

        public string CleanPrice_Edit
        {
            get { return _cleanPrice_Edit; }
            set
            {
                _cleanPrice_Edit = value;
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

        private string _fullPrice_Edit;

        public string FullPrice_Edit
        {
            get { return _fullPrice_Edit; }
            set
            {
                _fullPrice_Edit = value;
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
                _price = value;
                this.OnPropertyChanged();
            }
        }
        private decimal? _Yieldtoexecution;

        public decimal? Yieldtoexecution
        {
            get { return _Yieldtoexecution; }
            set
            {
                _Yieldtoexecution = value;
                this.OnPropertyChanged();
            }
        }
        private decimal? _Yieldtoexecution_Edit;

        public decimal? Yieldtoexecution_Edit
        {
            get { return _Yieldtoexecution_Edit; }
            set
            {
                _Yieldtoexecution_Edit = value;
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

        private string _settlTypeDisplay;

        public string SettlTypeDisplay
        {
            get { return _settlTypeDisplay; }
            set
            {
                _settlTypeDisplay = value;
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
            }
        }

        private string _type;

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
            get { return _closeByName; }
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

        public STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest Request { get; set; }

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
                    BestBid_FC.BeginAnimation(SolidColorBrush.ColorProperty, BID_CA); // 颜色绘制使用动画绘制
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
                    BestOfr_FC.BeginAnimation(SolidColorBrush.ColorProperty, OFR_CA); // 颜色绘制使用动画绘制
                }
            }
        }

        private static ColorAnimation BID_CA = new ColorAnimation()
        {
            From = (Color)ColorConverter.ConvertFromString("#4CFB943C"),
            To = (Color)ColorConverter.ConvertFromString("#FB943C"),
            AutoReverse = false, // 正常播放
            RepeatBehavior = new RepeatBehavior(3),
            Duration = new Duration(TimeSpan.FromSeconds(1)) // 动画一次所用时间
        };//动画效果
        private SolidColorBrush _bestBid_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB943C"));

        public SolidColorBrush BestBid_FC
        {
            get
            {
                return _bestBid_FC;
            }
            set
            {
                if (_bestBid_FC != value)
                {
                    _bestBid_FC = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private static ColorAnimation OFR_CA = new ColorAnimation()
        {
            From = (Color)ColorConverter.ConvertFromString("#4C58B3FF"),
            To = (Color)ColorConverter.ConvertFromString("#58B3FF"),
            AutoReverse = false, // 正常播放
            RepeatBehavior = new RepeatBehavior(3),
            Duration = new Duration(TimeSpan.FromSeconds(1)), // 动画一次所用时间
        }; //动画效果
        private SolidColorBrush _bestOfr_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58B3FF"));

        public SolidColorBrush BestOfr_FC
        {
            get
            {
                return _bestOfr_FC;
            }
            set
            {
                if (_bestOfr_FC != value)
                {
                    _bestOfr_FC = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _accountID;

        public string AccountID
        {
            get { return _accountID; }
            set { _accountID = value; }
        }

        private string _accountName;

        public string AccountName
        {
            get { return _accountName; }
            set { _accountName = value; }
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
        //我方
        private string _PartyName;

        public string PartyName
        {
            get { return _PartyName; }
            set
            {
                _PartyName = value;
                this.OnPropertyChanged();
            }
        }
        //对方机构来源客户机构
        private string _CounterPartyName;

        public string CounterPartyName
        {
            get { return _CounterPartyName; }
            set
            {
                _CounterPartyName = value;
                this.OnPropertyChanged();
            }
        }

        private string _CounterPartyId;

        public string CounterPartyId
        {
            get { return _CounterPartyId; }
            set
            {
                _CounterPartyId = value;
                this.OnPropertyChanged();
            }
        }
        private string _CounterPartyName_Edit;

        public string CounterPartyName_Edit
        {
            get { return _CounterPartyName_Edit; }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                _CounterPartyName_Edit = newValue;
                this.OnPropertyChanged();
            }
        }
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
        //上交所机构代码
        private string _sseinstitutioncode;

        public string Sseinstitutioncode
        {
            get { return _sseinstitutioncode; }
            set
            {
                if (_sseinstitutioncode != value)
                {
                    _sseinstitutioncode = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //深交所-交易商代码
        private string _szedealercode;

        public string Szedealercode
        {
            get { return _szedealercode; }
            set
            {
                if (_szedealercode != value)
                {
                    _szedealercode = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //深交所-交易主体代码
        private string _szetradingentitycode;

        public string Szetradingentitycode
        {
            get { return _szetradingentitycode; }
            set
            {
                if (_szetradingentitycode != value)
                {
                    _szetradingentitycode = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //深交所-交易员代码
        private string _szetradercode;

        public string Szetradercode
        {
            get { return _szetradercode; }
            set
            {
                if (_szetradercode != value)
                {
                    _szetradercode = value;
                    this.OnPropertyChanged();
                }
            }
        }

        //发单方向默认为Ofr->Bid，点击后变为Ofr<-Bid
        private string _DealSide;

        public string DealSide
        {
            get { return _DealSide; }
            set
            {
                _DealSide = value;
                this.OnPropertyChanged();
                if ("2".Equals(_DealSide))
                {
                    DealSideDispaly = "->";
                }
                else
                {
                    DealSideDispaly = "<-";
                }
            }
        }
        private string _DealSideDispaly;
        public string DealSideDispaly
        {
            get { return _DealSideDispaly; }
            set
            {
                _DealSideDispaly = value;
                this.OnPropertyChanged();
            }
        }
        private ComboxBaseModel _TrdSubTypeItem;
        /// <summary>
        public ComboxBaseModel TrdSubTypeItem
        {
            get
            {
                return _TrdSubTypeItem;
            }
            set
            {
                if (_TrdSubTypeItem != value)
                {
                    _TrdSubTypeItem = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private List<ComboxBaseModel> _TrdSubTypeItems;
        /// <summary>
        /// 下拉数据源
        /// </summary>
        public List<ComboxBaseModel> TrdSubTypeItems
        {
            get
            {
                return _TrdSubTypeItems;
            }
            set
            {
                if (_TrdSubTypeItems != value)
                {
                    _TrdSubTypeItems = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //成交方式，银行间默认对话，可下拉选择下拉列表为对话/请求；交易所券时，默认为空
        private ComboxBaseModel _DealWaySelect;
        /// <summary>
        /// 选中渠道
        /// </summary>
        public ComboxBaseModel DealWaySelect
        {
            get
            {
                return _DealWaySelect;
            }
            set
            {
                if (_DealWaySelect != value)
                {
                    _DealWaySelect = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private List<ComboxBaseModel> _DealWayItems;
        /// <summary>
        /// 下拉数据源
        /// </summary>
        public List<ComboxBaseModel> DealWayItems
        {
            get
            {
                return _DealWayItems;
            }
            set
            {
                if (_DealWayItems != value)
                {
                    _DealWayItems = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _DealWayVisible;
        public string DealWayVisible
        {
            get { return _DealWayVisible; }
            set
            {
                _DealWayVisible = value;
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

        private string _PaymentRate_Edit;

        public string PaymentRate_Edit
        {
            get { return _PaymentRate_Edit; }
            set
            {
                _PaymentRate_Edit = value;
                this.OnPropertyChanged();
            }
        }
        //代付费率
        private string _PaymentRate;

        public string PaymentRate
        {
            get { return _PaymentRate; }
            set
            {
                _PaymentRate = value;
                this.OnPropertyChanged();
            }
        }
        
        //费后到期收益率
        private decimal? _yield2;

        public decimal? Yield2
        {
            get { return _yield2; }
            set
            {
                _yield2 = value;
                this.OnPropertyChanged();
            }
        }
        //费后全价
        private decimal? _fullPrice2;

        public decimal? FullPrice2
        {
            get { return _fullPrice2; }
            set
            {
                _fullPrice2 = value;
                this.OnPropertyChanged();
            }
        }
        //费后净价
        private decimal? _NetPrice2;

        public decimal? NetPrice2
        {
            get { return _NetPrice2; }
            set
            {
                _NetPrice2 = value;
                this.OnPropertyChanged();
            }
        }
        //费后行权收益率
        private decimal? _Yieldtoexecution2;

        public decimal? Yieldtoexecution2
        {
            get { return _Yieldtoexecution2; }
            set
            {
                _Yieldtoexecution2 = value;
                this.OnPropertyChanged();
            }
        }

        //结算金额
        private decimal? _settlAmount;

        public decimal? SettlAmount
        {
            get { return _settlAmount; }
            set
            {
                _settlAmount = value;
                this.OnPropertyChanged();
            }
        }
        private string _TrdSubTypeItem_Edit;
        /// <summary>
        public string TrdSubTypeItem_Edit
        {
            get
            {
                return _TrdSubTypeItem_Edit;
            }
            set
            {
                if (_TrdSubTypeItem_Edit != value)
                {
                    _TrdSubTypeItem_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TrdSubType;
        /// <summary>
        public string TrdSubType
        {
            get
            {
                return _TrdSubType;
            }
            set
            {
                if (_TrdSubType != value)
                {
                    _TrdSubType = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _DealWayItem_Edit;
        /// <summary>
        public string DealWayItem_Edit
        {
            get
            {
                return _DealWayItem_Edit;
            }
            set
            {
                if (_DealWayItem_Edit != value)
                {
                    _DealWayItem_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _DealWay;
        /// <summary>
        public string DealWay
        {
            get
            {
                return _DealWay;
            }
            set
            {
                if (_DealWay != value)
                {
                    _DealWay = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _CounterPartySubName;
        /// <summary>
        public string CounterPartySubName
        {
            get
            {
                return _CounterPartySubName;
            }
            set
            {
                if (_CounterPartySubName != value)
                {
                    _CounterPartySubName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _CounterPartySubName_Edit;
        /// <summary>
        public string CounterPartySubName_Edit
        {
            get
            {
                return _CounterPartySubName_Edit;
            }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                if (_CounterPartySubName_Edit != newValue)
                {
                    _CounterPartySubName_Edit = newValue;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _RealCounterPartyName;
        /// <summary>
        public string RealCounterPartyName
        {
            get
            {
                return _RealCounterPartyName;
            }
            set
            {
                if (_RealCounterPartyName != value)
                {
                    _RealCounterPartyName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private CrmInst _RealcounterpartynameItem_Edit;

        public CrmInst RealcounterpartynameItem_Edit
        {
            get { return _RealcounterpartynameItem_Edit; }
            set
            {
                if (_RealcounterpartynameItem_Edit != value)
                {
                    _RealcounterpartynameItem_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _RealCounterPartyName_Edit;
        /// <summary>
        public string RealCounterPartyName_Edit
        {
            get
            {
                return _RealCounterPartyName_Edit;
            }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                if (_RealCounterPartyName_Edit != newValue)
                {
                    _RealCounterPartyName_Edit = newValue;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _TradeDate;
        /// <summary>
        public string TradeDate
        {
            get
            {
                return _TradeDate;
            }
            set
            {
                if (_TradeDate != value)
                {
                    _TradeDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SettlDate;
        /// <summary>
        public string SettlDate
        {
            get
            {
                return _SettlDate;
            }
            set
            {
                if (_SettlDate != value)
                {
                    _SettlDate = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _AllocText;

        public string AllocText
        {
            get { return _AllocText; }
            set
            {
                _AllocText = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 0-含权债
        /// </summary>
        private string _IsExeCdcType = string.Empty;

        public string IsExeCdcType
        {
            get { return _IsExeCdcType; }
            set
            {
                _IsExeCdcType = value;
                this.OnPropertyChanged();
            }
        }
        //约定号
        private string _TrdMatchId;

        public string TrdMatchId
        {
            get { return _TrdMatchId; }
            set
            {
                _TrdMatchId = value;
                this.OnPropertyChanged();
            }
        }

    }
    public class ComboxBaseModel
    {
        /// <summary>
        /// 显示列
        /// </summary>
        public string DisplayMemberPath { get; set; }

        /// <summary>
        /// 值列
        /// </summary>
        public string ValueMemberPath { get; set; }

        /// <summary>
        /// 组件列
        /// </summary>
        public string KeyMemberPath { get; set; }
    }
}
