using com.gateway.log;
using STC.Common.Handler.Inner;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Inner;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Component;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// NofirmView的数据内容
    /// </summary>
    public class NofirmViewModel : BaseViewModel
    {

        private Window _myWindow = null;
        private DispatcherTimer firmToNoTimer;
        private string _iocTimeOut = string.Empty;
        private string _strategyOrderId = string.Empty;
        private string _oldStrategyOrderId = string.Empty;
        private string _limitStr = "最大可输入{0}万";

        public NofirmViewModel()
        {
            InitTime();
        }

        #region 属性
        private string _limitToolTxt;
        public string LimitToolTxt { get { return string.Format(_limitStr, _limitAmount); } }


        private decimal _limitAmount;
        public decimal LimitAmount { get { return _limitAmount; } set { _limitAmount = value; this.OnPropertyChanged(); } }

        private string _outTime;
        public string OutTime { get { return _outTime; } set { _outTime = value; this.OnPropertyChanged(); } }

        private string _side;
        public string Side { get { return _side; } set { _side = value; } }

        private string _goodCode;
        public string GoodCode { get { return _goodCode; } set { _goodCode = value; } }

        private string _price;
        public string Price { get { return _price; } set { _price = value; } }

        private Brush _priceBrush;
        public Brush PriceBrush { get { return _priceBrush; } set { _priceBrush = value; } }

        private string _amount;
        public string Amount { get { return _amount; } set { _amount = value; } }

        private string _counterSubPartyName;
        public string CounterSubPartyName { get { return _counterSubPartyName; } set { _counterSubPartyName = value; } }

        private string _counterSubPartySideAmount;
        public string CounterSubPartySideAmount { get { return _counterSubPartySideAmount; } set { _counterSubPartySideAmount = value; } }

        private Brush _counterSubPartySideAmountBrush;
        public Brush CounterSubPartySideAmountBrush { get { return _counterSubPartySideAmountBrush; } set { _counterSubPartySideAmountBrush = value; } }

        private string _counterSubPartyAmount;
        public string CounterSubPartyAmount { get { return _counterSubPartyAmount; } set { _counterSubPartyAmount = value; } }

        private string _counterSubAmount;
        public string CounterSubAmount { get { return _counterSubAmount; } set { _counterSubAmount = value; } }

        private string _counterSubSideAmount;
        public string CounterSubSideAmount { get { return _counterSubSideAmount; } set { _counterSubSideAmount = value; } }

        private Brush _counterSubSideAmountBrush;
        public Brush CounterSubSideAmountBrush { get { return _counterSubSideAmountBrush; } set { _counterSubSideAmountBrush = value; } }

        private string _counterPartyName;
        public string CounterPartyName { get { return _counterPartyName; } set { _counterPartyName = value; } }


        private bool _firmGridEnable = true;
        public bool FirmGridEnable { get { return _firmGridEnable; } set { _firmGridEnable = value; } }

        #region 命令事件
        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }
        public ICommand CommitCommand { get { return new RelayCommand(CommitClick); } }
        public ICommand RefuseCommand { get { return new RelayCommand(RefuseClick); } }
        #endregion



        #endregion

        /// <summary>
        /// X按钮处理
        /// </summary>
        public void CancelClick()
        {
            if (_myWindow != null)
                _myWindow.Close();
        }

        /// <summary>
        /// 提交按钮处理
        /// </summary>
        public void CommitClick()
        {
            decimal d = 0;
            if (decimal.TryParse(Amount, out d) && ((d % 100 > 0) || d == 0))
            {
                Common.View.Component.CustomMessageBox.Error(string.Format("量必须是100的整数倍，当前输入值：{0}", Amount));

                return;
            }


            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    InnerOrderReq innerOrderReq = new InnerOrderReq();
                    innerOrderReq.StrategyOrderId = _strategyOrderId;
                    innerOrderReq.OrderQty = Amount;

                    InnerHandler.Instance.FakTransactionOrder(innerOrderReq);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CancelClick();
                    });
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 拒绝按钮处理
        /// </summary>
        public void RefuseClick()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    InnerOrderReq innerOrderReq = new InnerOrderReq();
                    innerOrderReq.OrderID = _strategyOrderId;

                    InnerHandler.Instance.FakCancelOrder(innerOrderReq);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CancelClick();
                    });
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        /// <summary>
        /// 初始化timer
        /// </summary>
        private void InitTime()
        {
            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = TimeSpan.FromMilliseconds(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
                
            }
        }

       /// <summary>
       /// 处理定时器逻辑，实时刷新界面状态
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void FirmToNoTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int outTime = 0;
                if (Int32.TryParse(_iocTimeOut, out outTime))
                {
                    outTime--;
                    _iocTimeOut = outTime.ToString();

                    Console.WriteLine("--------------:" + _iocTimeOut);

                    if (outTime >= 0)
                        OutTime = string.Format("{0}", _iocTimeOut);
                    else
                    {
                        firmToNoTimer.Stop();


                        TextBox tbAmount = _myWindow.FindName("tbAmount") as TextBox;
                        if (tbAmount != null)
                            tbAmount.IsEnabled = false;

                        Button btnRefuse = _myWindow.FindName("btnRefuse") as Button;
                        if (btnRefuse != null)
                            btnRefuse.IsEnabled = false;
                        Button btnCommit = _myWindow.FindName("btnCommit") as Button;
                        if (btnCommit != null)
                            btnCommit.IsEnabled = false;
                    }
                }
            }
            catch (Exception otherEx)
            {
                LogHandler.Error(otherEx);
            }
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="own"></param>
        /// <param name="iocOrder"></param>
        /// <param name="partyOrder"></param>
        public void Init(Window own, Order iocOrder, Order partyOrder)
        {
            _myWindow = own;
            _myWindow.Unloaded += _myWindow_Unloaded;

            if("1" == partyOrder.WholeFlag)
            {
                FirmGridEnable = false;
            }
            GoodCode = iocOrder.Symbol;
            if (iocOrder.Side.Equals("1") || iocOrder.Side.ToUpper() == "BUY")
            {
                Side = "买入";
                CounterSubPartySideAmount = string.Format("Ofr {0}万/{1}万", iocOrder.Surplus, iocOrder.Qty);
                CounterSubSideAmount = string.Format("Bid {0}万/{1}万", partyOrder.Surplus, partyOrder.Qty);
                CounterSubPartySideAmountBrush = PriceBrush = (Brush)_myWindow.FindResource("ColorBrush.Orange5");
                CounterSubSideAmountBrush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
            }
            else
            {
                Side = "卖出";
                CounterSubPartySideAmount = string.Format("Bid {0}万/{1}万", iocOrder.Surplus, iocOrder.Qty);
                CounterSubSideAmount = string.Format("Ofr {0}万/{1}万", partyOrder.Surplus, partyOrder.Qty);
                CounterSubSideAmountBrush = PriceBrush = (Brush)_myWindow.FindResource("ColorBrush.Orange5");
                CounterSubPartySideAmountBrush = PriceBrush = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
            }

            Price = string.Format("@{0}", iocOrder.Price.ToString("N4"));
            Amount = iocOrder.Surplus > partyOrder.Surplus ? partyOrder.Surplus.ToString(): iocOrder.Surplus.ToString();

            LimitAmount = iocOrder.Surplus;
            
            CounterSubPartyName = string.Format("{0}-{1}", iocOrder.RoleName, iocOrder.Partysubname);
            if (!string.IsNullOrEmpty(partyOrder.InstitutionName))
                CounterPartyName = string.Format("[{0}]", partyOrder.InstitutionName);
            else
                CounterPartyName = partyOrder.InstitutionName;

            _strategyOrderId = iocOrder.OrderId;

            _oldStrategyOrderId = partyOrder.OrderId;


            DateTime transactTime = CommonUtils.ConvertToDateTime(iocOrder.TransactTime);
            TimeSpan diffTime = DateTimeEx.ServerTime - transactTime;

            double d = 0;
            if (double.TryParse(iocOrder.IocTimeOut, out d))
            {
                double result = d - diffTime.TotalSeconds;
                _iocTimeOut = result>=0?result.ToString("N0"):"0";
            }

            OutTime = string.Format("{0}", _iocTimeOut);
            
            firmToNoTimer.Start();

            TBookHandler.Instance.Init(Instance_OnOrder, null);
        }

        /// <summary>
        /// 卸载界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _myWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _myWindow.Unloaded -= _myWindow_Unloaded;
            TBookHandler.Instance.UnSubscribeOrder(Instance_OnOrder);
        }

        /// <summary>
        /// 订单回调数据处理
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                {
                    if (!string.IsNullOrEmpty(obj.OrderID) && !string.IsNullOrEmpty(_oldStrategyOrderId) && obj.OrderID.Equals(_oldStrategyOrderId) && obj.ExecType == "4")
                    {
                        LogHelper.Info(string.Format("NonfirmView OrderID:{0}, execType:{1}, oldStrategyOrderId:{2}", obj.OrderID, obj.ExecType, _oldStrategyOrderId));

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CancelClick();
                        });
                    }
                }));
            });
          
        }
    }
}
