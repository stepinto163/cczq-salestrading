using STC.Common.Handler.PT;
using STC.Common.IDCEnum;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp.Utils;
using STC.Common.ViewModel.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// UnderlineExecTpicUI.xaml 的交互逻辑
    /// </summary>
    public partial class UnderlineExecTpicUI : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public UnderlineExecTpicUIViewModel viewModel;
        public UnderlineExecTpicUI()
        {
            InitializeComponent();
            viewModel = new UnderlineExecTpicUIViewModel();
            this.DataContext = viewModel;
            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
            this.Loaded += UnderlineExecTpicUI_Loaded;
        }

        public void SetShowData(ExecutionReport po, string msgid, UnderlineExecTpicUI ui)
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
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UnderlineExecTpicUI));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UnderlineExecTpicUI));

        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent(
        "NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UnderlineExecTpicUI));

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
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(UnderlineExecTpicUI), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

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
                    var notification = VisualTreeHelperExtensions.GetParent<UnderlineExecTpicUI>(button);
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
                });
            });
        }
        private void UnderlineExecTpicUI_Loaded(object sender, RoutedEventArgs e)
        {
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
    }
}
