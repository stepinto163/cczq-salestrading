using System;
using System.Windows;

namespace STC.Common.View.StrongNotify
{
    public interface IStrongNotifyInterface
    {
        ////例子代码参考 TestMessageControl.xaml.cs,界面参考 TestMessageControl.xaml
        ////需要在 UserControl的构造函数中增加下面的代码
        ///NotificationNewMsg += NotificationWindow_NotificationNewMsg;

        /// <summary>
        /// 关闭程序 //注意 typeof 要改为 UserControl的名字
        /// <code>
        ///public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent("NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        ///public event RoutedEventHandler NotificationClosed
        ///{
        ///   add { AddHandler(NotificationClosedEvent, value); }
        ///   remove { RemoveHandler(NotificationClosedEvent, value); }
        ///}
        /// </code>
        /// </summary>
        event RoutedEventHandler NotificationClosed;

        /// <summary>
        /// 弹框  注意 typeof 要改为 UserControl的名字
        /// <code>
        /// public static readonly RoutedEvent NotificationPopupEvent = EventManager.RegisterRoutedEvent("NotificationPopup", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        ///public event RoutedEventHandler NotificationPopup
        ///{
        ///    add { AddHandler(NotificationPopupEvent, value); }
        ///    remove { RemoveHandler(NotificationPopupEvent, value); }
        ///}
        /// </code>
        /// </summary>
        event RoutedEventHandler NotificationPopup;

        /// <summary>
        /// 更新提醒 注意 typeof 要改为 UserControl的名字
        /// <code>
        ///public static readonly RoutedEvent NotificationNewMsgEvent = EventManager.RegisterRoutedEvent("NotificationNewMsg", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TestMessageControl));
        ///public event RoutedEventHandler NotificationNewMsg
        ///{
        ///    add { AddHandler(NotificationNewMsgEvent, value); }
        ///    remove { RemoveHandler(NotificationNewMsgEvent, value); }
        ///}
        /// </code>
        /// </summary>
        event RoutedEventHandler NotificationNewMsg;

        /// <summary>
        /// 关闭程序是 外部函数调用方法
        /// <code>
        /// public event EventHandler Closed;
        /// public bool IsClosing { get; set; }
        /// private void OnCloseButtonOnClick(object sender, RoutedEventArgs args)
        /// {
        ///    if (IsClosing)
        ///    {
        ///        return;
        ///    }
        ///    IsClosing = true;
        ///    // 触发关闭事件
        ///    Closed?.Invoke(this, EventArgs.Empty);
        ///    // 如果在独立窗口中，关闭窗口
        ///    if (hostWindow != null)
        ///    {
        ///        hostWindow.Close();
        ///    }
        ///    RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        /// }
        /// </code>
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// 关闭程序是 外部函数调用方法
        /// <code>
        ///public void Close()
        ///{
        ///    OnCloseButtonOnClick(null, null);
        ///}
        /// </code>
        /// </summary>
        void Close();


        /// <summary>
        /// 数据刷新时，外部调用显示提醒效果
        /// <code>
        /// public void ShowNewMsgAlert()
        /// {
        ///    if (IsClosing)
        ///    {
        ///        return;
        ///    }
        ///    RaiseEvent(new RoutedEventArgs(NotificationNewMsgEvent));
        /// }
        /// </code>
        /// </summary>
        void ShowNewMsgAlert();

        #region 提醒代码
        // private void NotificationWindow_NotificationNewMsg(object sender, RoutedEventArgs e)
        // {
        //    Task.Factory.StartNew(() =>
        //    {
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            if (MainBorder != null && MainBorder.IsLoaded)
        //            {
        //                ColorAnimation OFR_CA = new ColorAnimation();
        //                OFR_CA.From = (Color)ColorConverter.ConvertFromString("#00CDAD");
        //                OFR_CA.To = (Color)ColorConverter.ConvertFromString("#1000CDAD");
        //                OFR_CA.AutoReverse = false;
        //                OFR_CA.RepeatBehavior = new RepeatBehavior(3);
        //                OFR_CA.Duration = TimeSpan.FromMilliseconds(500);
        //                OFR_CA.FillBehavior = FillBehavior.Stop;
        //                var storyboard = new Storyboard();
        //                storyboard.Children.Add(OFR_CA);
        //                Storyboard.SetTarget(OFR_CA, MainBorder);
        //                Storyboard.SetTargetProperty(OFR_CA, new PropertyPath("BorderBrush.Color"));
        //                // 注册动画完成事件
        //                storyboard.Completed += (s, el) =>
        //                {
        //                    // 移除动画
        //                    MainBorder.BeginAnimation(Border.BorderBrushProperty, null);
        //                    // 恢复控件原始样式
        //                    if (MainBorder.Style != null)
        //                    {
        //                        MainBorder.Style = MainBorder.Style; // 重新应用样式
        //                    }
        //                };
        //                storyboard.Begin();
        //            }
        //        });
        //    });
        // }
        #endregion

        /// <summary>
        /// 所有强提醒窗口的唯一ID
        /// <code>
        /// private string msgid = string.Empty;
        /// public string MsgId
        ///{
        ///    get
        ///    {
        ///        return msgid;
        ///    }
        ///    set
        ///    {
        ///        msgid = value;
        ///    }
        /// }
        /// </code>
        /// </summary>
        string MsgId { get; set; }

        /// <summary>
        /// 弹出后显示的windows的标题
        /// <code>
        /// public string wintitle = string.Empty;
        /// public string WinTitle
        /// {
        ///    get
        ///    {
        ///        return wintitle;
        ///    }
        ///    set
        ///    {
        ///        wintitle = value;
        ///        Title.Text = wintitle;
        ///    }
        /// }
        /// </code>
        /// </summary>
        string WinTitle { get; set; }

        /// <summary>
        /// 设置父窗口windows对象
        /// <code>
        ///public StrongNoBorderWin hostWindow; // 独立窗口引用
        ///public void SetHostWindow(StrongNoBorderWin obj)
        ///{
        ///    btnPop.Visibility = Visibility.Collapsed;
        ///    btnPop.IsChecked = true;
        ///    this.hostWindow = obj;
        ///}
        /// </code>
        /// <param name="obj"></param>
        /// </summary>
        void SetHostWindow(StrongNoBorderWin obj);

        /// <summary>
        /// 设置弹出按钮可见
        /// <code>
        ///public void SetPopVisible()
        ///{
        ///    btnPop.Visibility = Visibility.Visible;
        ///}
        /// </code>
        /// </summary>
        void SetPopVisible();

        /// <summary>
        /// 返回触发事件
        /// <code>
        ///public void OnReturnControlClick(object sender, RoutedEventArgs e)
        ///{
        ///    RaiseEvent(new RoutedEventArgs(NotificationPopupEvent, sender));
        ///}
        /// </code>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// </summary>
        void OnReturnControlClick(object sender, RoutedEventArgs e);


        #region 拖拽效果
        //private void AttachEvents()
        //{
        //    Border_Left1.PreviewMouseMove += Border_Left1_PreviewMouseMove;
        //}

        //private void Border_Left1_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    if (Mouse.LeftButton != MouseButtonState.Pressed)
        //        return;

        //    try
        //    {
        //        var pos = e.GetPosition(Border_Left1);
        //        if (pos.Y > 0 && pos.Y <= 28 && pos.X < this.ActualWidth - 48)
        //        {
        //            HitTestResult result = VisualTreeHelper.HitTest(Border_Left1, pos);
        //            if (result == null)
        //                return;
        //            //注意：以下代码请勿轻易修改,尤其是 Tag, OnReturnControlClick方法的调用和方法
        //            PanleDragDropAdorner adorner = new PanleDragDropAdorner(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
        //            AdornerLayer mAdornerLayer = AdornerLayer.GetAdornerLayer(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
        //            mAdornerLayer.Add(adorner);

        //            System.Windows.DragDrop.DoDragDrop(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel(), this, System.Windows.DragDropEffects.Move);

        //            OnReturnControlClick(sender, e);
        //        }
        //        else
        //        {
        //            e.Handled = true;
        //        }
        //    }
        //    catch { }
        //}

        #endregion
    }
}
