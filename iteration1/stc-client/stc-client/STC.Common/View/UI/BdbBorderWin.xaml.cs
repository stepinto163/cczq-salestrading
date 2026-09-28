using STC.Common.Model.Fix.Message;
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
    public partial class BdbBorderWin : Window
    {
        public Window myWindow;
        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private string BridgeWinName = "OrderControl";

        public string CommandParameter = string.Empty;  //窗口参数
        
        public event Action<object, RoutedEventArgs> OnCloseClick;

        public string SecurityID = string.Empty;

        private readonly string IsTopMost = "IsTopMost";
        private readonly bool IsAlwaysTopmost = false;
        public BdbBorderWin(bool isTopMost = false)
        {
            InitializeComponent();
            myWindow = this;
            this.Loaded += PopupNoBorder_Loaded;
            this.Unloaded += PopupNoBorder_Unloaded;
            this.Topmost = isTopMost;
            SetConnectStatus(false);
            IsAlwaysTopmost = isTopMost;
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
            this.Closing += MainWindow_Closing;
        }

        private void HideOldTokenUI()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                chkIsConnect.Visibility = Visibility.Collapsed;
                btnReconnect.Visibility = Visibility.Collapsed;
            }));
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //可选：隐藏窗口而不是关闭
            this.Hide();
            // 取消关闭操作
            e.Cancel = true;
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
            this.Tag = key;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Title = title;
                TextBlockSecurity.Text = title;
                SecurityID = key;
            }));
        }

        public void SetConnectStatus(bool isConnected = false, string userinfo = "")
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                chkIsConnect.IsChecked = isConnected;
                TextUserInfo.Text = !isConnected ? $"金融Q(用户信息未获取)" : userinfo;
                //btnReconnect.Visibility = btnConnectService.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
            }));
        }


        public void SetWebPageTitle(string title)
        {
            //BDB报价助手不修改标题
            ////Application.Current.Dispatcher.Invoke(new Action(() =>
            ////{
            ////    TextBlockSecurity.Text = title;
            ////}));
        }

        public void SetFixVisible(bool isvisible)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                btnFixFloatButton.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void PopupNoBorder_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void PopupNoBorder_Loaded(object sender, RoutedEventArgs e)
        {
            AddWindowsEvent();
        }


        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    BdbBorderWin po = (BdbBorderWin)sender;
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
            if (this.Tag != null && this.Tag.ToString() == "WebFQ")
            {
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "HideWebFQ", Info3 = "" });
                return;
            }
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
                if (this.Tag != null && this.Tag.ToString() == "WebFQ")
                {
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "HideWebFQ", Info3 = "" });
                    return;
                }
            }
        }

        public void AddWindowsEvent()
        {
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        public void RemoveWindowsEvent()
        {
            this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
        }

        /// <summary>
        /// 设置窗口启动参数，用于恢复弹出窗口
        /// </summary>
        /// <param name="parameter"></param>
        public void SetCommandParameter(string parameter)
        {
            this.CommandParameter = parameter;
        }


        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btnFixFloatButton.IsChecked.Value)
            {
                if (this != null)
                {
                    this.Topmost = true;
                    btnFixFloatButton.ToolTip = "取消置顶";
                }
            }
            else
            {
                if (this != null)
                {
                    this.Topmost = false;
                    btnFixFloatButton.ToolTip = "置顶";
                }
            }
        }

        /// <summary>
        /// 如果重新连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReconnect_Click(object sender, RoutedEventArgs e)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "GetToken", Info3 = "" });
        }

        private Model.QQ.FQBindTokenResponse fQBindTokenResponse;

        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "getFQ" && !string.IsNullOrEmpty(obj.Info1) && !string.IsNullOrEmpty(obj.Info2))
            {
                try
                {
                    string winHeader = obj.Info1;
                    string key = obj.Info2 ?? "";
                    string value = obj.Info3 ?? "";

                    if (key == "LocalSendToken")
                    {
                        if (!(string.IsNullOrEmpty(value)))
                        {
                            Model.QQ.FQBindTokenResponse res = STC.Common.Utils.CommonUtils.DeserializeObject<Model.QQ.FQBindTokenResponse>(value);
                            fQBindTokenResponse = res;
                            string userinfo = $"金融Q({res.userInfo.name}/{res.userInfo.corpname} {res.userInfo.qq})";
                            if (!string.IsNullOrEmpty(userinfo))
                            {
                               SetConnectStatus(true, userinfo);
                            }
                        }
                    }
                    else if (key == "LocalSendTokenFanout")
                    {
                        HideOldTokenUI();
                        string userinfo = string.Empty;
                        if (!(string.IsNullOrEmpty(value)))
                        {
                            Model.QQ.FQBindTokenResponse res = STC.Common.Utils.CommonUtils.DeserializeObject<Model.QQ.FQBindTokenResponse>(value);
                            fQBindTokenResponse = res;
                            userinfo = $"金融Q({res.userInfo.name}/{res.userInfo.corpname} {res.userInfo.qq})";
                            if (!string.IsNullOrEmpty(userinfo))
                            {
                                SetConnectStatus(true, userinfo);
                            }
                        }
                        else
                        {
                            userinfo = $"金融Q(用户信息未获取)";
                            SetConnectStatus(true, userinfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    com.gateway.log.LogHelper.Error(ex.Message);
                }
            }
        }

        private void btnConnectService_Click(object sender, RoutedEventArgs e)
        {
            AppUtils.Application.PluginManager.Application.BroadCast("PopUpQBAssistant", null);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            MainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12696B"));
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            MainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33AAA6"));
        }
    }

}
