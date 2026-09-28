using Ats.Foundation.Condition;
using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.PT;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.MyTrading;
using STC.Common.View.UI;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.View.ExecManage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static STC.PT.Plugin.View.Component.AutoCompleteComboBox;
using CommonUtils = STC.Common.Utils.CommonUtils;
using IntentionQuoteInfo = STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo;
using RecognizeIntentionQuoteReplyPo = STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo;

namespace STC.PT.Plugin.ViewModel.ExecManage
{
    public class ExecManualEntryViewModel : ViewModelBase
    {
        #region 字段定义

        public Window myWindow;
        private string _VenueDisplay = "自谈";

        public string VenueDisplay
        {
            get { return _VenueDisplay; }
            set
            {
                _VenueDisplay = value;
                this.OnPropertyChanged();
            }
        }
        private string priceType = string.Empty;

        public string PriceType
        {
            get { return priceType; }
            set
            {
                priceType = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 0-新增 1-修改
        /// </summary>
        private string _IsAdd ="0";
        public string IsAdd
        {
            get { return _IsAdd; }
            set
            {
                _IsAdd = value;
                if ("0".Equals(_IsAdd)) 
                {
                    this.PartySubName = LocalEnv.UserInfo.FullName;
                }
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 0-手工 1-接口
        /// </summary>
        private string _IsManual = "0";
        public string IsManual
        {
            get { return _IsManual; }
            set
            {
                _IsManual = value;
                this.OnPropertyChanged();
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
                    /*if (!"0".Equals(orgamount))
                        Qty = !IsIRS ? !IsCTP ? (decimal.Parse(orgamount) / 1000).ToString() : orgamount : (decimal.Parse(orgamount) / 100).ToString();
*/                  setDefult();
                    RefeshInstMsg();
                }
            }
        }
        private int _SideIndex = 1;
        public int SideIndex
        {
            get { return _SideIndex; }
            set
            {
                int old = _SideIndex;
                if (value != -1)
                {
                    _SideIndex = value;
                    this.OnPropertyChanged();
                }
                else 
                {
                    _SideIndex = old;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 0-到期，1-行权
        /// </summary>
        private string _YieldTypeFlag = string.Empty;

        public string YieldTypeFlag
        {
            get { return _YieldTypeFlag; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _YieldTypeFlag = value;
                    this.OnPropertyChanged();
                }
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

        /// <summary>
        /// 到期收益率
        /// </summary>
        private string _Price = string.Empty;
        public string Price
        {
            get
            {
                return _Price;
            }

            set
            {
                string oldPrice = _Price;
                _Price = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                if (string.IsNullOrEmpty(_Price))
                {
                    NetPrice = string.Empty;
                    FullPrice = string.Empty;
                    //ExePrice = string.Empty;
                }
                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    _NetPrice = string.Empty;
                    this.OnPropertyChanged("NetPrice");
                    _FullPrice = string.Empty;
                    this.OnPropertyChanged("FullPrice");
                    _ExePrice = string.Empty;
                    this.OnPropertyChanged("ExePrice");
                    CalBond("4", GetSetttle(), NumChange(_Price),false);
                }

                this.OnPropertyChanged();

            }
        }
        /// <summary>
        /// 行权收益率
        /// </summary>
        private string _ExePrice = string.Empty;

        public string ExePrice
        {
            get { return _ExePrice; }
            set
            {
                string oldPrice = _ExePrice;
                _ExePrice = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                if (string.IsNullOrEmpty(_ExePrice))
                {
                    //Price = string.Empty;
                    NetPrice = string.Empty;
                    FullPrice = string.Empty;
                }
                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    _Price = string.Empty;
                    this.OnPropertyChanged("Price");
                    _NetPrice = string.Empty;
                    this.OnPropertyChanged("NetPrice");
                    _FullPrice = string.Empty;
                    this.OnPropertyChanged("FullPrice");
                    CalBond("7", GetSetttle(), NumChange(_ExePrice), false);
                }
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 0-净价，1-全价
        /// </summary>
        private string _NetPriceFlag = string.Empty;

        public string NetPriceFlag
        {
            get { return _NetPriceFlag; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _NetPriceFlag = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 净价
        /// </summary>
        private string _NetPrice ;

        public string NetPrice
        {
            get { return _NetPrice; }
            set
            {
                string oldPrice = _NetPrice;
                _NetPrice = value;

                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                decimal d = 0;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                {
                    CalBond("2", GetSetttle(), _NetPrice, false);
                }

                if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                {
                    //CustomPrice = string.Empty;
                    Price = string.Empty;
                    ExePrice = string.Empty;
                    //_NetPrice = string.Empty;
                }

                this.OnPropertyChanged();
                //setTrdMatchId();
                setPaymentNetPrice();
            }
        }
        private void setTrdMatchId() 
        {
            if (string.IsNullOrEmpty(PaymentNetPrice))
            {
                TrdMatchId = "";
            }
            else 
            {
                int dotIndex = PaymentNetPrice.IndexOf('.');
                string fractionalPart;

                if (dotIndex >= 0)
                {
                    fractionalPart = PaymentNetPrice.Substring(dotIndex + 1);
                }
                else
                {
                    fractionalPart = "0";
                }
                if (fractionalPart.Length < 3)
                    fractionalPart = fractionalPart.PadRight(3, '0');
                else 
                {
                    fractionalPart = fractionalPart.Substring(fractionalPart.Length - 3, 3);
                }

                if (fractionalPart[0] == '0')
                {
                    fractionalPart = '1' + fractionalPart.Substring(1);
                }
                TrdMatchId = fractionalPart;
            }
        }

        
        /// <summary>
        /// 全价
        /// </summary>
        private string _FullPrice = string.Empty;

        public string FullPrice
        {
            get { return _FullPrice; }
            set
            {
                string oldPrice = _FullPrice;
                _FullPrice = value;

                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                decimal d = 0;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                {
                    CalBond("3", GetSetttle(), _FullPrice, false);
                }

                if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                {
                    //CustomPrice = string.Empty;
                    _Price = string.Empty;
                    _ExePrice = string.Empty;
                    //_NetPrice = string.Empty;
                }
                this.OnPropertyChanged();
            }
        }


        private string _SourceQty;
        public string SourceQty
        {
            get { return _SourceQty; }
            set
            {
                if (value != null)
                {
                    _SourceQty = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 量
        /// </summary>
        private string _Qty;
        public string Qty
        {
            get { return _Qty; }
            set
            {
                if (value != null)
                {
                    _Qty = value;

                    if (string.IsNullOrEmpty(SourceQty))
                    {
                        if (IsIRS)
                        {
                            if (!string.IsNullOrEmpty(value))
                                SourceQty = (decimal.Parse(value) * 100).ToString();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(value))
                                SourceQty = (decimal.Parse(value) * 1000).ToString();
                        }
                    }

                    this.OnPropertyChanged();
                    setSettlAmount();
                }
            }
        }


        private CrmInst _RealcounterpartynameItem;

        public CrmInst RealcounterpartynameItem
        {
            get { return _RealcounterpartynameItem; }
            set
            {
                if (_RealcounterpartynameItem != value)
                {
                    _RealcounterpartynameItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _RealcounterpartynameText;

        public string RealcounterpartynameText
        {
            get { return _RealcounterpartynameText; }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                if (_RealcounterpartynameText !=newValue)
                {
                    _RealcounterpartynameText = newValue;
                    this.OnPropertyChanged();
                }
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

                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                string newDate = _TradeDate == null ? string.Empty : _TradeDate.Value.Date.ToString(YYYYMMDD);
                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    if ("1".Equals(YieldTypeFlag)) 
                    {
                        CalBond("7", GetSetttle(), NumChange(_ExePrice), false);
                    }
                    else
                    {
                        CalBond("4", GetSetttle(), NumChange(_Price), false);
                    }
                }
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
                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                if (!isSendReq && !oldIndex.Equals(value))
                {
                    if ("1".Equals(YieldTypeFlag))
                    {
                        CalBond("7", GetSetttle(), NumChange(_ExePrice), false);
                    }
                    else
                    {
                        CalBond("4", GetSetttle(), NumChange(_Price), false);
                    }
                }
                //CalBond(GetSetttle(), true);
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
        Dictionary<string, string> _OrderSideSource = new Dictionary<string, string>();
        public Dictionary<string, string> OrderSideSource
        {
            get
            {
                return _OrderSideSource;
            }
            set
            {
                _OrderSideSource = value;
                this.OnPropertyChanged();
            }
        }
        private string _OrderSide_Text;
        public string OrderSide_Text
        {
            get
            {
                return _OrderSide_Text;
            }
            set
            {
                _OrderSide_Text = value;
                this.OnPropertyChanged();
            }
        }

        Dictionary<string, string> _OrderMethodSource = new Dictionary<string, string>();
        public Dictionary<string, string> OrderMethodSource
        {
            get
            {
                return _OrderMethodSource;
            }
            set
            {
                _OrderMethodSource = value;
                this.OnPropertyChanged();
            }
        }
        private string _OrderMethod_Text;
        public string OrderMethod_Text
        {
            get
            {
                return _OrderMethod_Text;
            }
            set
            {
                _OrderMethod_Text = value;
                this.OnPropertyChanged();
            }
        }
        private CrmInst _CounterPartyNameItem;

        public CrmInst CounterPartyNameItem
        {
            get { return _CounterPartyNameItem; }
            set
            {
                if (_CounterPartyNameItem != value)
                {
                    _CounterPartyNameItem = value;

                    if (value != null)
                    {
                        /*List<CrmInst> result = GetCrmInst();
                        if (this._CounterPartyNameItem != null && !string.IsNullOrEmpty(this._CounterPartyNameItem.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._CounterPartyNameItem.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                //this.SelectCounterClientTypeItem = result.First();
                            }

                        }*/
                    }
                    RefeshInstMsg();
                    this.OnPropertyChanged();
                    //setTxt();
                }
            }
        }
        private void RefeshInstMsg() 
        {
            if (isSetValue) 
            {
                return;
            }
            if (CounterPartyNameItem != null && !string.IsNullOrEmpty(SearchText))
            {
                if (SearchText.ToUpper().EndsWith(".SH"))
                {
                    ShanghaiInstCode = CounterPartyNameItem.sseinstitutioncode;
                    ShenzhenInstCode = "";
                    ShenzhenTradingPart = "";
                    ShenzhenPartySubNameText = "";
                }
                else if (SearchText.ToUpper().EndsWith(".SZ"))
                {

                    ShanghaiInstCode = "";
                    ShenzhenInstCode = CounterPartyNameItem.szedealercode;
                    ShenzhenTradingPart = CounterPartyNameItem.ssetradingentitycode;
                    string instname = CounterPartyNameItem.shortnamecn;
                    List<CrmContact> list = STC.PT.Plugin.Utils.CommonUtils.GetContactInfoPoByInstname(instname,true);
                    if (list.Count > 0 && list.Count ==1)
                    {
                        CrmContact crmContact = list[0];
                        CrmContactPT crmContactPT = new CrmContactPT
                        {
                            id = crmContact.id,
                            namecn = crmContact.namecn,
                            instname = crmContact.instname
                        };
                        ShenzhenPartySubNameText = crmContactPT.DisplayName;
                    }
                    else 
                    {
                        ShenzhenPartySubNameText = "";
                    }
                }
                else
                {
                    ShanghaiInstCode = "";
                    ShenzhenInstCode = "";
                    ShenzhenTradingPart = "";
                    ShenzhenPartySubNameText = "";
                }
            }
            else 
            {
                ShanghaiInstCode = "";
                ShenzhenInstCode = "";
                ShenzhenTradingPart = "";
                ShenzhenPartySubNameText = "";
            }
        }
        private string _CounterPartyNameText;

        public string CounterPartyNameText
        {
            get { return _CounterPartyNameText; }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                if (_CounterPartyNameText != newValue)
                {
                    /*if (isSysAuto_Institution == false
                    && _CounterinstitutionName_Edit != value)
                    {
                        isSysAuto_Trader = true;
                        SelectedCounterTrader_Edit = null;
                        CounterTraderName_Edit = string.Empty;
                    }
                    isSysAuto_Institution = false;*/

                    _CounterPartyNameText = newValue;
                    if (_CounterPartyNameText != null) 
                    {
                        List<CrmInst> results = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(_CounterPartyNameText, 2,  null);
                        if (results != null && results.Count == 1)
                        {
                            CounterPartyNameItem = results[0];
                        }
                        else {
                            CounterPartyNameItem = null;
                        }
                    }
                    this.OnPropertyChanged();
                    //setTxt();
                }
            }
        }

        
        private string _PartyInstName;
        public string PartyInstName
        {
            get
            {
                return _PartyInstName;
            }
            set
            {
                _PartyInstName = value;
                this.OnPropertyChanged();
            }
        }
        private StcTbookUsers _PartySubNameItem;
        public StcTbookUsers PartySubNameItem
        {
            get
            {
                return _PartySubNameItem;
            }
            set
            {
                _PartySubNameItem = value;
                this.OnPropertyChanged();
            }
        }
        private string _PartySubName;
        public string PartySubName
        {
            get
            {
                return _PartySubName;
            }
            set
            {
                _PartySubName = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 本方交易员是否可编辑 0-可编辑 1-不可编辑
        /// </summary>
        private string _PartySubNameIsReadOnly;
        public string PartySubNameIsReadOnly
        {
            get
            {
                return _PartySubNameIsReadOnly;
            }
            set
            {
                _PartySubNameIsReadOnly = value;
                this.OnPropertyChanged();
            }
        }
        private CrmContact _CounterPartySubNameItem;

        public CrmContact CounterPartySubNameItem
        {
            get { return _CounterPartySubNameItem; }
            set
            {

                if (_CounterPartySubNameItem != value)
                {
                    if (value != null)
                    {
                        if (CounterPartyNameItem == null || CounterPartyNameItem.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                isSysAuto_Institution = true;
                                isSysAuto_Trader = true;
                                CounterPartyNameItem = results[0];
                                CounterPartyNameText = results[0].shortnamecn;
                            }
                        }

                        if (CounterPartyNameItem != null && CounterPartyNameItem.idbkey == value.instid)
                        {
                            isSysAuto_Institution = false;
                            isSysAuto_Trader = true;
                            CounterPartyNameItem = CounterPartyNameItem;
                            CounterPartyNameText = CounterPartyNameText;
                        }
                    }
                    _CounterPartySubNameItem = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _CounterPartySubNameText;

        public string CounterPartySubNameText
        {
            get { return _CounterPartySubNameText; }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 50)
                    newValue = newValue.Substring(0, 50);
                if (_CounterPartySubNameText != newValue)
                {
                    isSysAuto_Trader = false;
                    _CounterPartySubNameText = newValue;
                    if(!isSetValue)
                        setDefult();
                    this.OnPropertyChanged();
                }
            }
        }

        private string _ShanghaiInstCode;
        public string ShanghaiInstCode
        {
            get
            {
                return _ShanghaiInstCode;
            }
            set
            {
                _ShanghaiInstCode = value;
                this.OnPropertyChanged();
            }
        }
        private string _ShenzhenInstCode;
        public string ShenzhenInstCode
        {
            get
            {
                return _ShenzhenInstCode;
            }
            set
            {
                _ShenzhenInstCode = value;
                this.OnPropertyChanged();
            }
        }
        private string _PaymentRate;
        public string PaymentRate
        {
            get
            {
                return _PaymentRate;
            }
            set
            {
                _PaymentRate = value;
                this.OnPropertyChanged();
                setPaymentNetPrice();
            }
        }
        private string _ShenzhenTradingPart;
        public string ShenzhenTradingPart
        {
            get
            {
                return _ShenzhenTradingPart;
            }
            set
            {
                _ShenzhenTradingPart = value;
                this.OnPropertyChanged();
            }
        }
        private string _PaymentNetPrice;
        public string PaymentNetPrice
        {
            get
            {
                return _PaymentNetPrice;
            }
            set
            {
                string oldPrice = _PaymentNetPrice;
                _PaymentNetPrice = value;
                if (isSetValue)
                {
                    this.OnPropertyChanged();
                    return;
                }
                decimal d = 0;
                if (!string.IsNullOrEmpty(value)&& (string.IsNullOrEmpty(oldPrice) || !oldPrice.Equals(value) && decimal.TryParse(value, out d)))
                {
                    CalBond("2", GetSetttle(), _PaymentNetPrice, true);
                }
                this.OnPropertyChanged();
                setTrdMatchId();
            }
        }

        private void setPaymentNetPrice()
        {
            if (isSetValue)
                return;
            if (!string.IsNullOrEmpty(NetPrice))
            {
                decimal paymentRate = string.IsNullOrEmpty(PaymentRate) ? 0 : decimal.Parse(PaymentRate);
                decimal coufee = string.IsNullOrEmpty(Couponrate) ? 0 : decimal.Parse(Couponrate);
                decimal netprice = decimal.Parse(NetPrice);
                
                if (SideIndex == 1)//Ofr成交净价-代付费率-过券费率
                {
                    PaymentNetPrice = (netprice-paymentRate / 1000 - coufee / 1000).ToString();
                }
                else //Bid成交净价+代付费率+过券费率
                {
                    PaymentNetPrice = (netprice + paymentRate / 1000 + coufee / 1000).ToString();
                }
            }
            else
            {
                PaymentNetPrice = "";
            }
        }
        private string _PaymentFullPrice;
        public string PaymentFullPrice
        {
            get
            {
                return _PaymentFullPrice;
            }
            set
            {
                _PaymentFullPrice = value;
                this.OnPropertyChanged();
                setSettlAmount();
            }
        }
        private string _PaymentExePrice;
        public string PaymentExePrice
        {
            get
            {
                return _PaymentExePrice;
            }
            set
            {
                _PaymentExePrice = value;
                this.OnPropertyChanged();
            }
        }
        private void setSettlAmount() 
        {
            if (!string.IsNullOrEmpty(PaymentFullPrice) && !string.IsNullOrEmpty(Qty))
            {
                SettlAmount = (decimal.Parse(PaymentFullPrice) * decimal.Parse(Qty) * 10000);
            }
            else 
            {
                SettlAmount = null;
            }
        }
        private decimal? _SettlAmount;
        public decimal? SettlAmount
        {
            get
            {
                return _SettlAmount;
            }
            set
            {
                _SettlAmount = value;
                this.OnPropertyChanged();
            }
        }
        private string _PaymentYield;
        public string PaymentYield
        {
            get
            {
                return _PaymentYield;
            }
            set
            {
                _PaymentYield = value;
                /*string oldPrice = _PaymentYield;
                _PaymentYield = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value; 
                if (isSetValue)
                    return;
                decimal d = 0;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !object.Equals(oldPrice,value) && decimal.TryParse(value, out d))
                {
                    CalBond("4", GetSetttle(), NumChange(_PaymentYield), true);
                }*/
                this.OnPropertyChanged();
            }
        }

        private string _Couponrate;
        public string Couponrate
        {
            get
            {
                return _Couponrate;
            }
            set
            {
                _Couponrate =  value;
                this.OnPropertyChanged();
                setPaymentNetPrice();
            }
        }
        private CrmContactPT _ShenzhenPartySubNameItem;

        public CrmContactPT ShenzhenPartySubNameItem
        {
            get { return _ShenzhenPartySubNameItem; }
            set
            {

                if (_ShenzhenPartySubNameItem != value)
                {
                    _ShenzhenPartySubNameItem = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _ShenzhenPartySubNameText;

        public string ShenzhenPartySubNameText
        {
            get { return _ShenzhenPartySubNameText; }
            set
            {
                if (!(string.IsNullOrEmpty(_ShenzhenPartySubNameText) && string.IsNullOrEmpty(value)) && _ShenzhenPartySubNameText != value)
                {
                    isSysAuto_Trader = false;
                    _ShenzhenPartySubNameText = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //约定号
        private string _TrdMatchId;
        public string TrdMatchId
        {
            get
            {
                return _TrdMatchId;
            }
            set
            {
                _TrdMatchId = value;
                this.OnPropertyChanged();
            }
        }
        private string _Remark;
        public string Remark
        {
            get
            {
                return _Remark;
            }
            set
            {
                var newValue = value ?? string.Empty;
                if (newValue.Length > 1000)
                    newValue = newValue.Substring(0, 1000);

                if (_Remark != newValue)
                {
                    _Remark = newValue;
                    this.OnPropertyChanged();
                }
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
        private string _SymbolType = string.Empty;
        public string SymbolType
        {
            get
            {
                return _SymbolType;
            }

            set
            {
                if (value != _SymbolType)
                {
                    _SymbolType = value;
                    this.OnPropertyChanged();
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

        public PtExecordersData _ExecordersData;
        public PtExecordersData ExecordersData
        {
            get { return _ExecordersData; }
            set
            {
                _ExecordersData = value;
                if (value != null)
                {
                    Refresh();
                }
                else 
                {
                    VenueDisplay = "自谈";
                }
                this.OnPropertyChanged();
            }
        }

        private string _AllocText;
        public string AllocText
        {
            get
            {
                return _AllocText;
            }
            set
            {
                _AllocText = value;
                this.OnPropertyChanged();
            }
        }

        public bool isSales { get; set; }
        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }
        public ICommand ConfirmCommand { get { return new RelayCommand(ConfirmClick); } }

        public ICommand ConfirmAndAddCommand { get { return new RelayCommand(ConfirmAndAddClick); } }
        public event Action RequestClose;

        // 触发关闭窗口的方法
        public void CloseWindow()
        {
            RequestClose?.Invoke();
        }
        #endregion

        public ExecManualEntryViewModel()
        {
            this.NetPriceFlag = "0";
            NetPrice = string.Empty;
            Price = string.Empty;
            SettleType = "2";
            SettlTypeIndex = 1;
            CurrentDate = DateTime.Today;
            GetSettleTypeFromServer("2");
            OrderSideSource.Clear();
            OrderSideSource.Add("0", "待定");
            OrderSideSource.Add("1","我方发");
            OrderSideSource.Add("2", "对方发");
            OrderMethodSource.Clear();
            OrderMethodSource.Add("0", "待定");
            OrderMethodSource.Add("1", "对话");
            OrderMethodSource.Add("2", "请求");
            OrderMethodSource.Add("6", "固收");
            OrderMethodSource.Add("7", "竞价");
            PartyInstName = PTExecOrderProcesser.Instance.partyName;
            //PartySubName = LocalEnv.UserInfo.FullName;
            OrderSide_Text = "1";
        }

        private void Refresh()
        {
            isSetValue = true;
            PartySubNameIsReadOnly = "1";
            PartySubName = ExecordersData.PartySubName;
            if ("接口".Equals(ExecordersData.Entrytype))
            {
                IsManual = "1";
                if (string.IsNullOrEmpty(PartySubName)) 
                {
                    PartySubNameIsReadOnly = "0";
                }
            }
            else
            {
                IsManual = "0";
            }
            VenueDisplay = ComboSourceUtil.Instance.FindEnum(ExecordersData.Venues, "venues_exept");
            this.SearchText = ExecordersData.SecurityID;
            this.Symbol = ExecordersData.Symbol;
            this.SideIndex = "1".Equals(ExecordersData.Side) ? 0 : 1;
            this.Price = ExecordersData.LastPx;
            if ("0".Equals(IsExeCdcType))
            {
                this.ExePrice = ExecordersData.Yieldtoexecution;
                this.PaymentExePrice = ExecordersData.Yieldtoexecution2 == decimal.Zero?"": CommonUtils.ConvertToString(ExecordersData.Yieldtoexecution2);
            }
            this.NetPriceFlag = "0";
            this.NetPrice = CommonUtils.ConvertToDecimalOrNull(ExecordersData.NetPrice) == decimal.Zero ? "": ExecordersData.NetPrice;
            this.FullPrice = CommonUtils.ConvertToDecimalOrNull(ExecordersData.FullPrice) == decimal.Zero? "": ExecordersData.FullPrice;
            this.Qty = ExecordersData.LastQty; 
            this.RealcounterpartynameText = ExecordersData.Realcounterpartyname;
            this.TradeDate = DateTime.ParseExact(ExecordersData.TradeDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.SettlTypeIndex = "+1".Equals(ExecordersData.SettlType) ? 0 : 1;
            this.OrderSide_Text = ExecordersData.TrdSubType;
            this.OrderMethod_Text = ExecordersData.TrdType;
            this.CounterPartyNameText = ExecordersData.CounterPartyName;
            this.CounterPartySubNameText = ExecordersData.CounterPartySubName;
            this.ShanghaiInstCode = ExecordersData.Sseinstitutioncode;
            this.ShenzhenInstCode = ExecordersData.Szedealercode;
            this.ShenzhenTradingPart = ExecordersData.Szetradingentitycode;
            this.ShenzhenPartySubNameText = ExecordersData.Szetradercode;
            this.PaymentRate = ExecordersData.BridgePaymentFixedRate;
            this.PaymentNetPrice = ExecordersData.CleanPrice2 == decimal.Zero ? "" : CommonUtils.ConvertToString(ExecordersData.CleanPrice2);
            this.PaymentFullPrice = ExecordersData.FullPrice2 == decimal.Zero ? "" : CommonUtils.ConvertToString(ExecordersData.FullPrice2);
            this.SettlAmount = ExecordersData.SettlAmount;
            this.Couponrate = ExecordersData.Couponrate;
            this.PaymentYield = ExecordersData.Yield2 == decimal.Zero ? "" : CommonUtils.ConvertToString(ExecordersData.Yield2);
            this.TrdMatchId = ExecordersData.TrdMatchId;
            this.Remark = ExecordersData.Remark;
            isSetValue = false;

        }
        #region 事件

        private void CancelClick()
        {

        }
        Dictionary<string, PtExecOrderSaveReq> execDic = new Dictionary<string, PtExecOrderSaveReq>();
        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
        private void ConfirmClick()
        {
            execDic.Clear();
            warnReplyDic.Clear();
            if (!check())
                return;
            PtExecOrderSaveReq req = getPtExecOrderSaveReq();
            ThreadPool.QueueUserWorkItem(o =>
            {
                if ("0".Equals(IsAdd))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        /*req.counterpartyid = PTExecOrderProcesser.Instance.partyId;
                        req.counterpartyname = PTExecOrderProcesser.Instance.partyName;*/

                        if (IsEnableParse)
                        {
                            req.allocText = AllocText;
                        }
                        if (!string.IsNullOrEmpty(Price))
                            req.price = Price;
                        req.partysubname = LocalEnv.UserInfo.FullName;
                        req.partysubid = LocalEnv.UserInfo.UserId;
                        req.partyid = PTExecOrderProcesser.Instance.partyId;
                        req.partyname = PTExecOrderProcesser.Instance.partyName;
                        req.traderid = LocalEnv.UserInfo.UserName;
                        req.tradername = LocalEnv.UserInfo.FullName;
                        PResult result = PTHandler.Instance.ExecOrderSave(req);
                        if (result != null )
                        {
                            if ("10000".Equals(result.Code))
                            {
                                bool isProhibitory = false;
                                List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(result.Msg);
                                foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                                {
                                    if (placeOrderResult.Code == "10002")
                                    {
                                        isProhibitory = true;
                                        break;
                                    }
                                }
                                warnReplyDic[result.RequestID] = orderRisks;
                                execDic[result.RequestID] = req;
                                BatchWarnView batchWarnView = new BatchWarnView(warnReplyDic, null, execDic, false, isProhibitory);
                                batchWarnView.Owner = myWindow;
                                bool? isstatus = batchWarnView.ShowDialog();
                                if (isstatus == true) 
                                {
                                    CloseWindow();
                                }
                            }
                            else
                            {
                                CloseWindow();
                            }
                        }
                        else
                        {
                            showMessage(string.Format("{0}失败{1}", "新增", result != null ? result.Msg : ""), false);
                        }
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        req.execid = ExecordersData.ExecID + "";
                        req.venuetypegw = ExecordersData.Venuetypegw;
                        req.partysubname = ExecordersData.PartySubName;
                        req.partysubid = ExecordersData.PartySubId;
                        req.traderid = ExecordersData.TraderID;
                        req.tradername = ExecordersData.TraderName;
                        if (!string.IsNullOrEmpty(ExecordersData.MatchQty))
                        {
                            if (CommonUtils.ConvertToDecimal(req.qty) > CommonUtils.ConvertToDecimal(ExecordersData.MatchQty))
                            {
                                req.matchstatus = "6";
                            }
                            else if (CommonUtils.ConvertToDecimal(req.qty) == CommonUtils.ConvertToDecimal(ExecordersData.MatchQty))
                            {
                                req.matchstatus = "7";
                            }
                        }
                        PtExecUpdateReplyPo result = PTHandler.Instance.ExecOrderUpdate(req);
                        if (result != null)
                        {
                            if ("10000".Equals(result.Code))
                            {
                                bool isProhibitory = false;
                                List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(result.Msg);
                                foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                                {
                                    if (placeOrderResult.Code == "10002")
                                    {
                                        isProhibitory = true;
                                        break;
                                    }
                                }
                                warnReplyDic[result.RequestID] = orderRisks;
                                execDic[result.RequestID] = req;
                                BatchWarnView batchWarnView = new BatchWarnView(warnReplyDic, null, execDic, false, isProhibitory);
                                batchWarnView.Owner = myWindow;
                                bool? isstatus = batchWarnView.ShowDialog();
                                if (isstatus == true)
                                {
                                    CloseWindow();
                                }
                            }
                            else
                            {
                                CloseWindow();
                            }
                        }
                        else
                        {
                            showMessage(string.Format("修改失败{0}", result != null ? result.Msg : ""), false);
                        }
                    }));
                }
            });
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message, bool autoClose)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message, autoClose);
            me.ShowDialog();
        }

        public bool check()
        {
            bool ischeck = false;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                CustomMessageBox.Info("缺少债券!", "录入");
                return ischeck;
            }
            if (SideIndex == -1)
            {
                CustomMessageBox.Info("缺少方向!", "录入");
                return ischeck;
            }
            if (!IsCTP)
            {
                if (string.IsNullOrWhiteSpace(Price))
                {
                    CustomMessageBox.Info("缺少价格!", "录入");
                    return ischeck;
                }
            }
            if (!IsIRS)
            {
                if (string.IsNullOrWhiteSpace(NetPrice))
                {
                    CustomMessageBox.Info("缺少净价!", "录入");
                    return ischeck;
                }
                /*if (string.IsNullOrWhiteSpace(FullPrice))
                {
                    CustomMessageBox.Info("缺少全价!", "录入");
                    return ischeck;
                }*/
            }
            if (string.IsNullOrWhiteSpace(Qty))
            {
                CustomMessageBox.Info("缺少量!", "录入");
                return ischeck;
            }
            else 
            {
                if (!"0".Equals(IsAdd) && ExecordersData != null && 
                    !string.IsNullOrEmpty(ExecordersData.MatchQty)&&
                    CommonUtils.ConvertToDecimal(ExecordersData.MatchQty) > CommonUtils.ConvertToDecimal(Qty))
                {
                    CustomMessageBox.Info("成交量不能小于已匹配的量!", "录入");
                    return ischeck;
                }
            }
           /* if (string.IsNullOrWhiteSpace(RealcounterpartynameText))
            {
                CustomMessageBox.Info("缺少终端对手方!", "录入");
                return ischeck;
            }*/
            if (SettlTypeIndex == -1)
            {
                CustomMessageBox.Info("缺少清算速度!", "录入");
                return ischeck;
            }
            if (TradeDate == null || !TradeDate.HasValue)
            {
                CustomMessageBox.Info("缺少交易日期!", "录入");
                return ischeck;
            }
            /*if (string.IsNullOrWhiteSpace(CounterPartyNameText))
            {
                CustomMessageBox.Info("缺少发单机构!", "录入");
                return ischeck;
            }*/
            if (!string.IsNullOrWhiteSpace(PaymentRate) && 
                (CommonUtils.ConvertToDecimal(PaymentRate) < 1 || CommonUtils.ConvertToDecimal(PaymentRate) > 100))
            {
                CustomMessageBox.Info("代付费率设置错误，取值需要在[1,100]!", "录入");
                return ischeck;
            }
            ischeck = true;
            return ischeck;
        }
        #endregion

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

        /// <summary>
        /// 获取下单用的值 万
        /// </summary>
        /// <returns></returns>
        public decimal GetTrueAmount()
        {
            decimal d = 0;
            decimal.TryParse(_Qty, out d);

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

        

        private string Txt_KW = "kw";
        private string Txt_W = "w";
        private string Txt_IRS = "百万";
        private string Txt_CTP = "手";

        public void setIsBrokerSecurity(string securityID, bool isBillon = false)
        {
            IsBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(securityID);
            IsExeCdcType = "1";
            YieldTypeFlag = "0";

            var x = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
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
                if (x != null)
                {
                    if (!string.IsNullOrEmpty(x.Option_Type) &&
                            ((!x.Option_Type.Equals("NON") && x.Option_Date != null) ||
                            (x.Option_Type.Equals("ETS") && x.Option_Date == null)))
                    {
                        IsExeCdcType = "0";
                        YieldTypeFlag = "1";
                    }
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
                if (x != null && !string.IsNullOrEmpty(x.Option_Type) &&
                        ((!x.Option_Type.Equals("NON") && x.Option_Date != null) ||
                        (x.Option_Type.Equals("ETS") && x.Option_Date == null)))
                {
                    IsExeCdcType = "0";
                    YieldTypeFlag = "1";
                }
            }
        }
        public void setDefult() 
        {
            if (isSetValue)
                return;
            if (string.IsNullOrEmpty(SearchText))
                return;
            string securityid = SearchText;
            if (securityid.ToUpper().EndsWith(".IB"))
            {
                if (string.IsNullOrEmpty(CounterPartySubNameText))
                {
                    OrderMethod_Text = "2";//对话
                }
                else
                {
                    OrderMethod_Text = "1";//请求
                }
            }
            else if(securityid.ToUpper().EndsWith(".SH") || securityid.ToUpper().EndsWith(".SZ"))
            {
                OrderMethod_Text = "6";//固收
            }
        }
        public void InitPriceView(string searchText = "")
        {
            string securityID = string.Empty;
            var x = CommonUtils.GetBondsInfoPoBySecurityID(searchText);
            if (x != null)
            {
                Symbol = (x as BondsInfoPo).Short_Name;
                SymbolType = (x as BondsInfoPo).SECURITY_TYPE;
                IsIRS = (x as BondsInfoPo).MarketIndicator == "2";
                IsCTP = (x as BondsInfoPo).MarketIndicator == "5";
            }
        }
        public static List<KeyValuePair<string, string>> QuotemethodItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("quotemethod_pt");
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
            }
        }


        #region 机构交易员

        private bool isSysAuto_Institution = false;

        private bool isSysAuto_Trader = false;

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
        internal void FilterCounterInstitution(AutoCompleteArgs args,bool status)
        {
            string currentInstId = "";
            if (status)
            {
                currentInstId = this.CounterPartySubNameItem == null ? string.Empty : this.CounterPartySubNameItem.instid;
            }

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

                        List<CrmInst> selectDatas = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(str, 2000, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 2000, instIdList);
                }
            }
            else
            {
                /* List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                 if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                 {
                     result.AddRange(myInstList);
                 }
                 else
                 {
                     result.AddRange(STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(instIdList));
                 }*/

                result.AddRange(STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(instIdList));
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
        #endregion

        public bool isSetValue = false;

        private bool isSendReq = false;

        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private bool _needCalPrice = true;
        private object _calLock = new object();
        private CalResult _lastCalResult = null;



        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val,bool isPayment)
        {
            calBondErrMsg = string.Empty;
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) || !_needCalPrice)
                return;

            //ThreadPool.QueueUserWorkItem(async _ =>
            //{
                //await _writeSemaphore.WaitAsync();
                _lastReqData = string.Format("{0}-{1}-{2}", calType, settleType, val);
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
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    isSendReq = true;
                                    lock (_calLock)
                                    {
                                        _lastCalResult = result;

                                    }
                                    if (isPayment) 
                                    {
                                        if (calType.Equals("4"))
                                        {
                                            _PaymentNetPrice = result.Data.CleanPrice.ToString("F4");
                                            this.OnPropertyChanged("PaymentNetPrice");
                                            PaymentFullPrice = result.Data.FullPrice.ToString("F4");
                                            this.OnPropertyChanged("PaymentFullPrice");
                                            if ("0".Equals(IsExeCdcType))
                                            {
                                                PaymentExePrice = (result.Data.YieldToExecution * 100).ToString("F4");
                                                this.OnPropertyChanged("PaymentExePrice");
                                            }
                                            setTrdMatchId();
                                            setSettlAmount();
                                        }
                                        else if (calType.Equals("2"))//净价
                                        {
                                            if (result.IsSuccess())
                                            {
                                                PaymentYield = (result.Data.Yield * 100).ToString("F4");
                                                PaymentFullPrice = result.Data.FullPrice.ToString("F4");
                                                this.OnPropertyChanged("PaymentYield");
                                                this.OnPropertyChanged("PaymentFullPrice");
                                                if ("0".Equals(IsExeCdcType))
                                                {
                                                    PaymentExePrice = (result.Data.YieldToExecution * 100).ToString("F4");
                                                    this.OnPropertyChanged("PaymentExePrice");
                                                }
                                                setSettlAmount();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (calType.Equals("4"))
                                        {
                                            if (SearchText.EndsWith(".SH")) 
                                            {
                                                NetPrice = result.Data.CleanPrice.ToString("F3");
                                            }
                                            else
                                            {
                                                NetPrice = result.Data.CleanPrice.ToString("F4");
                                            }

                                            setPaymentNetPrice();
                                            FullPrice = result.Data.FullPrice.ToString("F4");
                                            if ("0".Equals(IsExeCdcType))
                                            {
                                                ExePrice = (result.Data.YieldToExecution * 100).ToString("F4");
                                            }
                                            setTrdMatchId();
                                            this.OnPropertyChanged("NetPrice");
                                            this.OnPropertyChanged("FullPrice");
                                            this.OnPropertyChanged("ExePrice");
                                        }
                                        else if (calType.Equals("2"))//净价
                                        {
                                            if (result.IsSuccess())
                                            {
                                                Price = (result.Data.Yield * 100).ToString("F4");
                                                FullPrice = result.Data.FullPrice.ToString("F4");
                                                if ("0".Equals(IsExeCdcType))
                                                {
                                                    ExePrice = (result.Data.YieldToExecution * 100).ToString("F4");
                                                }
                                                this.OnPropertyChanged("Price");
                                                this.OnPropertyChanged("FullPrice");
                                                this.OnPropertyChanged("ExePrice");
                                            }
                                        }
                                        else if (calType.Equals("7"))//行权收益率
                                        {
                                            if (result.IsSuccess())
                                            {
                                                Price = (result.Data.Yield * 100).ToString("F4");
                                                FullPrice = result.Data.FullPrice.ToString("F4");
                                                //NetPrice = result.Data.CleanPrice.ToString("F4");
                                                if (SearchText.EndsWith(".SH"))
                                                {
                                                    NetPrice = result.Data.CleanPrice.ToString("F3");
                                                }
                                                else
                                                {
                                                    NetPrice = result.Data.CleanPrice.ToString("F4");
                                                }
                                                setTrdMatchId();
                                                this.OnPropertyChanged("Price");
                                                this.OnPropertyChanged("FullPrice");
                                                this.OnPropertyChanged("NetPrice");
                                            }
                                        }
                                    }

                                    isSendReq = false;
                                });
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
                    //_writeSemaphore.Release();
                }
            //});
        }

        /// <summary>
        /// 清算时间切换至其他
        /// </summary>
        private void SetOtherVisibile()
        {
            string settle = SettleType.ToString();
            if (settle.Equals("") || settle.Equals("1") || settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3,2") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
            {
                //this.CalBond("4", true);
            }
            else
            {
                //恢复 Settle
                string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                }
            }
        }

        /// <summary>
        /// 计算正确的清算速度
        /// </summary>
        /// <param name="settleType">明天+0 ，6.15+0 </param>
        private void GetSettleTypeFromServer(string settleType)
        {
            if (isSetValue)
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
                            string tradedate = result.Data.settlementDate;
                            if (!string.IsNullOrEmpty(tradedate))
                            {
                                TradeDate = CommonUtils.GetDate(tradedate);
                            }
                            else 
                            {
                                TradeDate = DateTime.Now;
                            }

                        }
                        catch (Exception e) 
                        { }

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
            if (SettlTypeIndex == 0)
            {
                //选择+1
                settle = "2";
                if (TradeDate != null && TradeDate.HasValue)
                {
                    settle = TradeDate.Value.ToString("MM.dd") + "+1";
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
            _needCalPrice = false;
            try
            {
                if (!string.IsNullOrEmpty(settle))
                {

                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                    {
                        
                    }
                    else
                    {
                        if (settle.Equals("1") || settle.Equals("T+0") || settle.Equals("+0"))//T+0
                        {
                            
                        }
                        else if (settle.Equals("明天+1") || settle.Equals("4"))
                        {
                            //_needCalPrice = true;
                            //this.CalDate("4", false);
                            //CalBond("4", false);
                        }
                        else if (settle.Equals("远"))
                        {
                        }
                        else
                        {
                            string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            int month = 0;
                            int day = 0;
                            if (arr != null && arr.Length > 1)
                            {
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
                                        //SendPriceItem.CustomSettleDate = dt.ToString("yyy-MM-dd");
                                    }
                                    catch (Exception e) { }
                                }
                                //_needCalPrice = true;
                                if (arr[1].Equals("0"))
                                {
                                }
                                else if (arr[1].Equals("1"))
                                {
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
            
        }
        private bool isenableparse = true;
        public bool IsEnableParse
        {
            get { return isenableparse; }
            set { isenableparse = value; this.OnPropertyChanged(); }
        }
        public void RecognizeExec(string content)
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                IsEnableParse = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        RecognizeDealReq req = new RecognizeDealReq();
                        req.user = LocalEnv.UserInfo.UserName;
                        req.my_side_company_name = PTExecOrderProcesser.Instance.partyName;
                        req.prompt = content;
                        /*STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                        request.userName = LocalEnv.UserInfo.UserName;
                        request.content = content;*/


                        //STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = null;
                        /////获取订单 
                        //if (IsIRS)
                        //{
                        //    string[] lsstring = content.Split(enterSplitChar, StringSplitOptions.RemoveEmptyEntries);

                        //    if (lsstring != null && lsstring.Length > 0)
                        //    {
                        //        List<string> ls = new List<string>();
                        //        ls.Add(lsstring[0]);
                        //        request.contentList = ls;
                        //        reply = ParserHandler.Instance.RequestParserIRS(request);

                        //    }
                        //}
                        //else

                        RecognizeDealReply reply = ParserHandler.Instance.RequestParserDeal(req);


                        if (reply == null)
                        {
                            IsEnableParse = true;
                            CommonUtils.Prompt2Second("警告", $"报价识别失败");
                        }
                        else
                        {
                            if (!reply.IsSuccess())
                            {
                                IsEnableParse = true;
                                CommonUtils.Prompt2Second("警告", reply.Msg);
                            }
                            else if (reply.IsSuccess())
                            {
                                IsEnableParse = true;
                                SetDealParseData(reply);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        IsEnableParse = true;
                        LogHelper.Error("RequestOrders", e);
                    }
                });
            });
        }

        private void SetDealParseData(RecognizeDealReply replyPo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (replyPo != null && replyPo.Datas != null && replyPo.Datas.Count > 0)
                {
                    isSetValue = true;
                    TradeInfo obj = replyPo.Datas[0];
                    SearchText = obj.securityID;
                    if (!string.IsNullOrEmpty(obj.side))
                    {
                        SideIndex = "1".Equals(obj.side) ? 0 : 1;
                    }
                    if (!string.IsNullOrEmpty(obj.settlType))
                    {
                        if ("1".Equals(obj.settlType))
                        {
                            SettlTypeIndex = 1;
                        }
                        else if ("2".Equals(obj.settlType))
                        {
                            SettlTypeIndex = 0;
                        }
                        else if ("3".Equals(obj.settlType))
                        {
                            SettlTypeIndex = 1;
                        }
                        else
                        {
                            string settlType = obj.settlType;
                            int beginIndex = settlType.IndexOf('+');
                            settlType = settlType.Substring(beginIndex);
                            if ("+1".Equals(settlType))
                            {
                                SettlTypeIndex = 0;
                            }
                            else 
                            {
                                SettlTypeIndex = 1;
                            }
                        }
                    }
                    else
                    {
                        SettlTypeIndex = 1;
                    }

                    if (!string.IsNullOrEmpty(obj.tradeDate))
                    {
                        TradeDate = DateTime.ParseExact(obj.tradeDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(obj.settlType))
                        {
                            GetSettleTypeFromServer("3");
                        }
                        else 
                        {
                            GetSettleTypeFromServer(SettlTypeIndex == 0 ? "2" : "1");
                        }
                    }
                    PriceType = obj.priceType;
                    if (!string.IsNullOrEmpty(PriceType))
                    {
                        if ("1".Equals(obj.priceType))
                        {
                            Price = obj.price;
                            CalBond("4", GetSetttle(), NumChange(_Price), false);
                        }
                        else if ("2".Equals(obj.priceType))
                        {
                            ExePrice = obj.price;
                            CalBond("7", GetSetttle(), NumChange(_ExePrice), false);
                        }
                        else if ("3".Equals(obj.priceType))
                        {
                            NetPrice = string.IsNullOrEmpty(obj.price) ? obj.cleanPrice : obj.price; 

                            CalBond("2", GetSetttle(), _NetPrice, false);
                        }
                        else if ("4".Equals(obj.priceType))
                        {
                            FullPrice = obj.price;
                            CalBond("3", GetSetttle(), _FullPrice, false);
                        }
                        else if ("1".Equals(PriceType))
                        {
                            NetPrice = obj.price;
                            CalBond("2", GetSetttle(), _NetPrice, false);
                        }
                        else
                        {
                            CalBond("4", GetSetttle(), NumChange(_Price), false);
                            Price = obj.price;
                        }
                    }
                    else 
                    {
                        Price = obj.price;
                        CalBond("4", GetSetttle(), NumChange(_Price), false);
                    }
                    Qty = obj.allocQty;
                    RealcounterpartynameText = obj.realCounterPartyName;
                    CounterPartyNameText = obj.counterPartyName;
                    CounterPartySubNameText = obj.counterPartySubName;
                    PaymentRate = obj.bridgePaymentFixedRate;
                    if (!string.IsNullOrEmpty(obj.trdType))
                        OrderMethod_Text = obj.trdType;
                    if (!string.IsNullOrEmpty(obj.trdSubType))
                        OrderSide_Text = obj.trdSubType;
                    AllocText = obj.allocText;
                    isSetValue = false;
                    setPaymentNetPrice();
                    setTrdMatchId();
                    RefeshInstMsg();
                }
            }));
        }
        private void SetParseData(RecognizeIntentionQuoteReplyPo replyPo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (replyPo != null && replyPo.Datas != null && replyPo.Datas.Count > 0)
                {
                    IntentionQuoteInfo intentionQuoteInfo = replyPo.Datas[0];
                    SearchText = intentionQuoteInfo.securityId;
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.side))
                    {
                        SideIndex = "1".Equals(intentionQuoteInfo.side) ? 0 : 1;
                    }
                    PriceType = intentionQuoteInfo.priceType;
                    if (!string.IsNullOrEmpty(PriceType))
                    {
                        if ("1".Equals(PriceType))
                        {
                            NetPrice = intentionQuoteInfo.price.ToString();
                        }
                        else 
                        {
                            Price = intentionQuoteInfo.price.ToString();
                        }
                    }
                    Qty = intentionQuoteInfo.qty.ToString();
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.settlType))
                    {
                        SettlTypeIndex = "2".Equals(intentionQuoteInfo.settlType) ? 0 : 1;
                    }


                    List<CrmInst> crmInsts = null;
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.parseCompanyIdbkey))
                        crmInsts = CommonUtils.GetInstitutionsIDInfoPo(intentionQuoteInfo.parseCompanyIdbkey);


                    List<CrmContact> crmContacts = null;
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.parseCompanyIdbkey) && !string.IsNullOrEmpty(intentionQuoteInfo.parseTraderIdbkey))
                        crmContacts = CommonUtils.GetContactInfoPo(intentionQuoteInfo.parseTraderIdbkey, intentionQuoteInfo.parseCompanyIdbkey);



                    if (crmInsts != null && crmInsts.Count > 0)
                    {
                        //SelectedInstintuion_Edit = crmInsts[0];

                        //InstitutionName_Edit = SelectedInstintuion_Edit.shortnamecn;
                    }

                    if (crmContacts != null && crmContacts.Count > 0)
                    {
                        //SelectedTrader_Edit = crmContacts[0];

                        //TraderName_Edit = SelectedTrader_Edit.namecn;
                    }

                }
            }));
        }

        internal void FilterCounterPartySubName(AutoCompleteArgs args)
        {
            string currentInstID = this.CounterPartyNameItem == null ? null : this.CounterPartyNameItem.idbkey;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = CommonUtils.GetContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = STC.PT.Plugin.Utils.CommonUtils.GetContactInfoPo(args.Pattern.Trim(), currentInstID);
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
                    result = STC.PT.Plugin.Utils.CommonUtils.GetContactInfoPo(null, currentInstID);
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

        internal void FilterShenzhenPartySubName(AutoCompleteArgs args)
        {
            if (string.IsNullOrEmpty(SearchText) || !SearchText.ToUpper().EndsWith(".SZ"))
            {
                return;
            }
            List<CrmContact> result = new List<CrmContact>();
            {
                result = STC.PT.Plugin.Utils.CommonUtils.GetContactInfoPoByInstname(CounterPartyNameText, false);
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                // 排序
                if (result != null)
                {
                    result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
                }
            }
            var ptList = result.Select(c => new CrmContactPT
            {
                 id = c.id,
                 namecn = c.namecn,
                 instname = c.instname
            }).ToList<CrmContactPT>();
            args.DataSource = ptList;
        }

        private void ConfirmAndAddClick()
        {
            execDic.Clear();
            warnReplyDic.Clear();
            if (!check())
                return;
            PtExecOrderSaveReq req = getPtExecOrderSaveReq();
            ThreadPool.QueueUserWorkItem(o =>
            {
                if ("0".Equals(IsAdd))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        /*req.counterpartyid = PTExecOrderProcesser.Instance.partyId;
                        req.counterpartyname = PTExecOrderProcesser.Instance.partyName;*/
                        if (IsEnableParse)
                        {
                            req.allocText = AllocText;
                        }
                        if (!string.IsNullOrEmpty(Price))
                            req.price = Price;
                        req.partysubname = LocalEnv.UserInfo.FullName;
                        req.partysubid = LocalEnv.UserInfo.UserId;
                        req.partyid = PTExecOrderProcesser.Instance.partyId;
                        req.partyname = PTExecOrderProcesser.Instance.partyName;
                        req.traderid = LocalEnv.UserInfo.UserName;
                        req.tradername = LocalEnv.UserInfo.FullName;
                        PResult result = PTHandler.Instance.ExecOrderSave(req);
                        if (result != null)
                        { 
                            //req.execid = result.RequestID;
                            if ("10000".Equals(result.Code))
                            {
                                bool isProhibitory = false;
                                List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(result.Msg);
                                foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                                {
                                    if (placeOrderResult.Code == "10002")
                                    {
                                        isProhibitory = true;
                                        break;
                                    }
                                }
                                warnReplyDic[result.RequestID] = orderRisks;
                                execDic[result.RequestID] = req;
                                BatchWarnView batchWarnView = new BatchWarnView(warnReplyDic, null, execDic, false, isProhibitory);
                                batchWarnView.Owner = myWindow;
                                bool? isSuccess = batchWarnView.ShowDialog();
                                if (isSuccess == true)
                                {
                                    //this.sendIntentionQuote(req);
                                    showIntentionQuote(req);
                                }
                            }
                            else 
                            {
                                //this.sendIntentionQuote(req);
                                showIntentionQuote(req);
                            }
                        }
                        else
                        {
                            showMessage(string.Format("{0}失败{1}", "新增", result != null ? result.Msg : ""), false);
                        }
                    }));
                }
            });
        }

        private void showIntentionQuote(PtExecOrderSaveReq req)
        {
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = getIntentionQuoteInfo(req);
            //Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo intentionResult = IntentionOldHandler.Instance.InsertIntentionQuote_Common(request);
            OpenMatchIntention(req.execid, request);
            CloseWindow();
        }

        private void sendIntentionQuote(PtExecOrderSaveReq req) 
        {
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = getIntentionQuoteInfo(req);
            Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo intentionResult = IntentionOldHandler.Instance.InsertIntentionQuote_Common(request);

            if (intentionResult != null )
            {
                if (intentionResult.Code == "10000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string riskCheckResult = string.Empty;
                        string riskCheckDetail = string.Empty;
                        if (intentionResult != null && intentionResult.Datas != null && intentionResult.Datas.Count > 0)
                        {
                            foreach (Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in intentionResult.Datas)
                            {
                                if (JsonSplit.IsJson(obj.info5))
                                {
                                    NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);

                                    riskCheckResult = info5tag.RISK_CHECK_RESULT;
                                    riskCheckDetail = info5tag.RISK_CHECK_DETAIL;
                                }
                            }
                        }

                        BaseReplyPo reply = new BaseReplyPo();
                        reply.Code = intentionResult.Code;
                        reply.Msg = riskCheckDetail;

                        NewOrderSingle nos = new NewOrderSingle();
                        nos.SecurityID = req.securityid;
                        nos.OrderQty = req.qty;
                        nos.Side = req.side;
                        SendPriceWarnView warnView = new SendPriceWarnView(reply, nos, request);

                        warnView.ShowDialog();
                    }));
                }
                else if (intentionResult.Code == "0000")
                {
                    OpenMatchIntention(req.execid);
                    CloseWindow();
                }
                else 
                {
                    string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败："+(!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        showMessage(errMsg, false);
                    }
                    CloseWindow();
                }
            }
            else
            {
                string errMsg = intentionResult == null ? $"创建个需失败" : "创建个需失败："+(!intentionResult.IsSuccess() ? intentionResult.Msg : string.Empty);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    showMessage(errMsg, false);
                }
                CloseWindow();
            }
        }
        private void OpenMatchIntention(string execid, STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = null)
        {
            PtExecOrderQueryReq req = new PtExecOrderQueryReq();
            req.execid = execid;
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                PtExecordersReplyPo reply = PTHandler.Instance.ExecOrderQuery(req);
                if (reply != null && reply.Datas != null && reply.Datas.Count() > 0) 
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        PtExecordersData po = new PtExecordersData();
                        TbookPtExecorders order = reply.Datas[0];
                        po.ExecID = order.Execid;
                        po.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                        po.MatchQty = Common.Utils.CommonUtils.ConvertToString(order.Matchqty);
                        po.ExecID = order.Execid;
                        po.SecurityID = order.Securityid;
                        po.Symbol = order.Symbol;
                        po.Side = order.Side;
                        po.Sendstatus = order.Sendstatus;
                        po.LastPx = order.Price.HasValue ? order.Price.Value + "" : "";
                        po.NetPrice = order.Netprice.HasValue ? order.Netprice.Value + "" : "";
                        po.Yieldtoexecution = order.Yieldtoexecution.HasValue ? order.Yieldtoexecution.Value + "" : "";
                        MatchIntentionView view = new MatchIntentionView(po, CommonUtils.IsInvestor(), true);
                        WindowManager.Instance.OpenWindow("MatchIntention_" + execid, view);
                    }));
                }
            });
        }
        private STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq getIntentionQuoteInfo(PtExecOrderSaveReq req) 
        {
            STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request = new STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq();
            request.userName = LocalEnv.UserInfo.UserName;
            string quotetype = "INTENTION";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                quotetype = "BUYER_INTENTION";
            }
            request.quoteType = quotetype;
            request.quoteList = new List<IntentionQuoteInfo>();
            IntentionQuoteInfo record = new IntentionQuoteInfo();
            record.counterPartyID = req.counterpartyid;
            record.counterPartyName = req.counterpartyname;
            record.counterPartySubID = req.counterpartysubid;
            record.counterPartySubName = req.counterpartysubname;
            record.traderId = req.traderid;
            record.traderName = req.tradername;
            record.side = req.side;
            record.securityId = req.securityid;
            record.symbol = req.symbol;
            record.price = CommonUtils.ConvertToDecimalOrNull(req.price);
            record.priceType = "3";
            record.cleanPrice = CommonUtils.ConvertToDecimalOrNull(req.netprice);
            record.qty = CommonUtils.ConvertToDecimalOrNull(req.qty);
            record.settlType = string.Format("{0}{1}", TradeDate.Value.ToString("MM.dd"), req.settltype);
            record.userName = req.username;
            record.closeBy = LocalEnv.UserInfo.UserName;
            record.closeByName = LocalEnv.UserInfo.FullName;
            record.status = "0";
            record.channel = "CLIENT";
            record.clordId = DataTimeSequence.Instance.GetId();

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                if (CommonUtils.IsInvestor())
                {
                    request.actionFlag = "0";
                    record.partyManager = LocalEnv.UserInfo.UserName;
                    record.partyManagerName = LocalEnv.UserInfo.FullName;
                    List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                    List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId);
                    if (traderList != null)
                    {
                        List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                        if (list != null && list.Count > 0)
                        {
                            StcTbookUsers stcTbookUsers = list[0];
                            record.partyTrader = stcTbookUsers.Username;
                            record.partyTraderName = stcTbookUsers.Name;
                        }
                    }
                }
                else
                {
                    request.actionFlag = "2";
                    record.partyTrader = LocalEnv.UserInfo.UserName;
                    record.partyTraderName = LocalEnv.UserInfo.FullName;
                }
                request.riskManualCk = "1";
            }
            if (!string.IsNullOrEmpty(req.execid)) 
            {
                /*string execidjson = "{{\"execid\":\"{0}\"}}";
                record.info2 = string.Format(execidjson,req.execid);*/
            }
            record.institutionId = LocalEnv.UserInfo.Termids;
            record.institutionName = LocalEnv.UserInfo.biztype;
            record.userName = LocalEnv.UserInfo.UserName;
            request.quoteList.Add(record);
            return request;
        }

        private PtExecOrderSaveReq getPtExecOrderSaveReq() 
        {
            PtExecOrderSaveReq req = new PtExecOrderSaveReq();
            req.securityid = SearchText;
            req.symbol = Symbol;
            req.symboltype = SymbolType;
            req.venues = ExecordersData != null ? ExecordersData.Venues: "MANUAL";
            req.side = SideIndex == 0 ? "1" : "2";
            if (!string.IsNullOrEmpty(Price))
            {
                req.yield = Price;
                req.price = Price;
            }
            if (!string.IsNullOrEmpty(ExePrice))
            {
                req.exePrice = ExePrice;
            }
            if (!string.IsNullOrEmpty(NetPrice))
            {
                decimal result = decimal.Zero;
                if (decimal.TryParse(NetPrice, out result))
                {
                    req.netprice = CommonUtils.ConvertToString(result);
                    if (IsCTP)
                    {
                        req.price = req.netprice;
                    }
                }
            }
            if (!string.IsNullOrEmpty(FullPrice))
            {
                decimal result = decimal.Zero;
                if (decimal.TryParse(FullPrice, out result))
                {
                    req.fullPrice = CommonUtils.ConvertToString(result);
                }
            }
            if (!string.IsNullOrEmpty(Qty))
            {
                decimal result = decimal.Zero;
                if (decimal.TryParse(Qty, out result))
                    req.qty = CommonUtils.ConvertToString(result);
            }
            req.realcounterpartyname = RealcounterpartynameText;

            req.tradedate = TradeDate.HasValue ? TradeDate.Value.ToString("yyyyMMdd") : "";//"yyyyMMdd"

            if (IsIRS || IsCTP)
            {
                req.settltype = "+0";
            }
            else
            {
                req.settltype = SettlTypeIndex == 1 ? "+0" : "+1";
            }
            req.trdSubType = OrderSide_Text;
            req.trdType = OrderMethod_Text;
            CrmInst crmInst = CounterPartyNameItem;
            if (crmInst != null)
            {
                req.counterpartyid = crmInst != null ? crmInst.idbkey : "";
                req.counterpartyname = crmInst != null ? crmInst.shortnamecn : "";
                req.counterpartyfullname = crmInst != null ? crmInst.fullnamecn : "";
            }
            else
            {
                req.counterpartyname = CounterPartyNameText;
            }
            if (!string.IsNullOrEmpty(CounterPartySubNameText))
            {
                req.counterpartysubname = CounterPartySubNameText;
            }
            req.bridgePaymentFixedRate = PaymentRate;
            req.cleanPrice2 = PaymentNetPrice;
            req.fullPrice2 = PaymentFullPrice;
            req.couponrate = Couponrate;
            req.settlAmount = CommonUtils.ConvertToString(SettlAmount);
            req.yield2 = PaymentYield;
            req.trdMatchId = TrdMatchId;
            req.exePrice2 = PaymentExePrice;
            req.remark = Remark;
            req.sseinstitutioncode = ShanghaiInstCode;
            req.szedealercode = ShenzhenInstCode;
            req.szetradingentitycode = ShenzhenTradingPart;
            req.szetradercode = ShenzhenPartySubNameText;
            if ("0".Equals(PartySubNameIsReadOnly))
            {
                req.partysubname = PartySubName;
                if (PartySubNameItem != null)
                {
                    req.partysubid = PartySubNameItem.Userid;
                    req.name = PartySubNameItem.Name;
                    req.username = PartySubNameItem.Username;
                    req.userid = PartySubNameItem.Userid;
                }
            }
            return req;
        }
        internal void FilterPartySubName(AutoCompleteArgs args)
        {
            if ("0".Equals(IsManual) || ExecordersData == null) 
            {
                return;
            }
            string toqq = this.ExecordersData.Toqq;
            List<StcTbookUsers> result = new List<StcTbookUsers>();
            {
                result = CommonUtils.GetUsersInfoByToqq(toqq);
            }

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                string condition = args.Pattern.Trim();
                result = result.Where(d => string.IsNullOrEmpty(condition) || (!string.IsNullOrEmpty(condition)
                                            && ((d.Username != null && d.Username.Contains(condition))
                                            || (d.Name != null && d.Name.Contains(condition))))).ToList();   
                
            }
            args.DataSource = result;
        }

        internal void FilterShanghaiInst(AutoCompleteArgs args,string param)
        {
            /*if (this.CounterPartyNameItem == null) 
            {
                return;
            }*/
            if (string.IsNullOrEmpty(SearchText) || !SearchText.ToUpper().EndsWith(param))
            {
                return;
            }
            List<CrmInst> results = STC.PT.Plugin.Utils.CommonUtils.GetInstitutionsInfoPo(_CounterPartyNameText, 2, null);
            args.DataSource = results;
        }
    }
}
