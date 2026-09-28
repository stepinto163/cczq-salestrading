using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using STC.Common.View.Drp.Utils;
using STC.Common.ViewModel.Drp;
using STC.Common.Model.Drp;
using System.Text.RegularExpressions;
using System.Threading;
using STC.Common.Utils;

namespace STC.Common.View.Drp.Component
{
    public partial class LastWorkDayUnRefView : UserControl
    {
        public LastWorkDayUnRefView()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
        }

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        public bool IsClosing { get; set; }

        public string MsgId { get; set; }

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LastWorkDayUnRefView));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LastWorkDayUnRefView));

        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent(
        "NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LastWorkDayUnRefView));

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
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(LastWorkDayUnRefView), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

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
                    var notification = VisualTreeHelperExtensions.GetParent<LastWorkDayUnRefView>(button);
                    notification?.Close();
                };
            }
        }

        public override void OnApplyTemplate()
        {
            try
            {
                base.OnApplyTemplate();

                PART_CloseButton.Click += OnCloseButtonOnClick;

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
            if (this.DataContext != null && this.DataContext is LastWorkDayUnRefViewModel)
            {
                (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
            }
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is LastWorkDayUnRefViewModel)
            {
                (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
            }
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
                    OFR_CA.Completed += (s, e1) =>
                    {
                        (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
                    };

                    //(this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    try
                    {
                        (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.MainWindow.FindResource("MenuItem.MouseOver.Foreground").ToString()));
                    }
                    catch
                    {
                        (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    }

                    (this.DataContext as LastWorkDayUnRefViewModel).BorderBrush.BeginAnimation(Border.OpacityProperty, OFR_CA);
                });
            });
        }
    }
}
