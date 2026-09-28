using STC.Common.Utils;
using System;
using System.Windows;
using System.Windows.Input;

namespace STC.Common.View.StrongNotify
{
    /// <summary>
    /// NoBorderWin.xaml 的交互逻辑
    /// </summary>
    public partial class StrongNoBorderWin : Window
    {
        public Window myWindow;
        public StrongNoBorderWin()
        {
            InitializeComponent();
            Icon = LocalEnv.ServerInfo.AppImage;
            myWindow = this;
            Loaded += PopupNoBorder_Loaded;
            Unloaded += PopupNoBorder_Unloaded;
        }

        /// <summary>
        /// 原始控件的宽
        /// </summary>
        public double oriWidth;

        /// <summary>
        /// 原始控件的高
        /// </summary>
        public double oriHeight;

        /// <summary>
        /// * 添加移除Content  用于传递控件的引用
        /// </summary>
        /// <param name="element"></param>
        public UIElement ControlToMove
        {
            get
            {
                return (UIElement)MainFrame.Content;
            }

            set 
            {
                if (value != null)
                {
                    MainFrame.Content = value;
                    this.oriWidth = double.Parse(StrongNotifyHandler.Instance.GetPropertyValueFromControl(value, "ActualWidth"));
                    this.Title = StrongNotifyHandler.Instance.GetPropertyValueFromControl(value, "WinTitle");
                    this.Width = this.oriWidth + 4;
                }
                else
                {
                    MainFrame.Content = value;
                }
            }
        }

        /// <summary>
        /// * 设置Content 
        /// </summary>
        /// <param name="obj"></param>
        public void SetControlToMove(object obj)
        {
            this.ControlToMove = (UIElement)obj;
        }

        /// <summary>
        /// 设置ReSize，false 不允许改变窗口的大小, true 允许改变大小
        /// </summary>
        /// <param name="enable"></param>
        public void EnableReSize(bool enable = true)
        {
            if (enable)
            {
                SizeChanged += (senderArgs, eventArgs) =>
                {
                    StrongNotifyHandler.Instance.SetPropertyValueFromControl(ControlToMove, "Width", Width - 4);
                };
            }
            else
            {
                this.ResizeMode = ResizeMode.NoResize;
            }
        }
        
        public event Action OnReturnRequested;

        /// <summary>
        /// 移除Content
        /// </summary>
        private void OnReturnControlClick(object sender, RoutedEventArgs e)
        {
            StrongNotifyHandler.Instance.InvokeMethodFromControl(ControlToMove, "SetPopVisible", new object[] { });
            StrongNotifyHandler.Instance.SetPropertyValueFromControl(ControlToMove, "Width", oriWidth);
            OnReturnRequested?.Invoke(); // 通知主窗口返回控件
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                this.Close();
            }
        }

        #region 弹出 拖拽 
        private void PopupNoBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            RemoveWindowsEvent();
        }

        private void PopupNoBorder_Loaded(object sender, RoutedEventArgs e)
        {
            AddWindowsEvent();
        }


        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    StrongNoBorderWin po = (StrongNoBorderWin)sender;
                    var p = e.GetPosition(po);
                    if (p.Y > 0 && p.Y <= 30)
                    {
                        this.DragMove();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                catch { }
            }));
        }


        private void AddWindowsEvent()
        {
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void RemoveWindowsEvent()
        {
            this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
        }
        #endregion
    }
}
