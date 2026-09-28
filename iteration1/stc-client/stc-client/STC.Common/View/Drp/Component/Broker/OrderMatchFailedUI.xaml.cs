using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp.Utils;
using STC.Common.ViewModel.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// OrderMatchFailedUI.xaml 的交互逻辑
    /// </summary>
    public partial class OrderMatchFailedUI : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        //public ExecutionReport orderPo = null;

        public OrderMatchFailedUIViewModel viewModel;
        public OrderMatchFailedUI()
        {
            InitializeComponent();
            viewModel = new OrderMatchFailedUIViewModel();
            this.DataContext = viewModel; 
            //CommonUtils.SetIcon(TitleIcon);
            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
            //this.Loaded += OrderMatchFailedUI_Loaded;
        }

        public void SetShowData(ExecutionReport po, string msgid, OrderMatchFailedUI ui)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.MsgId = msgid;
                    viewModel.ParentWindow = ui;
                    viewModel.BindingData(po);
                });
            });
        }

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        public bool IsClosing { get; set; }

        public string MsgId { get; set; }

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrderMatchFailedUI));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrderMatchFailedUI));

        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent(
        "NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrderMatchFailedUI));

        public event RoutedEventHandler NotificationCloseInvoked
        {
            add { AddHandler(NotificationCloseInvokedEvent, value); }
            remove { RemoveHandler(NotificationCloseInvokedEvent, value); }
        }

        public event RoutedEventHandler NotificationClosed
        {
            add { AddHandler(NotificationClosedEvent, value); }
            remove { RemoveHandler(NotificationClosedEvent, value); }
        }

        public event RoutedEventHandler NotificationNewMsg
        {
            add { AddHandler(NotificationNewMsgEvent, value); }
            remove { RemoveHandler(NotificationNewMsgEvent, value); }
        }

        public static bool GetCloseOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseOnClickProperty);
        }

        public static void SetCloseOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseOnClickProperty, value);
        }

        public static readonly DependencyProperty CloseOnClickProperty =
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(OrderMatchFailedUI), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

        private static void CloseOnClickChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var button = dependencyObject as Button;
            if (button == null)
            {
                return;
            }

            var value = (bool)dependencyPropertyChangedEventArgs.NewValue;

            if (value)
            {
                button.Click += (sender, args) =>
                {
                    var notification = VisualTreeHelperExtensions.GetParent<OrderMatchFailedUI>(button);
                    notification?.Close();
                };
            }
        }

        public override void OnApplyTemplate()
        {
            try
            {
                base.OnApplyTemplate();

                btnClose.Click += OnCloseButtonOnClick;

                var storyboards = Template.Triggers.OfType<EventTrigger>().FirstOrDefault(t => t.RoutedEvent == NotificationCloseInvokedEvent)?.Actions.OfType<BeginStoryboard>().Select(a => a.Storyboard);
                _closingAnimationTime = new TimeSpan(storyboards?.Max(s => Math.Min((s.Duration.HasTimeSpan ? s.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero) : TimeSpan.MaxValue).Ticks, s.Children.Select(ch => ch.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero)).Max().Ticks)) ?? 0);
            }
            catch (Exception ex)
            {

            }
        }
        private void OnCloseButtonOnClick(object sender, RoutedEventArgs args)
        {
            var button = sender as Button;
            if (button == null) return;

            button.Click -= OnCloseButtonOnClick;
            Close();
        }

        //TODO: .NET40
        public async void Close()
        {
            if (IsClosing)
            {
                return;
            }

            IsClosing = true;

            RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
            await Task.Delay(_closingAnimationTime);
            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*if (this.DataContext != null && this.DataContext is MMBreakAlarmReminderViewModel)
            {
                (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
            }*/
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*if (this.DataContext != null && this.DataContext is MMBreakAlarmReminderViewModel)
            {
                (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
            }*/
        }
        public void ShowNewMsgAlert()
        {
            if (IsClosing)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(NotificationNewMsgEvent));
        }

        private void NotificationWindow_NotificationNewMsg(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DoubleAnimation OFR_CA = new DoubleAnimation();
                    OFR_CA.From = 1;
                    OFR_CA.To = 0.3;
                    OFR_CA.AutoReverse = false;
                    OFR_CA.RepeatBehavior = new RepeatBehavior(1);
                    OFR_CA.Duration = new Duration(TimeSpan.FromSeconds(3));
                    /*OFR_CA.Completed += (s, e1) =>
                    {
                        (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
                    };

                    //(this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    try
                    {
                        (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.MainWindow.FindResource("MenuItem.MouseOver.Foreground").ToString()));
                    }
                    catch
                    {
                        (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    }

                    (this.DataContext as MMBreakAlarmReminderViewModel).BorderBrush.BeginAnimation(Border.OpacityProperty, OFR_CA);*/
                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window win = this.Parent as Window;
            if (win != null)
            {
                win.Close();
            }
        }

        /*private void OrderMatchFailedUI_Loaded(object sender, RoutedEventArgs e)
        {*/
        public void setOrderMatchFailed() { 
            //if (orderPo != null)
            //{
            //    tbGoodsCode.Text = orderPo.SecurityID ;//orderPo.goodsCode;
            //    if (!string.IsNullOrEmpty(orderPo.UpdateTime))
            //        tbUpdateTime.Text = CommonUtils.ConvertToDateTime(orderPo.UpdateTime).ToString("HH:mm:ss");

            //    if (!string.IsNullOrWhiteSpace(orderPo.TraderName))
            //    {
            //        tbBTraderInfo.Text = string.Format("{0}({1})", orderPo.InstitutionName, orderPo.TraderName);
            //    }
            //    else
            //    {
            //        tbBTraderInfo.Text = string.Format("");
            //    }
            //    tbABroker.Content = orderPo.Name;
            //    if (LocalEnv.UserInfo.IsMyself(orderPo.UserID))
            //    {
            //        tbABroker.Style = (Style)FindResource("SelfBrokerLableStyle");
            //    }
            //    else
            //    {
            //        tbABroker.Style = (Style)FindResource("OtherBrokerLableStyle");
            //    }

            //    tbAmount.Text = orderPo.OrderQty + "";
            //    tbPrice.Text = orderPo.Price;

            //    tbSide.Text = orderPo.Side.Equals("1") ? "GVN" : "TKN";
            //    if (orderPo.Side.Equals("1"))
            //    {
            //        tbPrice.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9200"));//(Brush)FindResource("scbBidPrice");
            //        tbSide.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9200"));//(Brush)FindResource("scbBidPrice");
            //    }
            //    else
            //    {
            //        tbPrice.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12A1FF"));// (Brush)FindResource("scbOfferPrice");
            //        tbSide.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12A1FF")); //(Brush)FindResource("scbOfferPrice");
            //    }

            //    tbSettle.Text =  ComboSourceUtil.Instance.FindEnum(orderPo.SettlType, "settletype_drp"); 

            //    //unMatchReason.orderPo = orderPo;
            //    //unMatchReason.RefreshForm();
            //}
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.Dr
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
        }
        //private string convertSettlType() { }
    }
}
