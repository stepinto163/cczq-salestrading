using com.gateway.log;
using STC.Common.Handler.Inner;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Inner;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.Drp.Component;
using STC.Common.ViewModel.Drp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Common.ViewModel.Drp
{
    /// <summary>
    /// 点价提醒界面
    /// </summary>
    public class NofirmPointPriceViewModel : ViewModelBase
    {
        #region 构造
        public bool IsSetFocus = false;

        private string id;
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    this.OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string title = "被点价";
        /// <summary>
        /// 自定义标题
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    this.OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string message;
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    this.OnPropertyChanged(nameof(Message));
                }
            }
        }

        private string detail;
        /// <summary>
        /// DRPDetail
        /// </summary>
        public string Detail
        {
            get
            {
                return detail;
            }
            set
            {
                if (detail != value)
                {
                    detail = value;
                    this.OnPropertyChanged(nameof(Detail));
                }
            }
        }

        private string contenttype;
        /// <summary>
        /// 消息类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return contenttype;
            }
            set
            {
                if (contenttype != value)
                {
                    contenttype = value;
                    this.OnPropertyChanged(nameof(ContentType));
                }
            }
        }


        private bool isshow;
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow
        {
            get
            {
                return isshow;
            }
            set
            {
                if (isshow != value)
                {
                    isshow = value;
                    this.OnPropertyChanged(nameof(IsShow));
                }
            }
        }

        private string delflag;
        /// <summary>
        /// 是否删除
        /// </summary>
        public string DelFlag
        {
            get
            {
                return delflag;
            }
            set
            {
                if (delflag != value)
                {
                    delflag = value;
                    this.OnPropertyChanged(nameof(DelFlag));
                }
            }
        }


        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
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


        private string symbol;
        public string Symbol { get { return symbol; } set { symbol = value; this.OnPropertyChanged(); } }


        private string side;
        /// <summary>
        /// 买卖方向
        /// </summary>
        public string Side { get { return side; } set { side = value; this.OnPropertyChanged(); } }

        private string wholeFlag;

        /// <summary>
        /// 整量
        /// </summary>
        public string WholeFlag 
        { 
            get 
            { 
                return wholeFlag; 
            } 
            set 
            { 
                wholeFlag = value;
                IsEnableEditAmount = value == "0" ? true : false;
                this.OnPropertyChanged(); 
            } 
        }


        private bool isEnableEditAmount = true;
        /// <summary>
        /// 是否允许修改量
        /// </summary>
        public bool IsEnableEditAmount { get { return isEnableEditAmount; } set { isEnableEditAmount = value; this.OnPropertyChanged(); } }

        private string price;
        /// <summary>
        /// 收益率
        /// </summary>
        public string Price
        {
            get { return price; }
            set
            {
                price = value;
                this.OnPropertyChanged();
            }
        }


        private string pointprice;
        /// <summary>
        /// 建议收益率
        /// </summary>
        public string PointPrice
        {
            get { return pointprice; }
            set
            {
                pointprice = value;
                this.OnPropertyChanged();
            }
        }


        private string firmType;
        /// <summary>
        /// 整量
        /// </summary>
        public string FirmType { get { return firmType; } set { firmType = value; this.OnPropertyChanged(); } }

        private string amount;
        /// <summary>
        /// 量
        /// </summary>
        public string Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                this.OnPropertyChanged();
            }
        }

        private string pointAmount;
        /// <summary>
        /// 建议量
        /// </summary>
        public string PointAmount
        {
            get
            {
                return pointAmount;
            }
            set
            {
                pointAmount = value;
                AlertsignPointAmountforground = CommonUtils.ConvertToDecimal(pointAmount) <= SourcePointAmount ? new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7)) : new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A));
                this.OnPropertyChanged();
            }
        }

        private Brush alertsignpointamountforground;
        /// <summary>
        /// 建议量>回价量 警示颜色
        /// </summary>
        public Brush AlertsignPointAmountforground
        {
            get { return alertsignpointamountforground; }
            set
            {
                alertsignpointamountforground = value;
                this.OnPropertyChanged();
            }
        }

        private string orderStatus;
        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get { return orderStatus; } set { orderStatus = value; } }


        private string settle;
        /// <summary>
        /// Settle
        /// </summary>
        public string Settle { get { return settle; } set { settle = value; this.OnPropertyChanged(); } }

        private string showSettle;
        /// <summary>
        /// 用于显示的清算速度
        /// </summary>
        public string ShowSettle { get { return showSettle; } set { showSettle = value; this.OnPropertyChanged(); } }

        public string settldate;
        /// <summary>
        /// 交易日
        /// </summary>
        public string SettlDate { get { return settldate; } set { settldate = value; this.OnPropertyChanged(); } }

        private Visibility _wholeVisibility = Visibility.Collapsed;
        public Visibility WholeVisibility
        {
            get
            {
                return _wholeVisibility;
            }

            set
            {
                _wholeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string netPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice { get { return netPrice; } set { netPrice = value; this.OnPropertyChanged(); } }



        private string securityid;
        /// <summary>
        /// 债券ID
        /// </summary>
        public string SecurityID { get { return securityid; } set { securityid = value; this.OnPropertyChanged(); } }


        private string timeInForce;
        public string TimeInForce { get { return timeInForce; } set { timeInForce = value; this.OnPropertyChanged(); } }


        private string iocTimeOut;
        /// <summary>
        /// 超时时间
        /// </summary>
        public string IocTimeOut { get { return iocTimeOut; } set { iocTimeOut = value; this.OnPropertyChanged(); } }

        private string ordervaliduntiltime;
        public string OrderValidUntilTime { get { return ordervaliduntiltime; } set { ordervaliduntiltime = value; this.OnPropertyChanged(); } }

        private string transacttime;
        public string TransactTime { get { return transacttime; } set { transacttime = value; this.OnPropertyChanged(); } }

        private string quoteid;
        /// <summary>
        /// 询价ID
        /// </summary>
        public string QuoteID { get { return quoteid; } set { quoteid = value; this.OnPropertyChanged(); } }


        private string orderId;
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get { return orderId; } set { orderId = value; this.OnPropertyChanged(); } }

        private string actiontype;
        public string ActionType { get { return actiontype; } set { actiontype = value; this.OnPropertyChanged(); } }


        private string ioccount;
        /// <summary>
        /// 回价个数
        /// </summary>
        public string IocCount { get { return ioccount; } set { ioccount = value; this.OnPropertyChanged(); } }


        private string _showside;
        /// <summary>
        /// 交易方向，用于显示
        /// </summary>
        public string ShowSide
        {
            get
            {
                return _showside;
            }

            set
            {
                _showside = value;

                this.OnPropertyChanged();
            }
        }

        private string _ShowPrice;
        /// <summary>
        /// 收益率 用于显示
        /// </summary>
        public string ShowPrice
        {
            get
            {
                return _ShowPrice;
            }

            set
            {
                _ShowPrice = value;
                this.OnPropertyChanged();
            }
        }


        private string name;
        public string UserName { get { return name; } set { name = value; this.OnPropertyChanged(); } }


        private decimal sourcepointAmount;
        /// <summary>
        /// 用户点价量
        /// </summary>
        public decimal SourcePointAmount
        {
            get
            {
                return sourcepointAmount;
            }
            set
            {
                sourcepointAmount = value;
                this.OnPropertyChanged();
            }
        }

        private PointPriceDetailModel drpdetail;
        /// <summary>
        /// 原始 JSON
        /// </summary>
        public PointPriceDetailModel DrpDetail
        {
            get { return drpdetail; }
            set
            {
                if (value != null)
                {
                    drpdetail = value;
                    this.OnPropertyChanged(nameof(DrpDetail));
                }

            }
        }

        private string orderremainseconds = string.Empty;
        
        /// <summary>
        /// 倒计时
        /// </summary>
        public string OrderRemainSeconds 
        { 
            get 
            { 
                return orderremainseconds; 
            } 

            set 
            { 
                orderremainseconds = value;
                if (OrderRemainSecondsVisible == Visibility.Collapsed)
                {
                    OrderRemainSecondsVisible = Visibility.Visible;
                }
                this.OnPropertyChanged(); 
            } 
        }

        private Visibility orderremainsecondsvisible = Visibility.Collapsed;

        /// <summary>
        /// s 是否可见
        /// </summary>
        public Visibility OrderRemainSecondsVisible
        {
            get
            {
                return orderremainsecondsvisible;
            }

            set
            {
                orderremainsecondsvisible = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _leavesqtytotal = decimal.Zero;
        /// <summary>
        /// 待确认量
        /// </summary>
        public decimal LeavesQtyTotal
        {
            get
            {
                return _leavesqtytotal;
            }
            set
            {
                _leavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        private string _showleavesqtytotal = "--";
        /// <summary>
        /// 待确认量
        /// </summary>
        public string ShowLeavesQtyTotal
        {
            get
            {
                return _showleavesqtytotal;
            }
            set
            {
                _showleavesqtytotal = value;
                this.OnPropertyChanged();
            }
        }

        #region IRS
        /// <summary>
        /// 是否显示 IRS模式, IRS时不可见 现券时可见
        /// </summary>
        private bool isShowIRS = false;
        public bool IsShowIRS
        {
            get { return isShowIRS; }
            set
            {
                isShowIRS = value;
                this.OnPropertyChanged("IsShowIRS");
            }
        }


        /// <summary>
        /// 是否是交易所债券
        /// </summary>
        private bool isShowSHE = false;
        public bool IsShowSHE
        {
            get { return isShowSHE; }
            set
            {
                isShowSHE = value;
                ShowKwUnit = isShowSHE ? "w" : "kw";
                this.OnPropertyChanged("IsShowIRS");
            }
        }

        private string _ShowKwUnit = "kw";
        /// <summary>
        /// kw w
        /// </summary>
        public string ShowKwUnit
        {
            get
            {
                return _ShowKwUnit;
            }
            set
            {
                _ShowKwUnit = value;
                this.OnPropertyChanged();
            }
        }
        
        #endregion

        public ICommand CloseSelfCommand { get; set; }
        public ICommand RefuseCommand { get; set; }
        public ICommand CommitCommand { get; set; }
        public ICommand AddAmountCommand { get; set; }
        public ICommand ReduceAmountCommand { get; set; }

        //Modi:cyc Date:2023-10-30 Func:新倒计时
        System.Threading.Timer stTimer;
        private object Updatelock = new object();

        public NofirmPointPriceView ParentWindow;

        public NofirmPointPriceViewModel()
        {
            this.AddAmountCommand = new ComRelayCommand(OnAddAmountCommand);
            this.ReduceAmountCommand = new ComRelayCommand(OnReduceAmountCommand);
            this.RefuseCommand = new ComRelayCommand(OnRefuseCommand);
            this.CommitCommand = new ComRelayCommand(OnCommitCommand);
            StartTimeTick();
        }

        #endregion

        #region Command
        private void OnRefuseCommand()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    InnerOrderReq innerOrderReq = new InnerOrderReq();
                    innerOrderReq.OrderID = OrderId;

                    PResult result = InnerHandler.Instance.FakCancelOrder(innerOrderReq);
                    OnCloseSelfCommand();
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void OnCommitCommand()
        {
            decimal d = 0;
            decimal pm = 0;
            if (isShowIRS)
            {
                if (decimal.TryParse(PointAmount, out d))
                {
                    pm = d * 100;
                }
            }
            else
            {
                if (!IsShowSHE)
                {
                    if (decimal.TryParse(PointAmount, out d))
                    {
                        pm = d * 1000;
                    }
                }
                else
                {
                    if (decimal.TryParse(PointAmount, out d))
                    {
                        pm = d * 1;
                    }
                }
            }

            if (IsShowIRS)
            {
                // IRS
                if ((pm % 100 > 0) || d == 0)
                {
                    CustomMessageBox.Error(string.Format("确认量必须是100的整数倍，当前输入值：{0}", pm));
                    return;
                }
            }
            else
            {
                if (!IsShowSHE)
                {
                    //现券
                    if ((pm % 100 > 0) || d == 0)
                    {
                        CustomMessageBox.Error(string.Format("确认量必须是100的整数倍，当前输入值：{0}", pm));
                        return;
                    }
                }
                else
                {
                    if ((pm % 1> 0) || d == 0)
                    {
                        CustomMessageBox.Error(string.Format("确认量必须是1的整数倍，当前输入值：{0}", pm));
                        return;
                    }
                }
            }

            if (IsShowIRS)
            {
                // IRS
                if (pm > SourcePointAmount)
                {
                    string ErrorTxt = "确认量需要小于等于点价总量";
                    CustomMessageBox.Warning($"{ErrorTxt}");
                    return;
                }
            }
            else
            {
                if (pm > SourcePointAmount)
                {
                    string ErrorTxt = "确认量需要小于等于点价总量";
                    CustomMessageBox.Warning($"{ErrorTxt}");
                    return;
                }
            }

            if (pm > LeavesQtyTotal)
            {
                string ErrorTxt = "确认量需要小于等于报价总量";
                CustomMessageBox.Warning($"{ErrorTxt}");
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    InnerOrderReq innerOrderReq = new InnerOrderReq();
                    innerOrderReq.StrategyOrderId = orderId;
                    innerOrderReq.OrderQty = pm.ToString();

                    PResult result = InnerHandler.Instance.FakTransactionOrder(innerOrderReq);
                    if (result.IsSuccess())
                    {
                        OnCloseSelfCommand();
                    }
                    else
                    {
                        CustomMessageBox.Warning($"失败原因：{result.Msg}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void OnAddAmountCommand()
        {
            decimal amount = 0;
            if (decimal.TryParse(PointAmount, out amount))
            {
                if (!IsShowSHE)
                {
                    amount += 0.1M;
                    if (amount <= 1000)
                    {
                        PointAmount = amount.ToString();
                    }
                    else if (amount <= 1000000)
                    {
                        PointAmount = amount.ToString();
                    }
                }
                else
                {
                    amount += 1M;
                    PointAmount = amount.ToString();
                }
            }
        }

        private void OnReduceAmountCommand()
        {
            decimal amount = 0;
            if (decimal.TryParse(PointAmount, out amount))
            {
                if (!IsShowSHE)
                {
                    amount = amount - 0.1M;
                    if (amount >= 0)
                    {
                        PointAmount = amount.ToString();
                    }
                }
                else
                {
                    amount = amount - 1M;
                    if (amount >= 0)
                    {
                        PointAmount = amount.ToString();
                    }
                }
            }
        }

        private void OnCloseSelfCommand()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ParentWindow != null)
                {
                    try
                    {
                        ParentWindow.Close();
                    }
                    catch(Exception ex)
                    { 
                    
                    }
                }
            });
        }

        #endregion

        #region 定向报价倒计时

        private void StartTimeTick()
        {
            //dueTime: 定时器延时多久开始调用。单位 毫秒
            //period: 定时器每隔多久调用一次。单位 毫秒
            stTimer = new System.Threading.Timer(new TimerCallback(DescTimer_Tick), null, 100, 500);
        }

        /// <summary>
        /// 更新剩余时间
        /// </summary>

        private void DescTimer_Tick(object state)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DateTime ordertransactTime = CommonUtils.ConvertToDateTime(OrderValidUntilTime.ToString());
                    TimeSpan orderdiffTime = ordertransactTime - DateTimeEx.GetNow();
                    int res = Convert.ToInt32(orderdiffTime.TotalSeconds);
                    this.OrderRemainSeconds = res < 0 ? "0" : $"{res}";
                });
            }
            catch (Exception ex)
            {

            }
        }
        #endregion


        /// <summary>
        /// 报价或议价
        /// </summary>
        /// <param name="inparam"></param>
        public void SetClipboard(object inparam)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string side = Side == "1" ? "TKN" : "GVN";
                string txt = string.Empty;
                //decimal d = 0;
                //decimal pm = 0;
                //if (decimal.TryParse(PointAmount, out d))
                //{
                //    pm = d * 1000;
                //}

                if (!string.IsNullOrEmpty(OrderRemainSeconds) && OrderRemainSeconds != "--" && OrderRemainSeconds != "0")
                    txt = $"{this.Symbol} {side} {Price} {SourcePointAmount}万 是否OK？({OrderRemainSeconds}s 内回复有效)";
                else
                    txt = $"{this.Symbol} {side} {Price} {SourcePointAmount}万 是否OK？";


                ClipboardHelper.CopyToClipboard(txt);
                CommonUtils.Prompt2Second("提示", "已复制");
            });
        }

        #region Dispose
        ~NofirmPointPriceViewModel()
        {
            this.stTimer.Dispose();
            LogHandler.Info("DRPNotificationViewModel Dispose");
            this.Dispose();
        }
        #endregion
    }
}
