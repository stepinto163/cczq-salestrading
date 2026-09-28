using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Risk;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.Common.View.Component.AutoCompleteComboBox;
using WtUtils = STC.WorkBench.Plugin.Utils.CommonUtils;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class RecognizeIntentionQuoteViewModel: ViewModelBase
    {
        private Dictionary<string, string> _priceType_empty_Dic = new Dictionary<string, string>();
        public Dictionary<string, string> PriceTypeEmptyDic
        {
            get
            {
                if (_priceType_empty_Dic.Count == 0)
                {
                    _priceType_empty_Dic["1"] = "净价";
                    //_priceType_empty_Dic["2"] = "全价";
                    //_priceType_empty_Dic["3"] = "收益率";
                    _priceType_empty_Dic["7"] = "行权";
                    _priceType_empty_Dic["3"] = "到期";
                }
                return _priceType_empty_Dic;
            }
        }

        public string _PriceType;

        public string PriceType
        {
            get { return _PriceType; }
            set
            {
                _PriceType = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

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

                    if (value != null)
                    {
                        List<CrmInst> result = GetCrmInst();
                        if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                this.SelectClientTypeItem = result.First();
                            }

                        }
                    }


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

        private StcTbookUsers _selectedInvestor_Edit;

        public StcTbookUsers SelectedInvestor_Edit
        {
            get { return _selectedInvestor_Edit; }
            set
            {
                _selectedInvestor_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _investor_Edit;

        public string Investor_Edit
        {
            get { return _investor_Edit; }
            set
            {
                _investor_Edit = value;
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

        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
            }
        }

        private Dictionary<string, string> _opDic = new Dictionary<string, string>();

        public Dictionary<string, string> OpDic
        {
            get
            {
                if (_opDic.Count == 0)
                {
                    _opDic["+"] = "+";
                    _opDic["-"] = "-";
                }
                return _opDic;
            }
        }


        private bool _isVip = false;

        public bool IsVip
        {
            get
            {
                return _isVip;
            }
            set
            {
                if (_isVip != value)
                {
                    _isVip = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 点差
        /// </summary>
        private decimal? _bp;

        public decimal? BP
        {
            get 
            {
                return _bp;
            }
            set
            {
                if (_bp != value)
                {
                    _bp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _CommonQty;

        public decimal? CommonQty
        {
            get { return _CommonQty; }
            set
            {
                _CommonQty = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _BidBp;

        public decimal? BidBp
        {
            get { return _BidBp; }
            set
            {
                _BidBp = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _OfrBp;

        public decimal? OfrBp
        {
            get { return _OfrBp; }
            set
            {
                _OfrBp = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 急
        /// </summary>
        private string _hurryFlag;

        public string HurryFlag
        {
            get 
            { 
                return _hurryFlag; 
            }
            set 
            {
                _hurryFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 荐
        /// </summary>
        private string _suggestFlag;

        public string SuggestFlag
        {
            get 
            { 
                return _suggestFlag;
            }
            set 
            {
                _suggestFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 火
        /// </summary>
        private string _fireFlag;

        public string FireFlag
        {
            get 
            { 
                return _fireFlag;
            }
            set 
            { 
                _fireFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 底价
        /// </summary>
        private string _floorPrice;

        public string FloorPrice
        {
            get 
            { 
                return _floorPrice;
            }
            set 
            { 
                _floorPrice = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// OCO 打包 1:oco 2:打包
        /// </summary>
        private string _ocoFlag;

        public string OcoFlag
        {
            get 
            { 
                return _ocoFlag;
            }
            set 
            { 
                _ocoFlag = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 公开备注
        /// </summary>
        private string _publicRemarks;

        public string PublicRemarks
        {
            get { return _publicRemarks; }
            set 
            { _publicRemarks = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 个人备注
        /// </summary>
        private string _personalRemarks;

        public string PersonalRemarks
        {
            get 
            { 
                return _personalRemarks;
            }
            set 
            { 
                _personalRemarks = value;
                this.OnPropertyChanged();
            }
        }

        private string _commandNotes;

        public string CommandNotes
        {
            get { return _commandNotes; }
            set
            {
                if (_commandNotes != value)
                {
                    _commandNotes = value;
                    this.OnPropertyChanged();

                    foreach (IntentionQuoteItem item in this.IntentionQuoteInfo)
                    {
                        item.CommandNote = value;
                    }
                }
            }
        }


        //private string  _selectedValue;

        //public string SelectedValue
        //{
        //    get { return _selectedValue; }
        //    set { _selectedValue = value; }
        //}


        ///// <summary>
        ///// 换券标识 true: 换券; false 不换券
        ///// </summary>
        //private bool _ExchangeSignFlag;

        //public bool ExchangeSignFlag
        //{
        //    get { return _ExchangeSignFlag; }
        //    set { _ExchangeSignFlag = value; }
        //}

        ///// <summary>
        ///// 返点标签 1 数字,2文本
        ///// </summary>
        //private string _FanDianFlag;

        //public string FanDianFlag
        //{
        //    get { return _FanDianFlag; }
        //    set { _FanDianFlag = value; }
        //}

        ///// <summary>
        ///// 暗盘标记 1暗盘，2明盘
        ///// </summary>
        //private string _Handlinst;

        //public string Handlinst
        //{
        //    get { return _Handlinst; }
        //    set { _Handlinst = value; }
        //}

        ///// <summary>
        ///// 3-收益率（到期） 7-行权 1-净价;2- 全价;
        ///// </summary>
        //private string _PriceType;

        //public string PriceType
        //{
        //    get { return _PriceType; }
        //    set { _PriceType = value; }
        //}



        //private Dictionary<string, object> _tagDic = new Dictionary<string, object>();

        //public Dictionary<string, object> TagDic
        //{
        //    get 
        //    {
        //        if (_tagDic.Count == 0)
        //        {
        //            _tagDic["hq"] = "换券";
        //            _tagDic["fd"] = "返点";
        //            _tagDic["ap"] = "暗盘";
        //            _tagDic["xq"] = "行权";
        //            _tagDic["dq"] = "到期";
        //            _tagDic["%"] = "%";
        //            _tagDic["jj"] = "净价";
        //            _tagDic["qj"] = "全价";
        //        }
        //        return _tagDic;
        //    }
        //}

        //private string _orgType_Edit;

        //public string OrgType_Edit
        //{
        //    get { return _orgType_Edit; }
        //    set { 
        //        _orgType_Edit = value;
        //        this.WaterMark_Visibility = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private Visibility _waterMark_Visibility = Visibility.Visible;

        //public Visibility WaterMark_Visibility
        //{
        //    get { return _waterMark_Visibility; }
        //    set
        //    {
        //        _waterMark_Visibility = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private List<KeyValuePair<string, string>> _orgTypeDic = new List<KeyValuePair<string, string>>();

        //public List<KeyValuePair<string, string>> OrgTypeDic
        //{
        //    get 
        //    {
        //        if (_orgTypeDic.Count == 0)
        //        {
        //            Dictionary<string, QuoteTag> dic = IntentionOldHandler.Instance.QueryTags();
                    
        //            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
        //                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
        //            {
        //                foreach (ChildTag ct in dic["ORG_TYPE"].childTags)
        //                {
        //                    _orgTypeDic.Add(new KeyValuePair<string, string>(ct.childTagValue, ct.childTagName));
        //                }
        //            }
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0000", string.Empty));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0001", "银行配置户"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0111", "银行交易户"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0010", "券商"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0011", "基金"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0101", "保险"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("1000", "理财子"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0100", "资管"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("1001", "期货"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("1010", "信托"));
        //            //_orgTypeDic.Add(new KeyValuePair<string, string>("0110", "其他"));
        //        }
        //        return _orgTypeDic;
        //    }
        //}

        #region 有效时长

        private int validSelectIndex = 0;

        public int ValidSelectIndex
        {
            get
            {
                return validSelectIndex;
            }

            set
            {
                validSelectIndex = value;
                if (validSelectIndex == 0)
                {
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 1)
                {
                    HourValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 2)
                {
                    TimeValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 3)
                {
                    NoLimitValidTimeQuotationCheck = true;
                }

                this.OnPropertyChanged();
            }
        }

        private int defaultValidUntilTime = 30;
        public int DefaultValidUntilTime
        {
            get
            {
                return defaultValidUntilTime;
            }

            set
            {
                defaultValidUntilTime = value;
                this.OnPropertyChanged();
            }
        }



        private bool defaultValidTimeQuotationCheck = true;
        /// <summary>
        /// 默认
        /// </summary>
        public bool DefaultValidTimeQuotationCheck
        {
            get
            {
                return defaultValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(HourValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }

                defaultValidTimeQuotationCheck = value;
                if (defaultValidTimeQuotationCheck)
                {
                    HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }

                this.OnPropertyChanged();
            }
        }

        private bool hourValidTimeQuotationCheck = false;
        /// <summary>
        /// 时长
        /// </summary>
        public bool HourValidTimeQuotationCheck
        {
            get
            {
                return hourValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                hourValidTimeQuotationCheck = value;
                if (hourValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = TimeValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinute = string.Empty;
        public string ValidMinute
        {
            get { return validMinute; }
            set
            {
                if (value != null)
                {
                    validMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecond = string.Empty;
        public string ValidSecond
        {
            get { return validSecond; }
            set
            {
                if (value != null)
                {
                    validSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 小时
        /// </summary>
        private string validTimeHour = string.Empty;
        public string ValidTimeHour
        {
            get { return validTimeHour; }
            set
            {
                if (value != null)
                {
                    validTimeHour = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 时点 - 分钟
        /// </summary>
        private string validTimeMinute = string.Empty;
        public string ValidTimeMinute
        {
            get { return validTimeMinute; }
            set
            {
                if (value != null)
                {
                    validTimeMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 时点 - 秒钟
        /// </summary>
        private string validTimeSecond = string.Empty;
        public string ValidTimeSecond
        {
            get { return validTimeSecond; }
            set
            {
                if (value != null)
                {
                    validTimeSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private bool timeValidTimeQuotationCheck = false;
        /// <summary>
        /// 时点
        /// </summary>
        public bool TimeValidTimeQuotationCheck
        {
            get
            {
                return timeValidTimeQuotationCheck;
            }

            set
            {
                if (!NoLimitValidTimeQuotationCheck)
                {
                    if (!(DefaultValidTimeQuotationCheck ^ HourValidTimeQuotationCheck))
                    {
                        return;
                    }
                }
                timeValidTimeQuotationCheck = value;
                if (timeValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = NoLimitValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        private bool noLimitValidTimeQuotationCheck = false;
        /// <summary>
        /// 无
        /// </summary>
        public bool NoLimitValidTimeQuotationCheck
        {
            get
            {
                return noLimitValidTimeQuotationCheck;
            }

            set
            {
                noLimitValidTimeQuotationCheck = value;
                if (noLimitValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged();
            }
        }

        #endregion



        private bool _IsAllChecked_Same = false;
        public bool IsAllChecked_Same
        {
            get { return _IsAllChecked_Same; }
            set
            {
                _IsAllChecked_Same = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsAllChecked_Opposite = false;

        public bool IsAllChecked_Opposite
        {
            get { return _IsAllChecked_Opposite; }
            set
            {
                _IsAllChecked_Opposite = value;
                this.OnPropertyChanged();
            }
        }

        private object _LockObject = new object();
        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }

        private ObservableCollection<IntentionQuoteItem> intentionQuoteInfo = new ObservableCollection<IntentionQuoteItem>();

        public ObservableCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private IntentionQuoteItem _selectedItem_Recognize;
        public IntentionQuoteItem SelectedItem_Recognize
        {
            get
            {
                return this._selectedItem_Recognize;
            }

            set
            {
                this._selectedItem_Recognize = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DelOrderCommand { get; set; }

        private bool _isPublishBtnEnable;

        public bool IsPublishBtnEnable
        {
            get { return _isPublishBtnEnable; }
            set {
                _isPublishBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isFirmPublishBtnEnable;

        public bool IsFirmPublishBtnEnable
        {
            get { return _isFirmPublishBtnEnable; }
            set
            {
                _isFirmPublishBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ParserImageVisibility = Visibility.Collapsed;

        public Visibility ParserImageVisibility
        {
            get { return _ParserImageVisibility; }
            set
            {
                _ParserImageVisibility = value;
                this.OnPropertyChanged();
            }
        }

        internal void UnregisterEvent()
        { 

            IntentionOldHandler.Instance.OnRecognition -= Instance_OnRecoginition;

            if (_bestPriceDic != null)
            {
                foreach(string securityId in _bestPriceDic.Keys)
                {
                    PriceHandler.Instance.UnregisterCTPrice(securityId, UpdateCtPrice);
                }
                _bestPriceDic.Clear();
            }
        }

        ConsistencyCheckPo consistency;

        private bool _IsAutoPrase = false;
        /// <summary>
        /// 自动解析
        /// </summary>
        public bool IsAutoPrase
        {
            get { return _IsAutoPrase; }
            set
            {
                _IsAutoPrase = value;
                this.OnPropertyChanged();
            }
        }

        public RecognizeIntentionQuoteViewModel()
        {
            if (IntentionOldHandler.Instance.isSALER())
            {
                _innerType = "2";
            }

            if (LocalEnv.ConfInfo.IsParserImage == "1")
            {
                ParserImageVisibility = Visibility.Visible;
            }

            if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
            {
                if (AdminHandler.Instance.GetCachestcUserQuery().Datas.Any(item => item.Username == LocalEnv.UserInfo.UserName))
                {
                    StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.First(item => item.Username == LocalEnv.UserInfo.UserName);
                    this.SelectedBroker_Edit = currentUser;
                    this.BrokerName_Edit = currentUser.Name;
                }
            }
            

            DelOrderCommand = new ComRelayCommand(DeleteOrder);

            InitAccountUsers();
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            IntentionOldHandler.Instance.OnRecognition += Instance_OnRecoginition;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
            ParseValidTimeFormat();
            consistency = UserSettingAdapter.Instance.GetConsistencyCheckPo();

            this.ViewSource.View.Filter = x =>
            {
                IntentionQuoteItem item = x as IntentionQuoteItem;

                if (_exchangeFilterList != null && _exchangeFilterList.Count > 0 && !_exchangeFilterList.Contains(item.Exchange))
                {
                    return false;
                }

                return true;
            };
        }

        private void IntentionQuoteInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsFirmPublishBtnEnable = IsPublishBtnEnable = this.intentionQuoteInfo.Count > 0 ? true : false;

            if (IsFirmPublishBtnEnable)
            {
                foreach (IntentionQuoteItem data in this.IntentionQuoteInfo)
                {
                    if (!data.Price.HasValue || !data.Qty.HasValue)
                    {
                        IsFirmPublishBtnEnable = false;
                        break;
                    }
                }
            }
            SetOppositeCheckAll();
            SetSameCheckAll();
        }

        internal void ClickOppositeCheckAll(bool flag)
        {
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                item.IsOpposite = flag;
            }
        }

        internal void ClickSameCheckAll(bool flag)
        {
            foreach(IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                item.IsSame = flag;
            }
        }

        internal void SetOppositeCheckAll()
        {
            bool flag = true;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsOpposite;
            }
            this.IsAllChecked_Opposite = this.intentionQuoteInfo.Count > 0 && flag;
        }

        internal void SetSameCheckAll()
        {
            bool flag = true;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsSame;
            }
            this.IsAllChecked_Same = this.intentionQuoteInfo.Count > 0 && flag;
        }

        private void Instance_OnRecoginition(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                AddRecognize(obj);
                //IntentionQuoteItem item = null;
                //var findList = IntentionQuoteInfo.Where(x => x.IntentionId == obj.intentionId);
                //if (findList != null && findList.Count() > 0)
                //{
                //    item = findList.First();
                //    if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                //        && obj.status != "0")
                //    {
                //        this.IntentionQuoteInfo.Remove(item);
                //        return;
                //    }

                //    item.Status = obj.status;

                //    item.InstitutionId = obj.institutionId;
                //    item.InstitutionName = obj.institutionName;
                //    item.TraderId = obj.traderId;
                //    item.TraderName = obj.traderName;
                //    item.Side = obj.side;
                //    item.Price = obj.price;
                //    item.Qty = obj.qty;
                //    item.SettlType = obj.settlType;
                //    item.UpdateTime = obj.updateTime;
                //    item.UserName = obj.userName;
                //    item.CloseBy = obj.closeBy;
                //    item.OrderID = obj.orderId;
                //    item.QqNumber = obj.qqNumber;

                //}

            }));
        }

        internal void ClearList()
        {
            if (this.intentionQuoteInfo != null)
            {
                this.IntentionQuoteInfo.Clear();
            }
        }

        private bool isStartRecog = false;
        private IntentionQuoteItem AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            if(this.IntentionQuoteInfo.Any(entry => entry.ClOrdID == obj.clordid))
            {
                return this.IntentionQuoteInfo.First(entry => entry.ClOrdID == obj.clordid);
            }

            if (isStartRecog)
            {
                isStartRecog = false;

                //bool hasTraderInfo = false;

                if (!string.IsNullOrEmpty(obj.parseCompanyIdbkey))
                {
                    this.SelectedTrader_Edit = null;
                    this.TraderName_Edit = string.Empty;

                    List<string> instList = new List<string>();
                    instList.Add(obj.parseCompanyIdbkey);
                    List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                    if (results != null && results.Count > 0)
                    {
                        this.SelectedInstintuion_Edit = results.First();
                        this.InstitutionName_Edit = this.SelectedInstintuion_Edit.shortnamecn;
                    }
                    else 
                    {
                        this.InstitutionName_Edit = obj.institutionName;
                    }
                }

                if (!string.IsNullOrEmpty(obj.parseTraderIdbkey))
                {

                    List<CrmContact> traderList = null;
                    if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                    {
                        traderList = WtUtils.GetContactInfoPo(obj.parseTraderIdbkey, string.Empty);
                    }
                    else
                    {
                        traderList = WtUtils.GetDBContactInfoPo(null, null, obj.parseTraderIdbkey);
                    }
                    if (traderList != null && traderList.Count > 0)
                    {
                        CrmContact crmContact = traderList.First();
                        //List<CrmInst> crmInstList = WtUtils.GetInstitutionsIDInfoPo(crmContact.instid);
                        //if (crmInstList != null && crmInstList.Count > 0)
                        //{
                        //    this.SelectedInstintuion_Edit = crmInstList.First();
                        //}
                        //this.InstitutionName_Edit = crmContact.instname;
                        this.TraderName_Edit = crmContact.namecn;
                        this.SelectedTrader_Edit = crmContact;
                        //hasTraderInfo = true;
                    }
                    else 
                    {
                        this.TraderName_Edit = obj.traderName;
                    }

                }
                //if (hasTraderInfo == false)
                //{
                //    if (!string.IsNullOrEmpty(obj.parseCompanyIdbkey))
                //    {
                //        this.SelectedTrader_Edit = null;
                //        this.TraderName_Edit = string.Empty;

                //        List<string> instList = new List<string>();
                //        instList.Add(obj.parseCompanyIdbkey);
                //        List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                //        if (results != null && results.Count > 0)
                //        {
                //            this.SelectedInstintuion_Edit = results.First();
                //            this.InstitutionName_Edit = this.SelectedInstintuion_Edit.instname;
                //        }
                //    }
                //}
            }


            IntentionQuoteItem item = new IntentionQuoteItem();
            item.CommandNote = this.CommandNotes;
            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
            if (obj.securityId.ToUpper().EndsWith(".IB"))
            {
                item.Exchange = "银行间";
            }
            else if (obj.securityId.ToUpper().EndsWith(".SH"))
            {
                item.Exchange = "上交";
;            }
            else if (obj.securityId.ToUpper().EndsWith(".SZ"))
            {
                item.Exchange = "深交";
            }

            string marketindicator = "";
            if (bondsInfoPo != null)
            {
                //Symbol = bondsInfoPo.Short_Name;
                //Security = bondsInfoPo.Bond_ID;
                //SymbolName = bondsInfoPo.Short_Name;
                item.RemainingTime = bondsInfoPo.RemaingingTime;
                item.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                item.BondSubType = bondsInfoPo.Bond_Subtype;
                //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                //findData.Issuer = bondsInfoPo.issuer;
                item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                //findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                marketindicator = bondsInfoPo.MarketIndicator;
            }
            if (consistency != null)
            {
                item.IsOpposite = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeReverse);
                item.IsSame = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeSyntropy);
            }
            item.IsChecked = true;
            item.IntentionId = obj.intentionId;
            item.InstitutionId = obj.institutionId;
            item.InstitutionName = obj.institutionName;
            item.TraderId = obj.traderId;
            item.TraderName = obj.traderName;
            item.QqNumber = obj.qqNumber;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;



            //BondsInfoPo bi = CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
            if (bondsInfoPo != null)
            {
                if (bondsInfoPo.SECURITY_TYPE == "BNC")
                {
                    if (CommonUtils.RateDP == "1")
                    {
                        item.DeciamlLength = 2;
                        item.RangeValue = 0.01m;
                    }
                    else
                    {
                        item.DeciamlLength = 4;
                        item.RangeValue = 0.0025m;
                    }
                }
                else
                {
                    if (CommonUtils.CreditDP == "2")
                    {
                        item.DeciamlLength = 4;
                        item.RangeValue = 0.0025m;
                    }
                    else
                    {
                        item.DeciamlLength = 2;
                        item.RangeValue = 0.01m;
                    }
                }
            }
            else
            {
                item.DeciamlLength = 4;
                item.RangeValue = 0.0025m;
            }
            item.Symbol = obj.symbol;
            item.CleanPrice = obj.cleanPrice;
            item.FullPrice = obj.fullPrice;
            item.Price = obj.price;
            //item.ClOrdID = obj.clordid;
            decimal qty = obj.qty.ConvertToDecimal() / 100;
            if (qty >= 1)
            {
                item.Qty = qty * 100;
            }
            //item.Qty = obj.qty;

            item.SettlType = obj.settlType;
            //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;

            //item.BrokerId = obj.brokerId;
            //item.BrokerName = obj.brokerName;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            item.CloseBy = obj.closeBy;
            item.CloseByName = obj.closeByName;
            item.Status = obj.status;
            item.OrderID = obj.orderId;
            item.WholeFlag = obj.wholeFlag;
            item.ClOrdID = obj.clordid;
            item.TimeToMaturity = obj.timeToMaturity;
            item.TimeToMaturityToWorkDay = obj.timeToMaturityToWorkDay;
            item.TimeToMaturity_Convert =  Utils.CommonUtils.ConvertRemainingTime(obj.timeToMaturity);
            try
            {
                //if (!_symbolCountDic.ContainsKey(er.securityid))
                //{
                //    _symbolCountDic[er.securityid] = null;
                //    LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                //    PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                //}

                if (_bestPriceDic.ContainsKey(obj.securityId))
                {
                    item.Best_Bid = GetBestPrice(_bestPriceDic[obj.securityId][0]);
                    item.Best_Ofr = GetBestPrice(_bestPriceDic[obj.securityId][1]);
                    item.Best_Bid_PriceType = GetPriceType(_bestPriceDic[obj.securityId][0]);
                    item.Best_Ofr_PriceType = GetPriceType(_bestPriceDic[obj.securityId][1]);
                }
                else
                {
                    PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                }

                //CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(obj.securityId);
                //if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                //{
                //    string ccdcValue = ccdcValueModel.CCDCValuation;
                //    int index = ccdcValue.IndexOf("|");
                //    if (index != -1)
                //    {
                //        ccdcValue = ccdcValue.Substring(0, index);
                //    }
                //    item.CCDCValuation = ccdcValue.ConvertToDecimal();
                //    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                //    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                //}
                item.Sign = obj.sign;
                item.Sign1 = obj.sign1;
                item.Sign2 = obj.sign2;
                item.Sign3 = obj.sign3;
                item.Sign4 = obj.sign4;

                item.TermDays = obj.termDays;
                item.CouponRateCurrent = obj.couponRateCurrent;
                item.IssuerCode = obj.issuerCode;
                item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                item.WarranterCode = obj.warranterCode;
                item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                item.ChinaBondValuation = obj.chinaBondValuation;
                item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                item.CsiBondValuation = obj.csiBondValuation;
                item.CsiBondValuationFull = obj.csiBondValuationFull;
                if (SignParser.IsCumType(item.SignDic1))
                {
                    if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                    {
                        int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                        }
                    }

                }
                else
                {
                    item.ChinaBondDue = obj.chinaBondValuation;
                }
                if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                {
                    int speIndex = obj.csiBondValuationFull.IndexOf('|');
                    if (speIndex > -1)
                    {
                        item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                        item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                    }
                }
                else
                {
                    item.CsiBondValuationDue = obj.csiBondValuation;
                }
                if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                    else if (item.Side == "2")
                        item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                }
                else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                    else if (item.Side == "2")
                        item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                }
                if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                    else if (item.Side == "2")
                        item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                }
                else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                    else if (item.Side == "2")
                        item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
                }
                item.ModifiedDuration = obj.modifiedDuration;
                if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                {
                    JObject jsonObj = JObject.Parse(obj.inf3);
                    if (jsonObj.ContainsKey("Val_Modified_Duration"))
                    {
                        //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                        string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                        //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        //string valuation_date = jsonObj["valuation_date"].ToString();
                        if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                        {
                            item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                        }
                    }

                    if (jsonObj.ContainsKey("Outstanding_Amount"))
                    {
                        string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                        {
                            item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                        }
                    }
                }
                item.DayOffStr = obj.dayOffStr;
                item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                item.Level = SignParser.GenLevel(item.SignDic3);
                item.RegionType = SignParser.GenRegionType(item.SignDic2);
                item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                item.IssueType = SignParser.GenIssueType(item.SignDic1);
                item.IssueAmount = obj.issueAmount / 10000;
                if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                {
                    item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                }
                item.QuoteType = obj.quotetype;
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            item.PriceType = obj.priceType;

            if (bondsInfoPo != null)
            {
                // 含权
                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                {
                    if (item.PriceTypeDic.Count == 2)
                    {
                        item.PriceTypeDic.Add(new KeyValuePair<string, string>("7", "行权"));
                    }
                    item.PriceType = "7";
                }
                else
                {
                    item.PriceType = "3";
                    if (item.PriceTypeDic.Count == 3)
                    {
                        item.PriceTypeDic.RemoveAt(2);
                    }
                }
            }


            item.PropertyChanged += Item_PropertyChanged;

            this.IntentionQuoteInfo.Add(item);

            return item;
        }

        
        /// <summary>
        /// key:securityid; value:bestPrices 0:bestBid 1:bestOfr 
        /// </summary>
        ConcurrentDictionary<string, GNoMDEntries[]> _bestPriceDic = new ConcurrentDictionary<string, GNoMDEntries[]>();

        private void GetBestSnapshot(MarketDataSnapshotFullRefresh price)
        {
            if (!_bestPriceDic.ContainsKey(price.SecurityID))
            {
                _bestPriceDic[price.SecurityID] = new GNoMDEntries[2];
                _bestPriceDic[price.SecurityID][0] = null;
                _bestPriceDic[price.SecurityID][1] = null;
            }

            //if (!_priceChangeDic.ContainsKey(price.SecurityID))
            //{
            //    _priceChangeDic[price.SecurityID] = new bool[2];
            //    _priceChangeDic[price.SecurityID][0] = false;
            //    _priceChangeDic[price.SecurityID][1] = false;
            //}

            // 剔除
            var bestPrices_best_bid = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "0").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_bid != null && bestPrices_best_bid.Count() > 0)
            {
                GNoMDEntries entry = bestPrices_best_bid.First();
                

                if (_bestPriceDic[price.SecurityID][0] != entry)
                {
                    _bestPriceDic[price.SecurityID][0] = entry;
                    //_priceChangeDic[price.SecurityID][0] = true;
                }
            }
            else
            {
                _bestPriceDic[price.SecurityID][0] = null;
            }

            var bestPrices_best_ofr = price.NoMDEntries?.Where(entry => entry.VenueTypeGW != "INNER" && entry.MDEntryType == "1").OrderBy(entry => entry.MDPriceLevel.ConvertToLong());
            if (bestPrices_best_ofr != null && bestPrices_best_ofr.Count() > 0)
            {
                GNoMDEntries entry = bestPrices_best_ofr.First();
               
                if (_bestPriceDic[price.SecurityID][1] != entry)
                {
                    _bestPriceDic[price.SecurityID][1] = entry;
                    //_priceChangeDic[price.SecurityID][1] = true;
                }
            }
            else
            {
                _bestPriceDic[price.SecurityID][1] = null;
            }

            //var bestPrices = price.NoMDEntries?.Where(item => item.MDPriceLevel == "1");
            //foreach (GNoMDEntries entry in bestPrices)
            //{
            //    if (entry.MDEntryType == "0") // bid
            //    {
            //        _bestPriceDic[price.SecurityID][0] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //    else if (entry.MDEntryType == "1") // ofr
            //    {
            //        _bestPriceDic[price.SecurityID][1] = entry.MDEntryPx.ConvertToDecimal();
            //    }
            //}
        }

        private void UpdateCtPrice(MarketDataSnapshotFullRefresh price)
        {
            GetBestSnapshot(price);
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //CCDCValuaModel srcccdc = null;

                    //try
                    //{
                    //    srcccdc = CommonUtils.GetCCDCValua(price.SecurityID);
                    //    if (srcccdc == null)
                    //    {
                    //        srcccdc = CommonUtils.GenerateCCDCValue(price);

                    //        CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    LogHandler.Error(e);
                    //}
                    
                    lock (_LockObject)
                    {
                        IEnumerable<IntentionQuoteItem> findDatas_best = IntentionQuoteInfo.Where(item => item.SecurityId == price.SecurityID);
                        foreach (IntentionQuoteItem findData in findDatas_best)
                        {
                            //string ccdcValue = srcccdc.CCDCValuation;
                            //int index = ccdcValue.IndexOf("|");
                            //if (index != -1)
                            //{
                            //    ccdcValue = ccdcValue.Substring(0, index);
                            //}
                            //findData.CCDCValuation = ccdcValue.ConvertToDecimal();
                            //findData.CCDCValuation_Date = srcccdc.CCDCValuaDate;
                            //findData.ShowLastWorkDay = srcccdc.ShowLastWorkDay;
                            if (_bestPriceDic.ContainsKey(price.SecurityID))
                            {
                                findData.Best_Bid = GetBestPrice(_bestPriceDic[price.SecurityID][0]);
                                findData.Best_Ofr = GetBestPrice(_bestPriceDic[price.SecurityID][1]);
                                findData.Best_Bid_PriceType = GetPriceType(_bestPriceDic[price.SecurityID][0]);
                                findData.Best_Ofr_PriceType = GetPriceType(_bestPriceDic[price.SecurityID][1]);
                            }
                            
                        }
                    }
                }));
            }
            
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Qty"
                || e.PropertyName == "Price")
            {
                IsFirmPublishBtnEnable = IsPublishBtnEnable = this.intentionQuoteInfo.Count > 0 ? true : false;

                if (IsFirmPublishBtnEnable)
                {
                    foreach (IntentionQuoteItem data in this.IntentionQuoteInfo)
                    {
                        if (!data.Price.HasValue || !data.Qty.HasValue)
                        {
                            IsFirmPublishBtnEnable = false;
                            break;
                        }
                    }
                }
            }
        }

        public void RecognizeIntentionQuote(string content)
        {
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string pattern = @"(\r\n)+";
                    string txtcont = System.Text.RegularExpressions.Regex.Replace(content, pattern, "\r\n");
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);
                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }

                                    // 风控检查
                                    if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                                    {
                                        BatchCheckRisk();
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }


        public void RecognizeIntentionQuoteImage(string content)
        {
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = content;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserImage(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);

                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }

                                    // 风控检查
                                    if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                                    {
                                        BatchCheckRisk();
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }

        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (editBtn != null && editBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = editBtn.DataContext as IntentionQuoteItem;
                EditRow(item);
            }
        }

        internal void EditRow(IntentionQuoteItem item)
        {
            item.IsEdit = "0";
            item.InstitutionName_Edit = item.InstitutionName;
            //item.InstitutionName_Text = item.InstitutionName;
            item.TraderName_Edit = item.TraderName;
            //item.TraderName_Text = item.TraderName;
            item.Side_Edit = item.Side;
            item.Price_Edit = item.Price;
            item.Qty_Edit = item.Qty;
            item.SettlType_Edit = item.SettlType;
            item.WholeFlag_Edit = item.WholeFlag;
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;
        }

        internal static string COLUMNNAME_INSTITUTION = "来源方机构";
        internal static string COLUMNNAME_TRADER = "来源方";
        internal static string COLUMNNAME_SIDE = "方向";
        internal static string COLUMNNAME_PRICE = "报价";
        internal static string COLUMNNAME_QTY = "报量(万)";
        internal static string COLUMNNAME_SETTLETYPE = "清算速度";

        internal void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
            // 价格
            decimal? price = null;
            // 报量
            decimal? qty = null;
            // 整量
            string wholeFlag = null;
            // 清算速度
            string settlType = null;
            // 交易员编号
            string traderId = null;
            // 交易员名称
            string traderName = null;
            // 经纪商编号
            string brokerId = null;
            // 经纪商名称
            string brokerName = null;
            // 机构编号
            string institutionId = null;
            // 机构名称
            string institutionName = null;
            // 方向
            string side = null;

            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                if (this.SelectedInstintuion_Edit != null)
                {
                    institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = this.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = this.InstitutionName_Edit;
                }
                if (institutionId == item.InstitutionId && institutionName == item.InstitutionName)
                {
                    return;
                }

                //// 联动检查客户名
                //if (!string.IsNullOrEmpty(institutionId) 
                //        && !string.IsNullOrEmpty(item.TraderId))
                //{
                //    List<CrmContact> CrmContactList = CommonUtils.GetContactidInfoPo(item.InstitutionId);
                //    if (!CrmContactList.Any(m=>m.ContactId == item.TraderId))
                //    {
                //        traderName = string.Empty;
                //        traderId = string.Empty;
                //    }
                //}
            }

            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                if (this.SelectedTrader_Edit != null)
                {
                    traderName = this.SelectedTrader_Edit.namecn;
                    traderId = this.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderName = this.TraderName_Edit;
                    traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                if (traderId == item.TraderId && traderName == item.TraderName)
                {
                    return;
                }
            }

            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            {
                side = item.Side_Edit;
                if (side == item.Side)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                price = item.Price_Edit;
                if (price == item.Price)
                    return;
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                qty = item.Qty_Edit;
                wholeFlag = item.WholeFlag_Edit;
                if (qty == item.Qty && wholeFlag == item.WholeFlag)
                    return;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                //DateTime createtime = CommonUtils.ConvertToDateTime(item.CreateTime);
                //settlType = CommonUtils.IsSpotSettle(createtime, item.edit_settltype_date, item.edit_settletype);
                //if (string.IsNullOrEmpty(settlType))
                //{
                //    settlType = item.edit_settltype_date.ToString("MM.dd") + (item.edit_settletype == 0 ? "+0" : "+1");
                //}
                settlType = item.SettlType_Edit;
                if (settlType == item.SettlType)
                    return;
            }

            // 新更新
            //UpdateIntentionQuoteRequest request = new UpdateIntentionQuoteRequest();
            //request.intentionId = item.IntentionId;
            //request.institutionId = institutionId;
            //request.institutionName = institutionName;
            //request.traderId = traderId;
            //request.traderName = traderName;
            //request.side = side;
            //request.price = price;
            //request.qty = qty;
            //request.settlType = settlType;
            //bool isSuccess = await IntentionOldHandler.Instance.UpdateIntentionQuote(request);
            //if (isSuccess)
            //{
            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                item.InstitutionName = institutionName;
                item.InstitutionId = institutionId;
            }
            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                item.TraderName = traderName;
                item.TraderId = traderId;
            }
            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
                item.Side = side;
            //// 价格
            //if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            //    item.Price = price;
            //// 数量
            //if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            //{
            //    item.Qty = qty;
            //    item.WholeFlag = wholeFlag;
            //}
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
                item.SettlType = settlType;

            //if (string.IsNullOrEmpty(item.OrderID) == false)
            //{
            //    // 更新我的订单/我的撤单
            //    MyOrderProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //       item.InstitutionId, item.InstitutionName,
            //       item.TraderId, item.TraderName
            //       );
            //    // 更新我的成交
            //    MyExecutionProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //        item.InstitutionId, item.InstitutionName,
            //        item.TraderId, item.TraderName
            //        );
            //}
            //}
            // 风控检查
            if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
            {
                List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
                list.Add(item);
                BatchCheckRisk(false, null, list);
            }
        }

        public void DeleteOrder()
        {
            if (this.SelectedItem_Recognize != null)
            {
                this.intentionQuoteInfo.Remove(this.SelectedItem_Recognize);
                ClickChkBox();
            }
        }

        public async void UpdateOrder(Window win, string quoteType = "INTENTION")
        {
            if (this.ViewSource.View.IsEmpty == false)
            {
                bool hasError = false;
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    foreach (IntentionQuoteItem item in this.ViewSource.View)
                    {
                        if (!item.IsChecked) continue;

                        string managerid;
                        string managerName;
                        string traderid;
                        string traderName;

                        if (this.SelectedTrader_Buy_Edit != null)
                        {
                            traderid = this.SelectedTrader_Buy_Edit.Username;
                            traderName = this.SelectedTrader_Buy_Edit.Name;
                        }
                        else
                        {
                            traderid = this.Trader_Buy_Edit;
                            traderName = this.Trader_Buy_Edit;
                        }

                        if (this.SelectedInvestor_Edit != null)
                        {
                            managerid = this.SelectedInvestor_Edit.Username;
                            managerName = this.SelectedInvestor_Edit.Name;
                        }
                        else
                        {
                            managerid = this.Investor_Edit;
                            managerName = this.Investor_Edit;
                        }

                        if (CommonUtils.IsInvestor())
                        {
                            if (string.IsNullOrEmpty(managerid))
                            {
                                managerid = LocalEnv.UserInfo.UserName;
                                managerName = LocalEnv.UserInfo.FullName;
                            }

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(traderid))
                            {
                                traderid = LocalEnv.UserInfo.UserName;
                                traderName = LocalEnv.UserInfo.FullName;
                            }
                        }

                        // 持仓组合id
                        string bookid = string.Empty;
                        if (item.SelectedGroupItem != null && item.SelectedGroupItem.Count > 0)
                        {
                            bookid = item.SelectedGroupItem[0];
                        }
                        //持仓组合名称
                        string bookName = string.Empty;
                        if (item.SelectedGroupText != null && item.SelectedGroupText.Count > 0)
                        {
                            bookName = item.SelectedGroupText[0];
                        }
                        // 持仓组合账号id
                        string accountid = string.Empty;
                        if (!string.IsNullOrEmpty(item.SelectedTradeAccountId))
                        {
                            accountid = item.SelectedTradeAccountId;
                        }
                        // 持仓组合账号名称
                        string accountName = string.Empty;
                        if (!string.IsNullOrEmpty(item.SelectedTradeAccountName))
                        {
                            accountName = item.SelectedTradeAccountName;
                        }

                        STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameData = IntentionOldHandler.Instance.GetSameIntentionInfo(managerid, traderid, item.SecurityId, item.Side, accountid);

                        if (sameData != null)
                        {
                            // 如果是撤销中， 直接拒绝
                            if (sameData.status == "82")
                            {
                                string side = ComboSourceUtil.Instance.FindEnum(item.Side, "side");
                                string manager = string.IsNullOrEmpty(managerName) ? string.Empty : $"{managerName}(投资经理)";
                                string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                                string trader = string.IsNullOrEmpty(traderName) ? string.Empty : $"{traderName}(交易员),";
                                string account = string.Empty;
                                if (LocalEnv.ConfInfo.IsPAYL)
                                {
                                    account = string.IsNullOrEmpty(accountName) ? string.Empty : $"在{accountName}(交易账户)下,";
                                }
                                string message = $"已有{manager}{managerComma}{trader}{account}针对{item.SecurityId}债券的{side}需求正在撤销中，将无法新增需求";

                                item.RiskNote = message;

                                hasError = true;
                            }
                        }
                    }
                }

                if (hasError)
                {
                    CustomMessageBox.Info($"要素有误", "提示");
                    return;
                }


                STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
                request.quoteType = quoteType;
                request.userName = LocalEnv.UserInfo.UserName;

                List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> quoteList = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();

                request.quoteList = quoteList;
                foreach (IntentionQuoteItem item in this.ViewSource.View)
                {
                    if (!item.IsChecked) continue;
                    STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo record = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();
                    record.intentionId = item.IntentionId;

                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        record.counterPartyID = record.institutionId = LocalEnv.UserInfo.Termids;
                        record.counterPartyName = record.institutionName = LocalEnv.UserInfo.biztype;
                        record.traderId = LocalEnv.UserInfo.UserName;
                        record.traderName = LocalEnv.UserInfo.FullName;

                        string managerid;
                        string managerName;
                        string traderid;
                        string traderName;

                        if (this.SelectedTrader_Buy_Edit != null)
                        {
                            traderid = this.SelectedTrader_Buy_Edit.Username;
                            traderName = this.SelectedTrader_Buy_Edit.Name;
                        }
                        else
                        {
                            traderid = this.Trader_Buy_Edit;
                            traderName = this.Trader_Buy_Edit;
                        }

                        if (this.SelectedInvestor_Edit != null)
                        {
                            managerid = this.SelectedInvestor_Edit.Username;
                            managerName = this.SelectedInvestor_Edit.Name;
                        }
                        else
                        {
                            managerid = this.Investor_Edit;
                            managerName = this.Investor_Edit;
                        }

                        if (CommonUtils.IsInvestor())
                        {
                            if (string.IsNullOrEmpty(managerid))
                            {
                                managerid = LocalEnv.UserInfo.UserName;
                                managerName = LocalEnv.UserInfo.FullName;
                            }
                            record.roletype = "0011";//投机经理

                            record.counterPartySubID = traderid;
                            record.counterPartySubName = traderName;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(traderid))
                            {
                                traderid = LocalEnv.UserInfo.UserName;
                                traderName = LocalEnv.UserInfo.FullName;
                            }
                            record.roletype = "0100";//交易员

                            record.counterPartySubID = managerid;
                            record.counterPartySubName = managerName;
                        }

                        record.partyManager = managerid;
                        record.partyManagerName = managerName;
                        record.partyTrader = traderid;
                        record.partyTraderName = traderName;

                        NoteData info5tag = new NoteData();
                        info5tag.PUBLIC_REMARKS = this.PublicRemarks;
                        if (info5tag.PERSONAL_REMARKS == null)
                        {
                            info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                        }
                        info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = this.PersonalRemarks;
                        info5tag.COMMAND_NOTES = this.CommandNotes;

                        record.info5 = JsonConvert.SerializeObject(info5tag);

                    }
                    else
                    {
                        // 机构编号
                        string institutionId = null;
                        // 机构名称
                        string institutionName = null;
                        if (this.SelectedInstintuion_Edit != null)
                        {
                            institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                            institutionId = this.SelectedInstintuion_Edit.idbkey;
                        }
                        else
                        {
                            institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                            institutionName = this.InstitutionName_Edit;
                        }
                        record.institutionId = institutionId;
                        record.institutionName = institutionName;
                        // 交易员编号
                        string traderId = null;
                        // 交易员名称
                        string traderName = null;
                        if (this.SelectedTrader_Edit != null)
                        {
                            traderName = this.SelectedTrader_Edit.namecn;
                            traderId = this.SelectedTrader_Edit.idbkey;
                            //item.QqNumber = item.SelectedTrader_Edit.QQ;
                        }
                        else
                        {
                            traderName = this.TraderName_Edit;
                            traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                        }
                        record.traderId = traderId;
                        record.traderName = traderName;
                    }
                    

                    if (this.SelectedBroker_Edit != null)
                    {
                        record.userName = this.SelectedBroker_Edit.Username;
                    }
                   
                    record.side = item.Side;
                    record.securityId = item.SecurityId;
                    record.symbol = item.Symbol;
                    //record.price = item.Price;
                    record.qty = item.Qty;
                    record.settlType = item.SettlType;
                    record.text = item.Text;
                    //record.userName = item.UserName;
                    record.closeBy = LocalEnv.UserInfo.UserName;// item.CloseBy;
                    record.closeByName = LocalEnv.UserInfo.FullName;
                    record.status = item.Status;
                    record.brokerId = item.UserName;
                    record.brokerName = item.UserName;
                    record.qqNumber = item.QqNumber;
                    record.createTime = item.CreateTime;
                    record.updateTime = item.UpdateTime;
                    record.fullPrice = item.FullPrice;
                    record.cleanPrice = item.CleanPrice;
                    record.wholeFlag = item.WholeFlag;
                    record.priceType = item.PriceType;

                    if (LocalEnv.ConfInfo.IsAccountUser == "1")
                    {
                        if (this.AccountUserItem != null)
                        {
                            record.accountID = this.AccountUserItem.Id;
                            record.accountName = this.AccountUserItem.AccountName;
                        }
                    }

                    record.vipCode = IsVip ? "0001" : "0010";
                    if (item.IsOpposite && item.IsSame)
                    {
                        record.ifSubBond = "ALL";
                    }
                    else if (item.IsOpposite)
                    {
                        record.ifSubBond = item.Side == "1" ? "OFR" : "BID";
                    }
                    else if (item.IsSame)
                    {
                        record.ifSubBond = item.Side == "1" ? "BID" : "OFR";
                    }
                    record.orgType = SelectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;

                    //默认时间
                    if (this.DefaultValidTimeQuotationCheck)
                    {
                        int hour = DateTimeEx.ServerTime.Hour;
                        int min = DateTimeEx.ServerTime.Minute;
                        if (hour > 0 && hour < 6)
                        {
                            record.validuntitime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (hour >= 6 && hour < 12)
                        {
                            record.validuntitime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            record.validuntitime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }

                    // 时长
                    if (this.HourValidTimeQuotationCheck)
                    {
                        // ValidMinute  ValidSecond
                        int totalsecond = 0;
                        if (!string.IsNullOrEmpty(this.ValidMinute))
                        {
                            totalsecond = int.Parse(this.ValidMinute) * 60;
                        }

                        if (!string.IsNullOrEmpty(this.ValidSecond))
                        {
                            totalsecond += int.Parse(this.ValidSecond);
                        }

                        if (totalsecond == 0)
                        {
                            record.validuntitime = DateTimeEx.ServerTime.AddSeconds(this.DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            record.validuntitime = DateTimeEx.ServerTime.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }

                    // 时点
                    if (this.TimeValidTimeQuotationCheck)
                    {
                        // ValidTimeHour ValidTimeMinute ValidTimeSecond
                        int hour = 0;
                        int min = 0;
                        int sec = 0;

                        if (!string.IsNullOrEmpty(this.ValidTimeHour))
                        {
                            hour = int.Parse(this.ValidTimeHour);
                        }

                        if (!string.IsNullOrEmpty(this.ValidTimeMinute))
                        {
                            min = int.Parse(this.ValidTimeMinute);
                        }

                        if (!string.IsNullOrEmpty(this.ValidTimeSecond))
                        {
                            sec = int.Parse(this.ValidTimeSecond);
                        }

                        DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd").ToString());
                        tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                        record.validuntitime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    // 无
                    if (this.NoLimitValidTimeQuotationCheck)
                    {
                        // 不填写
                    }

                    // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
                    string validTimeType = this.DefaultValidTimeQuotationCheck ? "0" : this.HourValidTimeQuotationCheck ? "1" : this.TimeValidTimeQuotationCheck ? "2" : this.NoLimitValidTimeQuotationCheck ? "3" : "0";
                    // 默认
                    string info1 = string.Empty;
                    if (this.DefaultValidTimeQuotationCheck)
                    {
                        info1 = $"{validTimeType}|";
                        OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = record.validuntitime, IF_SUB_BOND = record.ifSubBond ?? string.Empty , VIP = record.vipCode ?? string.Empty, ORG_TYPE = record.orgType ?? string.Empty };
                        record.inf1 = JsonConvert.SerializeObject(otm);
                    }

                    // 时长
                    if (this.HourValidTimeQuotationCheck)
                    {
                        // ValidMinute,ValidSecond
                        string min = string.IsNullOrEmpty(this.ValidMinute) ? "" : this.ValidMinute;
                        string sec = string.IsNullOrEmpty(this.ValidSecond) ? "" : this.ValidSecond;
                        info1 = $"{validTimeType}|{min}:{sec}";
                        OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = record.validuntitime, IF_SUB_BOND = record.ifSubBond ?? string.Empty, VIP = record.vipCode ?? string.Empty, ORG_TYPE = record.orgType ?? string.Empty };
                        record.inf1 = JsonConvert.SerializeObject(otm);
                    }
                    // 时点
                    if (this.TimeValidTimeQuotationCheck)
                    {
                        // ValidTimeHour ValidTimeMinute ValidTimeSecond
                        string hour = string.IsNullOrEmpty(this.ValidTimeHour) ? "" : this.ValidTimeHour;
                        string min = string.IsNullOrEmpty(this.ValidTimeMinute) ? "" : this.ValidTimeMinute;
                        string sec = string.IsNullOrEmpty(this.ValidTimeSecond) ? "" : this.ValidTimeSecond;
                        info1 = $"{validTimeType}|{hour}:{min}:{sec}";
                        OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = record.validuntitime, IF_SUB_BOND = record.ifSubBond ?? string.Empty, VIP = record.vipCode ?? string.Empty, ORG_TYPE = record.orgType ?? string.Empty };
                        record.inf1 = JsonConvert.SerializeObject(otm);
                    }

                    // 无
                    if (this.NoLimitValidTimeQuotationCheck)
                    {
                        info1 = $"{validTimeType}|";
                        OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = string.Empty, IF_SUB_BOND = record.ifSubBond ?? string.Empty, VIP = record.vipCode ?? string.Empty, ORG_TYPE = record.orgType ?? string.Empty };
                        record.inf1 = JsonConvert.SerializeObject(otm);
                    }

                    // 用户设置的到期时间,需要大于用户自定义的时间
                    string selfUntilTime = record.validuntitime;
                    UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
                    if (userSetting != null)
                    {
                        // 个人设置时间
                        string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                        // 如果撤单时间和个人设置时间不为空
                        if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                        {
                            validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTime.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                            // 默认，时长，时点
                            if (!this.NoLimitValidTimeQuotationCheck)
                            {
                                record.validuntitime = GetMinDate(selfUntilTime, validUntiTimeSetting, this.DefaultValidTimeQuotationCheck);
                            }
                            else
                            {
                                //选择了无
                                if (DateTimeEx.ServerTime.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                                {
                                    record.validuntitime = validUntiTimeSetting;
                                }
                                else
                                {
                                    record.validuntitime = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            // 默认，时长，时点
                            if (!this.NoLimitValidTimeQuotationCheck)
                            {
                                // 默认
                                if (this.DefaultValidTimeQuotationCheck)
                                {
                                    record.validuntitime = GetMinDate(selfUntilTime, "", true);
                                }
                            }
                            else
                            {
                                record.validuntitime = string.Empty;
                            }
                        }
                    }

                    record.hurryFlag = this.HurryFlag;
                    record.suggestFlag = this.SuggestFlag;
                    record.fireFlag = this.FireFlag;
                    record.floorPrice = this.FloorPrice;
                    record.OcoFlag = this.OcoFlag;
                    record.publicRemarks = this.PublicRemarks;
                    record.personalRemarks = this.PersonalRemarks;
                    record.customerPrice = item.Price;
                    //record.customerSpread = this.BP.HasValue ? this.BP : null;

                    record.price = item.Price;
                    record.channel = "CLIENT";
                    record.handlinst = "2";
                    record.clordId = DataTimeSequence.Instance.GetId();
                    record.quotetype = quoteType;
                    record.tempPrice = item.Price;

                    quoteList.Add(record);
                }
                Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result = await IntentionOldHandler.Instance.InsertIntentionQuote(request);
                if (result.IsSuccess())
                {
                    RemenberInstAndTrade();
                }

                win.Close();
                //if (isSucces)
                //{
                //    foreach(IntentionQuoteItem item in this.viewSource.View)
                //    {
                //        IntentionQuoteInfo obj = new IntentionQuoteInfo();
                //        // 意向报价唯一编号
                //        obj.intentionId = item.IntentionId;
                //        // 机构编号
                //        string institutionId = null;
                //        // 机构名称
                //        string institutionName = null;
                //        if (this.SelectedInstintuion_Edit != null)
                //        {
                //            institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                //            institutionId = this.SelectedInstintuion_Edit.idbkey;
                //        }
                //        else
                //        {
                //            institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                //            institutionName = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionName : string.Empty;
                //        }
                //        // 机构编号
                //        obj.institutionId = institutionId;
                //        // 机构名称
                //        obj.institutionName = institutionName;
                //        // 交易员编号
                //        string traderId = null;
                //        // 交易员名称
                //        string traderName = null;
                //        if (this.SelectedTrader_Edit != null)
                //        {
                //            traderName = this.SelectedTrader_Edit.namecn;
                //            traderId = this.SelectedTrader_Edit.idbkey;
                //            //item.QqNumber = item.SelectedTrader_Edit.QQ;
                //        }
                //        else
                //        {
                //            traderName = this.TraderName_Edit;
                //            traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                //        }
                //        // 交易员编号
                //        obj.traderId = traderId;
                //        // 交易员名称
                //        obj.traderName = traderName;
                //        // 经纪商编号
                //        obj.brokerId = item.BrokerId;
                //        // 经纪商名称
                //        obj.brokerName = item.BrokerName;
                //        // QQ号码
                //        obj.qqNumber = item.QqNumber;
                //        // 债券编号
                //        obj.securityId = item.SecurityId;
                //        // 债券简称
                //        obj.symbol = item.Symbol;
                //        // 报价方向 1=买、2=卖
                //        obj.side = item.Side;
                //        // 价格
                //        obj.price = item.Price;
                //        // 净价
                //        obj.cleanPrice = item.CleanPrice;
                //        // 全价
                //        obj.fullPrice = item.FullPrice;
                //        // 报量
                //        obj.qty = item.Qty;
                //        // 清算类型 1=T+0, 2=T+1, 3=Tm+0, 4=Tm+1, 99=其他
                //        obj.settlType = item.SettlType;
                //        // 原始文本 
                //        obj.text = item.Text;
                //        // 订单状态 ,0=待处理,1=已处理,2=已成交, 3=已忽略
                //        obj.status = item.Status;
                //        // 创建时间
                //        obj.createTime = item.CreateTime;
                //        // 更新时间
                //        obj.updateTime = item.UpdateTime;
                //        // 操作人
                //        obj.closeBy = item.CloseBy;
                //        // 用户名
                //        obj.userName = item.UserName;
                //        IntentionOldHandler.Instance.Instance_OnIntention(obj);
                //    }
                //}
            }
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {

            //BankListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BankListViewSource.View;
            string currentInstId = this.SelectedTrader_Edit == null ? string.Empty : this.SelectedTrader_Edit.instid;

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentInstId))
            {
                if (instIdList == null)
                    instIdList = new List<string>();
                if (!instIdList.Contains(currentInstId))
                {
                    instIdList.Add(currentInstId);
                }
            }

            List<CrmInst> result = new List<CrmInst>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {

                        List<CrmInst> selectDatas = WtUtils.GetInstitutionsInfoPo(str, 2000, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = WtUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 2000, instIdList);
                }
            }
            else
            {
                List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                {
                    result.AddRange(myInstList);
                }
                else
                {
                    result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new InstitutionComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        internal void FilterTrader(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? null : this._selectedInstitution_Edit.idbkey;
            
            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetDBContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetDBContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result = WtUtils.GetDBContactInfoPo(null, currentInstID);
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        internal void FilterTraderLocal(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? string.Empty : this._selectedInstitution_Edit.idbkey;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result.AddRange(WtUtils.GetContactInfoPo(null, currentInstID));
                }
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }

            args.DataSource = result;
        }

        #region 账户组合

        private Visibility _accountUserVisibility = Visibility.Collapsed;

        public Visibility AccountUserVisibility
        {
            get { return _accountUserVisibility; }
            set
            {
                _accountUserVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private AccountUserItem _accountUserItem = null;

        public AccountUserItem AccountUserItem

        {
            get { return _accountUserItem; }
            set
            {
                _accountUserItem = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        public ObservableCollection<AccountUserItem> AccountUserSource
        {
            get
            {
                return _accountUserSource;
            }
        }


        private void InitAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            string defaultAccountID = string.Empty;
            AccountUserVisibility = Visibility.Visible;
            UserSetting Us = SystemParamHandler.Instance.GetUserSetting();

            if (Us != null && !string.IsNullOrEmpty(Us.inf2))
            {
                if (JsonSplit.IsJson(Us.inf2))
                {
                    ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                    defaultAccountID = ck.AccountId;
                }
            }
            _accountUserSource.Add(new AccountUserItem());
            AccountAndUsersQueryReplyPo result = AdminHandler.Instance.getAccountUser();
            if (result != null && result.Datas != null)
            {
                foreach (TbookAccountUsers accountUser in result.Datas)
                {
                    AccountUserItem item = new AccountUserItem() { Id = accountUser.accountid, AccountName = accountUser.accountname };
                    _accountUserSource.Add(item);
                    if (!string.IsNullOrEmpty(defaultAccountID) && defaultAccountID == accountUser.accountid)
                    {
                        AccountUserItem = item;
                    }
                }
            }

        }

        #endregion

        #region firm发布 、NF发布
        private ConfirmIntentionQuoteReplyPo CheckParam(IntentionQuoteItem item)
        {
            ConfirmIntentionQuoteReplyPo reply = new ConfirmIntentionQuoteReplyPo();
            reply.intentionId = item.IntentionId;
            reply.orderId = item.OrderID;
            reply.securityId = item.SecurityId;
            reply.status = item.Status;
            reply.Code = "0000";
            decimal d = 0;
            string errorMsg = string.Empty;
            //if (item.Price == null || item.Price == 0)
            //    errorMsg = "未填写价格或价格为0";
            //else if (item.Qty == null || item.Qty == 0)
            //    errorMsg = "未填写量或量为0";
            //else if (item.Qty % 100 > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty.Value.ToString("0"));
            //else 
            if (string.IsNullOrEmpty(item.SettlType))
                errorMsg = "清算速度未填写";
            else if (string.IsNullOrEmpty(item.Side))
                errorMsg = "方向未填写";
            //else if (item.Qty != null && item.Qty > 0 && decimal.TryParse(item.Qty.ToString(), out d) && (d % 100) > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                reply.Code = "10001";

                List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                TPlaceOrderResult por = new TPlaceOrderResult();
                por.Code = "10003";
                por.DoubleWarn = false;
                por.Reason = errorMsg;
                result.Add(por);

                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);
            }
            return reply;
        }

        private void GetLocalCheckErrorInfo(IntentionQuoteItem item,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                Common.Model.BaseReplyPo replyPo = CheckParam(item);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10001")
                        {
                            exceptionReplyDic[item.ClOrdID] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }
        private void GetErrorInfo(Common.Model.Fix.Message.NewOrderSingle request,
                ref int failCount,
                ref int totalCount,
                Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                request.HandlInst = "2";
                if (LocalEnv.ConfInfo.IsRiskCheck_Sell)
                {
                    request.RiskManualCk = "2";
                }
                RecognizeIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.PublishIntention(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.ClOrdID] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.ClOrdID] = orderRisks;
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.ClOrdID] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        //private void SetDoAllBtnEnable(bool isEnable)
        //{
        //    this.ConfirmEnable = isEnable;
        //    this.QuickConfirmEnable = isEnable;
        //    this.TGEnable = isEnable;
        //}

        internal void DoAllConfirmCommand(Window win)
        {

            //SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                foreach (IntentionQuoteItem item in this.IntentionQuoteInfo)
                {
                    if (!item.Price.HasValue || !item.Qty.HasValue || !item.IsChecked) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.ClOrdID] = item;
                }

                if (!Common.View.Component.CustomMessageBox.Question($"是否执行Firm发布操作？({orderDic.Count}笔)", "Firm发布"))
                {
                    return;
                }


                //if (exceptionReplyDic_local.Count > 1)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.ClOrdID))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            Common.Model.Fix.Message.NewOrderSingle request = GetNewOrderSingle(item, "1");
                            orderDic[item.ClOrdID].SingleOrder = request;

                            GetErrorInfo(item.SingleOrder,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                        try
                        {
                            Thread.Sleep(20);
                        }
                        catch (Exception ex)
                        { }
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                        if (totalCount > 0 && totalCount != failCount)
                        {
                            RemenberInstAndTrade();
                        }
                    }));
                });
                win.Close();
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllConfirmCommand1", e);
                win.Close();
            }
            finally
            {
                //win.Close();
                //SetDoAllBtnEnable(true);
            }
        }

        internal void DoAllQuickConfirmCommand(Window win)
        {

           
            //SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                foreach (IntentionQuoteItem item in this.viewSource.View)
                {
                    if (!item.IsChecked) continue;

                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.ClOrdID] = item;
                }

                //if (!Common.View.Component.CustomMessageBox.Question($"是否执行NF发布操作？({orderDic.Count}笔)", "NF发布"))
                //{
                //    return;
                //}


                //if (exceptionReplyDic_local.Count > 0)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    List<NewOrderSingle> quoteList = new List<NewOrderSingle>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.ClOrdID))
                        {
                            continue;
                        }
                        Common.Model.Fix.Message.NewOrderSingle request = GetNewOrderSingle(item, "0");
                        request.BookID = LocalEnv.ManualOrderDefaultBookId ?? null;
                        quoteList.Add(request);
                    }

                    Common.Model.Table.WorkBrench.BatchPublishIntentionQuoteReq req = new Common.Model.Table.WorkBrench.BatchPublishIntentionQuoteReq();
                    req.userName = LocalEnv.UserInfo.UserName;
                    req.quoteList = quoteList;

                    PResult result = TableOldHandler.Instance.BatchPublishIntention(req);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (result == null || !result.IsSuccess())
                        {
                            string errorMsg = result == null || string.IsNullOrEmpty(result.Msg) ? "发内部失败" : result.Msg;
                            CustomMessageBox.Info(errorMsg, "发内部");
                        }
                        else
                        {
                            RemenberInstAndTrade();
                        }
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllQuickConfirmCommand", e);
            }
            finally
            {
                //SetDoAllBtnEnable(true);
                win.Close();
            }
        }

        private Common.Model.Fix.Message.NewOrderSingle GetNewOrderSingle(IntentionQuoteItem item, string firmflag)
        {
            Common.Model.Fix.Message.NewOrderSingle request = new Common.Model.Fix.Message.NewOrderSingle();
            request.FirmFlag = firmflag;
            request.OrgType = SelectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;
            request.VipCode = IsVip ? "0001" : "0010";
            if (item.IsOpposite && item.IsSame)
            {
                request.IfSubBond = "ALL";
            }
            else if (item.IsOpposite)
            {
                request.IfSubBond = item.Side == "1" ? "OFR" : "BID";
            }
            else if (item.IsSame)
            {
                request.IfSubBond = item.Side == "1" ? "BID" : "OFR";
            }
            request.AlgoName = "CROSS";
            request.VenueTypeGW = "INNER";
            request.Venues = "MANUAL";
            request.ClOrdID = item.ClOrdID;
            //request.IntentionID = item.IntentionId;
            request.MarketIndicator = "4";
            request.OrdType = "2";
            request.OrderQty = item.Qty.ToString();
            //request.Price = item.Price.ToString();
            request.PriceType = item.PriceType; //收益率
            //request.NetPrice = ;
            request.SecurityID = item.SecurityId;
            request.Symbol = item.Symbol;
            //request.OrdStatus = ;
            request.RiskManualCk = "";
            request.SettlType = item.SettlType;
            request.Side = item.Side;
            request.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version); ;
            request.TimeInForce = "0";
            request.Name = request.UserName = LocalEnv.UserInfo.UserName;
            request.WholeFlag = item.WholeFlag;
            request.HandlInst = "1";
            //request.QuantID = ;
            request.MacID = EngineHelper.GetMacByNetworkInterface();
            request.InsideIP = EngineHelper.GetIp();
            request.HardDiskID = EngineHelper.Getharddiskid2();
            // 默认时间
            if (this.DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTimeNow.Hour;
                int min = DateTimeEx.ServerTimeNow.Minute;
                if (hour > 0 && hour < 6)
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (hour >= 6 && hour < 12)
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    request.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时长
            if (this.HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(this.ValidMinute))
                {
                    totalsecond = int.Parse(this.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(this.ValidSecond))
                {
                    totalsecond += int.Parse(this.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    request.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(this.DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    request.ValidUntilTime = DateTimeEx.ServerTimeNow.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时点
            if (this.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    hour = int.Parse(this.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeMinute))
                {
                    min = int.Parse(this.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeSecond))
                {
                    sec = int.Parse(this.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                request.ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 无
            if (this.NoLimitValidTimeQuotationCheck)
            {
                // 不填写
            }
            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = this.DefaultValidTimeQuotationCheck ? "0" : this.HourValidTimeQuotationCheck ? "1" : this.TimeValidTimeQuotationCheck ? "2" : this.NoLimitValidTimeQuotationCheck ? "3" : "0";
            // 默认
            string info1 = string.Empty;
            if (this.DefaultValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 时长
            if (this.HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(this.ValidMinute) ? "" : this.ValidMinute;
                string sec = string.IsNullOrEmpty(this.ValidSecond) ? "" : this.ValidSecond;
                info1 = $"{validTimeType}|{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }
            // 时点
            if (this.TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(this.ValidTimeHour) ? "" : this.ValidTimeHour;
                string min = string.IsNullOrEmpty(this.ValidTimeMinute) ? "" : this.ValidTimeMinute;
                string sec = string.IsNullOrEmpty(this.ValidTimeSecond) ? "" : this.ValidTimeSecond;
                info1 = $"{validTimeType}|{hour}:{min}:{sec}";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = request.ValidUntilTime, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 无
            if (this.NoLimitValidTimeQuotationCheck)
            {
                info1 = $"{validTimeType}|";
                OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = string.Empty, IF_SUB_BOND = request.IfSubBond ?? string.Empty, VIP = request.VipCode ?? string.Empty, ORG_TYPE = request.OrgType ?? string.Empty };
                request.Info1 = JsonConvert.SerializeObject(otm);
            }

            // 用户设置的到期时间,需要大于用户自定义的时间
            string selfUntilTime = request.ValidUntilTime;
            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                // 个人设置时间
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;

                // 如果撤单时间和个人设置时间不为空
                if (!string.IsNullOrEmpty(selfUntilTime) && (!string.IsNullOrEmpty(validUntiTimeSetting)))
                {
                    validUntiTimeSetting = string.Format("{0} {1}", DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString(), validUntiTimeSetting);
                    // 默认，时长，时点
                    if (!this.NoLimitValidTimeQuotationCheck)
                    {
                        request.ValidUntilTime = GetMinDate(selfUntilTime, validUntiTimeSetting, this.DefaultValidTimeQuotationCheck);
                    }
                    else
                    {
                        //选择了无
                        if (DateTimeEx.ServerTimeNow.CompareTo(DateTime.Parse(validUntiTimeSetting)) <= 0)
                        {
                            request.ValidUntilTime = validUntiTimeSetting;
                        }
                        else
                        {
                            request.ValidUntilTime = string.Empty;
                        }
                    }
                }
                else
                {
                    // 默认，时长，时点
                    if (!this.NoLimitValidTimeQuotationCheck)
                    {
                        // 默认
                        if (this.DefaultValidTimeQuotationCheck)
                        {
                            request.ValidUntilTime = GetMinDate(selfUntilTime, "", true);
                        }
                    }
                    else
                    {
                        request.ValidUntilTime = string.Empty;
                    }
                }
            }
            //request.PriceCondition = ;
            if(IntentionOldHandler.Instance.isSALER())
            {
                // 机构编号
                string institutionId = null;
                // 机构名称
                string institutionName = null;
                if (this.SelectedInstintuion_Edit != null)
                {
                    institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = this.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = this.InstitutionName_Edit;
                }
                request.InstitutionID = institutionId;
                request.InstitutionName = institutionName;
                // 交易员编号
                string traderId = null;
                // 交易员名称
                string traderName = null;
                if (this.SelectedTrader_Edit != null)
                {
                    traderName = this.SelectedTrader_Edit.namecn;
                    traderId = this.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderName = this.TraderName_Edit;
                    traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                request.TraderID = traderId;
                request.TraderName = traderName;
            }

            if (this.SelectedBroker_Edit != null)
            {
                request.UserName = this.SelectedBroker_Edit.Username;
            }

            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                if (this.AccountUserItem != null)
                {
                    request.AccountID = this.AccountUserItem.Id;
                    request.AccountName = this.AccountUserItem.AccountName;
                }
            }

            request.Text = item.Text;

            request.OCOFlag = this.OcoFlag;

            request.Price = item.Price.HasValue ? item.Price.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(request.Price))
            {
                CalResult calResult = CommonUtils.CalBond_PriceType(request.SecurityID, request.SettlType, request.Price, request.PriceType);
                if (calResult != null && calResult.Data != null)
                {
                    if (request.PriceType == "7")
                    {
                        request.Price = (calResult.Data.YieldToExecution * 100).ToString();
                    }
                    else
                    {
                        request.Price = (calResult.Data.Yield * 100).ToString();
                    }
                    request.NetPrice = calResult.Data.CleanPrice.ToString();
                    request.FullPrice = calResult.Data.FullPrice.ToString();
                }
            }

            TagData info2tag = new TagData() { IF_SUB_BOND = request.IfSubBond, ORG_TYPE = request.OrgType, VIP = request.VipCode,
                HURRY_FLAG = this.HurryFlag,
                SUGGEST_FLAG = this.SuggestFlag,
                FIRE_FLAG = this.FireFlag,
                FLOOR_PRICE = this.FloorPrice,
                CUSTOMER_PRICE = request.Price,
                //CUSTOMER_SPREAD = this.BP.HasValue ? this.BP.ToString() : string.Empty,
                PUBLIC_REMARKS = this.PublicRemarks,
                PERSONAL_REMARKS = this.PersonalRemarks,
                OCO_FLAG = this.OcoFlag
            };
            request.Info2 = JsonConvert.SerializeObject(info2tag);
            request.Channel = "CLIENT";

            

            return request;
        }


        

        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        internal string GetErrorTxt()
        {
            string errMsg = string.Empty;
            if (this.HourValidTimeQuotationCheck)
            {
                // 时长
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(this.ValidMinute) && string.IsNullOrEmpty(this.ValidSecond))
                {
                    errMsg = "有效时长需大于0秒";
                }

                int totalsecond = 0;
                if (!string.IsNullOrEmpty(this.ValidMinute))
                {
                    totalsecond = int.Parse(this.ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(this.ValidSecond))
                {
                    totalsecond += int.Parse(this.ValidSecond);
                }

                if (totalsecond == 0)
                {
                    errMsg = "有效时长需大于0秒";
                }
            }
            else if (this.TimeValidTimeQuotationCheck)
            {
                // 时点
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if (string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    errMsg = "必须输入时";
                }

                int hour = 0;
                int min = 0;
                int sec = 0;

                if (!string.IsNullOrEmpty(this.ValidTimeHour))
                {
                    hour = int.Parse(this.ValidTimeHour);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeMinute))
                {
                    min = int.Parse(this.ValidTimeMinute);
                }

                if (!string.IsNullOrEmpty(this.ValidTimeSecond))
                {
                    sec = int.Parse(this.ValidTimeSecond);
                }

                DateTime tmpdt = DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd").ToString());
                tmpdt = tmpdt.AddHours(hour).AddMinutes(min).AddSeconds(sec);
                if (DateTimeEx.ServerTimeNow >= tmpdt)
                {
                    errMsg = "订单撤单时点不能在报单时间之前";
                }
            }
            return errMsg;
        }

        /// <summary>
        /// 取2个日期的最小值,当为默认时
        /// </summary>
        /// <param name="selfUntilTime">撤单时间</param>
        /// <param name="validUntiTimeSetting">个人设置</param>
        /// <returns></returns>
        private string GetMinDate(string selfUntilTime, string validUntiTimeSetting = "", bool isDefault = true)
        {
            List<DateTime> LstTime = new List<DateTime>();
            DateTime cudatetime = DateTimeEx.ServerTimeNow;
            LogHandler.Info($"GetMinDate:selfUntilTime:{selfUntilTime} validUntiTimeSetting:{validUntiTimeSetting} isDefault:{isDefault.ToString()}" + " ServerTime:" + cudatetime.ToString("yyyy-MM-dd HH:mm:ss"));
            // 如果是时点, 失效时间<当前时间，则直接失效
            if (!isDefault)
            {
                if (cudatetime.CompareTo(DateTime.Parse(selfUntilTime)) >= 0)
                {
                    return selfUntilTime;
                }
            }

            // DateTime cudatetime = DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 08:35:00");
            if (isDefault)
            {
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 06:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 12:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd") + " 16:30:00"));
            }

            LstTime.Add(cudatetime);
            LstTime.Add(DateTime.Parse(selfUntilTime));
            if (!string.IsNullOrEmpty(validUntiTimeSetting))
            {
                LstTime.Add(DateTime.Parse(validUntiTimeSetting));
            }

            List<DateTime> NewLstTime = LstTime.Distinct().ToList();
            NewLstTime.Sort();
            int i = NewLstTime.IndexOf(cudatetime);
            NewLstTime.RemoveRange(0, i);

            if (NewLstTime.Count == 1)
            {
                return string.Empty;
            }
            else
            {
                return NewLstTime[1].ToString("yyyy-MM-dd HH:mm:ss");
            }
        }



        /// <summary>
        /// 报价选择类型解析  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
        /// </summary>
        /// <param name="info2"></param>
        public void ParseValidTimeFormat()
        {
            string info2 = string.Empty;
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                if (!string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                    if (consistencyCheckPo != null)
                    {
                        info2 = consistencyCheckPo.DefaultValidTime;
                    }
                }
            }

            if (!string.IsNullOrEmpty(info2))
            {
                //报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
                if (info2.StartsWith("0|"))
                {
                    ValidSelectIndex = 0;
                }
                else if (info2.StartsWith("1|"))
                {
                    ValidSelectIndex = 1;
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinute = s.Split(':')[0];
                        ValidSecond = s.Split(':')[1];
                    }
                }
                else if (info2.StartsWith("2|"))
                {
                    ValidSelectIndex = 2;

                    // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidTimeHour = s.Split(':')[0];
                        ValidTimeMinute = s.Split(':')[1];
                        ValidTimeSecond = s.Split(':')[2];
                    }
                }
                else if (info2.StartsWith("3|"))
                {
                    ValidSelectIndex = 3;
                    // 无
                }
            }
        }
        #endregion

        private void RemenberInstAndTrade()
        {
            //如果是自动解析的，则打开需求工作台
            if (IsAutoPrase)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (TabPageHelper.IsExistTabItem("需求工作台"))
                    {
                    }
                }));
            }

            if (this.SelectedTrader_Edit != null && !string.IsNullOrEmpty(this.SelectedTrader_Edit.idbkey))
            {
                Common.Utils.CommonUtils.AddContract(this.SelectedTrader_Edit.idbkey);
                Common.Utils.CommonUtils.SaveContract();
            }
            if (this.SelectedInstintuion_Edit != null && !string.IsNullOrEmpty(SelectedInstintuion_Edit.idbkey))
            {
                Common.Utils.CommonUtils.AddInst(SelectedInstintuion_Edit.idbkey);
                Common.Utils.CommonUtils.SaveInst();
            }
        }



        private CrmInst selectClientTypeItem;

        public CrmInst SelectClientTypeItem
        {
            get { return selectClientTypeItem; }
            set
            {
                if (selectClientTypeItem != value)
                {
                    selectClientTypeItem = value;
                    if (selectClientTypeItem != null)
                        ClientTypeEnum = selectClientTypeItem.shortnamecn;
                    this.OnPropertyChanged();
                }
            }
        }

        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }


        internal void FilterClientType(AutoCompleteArgs args)
        {
            List<CrmInst> result = GetCrmInst(); ;
            if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
            {
                result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
            }
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = result.Where(item => item.shortnamecn.Contains(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }
        private List<CrmInst> GetCrmInst()
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }
            return result;
        }

        internal void FilterBrokerType(AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            STCUserReplyPo result = AdminHandler.Instance.GetCachestcUserQuery();

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102") && ((d.Username != null && d.Username.Contains(args.Pattern))
                || (d.Name != null && d.Name.Contains(args.Pattern)))).ToList();//多条件筛选，新功能增加
            }
            else
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102")).ToList();
            }

            args.DataSource = list;
        }

        #region 选择

        private int dataCount = 0;

        public int DataCount
        {
            get { return dataCount; }
            set
            {
                dataCount = value;
                this.OnPropertyChanged();
            }
        }

        private int selectedCount = 0;

        public int SelectedCount
        {
            get { return selectedCount; }
            set
            {
                selectedCount = value;
                this.OnPropertyChanged();
            }
        }

        // 是否选中全选多选框
        private bool _IsAllChecked = true;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                    int i = 0;
                    foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
                    {
                        item.IsChecked = value;
                        if (value)
                        {
                            i++;
                        }
                    }
                    SelectedCount = i;
                    //CountText = $"共{dataCount},选中{selectedCount}";
                }
            }
        }

        internal void ClickChkBox()
        {
            bool flag = true;
            int i = 0;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsChecked;
                if (item.IsChecked)
                {
                    i++;
                }
            }
            _IsAllChecked = this.intentionQuoteInfo.Count > 0 && flag;
            this.OnPropertyChanged("IsAllChecked");

            SelectedCount = i;
            //CountText = $"共{dataCount},选中{selectedCount}";
        }
        #endregion

        #region 交易所过滤
        List<string> _exchangeFilterList = new List<string>();

        internal void FilterExchage(List<string> filterList)
        {
            _exchangeFilterList = filterList;
            this.ViewSource.View?.Refresh();
        }
        #endregion

        #region 最优 价格标准
        private decimal? GetBestPrice(GNoMDEntries entry)
        {
            if (entry == null)
                return null;

            if (string.IsNullOrEmpty(entry.MDEntryPx))
            {
                return null;
            }
            else
            {
                return entry.MDEntryPx.ConvertToDecimal();
            }
        }

        /// <summary>
        /// 1:净价
        /// 2:全价
        /// 7:行权收益率
        /// 5:到期收益率
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private string GetPriceType(GNoMDEntries entry)
        {
            if (entry != null)
            {
                if (string.IsNullOrEmpty(entry.CalFlag))
                {
                    if (entry.PriceType == "1")// 净价
                    {
                        return "1";
                    }
                    else if (entry.PriceType == "3")//到期收益率
                    {
                        return "3";
                    }
                    else if (entry.PriceType == "2")//全价
                    {
                        return "2";
                    }
                    else if (entry.PriceType == "6")//行权收益率
                    {
                        return "7";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    if (entry.CalFlag == "2")//到期收益率
                    {
                        return "3";
                    }
                    else if (entry.CalFlag == "4")//净价
                    {
                        return "1";
                    }
                    else if (entry.CalFlag == "3")//全价
                    {
                        return "2";
                    }
                    else if (entry.CalFlag == "6")//行权收益率
                    {
                        return "7";
                    }
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region 卖方风控

        internal void BatchCheckRisk(bool isPrompt = false, Window win = null, List<IntentionQuoteItem> list = null)
        {
            List<IntentionQuoteItem> opsList = new List<IntentionQuoteItem>();
            if (list == null)
            {
                opsList.AddRange(this.intentionQuoteInfo);
            }
            else
            {
                opsList.AddRange(list);
            }

            List<NewOrderSingle> batchRiskList = new List<NewOrderSingle>();
            // 过风控
            foreach (IntentionQuoteItem item in opsList)
            {
                if (item.IsChecked == false) continue;

                NewOrderSingle riskReq = new NewOrderSingle();
                riskReq.Venues = "MANUAL";
                riskReq.VenueTypeGW = "INNER";
                riskReq.AlgoName = "CROSS";
                riskReq.StrategyOrderId = item.OrderID;
                riskReq.MarketIndicator = "4";
                riskReq.SecurityID = item.SecurityId;
                riskReq.ActionFlag = "6";
                riskReq.Side = item.Side;
                riskReq.OrderQty = item.Qty?.ToString("0");
                riskReq.CounterPartyName = string.Empty;
                riskReq.SettlType = item.SettlType;
                riskReq.RiskReTryFlag = "0";
                riskReq.UserName = LocalEnv.UserInfo.UserName;
                riskReq.ClOrdID = item.ClOrdID;
                riskReq.OrdStatus = "N";
                riskReq.Price = item.Price.HasValue ? item.Price.Value.ToString() : string.Empty;
                riskReq.OrderQty = item.Qty.HasValue ? item.Qty.Value.ToString() : string.Empty;
                riskReq.SecurityType = item.SecurityType;
                batchRiskList.Add(riskReq);
            }

            Task.Run(() =>
            {
                BatchCheckRiskReplyPo po = RiskHandler.Instance.BatchCheckRisk(batchRiskList);
                if (po == null || !po.IsSuccess())
                {
                    if (po != null && po.Code == "10000" && po.Msg != null && po.Msg.Count > 0)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                        {
                            bool isSuccess = true;
                            foreach (IntentionQuoteItem item in opsList)
                            {
                                string clordid = item.ClOrdID;
                                if (po.Msg.ContainsKey(clordid))
                                {
                                    List<TPlaceOrderResult> riskResult = po.Msg[clordid];
                                    if (riskResult != null)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        foreach (TPlaceOrderResult failMsg in riskResult)
                                        {
                                            if (sb.Length > 0)
                                            {
                                                sb.Append(";");
                                            }

                                            if (failMsg.Code == "10001")
                                            {
                                                sb.Append(failMsg.Reason);
                                            }

                                            if (failMsg.Code == "10002")
                                            {
                                                sb.Append(failMsg.Reason);
                                            }
                                        }
                                        item.RiskNote = sb.ToString();// 风控检查备注
                                        if (!string.IsNullOrEmpty(item.RiskNote))
                                        {
                                            isSuccess = false;
                                        }
                                    }
                                }
                                await System.Windows.Threading.Dispatcher.Yield();
                            }

                            if (isPrompt && isSuccess)
                            {
                                CustomMessageBox.Info("风控检查全部通过", "风控检查", win);
                            }
                        }));
                    }
                }
                else
                {
                    if (isPrompt)
                    {
                        CustomMessageBox.Info("风控检查全部通过", "风控检查", win);
                    }
                }
            });
        }

        #endregion
    }
}
