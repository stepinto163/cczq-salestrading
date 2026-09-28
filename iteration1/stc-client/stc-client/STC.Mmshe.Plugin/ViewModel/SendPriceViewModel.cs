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
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.CustomeUI;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.View;
using STC.Mm.Plugin.View.Card;
using STC.Mm.Plugin.ViewModel.Adapter;
using STC.Mm.Plugin.ViewModel.Data;
using STC.XBond.Plugin.Utils;
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

namespace STC.Mm.Plugin.ViewModel
{
    /// <summary>
    /// 报价界面SendPointPriceView的数据内容
    /// </summary>
    public class SendPriceViewModel : ViewModelBase
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

        private Window myWindow;
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

        ~SendPriceViewModel()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();
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
                    this.OnPropertyChanged();
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

                this.OnPropertyChanged();
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

                this.OnPropertyChanged();

                if (!_isFromXBond)
                {
                    //bool directCheck = directionCheck;

                    ChangeUI();

                    //directionCheck = directCheck;
                    //this.OnPropertyChanged("DirectionCheck");
                }

                //UpdateDirectionItem();

                UpdateBestPrice();
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
            ListBox listBox = myWindow.FindName("lbSecondLevel") as ListBox;
            listBox.SelectedIndex = -1;

            listBox = myWindow.FindName("lbFirstLevel") as ListBox;
            listBox.SelectedIndex = -1;

            listBox = myWindow.FindName("lbDirect") as ListBox;
            listBox.SelectedIndex = -1;
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
                    ResetListBoxSelected();

                    if (defaultCheck)
                    {
                        RandomEnable = true;
                        CheckNotOut = DirectionCheck = false;

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
                    ResetListBoxSelected();
                    if (directionCheck)
                    {
                        CheckNotOut = DefaultCheck = false;

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

                this.OnPropertyChanged();

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
                foreach (ListBoxItemData itemData in DirectionItems)
                {
                    if (itemData.Name.ToUpper().Equals(XBOND))
                        itemData.ItemEnable = false;
                }
            }
        }

        private int settleBoxIndex = 0;

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

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();

                SetOtherVisibile();

                if (!oldIndex.Equals(value))
                    UpdateBestPrice();
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

                this.OnPropertyChanged();

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
                searchText = value;
                this.OnPropertyChanged();
                SendPriceItem.Security = searchText;

                RefreshUI();

                _sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());

                InitPriceView();
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

                ChangeUI();

                OnWholeFlag();

                if (value)
                    ResetQtyWarn();
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

                this.OnPropertyChanged();

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
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();

                if (!oldIndex.Equals(value))
                {
                    bool directCheck = directionCheck;

                    ChangeUI();

                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");

                    UpdateBestPrice();
                }
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
                _checkAll = value;

                DirectionEnable = true;

                if (value)
                {
                    myWindow.Height = _isXBond ? 488 : 488;// 548;

                    ChanelAllVisibility = Visibility.Visible;
                    CheckInner = _isXBond ? true : false;
                    CheckXBond = _isXBond ? true : false;
                    CheckXBondVisibility = Visibility.Collapsed;

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
                }
                this.OnPropertyChanged();
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

                //if (_isXBond)
                //{
                //    myWindow.Height = 438;
                //    AllowanceChannelVisibility = Visibility.Visible;
                //}
                //else
                //{
                //    if (!(CheckXBondVisibility == Visibility.Visible && isShowXBond))
                //    {
                //        myWindow.Height = 374;
                //        AllowanceChannelVisibility = Visibility.Collapsed;
                //    }
                //    else
                //    {
                //        myWindow.Height = 438;
                //        AllowanceChannelVisibility = Visibility.Visible;
                //    }
                //}

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
                    myWindow.Height = 458;
                    AllowanceChannelVisibility = Visibility.Visible;
                }
                else
                {
                    LogHelper.Info("CheckXBondVisibility:" + CheckXBondVisibility + ", isShowXBond:" + isShowXBond);
                    if (!(CheckXBondVisibility == Visibility.Visible && isShowXBond))
                    {
                        myWindow.Height = 394;
                        AllowanceChannelVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        myWindow.Height = 458;
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

                    myWindow.Height = 488;// 548;
                    DirectionEnable = true;

                }
            }
            //LogHelper.Info("Height:" + myWindow.Height);
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

        private Visibility _checkInnerVisibility = Visibility.Visible;
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

                this.OnPropertyChanged();

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

                if (InnerDefaultVisibility == Visibility.Collapsed && myWindow.Height == 488 && CheckXBondVisibility == Visibility.Visible)
                //if (myWindow.Height == 488 && CheckXBondVisibility == Visibility.Visible)
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
                    if (requst != null && result.Data != null && result.IsSuccess())
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

                ComboBoxSelectIndex = 0;

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

                spotSettleBoxIndex = 0;
                this.OnPropertyChanged("SpotSettleBoxIndex");
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

                if (CheckAll)
                    myWindow.Height = 488;
                //myWindow.Height = 548;

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

                        myWindow.Height = _isXBond ? 458 : 488;// 548;

                        if (!_wholeFlagChecked && !firmChecked)
                        {
                            _checkAll = true;
                            _checkInner = false;
                            _checkXBond = false;

                            this.OnPropertyChanged("CheckAll");
                            this.OnPropertyChanged("CheckInner");
                            this.OnPropertyChanged("CheckXBond");

                            _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                            this.OnPropertyChanged("CheckAllVisibility");
                        }
                    }
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);


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
                    foreach (ListBoxItemData itemData in DirectionItems)
                    {
                        if (itemData.Name.ToUpper().Equals(XBOND))
                            itemData.ItemEnable = false;
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
            myWindow.Height = 488;

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
        public ICommand OneCommand { get { return new RelayCommand(OnOneClick); } }
        public ICommand ThreeCommand { get { return new RelayCommand(OnThreeClick); } }
        public ICommand FiveCommand { get { return new RelayCommand(OnFiveClick); } }


        #endregion

        /// <summary>
        /// 下单量1000
        /// </summary>
        private void OnOneClick()
        {
            SendPriceItem.Amount = "1000";
        }

        /// <summary>
        /// 下单量3000
        /// </summary>
        private void OnThreeClick()
        {
            SendPriceItem.Amount = "3000";
        }

        /// <summary>
        /// 下单量5000
        /// </summary>
        private void OnFiveClick()
        {
            SendPriceItem.Amount = "5000";
        }

        /// <summary>
        /// 切换至X-Bond报价
        /// </summary>
        private void OnXBondClick()
        {
            DefaultCheck = false;
            DirectionCheck = true;

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
                    //myWindow.Height = 548;
                    myWindow.Height = 488;

                    CheckAll = true;
                    //_checkAll = false;
                    //this.OnPropertyChanged("CheckAll");

                    _checkAllVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckAllVisibility");

                    //OnCheckInner(false);
                }

                SettleBoxIndex = 0;
                SendPriceItem.FirmVisibility = Visibility.Collapsed;//firmChecked ? Visibility.Collapsed : Visibility.Visible;
                DarkDiskFlagVisibility = Visibility.Visible;
            }
            else
            {
                AddOneVisibility = Visibility.Collapsed;

                SendPriceItem.FirmVisibility = Visibility.Collapsed;
                DarkDiskFlagVisibility = Visibility.Collapsed;
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
            foreach (ListBoxItemData itemData in listBox.Items)
            {
                if (itemData != null)
                {
                    itemData.ItemEnable = isEnable;
                }
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
            bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            InitUI(priceMatchParam.Symbol, !isOfr);

            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                _wholeFlagChecked = true;

            }
            else
                _wholeFlagChecked = false;

            SendPriceItem.Amount = priceMatchParam.CustomFirmAmount;
            decimal d = 0;

            if (decimal.TryParse(priceMatchParam.Price, out d))
            {
                customPrice = d.ToString("N4").Replace(",", "");
                this.OnPropertyChanged("CustomPrice");
            }
            else
            {
                customPrice = priceMatchParam.Price;
                this.OnPropertyChanged("CustomPrice");
            }


            netPrice = priceMatchParam.NetPrice;
            this.OnPropertyChanged("NetPrice");


            //if (decimal.TryParse(priceMatchParam.Price, out d))
            //    CustomPrice = d.ToString("N4");
            //else
            //    CustomPrice = priceMatchParam.Price;

            //NetPrice = priceMatchParam.NetPrice;

            if (!priceMatchParam.IsFirm)
                FirmChecked = !priceMatchParam.IsFirm;

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

            SetPermission();
            _isSetPermission = true;


            if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            {
                WholeFlagChecked = true;

            }
            else
                WholeFlagChecked = false;

            ParseSettle(_priceMatchParam.SettlType);

            SetChannel(priceMatchParam);

            if (!string.IsNullOrEmpty(priceMatchParam.Terminal) && priceMatchParam.Terminal.EndsWith("xbond"))
            {
                this.OnXBondClick();

                if (!string.IsNullOrEmpty(_priceMatchParam.SettlType))
                {
                    if (_priceMatchParam.SettlType.Equals("1") || priceMatchParam.SettlType.Equals("T+0"))
                    {
                        spotSettleBoxIndex = 1;
                        this.OnPropertyChanged("SpotSettleBoxIndex");
                    }
                    else
                    {
                        spotSettleBoxIndex = 0;
                        this.OnPropertyChanged("SpotSettleBoxIndex");
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
            _needCalPrice = false;
            try
            {
                if (!string.IsNullOrEmpty(settle))
                {
                    if (!_isXBond)
                    {
                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1")
                            || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)"))
                        {
                            SettleBoxIndex = 0;

                            _addOneNotLimitCheck = false;
                            this.OnPropertyChanged("AddOneNotLimitCheck");

                            if (settle.Equals(ADD_ONE_NOT_LIMIT))
                            {
                                _addOneNotLimitCheck = true;
                                this.OnPropertyChanged("AddOneNotLimitCheck");
                            }
                            else if (settle.Equals("3"))
                            {
                                _addOneTomorrowCheck = true;
                                this.OnPropertyChanged("AddOneTomorrowCheck");
                            }
                            else if (settle.Equals("2"))
                            {
                                _addOneTodayCheck = true;
                                this.OnPropertyChanged("AddOneTodayCheck");
                            }
                        }
                        else
                        {
                            //_needCalPrice = false;
                            SettleBoxIndex = 1;
                            if (settle.Equals("1") || settle.Equals("T+0"))//T+0
                            {
                                try
                                {
                                    SelectDate = DateTime.Now;
                                    SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                                }
                                catch (Exception e) { }
                                //_needCalPrice = true;
                                ComboBoxSelectIndex = 0;
                                SendPriceItem.SettleType = "T+0";
                            }
                            else if (settle.Equals("明天+1") || settle.Equals("4"))
                            {
                                //_needCalPrice = true;
                                this.CalDate("4", false);
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
                    else
                    {
                        if (settle.Equals("+1"))
                            SpotSettleBoxIndex = 0;
                        else if (settle.Equals("+0"))
                            SpotSettleBoxIndex = 1;
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
        /// 初始化界面
        /// </summary>
        /// <param name="security"></param>
        /// <param name="bidOrOfr"></param>
        private void InitUI(string security, bool bidOrOfr)
        {
            isBid = bidOrOfr;
            _sendPriceAdapter = new SendPriceModelAdapter();

            SendPriceType = XBOND_TYPE;
            string goodsType = string.Empty;
            currentGoodsType = goodsType = CommonUtils.SetGoodsShortname(security);


            List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(security, 1);
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
            {
                _bondsInfoPo = bondsInfoPos[0];
            }


            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = security;

            SearchText = SendPriceItem.Security;
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
            CheckAll = true;
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

            if (!DirectionCheck)
            {
                SetItemParam(DefaultSecondItems, goodsType, "2");
            }

            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                sendPriceItem.Amount = SystemParamHandler.Instance.GetUserSetting().userQty;
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


            _sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());
            _sendPriceAdapter.Init(SendPriceItem);
        }

        /// <summary>
        /// 更新最优行情
        /// </summary>
        private void UpdateBestPrice()
        {
            if (!isInit && _sendPriceAdapter != null)
                _sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());
        }

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
                _checkInnerVisibility = Visibility.Visible;
                this.OnPropertyChanged("CheckInnerVisibility");

                if (!firmChecked)
                {
                    _checkAllVisibility = _isXBond ? Visibility.Collapsed : Visibility.Visible;
                    this.OnPropertyChanged("CheckAllVisibility");

                    _checkXBondVisibility = Visibility.Visible;
                    this.OnPropertyChanged("CheckXBondVisibility");

                    myWindow.Height = _isXBond ? 488 : 488;// 548;
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


                myWindow.Height = _isXBond ? 488 : 394;

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
            OnCheckInner(_checkInner);
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
            //sendPriceItem.SettleType = string.Empty;
            ComboBoxSelectIndex = 0;
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
        private string GetSetttle()
        {
            string settle = string.Empty;

            if (!_isXBond)
            {
                if (SettleBoxIndex == 0)
                    settle = AddOneNotLimitCheck ? ADD_ONE_NOT_LIMIT : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : ADD_ONE_NOT_LIMIT;
                else
                {
                    if (SelectDate != null && SelectDate.HasValue)
                    {
                        if (_sendPriceAdapter.IsSpotSettle(SelectDate, ComboBoxSelectIndex))
                            settle = _sendPriceAdapter.IsTomorrow(SelectDate, ComboBoxSelectIndex) ? "2" : "1";//2:明天+0转T+1
                        else
                            settle = SelectDate.Value.ToString("MM.dd") + (ComboBoxSelectIndex == 0 ? "+0" : "+1");
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
                settle = "1";//默认+0

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
                sendPricePo.ValidUntilTime = validUntiTimeSetting;
            }
            //sendPricePo.MarketIndicator = "4";

            if (_bondsInfoPo != null)
                sendPricePo.MarketIndicator = _bondsInfoPo.MarketIndicator;
            else
                sendPricePo.MarketIndicator = "4";


            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = sendPriceItem.Amount;
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
        private void OnConfirmClick()
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


            if (_lastCalResult != null && _lastCalResult.Data != null && !string.IsNullOrEmpty(_lastCalResult.ReqData) && _lastCalResult.IsSuccess() && !string.IsNullOrEmpty(_lastReqData))
            {
                if (!_lastCalResult.ReqData.Equals(_lastReqData))
                {
                    por.Reason = "调用计算器服务太频繁,请稍等一会再修改报价参数";
                }
            }

            decimal d = 0;
            if (decimal.TryParse(sendPriceItem.Amount, out d) && (d % 100) > 0)
                por.Reason = string.Format("量必须是100的整数倍，当前输入值：{0}", sendPriceItem.Amount);


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

            decimal d = 0;
            if (decimal.TryParse(sendPriceItem.Amount, out d) && (d % 100) > 0)
                result = false;

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
            if (SettleBoxIndex == 0 || _isXBond)
            {
                SendPriceItem.DateVisible = Visibility.Collapsed;
                SendPriceItem.SettleTypeVisible = Visibility.Collapsed;

                if (!_isXBond)
                {
                    _addOneVisibility = Visibility.Visible;
                    this.OnPropertyChanged("AddOneVisibility");
                }
                else
                {
                    _addOneVisibility = Visibility.Collapsed;
                    this.OnPropertyChanged("AddOneVisibility");
                }

                _addOneNotLimitCheck = true;
                this.OnPropertyChanged("AddOneNotLimitCheck");

                _addOneTodayCheck = _addOneTomorrowCheck = false;
                this.OnPropertyChanged("AddOneTodayCheck");

                this.OnPropertyChanged("AddOneTomorrowCheck");

                SelectDate = null;
                ComboBoxSelectIndex = -1;

                if (DirectionItems != null && DirectionCheck)
                {
                    foreach (ListBoxItemData item in DirectionItems)
                    {
                        if (item.Name.ToUpper().Equals(XBOND))
                            item.ItemEnable = IsShowXBondItem();
                        else
                            item.ItemEnable = true;
                    }
                }

                if (CheckInner && !_isSetPermission)
                {
                    bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID)
                        || AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);

                    if (isAllow)
                        isAllow = !FirmChecked && !_wholeFlagChecked && !CheckAll;

                    if (isAllow)
                    {
                        _checkXBondVisibility = Visibility.Visible;
                        this.OnPropertyChanged("CheckXBondVisibility");
                    }

                    _checkXBond = true;
                    this.OnPropertyChanged("CheckXBond");
                }

            }
            else if (SettleBoxIndex == 1)
            {
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;

                _addOneVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("AddOneVisibility");

                try
                {
                    SelectDate = DateTime.Now;
                    SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                }
                catch (Exception e) { }
                ComboBoxSelectIndex = 0;
                SendPriceItem.SettleType = "T+0";
            }
        }

        /// <summary>
        /// 切换至Bid标签
        /// </summary>
        private void OnBidClick()
        {
            isBid = true;
            isInit = true;
            FirmChecked = false;
            CheckNotOut = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
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

            SetPermission();

            OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            isInit = false;
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
            isInit = true;
            FirmChecked = false;
            CheckNotOut = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Blue8");
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

            SetPermission();

            OnWholeFlag();

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            isInit = false;
        }

        private void OnRefreshClick()
        {

        }


        /// <summary>
        /// 展开
        /// </summary>
        private void OnExpandClick()
        {
            if (myWindow != null)
            {
                Button btn_Expand = (Button)myWindow.FindName("btn_Expand");

                //Border Border_Left1 = (Border)myWindow.FindName("Border_Left1");
                //Border Border_Left2 = (Border)myWindow.FindName("Border_Left2");
                //Border Border_Left3 = (Border)myWindow.FindName("Border_Left3");
                //Border Border_Left4 = (Border)myWindow.FindName("Border_Left4");
                //Border Border_Left5 = (Border)myWindow.FindName("Border_Left5");
                //Border Border_Right1 = (Border)myWindow.FindName("Border_Right1");
                //Border Border_Right2 = (Border)myWindow.FindName("Border_Right2");

                if (btn_Expand.Tag.ToString() == "Collapsed")
                {
                    this.CardViewVisibility = Visibility.Visible;
                    //Border_Left1.BorderThickness = new Thickness(2, 2, 1, 0);
                    //Border_Left2.BorderThickness = new Thickness(2, 0, 1, 0);
                    //Border_Left3.BorderThickness = new Thickness(2, 0, 1, 0);
                    //Border_Left4.BorderThickness = new Thickness(2, 1, 1, 1);
                    //Border_Left5.BorderThickness = new Thickness(2, 0, 1, 2);
                    //Border_Right1.BorderThickness = new Thickness(1, 2, 2, 0);
                    //Border_Right2.BorderThickness = new Thickness(1, 0, 2, 2);

                    btn_Expand.Content = "<<";
                    myWindow.Width = 478+296;
                    btn_Expand.Tag = "Visible";
                    InitPriceView();
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.CardViewVisibility = Visibility.Collapsed;

                        //Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        //Border_Left2.BorderThickness = new Thickness(2, 0, 2, 0);
                        //Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        //Border_Left4.BorderThickness = new Thickness(2, 1, 2, 1);
                        //Border_Left5.BorderThickness = new Thickness(2, 0, 2, 2);
                        //Border_Right1.BorderThickness = new Thickness(1, 2, 2, 0);
                        //Border_Right2.BorderThickness = new Thickness(2, 0, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 478;
                        btn_Expand.Tag = "Collapsed";
                        PriceAndOrderView dgPrice = (PriceAndOrderView)myWindow.FindName("dgPrice");
                        //dgPrice.SetBrokerSelect("ALL");
                    }));
                }
            }
        }

        private void InitPriceView()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = SearchText;
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
    }
    /// <summary>
    /// 指定渠道配置项
    /// </summary>
    public class ListBoxItemData : ViewModelBase
    {
        private string _venuetypegw;
        public string Venuetypegw { get { return _venuetypegw; } set { _venuetypegw = value; } }
        private string _name;
        public string Name { get { return _name; } set { _name = value; this.OnPropertyChanged(); } }
        private string _code;
        public string Code { get { return _code; } set { _code = value; this.OnPropertyChanged(); } }
        private int _level;
        public int Level { get { return _level; } set { _level = value; this.OnPropertyChanged(); } }
        private int _seq;
        public int Seq { get { return _seq; } set { _seq = value; this.OnPropertyChanged(); } }
        private string _userVenue;
        public string UserVenue { get { return _userVenue; } set { _userVenue = value; this.OnPropertyChanged(); } }
        public SendPriceItem SendPriceItem { get; set; }
        private bool _checked;
        public bool Checked { get { return _checked; } set { _checked = value; this.OnPropertyChanged(); } }
        private Brush _bgColor;
        public Brush BgColor { get { return _bgColor; } set { _bgColor = value; this.OnPropertyChanged(); } }
        private bool _itemEnable;
        public bool ItemEnable { get { return _itemEnable; } set { _itemEnable = value; this.OnPropertyChanged(); } }

        private Visibility itemVisibility=Visibility.Visible;
        public Visibility ItemVisibility { get { return itemVisibility; } set { itemVisibility = value; this.OnPropertyChanged(); } }
    }

    /// <summary>
    /// 指定渠道项排序
    /// </summary>
    public class ListBoxItemSort : IComparer<ListBoxItemData>
    {
        public int Compare(ListBoxItemData x, ListBoxItemData y)
        {
            return x.Seq.CompareTo(y.Seq);
        }
    }
}
