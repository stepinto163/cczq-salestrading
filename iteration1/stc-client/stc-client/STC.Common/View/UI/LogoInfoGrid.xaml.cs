using com.gateway.connector;
using com.gateway.log;
using Dragablz;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.View.UI
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


        private readonly string _ModelId = "4000";
        private readonly string _Code = "4002";

        public LogoInfoGrid()
        {
            InitializeComponent();

            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            LocalEnv.IsHaveTradeBookPermission = isPermission;
            //btnMsg.Visibility = isPermission ? Visibility.Visible : Visibility.Collapsed;

            traderStatusDic.Add("7", "开市中");
            traderStatusDic.Add("2", "开盘中");
            traderStatusDic.Add("8", "交易暂停");
            traderStatusDic.Add("3", "收盘中");
            traderStatusDic.Add("9", "闭市中");
            traderStatusDic.Add("18", "集中报价开始时刻");
            traderStatusDic.Add("22", "集中报价结束时刻");
            traderStatusDic.Add("19", "集中匹配开始时刻");
            DesignHandler.Instance.OnSystemExit += Instance_OnSystemExit;
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            //设置自定义图片
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "gtja":
                    //设置自定义图片
                    TitleIconTop.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_gtja_daohe.png"));
                    TitleIconTop.Opacity = 0.7;
                    TitleIconLeft.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_gtja_cwjry.png"));
                    TitleIconLeft.Opacity = 0.6;
                    TitleIconMiddle.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_gtja_icon.png"));
                    TitleIconMiddle.Opacity = 0.7;
                    TitleIconRight.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_gtja_ficc.png"));
                    TitleIconRight.Opacity = 0.7;
                    //btnSkin.Visibility = Visibility.Visible;
                    break;
                case "bdb":
                case "idcclient":
                    btnRedirect.Visibility = Visibility.Collapsed;
                    Grid.SetColumn(btnSetting, 5);
                    Grid.SetColumn(bdStatus, 4);
                    tradeTX.Visibility = Visibility.Collapsed;
                    gridTrader.MinHeight = 0;
                    break;
                default:
                    break;
            }

            DesignHandler.Instance.OnNotifyWindowState += Instance_OnNotifyWindowState;
        }

        private void Instance_OnSystemExit(object obj)
        {
            if ((bool)obj == true)
            {
                btnRedirect_Click(null, null);
            }
            else
            {
                btnClose_Click(null, null);
            }
        }

        public event Action<object, RoutedEventArgs> OnMinClick;
        public event Action<object, RoutedEventArgs> OnMaxClick;
        public event Action<object, RoutedEventArgs> OnCloseClick;

        public void SubscribeConStatus()
        {
            InitGridUI();

            if (LocalEnv.ConfInfo.ServerStatus.Count > 0)
            {
                tbError.Text = string.Format("{0}{1}", LocalEnv.ConfInfo.ServerStatus.Count, ErrorTxt);
                tbConStatusTxt.Text = this.ConStatusError;
                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red7");
                tbError.Visibility = Visibility.Visible;
                //grid.RowDefinitions[1].Height = new GridLength(65);
                bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.Red7");
            }
            else
            {
                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Gray14");
                tbError.Visibility = Visibility.Collapsed;
                tbConStatusTxt.Text = this.ConStatusNormal;
                tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.While");
                bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Green14");

                this.tbStatus.Visibility = Visibility.Collapsed;
                this.tbStatusTxt.Visibility = Visibility.Collapsed;
                this.tradeTX.Visibility = Visibility.Collapsed;
                this.priceTX.Visibility = Visibility.Collapsed;
                this.gridPrice.Visibility = Visibility.Collapsed;
                this.gridTrader.Visibility = Visibility.Collapsed;
                BorderConnectStatus.Visibility = Visibility.Collapsed;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                if (LocalIntegrationProfile.Current.Enabled)
                {
                    return;
                }
                try
                {
                    TBookHandler.Instance.SubscribeConStatus(Instance_OnConStatus);
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });

            if (LocalEnv.ConfInfo.ApiKey.Equals(ApiConsts.API_KEY_GW))
                ApiManager.Instance.GetDefaultApi().SubScribeConStatus(this.OnGateConnectStatus);
            
        }

        private void OnGateConnectStatus(object code)
        {
            if (code != null)
            {
                int status = (int)code;
                switch (status)
                {
                    case IEventListener.CONNECTION_CLOSED:
                       
                        ConnectClose();

                        break;

                    case IEventListener.CONNECTION_LOGIN_SUCCESS:
                        //ApiManager.Instance.GetDefaultApi().UnSubScribeConStatus(OnGateConnectStatus);

                        break;

                    case IEventListener.CONNECTION_SUCCESS:
                  

                        this.Dispatcher.Invoke(() =>
                        {
                            bdSTC.Background = (Brush)this.FindResource("ColorBrush.Green14");
                            tbSTC.Foreground = (Brush)this.FindResource("ColorBrush.While");
                        });

                        break;

                    case IEventListener.CONNECTION_LOGOUT:
                        Console.WriteLine("CONNECTION_LOGOUT");

                        LogHelper.Info("GateWay CONNECTION_LOGOUT");

                        break;
                }
            }
        }

        private void InitGridUI()
        {
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    CustomStatusShow(ApiConsts.Price, gridPrice);
                    break;
                default:
                    CustomStatusShow(ApiConsts.Trade, gridTrader);
                    CustomStatusShow(ApiConsts.Price, gridPrice);
                    break;
            }
          
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

            int rowTotal = (serverStatus.Count % 3) == 0 ? serverStatus.Count % 3 : serverStatus.Count % 3 + 1;

            if (rowTotal > 0)
            {
                for (int i = 0; i < rowTotal; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }
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
            tbSTC.Text = LocalEnv.ServerInfo.AppName.ToString();
        }

        private void ConnectClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                bdSTC.Background = (Brush)this.FindResource("ColorBrush.Red7");
                tbSTC.Foreground = (Brush)this.FindResource("ColorBrush.Red7");

                int count = 1;
                foreach (UIElement ui in gridPrice.Children)
                {
                    ServerStatusUI ss = ui as ServerStatusUI;
                    if (ss != null)
                    {
                        ss.SetShowBg(bdSTC.Background, tbSTC.Foreground);
                        count++;
                    }
                }

                foreach (UIElement ui in gridTrader.Children)
                {
                    ServerStatusUI ss = ui as ServerStatusUI;
                    if (ss != null)
                    {
                        ss.SetShowBg(bdSTC.Background, tbSTC.Foreground);
                        count++;
                    }
                }

                //popMsg.Height = 158;
                tbError.Text = string.Format("{0}{1}", count, ErrorTxt);
                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red7");
                //grid.RowDefinitions[1].Height = new GridLength(65);
                tbError.Visibility = Visibility.Visible;

                tbConStatusTxt.Text = this.ConStatusError;
                bdConStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                tbConStatusTxt.Foreground = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
            });
        }

        #region 链接状态更新

        private Brush getShowBG(string status)
        {
            Brush result = null;
            if ("1".Equals(status))
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
            if ("1".Equals(status))
                result = (Brush)this.FindResource("ColorBrush.While");
            else
                result = (Brush)this.FindResource("ColorBrush.Red7");

            return result;
        }

        private ServerStatusUI GetServerStatusUI(SessionStatus sessionStatus)
        {
            ServerStatusUI result = null;

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
                        result = ss;
                        break;
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

                    switch (LocalEnv.ServerInfo.AppName.ToLower())
                    {
                        case "bdb":
                        case "idcclient":
                            //idb 隐藏
                            break;
                        default:
                            tbStatus.Visibility = tbStatusTxt.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                            break;
                    }
                            
                }
            }
        }

        private void Instance_OnConStatus(List<SessionStatus> sessionStatus)
        {
            ThreadPool.QueueUserWorkItem(o => 
            {
                if (sessionStatus != null && sessionStatus.Count > 0)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        int count = 0;
                        bool needRefresh = false;
                        
                        //如果只有GR，并且没有打开其它连接监控，则当STC链接恢复时，修改主连接状态
                        int connstatus = sessionStatus.Where( p => p.TargetSessionId == "TRADE" && p.VenueTypeGW == "INNER" && p.NetStatus == "1").ToList().Count();
                        if (connstatus == 1 && LocalEnv.ConfInfo.ServerStatus.Count == 0)
                        {
                            border.BorderBrush = (Brush)this.FindResource("ColorBrush.Gray14");
                            tbError.Visibility = Visibility.Collapsed;
                            //grid.RowDefinitions[1].Height = new GridLength(33);
                            tbConStatusTxt.Text = this.ConStatusNormal;
                            tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.While");
                            bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Green14");
                            return;
                        }

                        

                        foreach (SessionStatus ss in sessionStatus)
                        {
                            Brush brushBg = getShowBG(ss.NetStatus);
                            Brush brushForbg = getShowForground(ss.NetStatus);

                            ServerStatusUI serverStatusUI = GetServerStatusUI(ss);

                            if (serverStatusUI != null)
                            {
                                serverStatusUI.SetShowBg(brushBg, brushForbg);

                                needRefresh = true;
                            }

                            if (LocalEnv.ConfInfo.ServerStatus.Count > 0)
                            {
                                SetOpenClose(ss);
                            }
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
                                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red7");
                                tbError.Visibility = Visibility.Visible;
                                //grid.RowDefinitions[1].Height = new GridLength(65);
                                bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                                tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.Red7");
                            }
                            else
                            {
                                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Gray14");
                                tbError.Visibility = Visibility.Collapsed;
                                //grid.RowDefinitions[1].Height = new GridLength(33);
                                tbConStatusTxt.Text = this.ConStatusNormal;
                                tbConStatusTxt.Foreground = (Brush)this.FindResource("ColorBrush.While");
                                bdConStatus.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Green14");
                            }
                        }
                    });
                }
            });
           
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                SaveOpenWindows();
            });

            OnCloseClick?.Invoke(sender, e);
        }

        private void Instance_OnNotifyWindowState(object obj)
        {
            if (obj.GetType() == typeof(System.Windows.WindowState))
            {
                WindowState winstate = (System.Windows.WindowState)obj;
                this.Dispatcher.Invoke(() =>
                {
                    if (winstate == WindowState.Normal)
                    {
                        this.btnMax.Checked -= btnMax_Click;
                        this.btnMax.Unchecked -= btnMax_Click;
                        this.btnMax.IsChecked = false;
                        this.btnMax.Checked += btnMax_Click;
                        this.btnMax.Unchecked += btnMax_Click;
                    }
                    else if (winstate == WindowState.Maximized || winstate == WindowState.Minimized)
                    {
                        this.btnMax.Checked -= btnMax_Click;
                        this.btnMax.Unchecked -= btnMax_Click;
                        this.btnMax.IsChecked = true;
                        this.btnMax.Checked += btnMax_Click;
                        this.btnMax.Unchecked += btnMax_Click;
                    }
                });
            }
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

        private string GetCacheKey()
        {
            return string.Format("{0}-{1}", OpenPlugin, LocalEnv.UserInfo.UserName);
        }

        private void WriteHistory()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                IEnumerable<DragablzItem> items = AppUtils.Application.TabablzPanel.GetOrderedHeaders();
                foreach (DragablzItem tab in items)
                {
                    TabItem temItem = tab.DataContext as TabItem;
                    int pluginKey = (int)temItem.Tag;
                    if (pluginKey < 0)//TradeBookPlugin特殊处理
                        pluginKey = 22;
                    IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(pluginKey);
                    if (plugin != null)
                    {
                        sb.Append(string.Format("{0}-{1},", pluginKey, temItem.Header));
                    }
                }

                string selectPlugin = sb.ToString();
                selectPlugin = selectPlugin.Length > 0 ? selectPlugin.Substring(0, selectPlugin.Length - 1) : string.Empty;

                TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
                if (tabItem != null)
                {
                    IniFileHelper.WriteValue(GetCacheKey(), SelectedTagKey, tabItem.Header as string);
                }

                IniFileHelper.WriteValue(GetCacheKey(), OpenPluginKey, selectPlugin);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        public void SaveOpenWindows()
        {
            LogHelper.Info("Save open popups...");
            //XBond 记忆行情小窗窗口
            string[] keys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("QuotesMiniWindows")).ToArray();
            var strkeys = string.Join(",", keys).Replace("QuotesMiniWindows-", "");
            IniFileHelper.WriteValue($"QuotesMiniWindows-{LocalEnv.UserInfo.UserName}", "Keys", strkeys);

            //记忆GR 报价小窗
            string[] grkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("BrokerCardPriceViewWindows")).ToArray();
            var strgrkeys = string.Join(",", grkeys).Replace("BrokerCardPriceViewWindows-", "");
            IniFileHelper.WriteValue($"BrokerCardPrice-{LocalEnv.UserInfo.UserName}", "Keys", strgrkeys);

            //记忆GR 明细小窗，过桥小窗
            string[] cpkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("Transaction")).ToArray();
            var cpgrkeys = string.Join(",", cpkeys).Replace("Transaction-", "");
            IniFileHelper.WriteValue($"GrWin-{LocalEnv.UserInfo.UserName}", "Keys", cpgrkeys);

            //XBond 记忆打开的窗口
            string[] XBondwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.XBond.Plugin")).ToArray();
            var XBondstrwkeys = string.Join(",", XBondwkeys).Replace("STC.XBond.Plugin-", "");
            IniFileHelper.WriteValue($"STC.XBond.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", XBondstrwkeys);


            // 双边做市 记忆策略小窗窗口
            string[] Mmwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Mm.Plugin")).ToArray();
            var Mmstrwkeys = string.Join(",", Mmwkeys).Replace("STC.Mm.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Mm.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Mmstrwkeys);


            // 双边做市 记忆策略行情小窗
            string[] OCwkeysorder = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("OrderControl")).ToArray();
            var OCstrwkeysorder = string.Join(",", OCwkeysorder).Replace("OrderControl-", "");
            IniFileHelper.WriteValue($"OrderControl-{LocalEnv.UserInfo.UserName}", "Keys", OCstrwkeysorder);

            // 策略控制台 记忆窗口
            string[] quantwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.quant.plugin")).ToArray();
            var quantstrwkeys = string.Join(",", quantwkeys).Replace("STC.quant.plugin-", "");
            IniFileHelper.WriteValue($"STC.quant.plugin-{LocalEnv.UserInfo.UserName}", "Keys", quantstrwkeys);

            // 新需求工作台 记忆窗口
            string[] WorkTablewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.WorkTable.Plugin")).ToArray();
            var WorkTablestrwkeys = string.Join(",", WorkTablewkeys).Replace("STC.WorkTable.Plugin-", "");
            IniFileHelper.WriteValue($"STC.WorkTable.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", WorkTablestrwkeys);

            // 需求工作台 记忆窗口
            string[] WorkBenchwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.WorkBench.Plugin")).ToArray();
            var WorkBenchstrwkeys = string.Join(",", WorkBenchwkeys).Replace("STC.WorkBench.Plugin-", "");
            IniFileHelper.WriteValue($"STC.WorkBench.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", WorkBenchstrwkeys);

            // 经纪商成交 记忆窗口
            string[] Executewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Execute.Plugin")).ToArray();
            var Executestrwkeys = string.Join(",", Executewkeys).Replace("STC.Execute.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Execute.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Executestrwkeys);

            // 交易所做市 记忆策略小窗窗口
            string[] Mmshewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Mmshe.Plugin")).ToArray();
            var Mmshestrwkeys = string.Join(",", Mmshewkeys).Replace("STC.Mmshe.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Mmshe.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Mmshestrwkeys);

            // 交易所做市 记忆策略行情小窗
            string[] OCHwkeysorder = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("OrderControlShe")).ToArray();
            var OCHstrwkeysorder = string.Join(",", OCHwkeysorder).Replace("OrderControlShe-", "");
            IniFileHelper.WriteValue($"OrderControlShe-{LocalEnv.UserInfo.UserName}", "Keys", OCHstrwkeysorder);

            // BDB 
            string[] dbdKeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("BDB.Price.Plugin")).ToArray();
            var dbdkey = string.Join(",", dbdKeys).Replace("BDB.Price.Plugin-", "");
            IniFileHelper.WriteValue($"BDB.Price.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", dbdkey);
        }

        private void btnRedirect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogHelper.Info("Restart one...");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SaveOpenWindows();
                });
                // WindowManager.Instance.CloseAllWindow();
                DesignHandler.Instance.OnSettingChange -= Instance_OnSettingChange;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    WriteHistory();
                });

                LogHelper.Info("Restart two...");
                IniFileHelper.WriteValue("StartType", OpenType, "1");
                LogHelper.Info("Restart three...");
                //Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalEnv.ServerInfo.AppExe));
                if (sender == null)
                    System.Environment.SetEnvironmentVariable("isStart", "1");
                else
                    System.Environment.SetEnvironmentVariable("isStart", "0");

                System.Windows.Forms.Application.Restart();
                LogHelper.Info("Restart four...");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LogHelper.Info("Restart five...");
                    Application.Current.Shutdown();
                    LogHelper.Info("Restart six...");
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private bool isSettingShow = false;
        private SettingView settingView = null;

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            //如果BDB，并且不含 XBound插件
            if (STC.Common.Utils.CommonUtils.IsBDBAppName() && !STC.Common.Utils.CommonUtils.IsContainsXBoundPlugin())
            {
                DesignHandler.Instance.showDesignUI(sender);
            }
            else
            {
                if (!WindowManager.Instance.IsExist(nameof(SettingView)))
                {
                    SettingView settingView = new SettingView();
                    DesignHandler.Instance.showDesignUI(settingView);
                    settingView.SetInitSelect();
                    WindowManager.Instance.OpenOnlyOneWindow(nameof(SettingView), settingView);
                }
                else
                {
                    SettingView settingView = WindowManager.Instance.GetExistWin(nameof(SettingView)) as SettingView;
                    if (settingView != null)
                        settingView.SetInitSelect();
                }
            }
        }

        private void SettingView_Closed(object sender, EventArgs e)
        {
            isSettingShow = false;
            settingView = null;
        }

        private void BtnMsg_Click(object sender, RoutedEventArgs e)
        {
            DesignHandler.Instance.SmallbellChange();
        }

        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                SysSettingDic[msg.key] = msg.value;
                if (msg.key == "SystemMsgCount")
                {
                    string count = msg.value;
                    if (string.IsNullOrEmpty(count) || count == "0")
                    {
                        MsgElli.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MsgElli.Visibility = Visibility.Visible;
                    }
                }
                else if (msg.key != "MessageBellPopupStyle" && msg.key != "PopMsgMode")
                {
                    UpdateSettingStatus();
                }
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

        private Point offset;
        private void Pic_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnMaxClick?.Invoke(sender, e);
            }
            else
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    try
                    {
                        offset = e.GetPosition(this);
                        DesignHandler.Instance.WindowDragMove(offset);
                    }
                    catch { }
                }
                if (e.ButtonState == MouseButtonState.Released)
                {
                    Point newPosition = e.GetPosition(this);
                    if (newPosition.X != offset.X || newPosition.Y != offset.Y)
                    {
                        DesignHandler.Instance.SaveWindowPos("");
                    }
                }
            }
        }

        /// <summary>
        /// 皮肤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSkin_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LocalEnv.ConfInfo.DefaultSkinStyle = ConfigurationManager.AppSettings["DefaultSkinStyle"] ?? "DefaultSkin";
                LocalEnv.ConfInfo.DefaultSkinStyle = LocalEnv.ConfInfo.DefaultSkinStyle == "DefaultSkin" ? "BlueSkin" : "DefaultSkin";
                //加载样式
                List<Common.Skin> resources = System.Windows.Application.Current.Resources.MergedDictionaries.Where(d => d is Common.Skin).Cast<Common.Skin>().ToList();
                if (LocalEnv.ConfInfo.DefaultSkinStyle != null)
                {
                    int cnt = System.Windows.Application.Current.Resources.MergedDictionaries.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        System.Windows.ResourceDictionary rd = System.Windows.Application.Current.Resources.MergedDictionaries[i];
                        if (!(rd is Common.Skin))
                        {
                            System.Windows.Application.Current.Resources.MergedDictionaries.Remove(rd);
                            cnt--;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    System.Windows.Application.Current.Resources.MergedDictionaries.Clear();

                    foreach (Common.Skin d in resources)
                    {
                        System.Windows.ResourceDictionary dict1 = new System.Windows.ResourceDictionary();
                        string url = $"pack://application:,,,/STC.Common;component/Resources/{LocalEnv.ConfInfo.DefaultSkinStyle}/{d.Name}.xaml";
                        var skinSource = new Uri(url, UriKind.RelativeOrAbsolute);
                        try
                        {
                            dict1.Source = skinSource;
                            System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict1);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Error(ex.Message);
                        }
                    }

                    System.Windows.ResourceDictionary dict2 = new System.Windows.ResourceDictionary();
                    string url2 = $"pack://application:,,,/STC.Common;component/themes/Generic.xaml";
                    dict2.Source = new Uri(url2, UriKind.RelativeOrAbsolute); ;
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict2);
                }
            });
        }
    }
}
