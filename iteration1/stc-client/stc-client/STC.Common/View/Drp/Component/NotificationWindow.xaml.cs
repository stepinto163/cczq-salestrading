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
    public partial class NotificationWindow : UserControl
    {

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public NotificationWindow()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
        }


        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        public bool IsClosing { get; set; }

        public string MsgId { get; set; }

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotificationWindow));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotificationWindow));

        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent(
         "NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotificationWindow));

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
            DependencyProperty.RegisterAttached("CloseOnClick", typeof(bool), typeof(NotificationWindow), new FrameworkPropertyMetadata(false, CloseOnClickChanged));

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
                    var notification = VisualTreeHelperExtensions.GetParent<NotificationWindow>(button);
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


        public void ShowNewMsgAlert()
        {
            if (IsClosing)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(NotificationNewMsgEvent));
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DRPNotificationViewModel)
            {
                (this.DataContext as DRPNotificationViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            }
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DRPNotificationViewModel)
            {
                (this.DataContext as DRPNotificationViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
            }
        }

        private void btnReply_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DRPNotificationViewModel)
            {
                DRPListModel ss = (this.DataContext as DRPNotificationViewModel).SelectDrp;
                if (ss != null)
                {
                    (this.DataContext as DRPNotificationViewModel).OnReplyCommand(ss.Id);
                }
            }
        }

        private void btnRefuse_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is DRPNotificationViewModel)
            {
                DRPListModel ss = (this.DataContext as DRPNotificationViewModel).SelectDrp;
                if (ss != null)
                {
                    (this.DataContext as DRPNotificationViewModel).OnRefuseCommand(ss.Id);
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "PriceTxt") || (tb.Name == "netPrice"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 10000000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
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

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }


        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                e.Handled = true;
                return;
            }

            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }

                // tag 改变用途
                string tag = string.Empty;

                // tag 改变用途
                if (tBox.Name == "AmountTxt")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "PriceTxt") || (tBox.Name == "netPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }

                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000000;
                    min = 1000;
                }
                else if (tag == "2")//价格
                {
                    max = 9999;
                    min = -100;
                }

                //if (decimal.TryParse(text, out d) && (d > max || d < min))
                //{
                //    e.Handled = true;
                //    return;
                //}
            }
        }


        private void PriceTxt_MouseEnter(object sender, MouseEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (!(this.DataContext as DRPNotificationViewModel).IsSetFocus)
                    {
                        Point p = Mouse.GetPosition(e.Source as FrameworkElement);
                        mouse_event(0x0002, int.Parse(p.X.ToString()), int.Parse(p.Y.ToString()), 0, 0);
                        Thread.Sleep(10);
                        mouse_event(0x0004, int.Parse(p.X.ToString()), int.Parse(p.Y.ToString()), 0, 0);
                        (this.DataContext as DRPNotificationViewModel).IsSetFocus = true;
                    }
                }
                catch { }
            });
        }

        private void AmountTxt_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!(this.DataContext as DRPNotificationViewModel).IsSetFocus)
                    {
                        Point p = Mouse.GetPosition(e.Source as FrameworkElement);
                        mouse_event(0x0002, int.Parse(p.X.ToString()), int.Parse(p.Y.ToString()), 0, 0);
                        Thread.Sleep(10);
                        mouse_event(0x0004, int.Parse(p.X.ToString()), int.Parse(p.Y.ToString()), 0, 0);
                        (this.DataContext as DRPNotificationViewModel).IsSetFocus = true;
                    }
                });
            }
            catch { }
        }

        /// <summary>
        /// 复制到剪贴板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrice_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    if (sender is TextBlock)
            //    {
            //        string id = (sender as TextBlock).Tag.ToString();
            //        (this.DataContext as DRPNotificationViewModel).SetClipboard(id);
            //    }
            //});
        }

        private void btnCopyMsg_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender is Button)
                {
                    string id = (sender as Button).Tag.ToString();
                    (this.DataContext as DRPNotificationViewModel).SetClipboard(id);
                }
            });
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
                    OFR_CA.RepeatBehavior = new RepeatBehavior(3);
                    OFR_CA.Duration = new Duration(TimeSpan.FromSeconds(1));
                    OFR_CA.Completed += (s, e1) =>
                    {
                        this.NewAlertBorder.Visibility = Visibility.Collapsed;
                    };
                    //this.NewAlertBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    try
                    {
                        this.NewAlertBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.MainWindow.FindResource("MenuItem.MouseOver.Foreground").ToString()));
                    }
                    catch
                    {
                        this.NewAlertBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    }


                    this.NewAlertBorder.Visibility = Visibility.Visible;
                    this.NewAlertBorder.BeginAnimation(Border.OpacityProperty, OFR_CA);
                });
            });

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DoubleAnimation OFR_Border = new DoubleAnimation();
                    OFR_Border.From = 1;
                    OFR_Border.To = 0.3;
                    OFR_Border.AutoReverse = false;
                    OFR_Border.RepeatBehavior = new RepeatBehavior(3);
                    OFR_Border.Duration = new Duration(TimeSpan.FromSeconds(1));
                    OFR_Border.Completed += (s, e1) =>
                    {
                        (this.DataContext as DRPNotificationViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
                    };

                    try
                    {
                        (this.DataContext as DRPNotificationViewModel).BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.MainWindow.FindResource("MenuItem.MouseOver.Foreground").ToString()));
                    }
                    catch 
                    {
                        (this.DataContext as DRPNotificationViewModel).BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0x00, 0xCD, 0xAD));
                    }

                    (this.DataContext as DRPNotificationViewModel).BorderBrush.BeginAnimation(Border.OpacityProperty, OFR_Border);
                });
            });
        }

        #region 显示弹出式报价跟踪
        /// <summary>
        /// 打开报价追踪
        /// </summary>
        //// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWarnPopup();
        }

        /// <summary>
        /// 关闭报价追踪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Expand_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!btn_Expand.IsMouseOver && !this.AnsAttention.IsMouseOver)
            {
                CollapsedWarnPopup();
            }
        }

        private void ShowWarnPopup()
        {
            MM_Duty_pop.Visibility = Visibility.Visible;
            MM_Duty_pop.IsOpen = true;
            AnsAttention.QueryLog((this.DataContext as DRPNotificationViewModel).QuoteID.Replace("DQLimit_", ""), "1");
        }

        private void CollapsedWarnPopup()
        {
            MM_Duty_pop.IsOpen = false;
            MM_Duty_pop.Visibility = Visibility.Collapsed;
        }

        private void Ans_status_tb_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapsedWarnPopup();
        }
        #endregion
    }
}
