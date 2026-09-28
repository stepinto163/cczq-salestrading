using Ats.Foundation.Message;
using com.gateway.log;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using STC.Common.Model.Fix.Message;
using STC.Common.Model;
using STC.Common.Model.Cal;
using STC.Common.Handler.Cal;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.ViewModel.Data;
using STC.Common.Handler.Admin;
using BDB.Trade.Plugin.View.Order;
using STC.Common.View.UI;
using STC.Common.View.Component;
using BDB.Trade.Plugin.View.Trade;
using STC.Common.Handler.SystemParam;
using BDB.Trade.Plugin.Handler;
using STC.Common.Data;
using STC.Common.ViewModel.Data;
using STC.Common.Utils;
using OrdersHandler = BDB.Trade.Plugin.Handler.OrdersHandler;
using ModifyOrdersReply = BDB.Trade.Plugin.Handler.ModifyOrdersReply;
using STC.Frame.UI;

namespace BDB.Trade.Plugin.ViewModel.Order
{
    public class SendPriceViewModelLite : BaseViewModel
    {
        #region structure
        private readonly string MODULEID = "1002";
        private readonly string CHECK_ALL_ACTIONID = "100203";

        private readonly string XBOND_TYPE = ">> X-bond报价";
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
        private ManualResetEvent manualevent = new ManualResetEvent(false);
        private Thread payMoneyThread;

        private SendPriceModelAdapter _sendPriceAdapter = null;

        ~SendPriceViewModelLite()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();
        }
        #endregion

        #region 界面属性

        #region  清算速度 +1 +0 远

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
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged(nameof(SettleBoxIndex));

                SetOtherVisibile();

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




        private bool _addOneNotLimitCheck = true;
        /// <summary>
        /// T+1不限
        /// </summary>
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

                this.OnPropertyChanged(nameof(AddOneNotLimitCheck));

                if (value)
                {
                    AddOneTodayCheck = AddOneTomorrowCheck = false;

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
            }
        }

        private bool _addOneTodayCheck = false;
        /// <summary>
        /// 仅今+1
        /// </summary>
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

                this.OnPropertyChanged(nameof(AddOneTodayCheck));

                if (value)
                {
                    AddOneTomorrowCheck = AddOneNotLimitCheck = false;

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
            }
        }

        private bool _addOneTomorrowCheck = false;
        /// <summary>
        /// 仅明+0
        /// </summary>
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

                this.OnPropertyChanged(nameof(AddOneTomorrowCheck));

                if (value)
                {
                    AddOneNotLimitCheck = AddOneTodayCheck = false;

                    CalBond("4", GetSetttle(), NumChange(customPrice));

                    if (!oldVal.Equals(value))
                        UpdateBestPrice();
                }
            }
        }
        #endregion

        #region  报价有效时长属性
        private int validSelectIndex = 0;

        public int ValidSelectIndex
        {
            get
            {
                return validSelectIndex;
            }

            set
            {
                validSelectIndex = value;
                if (validSelectIndex == 0)
                {
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 1)
                {
                    HourValidTimeQuotationCheck = true;
                }
                else if (validSelectIndex == 2)
                {
                    TimeValidTimeQuotationCheck = true;
                }

                this.OnPropertyChanged(nameof(ValidSelectIndex));
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
                if (!(HourValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                {
                    return;
                }

                defaultValidTimeQuotationCheck = value;
                if (defaultValidTimeQuotationCheck)
                {
                    HourValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
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
                if (!(DefaultValidTimeQuotationCheck ^ TimeValidTimeQuotationCheck))
                {
                    return;
                }

                hourValidTimeQuotationCheck = value;
                if (hourValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = TimeValidTimeQuotationCheck = false;
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
                if (!(DefaultValidTimeQuotationCheck ^ HourValidTimeQuotationCheck))
                {
                    return;
                }
                timeValidTimeQuotationCheck = value;
                if (timeValidTimeQuotationCheck)
                {
                    DefaultValidTimeQuotationCheck = HourValidTimeQuotationCheck = false;
                    ValidMinute = ValidSecond = "";
                    ValidTimeHour = ValidTimeMinute = ValidTimeSecond = "";
                }
                this.OnPropertyChanged(nameof(TimeValidTimeQuotationCheck));
            }
        }
        #endregion


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
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_bid_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_bid_normal.png";
                }
                else
                {
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_ofr_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_ofr_normal.png";
                }

            }
            else
            {
                sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_no_hover.png";
                sendPriceItem.FirmCheckIS = "pack://application:,,,/BDB.XBond.Plugin;component/Images/stc_button_nonfirm_no_normal.png";

                //FirmTimeLimit = firmChecked;

            }

            FirmTxt = firmChecked ? NONFIRM : FIRM;
            if (_isXBond)
                SendPriceItem.FirmVisibility = Visibility.Collapsed;
            else
                SendPriceItem.FirmVisibility = firmChecked ? Visibility.Collapsed : Visibility.Visible;
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

        //728033
        //全市场 最优Bid
        private string allbestbid;
        public string AllBestBid
        {
            get { return allbestbid; }
            set
            {
                allbestbid = value;
                this.OnPropertyChanged("AllBestBid");
            }
        }

        private SolidColorBrush allBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C));
        public SolidColorBrush AllBestBidForground
        {
            get { return allBestBidForground; }
            set
            {
                allBestBidForground = value;
                this.OnPropertyChanged("AllBestBidForground");
            }
        }

        //全市场 最优Ofr
        private string allbestofr;
        public string AllBestOfr
        {
            get { return allbestofr; }
            set
            {
                allbestofr = value;
                this.OnPropertyChanged("AllBestOfr");
            }
        }

        private SolidColorBrush allBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
        public SolidColorBrush AllBestOfrForground
        {
            get { return allBestOfrForground; }
            set
            {
                allBestOfrForground = value;
                this.OnPropertyChanged("AllBestOfrForground");
            }
        }
        #region listbox选中索引

        private void ResetListBoxSelected()
        {
            // ListBox listBox = myWindow.FindName("lbDirect") as ListBox;
        }

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

                        SetListBoxItemEnable(listBox, true);


                    }
                    else
                    {
                        RandomEnable = false;
                        sendPriceItem.DefaultItemEnable = false;
                    }
                }
                this.OnPropertyChanged();

                ResetQtyWarn();
            }
        }

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
            //if (DefaultCheck)
            //{
            //    if (!string.IsNullOrEmpty(useRandom))
            //    {
            //        if (decimal.TryParse(useRandom, out dRandom) && decimal.TryParse(useQty, out dAmount))
            //            useMarketQty = sendPriceItem.MarketQty = ((DefaultFirstItems.Count + dRandom) * dAmount).ToString();
            //    }
            //    else
            //    {
            //        if (decimal.TryParse(SendPriceItem.RandomNum, out dRandom) && decimal.TryParse(useQty, out dAmount))
            //            useMarketQty = sendPriceItem.MarketQty = ((DefaultFirstItems.Count + dRandom) * dAmount).ToString();
            //        else if (decimal.TryParse(useQty, out dAmount))
            //            useMarketQty = sendPriceItem.MarketQty = (DefaultFirstItems.Count * dAmount).ToString();
            //    }
            //}
            //else if (DirectionCheck)
            if (DirectionCheck)
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


                        //ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                        //SetListBoxItemEnable(listBox, true);
                    }
                }

                this.OnPropertyChanged();

                ResetQtyWarn();
            }
        }

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

                Button btn_Expand = (Button)myWindow.FindName("btn_Expand");
                if (btn_Expand.Tag.ToString() == "Visible")
                {
                    this.InitPriceView();
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

                this.OnPropertyChanged(nameof(CustomPrice));
            }
        }

        private string customAmout = string.Empty;
        public string CustomAmout
        {
            get
            {
                return customAmout;
            }

            set
            {
                customAmout = value;
                this.OnPropertyChanged(nameof(CustomAmout));
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


                    directionCheck = directCheck;
                    this.OnPropertyChanged("DirectionCheck");

                    UpdateBestPrice();
                }
            }
        }

        
       

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


                    //ListBox listBox = myWindow.FindName("lbDirect") as ListBox;

                    //SetListBoxItemEnable(listBox, true);

                }

                this.OnPropertyChanged();

                ResetQtyWarn();
            }
        }

        private Dictionary<string, IList> filterDic = new Dictionary<string, IList>();
        public Dictionary<string, IList> FilterDic
        {
            get { return filterDic; }
            set
            {
                filterDic = value;
                this.OnPropertyChanged();
                ResetQtyWarn();
            }
        }

        /// <summary>
        ///  默认选择Ｂｒｏｋｅｒ
        /// </summary>
        private string defaultSelectItem = string.Empty;
        public string DefaultSelectItem
        {
            get { return defaultSelectItem; }
            set
            {
                if (value != null)
                {
                    defaultSelectItem = value;
                    this.OnPropertyChanged(nameof(DefaultSelectItem));
                }
            }
        }
        #endregion

        #region 报价窗口
        private bool isSendReq = false;
        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private object _calLock = new object();
        private CalResult _lastCalResult = null;
        private void CalBond(string calType, string settleType, string val)
        {
            calBondErrMsg = string.Empty;
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) || !_needCalPrice)
                return;

            //isSendReq = true;

            /**
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
            });\

            **/
        }

        private bool _isFromXBond = false;
       
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
        // Ok or Cancel
        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnConfirmClick); } }


        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new ComRelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new ComRelayCommand(ReducePriceClick); } }
        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }

        public ICommand ExpandCommand { get { return new ComRelayCommand(OnExpandClick); } }

        #endregion


        #region UI Amout,Price  调整
        private void AddPriceClick()
        {
            double price = 0;
            if (double.TryParse(CustomPrice, out price))
            {
                price = price + 0.0025;
                if (price > 0)
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
                if (price > 0)
                {
                    CustomPrice = price.ToString("F4");
                }
            }
        }
        private void AddAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(CustomAmout, out amount))
            {
                amount += 1000;
                if (amount >= 1000000)
                {
                    CustomAmout = "1000000";
                }
                else
                {
                    CustomAmout = amount.ToString();
                }
            }
            else
            {
                CustomAmout = "1000";
            }


            SetAmountTxtSelectIndex();
        }
        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(CustomAmout, out amount))
            {
                amount -= 1000;
                if (amount > 0)
                {
                    CustomAmout = amount.ToString();
                }
                else
                {
                    CustomAmout = "1000";
                }
            }
            else
            {
                CustomAmout = "5000";
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
                    CustomAmout = "2000";
                }
                else if (setAmountTxtIndex == 1)
                {
                    CustomAmout = "3000";
                }
                else if (setAmountTxtIndex == 2)
                {
                    CustomAmout = "5000";
                }

                this.OnPropertyChanged(nameof(SetAmountTxtIndex));

            }
        }

        public void SetAmountTxtSelectIndex()
        {
            List<string> valueList = new List<string>() { "2000", "3000", "5000" };
            if (!valueList.Contains(customAmout))
            {
                SetAmountTxtIndex = -1;
            }
            else
            {
                int tem2 = valueList.FindLastIndex(x => x == customAmout);
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
        #endregion


        private void SetChannel(PriceMatchParam priceMatchParam)
        {
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

            if (!string.IsNullOrEmpty(priceMatchParam.Venues))
            {
                //ListBox listBox = myWindow.FindName("lbDirect") as ListBox;
                string[] arr = priceMatchParam.Venues.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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

                        string tem = string.Format("{0}",itemData.Code);

                        foreach (string str in arr)
                        {
                            if (str.ToUpper().Equals(tem.ToUpper()))
                            {
                                itemData.Checked = true;
                                isContain = true;
                            }
                            else
                            {
                                itemData.ItemEnable = false;
                            }
                        }

                        if (!isContain)
                        {
                            itemData.Checked = false;
                        }

                        if (itemData.Name.ToUpper().Equals(XBOND))
                        {
                            itemData.ItemEnable = false;
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

        private void SetListBoxItemEnable(ListBox listBox, bool isEnable)
        {
            foreach (ListBoxItemData itemData in listBox.Items)
            {
                if (itemData != null)
                {
                    itemData.ItemEnable = true;
                }
            }

        }

        private bool _isSetPermission = false;
        public void Init(Window myWindow, PriceMatchParam priceMatchParam)
        {
            this.myWindow = myWindow;
            _isUpdatePrice = priceMatchParam.UpdataPrice;
            _priceMatchParam = priceMatchParam;
            bool isOfr = false;
            if (_priceMatchParam.SettlType == null)
            {
                isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            }
            else
            {
                isOfr = priceMatchParam.Side.ToUpper().Equals("SELL") || priceMatchParam.Side.Equals("2");
            }

            InitUI(priceMatchParam.Symbol, isOfr);

            //加入权限判断，如果没有broker
            string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 0)
            {
                this.BrokerWarning = "渠道：暂无权限，请联系经纪商开通";
            }

            SendPriceItem.Amount = priceMatchParam.CustomFirmAmount;
            CustomAmout = priceMatchParam.CustomFirmAmount;

            NetPrice = priceMatchParam.NetPrice;


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


            ParseSettle(_priceMatchParam.SettlType);
            if (priceMatchParam.Venues != null)
            {
                DefaultSelectItem = priceMatchParam.Venues;
            }

            // 价格处理
            if (!string.IsNullOrEmpty(priceMatchParam.Price))
            {
                decimal d = 0;
                if (decimal.TryParse(priceMatchParam.Price, out d))
                {
                    this.CustomPrice = d.ToString("N4").Replace(",", "");
                }

                this.GetBestPrice(priceMatchParam.Symbol, false);
            }
            else
            {
                this.GetBestPrice(priceMatchParam.Symbol);
            }
            

            //if (!string.IsNullOrEmpty(priceMatchParam.CustomFirmAmount))
            //{
            //    this.CustomPrice = priceMatchParam.Price;
            //    this.GetBestPrice(priceMatchParam.Symbol, false);
            //}


            // SetChannel(priceMatchParam);
            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            // string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : "0";
            if (!string.IsNullOrEmpty(priceMatchParam.Info1))
            {
                if (priceMatchParam.Info1.StartsWith("0|"))
                {
                    validSelectIndex = 0;
                    DefaultValidTimeQuotationCheck = true;
                }
                else if (priceMatchParam.Info1.StartsWith("1|"))
                {
                    validSelectIndex = 1;
                    HourValidTimeQuotationCheck = true;
                    // 时长
                    if (HourValidTimeQuotationCheck)
                    {
                        // ValidMinute,ValidSecond
                        string s = priceMatchParam.Info1.Split('|')[1];
                        if (s.Contains(":"))
                        {
                            ValidMinute = s.Split(':')[0];
                            ValidSecond = s.Split(':')[1];
                        }
                    }
                }
                else if (priceMatchParam.Info1.StartsWith("2|"))
                {
                    validSelectIndex = 2;
                    TimeValidTimeQuotationCheck = true;
                    // 时点
                    if (TimeValidTimeQuotationCheck)
                    {
                        // ValidTimeHour ValidTimeMinute ValidTimeSecond
                        string s = priceMatchParam.Info1.Split('|')[1];
                        if (s.Contains(":"))
                        {
                            ValidTimeHour = s.Split(':')[0];
                            ValidTimeMinute = s.Split(':')[1];
                            ValidTimeSecond = s.Split(':')[2];
                        }
                    }
                }
            }



            isInit = false;
            _isSetPermission = false;

            // 订阅注册
            //PriceHandler.Instance.RegisterCTPrice(SearchText, Instance_OnMDPrice);
        }
        
        private void GetBestPrice(string symbol, bool isChangeCustomPrice = true)
        {
            decimal d = 0;
            bool isOfr = !this.isBid;
            // 计算最优价
            BilateralBestPrice BestPrice = PriceHandler.Instance.CalBestPrice(symbol, "", "");
            if (isChangeCustomPrice)
            {
                this.CustomPrice = string.Empty;
                if (isOfr)
                {
                    if (BestPrice.OfrBest != null)
                    {
                        if (decimal.TryParse(BestPrice.OfrBest.bestPx, out d))
                        {
                            this.CustomPrice = d.ToString("N4").Replace(",", "");
                        }
                    }
                }
                else
                {
                    if (BestPrice.BidBest != null)
                    {
                        if (decimal.TryParse(BestPrice.BidBest.bestPx, out d))
                        {
                            this.CustomPrice = d.ToString("N4").Replace(",", "");
                        }
                    }
                }
            }

            // 计算最优价
            if (BestPrice.BidBest != null)
            {
                this.AllBestBid = BestPrice.BidBest.bestPx;
            }

            if (BestPrice.OfrBest != null)
            {
                this.AllBestOfr = BestPrice.OfrBest.bestPx;
            }
        }

        private bool isSendCalDate = false;
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
                            ComboBoxSelectIndex = 0;
                            SendPriceItem.SettleType = "T+0";
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
            catch (Exception e)
            {

            }
            finally
            {
                _needCalPrice = true;
            }
        }

        private string SetGoodsShortname(string goodcode)
        {
            string goodsType = string.Empty;

            if (!string.IsNullOrEmpty(goodcode))
            {
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(goodcode);
                if (bondsInfoPo != null)
                {
                    goodsType = bondsInfoPo.SECURITY_TYPE;

                    if (!string.IsNullOrEmpty(goodsType) && (goodsType.ToUpper().Equals("ABS") || goodsType.ToUpper().Equals("NCD")))
                        goodsType = "BCO";
                }
            }
            else
                goodsType = "BNC";//默认利率

            return goodsType;
        }

        private void InitUI(string security, bool isOfr)
        {
            // 判断是否是买入
            isBid = !isOfr;
            this.IsBidSelect = isBid ? "1" : "0";
            payMoneyThread = new Thread(new ThreadStart(UpdateData));
            payMoneyThread.Name = "更新价格";
            payMoneyThread.Priority = ThreadPriority.AboveNormal;
            payMoneyThread.Start();

            _sendPriceAdapter = new SendPriceModelAdapter();

            SendPriceType = XBOND_TYPE;
            string goodsType = string.Empty;
            currentGoodsType = goodsType = SetGoodsShortname(security);

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

            // 计算最优价
            BilateralBestPrice BestPrice = PriceHandler.Instance.CalBestPrice(SearchText, "", "");
            if (BestPrice.BidBest != null)
            {
                this.AllBestBid = BestPrice.BidBest.bestPx;
            }
            else
            {
                this.AllBestBid = "";
            }

            if (BestPrice.OfrBest != null)
            {
                this.AllBestOfr = BestPrice.OfrBest.bestPx;
            }
            else
            {
                this.AllBestOfr = "";
            }

            // 默认最优双边配色
            AllBestBidForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Orange7");
            AllBestOfrForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Blue8");

            List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");

            SendPriceItem.NonfirmTime = SystemParamHandler.Instance.GetFirmtonontime();
            if (!string.IsNullOrEmpty(SendPriceItem.NonfirmTime))
                FirmTimeLimit = true;

            // 默认等待时长 
            // to do...  以后做成配置
            this.DefaultValidUntilTime = 30;

            DefaultFirstItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "1"));
            DefaultSecondItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, "2"));
            if (DefaultSecondItems != null)
                RandomMax = DefaultSecondItems.Count;

            // DirectionItems = _sendPriceAdapter.VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(goodsType, string.Empty));
            List<KeyValuePair<string, string>> venues_en = ComboSourceUtil.Instance.GetComboSource("venues_en");
            List<KeyValuePair<string, string>> venues_cn = ComboSourceUtil.Instance.GetComboSource("venues_cn_tocn");
            DirectionItems = new List<ListBoxItemData>();
            for (int i = 0; i < venues_en.Count; i++)
            {
                ListBoxItemData lbid = new ListBoxItemData();
                lbid.Code = venues_en[i].Value;
                lbid.Name = venues_cn[i].Value;
                lbid.Venuetypegw = "SOR";
                DirectionItems.Add(lbid);
            }

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

            // 订阅注册
            PriceHandler.Instance.RegisterCTPrice(SearchText, Instance_OnMDPrice);
        }

        private void UpdateBestPrice()
        {
            if (!isInit && _sendPriceAdapter != null)
                _sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());
        }

        /// <summary>
        /// 做市报价（SOR）回调
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (!string.IsNullOrEmpty(SearchText) && SearchText.Equals(obj.SecurityID))
            {
                manualevent.Set();
                LogHelper.Info($"SearchText:{SearchText}:{fastJSON.JSON.Instance.ToJSON(obj)}");
            }
        }

        public void UpdateData()
        {
            while (true)
            {
                manualevent.WaitOne();
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    UpdateUi();
                }));
                manualevent.Reset();
            }
        }
        public void UpdateUi()
        {
            try
            {
                // 计算最优价
                BilateralBestPrice BestPrice = PriceHandler.Instance.CalBestPrice(SearchText, "", "");
                if (BestPrice.BidBest != null)
                {
                    this.AllBestBid = BestPrice.BidBest.bestPx;
                }
                else
                {
                    this.AllBestBid = "";
                }

                if (BestPrice.OfrBest != null)
                {
                    this.AllBestOfr = BestPrice.OfrBest.bestPx;
                }
                else
                {
                    this.AllBestOfr = "";
                }
            }
            catch (Exception ex)
            {

            }

        }


        private void SetPermission()
        {
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            
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

                //if (operationType.Equals("3") && userItemDatas != null && userItemDatas.Count > 0)
                //    DirectionCheck = true;

                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = SendPriceItem.BorderBH;

                    if (userItemDatas != null)
                    {
                        foreach (UserVenue uv in userItemDatas)
                        {
                            if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                            {
                                item.Checked = true;
                                //DirectionCheck = true;

                                break;
                            }

                            if (!string.IsNullOrEmpty(uv.venuesselectf) && uv.venuesselectf.Equals("2"))
                            {
                                DefaultCheck = true;
                                SendPriceItem.RandomNum = uv.venuesrdmnum;
                            }
                        }
                    }
                }

                //if (DirectionCheck)
                //{
                    foreach (ListBoxItemData item in itemDatas)
                    {
                        item.ItemEnable = true;
                    }
                //}
            }
        }

        private void MyWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (payMoneyThread.IsAlive)
            {
                payMoneyThread.Abort();
            }
            //PriceHandler.Instance.UnregisterCTPrice(SearchText, Instance_OnMDPrice);
        }


        private void RefreshUI()
        {
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
            ComboBoxSelectIndex = 0;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;
            
            this.GetBestPrice(this.SendPriceItem.Security);

        }

        private void OnCancelClick()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();

            if (myWindow != null)
                myWindow.Close();
        }

        private string GetSetttle()
        {
            string settle = string.Empty;


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


            if (string.IsNullOrEmpty(settle))
                settle = "1";//默认+0

            return settle;
        }

        private void UpdatePrice()
        {
            NewOrderSingle newOrder = GetNewOrderSingle();
            newOrder.Info5 = "转firm订单";
            OrdersHandler.Instance.ModifyOrders(newOrder, _priceMatchParam.OrderId, UpdatePriceCallBack);
        }

        private void UpdatePriceCallBack(ModifyOrdersReply reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {

                if (reply.Code == "10000" || reply.IsSuccess())
                {
                    RiskViewUI warnView = new RiskViewUI(reply, GetNewOrderSingle());
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
                    MessageBoxGUI22 msgBox = new MessageBoxGUI22();
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

        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();
            sendPricePo.AlgoName = "STP";
            sendPricePo.VenueTypeGW = "Connect2";
            sendPricePo.Venues = "TPSC";
            
            sendPricePo.ClOrdID = string.IsNullOrEmpty(_priceMatchParam.ClOrdId) ? DataTimeSequence.Instance.GetId() : _priceMatchParam.ClOrdId;
            sendPricePo.Info1 = _priceMatchParam.Info1;
            sendPricePo.Info2 = _priceMatchParam.Info2;
            sendPricePo.Info3 = _priceMatchParam.Info3;
            sendPricePo.Info4 = _priceMatchParam.Info4;
            sendPricePo.Info5 = _priceMatchParam.Info5;

            sendPricePo.MarketIndicator = "4";
            sendPricePo.OrdType = "2";
            sendPricePo.OrderQty = CustomAmout;
            sendPricePo.Price = CustomPrice;

            sendPricePo.NetPrice = NetPrice;

            sendPricePo.SecurityID = sendPriceItem.Security;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            sendPricePo.SettlType = GetSetttle();

            sendPricePo.Side = isBid ? "1" : "2";

            sendPricePo.Terminal = string.Format(_isXBond ? "StcLitelient_v{0}" : "StcLitelient_v{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            //sendPricePo.UserName = LocalEnv.UserInfo.UserName;

            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = FirmChecked ? "0" : "1";

            sendPricePo.WholeFlag = "0";

            sendPricePo.HandlInst = "1";

            sendPricePo.QuantID = "";
            // Firm报价变成NonFirm时间 删除
            string s = _sendPriceAdapter.GetLiteSelectItem(this.FilterDic);
            if (string.IsNullOrEmpty(s))
            {
                sendPricePo.CrossVenues = "1";
                sendPricePo.VenuesSelectF = "1";
            }
            else
            {
                sendPricePo.CrossVenues = "1";
                sendPricePo.VenuesSelectF = "2";
                sendPricePo.Venues = _sendPriceAdapter.GetLiteSelectItem(this.FilterDic);
            }

            // 默认时间
            if (DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTime.Hour;
                int min = DateTimeEx.ServerTime.Minute;
                if (hour >= 12)
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                
                if (DateTimeEx.ServerTime > DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00"))
                {
                    sendPricePo.ValidUntilTime = string.Empty;
                }
            }

            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond == 0)
                {
                    sendPricePo.ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(DefaultValidUntilTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    sendPricePo.ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                int tmpint = 0;
                string hour = string.Empty;
                string min = string.Empty;
                string sec = string.Empty;

                if (!string.IsNullOrEmpty(ValidTimeHour))
                {
                    tmpint = int.Parse(ValidTimeHour);
                    hour = "0" + tmpint.ToString();
                    hour = hour.Substring(hour.Length - 2, 2);
                }

                if (!string.IsNullOrEmpty(ValidTimeMinute))
                {
                    tmpint = int.Parse(ValidTimeMinute);
                    min = "0" + tmpint.ToString();
                    min = min.Substring(min.Length - 2, 2);
                }

                if (!string.IsNullOrEmpty(ValidTimeSecond))
                {
                    tmpint = int.Parse(ValidTimeSecond);
                    sec = "0" + tmpint.ToString();
                    sec = sec.Substring(sec.Length - 2, 2);
                }

                DateTime tmpdt = DateTime.Now;
                if (DateTime.TryParse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + $" {hour}:{min}:{sec}", out tmpdt ))
                {
                    sendPricePo.ValidUntilTime = tmpdt.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // Modi:cyc Date:2021-04-13 Func:增加用户选择的 需求有效时长 选项记忆功能
            string validTimeType = DefaultValidTimeQuotationCheck ? "0" : HourValidTimeQuotationCheck ? "1" : TimeValidTimeQuotationCheck ? "2" : "0";
            // 默认
            if (DefaultValidTimeQuotationCheck)
            {
                sendPricePo.Info1 = $"{validTimeType}|";
            }
            
            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute,ValidSecond
                string min = string.IsNullOrEmpty(ValidMinute) ? "" : ValidMinute;
                string sec = string.IsNullOrEmpty(ValidSecond) ? "" : ValidSecond;
                sendPricePo.Info1 = $"{validTimeType}|{min}:{sec}";
            }
            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                string hour = string.IsNullOrEmpty(ValidTimeHour) ? "" : ValidTimeHour;
                string min = string.IsNullOrEmpty(ValidTimeMinute) ? "" : ValidTimeMinute;
                string sec = string.IsNullOrEmpty(ValidTimeSecond) ? "" : ValidTimeSecond;
                sendPricePo.Info1 = $"{validTimeType}|{hour}:{min}:{sec}";
            }

            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            if (_priceMatchParam != null)
                sendPricePo.Info3 = _priceMatchParam.Info3;

            return sendPricePo;
        }

        private void PlaceOrder()
        {
            NewOrderSingle sendPricePo = GetNewOrderSingle();
            SendPriceEnable = false;
            // 默认时间
            if (DefaultValidTimeQuotationCheck)
            {
                int hour = DateTimeEx.ServerTime.Hour;
                int min = DateTimeEx.ServerTime.Minute;
                if (hour >= 12)
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    sendPricePo.ValidUntilTime = (DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 12:00:00")).ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (DateTimeEx.ServerTime > DateTime.Parse(DateTimeEx.ServerTime.ToString("yyyy-MM-dd") + " 16:30:00"))
                {
                    sendPricePo.ValidUntilTime = string.Empty;
                }
            }

            // Modi:cyc Date:2021-04-14 Func:增加多渠道分别下单功能

            string[] subVenues = sendPricePo.Venues.Split(',');

            for (int j = 0; j< subVenues.Length;j++)
            {
                NewOrderSingle sendOrder = new NewOrderSingle();
                sendOrder = sendPricePo.Clone();
                sendOrder.Venues = subVenues[j];
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        PResult result = OrdersHandler.Instance.PlaceOrder(sendOrder);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                if (result != null)
                                {
                                    if (result.Code == "10000" || result.IsSuccess())
                                    {
                                        RiskViewUI warnView = new RiskViewUI(result, sendOrder);
                                        warnView.OnAction += WarnView_OnAction;
                                        myWindow.Visibility = Visibility.Hidden;
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
                                        //MessageBoxGUI msgBox = new MessageBoxGUI();
                                        //msgBox.setWarnMsg(result.Msg);
                                        //msgBox.Owner = Application.Current.MainWindow;
                                        //msgBox.ShowDialog();
                                        CustomMessageBox.Info(result.Msg, "发布需求");

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
        }

        private void WarnView_OnAction(object sender, bool isCancel, bool isDoubleCheck)
        {
            if (sender is RiskViewUI)
            {
                RiskViewUI warnView = sender as RiskViewUI;
                warnView.OnAction -= WarnView_OnAction;
                if (isCancel)
                {
                    myWindow.Topmost = true;
                    myWindow.Visibility = Visibility.Visible;
                    myWindow.Topmost = false;
                }
            }

            if (!isCancel)
                OnCancelClick();
            else
                SendPriceEnable = !isDoubleCheck;
        }

        private void OnConfirmClick()
        {
            SendPriceEnable = false;
            myWindow.Topmost = false;
            if (CheckParam())
            {
                //if (_isUpdatePrice)
                //    UpdatePrice();
                //else
                    PlaceOrder();
            }
            else
            {
                SendPriceEnable = true;
                //SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle());
                //warnView.Owner = myWindow;
                //warnView.Show();
                RiskViewUI warnView = new RiskViewUI(GetErrorTxt(), GetNewOrderSingle());
                warnView.Owner = myWindow;
                warnView.Show();
            }
        }

        private BaseReplyPo GetErrorTxt()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10002";

            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10002";
            por.DoubleWarn = false;

            decimal temD = 0;

            List<string> LstReason = new List<string>();
            //if (CheckAllVisibility == Visibility.Collapsed && CheckInnerVisibility == Visibility.Collapsed && CheckXBondVisibility == Visibility.Collapsed)
            //    por.Reason = "没有撮合渠道权限";
            if (string.IsNullOrEmpty(sendPriceItem.Security))
                LstReason.Add("未选择债券");
            else if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
                LstReason.Add("未填写价格或价格为0");
            else if (!decimal.TryParse(CustomAmout, out temD) || temD == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
                LstReason.Add("未填写量或量为0");


            decimal d = 0;
            if (decimal.TryParse(CustomAmout, out d) && (d % 1000) > 0)
                LstReason.Add(string.Format("量必须是1000的整数倍，当前输入值：{0}", sendPriceItem.Amount));

            // 检查是否是时点
            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(ValidMinute) && string.IsNullOrEmpty(ValidSecond))
                {
                    LstReason.Add("有效时长为时长时  分，秒不能为空！");
                }
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if ((string.IsNullOrEmpty(ValidTimeHour)) || (string.IsNullOrEmpty(ValidTimeMinute)) || (string.IsNullOrEmpty(ValidTimeSecond)))
                {
                    LstReason.Add("有效时长为时点时 时，分，秒不能为空！");
                }
            }

            string s = _sendPriceAdapter.GetLiteSelectItem(this.FilterDic);
            if (string.IsNullOrEmpty(s))
            {
                LstReason.Add("至少选择一个渠道");
            }
            for (int i = 0; i < LstReason.Count; i++)
            {
                if (string.IsNullOrEmpty(por.Reason))
                {
                    por.Reason = LstReason[i];
                }
                else
                {
                    por.Reason = por.Reason + "\r\n" + LstReason[i];
                }
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

        private bool CheckParam()
        {
            bool result = true;
            decimal temD = 0;
            if (string.IsNullOrEmpty(sendPriceItem.Security))
                result = false;
            else if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
                result = false;
            //else if (string.IsNullOrEmpty(NetPrice))
            //    result = false;
            else if (!decimal.TryParse(CustomAmout, out temD) || temD == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
                result = false;
            //else if ((DirectionCheck && CheckAll) || (_isXBond && DirectionCheck))
            //{
            //    string str = _sendPriceAdapter.GetSelectItem(myWindow.FindName("lbDirect") as ListBox);
            //    if (string.IsNullOrEmpty(str))
            //        result = false;
            //}
            //else if (DefaultCheck)
            //{
            //    decimal dd = 0;
            //    if (decimal.TryParse(SendPriceItem.RandomNum, out dd) && dd > defaultSecondItems.Count)
            //        result = false;
            //}

            decimal d = 0;
            if (decimal.TryParse(CustomAmout, out d) && (d % 100) > 0)
                result = false;

            string s = _sendPriceAdapter.GetLiteSelectItem(this.FilterDic);
            if (string.IsNullOrEmpty(s))
            {
                result = false;
            }

            // 检查是否是时点
            // 时长
            if (HourValidTimeQuotationCheck)
            {
                // ValidMinute  ValidSecond
                if (string.IsNullOrEmpty(ValidMinute) && string.IsNullOrEmpty(ValidSecond))
                {
                    result = false;
                }
            }

            // 时点
            if (TimeValidTimeQuotationCheck)
            {
                // ValidTimeHour ValidTimeMinute ValidTimeSecond
                if ((string.IsNullOrEmpty(ValidTimeHour)) || (string.IsNullOrEmpty(ValidTimeMinute))  || (string.IsNullOrEmpty(ValidTimeSecond)))
                {
                    result = false;
                }
            }

            return result;
        }

        private void CloseClick()
        {
            if (_sendPriceAdapter != null)
                _sendPriceAdapter.Clear();

            myWindow.Close();
        }


        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0 || SettleBoxIndex == 1)
            {
                SendPriceItem.DateVisible = Visibility.Collapsed;
                SendPriceItem.SettleTypeVisible = Visibility.Collapsed;

                SelectDate = null;
                ComboBoxSelectIndex = 0;
            }
            else if (SettleBoxIndex == 2)
            {
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;

               try
                {
                    SelectDate = DateTime.Now;
                    SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                }
                catch (Exception e) { }
                ComboBoxSelectIndex = 0;
                SendPriceItem.SettleType = "T+0";
            }

            if (!isInit)
            {
                //////新增同步
                ////PriceView dgPrice = (PriceView)myWindow.FindName("dgPrice");
                ////if (SettleBoxIndex == 0)
                ////{
                ////    dgPrice.SetSettleSelect("+1");
                ////}
                ////else if (SettleBoxIndex == 1)
                ////{
                ////    dgPrice.SetSettleSelect("+0");
                ////}
                ////else if (SettleBoxIndex == 2)
                ////{
                ////    dgPrice.SetSettleSelect("远");
                ////}
            }
        }
        

        private void OnBidClick()
        {
            this.IsBidSelect = "1";
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
            OfrOrBid = "0";
            this.GetBestPrice(sendPriceItem.Security);
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

            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            isInit = false;
        }

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

        private void OnOfrClick()
        {
            this.IsBidSelect = "0";
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
            OfrOrBid = "1";
            this.GetBestPrice(sendPriceItem.Security);
            //_isXBond = false;

            ChangeBoxItemBG(DirectionItems);
            SetItemParam(DirectionItems, currentGoodsType, "3");
            //SetItemParam(DirectionItems, currentGoodsType, "2");

            //if (DirectionCheck)
            //    DefaultCheck = false;

            SetPermission();
            if (_isXBond)
                SendPriceItem.NetPriceVisible = Visibility.Visible;

            isInit = false;
        }
        #endregion

        #region 新增的行情窗口处理

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

        /// <summary>
        /// 展开
        /// </summary>
        private void OnExpandClick()
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
                Border Border_Right2 = (Border)myWindow.FindName("Border_Right2");

                if (btn_Expand.Tag.ToString() == "Collapsed")
                {
                    this.CardViewVisibility = Visibility.Visible;
                    Border_Left1.BorderThickness = new Thickness(2, 2, 1, 0);
                    Border_Left2.BorderThickness = new Thickness(2, 0, 1, 0);
                    Border_Left3.BorderThickness = new Thickness(2, 0, 1, 0);
                    Border_Left4.BorderThickness = new Thickness(2, 1, 1, 1);
                    Border_Left5.BorderThickness = new Thickness(2, 0, 1, 2);
                    Border_Right1.BorderThickness = new Thickness(1, 2, 2, 0);
                    Border_Right2.BorderThickness = new Thickness(1, 0, 2, 2);

                    btn_Expand.Content = "<<";
                    myWindow.Width = 680;
                    btn_Expand.Tag = "Visible";
                    InitPriceView();
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.CardViewVisibility = Visibility.Collapsed;

                        Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        Border_Left2.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 1, 2, 1);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 2, 2);
                        Border_Right1.BorderThickness = new Thickness(1, 2, 2, 0);
                        Border_Right2.BorderThickness = new Thickness(2, 0, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 395;
                        btn_Expand.Tag = "Collapsed";
                        PriceView dgPrice = (PriceView)myWindow.FindName("dgPrice");
                        dgPrice.SetBrokerSelect("ALL");
                    }));
                }
            }
        }

        private void InitPriceView()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string security = SearchText;
                PriceView dgPrice = (PriceView)myWindow.FindName("dgPrice");
                MarketOrderView dgMarket = (MarketOrderView)myWindow.FindName("dgMarket");
                dgPrice.Init(ViewModel.Data.TabShowModel.Card, security, null);
                dgPrice.RemoveDbDoubleClickEvent();
                dgMarket.Init(ViewModel.Data.TabShowModel.Card, security);
            }));
        }
        #endregion
    }

    
}
