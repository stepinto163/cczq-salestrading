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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// PopupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopupNoBorder : Window
    {
        public Window myWindow;
        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private bool isMax = false;
        private bool isRestore = false;
        private readonly string BridgeWinName = "QuotesMiniWindows";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        public string SecurityID = "";
        private readonly string IsTopMost = "IsTopMost";
        public PopupNoBorder()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            myWindow = this;
            this.Loaded += PopupNoBorder_Loaded;
            this.Unloaded += PopupNoBorder_Unloaded;
        }

        public PopupNoBorder(string securityID)
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            this.SecurityID = securityID.Split('.')[0];
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
            ReStoreWindow();
            this.SizeChanged += PopupNoBorder_SizeChanged;
        }

        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                
                    PopupNoBorder po = (PopupNoBorder)sender;
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
            PopupNoBorder po = (PopupNoBorder)sender;
            var p = e.GetPosition(po);
            if (p.Y > 0 && p.Y <= 30)
            {
                if (!isMax)
                {
                    this.SizeChanged -= PopupNoBorder_SizeChanged;
                    SaveWindowPos();
                    this.Height = mainScreen.WorkingArea.Height;
                    this.Top = mainScreen.WorkingArea.Y;
                    isMax = true;
                    this.SizeChanged += PopupNoBorder_SizeChanged;
                }
                else
                {
                    this.SizeChanged -= PopupNoBorder_SizeChanged;
                    ReStoreWindow();
                    isMax = false;
                    this.SizeChanged += PopupNoBorder_SizeChanged;
                }
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

        private void ReStoreWindow()
        {
            string height = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight);
            string width = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth);

            double d = 0;
            if (double.TryParse(height, out d))
                this.Height = d;
            if (double.TryParse(width, out d))
                this.Width = d;

            string positionX = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX);
            string positionY = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY);
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

            string topmost = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", IsTopMost);
            if (string.IsNullOrEmpty(topmost))
            {
                this.Topmost = false;
            }
            else
            {
                this.Topmost = topmost == "1" ? true : false;
            }
        }

        private void SaveWindowPos()
        {
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinHeight, this.Height + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", WinWidth, this.Width + "");

            //if (LocalEnv.ConfInfo.ExecPosition)
            //{
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionX, this.Left + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", PositionY, this.Top + "");
            //}

            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{SecurityID}", IsTopMost, this.Topmost ? "1" : "0");
        }
    }
}
