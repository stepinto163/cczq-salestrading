using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerPanel;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Card;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class SendRequireViewModel : BaseViewModel
    {
        private Window myWindow;

        private BondsInfoPo _bondsInfoPo = null;
        private SendPriceModelAdapter _sendPriceAdapter = null;

        private bool isBid = true;
        private bool isInit = true;
        public bool _isUpdatePrice = false;
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

        private bool _SettleIsDropDownOpen = false;
        public bool SettleIsDropDownOpen
        {
            get
            {
                return _SettleIsDropDownOpen;
            }

            set
            {
                _SettleIsDropDownOpen = value;
                SettleBoxIndex = -1;
                AddOneOpenedTag = "1";
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
                    SendPriceItem.Security = searchText;

                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsI2T)
                    {
                        string type = CommonUtils.ConvertToPAType(value);

                        List<string> traderList = AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId, type);
                        if (traderList != null)
                        {
                            List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                            List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                            if (list != null && list.Count > 0)
                            {
                                StcTbookUsers stcTbookUsers = list[0];
                                if (GetRequireParseViewModel() != null)
                                {
                                    GetRequireParseViewModel().SelectTraderItem = stcTbookUsers;
                                    GetRequireParseViewModel().SelectTraderText = stcTbookUsers.Name;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        _bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(value);
                        if (_bondsInfoPo != null)
                        {
                            ShowText = $"{SendPriceItem.Security} {_bondsInfoPo.Short_Name}";
                            Symbol = _bondsInfoPo.Short_Name;
                            IsIRS = _bondsInfoPo.MarketIndicator == "2";

                            // 含权
                            if (!string.IsNullOrEmpty(_bondsInfoPo.Option_Type) &&
                            ((!_bondsInfoPo.Option_Type.Equals("NON") && _bondsInfoPo.Option_Date != null) ||
                            (_bondsInfoPo.Option_Type.Equals("ETS") && _bondsInfoPo.Option_Date == null)))
                            {

                                if (PriceTypeEmptyDic.Count == 2)
                                {
                                    PriceTypeEmptyDic.Add(new KeyValuePair<string, string>("7", "行权"));
                                }
                                this.PriceType = "7";
                            }
                            else
                            {
                                this.PriceType = "3";
                                if (PriceTypeEmptyDic.Count == 3)
                                {
                                    PriceTypeEmptyDic.RemoveAt(2);
                                }
                            }
                        }
                    }

                    GetDetailConditionViewModel().SecurityID = searchText;

                    RefreshUI();

                    InnerPriceViewAdapter.Instance.InitPriceView(searchText);

                    QtyViewModel qtyViewModel = GetQtyViewModel();
                    decimal? orgamount = null;
                    if (qtyViewModel != null)
                    {
                        orgamount = qtyViewModel.TrueAmount;
                    }

                    InitPriceView(searchText);
                    this.GetQtyViewModel().Init(searchText);

                    if (qtyViewModel != null && orgamount.HasValue)
                    {
                        qtyViewModel.TrueAmount = orgamount.Value;
                    }

                    if (isInit == false)
                    {
                        _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
                        UpdateBestPrice();
                        CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
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

        #region IRS 利率互换设置
        private bool isIRS = false;
        /// <summary>
        /// 是否是IRS
        /// </summary>
        public bool IsIRS
        {
            get { return isIRS; }
            set 
            { 
                isIRS = value;
                if (isIRS)
                {
                    GetDetailConditionViewModel().SetMorethanMarketVisible(Visibility.Collapsed);
                    RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                    parseView.SetContentView(Visibility.Collapsed);
                    IconLinkedNetPriceVisibility = Visibility.Hidden;
                }
                else
                {
                    GetDetailConditionViewModel().SetMorethanMarketVisible(Visibility.Visible);
                    RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                    parseView.SetContentView(Visibility.Visible);
                    IconLinkedNetPriceVisibility = Visibility.Visible;
                }
            }
        }

        private Visibility icon_linkedNetPriceVisibility = Visibility.Visible;
        public Visibility IconLinkedNetPriceVisibility
        {
            get
            {
                return icon_linkedNetPriceVisibility;
            }

            set
            {
                icon_linkedNetPriceVisibility = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public void InitPriceView(string searchText = "")
        {

            string securityID = string.Empty;

            PriceMatchParam _tmppriceMatchParam = this._priceMatchParam;
            if (string.IsNullOrEmpty(searchText))
            {
                if (_tmppriceMatchParam != null)
                {
                    securityID = _tmppriceMatchParam.Symbol;
                }
            }
            else
            {
                securityID = searchText;
            }


            View.Card.PriceAndOrderView tradeBookView = (View.Card.PriceAndOrderView)myWindow.FindName("dgPrice");
            PriceAndOrderViewModel Qm = tradeBookView.DataContext as PriceAndOrderViewModel;

            if (securityID != null)
            {
                tradeBookView.InitTableModel(securityID, "");
            }

            //tradeBookView.DataContext = Qm;
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
            FwdSelectDate = DateTime.Now;
            sendPriceItem.CustomPrice = string.Empty;
            //sendPriceItem.CustomSettleDate = string.Empty;
            NetPrice = string.Empty;
            sendPriceItem.NonfirmTime = string.Empty;
            
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                ParseSettle("3");
            }
            else
            {
                SettleBoxIndex = -1;
                SettleBoxIndex2 = 0;
            }
            
            //sendPriceItem.SettleType = string.Empty;
            //ComboBoxSelectIndex = 0;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;

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
                    if (CustomerPrice.HasValue)
                    {
                        if (value.HasValue)
                        {
                            if (this.isBid)
                                CustomPrice = (CustomerPrice + value / 100).ToString();
                            else
                                CustomPrice = (CustomerPrice - value / 100).ToString();
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 客户报价
        /// </summary>
        private decimal? _CustomerPrice;

        public decimal? CustomerPrice
        {
            get
            {
                return _CustomerPrice;
            }
            set
            {
                if (_CustomerPrice != value)
                {
                    _CustomerPrice = value;
                    this.OnPropertyChanged();
                    if (BP.HasValue && value.HasValue)
                    {
                        if (this.isBid)
                            CustomPrice = (value + BP / 100).ToString();
                        else
                            CustomPrice = (value - BP / 100).ToString();
                    }
                }
            }
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

                if (oldPrice != customPrice || string.IsNullOrEmpty(NetPrice))
                {
                    if (string.IsNullOrEmpty(customPrice))
                    {
                        NetPrice = string.Empty;
                    }

                    if (!isSendReq && !string.IsNullOrEmpty(value) && oldPrice != value)
                    {
                        netPrice = string.Empty;
                        this.OnPropertyChanged("NetPrice");
                        //isSendReq = true;

                        CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));
                    }
                    this.OnPropertyChanged();
                }

                if (CommonUtils.IsNumeric(customPrice) && CustomerPrice.HasValue)
                {
                    if (this.isBid)
                    {
                        this.BP = (customPrice.ConvertToDecimal() - CustomerPrice) * 100;
                    }
                    else
                    {
                        this.BP = (CustomerPrice - customPrice.ConvertToDecimal()) * 100;
                    }
                }
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

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneTodayCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");

                    CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
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

                    CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
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

                    CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
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

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));
                }

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

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    if (_priceMatchParam == null)
                    {
                        CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));
                    }
                }

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
                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    //isSendReq = true;
                    CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));
                }

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


        private string GetCalType_Yield()
        {
            string calType = "4";
            if (this.PriceType == "1")
            {
                if (_bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.Option_Type) &&
                                               ((!_bondsInfoPo.Option_Type.Equals("NON") && _bondsInfoPo.Option_Date != null) ||
                                               (_bondsInfoPo.Option_Type.Equals("ETS") && _bondsInfoPo.Option_Date == null)))
                {
                    calType = "7";
                }
            }
            else if (this.PriceType == "7")
            {
                calType = "7";
            }
            return calType;
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

                                    if (calType.Equals("4") || calType.Equals("7"))
                                    {
                                        if (!string.IsNullOrEmpty(SendPriceItem.Security) && !CommonUtils.IsBrokerSecurity(SendPriceItem.Security))
                                        {
                                            netPrice = result.Data.CleanPrice.ToString("F3");

                                        }
                                        else
                                        {
                                            netPrice = result.Data.CleanPrice.ToString("F4");
                                        }
                                        this.OnPropertyChanged("NetPrice");
                                    }
                                    else if (calType.Equals("2"))//净价
                                    {
                                        if (result.IsSuccess())
                                        {
                                            if (GetCalType_Yield() == "7")
                                            {
                                                customPrice = (result.Data.YieldToExecution * 100).ToString("F4");
                                            }
                                            else
                                            {
                                                customPrice = (result.Data.Yield * 100).ToString("F4");
                                            }

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
                if (btn_Expand.Content.ToString() == ">>")
                {
                    LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("GetFocusBorderBrush");
                }
                else
                {
                    LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
                }
            }
        }


        #region 命令事件

        public ICommand ExpandCommand { get { return new ComRelayCommand(OnExpandClick); } }
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

        public void OnExpandClick()
        {


            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (myWindow != null)
                {
                    Button btn_Expand = (Button)myWindow.FindName("btn_Expand");

                    Border Border_Left1 = (Border)myWindow.FindName("Border_Left1");
                    Border Border_Left3 = (Border)myWindow.FindName("Border_Left3");
                    Border Border_Left4 = (Border)myWindow.FindName("Border_Left4");
                    Border Border_Left5 = (Border)myWindow.FindName("Border_Left5");
                    Border Border_Left7 = (Border)myWindow.FindName("Border_Left7");
                    //Border Border_Right1 = (Border)myWindow.FindName("Border_Right1");
                    Border Border_Right2 = (Border)myWindow.FindName("Border_Right2");
                    Line LineSplit = (Line)myWindow.FindName("LineSplit");
                    if (btn_Expand.Tag.ToString() == "Collapsed")
                    {
                        CardViewVisibility = Visibility.Visible;
                        //Border_Left1.BorderThickness = new Thickness(2, 2, 0, 0);
                        //Border_Left3.BorderThickness = new Thickness(2, 0, 0, 0);
                        //Border_Left4.BorderThickness = new Thickness(2, 0, 0, 0);
                        //Border_Left5.BorderThickness = new Thickness(2, 0, 0, 2);
                        //Border_Right1.BorderThickness = new Thickness(0.5, 2, 2, 2);
                        btn_Expand.Content = "<<";
                        myWindow.Width = 714;
                        btn_Expand.Tag = "Visible";
                        LineSplit.Stroke = (Brush)myWindow.FindResource("LostFocusBorderBrush");
                        LineSplit.Visibility = Visibility.Visible;
                        InitPriceView();

                        SetPriceAndOrderHeigh();
                    }
                    else
                    {
                        CardViewVisibility = Visibility.Collapsed;

                        Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 2, 2);
                        //Border_Right1.BorderThickness = new Thickness(2, 2, 2, 2);
                        //Border_Right2.BorderThickness = new Thickness(2, 0, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 395;
                        btn_Expand.Tag = "Collapsed";
                        View.Card.PriceAndOrderView dgPrice = (View.Card.PriceAndOrderView)myWindow.FindName("dgPrice");
                        LineSplit.Visibility = Visibility.Collapsed;
                        //dgPrice.SetBrokerSelect("ALL");
                    }

                }
            }));
        }

        public void SetPriceAndOrderHeigh()
        {
            if (myWindow != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;

                    listBox.Height = myWindow.Height;
                }));

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
            this.IsBidSelect = isBid ? "1" : "0";

            GetRequireParseViewModel().BorderBrush = sendPriceItem.BorderBH;
            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.BorderBH = sendPriceItem.BorderBH;
                qtyViewModel.QtyBorderBH = sendPriceItem.QtyBorderBH;
            }


            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
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
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (myWindow != null)
                    myWindow.Close();
            }));
        }

        private void OnRefreshClick()
        {

        }

        private void OnCancelClick()
        {
            CloseClick();
        }

        private void OnConfirmClick()
        {
            this.myWindow.Topmost = false;

            InnerPlaceOrderAdapter.Instance.PlaceOrder(this, myWindow);
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
            if (double.TryParse(CustomPrice, out price))
            {
                price = price + 0.0025;
                if (price >= -100 && price <= 9999)
                {
                    CustomPrice = price.ToString("F4");
                }
            }
        }

        private void ReducePriceClick()
        {
            double price = 0;
            if (double.TryParse(CustomPrice, out price))
            {
                price = price - 0.0025;
                if (price >= -100 && price <= 9999)
                {
                    CustomPrice = price.ToString("F4");
                }
            }
        }


        #endregion


        


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
        private void InitUI(string security, bool isOfr, bool isReadClipboard = false)
        {

            // 判断是否是买入
            isBid = isOfr;
            _sendPriceAdapter = new SendPriceModelAdapter();

            string goodsType = string.Empty;
            goodsType = CommonUtils.SetGoodsShortname(security);

            QtyView qtyView = myWindow.FindName("qtyView") as QtyView;
            if (qtyView != null)
            {
                (qtyView.DataContext as QtyViewModel).Init(security);
            }

            InnerPriceViewAdapter.Instance.Init(this, myWindow);
            PermisionHelper.Instance.InitPermission(goodsType);

            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = security;


            if (!string.IsNullOrEmpty(security))
            {
                _bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(security);
                if (_bondsInfoPo != null)
                {
                    ShowText = $"{SendPriceItem.Security} {_bondsInfoPo.Short_Name}";
                    Symbol = _bondsInfoPo.Short_Name;
                    IsIRS = _bondsInfoPo.MarketIndicator == "2";

                    // 含权
                    if (!string.IsNullOrEmpty(_bondsInfoPo.Option_Type) &&
                    ((!_bondsInfoPo.Option_Type.Equals("NON") && _bondsInfoPo.Option_Date != null) ||
                    (_bondsInfoPo.Option_Type.Equals("ETS") && _bondsInfoPo.Option_Date == null)))
                    {
                        if (PriceTypeEmptyDic.Count == 2)
                        {
                            PriceTypeEmptyDic.Add(new KeyValuePair<string, string>("7", "行权"));
                        }
                        this.PriceType = "7";
                    }
                    else
                    {
                        this.PriceType = "3";
                        if (PriceTypeEmptyDic.Count == 3)
                        {
                            PriceTypeEmptyDic.RemoveAt(2);
                        }
                    }
                }
            }

            if (isBid)
            {
                OnBidClick();

                (qtyView.DataContext as QtyViewModel).BorderBH = SendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");

            }
            else
            {
                OnOfrClick();

                (qtyView.DataContext as QtyViewModel).BorderBH = SendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
            }


            SendPriceItem.NonfirmTime = SystemParamHandler.Instance.GetFirmtonontime();

            UserSettingAdapter.Instance.ParseAmount(this);

            QtyViewModel qtyViewModel = qtyView.DataContext as QtyViewModel;
            qtyViewModel.Init(security);
            if (SendPriceItem.Amount != null)
                qtyViewModel.Amount = SendPriceItem.Amount.ToString();

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

            GetDetailConditionViewModel().SecurityID = security;
            GetDetailConditionViewModel().Side = isBid ? "0" : "1";
            
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

            if (isReadClipboard)
            {
                RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                parseView.SetContentText(Clipboard.GetText().Trim());
                parseView.SetContentFocus();
            }
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
                    bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
                    InitUI(priceMatchParam.Symbol, !isOfr);
                    this.searchText = priceMatchParam.Symbol;
                    GetRequireParseViewModel().SetInstitutionAndTrader(priceMatchParam);
                    GetRequireParseViewModel().SetBrokerName(priceMatchParam.Trader);
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
                    isSendReq = false;
                    customPrice = string.Empty;
                    if (decimal.TryParse(priceMatchParam.Price, out d))
                    {
                        CustomerPrice = d;
                        CustomPrice = d.ToString("N4").Replace(",", "");
                    }
                    else
                    {
                        CustomPrice = priceMatchParam.Price;
                    }
                    //this.PriceType = priceMatchParam.PriceType;
                    
                    SetPriceFocus();
                    RequireParseView parseView = myWindow.FindName("parseView") as RequireParseView;
                    (parseView.DataContext as RequireParseViewModel).IsEnableParse = true;
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
                    if (!string.IsNullOrEmpty(this.CustomPrice))
                    {
                        AlertSignToolTipText = $"与最新成交 {this.LastPrice} 不一致";
                        if (decimal.Parse(this.LastPrice) != Decimal.Parse(this.CustomPrice))
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

            _isUpdatePrice = priceMatchParam.UpdataPrice;
            string firmtype = GetDefaultFirmTypeFromUserSetting();
            priceMatchParam.IsFirm = firmtype == "0" ? false : true;
            _priceMatchParam = priceMatchParam;

            InitPAYL();
            InitAccountUsers();

            bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            InitUI(priceMatchParam.Symbol, !isOfr);
            UserSettingAdapter.Instance.ParseValidTimeFormat(this);

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

            this.PriceType = priceMatchParam.PriceType;
            if (priceMatchParam.PriceType == "1")
            {
                NetPrice = priceMatchParam.NetPrice;
            }
            else if (priceMatchParam.PriceType == "2")
            {

            }
            else
            {
                decimal d = 0;

                if (decimal.TryParse(priceMatchParam.Price, out d))
                {
                    //decimal calValue = ConvertToYield(priceMatchParam.Symbol, priceMatchParam.PriceType, priceMatchParam.SettlType, d);
                    //CustomPrice = calValue.ToString("N4").Replace(",", "");
                    CustomPrice = d.ToString("N4").Replace(",", "");
                }
                else
                {
                    CustomPrice = priceMatchParam.Price;
                }
            }

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


            // Modi:cyc Date:2021-04-13 Func: 增加用户选择的 需求有效时长 选项记忆功能
            // string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : "0";

            TagConditionViewModel detailConditionViewModel = GetDetailConditionViewModel();

            if (!string.IsNullOrEmpty(priceMatchParam.Info1))
            {
                OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(priceMatchParam.Info1);
                //恢复时间设置
                detailConditionViewModel.ParseValidTimeFormat(otm.ValidTimeType);
                //恢复撤单条件
                detailConditionViewModel.ParsePriceCondition(otm);
                //恢复订阅
                detailConditionViewModel.ParseTagData(otm.IF_SUB_BOND, priceMatchParam.Side == "1" ? "0" : "1");
                //恢复原文
                GetRequireParseViewModel().BondTxt = string.IsNullOrEmpty(priceMatchParam.Text) ? otm.Text : priceMatchParam.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(priceMatchParam.IntentionID))
                {
                    GetDetailConditionViewModel().CheckReverse = false;
                    GetDetailConditionViewModel().CheckSyntropy = false;
                }
            }

            if (!string.IsNullOrEmpty(priceMatchParam.Info2))
            {
                GetRequireParseViewModel().SetTagData(priceMatchParam.Info2);

                detailConditionViewModel.SetTagData(priceMatchParam.Info2);

                if (JsonSplit.IsJson(priceMatchParam.Info2) && (priceMatchParam.PriceType != "1" && priceMatchParam.PriceType != "2"))
                {
                    try
                    {
                        TagData tagData = JsonConvert.DeserializeObject<TagData>(priceMatchParam.Info2);

                        if (tagData != null)
                        {
                            if (CommonUtils.IsNumeric(tagData.CUSTOMER_PRICE))
                            {
                                this.CustomerPrice = tagData.CUSTOMER_PRICE.ConvertToDecimal().ToString("0.####").ConvertToDecimal();
                            }
                            if (CommonUtils.IsNumeric(tagData.CUSTOMER_SPREAD))
                            {
                                this.BP = tagData.CUSTOMER_SPREAD.ConvertToDecimal().ToString("0.####").ConvertToDecimal();
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
                
            else
                GetRequireParseViewModel().SetTagData(priceMatchParam.Info1);

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                detailConditionViewModel.SetNoteData(priceMatchParam.Info5);
            }

            GetRequireParseViewModel().SetInstitutionAndTrader(priceMatchParam);
            GetRequireParseViewModel().SetBrokerName(priceMatchParam.Trader);

            isInit = false;

        }

        /// <summary>
        ///  返回用户 默认类型 0(nonfirm)，未选中：1(firm)
        /// </summary>
        /// <returns></returns>
        private string GetDefaultFirmTypeFromUserSetting()
        {
            string res = "0";
            if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().inf2))
            {
                ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                if (consistencyCheckPo != null)
                {
                    res = consistencyCheckPo.DefaultFirmType ?? "0";
                }
            }

            return res;
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

        public SendRequireViewModel()
        {
            
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        public void Init(Window myWindow, string security, bool bidOrOfr, bool isReadClipboard = false)
        {
            InitPAYL();
            InitAccountUsers();

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

            InitUI(security, bidOrOfr, isReadClipboard);

            string firmtype = GetDefaultFirmTypeFromUserSetting();
            this.FirmSelectIndex = firmtype == "0" ? 1 : 0;

            UserSettingAdapter.Instance.ParseValidTimeFormat(this);

            CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);

            isInit = false;
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
                    if (requst != null && result != null)
                    {
                        if (result.Data != null)
                        {
                            try
                            {
                                SendPriceItem.SettleType = result.Data.settlementType2;
                            }
                            catch (Exception e) { }
                        }

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

        //private AccountUserItem _accountUserItem = null;

        //public AccountUserItem AccountUserItem

        //{
        //    get { return _accountUserItem; }
        //    set { 
        //        _accountUserItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        //public ObservableCollection<AccountUserItem> AccountUserSource
        //{
        //    get
        //    {
        //        return _accountUserSource;
        //    }
        //}


        internal void InitAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            AccountUserVisibility = Visibility.Visible;

            SetDefaultAccountUsers();

            ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
            MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
        }

        internal void SetDefaultAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            string defaultAccountID = string.Empty;

            UserSetting Us = SystemParamHandler.Instance.GetUserSetting();

            if (Us != null && !string.IsNullOrEmpty(Us.inf2))
            {
                if (JsonSplit.IsJson(Us.inf2))
                {
                    ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                    defaultAccountID = ck.BookId;
                }
            }
            SelectedItem = defaultAccountID;
        }


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

        #endregion


        #region 平安养老
        private Visibility _accountUserVisibility_PA = Visibility.Collapsed;

        public Visibility AccountUserVisibility_PA
        {
            get { return _accountUserVisibility_PA; }
            set
            {
                _accountUserVisibility_PA = value;
                this.OnPropertyChanged();
            }
        }

        private void InitPAYL()
        {
            if (LocalEnv.ConfInfo.IsPAYL == false)
                return;

            AccountUserVisibility_PA = Visibility.Visible;

            InitBookUser();
        }


        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource_PA = new ObservableCollection<TreeNodeModel>();
        /// <summary>
        /// 组合
        /// </summary>
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource_PA
        {
            get { return _MultiTreeComboxItemsSource_PA; }
            set
            {
                if (_MultiTreeComboxItemsSource_PA != value)
                {
                    _MultiTreeComboxItemsSource_PA = value;
                    this.OnPropertyChanged();
                }
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

        private string _SelectedTradeAccountCode = null;
        /// <summary>
        /// 选中的交易账户代码
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

        public Dictionary<string, object> _selectedGroupDict = new Dictionary<string, object>();
        /// <summary>
        /// 选中的组合字典
        /// </summary>
        public Dictionary<string, object> SelectedGroupDict
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
        /// 是否投资经理
        /// </summary>
        public bool IsFundAccount = false;
        private void InitBookUser()
        {
            MultiTreeComboxItemsSource_PA = !CommonUtils.IsInvestor() ? STC.Common.Handler.PP.PPHandler.Instance.GetTradeTBookGroup() : STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();
        }

        #endregion


        #region 报价标准
        private ObservableCollection<KeyValuePair<string, string>> _priceType_empty_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeEmptyDic
        {
            get
            {
                if (_priceType_empty_Dic.Count == 0)
                {
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_empty_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_empty_Dic;
            }
        }

        public string _PriceType = "3";

        public string PriceType
        {
            get { return _PriceType; }
            set
            {
                if (_PriceType != value)
                {
                    _PriceType = value;
                    this.OnPropertyChanged();

                    if (value == "1")
                    {
                        CalBond("2", GetSetttle(), NetPrice);
                    }
                    else
                    {
                        CalBond(GetCalType_Yield(), GetSetttle(), NumChange(customPrice));
                    }
                }
                
            }
        }

        #endregion
    }
}
