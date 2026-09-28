using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.PT.Plugin.Handler.Process;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using static STC.PT.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.PT.Plugin.Utils.CommonUtils;
namespace STC.PT.Plugin.ViewModel
{
    public class ManualEntryViewModel : ViewModelBase
    {
        private int _SideIndex = -1;
        public int SideIndex
        {
            get { return _SideIndex; }
            set
            {
                _SideIndex = value;
                this.OnPropertyChanged();
            }
        }
        private DateTime? _TradeDate;
        public DateTime? TradeDate
        {
            get { return _TradeDate; }
            set
            {
                string oldDate = _TradeDate == null ? string.Empty : _TradeDate.Value.Date.ToString(YYYYMMDD);
                _TradeDate = value;
                string newDate = _TradeDate == null ? string.Empty : _TradeDate.Value.Date.ToString(YYYYMMDD);
                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }
                CalBond(GetSetttle(), true);
                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();
            }
        }
        private DateTime? _CurrentDate;
        public DateTime? CurrentDate
        {
            get { return _CurrentDate; }
            set
            {
                _CurrentDate = value;
                this.OnPropertyChanged();
            }
        }
        private int _SettlTypeIndex = -1;

        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int SettlTypeIndex
        {
            get
            {
                return _SettlTypeIndex;
            }
            set
            {
                int oldIndex = _SettlTypeIndex;
                _SettlTypeIndex = value;


                this.AddOneOpenedTag = "0";

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }
                CalBond(GetSetttle(), true);
                this.OnPropertyChanged();

                SetOtherVisibile();
                if (!oldIndex.Equals(value))
                {

                }
            }
        }
        private readonly string ADD_ONE_NOT_LIMIT = "2";
        private const string YYYYMMDD = "yyyyMMdd";
        private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _TreeComboxItemsSource; }
            set
            {
                if (_TreeComboxItemsSource != value)
                {
                    _TreeComboxItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //private TBookBookTreeItemModel _SelectedItem = null;
        //public TBookBookTreeItemModel SelectedItem
        //{
        //    get { return _SelectedItem; }
        //    set
        //    {
        //        _SelectedItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        private string _SelectedItem = null;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }


        private string _SelectedText = null;
        public string SelectedText
        {
            get { return _SelectedText; }
            set
            {
                _SelectedText = value;
                this.OnPropertyChanged();
            }
        }
        private int _OpenCloseIndex;
        /// <summary>
        /// 开平标志 选项 
        /// </summary>
        public int OpenCloseIndex
        {
            get
            {
                return _OpenCloseIndex;
            }
            set
            {
                if (value == -1)
                {
                    _OpenCloseIndex = 0;
                }
                else 
                {
                    _OpenCloseIndex = value;
                }
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 开仓
        /// </summary>
        private bool _openSelected = false;
        public bool OpenSelected
        {
            get
            {
                return _openSelected;
            }
            set
            {
                _openSelected = value;
                this.OnPropertyChanged(nameof(OpenSelected));
            }
        }

        /// <summary>
        /// 平仓
        /// </summary>
        private bool _closeSelected = false;
        public bool CloseSelected
        {
            get
            {
                return _closeSelected;
            }
            set
            {
                _closeSelected = value;
                this.OnPropertyChanged(nameof(CloseSelected));
            }
        }
        public ManualEntryViewModel()
        {
            FwdSelectDate = DateTime.Now;
            NetPrice = string.Empty;
            CustomPrice = string.Empty;
            SettleBoxIndex = -1;
            SettleBoxIndex2 = 0;
            SettleType = "2,3";
            CurrentDate = DateTime.Today;
        }
        #region 返回根据节点Id找到选择的第一个节点
        /// <summary>
        /// 解析手工单默认组合
        /// </summary>
        /// <param name="info2"></param>
        public void ParseManualOrderDefaultBookId(string info2)
        {
            if (info2 == null || MultiTreeComboxItemsSource == null)
                return;

            //设置树数据源
            this.MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();

            TreeNodeModel dr = null;
            foreach (TreeNodeModel pbd in MultiTreeComboxItemsSource)
            {
                dr = FindNode(pbd, info2);
                if (dr != null)
                    break;
            }

            if (dr != null)
            {
                dr.Checked = "1";
                SelectedItem = dr.Id;
            }
        }

        private TreeNodeModel FindNode(TreeNodeModel tnParent, string id)
        {
            if (tnParent == null) return null;
            if (tnParent.Id == id)
            {
                return tnParent;
            }

            TreeNodeModel tnRet = null;
            foreach (TreeNodeModel tn in tnParent.Nodes)
            {
                tnRet = FindNode(tn, id);
                if (tnRet != null) break;
            }

            return tnRet;
        }
        #endregion
        private void PTInnerSettingViewModel_PTCopyInfoChange()
        {
           
        }

        private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        public ObservableCollection<AccountUserItem> AccountUserSource
        {
            get
            {
                return _accountUserSource;
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

        // 债券名称
        private string symbol = string.Empty;
        public string Symbol
        {
            get
            {
                return symbol;
            }

            set
            {
                if (value != symbol)
                {
                    symbol = value;
                    this.OnPropertyChanged(nameof(Symbol));
                }
            }
        }

        private string side = string.Empty;
        public string Side
        {
            get
            {
                return side;
            }

            set
            {
                if (value != side)
                {
                    side = value;
                    this.OnPropertyChanged(nameof(Side));
                    setTxt();
                }
            }
        }

        private string seatNumber = string.Empty;
        public string SeatNumber
        {
            get
            {
                return seatNumber;
            }
            set
            {
                if (value != seatNumber)
                {
                    seatNumber = value;
                    this.OnPropertyChanged(nameof(SeatNumber));
                    setTxt();
                }
            }
        }

        private string comment = string.Empty;
        public string Comment
        {
            get
            {
                return comment;
            }
            set
            {
                if (value != comment)
                {
                    comment = value;
                    this.OnPropertyChanged(nameof(Comment));
                    setTxt();
                }
            }
        }

        private string quoteWay = string.Empty;
        public string QuoteWay
        {
            get
            {
                return quoteWay;
            }
            set
            {
                if (value != quoteWay)
                {
                    quoteWay = value;
                    this.OnPropertyChanged(nameof(QuoteWay));
                    setTxt();
                }
            }
        }

        private string book = string.Empty;
        public string Book
        {
            get
            {
                return book;
            }
            set
            {
                if (value != book)
                {
                    book = value;
                    this.OnPropertyChanged(nameof(Book));
                }
            }
        }


        public string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value != null)
                {
                    searchText = value;
                    this.OnPropertyChanged();
                    if (string.IsNullOrEmpty(searchText))
                    {
                        Symbol = "";
                        return;
                    }

                    string orgamount = GetTrueAmount().ToString();
                    InitPriceView(searchText);
                    setIsBrokerSecurity(searchText);
                    if (!"0".Equals(orgamount))
                        Amount = !IsIRS ? !IsCTP?(decimal.Parse(orgamount) / 1000).ToString(): orgamount : (decimal.Parse(orgamount) / 100).ToString();
                    setTxt();
                }
            }
        }

        public void InitPriceView(string searchText = "")
        {
            string securityID = string.Empty;
            var x = CommonUtils.GetBondsInfoPoBySecurityID(searchText);
            if (x != null)
            {
                Symbol = (x as BondsInfoPo).Short_Name;
                IsIRS = (x as BondsInfoPo).MarketIndicator == "2";
                IsCTP = (x as BondsInfoPo).MarketIndicator == "5";
            }
        }

        /// <summary>
        /// 获取下单用的值 万
        /// </summary>
        /// <returns></returns>
        public decimal GetTrueAmount()
        {
            decimal d = 0;
            decimal.TryParse(amount, out d);

            decimal res = 0;
            if (IsIRS)
            {
                //IRS
                res = d * 100;
            }
            else if (IsCTP)
            {
                //IRS
                res = d;
            }
            else
            {
                //现券
                res = d * 1000;
            }

            return res;
        }

        private string sourceamount;
        public string SourceAmount
        {
            get { return sourceamount; }
            set
            {
                if (value != null)
                {
                    sourceamount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string amount;
        public string Amount
        {
            get { return amount; }
            set
            {
                if (value != null)
                {
                    amount = value;

                    if (string.IsNullOrEmpty(SourceAmount))
                    {
                        if (IsIRS)
                        {
                            if (!string.IsNullOrEmpty(value))
                                SourceAmount = (decimal.Parse(value) * 100).ToString();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(value))
                                SourceAmount = (decimal.Parse(value) * 1000).ToString();
                        }
                    }

                    this.OnPropertyChanged();
                    setTxt();
                }
            }
        }

        private string qtyTxt = "kw";

        public string QtyTxt
        {
            get { return qtyTxt; }
            set
            {
                qtyTxt = value;
                this.OnPropertyChanged();
                setTxt();
            }
        }

        private string copyTxt = "";

        public string CopyTxt
        {
            get { return copyTxt; }
            set
            {
                copyTxt = value;
                this.OnPropertyChanged();
            }
        }


        private bool isIRS = false;
        /// <summary>
        /// 是否是IRS
        /// </summary>
        public bool IsIRS
        {
            get { return isIRS; }
            set { isIRS = value; }
        }
        private bool isCTP = false;
        /// <summary>
        /// 是否是国债期货
        /// </summary>
        public bool IsCTP
        {
            get { return isCTP; }
            set 
            { 
                isCTP = value;
                this.OnPropertyChanged();
            }
        }
        private bool isBank = false;
        /// <summary>
        /// 是否是IsBank
        /// </summary>
        public bool IsBank
        {
            get { return isBank; }
            set { isBank = value; }
        }

        private string Txt_KW = "kw";
        private string Txt_W = "w";
        private string Txt_IRS = "百万";
        private string Txt_CTP = "手";

        public void setIsBrokerSecurity(string securityID, bool isBillon = false)
        {
            IsBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(securityID);

            if (!IsBank)
            {
                if (isBillon)
                {
                    QtyTxt = Txt_IRS;
                }
                else
                {
                    QtyTxt = Txt_KW;
                }


                var x = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                if (x != null)
                {
                    if ((x as BondsInfoPo).MarketIndicator == "2")
                    {
                        QtyTxt = Txt_IRS;
                        IsIRS = true;
                        IsCTP = false;
                    }
                    else if ((x as BondsInfoPo).MarketIndicator == "5")
                    {
                        QtyTxt = Txt_CTP;
                        IsIRS = false;
                        IsCTP = true;
                        OpenCloseIndex = 0;
                        OpenSelected = true;
                    }
                    else
                    {
                        IsIRS = false; 
                        IsCTP = false;
                    }
                }
            }
            else
            {
                QtyTxt = Txt_W;
            }
        }

        public static List<KeyValuePair<string, string>> QuotemethodItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("quotemethod_pt");
            }
        }

        private string customSettleDate;
        public string CustomSettleDate
        {
            get { return customSettleDate; }
            set
            {
                if (value != null)
                {
                    customSettleDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string settleType;

        public string SettleType
        {
            get
            {
                return settleType;
            }
            set
            {
                settleType = value;
                this.OnPropertyChanged();
            }
        }

        private string settle;

        public string Settle
        {
            get
            {
                return settle;
            }
            set
            {
                settle = value;
                this.OnPropertyChanged();
                setTxt();
            }
        }

        private Visibility dateVisible = Visibility.Collapsed;
        public Visibility DateVisible
        {
            get { return dateVisible; }
            set
            {
                dateVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility settleTypeVisible = Visibility.Visible;
        public Visibility SettleTypeVisible
        {
            get { return dateVisible; }
            set
            {
                settleTypeVisible = value;
                this.OnPropertyChanged();
            }
        }


        #region 机构交易员
        public string defaultAccountID = string.Empty;
        public void InitAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

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
                    setTxt();
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
                    if (isSysAuto_Institution == false
                    && _institutionName_Edit != value)
                    {
                        isSysAuto_Trader = true;
                        SelectedTrader_Edit = null;
                        TraderName_Edit = string.Empty;
                    }
                    isSysAuto_Institution = false;

                    _institutionName_Edit = value;
                    this.OnPropertyChanged();
                    setTxt();
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
                    //    SelectedInstintuion_Edit = null;6
                    //    InstitutionName_Edit = string.Empty;
                    //}
                    isSysAuto_Trader = false;

                    _traderName_Edit = value;
                    this.OnPropertyChanged();
                    setTxt();
                }
            }
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


        private CrmInst selectCounterClientTypeItem;

        public CrmInst SelectCounterClientTypeItem
        {
            get { return selectCounterClientTypeItem; }
            set
            {
                if (selectCounterClientTypeItem != value)
                {
                    selectCounterClientTypeItem = value;
                    if (selectCounterClientTypeItem != null)
                        CounterClientTypeEnum = selectCounterClientTypeItem.shortnamecn;
                    this.OnPropertyChanged();
                }
            }
        }
        private string CounterclientTypeEnum;
        public string CounterClientTypeEnum
        {
            get { return CounterclientTypeEnum; }
            set { CounterclientTypeEnum = value; this.OnPropertyChanged(); }
        }

        private CrmInst _selectedCounterInstitution_Edit;

        public CrmInst SelectedCounterInstintuion_Edit
        {
            get { return _selectedCounterInstitution_Edit; }
            set
            {
                if (_selectedCounterInstitution_Edit != value)
                {
                    _selectedCounterInstitution_Edit = value;

                    if (value != null)
                    {
                        List<CrmInst> result = GetCrmInst();
                        if (this._selectedCounterInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedCounterInstitution_Edit.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._selectedCounterInstitution_Edit.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                this.SelectCounterClientTypeItem = result.First();
                            }

                        }
                    }


                    this.OnPropertyChanged();
                    setTxt();
                }
            }
        }

        private string _CounterinstitutionName_Edit;

        public string CounterInstitutionName_Edit
        {
            get { return _CounterinstitutionName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_CounterinstitutionName_Edit) && string.IsNullOrEmpty(value)) && _CounterinstitutionName_Edit != value)
                {
                    if (isSysAuto_Institution == false
                    && _CounterinstitutionName_Edit != value)
                    {
                        isSysAuto_Trader = true;
                        SelectedCounterTrader_Edit = null;
                        CounterTraderName_Edit = string.Empty;
                    }
                    isSysAuto_Institution = false;

                    _CounterinstitutionName_Edit = value;
                    this.OnPropertyChanged();
                    setTxt();
                }
            }
        }
        private string _CounterTrdNameEnable = "0";
        public string CounterTrdNameEnable
        {
            get { return _CounterTrdNameEnable; }
            set { _CounterTrdNameEnable = value;
                this.OnPropertyChanged();
            }
        }
        private CrmContact _selectedCounterTrader_Edit;

        public CrmContact SelectedCounterTrader_Edit
        {
            get { return _selectedCounterTrader_Edit; }
            set
            {

                if (_selectedCounterTrader_Edit != value)
                {
                    // 只有crmContact中才有机构信息，可以联动显示
                    if (value != null)
                    {
                        if (SelectedCounterInstintuion_Edit == null || SelectedCounterInstintuion_Edit.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = WtUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                isSysAuto_Institution = true;
                                isSysAuto_Trader = true;
                                SelectedCounterInstintuion_Edit = results[0];
                                CounterInstitutionName_Edit = results[0].shortnamecn;
                            }
                        }

                        if (SelectedCounterInstintuion_Edit != null && SelectedCounterInstintuion_Edit.idbkey == value.instid)
                        {
                            isSysAuto_Institution = false;
                            isSysAuto_Trader = true;
                            SelectedCounterInstintuion_Edit = SelectedCounterInstintuion_Edit;
                            CounterInstitutionName_Edit = CounterInstitutionName_Edit;
                        }
                    }

                    _selectedCounterTrader_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _CountertraderName_Edit;

        public string CounterTraderName_Edit
        {
            get { return _CountertraderName_Edit; }
            set
            {
                if (!(string.IsNullOrEmpty(_CountertraderName_Edit) && string.IsNullOrEmpty(value)) && _CountertraderName_Edit != value)
                {
                    //if (isSysAuto_Trader == false
                    //    && _traderName_Edit != value)
                    //{
                    //    isSysAuto_Institution = true;
                    //    SelectedInstintuion_Edit = null;
                    //    InstitutionName_Edit = string.Empty;
                    //}
                    isSysAuto_Trader = false;

                    _CountertraderName_Edit = value;
                    this.OnPropertyChanged();
                    setTxt();
                }
            }
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {
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
        internal void FilterCounterInstitution(AutoCompleteArgs args)
        {
            string currentInstId = this.SelectedCounterTrader_Edit == null ? string.Empty : this.SelectedCounterTrader_Edit.instid;

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

        internal void FilterCounterTraderLocal(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedCounterInstitution_Edit == null ? string.Empty : this._selectedCounterInstitution_Edit.idbkey;

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
        internal void FilterCounterTrader(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedCounterInstitution_Edit == null ? null : this._selectedCounterInstitution_Edit.idbkey;

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
        private StcTbookUsers _SelectedCounterUser_Edit;

        public StcTbookUsers SelectedCounterUser_Edit
        {
            get { return _SelectedCounterUser_Edit; }
            set
            {
                if (_SelectedCounterUser_Edit != value)
                {
                    _SelectedCounterUser_Edit = value;

                    this.OnPropertyChanged();
                }
            }
        }
        private string _CounterUserName_Edit;

        public string CounterUserName_Edit
        {
            get { return _CounterUserName_Edit; }
            set
            {
                _CounterUserName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        internal void FilterCounterUser(AutoCompleteArgs args)
        {
            List<StcTbookUsers> result = PTExecOrderProcesser.Instance.UserList;
            if (!string.IsNullOrEmpty(args.Pattern)) 
            {
               result = result.Where(item => item.Username.Contains(args.Pattern.Trim()) || item.Name.Contains(args.Pattern.Trim())).ToList();
            }
            args.DataSource = result;
        }
        #endregion

        public bool isSetValue = false;

        private bool isSendReq = false;

        private string customPrice = string.Empty;
        public string CustomPrice
        {
            get
            {
                return customPrice;
            }

            set
            {
                string oldPrice = customPrice;
                customPrice = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                if (string.IsNullOrEmpty(customPrice))
                {
                    NetPrice = string.Empty;
                }
                setTxt();
                if (isSetValue)
                    return;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    netPrice = string.Empty;
                    this.OnPropertyChanged("NetPrice");
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();
               
            }
        }

        private string netPrice;
        public string NetPrice
        {
            get { return netPrice; }
            set
            {
                string oldPrice = netPrice;
                netPrice = value;
                if (isSetValue)
                    return;
                decimal d = 0;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                {
                    CalBond("2", GetSetttle(), netPrice);
                }

                if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                {
                    //CustomPrice = string.Empty;
                    customPrice = string.Empty;
                }

                this.OnPropertyChanged();
                //setTxt();
            }
        }

        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private bool _needCalPrice = true;
        private object _calLock = new object();
        private CalResult _lastCalResult = null;




        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val)
        {
            calBondErrMsg = string.Empty;
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) || !_needCalPrice)
                return;

            //isSendReq = true;

            _lastReqData = string.Format("{0}-{1}-{2}", calType, settleType, val);
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = SearchText; //sendPriceItem.Security;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = val;// customPrice;
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                        requst.CalPath = LocalEnv.ConfInfo.CalType;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null)
                    {
                        if (result != null && result.Data != null && result.IsSuccess())
                        {
                            result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);

                            if (_lastReqData.Equals(result.ReqData))
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    //if (result.IsSuccess() && sendPriceItem.CalWarnVisibility == Visibility.Visible)
                                    //    sendPriceItem.CalWarnVisibility = Visibility.Collapsed;

                                    lock (_calLock)
                                    {
                                        _lastCalResult = result;

                                    }

                                    if (calType.Equals("4"))
                                    {
                                        //NetPrice = result.Data.CleanPrice.ToString("F4");

                                        netPrice = result.Data.CleanPrice.ToString("F4");
                                        this.OnPropertyChanged("NetPrice");
                                    }
                                    else if (calType.Equals("2"))//净价
                                    {
                                        if (result.IsSuccess())
                                        {
                                            customPrice = (result.Data.Yield * 100).ToString("F4");

                                            this.OnPropertyChanged("CustomPrice");
                                            setTxt();
                                        }
                                    }

                                    isSendReq = false;
                                }));
                            }
                        }
                        else
                        {
                            lock (_calLock)
                            {
                                _lastCalResult = result;
                            }
                        }
                    }
                    else
                    {
                        isSendReq = false;
                        calBondErrMsg = result.Msg;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
                finally
                {
                    //isSendReq = false;
                    
                }
            });
        }


        private void CalBond(string settleType, bool isTomorrow)
        {
            if (isSendReq)
                return;
            //DateTime FwdSelectDate = DateTime.Now;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (result != null)
                    {
                        if (result.Data != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                try
                                {
                                    string year = result.Data.settlementDate.Substring(0, 4);
                                    string month = result.Data.settlementDate.Substring(4, 2);
                                    string day = result.Data.settlementDate.Substring(6, 2);

                                    FwdSelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                    CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                                    settlementDate = result.Data.settlementDate;
                                }
                                catch (Exception e) { }

                                // FwdComboBoxSelectIndex = 0;
                                SettleType = "T+0";

                                isSendReq = false;
                            }));
                        }
                    }
                    else
                        isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
            });
        }

        private string addOneShowText = "+1";
        public string AddOneShowText
        {
            get { return addOneShowText; }
            set
            {
                addOneShowText = value;
                OnPropertyChanged(nameof(AddOneShowText));
            }
        }
        /// <summary>
        /// 用于改变当点击 +1时，下拉菜单弹出，需要改变 +1 的上边框颜色，同时用于判断是否选中的 +1 ,用这个值改变默认的背景色
        /// </summary>
        private string addOneOpenedTag = "1";
        public string AddOneOpenedTag
        {
            get { return addOneOpenedTag; }
            set
            {
                addOneOpenedTag = value;
                OnPropertyChanged(nameof(AddOneOpenedTag));
            }
        }

        private bool _addOneTodayCheck = false;
        public bool AddOneTodayCheck
        {
            get
            {
                return _addOneTodayCheck;
            }

            set
            {
                bool oldVal = _addOneTodayCheck;
                _addOneTodayCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    //if (!oldVal.Equals(value))
                    //    UpdateBestPrice();
                }
            }
        }

        private bool _addOneTomorrowCheck = false;
        public bool AddOneTomorrowCheck
        {
            get
            {
                return _addOneTomorrowCheck;
            }

            set
            {
                bool oldVal = _addOneTomorrowCheck;
                _addOneTomorrowCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTodayCheck = false;
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTodayCheck");

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    //if (!oldVal.Equals(value))
                    //    UpdateBestPrice();
                }
            }
        }

        private bool _addOneNotLimitCheck = true;
        public bool AddOneNotLimitCheck
        {
            get
            {
                return _addOneNotLimitCheck;
            }

            set
            {
                bool oldVal = _addOneNotLimitCheck;
                _addOneNotLimitCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneTodayCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    //if (!oldVal.Equals(value))
                    //    UpdateBestPrice();
                }
            }
        }

        private DateTime? fwdSelectDate;
        public DateTime? FwdSelectDate
        {
            get { return fwdSelectDate; }
            set
            {
                string oldDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);
                fwdSelectDate = value;
                string newDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);
                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();

                if (value != null && !oldDate.Equals(newDate))
                {
                }
            }
        }

        private string _settlementDate;
        public string settlementDate {
            get
            {
                return _settlementDate;
            }

            set
            {
                _settlementDate = value;
                this.OnPropertyChanged();
            }
        }

        private int settleBoxIndex = -1;

        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int SettleBoxIndex
        {
            get
            {
                return settleBoxIndex;
            }

            set
            {
                int oldIndex = settleBoxIndex;
                settleBoxIndex = value;


                this.AddOneOpenedTag = "0";

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();

                SetOtherVisibile();
                if (!oldIndex.Equals(value))
                {
                    //UpdateBestPrice();
                    //if (oldIndex == 1 && value != 1)
                    //{
                    //    ChangeUI();
                    //}
                }

            }
        }

        private int fwdComboBoxSelectIndex = 0;

        public int FwdComboBoxSelectIndex
        {
            get
            {
                return fwdComboBoxSelectIndex;
            }

            set
            {
                int oldIndex = fwdComboBoxSelectIndex;
                fwdComboBoxSelectIndex = value;

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    //if (_priceMatchParam == null)
                    //{
                    //    CalBond("4", GetSetttle(), NumChange(customPrice));
                    //}
                }

                GetSettleTypeFromServer(GetSetttle());

                //if (!oldIndex.Equals(value))
                //{
                //    ChangeUI();

                //    UpdateBestPrice();
                //}

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 清算时间切换至其他
        /// </summary>
        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0 || SettleBoxIndex == -1)
            {
                DateVisible = Visibility.Collapsed;
                SettleTypeVisible = Visibility.Collapsed;
                // SelectDate = null;
                // ComboBoxSelectIndex = 0;

                //if (getCrossViewModel().SelectTabIndex == 0 && !WholeFlagChecked)
                //{
                //    if (PermisionHelper.Instance.IsHaveAllPermission || PermisionHelper.Instance.IsHaveInnerXbondPermission)
                //        getCrossViewModel().SetVenueItemEnable(true, PermisionHelper.Instance.XBOND);
                //}
            }
            else if (SettleBoxIndex == 1)
            {
                FwdComboBoxSelectIndex = 0;
                DateVisible = Visibility.Visible;
                SettleTypeVisible = Visibility.Visible;

                ////if (getCrossViewModel().SelectTabIndex == 0)
                //{
                //    getCrossViewModel().SetVenueItemEnable(false, PermisionHelper.Instance.XBOND);
                //}


                //if (_priceMatchParam == null)
                //{
                //    this.CalBond("4", true);
                //}
                //else
                //{
                string settle = SettleType.ToString();
                if (settle.Equals("") || settle.Equals("1") || settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3,2") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                {
                    this.CalBond("4", true);
                }
                else
                {
                    //恢复 Settle
                    string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 1)
                    {
                        if (arr[1].Equals("0"))
                        {
                            fwdComboBoxSelectIndex = 0;
                        }
                        else if (arr[1].Equals("1"))
                        {
                            fwdComboBoxSelectIndex = 1;
                        }
                        this.OnPropertyChanged("FwdComboBoxSelectIndex");
                    }
                }
            }
            //}
            //catch (Exception e) { }
            // }
        }

        private int settleBoxIndex2 = 0;
        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int SettleBoxIndex2
        {
            get
            {
                return settleBoxIndex2;
            }
            set
            {
                int oldIndex = settleBoxIndex;
                settleBoxIndex2 = value;
                if (settleBoxIndex2 >= 0)
                {
                    AddOneOpenedTag = "1";
                }

                //if (SettleBoxIndex2 == 0)
                //{
                //    AddOneNotLimitCheck = true;
                //    AddOneTodayCheck = false;
                //    AddOneTomorrowCheck = false;

                //}
                //else 
                if (SettleBoxIndex2 == 0)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = true;
                    AddOneTomorrowCheck = false;

                }
                else if (SettleBoxIndex2 == 1)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = true;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 计算正确的清算速度
        /// </summary>
        /// <param name="settleType">明天+0 ，6.15+0 </param>
        private void GetSettleTypeFromServer(string settleType)
        {
            if (isSendReq)
                return; 

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (result != null && result.Data != null)
                    {
                        try
                        {
                            SettleType = result.Data.settlementType2;
                        }
                        catch (Exception e) { }

                        isSendReq = false;
                    }
                    else
                        isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
            });
        }

        /// <summary>
        /// 获得清算速度
        /// </summary>
        /// <returns></returns>
        private string GetSetttle(bool isfilter = false)
        {
            string settle = string.Empty;

            //默认+1选择
            if (SettlTypeIndex == -1)
                settle = AddOneNotLimitCheck ? ADD_ONE_NOT_LIMIT : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : ADD_ONE_NOT_LIMIT;
            else if (SettlTypeIndex == 0)
            {
                //选择+1
                settle = "2";
                if (TradeDate != null && TradeDate.HasValue)
                {
                    settle = TradeDate.Value.ToString("MM.dd") +  "+1";
                }
            }
            else if (SettlTypeIndex == 1)
            {
                settle = "1";
                if (TradeDate != null && TradeDate.HasValue)
                {
                    settle = TradeDate.Value.ToString("MM.dd") + "+0";
                }
            }
            if (string.IsNullOrEmpty(settle))
                settle = "2";//默认+1
            this.Settle = settle;
            return settle;
        }


        /// <summary>
        /// 解析清算速度
        /// </summary>
        /// <param name="settle"></param>
        public void ParseSettle(string settle)
        {
            SettleBoxIndex2 = 0;
            _needCalPrice = false;
            AddOneShowText = "+1";
            try
            {
                if (!string.IsNullOrEmpty(settle))
                {

                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                    {
                        SettleBoxIndex = -1;
                        AddOneNotLimitCheck = false;

                        //if (settle.Equals(ADD_ONE_NOT_LIMIT))
                        //{
                        //    SettleBoxIndex2 = 0;
                        //    AddOneNotLimitCheck = true;
                        //}
                        //else 
                        if (settle.Equals("3"))
                        {
                            SettleBoxIndex2 = 1;
                            AddOneTomorrowCheck = true;
                        }
                        else if (settle.Equals("2")|| settle.Equals("2,3"))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneTodayCheck = true;
                        }
                        //else if (settle.Equals("+1"))
                        //{
                        //    SettleBoxIndex2 = 0;
                        //    AddOneNotLimitCheck = true;
                        //}
                        else if (settle.Equals("全"))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
                    }
                    else
                    {
                        //_needCalPrice = false;
                        SettleBoxIndex = -1;
                        if (settle.Equals("1") || settle.Equals("T+0") || settle.Equals("+0"))//T+0
                        {
                            try
                            {
                                FwdSelectDate = DateTime.Now;
                                //SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                            }
                            catch (Exception e) { }
                            //_needCalPrice = true;
                            SettleBoxIndex = 0;
                            //ComboBoxSelectIndex = 0;
                            //SendPriceItem.SettleType = "T+0";
                        }
                        else if (settle.Equals("明天+1") || settle.Equals("4"))
                        {
                            //_needCalPrice = true;
                            //this.CalDate("4", false);
                            CalBond("4", false);
                        }
                        else if (settle.Equals("远"))
                        {
                            SettleBoxIndex = 1;
                        }
                        else
                        {
                            string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            int month = 0;
                            int day = 0;
                            if (arr != null && arr.Length > 1)
                            {
                                SettleBoxIndex = 1;
                                string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                if (dtStr != null && dtStr.Length > 1
                                            && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                                {
                                    DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                                    if (dt < DateTime.Now)
                                    {
                                        dt = new DateTime(DateTime.Now.Year, month, day).AddYears(1);
                                    }

                                    try
                                    {
                                        FwdSelectDate = new DateTime?(dt);
                                        //SendPriceItem.CustomSettleDate = dt.ToString("yyy-MM-dd");
                                    }
                                    catch (Exception e) { }
                                }
                                //_needCalPrice = true;
                                if (arr[1].Equals("0"))
                                {
                                    FwdComboBoxSelectIndex = 0;
                                    //SendPriceItem.SettleType = "T+0";
                                }
                                else if (arr[1].Equals("1"))
                                {
                                    FwdComboBoxSelectIndex = 1;
                                    //SendPriceItem.SettleType = "T+1";
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {

            }
            finally
            {
                _needCalPrice = true;
            }
        }

        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }
            return price;
        }

        public void setTxt()
        {
            string settletype = ComboSourceUtil.Instance.FindEnum(settle, "settletype_ptcopy");
            string securityid = SearchText;
            string symbol = Symbol;
            string price = CustomPrice;
            string netprice = NetPrice;
            string amount = Amount;
            string side = Side;
            string quoteWay1 = "";
            string quote = "";
            string instname = "";
            string tabStr = " ";

            if (!string.IsNullOrEmpty(settletype) &&
               !string.IsNullOrEmpty(securityid) &&
               !string.IsNullOrEmpty(price) &&
               !string.IsNullOrEmpty(netprice) &&
               !string.IsNullOrEmpty(amount))
            {
                string temp = ComboSourceUtil.Instance.FindEnum(QuoteWay + "", "quotemethod_pt");
                if (temp.Contains("待定"))
                {
                    quote = "报价方式待定";
                }
                else if (temp.Contains("收"))
                {
                    instname = PTInnerSettingViewModel.Instance.PartyName;
                    quote = string.IsNullOrEmpty(temp) ? "" : temp.Replace("收", "");
                    quoteWay1 = "发";
                }
                else if (temp.Contains("发"))
                {
                    instname = InstitutionName_Edit;
                    quote = string.IsNullOrEmpty(temp) ? "" : temp.Replace("发", "");
                    quoteWay1 = "发";
                }
                string PartyName = PTInnerSettingViewModel.Instance.PartyName;
                if (side == "1")
                {
                    if (IsBank)//交易所
                    {
                        string xw = string.IsNullOrEmpty(SeatNumber) ? "席位号待定" : SeatNumber;
                        string yd = string.IsNullOrEmpty(comment) ? "约定号待定" : "约定号" + comment;
                        string xwNum = securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                        CopyTxt = $"{settletype}{tabStr}{securityid}{tabStr}{symbol}{tabStr}{price}%{tabStr}{amount}{QtyTxt}{tabStr}{PartyName}{xwNum}{tabStr}to{tabStr}{InstitutionName_Edit}{xw}{tabStr}{yd}";
                    }
                    else
                    {
                        CopyTxt = $"{settletype}{tabStr}{securityid}{tabStr}{symbol}{tabStr}{price}%{tabStr}{amount}{QtyTxt}{tabStr}{PartyName}-{LocalEnv.UserInfo.FullName}{tabStr}to{tabStr}{InstitutionName_Edit}-{TraderName_Edit}{tabStr}{quoteWay1}{instname}{quote}";
                    }
                }
                else
                {
                    if (IsBank)//交易所
                    {
                        string xw = string.IsNullOrEmpty(SeatNumber) ? "席位号待定" : SeatNumber;
                        string yd = string.IsNullOrEmpty(comment) ? "约定号待定" : "约定号" + comment;
                        string xwNum = securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                        CopyTxt = $"{settletype}{tabStr}{securityid}{tabStr}{symbol}{tabStr}{price}%{tabStr}{amount}{QtyTxt}{tabStr}{InstitutionName_Edit}{xw}{tabStr}to{tabStr}{PartyName}{xwNum}{tabStr}{yd}";
                    }
                    else
                    {
                        CopyTxt = $"{settletype}{tabStr}{securityid}{tabStr}{symbol}{tabStr}{price}%{tabStr}{amount}{QtyTxt}{tabStr}{InstitutionName_Edit}-{TraderName_Edit}{tabStr}to{tabStr}{PartyName}-{LocalEnv.UserInfo.FullName}{tabStr}{quoteWay1}{instname}{quote}";
                    }
                }
            }
        }
    }
}
