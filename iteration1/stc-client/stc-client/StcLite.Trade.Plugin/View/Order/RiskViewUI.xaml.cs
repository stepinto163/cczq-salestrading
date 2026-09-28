using BDB.Trade.Plugin.ViewModel.Order;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BDB.Trade.Plugin.View.Order
{
    /// <summary>
    /// SendPriceWarn.xaml 的交互逻辑
    /// </summary>
    public partial class RiskViewUI : Window
    {

        private RiskViewUIViewModel viewModel = null;
        private bool _isSuccess = false;

        public event Action<object, bool, bool> OnAction;
        private bool _needDoubleCheck = false;

        public void OnClose(object sender, bool isCancel, bool isDoubleCheck)
        {
            _needDoubleCheck = isDoubleCheck;
            OnAction?.Invoke(sender, isCancel, isDoubleCheck);
        }

        public Delegate[] GetSubscribe()
        {
            return OnAction != null ? OnAction.GetInvocationList() : null;
        }

        public void SetSubscribe(Delegate[] sub)
        {
            try
            {
                foreach (Delegate d in sub)
                    OnAction += (Action<object, bool, bool>)d;
            }
            catch (Exception e)
            {

            }
        }

        public RiskViewUI()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            this.Closed += SendPriceWarnView_Closed;
        }

        private void SendPriceWarnView_Closed(object sender, EventArgs e)
        {
            if (!_needDoubleCheck)
            {
                this.OnClose(this, true, false);

                if (this.Owner != null)
                    this.Owner.Focus();
            }

        }

        /// <summary>
        /// 2次警告
        /// </summary>
        /// <param name="placeOrderResult"></param>
        /// <param name="order"></param>
        public RiskViewUI(List<TPlaceOrderResult> placeOrderResult, NewOrderSingle order)
        {
            InitializeComponent();

            this.Closed += SendPriceWarnView_Closed;

            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new RiskViewUIViewModel();
            this.DataContext = viewModel;
            viewModel.Init(this, order, placeOrderResult);
        }

        /// <summary>
        /// 第一次风控，或成功提示，或字段检查
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="order"></param>
        public RiskViewUI(BaseReplyPo resp, NewOrderSingle order)
        {
            InitializeComponent();

            this.Closed += SendPriceWarnView_Closed;

            if (resp.IsSuccess())
            {
                this.Title = "下单提示";
                _isSuccess = true;
            }
            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new RiskViewUIViewModel();
            this.DataContext = viewModel;
            viewModel.Init(this, resp, order);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => {

                this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                    new Action(() => {
                        if (_isSuccess)
                        {
                            OnClose(this, false, false);
                        }
                    }));

                Thread.Sleep(2000);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => {
                        if (_isSuccess)
                        {
                            this.Close();
                            if (this.Owner != null)
                                this.Owner.Focus();
                        }
                    }));
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                    viewModel.OkCommand.Execute(null);
            }
        }
    }
}
