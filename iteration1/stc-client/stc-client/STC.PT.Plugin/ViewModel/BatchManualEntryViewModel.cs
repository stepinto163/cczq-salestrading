using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static STC.PT.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.PT.Plugin.Utils.CommonUtils;

namespace STC.PT.Plugin.ViewModel
{
    public class BatchManualEntryViewModel : ViewModelBase
    {
        private const string YYYYMMDD = "yyyyMMdd";
        public BatchManualEntry manualEntry;
        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set 
            { 
                _SearchText = value;
                this.OnPropertyChanged();
                if (string.IsNullOrEmpty(_SearchText))
                {
                    return;
                }
                setIsBrokerSecurity(_SearchText);
            }
        }
        public void setIsBrokerSecurity(string securityID, bool isBillon = false)
        {
            IsBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(securityID);
            if (!IsBank)
            {
                var x = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                if (x != null)
                {
                    if ((x as BondsInfoPo).MarketIndicator == "2")
                    {
                        IsIRS = true;
                    }
                    else
                    {
                        IsIRS = false;
                    }
                }
            }
        }

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

        private string _CustomPrice;
        public string CustomPrice
        {
            get { return _CustomPrice; }
            set
            {
                _CustomPrice = value;
                this.OnPropertyChanged();
                if (!string.IsNullOrEmpty(_CustomPrice)) 
                {
                    NetPrice = "";
                }
            }
        }

        private string _NetPrice;
        public string NetPrice
        {
            get { return _NetPrice; }
            set
            {
                _NetPrice = value;
                this.OnPropertyChanged();
                if (!string.IsNullOrEmpty(_NetPrice))
                {
                    CustomPrice = "";
                }
            }
        }


        private string _Amount;
        public string Amount
        {
            get { return _Amount; }
            set
            {
                _Amount = value;
                this.OnPropertyChanged();
            }
        }
        private DateTime? _TradeDate;
        public DateTime? TradeDate
        {
            get { return _TradeDate; }
            set
            {
                _TradeDate = value;
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
                _SettlTypeIndex = value;
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
        private bool isBank = false;
        /// <summary>
        /// 是否是IsBank
        /// </summary>
        public bool IsBank
        {
            get { return isBank; }
            set { isBank = value; }
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
                    //setTxt();
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
                    //setTxt();
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
                    isSysAuto_Trader = false;

                    _traderName_Edit = value;
                    this.OnPropertyChanged();
                    //setTxt();
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
                        SelectedTrader_Edit = null;
                        CounterTraderName_Edit = string.Empty;
                    }
                    isSysAuto_Institution = false;

                    _CounterinstitutionName_Edit = value;
                    this.OnPropertyChanged();
                }
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
                    isSysAuto_Trader = false;

                    _CountertraderName_Edit = value;
                    this.OnPropertyChanged();
                }
            }
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
                }
            }
        }
        private string _BusinessType = string.Empty;
        public string BusinessType
        {
            get
            {
                return _BusinessType;
            }
            set
            {
                if (value != _BusinessType)
                {
                    _BusinessType = value;
                    this.OnPropertyChanged(nameof(BusinessType));
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
        private IList _SelectedItemsList;
        public IList SelectedItemsList 
        {
            get { return _SelectedItemsList; }
            set 
            { 
                _SelectedItemsList = value;
                this.OnPropertyChanged();
            }
        }
        private ObservableCollection<TBookBookTreeItemModel> _TreeComboxItemsSource = new ObservableCollection<TBookBookTreeItemModel>();
        public ObservableCollection<TBookBookTreeItemModel> TreeComboxItemsSource
        {
            get { return _TreeComboxItemsSource; }
            set
            {
                if (_TreeComboxItemsSource != value)
                {
                    _TreeComboxItemsSource = value;
                    this.OnPropertyChanged("TreeComboxItemsSource");
                }
            }
        }
        private TBookBookTreeItemModel _SelectedItem = null;
        public TBookBookTreeItemModel SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }
        public ICommand ConfirmCommand { get { return new Utils.RelayCommand(ConfirmClick); } }

        public BatchManualEntryViewModel() 
        {
            CurrentDate = DateTime.Today;
            this.TreeComboxItemsSource = PTExecOrderProcesser.Instance.BookList;
        }
        #region 返回根据节点Id找到选择的第一个节点
        /// <summary>
        /// 解析手工单默认组合
        /// </summary>
        /// <param name="info2"></param>
        public void ParseManualOrderDefaultBookId(string info2)
        {
            if (info2 == null)
                return;

            if (this.TreeComboxItemsSource.Count == 0)
                //设置树数据源
                this.TreeComboxItemsSource = STC.Common.Handler.Admin.AdminHandler.Instance.GetTCurrentBookCollection();

            TBookBookTreeItemModel dr = null;
            foreach (TBookBookTreeItemModel pbd in TreeComboxItemsSource)
            {
                dr = FindNode(pbd, info2);
                if (dr != null)
                    break;
            }

            if (dr != null)
            {
                dr.Checked = "1";
                SelectedItem = dr;
            }
        }

        private TBookBookTreeItemModel FindNode(TBookBookTreeItemModel tnParent, string id)
        {
            if (tnParent == null) return null;
            if (tnParent.Id == id)
            {
                return tnParent;
            }

            TBookBookTreeItemModel tnRet = null;
            foreach (TBookBookTreeItemModel tn in tnParent.Nodes)
            {
                tnRet = FindNode(tn, id);
                if (tnRet != null) break;
            }

            return tnRet;
        }
        #endregion
        private void ConfirmClick()
        {
            BatchUpdateDetailsView view = new BatchUpdateDetailsView();
            PTUpdatePo po = new PTUpdatePo();
            po.SecurityID = SearchText;
            string side = SideIndex == 0 ? "1" : (SideIndex == 1 ? "2" : "");
            po.Side = side;
            po.LastPx = CustomPrice;
            po.NetPrice = NetPrice;
            po.LastQty = Amount;
            po.TradeDate = TradeDate.HasValue?TradeDate.Value.Date.ToString(YYYYMMDD):"";
            po.SettlType = SettlTypeIndex == 0 ? "+1" : (SettlTypeIndex == 1?"+0":"");
            po.InstitutionName = InstitutionName_Edit;
            po.ClientInstID = SelectedInstintuion_Edit!= null? SelectedInstintuion_Edit.idbkey:"";
            po.TraderName = TraderName_Edit;
            po.ClientID = SelectedTrader_Edit!= null? SelectedTrader_Edit.idbkey:"";
            po.Seatnumber = SeatNumber;
            po.Quotemethod = QuoteWay;
            po.Remark = Comment;
            po.BusinessType = BusinessType;
            po.Bookid = SelectedItem != null ? SelectedItem.Id:"";
            po.CounterPartyName = CounterInstitutionName_Edit;
            po.CounterPartyid = SelectedCounterInstintuion_Edit != null ? SelectedCounterInstintuion_Edit.idbkey : "";
            po.CounterPartySubName = CounterTraderName_Edit;
            po.CounterPartySubid = SelectedCounterTrader_Edit != null ? SelectedCounterTrader_Edit.idbkey : "";
            view.setData(SelectedItemsList,po,manualEntry);
            view.ShowDialog();
        }
    }
}
