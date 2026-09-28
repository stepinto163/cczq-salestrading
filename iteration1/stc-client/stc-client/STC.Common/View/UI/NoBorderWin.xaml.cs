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
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// NoBorderWin.xaml 的交互逻辑
    /// </summary>
    public partial class NoBorderWin : Window
    {
        public Window myWindow;
        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private bool isMax = false;
        private bool isRestore = true;
        public string BridgeWinName = "OrderControl";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        public string SecurityID = "";
        private readonly string IsTopMost = "IsTopMost";
        private readonly bool IsAlwaysTopmost = false;
        
        public NoBorderWin()
        {
            InitializeComponent();
            Icon = LocalEnv.ServerInfo.AppImage;
            myWindow = this;
            Loaded += PopupNoBorder_Loaded;
            Unloaded += PopupNoBorder_Unloaded;
        }

        public NoBorderWin(string securityID, bool isAlwaysTopmost = false, bool isrestore = true)
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            this.SecurityID = securityID.Split('.')[0];
            IsAlwaysTopmost = isAlwaysTopmost;
            isRestore = isrestore;
            myWindow = this;
            this.Loaded += PopupNoBorder_Loaded;
            this.Unloaded += PopupNoBorder_Unloaded;
        }

        public void SetMainControl(object element)
        {
            this.MainFrame.Content = element;
        }

        private void PopupNoBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PopupNoBorder_Loaded;
            this.Unloaded -= PopupNoBorder_Unloaded;
            this.SizeChanged -= PopupNoBorder_SizeChanged;
            RemoveWindowsEvent();
        }

        private void PopupNoBorder_Loaded(object sender, RoutedEventArgs e)
        {
            AddWindowsEvent();
            if (isRestore)
                ReStoreWindow();
            this.SizeChanged += PopupNoBorder_SizeChanged;
            // 订阅窗口位置变化事件
            LocationChanged += NoBorderWin_LocationChanged;
        }

        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {

                    NoBorderWin po = (NoBorderWin)sender;
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
            NoBorderWin po = (NoBorderWin)sender;
            var p = e.GetPosition(po);
            if (p.Y > 0 && p.Y <= 30)
            {
                this.SizeChanged -= PopupNoBorder_SizeChanged;
                if (!IsMaxAndSaveed())
                {
                    var window = Window.GetWindow(this);//获取当前主窗口
                    var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                    Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                    PresentationSource source = PresentationSource.FromVisual(window);
                    Matrix m = source.CompositionTarget.TransformFromDevice;
                    double widthRatio = m.M11;
                    double heightRatio = m.M22;
                    this.MaxHeight = screen.WorkingArea.Height;
                    this.Height = screen.WorkingArea.Height * heightRatio;
                    this.Top = rect.Top == 0 ? 0 : rect.Top * heightRatio;
                    SaveWindowPos();
                }
                else
                {
                    RestorWindowDefault();
                }
                this.SizeChanged += PopupNoBorder_SizeChanged;
            }
            else
            {
                e.Handled = true;
            }
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

        private void PopupNoBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPos();
        }

        private void NoBorderWin_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowPos();
        }

        private bool IsSavePosition = false;
        private bool IsSaveHeightWidth = false;
        private void ReStoreWindow()
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


                    string positionX = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX);
                    string positionY = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY);
                    IsSavePosition = (!string.IsNullOrEmpty(positionX)) && (!string.IsNullOrEmpty(positionY));

                    double dY = 0;
                    double d = 0;

                    string lastheight = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight);
                    string lastwidth = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth);
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
                        this.Top = (screen.WorkingArea.Height * heightRatio - this.Height) / 2 + screen.WorkingArea.Top * heightRatio;
                        this.Left = (screen.WorkingArea.Width * widthRatio - this.Width) / 2 + screen.WorkingArea.Left * widthRatio;
                    }


                    //this.WindowState = WindowState.Normal;

                }
                catch { }

                string topmost = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", IsTopMost);
                if (!IsAlwaysTopmost)
                {
                    if (string.IsNullOrEmpty(topmost))
                    {
                        this.Topmost = true;
                    }
                    else
                    {
                        this.Topmost = topmost == "1" ? true : false;
                    }
                }
                else
                {
                    this.Topmost = true;
                }
            }));
        }

        private void RestorWindowDefault()
        {
            this.Height = 483;
            this.Width = 286;
            this.WindowState = WindowState.Normal;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var window = Window.GetWindow(this);//获取当前主窗口

            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
            Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

            PresentationSource source = PresentationSource.FromVisual(window);

            Matrix m = source.CompositionTarget.TransformFromDevice;
            double heightRatio = m.M22;
             this.Top = (rect.Height * heightRatio - this.Height) / 2;
            SaveWindowPos();
        }

        private void SaveWindowPos()
        {
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight, this.ActualHeight + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth, this.ActualWidth + "");

            //if (LocalEnv.ConfInfo.ExecPosition)
            //{
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX, this.Left + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY, this.Top + "");
            //}

            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", IsTopMost, this.Topmost ? "1" : "0");
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

                if (dheight == screen.WorkingArea.Height * heightRatio)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
