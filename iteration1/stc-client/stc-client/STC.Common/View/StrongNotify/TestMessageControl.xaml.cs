using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Common.View.StrongNotify
{
    /// <summary>
    /// MessageControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestMessageControl : UserControl, IStrongNotifyInterface
    {
        public TestMessageControl()
        {
            InitializeComponent();
            btn_Cancel.Click += Btn_Cancel_Click;

            NotificationNewMsg += NotificationWindow_NotificationNewMsg;
            AttachEvents();
        }

        #region 依赖 DependencyProperty

        //注意 typeof 要改为 UserControl的名字
        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent("NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        /// <summary>
        /// 关闭程序
        /// </summary>
        public event RoutedEventHandler NotificationClosed
        {
            add { AddHandler(NotificationClosedEvent, value); }
            remove { RemoveHandler(NotificationClosedEvent, value); }
        }

        //注意 typeof 要改为 UserControl的名字
        public static readonly RoutedEvent NotificationPopupEvent = EventManager.RegisterRoutedEvent("NotificationPopup", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        /// <summary>
        /// 弹框
        /// </summary>
        public event RoutedEventHandler NotificationPopup
        {
            add { AddHandler(NotificationPopupEvent, value); }
            remove { RemoveHandler(NotificationPopupEvent, value); }
        }


        //注意 typeof 要改为 UserControl的名字
        public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent("NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        /// <summary>
        /// 更新提醒
        /// </summary>
        public event RoutedEventHandler NotificationNewMsg
        {
            add { AddHandler(NotificationNewMsgEvent, value); }
            remove { RemoveHandler(NotificationNewMsgEvent, value); }
        }
        #endregion

        #region 关闭程序,不要改
        private void OnCloseButtonOnClick(object sender, RoutedEventArgs args)
        {
            if (IsClosing)
            {
                return;
            }

            IsClosing = true;
            
            // 触发关闭事件
            Closed?.Invoke(this, EventArgs.Empty);

            // 如果在独立窗口中，关闭窗口
            if (hostWindow != null)
            {
                hostWindow.Close();
            }

            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }

        /// <summary>
        /// 关闭程序是 外部函数调用方法
        /// </summary>
        public void Close()
        {
            OnCloseButtonOnClick(null, null);
        }

        #endregion

        #region 更新提醒

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
                    if (MainBorder != null && MainBorder.IsLoaded)
                    {
                        ColorAnimation OFR_CA = new ColorAnimation();
                        OFR_CA.From = (Color)ColorConverter.ConvertFromString("#00CDAD");
                        OFR_CA.To = (Color)ColorConverter.ConvertFromString("#1000CDAD");
                        OFR_CA.AutoReverse = false;
                        OFR_CA.RepeatBehavior = new RepeatBehavior(3);
                        OFR_CA.Duration = TimeSpan.FromMilliseconds(500);
                        OFR_CA.FillBehavior = FillBehavior.Stop;
                        var storyboard = new Storyboard();
                        storyboard.Children.Add(OFR_CA);
                        Storyboard.SetTarget(OFR_CA, MainBorder);
                        Storyboard.SetTargetProperty(OFR_CA, new PropertyPath("BorderBrush.Color"));
                        // 注册动画完成事件
                        storyboard.Completed += (s, el) =>
                        {
                            // 移除动画
                            MainBorder.BeginAnimation(Border.BorderBrushProperty, null);
                            // 恢复控件原始样式
                            if (MainBorder.Style != null)
                            {
                                MainBorder.Style = MainBorder.Style; // 重新应用样式
                            }
                        };
                        storyboard.Begin();
                    }
                });
            });
        }
        #endregion

        #region 弹出和关闭程序
        public event EventHandler Closed;

        public bool IsClosing { get; set; }

        private string msgid = string.Empty;
        /// <summary>
        /// 所有强提醒窗口的唯一ID
        /// </summary>
        public string MsgId
        {
            get
            {
                return msgid;
            }
            set
            {
                msgid = value;
            }
        }

        public string wintitle = string.Empty;
        /// <summary>
        /// 弹出后显示的windows的标题
        /// </summary>
        public string WinTitle
        {
            get
            {
                return wintitle;
            }
            set
            {
                wintitle = value;
                Title.Text = wintitle;
            }
        }

        public StrongNoBorderWin hostWindow; // 独立窗口引用

        /// <summary>
        /// 设置父窗口windows对象
        /// </summary>
        /// <param name="obj"></param>
        public void SetHostWindow(StrongNoBorderWin obj)
        {
            btnPop.Visibility = Visibility.Collapsed;
            btnPop.IsChecked = true;
            this.hostWindow = obj;
        }

        /// <summary>
        /// 设置弹出按钮可见
        /// </summary>
        public void SetPopVisible()
        {
            btnPop.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 返回触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnReturnControlClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(NotificationPopupEvent, sender));
        }

        #endregion

        #region 拖拽效果
       
        private void AttachEvents()
        {
            Border_Left1.PreviewMouseMove += Border_Left1_PreviewMouseMove;
        }

        private void Border_Left1_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            try
            {
                var pos = e.GetPosition(Border_Left1);
                if (pos.Y > 0 && pos.Y <= 28 && pos.X < this.ActualWidth - 48)
                {
                    HitTestResult result = VisualTreeHelper.HitTest(Border_Left1, pos);
                    if (result == null)
                        return;
                    //注意：以下代码请勿轻易修改,尤其是 Tag, OnReturnControlClick方法的调用和方法
                    PanleDragDropAdorner adorner = new PanleDragDropAdorner(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
                    AdornerLayer mAdornerLayer = AdornerLayer.GetAdornerLayer(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
                    mAdornerLayer.Add(adorner);

                    System.Windows.DragDrop.DoDragDrop(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel(), this, System.Windows.DragDropEffects.Move);

                    OnReturnControlClick(sender, e);
                }
                else
                {
                    e.Handled = true;
                }
            }
            catch { }
        }

        #endregion

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
