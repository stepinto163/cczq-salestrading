using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.WorkTable.Plugin.ViewModel;
using STC.WorkTable.Plugin.ViewModel.Data;
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

namespace STC.WorkTable.Plugin.View
{
    /// <summary>
    /// SendPriceWarn.xaml 的交互逻辑
    /// </summary>
    public partial class SendPriceIntentionWarnView : Window
    {
        private SendPriceIntentionWarnViewModel viewModel = null;
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
                if (sub != null)
                {
                    foreach (Delegate d in sub)
                        OnAction += (Action<object, bool, bool>)d;
                }
            }
            catch (Exception e)
            {

            }
        }

        public SendPriceIntentionWarnView()
        {
            InitializeComponent();

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
        public SendPriceIntentionWarnView(List<TPlaceOrderResult> placeOrderResult, IntentionOrder order)
        {
            InitializeComponent();

            this.Closed += SendPriceWarnView_Closed;

            viewModel = new SendPriceIntentionWarnViewModel();
            this.DataContext = viewModel;
            viewModel.Init(this, order, placeOrderResult);

        }

        //public SendPriceWarnView(TPlaceOrderResult placeOrderResult, NewOrderSingle order)
        //{
        //    InitializeComponent();

        //    CommonUtils.SetIcon(this);
        //    viewModel = new SendPriceWarnViewModel();
        //    this.DataContext = viewModel;
        //    viewModel.Init(this, order, placeOrderResult);

        //}


        public SendPriceIntentionWarnView(ConfirmIntentionQuoteReplyPo resp, IntentionOrder order)
        {
            InitializeComponent();

            this.Closed += SendPriceWarnView_Closed;

            if (resp.IsSuccess())
            {
                this.Title = "下单提示";
                _isSuccess = true;
            }
            viewModel = new SendPriceIntentionWarnViewModel();
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
    }
}
