using com.gateway.log;
using STC.Common.Utils;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace STC.Common.View.StrongNotify
{
    /// <summary>
    /// StrongNotifyMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StrongNotifyMainWindow : Window
    {
        private bool _init = false;
        public StrongNotifyMainWindow()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            this.Closing += StrongNotifyMainWindow_Closing;
            this.CustomPanel.ChildrenChanged += CustomPanel_ChildrenChanged;
            
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// 设置强提醒容器准备好，准备好后启动监听屏幕缩放功能
        /// </summary>
        public void SetIsReady()
        {
            if (!_init)
            {
                _init = true;
                this.SizeChanged += StrongNotifyMainWindow_SizeChanged;
                this.LocationChanged += StrongNotifyMainWindow_LocationChanged;
                //此事件能解决frmaework4.0无法检测到2k以上显示器dpi比起始值小时检测问题，但性能开销大取消该功能，用户可以拖动主程序来位置解决问题
                //this.LayoutUpdated += StrongNotifyMainWindow_LayoutUpdated;
            }
        }


        public RightToLeftBottomToTopPanel GetCustomPanel()
        {
            return CustomPanel;
        }


        #region 消息管理器标题修改
        private void CustomPanel_ChildrenChanged(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                this.Title = $"消息管理器 [{CustomPanel.Children.Count}]";
            }));
        }
        #endregion

        #region 程序重启或退出
        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "STC_RestartApp")
            {
                //更新组
                try
                {
                    this.Closing -= StrongNotifyMainWindow_Closing;
                    this.Close();
                }
                catch { }
            }
        }
        #endregion

        #region Close Size 事件拦截
        /// <summary>
        /// ALT+F4用户强制关闭窗口拦截
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StrongNotifyMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// 间接检查缩放比例变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StrongNotifyMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_init)
            {
                this.SizeChanged -= StrongNotifyMainWindow_SizeChanged;
                this.LocationChanged -= StrongNotifyMainWindow_LocationChanged;
                LogHelper.Info($"当前屏幕缩放发生变化！");
                ReSetWindowsSize();
                this.SizeChanged += StrongNotifyMainWindow_SizeChanged;
                this.LocationChanged += StrongNotifyMainWindow_LocationChanged;

            }
        }
        /// <summary>
        /// 间接检查缩放或位置变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StrongNotifyMainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (_init)
            {
                this.SizeChanged -= StrongNotifyMainWindow_SizeChanged;
                this.LocationChanged -= StrongNotifyMainWindow_LocationChanged;
                LogHelper.Info($"当前屏幕位置发生变化！");
                ReSetWindowsSize();
                this.SizeChanged += StrongNotifyMainWindow_SizeChanged;
                this.LocationChanged += StrongNotifyMainWindow_LocationChanged;

            }
        }

        /// <summary>
        /// 当屏幕缩放比例变化时重新设置容器
        /// </summary>
        public void ReSetWindowsSize()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    System.Windows.Forms.Screen mainScreen = StrongNotifyHandler.Instance.ManagerInstance.GetMainScreen();
                    foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                    {
                        if (sc.DeviceName.Replace("\\", "").Replace(".", "") == mainScreen.DeviceName.Replace("\\", "").Replace(".", ""))
                        {
                            var window = Window.GetWindow(this);//获取当前主窗口
                            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                            Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                            PresentationSource source = PresentationSource.FromVisual(window);

                            Matrix m = source.CompositionTarget.TransformFromDevice;

                            double widthRatio = m.M11;

                            double heightRatio = m.M22;

                            //this.MaxWidth = screen.WorkingArea.Width;
                            //this.MaxHeight = screen.WorkingArea.Height;
                            this.Width = screen.WorkingArea.Width * widthRatio;
                            this.Height = screen.WorkingArea.Height * heightRatio;
                            this.Left = rect.Left == 0 ? 0 : rect.Left * widthRatio;
                            this.Top = rect.Top == 0 ? 0 : rect.Top * heightRatio;
                            break;
                        }
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        #endregion


    }
}
