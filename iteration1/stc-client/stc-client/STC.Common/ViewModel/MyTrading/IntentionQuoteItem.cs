using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Common.ViewModel.MyTrading
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
                            List<CrmInst> results = CommonUtils.GetInstitutionsInfoPo(instList);
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
                _price = value;
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
        #endregion

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

        private string _orderCount = string.Empty;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
                this.OnPropertyChanged();

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


    }
}
