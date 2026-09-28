using STC.Common.Utils;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace STC.Common.View.UI
{
    /// <summary>
    /// NoBorderWin.xaml 的交互逻辑
    /// </summary>
    public partial class BorderWin : Window
    {
        public Window myWindow;
        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private string BridgeWinName = "OrderControl";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string DefaultWinHeight = "DefaultHeight";
        private readonly string DefaultWinWidth = "DefaultWidth";
        public string SecurityID = string.Empty;
        public string CommandParameter = string.Empty;  //窗口参数
        private bool IsSaveHeightWidth = false;
        private bool IsSavePosition = false;
        public event Action<object, RoutedEventArgs> OnMinClick;
        public event Action<object, RoutedEventArgs> OnMaxClick;
        public event Action<object, RoutedEventArgs> OnCloseClick;
        private DependencyPropertyChangedEventArgs nameof(string securityID)
        {
            throw new NotImplementedException();
        }

        private readonly string IsTopMost = "IsTopMost";
        private readonly bool IsAlwaysTopmost = false;
        public BorderWin(bool isTopMost = false)
        {
            InitializeComponent();
            myWindow = this;
            this.Loaded += PopupNoBorder_Loaded;
            this.Unloaded += PopupNoBorder_Unloaded;
            this.Topmost = isTopMost;
            IsAlwaysTopmost = isTopMost;
            CommonUtils.SetIcon(TitleIcon);
        }

        public void SetMainControl(object element)
        {
            this.MainFrame.Content = element;
        }


        public object GetMainControl()
        {
            return this.MainFrame.Content;
        }

        /// <summary>
        /// 设置窗体标题
        /// </summary>
        /// <param name="key"></param>
        /// <param name="title"></param>
        public void SetTitleAndKey(string key, string title, string bridgewinname = "Chrome")
        {
            this.BridgeWinName = bridgewinname;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Title = title;
                TextBlockSecurity.Text = title;
                SecurityID = key;
            }));
        }

        public void SetWebPageTitle(string title)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TextBlockSecurity.Text = title;
            }));
        }

        public void SetFixVisible(bool isvisible)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                btnFixFloatButton.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public void SetMinVisible(bool isvisible)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                btnMin.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public void SetMaxVisible(bool isvisible)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                btnMax.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void PopupNoBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PopupNoBorder_Loaded;
            this.Unloaded -= PopupNoBorder_Unloaded;
            RemoveWindowsEvent();
            GC.Collect();
        }

        private void PopupNoBorder_Loaded(object sender, RoutedEventArgs e)
        {
            AddWindowsEvent();
            RestoreNormal();
            this.SizeChanged += BorderWin_SizeChanged;
            // 订阅窗口位置变化事件
            LocationChanged += BorderWin_LocationChanged;
        }


        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    BorderWin po = (BorderWin)sender;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseClick?.Invoke(sender, e);
            SaveWindowPos();
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            btnClose_Click(null, null);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                this.Close();
            }
        }

        public void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    BorderWin po = (BorderWin)sender;
                    var p = e.GetPosition(po);
                    if (p.Y > 0 && p.Y <= 30)
                    {
                        btnMax_Click(sender, e);
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                catch { }
            }));
            
        }

        public void AddWindowsEvent()
        {
            this.MouseDoubleClick += Window_MouseDoubleClick;
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        public void RemoveWindowsEvent()
        {
            this.MouseDoubleClick -= Window_MouseDoubleClick;
            this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
        }

        public void RemoveWindowsMouseDoubleClickEvent()
        {
            this.MouseDoubleClick -= Window_MouseDoubleClick;
        }

        private void RestoreNormal(bool isReset = false)
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


                    string positionX = IniFileHelper.ReadValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX);
                    string positionY = IniFileHelper.ReadValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY);
                    IsSavePosition = (!string.IsNullOrEmpty(positionX)) && (!string.IsNullOrEmpty(positionY));

                    double dY = 0;
                    double d = 0;

                    string lastheight = IniFileHelper.ReadValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight);
                    string lastwidth = IniFileHelper.ReadValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth);
                    IsSaveHeightWidth = (!string.IsNullOrEmpty(lastheight)) && (!string.IsNullOrEmpty(lastwidth));

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


                        if (IsSavePosition)
                        {
                            double lastdY = 0;
                            double lastd = 0;
                            this.MaxWidth = screen.WorkingArea.Width;
                            this.MaxHeight = screen.WorkingArea.Height;
                            if (double.TryParse(lastheight, out lastdY))
                                this.Height = lastdY;
                            if (double.TryParse(lastwidth, out lastd))
                                this.Width = lastd;
                        }
                    }
                    else
                    {
                        this.Top = (screen.WorkingArea.Height - this.Height) / 2 + screen.WorkingArea.Top * heightRatio;
                        this.Left = (screen.WorkingArea.Width * widthRatio - this.Width) / 2 + screen.WorkingArea.Left * widthRatio;
                    }


                    //this.WindowState = WindowState.Normal;

                }
                catch { }

                if (IsMaxAndSaveed())
                {
                    btnMax.Checked -= btnMax_Click;
                    btnMax.Unchecked -= btnMax_Click;
                    btnMax.IsChecked = true;
                    btnMax.Checked += btnMax_Click;
                    btnMax.Unchecked += btnMax_Click;
                }
            }));
        }


        public void SaveWindowPos(bool IsSavePost = true)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight, this.ActualHeight + "");
                IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth, this.ActualWidth + "");
                if (IsSavePost)
                {
                    IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX, this.Left + "");
                    IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY, this.Top + "");
                }
                IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", IsTopMost, this.Topmost ? "1" : "0");
                IniFileHelper.WriteValue($"{BridgeWinName}_{LocalEnv.UserInfo.UserName}_{SecurityID}", "CommandParameter", this.CommandParameter);

                IsSavePosition = true;
            }));
        }

        /// <summary>
        /// 设置窗口启动参数，用于恢复弹出窗口
        /// </summary>
        /// <param name="parameter"></param>
        public void SetCommandParameter(string parameter)
        {
            this.CommandParameter = parameter;
        }



        private double normalWidth = 0;
        private double normalHeight = 0;
        private double normalLeft = 0;
        private double normalTop = 0;

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            OnMaxClick?.Invoke(sender, e);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    this.SizeChanged -= BorderWin_SizeChanged;
                    this.LocationChanged -= BorderWin_LocationChanged;

                    btnMax.Checked -= btnMax_Click;
                    btnMax.Unchecked -= btnMax_Click;

                    if ((btnMax.IsChecked ?? false))
                    {
                        normalWidth = this.Width;
                        normalHeight = this.Height;
                        normalLeft = this.Left;
                        normalTop = this.Top;

                        var window = Window.GetWindow(this);//获取当前主窗口
                        var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                        System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                        Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                        PresentationSource source = PresentationSource.FromVisual(window);

                        Matrix m = source.CompositionTarget.TransformFromDevice;

                        double widthRatio = m.M11;

                        double heightRatio = m.M22;

                        this.MaxWidth = screen.WorkingArea.Width;
                        this.MaxHeight = screen.WorkingArea.Height;
                        this.Width = screen.WorkingArea.Width * widthRatio;
                        this.Height = screen.WorkingArea.Height * heightRatio;
                        this.Left = rect.Left == 0 ? 0 : rect.Left * widthRatio;
                        this.Top = rect.Top == 0 ? 0 : rect.Top * heightRatio;

                        btnMax.IsChecked = true;
                    }
                    else
                    {
                        OnNormalClick();
                    }

                    btnMax.Checked += btnMax_Click;
                    btnMax.Unchecked += btnMax_Click;

                    this.SizeChanged += BorderWin_SizeChanged;
                    this.LocationChanged += BorderWin_LocationChanged;
                    SaveWindowPos();
                }
                catch { }
            }));


        }


        private void OnNormalClick()
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{
            try
            {
                var window = Window.GetWindow(this);//获取当前主窗口
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

        private bool IsMaxAndSaveed()
        {
            string height = this.ActualHeight.ToString();
            string top = this.ActualWidth.ToString();
            double dheight = 0;
            double dtop = 0;
            if (double.TryParse(height, out dheight) && double.TryParse(top, out dtop))
            {
                var window = Window.GetWindow(this);//获取当前主窗口
                var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                PresentationSource source = PresentationSource.FromVisual(window);

                Matrix m = source.CompositionTarget.TransformFromDevice;

                double widthRatio = m.M11;

                double heightRatio = m.M22;

                if (dtop == rect.Width * heightRatio)
                {
                    return true;
                }
            }

            return false;
        }
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            OnMinClick?.Invoke(sender, e);
            this.WindowState = WindowState.Minimized;
        }

        private void BorderWin_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowPos();
        }

        private void BorderWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPos();
        }

        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btnFixFloatButton.IsChecked.Value)
            {
                if (this != null)
                {
                    this.Topmost = true;
                }
            }
            else
            {
                if (this != null)
                {
                    this.Topmost = false;
                }
            }
        }
    }
}
