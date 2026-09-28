using Newtonsoft.Json;
using STC.Common.Handler.RFQ;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.RFQ;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Adapter;
using STC.Common.ViewModel.Data;
using STC.Rfq.Plugin.Model;
using STC.Rfq.Plugin.Utils;
using STC.Rfq.Plugin.View;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.Rfq.Plugin.ViewModel
{
    public class RfqReplayRefuseViewModel : ViewModelBase
    {
        public Window myWindow;

        private SendPriceModelAdapter _sendPriceAdapter = null;

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

        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        /// <summary>
        /// 边框颜色
        /// </summary>
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


        private string _toptitle;
        /// <summary>
        /// 窗口标题
        /// </summary>
        public string TopTitle
        {
            get { return _toptitle; }
            set
            {
                if (value != null)
                {
                    _toptitle = value;
                    this.OnPropertyChanged(nameof(TopTitle));
                }
            }
        }


        private Brush borderBH;
        public Brush BorderBH
        {
            get { return borderBH; }
            set
            {
                if (value != null)
                {
                    borderBH = value;
                    this.OnPropertyChanged(nameof(BorderBH));
                }
            }
        }

        private Brush borderBG;
        /// <summary>
        /// 背景，需要根据Bid Ofr变化
        /// </summary>
        public Brush BorderBG
        {
            get { return borderBG; }
            set
            {
                //if (value != null)
                {
                    borderBG = value;
                    this.OnPropertyChanged(nameof(BorderBG));
                }
            }
        }

        private string searchText;
        /// <summary>
        /// 代码
        /// </summary>
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value != null)
                {
                    searchText = value;

                    string securityid = value.Split(' ')[0];
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                    if (bondsInfoPo != null)
                    {
                        SecurityID = bondsInfoPo.Bond_ID;
                        Symbol = bondsInfoPo.Short_Name;
                        SecurityType = bondsInfoPo.SECURITY_TYPE;


                        SendPriceItem.Security = value;
                        _sendPriceAdapter.SetFilterParam(false, "2");
                        CustomPrice = _sendPriceAdapter.GetInitBestPrice(IsBid);
                    }

                    this.OnPropertyChanged(nameof(SearchText));
                }
            }
        }


        private string _symbol = string.Empty;
        /// <summary>
        /// 简称
        /// </summary>
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged(nameof(Symbol));
                }
            }
        }

        private string _securityid = string.Empty;
        /// <summary>
        /// SecurityID
        /// </summary>
        public string SecurityID
        {
            get
            {
                return _securityid;
            }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.RfqODD.SecurityID = SecurityID;
                    this.RfqODD.Symbol = Symbol;
                    this.RfqODD.SecurityType = SecurityType;
                    this.OnPropertyChanged(nameof(SecurityID));
                }
            }
        }

        private string securitytype;
        public string SecurityType
        {
            get
            {
                return securitytype;
            }
            set
            {
                if (securitytype != value)
                {
                    securitytype = value;
                    this.OnPropertyChanged(nameof(SecurityType));
                }
            }
        }

        

        private string _side = string.Empty;
        /// <summary>
        /// 方向 >0多 <0空 1 Bid 2 Ofr
        /// </summary>
        public string Side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged(nameof(Side));
                }
            }
        }

        /// <summary>
        /// 回复量
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

        public bool IsFromPrice = false;

        /// <summary>
        /// 收益率
        /// </summary>
        private string customPrice = string.Empty;
        public string CustomPrice
        {
            get
            {
                return customPrice;
            }

            set
            {
                if (value != customPrice)
                {
                    customPrice = value;
                    //计算出净价
                    this.OnPropertyChanged(nameof(CustomPrice));
                    if (!IsFromPrice)
                        CalcNetPrice();
                }
            }
        }

       

        private string netPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice
        {
            get { return netPrice; }
            set
            {
                string oldPrice = netPrice;
                netPrice = value;
                decimal d = 0;
                if (!IsFromPrice && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                {
                    CalcPrice();
                }
                else
                {
                    IsFromPrice = false;
                }

                if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                {
                    customPrice = string.Empty;
                }

                this.OnPropertyChanged(nameof(NetPrice));
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


        private RfqOrderDetailData _rfqodd;
        /// <summary>
        /// 数据对象
        /// </summary>
        public RfqOrderDetailData RfqODD
        {
            get
            {
                return _rfqodd;
            }
            set
            {
                if (_rfqodd != value)
                {
                    _rfqodd = value;
                    this.OnPropertyChanged(nameof(RfqODD));
                }
            }
        }

        private string _ValidUntilTime;
        public string ValidUntilTime
        {
            get
            {
                return _ValidUntilTime;
            }
            set
            {
                if (_ValidUntilTime != value)
                {
                    _ValidUntilTime = value;
                    this.OnPropertyChanged(nameof(ValidUntilTime));
                }
            }
        }

        public int _remainseconds;
        /// <summary>
        /// 剩余时间
        /// </summary>
        public int RemainSeconds
        {
            get
            {
                return _remainseconds;
            }
            set
            {
                if (_remainseconds != value)
                {
                    _remainseconds = value;
                    this.OnPropertyChanged(nameof(RemainSeconds));
                }
            }
        }


        public string _showremainseconds;
        /// <summary>
        /// 显示剩余时间
        /// </summary>
        public string ShowRemainSeconds
        {
            get
            {
                return _showremainseconds;
            }
            set
            {
                if (_showremainseconds != value)
                {
                    _showremainseconds = value;
                    this.OnPropertyChanged(nameof(ShowRemainSeconds));
                }
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

        public bool IsBid { get; set; }

        public ICommand ExpandCommand { get { return new ComRelayCommand(OnExpandClick); } }
        public ICommand CloseCommand { get { return new ComRelayCommand(OnCloseCommandClick); } }

        /// <summary>
        /// 取消
        /// </summary>
        public ICommand CancelCommand { get { return new ComRelayCommand(OnCloseCommandClick); } }

        /// <summary>
        /// 撤单
        /// </summary>
        public ICommand CancelOrderCommand { get { return new ComRelayCommand(OnCancelOrderCommandClick); } }

        /// <summary>
        /// 确认或改价
        /// </summary>
        public ICommand ConfirmOrderCommand { get { return new ComRelayCommand(OnConfirmOrderCommandClick); } }
        
        /// <summary>
        /// 拒绝
        /// </summary>
        public ICommand RefuseOrderCommand { get { return new ComRelayCommand<object>(OnRefuseOrderCommand); } }

        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new ComRelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new ComRelayCommand(ReducePriceClick); } }
        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }

        private RfqOrderDetailData inrfqdd;
        public RfqOrderDetailData InRfqDD
        {
            get
            {
                return inrfqdd;
            }
            set
            {
                if (inrfqdd != value)
                {
                    inrfqdd = value;
                    this.OnPropertyChanged(nameof(inrfqdd));
                }
            }
        }

        //Modi:cyc Func:增加倒计时
        DispatcherTimer descTimer = new DispatcherTimer();

        public string Action;
        public RfqReplayRefuseViewModel(Window my, RfqOrderDetailData para)
        {
            this.myWindow = my;
            //最优价
            _sendPriceAdapter = new SendPriceModelAdapter();
            SendPriceItem = new SendPriceItem();
            _sendPriceAdapter.Init(SendPriceItem);
            if (SystemParamHandler.Instance.GetUserSetting() != null)
            {
                if (SystemParamHandler.Instance.GetUserSetting().inf2 != null)
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                    if (consistencyCheckPo != null)
                    {
                        //解析Rfq有效时长
                        ParseRfqValidTimeFormat(consistencyCheckPo.RfqDefaultValidTime);
                    }
                }
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (para == null)
                {
                    TopTitle = "请求回复Ofr";
                    BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
                    RfqODD = new RfqOrderDetailData();
                    RfqODD.OrderQty = "1";
                    RfqODD.OrderQty2 = "1";
                    //RfqODD.IocTimeOut = "2 h";
                    RfqODD.IsSelfRfq = "1";
                    RfqODD.CounterPartyName = "中国银行香港";
                    RfqODD.CounterPartySubName = "李小圆";
                    RfqODD.TradeDate = "2022-12-08";
                    RfqODD.SettlType = "2";
                    RfqODD.marketindicator = "4";
                    this.Side = "2";
                    this.SecurityType = "2";
                    string bidorofr = this.Side == "1" ? "Bid" : "Ofr";
                    this.IsBid  = bidorofr == "Bid" ? true : false;
                    _sendPriceAdapter.SetFilterParam(false, SecurityType);
                    InRfqDD = RfqODD;
                    TopTitle = $"请求回复{bidorofr}";
                    if (bidorofr == "Bid")
                    {
                        BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
                        BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
                    }
                    else
                    {
                        BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
                        BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
                    }
                }
                else
                {
                    string bidorofr = para.Side == "1" ? "Bid" : "Ofr";

                    TopTitle = $"请求回复{bidorofr}";
                    if (bidorofr == "Bid")
                    {
                        BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
                        BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
                    }
                    else
                    {
                        BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
                        BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
                    }

                    RfqODD = para;

                    this.AmountText = RfqODD.OrderQty2;
                    InRfqDD = para;

                    // Taker发送请求 30 Maker解锁 32 
                    if ((RfqODD.QuoteStatus == "30") || (RfqODD.QuoteStatus == "32") || (RfqODD.QuoteStatus == "42") || (RfqODD.QuoteStatus == "40"))
                    {
                        LockOrder();
                    }


                    SendPriceItem.Security = RfqODD.SecurityID;

                    this.IsBid = bidorofr == "Bid" ? true : false;
                    int defaSettlType = int.Parse(RfqODD.SettlType);
                    _sendPriceAdapter.SetFilterParam(false, defaSettlType == 1 ? "1" : "2,3");

                    //获取收益率
                    decimal tmpprice = 0;
                    if (!decimal.TryParse(RfqODD.Price, out tmpprice))
                    {
                        CustomPrice = _sendPriceAdapter.GetInitBestPrice(IsBid);
                    }
                    else
                    {
                        if (tmpprice <= 0)
                        {
                            CustomPrice = _sendPriceAdapter.GetInitBestPrice(IsBid);
                        }
                        else
                        {
                            this.CustomPrice = tmpprice.ToString("F4");
                        }
                    }

                    (this.myWindow as RfqReplayRefuseView).SetPriceFocus();
                }

                this.descTimer = new DispatcherTimer();
                this.descTimer.Interval = TimeSpan.FromSeconds(1);
                this.descTimer.Tick += DescTimer_Tick;
                this.descTimer.Start();
            }));
        }

        private void DescTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(InRfqDD.ValidUntilTime))
                {
                    DateTime date = RfqTools.ConvertCfetsDataTime(InRfqDD.ValidUntilTime);
                    if (date != null)
                    {
                        // 剩余时间
                        TimeSpan diffTime = date - DateTime.Now;
                        int res = Convert.ToInt32(diffTime.TotalSeconds);
                        InRfqDD.ShowRemainSeconds = res < 0 ? "0" : RfqTools.FormatTime(res);
                        InRfqDD.RemainSeconds = res < 0 ? 0 : res;
                    }
                }
                else
                {
                    this.RemainSeconds = 0;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CalcNetPrice()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string _tmnetprice = RfqHandler.Instance.GetNetPrice(this.RfqODD.SecurityID, "4", "2,3", RfqHandler.Instance.NumChange(this.CustomPrice));
                        this.IsFromPrice = true;
                        this.NetPrice = _tmnetprice;
                        this.OnPropertyChanged("NetPrice");
                    }));
                }
                catch (Exception ex)
                {
                    com.gateway.log.LogHelper.Error($"CalcNetPrice exception.{ex.Message}");
                }
            });
        }

        /// <summary>
        /// 反向计算收益率
        /// </summary>
        private void CalcPrice()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string _tmnetprice = RfqHandler.Instance.GetNetPrice(this.RfqODD.SecurityID, "2", "2,3", this.NetPrice);
                        this.IsFromPrice = false;
                        this.CustomPrice = _tmnetprice;
                    }));
                }
                catch (Exception ex)
                {
                    com.gateway.log.LogHelper.Error($"CalcNetPrice exception.{ex.Message}");
                }
            });
        }


        #region UI Command
        /// <summary>
        /// 撤单
        /// </summary>
        private void OnCancelOrderCommandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async() =>
            {
                RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                req.QuoteReqID = RfqODD.QuoteReqID;
                req.QuoteStatus = "40";
                req.SecurityID = RfqODD.SecurityID;
                req.ClOrdID = RfqODD.ClOrdID;
                req.UserName = LocalEnv.UserInfo.UserName; //用户名

                req.MacID = EngineHelper.GetMacByNetworkInterface();
                req.InsideIP = EngineHelper.GetIp();
                req.HardDiskID = EngineHelper.Getharddiskid2();
                req.Info5 = "Manual";
                PResult reply = await ChangeAcceptOrder(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        //this.SelectedItem_MyOrders[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = false;
                        CustomMessageBox.Info(string.Format($"撤单成功！"), "撤单", myWindow);
                    }
                    else
                    {
                        com.gateway.log.LogHelper.Error($"撤单失败, 原因: {reply.Msg}");
                        CustomMessageBox.Error(string.Format($"撤单失败, 原因:{reply.Msg}."), "撤单", myWindow);
                    }
                }

                OnCloseCommandClick();
            }));
        }

        /// <summary>
        /// 确认或改价
        /// </summary>
        private void OnConfirmOrderCommandClick()
        {
            if (this.Action != "NewOrder")
            {
                if (CheckParam())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                    {
                        // 时长
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

                        // RFQ中午休市的时间是12点到一点半
                        string ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DateTime dtstart = CommonUtils.ConvertToDateTime($"{DateTimeEx.ServerTime.ToString("yyyy-MM-dd")} 12:00:00");
                        if (dtstart.CompareTo(DateTime.Parse(ValidUntilTime)) <= 0)
                        {
                            ValidUntilTime = DateTime.Parse(ValidUntilTime).AddMinutes(90).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }

                        // 时长
                        string validTimeType = "1";
                        // ValidMinute,ValidSecond
                        string min = string.IsNullOrEmpty(ValidMinute) ? "" : ValidMinute;
                        string sec = string.IsNullOrEmpty(ValidSecond) ? "" : ValidSecond;
                        string info1 = $"{validTimeType}|{min}:{sec}";
                        OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = ValidUntilTime };
                        string Info1 = JsonConvert.SerializeObject(otm);

                        if (RfqODD != null)
                        {
                            RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                            req.QuoteReqID = RfqODD.QuoteReqID; //请求编号
                            req.SecurityID = RfqODD.SecurityID;
                            req.ClOrdID = RfqODD.ClOrdID;       //客户编号
                            req.MacID = EngineHelper.GetMacByNetworkInterface();
                            req.InsideIP = EngineHelper.GetIp();
                            req.HardDiskID = EngineHelper.Getharddiskid2();
                            if (CommonUtils.IsNumeric(AmountText))
                            {
                                decimal tmpv = CommonUtils.ConvertToDecimal(AmountText) * 1000.0m;
                                req.OrderQty = CommonUtils.formatString(tmpv.ToString(), 0); //报价量
                            }

                            //req.OrderQty = AmountText;          //报价量

                            req.UserName = LocalEnv.UserInfo.UserName; //用户名
                            req.Price = CustomPrice;
                            req.NetPrice = NetPrice;
                            req.QuoteStatus = "33";
                            req.Info1 = Info1;
                            req.ValidUntilTime = ValidUntilTime;
                            req.Yield = CustomPrice;
                            req.QuoteID = RfqODD.QuoteID;       //报价编号
                            req.MacID = EngineHelper.GetMacByNetworkInterface();
                            req.InsideIP = EngineHelper.GetIp();
                            req.HardDiskID = EngineHelper.Getharddiskid2();
                            req.Info5 = "Manual";
                            PResult reply = await ChangeAcceptOrder(req);
                            if (reply != null)
                            {
                                if (reply.Code == "0000")
                                {
                                    CustomMessageBox.Info(string.Format($"修改订单成功！"), "修改订单", myWindow);
                                    OnCloseCommandClick();
                                }
                                else
                                {
                                    com.gateway.log.LogHelper.Error($"修改订单失败, 原因: {reply.Msg}");
                                    CustomMessageBox.Error(string.Format($"修改订单失败, 原因:{reply.Msg}."), "修改订单", myWindow);
                                }
                            }
                            else
                            {
                                com.gateway.log.LogHelper.Error($"修改订单失败, 原因: 未返回数据");
                                CustomMessageBox.Error("修改订单失败", "错误", myWindow);
                            }
                        }
                    }));
                }
                else
                {
                    List<string> errmsg = GetErrorTxt();
                    CustomMessageBox.Error(string.Join("\r\n", errmsg.ToArray()), "校验错误", this.myWindow);
                }
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                {
                    // 时长
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

                    string ValidUntilTime = DateTimeEx.ServerTime.AddSeconds(totalsecond).ToString("yyyyMMdd-HH:mm:ss.fff");

                    // 时长
                    string validTimeType = "1";
                    // ValidMinute,ValidSecond
                    string min = string.IsNullOrEmpty(ValidMinute) ? "" : ValidMinute;
                    string sec = string.IsNullOrEmpty(ValidSecond) ? "" : ValidSecond;
                    string info1 = $"{validTimeType}|{min}:{sec}";
                    OrderTimeModel otm = new OrderTimeModel() { ValidTimeType = info1, ValidUntilTime = ValidUntilTime };
                    string Info1 = JsonConvert.SerializeObject(otm);

                    RfqNewOrderReq req = new RfqNewOrderReq();
                    req.ValidUntilTime = ValidUntilTime;
                    req.Info1 = info1;
                    req.UserName = LocalEnv.UserInfo.UserName; //用户名
                    req.MacID = EngineHelper.GetMacByNetworkInterface();
                    req.InsideIP = EngineHelper.GetIp();
                    req.HardDiskID = EngineHelper.Getharddiskid2();
                    req.SecurityID = this.SecurityID;
                    req.Symbol = this.Symbol;
                    req.SecurityType = this.SecurityType;
                    req.VenueTypeGW = "ODM";
                    req.Venues = "ODM";
                    req.ClOrdID = DataTimeSequence.Instance.GetId();
                    req.Side = this.Side;
                    if (CommonUtils.IsNumeric(AmountText))
                    {
                        decimal tmpv = CommonUtils.ConvertToDecimal(AmountText) * 1000.0m;
                        req.OrderQty = CommonUtils.formatString(tmpv.ToString(), 0); //报价量
                    }

                    req.Price = CustomPrice;
                    req.QuoteType = "112";
                    req.MsgType = "R";
                    req.QuoteReqID = "m"+ DataTimeSequence.Instance.GetId();
                    req.PartyID = "112025033021200105001";
                    req.PartyName = "鄞州银行";
                    req.PartySubID = "yzrbdealer";
                    req.PartySubName = "yzrbdealer";
                    PResult reply = await RfqNewOrder(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            CustomMessageBox.Info(string.Format($"新建订单成功！"), "新建订单", myWindow);
                            OnCloseCommandClick();
                        }
                        else
                        {
                            com.gateway.log.LogHelper.Error($"新建订单失败, 原因: {reply.Msg}");
                            CustomMessageBox.Error(string.Format($"新建订单失败, 原因:{reply.Msg}."), "新建订单", myWindow);
                        }
                    }
                    else
                    {
                        com.gateway.log.LogHelper.Error($"新建订单失败, 原因: 未返回数据");
                        CustomMessageBox.Error("新建订单失败", "错误", myWindow);
                    }
                }));
            }
        }

        /// <summary>
        /// 锁定订单
        /// </summary>
        private async void LockOrder()
        {
            RfqChangeAcceptOrderReq lockreq = new RfqChangeAcceptOrderReq();
            lockreq.QuoteReqID = RfqODD.QuoteReqID;
            lockreq.QuoteStatus = "31";
            lockreq.SecurityID = RfqODD.SecurityID;
            lockreq.ClOrdID = RfqODD.ClOrdID;
            lockreq.UserName = LocalEnv.UserInfo.UserName;
            lockreq.MacID = EngineHelper.GetMacByNetworkInterface();
            lockreq.InsideIP = EngineHelper.GetIp();
            lockreq.HardDiskID = EngineHelper.Getharddiskid2();
            PResult lockreply = await ChangeAcceptOrder(lockreq);
            if (lockreply != null)
            {
                if (lockreply.Code == "0000")
                {
                }
                else
                {
                    com.gateway.log.LogHelper.Error($"锁定订单失败, 原因:{lockreply.Msg}.");
                    CustomMessageBox.Info($"锁定订单失败, 原因:{lockreply.Msg}.", "锁定订单", myWindow);
                    return;
                }
            }
            else
            {
                com.gateway.log.LogHelper.Error($"锁定订单失败, 原因: 未返回数据");
                return;
            }
        }

        /// <summary>
        /// 解锁
        /// </summary>
        private async void UnLockOrder()
        {
            RfqChangeAcceptOrderReq lockreq = new RfqChangeAcceptOrderReq();
            lockreq.QuoteReqID = RfqODD.QuoteReqID;
            lockreq.QuoteStatus = "32";
            lockreq.SecurityID = RfqODD.SecurityID;
            lockreq.ClOrdID = RfqODD.ClOrdID;
            lockreq.UserName = LocalEnv.UserInfo.UserName;
            lockreq.MacID = EngineHelper.GetMacByNetworkInterface();
            lockreq.InsideIP = EngineHelper.GetIp();
            lockreq.HardDiskID = EngineHelper.Getharddiskid2();
            PResult lockreply = await ChangeAcceptOrder(lockreq);
            if (lockreply != null)
            {
                if (lockreply.Code == "0000")
                {
                }
                else
                {
                    if (myWindow != null)
                    {
                        CustomMessageBox.Info($"解锁订单失败, 原因:{lockreply.Msg}.", "解锁订单", myWindow);
                    }
                    else
                    {
                        CustomMessageBox.Info($"解锁订单失败, 原因:{lockreply.Msg}.", "解锁订单");
                    }
                    return;
                }
            }
            else
            {
                com.gateway.log.LogHelper.Error($"解锁订单失败, 原因: 未返回数据");
                return;
            }
        }

        /// <summary>
        /// 新建订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> RfqNewOrder(RfqNewOrderReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.RfqNewOrder(req);

            });
            return reply;

        }

        /// <summary>
        /// 改价回价拒绝
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> ChangeAcceptOrder(RfqChangeAcceptOrderReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.ChangeAcceptOrder(req);

            });
            return reply;
        }


        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="param"></param>
        private void OnRefuseOrderCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                req.QuoteReqID = RfqODD.QuoteReqID;
                req.QuoteStatus = "38";
                req.SecurityID = RfqODD.SecurityID;
                req.ClOrdID = RfqODD.ClOrdID;
                req.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                req.UserName = LocalEnv.UserInfo.UserName;
                req.MacID = EngineHelper.GetMacByNetworkInterface();
                req.InsideIP = EngineHelper.GetIp();
                req.HardDiskID = EngineHelper.Getharddiskid2();
                req.BookID = RfqODD.BookID;
                req.Info5 = "Manual";
                PResult reply = await ChangeAcceptOrder(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        CustomMessageBox.Info(string.Format($"拒绝成功！"), "拒绝", myWindow);
                    }
                    else
                    {
                        com.gateway.log.LogHelper.Error($"拒绝失败, 原因: {reply.Msg}");
                        CustomMessageBox.Error(string.Format($"拒绝失败, 原因:{reply.Msg}."), "拒绝", myWindow);
                    }
                }

                OnCloseCommandClick();
            }));
        }

        #region Check ang GetErrorText
        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        private bool CheckParam()
        {
            bool result = true;
            decimal temD = 0;
            // 时长
            // ValidMinute  ValidSecond
            if (string.IsNullOrEmpty(ValidMinute) && string.IsNullOrEmpty(ValidSecond))
            {
                result = false;
                return result;
            }

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
                result = false;
                return result;
            }

            DateTime indt = RfqTools.ConvertCfetsDataTime(InRfqDD.ValidUntilTime);
            if (DateTimeEx.ServerTime.AddSeconds(totalsecond) > indt)
            {
                result = false;
                return result;
            }


            if (string.IsNullOrEmpty(AmountText))
            {
                result = false;
                return result;
            }
            else
            {
                decimal tmpqtry = 0;
                if (!decimal.TryParse(AmountText, out tmpqtry))
                {
                    result = false;
                    return result;
                }
            }

            if (CommonUtils.ConvertToDecimal(this.InRfqDD.OrderQty2) < CommonUtils.ConvertToDecimal(AmountText))
            {
                result = false;
                return result;
            }


            return result;
        }

        /// <summary>
        /// 错误文本
        /// </summary>
        /// <returns></returns>
        private List<string> GetErrorTxt()
        {
            List<string> Reason = new List<string>();
            // 时长
            // ValidMinute  ValidSecond
            if (string.IsNullOrEmpty(ValidMinute) && string.IsNullOrEmpty(ValidSecond))
            {
                Reason.Add("有效时长需大于0秒，请检查");
            }
            else
            {
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
                    Reason.Add("有效时长需大于0秒，请检查");
                }

                DateTime indt = RfqTools.ConvertCfetsDataTime(InRfqDD.ValidUntilTime);
                if (DateTimeEx.ServerTime.AddSeconds(totalsecond) > indt)
                {
                    Reason.Add("回复有效时间超过请求有效时间，请检查");
                }
            }


            if (string.IsNullOrEmpty(AmountText))
            {
                Reason.Add("回复量不能为空，请检查");
            }
            else
            {
                decimal tmpqtry = 0;
                if (!decimal.TryParse(AmountText, out tmpqtry))
                {
                    Reason.Add("回复量输入无效，请检查");
                }
            }

            if (CommonUtils.ConvertToDecimal(this.InRfqDD.OrderQty2) < CommonUtils.ConvertToDecimal(AmountText))
            {
                Reason.Add("回复量超过请求量，请检查");
            }

            return Reason;
        }
        #endregion


        /// <summary>
        /// 关闭
        /// </summary>
        private void OnCloseCommandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (RfqODD.QuoteStatus == "30" || RfqODD.QuoteStatus == "31")
                {
                    UnLockOrder();
                }

                this.myWindow.Close();
            }));
        }

        /// <summary>
        /// 展开行情小窗
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
                        Border_Left3.BorderThickness = new Thickness(2, 0, 0, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 0, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 1, 2, 2);
                        Border_Right1.BorderThickness = new Thickness(0.5, 2, 2, 2);
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
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 1, 2, 2);
                        Border_Right1.BorderThickness = new Thickness(2, 2, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 397;
                        btn_Expand.Tag = "Collapsed";
                        PriceAndOrderView dgPrice = (PriceAndOrderView)myWindow.FindName("dgPrice");
                        LineSplit.Visibility = Visibility.Collapsed;
                    }
                }
            }));
        }

        public void InitPriceView(string searchText = "")
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = this.RfqODD.SecurityID;

                PriceAndOrderView tradeBookView = (PriceAndOrderView)myWindow.FindName("dgPrice");
                QuotationListViewModel Qm = new QuotationListViewModel();

                if (securityID != null)
                {
                    Qm.SecurityID = securityID;
                }

                Qm.GetLastExecutionsItem(securityID);
                tradeBookView.SetLastExecutionsSecurtityID(securityID);

                tradeBookView.RemoveDbDoubleClickEvent();
                tradeBookView.DataContext = Qm;
            }));
        }

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
            if (decimal.TryParse(AmountText, out amount))
            {
                amount += 1;
                if (amount <= 1000)
                {
                    AmountText = amount.ToString();
                }
            }
        }
        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(AmountText, out amount))
            {
                amount = amount - 1;
                if (amount >= 0)
                {
                    AmountText = amount.ToString();
                }
            }
        }

        /// <summary>
        /// Rfq选择类型解析  时长 "1|{min}:{sec}"
        /// </summary>
        /// <param name="info2"></param>
        private void ParseRfqValidTimeFormat(string info2)
        {
            if (!string.IsNullOrEmpty(info2))
            {
                if (info2.StartsWith("1|"))
                {
                    // ValidMinute,ValidSecond
                    string s = info2.Split('|')[1];
                    if (s.Contains(":"))
                    {
                        ValidMinute = s.Split(':')[0];
                        ValidSecond = s.Split(':')[1];
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
