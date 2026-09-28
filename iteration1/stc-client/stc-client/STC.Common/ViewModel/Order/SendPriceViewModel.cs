using com.gateway.log;
using netapi.Api;
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
using STC.Common.View.Card;
using STC.Common.View.Component;
using STC.Common.View.Order;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Adapter;
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

namespace STC.Common.ViewModel.Order
{
    public class SendPriceViewModel : BaseViewModel
    {
        private Window myWindow;

        private BondsInfoPo _bondsInfoPo = null;
        private SendPriceModelAdapter _sendPriceAdapter = null;

        public Action<NewOrderSingle> OnCustomConfirmAction;

        private bool isBid = true;
        private bool isInit = true;
        private bool _isUpdatePrice = false;
        private PriceMatchParam _priceMatchParam = null;
        private string currentGoodsType = string.Empty;
        private const string RANDOMTXT = "家渠道 (共{0}家)";
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

                    getCrossViewModel().RefreshVenue(value);

                    GetDetailConditionViewModel().SecurityID = value;

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

                    RefreshUI();

                    PriceViewAdapter.Instance.InitPriceView(searchText);
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

        private bool isIRS = false;
        /// <summary>
        /// 是否是IRS
        /// </summary>
        public bool IsIRS
        {
            get { return isIRS; }
            set { isIRS = value; }
        }

        public void InitPriceView(string searchText = "")
        {
            string securityID = string.Empty;
            var x = CommonUtils.GetBondsInfoPoBySecurityID(searchText);
            if (x != null)
            {
                Symbol = (x as BondsInfoPo).Short_Name;
                IsIRS = (x as BondsInfoPo).MarketIndicator == "2";
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
            FwdSelectDate = DateTime.Now;
            sendPriceItem.CustomPrice = string.Empty;
            //sendPriceItem.CustomSettleDate = string.Empty;
            
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
                if (string.IsNullOrEmpty(customPrice))
                {
                    NetPrice = string.Empty;
                }

                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    //netPrice = string.Empty;
                    //this.OnPropertyChanged("NetPrice");
                    //isSendReq = true;
                    if (!isInit)
                        CalBond("4", this._priceMatchParam.SettlType, NumChange(customPrice));
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
                decimal d = 0;
                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                {
                    if (!isInit)
                        CalBond("2", _priceMatchParam.SettlType, netPrice);
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
        public int ConditionPriceRowHeight
        {
            get { return conditionPriceRowHeight; }
            set
            {
                conditionPriceRowHeight = value;

                this.OnPropertyChanged();
            }
        }

        // crossView 与下方控件(投组账户行)之间预留的间距
        private const int ChannelAreaBottomGap = 10;
        // 承载 crossView 的行(Row4)的最小高度，需与 SendPriceView.xaml 中 MinHeight 保持一致
        private const int ChannelAreaMinHeight = 74;

        /// <summary>
        /// 根据 crossView(含渠道选择 lbDirect 折行)的实际高度，设置其承载行(Row4)的行高，
        /// 使该行紧贴内容并保留约 10px 间距；窗口借助 SizeToContent="Height" 随之自适应，
        /// 既消除多余留白，又保证折行后其他控件完整显示。
        /// </summary>
        /// <param name="crossViewHeight">crossView 当前所需高度（像素）</param>
        public void SetChannelAreaHeight(int crossViewHeight)
        {
            int rowHeight = crossViewHeight + ChannelAreaBottomGap;
            if (rowHeight < ChannelAreaMinHeight)
                rowHeight = ChannelAreaMinHeight;

            ConditionPriceRowHeight = rowHeight;
        }


        #region CrossView

        private int selectCrossViewTabIndex;
        public int SelectCrossViewTabIndex
        {
            get
            {
                return selectCrossViewTabIndex;
            }

            set
            {
                selectCrossViewTabIndex = value;
                this.OnPropertyChanged();
                if (value == 2)
                {
                    //报价渠道新增类型“Sniper单” 仅Firm 可选
                    FirmSelectIndex = 0;
                    FirmCheckedEnabled = false;
                }
                else
                {
                    FirmCheckedEnabled = true;
                }
                onCrossViewTabChange();
            }
        }

        private void onCrossViewTabChange()
        {
            if (LocalEnv.ConfInfo.IsShowPriceCondition == "1")
            {
                if (selectCrossViewTabIndex == 0)
                    myWindow.Height = 555;
                else if (selectCrossViewTabIndex == 1)
                    myWindow.Height = 617;
                else if (selectCrossViewTabIndex == 2)
                    myWindow.Height = 617;
                else if (selectCrossViewTabIndex == 3)
                    myWindow.Height = 663;
            }
            else
            {
                if (selectCrossViewTabIndex == 0)
                    myWindow.Height = 452;
                else if (selectCrossViewTabIndex == 1)
                    myWindow.Height = 536;
                else if (selectCrossViewTabIndex == 2)
                    myWindow.Height = 536;
                else if (selectCrossViewTabIndex == 3)
                    myWindow.Height = 568;
            }

            PriceViewAdapter.Instance.SetPriceAndOrderHeigh();
        }

        /// <summary>
        /// ”X-Bond报价“是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsShowXBondItem()
        {
            return !((SettleBoxIndex == 2 &&  _sendPriceAdapter.IsFwdSettle(FwdSelectDate, FwdComboBoxSelectIndex)) || firmChecked || WholeFlagChecked);
        }

        #endregion



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
        /// <summary>
        /// +1
        /// </summary>
        public string AddOneOpenedTag
        {
            get { return addOneOpenedTag; }
            set
            {
                addOneOpenedTag = value;
                OnPropertyChanged(nameof(AddOneOpenedTag));
            }
        }


        private int settleBoxIndex2 = 0;
        /// <summary>
        /// 清算速度 +1  选项 
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

                if (!oldIndex.Equals(value))
                {
                    if (!isInit)
                        CalBond("4", _priceMatchParam.SettlType, NumChange(customPrice));
                }


                OnPropertyChanged();
            }
        }

        private int settleBoxIndex = -1;

        /// <summary>
        /// 清算速度 +0 或 远 选项 
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
                    if (!isInit)
                        CalBond("4", _priceMatchParam.SettlType, NumChange(customPrice));
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
                // SelectDate = null;
                // ComboBoxSelectIndex = 0;

                if (getCrossViewModel().SelectTabIndex == 0 && !WholeFlagChecked)
                {
                    if (PermisionHelper.Instance.IsHaveAllPermission || PermisionHelper.Instance.IsHaveInnerXbondPermission)
                        getCrossViewModel().SetVenueItemEnable(true, PermisionHelper.Instance.XBOND);
                }
            }
            else if (SettleBoxIndex == 1)
            {
                FwdComboBoxSelectIndex = 0;
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;

                //if (getCrossViewModel().SelectTabIndex == 0)
                {
                    getCrossViewModel().SetVenueItemEnable(false, PermisionHelper.Instance.XBOND);
                }

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
                        if (!isInit)
                            CalBond("4", _priceMatchParam.SettlType, NumChange(customPrice));
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
                    if (!isInit)
                        //isSendReq = true;
                        CalBond("4", _priceMatchParam.SettlType, NumChange(customPrice));
                }

                GetSettleTypeFromServer(GetSetttle());

                this.OnPropertyChanged();

                if (value != null && !oldDate.Equals(newDate))
                {
                }
            }
        }

        private bool firmChecked = false;
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
        /// 是否可以改变 CheckXBond
        /// </summary>
        /// <returns></returns>
        private bool IsCanChangeXond()
        {
            // 恢复原订单值
            if (_priceMatchParam != null)
            {
                if (_priceMatchParam.CrossVenues != null)
                {
                    if (_priceMatchParam.CrossVenues.Equals("3"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 切换UI显示内容
        /// </summary>
        private void ChangeUI()
        {
            if (!FirmChecked && !WholeFlagChecked)
                getCrossViewModel().GetOCOViewModel().CheckInnerEnable = true;

            getCrossViewModel().GetOCOViewModel().CheckXBondEnable = true;

            _firmCheckedEnabled = true;
            this.OnPropertyChanged("FirmCheckedEnabled");

            if (getCrossViewModel().SelectTabIndex != 2)
                getCrossViewModel().SetVenueItemVisibility(Visibility.Visible);


            if (FirmChecked || WholeFlagChecked)
            {
                if (getCrossViewModel().GetOCOViewModel().CheckXBondVisibility == Visibility.Visible)
                {
                    getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(Visibility.Collapsed);

                    getCrossViewModel().GetOCOViewModel().SetCheckXBondEnable(false);

                }

                getCrossViewModel().SetVenueItemEnable(false);
                //UpdateDirectionItem();
            }
            else
            {
                bool isAllow = PermisionHelper.Instance.IsHaveAllPermission;
                if (isAllow)
                {
                    if (!getCrossViewModel().GetOCOViewModel().CheckAll)
                    {
                        if (PermisionHelper.Instance.IsHaveVenueXbondPermission)
                        {
                            getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(Visibility.Visible);
                        }

                        // 恢复原订单值
                        if (this.IsCanChangeXond())
                        {
                            getCrossViewModel().GetOCOViewModel().CheckXBond = true;
                        }
                        else
                        {
                            getCrossViewModel().GetOCOViewModel().CheckXBond = false;
                        }

                        if (!_wholeFlagChecked && !firmChecked)
                        {
                            //_checkAll = false;
                            //_checkInner = false;
                            // 恢复原订单值
                            if (this.IsCanChangeXond())
                            {
                                getCrossViewModel().GetOCOViewModel().CheckXBond = true;
                            }
                            else
                            {
                                getCrossViewModel().GetOCOViewModel().CheckXBond = false;
                            }

                            getCrossViewModel().GetOCOViewModel().SetCheckAll(false);

                            getCrossViewModel().GetOCOViewModel().SetCheckAllVisibility(Visibility.Visible);

                        }
                    }
                    else
                    {
                        getCrossViewModel().GetOCOViewModel().SetCheckAllVisibility(Visibility.Visible);
                    }
                }
                else
                {
                    isAllow = PermisionHelper.Instance.IsHaveInnerXbondPermission;

                    getCrossViewModel().GetOCOViewModel().SetCheckAllVisibility(Visibility.Collapsed);



                    Visibility visibility = (isAllow && PermisionHelper.Instance.IsHaveVenueXbondPermission) ? Visibility.Visible : Visibility.Collapsed;
                    getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(visibility);

                    if (isAllow)
                    {
                        if (_priceMatchParam == null)
                        {
                            getCrossViewModel().GetOCOViewModel().CheckInner = false;
                            getCrossViewModel().GetOCOViewModel().CheckNotOut = false;
                        }

                        getCrossViewModel().GetOCOViewModel().CheckNotoutEnable = true;
                    }

                    getCrossViewModel().GetOCOViewModel().SetCheckXBond(false);

                    this.OnPropertyChanged("CheckXBond");
                    //CheckXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                    //CheckXBond = false;
                }
            }


            //if (_wholeFlagChecked || FirmChecked)
            if (_wholeFlagChecked || FirmChecked)
            {
                if (getCrossViewModel().GetOCOViewModel().CheckInnerVisibility == Visibility.Visible)
                {
                    getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(Visibility.Collapsed);

                    getCrossViewModel().GetOCOViewModel().SetCheckAllVisibility(Visibility.Collapsed);

                    if (getCrossViewModel().SelectTabIndex == 0)
                        getCrossViewModel().GetOCOViewModel().SetCheckInner(true);

                }
            }

            //远期判断
            if ((SettleBoxIndex == 1 && _sendPriceAdapter.IsFwdSettle(FwdSelectDate, FwdComboBoxSelectIndex)) || firmChecked || WholeFlagChecked)
            {
                HideXboundCheckBoxAndDirectionItem();

                if (FirmSelectIndex == 0 && !WholeFlagChecked)
                {
                    getCrossViewModel().SetVenueItemEnable(true);

                    if (SettleBoxIndex == 1 && _sendPriceAdapter.IsFwdSettle(FwdSelectDate, FwdComboBoxSelectIndex))
                        getCrossViewModel().SetVenueItemEnable(false, PermisionHelper.Instance.XBOND);
                }
            }
            else if (getCrossViewModel().SelectTabIndex == 2 || getCrossViewModel().SelectTabIndex == 1)
            {
                //Firm
                if (FirmSelectIndex == 0)
                {
                    getCrossViewModel().SetVenueItemEnable(true);
                    getCrossViewModel().GetOCOViewModel().CheckXBondVisibility = Visibility.Visible;
                    getCrossViewModel().GetOCOViewModel().CheckAll = true;
                }
            }
            else
            {
                if (!(FirmChecked || _wholeFlagChecked))
                {
                    bool isAllow = PermisionHelper.Instance.IsHaveAllPermission;
                    if (isAllow)
                    {
                        if (!getCrossViewModel().GetOCOViewModel().CheckAll)
                        {
                            Visibility visibility = PermisionHelper.Instance.IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;

                            getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(visibility);


                            // 恢复原订单值
                            if (this.IsCanChangeXond())
                            {
                                getCrossViewModel().GetOCOViewModel().CheckXBond = true;
                            }
                            else
                            {
                                getCrossViewModel().GetOCOViewModel().CheckXBond = false;
                            }
                        }
                    }
                    else
                    {
                        isAllow = PermisionHelper.Instance.IsHaveInnerXbondPermission && !FirmChecked && !WholeFlagChecked;

                        Visibility visibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                        
                        getCrossViewModel().GetOCOViewModel().SetCheckXBondVisibility(visibility);
                    }
                }

                // 恢复非远期时关闭的 xbond
                if (getCrossViewModel().GetOCOViewModel().CheckXBondVisibility != Visibility.Visible)
                {
                    if (PermisionHelper.Instance.IsHaveInnerXbondPermission || PermisionHelper.Instance.IsHaveAllPermission)
                    {
                        if (PermisionHelper.Instance.IsHaveVenueXbondPermission)
                        {
                            getCrossViewModel().GetOCOViewModel().CheckXBondVisibility = Visibility.Visible;
                        }
                    }
                }

                if (!FirmChecked && !WholeFlagChecked)
                {
                    getCrossViewModel().SetVenueItemEnable(!getCrossViewModel().GetOCOViewModel().CheckNotOut);
                }

            }


            //直通打开内部
            if (getCrossViewModel().SelectTabIndex == 0)
            {
                if (!FirmChecked && !WholeFlagChecked)
                {
                    //Firm
                    getCrossViewModel().SetVenueItemEnable(true, "INNER");
                    if (!FirmChecked)
                        getCrossViewModel().SetVenueItemEnable(true);
                }
                else
                {
                    //NF 或 WholeFlag = 1
                    getCrossViewModel().SetVenueItemEnable(true, "INNER", true);
                }

                if (_sendPriceAdapter.IsFwdSettle(FwdSelectDate, FwdComboBoxSelectIndex))
                    getCrossViewModel().SetVenueItemEnable(false, PermisionHelper.Instance.XBOND);

            }

            OnCheckInner(getCrossViewModel().GetOCOViewModel().CheckInner);

            //_isFromXBond = false;

        }


        /// <summary>
        /// 撮合渠道选择”仅内部“
        /// </summary>
        /// <param name="checkStatus"></param>
        private void OnCheckInner(bool checkStatus)
        {
            if (checkStatus)
            {
                bool isShowXBond = (PermisionHelper.Instance.IsHaveAllPermission || PermisionHelper.Instance.IsHaveInnerXbondPermission) && PermisionHelper.Instance.IsHaveVenueXbondPermission;

                getCrossViewModel().GetOCOViewModel().ChanelAllVisibility = Visibility.Collapsed;

                if (isShowXBond && !FirmChecked && !_wholeFlagChecked
                    && ((SettleBoxIndex == 1 && !_sendPriceAdapter.IsFwdSettle(FwdSelectDate, FwdComboBoxSelectIndex)) || (SettleBoxIndex == 0 || SettleBoxIndex == 1)))
                    getCrossViewModel().GetOCOViewModel().CheckXBondVisibility = getCrossViewModel().CheckVenuePermission(PermisionHelper.Instance.XBOND) ? Visibility.Visible : Visibility.Collapsed;


                if (!(getCrossViewModel().GetOCOViewModel().CheckXBondVisibility == Visibility.Visible && isShowXBond))
                {
                    AllowanceChannelVisibility = Visibility.Collapsed;
                }
                else
                {
                    AllowanceChannelVisibility = Visibility.Visible;
                }


                if (!PermisionHelper.Instance.IsHaveInnerXbondPermission)
                {
                    getCrossViewModel().SetVenueItemVisibility(Visibility.Visible);

                }
            }
            else
            {
                AllowanceChannelVisibility = Visibility.Visible;


                if (SettleBoxIndex == 1)
                {
                    HideXboundCheckBoxAndDirectionItem();
                }
            }

            SetPriceAndOrderHeight();
        }

        private void SetPriceAndOrderHeight()
        {
        }

        /// <summary>
        /// 隐藏Xbond单选框和Xbond交易渠道
        /// </summary>
        private void HideXboundCheckBoxAndDirectionItem()
        {
            if (getCrossViewModel().GetOCOViewModel().CheckXBondVisibility == Visibility.Visible)
            {
                getCrossViewModel().GetOCOViewModel().CheckXBondVisibility = Visibility.Collapsed;
                getCrossViewModel().GetOCOViewModel().CheckXBond = false;
            }


            //getCrossViewModel().SetVenueItemEnable(false);

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
        private int firmselectindex = 0;

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
            FirmTxt = firmChecked ? NONFIRM : FIRM;
            
            SendPriceItem.FirmVisibility = Visibility.Collapsed;//firmChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private string firmTxt = FIRM;
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

                //getCrossViewModel().GetOCOViewModel().CheckXBond = false;
                //getCrossViewModel().GetOCOViewModel().CheckXBondVisibility = Visibility.Collapsed;

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
            {
                if (SettleBoxIndex2 == 0)
                    settle = "2,3";
                if (SettleBoxIndex2 == 1)
                    settle = "2";
                if (SettleBoxIndex2 == 2)
                    settle = "3";
            }
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

            //if ((myWindow.FindName("lbDirect") as ListBox).SelectedItems.Count <= 1)
            //{
            //    sendPriceItem.CustomQty = "";
            //    sendPriceItem.MarketQty = "";
            //    RiskWarnText = "*请详细确认以上信息";
            //}
            //else
            //{
            //    RiskWarnText = "*最大可成交量";
            //}
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
            settleType = GetSetttle();

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
                    //if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                    //    requst.CalPath = LocalEnv.ConfInfo.CalType;

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
                                        //NetPrice = result.Data.CleanPrice.ToString("F4");
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
                LineSplit.Stroke = (Brush)Application.Current.MainWindow.FindResource("LostFocusBorderBrush");
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
        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }

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

        private void OnExpandClick()
        {
            PriceViewAdapter.Instance.OnExpandClick();
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
            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.BorderBH = sendPriceItem.BorderBH;
                qtyViewModel.QtyBorderBH = sendPriceItem.QtyBorderBH;
            }

            getCrossViewModel().changeUI(isBid);

            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                SetPriceFocus();
            }


            GetDetailConditionViewModel().Side = isBid ? "0" : "1";

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


            netPrice = string.Empty;
            this.OnPropertyChanged("NetPrice");

            //sendPriceItem.BgImageSource = "/STC.Common;component/Images/stc_bg_ofr.png";

            sendPriceItem.BidBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.White-1");
            sendPriceItem.OfrBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue-15");
            sendPriceItem.ShowOfrEffect = (Effect)Application.Current.MainWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            sendPriceItem.BorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.QtyBorderBH = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue10");
            sendPriceItem.BorderBG = (Brush)Application.Current.MainWindow.FindResource("OfrLinearGradientBrush");
            this.IsBidSelect = isBid ? "1" : "0";
            QtyViewModel qtyViewModel = GetQtyViewModel();
            if (qtyViewModel != null)
            {
                qtyViewModel.BorderBH = sendPriceItem.BorderBH;
                qtyViewModel.QtyBorderBH = sendPriceItem.QtyBorderBH;
            }

            getCrossViewModel().changeUI(isBid);

            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                SetPriceFocus();
            }

            GetDetailConditionViewModel().Side = isBid ? "0" : "1";
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
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.myWindow.Topmost = false;
                    PlaceOrderAdapter.Instance.PlaceOrder(this, myWindow);
                }));
            });
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

        

        public CrossViewModel getCrossViewModel()
        {
            CrossView crossView = myWindow.FindName("crossView") as CrossView;
            return crossView.DataContext as CrossViewModel;
        }

        public SniperViewModel getSniperViewModel()
        {
            CrossView sniperView = myWindow.FindName("sniperView") as CrossView;
            return sniperView.DataContext as SniperViewModel;
        }


        public bool CheckBondType()
        {
            return _bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.MarketIndicator) && _bondsInfoPo.MarketIndicator.Equals("4");
        }


        public DetailConditionViewModel GetDetailConditionViewModel()
        {
            DetailConditionView detailConditionView = myWindow.FindName("conditionView") as DetailConditionView;

            return detailConditionView.DataContext as DetailConditionViewModel;
        }


        public QtyViewModel GetQtyViewModel()
        {
            QtyView qtyView = myWindow.FindName("qtyView") as QtyView;

            return qtyView.DataContext as QtyViewModel;
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        private void InitUI(string security, bool isOfr)
        {
            InitAccountUsers();
            // 判断是否是买入
            isBid = isOfr;
            _sendPriceAdapter = new SendPriceModelAdapter();

            string goodsType = string.Empty;
            currentGoodsType = goodsType = CommonUtils.SetGoodsShortname(security);

            QtyView qtyView = myWindow.FindName("qtyView") as QtyView;
            if (qtyView != null)
            {
                (qtyView.DataContext as QtyViewModel).Init(security);
            }

            PriceViewAdapter.Instance.Init(this, myWindow);
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
                }
            }

            CrossView crossView = myWindow.FindName("crossView") as CrossView;
            crossView.LbDirectSelectionChanged_CrossView = LbDirect_SelectionChanged_CrossView;
            (crossView.DataContext as CrossViewModel).initUI(security, goodsType, crossView, myWindow, isBid);

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

            // 缺省报量
            UserSettingAdapter.Instance.ParseAmount(this);

            QtyViewModel qtyViewModel = qtyView.DataContext as QtyViewModel;
            qtyViewModel.Init(security);
            if (SendPriceItem.Amount != null)
                qtyViewModel.Amount = SendPriceItem.Amount.ToString();

            if (LocalEnv.ConfInfo.IsShowPriceCondition != "1")
            {
                crossView.Margin = new Thickness(0, -10, 0, 0);
            }
            // ConditionPriceRowHeight(Row4 行高)由 CrossViewModel.changeHeight() -> SetChannelAreaHeight()
            // 根据 crossView 实际高度统一设置，以紧贴内容、消除多余留白
            

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
            LastUpdateTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //设置撤单条件订阅默认值
            GetDetailConditionViewModel().MyWindow = myWindow;
            GetDetailConditionViewModel().SecurityID = security;
            GetDetailConditionViewModel().Side = isBid ? "0" : "1";

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
        }

        private void LbDirect_SelectionChanged_CrossView(object sender, SelectionChangedEventArgs e)
        {
            bool isOuterSelected = false;
            foreach(ListBoxItemData itemData in getCrossViewModel().DirectionItems)
            {
                if (itemData.Code != "MANUAL" && itemData.Checked)
                {
                    isOuterSelected = true;
                }
            }

            if (isOuterSelected && SelectCrossViewTabIndex == 0)
            {
                GetDetailConditionViewModel().LBSubVisibility = Visibility.Collapsed;
            }
            else
            {
                GetDetailConditionViewModel().LBSubVisibility = Visibility.Visible;
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
                string serverTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
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
            string serverTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
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


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="priceMatchParam"></param>
        public void Init(Window myWindow, PriceMatchParam priceMatchParam)
        {
            this.myWindow = myWindow;
            _isUpdatePrice = priceMatchParam.UpdataPrice;
            
           
            _priceMatchParam = priceMatchParam;

            if (string.IsNullOrEmpty(priceMatchParam.SendOrderType))
            {
                if (PermisionHelper.Instance.isAllExec() || PermisionHelper.Instance.isOCO())
                {
                    priceMatchParam.SendOrderType = "OCO";
                }
            }

            bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            InitUI(priceMatchParam.Symbol, !isOfr);

            if (string.IsNullOrEmpty(priceMatchParam.Price))
            {
                priceMatchParam.Price = _sendPriceAdapter.GetInitBestPrice(isBid);
                this.CustomPrice = priceMatchParam.Price;
            }

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
                CustomPrice = d.ToString("N4").Replace(",", "");
            }
            else
            {
                CustomPrice = priceMatchParam.Price;
            }
            
            netPrice = string.IsNullOrEmpty(CustomPrice) ? "" : priceMatchParam.NetPrice;
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
            
            CrossView crossView = myWindow.FindName("crossView") as CrossView;
            CrossViewModel crossViewModel = crossView.DataContext as CrossViewModel;
            if (crossViewModel != null)
            {
                crossViewModel.SetPermission();
                crossViewModel.SetChannel(priceMatchParam);
                crossViewModel.changeHeight();
            }

            // Modi:cyc Date:2021-04-13 Func: 增加用户选择的 需求有效时长 选项记忆功能
            // string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : "0";

            DetailConditionViewModel detailConditionViewModel = GetDetailConditionViewModel();
            if (!string.IsNullOrEmpty(priceMatchParam.Info1))
            {
                detailConditionViewModel.SetValideTime(priceMatchParam.Info1);
            }
            else
            {
                UserSettingAdapter.Instance.ParseValidTimeFormat(this);
            }

            detailConditionViewModel.SetTagData(priceMatchParam.Info1, priceMatchParam.Side);

            UserSettingAdapter.Instance.SetPriceCondition(detailConditionViewModel, priceMatchParam.PriceCondition);

            isInit = false;

            CalBond("4", this._priceMatchParam.SettlType, NumChange(customPrice));

            this.FirmSelectIndex = !priceMatchParam.IsFirm ? 1 : 0;
            this.FirmChecked = FirmSelectIndex == 0 ? false : true;
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

                        if (settle.Equals(ADD_ONE_NOT_LIMIT))
                        {
                            SettleBoxIndex2 = 0;
                        }
                        else if (settle.Equals("3"))
                        {
                            SettleBoxIndex2 = 2;
                        }
                        else if (settle.Equals("2"))
                        {
                            SettleBoxIndex2 = 1;
                        }
                        else if (settle.Equals("+1"))
                        {
                            SettleBoxIndex2 = 0;
                        }
                        else if (settle.Equals("全"))
                        {
                            SettleBoxIndex2 = 0;
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


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        public void Init(Window myWindow, string security, bool bidOrOfr)
        {
            PriceMatchParam priceMatchParam = new PriceMatchParam();
            priceMatchParam.SendOrderType = "";
            priceMatchParam.Side = bidOrOfr ? "1" : "2";
            priceMatchParam.Symbol = security;
            priceMatchParam.SettlType = "2,3";
            priceMatchParam.Price = "";
            priceMatchParam.UpdataPrice = false;
            string firmtype = GetDefaultFirmTypeFromUserSetting();
            priceMatchParam.IsFirm = firmtype == "0" ? false : true;

            Init(myWindow, priceMatchParam);
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


        public PriceCondition GetPriceCondition()
        {
            ConditionCancelView conditionCancelView = myWindow.FindName("conditionView") as ConditionCancelView;
            PriceCondition priceCondition = null;
            if (conditionCancelView != null)
            {
                priceCondition = conditionCancelView.GetPriceCondition();
            }

            return priceCondition;
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
    }
}
