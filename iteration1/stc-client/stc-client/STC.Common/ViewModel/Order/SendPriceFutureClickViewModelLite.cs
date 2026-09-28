using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Price;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.DataProcesser;
using STC.Common.View;
using STC.Common.View.Card;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using STC.Common.Data;
using Newtonsoft.Json;
using STC.Common.Model.Position;
using STC.Common.Handler.Position;
using STC.Common.ViewModel.Card;

namespace STC.Common.ViewModel.Order
{
    /// <summary>
    /// 报价界面SendPointPriceView的数据内容
    /// </summary>
    public class SendPriceFutureClickViewModelLite : ViewModelBase
    {
        /// <summary>
        /// 渠道权限
        /// </summary>
        private readonly string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        private readonly string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        private readonly string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        private readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        private readonly string XBOND_TYPE = ">> X-bond报价";
        private readonly string NORMAL_TYPE = ">> 普通报价";
        private readonly string XBOND = "XBOND";

        private readonly string ADD_ONE_NOT_LIMIT = "2,3";

        public Window myWindow;
        private bool isBid = true;
        public bool IsBid
        {
            get { return isBid; }
            set
            {
                isBid = value;
                OnPropertyChanged(nameof(IsBid));
            }
        }
        private bool isInit = true;
        private bool _isUpdatePrice = false;
        private PriceMatchParam _priceMatchParam = null;
        private string currentGoodsType = string.Empty;
        private const string RANDOMTXT = "家渠道 (共{0}家)";
        private const string FIRM = "firm";
        private const string NONFIRM = "nonfirm";
        private const string YYYYMMDD = "yyyyMMdd";
        private bool _isXBond = false;

        private BondsInfoPo _bondsInfoPo = null;

        private SendPriceModelAdapter _sendPriceAdapter = null;

        public Action<NewOrderSingle> OnCustomConfirmAction;

        public ICommand ExpandCommand { get { return new ComRelayCommand(OnExpandClick); } }
        public static List<KeyValuePair<string, string>> LstBookList = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;

        #region 构造

        public SendPriceFutureClickViewModelLite()
        {
            //开平仓，投机套保套利默认值
            OpenSelected = true;
            CloseSelected = false;
            SpeculateSelected = true;
            HedgeSelected = false;
            ArbitrageSelected = false;
        }
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

        #endregion

        ~SendPriceFutureClickViewModelLite()
        {
            if (_sendPriceAdapter != null)
            {
                _sendPriceAdapter.Clear();
                LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
                if (this.timer != null)
                    this.timer.Stop();
            }
        }

        #region 界面属性

        private string sendBtnTxt = "Bid";
        public string SendBtnTxt
        {
            get { return sendBtnTxt; }
            set
            {
                sendBtnTxt = value;

                this.OnPropertyChanged();
            }
        }

        private string clickTxt = string.Empty;
        public string ClickTxt
        {
            get { return clickTxt; }
            set
            {
                clickTxt = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _addOneVisibility = Visibility.Visible;
        public Visibility AddOneVisibility
        {
            get { return _addOneVisibility; }
            set
            {
                _addOneVisibility = value;

                this.OnPropertyChanged();
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

                    CalBond("4", GetSetttle(), NumChange(customPrice));

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

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
            }
        }


        private int _randomMax = 0;

        public int RandomMax
        {
            get { return _randomMax; }
            set
            {
                _randomMax = value;
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
                    this.OnPropertyChanged(nameof(SendPriceItem));
                }
            }
        }

        private bool addEnable = false;
        public bool AddEnable
        {
            get
            {
                return addEnable;
            }

            set
            {
                addEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool downEnable = false;
        public bool DownEnable
        {
            get
            {
                return downEnable;
            }

            set
            {
                downEnable = value;

                this.OnPropertyChanged();
            }
        }

        #region  清算速度 +1 +0 远

        private int settleBoxIndex = 0;
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
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged(nameof(SettleBoxIndex));

                SetOtherVisibile();

                if (!oldIndex.Equals(value))
                {
                    UpdateBestPrice();
                    if (oldIndex == 2 && value != 2)
                    {
                        ChangeUI();
                    }
                }

                if ((value == 2) && oldIndex != 2)
                {
                    HideXboundCheckBoxAndDirectionItem();
                }
            }
        }

        #region 新增 +1 带DropDown的 ListItem 
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

                OnPropertyChanged(nameof(SettleBoxIndex2));
            }
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

        private string brokerWarning = "渠道：按住ctrl点击可复选";
        public string BrokerWarning
        {
            get { return brokerWarning; }
            set
            {
                brokerWarning = value;
                OnPropertyChanged(nameof(BrokerWarning));
            }
        }

        /// <summary>
        /// Ofr Or Bid 用于改变金额，数量的背景色
        /// </summary>
        private string ofrOrBid = "0";
        public string OfrOrBid
        {
            get { return ofrOrBid; }
            set
            {
                ofrOrBid = value;
                this.OnPropertyChanged(nameof(OfrOrBid));
            }
        }

        #endregion


        #endregion


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

        private bool firmTimeLimit = false;
        public bool FirmTimeLimit
        {
            get
            {
                return firmTimeLimit;
            }

            set
            {
                firmTimeLimit = value;
                AddEnable = DownEnable = firmTimeLimit;

                decimal d = 0;
                if (decimal.TryParse(sendPriceItem.NonfirmTime, out d))
                {
                    if (d >= 120)
                    {
                        AddEnable = false;
                        DownEnable = true;
                    }
                    else if (d <= 1)
                    {
                        AddEnable = true;
                        DownEnable = false;
                    }

                }

                this.OnPropertyChanged();
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

                //FirmTimeLimit = firmChecked;

            }

            FirmTxt = firmChecked ? NONFIRM : FIRM;
            if (_isXBond)
                SendPriceItem.FirmVisibility = Visibility.Collapsed;
            else
                SendPriceItem.FirmVisibility = Visibility.Collapsed;//firmChecked ? Visibility.Collapsed : Visibility.Visible;
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

                this.OnPropertyChanged(nameof(FirmChecked));

                if (!_isFromXBond)
                {
                    bool directCheck = directionCheck;
                    if (!_isSetPermission)
                    {
                        ChangeUI();
                    }

                    if (!_isXBond)
                    {
                        CheckXBond = _isXBond ? true : false;
                        CheckXBondVisibility = Visibility.Collapsed;

                        if (!_wholeFlagChecked && !_isSetPermission)
                        {
                            SetPermission();
                        }
                    }

                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");
                }

                UpdateBestPrice();

                SetPriceAndOrderHeighWholeFlag(value);
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

        private Visibility nonfirmcheckedvisible = Visibility.Visible;
        public Visibility NonFirmCheckedVisible
        {
            get
            {
                return nonfirmcheckedvisible;
            }

            set
            {
                nonfirmcheckedvisible = value;
                this.OnPropertyChanged(nameof(NonFirmCheckedVisible));
            }
        }
        #region listbox选中索引

        /// <summary>
        /// 重置渠道各个选项
        /// </summary>
        private void ResetListBoxSelected()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ListBox listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                listBox.SelectedIndex = -1;

                listBox = myWindow.FindName("lbFirstLevel") as ListBox;
                listBox.SelectedIndex = -1;

                listBox = myWindow.FindName("lbDirect") as ListBox;
                listBox.SelectedIndex = -1;
            }));
        }

        /// <summary>
        /// 余量发送渠道选择"默认"
        /// </summary>
        private bool defaultCheck = false;
        public bool DefaultCheck
        {
            get
            {
                return defaultCheck;
            }

            set
            {
                defaultCheck = value;

                if (!isInit)
                {
                    // ResetListBoxSelected();

                    if (defaultCheck)
                    {
                        RandomEnable = true;
                        // CheckNotOut = DirectionCheck = false;

                        sendPriceItem.DefaultItemEnable = true;

                        sendPriceItem.DirectionItemEnable = false;

                        ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                        listBox = myWindow.FindName("lbFirstLevel") as ListBox;
                        //SetListBoxItemEnable(listBox, true);

                        listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                        //SetListBoxItemEnable(listBox, true);

                    }
                    else
                    {
                        RandomEnable = false;
                        sendPriceItem.DefaultItemEnable = false;

                        ChangeBoxItemBG(DefaultFirstItems);
                        ChangeBoxItemBG(DefaultSecondItems);
                        ChangeBoxItemCheck(DefaultFirstItems, false);
                        ChangeBoxItemCheck(DefaultSecondItems, false);

                        ListBox listBox = myWindow.FindName("lbFirstLevel") as ListBox;

                        listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                    }
                }
                this.OnPropertyChanged();

                ResetQtyWarn();
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

            if ((myWindow.FindName("lbFollowingPrice") as ListBox).SelectedItems.Count <= 1)
            {
                sendPriceItem.CustomQty = "";
                sendPriceItem.MarketQty = "";
                RiskWarnText = "*请详细确认以上信息";
            }
            else
            {
                RiskWarnText = "*最大可成交量";
            }
        }

        private bool randomEnable = false;
        public bool RandomEnable
        {
            get
            {
                return randomEnable;
            }

            set
            {
                randomEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _directionEnable = true;
        public bool DirectionEnable
        {
            get
            {
                return _directionEnable;
            }

            set
            {
                _directionEnable = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 余量发送渠道选择”指定“
        /// </summary>
        private bool directionCheck = false;
        public bool DirectionCheck
        {
            get
            {
                return directionCheck;
            }

            set
            {
                directionCheck = value;

                if (!isInit)
                {
                    // ResetListBoxSelected();
                    if (directionCheck)
                    {
                        // CheckNotOut = DefaultCheck = false;
                        // 仅内部
                        // CheckNotOut =  false;

                        sendPriceItem.DirectionItemEnable = true;


                        ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                        listBox = myWindow.FindName("lbFirstLevel") as ListBox;

                        listBox = myWindow.FindName("lbSecondLevel") as ListBox;

                        //远期判断
                        if (_sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex) || (!_isXBond && firmChecked))
                        {
                            foreach (ListBoxItemData itemData in DirectionItems)
                            {
                                if (itemData.Name.ToUpper().Equals(XBOND))
                                    itemData.ItemEnable = false;
                            }
                        }
                    }
                    else
                    {
                        sendPriceItem.DirectionItemEnable = false;

                        ChangeBoxItemBG(DirectionItems);
                        ChangeBoxItemCheck(DirectionItems, false);


                        ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                    }
                }

                this.OnPropertyChanged(nameof(DirectionCheck));

                ResetQtyWarn();
            }
        }

        /// <summary>
        /// 更新指定渠道选项内容
        /// </summary>
        private void UpdateDirectionItem()
        {

            if (!_isXBond && firmChecked && DirectionCheck)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        if (itemData.Name.ToUpper().Equals(XBOND))
                            itemData.ItemEnable = false;
                    }
                }
            }
        }


        private int spotSettleBoxIndex = 0;

        public int SpotSettleBoxIndex
        {
            get
            {
                return spotSettleBoxIndex;
            }

            set
            {
                int oldIndex = spotSettleBoxIndex;
                spotSettleBoxIndex = value;

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged(nameof(SpotSettleBoxIndex));

                if (!oldIndex.Equals(value))
                    UpdateBestPrice();
            }
        }



        private Visibility _xbondVisibility = Visibility.Visible;

        public Visibility XBondVisibility
        {
            get
            {
                return _xbondVisibility;
            }

            set
            {
                _xbondVisibility = value;

                this.OnPropertyChanged(nameof(XBondVisibility));
            }
        }

        private Visibility spotSettleBoxVisibility = Visibility.Collapsed;

        public Visibility SpotSettleBoxVisibility
        {
            get
            {
                return spotSettleBoxVisibility;
            }

            set
            {
                Visibility old = spotSettleBoxVisibility;
                spotSettleBoxVisibility = value;

                this.OnPropertyChanged();

                if (value == Visibility.Visible && old != value)
                {
                    SpotSettleBoxIndex = 1;
                }
            }
        }

        private Visibility settleBoxVisibility = Visibility.Visible;

        public Visibility SettleBoxVisibility
        {
            get
            {
                return settleBoxVisibility;
            }

            set
            {
                Visibility old = settleBoxVisibility;
                settleBoxVisibility = value;

                this.OnPropertyChanged();
                if (value == Visibility.Visible && old != value)
                {
                    SettleBoxIndex = 0;
                }

            }
        }

        #endregion

        #region listbox 动态数据源
        private List<ListBoxItemData> defaultFirstItems;
        public List<ListBoxItemData> DefaultFirstItems
        {
            get
            {
                return defaultFirstItems;
            }

            set
            {
                defaultFirstItems = value;
                this.OnPropertyChanged();
            }
        }


        private List<ListBoxItemData> defaultSecondItems;
        public List<ListBoxItemData> DefaultSecondItems
        {
            get
            {
                return defaultSecondItems;
            }

            set
            {
                defaultSecondItems = value;
                this.OnPropertyChanged();
            }
        }
        private List<ListBoxItemData> directionItems;
        public List<ListBoxItemData> DirectionItems
        {
            get
            {
                return directionItems;
            }

            set
            {
                directionItems = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private ListBoxItem selectedItem = null;

        public ListBoxItem SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                selectedItem = value;
                this.OnPropertyChanged();
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

                    UpdateVenue(searchText);

                    RefreshUI();

                    InitPriceView(searchText);
                    if (isInit == false)
                    {
                        _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
                        UpdateBestPrice();
                        CustomPrice = _sendPriceAdapter.GetInitBestPrice3Decimal(isBid ? 1 : 4);
                        SetPriceFocus();
                    }
                }
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


        private void UpdateVenue(string security)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(security))
                {
                    string goodsType = string.Empty;
                    currentGoodsType = goodsType = CommonUtils.SetGoodsShortname(security);

                    List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");

                    DefaultFirstItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "1"));
                    DefaultSecondItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "2"));
                    if (DefaultSecondItems != null)
                        RandomMax = DefaultSecondItems.Count;

                    DirectionItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty));
                    SetItemParam(DirectionItems, goodsType, "3");

                    if (!DirectionCheck)
                    {
                        SetItemParam(DefaultSecondItems, goodsType, "2");
                    }

                    if (DefaultFirstItems != null)
                    {
                        foreach (ListBoxItemData itemData in DefaultFirstItems)
                            itemData.ItemEnable = false;
                    }

                    if (DefaultSecondItems != null)
                    {
                        foreach (ListBoxItemData itemData in DefaultSecondItems)
                            itemData.ItemEnable = false;
                    }
                    int count = 0;

                    if (DefaultSecondItems != null)
                        count += DefaultSecondItems.Count;


                    ChangeBoxItemBG(DirectionItems);
                    ChangeBoxItemCheck(DirectionItems, false);


                    ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                    RandomTxt = string.Format(RANDOMTXT, count);
                }
            }));
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
        //最新价 报价界面
        private string latestPrice = string.Empty;
        public string LatestPrice
        {
            get
            {
                return latestPrice;
            }

            set
            {
                latestPrice = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                this.OnPropertyChanged();
            }
        }
        
        //价格涨跌 报价界面
        private string deltaPrice = string.Empty;
        public string DeltaPrice
        {
            get
            {
                return deltaPrice;
            }

            set
            {
                deltaPrice = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                this.OnPropertyChanged();
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

                //if (oldPrice != null && value != null && !oldPrice.Equals(value))// && string.IsNullOrEmpty(value))
                //{
                //    //NetPrice = string.Empty;
                //    netPrice = string.Empty;
                //}
                ClickTxt = string.Format("{0} {1} {2}", SendPriceItem.Security, IsBid ? "Ofr" : "Bid", customPrice);
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

        private string _randomTxt;
        public string RandomTxt
        {
            get
            {
                return _randomTxt;
            }

            set
            {
                _randomTxt = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility _wholeFlagVisibility = Visibility.Visible;
        public Visibility WholeFlagVisibility
        {
            get
            {
                return _wholeFlagVisibility;
            }

            set
            {
                _wholeFlagVisibility = value;
                this.OnPropertyChanged(nameof(WholeFlagVisibility));

            }
        }

        private Visibility _darkDiskFlagVisibility = Visibility.Visible;
        public Visibility DarkDiskFlagVisibility
        {
            get
            {
                return _darkDiskFlagVisibility;
            }

            set
            {
                _darkDiskFlagVisibility = value;
                this.OnPropertyChanged();

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

                if (!_isSetPermission)
                {
                    ChangeUI();
                }

                OnWholeFlag();

                if (value)
                    ResetQtyWarn();

                SetPriceAndOrderHeighWholeFlag(value);
            }
        }

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

        //整量逻辑
        private void OnWholeFlag()
        {
            if (_wholeFlagChecked)
            {
                _checkXBond = false;
                this.OnPropertyChanged("CheckXBond");
                _checkXBondVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("CheckXBondVisibility");
            }
            else
            {
                if (!_isXBond)
                {
                    CheckXBond = _isXBond ? true : false;
                    CheckXBondVisibility = Visibility.Collapsed;

                    if (!_wholeFlagChecked && !_isSetPermission)
                    {
                        SetPermission();

                    }
                }
            }
        }

        private DateTime? selectDate;
        public DateTime? SelectDate
        {
            get { return selectDate; }
            set
            {
                string oldDate = selectDate == null ? string.Empty : selectDate.Value.Date.ToString(YYYYMMDD);
                selectDate = value;
                string newDate = selectDate == null ? string.Empty : selectDate.Value.Date.ToString(YYYYMMDD);
                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged(nameof(SelectDate));

                if (value != null && !oldDate.Equals(newDate))
                {
                    bool directCheck = directionCheck;

                    ChangeUI();

                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");

                    UpdateBestPrice();
                }
            }
        }

        private int comboBoxSelectIndex = 0;
        public int ComboBoxSelectIndex
        {
            get
            {
                return comboBoxSelectIndex;
            }

            set
            {
                int oldIndex = comboBoxSelectIndex;

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    if (_priceMatchParam == null)
                    {
                        CalBond("4", GetSetttle(), NumChange(customPrice));
                    }
                }

                if (!oldIndex.Equals(value))
                {
                    bool directCheck = directionCheck;

                    ChangeUI();

                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");

                    UpdateBestPrice();
                }

                comboBoxSelectIndex = value;

                this.OnPropertyChanged(nameof(ComboBoxSelectIndex));
            }
        }

        private Visibility _settlTypeComboxVisibility = Visibility.Visible;
        public Visibility SettlTypeComboxVisibility
        {
            get
            {
                return _settlTypeComboxVisibility;
            }

            set
            {
                _settlTypeComboxVisibility = value;
                LbOneVisibility = _settlTypeComboxVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                this.OnPropertyChanged(nameof(SettlTypeComboxVisibility));
            }
        }

        private Visibility _lbOneVisibility = Visibility.Collapsed;
        public Visibility LbOneVisibility
        {
            get
            {
                return _lbOneVisibility;
            }

            set
            {
                _lbOneVisibility = value;

                this.OnPropertyChanged(nameof(LbOneVisibility));
            }
        }

        private Visibility _checkAllVisibility = Visibility.Visible;
        public Visibility CheckAllVisibility
        {
            get
            {
                return _checkAllVisibility;
            }

            set
            {
                _checkAllVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _channelAllVisibility = Visibility.Visible;
        public Visibility ChannelAllVisibility
        {
            get
            {
                return _channelAllVisibility;
            }

            set
            {
                _channelAllVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private bool _checkNotoutEnable = true;
        public bool CheckNotoutEnable
        {
            get
            {
                return _checkNotoutEnable;
            }
            set
            {
                _checkNotoutEnable = value;
                this.OnPropertyChanged(nameof(CheckNotoutEnable));
            }
        }

        private bool _checkInner = false;
        public bool CheckInner
        {
            get
            {
                return _checkInner;
            }

            set
            {
                _checkInner = value;
                this.OnPropertyChanged(nameof(CheckInner));
                if (value)
                {
                    CheckXBond = false;
                    DirectionCheck = true;
                    CheckNotOut = true;
                    CheckNotoutEnable = false;
                }
                else
                {
                    CheckNotOut = false;
                    CheckNotoutEnable = this._isXBond ? false : true;
                }

            }
        }

        #region 是否可见

        private Visibility _checkInnerVisibility = Visibility.Collapsed;
        public Visibility CheckInnerVisibility
        {
            get
            {
                return _checkInnerVisibility;
            }

            set
            {
                _checkInnerVisibility = value;

                this.OnPropertyChanged(nameof(CheckInnerVisibility));
            }
        }

        private Visibility _checkXBondVisibility = Visibility.Collapsed;
        public Visibility CheckXBondVisibility
        {
            get
            {
                return _checkXBondVisibility;
            }

            set
            {
                _checkXBondVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private Visibility _chanelAllVisibility = Visibility.Collapsed;
        public Visibility ChanelAllVisibility
        {
            get
            {
                return _chanelAllVisibility;
            }

            set
            {
                _chanelAllVisibility = value;

                this.OnPropertyChanged();
            }
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


        private Visibility _innerDefaultVisibility = Visibility.Visible;
        public Visibility InnerDefaultVisibility
        {
            get
            {
                return _innerDefaultVisibility;
            }

            set
            {
                _innerDefaultVisibility = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 【余量发送渠道】选择【不发送外部】
        /// </summary>
        private bool _checkNotOut = false;
        public bool CheckNotOut
        {
            get
            {
                return _checkNotOut;
            }

            set
            {
                _checkNotOut = value;

                if (value)
                {
                    DefaultCheck = DirectionCheck = false;
                    ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                    listBox = myWindow.FindName("lbFirstLevel") as ListBox;

                    listBox = myWindow.FindName("lbSecondLevel") as ListBox;

                }
                else
                {
                    // 指定
                    DirectionCheck = true;
                    AppointClick();
                }

                this.OnPropertyChanged(nameof(CheckNotOut));

                ResetQtyWarn();
            }
        }

        /// <summary>
        /// 跟盘价 checkbox
        /// </summary>
        private bool _followingPriceCheck = false;
        public bool FollowingPriceCheck
        {
            get
            {
                return _followingPriceCheck;
            }
            set
            {
                _followingPriceCheck = value;
                this.OnPropertyChanged(nameof(FollowingPriceCheck));
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

        /// <summary>
        /// 投机
        /// </summary>
        private bool _speculateSelected = false;
        public bool SpeculateSelected
        {
            get
            {
                return _speculateSelected;
            }
            set
            {
                _speculateSelected = value;
                this.OnPropertyChanged(nameof(SpeculateSelected));
            }
        }

        /// <summary>
        /// 套利
        /// </summary>
        private bool _arbitrageSelected = false;
        public bool ArbitrageSelected
        {
            get
            {
                return _arbitrageSelected;
            }
            set
            {
                _arbitrageSelected = value;
                this.OnPropertyChanged(nameof(ArbitrageSelected));
            }
        }

        /// <summary>
        /// 套保
        /// </summary>
        private bool _hedgeSelected = false;
        public bool HedgeSelected
        {
            get
            {
                return _hedgeSelected;
            }
            set
            {
                _hedgeSelected = value;
                this.OnPropertyChanged(nameof(HedgeSelected));
            }
        }

        /// <summary>
        /// 撮合渠道选择X-Bond
        /// </summary>
        private bool _checkXBond = false;
        public bool CheckXBond
        {
            get
            {
                return _checkXBond;
            }

            set
            {
                _checkXBond = value;

                this.OnPropertyChanged(nameof(CheckXBond));

                if (value)
                {
                    CheckInner = false;

                    ////if (InnerDefaultVisibility == Visibility.Collapsed && myWindow.Height == 488 && CheckXBondVisibility == Visibility.Visible)
                    if (CheckXBondVisibility == Visibility.Visible)
                    {
                        if (!_checkXBond)
                        {
                            DirectionEnable = DirectionCheck = false;

                            if (DirectionItems != null)
                            {
                                foreach (ListBoxItemData itemData in DirectionItems)
                                {
                                    if (!itemData.Name.ToUpper().Equals(XBOND))
                                        itemData.ItemEnable = false;
                                }
                            }
                        }
                        else
                        {
                            DirectionEnable = true;

                            if (!CheckNotOut)
                                DirectionCheck = true;

                            if (DirectionItems != null)
                            {
                                foreach (ListBoxItemData itemData in DirectionItems)
                                {
                                    if (!itemData.Name.ToUpper().Equals(XBOND))
                                    {
                                        itemData.ItemVisibility = Visibility.Collapsed;
                                        itemData.Checked = false;
                                    }
                                    else
                                    {
                                        if (!CheckNotOut)
                                        {
                                            itemData.Checked = true;
                                            itemData.ItemEnable = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool _checkInnerEnable = true;
        public bool CheckInnerEnable
        {
            get
            {
                return _checkInnerEnable;
            }

            set
            {
                _checkInnerEnable = value;

                this.OnPropertyChanged(nameof(CheckInnerEnable));
            }
        }

        private bool _checkXBondEnable = true;
        public bool CheckXBondEnable
        {
            get
            {
                return _checkXBondEnable;
            }

            set
            {
                _checkXBondEnable = value;

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
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
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


        #region 渠道权限和交易权限
        /// <summary>
        /// 渠道权限-仅内部
        /// </summary>
        private bool IsHaveInnerPermission = false;

        /// <summary>
        /// 渠道权限-内部，X-Bond
        /// </summary>
        private bool IsHaveInnerXbondPermission = false;

        /// <summary>
        /// 渠道权限-所有权限
        /// </summary>
        private bool IsHaveAllPermission = false;

        /// <summary>
        /// 是否有Xbond撮合权限
        /// </summary>
        private bool IsHaveVenueXbondPermission = false;

        /// <summary>
        /// 是否有内部撮合权限
        /// </summary>
        private bool _isHaveVenueInnerPermission = false;
        public bool IsHaveVenueInnerPermission
        {
            get
            {
                return _isHaveVenueInnerPermission;
            }

            set
            {
                _isHaveVenueInnerPermission = value;
                this.OnPropertyChanged(nameof(IsHaveVenueInnerPermission));
            }
        }

        /// <summary>
        /// 撮合渠道是否可见
        /// </summary>
        private Visibility venuevisible = Visibility.Visible;
        public Visibility VenueVisible
        {
            get { return venuevisible; }
            set
            {
                venuevisible = value;
                this.OnPropertyChanged("VenueVisible");
            }
        }

        /// <summary>
        /// Bid最新价/涨跌 
        /// </summary>
        private Visibility bidLatestPriceVisible = Visibility.Collapsed;
        public Visibility BidLatestPriceVisible
        {
            get { return bidLatestPriceVisible; }
            set
            {
                bidLatestPriceVisible = value;
                this.OnPropertyChanged("BidLatestPriceVisible");
            }
        }

        /// <summary>
        /// Ofr最新价/涨跌 是否可见
        /// </summary>
        private Visibility ofrLatestPriceVisible = Visibility.Collapsed;
        public Visibility OfrLatestPriceVisible
        {
            get { return ofrLatestPriceVisible; }
            set
            {
                ofrLatestPriceVisible = value;
                this.OnPropertyChanged("OfrLatestPriceVisible");
            }
        }
        #endregion

        private DispatcherTimer timer = null;
        private bool isHvaeNewExecution = false;

        #region  报单类型属性
        private int ordTypeSelectIndex = 0;

        public int OrdTypeSelectIndex
        {
            get
            {
                return ordTypeSelectIndex;
            }

            set
            {
                ordTypeSelectIndex = value;
                this.OnPropertyChanged(nameof(OrdTypeSelectIndex));
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
                this.OnPropertyChanged(nameof(DefaultValidUntilTime));
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

                this.OnPropertyChanged(nameof(DefaultValidTimeQuotationCheck));
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
                this.OnPropertyChanged(nameof(HourValidTimeQuotationCheck));
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
                    this.OnPropertyChanged(nameof(ValidMinute));
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
                    this.OnPropertyChanged(nameof(ValidSecond));
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
                    this.OnPropertyChanged(nameof(ValidTimeHour));
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
                    this.OnPropertyChanged(nameof(ValidTimeMinute));
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
                    this.OnPropertyChanged(nameof(ValidTimeSecond));
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
                this.OnPropertyChanged(nameof(TimeValidTimeQuotationCheck));
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
                this.OnPropertyChanged(nameof(NoLimitValidTimeQuotationCheck));
            }
        }
        #endregion

        #endregion

        private bool isSendReq = false;
        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
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
            //calBondErrMsg = string.Empty;
            //decimal d = 0;
            //if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) || !_needCalPrice)
            //    return;

            ////isSendReq = true;

            //_lastReqData = string.Format("{0}-{1}-{2}", calType, settleType, val);
            //ThreadPool.QueueUserWorkItem(o =>
            //{
            //    try
            //    {
            //        CalReq requst = new CalReq();
            //        requst.SecurityID = sendPriceItem.Security;
            //        requst.CalType = calType;//"4";
            //        requst.SettlType = settleType;// "1";
            //        requst.Value = val;// customPrice;
            //        if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
            //            requst.CalPath = LocalEnv.ConfInfo.CalType;

            //        CalResult result = CalHandler.Instance.CalBond(requst);
            //        if (requst != null)
            //        {
            //            if (result.Data != null && result.IsSuccess())
            //            {
            //                result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);

            //                if (_lastReqData.Equals(result.ReqData))
            //                {
            //                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //                    {
            //                        if (result.IsSuccess() && sendPriceItem.CalWarnVisibility == Visibility.Visible)
            //                            sendPriceItem.CalWarnVisibility = Visibility.Collapsed;

            //                        lock (_calLock)
            //                        {
            //                            _lastCalResult = result;

            //                        }

            //                        if (calType.Equals("4"))
            //                        {
            //                            //NetPrice = result.Data.CleanPrice.ToString("F4");

            //                            netPrice = result.Data.CleanPrice.ToString("F4");
            //                            this.OnPropertyChanged("NetPrice");
            //                        }
            //                        else if (calType.Equals("2"))//净价
            //                        {
            //                            if (result.IsSuccess())
            //                            {
            //                                customPrice = (result.Data.Yield * 100).ToString("F4");

            //                                this.OnPropertyChanged("CustomPrice");
            //                            }
            //                            else
            //                            {
            //                                if (_isXBond)
            //                                    sendPriceItem.CalWarnVisibility = Visibility.Visible;
            //                            }
            //                        }

            //                        isSendReq = false;
            //                    }));
            //                }
            //            }
            //        }
            //        else
            //        {
            //            isSendReq = false;
            //            calBondErrMsg = result.Msg;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        LogHelper.Error(e);
            //        isSendReq = false;
            //    }
            //    finally
            //    {
            //        //isSendReq = false;
            //    }
            //});
        }

        private bool _isFromXBond = false;
        /// <summary>
        /// 切换UI显示内容
        /// </summary>
        private void ChangeUI()
        {

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_isXBond)
                {
                    _checkInnerEnable = false;
                    _checkXBondEnable = false;

                    _wholeFlagVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("WholeFlagVisibility");

                    this.OnPropertyChanged("CheckInnerEnable");
                    this.OnPropertyChanged("CheckXBondEnable");

                    selectDate = DateTime.Now;
                    this.OnPropertyChanged("SelectDate");

                    // ComboBoxSelectIndex = 0;

                    if (CheckAllVisibility == Visibility.Visible)
                    {
                        this.OnPropertyChanged("CheckAllVisibility");
                        this.OnPropertyChanged("CheckAll");
                    }

                    if (CheckInnerVisibility == Visibility.Visible)
                    {
                        _checkInner = true;
                        this.OnPropertyChanged("CheckInner");
                    }

                    if (CheckXBondVisibility == Visibility.Visible)
                    {
                        _checkXBond = true;
                        this.OnPropertyChanged("CheckXBond");
                    }


                    spotSettleBoxVisibility = Visibility.Visible;
                    this.OnPropertyChanged("SpotSettleBoxVisibility");

                    //spotSettleBoxIndex = 0;
                    //this.OnPropertyChanged("SpotSettleBoxIndex");
                    firmChecked = false;
                    this.OnPropertyChanged("FirmChecked");

                    _firmCheckedEnabled = false;
                    this.OnPropertyChanged("FirmCheckedEnabled");

                    FirmCheckUpdate();

                    settleBoxVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("SettleBoxVisibility");

                    SendPriceItem.DateVisible = Visibility.Collapsed;
                    SendPriceItem.SettleTypeVisible = Visibility.Collapsed;


                    OnlyShowXBond();

                }
                else
                {
                    _wholeFlagVisibility = Visibility.Visible;
                    this.OnPropertyChanged("WholeFlagVisibility");

                    if (!FirmChecked)
                        CheckInnerEnable = true;

                    CheckXBondEnable = true;

                    _firmCheckedEnabled = true;
                    this.OnPropertyChanged("FirmCheckedEnabled");

                    spotSettleBoxVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("SpotSettleBoxVisibility");

                    settleBoxVisibility = Visibility.Visible;
                    this.OnPropertyChanged("SettleBoxVisibility");

                    _innerDefaultVisibility = Visibility.Visible;
                    this.OnPropertyChanged("InnerDefaultVisibility");

                    ////if (CheckAll)
                    ////    myWindow.Height = 488;

                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = Visibility.Visible;
                        }
                    }
                }

                if (FirmChecked)
                {
                    if (CheckXBondVisibility == Visibility.Visible)
                    {
                        _checkXBondVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckXBondVisibility");

                        _checkXBond = _isXBond ? true : false;
                        this.OnPropertyChanged("CheckXBond");
                    }

                    UpdateDirectionItem();
                }
                else
                {
                    bool isAllow = IsHaveAllPermission;
                    if (isAllow)
                    {
                        _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                        this.OnPropertyChanged("CheckAllVisibility");
                    }
                    else
                    {
                        isAllow = IsHaveInnerXbondPermission;

                        //if (IsHaveInnerPermission) 
                        //{
                        _checkAllVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckAllVisibility");
                        //}

                        _checkXBondVisibility = (isAllow && IsHaveVenueXbondPermission) ? Visibility.Visible : Visibility.Collapsed;
                        this.OnPropertyChanged("CheckXBondVisibility");

                        if (isAllow)
                            _checkXBond = _isXBond ? true : false;
                        else
                            _checkXBond = false;
                        this.OnPropertyChanged("CheckXBond");

                        //CheckXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                        //CheckXBond = false;
                    }
                }

                //if (_wholeFlagChecked || FirmChecked)
                if (_wholeFlagChecked)
                {
                    if (CheckInnerVisibility == Visibility.Visible)
                    {
                        _checkXBondVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckXBondVisibility");

                        _checkAllVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckAllVisibility");

                        _checkInner = true;
                        this.OnPropertyChanged("CheckInner");

                        //CheckInner = true;
                        //CheckXBondVisibility = CheckAllVisibility = Visibility.Collapsed;
                    }
                }

                //远期判断
                if ((SettleBoxIndex == 2 && !_isXBond && _sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || firmChecked)
                {
                    HideXboundCheckBoxAndDirectionItem();
                }
                else
                {
                    if (!(FirmChecked || _wholeFlagChecked))
                    {
                        bool isAllow = IsHaveAllPermission;
                        if (isAllow)
                        {
                            if (!FirmChecked)
                            {
                                _checkXBondVisibility = IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;
                                this.OnPropertyChanged("CheckXBondVisibility");

                                // 恢复原订单值
                                if (this.IsCanChangeXond())
                                {
                                    CheckXBond = true;
                                }
                                else
                                {
                                    CheckXBond = _isXBond ? true : false;
                                }
                            }
                        }
                        else
                        {
                            isAllow = IsHaveInnerXbondPermission && !FirmChecked;

                            _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                            this.OnPropertyChanged("CheckXBondVisibility");

                            _checkXBond = _isXBond ? true : false;
                            this.OnPropertyChanged("CheckXBond");

                        }
                    }

                    // 恢复非远期时关闭的 xbond
                    if (CheckXBondVisibility != Visibility.Visible)
                    {
                        if (IsHaveInnerXbondPermission || IsHaveAllPermission)
                        {
                            if (IsHaveVenueXbondPermission)
                            {
                                CheckXBondVisibility = Visibility.Visible;
                            }
                        }
                    }

                    if (DirectionCheck && DirectionItems != null && !FirmChecked)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            if (itemData.Name.ToUpper().Equals(XBOND))
                            {
                                itemData.ItemEnable = !CheckNotOut;
                            }
                        }
                    }


                }

                _isFromXBond = false;
            }));
        }

        /// <summary>
        /// ”X-Bond报价“是否可用
        /// </summary>
        /// <returns></returns>
        private bool IsShowXBondItem()
        {
            return !((SettleBoxIndex == 2 && !_isXBond && _sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || firmChecked);
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
        /// 隐藏Xbond单选框和Xbond交易渠道
        /// </summary>
        private void HideXboundCheckBoxAndDirectionItem()
        {
            if (CheckXBondVisibility == Visibility.Visible)
            {
                CheckXBondVisibility = Visibility.Collapsed;
                CheckXBond = false;
            }

            if (DirectionCheck)
            {
                if (DirectionItems != null)
                {
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        if (itemData.Name.ToUpper().Equals(XBOND))
                            itemData.ItemEnable = false;
                    }
                }
            }
        }


        /// <summary>
        /// 仅显示X-Bond报价
        /// </summary>
        private void OnlyShowXBond()
        {
            _innerDefaultVisibility = Visibility.Collapsed;
            this.OnPropertyChanged("InnerDefaultVisibility");
            ////myWindow.Height = 488;
            AllowanceChannelVisibility = Visibility.Visible;
            if (_isXBond)
            {
                directionCheck = true;
                this.OnPropertyChanged("DirectionCheck");


                _directionEnable = false;
                this.OnPropertyChanged("DirectionEnable");

            }
            else
            {
                if (CheckInner && CheckInnerVisibility == Visibility.Visible && CheckXBond && CheckXBondVisibility == Visibility.Visible)
                {
                    _checkNotOut = false;
                    this.OnPropertyChanged("CheckNotOut");
                    defaultCheck = false;
                    this.OnPropertyChanged("DefaultCheck");

                    directionCheck = true;
                    this.OnPropertyChanged("DirectionCheck");
                }
            }

            if (DirectionItems != null)
            {
                foreach (ListBoxItemData itemData in DirectionItems)
                {
                    if (!itemData.Name.ToUpper().Equals(XBOND))
                    {
                        itemData.ItemVisibility = Visibility.Collapsed;
                        itemData.Checked = false;
                    }
                    else
                    {
                        itemData.Checked = !CheckNotOut;

                        if (_isXBond)
                        {
                            itemData.ItemEnable = false;
                        }
                        else
                        {
                            if (!CheckNotOut)
                            {
                                itemData.ItemEnable = true;
                            }
                            else
                            {
                                itemData.Checked = false;
                                itemData.ItemEnable = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 标题栏报价方式选择显示内容
        /// </summary>
        private string _sendPriceType = string.Empty;
        public string SendPriceType
        {
            get
            {
                return _sendPriceType;
            }

            set
            {
                _sendPriceType = value;
                this.OnPropertyChanged();
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
        public ICommand XBondCommand { get { return new ComRelayCommand(OnXBondClick); } }

        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new ComRelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new ComRelayCommand(ReducePriceClick); } }
        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }

        #endregion

        #region UI Amout,Price  调整
        private void AddPriceClick()
        {
            double price = 0;
            if (double.TryParse(SendPriceItem.CustomPrice, out price))
            {
                price = price + 0.005;
                if (price >= -100 && price <= 9999)
                {
                    SendPriceItem.CustomPrice = price.ToString("F3");
                    CustomPrice = price.ToString("F3");
                }
            }
        }
        private void ReducePriceClick()
        {
            double price = 0;
            if (double.TryParse(SendPriceItem.CustomPrice, out price))
            {
                price = price - 0.005;
                if (price >= -100 && price <= 9999)
                {
                    SendPriceItem.CustomPrice = price.ToString("F3");
                    CustomPrice = price.ToString("F3");
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


            SetAmountTxtSelectIndex();
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

            SetAmountTxtSelectIndex();
        }


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
                    OnFiveClick();
                }
                else if (setAmountTxtIndex == 2)
                {
                    OnTenClick();
                }
                else if (setAmountTxtIndex == 3)
                {
                    OnTwentyClick();
                }
                else if (setAmountTxtIndex == 4)
                {
                    OnFiftyClick();
                }
                this.OnPropertyChanged(nameof(SetAmountTxtIndex));
            }
        }

        /// <summary>
        /// 下单量1手
        /// </summary>
        private void OnOneClick()
        {
            SendPriceItem.Amount = "1";
        }

        /// <summary>
        /// 下单量5手
        /// </summary>
        private void OnFiveClick()
        {
            SendPriceItem.Amount = "5";
        }

        /// <summary>
        /// 下单量10手
        /// </summary>
        private void OnTenClick()
        {
            SendPriceItem.Amount = "10";
        }

        /// <summary>
        /// 下单量20手
        /// </summary>
        private void OnTwentyClick()
        {
            SendPriceItem.Amount = "20";
        }

        /// <summary>
        /// 下单量50手
        /// </summary>
        private void OnFiftyClick()
        {
            SendPriceItem.Amount = "50";
        }
        #endregion

        /// <summary>
        /// 切换至X-Bond报价
        /// </summary>
        private void OnXBondClick()
        {
            DefaultCheck = false;
            DirectionCheck = true;
            _isXBond = !_isXBond;

            // 标题按钮改为显示 ">> 普通报价"
            SendPriceType = _isXBond ? NORMAL_TYPE : XBOND_TYPE;
            NonFirmCheckedVisible = SendPriceType == NORMAL_TYPE ? Visibility.Collapsed : Visibility.Visible;
            CheckNotoutEnable = SendPriceType == NORMAL_TYPE ? false : true;

            sendPriceItem.NetPriceVisible = _isXBond ? Visibility.Visible : Visibility.Collapsed;
            _isFromXBond = true;
            ChangeUI();

            if (!_isXBond)
            {
                bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                if (isAllow)
                {
                    ////myWindow.Height = 488;
                    CheckAllVisibility = Visibility.Visible;

                    //OnCheckInner(false);
                }

                SettleBoxIndex = 0;
                SendPriceItem.FirmVisibility = Visibility.Collapsed;//firmChecked ? Visibility.Collapsed : Visibility.Visible;
                DarkDiskFlagVisibility = Visibility.Visible;

                SettleBoxVisibility = Visibility.Visible;
                SettlTypeComboxVisibility = Visibility.Visible;

                // 显示 Price + -
                Button btnAddPrice = myWindow.FindName("btnAddPrice") as Button;
                Button btnReducePrice = myWindow.FindName("btnReducePrice") as Button;
                btnAddPrice.Visibility = Visibility.Visible;
                btnReducePrice.Visibility = Visibility.Visible;

                TextBlock textBlcok_BestPrice = myWindow.FindName("textBlcok_BestPrice") as TextBlock;
                textBlcok_BestPrice.Text = "市场最优";
                textBlcok_BestPrice.FontSize = 12;
                Grid bor_AddOrReduce = myWindow.FindName("bor_AddOrReduce") as Grid;
                bor_AddOrReduce.Width = 36;

                Button btnAddAmout = myWindow.FindName("btnAddAmout") as Button;
                Button btnReduceAmout = myWindow.FindName("btnReduceAmout") as Button;
                btnAddAmout.Visibility = Visibility.Visible;
                btnReduceAmout.Visibility = Visibility.Visible;

                TextBox priceTxt = myWindow.FindName("PriceTxt") as TextBox;
                priceTxt.Width = 145;
                priceTxt.FontSize = 24;

                TextBox netPrice = myWindow.FindName("netPrice") as TextBox;
                netPrice.FontSize = 24;

                TextBox amountTxt = myWindow.FindName("AmountTxt") as TextBox;
                amountTxt.Width = 145;
                amountTxt.Margin = new Thickness(10, 2, 0, 0);
                amountTxt.FontSize = 24;

                TextBlock amountTxtKw = myWindow.FindName("AmountTxtKw") as TextBlock;
                amountTxtKw.Margin = new Thickness(124, 30, 2, 0);

                ListBox ConstAmount = myWindow.FindName("ConstAmount") as ListBox;
                ConstAmount.BorderThickness = new Thickness(0, 0, 0, 0);

                ListBoxItem constAmount2 = myWindow.FindName("ConstAmount2") as ListBoxItem;
                constAmount2.Width = 47;
                constAmount2.FontSize = 12;
                ListBoxItem constAmount3 = myWindow.FindName("ConstAmount3") as ListBoxItem;
                constAmount3.Width = 47;
                constAmount3.FontSize = 12;
                ListBoxItem constAmount5 = myWindow.FindName("ConstAmount5") as ListBoxItem;
                constAmount5.Width = 47;
                constAmount5.FontSize = 12;
            }
            else
            {
                // Xbond NetPrice SendPrice
                SettleBoxIndex = 0;
                SettleBoxIndex2 = 0;
                AddOneVisibility = Visibility.Collapsed;

                SendPriceItem.FirmVisibility = Visibility.Collapsed;
                DarkDiskFlagVisibility = Visibility.Collapsed;

                SettleBoxVisibility = Visibility.Collapsed;
                SettlTypeComboxVisibility = Visibility.Collapsed;


                // 隐藏 Price + -
                Button btnAddPrice = myWindow.FindName("btnAddPrice") as Button;
                Button btnReducePrice = myWindow.FindName("btnReducePrice") as Button;
                btnAddPrice.Visibility = Visibility.Collapsed;
                btnReducePrice.Visibility = Visibility.Collapsed;

                TextBlock textBlcok_BestPrice = myWindow.FindName("textBlcok_BestPrice") as TextBlock;
                textBlcok_BestPrice.Text = "最优";
                textBlcok_BestPrice.FontSize = 11;
                Grid bor_AddOrReduce = myWindow.FindName("bor_AddOrReduce") as Grid;
                bor_AddOrReduce.Width = 0;

                Button btnAddAmout = myWindow.FindName("btnAddAmout") as Button;
                Button btnReduceAmout = myWindow.FindName("btnReduceAmout") as Button;
                btnAddAmout.Visibility = Visibility.Collapsed;
                btnReduceAmout.Visibility = Visibility.Collapsed;

                TextBox priceTxt = myWindow.FindName("PriceTxt") as TextBox;
                priceTxt.Width = 130;
                priceTxt.FontSize = 20;
                // priceTxt.Margin = new Thickness(10, -10, 0, 0);

                TextBox netPrice = myWindow.FindName("netPrice") as TextBox;
                netPrice.Width = 100;
                netPrice.FontSize = 18;

                TextBox amountTxt = myWindow.FindName("AmountTxt") as TextBox;
                amountTxt.Width = 116;
                amountTxt.Margin = new Thickness(10, 2, 0, 0);
                amountTxt.FontSize = 20;

                TextBlock amountTxtKw = myWindow.FindName("AmountTxtKw") as TextBlock;
                amountTxtKw.Margin = new Thickness(102, 30, 2, 0);

                ListBox ConstAmount = myWindow.FindName("ConstAmount") as ListBox;
                ConstAmount.BorderThickness = new Thickness(0, 0, 0, 0);
                ListBoxItem constAmount2 = myWindow.FindName("ConstAmount2") as ListBoxItem;
                constAmount2.Width = 38;
                constAmount2.FontSize = 11;
                ListBoxItem constAmount3 = myWindow.FindName("ConstAmount3") as ListBoxItem;
                constAmount3.Width = 38;
                constAmount3.FontSize = 11;
                ListBoxItem constAmount5 = myWindow.FindName("ConstAmount5") as ListBoxItem;
                constAmount5.Width = 38;
                constAmount5.FontSize = 11;

            }
        }

        /// <summary>
        /// 增加转nofirm的时间
        /// </summary>
        private void OnAddClick()
        {
            if (string.IsNullOrEmpty(sendPriceItem.NonfirmTime))
                sendPriceItem.NonfirmTime = "1";
            else
            {
                decimal d = 0;
                if (decimal.TryParse(sendPriceItem.NonfirmTime, out d))
                {
                    if ((d >= 120))
                    {
                        AddEnable = false;
                    }
                    else
                        sendPriceItem.NonfirmTime = (d + 1).ToString();

                    DownEnable = d > 1;
                }
            }
        }

        /// <summary>
        /// 减少转nofirm的时间
        /// </summary>
        private void OnDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(sendPriceItem.NonfirmTime, out d) && (d - 1) >= 0)
            {
                if (d <= 1)
                {
                    DownEnable = false;
                }
                else
                    sendPriceItem.NonfirmTime = (d - 1).ToString();

                AddEnable = (d - 1) < 120;
            }
        }


        public void SetAmountTxtSelectIndex()
        {
            List<string> valueList = new List<string>() { "1", "5", "10", "20", "50" };
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
        /// 设置渠道相关的界面显示状态
        /// </summary>
        /// <param name="priceMatchParam"></param>
        private void SetChannel(PriceMatchParam priceMatchParam)
        {
            if (!string.IsNullOrEmpty(priceMatchParam.CrossVenues))
            {
                if (priceMatchParam.CrossVenues.Equals("1"))
                {
                }
                else if (priceMatchParam.CrossVenues.Equals("2"))
                {
                    CheckInner = true;
                }
                else if (priceMatchParam.CrossVenues.Equals("3"))
                {
                    CheckXBond = true;
                }
            }

            if (!string.IsNullOrEmpty(priceMatchParam.VenuesSelectF))
            {
                if (priceMatchParam.VenuesSelectF.Equals("1"))
                    CheckNotOut = true;
                else if (priceMatchParam.VenuesSelectF.Equals("2"))
                {
                    DefaultCheck = true;

                    SendPriceItem.RandomNum = priceMatchParam.VenuesRdmNum;
                    RandomEnable = true;
                }
                else if (priceMatchParam.VenuesSelectF.Equals("3"))
                    DirectionCheck = true;
            }

            if (!string.IsNullOrEmpty(priceMatchParam.SelfSelectVenues))
            {
                //ListBox listBox = myWindow.FindName("lbDirect") as ListBox;
                string[] arr = priceMatchParam.SelfSelectVenues.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (ListBoxItemData itemData in listBox.SelectedItems)
                bool isContain = false;
                foreach (ListBoxItemData itemData in DirectionItems)
                {
                    isContain = false;
                    //if (itemData.Name.ToUpper().Equals(XBOND))
                    //    itemData.ItemEnable = IsShowXBondItem();
                    //else
                    {
                        itemData.ItemEnable = true;

                        string tem = string.Format("{0}-{1}", itemData.Venuetypegw, itemData.Code);

                        foreach (string str in arr)
                        {
                            if (str.ToUpper().Equals(tem.ToUpper()))
                            {
                                itemData.Checked = true;
                                isContain = true;
                            }
                        }

                        if (!isContain)
                        {
                            itemData.Checked = false;
                        }

                        if (itemData.Name.ToUpper().Equals(XBOND))
                        {
                            itemData.ItemEnable = IsShowXBondItem();
                            if (!itemData.ItemEnable)
                                itemData.Checked = false;
                        }
                    }
                }
            }
            else
            {
                //foreach (ListBoxItemData itemData in DirectionItems)
                //{
                //    itemData.ItemEnable = false;
                //    itemData.Checked = false;
                //}
            }

        }

        private bool _isSetPermission = false;
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
            _isSetPermission = true;
            bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            InitUI(priceMatchParam.Symbol, !isOfr);

            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                _wholeFlagChecked = true;

            }
            else
                _wholeFlagChecked = false;

            if (priceMatchParam.CustomFirmAmount != null)
            {
                if (priceMatchParam.Symbol.Contains("."))
                    SendPriceItem.Amount = (Decimal.Parse(priceMatchParam.CustomFirmAmount) / 1000).ToString();
                else
                    SendPriceItem.Amount = (Decimal.Parse(priceMatchParam.CustomFirmAmount)).ToString();
            }


            decimal d = 0;

            if (decimal.TryParse(priceMatchParam.Price, out d))
            {
                CustomPrice = d.ToString("N3").Replace(",", "");
            }
            else
            {
                CustomPrice = priceMatchParam.Price;
            }


            netPrice = priceMatchParam.NetPrice;
            this.OnPropertyChanged("NetPrice");


            //if (decimal.TryParse(priceMatchParam.Price, out d))
            //    CustomPrice = d.ToString("N4");
            //else
            //    CustomPrice = priceMatchParam.Price;

            //NetPrice = priceMatchParam.NetPrice;

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

            if (priceMatchParam.IsFirm == true && priceMatchParam.NonfirmTime.Length > 0)
            {
                FirmTimeLimit = true;
            }

            //join方式采用偏好设置参数
            if (!(!string.IsNullOrEmpty(priceMatchParam.OperationType) && (priceMatchParam.OperationType.Equals("1") || priceMatchParam.OperationType.Equals("2"))))
            {
                SendPriceItem.NonfirmTime = priceMatchParam.NonfirmTime;
                if (!string.IsNullOrEmpty(SendPriceItem.NonfirmTime))
                    FirmTimeLimit = true;
                else
                    FirmTimeLimit = false;
            }


            if (!string.IsNullOrEmpty(priceMatchParam.RandomNum))
            {
                SendPriceItem.RandomNum = priceMatchParam.RandomNum;

                DefaultCheck = true;
            }


            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                WholeFlagChecked = true;
            }
            else
                WholeFlagChecked = false;


            ParseSettle(_priceMatchParam.SettlType);


            SetPermission();

            SetChannel(priceMatchParam);


            if (!string.IsNullOrEmpty(priceMatchParam.Terminal) && priceMatchParam.Terminal.EndsWith("xbond"))
            {
                this.OnXBondClick();

                if (!string.IsNullOrEmpty(_priceMatchParam.SettlType))
                {
                    if (_priceMatchParam.SettlType.Equals("1") || priceMatchParam.SettlType.Equals("T+0"))
                    {
                        SpotSettleBoxIndex = 1;
                    }
                    else
                    {
                        SpotSettleBoxIndex = 0;
                    }
                }
            }

            // Modi:cyc Date:2021-04-13 Func: 增加用户选择的 需求有效时长 选项记忆功能
            // string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : "0";

            if (!string.IsNullOrEmpty(priceMatchParam.Info1))
            {
                if (JsonSplit.IsJson(priceMatchParam.Info1))
                {
                    OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(priceMatchParam.Info1);

                    //if (otm.ValidTimeType.StartsWith("0|"))
                    //{
                    //    validSelectIndex = 0;
                    //    DefaultValidTimeQuotationCheck = true;
                    //}
                    //else if (otm.ValidTimeType.StartsWith("1|"))
                    //{
                    //    validSelectIndex = 1;
                    //    HourValidTimeQuotationCheck = true;
                    //    // 时长
                    //    if (HourValidTimeQuotationCheck)
                    //    {
                    //        // ValidMinute,ValidSecond
                    //        string s = otm.ValidTimeType.Split('|')[1];
                    //        if (s.Contains(":"))
                    //        {
                    //            ValidMinute = s.Split(':')[0];
                    //            ValidSecond = s.Split(':')[1];
                    //        }
                    //    }
                    //}
                    //else if (otm.ValidTimeType.StartsWith("2|"))
                    //{
                    //    validSelectIndex = 2;
                    //    TimeValidTimeQuotationCheck = true;
                    //    // 时点
                    //    if (TimeValidTimeQuotationCheck)
                    //    {
                    //        // ValidTimeHour ValidTimeMinute ValidTimeSecond
                    //        string s = otm.ValidTimeType.Split('|')[1];
                    //        if (s.Contains(":"))
                    //        {
                    //            ValidTimeHour = s.Split(':')[0];
                    //            ValidTimeMinute = s.Split(':')[1];
                    //            ValidTimeSecond = s.Split(':')[2];
                    //        }
                    //    }
                    //}
                    //else if (otm.ValidTimeType.StartsWith("3|"))
                    //{
                    //    validSelectIndex = 3;
                    //    NoLimitValidTimeQuotationCheck = true;
                    //    // 无
                    //}
                }
            }

            isInit = false;
            _isSetPermission = false;

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
                                SelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                            }
                            catch (Exception e) { }

                            ComboBoxSelectIndex = isTomorrow ? 0 : 1;
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

        private bool _needCalPrice = true;
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
                if (!_isXBond)
                {
                    if (!string.IsNullOrEmpty(settle))
                    {

                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                        {
                            SettleBoxIndex = 0;
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
                            SettleBoxIndex = 0;
                            if (settle.Equals("1") || settle.Equals("T+0") || settle.Equals("+0"))//T+0
                            {
                                try
                                {
                                    SelectDate = DateTime.Now;
                                    SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                                }
                                catch (Exception e) { }
                                //_needCalPrice = true;
                                SettleBoxIndex = 1;
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
                                SettleBoxIndex = 2;
                            }
                            else
                            {
                                string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                                int month = 0;
                                int day = 0;
                                if (arr != null && arr.Length > 1)
                                {
                                    SettleBoxIndex = 2;
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
                                            SelectDate = new DateTime?(dt);
                                            SendPriceItem.CustomSettleDate = dt.ToString("yyy-MM-dd");
                                        }
                                        catch (Exception e) { }
                                    }
                                    //_needCalPrice = true;
                                    if (arr[1].Equals("0"))
                                    {
                                        ComboBoxSelectIndex = 0;
                                        SendPriceItem.SettleType = "T+0";
                                    }
                                    else if (arr[1].Equals("1"))
                                    {
                                        ComboBoxSelectIndex = 1;
                                        SendPriceItem.SettleType = "T+1";
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (settle.Equals("2"))
                        SpotSettleBoxIndex = 0;
                    else if (settle.Equals("1"))
                        SpotSettleBoxIndex = 1;
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
        /// 初始化界面
        /// </summary>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        private void InitUI(string security, bool isOfr)
        {
            // 判断是否是买入
            isBid = isOfr;
            SendBtnTxt = isBid ? "Bid" : "Ofr";
            
            this.IsBidSelect = isBid ? "1" : "0";
            _sendPriceAdapter = new SendPriceModelAdapter();

            // 默认标题按钮显示 ">> X-bond报价"
            SendPriceType = XBOND_TYPE;
            string goodsType = string.Empty;
            currentGoodsType = goodsType = CommonUtils.SetGoodsShortname(security);

            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = security;
            SendPriceItem.Digit = 3;
            if (!string.IsNullOrEmpty(security))
            {
                _bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(security);
                if (_bondsInfoPo != null)
                {
                    // this.SearchText = security;
                    ShowText = $"{SendPriceItem.Security} {_bondsInfoPo.Short_Name}";
                }
            }

            if (isBid)
            {
                OnBidClick();

                SendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            }
            else
            {
                OnOfrClick();

                SendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            }

            List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");

            SendPriceItem.NonfirmTime = SystemParamHandler.Instance.GetFirmtonontime();
            if (!string.IsNullOrEmpty(SendPriceItem.NonfirmTime))
                FirmTimeLimit = true;


            DefaultFirstItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "1"));
            DefaultSecondItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "2"));
            if (DefaultSecondItems != null)
                RandomMax = DefaultSecondItems.Count;
            DirectionItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty));
            SetItemParam(DirectionItems, goodsType, "3");

            // 是否有内部交易权限
            IsHaveVenueInnerPermission = GetIsHaveInner(goodsType);

            // 获取渠道权限
            IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
            IsHaveInnerXbondPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
            IsHaveAllPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            IsHaveVenueXbondPermission = CheckVenuePermission(XBOND);

            if ((!IsHaveInnerPermission) && (!IsHaveInnerXbondPermission) && (!IsHaveAllPermission))
            {
                VenueVisible = Visibility.Collapsed;
                SendPriceEnable = false;
                CheckNotoutEnable = false;
                CheckNotOut = true;
                RiskWarnText = "* 没有渠道撮合权限";
            }
            else
            {
                this.VenueVisible = (!IsHaveVenueInnerPermission && !IsHaveVenueXbondPermission) ? Visibility.Collapsed : Visibility.Visible;
            }
            if (!DirectionCheck)
            {
                SetItemParam(DefaultSecondItems, goodsType, "2");
            }

            // 缺省报量
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
                if (!string.IsNullOrEmpty(Us.userQty))
                {
                    SendPriceItem.Amount = (Decimal.Parse(Us.userQty) / 1000).ToString();
                }
                if (!string.IsNullOrEmpty(Us.inf2))
                {
                    if (JsonSplit.IsJson(Us.inf2))
                    {
                        ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                        SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
                        PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;
                    }
                }

            }

            if (!DirectionCheck && !DefaultCheck)
            {
                if (userVenues != null && userVenues.Count > 0)
                {
                    foreach (UserVenue uv in userVenues)
                    {
                        if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("1"))
                        {
                            CheckNotOut = true;

                            break;
                        }
                    }
                }
            }

            if (DefaultFirstItems != null)
            {
                foreach (ListBoxItemData itemData in DefaultFirstItems)
                    itemData.ItemEnable = false;
            }

            if (DefaultSecondItems != null)
            {
                foreach (ListBoxItemData itemData in DefaultSecondItems)
                    itemData.ItemEnable = false;
            }
            int count = 0;

            if (DefaultSecondItems != null)
                count += DefaultSecondItems.Count;

            RandomTxt = string.Format(RANDOMTXT, count);

            // SearchText = SendPriceItem.Security;

            //_sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());
            if (_priceMatchParam != null)
            {
                _sendPriceAdapter.SetFilterParam(false, _priceMatchParam.SettlType);
            }
            else
            {
                _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
            }
            SendPriceItem.FollowingPrice = FollowingPriceCheck;
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
            LstBookList.Clear();
            GetBookListByUser();
        }

        public void GetBookListByUser()
        {
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = PositionHandler.Instance.QueryPositionBook(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        LstBookList.Add(new KeyValuePair<string, string>(pbm.id, $"{pbm.id}-{pbm.bookname}"));
                    }
                }
            }
        }

        private bool GetIsHaveInner(string goodsType)
        {
            List<Venue> LstHaveInner = SystemParamHandler.Instance.GetListVenueByFilter(goodsType, "1");
            if (LstHaveInner != null)
            {
                var result = LstHaveInner.Where(delegate (Venue uv)
                {
                    if (uv.venuetypegw == "INNER")
                        return true;
                    else
                        return false;
                }).ToList();

                return result.Count > 0 ? true : false;
            }
            else
                return false;
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

        //private string GetNewSetttle()
        //{
        //    string settle = "2,3";
        //    if (AddOneOpenedTag == "1")
        //        settle = AddOneNotLimitCheck ? "2,3" : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : "2,3";
        //    if (AddOneOpenedTag == "0")
        //    {
        //        if (SettleBoxIndex == 0)
        //        {
        //            settle = "1";
        //        }
        //        else
        //        {
        //            try
        //            {
        //                settle = DictSettleBox[SettleBoxIndex];
        //            }
        //            catch
        //            { }
        //        }
        //    }

        //    return settle;
        //}

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        public void Init(Window myWindow, string security, bool bidOrOfr)
        {
            isBid = bidOrOfr;
            this.myWindow = myWindow;
            myWindow.Unloaded += MyWindow_Unloaded;

            InitUI(security, bidOrOfr);

            SetPermission();

            sendPriceItem.CustomPrice = _sendPriceAdapter.GetInitBestPrice3Decimal(isBid ? 1 : 4);

            isInit = false;
        }

        private bool CheckVenuePermission(string venue)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(venue) && DirectionItems != null && DirectionItems.Count > 0)
            {
                foreach (ListBoxItemData listBoxItemData in DirectionItems)
                {
                    if (listBoxItemData.Name.ToUpper() == (venue.ToUpper()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
                result = true;

            return result;
        }

        /// <summary>
        /// 设置功能权限
        /// </summary>
        private void SetPermission()
        {
            bool isAllow = IsHaveAllPermission;
            if (isAllow)
            {
                if (this.IsHaveVenueInnerPermission)
                {
                    CheckInnerVisibility = Visibility.Visible;
                }
                else
                {
                    CheckInnerVisibility = Visibility.Collapsed;
                }

                if (!firmChecked)
                {
                    _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                    this.OnPropertyChanged("CheckAllVisibility");

                    _checkXBondVisibility = IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    ////myWindow.Height = _isXBond ? 488 : 488;// 548;
                }

                if (_isXBond)
                {
                    CheckInner = false;
                    CheckXBond = true;

                }
                else
                {
                    if (!_wholeFlagChecked && !firmChecked)
                    {
                        if (!CheckInner)
                        {
                            CheckInner = false;
                        }

                        CheckXBond = false;
                    }
                    else
                    {
                        CheckAllVisibility = Visibility.Collapsed;
                        CheckInnerVisibility = Visibility.Visible;
                        CheckInnerEnable = false;
                        CheckInner = true;
                        CheckXBond = false;
                    }
                    //_checkXBondVisibility = Visibility.Collapsed;
                    //this.OnPropertyChanged("CheckXBondVisibility");
                }

                if (IsHaveInnerXbondPermission || IsHaveAllPermission)
                {
                    if (!IsHaveVenueXbondPermission)
                    {
                        XBondVisibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                _checkAllVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("CheckAllVisibility");


                ////myWindow.Height = _isXBond ? 488 : 394;

                _chanelAllVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("ChanelAllVisibility");

                isAllow = IsHaveInnerPermission;
                if (isAllow)
                {
                    CheckInnerVisibility = Visibility.Visible;

                    CheckInner = true;

                    //_checkXBondVisibility = Visibility.Collapsed;
                    //this.OnPropertyChanged("CheckXBondVisibility");

                    XBondVisibility = Visibility.Collapsed;
                }
                else
                {
                    isAllow = IsHaveInnerXbondPermission;

                    _checkXBondVisibility = isAllow && IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    CheckInnerVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;

                    if (isAllow)
                    {
                        CheckInner = true;
                        // CheckXBond = true;
                    }

                    if (IsHaveInnerXbondPermission || IsHaveAllPermission)
                    {
                        if (!IsHaveVenueXbondPermission)
                        {
                            XBondVisibility = Visibility.Collapsed;
                        }
                    }
                }
            }


            //OnCheckInner(_checkInner);
        }


        /// <summary>
        /// operationType/venuesselectf: 1: 不发送外部 2:默认 3:指定
        /// </summary>
        /// <param name="itemDatas"></param>
        /// <param name="goodsType"></param>
        /// <param name="operationType"></param>
        private void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                List<UserVenue> userItemDatas = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, operationType);

                if (operationType.Equals("3") && userItemDatas != null && userItemDatas.Count > 0)
                    DirectionCheck = true;

                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = SendPriceItem.BorderBH;

                    foreach (UserVenue uv in userItemDatas)
                    {
                        if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                        {
                            item.Checked = true;
                            DirectionCheck = true;

                            break;
                        }

                        if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("2"))
                        {
                            DefaultCheck = true;
                            SendPriceItem.RandomNum = uv.venuesrdmnum;
                        }
                    }
                }

                if (DirectionCheck)
                {
                    foreach (ListBoxItemData item in itemDatas)
                    {
                        item.ItemEnable = true;
                    }
                }
            }
        }

        private void MyWindow_Unloaded(object sender, RoutedEventArgs e)
        {
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
            SelectDate = DateTime.Now;
            NetPrice = string.Empty;
            sendPriceItem.NonfirmTime = string.Empty;
            SettleBoxIndex = 0;
            SettleBoxIndex2 = 0;
            //sendPriceItem.SettleType = string.Empty;
            //ComboBoxSelectIndex = 0;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;

        }

        /// <summary>
        /// 点击取消按钮
        /// </summary>
        private void OnCancelClick()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();

            if (myWindow != null)
                myWindow.Close();
        }

        /// <summary>
        /// 获得清算速度
        /// </summary>
        /// <returns></returns>
        private string GetSetttle(bool isfilter = false)
        {
            string settle = string.Empty;

            if (!_isXBond)
            {
                if (SettleBoxIndex == 0)
                    settle = AddOneNotLimitCheck ? ADD_ONE_NOT_LIMIT : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : ADD_ONE_NOT_LIMIT;
                else if (SettleBoxIndex == 1)
                {
                    settle = "1";
                }
                else
                {
                    // 如果不是获取最优价
                    if (!isfilter)
                    {
                        if (SelectDate != null && SelectDate.HasValue)
                        {
                            if (_sendPriceAdapter.IsSpotSettle(SelectDate, ComboBoxSelectIndex))
                                settle = _sendPriceAdapter.IsTomorrow(SelectDate, ComboBoxSelectIndex) ? "2" : "1";//2:明天+0转T+1
                            else
                                settle = SelectDate.Value.ToString("MM.dd") + (ComboBoxSelectIndex == 0 ? "+0" : "+1");
                        }
                    }
                    else
                    {
                        settle = "4"; //远期
                    }
                }
            }
            else
            {
                if (SpotSettleBoxIndex == 0)
                    settle = "2";
                else if (SpotSettleBoxIndex == 1)
                    settle = "1";

            }

            if (string.IsNullOrEmpty(settle))
                settle = "2,3";//默认+1

            return settle;
        }


        /// <summary>
        /// 更新订单（即先撤单后重下）
        /// </summary>
        private void UpdatePrice()
        {
            NewOrderSingle newOrder = GetNewOrderSingle();
            newOrder.Info5 = "转firm订单";
            OrdersHandler.Instance.ModifyOrders(newOrder, _priceMatchParam.OrderId, UpdatePriceCallBack);
        }

        /// <summary>
        /// 更新订单后的结果回调
        /// </summary>
        /// <param name="reply"></param>
        private void UpdatePriceCallBack(ModifyOrdersReply reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {

                if (reply.Code == "10000" || reply.IsSuccess())
                {
                    SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                    warnView.Owner = Application.Current.MainWindow;
                    //warnView.OnAction += WarnView_OnAction;
                    //if (!reply.IsSuccess())
                    //{
                    //    //warnView.OnAction += WarnView_OnAction;
                    //    //warnView.Owner = myWindow;
                    //    warnView.ShowDialog();
                    //}
                    //else
                    warnView.Show();
                }
                else
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    if (this.myWindow.IsActive)
                    {
                        msgBox.Owner = this.myWindow;
                    }
                    msgBox.setWarnMsg(reply.Msg);
                    msgBox.ShowDialog();

                    //OnCancelClick();
                }

                //if (!reply.IsSuccess())
                //{
                //    SendPriceWarnView warnView = new SendPriceWarnView(reply, GetNewOrderSingle());
                //    //warnView.Init(reply.Msg, GetNewOrderSingle());
                //    warnView.Show();
                //}

                //SendPriceEnable = true;

                OnCancelClick();
            }));
        }

        private bool CheckBondType()
        {
            return _bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.MarketIndicator) && _bondsInfoPo.MarketIndicator.Equals("4");
        }

        /// <summary>
        /// 生成订单报文-国债期货
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();
            ComboBox cbBook = myWindow.FindName("cbBookId") as ComboBox;
            sendPricePo.BookID = (string)cbBook.SelectedValue;
            sendPricePo.AlgoName = "STP";
            sendPricePo.VenueTypeGW = "CTP";
            sendPricePo.Venues = "CTP";
            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            if (_priceMatchParam != null)
            {
                sendPricePo.Info1 = _priceMatchParam.Info1;
                sendPricePo.Info2 = _priceMatchParam.Info2;
                sendPricePo.Info3 = _priceMatchParam.Info3;
                sendPricePo.Info4 = _priceMatchParam.Info4;
                sendPricePo.Info5 = _priceMatchParam.Info5;
            }

            sendPricePo.MarketIndicator = Consts.MARKETINDICATOR_FUTURE;
            //报单类型 2=限价，1=市价
            sendPricePo.OrdType = OrdTypeSelectIndex == 0 ? "2" : "1";
            if (!string.IsNullOrEmpty(sendPriceItem.Amount))
            {
                if (sendPriceItem.Security.Contains("."))
                    sendPricePo.OrderQty = (decimal.Parse(sendPriceItem.Amount) * 1000).ToString();
                else
                    sendPricePo.OrderQty = (decimal.Parse(sendPriceItem.Amount)).ToString();
            }
            //报价
            sendPricePo.Price = SendPriceItem.CustomPrice;
            sendPricePo.HedgeFlag = SpeculateSelected ? "1" : HedgeSelected ? "3" : "2";    //1=投机，2=套利，3=套保
            sendPricePo.OffsetFlag = OpenSelected ? "0" : "1";
            //if (sendPricePo.MarketIndicator == "4")
            //    sendPricePo.NetPrice = NetPrice;
            //else
            //    sendPricePo.NetPrice = "100";

            sendPricePo.SecurityID = sendPriceItem.Security;

            //孔要求把这个字段去掉
            //sendPricePo.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            //国债期货忽略清算速度
            sendPricePo.SettlType = "1";

            sendPricePo.Side = isBid ? "1" : "2";

            sendPricePo.Terminal = string.Format(_isXBond ? "STClient_v{0}_xbond" : "STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            sendPricePo.CrossVenues = _checkInner ? "2" : string.Empty;

            if (_checkXBond && _checkXBondVisibility == Visibility.Visible)
            {
                sendPricePo.CrossVenues = "3";
                //sendPricePo.Venues = "ODM"; //ODM=XB  QDMESP=双边
            }


            sendPricePo.HandlInst = _darkDiskFlagChecked ? "2" : "1";

            sendPricePo.QuantID = "";

            if (!(_checkInner && _checkXBondVisibility == Visibility.Collapsed))
                sendPricePo.VenuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;

            if (defaultCheck)
            {
                sendPricePo.SelfSelectVenues = string.Empty;
            }

            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            // 用户设置的到期时间,需要大于用户自定义的时间

            if (_priceMatchParam != null)
            {
                sendPricePo.Info3 = _priceMatchParam.Info3;
                sendPricePo.Info4 = _priceMatchParam.Info4;
            }

            if (_priceMatchParam != null && _priceMatchParam.OldOrder != null)
            {
                sendPricePo.AlgoName = _priceMatchParam.OldOrder.AlgoName;
                sendPricePo.VenueTypeGW = _priceMatchParam.OldOrder.VenueTypeGW;


                sendPricePo.UserID = _priceMatchParam.OldOrder.UserID;
                sendPricePo.UserName = _priceMatchParam.OldOrder.UserName;
                sendPricePo.BizType = _priceMatchParam.OldOrder.BizType;
                sendPricePo.BizTypeID = _priceMatchParam.OldOrder.BizTypeID;
                sendPricePo.RoleID = _priceMatchParam.OldOrder.RoleID;
                sendPricePo.RoleName = _priceMatchParam.OldOrder.RoleName;
                sendPricePo.QuantID = _priceMatchParam.OldOrder.QuantID;
                //sendPricePo.QuoteID = _priceMatchParam.OldOrder.QuoteID;
                sendPricePo.StrategyOrderId = _priceMatchParam.OldOrder.StrategyOrderId;
                sendPricePo.InstitutionID = _priceMatchParam.OldOrder.InstitutionID;
                sendPricePo.InstitutionName = _priceMatchParam.OldOrder.InstitutionName;
                sendPricePo.TraderID = _priceMatchParam.OldOrder.TraderID;
                sendPricePo.TraderName = _priceMatchParam.OldOrder.TraderName;
            }

            if (_priceMatchParam != null)
            {
                sendPricePo.InValidFlag = _priceMatchParam.InValidFlag;
                sendPricePo.TrigEvent = _priceMatchParam.TrigEvent;
                sendPricePo.CancEvent = _priceMatchParam.CancEvent;
                sendPricePo.EREventName = _priceMatchParam.EREventName;
                sendPricePo.CancEventName = _priceMatchParam.CancEventName;
            }

            //期货都是Firm
            sendPricePo.FirmFlag = "1";
            return sendPricePo;
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
            DateTime cudatetime = DateTimeEx.ServerTime;
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
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 06:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 12:00:00"));
                LstTime.Add(DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00"));
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
        /// 下单
        /// </summary>
        private void PlaceOrder()
        {
            NewOrderSingle sendPricePo = GetNewOrderSingle();
            SendPriceEnable = false;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    PResult result = OrdersHandler.Instance.PlaceOrder(sendPricePo);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {

                            if (result != null)
                            {
                                if (result.Code == "10000" || result.IsSuccess())
                                {
                                    SendPriceWarnView warnView = new SendPriceWarnView(result, sendPricePo);
                                    warnView.OnAction += WarnView_OnAction;
                                    if (!result.IsSuccess())
                                    {
                                        //warnView.OnAction += WarnView_OnAction;
                                        //warnView.Owner = myWindow;
                                        warnView.ShowDialog();
                                    }
                                    else
                                    {
                                        warnView.Owner = Application.Current.MainWindow;
                                        warnView.Show();
                                    }
                                }
                                else
                                {
                                    MessageBoxGUI msgBox = new MessageBoxGUI();
                                    msgBox.setWarnMsg(result.Msg);
                                    msgBox.Owner = this.myWindow;
                                    msgBox.ShowDialog();

                                    OnCancelClick();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error(e);

                            OnCancelClick();
                        }
                        //finally
                        //{
                        //    if (result.IsSuccess())
                        //        OnCancelClick();
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 处理报价警告窗口的用户操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isCancel"></param>
        /// <param name="isDoubleCheck"></param>
        private void WarnView_OnAction(object sender, bool isCancel, bool isDoubleCheck)
        {
            SendPriceWarnView warnView = sender as SendPriceWarnView;
            warnView.OnAction -= WarnView_OnAction;
            if (!isCancel)
                OnCancelClick();
            else
                SendPriceEnable = true;
        }

        /// <summary>
        /// 点击“报价”按钮
        /// </summary>
        public void OnConfirmClick()
        {
            this.myWindow.Topmost = false;

            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击报价按钮");
            }
            SendPriceEnable = false;

            if (CheckParam())
            {
                //参考SendPriceViewModuleLite
                if (OnCustomConfirmAction != null)
                {
                    OnCustomConfirmAction(GetNewOrderSingle());
                    return;
                }

                if (_isUpdatePrice)
                    UpdatePrice();
                else
                    PlaceOrder();
            }
            else
            {
                SendPriceEnable = true;
                SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle(), "5");
                warnView.Owner = myWindow;
                warnView.Show();
            }
        }

        /// <summary>
        /// 报价错误信息
        /// </summary>
        /// <returns></returns>
        private BaseReplyPo GetErrorTxt()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10001";

            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            decimal temD = 0;

            if (CheckAllVisibility == Visibility.Collapsed && CheckInnerVisibility == Visibility.Collapsed && CheckXBondVisibility == Visibility.Collapsed)
                por.Reason = "没有撮合渠道权限";
            else if (string.IsNullOrEmpty(sendPriceItem.Security))
                por.Reason = "未选择债券";
            else if (!decimal.TryParse(sendPriceItem.CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
                por.Reason = "未填写价格或价格为0";
            else if (!decimal.TryParse(sendPriceItem.Amount, out temD) || temD == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
                por.Reason = "未填写量或量为0";
            else if (decimal.TryParse(sendPriceItem.CustomPrice, out temD))
            {
                if (sendPriceItem.UpperLimitPrice.HasValue)
                {
                    //报价大于涨跌价
                    if (decimal.Compare(temD, sendPriceItem.UpperLimitPrice.Value) > 0)
                    {
                        por.Reason = "报价不能超过涨停价";
                    }
                }
                if (sendPriceItem.LowerLimitPrice.HasValue)
                {
                    //报价小于跌停价
                    if (decimal.Compare(temD, sendPriceItem.LowerLimitPrice.Value) < 0)
                    {
                        por.Reason = "报价不能低于跌停价";
                    }
                }
            }

            if (!OpenSelected && !CloseSelected)  //开平仓未选择
            {
                por.Reason = "开平仓未选择";
            }
            if (!SpeculateSelected && !HedgeSelected && !ArbitrageSelected)  //投机/套保/套利未选择
            {
                por.Reason = "投机/套保/套利未选择";
            }
            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        private BaseReplyPo GetErrorTxt(BaseReplyPo reply)
        {
            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            por.Reason = reply.Msg;

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        private bool CheckParam()
        {
            bool result = true;
            decimal temD = 0;

            if (string.IsNullOrEmpty(sendPriceItem.Security))
            {
                result = false;
            }
            else if (!decimal.TryParse(sendPriceItem.CustomPrice, out temD) || temD == 0) //报的价格不对或者为零
            {
                result = false;
            }
            else if (!decimal.TryParse(sendPriceItem.Amount, out temD) || temD == 0)//报的量不对或者为零
            {
                result = false;
            }
            else if (decimal.TryParse(sendPriceItem.CustomPrice, out temD))
            {
                if (sendPriceItem.UpperLimitPrice.HasValue)
                {
                    //报价大于涨跌价
                    if (decimal.Compare(temD, sendPriceItem.UpperLimitPrice.Value) > 0)
                    {
                        result = false;
                    }
                }
                if (sendPriceItem.LowerLimitPrice.HasValue)
                {
                    //报价小于跌停价
                    if (decimal.Compare(temD, sendPriceItem.LowerLimitPrice.Value) < 0)
                    {
                        result = false;
                    }
                }
            }

            if (!OpenSelected && !CloseSelected)  //开平仓未选择
            {
                result = false;
            }
            if (!SpeculateSelected&& !HedgeSelected && !ArbitrageSelected)  //投机/套保/套利未选择
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void CloseClick()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();

            myWindow.Close();
        }

        /// <summary>
        /// 勾选"指定"单选框
        /// </summary>
        private void AppointClick()
        {
            if (sendPriceItem.Appoint)
                sendPriceItem.ChannelIndex = 0;
            else
                sendPriceItem.ChannelIndex = -1;
        }

        /// <summary>
        /// 清算时间切换至其他
        /// </summary>
        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0 || SettleBoxIndex == 1)
            {
                SendPriceItem.DateVisible = Visibility.Collapsed;
                SendPriceItem.SettleTypeVisible = Visibility.Collapsed;
                // SelectDate = null;
                // ComboBoxSelectIndex = 0;
            }
            else if (SettleBoxIndex == 2)
            {
                ComboBoxSelectIndex = 0;
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
                                    comboBoxSelectIndex = 0;
                                }
                                else if (arr[1].Equals("1"))
                                {
                                    comboBoxSelectIndex = 1;
                                }
                                this.OnPropertyChanged("ComboBoxSelectIndex");
                            }
                        }
                    }
                }
                catch (Exception e) { }

                //ComboBoxSelectIndex = 0;
                //SendPriceItem.SettleType = "T+0";
            }
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
                    if (requst != null && result.Data != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                string year = result.Data.settlementDate.Substring(0, 4);
                                string month = result.Data.settlementDate.Substring(4, 2);
                                string day = result.Data.settlementDate.Substring(6, 2);

                                SelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                            }
                            catch (Exception e) { }

                            ComboBoxSelectIndex = 0;
                            SendPriceItem.SettleType = "T+0";

                            isSendReq = false;
                        }));
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
        /// 切换至Bid标签
        /// </summary>
        private void OnBidClick()
        {
            isBid = true;
            this.IsBidSelect = isBid ? "1" : "0";
            FirmChecked = false;
            CheckNotOut = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("BidBtnGradientBrush");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("MainFormBackground");
            sendPriceItem.ShowBidEffect = (Effect)myWindow.FindResource("BidEffect");
            sendPriceItem.ShowOfrEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
            sendPriceItem.NetPriceVisible = Visibility.Collapsed;

            //_isXBond = false;

            ChangeBoxItemBG(DefaultFirstItems);
            ChangeBoxItemBG(DefaultSecondItems);
            ChangeBoxItemBG(DirectionItems);

            SetItemParam(DirectionItems, currentGoodsType, "3");
            //SetItemParam(DirectionItems, currentGoodsType, "2");
            ChangeBoxItemCheck(DefaultFirstItems, false);
            ChangeBoxItemCheck(DefaultSecondItems, false);

            if (DirectionCheck)
                DefaultCheck = false;

            // SetPermission();

            // OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            if (isInit == false)
            {
                FollowingPriceCheck = false;
                UpdateCustomPrice();
                SetPriceFocus();
            }
            BidLatestPriceVisible = Visibility.Visible;
            OfrLatestPriceVisible = Visibility.Collapsed;
        }

        /// <summary>
        /// 设置渠道的标签的背景色
        /// </summary>
        /// <param name="itemDatas"></param>
        private void ChangeBoxItemBG(List<ListBoxItemData> itemDatas)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData itemData in itemDatas)
                {
                    itemData.BgColor = SendPriceItem.BorderBH;
                }
            }
        }

        /// <summary>
        /// 设置所有指定渠道的勾选
        /// </summary>
        /// <param name="itemDatas">指定渠道选项列表</param>
        /// <param name="checkStatus">需要设置的勾选状态， true：勾选； false：取消勾选</param>
        private void ChangeBoxItemCheck(List<ListBoxItemData> itemDatas, bool checkStatus)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData itemData in itemDatas)
                {
                    itemData.Checked = checkStatus;
                }
            }
        }

        /// <summary>
        /// 切换至Ofr标签
        /// </summary>
        private void OnOfrClick()
        {
            isBid = false;
            this.IsBidSelect = isBid ? "1" : "0";
            FirmChecked = false;
            CheckNotOut = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("MainFormBackground");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("OfrBtnGradientBrush");
            sendPriceItem.ShowOfrEffect = (Effect)myWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
            sendPriceItem.NetPriceVisible = Visibility.Collapsed;

            //_isXBond = false;

            ChangeBoxItemBG(DefaultFirstItems);
            ChangeBoxItemBG(DefaultSecondItems);
            ChangeBoxItemBG(DirectionItems);

            ChangeBoxItemCheck(DefaultFirstItems, false);
            ChangeBoxItemCheck(DefaultSecondItems, false);

            SetItemParam(DirectionItems, currentGoodsType, "3");
            //SetItemParam(DirectionItems, currentGoodsType, "2");

            if (DirectionCheck)
                DefaultCheck = false;

            // SetPermission();

            // OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            if (isInit == false)
            {
                FollowingPriceCheck = false;
                UpdateCustomPrice();
                SetPriceFocus();
            }
            BidLatestPriceVisible = Visibility.Collapsed;
            OfrLatestPriceVisible = Visibility.Visible;
        }

        /*
         * 根据情况更新报价框价格  
         */
        private void UpdateCustomPrice()
        {
            if (FollowingPriceCheck)
            {
                //切换Bid Ofr标签，如果跟盘价打开，默认为对价
                ListBoxItem follwingPrice1 = (myWindow.FindName("FollowingPrice1") as ListBoxItem); //对价
                ListBoxItem follwingPrice5 = (myWindow.FindName("FollowingPrice5") as ListBoxItem); //最新价
                ListBoxItem follwingPrice10 = (myWindow.FindName("FollowingPrice10") as ListBoxItem);//市价
                int priceType = 1;
                if (isBid)
                {
                    sendPriceItem.PriceType = follwingPrice1.IsSelected ? 1 : follwingPrice5.IsSelected ? 2 : follwingPrice10.IsSelected ? 3 : 1; //默认是Bid对价=1
                }
                else
                {
                    sendPriceItem.PriceType = follwingPrice1.IsSelected ? 4 : follwingPrice5.IsSelected ? 5 : follwingPrice10.IsSelected ? 6 : 4; //默认是Ofr对价=4
                }
                sendPriceItem.CustomPrice = _sendPriceAdapter.GetInitBestPrice3Decimal(sendPriceItem.PriceType);
            }
            else
            {
                sendPriceItem.CustomPrice = _sendPriceAdapter.GetInitBestPrice3Decimal(isBid ? 1 : 4);
            }
        }

        private void OnRefreshClick()
        {

        }


        /// <summary>
        /// 展开
        /// </summary>
        private void OnExpandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (myWindow != null)
                {
                    Button btn_Expand = (Button)myWindow.FindName("btn_Expand");

                    Border Border_Left1 = (Border)myWindow.FindName("Border_Left1");
                    Border Border_Left2 = (Border)myWindow.FindName("Border_Left2");
                    Border Border_Left3 = (Border)myWindow.FindName("Border_Left3");
                    Border Border_Left4 = (Border)myWindow.FindName("Border_Left4");
                    Border Border_Left5 = (Border)myWindow.FindName("Border_Left5");
                    Border Border_Right1 = (Border)myWindow.FindName("Border_Right1");
                    //Border Border_Right2 = (Border)myWindow.FindName("Border_Right2");
                    Line LineSplit = (Line)myWindow.FindName("LineSplit");
                    if (btn_Expand.Tag.ToString() == "Collapsed")
                    {
                        this.CardViewVisibility = Visibility.Visible;
                        Border_Left1.BorderThickness = new Thickness(2, 2, 0, 0);
                        Border_Left2.BorderThickness = new Thickness(2, 0, 0, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 0, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 0, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 0, 2);
                        Border_Right1.BorderThickness = new Thickness(0.5, 2, 2, 2);
                        //Border_Right2.BorderThickness = new Thickness(1, 0, 2, 2);
                        btn_Expand.Content = "<<";
                        myWindow.Width = 688;
                        btn_Expand.Tag = "Visible";
                        LineSplit.Stroke = (Brush)myWindow.FindResource("LostFocusBorderBrush");
                        LineSplit.Visibility = Visibility.Visible;
                        InitPriceView();
                    }
                    else
                    {
                        this.CardViewVisibility = Visibility.Collapsed;

                        Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        Border_Left2.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 2, 2);
                        Border_Right1.BorderThickness = new Thickness(2, 2, 2, 2);
                        //Border_Right2.BorderThickness = new Thickness(2, 0, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 395;
                        btn_Expand.Tag = "Collapsed";
                        PriceAndOrderView dgPrice = (PriceAndOrderView)myWindow.FindName("dgPrice");
                        LineSplit.Visibility = Visibility.Collapsed;
                        //dgPrice.SetBrokerSelect("ALL");
                    }

                    SetPriceAndOrderHeight();
                }
            }));
        }


        private void SetPriceAndOrderHeight()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Line LineSplit = myWindow.FindName("LineSplit") as Line;
                if (this.AllowanceChannelVisibility == Visibility.Visible)
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;
                    if ((FirmSelectIndex == 0) && (!WholeFlagChecked))
                    {
                        listBox.Height = 420 + 52 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                        LineSplit.Y2 = 418 + 52 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                    }
                    else
                    {
                        if (this.VenueVisible == Visibility.Visible)
                        {
                            listBox.Height = 356 + 52;
                            LineSplit.Y2 = 353 + 52;
                        }
                        else
                        {
                            listBox.Height = 420 + 52 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                            LineSplit.Y2 = 418 + 52 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                        }
                    }
                }
                else
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;
                    listBox.Height = 356 + 52;
                    LineSplit.Y2 = 353 + 52;
                }
            }));
        }


        private void SetPriceAndOrderHeighWholeFlag(bool isValue)
        {
            //if (isValue)
            //{
            //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //    {
            //        Border listBox = myWindow.FindName("Border_Right1") as Border;
            //        listBox.Height = 360;
            //    }));
            //}
            //else
            //{
            //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //    {
            //        Border listBox = myWindow.FindName("Border_Right1") as Border;
            //        //if ((FirmSelectIndex == 0) && (!WholeFlagChecked))
            //        //{
            //            listBox.Height = 421;
            //        //}
            //        //else
            //        //{
            //        //    listBox.Height = 360;
            //        //}
            //    }));
            //}
        }


        private void InitPriceView(string searchText = "")
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = string.Empty;
                if (string.IsNullOrEmpty(searchText))
                {
                    if (_priceMatchParam != null)
                    {
                        securityID = _priceMatchParam.Symbol;
                    }
                    else
                    {
                        securityID = SendPriceItem.Security;
                    }
                }
                else
                {
                    securityID = searchText;
                }

                PriceAndOrderView tradeBookView = (PriceAndOrderView)myWindow.FindName("dgPrice");
                PriceAndOrderViewModel Qm = new PriceAndOrderViewModel();

                if (securityID != null)
                {
                    Qm.SecurityID = securityID;
                }

                Qm.GetLastExecutionsItem(securityID);
                if (tradeBookView != null)
                {
                    tradeBookView.SetLastExecutionsSecurtityID(securityID);
                    tradeBookView.RemoveDbDoubleClickEvent();
                    tradeBookView.DataContext = Qm;
                }
            }));
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
                        if (Decimal.Parse(this.LastPrice) != Decimal.Parse(this.CustomPrice))
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
    }
}

