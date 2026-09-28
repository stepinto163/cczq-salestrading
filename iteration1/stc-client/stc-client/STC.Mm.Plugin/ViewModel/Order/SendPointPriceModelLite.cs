using Ats.Foundation.Message;
using Ats.Foundation.Utils.Seqence;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;
using STC.Frame.UI;
using STC.Frame.Utils;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.DataProcesser;
using STC.Mm.Plugin.View;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using DataTimeSequence = STC.Frame.Utils.DataTimeSequence;

namespace STC.Mm.Plugin.ViewModel.Order
{
    public class SendPointPriceModelLite : BaseViewModel
    {
        public static string InSecurity = "";
        private readonly string MODULEID = "1002";
        private readonly string CHECK_ALL_ACTIONID = "100203";
        private readonly string CHECK_INNER_ACTIONID = "100201";
        private readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        private readonly string XBOND_TYPE = "";//">> X-bond报价";
        private readonly string NORMAL_TYPE = ">> 普通报价";

        private SendPriceModelAdapter _sendPriceAdapter = null;
        private bool _isXBond = false;

        private bool _isFirm = false;
        public Window myWindow;
        private bool isBid = true;
        private bool _isInit = true;
        private const string YYYYMMDD = "yyyyMMdd";

        private string _currentSource = string.Empty;
        private string _currentVenueTypeGW = string.Empty;
        private string _currentId = string.Empty;
        private string _trsactTime = string.Empty;
        private string _deliveryType = string.Empty;
        private string _clearingMethod = string.Empty;
        private string _quoteType = string.Empty;
        private BondsInfoPo _bondsInfoPo = null;

        private ManualResetEvent manualevent = new ManualResetEvent(false);

        public SendPointPriceModelLite()
        {
            
        }
        #region 界面属性
        /// <summary>
        /// 交易方向
        /// </summary>
        private string titlebidOrOfr = string.Empty;
        public string TitleBidOrOfr
        {
            get { return titlebidOrOfr; }
            set
            {
                titlebidOrOfr = value;
                this.OnPropertyChanged("TitleBidOrOfr");
            }
        }

        /// <summary>
        /// Title交易方向
        /// </summary>
        private string bidOrOfr = string.Empty;
        public string BidOrOfr
        {
            get { return bidOrOfr; }
            set
            {
                bidOrOfr = value;
                this.OnPropertyChanged("BidOrOfr");
            }
        }

        /// <summary>
        /// Title交易方向 小写
        /// </summary>
        private string bidOrOfrLower = string.Empty;
        public string BidOrOfrLower
        {
            get { return bidOrOfrLower; }
            set
            {
                bidOrOfrLower = value;
                this.OnPropertyChanged("BidOrOfrLower");
            }
        }

        /// <summary>
        /// 债券代码
        /// </summary>
        private string security = string.Empty;
        public string Security
        {
            get { return security; }
            set
            {
                security = value;
                this.OnPropertyChanged("Security");
            }
        }

        /// <summary>
        /// 债券简称
        /// </summary>
        private string chssecurity = string.Empty;
        public string ChsSecurity
        {
            get { return chssecurity; }
            set
            {
                chssecurity = value;
                this.OnPropertyChanged("ChsSecurity");
            }
        }

        public bool IsBid
        {
            get { return isBid; }
            set
            {
                isBid = value;
                this.OnPropertyChanged("BidOrOfr");
            }
        }

        /// <summary>
        /// 渠道名称
        /// </summary>
        private string venues = string.Empty;
        public string Venues
        {
            get
            {
                return venues;
            }
            set
            {
                venues = value;
                this.OnPropertyChanged("Venues");
            }
        }

        /// <summary>
        /// 渠道名称 代码
        /// </summary>
        private string sourceTrade = string.Empty;
        public string SourceTrade
        {
            get
            {
                return sourceTrade;
            }
            set
            {
                sourceTrade = value;
                this.OnPropertyChanged("SourceTrade");
            }
        }

        /// <summary>
        /// 订单Tip信息
        /// </summary>
        private string orderinfotip = string.Empty;
        public string OrderInfoTip
        {
            get { return orderinfotip; }
            set
            {
                orderinfotip = value;
                if (orderinfotip.Length >= 36)
                {
                    OrderInfoTipLimit = orderinfotip.Substring(0, 36) + "...";
                }
                else
                {
                    OrderInfoTipLimit = OrderInfoTip;
                }
                this.OnPropertyChanged(nameof(OrderInfoTip));
            }
        }

        /// <summary>
        /// 截断的订单Tip信息
        /// </summary>
        private string orderinfotipLimit = string.Empty;
        public string OrderInfoTipLimit
        {
            get { return orderinfotipLimit; }
            set
            {
                orderinfotipLimit = value;
                this.OnPropertyChanged(nameof(OrderInfoTipLimit));
            }
        }

        /// <summary>
        /// Settle
        /// </summary>
        private string settle2 = "2,3";
        public string Settle
        {
            get { return settle2; }
            set
            {
                settle2 = value;
                this.OnPropertyChanged(nameof(Settle));
            }
        }

        /// <summary>
        /// 输入价格
        /// </summary>
        private string clickPrice = string.Empty;
        public string ClickPrice
        {
            get { return clickPrice; }
            set
            {
                clickPrice = value;
                this.OnPropertyChanged(nameof(ClickPrice));
            }
        }

        /// <summary>
        /// 下单客户端ID
        /// </summary>
        private string clOrdID = string.Empty;
        public string ClOrdID
        {
            get { return clOrdID; }
            set
            {
                clOrdID = value;
                this.OnPropertyChanged(nameof(ClOrdID));
            }
        }


        /// <summary>
        /// broker最优价
        /// </summary>
        private string bestPrice = string.Empty;
        public string BestPrice
        {
            get { return bestPrice; }
            set
            {
                bestPrice = value;
                this.OnPropertyChanged(nameof(BestPrice));
            }
        }
        /// <summary>
        /// 意向价格是否有效
        /// </summary>
        private bool pricevalid = true;
        public bool PriceValid
        {
            get { return pricevalid; }
            set
            {
                pricevalid = value;
                this.ChsPriceValid = value ? "有效" : "无效";
                this.OnPropertyChanged(nameof(PriceValid));
                if (!pricevalid)
                {
                    SendPriceEnable = false;
                }
            }
        }

        private bool sendpriceenable = true;
        public bool SendPriceEnable
        {
            get { return sendpriceenable; }
            set
            {
                sendpriceenable = value;
                this.OnPropertyChanged(nameof(SendPriceEnable));
            }
        }
        

        private string chspricevalid = string.Empty;
        public string ChsPriceValid
        {
            get
            {
                return chspricevalid;
            }
            set
            {
                chspricevalid = value;
                this.OnPropertyChanged(nameof(ChsPriceValid));
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
                _addOneNotLimitCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
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
                _addOneTodayCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTomorrowCheck = false;
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
                _addOneTomorrowCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTodayCheck = false;
                }
            }
        }

        /// <summary>
        /// Amount
        /// </summary>
        private string _AmountText = "";
        public string AmountText
        {
            get { return _AmountText; }
            set
            {
                if (value != _AmountText)
                {
                    _AmountText = value;
                    this.OnPropertyChanged(nameof(AmountText));
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
                    this.OnPropertyChanged(nameof(SendPriceItem));
                    UpdateUiBestBidOrOfrForground();
                }
            }
        }

        private string _iocOutTime = string.Empty;
        public string IocOutTime
        {
            get
            {
                return _iocOutTime;
            }

            set
            {
                _iocOutTime = value;
                this.OnPropertyChanged(nameof(IocOutTime));
            }
        }

        private decimal _lockTimeOutHeight = decimal.Zero;

        public decimal LockTimeOutHeight
        {
            get { return _lockTimeOutHeight; }
            set
            {
                _lockTimeOutHeight = value;
                this.OnPropertyChanged();
            }
        }

        private bool _lockTimeOutCheck = false;
        public bool LockTimeOutCheck
        {
            get
            {
                return _lockTimeOutCheck;
            }

            set
            {
                _lockTimeOutCheck = value;
                this.OnPropertyChanged();
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

                if (firmChecked)
                {
                    if (isBid)
                    {
                        sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_bid_hover.png";
                        sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_bid_normal.png";
                    }
                    else
                    {
                        sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_ofr_hover.png";
                        sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_ofr_normal.png";
                    }
                }
                else
                {
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_no_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_no_normal.png";
                }
                this.OnPropertyChanged();
            }
        }

        private bool ofrButtonEnable = true;
        public bool OfrButtonEnable
        {
            get
            {
                return ofrButtonEnable;
            }

            set
            {
                ofrButtonEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool bidButtonEnable = true;
        public bool BidButtonEnable
        {
            get
            {
                return bidButtonEnable;
            }

            set
            {
                bidButtonEnable = value;
                this.OnPropertyChanged();
            }
        }

        #region listbox选中索引

        private int settleBoxIndex = 0;

        public int SettleBoxIndex
        {
            get
            {
                return settleBoxIndex;
            }

            set
            {
                settleBoxIndex = value;
                this.AddOneOpenedTag = "0";
                this.OnPropertyChanged(nameof(SettleBoxIndex));
                string newsettle = GetNewSetttle();
                _sendPriceAdapter.SetFilterParam(false, newsettle, Venues);
            }
        }

        /// <summary>
        /// SettleBox 字典
        /// </summary>
        private Dictionary<int, string> dictSettleBox = new Dictionary<int, string>();
        public Dictionary<int, string> DictSettleBox
        {
            get
            {
                return dictSettleBox;
            }

            set
            {
                dictSettleBox = value;
                this.OnPropertyChanged(nameof(DictSettleBox));
            }
        }


        private int setTimeOutIndex = 1;

        public int SetTimeOutIndex
        {
            get
            {
                return setTimeOutIndex;
            }

            set
            {
                setTimeOutIndex = value;
                if (setTimeOutIndex == 0)
                {
                    SetTimeOut20Click();
                }
                else if (setTimeOutIndex == 1)
                {
                    SetTimeOut30Click();
                }
                else if (setTimeOutIndex == 2)
                {
                    SetTimeOut60Click();
                }
                this.OnPropertyChanged(nameof(SetTimeOutIndex));
            }
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

                string newsettle = GetNewSetttle();
                _sendPriceAdapter.SetFilterParam(false, newsettle, Venues);
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
                customPrice = value;

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

                decimal d = 0;

                netPrice = decimal.TryParse(value, out d) ? d.ToString("F4") : string.Empty;

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

        private DateTime? selectDate;
        public DateTime? SelectDate
        {
            get { return selectDate; }
            set
            {
                string oldDate = selectDate == null ? string.Empty : selectDate.Value.Date.ToString(YYYYMMDD);
                selectDate = value;
                string newDate = selectDate == null ? string.Empty : selectDate.Value.Date.ToString(YYYYMMDD);

                this.OnPropertyChanged();
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

                this.OnPropertyChanged();
            }
        }

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

                if (value)
                {
                    CheckInner = false;
                }
                this.OnPropertyChanged();
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

                if (value)
                {
                    CheckAll = false;
                }

                this.OnPropertyChanged();
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
                spotSettleBoxIndex = value;

                this.OnPropertyChanged();

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

        private SolidColorBrush allBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00,0x9A,0xFF));
        public SolidColorBrush AllBestOfrForground
        {
            get { return allBestOfrForground; }
            set
            {
                allBestOfrForground = value;
                this.OnPropertyChanged("AllBestOfrForground");
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

        private int maxPriceAreaHeight = 234;

        public int MaxPriceAreaHeight
        {
            get { return maxPriceAreaHeight; }
            set
            {
                maxPriceAreaHeight = value;
                this.OnPropertyChanged(nameof(MaxPriceAreaHeight));
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

        public bool isFirmEnabled = true;
        public bool IsFirmEnable
        { 
            get { return isFirmEnabled; }
            set
            {
                isFirmEnabled = value;
                this.OnPropertyChanged(nameof(IsFirmEnable));
            }
        }

        /// <summary>
        /// 点击Hit方向颜色
        /// </summary>
        private Brush bidorofrtextblockforground = new SolidColorBrush(Colors.Blue);
        public Brush BidOrOfrTextBlockForground
        {
            get { return bidorofrtextblockforground; }
            set
            {
                bidorofrtextblockforground = value;
                this.OnPropertyChanged("BidOrOfrTextBlockForground");
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
        #endregion

        private bool isSendReq = false;

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
                                string year = result.Data.dealDate.Substring(0, 4);
                                string month = result.Data.dealDate.Substring(4, 2);
                                string day = result.Data.dealDate.Substring(6, 2);
                                SelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                            }
                            catch (Exception e) { }

                            ComboBoxSelectIndex = isTomorrow ? 0 : 1;
                            SendPriceItem.SettleType = isTomorrow ? "T+0" : "T+1";
                            //if (isTomorrow)
                            //{
                            //    SettleAdd0 = true;
                            //}
                            //else
                            //{
                            //    SettleAdd1 = true;
                            //}

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

        // Set TimeOut
        public ICommand SetTimeOut20Command { get { return new RelayCommand(SetTimeOut20Click); } }
        public ICommand SetTimeOut30Command { get { return new RelayCommand(SetTimeOut30Click); } }
        public ICommand SetTimeOut60Command { get { return new RelayCommand(SetTimeOut60Click); } }

        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new RelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new RelayCommand(ReducePriceClick); } }

        // + - Amout
        public ICommand AddAmountCommand { get { return new RelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new RelayCommand(ReduceAmountClick); } }
        #endregion

        private void OnOneClick()
        {
            this.AmountText = "2";
        }

        private void OnThreeClick()
        {
            this.AmountText = "3";
        }

        private void OnFiveClick()
        {
            this.AmountText = "5";
        }



        private void SetXbondUI(bool isXbond)
        {
            _isXBond = isXbond;

            SendPriceType = _isXBond ? NORMAL_TYPE : XBOND_TYPE;

            sendPriceItem.NetPriceVisible = _isXBond ? Visibility.Visible : Visibility.Collapsed;

            ChangeUI();
        }

        private void OnXBondClick()
        {
            _isXBond = !_isXBond;

            SendPriceType = _isXBond ? NORMAL_TYPE : XBOND_TYPE;

            sendPriceItem.NetPriceVisible = _isXBond ? Visibility.Visible : Visibility.Collapsed;

            ChangeUI();
        }

        private void ChangeUI()
        {
            if (_isXBond)
            {
                SpotSettleBoxVisibility = Visibility.Visible;
                SettleBoxVisibility = Visibility.Collapsed;
            }
            else
            {

                SpotSettleBoxVisibility = Visibility.Collapsed;
                SettleBoxVisibility = Visibility.Visible;
            }
        }


        public void SetAmountTxtSelectIndex(string intxt = "")
        {
            List<string> valueList = new List<string>() { "2", "3", "5" };
            if (string.IsNullOrEmpty(intxt))
            {
                intxt = this.AmountText;
            }

            if (!valueList.Contains(intxt))
            {
                SetAmountTxtIndex = -1;
            }
            else
            {
                int tem2 = valueList.FindLastIndex(x => x == intxt);
                SetAmountTxtIndex = tem2;
            }
        }

        private void OnAddClick()
        {
            if (string.IsNullOrEmpty(sendPriceItem.NonfirmTime))
                sendPriceItem.NonfirmTime = "1";
            else
            {
                decimal d = 0;
                if (decimal.TryParse(sendPriceItem.NonfirmTime, out d))
                    sendPriceItem.NonfirmTime = (d + 1).ToString();
            }
        }

        private void OnDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(sendPriceItem.NonfirmTime, out d) && (d - 1) >= 0)
                sendPriceItem.NonfirmTime = (d - 1).ToString();

        }

        #region UI Amout,Price  调整  SetAmountTxtIndex
        private void AddPriceClick()
        {
            double price = 0;
            if (double.TryParse(CustomPrice, out price))
            {
                price = price + 0.0025;

                if (price > 0)
                {
                    decimal d = 0;
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
            if (decimal.TryParse(this.AmountText, out amount))
            {
                amount += 1;
                if (amount >= 1000)
                {
                    this.AmountText = "1000";
                }
                else
                {
                    this.AmountText = amount.ToString();
                }
            }
            else
            {
                this.AmountText = "1";
            }
        }
        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(this.AmountText, out amount))
            {
                amount = amount - 1;
                if (amount > 0)
                {
                    this.AmountText = amount.ToString();
                }
                else
                {
                    this.AmountText = "1";
                }
            }
            else
            {
                this.AmountText = "5";
            }
        }
        #endregion

        private void SetTimeOut20Click()
        {
            IocOutTime = "20";
        }

        private void SetTimeOut30Click()
        {
            IocOutTime = "30";
        }

        private void SetTimeOut60Click()
        {
            IocOutTime = "60";
        }

        public void SetTimeOutSelectIndex()
        {
            List<string> valueList = new List<string>() { "20", "30", "60" };
            if (!valueList.Contains(IocOutTime))
            {
                SetTimeOutIndex = -1;
            }
            else
            {
                int tem2 = valueList.FindLastIndex(x => x == IocOutTime);
                SetTimeOutIndex = tem2;
            }
        }

        private void ParseSettle(string settle)
        {
            SettleBoxIndex2 = 0;
            AddOneShowText = "+1";
            try
            {
                //判断是多清算速度，如果是则选择 +1
                List<string> lstsettle = new List<string>();
                settle = settle.Replace(",", ":");
                string[] tmpsettle = settle.Split(':');

                if (!string.IsNullOrEmpty(settle))
                {
                    // 如果传入单个清算速度
                    if (tmpsettle.Length == 1)
                    {
                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                        {
                            SettleBoxIndex = -1;
                            AddOneNotLimitCheck = false;
                            AddOneOpenedTag = "1";
                            if (settle.Equals("2,3"))
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
                            for (int i = 0; i < DictSettleBox.Count; i++)
                            {
                                if (DictSettleBox[i] == settle)
                                {
                                    SettleBoxIndex = i;
                                    AddOneOpenedTag = "0";
                                    break;
                                }
                            }

                            if (settle.Split(',').Length >= 2)
                            {
                                SettleBoxIndex = -1;
                                AddOneOpenedTag = "1";
                                SettleBoxIndex2 = 0;
                                AddOneNotLimitCheck = true;
                            }
                        }
                    }
                    else
                    {
                        SettleBoxIndex = -1;
                        AddOneOpenedTag = "1";
                        SettleBoxIndex2 = 0;
                        AddOneNotLimitCheck = true;
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

        /// <summary>
        ///  初始化界面函数
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security">债券代码 例如 180210.IB</param>
        /// <param name="isBid">方向 1 = Bid 2 = Ofr</param>
        /// <param name="clickPrice">收益率（%）</param>
        /// <param name="clickAmount">数量（万）</param>
        /// <param name="settle"></param>
        /// <param name="netPrice"></param>
        /// <param name="source"></param>
        /// <param name="wholeflag"></param>
        /// <param name="isFrim"></param>
        /// <param name="orderinfotip">订单盈利信息</param>
        /// <param name="clOrdID">客户端ID</param>
        public void Init(Window myWindow, string security, bool isBid, string clickPrice, string clickAmount,string insettle, string netPrice, string source, 
            string wholeflag, bool isFrim, string id = null, string trsactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null, 
            string orderinfotip = "", string clOrdID = "", string venueTypeGW = null, string iocTimeOut = null) 
        {
            InSecurity = security;
            //payMoneyThread = new Thread(new ThreadStart(UpdateData));
            //payMoneyThread.Name = "更新价格";
            //payMoneyThread.Priority = ThreadPriority.AboveNormal;
            //payMoneyThread.Start();

            _sendPriceAdapter = new SendPriceModelAdapter();
            //venue = "CDH";
            this.IsBid = !isBid;
            this.BidOrOfr = this.IsBid ? "Ofr" : "Bid";
            this.BidOrOfrLower = this.BidOrOfr;
            this.TitleBidOrOfr = this.IsBid ? "TKN" : "GVN";
            this.myWindow = myWindow;
            myWindow.Unloaded += MyWindow_Unloaded;
            this.ClOrdID = clOrdID;
            // 传入参数
            this.Venues = source;
            this.SourceTrade = ComboSourceUtil.Instance.FindEnum(source, "venues");
            var x = CommonUtils.GetBondsInfoPoBySecurityID(security);
            if (x != null)
            {
                this.ChsSecurity = CommonUtils.GetBondsInfoPoBySecurityID(security).Short_Name;
            }

            this.ClickPrice = clickPrice;
            this.Settle = insettle;
            // 设置显示信息  3.2250% 2000*+4000(明+0)+1000(+0)+3000(远)
            this.OrderInfoTip = $"{clickPrice}% {orderinfotip}";
            this.Security = security;
            this.ChsPriceValid = "";

            //if (!string.IsNullOrEmpty(venue) && venue.ToUpper().Equals("CDH"))
            {
                LockTimeOutHeight = 47;

                if (SystemParamHandler.Instance.GetUserSetting() != null  && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
                {
                    IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
                    LockTimeOutCheck = true;

                    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
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
                else
                {
                    IocOutTime = "30";
                    LockTimeOutCheck = false;
                }

               
            }

            _isFirm = isFrim;
            if (isFrim)
            {
                LockTimeOutCheck = !isFrim;
                IsFirmEnable = !isFrim;
            }

            if (!string.IsNullOrEmpty(iocTimeOut))
            {
                IocOutTime = iocTimeOut;
                IsFirmEnable = LockTimeOutCheck = true;
            }

            //else
            //{
            //    this.myWindow.Height = 300;
            //    LockTimeOutHeight = 0;
            //}

            string goodsType = string.Empty;
            List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(security, 1);
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
            {
                goodsType = bondsInfoPos[0].SECURITY_TYPE;

                _bondsInfoPo = bondsInfoPos[0];
            }


            SendPriceItem = new SendPriceItem();
            SendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Gray15");
            SendPriceItem.Security = security;
            SearchText = SendPriceItem.Security;

            CheckAll = true;
            OfrButtonEnable = isBid;
            BidButtonEnable = !isBid;
            NetPrice = netPrice;

            if (this.IsBid)
            {
                OnBidClick();
            }
            else
            {
                OnOfrClick();
            }

            if (clickAmount != null)
            {
                this.AmountText = (Decimal.Parse(clickAmount) / 1000).ToString();
            }
            
            CustomPrice = ConvertDecimal(clickPrice).ToString("F4");

            // 默认最优双边配色
            AllBestBidForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Orange7");
            AllBestOfrForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Blue8");

            // x-bound 没有起作用
            if (!string.IsNullOrEmpty(source) && source.ToUpper().Equals("XBOND"))
                OnXBondClick();
            else
                SetXbondUI(false);

            _currentSource = source;
            _currentId = id;
            _trsactTime = trsactTime;
            _currentVenueTypeGW = venueTypeGW;
            _deliveryType = deliveryType;
            _clearingMethod = clearingMethod;
            _quoteType = quoteType;


            ParseSettle(insettle);

            // to do ...
            // 增加 传入的清算速度检测，如果没有报价则失效
            ////List<MarketDataSnapshotFullRefresh> allMarketDataSnapshotFull = PriceHandler.Instance.GetAllMarketData(this.Venues, security);
            ////MarketDataSnapshotFullRefresh alldetail = allMarketDataSnapshotFull.Find(s => s.Venues == this.Venues);
            ////string txt = JsonConvert.SerializeObject(alldetail);
            ////// 清算速度 检查
            ////bool isfoundsettle = IsHaveSettle(alldetail, insettle);
            ////if (!isfoundsettle)
            ////{
            ////    this.PriceValid = false;
            ////    this.OrderInfoTip = $"-- --";
            ////}
            ////else
            ////{
            ////this.PriceValid = true;
            ////    string cornermarket = this.CalCornerMarket(alldetail, insettle);
            ////    this.OrderInfoTip = $"{this.BestPrice}% {cornermarket}";
            ////}

            // 计算角标  CalCornerMarket

            if (wholeflag == "1")
            {
                sendPriceItem.AmountBoxIndex = 1;
            }
            else
            {
                sendPriceItem.AmountBoxIndex = 0;
            }

            //CalBond("4", GetSetttle(), NumChange(customPrice));

            List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");
            if (userVenues != null && userVenues.Count > 0)
                SendPriceItem.NonfirmTime = userVenues[0].firmtonontime;


            _sendPriceAdapter.SetFilterParam(false, insettle, Venues);
            _sendPriceAdapter.Init(SendPriceItem);
            // 订阅报价
            _sendPriceAdapter.OnMDPrice += _sendPriceAdapter_OnMDPrice;
            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
            this.PriceValid = true;

            // to do ...
            if (PointPriceConsistencyCheck)
            {
                this.timer = new DispatcherTimer();
                this.timer.Tick += Timer_Tick;
                this.timer.Interval = TimeSpan.FromSeconds(1);
                this.timer.Start();
            }
            // 服务器时间
            LastUpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }


        public void SetPriceFocus()
        {
            TextBox AmountTxt = (TextBox)myWindow.FindName("AmountTxt");
            // 选择最后位数
            //int pos = AmountTxt.Text.IndexOf('.');

            //if (pos > 0)
            //{
            int sellength = AmountTxt.Text.Length;
            AmountTxt.Focus();
            // AmountTxt.SelectAll();
            AmountTxt.SelectionStart = 0;
            AmountTxt.SelectionLength = sellength;
            //}
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
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;

        private DispatcherTimer timer = null;
        private bool isHvaeNewExecution = false;
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
                    //this.OrderInfoTip = $"{LastPrice}% {LastUpdateTime}";
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
                    if (Decimal.Parse(this.LastPrice) != Decimal.Parse(this.CustomPrice))
                    {
                        AlertSignToolTipText = $"与最新成交 {this.LastPrice} 不一致";
                        AlertSignVisible = Visibility.Visible;
                        this.timer.Stop();
                    }
                }
            }
        }

        private void _sendPriceAdapter_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                //this.PriceValid = false;
                UpdateUiBestBidOrOfrForground();
            }
        }

        /// <summary>
        /// 查找是否包含 Settle
        /// </summary>
        /// <param name="allMarket"></param>
        /// <param name="insettle"></param>
        /// <returns></returns>
        private bool IsHaveSettle(MarketDataSnapshotFullRefresh allMarket,List<string> insettle)
        {
            bool isfoundsettle = false;
            if (allMarket != null)
            {
                List<MarketDataSnapshotFullRefresh.GNoMDEntries> lstMarketDataSnapshot = allMarket.Clone().NoMDEntries;
                for (int j = 0; j < insettle.Count; j++)
                {
                    if (lstMarketDataSnapshot != null)
                    {
                        if (insettle[j] == "1")
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if (lstMarketDataSnapshot[i].SettlType == "1")
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (insettle[j] == "2,3")
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if ((lstMarketDataSnapshot[i].SettlType == "2") || (lstMarketDataSnapshot[i].SettlType == "3") || (lstMarketDataSnapshot[i].SettlType == "2,3") || (lstMarketDataSnapshot[i].SettlType == "3,2"))
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (insettle[j].Contains("."))
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if (insettle.Contains(lstMarketDataSnapshot[i].SettlType))
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if  (isfoundsettle)
                    {
                        break;
                    }
                }
            }

            return isfoundsettle;
        }


        private decimal ConvertDecimal(string instr)
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(instr))
            {
                return 0;
            }
            if (decimal.TryParse(instr, out d))
            {
                return d;
            }

            return 0;
        }
        // to do ...
        /////// <summary>
        /////// 做市报价（SOR）回调
        /////// </summary>
        /////// <param name="obj"></param>
        ////private void Instance_OnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        ////{
        ////    if (!string.IsNullOrEmpty(InSecurity) && InSecurity.Equals(obj.SecurityID))
        ////    {
        ////        manualevent.Set();
        ////    }
        ////}

        
        private void MyWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (payMoneyThread.IsAlive)
            //{
            //    payMoneyThread.Abort();
            //}

            // to do ...
            ////PriceHandler.Instance.UnregisterCTPrice(this.Security, Instance_OnMDPrice);
            // 订阅报价
            _sendPriceAdapter.OnMDPrice -= _sendPriceAdapter_OnMDPrice;

        }


        private void RefreshUI()
        {
            sendPriceItem.AmountBoxIndex = 0;
            sendPriceItem.BestBid = null;
            sendPriceItem.BestBidFloat = sendPriceItem.BestBidInt = string.Empty;
            sendPriceItem.BestOfr = null;
            sendPriceItem.BestOfrFloat = sendPriceItem.BestOfrInt = string.Empty;
            sendPriceItem.AmountBoxIndex = 0;
            sendPriceItem.Appoint = false;
            sendPriceItem.ChannelIndex = -1;
            sendPriceItem.CustomPrice = string.Empty;
            sendPriceItem.CustomSettleDate = string.Empty;
            NetPrice = string.Empty;
            sendPriceItem.NonfirmTime = string.Empty;
            SettleBoxIndex = 0;
            sendPriceItem.SettleType = string.Empty;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;

        }
        

        private void OnCancelClick()
        {
            if (myWindow != null)
                myWindow.Close();
        }

        private string GetSetttle()
        {
            string settle = "2,3";
            if (AddOneOpenedTag == "1")
                settle = AddOneNotLimitCheck ? "2,3" : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : "2,3";
            if (AddOneOpenedTag == "0")
            {
                if (SettleBoxIndex == 0)
                {
                    settle = "1";
                }
                else
                {
                    settle = DictSettleBox[SettleBoxIndex];
                }
            }

            return settle;
        }

        private string GetNewSetttle()
        {
            string settle = "2,3";
            if (AddOneOpenedTag == "1")
                settle = AddOneNotLimitCheck ? "2,3" : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : "2,3";
            if (AddOneOpenedTag == "0")
            {
                if (SettleBoxIndex == 0)
                {
                    settle = "1";
                }
                else
                {
                    try
                    {
                        settle = DictSettleBox[SettleBoxIndex];
                    }
                    catch 
                    { }
                }
            }

            return settle;
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

        private void SendPointPrice(object newOrder)
        {
            try
            {
                PResult result = OrdersHandler.Instance.PlaceOrder(newOrder as NewOrderSingle);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (!result.IsSuccess())
                        {
                            if (result.Code == "10000")
                            {
                                // to do ...
                                ////RiskViewUI warnView = new RiskViewUI(result, newOrder as NewOrderSingle);
                                ////warnView.Owner = Application.Current.MainWindow;
                                ////warnView.Show();
                                SendPriceWarnView warnView = new SendPriceWarnView(result, newOrder as NewOrderSingle);
                                warnView.Owner = Application.Current.MainWindow;
                                warnView.Show();
                            }
                            else
                            {
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.setWarnMsg(result.Msg);
                                msgBox.ShowDialog();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                    finally
                    {
                        OnCancelClick();
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        
        private string GetPermission()
        {
            string result = string.Empty;
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            if (isAllow)
            {
                result = "1";
            }
            else
            {
                isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                if (isAllow)
                {
                    result = "2";
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
                    result = "3";
                }
            }
            return result;
        }

        private bool CheckBondType()
        {
            return _bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.MarketIndicator) && _bondsInfoPo.MarketIndicator.Equals("4");
        }

        private bool CheckParam()
        {
            bool result = true;
            decimal temD = 0;
            if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
                result = false;
            else if (!decimal.TryParse(this.AmountText, out temD) || temD == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
                result = false;

            return result;
        }

        private BaseReplyPo GetErrorTxt()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10001";

            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            decimal temD = 0;

            if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
                por.Reason = "未填写价格或价格为0";
            else if (!decimal.TryParse(this.AmountText, out temD) || temD == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
                por.Reason = "未填写量或量为0";

            //decimal d = 0;
            //if (decimal.TryParse(this.AmountText, out d) && (d % 100) > 0)
            //    por.Reason = string.Format("量必须是100的整数倍，当前输入值：{0}", this.AmountText);

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle newOrder = new NewOrderSingle();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;
            //if (SystemParamHandler.Instance.GetUserSetting() != null)
            //{
            //    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
            //    if (!string.IsNullOrEmpty(Us.TradeType))
            //    {
            //        tradeType = Us.TradeType.ToLower();
            //    }
            //}

            if (tradeType == "1")
            {
                if (!string.IsNullOrEmpty(_currentSource) && _currentSource == "QDMESP")
                {
                    newOrder.AlgoName = "STP";
                    newOrder.VenueTypeGW = "QDMESP";
                    newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(_trsactTime))
                        newOrder.Info2 = _trsactTime;

                    if (!string.IsNullOrEmpty(_deliveryType))
                        newOrder.DeliveryType = _deliveryType;

                    if (!string.IsNullOrEmpty(_clearingMethod))
                        newOrder.ClearingMethod = _clearingMethod;

                    if (!string.IsNullOrEmpty(_quoteType))
                        newOrder.QuoteType = _quoteType;
                }
                else
                {
                    newOrder.AlgoName = "CROSS";
                    newOrder.VenueTypeGW = "INNER";
                    newOrder.Venues = "MANUAL";
                }
            }
            else if (tradeType == "2")
            {
                newOrder.AlgoName = "STP";
                if (!string.IsNullOrEmpty(this._currentVenueTypeGW) && this._currentVenueTypeGW == "CDH")
                    newOrder.VenueTypeGW = "SOR";
                else
                    newOrder.VenueTypeGW = this._currentVenueTypeGW;

                newOrder.Venues = this._currentSource;

                if (!string.IsNullOrEmpty(_currentSource) && _currentSource == "QDMESP")
                {
                    //newOrder.AlgoName = "STP";
                    //newOrder.VenueTypeGW = "QDMESP";
                    //newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(_trsactTime))
                        newOrder.Info2 = _trsactTime;

                    if (!string.IsNullOrEmpty(_deliveryType))
                        newOrder.DeliveryType = _deliveryType;

                    if (!string.IsNullOrEmpty(_clearingMethod))
                        newOrder.ClearingMethod = _clearingMethod;

                    if (!string.IsNullOrEmpty(_quoteType))
                        newOrder.QuoteType = _quoteType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;
                }
            }

            if (_bondsInfoPo != null)
                newOrder.MarketIndicator = _bondsInfoPo.MarketIndicator;
            else
                newOrder.MarketIndicator = "4";

            newOrder.UserName = LocalEnv.UserInfo.UserName;
            if (CheckBondType())
                newOrder.NetPrice = netPrice;
            else
                newOrder.NetPrice = "100";

            newOrder.CrossVenues = GetPermission();//_checkAll ? "1" : _checkInner ? "2" : string.Empty;
            newOrder.ClOrdID =string.IsNullOrEmpty(this.ClOrdID)?DataTimeSequence.Instance.GetId():this.ClOrdID;//"c" + DateTime.Now.Ticks;
            newOrder.SecurityID = sendPriceItem.Security;
            //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
            newOrder.Side = this.IsBid ? "1" : "2";
            newOrder.Price = CustomPrice;//"4.2";
            newOrder.OrdType = "2";
            newOrder.OrderQty = (decimal.Parse(this.AmountText) * 1000).ToString();
            newOrder.TimeInForce = "3";
            newOrder.SettlType = GetSetttle();
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //选中：0(nonfirm)，未选中：1(firm)
            //newOrder.FirmFlag = _isFirm ? "1" : "0";
            newOrder.FirmFlag = firmChecked ? "0" : "1";

            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();

            if (LockTimeOutHeight > 0 && LockTimeOutCheck && !string.IsNullOrEmpty(IocOutTime))
            {
                newOrder.IocTimeOut = IocOutTime;
            }

            string s = JsonConvert.SerializeObject(newOrder);
            return newOrder;
        }

        private void OnConfirmClick()
        {
            if (!CheckParam())
            {
                // to do ...
                ////RiskViewUI warnView = new RiskViewUI(GetErrorTxt(), GetNewOrderSingle());
                ////warnView.Owner = myWindow;
                ////warnView.Show();
                SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle());
                warnView.Owner = myWindow;
                warnView.Show();
            }
            else
            {
                NewOrderSingle newOrder = GetNewOrderSingle();

                ThreadPoolHelper.InvokeThreadPool(SendPointPrice, newOrder);
            }
        }

        private void CloseClick()
        {
            myWindow.Close();
        }

        private void AppointClick()
        {
            if (sendPriceItem.Appoint)
                sendPriceItem.ChannelIndex = 0;
            else
                sendPriceItem.ChannelIndex = -1;
        }

        private void SetOtherVisibile()
        {
            if (SettleBoxIndex == 0)
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

            }
            else if (SettleBoxIndex == 1)
            {
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;

                _addOneVisibility = Visibility.Collapsed;
                this.OnPropertyChanged("AddOneVisibility");
            }
        }


        private void OnBidClick()
        {
            IsBid = true;
            OfrOrBid = "0";
            sendPriceItem.RandomNum = string.Empty;
            FirmChecked = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.ShowOfrEffect = (Effect)myWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
            BidOrOfrTextBlockForground = (Brush)myWindow.FindResource("ColorBrush.Blue8");
        }

        private void OnOfrClick()
        {
            IsBid = false;
            OfrOrBid = "1";
            FirmChecked = false;
            sendPriceItem.RandomNum = string.Empty;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.ShowBidEffect = (Effect)myWindow.FindResource("BidEffect");
            sendPriceItem.ShowOfrEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
            BidOrOfrTextBlockForground = (Brush)myWindow.FindResource("ColorBrush.Orange5");
        }

        private void OnRefreshClick()
        {

        }

        private void UpdateUiBestBidOrOfrForground()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string bestBrokerBid = this.SendPriceItem.BestBrokerBid == null ? "--" : this.SendPriceItem.BestBrokerBid.ToString();
                string bestBrokerOfr = this.SendPriceItem.BestBrokerOfr == null ? "--" : this.SendPriceItem.BestBrokerOfr.ToString();
                string bestBid = this.SendPriceItem.BestBid == null ? "--" : this.SendPriceItem.BestBid.ToString();
                string bestOfr = this.SendPriceItem.BestOfr == null ? "--" : this.SendPriceItem.BestOfr.ToString();
                // 比较最优价，修改前景色
                if (!string.IsNullOrEmpty(this.CustomPrice))
                {
                    // 判断 Ofr
                    if (IsBid)
                    {
                        if (bestOfr != CustomPrice)
                        {
                            AllBestOfrForground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            AllBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
                        }
                    }
                    else
                    {
                        if (bestBid != CustomPrice)
                        {
                            AllBestBidForground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            AllBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C));
                        }
                    }
                }
                else
                {
                    AllBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C)); 
                    AllBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
                }

                // to do
                // 根据报价判断是否有该价格，已修改点价的有效性
            }));
        }
    }
}
