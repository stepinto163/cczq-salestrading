using MDT.Tools.Wpf.AutoUpdater.DiyMessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using STC.Common.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;
using System.Windows.Media;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Handler.Base;
using System.Text;
using Dragablz;
using System.Runtime.InteropServices;
using WinInterop = System.Windows.Interop;
using STC.Common.Utils.Events;
using com.gateway.log;
using netapi.Api;
using com.gateway.connector;
using System.Windows.Interop;
using DataTimeSequence = STC.Common.Utils.DataTimeSequence;
using STC.Common.Model;
using STC.Frame.UI;
using STC.Frame.CustomUI;
using STC.Frame.Utils;
using STC.Common.Data;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.UI.Utils;
using STC.Frame.Update;
using System.IO;
using System.Reflection;

namespace STC.Frame
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window, IForm
    {
        private const string OpenPluginKey = "Plugin";
        private const string SelectedTagKey = "FocusPlugin";

        private const string Window_height = "Height";
        private const string Window_width = "Width";
        private const string Window_Left = "Left";
        private const string Window_Top = "Top";

        private readonly string BridgeWinName = "MainWindow";

        private bool isClosed = false;
        private int MaxWarningCount = 5;
        private int warningCount = 0;
        private int WarningCount
        {
            get { return warningCount; }
            set
            {
                warningCount = value;
                LogHelper.Info("reconnect count:{0}", warningCount);
            }
        }
        private SysWarnGUI sysWarnGUI = null;
        private double defaultWidth = 0;

        Window LoginWin;

        public Window2(Window w)
        {
            InitializeComponent();
            //Initialize();
            LoginWin = w;
            this.Name = "MainWindow";
            sysWarnGUI = new SysWarnGUI();
            sysWarnGUI.MClosed += SysWarnGUI_MClosed;
            int temp;
            if (int.TryParse(LocalEnv.ConfInfo.MaxWarningCount, out temp) && temp > 0)
                MaxWarningCount = temp;
            LogHelper.Info("MaxWarningCount:{0}", MaxWarningCount);
            //dpFrame.MaxHeight = SystemParameters.WorkArea.Height;
            //dpFrame.MaxWidth = SystemParameters.WorkArea.Width;
            Loaded += Window1_Loaded;
            ApiManager.Instance.GetDefaultApi().SubScribeConStatus(OnGateConnectStatus);
            this.Closing += Window1_Closing;
            this.Icon = LocalEnv.ServerInfo.AppImage;
            WindowIconHelper.Instance.Start();
            //this.lbSysName.Content = LocalEnv.ServerInfo.AppName;
            this.Title = LocalEnv.ServerInfo.AppName;
            //this.lbVersionCode.Content = LocalEnv.ServerInfo.Version;

            PreviewKeyDown += Window1_PreviewKeyDown;

            PreviewMouseDown += Window1_PreviewMouseDown;

            logoInfo.OnCloseClick += LogoInfo_OnCloseClick;
            logoInfo.OnMaxClick += LogoInfo_OnMaxClick;
            logoInfo.OnMinClick += LogoInfo_OnMinClick;

            SourceInitialized += new EventHandler(win_SourceInitialized);

            tcPlugin.Items.Clear();
            tcPlugin.ClosingItemCallback = OnTabItemClose;


            ThreadPool.RegisterWaitForSingleObject(BaseMachineHelper._appEvent, OnProgramStarted, null, -1, false);

            DesignHandler.Instance.OnWindowDragMove += Instance_OnWindowDragMove;
            DesignHandler.Instance.OnMax += Instance_OnMax;
            //加载树状分组数据
            if (LocalEnv.IsHaveTradeBookPermission || LocalEnv.ConfInfo.IsPAYL)
                LoadTreeBookUserConfig();

            DesignHandler.Instance.OnMax += Instance_OnMax;
            //订阅App消息
            STC.Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// 窗口拖放
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnWindowDragMove(object obj)
        {
            this.DragMove();
        }


        private void Window1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ////if (tcPlugin.SelectedItem != null && tcPlugin.SelectedItem is TabItem)
            ////{
               
            ////    TabItem currentItem = tcPlugin.SelectedItem as TabItem;
            ////    if (currentItem != null && currentItem.Header != null && currentItem.Header.ToString() == "行情看板")
            ////    {

            ////        // 当鼠标单击发生时，强制失去焦点的控件失去焦点
            ////        var elementWithFocus = Keyboard.FocusedElement as UIElement;
            ////        if (elementWithFocus is TextBox)
            ////        {
            ////            Point pntMouse = Mouse.GetPosition(this);
            ////            Point pntB = elementWithFocus.TranslatePoint(new Point(), this);
            ////            Rect rectB = new Rect(pntB, elementWithFocus.RenderSize);
            ////            bool bContained = rectB.Contains(pntMouse);
            ////            // 如果不是输入框，失去焦点
            ////            if (!bContained)
            ////            {
            ////                TextBox textBoxWithFocus = elementWithFocus as TextBox;
            ////                textBoxWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            ////            }
                        
            ////        }
            ////    }
            ////}
        }

        private void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppUtils.AppClosing = true;
        }


        /// <summary>
        /// 加载树状分组数据
        /// </summary>
        private void LoadTreeBookUserConfig()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                //查询GR全局设置
                try
                {
                    //AdminHandler.Instance.queryBookUser();
                    AdminHandler.Instance.InitBookDicByUser("*");

                }
                catch { }
            });
        }

        void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }


        private void Window1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Console.WriteLine("Window1_PreviewKeyDown: {0}", e.Key);
            if (MainWindowHandler.PassPreviewKey)
            {
                return;
            }

            if (AppUtils.Application.Panel.Items.Count > 0)
            {
                object selt = AppUtils.Application.Panel.SelectedItem;
                if (selt.GetType() == typeof(TabItem))
                {
                    string tag = ((TabItem)selt).Tag.ToString() ?? "80";
                    //如果是网页关闭快捷搜索
                    if (tag == ((int)PluginKeyEnum.ChromePlugin).ToString())
                    {
                        return;
                    }
                }
            }

            DesignHandler.Instance.PublishKeyDown(e);


            //switch (e.Key)
            //{
            //    case Key.Left:
            //    case Key.PageUp:
            //        if (MainWindowHandler.PageHandler != null)
            //        {
            //            MainWindowHandler.PageHandler.PrevPage();
            //            e.Handled = true;
            //        }
            //        break;
            //    case Key.Right:
            //    case Key.PageDown:
            //        if (MainWindowHandler.PageHandler != null)
            //        {
            //            MainWindowHandler.PageHandler.NextPage();
            //            e.Handled = true;
            //        }
            //        break;
            //    case Key.Home:
            //        if (MainWindowHandler.PageHandler != null)
            //        {
            //            MainWindowHandler.PageHandler.FirstPage();
            //            e.Handled = true;
            //        }
            //        break;
            //    case Key.Up:
            //        break;
            //    case Key.End:
            //        if (MainWindowHandler.PageHandler != null)
            //        {
            //            MainWindowHandler.PageHandler.LastPage();
            //            e.Handled = true;
            //        }
            //        break;
            //    case Key.Down:
            //        break;
            //    case Key.Tab:
            //        LoopNextTab();
            //        e.Handled = true;
            //        break;
            //    case Key.F9:
            //        if (tcContent.SelectedIndex == 0)
            //        {
            //            settingButton_Click(null, null);
            //        }
            //        e.Handled = true;
            //        break;
            //}
        }


        #region 窗口最大化
        private static System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);


                // 设置最小宽度和高度
                mmi.ptMinTrackSize.x = 800;
                mmi.ptMinTrackSize.y = 620;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }


        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion

        private void OnProgramStarted(object o, bool timedout)
        {
            LogHelper.Info("OnProgramStarted");
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (!AppUtils.AppClosing)
                    {
                        //this.WindowState = FormWindowState.Maximized;
                        //this.BringToFront();
                        this.WindowState = WindowState.Normal;
                        this.Activate();
                        this.Focus();
                        this.Show();
                    }
                });
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        private void LogoInfo_OnMinClick(object arg1, RoutedEventArgs arg2)
        {
            this.WindowState = WindowState.Minimized;
        }


        private double normalWidth = 0;
        private double normalHeight = 0;
        private double normalLeft = 0;
        private double normalTop = 0;

        private void LogoInfo_OnMaxClick(object arg1, RoutedEventArgs arg2)
        {
            if (WindowState != WindowState.Maximized)
            {
                normalWidth = this.Width;
                normalHeight = this.Height;
                normalLeft = this.Left;
                normalTop = this.Top;

                OnMaxClick();

                //Rect rect = SystemParameters.WorkArea;
                //this.MaxWidth = rect.Width;
                //this.MaxHeight = rect.Height;
                //this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;

                //OnNormalClick();
                ReStoreWindow(true);
            }

            if (this.Width > 0)
            {
                double result = this.Width - 200 - 200 - tcPlugin.Items.Count * 100;
                //tbMove.Width = result > 0 ? result : 0;
            }

            //将小铃铛再次复位
            //DesignHandler.Instance.SetGlobalActivate(false);
        }


        private void OnMaxClick()
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{
            try
            {
                this.WindowState = WindowState.Maximized;
                var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                PresentationSource source = PresentationSource.FromVisual(window);
                Matrix m = source.CompositionTarget.TransformFromDevice;
                double widthRatio = m.M11;
                double heightRatio = m.M22;

                this.Width = screen.WorkingArea.Width * widthRatio;
                this.Height = screen.WorkingArea.Height * heightRatio;
                this.Left = rect.Left == 0 ? 0 : screen.WorkingArea.Left * widthRatio;
                this.Top = rect.Top == 0 ? 0 : screen.WorkingArea.Top * heightRatio;
                this.MaxWidth = this.Width;
                this.MaxHeight = this.Height;
            }
            catch { }
            //}));
        }

        private System.Windows.Forms.Screen GetShowScreen()
        {
            var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕

            return screen;
        }

        private void OnNormalClick()
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{
            try
            {
                var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                PresentationSource source = PresentationSource.FromVisual(window);
                Matrix m = source.CompositionTarget.TransformFromDevice;
                double widthRatio = m.M11;
                double heightRatio = m.M22;

                if (normalHeight != 0 && normalWidth != 0)
                {
                    if (screen.WorkingArea.Width * widthRatio == normalWidth)
                    {
                        this.Width = (screen.WorkingArea.Width * widthRatio) / 4 * 3 + 50;
                    }
                    else
                    {
                        this.Width = normalWidth;
                    }

                    if (screen.WorkingArea.Height * heightRatio == normalHeight)
                    {
                        this.Height = (screen.WorkingArea.Height * heightRatio) / 5 * 4;
                    }
                    else
                    {
                        this.Height = normalHeight;
                    }
                }
                else
                {
                    this.Width = (screen.WorkingArea.Width * widthRatio) / 4 * 3 + 50;
                    this.Height = (screen.WorkingArea.Height * heightRatio) / 5 * 4;
                }

                if (normalTop != 0 && normalLeft != 0)
                {
                    this.Top = normalTop;
                    this.Left = normalLeft;
                }
                else
                {
                    this.Top = (screen.WorkingArea.Height - this.Height) / 2 + screen.WorkingArea.Top * heightRatio;
                    this.Left = (screen.WorkingArea.Width * widthRatio - this.Width) / 2 + screen.WorkingArea.Left * widthRatio;
                }

                if (normalHeight != 0 && normalWidth != 0 && (normalTop != 0 || normalLeft != 0))
                {
                    this.WindowState = WindowState.Normal;

                    if (screen.WorkingArea.Width * widthRatio == normalWidth && screen.WorkingArea.Height * heightRatio == normalHeight)
                    {
                        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                }
                else
                {
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    this.WindowState = WindowState.Normal;
                }
            }
            catch { }
            //}));
        }


        private void LogoInfo_OnCloseClick(object arg1, RoutedEventArgs arg2)
        {
            WindowIconHelper.Instance.Clear();
            this.Close();
        }

        private void Instance_OnMax(object obj)
        {
            normalWidth = this.Width;
            normalHeight = this.Height;
            normalLeft = this.Left;
            normalTop = this.Top;

            OnMaxClick();
        }

        #region 新UI逻辑
        #endregion


        private void Window1_Closed(object sender, EventArgs e)
        {
            btnClose_Click(null, null);

            //WindowIconHelper.Instance.Clear();
        }

        private void Window1_StateChanged(object sender, EventArgs e)
        {
            DesignHandler.Instance.NotifyWindowState(this.WindowState);
        }

        private void OnGateConnectStatus(object code)
        {
            if (code != null)
            {
                int status = (int)code;
                switch (status)
                {
                    case IEventListener.CONNECTION_CLOSED:

                        LogHelper.Info("Gateway CONNECTION_CLOSED");
                        if (WarningCount >= MaxWarningCount)
                        {
                            LoginHelper.Instance.Logout2("多次尝试重新连接失败，请重新登录");
                        }
                        else
                        {
                            ShowStatus(true);
                        }
                        break;

                    case IEventListener.CONNECTION_LOGIN_SUCCESS:


                        ShowStatus(false);

                        BaseHandler.Instance.Reset();

                        WarningCount = 0;

                        break;

                    case IEventListener.CONNECTION_LOGOUT:

                        LoginHelper.Instance.Logout2("账号已在其他地方登录，请重新登录！");

                        LogHelper.Info("GateWay CONNECTION_LOGOUT");

                        break;

                }
            }
        }


        private void SysWarnGUI_MClosed()
        {
            WarningCount++;
        }

        private void ShowStatus(bool flag)
        {
            Dispatcher.Invoke(new Action<bool>((showWarn) =>
            {
                if (flag)
                {
                    sysWarnGUI.Show();
                    //lbServerStatus.Text = string.Format("连接状态：{0}", "已断开");
                }
                else
                {
                    sysWarnGUI.Hide();
                    //lbServerStatus.Text = string.Format("连接状态：{0}", "已连接");
                }
            }), flag);
        }


        private LogoButton logoButton = new LogoButton();
        MDT.Tools.Wpf.AutoUpdater.AutoUpdater autoUpdater = new MDT.Tools.Wpf.AutoUpdater.AutoUpdater();
        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            //if (LocalEnv.ConfInfo.IsTest)
            //{
            //    tbMove.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    tbMove.Visibility = Visibility.Visible;
            //}

            DesignHandler.Instance.DesignNotify += Instance_DesignNotify;

            sysWarnGUI.Owner = this;

            this.Dispatcher.Invoke(() =>
            {

                tcPlugin.HeaderPrefixContent = logoButton;

                logoInfo.setUserName(LocalEnv.UserInfo.FullName);
                Version version = ReflectionHelper.GetFrameVersion();
                DateTime buildtime = System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location);

                //this.Height = mainScreen.WorkingArea.Height / 5 * 4;
                //defaultWidth = currentWidth = this.Width = mainScreen.WorkingArea.Width / 4 * 3 + 100;
                //this.Top = (mainScreen.WorkingArea.Height - this.Height) / 2;

                //this.Height = SystemParameters.WorkArea.Height / 5 * 4;//mainScreen.WorkingArea.Height / 5 * 4;
                //defaultWidth = currentWidth = this.Width = SystemParameters.WorkArea.Width / 4 * 3; ;// mainScreen.WorkingArea.Width / 4 * 3;// + 100;
                //                                                                                     //this.Top = (mainScreen.WorkingArea.Height - this.Height) / 2;
                logoInfo.SubscribeConStatus();

                //if (this.Panel.Items.Count > 0)
                //    this.Panel.SelectedIndex = 0;

                DataTimeSequence.Instance.Number = 1;

                DesignHandler.IsShow = true;

                //this.Dispatcher.Invoke(() =>
                //{
                //    LogHelper.Info("Load History");
                //    ReadHistory();
                //});


                //if (this.Panel.Items.Count == 1)
                tcPlugin_SelectionChanged(tcPlugin, null);


                SetUserDrag();

                ReStoreWindow();

            });

            //ClientUpdate.Instance.UpdateClient();
            //DesignHandler.Instance.SetGlobalActivate(false);
        }

        private void SetUserDrag()
        {
            Thread t = new Thread(UpdateDrag);
            t.IsBackground = true;
            t.Start();
        }

        private void UpdateDrag()
        {
            try
            {
                Thread.Sleep(5000);

                this.Dispatcher.Invoke(() =>
                {
                    LogHelper.Info(string.Format("UserDrag drag function: {0}.", LocalEnv.ConfInfo.UserDrag));
                    if (!LocalEnv.ConfInfo.UserDrag)
                    {
                        tcPlugin.InterTabController = null;
                    }
                });
            }
            catch (Exception e)
            { }

        }


        private void Instance_DesignNotify(DisplayModel obj)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (obj == DisplayModel.NotShowPrice)
                    this.Width = defaultWidth - 1000;
                else if (obj == DisplayModel.TwoCol)
                    this.Width = defaultWidth - 500;
                else if (obj == DisplayModel.FourCol)
                    this.Width = defaultWidth;
            });
        }

        private void LoadPlugin()
        {
            _pluginManager.Loading();
        }

        #region 记录打开的tag页面

        private void WriteHistory()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<DragablzItem> items = TabablzPanel.GetOrderedHeaders();
            foreach (DragablzItem tab in items)
            {
                TabItem temItem = tab.DataContext as TabItem;
                if (temItem.Tag != null && temItem.Tag is int)
                {
                    int pluginKey = (int)temItem.Tag;

                    IPlugin plugin = this._pluginManager.GetPlugin(pluginKey);
                    if (plugin != null)
                    {
                        sb.Append(string.Format("{0}-{1},", pluginKey, temItem.Header));
                    }
                }
            }

            string selectPlugin = sb.ToString();
            selectPlugin = selectPlugin.Length > 0 ? selectPlugin.Substring(0, selectPlugin.Length - 1) : string.Empty;

            IPlugin focusPlugin = null;
            TabItem tabItem = Panel.SelectedItem as TabItem;
            if (tabItem != null)
            {
                IniFileHelper.WriteValue(AppUtils.GetCacheKey(), SelectedTagKey, tabItem.Header as string);

            }

            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), OpenPluginKey, selectPlugin);

            SaveWindowPos();
        }

        private void ReadHistory()
        {
            string historyTag = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), OpenPluginKey);
            string selectTag = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), SelectedTagKey);

            string preHeight = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_height);
            string preWidth = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_width);
            string preLeft = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_Left);
            string preTop = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_Top);

            this.WindowState = WindowState.Normal;
            bool isDefault = true;
            double d = 0;

            System.Windows.Forms.Screen screen = GetShowScreen();
            bool isSamllScreen = false;
            if (screen.WorkingArea.Width < 1500 && screen.WorkingArea.Height < 1000)
            {
                isSamllScreen = true;
            }

            if (double.TryParse(preHeight, out d))
            {
                //if (d > SystemParameters.WorkArea.Height)
                //{
                //    this.Height = SystemParameters.WorkArea.Height / 5 * 4;
                //}
                //else
                {
                    this.Height = d;
                }
            }
            else
            {
                if (isSamllScreen)
                {
                    this.Height = screen.WorkingArea.Height / 5 * 4;
                }
                else
                {
                    this.Height = 2 * 372 + 132;
                }
            }
            if (double.TryParse(preWidth, out d))
            {
                //if (d > SystemParameters.WorkArea.Width)
                //{
                //    defaultWidth = currentWidth = this.Width = SystemParameters.WorkArea.Width / 5 * 4;
                //}
                //else
                {
                    defaultWidth = currentWidth = this.Width = d;
                    if (defaultWidth <= 640)
                    {
                        this.Width = defaultWidth;
                        this.MinWidth = defaultWidth;
                    }
                }
            }
            else
            {
                if (isSamllScreen)
                {
                    this.Width = screen.WorkingArea.Width / 5 * 4;
                }
                else
                {
                    this.Width = 4 * 284 + 20;
                }
            }
            if (double.TryParse(preLeft, out d))
            {
                //if (d > SystemParameters.WorkArea.Width)
                //{
                //    this.Left = SystemParameters.WorkArea.Width / 4;
                //}
                //else
                {
                    this.Left = d;
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }
                isDefault = false;
            }
            if (double.TryParse(preTop, out d))
            {
                //if (d > SystemParameters.WorkArea.Height)
                //{
                //    this.Top = SystemParameters.WorkArea.Height / 4;
                //}
                //else
                {
                    this.Top = d;

                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                }
                isDefault = false;
            }

            if (isDefault)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            //判断当前所在屏幕的显示区域，并设置窗口最大化会恢复的状态
            var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
            System.Windows.Forms.Screen screen2 = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
            PresentationSource source = PresentationSource.FromVisual(window);
            Matrix m = source.CompositionTarget.TransformFromDevice;
            double widthRatio = m.M11;
            double heightRatio = m.M22;
            double winWidth = screen2.WorkingArea.Width * widthRatio;
            double winHeight = screen2.WorkingArea.Height * heightRatio;
            if (this.Width == winWidth && this.Height == winHeight)
            {
                WindowState = WindowState.Maximized;
            }

            if (string.IsNullOrEmpty(historyTag))
            {
                historyTag = System.Configuration.ConfigurationManager.AppSettings["DefaultPlugin"] ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(historyTag))
            {
                string[] arr = historyTag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string[] kv = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        int pluginKey = -1;
                        if (kv != null && kv.Length > 0 && int.TryParse(kv[0], out pluginKey))
                        {

                            IPlugin plugin = _pluginManager.GetPlugin(pluginKey);
                            if (plugin != null && CheckPermission(pluginKey) && plugin.PluginKey != (int)PluginKeyEnum.DownloadCenterPlugin)
                            {
                                plugin.PluginData = kv.Length > 1 ? kv[1] : string.Empty;
                                _pluginManager.EnablePlugin(pluginKey);

                                // pluginKey不是alone， 可以多次打开
                                if (!plugin.IsAlone)
                                {
                                    plugin.Enabled = false;
                                }

                                //如果有chrome插件，恢复加载chrome插件
                                if (pluginKey == (int)PluginKeyEnum.ChromePlugin)
                                {
                                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "OpenHistroyChromePlugin", Info1 = CommonUtils.SerializeObject(new List<string>() { plugin.PluginData.ToString() }), Info2 = "", Info3 = "" });
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(selectTag))
            {
                selectTag = System.Configuration.ConfigurationManager.AppSettings["DefaultFocusPlugin"] ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(selectTag))
            {
                if (Panel.Items.Count > 0)
                {
                    int index = 0;
                    foreach (TabItem tabItem in Panel.Items)
                    {
                        string pluginKey = tabItem.Header as string;
                        if (!string.IsNullOrEmpty(pluginKey) && selectTag.Equals(pluginKey))
                        {
                            break;
                        }
                        index++;
                    }

                    if (Panel.Items.Count >= index)
                        Panel.SelectedIndex = index;
                }
            }
            else
            {
                if (Panel.Items.Count > 0)
                {
                    int index = 0;
                    foreach (TabItem tabItem in Panel.Items)
                    {
                        if (tabItem.Tag.ToString() == ((int)PluginKeyEnum.TradingPlugin).ToString())
                        {
                            break;
                        }
                        index++;
                    }

                    if (Panel.Items.Count >= index)
                        Panel.SelectedIndex = index;
                }
            }

        }


        private bool CheckPermission(int pluginKey)
        {
            bool result = false;
            if (pluginKey == (int)PluginKeyEnum.HistoryPlugin)
                result = !string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1";
            else
                result = true;

            return result;
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            WriteHistory();
            //Application.Current.Shutdown();
            WindowIconHelper.Instance.Clear();
            Exit();
            LoginHelper.Instance.AppExit();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private bool isMax = false;
        //窗体是否折叠
        public bool IsFold = false;
        public double widthCache = 1020;


        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Normal)
            {
                //this.WindowState = WindowState.Normal;
                //this.isMax = false;
                //btnMax_Click(null, null);
                WindowIconHelper.Instance.Stop();
            }
            //else
            //    window.WindowState = WindowState.Maximized;
        }

        private double currentWidth = 0;

        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ResetMainScreen();

            //currentWidth = e.NewSize.Width;

            //if (e.NewSize.Height != mainScreen.WorkingArea.Height)
            //    isMax = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();


            }
            catch { }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var intPtr = new WindowInteropHelper(this).Handle;//获取当前窗口的句柄
            IntPtr sysMenuHandle = Win32Helper.GetSystemMenu(intPtr, false);
            Win32Helper.AppendMenu(sysMenuHandle, Win32Helper.MF_SEPARATOR, 0, string.Empty);//添加一个分隔线
            Win32Helper.AppendMenu(sysMenuHandle, Win32Helper.MF_STRING, Win32Helper.IDM_ABOUT, "恢复到主屏幕");

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            timer.Tick += (s, e1) => 
            {
                //Console.WriteLine("---" + DateTimeEx.ServerTime.ToString("ddd HH:mm:ss") + "--" + DateTime.Now.ToString("HH:mm:ss"));
                tbTime.Text = DateTimeEx.ServerTime.ToString("ddd HH:mm:ss");
            };
            timer.IsEnabled = true;

            //System.Threading.ThreadPool.QueueUserWorkItem(o =>
            //{
            //    while (true)
            //    {
            //        setServerTime();
            //        Thread.Sleep(1000);
            //    }
            //});
        }

        private void setServerTime()
        {
            Action action = () => tbTime.Text = string.Format("{0}", DateTimeEx.ServerTime.ToString("ddd HH:mm:ss"));//DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss"));
            if (Thread.CurrentThread != tbTime.Dispatcher.Thread)
            {
                tbTime.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("暂未提供", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
            //DesignHandler.Instance.showDesignUI(sender);
        }


        public void setOpenCloseVisible(Visibility visibility)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                //cbOpenClose.Visibility = visibility;
            }));

        }

        #region Initialize
        public void Initialize()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Panel.Items.Clear();
                List<string> pluginFiles = RoleHelper.GetPluginFile(LocalEnv.UserInfo.prods);
                List<Type> types = new List<Type>();
                string files = "";
                foreach (string pluginfile in pluginFiles)
                {
                    files += pluginfile + ",";
                }
                PluginHelper.PluginSign1 = files;
                _pluginUtils = new PluginUtils();

                _pluginManager = new PluginManager();
                FrameAdapter.Application = _pluginManager.Application = this;
                _pluginManager.Init();

                AppUtils.Application = this;

                //CheckHistory();

                //this.PluginManager.PluginChanged += PluginManager_PluginChanged;
                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "dmart":
                    case "gtja":
                    case "idbpro":
                        (LoginWin as LoginGUI).SetProcess(80);

                        LoadPlugin();
                        this.Show();
                        //this.Cursor = Cursors.Arrow;
                        (LoginWin as LoginGUI).SetProcess(90);
                        (LoginWin as LoginGUI).Hide();
                        break;
                    case "bdb":
                    case "idcclient":
                        if (LocalEnv.ConfInfo.LoginType == "1")
                        {
                            //QbLoginWindow
                            LoadPlugin();
                            this.Show();
                            (LoginWin as QbLoginWindow).Hide();
                        }
                        else
                        {
                            //LoginGUI
                            (LoginWin as LoginGUI).SetProcess(80);
                            LoadPlugin();
                            this.Show();
                            //this.Cursor = Cursors.Arrow;
                            (LoginWin as LoginGUI).SetProcess(90);
                            (LoginWin as LoginGUI).Hide();
                        }

                        break;

                }
            });

            ReadHistory();


            this.StateChanged += Window1_StateChanged;
            this.SizeChanged += Window1_SizeChanged;
            this.Closed += Window1_Closed;


        }

        //排除和历史重复的插件，防止重复加载
        private void CheckHistory()
        {
            string historyTag = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), OpenPluginKey);
            if (!string.IsNullOrEmpty(historyTag))
            {
                string[] arr = historyTag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    if (!string.IsNullOrEmpty(historyTag))
                    {
                        //包含历史Tab时，重置默认打开插件
                        foreach (IPlugin plugin in this._pluginManager.PluginList)
                        {
                            if (plugin.PluginKey != (int)PluginKeyEnum.DataPlugin && plugin.PluginKey != (int)PluginKeyEnum.SysPlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.MsgPlugin
                                //&& plugin.PluginKey != (int)PluginKeyEnum.SimulatePlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.ChromePlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.SettingPlugin
                                 && plugin.PluginKey != (int)PluginKeyEnum.InnerBoardPlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.TradingPlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.DownloadCenterPlugin) //3:BDB.Sys.Plugin;1:BasePlugin
                                plugin.Enabled = false;
                        }
                    }

                    bool isDown = false;
                    foreach (string str in arr)
                    {
                        string[] kv = str.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        int pluginKey = -1;
                        if (kv != null && kv.Length > 0 && int.TryParse(kv[0], out pluginKey))
                        {
                            IPlugin plugin = _pluginManager.GetPlugin(pluginKey);
                            if (plugin != null && CheckPermission(pluginKey))
                            {
                                _pluginManager.DisEnablePlugin(pluginKey);

                                //if (pluginKey == (int)PluginKeyEnum.DownloadCenterPlugin)
                                //{
                                //    isDown = true;
                                //}

                            }
                        }
                    }

                    //if (!isDown)
                    //{
                    //    _pluginManager.DisEnablePlugin((int)PluginKeyEnum.DownloadCenterPlugin);
                    //}
                }
            }
        }


        private void PluginManager_PluginChanged()
        {
            //btnAdd.Margin = new Thickness(tcPlugin.Items.Count * 90, 3, 0, 0);
        }

        #endregion



        #region IForm

        private IPluginManager _pluginManager;
        private PluginUtils _pluginUtils;


        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        public TabablzControl TabablzPanel
        {
            get { return tcPlugin; }
        }


        public TabControl Panel
        {
            get { return tcPlugin; }
        }

        public Grid CommonPanel
        {
            get { return gridCommon; }
        }

        public Grid HeadPanel
        {
            get { return null; }
        }


        public STC.Common.View.UI.LogoButton MenuPanel { get { return logoButton; } }

        public Grid CustomePanel
        {
            get { return null; }
        }

        public void RegisterObject(string name, object obj)
        {
            _pluginUtils.RegisterObject(name, obj);
        }

        public object GetObject(string name)
        {
            object o = _pluginUtils.GetObject(name);
            return o;
        }

        public void Remove(string name)
        {
            _pluginUtils.Remove(name);
        }

        public void Exit()
        {
            _pluginManager.Unloading();
        }

        private readonly PluginBroadCast _pluginBroadCast = new PluginBroadCast();

        public void Subscribe(string name, IPlugin plugin)
        {
            _pluginBroadCast.Subscribe(name, plugin);
        }
        public void Unsubscribe(string name, IPlugin plugin)
        {
            _pluginBroadCast.Unsubscribe(name, plugin);
        }
        public void BroadCast(string name, object o)
        {
            _pluginBroadCast.BroadCast(name, o);
        }
        #endregion

        private void btnSetting_MouseEnter(object sender, MouseEventArgs e)
        {
            DesignHandler.Instance.MouseoverDesignUI(sender);
        }

        private void btnTabClose_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectItem = null;
            try
            {
                Button btn = sender as Button;

                Grid grid = btn.Parent as Grid;

                TextBlock tb = grid.Children[0] as TextBlock;

                foreach (TabItem tabItem in this.Panel.Items)
                {
                    if (tabItem.Header.ToString().Equals(tb.Text))
                    {
                        selectItem = tabItem;
                        break;
                    }
                }


                tcPlugin.Items.Remove(selectItem);


                int pluginKey = (int)selectItem.Tag;
                if (pluginKey > 0)
                    this.PluginManager.DisEnablePlugin(pluginKey);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private bool isDragging = false;
        private Point offset;
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                LogoInfo_OnMaxClick(null, null);
            }
            else
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    try
                    {
                        offset = e.GetPosition(this);
                        this.DragMove();
                    }
                    catch { }
                }
                if (e.ButtonState == MouseButtonState.Released)
                {
                    Point newPosition = e.GetPosition(this);
                    if (newPosition.X != offset.X || newPosition.Y != offset.Y)
                    {
                        SaveWindowPos();
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                    {
                        this.Show();
                        this.Activate();
                    }));
                }
            }
        }

        private void mainWindow_Activated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            if (window != null && window.IsActive)
                WindowIconHelper.Instance.Stop();
        }



        public void SetCommonVisibility(Visibility visibility)
        {
            if (!isGetPermission)
            {
                isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
                isAllow = AdminHandler.Instance.CheckPermission("1003", null);
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                isGetPermission = true;

                if (!isAllow || isLite || isNotAllow)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        gridMain.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
                        gridMain.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);

                    }));
                }
            }

            if (!(!isAllow || isLite))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (visibility == Visibility.Collapsed)
                    {
                        gridMain.RowDefinitions[2].Height = gridMain.RowDefinitions[3].Height = new GridLength(0);
                        gridCommon.Visibility = gridSplitter.Visibility = btnHor.Visibility = visibility;
                    }
                    else
                    {
                        gridCommon.Visibility = gridSplitter.Visibility = btnHor.Visibility = visibility;
                        if (gridMain.RowDefinitions[2].Height.Value == 0)
                        {
                            gridMain.RowDefinitions[2].Height = new GridLength(8);
                        }

                        SetExecutionsHeight();
                    }
                }));
            }
            
        }

        private void SetExecutionsHeight()
        {
            string expandbettoncollapsed = IniFileHelper.ReadValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed");
            if (!string.IsNullOrEmpty(expandbettoncollapsed))
            {
                btnHor.Content = expandbettoncollapsed == "1" ? "∧" : "∨";
                if (expandbettoncollapsed == "1")
                {
                    Row0.Height = new GridLength(60, GridUnitType.Star);
                    Row2.Height = new GridLength(0);
                }
                else
                {
                    string height = IniFileHelper.ReadValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "MainWindowTopHeight");
                    double d = 0;
                    string width = IniFileHelper.ReadValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "MainWindowBottomHeight");
                    if (!string.IsNullOrEmpty(height))
                    {
                        height = height.Replace("*", "");
                    }
                    if (!string.IsNullOrEmpty(width))
                    {
                        width = width.Replace("*", "");
                    }
                    if (double.TryParse(height, out d))
                    {
                        if (d > 0)
                        {
                            Row0.Height = new GridLength(d, GridUnitType.Star);
                            double w = 0;
                            if (double.TryParse(width, out w))
                            {
                                Row2.Height = new GridLength(w, GridUnitType.Star);
                            }
                        }
                        else
                        {
                            Row0.Height = new GridLength(60, GridUnitType.Star);
                            Row2.Height = new GridLength(40, GridUnitType.Star);
                        }
                    }
                    else
                    {
                        Row0.Height = new GridLength(60, GridUnitType.Star);
                        Row2.Height = new GridLength(40, GridUnitType.Star);
                    }
                }
            }
            else
            {
                if (IntentionOldHandler.Instance.isSALER())
                {
                    btnHor.Content = "∧";
                    Row0.Height = new GridLength(60, GridUnitType.Star);
                    Row2.Height = new GridLength(0);
                }
                else
                {
                    Row0.Height = new GridLength(60, GridUnitType.Star);
                    Row2.Height = new GridLength(40, GridUnitType.Star);
                }

            }

        }

        #region 保存和恢复窗口
        private void Window1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
                SaveWindowPos();
        }
        private void SaveWindowPos()
        {
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_height, this.ActualHeight + "");
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_width, this.ActualWidth + "");
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_Left, this.Left + "");
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_Top, this.Top + "");
            //DesignHandler.Instance.SetGlobalActivate(false);
        }

        private void ReStoreWindow(bool isReset = false)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                PresentationSource source = PresentationSource.FromVisual(window);
                Matrix m = source.CompositionTarget.TransformFromDevice;
                double widthRatio = m.M11;
                double heightRatio = m.M22;
                //窗口可显示区域大小
                double winWidth = screen.WorkingArea.Width * widthRatio;
                double winHeight = screen.WorkingArea.Height * heightRatio;
                //保存的窗口大小
                string height = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_height);//IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", Window_height);
                string width = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_width);//IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", Window_width);

                //如果第一次登陆，则默认打开正常窗口
                if (string.IsNullOrEmpty(height) && string.IsNullOrEmpty(width))
                {
                    RestoreNormal();
                    return;
                }
                else
                {
                    //判断最后一次登录是否是最大化，如果是最大化则恢复为默认
                    if (isReset)
                    {

                        if (width == winWidth.ToString() && height == winHeight.ToString())
                        {
                            RestoreNormal();
                            return;
                        }
                    }
                }


                double d = 0;
                if (double.TryParse(height, out d))
                    this.Height = d;
                if (double.TryParse(width, out d))
                    this.Width = d;

                string positionX = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_Left); //IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", Window_Left);
                string positionY = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), Window_Top); //IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", Window_Top);
                double dY = 0;
                if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
                {
                    bool isContain = false;
                    // 假设坐标点是 (100, 100)
                    Point pointToCheck = new Point((int)d, (int)dY);

                    foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                    {
                        // 获取当前屏幕的工作区域和 DPI
                        System.Drawing.Rectangle screenBounds = sc.WorkingArea;
                        Matrix screenDpiMatrix = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;

                        // 将坐标点转换为当前屏幕的逻辑坐标点
                        Point pointOnCurrentScreen = screenDpiMatrix.Transform(pointToCheck);

                        // 判断逻辑坐标点是否在当前屏幕工作区域内
                        if (CommonUtils.IsPointInScreen(pointOnCurrentScreen, screenBounds))
                        {
                            isContain = true;
                            break;  // 如果找到一个屏幕，就结束循环
                        }
                    }

                    if (isContain)
                    {
                        this.Top = dY;
                        this.Left = d;
                    }
                }

                if (width == winWidth.ToString() && height == winHeight.ToString())
                {
                    WindowState = WindowState.Maximized;
                }
            }));
        }



        private void RestoreNormal(bool isMain = false)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                    var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                    Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                    PresentationSource source = PresentationSource.FromVisual(window);
                    Matrix m = source.CompositionTarget.TransformFromDevice;
                    double widthRatio = m.M11;
                    double heightRatio = m.M22;
                    this.Width = (screen.WorkingArea.Width * widthRatio) / 4 * 3 + 50;
                    this.Height = (screen.WorkingArea.Height * heightRatio) / 5 * 4;
                    if (isMain == false)
                    {
                        this.Top = (screen.WorkingArea.Height - this.Height) / 2 + screen.WorkingArea.Top * heightRatio;
                        this.Left = (screen.WorkingArea.Width * widthRatio - this.Width) / 2 + screen.WorkingArea.Left * widthRatio;
                    }
                    else
                    {
                        this.Top = (screen.WorkingArea.Height - this.Height) / 2;
                        this.Left = (screen.WorkingArea.Width * widthRatio - this.Width) / 2;
                    }
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    this.WindowState = WindowState.Normal;
                }
                catch { }
            }));
        }
        #endregion



        private int _preFocusPluginKey = -1;
        private bool _isFirst = true;

        public Type ChartType = null;
        public Type ChartModel = null;
        public Assembly ChartAsm = null;
        public object CandleLineInstance = null;

        private bool isLite = true;
        private bool isAllow = true;
        private bool isGetPermission = false;

        private void tcPlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isGetPermission)
            {
                isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
                isAllow = AdminHandler.Instance.CheckPermission("1003", null);

                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                isGetPermission = true;

                if (!isAllow || isLite || isNotAllow)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        gridMain.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
                        gridMain.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);

                    }));
                }
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //CommonUtils._ChangeTab_ManualResetEvent.Reset();
                if (this.Width > 0)
                {
                    double result = this.Width - 200 - 200 - tcPlugin.Items.Count * 100;
                    //tbMove.Width = result > 0 ? result : 0;
                    double left = 195 + tcPlugin.Items.Count * 100;
                    if (gridTitle.Margin != new Thickness(left, 0, 0, 0))
                        gridTitle.Margin = new Thickness(left, 0, 0, 0);
                }

                TabControl tc = sender as TabControl;
                TabItem tabItem = tc.SelectedItem as TabItem;
                if (tabItem != null && tabItem.Tag != null)
                {
                    int pluginKey = (int)tabItem.Tag;

                    if (!_isFirst && e != null)
                    {
                        if (!(_preFocusPluginKey == -1 || (_preFocusPluginKey > 0 && _preFocusPluginKey != pluginKey)))
                        {
                            //CommonUtils._ChangeTab_ManualResetEvent.Set();
                            return;
                        }
                    }

                    _isFirst = false;

                    _preFocusPluginKey = pluginKey;

                    if ((int)PluginKeyEnum.TradingPlugin == pluginKey)
                    {
                        string plugname = tabItem.Content.ToString();
                        if (plugname == "BDB.Trade.Plugin.View.TradeMainView")
                        {
                            SetCommonVisibility(Visibility.Collapsed);
                            //CommonUtils._ChangeTab_ManualResetEvent.Set();
                            return;
                        }
                    }

                    IPlugin plugin = _pluginManager.GetPlugin(pluginKey);
                    if (plugin != null)
                    {
                        if (CommonPanel.Children.Count == 0)
                        {
                            SetCommonVisibility(Visibility.Collapsed);
                            //CommonUtils._ChangeTab_ManualResetEvent.Set();
                            return;
                        }

                        if (plugin.IsFullScreen)
                            SetCommonVisibility(Visibility.Collapsed);
                        else
                            SetCommonVisibility(Visibility.Visible);
                    }
                    else if (pluginKey < 0)//traderbook特殊处理
                    {
                        SetCommonVisibility(Visibility.Visible);
                    }
                }

                SetAppointItemVisibility();
            }));
            //CommonUtils._ChangeTab_ManualResetEvent.Set();
        }

        private void SetAppointItemVisibility()
        {
            try
            {
                IEnumerable<DragablzItem> dragablzItems = tcPlugin.GetOrderedHeaders();
                if (dragablzItems != null)
                {
                    foreach (DragablzItem dragablzItem in dragablzItems)
                    {
                        TabItem tabItem = dragablzItem.DataContext as TabItem;
                        if (tabItem != null)
                        {
                            int pluginKey = (int)tabItem.Tag;
                            if (pluginKey == (int)PluginKeyEnum.TradingPlugin)
                            {
                                dragablzItem.SetCloseVisibility(Visibility.Collapsed);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

        }


        NotifyCloseTabItem notifyCloseTabItem;
        public void OnTabItemClose(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (args.DragablzItem.DataContext != null)
            {
                TabItem selectItem = args.DragablzItem.DataContext as TabItem;
                if (notifyCloseTabItem == null)
                {
                    notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                }
                notifyCloseTabItem.Publish(selectItem.GetHashCode());
                if (selectItem != null)
                {
                    int pluginKey = (int)selectItem.Tag;
                    if (pluginKey > 0)
                        FrameAdapter.Application.PluginManager.DisEnablePlugin(pluginKey);
                }
            }
        }

        private void mainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }



        private void mainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (LocalEnv.ConfInfo.LoginType == "1")
            {
                // 显示后启动更新
                ClientUpdate.Instance.Init();
                ClientUpdate.Instance.StartUpdateClient(LocalEnv.ConfInfo.LoginType);
            }
        }

        #region 上下分割窗口记忆
        private void BtnHor_Click(object sender, RoutedEventArgs e)
        {
            //未收缩
            if (btnHor.Content.ToString() == "∨")
            {
                Row0.Height = new GridLength(60, GridUnitType.Star);
                Row2.Height = new GridLength(0);
                btnHor.Content = "∧";
            }
            else
            {
                Row0.Height = new GridLength(60, GridUnitType.Star);
                Row2.Height = new GridLength(40, GridUnitType.Star);
                btnHor.Content = "∨";
            }

            SaveLastExecutionsHeight();
        }

        private void gridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SaveLastExecutionsHeight();
        }

        public void SaveLastExecutionsHeight()
        {
            IniFileHelper.WriteValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "MainWindowTopHeight", Row0.Height.ToString());
            IniFileHelper.WriteValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "MainWindowBottomHeight", Row2.Height.ToString());
            if (Row2.Height.Value > 0)
            {
                btnHor.Content = "∨";
            }
            IniFileHelper.WriteValue($"MainSplitter_{ LocalEnv.UserInfo.UserName}", "ExpandButtonCollapsed", btnHor.Content.ToString() == "∧" ? "1" : "0");
            IniFileHelper.RefreshIni();
        }
        #endregion


        #region window任务栏加菜单
        
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_SYSCOMMAND)
                switch (wideParam.ToInt32())
                {
                    case Win32Helper.IDM_ABOUT:
                        //MessageBox.Show("您点击了窗体新添加的任务栏菜单", "消息");
                        RestoreNormal(true);
                        break;
                    default:
                        break;
                }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr m_Hwnd = new IntPtr();

        private IntPtr mNextClipBoardViewerHWnd;
        /// <summary>
        /// WPF窗体的资源初始化完成，并且可以通过WindowInteropHelper获得该窗体的句柄用来与Win32交互后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 获取窗体句柄
            m_Hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(m_Hwnd);
            ////挂消息钩子
            //mNextClipBoardViewerHWnd = SetClipboardViewer(hWndSource.Handle);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        #endregion

        #region FQ消息提醒
        private void OnAppMsg(object msg)
        {
            STC.Common.Model.Fix.Message.PResult obj = (STC.Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "getFQ" && !string.IsNullOrEmpty(obj.Info1) && !string.IsNullOrEmpty(obj.Info2))
            {
                try
                {
                    string winHeader = obj.Info1;
                    string key = obj.Info2 ?? "";
                    string value = obj.Info3 ?? "";
                    if (key == "ShowFQMessage")
                    {
                        WindowIconHelper.Instance.ShowNotifyMessage(obj.Info4, value, 5000);
                    }
                }
                catch
                { }
            }
        }
        #endregion
    }
}
