using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Position;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Card;
using STC.Common.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace STC.Common.ViewModel.Order
{
    /// <summary>
    /// 报价界面SendPointPriceView的数据内容
    /// </summary>
    public class SendPriceFutureViewModelLite : ViewModelBase
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

        private readonly string DEPTH_TYPE = ">> 阶梯下单板";
        private readonly string NORMAL_TYPE = ">> 普通下单板";
        private readonly string XBOND = "XBOND";

        private readonly string ADD_ONE_NOT_LIMIT = "2,3";

        //报价界面档位行情刷新
        private Thread RefreshDataThread = null;

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
        //界面是否阶梯下单板
        private bool _isDepth = false;

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

        public SendPriceFutureViewModelLite()
        {
            //开平仓，投机套保套利默认值
            OpenCloseAutoSelected = false;
            OpenSelected = true;
            CloseSelected = false;
            SpeculateSelected = true;
            HedgeSelected = false;
            ArbitrageSelected = false;

            if (RefreshDataThread == null)
            {
                RefreshDataThread = new Thread(RefreshDepth);
                RefreshDataThread.IsBackground = true;
                RefreshDataThread.Start();
            }
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

        ~SendPriceFutureViewModelLite()
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
            if (_isDepth)
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

                    if (!_isDepth)
                    {
                        CheckXBond = _isDepth ? true : false;
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
                        if (_sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex) || (!_isDepth && firmChecked))
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

            if (!_isDepth && firmChecked && DirectionCheck)
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

        /**
         * 普通下单板可见
         */
        private Visibility _commonVisibility = Visibility.Visible;
        public Visibility CommonVisibility
        {
            get
            {
                return _commonVisibility;
            }

            set
            {
                _commonVisibility = value;
                this.OnPropertyChanged(nameof(CommonVisibility));
            }
        }

        /**
         * 阶梯下单板可见
         */
        private Visibility _depthVisibility = Visibility.Collapsed;
        public Visibility DepthVisibility
        {
            get
            {
                return _depthVisibility;
            }

            set
            {
                _depthVisibility = value;

                this.OnPropertyChanged(nameof(DepthVisibility));
            }
        }

        private int _commonRowSpan = 5;
        public int CommonRowSpan
        {
            get
            {
                return _commonRowSpan;
            }

            set
            {
                _commonRowSpan = value;
                this.OnPropertyChanged(nameof(CommonRowSpan));
            }
        }

        private int _commonRowNumber = 6;
        public int CommonRowNumber
        {
            get
            {
                return _commonRowNumber;
            }

            set
            {
                _commonRowNumber = value;
                this.OnPropertyChanged(nameof(CommonRowNumber));
            }
        }

        private int _borderLeft4RowNumber = 4;
        public int BorderLeft4RowNumber
        {
            get
            {
                return _borderLeft4RowNumber;
            }

            set
            {
                _borderLeft4RowNumber = value;
                this.OnPropertyChanged(nameof(BorderLeft4RowNumber));
            }
        }

        private int _borderLeft6RowNumber = 5;
        public int BorderLeft6RowNumber
        {
            get
            {
                return _borderLeft6RowNumber;
            }

            set
            {
                _borderLeft6RowNumber = value;
                this.OnPropertyChanged(nameof(BorderLeft6RowNumber));
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
                    //国债期货报价界面切换合约编号
                    SecurityID = searchText;
                    if (isInit == false)
                    {
                        _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
                        UpdateBestPrice();
                        SendPriceItem.CustomPrice = _sendPriceAdapter.GetInitBestPrice3Decimal(isBid ? 1 : 4);
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
                if (!_isDepth)
                {
                    CheckXBond = _isDepth ? true : false;
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
                    CheckNotoutEnable = this._isDepth ? false : true;
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
        /// 选自动时，开平仓方向-根据持仓决定
        /// </summary>
        private string _offsetFlagAuto = "0";   //默认开仓
        public string OffsetFlagAuto
        {
            get
            {
                return _offsetFlagAuto;
            }
            set
            {
                _offsetFlagAuto = value;
                this.OnPropertyChanged(nameof(OffsetFlagAuto));
            }
        }

        /// <summary>
        /// 自动开平仓
        /// </summary>
        private bool _openCloseAutoSelected = false;
        public bool OpenCloseAutoSelected
        {
            get
            {
                return _openCloseAutoSelected;
            }
            set
            {
                _openCloseAutoSelected = value;
                this.OnPropertyChanged(nameof(OpenCloseAutoSelected));
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
        /// 平今
        /// </summary>
        private bool _closeTodaySelected = false;
        public bool CloseTodaySelected
        {
            get
            {
                return _closeTodaySelected;
            }
            set
            {
                _closeTodaySelected = value;
                this.OnPropertyChanged(nameof(CloseTodaySelected));
            }
        }

        /// <summary>
        /// 平昨
        /// </summary>
        private bool _closeYesterdaySelected = false;
        public bool CloseYesterdaySelected
        {
            get
            {
                return _closeYesterdaySelected;
            }
            set
            {
                _closeYesterdaySelected = value;
                this.OnPropertyChanged(nameof(CloseYesterdaySelected));
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
        /// 国债期货-是否有阶梯下单板权限
        /// </summary>
        private bool IsHaveDepthPermission = true;

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
        private object _depthLockObj = new object();
        private CalResult _lastCalResult = null;
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val)
        {
        }

        private bool _isFromXBond = false;
        /// <summary>
        /// 切换UI显示内容
        /// </summary>
        private void ChangeUI()
        {

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_isDepth)
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

                        _checkXBond = _isDepth ? true : false;
                        this.OnPropertyChanged("CheckXBond");
                    }

                    UpdateDirectionItem();
                }
                else
                {
                    bool isAllow = IsHaveAllPermission;
                    if (isAllow)
                    {
                        _checkAllVisibility = _isDepth ? Visibility.Collapsed : Visibility.Visible;
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
                            _checkXBond = _isDepth ? true : false;
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
                if ((SettleBoxIndex == 2 && !_isDepth && _sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || firmChecked)
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
                                    CheckXBond = _isDepth ? true : false;
                                }
                            }
                        }
                        else
                        {
                            isAllow = IsHaveInnerXbondPermission && !FirmChecked;

                            _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                            this.OnPropertyChanged("CheckXBondVisibility");

                            _checkXBond = _isDepth ? true : false;
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
            return !((SettleBoxIndex == 2 && !_isDepth && _sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || firmChecked);
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
            if (_isDepth)
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

                        if (_isDepth)
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
        public ICommand DepthCommand { get { return new ComRelayCommand(OnDepthClick); } }

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
                SendPriceItem.Amount = "0";
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

        //隐藏/展示 普通下单界面元素
        //根据是否阶梯下单板 决定界面布局
        private void ShowCommonElement(bool isDepth)
        {
            StackPanel stackPanelLastPrice1 = (StackPanel)myWindow.FindName("stackPanelLastPrice1");
            Border Border_Left31 = (Border)myWindow.FindName("Border_Left31");
            Border Border_Left4 = (Border)myWindow.FindName("Border_Left4");
            Border Border_Left6 = (Border)myWindow.FindName("Border_Left6");
            Border Border_Left5 = (Border)myWindow.FindName("Border_Left5");
            Grid MainGrid = (Grid)myWindow.FindName("MainGrid");
            if (MainGrid != null)
            {
                if (!isDepth)
                {
                    MainGrid.RowDefinitions[0].Height = new GridLength(24, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[1].Height = new GridLength(32, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[2].Height = new GridLength(122, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[3].Height = new GridLength(96, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[4].Height = new GridLength(120, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[5].Height = new GridLength(54, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Auto);
                }
                else
                {
                    MainGrid.RowDefinitions[0].Height = new GridLength(24, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[1].Height = new GridLength(32, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[2].Height = new GridLength(122, GridUnitType.Pixel);
                    //MainGrid.RowDefinitions[3].Height = new GridLength(96, GridUnitType.Pixel);
                    //MainGrid.RowDefinitions[4].Height = new GridLength(120, GridUnitType.Pixel);
                    //MainGrid.RowDefinitions[5].Height = new GridLength(54, GridUnitType.Pixel);
                    //MainGrid.RowDefinitions[6].Height = new GridLength(0, GridUnitType.Auto);
                    MainGrid.RowDefinitions[3].Height = new GridLength(120, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[4].Height = new GridLength(54, GridUnitType.Pixel);
                    MainGrid.RowDefinitions[5].Height = new GridLength(0, GridUnitType.Auto);
                }
            }

            //展开收起按钮只在阶梯下单板显示
            Button btn_Expand = (Button)myWindow.FindName("btn_Expand");
            if (btn_Expand != null)
            {
                btn_Expand.Visibility = isDepth ? Visibility.Visible : Visibility.Collapsed;
                if (!isDepth)
                {
                    btn_Expand.Tag = "Visible";
                    btn_Expand.Content = ">>";
                }
            }

            if (isDepth)    //阶梯下单板
            {
                //最新价/涨跌显示框 - 普通下单界面
                if (stackPanelLastPrice1 != null)
                {
                    stackPanelLastPrice1.Visibility = Visibility.Collapsed;
                }
                //价格，跟盘价，涨跌停价显示框 - 普通下单界面
                if (Border_Left31 != null)
                {
                    Border_Left31.Visibility = Visibility.Collapsed;
                }
                if (DisplayTenTwenty == 20)
                {
                    Border_Left31.Visibility = Visibility.Collapsed;
                    Border_Left4.Visibility = Visibility.Collapsed;
                    Border_Left6.Visibility = Visibility.Collapsed;
                    CommonRowNumber = 3;
                    CommonRowSpan = 4;
                }
                else
                {
                    Border_Left31.Visibility = Visibility.Visible;
                    Border_Left4.Visibility = Visibility.Visible;
                    Border_Left6.Visibility = Visibility.Visible;
                    CommonRowNumber = 6;
                    CommonRowSpan = 1;
                }
                BorderLeft4RowNumber = 3;
                BorderLeft6RowNumber = 4;

            }
            else
            {
                //最新价/涨跌显示框 - 普通下单界面
                if (stackPanelLastPrice1 != null)
                {
                    stackPanelLastPrice1.Visibility = Visibility.Visible;
                }
                //价格，跟盘价，涨跌停价显示框 - 普通下单界面
                if (Border_Left31 != null)
                {
                    Border_Left31.Visibility = Visibility.Visible;
                }
                if (Border_Left4 != null)
                {
                    Border_Left4.Visibility = Visibility.Visible;
                }
                if (Border_Left6 != null)
                {
                    Border_Left6.Visibility = Visibility.Visible;
                }
                BorderLeft4RowNumber = 4;
                BorderLeft6RowNumber = 5;
                CommonRowNumber = 6;
            }
        }

        //阶梯下单板取消买卖颜色显示
        private void DisableBuySellBackgroundColor()
        {
            Border Border_Left2 = (Border)myWindow.FindName("Border_Left2");
            Button BidBtn = (Button)myWindow.FindName("BidBtn");
            Button OfrBtn = (Button)myWindow.FindName("OfrBtn");
            //if (Border_Left2 != null)
            //{
            //    Border_Left2.IsEnabled = false;
            //}
            if (BidBtn != null)
            {
                BidBtn.Visibility = Visibility.Collapsed;
            }
            if (BidBtn != null)
            {
                OfrBtn.Visibility = Visibility.Collapsed;
            }
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("DepthLinearGradientBrush");
        }

        //阶梯下单板恢复买卖颜色显示
        private void EnableBuySellBackgroundColor()
        {
            Border Border_Left2 = (Border)myWindow.FindName("Border_Left2");
            Button BidBtn = (Button)myWindow.FindName("BidBtn");
            Button OfrBtn = (Button)myWindow.FindName("OfrBtn");

            //if (Border_Left2 != null)
            //{
            //    Border_Left2.IsEnabled = true;
            //}
            if (BidBtn != null)
            {
                BidBtn.Visibility = Visibility.Visible;
            }
            if (BidBtn != null)
            {
                OfrBtn.Visibility = Visibility.Visible;
            }

            if (isBid)
            {
                sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
            }
            else
            {
                sendPriceItem.BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
            }
        }

        /// <summary>
        /// 切换至深度界面报价
        /// </summary>
        private void OnDepthClick()
        {
            DefaultCheck = false;
            DirectionCheck = true;
            _isDepth = !_isDepth;

            // 标题按钮改为显示 ">> 普通报价"
            SendPriceType = _isDepth ? NORMAL_TYPE : DEPTH_TYPE;

            sendPriceItem.NetPriceVisible = _isDepth ? Visibility.Visible : Visibility.Collapsed;
            _isFromXBond = true;



            if (!_isDepth)
            {
                //普通下单板
                CommonVisibility = Visibility.Visible;
                DepthVisibility = Visibility.Collapsed;
                ShowCommonElement(false);
                EnableBuySellBackgroundColor();
            }
            else
            {
                myWindow.Height = 940;
                CommonVisibility = Visibility.Collapsed;
                DepthVisibility = Visibility.Visible;
                ShowCommonElement(true);
                DisableBuySellBackgroundColor();
            }

            ////滚动到买卖档分隔处
            //DepthEntry de = IsFifthDepth ? FullDepthEntries.ToList().Find(entry => SendPriceItem.BestBid.Value.CompareTo(entry.Price.ConvertToDecimal()) == 0)
            //                            : FullTickEntries.ToList().Find(entry => SendPriceItem.BestBid.Value.CompareTo(entry.Price.ConvertToDecimal()) == 0);
            //if (de != null)
            //{
            //    ExDataGrid exDataGrid = (ExDataGrid)myWindow.FindName("DepthDataGrid");
            //    Button btn_Expand = (Button)myWindow.FindName("btn_Expand");
            //    if (exDataGrid != null && btn_Expand != null)
            //    {
            //        int index = de.Index + +("Visible".Equals(btn_Expand.Tag) ? 3 : 9);
            //        exDataGrid.ScrollIntoView(exDataGrid.Items[0]);
            //        exDataGrid.ScrollIntoView(exDataGrid.Items[index]);
            //    }
            //}
            MakeCenter();
            //汇总
            UpdateQtySum();
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
                SendPriceItem.CustomPrice = d.ToString("F3").Replace(",", "");
            }
            else
            {
                SendPriceItem.CustomPrice = priceMatchParam.Price;
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
                this.OnDepthClick();

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
                if (!_isDepth)
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
            this.IsBidSelect = isBid ? "1" : "0";
            _sendPriceAdapter = new SendPriceModelAdapter();

            // 默认标题按钮显示 ">> X-bond报价"
            SendPriceType = DEPTH_TYPE;
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

            //国债期货忽略缺省报量
            //// 缺省报量
            //if (SystemParamHandler.Instance.GetUserSetting() != null)
            //{
            //    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
            //    if (!string.IsNullOrEmpty(Us.userQty))
            //    {
            //        SendPriceItem.Amount = (Decimal.Parse(Us.userQty) / 1000).ToString();
            //    }
            //    if (!string.IsNullOrEmpty(Us.inf2))
            //    {
            //        if (JsonSplit.IsJson(Us.inf2))
            //        {
            //            ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
            //            SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
            //            PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;
            //        }
            //    }
            //}

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


            //初始化阶梯下单板数据源
            this.DepthMv.Source = FullDepthEntries;
            this.DepthMv.IsLiveFilteringRequested = true;
            this.DepthMv.IsLiveSortingRequested = true;
            this.DepthMv.Filter += DepthMv_Filter;
            this.DepthMv.SortDescriptions.Add(new SortDescription("PriceDecimal", ListSortDirection.Descending));
            this.DepthMv.View.Refresh();
            // 订阅我的订单
            MyOrderProcesser.Processer.OnOwnOrdersChange += Instance_OnOrder;
            ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;
        }

        private void DepthMv_Filter(object sender, FilterEventArgs e)
        {
            DepthEntry de = e.Item as DepthEntry;
            if (de != null)
            {
                e.Accepted = de.Show;
            }
            else
            {
                e.Accepted = false;
            }
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
            SecurityID = security;
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
                    _checkAllVisibility = _isDepth ? Visibility.Collapsed : Visibility.Visible;
                    this.OnPropertyChanged("CheckAllVisibility");

                    _checkXBondVisibility = IsHaveVenueXbondPermission ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    ////myWindow.Height = _isXBond ? 488 : 488;// 548;
                }

                if (_isDepth)
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
                        DepthVisibility = Visibility.Collapsed;
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

                    DepthVisibility = Visibility.Collapsed;
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
                            DepthVisibility = Visibility.Collapsed;
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

            StopRefreshDepth = true;
            PriceHandler.Instance.UnregisterCTPrice(SecurityID, Instance_OnCTPrice);
            MyOrderProcesser.Processer.OnOwnOrdersChange -= Instance_OnOrder;
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
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

            if (!_isDepth)
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
            sendPricePo.OffsetFlag = OpenCloseAutoSelected ? OffsetFlagAuto : OpenSelected ? "0" : "1";
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

            sendPricePo.Terminal = string.Format(_isDepth ? "STClient_v{0}_xbond" : "STClient_v{0}", LocalEnv.ServerInfo.Version);
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
        /// 双击价格下单 - 阶梯下单板
        /// </summary>
        private void PlaceOrder(string price, string side)
        {
            NewOrderSingle sendPricePo = GetNewOrderSingle();
            sendPricePo.Price = price;
            sendPricePo.Side = side;
            //SendPriceEnable = false;
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
                                    //warnView.OnAction += WarnView_OnAction;
                                    if (!result.IsSuccess())
                                    {
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
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
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
        /// 双击价格下单
        /// </summary>
        public void DoubleClickOrder(string price, string side)
        {
            this.myWindow.Topmost = false;
            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("双击价格报价");
            }
            //SendPriceEnable = false;
            PlaceOrder(price, side);
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

            if (!OpenSelected && !CloseSelected && !OpenCloseAutoSelected)  //开平仓未选择
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
        public bool CheckParam()
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
            if (!OpenSelected && !CloseSelected && !OpenCloseAutoSelected)  //开平仓未选择
            {
                result = false;
            }
            if (!SpeculateSelected && !HedgeSelected && !ArbitrageSelected)  //投机/套保/套利未选择
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseClick()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();

            StopRefreshDepth = true;
            PriceHandler.Instance.UnregisterCTPrice(SecurityID, Instance_OnCTPrice);
            MyOrderProcesser.Processer.OnOwnOrdersChange -= Instance_OnOrder;
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
            if (myWindow != null)
            {
                myWindow.Close();
            }
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

            if (_isDepth)
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

            if (_isDepth)
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
                    Border Border_Left6 = (Border)myWindow.FindName("Border_Left6");
                    Border Border_Left31 = (Border)myWindow.FindName("Border_Left31");
                    if (btn_Expand.Tag.ToString() == "Visible") //增加下单板高度，收起开平仓等界面元素
                    {
                        this.CardViewVisibility = Visibility.Visible;
                        Border_Left31.Visibility = Visibility.Collapsed;
                        Border_Left4.Visibility = Visibility.Collapsed;
                        Border_Left6.Visibility = Visibility.Collapsed;
                        btn_Expand.Content = "<<";
                        //myWindow.Width = 688;
                        Grid depthGrid = myWindow.FindName("gridDepth") as Grid;
                        if (depthGrid != null)
                        {
                            depthGrid.Height += 166 + 150;
                        }
                        btn_Expand.Tag = "Collapsed";
                        InitPriceView();
                        CommonRowNumber = 3;
                        CommonRowSpan = 4;
                        MakeCenter();
                    }
                    else
                    {
                        this.CardViewVisibility = Visibility.Collapsed;
                        Border_Left31.Visibility = Visibility.Visible;
                        Border_Left4.Visibility = Visibility.Visible;
                        Border_Left6.Visibility = Visibility.Visible;
                        btn_Expand.Content = ">>";
                        //myWindow.Width = 395;
                        Grid depthGrid = myWindow.FindName("gridDepth") as Grid;
                        if (depthGrid != null)
                        {
                            depthGrid.Height -= 166 + 150;
                        }
                        btn_Expand.Tag = "Visible";
                        CommonRowNumber = 6;
                        CommonRowSpan = 1;
                    }
                    MakeCenter();
                }
            }));
        }

        private void MakeCenter()
        {
            if (null == SendPriceItem.BestBid5)
            {
                return;
            }
            //Button btn_Expand = (Button)myWindow.FindName("btn_Expand");
            DataGrid DepthDataGrid = (DataGrid)myWindow.FindName("DepthDataGrid");
            if (IsFifthDepth)
            {
                //买一价
                decimal priceBottom = DisplayTenTwenty == 10 ? SendPriceItem.BestBid5.Value - 0.005m * 4 : SendPriceItem.BestBid5.Value - 0.005m * 9;
                DepthEntry de5 = FullDepthEntries.ToList().Find(entry => priceBottom.CompareTo(entry.PriceDecimal) == 0);
                if (de5 != null)
                {
                    //滚动到买卖档分隔处
                    DepthDataGrid.ScrollIntoView(DepthDataGrid.Items[0]);
                    DepthDataGrid.ScrollIntoView(de5);
                    //LogHandler.Info("Fifth模式-滚动到{0}", de5.Price);
                }
            }
            else
            {
                decimal priceBottom = DisplayTenTwenty == 10 ? SendPriceItem.BestBid5.Value - 0.005m * 4 : SendPriceItem.BestBid5.Value - 0.005m * 9;
                DepthEntry de5 = FullTickEntries.ToList().Find(entry => priceBottom.CompareTo(entry.PriceDecimal) == 0);
                if (de5 != null)
                {
                    //滚动到买卖档分隔处
                    DepthDataGrid.ScrollIntoView(DepthDataGrid.Items[0]);
                    DepthDataGrid.ScrollIntoView(de5);
                    //LogHandler.Info("Tick模式-滚动到{0}", de5.Price);
                }
            }
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


        public void InitPriceView(string searchText = "")
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

        private object baseLockObj = new object();
        private DataView _BaseDV;
        private DataView BaseDV
        {
            get
            {
                lock (baseLockObj)
                {
                    if (_BaseDV == null)
                    {
                        _BaseDV = new DataTable().DefaultView;
                        _BaseDV.Table.Columns.Add(CTPriceControler.KEY_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SECURITY_ID_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TOTAL_QTY_FIELD, typeof(decimal));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TIME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SOURCE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QTY_FIELD, typeof(long));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ENTRY_PX_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.NET_PRICE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SETTL_TYPE_YUAN_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SIDE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_BEST_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.FLAG_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.DELETE_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.FIRM_TYPE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.VENUE_TYPE_GW_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TEXT_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.MD_PRICE_LEVEL_FIELD, typeof(int));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.BARGAIN_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.WHOLE_FLAG_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.INSTITUTION_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.SHOWTRADER_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISME_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISINCOMPLETE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TRADERNAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.TRANSACT_TIME_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.DELIVERY_TYPE_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CLEARING_METHOD_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QUOTE_TYPE_FILED, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CUSTOMER_SOURCE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_NAME_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PARTY_ID_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_PERMISSION_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IS_MORE_FIELD, typeof(bool));
                        _BaseDV.Table.Columns.Add(CTPriceControler.QUOTE_STATUS_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.VENUE_SORT_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.ISINNER_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.MARKETINDICATOR_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.Yield_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.YieldNum_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlag_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.PriceType_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlagFieldTip_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.CalFlagNetPriceTip_Field, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.MIN_CHANGE_FIELD, typeof(string));
                        _BaseDV.Table.Columns.Add(CTPriceControler.IsInnerInComplete_Field, typeof(string));
                        _BaseDV.Sort = $"{CTPriceControler.SIDE_FIELD},{CTPriceControler.MD_PRICE_LEVEL_FIELD}";
                        _BaseDV.Table.PrimaryKey = new DataColumn[] { _BaseDV.Table.Columns[CTPriceControler.KEY_FIELD] };
                        this.OnPropertyChanged();
                    }
                    return this._BaseDV;
                }
            }
        }

        /// <summary>
        /// 我的成交数据回调方法 - 用于更新持仓均价
        /// </summary>
        private void Instance_OnOwnExecutions(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ComboBox combo = myWindow.FindName("cbBookId") as ComboBox;
                if (combo != null)
                {
                    //如果界面已选择组合，获取持仓均价
                    string bookid = combo.SelectedValue == null ? string.Empty : combo.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(bookid))
                    {
                        QueryPositionReq req = new QueryPositionReq();
                        req.bookid = bookid;
                        req.securityid = SendPriceItem.Security;
                        CommonUtils.STOPWATCH.Start();
                        QueryPositionResult reply = GetPositionDetailData(req);
                        CommonUtils.STOPWATCH.Stop();
                        LogHandler.Debug("合约{0},组合{1} 获取持仓均价耗时{2}ms", SendPriceItem.Security, bookid, CommonUtils.STOPWATCH.ElapsedMilliseconds);
                        if (reply != null)
                        {
                            if (reply.Datas != null && reply.Datas.Count > 0)
                            {
                                foreach (PositionDetailModel pdm in reply.Datas)
                                {
                                    decimal longavgprice = pdm.longavgprice;
                                    decimal shortavgprice = pdm.shortavgprice;
                                    SendPriceItem.LongAvgPrice = longavgprice;
                                    SendPriceItem.ShortAvgPrice = shortavgprice;
                                    SendPriceItem.LongAvgPriceStr = longavgprice.ToString("N4");
                                    SendPriceItem.ShortAvgPriceStr = shortavgprice.ToString("N4");
                                }
                            }
                            else
                            {
                                SendPriceItem.LongAvgPrice = 0m;
                                SendPriceItem.ShortAvgPrice = 0m;
                                SendPriceItem.LongAvgPriceStr = "--";
                                SendPriceItem.ShortAvgPriceStr = "--";
                            }
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 我的订单更新回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(string securityid, string side)
        {
            UpdateMyOrders();
            UpdateQtySum();
            RefreshDepthTickGrid();
        }

        private DataView _MyOrdersDV;
        /// <summary>
        /// 我的订单数据源
        /// </summary>
        public DataView MyOrdersDV
        {
            get
            {
                lock (MyOrderProcesser.Processer.LockObj_MyOrders)
                {
                    //if (_MyOrdersDV == null)
                    //{
                    DataTable tempTable = SortDataTable(MyOrderProcesser.Processer.MyOrdersDT,
                                        $"SecurityID='{SecurityID}'",
                                        $"{MyOrderProcesser.UPDATETIME_ORDER_FIELD} desc", 2);
                    _MyOrdersDV = new DataView(tempTable);
                    _MyOrdersDV.Table.Columns.Add("QtyNum", typeof(int), "Convert(Qty, 'System.Int32')");
                    //_MyOrdersDV.RowFilter = $"({MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'N' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '0' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '1' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = 'S' or {MyOrderProcesser.ORDERSTATUS_ORDER_FIELD} = '6')";
                    //_MyOrdersDV.Sort = $"{MyOrderProcesser.UPDATETIME_ORDER_FIELD} desc";
                    this.OnPropertyChanged();
                    //}
                    return this._MyOrdersDV;
                }
            }
        }

        /// DataTable筛选，排序返回符合条件行组成的新DataTable
        ///或直接用DefaultView按条件返回
        /// eg:SortExprDataTable(dt,"Sex='男'","Time Desc",1)
        /// 传入的DataTable
        /// 筛选条件
        /// 排序条件
        /// 1,直接用DefaultView按条件返回,效率较高;
        /// 2,DataTable筛选，排序返回符合条件行组成的新DataTable
        public static DataTable SortDataTable(DataTable dt, string strExpr, string strSort, int mode)
        {
            switch (mode)
            {
                case 1:
                    //方法一　直接用DefaultView按条件返回
                    dt.DefaultView.RowFilter = strExpr;
                    dt.DefaultView.Sort = strSort;
                    return dt;
                case 2:
                    //方法二　DataTable筛选，
                    //排序返回符合条件行组成的新DataTable
                    DataTable dt1 = new DataTable();
                    DataRow[] GetRows = dt.Select(strExpr, strSort);
                    //复制DataTable dt结构不包含数据
                    dt1 = dt.Clone();
                    foreach (DataRow row in GetRows)
                    {
                        dt1.Rows.Add(row.ItemArray);
                    }
                    return dt1;
                default:
                    return dt;
            }
        }

        //Tick行情
        private List<DepthEntry> listTickEntries = new List<DepthEntry>();
        public List<DepthEntry> ListTickEntries
        {
            get
            {
                return listTickEntries;
            }
            set
            {
                listTickEntries = value;
                this.OnPropertyChanged(nameof(ListTickEntries));
            }
        }

        private List<DepthEntry> listDepthEntries = new List<DepthEntry>();
        public List<DepthEntry> ListDepthEntries
        {
            get
            {
                return listDepthEntries;
            }
            set
            {
                listDepthEntries = value;
                this.OnPropertyChanged(nameof(ListDepthEntries));
            }
        }

        //Tick行情
        private ObservableCollection<DepthEntry> fullTickEntries = new ObservableCollection<DepthEntry>();
        public ObservableCollection<DepthEntry> FullTickEntries
        {
            get
            {
                return fullTickEntries;
            }
            set
            {
                fullTickEntries = value;
                this.OnPropertyChanged(nameof(FullTickEntries));
            }
        }

        private ObservableCollection<DepthEntry> fullDepthEntries = new ObservableCollection<DepthEntry>();
        public ObservableCollection<DepthEntry> FullDepthEntries
        {
            get
            {
                return fullDepthEntries;
            }
            set
            {
                fullDepthEntries = value;
                this.OnPropertyChanged(nameof(FullDepthEntries));
            }
        }

        private CollectionViewSource depthMv = new CollectionViewSource();
        public CollectionViewSource DepthMv
        {
            get { return depthMv; }
            set
            {
                depthMv = value;
                this.OnPropertyChanged(nameof(DepthMv));
            }
        }

        /// <summary>
        /// 债券代码
        /// </summary>
        private string _SecurityID = string.Empty;
        public string SecurityID
        {
            get
            {
                return this._SecurityID;
            }

            set
            {
                if (this._SecurityID != value)
                {
                    PriceHandler.Instance.UnregisterCTPrice(_SecurityID, Instance_OnCTPrice);
                    // 切换债券
                    this._SecurityID = value;
                    this.OnPropertyChanged();

                    if (!string.IsNullOrEmpty(_SecurityID))
                    {
                        PriceHandler.Instance.RegisterCTPrice(value, Instance_OnCTPrice);
                    }
                }
            }
        }

        private int displayTenTwenty = 10;
        /// <summary>
        /// 展示10行/20行 
        /// </summary>
        public int DisplayTenTwenty
        {
            get
            {
                return displayTenTwenty;
            }

            set
            {
                if (this.displayTenTwenty != value)
                {
                    this.displayTenTwenty = value;
                    this.OnPropertyChanged(nameof(DisplayTenTwenty));
                }
            }
        }

        /// <summary>
        /// 深度行情模式 10档/Tick
        /// </summary>
        private bool _isFifthDepth = true;
        public bool IsFifthDepth
        {
            get
            {
                return this._isFifthDepth;
            }

            set
            {
                if (this._isFifthDepth != value)
                {
                    this._isFifthDepth = value;
                    this.OnPropertyChanged(nameof(IsFifthDepth));
                }
            }
        }

        private int strategyBuyQtySum = 0;
        /// <summary>
        /// 策略买合计值 
        /// </summary>
        public int StrategyBuyQtySum
        {
            get
            {
                return strategyBuyQtySum;
            }

            set
            {
                if (this.strategyBuyQtySum != value)
                {
                    this.strategyBuyQtySum = value;
                    this.OnPropertyChanged(nameof(StrategyBuyQtySum));
                }
            }
        }

        private int strategySellQtySum = 0;
        /// <summary>
        /// 策略卖合计值 
        /// </summary>
        public int StrategySellQtySum
        {
            get
            {
                return strategySellQtySum;
            }

            set
            {
                if (this.strategySellQtySum != value)
                {
                    this.strategySellQtySum = value;
                    this.OnPropertyChanged(nameof(StrategySellQtySum));
                }
            }
        }

        private int manualBuyQtySum = 0;
        /// <summary>
        /// 手工买合计值 
        /// </summary>
        public int ManualBuyQtySum
        {
            get
            {
                return manualBuyQtySum;
            }

            set
            {
                if (this.manualBuyQtySum != value)
                {
                    this.manualBuyQtySum = value;
                    this.OnPropertyChanged(nameof(ManualBuyQtySum));
                }
            }
        }

        private int manualSellQtySum = 0;
        /// <summary>
        /// 手工卖合计值 
        /// </summary>
        public int ManualSellQtySum
        {
            get
            {
                return manualSellQtySum;
            }

            set
            {
                if (this.manualSellQtySum != value)
                {
                    this.manualSellQtySum = value;
                    this.OnPropertyChanged(nameof(ManualSellQtySum));
                }
            }
        }

        private int buyQtySum = 0;
        /// <summary>
        /// 买量合计值 
        /// </summary>
        public int BuyQtySum
        {
            get
            {
                return buyQtySum;
            }

            set
            {
                if (this.buyQtySum != value)
                {
                    this.buyQtySum = value;
                    this.OnPropertyChanged(nameof(BuyQtySum));
                }
            }
        }

        private int sellQtySum = 0;
        /// <summary>
        /// 卖量合计值 
        /// </summary>
        public int SellQtySum
        {
            get
            {
                return sellQtySum;
            }

            set
            {
                if (this.sellQtySum != value)
                {
                    this.sellQtySum = value;
                    this.OnPropertyChanged(nameof(SellQtySum));
                }
            }
        }

        private ConcurrentQueue<MarketDataSnapshotFullRefresh> _ctPriceQueue = new ConcurrentQueue<MarketDataSnapshotFullRefresh>();
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private object _queueLock = new object();
        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (obj != null && obj.SecurityID == this.SecurityID)
                {
                    if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                    {
                        LogHelper.Debug($"QuotationListViewModel onCTPrice:{obj.SecurityID}");
                    }

                    lock (_queueLock)
                    {
                        _ctPriceQueue.Enqueue(obj);
                        _autoResetEvent.Set();
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        public bool StopRefreshDepth = false;
        private void RefreshDepth()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshDepth)
                    {
                        break;
                    }

                    MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh = null;

                    if (_ctPriceQueue.TryDequeue(out marketDataSnapshotFullRefresh))
                    {
                        if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
                        {
                            LogHelper.Info("Into RefreshDepth, security:" + marketDataSnapshotFullRefresh.SecurityID);
                        }
                        UpdateData(marketDataSnapshotFullRefresh);
                        while (!_ctPriceQueue.IsEmpty)
                        {
                            LogHelper.Info("Into RefreshDepth clear _ctPriceQueue, security:" + marketDataSnapshotFullRefresh.SecurityID);
                            _ctPriceQueue.TryDequeue(out marketDataSnapshotFullRefresh);
                        }
                    }
                    else
                        _autoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshDepth Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshDepth exception:", ex);
                }
            }
        }

        private void UpdateData(MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {
            //LogHandler.Info("收到MarketDataSnapshotFullRefresh，开始UpdateData");
            //涨跌停价格
            decimal? decUpperLimitPx, decLowerLimitPx, lastPrice;
            if (marketDataSnapshotFullRefresh != null)
            {
                lock (CTPriceControler.MDLock)
                {
                    CTPriceControler.UpdateDataToBaseDV(this.BaseDV, marketDataSnapshotFullRefresh);
                }
                //涨停价
                decUpperLimitPx = CommonUtils.ConvertToDecimalOrNull(marketDataSnapshotFullRefresh.UpperLimitPrice);
                //跌停价
                decLowerLimitPx = CommonUtils.ConvertToDecimalOrNull(marketDataSnapshotFullRefresh.LowerLimitPrice);
                //最新成交价
                lastPrice = CommonUtils.ConvertToDecimalOrNull(marketDataSnapshotFullRefresh.LastPrice);
            }
            else
            {
                return;
            }
            //非阶梯下单板界面不更新阶梯下单板,只初始化
            if (_isDepth||!DepthInit)
            {
                DataTable tempBaseDt = null;
                lock (this.baseLockObj)
                {
                    try
                    {
                        DataTable tempdt = this.BaseDV.ToTable();
                        tempBaseDt = tempdt.Copy();
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }
                DataTable depthDt = null;
                #region DataTable复制
                string filter = $"{CTPriceControler.VENUE_TYPE_GW_FIELD} not in ('{CTPriceControler.CDH}','{CTPriceControler.SOR}') and {CTPriceControler.ISINCOMPLETE_FIELD} = '0'";
                filter = $"{filter}";
                depthDt = CTPriceControler.getDataTable(tempBaseDt, filter);
                #endregion

                //在RefreshDepth子线程中完成计算
                Stopwatch sw = new Stopwatch();
                sw.Start();
                lock (lock_ListEntries)
                {
                    UpdateDepthEntries(depthDt, lastPrice, decUpperLimitPx, decLowerLimitPx);
                    UpdateTickEntries(depthDt, lastPrice, decUpperLimitPx, decLowerLimitPx);
                    UpdateMyOrders();
                }
                UpdateQtySum();
                sw.Stop();
                LogHandler.Debug("UpdateData更新下单板耗时{0}ms", sw.ElapsedMilliseconds);

                depthDt.Clear();
                depthDt.Dispose();
                depthDt = null;

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    RefreshDepthTickGrid();
                }));
                DepthInit = true;
            }
        }

        private void RefreshDepthTickGrid()
        {
            //UI主线程复制子线程更新后的数据。
            try
            {
                FullDepthEntries.Clear();
                FullTickEntries.Clear();
                //更新DataGrid Source的时候防止子线程更新源数据
                lock (lock_ListEntries)
                {
                    foreach (DepthEntry e in ListDepthEntries)
                    {
                        this.FullDepthEntries.Add(e);
                    }
                    foreach (DepthEntry e in ListTickEntries)
                    {
                        this.FullTickEntries.Add(e);
                    }
                }
                this.DepthMv.View.Refresh();
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            finally
            {
            }
        }

        public QueryPositionResult GetPositionDetailData(QueryPositionReq req)
        {
            QueryPositionResult reply = null;
            reply = PositionHandler.Instance.QueryPositionDetail(req);
            return reply;
        }

        //Fifth/Tick阶梯报价数据更新锁
        private object lock_ListEntries = new object();

        private bool IsPriceBetweenDepth(decimal price, decimal buyLevel, decimal sellLevel)
        {
            return price.CompareTo(buyLevel) >= 0 && price.CompareTo(sellLevel) <= 0;
        }

        //Fifth模式下初始化全档位
        private void initListDepthEntries(DataTable tempDt, List<DepthEntry> depthEntries, decimal? lastPrice,
            decimal? buyLow, decimal? sellHigh,
            decimal? buy1, decimal? sell1,
            decimal? upperLimit, decimal? lowerLimit)
        {
            decimal temp = upperLimit.Value;
            int index = 1;
            //已经初始化， 更新行情变动即可
            if (depthEntries.Count > 0)
            {
                List<DepthEntry> all = depthEntries;
                foreach (DepthEntry depthEntry in all)
                {
                    //有行情买卖量先清空
                    depthEntry.SellQty = 0;
                    depthEntry.BuyQty = 0;
                }
                List<DepthEntry> list = depthEntries.FindAll(e => IsPriceBetweenDepth(e.PriceDecimal, buyLow.Value, sellHigh.Value));
                //先隐藏行情上下范围内的价格
                foreach (DepthEntry en in list)
                {
                    en.Show = false;
                }
                //将行情最上一档上面的价位都置成SELL
                DepthEntry deAboveSell = depthEntries.Find(e => e.PriceDecimal.CompareTo(sellHigh + 0.005m) == 0);
                if (deAboveSell != null)
                {
                    while (deAboveSell != null && deAboveSell.BuySell.Equals("BUY"))
                    {
                        decimal tempPrice = deAboveSell.PriceDecimal;
                        deAboveSell.BuySell = "SELL";
                        deAboveSell = depthEntries.Find(e => e.PriceDecimal.CompareTo(tempPrice + 0.005m) == 0);
                    }
                }
                //将行情最下一档下面的价位都置成BUY
                DepthEntry deBelowBuy = depthEntries.Find(e => e.PriceDecimal.CompareTo(buyLow - 0.005m) == 0);
                if (deBelowBuy != null)
                {
                    while (deBelowBuy != null && deBelowBuy.BuySell.Equals("SELL"))
                    {
                        decimal tempPrice = deBelowBuy.PriceDecimal;
                        deBelowBuy.BuySell = "BUY";
                        deBelowBuy = depthEntries.Find(e => e.PriceDecimal.CompareTo(tempPrice - 0.005m) == 0);
                    }
                }
                //Fifth模式中间显示行情，上下各显示到涨停/跌停价
                foreach (DataRow dr in tempDt.Rows)
                {
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                    long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settlType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 询问
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();
                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();
                    string traderName = dr[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : dr[CTPriceControler.TRADERNAME_FIELD].ToString();
                    string transactTime = dr[CTPriceControler.TRANSACT_TIME_FILED] == null ? string.Empty : dr[CTPriceControler.TRANSACT_TIME_FILED].ToString();
                    string deliveryType = dr[CTPriceControler.DELIVERY_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
                    string clearingMethod = dr[CTPriceControler.CLEARING_METHOD_FILED] == null ? string.Empty : dr[CTPriceControler.CLEARING_METHOD_FILED].ToString();
                    string quoteType = dr[CTPriceControler.QUOTE_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.QUOTE_TYPE_FILED].ToString();
                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();
                    string quoteStatus = dr[CTPriceControler.QUOTE_STATUS_FIELD] == null ? string.Empty : dr[CTPriceControler.QUOTE_STATUS_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];
                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();
                    //2-收益率反算净价  4-净价反算收益率  0-原始值
                    if (string.IsNullOrEmpty(calFlag))
                    {
                        calFlag = "0";
                    }
                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);
                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    // priceType 1 - 净价；2 - 全价；3 - 收益率  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    if (calFlag == "2")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    if (calFlag == "4")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                    }
                    if (priceType == "1")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                        calFlag = "4"; //第三方计算的收益率
                    }
                    if (priceType == "3")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    string securityId = dr[CTPriceControler.SECURITY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.SECURITY_ID_FIELD].ToString();
                    string side = dr[CTPriceControler.SIDE_FIELD] == null ? string.Empty : dr[CTPriceControler.SIDE_FIELD].ToString();

                    DepthEntry depthEntry = depthEntries.Find(e => e.PriceDecimal.CompareTo(CommonUtils.ConvertToDecimal(price)) == 0);
                    depthEntry.Show = true;
                    //方向买/卖
                    depthEntry.BuySell = "0".Equals(side) ? "BUY" : "SELL";
                    if ("0".Equals(side))
                    {
                        depthEntry.BuyQty = (int)qty;
                    }
                    else
                    {
                        depthEntry.SellQty = (int)qty;
                    }
                }
            }
            else if (depthEntries.Count == 0)
            {
                while (temp.CompareTo(lowerLimit.Value) >= 0)
                {
                    DepthEntry depthEntry = new DepthEntry();
                    depthEntry.Price = temp.ToString("F3");
                    depthEntry.PriceDecimal = temp;
                    depthEntry.SecurityId = SecurityID;
                    depthEntry.Index = index;
                    depthEntry.Show = true;
                    if (temp.CompareTo(buy1) > 0)
                    {
                        depthEntry.BuySell = "SELL";
                    }
                    else
                    {
                        depthEntry.BuySell = "BUY";
                    }
                    //去掉行情上下价格之间的价位
                    if (temp.CompareTo(sellHigh) <= 0 && temp.CompareTo(buyLow) >= 0)
                    {
                        depthEntry.BuySell = "NONE";
                        depthEntry.Show = false;
                    }
                    depthEntries.Add(depthEntry);
                    temp -= 0.005m;
                    index++;
                }

                //Fifth模式中间显示行情，上下各显示到涨停/跌停价
                foreach (DataRow dr in tempDt.Rows)
                {
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                    long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settlType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 询问
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();
                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();
                    string traderName = dr[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : dr[CTPriceControler.TRADERNAME_FIELD].ToString();
                    string transactTime = dr[CTPriceControler.TRANSACT_TIME_FILED] == null ? string.Empty : dr[CTPriceControler.TRANSACT_TIME_FILED].ToString();
                    string deliveryType = dr[CTPriceControler.DELIVERY_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
                    string clearingMethod = dr[CTPriceControler.CLEARING_METHOD_FILED] == null ? string.Empty : dr[CTPriceControler.CLEARING_METHOD_FILED].ToString();
                    string quoteType = dr[CTPriceControler.QUOTE_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.QUOTE_TYPE_FILED].ToString();
                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();
                    string quoteStatus = dr[CTPriceControler.QUOTE_STATUS_FIELD] == null ? string.Empty : dr[CTPriceControler.QUOTE_STATUS_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];
                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();
                    //2-收益率反算净价  4-净价反算收益率  0-原始值
                    if (string.IsNullOrEmpty(calFlag))
                    {
                        calFlag = "0";
                    }
                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);
                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    // priceType 1 - 净价；2 - 全价；3 - 收益率  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    if (calFlag == "2")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    if (calFlag == "4")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                    }
                    if (priceType == "1")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                        calFlag = "4"; //第三方计算的收益率
                    }
                    if (priceType == "3")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    string securityId = dr[CTPriceControler.SECURITY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.SECURITY_ID_FIELD].ToString();
                    string side = dr[CTPriceControler.SIDE_FIELD] == null ? string.Empty : dr[CTPriceControler.SIDE_FIELD].ToString();

                    DepthEntry depthEntry = depthEntries.Find(e => e.PriceDecimal.CompareTo(CommonUtils.ConvertToDecimal(price)) == 0);
                    if (depthEntry != null)
                    {
                        depthEntry.Show = true;
                        //方向买/卖
                        depthEntry.BuySell = "0".Equals(side) ? "BUY" : "SELL";
                        if ("0".Equals(side))
                        {
                            depthEntry.BuyQty = (int)qty;
                        }
                        else
                        {
                            depthEntry.SellQty = (int)qty;
                        }
                    }
                }
            }
        }

        //Tick模式下初始化全档位
        private void initListTickEntries(List<DepthEntry> tickEntries, decimal? lastPrice,
            decimal? buyLow, decimal? sellHigh,
            decimal? buy1, decimal? sell1,
            decimal? upperLimit, decimal? lowerLimit)
        {
            decimal temp = upperLimit.Value;
            int index = 1;
            while (temp.CompareTo(lowerLimit.Value) >= 0)
            {
                DepthEntry depthEntry = new DepthEntry();
                depthEntry.Price = temp.ToString("F3");
                depthEntry.PriceDecimal = temp;
                depthEntry.SecurityId = SecurityID;
                depthEntry.Index = index;
                if (temp.CompareTo(buy1) > 0)
                {
                    depthEntry.BuySell = "SELL";
                }
                else
                {
                    depthEntry.BuySell = "BUY";
                }
                //行情上下价格之间的价位 显示成灰色
                if (temp.CompareTo(sellHigh) <= 0 && temp.CompareTo(buyLow) >= 0)
                {
                    depthEntry.BuySell = "NONE";
                }
                depthEntry.Show = true;
                tickEntries.Add(depthEntry);
                temp -= 0.005m;
                index++;
            }
        }

        /// <summary>
        /// 使用行情数据更新TickEntries
        /// </summary>
        private void UpdateTickEntries(DataTable tempDt, decimal? lastPrice,
            decimal? upperLimit, decimal? lowerLimit)
        {
            if (tempDt != null && tempDt.Rows.Count > 0)
            {
                int count = tempDt.Rows.Count;
                DataRow firstRow = tempDt.Rows[0];
                DataRow lastRow = tempDt.Rows[count - 1];
                int buy1 = count / 2 - 1;    //买一
                int sell1 = count / 2;      //卖一
                DataRow buy1Row = tempDt.Rows[buy1];
                DataRow sell1Row = tempDt.Rows[sell1];
                string priceBuy = CommonUtils.formatString(CommonUtils.ConvertToString(firstRow[CTPriceControler.ENTRY_PX_FIELD]), 4);
                string priceSell = CommonUtils.formatString(CommonUtils.ConvertToString(lastRow[CTPriceControler.ENTRY_PX_FIELD]), 4);
                //买一
                string priceBuy1 = CommonUtils.formatString(CommonUtils.ConvertToString(buy1Row[CTPriceControler.ENTRY_PX_FIELD]), 4);
                //卖一
                string priceSell1 = CommonUtils.formatString(CommonUtils.ConvertToString(sell1Row[CTPriceControler.ENTRY_PX_FIELD]), 4);
                //Tick深度行情初始化
                //FullTickEntries.Clear();
                //initFullTickEntries(FullTickEntries, lastPrice, upperLimit, lowerLimit);
                ListTickEntries.Clear();
                initListTickEntries(ListTickEntries, lastPrice,
                    CommonUtils.ConvertToDecimal(priceBuy), CommonUtils.ConvertToDecimal(priceSell),
                    CommonUtils.ConvertToDecimal(priceBuy1), CommonUtils.ConvertToDecimal(priceSell1),
                    upperLimit, lowerLimit);
                foreach (DataRow dr in tempDt.Rows)
                {
                    string mdPriceLevel_Str = dr[CTPriceControler.MD_PRICE_LEVEL_FIELD] == null ? string.Empty : dr[CTPriceControler.MD_PRICE_LEVEL_FIELD].ToString();
                    int mdPriceLevel = CommonUtils.ConvertToInt(mdPriceLevel_Str);
                    string key = dr[CTPriceControler.KEY_FIELD] == null ? string.Empty : dr[CTPriceControler.KEY_FIELD].ToString();
                    string venueTypeGW = dr[CTPriceControler.VENUE_TYPE_GW_FIELD] == null ? string.Empty : dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString();
                    string time = dr[CTPriceControler.TIME_FIELD] == null ? string.Empty : dr[CTPriceControler.TIME_FIELD].ToString();
                    string firmFlag = dr[CTPriceControler.FIRM_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.FIRM_TYPE_FIELD].ToString();
                    string text = dr[CTPriceControler.TEXT_FIELD] == null ? string.Empty : dr[CTPriceControler.TEXT_FIELD].ToString();
                    string price = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.ENTRY_PX_FIELD]), 4);
                    decimal netPrice = CommonUtils.ConvertToDecimal(dr[CTPriceControler.NET_PRICE_FIELD] == null ? string.Empty : dr[CTPriceControler.NET_PRICE_FIELD].ToString());
                    long qty = CommonUtils.ConvertToLong(dr[CTPriceControler.QTY_FIELD] == null ? string.Empty : dr[CTPriceControler.QTY_FIELD].ToString());
                    string settlType = dr[CTPriceControler.SETTL_TYPE_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_FIELD].ToString();
                    string settlType_yuan = dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD] == null ? string.Empty : dr[CTPriceControler.SETTL_TYPE_YUAN_FIELD].ToString();
                    // 来源
                    string venues = dr[CTPriceControler.SOURCE_FIELD] == null ? string.Empty : dr[CTPriceControler.SOURCE_FIELD].ToString();
                    string partySubName = dr[CTPriceControler.PARTY_SUB_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_SUB_NAME_FIELD].ToString();
                    //string source = GenerateSourceByVenues(venues, partySubName);
                    // 询问
                    string bargainFlag = dr[CTPriceControler.BARGAIN_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.BARGAIN_FLAG_FIELD].ToString();
                    string wholeFlag = dr[CTPriceControler.WHOLE_FLAG_FIELD] == null ? string.Empty : dr[CTPriceControler.WHOLE_FLAG_FIELD].ToString();
                    string institutionName = dr[CTPriceControler.INSTITUTION_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.INSTITUTION_NAME_FIELD].ToString();
                    string showTrader = dr[CTPriceControler.SHOWTRADER_FIELD] == null ? string.Empty : dr[CTPriceControler.SHOWTRADER_FIELD].ToString();
                    string traderName = dr[CTPriceControler.TRADERNAME_FIELD] == null ? string.Empty : dr[CTPriceControler.TRADERNAME_FIELD].ToString();
                    string transactTime = dr[CTPriceControler.TRANSACT_TIME_FILED] == null ? string.Empty : dr[CTPriceControler.TRANSACT_TIME_FILED].ToString();
                    string deliveryType = dr[CTPriceControler.DELIVERY_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.DELIVERY_TYPE_FILED].ToString();
                    string clearingMethod = dr[CTPriceControler.CLEARING_METHOD_FILED] == null ? string.Empty : dr[CTPriceControler.CLEARING_METHOD_FILED].ToString();
                    string quoteType = dr[CTPriceControler.QUOTE_TYPE_FILED] == null ? string.Empty : dr[CTPriceControler.QUOTE_TYPE_FILED].ToString();
                    string partyName = dr[CTPriceControler.PARTY_NAME_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_NAME_FIELD].ToString();
                    string partyID = dr[CTPriceControler.PARTY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.PARTY_ID_FIELD].ToString();
                    string quoteStatus = dr[CTPriceControler.QUOTE_STATUS_FIELD] == null ? string.Empty : dr[CTPriceControler.QUOTE_STATUS_FIELD].ToString();
                    bool isMe = (bool)dr[CTPriceControler.ISME_FIELD];
                    string isInner = dr[CTPriceControler.VENUE_TYPE_GW_FIELD].ToString() == "INNER" ? "1" : "0";
                    string marketIndicator = dr[CTPriceControler.MARKETINDICATOR_FIELD] == null ? string.Empty : dr[CTPriceControler.MARKETINDICATOR_FIELD].ToString();
                    string calFlag = dr[CTPriceControler.CalFlag_Field] == null ? string.Empty : dr[CTPriceControler.CalFlag_Field].ToString();
                    //2-收益率反算净价  4-净价反算收益率  0-原始值
                    if (string.IsNullOrEmpty(calFlag))
                    {
                        calFlag = "0";
                    }
                    string yield = CommonUtils.formatString(CommonUtils.ConvertToString(dr[CTPriceControler.Yield_Field]), 4);
                    string priceType = dr[CTPriceControler.PriceType_Field] == null ? "3" : dr[CTPriceControler.PriceType_Field].ToString();
                    // priceType 1 - 净价；2 - 全价；3 - 收益率  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
                    //calFlag 2-收益率反算净价  4-净价反算收益率 0-原始值
                    string calTypeFieldTip = string.Empty; // 
                    string calTypeNetPriceTip = string.Empty; // 
                    if (calFlag == "2")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    if (calFlag == "4")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                    }
                    if (priceType == "1")
                    {
                        calTypeFieldTip = $"试算: {yield}% 换算仅供参考";
                        calFlag = "4"; //第三方计算的收益率
                    }
                    if (priceType == "3")
                    {
                        calTypeNetPriceTip = $"净价: {netPrice} 换算仅供参考";
                    }
                    string securityId = dr[CTPriceControler.SECURITY_ID_FIELD] == null ? string.Empty : dr[CTPriceControler.SECURITY_ID_FIELD].ToString();
                    string side = dr[CTPriceControler.SIDE_FIELD] == null ? string.Empty : dr[CTPriceControler.SIDE_FIELD].ToString();

                    //更新Tick行情相应价位的买/卖量
                    //DepthEntry depthEntry = FullTickEntries.ToList().Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    DepthEntry depthEntry = ListTickEntries.Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    if (depthEntry != null)
                    {
                        //方向买/卖
                        if ("0".Equals(side))
                        {
                            depthEntry.BuySell = "BUY";
                            depthEntry.BuyQty = (int)qty;
                        }
                        else
                        {
                            depthEntry.BuySell = "SELL";
                            depthEntry.SellQty = (int)qty;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 使用基础数据更新DepthEntries
        /// </summary>
        private void UpdateDepthEntries(DataTable tempDt, decimal? lastPrice, decimal? upperLimit, decimal? lowerLimit)
        {
            int count = tempDt.Rows.Count;
            DataRow firstRow = tempDt.Rows[0];
            DataRow lastRow = tempDt.Rows[count - 1];
            int buy1 = count / 2 - 1;    //买一
            int sell1 = count / 2;      //卖一
            DataRow buy1Row = tempDt.Rows[buy1];
            DataRow sell1Row = tempDt.Rows[sell1];
            string priceBuy = CommonUtils.formatString(CommonUtils.ConvertToString(firstRow[CTPriceControler.ENTRY_PX_FIELD]), 4);
            string priceSell = CommonUtils.formatString(CommonUtils.ConvertToString(lastRow[CTPriceControler.ENTRY_PX_FIELD]), 4);
            //买一
            string priceBuy1 = CommonUtils.formatString(CommonUtils.ConvertToString(buy1Row[CTPriceControler.ENTRY_PX_FIELD]), 4);
            //卖一
            string priceSell1 = CommonUtils.formatString(CommonUtils.ConvertToString(sell1Row[CTPriceControler.ENTRY_PX_FIELD]), 4);
            //initFullDepthEntries(tempDt, FullDepthEntries, lastPrice,
            initListDepthEntries(tempDt, ListDepthEntries, lastPrice,
                            CommonUtils.ConvertToDecimal(priceBuy), CommonUtils.ConvertToDecimal(priceSell),
                            CommonUtils.ConvertToDecimal(priceBuy1), CommonUtils.ConvertToDecimal(priceSell1),
                            upperLimit, lowerLimit);
        }

        private void UpdateMyOrders()
        {
            IDictionary priceMap = new Dictionary<string, string>();
            //先清空策略买/卖，手工买/卖 数量
            //foreach (DepthEntry depth in FullDepthEntries)
            foreach (DepthEntry depth in ListDepthEntries)
            {
                depth.ManualBuyQty = 0;
                depth.StrategyBuyQty = 0;
                depth.ManualSellQty = 0;
                depth.StrategySellQty = 0;
            }
            //foreach (DepthEntry depth in FullTickEntries)
            foreach (DepthEntry depth in ListTickEntries)
            {
                depth.ManualBuyQty = 0;
                depth.StrategyBuyQty = 0;
                depth.ManualSellQty = 0;
                depth.StrategySellQty = 0;
            }
            LogHandler.Info("我的订单，数量{0}", MyOrdersDV.Table.Rows.Count);
            foreach (DataRow item in MyOrdersDV.Table.Rows)
            {
                if (!priceMap.Contains(item[MyOrderProcesser.PRICE_ORDER_FIELD]))
                {
                    string price = item[MyOrderProcesser.PRICE_ORDER_FIELD].ToString();
                    string side = item[MyOrderProcesser.SIDE_ORDER_FIELD].ToString();
                    string manualBuyQty = MyOrdersDV.Table.Compute("SUM(QtyNum)", $"Side='1' and Price='{price}' and QuantID=''").ToString();
                    string manualSellQty = MyOrdersDV.Table.Compute("SUM(QtyNum)", $"Side='2' and Price='{price}' and QuantID=''").ToString();
                    string strategyBuyQty = MyOrdersDV.Table.Compute("SUM(QtyNum)", $"Side='1' and Price='{price}' and QuantID<>''").ToString();
                    string strategySellQty = MyOrdersDV.Table.Compute("SUM(QtyNum)", $"Side='2' and Price='{price}' and QuantID<>''").ToString();
                    //DepthEntry entryDepth = FullDepthEntries.ToList().Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    DepthEntry entryDepth = ListDepthEntries.Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    if (entryDepth != null)
                    {
                        if (!string.IsNullOrEmpty(manualBuyQty))
                        {
                            int qty1 = int.Parse(manualBuyQty);
                            entryDepth.ManualBuyQty = qty1;
                        }
                        if (!string.IsNullOrEmpty(strategyBuyQty))
                        {
                            int qty2 = int.Parse(strategyBuyQty);
                            entryDepth.StrategyBuyQty = qty2;
                        }
                        if (!string.IsNullOrEmpty(manualSellQty))
                        {
                            int qty1 = int.Parse(manualSellQty);
                            entryDepth.ManualSellQty = qty1;
                        }
                        if (!string.IsNullOrEmpty(strategySellQty))
                        {
                            int qty2 = int.Parse(strategySellQty);
                            entryDepth.StrategySellQty = qty2;
                        }
                    }
                    //DepthEntry entryTick = FullTickEntries.ToList().Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    DepthEntry entryTick = ListTickEntries.Find(e => price.ConvertToDecimal().CompareTo(e.Price.ConvertToDecimal()) == 0);
                    if (entryTick != null)
                    {
                        if (!string.IsNullOrEmpty(manualBuyQty))
                        {
                            int qty1 = int.Parse(manualBuyQty);
                            entryTick.ManualBuyQty = qty1;
                        }
                        if (!string.IsNullOrEmpty(strategyBuyQty))
                        {
                            int qty2 = int.Parse(strategyBuyQty);
                            entryTick.StrategyBuyQty = qty2;
                        }
                        if (!string.IsNullOrEmpty(manualSellQty))
                        {
                            int qty1 = int.Parse(manualSellQty);
                            entryTick.ManualSellQty = qty1;
                        }
                        if (!string.IsNullOrEmpty(strategySellQty))
                        {
                            int qty2 = int.Parse(strategySellQty);
                            entryTick.StrategySellQty = qty2;
                        }
                    }
                    priceMap.Add(price, side);
                }
            }
        }

        //计算汇总合计量
        public void UpdateQtySum()
        {
            StrategyBuyQtySum = 0;
            StrategySellQtySum = 0;
            ManualBuyQtySum = 0;
            ManualSellQtySum = 0;
            BuyQtySum = 0;
            SellQtySum = 0;
            if (IsFifthDepth)
            {
                //foreach (DepthEntry entry in FullDepthEntries)
                foreach (DepthEntry entry in ListDepthEntries)
                {
                    StrategyBuyQtySum += entry.StrategyBuyQty;
                    StrategySellQtySum += entry.StrategySellQty;
                    ManualBuyQtySum += entry.ManualBuyQty;
                    ManualSellQtySum += entry.ManualSellQty;
                    BuyQtySum += entry.BuyQty;
                    SellQtySum += entry.SellQty;
                }
            }
            else
            {
                //foreach (DepthEntry entry in FullTickEntries)
                foreach (DepthEntry entry in ListTickEntries)
                {
                    StrategyBuyQtySum += entry.StrategyBuyQty;
                    StrategySellQtySum += entry.StrategySellQty;
                    ManualBuyQtySum += entry.ManualBuyQty;
                    ManualSellQtySum += entry.ManualSellQty;
                    BuyQtySum += entry.BuyQty;
                    SellQtySum += entry.SellQty;
                }
            }
        }

        // 买入提示
        private string buyTip = string.Empty;
        public string BuyTip
        {
            get
            {
                return "+" + SendPriceItem.Amount;
            }
        }
        // 卖出提示
        private string sellTip = string.Empty;
        public string SellTip
        {
            get
            {
                return "+" + SendPriceItem.Amount;
            }
        }

        private int _firstVisibleRowIndex = 0;
        public int FirstVisibleRowIndex
        {
            get { return _firstVisibleRowIndex; }
            set
            {
                _firstVisibleRowIndex = value;
            }
        }
        private decimal firstVisibleRowPrice = decimal.Zero;
        public decimal FirstVisibleRowPrice
        {
            get { return firstVisibleRowPrice; }
            set
            {
                firstVisibleRowPrice = value;
                this.OnPropertyChanged();
            }
        }

        //阶梯下单板第一次初始化， 为了切换之后可以居中
        private bool depthInit= false;
        public bool DepthInit
        {
            get { return depthInit; }
            set
            {
                depthInit = value;
                this.OnPropertyChanged(nameof(DepthInit));
            }
        }
    }
}

