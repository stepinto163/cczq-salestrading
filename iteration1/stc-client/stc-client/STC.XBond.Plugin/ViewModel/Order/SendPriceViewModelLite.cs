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
using STC.Common.View.Card;
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View;
using STC.Common.ViewModel.Adapter;
using STC.XBond.Plugin.ViewModel.Data;
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

namespace STC.XBond.Plugin.ViewModel.Order
{
    /// <summary>
    /// 报价界面SendPointPriceView的数据内容
    /// </summary>
    public class SendPriceViewModelLite : ViewModelBase
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

        public ICommand ExpandCommand { get { return new RelayCommand(OnExpandClick); } }

        /// <summary>
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;

        #region 构造

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

        ~SendPriceViewModelLite()
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

                if (!this._isSetPermission)
                    ComboBoxSelectIndex = -1;

                if (!oldIndex.Equals(value))
                    UpdateBestPrice();
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
                        ChangeUI();

                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");
                }

                UpdateDirectionItem();

                UpdateBestPrice();

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
                    SetPriceAndOrderHeighWholeFlag(FirmChecked);
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

                        SetListBoxItemEnable(listBox, false);
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
                        SetListBoxItemEnable(listBox, false);

                        listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                        SetListBoxItemEnable(listBox, false);
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

            if (CheckNotOut || (!_isXBond && CheckInner))
            {
                if (CheckXBond)
                {
                    if (decimal.TryParse(useQty, out dAmount))
                        useMarketQty = sendPriceItem.MarketQty = ((myWindow.FindName("lbDirect") as ListBox).SelectedItems.Count * dAmount).ToString();
                }
                else
                    useMarketQty = sendPriceItem.MarketQty = "0";
            }
            else if (DefaultCheck)
            {
                if (!string.IsNullOrEmpty(useRandom))
                {
                    if (decimal.TryParse(useRandom, out dRandom) && decimal.TryParse(useQty, out dAmount))
                        useMarketQty = sendPriceItem.MarketQty = ((DefaultFirstItems.Count + dRandom) * dAmount).ToString();
                }
                else
                {
                    if (decimal.TryParse(SendPriceItem.RandomNum, out dRandom) && decimal.TryParse(useQty, out dAmount))
                        useMarketQty = sendPriceItem.MarketQty = ((DefaultFirstItems.Count + dRandom) * dAmount).ToString();
                    else if (decimal.TryParse(useQty, out dAmount))
                        useMarketQty = sendPriceItem.MarketQty = (DefaultFirstItems.Count * dAmount).ToString();
                }
            }
            else if (DirectionCheck)
            {
                if (decimal.TryParse(useQty, out dAmount))
                    useMarketQty = sendPriceItem.MarketQty = ((myWindow.FindName("lbDirect") as ListBox).SelectedItems.Count * dAmount).ToString();
            }
            else
            {
                useMarketQty = sendPriceItem.MarketQty = "0";
            }

            if (decimal.TryParse(useQty, out dAmount) && decimal.TryParse(useMarketQty, out dRandom))
            {
                sendPriceItem.CustomQty = (dAmount + dRandom).ToString();
            }

            if ((myWindow.FindName("lbDirect") as ListBox).SelectedItems.Count <= 1)
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

                        SetListBoxItemEnable(listBox, true);
                        listBox = myWindow.FindName("lbFirstLevel") as ListBox;
                        SetListBoxItemEnable(listBox, false);

                        listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                        SetListBoxItemEnable(listBox, false);

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

                        SetListBoxItemEnable(listBox, false);
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

                this.OnPropertyChanged();
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
                        CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
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

                    CheckAll = true;
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

                    SetListBoxItemEnable(listBox, true);

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

                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    netPrice = string.Empty;
                    this.OnPropertyChanged("NetPrice");
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

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
                this.OnPropertyChanged();

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

        //private int amountBoxIndex = 0;

        //public int AmountBoxIndex
        //{
        //    get
        //    {
        //        return amountBoxIndex;
        //    }

        //    set
        //    {
        //        amountBoxIndex = value;
        //        this.OnPropertyChanged();

        //        ChangeUI();

        //        OnWholeFlag();

        //    }
        //}

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
                comboBoxSelectIndex = value;

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

                this.OnPropertyChanged(nameof(ComboBoxSelectIndex));
            }
        }

        /// <summary>
        /// 撮合渠道选择”所有“
        /// </summary>
        private bool _checkAll = false;
        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }

            set
            {

                DirectionEnable = true;
                if (value != _checkAll)
                {
                    _checkAll = value;

                    if (value)
                    {
                        ////myWindow.Height = _isXBond ? 488 : 488;// 548;
                        ChanelAllVisibility = Visibility.Visible;
                        ////CheckXBond = true;
                        _checkInner = _isXBond ? true : false;
                        this.OnPropertyChanged(nameof(this.CheckInner));

                        CheckXBondVisibility = Visibility.Collapsed;

                        if (DirectionItems != null)
                        {
                            foreach (ListBoxItemData itemData in DirectionItems)
                            {
                                itemData.ItemVisibility = Visibility.Visible;
                                //if (itemData.Name.ToUpper().Equals(XBOND))
                                //{
                                    itemData.Checked = false;
                                //    if (FirmChecked)
                                //    {
                                //        itemData.ItemEnable = false;
                                //    }
                                //    else
                                //    {
                                //        itemData.ItemEnable = true;
                                //    }
                                //}
                                //else
                                    itemData.ItemEnable = true;
                            }

                            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                            AllowanceChannelVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                        }

                        SetPriceAndOrderHeight();
                    }
                    else
                    {
                        bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                        if ((!isAllow && !FirmChecked && !_wholeFlagChecked
                            && ((SettleBoxIndex == 2 && !_isXBond && !_sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || (SettleBoxIndex == 0 || SettleBoxIndex == 1)))
                            || (!isAllow && _isXBond))
                            CheckXBondVisibility = CheckVenuePermission(XBOND) ? Visibility.Visible : Visibility.Collapsed;
                        else if (CheckXBondVisibility == Visibility.Visible)
                            CheckXBondVisibility = Visibility.Collapsed;

                        _checkInner = true;
                        this.OnPropertyChanged(nameof(this.CheckInner));
                        OnCheckInner(true);
                    }

                    this.OnPropertyChanged();
                }
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

        /// <summary>
        /// 撮合渠道选择”仅内部“
        /// </summary>
        /// <param name="checkStatus"></param>
        private void OnCheckInner(bool checkStatus)
        {
            if (checkStatus)
            {
                bool isShowXBond = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID) || AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);

                ChanelAllVisibility = Visibility.Collapsed;

                CheckAll = false;

                if (isShowXBond && !FirmChecked && !_wholeFlagChecked
                    && ((SettleBoxIndex == 2 && !_isXBond && !_sendPriceAdapter.IsFwdSettle(SelectDate, ComboBoxSelectIndex)) || (SettleBoxIndex == 0 || SettleBoxIndex == 1)))
                    CheckXBondVisibility = CheckVenuePermission(XBOND) ? Visibility.Visible : Visibility.Collapsed;

                if (CheckXBondVisibility == Visibility.Visible && isShowXBond)
                {
                    CheckXBond = true;

                    OnlyShowXBond();
                }

                if (_isXBond)
                {
                    ////myWindow.Height = 458;
                    AllowanceChannelVisibility = Visibility.Visible;
                }
                else
                {
                    LogHelper.Info("CheckXBondVisibility:" + CheckXBondVisibility + ", isShowXBond:" + isShowXBond);
                    if (!(CheckXBondVisibility == Visibility.Visible && isShowXBond))
                    {
                        ////myWindow.Height = 394;
                        AllowanceChannelVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ////myWindow.Height = 458;
                        AllowanceChannelVisibility = Visibility.Visible;
                    }
                }
                //LogHelper.Info("-----Height:" + myWindow.Height);
            }
            else
            {
                if (!_isXBond)
                {
                    AllowanceChannelVisibility = Visibility.Visible;

                    _innerDefaultVisibility = Visibility.Visible;
                    this.OnPropertyChanged("InnerDefaultVisibility");

                    CheckXBond = false;
                    CheckXBondVisibility = Visibility.Collapsed;
                    if (DirectionItems != null)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            itemData.ItemVisibility = Visibility.Visible;
                            if (itemData.Name.ToUpper().Equals(XBOND))
                            {
                                itemData.ItemEnable = IsShowXBondItem() && DirectionCheck;
                                if (!itemData.ItemEnable)
                                    itemData.Checked = false;
                            }
                            else
                                itemData.ItemEnable = DirectionCheck;

                            if (!DirectionCheck)
                                itemData.Checked = DirectionCheck;
                        }
                    }

                    ////myWindow.Height = 488;// 548;
                    DirectionEnable = true;
                }

                //_checkAll = true;
                CheckAll = true;
            }
            //LogHelper.Info("Height:" + myWindow.Height);
            SetPriceAndOrderHeight();
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

                OnCheckInner(value);
                this.OnPropertyChanged();
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

                this.OnPropertyChanged();
            }
        }

        private Visibility _checkXBondVisibility = Visibility.Visible;
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


        private Visibility _chanelAllVisibility = Visibility.Visible;
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
                    SetListBoxItemEnable(listBox, false);

                    listBox = myWindow.FindName("lbFirstLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);

                    listBox = myWindow.FindName("lbSecondLevel") as ListBox;
                    SetListBoxItemEnable(listBox, false);

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

                this.OnPropertyChanged();

                ////if (InnerDefaultVisibility == Visibility.Collapsed && myWindow.Height == 488 && CheckXBondVisibility == Visibility.Visible)
                if (InnerDefaultVisibility == Visibility.Collapsed && CheckXBondVisibility == Visibility.Visible)
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
                        DirectionEnable = DirectionCheck = true;

                        if (DirectionItems != null)
                        {
                            foreach (ListBoxItemData itemData in DirectionItems)
                            {
                                if (!itemData.Name.ToUpper().Equals(XBOND))
                                    itemData.ItemVisibility = Visibility.Collapsed;
                                else
                                {
                                    itemData.ItemEnable = true;
                                    itemData.Checked = true;
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

                this.OnPropertyChanged();
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

        /// <summary>
        /// 是否有内部撮合权限
        /// </summary>
        private bool _isHaveInner = false;
        public bool IsHaveInner
        {
            get
            {
                return _isHaveInner;
            }

            set
            {
                _isHaveInner = value;
                this.OnPropertyChanged(nameof(IsHaveInner));
            }
        }

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

        private DispatcherTimer timer = null;
        private bool isHvaeNewExecution = false;
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
                        if (result.Data != null && result.IsSuccess())
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
                                        else
                                        {
                                            if (_isXBond)
                                                sendPriceItem.CalWarnVisibility = Visibility.Visible;
                                        }
                                    }

                                    isSendReq = false;
                                }));
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
                        _checkAllVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckAllVisibility");
                        _checkAll = false;
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

                    _checkInnerEnable = true;
                    _checkXBondEnable = true;

                    this.OnPropertyChanged("CheckInnerEnable");
                    this.OnPropertyChanged("CheckXBondEnable");

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
                    bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                    if (isAllow)
                    {
                        if (!CheckAll)
                        {
                            if (CheckVenuePermission(XBOND))
                            {
                                _checkXBondVisibility = Visibility.Visible;
                                this.OnPropertyChanged("CheckXBondVisibility");
                            }

                            _checkXBond = _isXBond ? true : false;
                            this.OnPropertyChanged("CheckXBond");

                            ////myWindow.Height = _isXBond ? 458 : 488;// 548

                            if (!_wholeFlagChecked && !firmChecked)
                            {
                                _checkAll = false;
                                _checkInner = false;
                                _checkXBond = false;

                                this.OnPropertyChanged("CheckAll");
                                this.OnPropertyChanged("CheckInner");
                                this.OnPropertyChanged("CheckXBond");

                                _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                                this.OnPropertyChanged("CheckAllVisibility");
                            }
                        }
                        else
                        {
                            _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                            this.OnPropertyChanged("CheckAllVisibility");
                        }
                    }
                    else
                    {
                        isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);

                        _checkAllVisibility = Visibility.Visible;
                        this.OnPropertyChanged("CheckAllVisibility");

                        _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
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

                //if (_wholeFlagChecked)
                if (_wholeFlagChecked || FirmChecked)
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
                    if (CheckXBondVisibility == Visibility.Visible)
                    {
                        _checkXBondVisibility = Visibility.Collapsed;
                        this.OnPropertyChanged("CheckXBondVisibility");

                        _checkXBond = false;
                        this.OnPropertyChanged("CheckXBond");
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
                else
                {
                    if (!(FirmChecked || _wholeFlagChecked))
                    {
                        bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                        if (isAllow)
                        {
                            if (!FirmChecked && !CheckAll)
                            {
                                _checkXBondVisibility = Visibility.Visible;
                                this.OnPropertyChanged("CheckXBondVisibility");

                                _checkXBond = _isXBond ? true : false;
                                this.OnPropertyChanged("CheckXBond");
                            }
                        }
                        else
                        {
                            isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID) && !FirmChecked;

                            _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                            this.OnPropertyChanged("CheckXBondVisibility");

                            _checkXBond = _isXBond ? true : false;
                            this.OnPropertyChanged("CheckXBond");

                        }
                    }

                    if (DirectionCheck && DirectionItems != null && !FirmChecked)
                    {
                        foreach (ListBoxItemData itemData in DirectionItems)
                        {
                            if (itemData.Name.ToUpper().Equals(XBOND))
                                itemData.ItemEnable = true;
                        }
                    }
                }

                 OnCheckInner(_checkInner);
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
                        itemData.Checked = true;
                        if (_isXBond)
                            itemData.ItemEnable = false;
                        else
                            itemData.ItemEnable = true;
                    }
                }
            }
        }

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
        public ICommand BidCommand { get { return new RelayCommand(OnBidClick); } }
        public ICommand OfrCommand { get { return new RelayCommand(OnOfrClick); } }
        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }

        public ICommand RefreshCommand { get { return new RelayCommand(OnRefreshClick); } }

        public ICommand CancelCommand { get { return new RelayCommand(OnCancelClick); } }
        public ICommand OkCommand { get { return new RelayCommand(OnConfirmClick); } }
        public ICommand AppointCommand { get { return new RelayCommand(AppointClick); } }
        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(OnDownClick); } }
        public ICommand XBondCommand { get { return new RelayCommand(OnXBondClick); } }

        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new RelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new RelayCommand(ReducePriceClick); } }
        // + - Amout
        public ICommand AddAmountCommand { get { return new RelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new RelayCommand(ReduceAmountClick); } }

        #endregion

        #region UI Amout,Price  调整
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
                    OnThreeClick();
                }
                else if (setAmountTxtIndex == 2)
                {
                    OnFiveClick();
                }
                this.OnPropertyChanged(nameof(SetAmountTxtIndex));
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

        #endregion

        /// <summary>
        /// 切换至X-Bond报价
        /// </summary>
        private void OnXBondClick()
        {
            DefaultCheck = false;
            DirectionCheck = true;
            CheckNotOut = false;
            _isXBond = !_isXBond;

            SendPriceType = _isXBond ? NORMAL_TYPE : XBOND_TYPE;

            sendPriceItem.NetPriceVisible = _isXBond ? Visibility.Visible : Visibility.Collapsed;
            _isFromXBond = true;
            ChangeUI();

            if (!_isXBond)
            {
                bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                if (isAllow)
                {
                    ////myWindow.Height = 488;
                    CheckAll = true;
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

        public string IsEdit
        {
            get
            {
                return string.IsNullOrEmpty(sendPriceItem.Amount) ? "True" : "False";
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
                    CheckAll = true;
                else if (priceMatchParam.CrossVenues.Equals("2"))
                {
                    CheckInner = true;
                    CheckXBond = false;
                }
                else if (priceMatchParam.CrossVenues.Equals("3"))
                {
                    CheckInner = true;
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

        /// <summary>
        /// 设置渠道项是否可用
        /// </summary>
        /// <param name="listBox">渠道项列表</param>
        /// <param name="isEnable">是否可用， true：可用； false:不可用</param>
        private void SetListBoxItemEnable(ListBox listBox, bool isEnable)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (ListBoxItemData itemData in listBox.Items)
                {
                    if (itemData != null)
                    {
                        itemData.ItemEnable = isEnable;
                    }
                }
            }));
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
                SendPriceItem.Amount = (Decimal.Parse(priceMatchParam.CustomFirmAmount) / 1000).ToString();
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
            this.IsBidSelect = isBid ? "1" : "0";
            _sendPriceAdapter = new SendPriceModelAdapter();

            SendPriceType = XBOND_TYPE;
            string goodsType = string.Empty;
            currentGoodsType = goodsType = CommonUtils.SetGoodsShortname(security);

            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = security;

            if (!string.IsNullOrEmpty(security))
            {
                List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(security, 1);
                if (bondsInfoPos != null && bondsInfoPos.Count > 0)
                {
                    _bondsInfoPo = bondsInfoPos[0];
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
            //this.IsHaveInner = true;
            this.IsHaveInner = GetIsHaveInner(goodsType);
            this.VenueVisible = this.IsHaveInner ? Visibility.Visible : Visibility.Collapsed;

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
                    SendPriceItem.Amount = (Decimal.Parse(Us.userQty)/1000).ToString();
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

            CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);

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
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            if (isAllow)
            {
                if (this.IsHaveInner)
                {
                    _checkInnerVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckInnerVisibility");
                }

                if (!firmChecked)
                {
                    _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                    this.OnPropertyChanged("CheckAllVisibility");

                    _checkXBondVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    ////myWindow.Height = _isXBond ? 488 : 488;// 548;
                }

                if (_isXBond)
                {
                    _checkAll = false;
                    _checkInner = true;
                    _checkXBond = true;

                }
                else
                {
                    if (!_wholeFlagChecked && !firmChecked)
                    {
                        _checkAll = true;
                        _checkInner = false;
                        _checkXBond = false;
                    }
                    else
                    {
                        _checkAll = false;
                        _checkInner = true;
                        _checkXBond = false;
                    }
                    _checkXBondVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");
                }

                if (!CheckVenuePermission(XBOND))
                {
                    XBondVisibility = Visibility.Collapsed;
                }

                this.OnPropertyChanged("CheckAll");
                this.OnPropertyChanged("CheckInner");
                this.OnPropertyChanged("CheckXBond");

            }
            else
            {
                _checkAllVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("CheckAllVisibility");


                ////myWindow.Height = _isXBond ? 488 : 394;

                _chanelAllVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("ChanelAllVisibility");

                isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                if (isAllow)
                {
                    _checkInnerVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckInnerVisibility");

                    _checkInner = true;
                    this.OnPropertyChanged("CheckInner");

                    _checkXBondVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    _xbondVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("XBondVisibility");
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);

                    _checkXBondVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    _checkInnerVisibility = isAllow ? Visibility.Visible : Visibility.Collapsed;
                    this.OnPropertyChanged("CheckInnerVisibility");

                    if (isAllow)
                    {
                        _checkInner = true;
                        this.OnPropertyChanged("CheckInner");

                        CheckXBond = true;
                    }
                    else
                    {
                        XBondVisibility = Visibility.Collapsed;
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

            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                string validUntiTimeSetting = userSetting.validUntiTimeSetting;
                string now = DateTime.Now.ToString("yyyy-MM-dd");
                sendPricePo.ValidUntilTime = string.Format("{0} {1}", now, validUntiTimeSetting);
                now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (now.CompareTo(sendPricePo.ValidUntilTime) >= 0)
                    sendPricePo.ValidUntilTime = string.Empty;
            }

            //sendPricePo.MarketIndicator = "4";

            if (_bondsInfoPo != null)
                sendPricePo.MarketIndicator = _bondsInfoPo.MarketIndicator;
            else
                sendPricePo.MarketIndicator = "4";


            sendPricePo.OrdType = "2";
            if (sendPriceItem.Amount != null)
            {
                sendPricePo.OrderQty = (decimal.Parse(sendPriceItem.Amount) * 1000).ToString();
            }
            sendPricePo.Price = CustomPrice;

            if (sendPricePo.MarketIndicator == "4")
                sendPricePo.NetPrice = NetPrice;
            else
                sendPricePo.NetPrice = "100";

            sendPricePo.SecurityID = sendPriceItem.Security;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = GetSetttle();

            sendPricePo.Side = isBid ? "1" : "2";

            sendPricePo.Terminal = string.Format(_isXBond ? "STClient_v{0}_xbond" : "STClient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = FirmChecked ? "0" : "1";
            
            sendPricePo.CrossVenues = _checkAll ? "1" : _checkInner ? "2" : string.Empty;

            if (_checkXBond && _checkXBondVisibility == Visibility.Visible)
                sendPricePo.CrossVenues = "3";

            if (!_wholeFlagChecked)
                sendPricePo.WholeFlag = "0";
            else
                sendPricePo.WholeFlag = "1";

            sendPricePo.HandlInst = _darkDiskFlagChecked ? "2" : "1";

            sendPricePo.QuantID = "";
            decimal d = 0;
            if (!FirmChecked && FirmTimeLimit && decimal.TryParse(sendPriceItem.NonfirmTime, out d) && d > 0)
                sendPricePo.FirmToNonTime = sendPriceItem.NonfirmTime.ToString();

            if (RandomEnable && CheckAll)
                sendPricePo.VenuesRdmNum = sendPriceItem.RandomNum;
            //else
            //    sendPricePo.VenuesRdmNum = "0";

            //if (CheckAll)
            //    sendPricePo.VenuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;
            //else if (_isXBond)
            //    sendPricePo.VenuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;

            if (!(_checkInner && _checkXBondVisibility == Visibility.Collapsed))
                sendPricePo.VenuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;

            if (defaultCheck)
            {
                sendPricePo.SelfSelectVenues = string.Empty;
            }
            else if (directionCheck)
            {
                sendPricePo.SelfSelectVenues = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
            }

            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");


            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            if (_priceMatchParam != null)
                sendPricePo.Info3 = _priceMatchParam.Info3;

            if (_priceMatchParam != null && _priceMatchParam.OldOrder != null)
            {
                sendPricePo.AlgoName = _priceMatchParam.OldOrder.AlgoName;
                sendPricePo.VenueTypeGW = _priceMatchParam.OldOrder.VenueTypeGW;

                //if (!(!FirmChecked && (!string.IsNullOrEmpty(_priceMatchParam.OldOrder.Venues) && _priceMatchParam.OldOrder.Venues.ToUpper().Equals("IDB"))))
                //    sendPricePo.Venues = _priceMatchParam.OldOrder.Venues;

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


            return sendPricePo;
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
                                    msgBox.Owner = Application.Current.MainWindow;
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
                SendPriceEnable = !isDoubleCheck;
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
                if (_isUpdatePrice)
                    UpdatePrice();
                else
                    PlaceOrder();
            }
            else
            {
                SendPriceEnable = true;
                SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle());
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
            else if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
                por.Reason = "未填写价格或价格为0";
            else if (!decimal.TryParse(sendPriceItem.Amount, out temD) || temD == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
                por.Reason = "未填写量或量为0";
            else if (string.IsNullOrEmpty(NetPrice))
            {
                if (calBondErrMsg.Length > 0)
                {
                    por.Reason = $"{calBondErrMsg}, 调用计算器服务失败,请重新打开窗口或修改报价参数";
                }
                else
                {
                    por.Reason = "调用计算器服务失败,请重新打开窗口或修改报价参数";
                }
            }
            else if (!CheckAll && !CheckInner && !CheckXBond)
                por.Reason = "必须选择一个撮合渠道";
            else if (!CheckAll && !CheckInner && CheckXBond)
                por.Reason = "撮合渠道不能单选X-bond";
            else if ((DirectionCheck && CheckAll) || (_isXBond && DirectionCheck))
            {
                string str = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
                if (string.IsNullOrEmpty(str))
                    por.Reason = "余量发送渠道-指定，必须选择一个渠道";
            }
            else if (CheckAll && !CheckNotOut && !DirectionCheck && !DefaultCheck)
                por.Reason = "余量发送渠道必须选择一个";
            else if (DefaultCheck && AllowanceChannelVisibility == Visibility.Visible)
            {
                decimal dd = 0;
                if (decimal.TryParse(SendPriceItem.RandomNum, out dd) && dd > defaultSecondItems.Count)
                    por.Reason = "随机渠道数不能大于二级渠道数";
            }

            // 检查个别情况的验证
            string venuesSelectF = string.Empty;
            if (!(_checkInner && _checkXBondVisibility == Visibility.Collapsed))
                venuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;
            if (venuesSelectF == "3")
            {
                string selfSelectVenues = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
                if (string.IsNullOrEmpty(selfSelectVenues))
                {
                    por.Reason = "余量发送渠道必须选择一个";
                }
            }

            if (_lastCalResult != null && _lastCalResult.Data != null && !string.IsNullOrEmpty(_lastCalResult.ReqData) && _lastCalResult.IsSuccess() && !string.IsNullOrEmpty(_lastReqData))
            {
                if (!_lastCalResult.ReqData.Equals(_lastReqData))
                {
                    por.Reason = "调用计算器服务太频繁,请稍等一会再修改报价参数";
                }
            }

            //decimal d = 0;
            //if (decimal.TryParse(sendPriceItem.Amount, out d) && (d % 100) > 0)
            //    por.Reason = string.Format("量必须是100的整数倍，当前输入值：{0}", sendPriceItem.Amount);


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
            if (CheckAllVisibility == Visibility.Collapsed && CheckInnerVisibility == Visibility.Collapsed && CheckXBondVisibility == Visibility.Collapsed)
                result = false;
            else if (string.IsNullOrEmpty(sendPriceItem.Security))
                result = false;
            else if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
                result = false;
            else if (CheckBondType() && string.IsNullOrEmpty(NetPrice))
                result = false;
            else if (!decimal.TryParse(sendPriceItem.Amount, out temD) || temD == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
                result = false;
            else if (!CheckAll && !CheckInner && !CheckXBond)
                result = false;
            else if (!CheckAll && !CheckInner && CheckXBond)
                result = false;
            else if ((DirectionCheck && CheckAll) || (_isXBond && DirectionCheck))
            {
                string str = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
                if (string.IsNullOrEmpty(str))
                    result = false;
            }
            else if (CheckAll && !CheckNotOut && !DirectionCheck && !DefaultCheck)
                result = false;
            else if (DefaultCheck && AllowanceChannelVisibility == Visibility.Visible)
            {
                decimal dd = 0;
                if (decimal.TryParse(SendPriceItem.RandomNum, out dd) && dd > defaultSecondItems.Count)
                    result = false;
            }

            if (_lastCalResult != null && _lastCalResult.Data != null && !string.IsNullOrEmpty(_lastCalResult.ReqData) && _lastCalResult.IsSuccess() && !string.IsNullOrEmpty(_lastReqData))
            {
                if (!_lastCalResult.ReqData.Equals(_lastReqData))
                {
                    result = false;
                }
            }

            // 检查个别情况的验证
            string venuesSelectF = string.Empty;
            if (!(_checkInner && _checkXBondVisibility == Visibility.Collapsed))
                venuesSelectF = _checkNotOut ? "1" : defaultCheck ? "2" : directionCheck ? "3" : string.Empty;
            if (venuesSelectF == "3")
            {
                string selfSelectVenues = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
                if (string.IsNullOrEmpty(selfSelectVenues))
                {
                    result = false;
                }
            }

            //decimal d = 0;
            //if (decimal.TryParse(sendPriceItem.Amount, out d) && (d % 100) > 0)
            //    result = false;

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

                if (CheckInner)
                {
                    selectDate = DateTime.Now;
                    this.OnPropertyChanged("SelectDate");
                }
                else
                {
                    try
                    {
                        if (_priceMatchParam == null)
                            this.CalBond("4", true);
                    }
                    catch (Exception e) { }
                }
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

            OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                SetPriceFocus();
            }
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

            OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            if (isInit == false)
            {
                CustomPrice = _sendPriceAdapter.GetInitBestPrice(isBid);
                SetPriceFocus();
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
                        myWindow.Width = 697;
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
                        listBox.Height = 420 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                        LineSplit.Y2 = 418 - (this.VenueVisible == Visibility.Visible ? 0 : 32); 
                    }
                    else
                    {
                        if (this.VenueVisible == Visibility.Visible)
                        {
                            listBox.Height = 356;
                            LineSplit.Y2 = 353;
                        }
                        else
                        {
                            listBox.Height = 420 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                            LineSplit.Y2 = 418 - (this.VenueVisible == Visibility.Visible ? 0 : 32);
                        }
                    }
                }
                else
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;
                    listBox.Height = 356;
                    LineSplit.Y2 = 353;
                }
            }));
        }


        private void SetPriceAndOrderHeighWholeFlag(bool isValue)
        {
            if (isValue)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;
                    listBox.Height = 360;
                }));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;
                    if ((FirmSelectIndex == 0) && (!WholeFlagChecked))
                    {
                        listBox.Height = 421;
                    }
                    else
                    {
                        listBox.Height = 360;
                    }
                }));
            }
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
                QuotationListViewModel Qm = new QuotationListViewModel();

                if (securityID != null)
                {
                    Qm.SecurityID = securityID;
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
                    if (bondsInfoPo != null)
                    {
                        Qm.Symbol = bondsInfoPo.Short_Name;
                    }
                }

                Qm.GetLastExecutionsItem(securityID);
                tradeBookView.RemoveDbDoubleClickEvent();
                tradeBookView.DataContext = Qm;
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

