using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// ManualEntry.xaml 的交互逻辑
    /// </summary>
    public partial class ManualEntry : WindowBase
    {
        private ManualEntryViewModel _manualEntryViewModel = new ManualEntryViewModel();
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }
        public PtExecordersData drv { get; set; }
        public DataRowView OrderDrv { get; set; }
        public bool isSales { get; set; }
        public string symboltype;
        public bool IsAdd = true;

        private bool _isSendFQ = false;
        public bool IsSendFQ
        {
            get { return _isSendFQ; }
            set 
            {
                _isSendFQ = value;
                if (_isSendFQ == true)
                {
                    btnOK.Visibility = Visibility.Collapsed;
                    btnSend.Visibility = Visibility.Visible;
                }
                else
                {
                    btnOK.Visibility = Visibility.Visible;
                    btnSend.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static List<KeyValuePair<string, string>> TagItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("tag1_pt");
            }
        }
        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("side_pt");
            }
        }
        public static List<KeyValuePair<string, string>> VenueItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("venues_pt");
            }
        }
        public static List<KeyValuePair<string, string>> QuotemethodItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("quotemethod_pt");
            }
        }

        public static List<KeyValuePair<string, string>> BusinessTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("businesstype");
            }
        }
        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        Dictionary<string, string> VenueGW = new Dictionary<string, string>();
        KeyValuePair<string, string> TagZ = new KeyValuePair<string, string>("", "");
        public ManualEntry()
        {
            InitializeComponent();
            //comBook.ItemsSource = PTExecOrderProcesser.Instance.BookDict;
            this.Loaded += ManualEntry_Loaded;
            this.DataContext = _manualEntryViewModel;
            VenueGW.Add("ODM", "ODM");
            VenueGW.Add("TPSC", "SOR");


            VenueGW.Add("CTTJ", "SOR");
            VenueGW.Add("CNEX", "SOR");
            VenueGW.Add("CCTB", "SOR");
            VenueGW.Add("PTCN", "SOR");
            VenueGW.Add("UEDA", "SOR");
            VenueGW.Add("MANUAL", "INNER");
            VenueGW.Add("QDMESP", "QDMESP");
            //VenueGW.Add("MANUAL", "TradeBook");

            if (!TagItemSource.Contains(TagZ))
                TagItemSource.Insert(0, TagZ);
            comLable.ItemsSource = TagItemSource;

            //comSide.ItemsSource = SideItemSource;
            comVenue.ItemsSource = VenueItemSource;
            comWay.ItemsSource = QuotemethodItemSource;
            comBusinessType.ItemsSource = BusinessTypeItemSource;
            //初始禁用
            comLable.IsEnabled = false;
            AccountUserCbc.IsEnabled = false;
            comVenue.IsEnabled = false;
            //txInst.IsEnabled = false;
            //txDelear.IsEnabled = false;
            AccountSP.Visibility = Visibility.Collapsed;
            //CouterSP.Visibility = Visibility.Collapsed;

            //ptGrid.RowDefinitions[8].Height = new GridLength(0);
            //ptGrid.RowDefinitions[9].Height = new GridLength(0);
            //GetAllBook();
        }

        private void ManualEntry_Loaded(object sender, RoutedEventArgs e)
        {
            if (drv != null)
                SetValue(drv);
            else if (OrderDrv != null)
                SetOrderValue(OrderDrv);
            else
            {
                _manualEntryViewModel.defaultAccountID = String.Empty;
                _manualEntryViewModel.InitAccountUsers();
                _manualEntryViewModel.SideIndex = 1;
                _manualEntryViewModel.QuoteWay = "1";//默认发请求
                _manualEntryViewModel.TradeDate= DateTime.Now;
                _manualEntryViewModel.SettlTypeIndex = 0;
                lbOpenClose.SelectedIndex = 0;
                _manualEntryViewModel.OpenSelected = true;
                _manualEntryViewModel.ParseManualOrderDefaultBookId(LocalEnv.ManualOrderDefaultBookId);
                CounterInsNameCb_edit.Text = PTExecOrderProcesser.Instance.partyName;
                ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
                _manualEntryViewModel.MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
            }
            if (isSales)
            {
                InsNametb.Visibility = Visibility.Visible;
                InsNameCb_edit.Visibility = Visibility.Visible;
                TrdNametb.Visibility = Visibility.Visible;
                TrdNameCb_edit.Visibility = Visibility.Visible;
            }
            else
            {
                InsNametb.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNametb.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
            }
        }
        private PtExecordersData CurrentDrv;
        public void SetValue(PtExecordersData drv)
        {
            CurrentDrv = drv;
            _manualEntryViewModel.isSetValue = true;

            if (IsAdd)
            {
                ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
                _manualEntryViewModel.MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
                _manualEntryViewModel.ParseManualOrderDefaultBookId(LocalEnv.ManualOrderDefaultBookId);
            }
            else 
            {
                ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(drv.Username);
                _manualEntryViewModel.MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
                _manualEntryViewModel.ParseManualOrderDefaultBookId(drv.Bookid);
            }
            Search_TextBox.SearchText = drv.SecurityID + "";
            txSymbol.Text = drv.Symbol + "";
            Search_TextBox.IsEnabled = false;
            txSymbol.IsEnabled = false;
            symboltype = drv.Symboltype + "";
            if (!string.IsNullOrEmpty(drv.LastPx + ""))
            {
                decimal price;
                if (decimal.TryParse(drv.LastPx + "", out price))
                {
                    txPrice.Text = price.ToString("N4");
                }
            }
            if (!string.IsNullOrEmpty(drv.LastQty + ""))
            {
                decimal lastQty = decimal.Zero;
                if (decimal.TryParse(drv.LastQty + "", out lastQty))
                {
                    txQty.Text = (lastQty / 1000).ToString("N3");
                    if (_manualEntryViewModel.QtyTxt.Contains("k"))
                        txQty.Text = (lastQty / 1000) + "";//.ToString("N3");
                    else
                        txQty.Text = lastQty + "";
                }
            }
            if (!string.IsNullOrEmpty(drv.NetPrice + ""))
            {
                decimal netPrice;
                if (decimal.TryParse(drv.NetPrice + "", out netPrice))
                {
                    txNetPrice.Text = netPrice.ToString("N4");
                }
            }

            //string side = drv[PTExecOrderProcesser.Side] + "";
            //if (PTExecOrderProcesser.Instance.sideDic.ContainsValue(side))
            //    comSide.Text = PTExecOrderProcesser.Instance.sideDic.FirstOrDefault(q => q.Value == side).Key;
            //comSide.SelectedValue = drv.Side + "";
            SideListBox.SelectedIndex = string.IsNullOrEmpty(drv.Side) ? -1 : "1".Equals(drv.Side) ? 0 : 1;
            if (!string.IsNullOrEmpty(drv.TradeDate))
            {
                datePicker.SelectedDate = DateTime.ParseExact(drv.TradeDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            SettlTypeListBox.SelectedIndex = string.IsNullOrEmpty(drv.SettlType) ? -1 : "+1".Equals(drv.SettlType) ? 0 : 1;
            comVenue.SelectedValue = drv.Venues + "";

            //txInst.Text = drv.CounterPartyName + "";//txInst.Text;//"平安银行";
            //txDelear.Text = drv.CounterPartySubName + "";//txDelear.Text;//"payhdealer2";
            InsNameCb_edit.Text = drv.InstitutionName + "";
            TrdNameCb_edit.Text = drv.TraderName + "";
            if (string.IsNullOrEmpty(drv.TraderID) == false)
            {
                List<CrmContact> crmContractList = CommonUtils.GetContactInfoPo(drv.TraderID, null);
                if (crmContractList != null && crmContractList.Count > 0)
                {
                    TrdNameCb_edit.SelectedItem = crmContractList[0];
                }
            }

            comLable.SelectedValue = drv.Tag1 + "";
            //string quote = drv[PTExecOrderProcesser.Quotemethod] + "";
            //if (PTExecOrderProcesser.Instance.sideDic.ContainsValue(quote))
            //    comWay.Text = PTExecOrderProcesser.Instance.wayDic.FirstOrDefault(q => q.Value == quote).Key;
            comWay.SelectedValue = drv.Quotemethod + "";
            //AccountUserCbc.SelectedItem = drv[PTExecOrderProcesser.AccountName] + "";
            AccountUserItem item = new AccountUserItem();
            item.Id = drv.AccountId + "";
            item.AccountName = drv.AccountName + "";
            //_manualEntryViewModel.AccountUserItem = item;
            //AccountUserCbc.SelectedItem = item;
            _manualEntryViewModel.defaultAccountID = item.Id;
            _manualEntryViewModel.InitAccountUsers();
            txSeatNumber.Text = drv.Seatnumber + "";
            txCommnet.Text = drv.Remark + "";
            string settle = drv.SettlType + "";
            _manualEntryViewModel.SettleType = settle;
            _manualEntryViewModel.ParseSettle(settle);
            _manualEntryViewModel.isSetValue = false;

            if (_manualEntryViewModel.IsBank)//true-交易所
            {
                comWay.Visibility = Visibility.Collapsed;
                txWay.Visibility = Visibility.Collapsed;
                tbSeatNumber.Visibility = Visibility.Visible;
                txSeatNumber.Visibility = Visibility.Visible;
                Grid.SetColumn(tbCommnet, 0);
                Grid.SetColumn(txCommnet, 1);
                tbCommnet.Text = "约定号:";
            }
            else
            {
                tbCommnet.Text = "备注:";
                comWay.Visibility = Visibility.Visible;
                txWay.Visibility = Visibility.Visible;
                tbSeatNumber.Visibility = Visibility.Collapsed;
                txSeatNumber.Visibility = Visibility.Collapsed;
            }
            txSecurityid.Visibility = Visibility.Visible;
            Search_TextBox.Visibility = Visibility.Collapsed;
            comBusinessType.SelectedValue = drv.BusinessType;
            //comBook.SelectedValue = drv.Bookid;
            CounterInsNameCb_edit.Text = drv.CounterPartyName + "";
            CounterTrdNameCb_edit.Text = drv.CounterPartySubName + "";
            string venues = drv.Venues;
            if (!"MANUAL".Equals(venues) && !"QUANT".Equals(venues) && !"IDB".Equals(venues))
            {
                _manualEntryViewModel.CounterTrdNameEnable = "0";
            }
            else 
            {
                _manualEntryViewModel.CounterTrdNameEnable = "1";
            }
        }

        public void SetOrderValue(DataRowView drv)
        {
            //CurrentDrv = drv;
            ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
            _manualEntryViewModel.MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
            _manualEntryViewModel.isSetValue = true;

            Search_TextBox.SearchText = drv[MyOrderProcesser.SECURITYID_ORDER_FIELD] + "";
            txSymbol.Text = drv[MyOrderProcesser.SYMBOL_ORDER_FIELD] + "";
            Search_TextBox.IsEnabled = false;
            txSymbol.IsEnabled = false;
            symboltype = drv[MyOrderProcesser.SECURITY_TYPE] + "";
            if (!string.IsNullOrEmpty(drv[MyOrderProcesser.PRICE_ORDER_FIELD] + ""))
            {
                decimal price;
                if (decimal.TryParse(drv[MyOrderProcesser.PRICE_ORDER_FIELD] + "", out price))
                {
                    txPrice.Text = price.ToString("N4");
                }
            }
            if (!string.IsNullOrEmpty(drv[MyOrderProcesser.QTY_ORDER_FIELD] + ""))
            {
                decimal lastQty = decimal.Zero;
                if (decimal.TryParse(drv[MyOrderProcesser.QTY_ORDER_FIELD] + "", out lastQty))
                {
                    txQty.Text = (lastQty / 1000).ToString("N3");
                    if (_manualEntryViewModel.QtyTxt.Contains("k"))
                        txQty.Text = (lastQty / 1000) + "";//.ToString("N3");
                    else
                        txQty.Text = lastQty + "";
                }
            }
            if (!string.IsNullOrEmpty(drv[MyOrderProcesser.NETPRICE_ORDER_FIELD] + ""))
            {
                decimal netPrice;
                if (decimal.TryParse(drv[MyOrderProcesser.NETPRICE_ORDER_FIELD] + "", out netPrice))
                {
                    txNetPrice.Text = netPrice.ToString("N4");
                }
            }
            SideListBox.SelectedIndex = string.IsNullOrEmpty(drv[MyOrderProcesser.SIDE_ORDER_FIELD] + "") ? -1 : "1".Equals(drv[MyOrderProcesser.SIDE_ORDER_FIELD] + "") ? 0 : 1;
            
            string settlType = drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] +"";
            if (settlType.Contains('+'))
            {
                string[] settlTypeArray = settlType.Split('+');
                if (settlTypeArray.Length == 2)
                {
                    int year = DateTime.Now.Year; // 或者你需要的年份  

                    string fullDateString = year.ToString() + "." + settlTypeArray[0];
                    _manualEntryViewModel.TradeDate = DateTime.ParseExact(fullDateString, "yyyy.MM.dd", CultureInfo.InvariantCulture);
                    string settl = settlTypeArray[1];
                    SettlTypeListBox.SelectedIndex = string.IsNullOrEmpty(settl + "") ? -1 :
                "1".Equals(settl)? 0 : 1;
                }
            }
            else
            {
                _manualEntryViewModel.TradeDate = DateTime.Now;
                SettlTypeListBox.SelectedIndex = string.IsNullOrEmpty(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] + "") ? -1 :
                    "2,3".Equals(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD])
                    || "2".Equals(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD])
                    || "3".Equals(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD]) ? 0 : 1;
            }
            //comSide.SelectedValue = drv[MyOrderProcesser.SIDE_ORDER_FIELD] + "";
            comVenue.SelectedValue = drv[MyOrderProcesser.VENEUES_ORDER_FIELD] + "";

            //txInst.Text = drv[MyOrderProcesser.CounterPartyName] + "";//txInst.Text;//"平安银行";
            //txDelear.Text = drv[MyOrderProcesser.CounterPartySubName] + "";//txDelear.Text;//"payhdealer2";
            InsNameCb_edit.Text = drv[MyOrderProcesser.INSTITUTIONNAME_FIELD] + "";
            TrdNameCb_edit.Text = drv[MyOrderProcesser.TRADERNAME_FIELD] + "";
            string trdid = drv[MyOrderProcesser.TRADERID_FIELD] + "";
            if (string.IsNullOrEmpty(trdid) == false)
            {
                List<CrmContact> crmContractList = CommonUtils.GetContactInfoPo(trdid, null);
                if (crmContractList != null && crmContractList.Count > 0)
                {
                    TrdNameCb_edit.SelectedItem = crmContractList[0];
                }
            }

            //comLable.SelectedValue = drv[MyOrderProcesser.Tag1] + "";
           
            //comWay.SelectedValue = drv[MyOrderProcesser.Quotemethod] + "";
            comWay.SelectedValue = "1";//默认发请求

            AccountUserItem item = new AccountUserItem();
            item.Id = drv[MyOrderProcesser.AccountID_Field] + "";
            item.AccountName = drv[MyOrderProcesser.AccountName_Field] + "";

            _manualEntryViewModel.defaultAccountID = item.Id;
            _manualEntryViewModel.InitAccountUsers();
            //txCommnet.Text = drv[MyOrderProcesser.Remark] + "";
            string settle = drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] + "";
            _manualEntryViewModel.SettleType = settle;
            _manualEntryViewModel.ParseSettle(settle);
            _manualEntryViewModel.isSetValue = false;
            //禁用
            comLable.IsEnabled = false;
            AccountUserCbc.IsEnabled = false;
            comVenue.IsEnabled = false;
            //if (LocalEnv.UserInfo.RoleName.Contains("销售"))
            //{
            //txInst.IsEnabled = false;
            //txDelear.IsEnabled = false;
            if (_manualEntryViewModel.IsBank)//true-交易所
            {
                comWay.Visibility = Visibility.Collapsed;
                txWay.Visibility = Visibility.Collapsed;
                tbSeatNumber.Visibility = Visibility.Visible;
                txSeatNumber.Visibility = Visibility.Visible;
                Grid.SetColumn(tbCommnet, 0);
                Grid.SetColumn(txCommnet, 1);
                tbCommnet.Text = "约定号:";
            }
            else
            {
                tbCommnet.Text = "备注:";
                comWay.Visibility = Visibility.Visible;
                txWay.Visibility = Visibility.Visible;
                tbSeatNumber.Visibility = Visibility.Collapsed;
                txSeatNumber.Visibility = Visibility.Collapsed;
            }
            txSecurityid.Visibility = Visibility.Visible;
            Search_TextBox.Visibility = Visibility.Collapsed;

            _manualEntryViewModel.ParseManualOrderDefaultBookId(drv[MyOrderProcesser.BOOKID_FIELD] + "");
            string venues = drv[MyOrderProcesser.VENEUES_ORDER_FIELD] + "";
            if (!"MANUAL".Equals(venues) && !"QUANT".Equals(venues) && !"IDB".Equals(venues))
            {
                _manualEntryViewModel.CounterTrdNameEnable = "0";
            }
            else
            {
                _manualEntryViewModel.CounterTrdNameEnable = "1";
            }
        }

        public bool check()
        {
            bool ischeck = false;
            if (string.IsNullOrWhiteSpace(Search_TextBox.SearchText))
            {
                CustomMessageBox.Info("缺少债券!", "录入");
                return ischeck;
            }
            if (SideListBox.SelectedIndex == -1)
            {
                CustomMessageBox.Info("缺少方向!", "录入");
                return ischeck;
            }
            if (!_manualEntryViewModel.IsCTP)
            {
                if (string.IsNullOrWhiteSpace(txPrice.Text))
                {
                    CustomMessageBox.Info("缺少价格!", "录入");
                    return ischeck;
                }
            }
            if (!_manualEntryViewModel.IsIRS)
            {
                if (string.IsNullOrWhiteSpace(txNetPrice.Text))
                {
                    CustomMessageBox.Info("缺少净价!", "录入");
                    return ischeck;
                }
            }
            if (string.IsNullOrWhiteSpace(txQty.Text))
            {
                CustomMessageBox.Info("缺少量!", "录入");
                return ischeck;
            }
            if (_manualEntryViewModel.SettlTypeIndex == -1)
            {
                CustomMessageBox.Info("缺少清算速度!", "录入");
                return ischeck;
            }
            if (_manualEntryViewModel.TradeDate == null || !_manualEntryViewModel.TradeDate.HasValue)
            {
                CustomMessageBox.Info("缺少交易日期!", "录入");
                return ischeck;
            }
            if (isSales)
            {
                if (string.IsNullOrWhiteSpace(_manualEntryViewModel.InstitutionName_Edit))
                {
                    CustomMessageBox.Info("缺少客户机构!", "录入");
                    return ischeck;
                }
                if (string.IsNullOrWhiteSpace(_manualEntryViewModel.TraderName_Edit))
                {
                    CustomMessageBox.Info("缺少客户!", "录入");
                    return ischeck;
                }
            }
            if (_manualEntryViewModel.IsCTP && _manualEntryViewModel.OpenCloseIndex == -1)
            {
                CustomMessageBox.Info("缺少开平标志!", "录入");
                return ischeck;
            }
            ischeck = true;
            return ischeck;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!check())
                return;
            PtExecOrderSaveReq req = new PtExecOrderSaveReq();
            req.securityid = Search_TextBox.SearchText;
            req.symbol = txSymbol.Text;
            req.symboltype = symboltype;
            if (!string.IsNullOrEmpty(txPrice.Text))
            {
                req.yield = txPrice.Text;
                req.price = txPrice.Text;
            }
            if (!string.IsNullOrEmpty(txQty.Text))
            {
                decimal result = decimal.Zero;
                if (decimal.TryParse(txQty.Text, out result))
                    req.qty = CommonUtils.ConvertToString(result * 1000);
                if (_manualEntryViewModel.QtyTxt.Contains("k"))
                    req.qty = CommonUtils.ConvertToString(result * 1000);
                else
                    req.qty = CommonUtils.ConvertToString(result);
            }
            if (!string.IsNullOrEmpty(txNetPrice.Text))
            {
                decimal result = decimal.Zero;
                if (decimal.TryParse(txNetPrice.Text, out result))
                {
                    req.netprice = CommonUtils.ConvertToString(result);
                    if (_manualEntryViewModel.IsCTP) 
                    {
                        req.price = req.netprice;
                    }
                }
            }
            req.side = SideListBox.SelectedIndex == 0 ? "1" : SideListBox.SelectedIndex == 1 ? "2" : "";//PTExecOrderProcesser.Instance.sideDic.ContainsKey(comSide.Text) ? PTExecOrderProcesser.Instance.sideDic[comSide.Text] : comSide.Text;
            req.tradedate = datePicker.SelectedDate.HasValue?datePicker.SelectedDate.Value.Date.ToString("yyyyMMdd") : "";
            req.venues = comVenue.SelectedValue + "";
            req.venuetypegw = VenueGW.ContainsKey(req.venues) ? VenueGW[req.venues] : "";
            if (isSales)
            {
                CrmInst crmInst = _manualEntryViewModel.SelectedInstintuion_Edit;
                CrmContact crmContact = _manualEntryViewModel.SelectedTrader_Edit;
                if (crmContact != null)
                {
                    req.institutionid = crmInst != null ? crmInst.idbkey : "";//crmContact.instid
                    req.institutionname = crmInst != null ? crmInst.shortnamecn : "";//crmContact.instname;
                    req.traderid = crmContact.idbkey;
                    req.tradername = crmContact.namecn;
                }
                else if (!string.IsNullOrEmpty(_manualEntryViewModel.InstitutionName_Edit) || !string.IsNullOrEmpty(_manualEntryViewModel.TraderName_Edit))
                {
                    req.institutionid = drv.InstitutionID;
                    req.institutionname = _manualEntryViewModel.InstitutionName_Edit;
                    req.traderid = drv.TraderID;
                    req.tradername = _manualEntryViewModel.TraderName_Edit;
                }
                else
                {
                    req.institutionid = "";
                    req.institutionname = "";
                    req.traderid = "";
                    req.tradername = "";
                }
            }
            req.tag1 = comLable.SelectedValue + "";
            if (!_manualEntryViewModel.IsBank && !_manualEntryViewModel.IsCTP)
            {
                req.quotemethod = comWay.SelectedValue + "";
            }
            if (_manualEntryViewModel.IsIRS || _manualEntryViewModel.IsCTP)
            {
                req.settltype = "+0";
            }
            else
            {
                req.settltype = SettlTypeListBox.SelectedIndex == 0 ? "+1" : SettlTypeListBox.SelectedIndex == 1 ? "+0" : "";
            }
            req.name = LocalEnv.UserInfo.FullName;
            req.username = LocalEnv.UserInfo.UserName;
            req.remark = txCommnet.Text;
            if (_manualEntryViewModel.AccountUserItem != null)
            {
                req.accountid = _manualEntryViewModel.AccountUserItem.Id;
                req.accountname = _manualEntryViewModel.AccountUserItem.AccountName;
            }
            else
            {
                req.accountid = "";
                req.accountname = "";
            }
            if (_manualEntryViewModel.IsBank)
            {
                req.seatnumber = txSeatNumber.Text;
            }
            req.businessType = comBusinessType.SelectedValue + "";
            //组合
            req.bookid = _manualEntryViewModel.SelectedItem;
            req.bookname = _manualEntryViewModel.SelectedText;
            StcTbookUsers stcTbook = _manualEntryViewModel.SelectedCounterUser_Edit;
            if (stcTbook != null)
            {
                req.counterpartysubid = stcTbook.Userid;
                req.counterpartysubname = stcTbook.Name;
            }
            else if (!string.IsNullOrEmpty(_manualEntryViewModel.CounterUserName_Edit))
            {
                req.counterpartysubid = drv.CounterPartySubId;
                req.counterpartysubname = drv.CounterPartySubName;
            }
            else 
            {
                req.counterpartysubid = "";
                req.counterpartysubname = "";
            }
            if (_manualEntryViewModel.IsCTP) 
            {
                req.offsetflag = _manualEntryViewModel.OpenCloseIndex == 1 ? "1" : "0";
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                if (IsAdd || OrderDrv != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ("克隆成交".Equals(this.Title))
                        {
                            req.venueexecid = drv.Venueexecid;
                            req.userid = LocalEnv.UserInfo.UserId;
                            req.counterpartyid = drv.CounterPartyId;
                            req.counterpartyname = drv.CounterPartyName;
                        }
                        else
                        {
                            req.counterpartyid = PTExecOrderProcesser.Instance.partyId;
                            req.counterpartyname = PTExecOrderProcesser.Instance.partyName;
                            if (OrderDrv != null) 
                            {
                                req.userid = LocalEnv.UserInfo.UserId;
                            }
                        }
                        if (!string.IsNullOrEmpty(txPrice.Text))
                        req.price = txPrice.Text;
                        req.strategyorderid = OrderDrv != null ? OrderDrv[MyOrderProcesser.STCCODE_ORDER_FIELD] + "" : "";
                        req.partysubname = LocalEnv.UserInfo.FullName;
                        req.partysubid = LocalEnv.UserInfo.UserId;
                        req.partyid = PTExecOrderProcesser.Instance.partyId;
                        req.partyname = PTExecOrderProcesser.Instance.partyName;
                        PResult result = PTHandler.Instance.ExecOrderSave(req);
                        if (result != null && "0000".Equals(result.Code))
                        {
                            this.Result = true;
                            this.Close();
                            e.Handled = true;
                            if (OrderDrv != null)
                            {
                                DeleteOrder(OrderDrv);
                                ClipboardHelper.CopyToClipboard(_manualEntryViewModel.CopyTxt.Replace(" ", PTInnerSettingViewModel.Instance.TabStr), true);
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.setWarnMsg("交易已成功，要素已复制", true);
                                msgBox.ShowDialog();

                            }
                            if ("克隆成交".Equals(this.Title)) 
                            {
                                showMessage("交易已克隆成功",true);
                            }
                        }
                        else
                        {
                            showMessage(string.Format("{0}失败{1}",this.Title, result != null ? result.Msg : ""), false);
                        }
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        req.execid = drv.ExecID + "";
                        PtExecUpdateReplyPo result = PTHandler.Instance.ExecOrderUpdate(req);
                        if (result != null && "0000".Equals(result.Code))
                        {
                            Dictionary<string, string> Datas = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Datas);
                            if (Datas != null && Datas.Count > 0)
                            {
                                StringBuilder builder = new StringBuilder();
                                foreach (string value in Datas.Values)
                                {
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        builder.Append(value).Append("\n");
                                    }
                                }
                                if (builder.Length > 0)
                                {
                                    showMessage(string.Format("{0}", builder.ToString()),false);
                                }
                            }
                            this.Result = true;
                            this.Close();
                            e.Handled = true;
                        }
                        else
                        {
                            showMessage(string.Format("修改失败{0}", result != null ? result.Msg : ""),false);
                        }
                    }));
                }
            });
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message,bool autoClose)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message, autoClose);
            me.ShowDialog();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }


        public async void DeleteOrder(DataRowView SelectedItem_MyOrders)
        {
            if (SelectedItem_MyOrders != null
                && SelectedItem_MyOrders.Row.RowState != DataRowState.Detached)
            {
                //bool result = Common.View.Component.CustomMessageBox.Question("是否撤销该笔报价？", "删除订单");
                //if (result == true)
                //{
                string algoname = CommonUtils.ConvertToString(SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                if (algoname == "DQ")
                {
                    //定向报价撤单, 如果是自己撤自己的DQ 则Info == 4
                    STC.Common.Model.Fix.Message.OrderCancelRequest rs = Common.ViewModel.Drp.Utils.DRPTools.GetCancelOrder(SelectedItem_MyOrders);
                    rs.Info1 = "4";
                    STC.Common.Model.Fix.Message.PResult res = await Common.ViewModel.Drp.Utils.DRPTools.CancelOrder(rs);
                    if (res != null)
                    {
                        if (res.Code == "0000")
                        {
                        }
                        else
                        {
                            Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{res.Msg}."), "删除订单");
                        }
                    }
                }
                else
                {
                    STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                    cancelPo.AlgoName = CommonUtils.ConvertToString(SelectedItem_MyOrders[MyOrderProcesser.ALGONAME_ORDER_FIELD]);
                    cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                    cancelPo.OrderID = CommonUtils.ConvertToString(SelectedItem_MyOrders[MyOrderProcesser.STCCODE_ORDER_FIELD]);
                    cancelPo.Rejecter = "MANUAL";
                    cancelPo.CxlRejReason = "6007";
                    cancelPo.RejectText = _manualEntryViewModel.CopyTxt;
                    TBookCancelReplyPo reply = await CancelOrder(cancelPo);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            //this.SelectedItem_MyOrders[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        }
                        else
                        {
                            Common.View.Component.CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                        }
                    }
                }
                // }
            }
        }

        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj == null)
                return;
            txSymbol.Text = obj.Short_Name;
            Common.Model.Base.BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(obj.Bond_ID);
            if (bondsInfoPo != null)
                symboltype = bondsInfoPo.SECURITY_TYPE;
            if (_manualEntryViewModel.IsBank)//true-交易所
            {
                comWay.Visibility = Visibility.Collapsed;
                txWay.Visibility = Visibility.Collapsed;
                Grid.SetColumn(tbCommnet, 0);
                Grid.SetColumn(txCommnet, 1);
                tbCommnet.Text = "约定号:";
                tbSeatNumber.Visibility = Visibility.Visible;
                txSeatNumber.Visibility = Visibility.Visible;
            }
            else
            {
                if (_manualEntryViewModel.IsCTP)
                {
                    comWay.Visibility = Visibility.Collapsed;
                    txWay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    comWay.Visibility = Visibility.Visible;
                    txWay.Visibility = Visibility.Visible;
                }
                tbCommnet.Text = "备注:";
                tbSeatNumber.Visibility = Visibility.Collapsed;
                txSeatNumber.Visibility = Visibility.Collapsed;
            }
        }

       


        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                _manualEntryViewModel.FilterInstitution(args);
            }
        }
        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    _manualEntryViewModel.FilterTraderLocal(args);
                }
                else
                {
                    _manualEntryViewModel.FilterTrader(args);
                }
            }
        }

        private void InsNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (_manualEntryViewModel.SelectedInstintuion_Edit == null
            //   && !string.IsNullOrEmpty(_manualEntryViewModel.InstitutionName_Edit))
            //{
            //    _manualEntryViewModel.InstitutionName_Edit = string.Empty;
            //}
        }

        private void TrdNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (_manualEntryViewModel.SelectedTrader_Edit == null
            //    && !string.IsNullOrEmpty(_manualEntryViewModel.TraderName_Edit))
            //{
            //    _manualEntryViewModel.TraderName_Edit = string.Empty;
            //}
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(InsNameCb_edit.Text))
            {
                InsNameCb_edit.ExpandOptions();
            }
        }

        private void TrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(TrdNameCb_edit.Text))
            {
                TrdNameCb_edit.ExpandOptions();
            }
        }

        private void Search_TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {

        }

        #region 校验
        private void TxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途TxNetPrice_PreviewKeyDown
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "txPrice") || (tb.Name == "txNetPrice"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            //else if (tag == "4")//随机渠道
            //{
            //    max = viewModel.RandomMax;
            //    min = 1;
            //}

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }
        #endregion

        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                Button btn = (Button)sender;
                Grid g = btn.Parent as Grid;
                if (g != null)
                {
                    Popup popup = g.FindName("PART_Popup") as Popup;
                    popup.HorizontalOffset = 103;
                }
            }
        }

        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //SettlTypeCombox.IsDropDownOpen = !SettlTypeCombox.IsDropDownOpen;
            //if (_manualEntryViewModel != null)
            //{
            //    _manualEntryViewModel.SettleBoxIndex = -1;
            //    _manualEntryViewModel.AddOneOpenedTag = "1";
            //}
        }

        private void TxNetPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "PriceTxt") || (tb.Name == "netPrice"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            //if (tag == "1" || tag == "3" || tag == "4")//量
            //    regex = new Regex("^[0-9]+$");
            //else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            //else if (tag == "4")//随机渠道
            //{
            //    max = viewModel.RandomMax;
            //    min = 1;
            //}

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }

        private void TxNetPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            // 增加 ↑↓来改变价格
            if (tBox.Name.ToString() == "PriceTxt")
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                   // viewModel.AddPriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                   // viewModel.ReducePriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "AmountTxt")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    //viewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    //viewModel.ReduceAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                //AmountTxt.Focus();
                //// 选择最后位数
                //int pos = AmountTxt.Text.Length;
                //if (pos > 0)
                //{
                //    int sellength = AmountTxt.Text.Length;
                //    AmountTxt.SelectionStart = 0;
                //    AmountTxt.SelectionLength = sellength;
                //}
                e.Handled = true;
                return;
            }
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }


                // tag 改变用途
                if (tBox.Name == "AmountTxt")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "PriceTxt") || (tBox.Name == "netPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
                }
                else if (tag == "2")//价格
                {
                    max = 9999;
                    min = -100;
                }
                else if (tag == "3")//过期时间
                {
                    max = 120;
                    min = 1;
                }
                //else if (tag == "4")//随机渠道
                //{
                //    max = viewModel.RandomMax;
                //    min = 1;
                //}

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        //public bool IsBrokerSecurity = true;
        private void TxQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex("^[0-9.-]+$");

            decimal max = 9999.99M;
            decimal min = -9999.99M;

            if (_manualEntryViewModel.IsIRS)//量
            {
                regex = new Regex("^[0-9]+$");

                max = 1000;
                min = 1;
            }
            else
                regex = new Regex("^[0-9.-]+$");

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".") && (_manualEntryViewModel.IsIRS || _manualEntryViewModel.IsCTP))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Handled = true;
                return;
            }
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }
        
            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2 && (!_manualEntryViewModel.IsIRS || !_manualEntryViewModel.IsCTP))
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }

        private void ComSide_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox c = sender as ComboBox;
                ComboBoxItem item = c.SelectedItem as ComboBoxItem;
                if (c.SelectedIndex >= 0)
                {
                    switch (c.SelectedIndex)
                    {
                        case 0:
                            c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FB943C"));//ColorBrush.Orange

                            break;
                        case 1:
                            c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#58B3FF"));

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex);
            }
        }

        private void CounterInsNameCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                _manualEntryViewModel.FilterCounterInstitution(args);
            }
        }

        private void CounterTrdNameCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                _manualEntryViewModel.FilterCounterUser(args);
                //if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                //{
                //    _manualEntryViewModel.FilterCounterTraderLocal(args);
                //}
                //else
                //{
                //    _manualEntryViewModel.FilterCounterTrader(args);
                //}
            }
        }

        private void CounterInsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CounterTrdNameCb_edit.Text))
            {
                CounterInsNameCb_edit.ExpandOptions();
            }
        }

        private void CounterTrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CounterTrdNameCb_edit.Text))
            {
                CounterTrdNameCb_edit.ExpandOptions();
            }
        }

        public void SetTradeStartDate()
        {
            if (_manualEntryViewModel != null)
            {
                //_manualEntryViewModel.CurrentDate = DateTime.MinValue;
                _manualEntryViewModel.CounterTrdNameEnable = "1";
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendFQMessageReq request = new SendFQMessageReq();
            request.content = _manualEntryViewModel.CopyTxt;
            request.userid = LocalEnv.UserInfo.UserId;
            request.Username = LocalEnv.UserInfo.UserName;
            request.price = _manualEntryViewModel.CustomPrice;
            request.code = _manualEntryViewModel.SearchText;
            request.direction = _manualEntryViewModel.Side == "1" ? "bid" : "ofr";
            request.name = _manualEntryViewModel.Symbol;
            request.volume = _manualEntryViewModel.SourceAmount;
            request.traderQQ = _manualEntryViewModel.SelectedTrader_Edit == null ? string.Empty : _manualEntryViewModel.SelectedTrader_Edit.qqlist;

            if (string.IsNullOrEmpty(request.traderQQ) == false)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PResult p = FinQHandler.Instance.SendFQMessage(request);
                    if (p == null || !p.IsSuccess())
                    {
                        string errorMsg = p != null && !string.IsNullOrEmpty(p.Msg) ? p.Msg : "推送失败";
                        CommonUtils.Prompt2Second("推送", errorMsg, this);
                    }
                    else
                    {
                        CommonUtils.Prompt2Second("推送", "推送成功", this);
                    }
                });
            }
            else
            {
                CommonUtils.Prompt2Second("推送", "请先选择客户", this);
            }
        }

        private void lbOpenClose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbOpenClose.SelectedItems.Count ==0)  
            {
                lbOpenClose.SelectedIndex = 0;
            }
        }
    }
}
