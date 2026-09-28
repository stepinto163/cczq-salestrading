using com.gateway.connector;
using com.gateway.log;
using Dragablz;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Frame.CustomUI 
{
    /// <summary>
    /// LogoInfoGrid.xaml 的交互逻辑
    /// </summary>
    public partial class LogoInfoGrid : UserControl
    {
        private readonly string ErrorTxt = "个连线中断，请立即处理。";
        private readonly string OpenTxt = "开市中";
        private readonly string CloseTxt = "闭市中";
        private readonly string ConStatusNormal = "连接正常";
        private readonly string ConStatusError = "连接中断";
        private Dictionary<string, string> traderStatusDic = new Dictionary<string, string>();
        private Dictionary<string, string> SysSettingDic = new Dictionary<string, string>();

        public LogoInfoGrid()
        {
            InitializeComponent();

            traderStatusDic.Add("7", "开市中");
            traderStatusDic.Add("2", "开盘中");
            traderStatusDic.Add("8", "交易暂停");
            traderStatusDic.Add("3", "收盘中");
            traderStatusDic.Add("9", "闭市中");
            //traderStatusDic.Add("18", "集中报价开始时刻");
            //traderStatusDic.Add("22", "集中报价结束时刻");
            //traderStatusDic.Add("19", "集中匹配开始时刻");
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;

            // 这里增加风控权限控制, 有下单权限则打开，否则隐藏 
            //if (LocalEnv.UserInfo.IdcTradePermissions != null)
            //{
            //    if (LocalEnv.UserInfo.IdcTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0] == "1")
            //    {
            //        btnSetting.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        btnSetting.Visibility = Visibility.Collapsed;
            //    }
            //}
            //else
            //    btnSetting.Visibility = Visibility.Collapsed;
        }

        private void Win_StateChanged(object sender, EventArgs e)
        {
            if (sender != null && sender is Window)
            {
                Window win = sender as Window;
                if (win != null)
                {
                    if (win.WindowState != WindowState.Maximized)
                    {
                        btnMax.Visibility = Visibility.Visible;
                        btnRestore.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnMax.Visibility = Visibility.Collapsed;
                        btnRestore.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public event Action<object, RoutedEventArgs> OnMinClick;
        public event Action<object, RoutedEventArgs> OnMaxClick;
        public event Action<object, RoutedEventArgs> OnCloseClick;

        public void SubscribeConStatus()
        {
            InitGridUI();

            tbError.Text = string.Format("{0}{1}", LocalEnv.ConfInfo.ServerStatus.Count, ErrorTxt);
            tbConStatusTxt.Text = this.ConStatusError;
            border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red11");
            tbError.Visibility = Visibility.Visible;
            grid.RowDefinitions[1].Height = new GridLength(65);
            bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
            tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.Red7");

            ApiManager.Instance.GetDefaultApi().SubScribeConStatus(OnGateConnectStatus);

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    TBookHandler.Instance.SubscribeConStatus(Instance_OnConStatus);
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
            Atf.MQIII.MQIIIManager.Instance.MQConnectionEvent += Instance_MQConnectionEvent;
            
        }

        private void OnLogOut(object obj)
        {

        }

        private void OnGateConnectStatus(object code)
        {
            if (code != null)
            {
                int status = (int)code;
                switch (status)
                {
                    case IEventListener.CONNECTION_CLOSED:
                        Console.WriteLine("CONNECTION_CLOSED");
                        LogHelper.Info("GateWay CONNECTION_CLOSED");
                        this.Instance_MQConnectionEvent(string.Empty, MQIIIEventStatus.CONNECTION_CLOSED);

                        break;

                    case IEventListener.CONNECTION_LOGIN_SUCCESS:
                        Console.WriteLine("CONNECTION_LOGIN_SUCCESS");
                        LogHelper.Info("GateWay CONNECTION_LOGIN_SUCCESS");
                        this.Instance_MQConnectionEvent(string.Empty, MQIIIEventStatus.CONNECTION_LOGIN_SUCCESS);

                        break;

                    case IEventListener.CONNECTION_SUCCESS:
                        Console.WriteLine("CONNECTION_SUCCESS");
                        LogHelper.Info("GateWay CONNECTION_SUCCESS");
                        this.Instance_MQConnectionEvent(string.Empty, MQIIIEventStatus.CONNECTION_CONNECTED);

                        break;

                    case IEventListener.CONNECTION_LOGOUT:
                        Console.WriteLine("CONNECTION_LOGOUT");
                        
                        LogHelper.Info("GateWay CONNECTION_LOGOUT");
                        LoginHelper.Instance.Logout2("账号已在其他地方登录，请重新登录！");

                        break;
                }
            }
        }

        private void InitGridUI()
        {
            CustomStatusShow(ApiConsts.Trade, gridTrader);
            CustomStatusShow(ApiConsts.Price, gridPrice);
            //CustomStatusShow(ApiConsts.Inner, gridInner);
        }

        private void CustomStatusShow(string targetSessionId, Grid grid)
        {
            int columnIndex = 0;
            int rowIndex = 0;
            List<ServerStatus> serverStatus = LocalEnv.ConfInfo.ServerStatus.Where(delegate (ServerStatus ss)
              {
                  if (ss.TargetSessionId.ToUpper().Equals(targetSessionId.ToUpper()))
                      return true;
                  else
                      return false;
              }).ToList();

            if (serverStatus == null || serverStatus.Count == 0)
            {
                if (targetSessionId == ApiConsts.Trade)
                {
                    gridStatus.RowDefinitions[1].MinHeight = 0;
                    gridStatus.RowDefinitions[1].Height = new GridLength(0);
                }
                else if (targetSessionId == ApiConsts.Price)
                {
                    gridStatus.RowDefinitions[2].MinHeight = 0;
                    gridStatus.RowDefinitions[2].Height = new GridLength(0);
                }
            }
            else
            {
                int rowTotal = (serverStatus.Count % 3) == 0 ? serverStatus.Count % 3 : serverStatus.Count % 3 + 1;

                for (int i = 0; i < rowTotal; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }



                foreach (ServerStatus ss in serverStatus)
                {
                    ServerStatusUI statusUI = new ServerStatusUI();
                    statusUI.Tag = ss;
                    statusUI.SetShowTxt(ss.Name);
                    statusUI.SetShowBg((Brush)this.FindResource("ColorBrush.Red7"), (Brush)this.FindResource("ColorBrush.Red7"));

                    grid.Children.Add(statusUI);

                    Grid.SetColumn(statusUI, columnIndex);
                    Grid.SetRow(statusUI, rowIndex);

                    columnIndex++;

                    if (columnIndex == 3)
                    {
                        rowIndex++;
                        columnIndex = 0;
                    }
                }
                tbError.Text = string.Format("{0}{1}", serverStatus.Count, ErrorTxt);
            }
        }


        private void Instance_MQConnectionEvent(string appName, Atf.MQIII.MQIIIEventStatus status)
        {

            this.Dispatcher.Invoke(() =>
            {
                switch (status)
                {
                    case MQIIIEventStatus.CONNECTION_CLOSED:

                        bdStcLite.Background = (Brush)this.FindResource("ColorBrush.Red7");
                        tbStcLite.Foreground = (Brush)this.FindResource("ColorBrush.Red7");

                        int count = 1;
                        foreach (UIElement ui in gridPrice.Children)
                        {
                            ServerStatusUI ss = ui as ServerStatusUI;
                            if (ss != null)
                            {
                                ss.SetShowBg(bdStcLite.Background, tbStcLite.Foreground);
                                count++;
                            }
                        }

                        foreach (UIElement ui in gridTrader.Children)
                        {
                            ServerStatusUI ss = ui as ServerStatusUI;
                            if (ss != null)
                            {
                                ss.SetShowBg(bdStcLite.Background, tbStcLite.Foreground);
                                count++;
                            }
                        }

                        //popMsg.Height = 158;
                        tbError.Text = string.Format("{0}{1}", count, ErrorTxt);
                        border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red11");
                        grid.RowDefinitions[1].Height = new GridLength(65);
                        tbError.Visibility = Visibility.Visible;

                        tbConStatusTxt.Text = this.ConStatusError;
                        bdConStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                        tbConStatusTxt.Foreground = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");

                        break;
                    case MQIIIEventStatus.CONNECTION_LOGIN_SUCCESS:
                        bdStcLite.Background = (Brush)this.FindResource("ColorBrush.Green14");
                        tbStcLite.Foreground = (Brush)this.FindResource("ColorBrush.While");
                        break;
                    default:
                        break;
                }
            });
        }

        #region 链接状态更新

        private Brush getShowBG(string status)
        {
            Brush result = null;
            if (status.Equals("1"))
                result = (Brush)this.FindResource("ColorBrush.Green14");
            else
            {
                result = (Brush)this.FindResource("ColorBrush.Red7");
            }
            return result;
        }

        private Brush getShowForground(string status)
        {
            Brush result = null;
            if (status.Equals("1"))
                result = (Brush)this.FindResource("ColorBrush.While");
            else
                result = (Brush)this.FindResource("ColorBrush.Red7");

            return result;
        }

        private List<ServerStatusUI> GetServerStatusUI(SessionStatus sessionStatus)
        {
            List<ServerStatusUI> result = new List<ServerStatusUI>();

            Grid grid = null;
            if (sessionStatus.TargetSessionId.ToUpper().Equals(ApiConsts.Trade.ToUpper()))
                grid = gridTrader;
            else
                grid = gridPrice;

            foreach (UIElement ui in grid.Children)
            {
                ServerStatusUI ss = ui as ServerStatusUI;
                if (ss != null)
                {
                    ServerStatus serverStatus = ss.Tag as ServerStatus;
                    if (serverStatus != null && serverStatus.TargetSessionId.ToUpper().Equals(sessionStatus.TargetSessionId.ToUpper())
                        && serverStatus.VenueTypeGW.ToUpper().Equals(sessionStatus.VenueTypeGW.ToUpper()))
                    {
                        result.Add(ss);
                    }
                }
            }

            return result;
        }

        private void SetOpenClose(SessionStatus sessionStatus)
        {
            if (sessionStatus.TargetSessionId.ToUpper().Equals(ApiConsts.Trade.ToUpper()) && sessionStatus.VenueTypeGW.ToUpper().Equals("ODM".ToUpper()))
            {
                if ((sessionStatus.TradSesStatus + "").Equals("9"))
                    tbStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.Red7");
                else
                    tbStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.While");

                if (!string.IsNullOrEmpty(sessionStatus.TradSesStatus))
                {
                    if (traderStatusDic.ContainsKey(sessionStatus.TradSesStatus))
                        tbStatusTxt.Text = traderStatusDic[sessionStatus.TradSesStatus];
                }
            }
        }

        private void Instance_OnConStatus(List<SessionStatus> sessionStatus)
        {
            if (sessionStatus != null && sessionStatus.Count > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    int count = 0;
                    bool needRefresh = false;
                    foreach (SessionStatus ss in sessionStatus)
                    {
                        Brush brushBg = getShowBG(ss.NetStatus);
                        Brush brushForbg = getShowForground(ss.NetStatus);

                        List<ServerStatusUI> serverStatusUI = GetServerStatusUI(ss);

                        if (serverStatusUI != null && serverStatusUI.Count > 0)
                        {
                            foreach (ServerStatusUI ui in serverStatusUI)
                                ui.SetShowBg(brushBg, brushForbg);

                            needRefresh = true;
                        }

                        SetOpenClose(ss);
                    }


                    if (needRefresh)
                    {
                        Brush brush = (Brush)this.FindResource("ColorBrush.Red7");

                        foreach (UIElement ui in gridTrader.Children)
                        {
                            ServerStatusUI ss = ui as ServerStatusUI;
                            if (ss != null)
                            {
                                if (ss.CheckStatus(brush))
                                    count++;
                            }
                        }

                        foreach (UIElement ui in gridPrice.Children)
                        {
                            ServerStatusUI ss = ui as ServerStatusUI;
                            if (ss != null)
                            {
                                if (ss.CheckStatus(brush))
                                    count++;
                            }
                        }

                        if (count > 0)
                        {
                            tbError.Text = string.Format("{0}{1}", count, ErrorTxt);
                            tbConStatusTxt.Text = this.ConStatusError;
                            border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red11");
                            tbError.Visibility = Visibility.Visible;
                            grid.RowDefinitions[1].Height = new GridLength(65);
                            bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                            tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.Red7");
                        }
                        else
                        {
                            border.BorderBrush = (Brush)this.FindResource("ColorBrush.Gray14");
                            tbError.Visibility = Visibility.Collapsed;
                            grid.RowDefinitions[1].Height = new GridLength(33);
                            tbConStatusTxt.Text = this.ConStatusNormal; 
                            tbConStatusTxt.Foreground =(Brush)this.FindResource("ColorBrush.While");
                            bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Green14");
                        }
                    }
                });
            }
        }
        #endregion

        public void setUserName(string txt)
        {
            this.Dispatcher.Invoke(() => 
            {
                tbUserName.Text = txt;
            });
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            OnMinClick?.Invoke(sender, e);
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            OnMaxClick?.Invoke(sender, e);

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseClick?.Invoke(sender, e);
        }

        private void btnStatus_MouseEnter(object sender, MouseEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (!ifLeft)
            {
                popMsg.HorizontalOffset = (popMsg.Width - bdStatus.Width)*(-1);
            }

            popMsg.IsOpen = true;
        }

        private void btnStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            //Console.WriteLine("popMsg_MouseLeave, x:" + p.X + ", Y:" + p.Y);

            if (p.X > bdStatus.Width || p.X < 0 || p.Y > bdStatus.Height || p.Y < 0)
            {
                popMsg.IsOpen = false;
            }

        }

        private const string OpenPlugin = "OpenPlugin";
        private const string OpenPluginKey = "Plugin";
        private const string SelectedTagKey = "FocusPlugin";
        private const string OpenType = "OpenType";

        private const string Window_height = "Height";
        private const string Window_width = "Width";
        private const string Window_Left = "Left";
        private const string Window_Top = "Top";

        private string GetCacheKey()
        {
            return string.Format("{0}-{1}", OpenPlugin, LocalEnv.UserInfo.UserName);
        }
        
        private void WriteHistory()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<DragablzItem> items = AppUtils.Application.TabablzPanel.GetOrderedHeaders();
            foreach (DragablzItem tab in items)
            {
                TabItem temItem = tab.DataContext as TabItem;
                int pluginKey = (int)temItem.Tag;

                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(pluginKey);
                if (plugin != null)
                {
                    sb.Append(string.Format("{0}-{1},", pluginKey, temItem.Header));
                }
            }

            string selectPlugin = sb.ToString();
            selectPlugin = selectPlugin.Length > 0 ? selectPlugin.Substring(0, selectPlugin.Length - 1) : string.Empty;

            IPlugin focusPlugin = null;
            TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
            if (tabItem != null)
            {
                IniFileHelper.WriteValue(AppUtils.GetCacheKey(), SelectedTagKey, tabItem.Header as string);

            }

            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), OpenPluginKey, selectPlugin);


            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_height, Application.Current.MainWindow.Height.ToString());
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_width, Application.Current.MainWindow.Width.ToString());
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_Left, Application.Current.MainWindow.Left.ToString());
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), Window_Top, Application.Current.MainWindow.Top.ToString());
        }

        private void btnRedirect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DesignHandler.Instance.OnSettingChange -= Instance_OnSettingChange;
                WriteHistory();
                IniFileHelper.WriteValue("StartType", OpenType, "1");
                //Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalEnv.ServerInfo.AppExe));
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.showDesignUI(sender);
        }

        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                SysSettingDic[msg.key] = msg.value;
                UpdateSettingStatus();
            }
        }

        private void UpdateSettingStatus()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                bool alliszero = true;
                foreach (var item in SysSettingDic)
                {
                    if (item.Value == "1")
                    {
                        elli.Visibility = Visibility.Visible;
                        alliszero = false;
                    }
                }
                if (alliszero)
                {
                    elli.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void btnFixFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btnFixFloatButton.IsChecked.Value)
            {
                Application.Current.MainWindow.Topmost = true;

            }
            else
            {
                Application.Current.MainWindow.Topmost = false;

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window win = Window.GetWindow(this);
            if (win != null)
            {
                win.StateChanged += Win_StateChanged;
            }
        }
    }
}
