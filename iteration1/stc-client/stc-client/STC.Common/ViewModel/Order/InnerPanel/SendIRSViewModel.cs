using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerPanel;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class SendIRSViewModel : BaseViewModel
    {
        private Window myWindow;

        private BondsInfoPo _bondsInfoPo = null;
        private SendPriceModelAdapter _sendPriceAdapter = null;

        private bool isBid = true;
        private bool isInit = true;
        private bool _isUpdatePrice = false;
        private PriceMatchParam _priceMatchParam = null;

        private const string FIRM = "Firm";
        private const string NONFIRM = "nonfirm";
        private const string YYYYMMDD = "yyyyMMdd";
        private readonly string ADD_ONE_NOT_LIMIT = "2,3";
        public SendPriceModelAdapter GetSendPriceModelAdapter()
        {
            return _sendPriceAdapter;
        }

        public PriceMatchParam GetPriceMatchParam()
        {
            return _priceMatchParam;
        }

        public BondsInfoPo GetBondsInfoPo()
        {
            return _bondsInfoPo;
        }

        public bool IsBid { get { return isBid; } }

        #region 界面属性



        private bool _darkDiskFlagChecked = false;
        public bool DarkDiskFlagChecked
        {
            get
            {
                return _darkDiskFlagChecked;
            }

            set
            {
                _darkDiskFlagChecked = value;

                this.OnPropertyChanged();

            }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged();
                }
            }
        }




        private SendPriceItem sendPriceItem = null;

        public SendPriceItem SendPriceItem
        {
            get { return sendPriceItem; }
            set
            {
                if (value != null)
                {
                    sendPriceItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private OptionClz _selectedProduct_Edit;

        public OptionClz SelectedProduct_Edit
        {
            get
            {
                return _selectedProduct_Edit;
            }

            set
            {
                if (value != _selectedProduct_Edit)
                {
                    _selectedProduct_Edit = value;
                    this.OnPropertyChanged();

                    if (value != null && SelectedTerm_Edit != null)
                    {
                        string securtityID = $"{value.Key}_{SelectedTerm_Edit.Key}";
                        this.SearchText = securtityID;
                    }
                    else
                    {
                        this.SearchText = string.Empty;
                    }
                }
            }
        }

        private string _productText_Edit;

        public string ProductText_Edit
        {
            get
            {
                return _productText_Edit;
            }
            set
            {
                if (value != _productText_Edit)
                {
                    _productText_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private OptionClz _selectedTerm_Edit;
        public OptionClz SelectedTerm_Edit
        {
            get
            {
                return _selectedTerm_Edit;
            }

            set
            {
                if (value != _selectedTerm_Edit)
                {
                    _selectedTerm_Edit = value;
                    this.OnPropertyChanged();

                    if (value != null && SelectedProduct_Edit != null)
                    {
                        string securtityID = $"{SelectedProduct_Edit.Key}_{value.Key}";
                        this.SearchText = securtityID;
                    }
                    else
                    {
                        this.SearchText = string.Empty;
                    }
                }
            }
        }

        private string _termText_Edit;

        public string TermText_Edit
        {
            get
            {
                return _termText_Edit;
            }
            set
            {
                if (value != _termText_Edit)
                {
                    _termText_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        DataTable proAndTermDt = new DataTable();

        private void InitProductAndTerm()
        {
            SortedDictionary<string, Model.Base.SimpleBondsInfoPo> dic = CommonUtils.GetAllBaseBondsInfo();
            List<KeyValuePair<string, Model.Base.SimpleBondsInfoPo>> productVtermList = (from d in dic where d.Value.MarketIndicator == "2" select d).ToList();

            proAndTermDt.Columns.Add(CommonConstants.PRODUCT_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.FR007_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.Shibor3M_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.LPR1Y_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.LPR5Y_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.FDR007_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.GB10_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.CDB10_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.D10G1_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.AAA3D3_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.ShiborON_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.FDR001_FIELD);
            proAndTermDt.Columns.Add(CommonConstants.S3MR07_FIELD);
            proAndTermDt.PrimaryKey = new DataColumn[] { proAndTermDt.Columns[CommonConstants.PRODUCT_FIELD] };

            DataRow dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_1M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_3M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_6M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_9M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_1Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_2Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_3Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_4Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_5Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_7Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_10Y_FIELD;
            proAndTermDt.Rows.Add(dr);


            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_3M6M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_6M9M_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_6M1Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_9M1Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_1Y2Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_1Y5Y_FIELD;
            proAndTermDt.Rows.Add(dr);

            dr = proAndTermDt.NewRow();
            dr[CommonConstants.PRODUCT_FIELD] = CommonConstants.TERM_2Y5Y_FIELD;
            proAndTermDt.Rows.Add(dr);


            if (productVtermList != null)
            {
                foreach (KeyValuePair<string, Model.Base.SimpleBondsInfoPo> pair in productVtermList)
                {
                    string product = string.Empty;
                    string term = string.Empty;
                    string securityID = pair.Key;
                    int index = securityID.IndexOf("_");
                    if (!string.IsNullOrEmpty(securityID) && index >= 0)
                    {
                        product = securityID.Substring(0, index);
                        term = securityID.Substring(index + 1);

                        DataRow findDr = proAndTermDt.Rows.Find(term);
                        findDr[product] = securityID;
                    }
                }
            }

            productVtermList.Clear();
            productVtermList = null;
            dic.Clear();
            dic = null;
        }

        internal void FilterProduct(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            AutoCompleteComboBox productCCB = sender as AutoCompleteComboBox;

            string term = string.Empty;
            if (SelectedTerm_Edit != null)
            {
                term = SelectedTerm_Edit.Key;
            }

            string pattern = string.Empty;
            List<OptionClz> productList = new List<OptionClz>();
            if (!productCCB.IsClickExpandButton)
            {
                pattern = args.Pattern;
            }

            if (!string.IsNullOrEmpty(term))
            {
                DataRow findRow = proAndTermDt.Rows.Find(term);
                if (findRow != null)
                {
                    foreach (DataColumn column in proAndTermDt.Columns)
                    {
                        if (column.ColumnName == CommonConstants.PRODUCT_FIELD)
                        {
                            continue;
                        }
                        string value = findRow[column].ToString();
                        if (!string.IsNullOrEmpty(value)
                            && (string.IsNullOrEmpty(pattern) || column.ColumnName.ToLower().Contains(pattern.ToLower())))
                        {
                            productList.Add(new OptionClz(column.ColumnName));
                        }
                    }
                }
            }
            else
            {
                foreach (DataColumn column in proAndTermDt.Columns)
                {
                    if (column.ColumnName == CommonConstants.PRODUCT_FIELD)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(pattern) || column.ColumnName.ToLower().Contains(pattern.ToLower()))
                    {
                        productList.Add(new OptionClz(column.ColumnName));
                    }
                }
            }


            args.DataSource = productList;
        }

        internal void FilterTerm(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            AutoCompleteComboBox termCCB = sender as AutoCompleteComboBox;

            string prodcut = string.Empty;
            if (SelectedProduct_Edit != null)
            {
                prodcut = SelectedProduct_Edit.Key;
            }

            string pattern = string.Empty;

            List<OptionClz> termList = new List<OptionClz>();
            if (!termCCB.IsClickExpandButton)
            {
                pattern = args.Pattern;
            }

            foreach (DataRow dr in this.proAndTermDt.Rows)
            {
                string currentTerm = dr[CommonConstants.PRODUCT_FIELD].ToString();
                if (!string.IsNullOrEmpty(prodcut))
                {
                    string value = dr[prodcut].ToString();
                    if (!string.IsNullOrEmpty(value)
                        && (string.IsNullOrEmpty(pattern) || currentTerm.ToLower().Contains(pattern.ToLower())))
                    {
                        termList.Add(new OptionClz(currentTerm));
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(pattern) || currentTerm.ToLower().Contains(pattern.ToLower()))
                    {
                        termList.Add(new OptionClz(currentTerm));
                    }
                }
            }

            args.DataSource = termList;
        }

        private string _priceTxtPercent = "%";

        public string PriceTxtPercent
        {
            get { return _priceTxtPercent; }
            set { _priceTxtPercent = value;
                this.OnPropertyChanged();
            }
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
                    int index = showtext.IndexOf("_");
                    if (index > -1)
                    {
                        this.ProductText_Edit = showtext.Substring(0, index);
                        this.SelectedProduct_Edit = new OptionClz(this.ProductText_Edit);
                        this.TermText_Edit = showtext.Substring(index + 1);
                        this.SelectedTerm_Edit = new OptionClz(this.TermText_Edit);
                    }
                    
                    this.OnPropertyChanged(nameof(ShowText));
                }
            }
        }

        // 搜索
        private string searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return searchText;
            }

            set
            {
                if (value != searchText)
                {
                    searchText = value;
                    this.OnPropertyChanged();


                    PriceTxtPercent = "%";
                    if (value != null)
                    {
                        try
                        {
                            if (CommonUtils.IsSpreadOrBasic(value))
                            {
                                PriceTxtPercent = "bp";
                            }
                        }
                        catch (Exception e)
                        {

                        }
                        
                    }

                    SendPriceItem.Security = searchText;

                    GetDetailConditionViewModel().SecurityID = searchText;

                    RefreshUI();

                    QtyViewModel qtyViewModel = GetQtyViewModel();
                    decimal? orgamount = null;
                    if (qtyViewModel != null)
                    {
                        orgamount = qtyViewModel.TrueAmount;
                    }
                    
                    InitPriceView(searchText);
                    this.GetQtyViewModel().Init(searchText, true);

                    if (qtyViewModel != null && orgamount.HasValue)
                    {
                        qtyViewModel.TrueAmount = orgamount.Value;
                    }

                    if (isInit == false)
                    {
                        _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
                        UpdateBestPrice();

                        CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                        if (CommonUtils.IsSpreadOrBasic(searchText) && !string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
                        {
                            CustomPrice = (CustomPrice.ConvertToDecimal() * 100).ToString("N2");
                        }

                        SetPriceFocus();
                    }
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

        public void InitPriceView(string searchText = "")
        {

            string securityID = string.Empty;
            var x = CommonUtils.GetBondsInfoPoBySecurityID(searchText);
            if (x != null)
            {
                Symbol = (x as BondsInfoPo).Short_Name;
            }
        }


        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshUI()
        {
            WholeFlagChecked = false;
            sendPriceItem.BestBid = null;
            sendPriceItem.BestBidFloat = sendPriceItem.BestBidInt = string.Empty;
            sendPriceItem.BestOfr = null;
            sendPriceItem.BestOfrFloat = sendPriceItem.BestOfrInt = string.Empty;
            sendPriceItem.Appoint = false;
            sendPriceItem.ChannelIndex = -1;
            sendPriceItem.CustomPrice = string.Empty;
            //sendPriceItem.CustomSettleDate = string.Empty;
            FwdSelectDate = DateTime.Now;
            NetPrice = string.Empty;
            sendPriceItem.NonfirmTime = string.Empty;
            SettleBoxIndex = -1;
            SettleBoxIndex2 = 0;
            //sendPriceItem.SettleType = string.Empty;
            //ComboBoxSelectIndex = 0;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;

        }


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

                //if (oldPrice != customPrice || string.IsNullOrEmpty(NetPrice))
                //{
                //    if (string.IsNullOrEmpty(customPrice))
                //    {
                //        NetPrice = string.Empty;
                //    }

                //    if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                //    {
                //        netPrice = string.Empty;
                //        this.OnPropertyChanged("NetPrice");
                //        //isSendReq = true;
                //        CalBond("4", GetSetttle(), NumChange(customPrice));
                //    }
                //    this.OnPropertyChanged();
                //}
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
                //decimal d = 0;
                //if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                //{
                //    CalBond("2", GetSetttle(), netPrice);
                //}

                //if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                //{
                //    //CustomPrice = string.Empty;
                //    customPrice = string.Empty;
                //}

                this.OnPropertyChanged();
            }
        }

        private int conditionPriceRowHeight = 174;
        private int ConditionPriceRowHeight
        {
            get { return conditionPriceRowHeight; }
            set
            {
                conditionPriceRowHeight = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _allowanceChannelVisibility = Visibility.Visible;
        public Visibility AllowanceChannelVisibility
        {
            get
            {
                return _allowanceChannelVisibility;
            }

            set
            {
                _allowanceChannelVisibility = value;

                this.OnPropertyChanged();
            }
        }

        #region  清算速度 +1 +0 远

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

                //if (value)
                //{
                //    _addOneTodayCheck = _addOneTomorrowCheck = false;
                //    this.OnPropertyChanged("AddOneTodayCheck");
                //    this.OnPropertyChanged("AddOneTomorrowCheck");

                //    CalBond("4", GetSetttle(), NumChange(customPrice));

                //    if (!oldVal.Equals(value))
                //        UpdateBestPrice();
                //}
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

                //if (value)
                //{
                //    _addOneNotLimitCheck = _addOneTomorrowCheck = false;
                //    this.OnPropertyChanged("AddOneNotLimitCheck");
                //    this.OnPropertyChanged("AddOneTomorrowCheck");

                //    CalBond("4", GetSetttle(), NumChange(customPrice));

                //    if (!oldVal.Equals(value))
                //        UpdateBestPrice();
                //}
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

                //if (value)
                //{
                //    _addOneNotLimitCheck = _addOneTodayCheck = false;
                //    this.OnPropertyChanged("AddOneNotLimitCheck");
                //    this.OnPropertyChanged("AddOneTodayCheck");

                //    CalBond("4", GetSetttle(), NumChange(customPrice));

                //    if (!oldVal.Equals(value))
                //        UpdateBestPrice();
                //}
            }
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

                if (SettleBoxIndex2 == 0)
                {
                    AddOneNotLimitCheck = true;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = false;

                }
                else if (SettleBoxIndex2 == 1)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = true;
                    AddOneTomorrowCheck = false;

                }
                else if (SettleBoxIndex2 == 2)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = true;
                }

                OnPropertyChanged();
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

                //if (!isSendReq && !oldIndex.Equals(value))
                //{
                //    //isSendReq = true;
                //    CalBond("4", GetSetttle(), NumChange(customPrice));
                //}

                this.OnPropertyChanged();

                SetOtherVisibile();

                if (!oldIndex.Equals(value))
                {
                    UpdateBestPrice();
                    if (oldIndex == 1 && value != 1)
                    {
                        ChangeUI();
                    }
                }

            }
        }


        /// <summary>
        /// 清算时间切换至其他
        /// </summary>
        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0 || SettleBoxIndex == -1)
            {
                SendPriceItem.DateVisible = Visibility.Collapsed;
                SendPriceItem.SettleTypeVisible = Visibility.Collapsed;
                
            }
            else if (SettleBoxIndex == 1)
            {
                FwdComboBoxSelectIndex = 0;
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;

                try
                {
                    if (_priceMatchParam == null)
                    {
                        this.CalBond("4", true);
                    }
                    else
                    {
                        string settle = _priceMatchParam.SettlType;
                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3,2") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                        {
                            this.CalBond("4", true);
                        }
                        else
                        {
                            // 恢复 Settle
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
                }
                catch (Exception e) { }

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

                //if (!isSendReq && !oldIndex.Equals(value))
                //{
                //    //isSendReq = true;
                //    if (_priceMatchParam == null)
                //    {
                //        CalBond("4", GetSetttle(), NumChange(customPrice));
                //    }
                //}

                GetSettleTypeFromServer(GetSetttle());

                if (!oldIndex.Equals(value))
                {
                    ChangeUI();

                    UpdateBestPrice();
                }

                this.OnPropertyChanged();
            }
        }




        private int addOneSettleBoxIndex = 0;
        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int AddOneSettleBoxIndex
        {
            get
            {
                return addOneSettleBoxIndex;
            }

            set
            {
                int oldIndex = settleBoxIndex;
                addOneSettleBoxIndex = value;
                if (addOneSettleBoxIndex >= 0)
                {
                }

                if (addOneSettleBoxIndex == 0)
                {
                }
                else if (addOneSettleBoxIndex == 1)
                {
                }
                else if (addOneSettleBoxIndex == 2)
                {
                }

                OnPropertyChanged();
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
                //if (!isSendReq && value != null && !oldDate.Equals(newDate))
                //{
                //    //isSendReq = true;
                //    CalBond("4", GetSetttle(), NumChange(customPrice));
                //}

                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();

                if (value != null && !oldDate.Equals(newDate))
                {
                }
            }
        }

        private bool firmChecked = true;
        public bool FirmChecked
        {
            get
            {
                return firmChecked;
            }

            set
            {
                firmChecked = value;

                FirmCheckUpdate();

                this.OnPropertyChanged();


                ChangeUI();

                UpdateBestPrice();

            }
        }



        private bool _firmCheckedEnabled = true;
        public bool FirmCheckedEnabled
        {
            get
            {
                return _firmCheckedEnabled;
            }

            set
            {
                _firmCheckedEnabled = value;
                this.OnPropertyChanged();
            }
        }


        #region IRS
        private string marketindicator = "2";
        /// <summary>
        /// 债券类型
        /// </summary>
        public string MarketIndicator
        {
            get { return marketindicator; }
            set
            {
                marketindicator = value;
                if (value == "2")
                {
                    //IsShowIRS = Visibility.Collapsed;
                    //AccountUserVisibility = Visibility.Collapsed;
                    //TextBlock qtyView = myWindow.FindName("PriceTxtPercent") as TextBlock;
                    //qtyView.Margin = new Thickness(78, 24, 0, 0);
                    //QtyView qtyView1 = myWindow.FindName("qtyView") as QtyView;
                    //qtyView1.Margin = new Thickness(66, 0, 0, 0);
                    //(myWindow as View.Order.SendPointPriceView).Width = 340;
                }
                this.OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 切换UI显示内容
        /// </summary>
        private void ChangeUI()
        {
            _firmCheckedEnabled = true;
            this.OnPropertyChanged("FirmCheckedEnabled");

        }




        /// <summary>
        /// 更新最优行情
        /// </summary>
        private void UpdateBestPrice()
        {
            if (!isInit && _sendPriceAdapter != null)
            {
                string settle = GetSetttle();
                _sendPriceAdapter.SetFilterParam(false, settle);
            }
        }

        /// <summary>
        /// Firm  NoFirm Select
        /// </summary>
        private int firmselectindex = 1;

        public int FirmSelectIndex
        {
            get
            {
                return firmselectindex;
            }

            set
            {
                if (firmselectindex != value)
                {
                    firmselectindex = value;
                    FirmChecked = value == 0 ? false : true;

                    this.OnPropertyChanged(nameof(FirmSelectIndex));
                }
            }
        }



        private void FirmCheckUpdate()
        {
            if (firmChecked)
            {
                if (isBid)
                {
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_bid_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_bid_normal.png";
                }
                else
                {
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_ofr_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_ofr_normal.png";
                }

            }
            else
            {
                sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_no_hover.png";
                sendPriceItem.FirmCheckIS = "pack://application:,,,/STC.XBond.Plugin;component/Images/stc_button_nonfirm_no_normal.png";

            }

            FirmTxt = firmChecked ? NONFIRM : FIRM;

            SendPriceItem.FirmVisibility = Visibility.Collapsed;//firmChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private string firmTxt = NONFIRM;
        public string FirmTxt
        {
            get
            {
                return firmTxt;
            }

            set
            {
                firmTxt = value;
                this.OnPropertyChanged(nameof(FirmTxt));
            }
        }


        private bool _wholeFlagChecked = false;
        public bool WholeFlagChecked
        {
            get
            {
                return _wholeFlagChecked;
            }

            set
            {
                _wholeFlagChecked = value;

                this.OnPropertyChanged();


                ChangeUI();

                if (value)
                    ResetQtyWarn();

            }
        }

        /// <summary>
        /// 获得清算速度
        /// </summary>
        /// <returns></returns>
        private string GetSetttle(bool isfilter = false)
        {
            string settle = string.Empty;

            //默认+1选择
            if (SettleBoxIndex == -1)
                settle = AddOneNotLimitCheck ? ADD_ONE_NOT_LIMIT : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : ADD_ONE_NOT_LIMIT;
            else if (SettleBoxIndex == 0)
            {
                //选择+0
                settle = "1";
            }
            else
            {
                // 如果不是获取最优价
                if (!isfilter)
                {
                    if (FwdSelectDate != null && FwdSelectDate.HasValue)
                    {
                        settle = FwdSelectDate.Value.ToString("MM.dd") + (FwdComboBoxSelectIndex == 0 ? "+0" : "+1");
                    }
                }
                else
                {
                    settle = "4"; //远期
                }
            }


            if (string.IsNullOrEmpty(settle))
                settle = "2,3";//默认+1

            return settle;
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

        #endregion



        private int setAmountTxtIndex = -1;

        public int SetAmountTxtIndex
        {
            get
            {
                return setAmountTxtIndex;
            }

            set
            {
                setAmountTxtIndex = value;
                if (setAmountTxtIndex == 0)
                {
                    OnOneClick();
                }
                else if (setAmountTxtIndex == 1)
                {
                    OnThreeClick();
                }
                else if (setAmountTxtIndex == 2)
                {
                    OnFiveClick();
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 下单量1000
        /// </summary>
        private void OnOneClick()
        {
            SendPriceItem.Amount = "2";
        }

        /// <summary>
        /// 下单量3000
        /// </summary>
        private void OnThreeClick()
        {
            SendPriceItem.Amount = "3";
        }

        /// <summary>
        /// 下单量5000
        /// </summary>
        private void OnFiveClick()
        {
            SendPriceItem.Amount = "5";
        }


        private bool sendPriceEnable = true;
        public bool SendPriceEnable
        {
            get
            {
                return sendPriceEnable;
            }

            set
            {
                sendPriceEnable = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 风险提示  最大可成交量
        /// </summary>
        private string _RiskWarnText = "*请详细确认以上信息";
        public string RiskWarnText
        {
            get { return _RiskWarnText; }
            set
            {
                _RiskWarnText = value;
                this.OnPropertyChanged("RiskWarnText");
            }
        }

        private string _RiskWarnMsg = "";
        public string RiskWarnMsg
        {
            get { return _RiskWarnMsg; }
            set
            {
                _RiskWarnMsg = value;
                this.OnPropertyChanged("RiskWarnMsg");
            }
        }


        /// <summary>
        /// 右下角确认信息内容
        /// </summary>
        /// <param name="random"></param>
        /// <param name="amount"></param>
        public void ResetQtyWarn(string random = null, string amount = null)
        {
            decimal dAmount = 0;
            decimal dRandom = 0;
            string useQty = string.Empty;
            string useRandom = string.Empty;

            string useMarketQty = string.Empty;

            if (random == "4")
            {
                sendPriceItem.CustomQty = sendPriceItem.Amount;
                useQty = sendPriceItem.Amount;

                useRandom = amount;
            }
            else
            {
                useQty = sendPriceItem.CustomQty = string.IsNullOrEmpty(amount) ? sendPriceItem.Amount : amount;
                sendPriceItem.CustomQty = string.IsNullOrEmpty(amount) ? sendPriceItem.Amount : amount;

            }


            if (decimal.TryParse(useQty, out dAmount) && decimal.TryParse(useMarketQty, out dRandom))
            {
                sendPriceItem.CustomQty = (dAmount + dRandom).ToString();
            }

        }

        #endregion


        private bool isSendReq = false;
        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private bool _needCalPrice = true;
        private object _calLock = new object();
        private CalResult _lastCalResult = null;


        public string GetCalBondErrMsg()
        {
            return calBondErrMsg;
        }

        public string GetLastReqData()
        {
            return _lastReqData;
        }

        public CalResult GetLastCalResult()
        {
            return _lastCalResult;
        }


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
                    requst.SecurityID = sendPriceItem.Security;
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
                                    if (result.IsSuccess() && sendPriceItem.CalWarnVisibility == Visibility.Visible)
                                        sendPriceItem.CalWarnVisibility = Visibility.Collapsed;

                                    lock (_calLock)
                                    {
                                        _lastCalResult = result;

                                    }

                                    if (calType.Equals("4"))
                                    {
                                        netPrice = result.Data.CleanPrice.ToString("F4");
                                        this.OnPropertyChanged("NetPrice");
                                    }
                                    else if (calType.Equals("2"))//净价
                                    {
                                        if (result.IsSuccess())
                                        {
                                            customPrice = (result.Data.Yield * 100).ToString("F4");

                                            this.OnPropertyChanged("CustomPrice");
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

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null)
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
                                    SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                                }
                                catch (Exception e) { }

                                FwdComboBoxSelectIndex = 0;
                                SendPriceItem.SettleType = "T+0";

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

        private bool isSendCalDate = false;
        /// <summary>
        /// 计算清算时间其他模式下的日期
        /// </summary>
        /// <param name="settleType"></param>
        /// <param name="isTomorrow"></param>
        private void CalDate(string settleType, bool isTomorrow)
        {
            if (isSendCalDate)
                return;

            isSendCalDate = true;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null && result.Data != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                string year = result.Data.dealDate.Substring(0, 4);
                                string month = result.Data.dealDate.Substring(4, 2);
                                string day = result.Data.dealDate.Substring(6, 2);
                                FwdSelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                            }
                            catch (Exception e) { }

                            FwdComboBoxSelectIndex = isTomorrow ? 0 : 1;
                            SendPriceItem.SettleType = isTomorrow ? "T+0" : "T+1";

                            isSendCalDate = false;
                        }));
                    }
                    else
                        isSendCalDate = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendCalDate = false;
                }
            });
        }

        public void onWindowActivated(int activeType)
        {
            Line LineSplit = myWindow.FindName("LineSplit") as Line;
            Button btn_Expand = myWindow.FindName("btn_Expand") as Button;
            if (activeType == 1) //Window_Deactivated
            {
                BorderBrush = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
                //LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
            }
            else if (activeType == 2) //Window_Activated
            {
                BorderBrush = (Brush)Application.Current.MainWindow.FindResource("GetFocusBorderBrush");
                //if (btn_Expand.Content.ToString() == ">>")
                //{
                //    LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("GetFocusBorderBrush");
                //}
                //else
                //{
                //    LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
                //}
            }
        }


        #region 命令事件
        public ICommand BidCommand { get { return new ComRelayCommand(OnBidClick); } }
        public ICommand OfrCommand { get { return new ComRelayCommand(OnOfrClick); } }
        public ICommand CloseCommand { get { return new ComRelayCommand(CloseClick); } }

        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshClick); } }

        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnConfirmClick); } }
        public ICommand AppointCommand { get { return new ComRelayCommand(AppointClick); } }
        public ICommand AddCommand { get { return new ComRelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new ComRelayCommand(OnDownClick); } }
        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new ComRelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new ComRelayCommand(ReducePriceClick); } }
        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }

        public bool IsUpdatePrice = false;
        #endregion

        #region 命令事件实现


        private Visibility cardViewVisibility = Visibility.Collapsed;
        public Visibility CardViewVisibility
        {
            get { return cardViewVisibility; }
            set
            {
                cardViewVisibility = value;
                this.OnPropertyChanged(nameof(CardViewVisibility));
            }
        }

        private string isBidSelect = "0";
        public string IsBidSelect
        {
            get { return isBidSelect; }
            set
            {
                isBidSelect = value;
                OnPropertyChanged(nameof(IsBidSelect));
            }
        }

        private void OnBidClick()
        {
            isBid = true;
            Button btnBid = myWindow.FindName("btnBid") as Button;
            Button btnOfr = myWindow.FindName("btnOfr") as Button;
            Panel.SetZIndex(btnBid, 10);
            Panel.SetZIndex(btnOfr, 1);

            netPrice = string.Empty;
            this.OnPropertyChanged("NetPrice");

            //sendPriceItem.BgImageSource = "/STC.Common;component/Images/stc_bg_bid.png";

            sendPriceItem.BidBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange-11");
            sendPriceItem.OfrBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.White-1");
            sendPriceItem.ShowBidEffect = (Effect)Application.Current.MainWindow.FindResource("BidEffect");
            sendPriceItem.ShowOfrEffect = null;
            sendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.BorderBG = (Brush)Application.Current.MainWindow.FindResource("BidLinearGradientBrush");
            sendPriceItem.QtyBorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange7");
            GetRequireParseViewModel().BorderBrush = sendPriceItem.BorderBH;
            this.IsBidSelect = isBid ? "1" : "0";
            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.BorderBH = sendPriceItem.BorderBH;
                qtyViewModel.QtyBorderBH = sendPriceItem.QtyBorderBH;
            }


            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                if (CommonUtils.IsSpreadOrBasic(searchText) && !string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
                {
                    CustomPrice = (CustomPrice.ConvertToDecimal() * 100).ToString("N2");
                }
                SetPriceFocus();
            }


            GetDetailConditionViewModel().Side = isBid ? "0" : "1";


            if (LocalEnv.ConfInfo.IsShowPriceCondition == "1")
            {
                ConditionCancelView conditionCancelView = myWindow.FindName("conditionView") as ConditionCancelView;
                if (conditionCancelView != null)
                {
                    conditionCancelView.SetBdBrush(sendPriceItem.BorderBH);
                    conditionCancelView.SetConditionTxt(IsBid ? 1 : 2);
                }
            }

        }

        public void SetPriceFocus()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TextBox PriceTxt = (TextBox)myWindow.FindName("PriceTxt");
                // 选择最后位数
                int pos = PriceTxt.Text.IndexOf('.');
                if (pos > 0)
                {
                    int sellength = PriceTxt.Text.Length - pos + 1;
                    PriceTxt.Focus();
                    PriceTxt.SelectionStart = pos + 1;
                    PriceTxt.SelectionLength = sellength;
                }
                else
                {
                    PriceTxt.Focus();
                }
            }));
        }


        private void OnOfrClick()
        {
            Button btnBid = myWindow.FindName("btnBid") as Button;
            Button btnOfr = myWindow.FindName("btnOfr") as Button;
            Panel.SetZIndex(btnBid, 1);
            Panel.SetZIndex(btnOfr, 10);

            isBid = false;


            GetDetailConditionViewModel().Side = isBid ? "0" : "1";


            netPrice = string.Empty;
            this.OnPropertyChanged("NetPrice");

            //sendPriceItem.BgImageSource = "/STC.Common;component/Images/stc_bg_ofr.png";

            sendPriceItem.BidBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.White-1");
            sendPriceItem.OfrBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-15");
            sendPriceItem.ShowOfrEffect = (Effect)Application.Current.MainWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            sendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.QtyBorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue10");

            GetRequireParseViewModel().BorderBrush = sendPriceItem.BorderBH;
            sendPriceItem.BorderBG = (Brush)Application.Current.MainWindow.FindResource("OfrLinearGradientBrush");
            this.IsBidSelect = isBid ? "1" : "0";
            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.BorderBH = sendPriceItem.BorderBH;
                qtyViewModel.QtyBorderBH = sendPriceItem.QtyBorderBH;
            }

            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                if (CommonUtils.IsSpreadOrBasic(searchText) && !string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
                {
                    CustomPrice = (CustomPrice.ConvertToDecimal() * 100).ToString("N2");
                }
                SetPriceFocus();
            }

            if (LocalEnv.ConfInfo.IsShowPriceCondition == "1")
            {
                ConditionCancelView conditionCancelView = myWindow.FindName("conditionView") as ConditionCancelView;
                if (conditionCancelView != null)
                {
                    conditionCancelView.SetBdBrush(sendPriceItem.BorderBH);
                    conditionCancelView.SetConditionTxt(IsBid ? 1 : 2);
                }
            }
        }

        private void CloseClick()
        {
            myWindow.Close();
        }

        private void OnRefreshClick()
        {

        }

        private void OnCancelClick()
        {
            CloseClick();
        }

        private async void OnConfirmClick()
        {
            this.myWindow.Topmost = false;
            bool isok = false;
            isok = await PublishOrder(this, myWindow);
            if (isok)
            {
                // 关闭
                OnCancelClick();
            }
        }

        private void AppointClick()
        {

        }


        private void OnAddClick()
        {

        }


        private void OnDownClick()
        {

        }


        private void AddPriceClick()
        {
            double price = 0;
            if (CommonUtils.IsSpreadOrBasic(this.SearchText))
            {
                if (double.TryParse(CustomPrice, out price))
                {
                    price = price + 0.25;
                    if (price >= -100 && price <= 9999)
                    {
                        CustomPrice = price.ToString("F2");
                    }
                }
            }
            else
            {
                if (double.TryParse(CustomPrice, out price))
                {
                    price = price + 0.0025;
                    if (price >= -100 && price <= 9999)
                    {
                        CustomPrice = price.ToString("F4");
                    }
                }
            }
        }

        private void ReducePriceClick()
        {
            double price = 0;
            if (CommonUtils.IsSpreadOrBasic(this.SearchText))
            {
                if (double.TryParse(CustomPrice, out price))
                {
                    price = price - 0.25;
                    if (price >= -100 && price <= 9999)
                    {
                        CustomPrice = price.ToString("F2");
                    }
                }
            }
            else
            {
                if (double.TryParse(CustomPrice, out price))
                {
                    price = price - 0.0025;
                    if (price >= -100 && price <= 9999)
                    {
                        CustomPrice = price.ToString("F4");
                    }
                }
            }
        }

        private void AddAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(SendPriceItem.Amount, out amount))
            {
                amount += 1;
                if (amount <= 1000)
                {
                    SendPriceItem.Amount = amount.ToString();
                }
            }
            else
            {
                SendPriceItem.Amount = "1";
            }


            SetAmountTxtSelectIndex("1");
        }

        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(SendPriceItem.Amount, out amount))
            {
                amount = amount - 1;
                if (amount >= 0)
                {
                    SendPriceItem.Amount = amount.ToString();
                }
            }
            else
            {
                SendPriceItem.Amount = "5";
            }

            SetAmountTxtSelectIndex("5");
        }
        #endregion


        public void SetAmountTxtSelectIndex(string txt)
        {
            List<string> valueList = new List<string>() { "2", "3", "5" };
            if (!valueList.Contains(SendPriceItem.Amount))
            {
                SetAmountTxtIndex = -1;
            }
            else
            {
                int tem2 = valueList.FindLastIndex(x => x == SendPriceItem.Amount);
                SetAmountTxtIndex = tem2;
            }
        }


        /// <summary>
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;
        private bool isHvaeNewExecution = false;
        private DispatcherTimer timer = null;

        private Visibility alertSignVisible = Visibility.Collapsed;
        public Visibility AlertSignVisible
        {
            get { return alertSignVisible; }
            set
            {
                alertSignVisible = value;
                this.OnPropertyChanged("AlertSignVisible");
            }
        }

        /// <summary>
        /// 最后成交时间
        /// </summary>
        private string _LastUpdateTime = string.Empty;
        public string LastUpdateTime
        {
            get { return _LastUpdateTime; }
            set
            {
                _LastUpdateTime = value;
                this.OnPropertyChanged("_LastUpdateTime");
            }
        }

        /// <summary>
        /// 最后成交价格
        /// </summary>
        private string _LastPrice = string.Empty;
        public string LastPrice
        {
            get { return _LastPrice; }
            set
            {
                _LastPrice = value;
                this.OnPropertyChanged("LastPrice");
            }
        }

        private string alertSignToolTipText = "5秒内与全市场最新成交不一致 偏离较大";
        public string AlertSignToolTipText
        {
            get { return alertSignToolTipText; }
            set
            {
                alertSignToolTipText = value;
                this.OnPropertyChanged("AlertSignToolTipText");
            }
        }

        private Visibility priceConditionVisibility = Visibility.Collapsed;
        public Visibility PriceConditionVisibility
        {
            get { return priceConditionVisibility; }
            set
            {
                priceConditionVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private bool _IsTableSvr = true;
        /// <summary>
        /// 是否启用TradeBook,默认TableSvr接口
        /// </summary>
        public bool IsTableSvr
        {
            get { return _IsTableSvr; }
            set
            {
                _IsTableSvr = value;
                this.OnPropertyChanged("LastPrice");
            }
        }

        public bool CheckBondType()
        {
            return _bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.MarketIndicator) && _bondsInfoPo.MarketIndicator.Equals("4");
        }

        public QtyViewModel GetQtyViewModel()
        {
            QtyView qtyView = myWindow.FindName("qtyView") as QtyView;

            return qtyView.DataContext as QtyViewModel;
        }


        public TagConditionViewModel GetDetailConditionViewModel()
        {

            TagConditionView detailConditionView = myWindow.FindName("detailConditionView") as TagConditionView;

            return detailConditionView.DataContext as TagConditionViewModel;
        }

        public RequireParseViewModel GetRequireParseViewModel()
        {
            RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;

            return parseView.DataContext as RequireParseViewModel;
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        private void InitUI(string security, bool isOfr, bool isReadClipboard = false, bool istablesvr = true)
        {
            //启用TableSvr接口
            IsTableSvr = istablesvr;

            // 判断是否是买入
            isBid = isOfr;
            _sendPriceAdapter = new SendPriceModelAdapter();

            string goodsType = string.Empty;
            goodsType = CommonUtils.SetGoodsShortname(security);

            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.Init(security, true);
            }
            
            //InnerPriceViewAdapter.Instance.Init(this, myWindow);
            PermisionHelper.Instance.InitPermission(goodsType);

            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = security;


            if (!string.IsNullOrEmpty(security))
            {
                _bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(security);
                if (_bondsInfoPo != null)
                {
                    ShowText = $"{SendPriceItem.Security}";
                    Symbol = _bondsInfoPo.Short_Name;
                }
            }

            if (isBid)
            {
                OnBidClick();

                if (qtyViewModel != null)
                {
                    qtyViewModel.BorderBH = SendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
                }
            }
            else
            {
                OnOfrClick();

                if (qtyViewModel != null)
                {
                    qtyViewModel.BorderBH = SendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                }
            }


            SendPriceItem.NonfirmTime = SystemParamHandler.Instance.GetFirmtonontime();
            
            // 缺省报量
            //if (SystemParamHandler.Instance.GetUserSetting() != null)
            //{
            //    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
            //    if (!string.IsNullOrEmpty(Us.userQty))
            //    {
            //        if (IsBrokerSecurity)
            //        {
            //            SendPriceItem.Amount = !IsIRS ? (decimal.Parse(Us.userQty) / 1000).ToString() : (decimal.Parse(Us.userQty) / 100).ToString();
            //        }
            //        if (IsBank)
            //        {
            //            SendPriceItem.Amount = (decimal.Parse(Us.userQty)).ToString();
            //        }
            //    }
            //}

            if (qtyViewModel != null && SendPriceItem.Amount != null)
            {
                qtyViewModel.Amount = SendPriceItem.Amount.ToString();
            }

            if (_priceMatchParam != null)
            {
                _sendPriceAdapter.SetFilterParam(false, _priceMatchParam.SettlType);
            }
            else
            {
                _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
            }
            _sendPriceAdapter.Init(SendPriceItem);


            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

            if (SendPriceConsistencyCheck)
            {
                // to do ...
                this.timer = new DispatcherTimer();
                this.timer.Tick += Timer_Tick;
                this.timer.Interval = TimeSpan.FromSeconds(1);
                this.timer.Start();
            }
            // 服务器时间
            LastUpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //GetDetailConditionViewModel().MyWindow = myWindow;
            GetDetailConditionViewModel().SecurityID = security;
            GetDetailConditionViewModel().Side = isBid ? "0" : "1";
            GetDetailConditionViewModel().SetMorethanMarketVisible(Visibility.Collapsed);
            //解析撤单条件和订阅
            ConsistencyCheckPo consistency = UserSettingAdapter.Instance.GetConsistencyCheckPo();
            if (consistency != null)
            {
                GetDetailConditionViewModel().CheckReverse = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeReverse);
                GetDetailConditionViewModel().CheckSyntropy = UserSettingAdapter.Instance.ParseCheckedType(consistency.SubscribeSyntropy);
            }


            //解析撤单条件
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().priceCondition))
            {
                PriceCondition priceCondition = JsonConvert.DeserializeObject<PriceCondition>(SystemParamHandler.Instance.GetUserSetting().priceCondition);
                GetDetailConditionViewModel().MorethanMarketBPCheck = string.IsNullOrEmpty(priceCondition.MorethanMarketBPCheck) ? false : priceCondition.MorethanMarketBPCheck == "1" ? true : false;
                GetDetailConditionViewModel().MorethanMarketBP = CommonUtils.IsNumeric(priceCondition.MorethanMarketBP) ? priceCondition.MorethanMarketBP : "";
            }

            //if (isReadClipboard)
            //{
            RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
            //parseView.SetContentText(Clipboard.GetText().Trim());
            //parseView.SetContentFocus();
            //parseView.SetContentView(Visibility.Collapsed);
            parseView.SetBrokerContentView(Visibility.Collapsed);
            
            
            //设置IRS识别接口
            parseView.SetIRS(true);

            if (!IsTableSvr)
            {
                parseView.SetContentVisible(Visibility.Collapsed);
            }
            //}
        }

        private void RequireParseViewModel_BeginParse(object obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                (parseView.DataContext as RequireParseViewModel).IsEnableParse = false;
            });
        }

        private void RequireParseViewModel_OnParse(List<Model.Table.WorkBrench.IntentionQuoteInfo> obj)
        {
            if (obj != null && obj.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    isInit = true;
                    PriceMatchParam priceMatchParam = InnerPanelHelper.IntentionToPriceParam(obj[0]);
                    priceMatchParam.UpdataPrice = false;
                    RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                    //设置是否已经允许的标志
                    (parseView.DataContext as RequireParseViewModel).IsEnableParse = true;
                    if ((parseView.DataContext as RequireParseViewModel).IsIRS)
                    {
                        priceMatchParam.CustomFirmAmount = string.IsNullOrEmpty(priceMatchParam.CustomFirmAmount) ? "" : (Decimal.Parse(priceMatchParam.CustomFirmAmount) * 100).ToString();
                    }
                    
                    this.Init(myWindow, priceMatchParam);
                    SetPriceFocus();
                });
            }
        }

        public void clear()
        {
            LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
            _sendPriceAdapter.Clear();
        }

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                // 更新时间
                string updateTime = obj.UpdateTime;
                string serverTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (DateTime.Parse(updateTime) > DateTime.Parse(LastUpdateTime))
                {
                    LastUpdateTime = obj.UpdateTime;
                    LastPrice = obj.Yield;
                    isHvaeNewExecution = true;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string serverTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (isHvaeNewExecution)
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Parse(serverTime).Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime.Parse(LastUpdateTime).Ticks);
                TimeSpan ts3 = ts1.Subtract(ts2).Duration();

                if (ts3.Seconds > 5)
                {
                    if (!string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
                    {
                        AlertSignToolTipText = $"与最新成交 {this.LastPrice} 不一致";
                        decimal price = this.customPrice.ConvertToDecimal();
                        if (CommonUtils.IsSpreadOrBasic(this.searchText))
                        {
                            price = price / 100;
                        }
                        if (decimal.Parse(this.LastPrice) != price)
                        {
                            AlertSignVisible = Visibility.Visible;
                            this.timer.Stop();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.LastPrice))
                        {
                            AlertSignVisible = Visibility.Visible;
                            this.timer.Stop();
                        }
                    }
                }
            }
        }

        private bool IsInitRequireParse = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="priceMatchParam"></param>
        public void Init(Window myWindow, PriceMatchParam priceMatchParam)
        {
            this.myWindow = myWindow;

            if (_priceMatchParam != null)
            {
                //如果是修改订单
                _priceMatchParam.Price = priceMatchParam.Price;
                _priceMatchParam.CustomFirmAmount = priceMatchParam.CustomFirmAmount;
                _priceMatchParam.Side = priceMatchParam.Side;
                _priceMatchParam.TimeToMaturity = priceMatchParam.TimeToMaturity;
                //修改订单不改债券
                //_priceMatchParam.Symbol = priceMatchParam.Symbol;
                //_priceMatchParam.SymbolName = priceMatchParam.SymbolName;
            }
            else
            {
                _isUpdatePrice = priceMatchParam.UpdataPrice;
                _priceMatchParam = priceMatchParam;
            }

            if (_isUpdatePrice)
            {
                GetRequireParseViewModel().IsEnableParse = false;

                if (!IsInitRequireParse)
                {
                    RequireParseViewModel requireParseViewModel = GetRequireParseViewModel();
                    if (requireParseViewModel != null)
                    {
                        requireParseViewModel.OnParse += RequireParseViewModel_OnParse;
                        requireParseViewModel.BeginParse += RequireParseViewModel_BeginParse;
                    }

                    IsInitRequireParse = true;
                }
            }

            bool isOfr = false;

            if (priceMatchParam.Side != null)
                 isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.ToUpper().Equals("OFR") || priceMatchParam.Side.Equals("2");

            TagConditionViewModel detailConditionViewModel = GetDetailConditionViewModel();
            OrderTimeModel otm = null;
            if (!string.IsNullOrEmpty(priceMatchParam.Info1))
            {
                if (JsonSplit.IsJson(priceMatchParam.Info1))
                {
                    detailConditionViewModel.ParseValidTimeFormat(priceMatchParam.Info1);
                    otm = JsonConvert.DeserializeObject<OrderTimeModel>(priceMatchParam.Info1);
                    //恢复识别文本
                    if (!string.IsNullOrEmpty(otm.Text))
                    {
                        GetRequireParseViewModel().BondTxt = otm.Text;
                    }
                }
            }

            if (otm != null)
            {
                IsTableSvr = otm.IsFromTableSvrOrTradeBook == "1" ? true : false;
            }

            InitUI(priceMatchParam.Symbol, !isOfr, false, IsTableSvr);

            if (otm != null)
            {
                //恢复时间设置
                detailConditionViewModel.ParseValidTimeFormat(otm.ValidTimeType);
                //恢复撤单条件
                detailConditionViewModel.ParsePriceCondition(otm);
                //恢复订阅
                detailConditionViewModel.ParseTagData(otm.IF_SUB_BOND, priceMatchParam.Side == "1" ? "0" : "1");
            }

            //恢复
            if (!string.IsNullOrEmpty(priceMatchParam.Info2))
            {
                GetRequireParseViewModel().SetTagData(priceMatchParam.Info2);
            }
            else
            {
                GetRequireParseViewModel().SetTagData(priceMatchParam.Info1);
            }
            
            GetRequireParseViewModel().SetInstitutionAndTrader(new PriceMatchParam() { TraderID = priceMatchParam.TraderID, InstitutionID = priceMatchParam.InstitutionID});

            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                _wholeFlagChecked = true;

            }
            else
                _wholeFlagChecked = false;

            if (!string.IsNullOrEmpty(priceMatchParam.CustomFirmAmount))
            {
                QtyViewModel qtyViewModel = GetQtyViewModel();

                
                if (qtyViewModel != null)
                {
                    qtyViewModel.TrueAmount = Decimal.Parse(priceMatchParam.CustomFirmAmount);
                    sendPriceItem.Amount = qtyViewModel.Amount;
                }
            }

            decimal d = 0;

            if (decimal.TryParse(priceMatchParam.Price, out d))
            {
                if (CommonUtils.IsSpreadOrBasic(priceMatchParam.Symbol))
                {
                    CustomPrice = (d * 100).ToString("N2").Replace(",", "");
                }
                else
                {
                    CustomPrice = d.ToString("N4").Replace(",", "");
                }
            }
            else
            {
                CustomPrice = priceMatchParam.Price;
            }


            netPrice = priceMatchParam.NetPrice;
            this.OnPropertyChanged("NetPrice");

            // firm or nonfirm
            if (!priceMatchParam.IsFirm)
            {
                //FirmChecked = !priceMatchParam.IsFirm;
                this.FirmSelectIndex = 1;
            }
            else
            {
                FirmSelectIndex = 0;
            }



            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                WholeFlagChecked = true;
            }
            else
                WholeFlagChecked = false;


            ParseSettle(_priceMatchParam.SettlType);

            isInit = false;

        }


        /// <summary>
        /// 解析清算速度
        /// </summary>
        /// <param name="settle"></param>
        private void ParseSettle(string settle)
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

                        if (settle.Equals(ADD_ONE_NOT_LIMIT))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
                        else if (settle.Equals("3"))
                        {
                            SettleBoxIndex2 = 2;
                            AddOneTomorrowCheck = true;
                        }
                        else if (settle.Equals("2"))
                        {
                            SettleBoxIndex2 = 1;
                            AddOneTodayCheck = true;
                        }
                        else if (settle.Equals("+1"))
                        {
                            SettleBoxIndex2 = 0;
                            AddOneNotLimitCheck = true;
                        }
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
                                SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
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
                            this.CalDate("4", false);
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
                                        SendPriceItem.CustomSettleDate = dt.ToString("yyy-MM-dd");
                                    }
                                    catch (Exception e) { }
                                }
                                //_needCalPrice = true;
                                if (arr[1].Equals("0"))
                                {
                                    FwdComboBoxSelectIndex = 0;
                                    SendPriceItem.SettleType = "T+0";
                                }
                                else if (arr[1].Equals("1"))
                                {
                                    FwdComboBoxSelectIndex = 1;
                                    SendPriceItem.SettleType = "T+1";
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

        public SendIRSViewModel()
        {
            InitProductAndTerm();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        public void Init(Window myWindow, string security, bool bidOrOfr, bool isReadClipboard = false, bool isTableSvr = true)
        {
            isBid = bidOrOfr;
            this.myWindow = myWindow;

            if (!IsInitRequireParse)
            {
                RequireParseViewModel requireParseViewModel = GetRequireParseViewModel();
                if (requireParseViewModel != null)
                {
                    requireParseViewModel.OnParse += RequireParseViewModel_OnParse;
                    requireParseViewModel.BeginParse += RequireParseViewModel_BeginParse;
                }

                IsInitRequireParse = true;
            }

            InitUI(security, bidOrOfr, isReadClipboard, isTableSvr);
            //to do ...
            //UserSettingAdapter.Instance.ParseValidTimeFormat(this);

            CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);

            if (CommonUtils.IsSpreadOrBasic(security) && !string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
            {
                CustomPrice = (CustomPrice.ConvertToDecimal() * 100).ToString("N2");
            }

            isInit = false;
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



        #endregion


        /// <summary>
        /// 计算正确的清算速度
        /// </summary>
        /// <param name="settleType">明天+0 ，6.15+0 </param>
        private void GetSettleTypeFromServer(string settleType)
        {
            return;
            if (isSendReq)
                return;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (requst != null && result.Data != null)
                    {
                        try
                        {
                            SendPriceItem.SettleType = result.Data.settlementType2;
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


        #region 下单

        /// <summary>
        /// 发布需求
        /// </summary>
        private async Task<bool> PublishOrder(SendIRSViewModel sendPriceViewModel, Window myWindow)
        {
            this.myWindow = myWindow;

            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击发布需求报价按钮");
            }
            
            sendPriceViewModel.SendPriceEnable = false;

            ////if (!CheckParam())
            ////{
            ////    return false;
            ////}
            ///
            //发送新订单
            NewOrderSingle req = GetNewOrderSingle();
            //撤单
            if (_isUpdatePrice)
            {
                STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                cancelPo.AlgoName = req.AlgoName;
                cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                cancelPo.OrderID = _priceMatchParam.OrderId;
                TBookCancelReplyPo reply = OrdersHandler.Instance.CancelOrder(cancelPo);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                    }
                    else
                    {
                        CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                        sendPriceViewModel.SendPriceEnable = true;
                        return false;
                    }
                }
            }

            List<NewOrderSingle> reqlst = new List<NewOrderSingle>();
            reqlst.Add(req);

            //bool res = CustomMessageBox.Question($"是否发布需求？", "信息");
            bool res = true;
            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击发布需求按钮");
            }

            if (res == true)
            {
                StringBuilder error = null;
                error = await SubPublishOrders(reqlst);
                if (error != null && error.Length > 0)
                {
                    CustomMessageBox.Error($"发布需求失败, 原因:\r\n{error.ToString()}");
                    sendPriceViewModel.SendPriceEnable = true;
                    return false;
                }
            }

            sendPriceViewModel.SendPriceEnable = true;
            return true;
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="tmp"></param>
        /// <returns></returns>
        private async Task<StringBuilder> SubPublishOrders(List<NewOrderSingle> reqlst)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                if (reqlst != null)
                {
                    int count = reqlst.Count;
                    Task[] lstTask = new Task[count];
                    ConcurrentDictionary<string, PResult> dic = new ConcurrentDictionary<string, PResult>();
                    for (int i = 0; i < count; i++)
                    {
                        NewOrderSingle sendPricePo = reqlst[i];
                        lstTask[i] = new Task(() =>
                        {
                            if (IsTableSvr)
                            {
                                //是否是TableSvr
                                sendPricePo.HandlInst = "2";
                                RecognizeIntentionQuoteReplyPo res2 = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                                if (res2.Code == "10000" || res2.IsSuccess())
                                {
                                    PResult result = new PResult();
                                    result.Code = "0000";
                                    result.Msg = res2.Msg;
                                    dic[sendPricePo.ClOrdID] = result;
                                    RemenberInstAndTrade(sendPricePo);
                                }
                                else
                                {
                                    PResult res = new PResult();
                                    res.Code = "-1";
                                    res.Msg = "服务器接收数据失败！";
                                    dic[sendPricePo.ClOrdID] = res;
                                }
                            }
                            else
                            {
                                //是否是TradeBook
                                PResult result = OrdersHandler.Instance.PlaceOrder(sendPricePo);
                                if (result != null && !string.IsNullOrEmpty(result.Msg))
                                {
                                    dic[sendPricePo.ClOrdID] = result;
                                    RemenberInstAndTrade(sendPricePo);
                                }
                                else
                                {
                                    PResult res = new PResult();
                                    res.Code = "-1";
                                    res.Msg = "服务器接收数据失败！";
                                    dic[sendPricePo.ClOrdID] = res;
                                }
                            }
                            
                        });

                        lstTask[i].Start();
                    }

                    Task.WaitAll(lstTask);

                    foreach (KeyValuePair<string, PResult> pair in dic)
                    {
                        string orderID = pair.Key;
                        PResult reply = pair.Value;
                        if (reply != null)
                        {
                            if (reply.IsSuccess() == false)
                            {
                                error.AppendLine($"订单号 {orderID}:{reply.Msg}");
                            }
                        }
                    }
                }
            });


            return error;
        }

        /// <summary>
        /// 生成订单报文
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();

            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";

            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            
            PriceMatchParam priceMatchParam = GetPriceMatchParam();
            if (priceMatchParam != null)
            {
                sendPricePo.Info1 = priceMatchParam.Info1;
                sendPricePo.Info2 = priceMatchParam.Info2;
                sendPricePo.Info3 = priceMatchParam.Info3;
                sendPricePo.Info4 = priceMatchParam.Info4;
                sendPricePo.Info5 = priceMatchParam.Info5;
            }

            sendPricePo.MarketIndicator = "2";
            sendPricePo.OrdType = "2";

            if (GetQtyViewModel() != null && GetQtyViewModel().GetAmount() != 0)
                sendPricePo.OrderQty = GetQtyViewModel().TrueAmount.ToString();

            if (CommonUtils.IsSpreadOrBasic(SendPriceItem.Security) && !string.IsNullOrEmpty(CustomPrice) && CommonUtils.IsNumeric(CustomPrice))
            {
                decimal customPrice = CustomPrice.ConvertToDecimal();
                sendPricePo.Price = (customPrice / 100).ToString();
            }
            else
            {
                sendPricePo.Price = CustomPrice;
            }

            sendPricePo.PriceType = "3"; //收益率
            sendPricePo.SecurityID = SendPriceItem.Security;
            sendPricePo.Symbol = Symbol;
            sendPricePo.Side = IsBid ? "1" : "2";

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = FirmChecked ? "0" : "1";

            sendPricePo.OrdType = "2";
            sendPricePo.SettlType = "2,3";
            sendPricePo.TimeInForce = "0";

            sendPricePo.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查

            //识别
            RequireParseViewModel requireParseViewModel = GetRequireParseViewModel();
            //if (IntentionOldHandler.Instance.isSALER())
            {
                sendPricePo.InstitutionID = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.idbkey;
                //sendPricePo.InstitutionName = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.shortnamecn;
                sendPricePo.InstitutionName = requireParseViewModel.InstitutionName_Edit;
                sendPricePo.TraderID = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.idbkey;
                //sendPricePo.TraderName = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.namecn;
                sendPricePo.TraderName = requireParseViewModel.TraderName_Edit;
            }

            sendPricePo.Text = requireParseViewModel.BondTxt;

            sendPricePo.PriceType = requireParseViewModel.PriceType;

            TagData clientTagData = requireParseViewModel.GetTagData();
            sendPricePo.OrgType = clientTagData.ORG_TYPE;
            sendPricePo.VipCode = clientTagData.VIP;

            TagConditionViewModel tagconditionviewmodel = GetDetailConditionViewModel();
            //标签
            TagData td = tagconditionviewmodel.GetTagData();
            sendPricePo.IfSubBond = td.IF_SUB_BOND;

            //撤单时间
            string info1 = tagconditionviewmodel.GetValidTimeType();

            //撤单条件
            PriceCondition priceCondition = tagconditionviewmodel.GetPriceCondition();
            if (priceCondition != null)
            {
                sendPricePo.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            }

            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();
            OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = validuntiltime, IF_SUB_BOND = sendPricePo.IfSubBond ?? string.Empty, VIP = sendPricePo.VipCode ?? string.Empty, ORG_TYPE = sendPricePo.OrgType ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty,Text = sendPricePo.Text, QuoteType = IsTableSvr ? "irs" : "tbookirs", IsFromTableSvrOrTradeBook = IsTableSvr? "1" : "0" };
            sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            sendPricePo.ValidUntilTime = validuntiltime;

            TagData info2tag = new TagData() { IF_SUB_BOND = otm.IF_SUB_BOND, ORG_TYPE = otm.ORG_TYPE, VIP = otm.VIP };
            sendPricePo.Info2 = JsonConvert.SerializeObject(info2tag);

            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;
            sendPricePo.Name = LocalEnv.UserInfo.UserName;
            sendPricePo.QuoteType = IsTableSvr ? "irs" : "tbookirs";
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            if (MarketIndicator == "2")
            {
                sendPricePo.SettlType = null;
                sendPricePo.NetPrice = null;
                sendPricePo.WholeFlag = null;
            }
            sendPricePo.Channel = "CLIENT";
            return sendPricePo;
        }

        #endregion

        private void RemenberInstAndTrade(NewOrderSingle sendPricePo)
        {
            if (!string.IsNullOrEmpty(sendPricePo.TraderID))
            {
                Common.Utils.CommonUtils.AddContract(sendPricePo.TraderID);
                Common.Utils.CommonUtils.SaveContract();
            }
            if (!string.IsNullOrEmpty(sendPricePo.InstitutionID))
            {
                Common.Utils.CommonUtils.AddInst(sendPricePo.InstitutionID);
                Common.Utils.CommonUtils.SaveInst();
            }
        }

    }

    public class OptionClz : ViewModelBase
    {
        private string _key;
        #region ctor
        public OptionClz(string key)
        {
            Key = key;
        }
        #endregion

        #region Properties
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }
        #endregion
    }
}
