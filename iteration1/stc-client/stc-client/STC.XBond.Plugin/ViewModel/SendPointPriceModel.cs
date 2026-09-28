using Ats.Foundation.Message;
using com.gateway.log;
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
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.ViewModel.Adapter;
using STC.XBond.Plugin.ViewModel.Data;
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

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 点价界面SendPointPriceView的数据内容
    /// </summary>
    public class SendPointPriceModel : ViewModelBase
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

        private readonly string XBOND_TYPE = "";//">> X-bond报价";
        private readonly string NORMAL_TYPE = ">> 普通报价";

        private SendPriceModelAdapter _sendPriceAdapter = null;
        private bool _isXBond = false;

        private bool _isFirm = false;
        private Window myWindow;
        private bool isBid = true;
        private bool _isInit = true;
        private const string YYYYMMDD = "yyyyMMdd";

        private readonly string ADD_ONE_NOT_LIMIT = "2,3";

        private string _currentSource = string.Empty;
        private string _currentId = string.Empty;
        private string _trsactTime = string.Empty;
        private string _deliveryType = string.Empty;
        private string _clearingMethod = string.Empty;
        private string _quoteType = string.Empty;

        private BondsInfoPo _bondsInfoPo = null;


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
                _addOneNotLimitCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
                    this.OnPropertyChanged("AddOneTodayCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");
                    
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
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTomorrowCheck");
                    
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
                    this.OnPropertyChanged("AddOneNotLimitCheck");
                    this.OnPropertyChanged("AddOneTodayCheck");
                    
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

                this.OnPropertyChanged();
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
                this.OnPropertyChanged();

                SetOtherVisibile();
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

                netPrice = decimal.TryParse(value, out d)? d.ToString("F4"):string.Empty;
                
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

                            ComboBoxSelectIndex = isTomorrow?0:1;
                            SendPriceItem.SettleType = isTomorrow?"T+0":"T+1";

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

        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private CalResult _lastCalResult = null;
        private void CalBond(string calType, string settleType, string val, string securityid)
        {
            calBondErrMsg = string.Empty;
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d))
                return;

            //isSendReq = true;

            _lastReqData = string.Format("{0}-{1}-{2}", calType, settleType, val);
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = securityid;
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
                                _lastCalResult = result;

                                if (calType.Equals("4"))
                                {
                                    netPrice = result.Data.CleanPrice.ToString("F4");
                                    this.OnPropertyChanged("NetPrice");
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

        private void OnOneClick()
        {
            SendPriceItem.Amount = "1000";
        }

        private void OnThreeClick()
        {
            SendPriceItem.Amount = "3000";
        }

        private void OnFiveClick()
        {
            SendPriceItem.Amount = "5000";
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

        private void ParseSettle(string settle)
        {
            if (!string.IsNullOrEmpty(settle))
            {
                if (!_isXBond)
                {
                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1"))
                        SettleBoxIndex = 0;





                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("T+1(不限)")
                            || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0)") || settle.Equals("T+1(今+1)"))
                    {
                        SettleBoxIndex = 0;

                        _addOneNotLimitCheck = false;
                        this.OnPropertyChanged("AddOneNotLimitCheck");

                        if (settle.Equals(ADD_ONE_NOT_LIMIT) || settle.Equals("T+1(不限)"))
                        {
                            _addOneNotLimitCheck = true;
                            this.OnPropertyChanged("AddOneNotLimitCheck");
                        }
                        else if (settle.Equals("明天+0") || settle.Equals("3") || settle.Equals("T+1(明+0)"))
                        {
                            _addOneTomorrowCheck = true;
                            this.OnPropertyChanged("AddOneTomorrowCheck");

                            SettleBoxIndex = 1;

                            this.CalBond("3", true);
                        }
                        else if (settle.Equals("2") || settle.Equals("T+1(今+1)"))
                        {
                            _addOneTodayCheck = true;
                            this.OnPropertyChanged("AddOneTodayCheck");
                        }
                    }
                    else
                    {
                        SettleBoxIndex = 1;
                        if (settle.Equals("1") || settle.Equals("T+0"))//T+0
                        {
                            try
                            {
                                SelectDate = DateTime.Now;
                                SendPriceItem.CustomSettleDate = DateTime.Now.ToString("yyy-MM-dd");
                            }
                            catch (Exception e) { }
                            ComboBoxSelectIndex = 0;
                            SendPriceItem.SettleType = "T+0";
                        }
                        else if (settle.Equals("明天+1") || settle.Equals("4"))
                        {
                            this.CalBond("4", false);
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

        public void Init(Window myWindow, string security, bool isBid, string clickPrice,
            string clickAmount, string settle, string netPrice, string source, string wholeflag, bool isFrim, 
            string id = null, string trsactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null)
        {
            _sendPriceAdapter = new SendPriceModelAdapter();

            //venue = "CDH";
            this.isBid = !isBid;
            this.myWindow = myWindow;
            myWindow.Unloaded += MyWindow_Unloaded;
            
            //if (!string.IsNullOrEmpty(venue) && venue.ToUpper().Equals("CDH"))
            {
                this.myWindow.Height = 366;
                LockTimeOutHeight = 47;

                if (SystemParamHandler.Instance.GetUserSetting() != null
                    && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
                {
                    IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
                    LockTimeOutCheck = true;
                }
                else
                {
                    IocOutTime = "30";
                    LockTimeOutCheck = true;
                }
            }

            //if (isFrim)
            //    LockTimeOutCheck = !isFrim;

            _isFirm = isFrim;

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


            //if (source == "ODM")
            //{
                //// 行情数据不正确，点价不能直接用行情中的netprice，需要使用点价的收益率重新计算净价
                //CalBond("4", settle, clickPrice, security);
            //}
            //else
            //{
            //    NetPrice = netPrice;
            //}

            if (this.isBid)
                OnBidClick();
            else
                OnOfrClick();

            SendPriceItem.Amount = clickAmount;
            CustomPrice = clickPrice;

            if (!string.IsNullOrEmpty(source) && source.ToUpper().Equals("XBOND"))
                OnXBondClick();
            else
                SetXbondUI(false);

            _currentSource = source;
            _currentId = id;
            _trsactTime = trsactTime;

            _deliveryType = deliveryType;
            _clearingMethod = clearingMethod;
            _quoteType = quoteType;

            ParseSettle(settle);

            if (wholeflag == "1")
            {
                sendPriceItem.AmountBoxIndex = 1;
            }
            else
            {
                sendPriceItem.AmountBoxIndex = 0;
            }

            _isInit = false;

            // 行情数据不正确，点价不能直接用行情中的netprice，需要使用点价的收益率重新计算净价 
            CalBond("4", GetSetttle(), NumChange(customPrice), security);

            List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");
            if (userVenues != null && userVenues.Count > 0)
                SendPriceItem.NonfirmTime = userVenues[0].firmtonontime;


            _sendPriceAdapter.SetFilterParam(!firmChecked, GetSetttle());
            _sendPriceAdapter.Init(SendPriceItem);
            
        }
        
        private void MyWindow_Unloaded(object sender, RoutedEventArgs e)
        {
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
                            settle = _sendPriceAdapter.IsTomorrow(SelectDate, ComboBoxSelectIndex) ? "3" : "1";//2:明天+0转T+1
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

        private string GetSelectItem(ListBox listBox)
        {
            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null)
                {
                    result += itemData.Code + ",";
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
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
            else if (!decimal.TryParse(sendPriceItem.Amount, out temD) || temD == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
                result = false;
            else if (CheckBondType() && string.IsNullOrEmpty(NetPrice))
                result = false;

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

        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle newOrder = new NewOrderSingle();
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
            newOrder.ClOrdID = DataTimeSequence.Instance.GetId();//"c" + DateTime.Now.Ticks;
            newOrder.SecurityID = sendPriceItem.Security;
            //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
            newOrder.Side = isBid ? "1" : "2";
            newOrder.Price = CustomPrice;//"4.2";
            newOrder.OrdType = "2";
            newOrder.OrderQty = sendPriceItem.Amount;
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

            return newOrder;
        }

        private void OnConfirmClick()
        {
            if (!CheckParam())
            {
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
            isBid = true;
            sendPriceItem.RandomNum = string.Empty;
            FirmChecked = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.ShowBidEffect = (Effect)myWindow.FindResource("BidEffect");
            sendPriceItem.ShowOfrEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
            sendPriceItem.NetPriceVisible = Visibility.Collapsed;
        }

        private void OnOfrClick()
        {
            isBid = false;
            FirmChecked = false;
            sendPriceItem.RandomNum = string.Empty;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.ShowOfrEffect = (Effect)myWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
            sendPriceItem.NetPriceVisible = Visibility.Collapsed;
        }

        private void OnRefreshClick()
        {

        }
    }
}
