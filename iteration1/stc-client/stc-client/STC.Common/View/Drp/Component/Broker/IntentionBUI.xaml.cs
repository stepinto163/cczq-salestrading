using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Drp.Utils;
using STC.Common.ViewModel.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// IntentionBUI.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionBUI : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public IntentionBUIViewModel viewModel;
        public IntentionBUI mywin;
        public IntentionBUI()
        {
            InitializeComponent();
            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
            this.Loaded += IntentionBUI_Loaded;
            viewModel = new IntentionBUIViewModel();
            this.DataContext = viewModel;
        }

        public void InitData(List<ExecutionReport> po, IntentionBUI ui, string msgid)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.MsgId = msgid;
                viewModel.ParentWindow = ui;
                viewModel.MsgId = msgid;
                viewModel.InitData(po);
            });
        }

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        public bool IsClosing { get; set; }

        public string MsgId { get; set; }

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntentionBUI));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntentionBUI));

        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent(
        "NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntentionBUI));

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
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(IntentionBUI), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

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
                    var notification = VisualTreeHelperExtensions.GetParent<IntentionBUI>(button);
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

        private void IntentionBUI_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;
            decimal max = 2000.00M;
            decimal min = 0M;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            string key = tBox.Tag.ToString();
            if (e.KeyboardDevice.IsKeyDown(Key.Up))
            {
                viewModel.OnAddClick(key);
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Down))
            {
                viewModel.OnSubtractionClick(key);
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Escape))
            {
                viewModel.OnCloseSelf();
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                GrFeedBackViewModel dr = viewModel.FeedBackQuotations.FirstOrDefault(a => a.ExecID == (string)tBox.Tag);
                if (dr == null)
                {
                    e.Handled = true;
                    return;
                }

                if (dr.ButtonIsEnable == "1")
                {
                    viewModel.OnOkCommand(key);
                }

                e.Handled = true;
                return;
            }

            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }

        }


        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex(@"^[0-9]\d*$");
            if (LocalEnv.UserInfo.CompanyId == "4")
            {
                regex = new Regex("^[0-9.-]+$");
            }

            decimal max = 2000M;
            decimal min = 0M;

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;


            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }


            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

            }
            else
            {
                e.Handled = true;
                return;
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min) && d != 0)
            {
                e.Handled = true;
                return;
            }

            if (LocalEnv.UserInfo.CompanyId == "4")
            {
                string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }

            e.Handled = false;
        }

        private void ucIntentionBUI_KeyDown(object sender, KeyEventArgs e)
        {
            IntentionBUIViewModel viewModel = (IntentionBUIViewModel)this.DataContext;

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                if (viewModel != null )
                    viewModel.OnCloseSelf();
            }
        }

    }
}
