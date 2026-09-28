using com.gateway.connector;
using com.gateway.log;
using Dragablz;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.PT;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.CustomeUI;
using STC.Frame.CustomUI;
using STC.Frame.UI;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WinInterop = System.Windows.Interop;

namespace STC.Frame
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window, IForm
    {
        private const string OpenPlugin = "OpenPlugin";
        private const string OpenPluginKey = "Plugin";
        private const string SelectedTagKey = "FocusPlugin";

        private const string Notice = "Notice";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string BridgeWinName = "MainWindow";
        private bool _needShowNotic = false;

        private bool _isInit = true;

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
        public double? GrMinHeight;
        public double? GrMinWidth;

        private SysWarnGUI sysWarnGUI = null;
        private double defaultWidth = 0;
        private int ts = 0;
        private long lastTick = 0;
        private string LockScreen = "0";
        Window LoginWin;
        private Popup popup = null;
        private Window mainWin = null;

        public Window1(Window w)
        {
            InitializeComponent();
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE || LocalEnv.ConfInfo.IsPAYL)
            {
                tcPlugin.ShowDefaultAddButton = false;
            }
            this.Icon = LocalEnv.ServerInfo.AppImage;
            LoginWin = w;
            mainWin = this;
            //Initialize();
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



            //this.lbSysName.Content = LocalEnv.ServerInfo.AppName;
            this.Title = LocalEnv.ServerInfo.AppName;
            //this.lbVersionCode.Content = LocalEnv.ServerInfo.Version;

            logoInfo.OnCloseClick += LogoInfo_OnCloseClick;
            logoInfo.OnMaxClick += LogoInfo_OnMaxClick;
            logoInfo.OnMinClick += LogoInfo_OnMinClick;

            SourceInitialized += new EventHandler(win_SourceInitialized);

            tcPlugin.Items.Clear();
            tcPlugin.ClosingItemCallback = OnTabItemClose;
            tcPlugin.NewItemFactory = CreateNewItem;

            ThreadPool.RegisterWaitForSingleObject(BaseMachineHelper._appEvent, OnProgramStarted, null, -1, false);

            //this.KeyDown += Window1_KeyDown;

            // 开启系统空闲监控
            //LockScreen = LocalEnv.ConfInfo.LockScreen;
            ////if (LockScreen != "0")
            ////{
            ////    ts = int.Parse(LocalEnv.ConfInfo.LockScreenMinute) * 60;
            ////    if (ts > 0)
            ////    {
            ////        System.Windows.Interop.ComponentDispatcher.ThreadIdle += ComponentDispatcher_ThreadIdle;
            ////    }
            ////}
            ///
            //this.Initialize();

            DesignHandler.Instance.OnSaveWindowPos += Instance_OnSaveWindowPos;
            DesignHandler.Instance.OnWindowDragMove += Instance_OnWindowDragMove;
            DesignHandler.Instance.OnMax += Instance_OnMax;
            this.PreviewKeyDown += Window1_PreviewKeyDown;
            this.PreviewKeyUp += Window1_PreviewKeyUp;

            InitFindSecurityView();
            
            //加载Gr配置文件
            LoadGrConfig();

            //加载树状分组数据
            if (!netapi.Api.LocalIntegrationProfile.Current.Enabled
                && (LocalEnv.IsHaveTradeBookPermission || LocalEnv.ConfInfo.IsPAYL))
            {
                LoadTreeBookUserConfig();
            }

            //订阅App消息
            STC.Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// //加载Gr配置文件
        /// </summary>
        private void LoadGrConfig()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                //查询GR全局设置
                try
                {
                    SystemParameterReq req = new SystemParameterReq();
                    req.ParamKey = "USER_GR_SETTINGS";
                    req.Username = LocalEnv.UserInfo.UserName;
                    TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                    if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                    {
                        LocalEnv.GrGlobSystemParameter = CommonUtils.DeserializeObject<GrSystemParameter>(reply.Datas.tbookSystemParameter.param_value);
                    }
                    else
                    {
                        if (!LocalEnv.IsHaveTradeBookPermission)
                            LogHelper.Info("请注意：没有查询到 GR全局设置 参数信息！");
                    }
                }
                catch { }

                // 读取GR页面布局设置
                LocalEnv.GrLayoutSetting = LocalEnv.QuoteSettingMode();

                //查询GR自动回复设置
                try
                {
                    SystemParameterReq req = new SystemParameterReq();
                    req.ParamKey = "STC_Broker_Order_Flow";
                    req.Username = LocalEnv.UserInfo.UserName;
                    TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                    if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                    {
                        LocalEnv.BrokerAutoConfirmParam = CommonUtils.DeserializeObject<STC.Common.Model.Drp.BrokerAutoConfirmParam>(reply.Datas.tbookSystemParameter.param_value);
                        //LocalEnv.BrokerAutoConfirmParam.brokerAAutoConfirm_BrokerBPartiallyFiledConfirm = "N";
                    }
                    else
                    {
                        if (!LocalEnv.IsHaveTradeBookPermission)
                            LogHelper.Info("请注意：没有查询到 GR系统弹窗开关 参数信息！");
                    }
                }
                catch
                {
                }
            });
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

        private void InitFindSecurityView()
        {
            popup = new Popup();

            popup.PlacementTarget = this;
            popup.Placement = PlacementMode.Absolute;
            popup.Width = 360;
            popup.Height = 300;


            FindSecurityView ui = new FindSecurityView();
            if (popup.Child != null)
                popup.Child = null;
            popup.Child = ui;
            popup.StaysOpen = false;
        }



        void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        public object CreateNewItem()
        {
            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.TradeBookPlugin);
            if (plugin != null)
            {
                AppUtils.Application.PluginManager.EnablePlugin((int)PluginKeyEnum.TradeBookPlugin);
                // pluginKey是22，表示TradeBook插件， 可以多次打开
                if (!plugin.IsAlone)
                {
                    plugin.Enabled = false;
                }

                if (AppUtils.Application.Panel.Items.Count > 0)
                {
                    foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                    {
                        if (tabItem.Header.ToString().Equals("Tradebook"))
                        {
                            AppUtils.Application.Panel.SelectedItem = tabItem;
                            break;
                        }
                    }
                }
            }


            return null;
        }

        #region 快捷键
        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr m_Hwnd = new IntPtr();

        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private Dictionary<EHotKeySetting, int> m_HotKeySettings = new Dictionary<EHotKeySetting, int>();

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            ObservableCollection<HotKeyModel> hotKeyList = new ObservableCollection<HotKeyModel>(DictHotKey.Where(o => o.IsGlobal == true));
          
            // 注册全局快捷键
            string failList = HotKeyHelper.RegisterGlobalHotKey(hotKeyList, m_Hwnd, out m_HotKeySettings);
            if (!string.IsNullOrEmpty(failList))
                LogHelper.Info(failList);

            //获取非全局快捷键
            DictLocalHotKey = new List<HotKeyModel>(DictHotKey.Where(o => o.IsGlobal == false).ToList());

            
            List<EKey> privatehotKeyList = new List<EKey>(DictHotKey.Where(o => o.IsGlobal == false).Select(o => o.SelectKey).ToList());
            foreach (EKey k in privatehotKeyList)
            {
                Key nekey = (Key)Enum.Parse(typeof(Key), k.ToString());
                LstKey.Add(nekey);
            }

            LstModel = new List<string>(DictHotKey.Where(o => o.IsGlobal == false).Select(o => o.ModelName).ToList().Distinct());

        }

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
            //挂消息钩子
            mNextClipBoardViewerHWnd = SetClipboardViewer(hWndSource.Handle);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 窗体回调函数，接收所有窗体消息的事件处理函数
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="msg">消息</param>
        /// <param name="wideParam">附加参数1</param>
        /// <param name="longParam">附加参数2</param>
        /// <param name="handled">是否处理</param>
        /// <returns>返回句柄</returns>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:

                    if (this.WindowState == WindowState.Minimized)
                    {
                        return IntPtr.Zero;
                    }

                    int sid = wideParam.ToInt32();

                    if (m_HotKeySettings.ContainsValue(sid))
                    {
                        DesignHandler.Instance.TriggerHotKey(m_HotKeySettings.FirstOrDefault(p => p.Value == sid).Key);
                        break;
                    }

                    //handled = true;
                    break;
                case WM_DRAWCLIPBOARD:
                    {
                        SendMessage(mNextClipBoardViewerHWnd, msg, wideParam.ToInt32(), longParam.ToInt32());
                        //文本内容检测
                        if (System.Windows.Clipboard.ContainsText())
                        {
                            try
                            {
                                if (LocalEnv.ConfInfo.IsShowCopyQuoteAlert == "1"
                                    || LocalEnv.ConfInfo.IsAutoQuoteRecognize == "1")
                                {
                                    string text = getClipboardText();
                                    RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                                    request.userName = LocalEnv.UserInfo.UserName;
                                    request.content = text;

                                    Task.Run(() => {
                                        ///获取订单 
                                        RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);
                                        if (reply.IsSuccess())
                                        {
                                            if (LocalEnv.ConfInfo.IsShowCopyQuoteAlert == "1")
                                                OnCopyMessage?.Invoke(text);

                                            if (LocalEnv.ConfInfo.IsAutoQuoteRecognize == "1")
                                                 OnCopyMessage_QuoteRecognize?.Invoke(text);
                                        }
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                               
                            }
                        }
                        //显示剪贴板中的图片信息
                        if (Clipboard.ContainsImage())
                        {
                            if (LocalEnv.ConfInfo.IsAutoQuoteRecognize == "1")
                                OnCopyMessage_QuoteRecognize?.Invoke(null);
                        }
                    }
                    break;
                case WM_CHANGECBCHAIN:
                    {
                        if (wideParam == (IntPtr)mNextClipBoardViewerHWnd)
                        {
                            mNextClipBoardViewerHWnd = longParam;
                        }
                        else
                        {
                            SendMessage(mNextClipBoardViewerHWnd, msg, wideParam.ToInt32(), longParam.ToInt32());
                        }
                    }
                    break;
                case Win32Helper.WM_SYSCOMMAND:
                    //window任务栏加菜单
                    if (wideParam == (IntPtr)Win32Helper.IDM_ABOUT)
                    {
                        //MessageBox.Show("您点击了窗体新添加的任务栏菜单", "消息");
                        RestoreNormal(true);
                    }
                    break;
            }

            return IntPtr.Zero;
        }


        #region 新的全局快捷键
        /// <summary>
        /// 全局HotKey列表
        /// </summary>
        public ObservableCollection<HotKeyModel> DictHotKey = new ObservableCollection<HotKeyModel>();
        public List<HotKeyModel> DictLocalHotKey = new List<HotKeyModel>();
        public List<Key> LstKey = new List<Key>();
        public List<string> LstModel = new List<string>();
        private void Window1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox)
            {
                return;
            }
            string controltype = e.OriginalSource.GetType().ToString();

            if (controltype.Contains("WatermarkTextBox") || controltype.Contains("DrpNumericUpDown") || controltype.Contains("BpNumericUpDown") || controltype.Contains("NumericUpDown"))
            {
                return;
            }

            //线下录入快捷键
            bool isOffLineKeys = (e.Key >= Key.F1 && e.Key <= Key.F12) || (e.Key == Key.System && e.SystemKey == Key.F10);
            //行情切换快捷键
            bool isPricingTabKeys = e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.Tab || e.Key == Key.Space;
            //如果是线下录入或行情切换键的范围
            if (isOffLineKeys || isPricingTabKeys)
            {
                if (!string.IsNullOrWhiteSpace(LocalEnv.GrGlobSystemParameter.HotKey))
                {
                    //线下录入快捷键
                    string OffLineHotKey = this.GetSysHotKey("OFFLINEENTRY");
                    if (e.Key.ToString() == OffLineHotKey || (e.Key == Key.System && e.SystemKey.ToString() == OffLineHotKey))
                    {
                        //发布消息
                        STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TriggerGrCommand", Info1 = "UnderLineExecWin" });
                    }

                    //处理行情切换
                    string PricingTabHotKey = this.GetSysHotKey("PRICINGTABENTRY");
                    if (e.Key.ToString() == PricingTabHotKey || (e.Key == Key.System && e.SystemKey.ToString() == PricingTabHotKey))
                    {
                        foreach (TabItem tabItem in this.Panel.Items)
                        {
                            if (tabItem.IsSelected)
                            {
                                if (tabItem.Tag.ToString() == "10")
                                {
                                    //发布消息
                                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TabControlNeedSelectionChanged", Info1 = "PricingPanel" });
                                }
                                break;
                            }
                        }
                    }
                }
            }


            //DictLocalHotKey 

            //内部看板 Ctrl + V
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

                    string header = ((TabItem)selt).Header.ToString();
                    if (header.Equals("内部看板"))
                    {
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                        {
                            //同时按下了Ctrl + V 键
                            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "InnerBoard_Open_SendRequireView", Msg = "同时按下了Ctrl + V 键" });
                        }
                    }
                    else if (LstModel.Contains(header))
                    {
                        if (LstKey.Contains(e.Key))
                        {
                            string kst = e.Key.ToString();
                            kst = kst == "Next" ? "PageDown" : kst;
                            string kn = DictLocalHotKey.Where(o => o.KeyName == kst).Select(o => o.Name).FirstOrDefault();
                            if (kn != null)
                            {
                                EHotKeySetting nekey = (EHotKeySetting)Enum.Parse(typeof(EHotKeySetting), kn);
                                DesignHandler.Instance.TriggerHotKey(nekey);
                            }

                        }
                    }
                }
            }

            if (!popup.IsOpen)
            {
                if (!(e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.RightAlt || e.Key == Key.LeftAlt) && e.KeyboardDevice.Modifiers != ModifierKeys.Control && e.KeyboardDevice.Modifiers != ModifierKeys.Alt)
                    ShowSearch(e.Key, e.ImeProcessedKey);
            }
            else
            {
                SetKey(e.Key, e.ImeProcessedKey);
            }
        }

        private void Window1_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox)
                return;
            if (e.OriginalSource is System.Windows.Controls.Primitives.DatePickerTextBox)
                return;
            string controltype = e.OriginalSource.GetType().ToString();
            if (controltype.Contains("WatermarkTextBox") || controltype.Contains("DrpNumericUpDown") || controltype.Contains("BpNumericUpDown") || controltype.Contains("NumericUpDown"))
            {
                return;
            }
        }


        //传输键盘Key
        private void SetKey(Key key, Key imeKey)
        {
            if (!LocalEnv.ConfInfo.CanGoodsQuickSearch)
                return;

            Key tmpKey = key;
            if (key == Key.ImeProcessed)
                tmpKey = imeKey;

            if ((tmpKey >= Key.A && tmpKey <= Key.Z) || (tmpKey >= Key.D0 && tmpKey <= Key.D9) || tmpKey == Key.Decimal || tmpKey == Key.Space || (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9))
            {
                FindSecurityView ui = popup.Child as FindSecurityView;
                if (key != Key.ImeProcessed)
                {
                    ui.tbSearch.Text += GetKeyString(tmpKey);
                    ui.tbSearch.SelectionStart = ui.tbSearch.Text.Length;
                }
                else if (key == Key.ImeProcessed && ((tmpKey >= Key.D0 && tmpKey <= Key.D9) || tmpKey == Key.Decimal ||
                    (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9)))
                {
                    ui.tbSearch.Text += GetKeyString(tmpKey);
                    ui.tbSearch.SelectionStart = ui.tbSearch.Text.Length;
                }
            }
        }
        private string GetKeyString(Key key)
        {
            string skey = "";
            if (key >= Key.A && key <= Key.Z)
                skey = (key + "").ToLower();
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
                skey = ((int)key - 74) + "";
            else if (key >= Key.D0 && key <= Key.D9)
                skey = ((int)key - 34) + "";
            else if (key == Key.Decimal)
                skey = ".";
            return skey;
        }

        
        private void ShowSearch(Key key, Key imeKey)
        {
            Key tmpKey = key;
            if (key == Key.ImeProcessed)
            {
                tmpKey = imeKey;

                popup.HorizontalOffset = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.ActualWidth - popup.Width - 2;
                popup.VerticalOffset = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.ActualHeight - popup.Height - 2;

                if (!popup.IsOpen)
                    popup.IsOpen = true;

                string character = imeKey.ToString();
                // 使用 ProcessKey 方法将字符发送给输入法
                // 模拟输入法事件
                System.Windows.Forms.SendKeys.SendWait(character);
                return;
            }

            if ((tmpKey >= Key.A && tmpKey <= Key.Z) ||
              (tmpKey >= Key.D0 && tmpKey <= Key.D9) ||
              tmpKey == Key.Decimal || tmpKey == Key.Space ||
              (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9))
            {
                FindSecurityView ui = popup.Child as FindSecurityView;
                if ((tmpKey >= Key.D0 && tmpKey <= Key.D9) || tmpKey == Key.Decimal ||
                    (tmpKey >= Key.NumPad0 && tmpKey <= Key.NumPad9) || (tmpKey >= Key.A && tmpKey <= Key.Z))
                {

                    ui.tbSearch.Text = GetKeyString(tmpKey);
                    ui.tbSearch.SelectionStart = ui.tbSearch.Text.Length;
                }
                popup.HorizontalOffset = System.Windows.Application.Current.MainWindow.Left + System.Windows.Application.Current.MainWindow.ActualWidth - popup.Width - 2;
                popup.VerticalOffset = System.Windows.Application.Current.MainWindow.Top + System.Windows.Application.Current.MainWindow.ActualHeight - popup.Height - 2;
                popup.IsOpen = true;
            }
        }

        private bool IsValidKey(System.Windows.Forms.Keys key)
        {

            bool flag = false;
            if ((key >= System.Windows.Forms.Keys.A && key <= System.Windows.Forms.Keys.Z) ||
                (key >= System.Windows.Forms.Keys.NumPad0 && key <= System.Windows.Forms.Keys.NumPad9) ||
                (key >= System.Windows.Forms.Keys.D0 && key <= System.Windows.Forms.Keys.D9) ||
                key == System.Windows.Forms.Keys.Decimal ||
                key == System.Windows.Forms.Keys.Space)
            {
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// 获取指定系统快捷键
        /// </summary>
        /// <returns></returns>
        public string GetSysHotKey(string tag)
        {
            string hotKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(LocalEnv.GrGlobSystemParameter.HotKey))
            {
                string[] hotKeyList = LocalEnv.GrGlobSystemParameter.HotKey.Split(',');
                if (hotKeyList.Length == 2 && hotKeyList[0] == LocalEnv.UserInfo.UserName)
                {
                    string[] hotKeyValues = hotKeyList[1].Split('|');
                    foreach (string hotKeyValue in hotKeyValues)
                    {
                        string[] hotKeys = hotKeyValue.Split(':');
                        if (hotKeys.Length == 2)
                        {
                            if (tag == hotKeys[0])
                                hotKey = hotKeys[1];
                        }
                    }
                }
            }
            return hotKey;
        }
        #endregion


        public event Action<object> OnCopyMessage;
        public event Action<object> OnCopyMessage_QuoteRecognize;
        #endregion


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


                    mmi.ptMinTrackSize.x = 600;
                    mmi.ptMinTrackSize.y = 550;
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
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                    this.Focus();
                    this.Show();
                });
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 主窗体最小化
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void LogoInfo_OnMinClick(object arg1, RoutedEventArgs arg2)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 主窗体最大化
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void LogoInfo_OnMaxClick(object arg1, RoutedEventArgs arg2)
        {
            if (WindowState != WindowState.Maximized)
            {
                OnMaxClick();
            }
            else
            {
                OnNormalClick();
            }

            //将小铃铛再次复位
            //DesignHandler.Instance.SetGlobalActivate(false);
        }

        private void OnMaxClick()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
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
            }));
        }

        private void OnNormalClick()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    ReStoreWindow(true);
                    this.WindowState = WindowState.Normal;
                }
                catch { }
            }));
        }

        private void LogoInfo_OnCloseClick(object arg1, RoutedEventArgs arg2)
        {
            Window1_Closed(null, null);
        }

        private void Window1_Closed(object sender, EventArgs e)
        {
            LogHelper.Info("Window1_Closed");

            btnClose_Click(null, null);
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

                    case IEventListener.CONNECTION_SUCCESS:
 
                       

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

       

        MDT.Tools.Wpf.AutoUpdater.AutoUpdater autoUpdater = new MDT.Tools.Wpf.AutoUpdater.AutoUpdater();
        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
            bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
            bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
            if (!isAllow || isLite || isNotAllow)
            {
                gridMain.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                gridMain.RowDefinitions[4].Height = new GridLength(0, GridUnitType.Star);
            }

            DesignHandler.Instance.DesignNotify += Instance_DesignNotify;
            
            sysWarnGUI.Owner = this;

            this.Dispatcher.Invoke(() =>
            {
                logoInfo.setUserName(LocalEnv.UserInfo.FullName);
                Version version = ReflectionHelper.GetFrameVersion();
                DateTime buildtime = System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location);                

                logoInfo.SubscribeConStatus();

                DataTimeSequence.Instance.Number = 1;

                DesignHandler.IsShow = true;

                //ReadHistory();

                _isInit = false;
                //if (this.Panel.Items.Count == 1)
                tcPlugin_SelectionChanged(tcPlugin, null);


                SetUserDrag();

                //Tbook权限
                if (LocalEnv.IsHaveTradeBookPermission)
                {
                    string notice = IniFileHelper.ReadValue(string.Format("{0}-{1}", Notice, LocalEnv.UserInfo.UserName), Notice);
                    bool.TryParse(notice, out _needShowNotic);
                    ShowNotice();
                }

                if (LocalEnv.ConfInfo.ApiKey.Equals(ApiConsts.API_KEY_GW))
                    ApiManager.Instance.GetDefaultApi().SubScribeConStatus(this.OnGateConnectStatus);

                ReStoreWindow();

                this.SizeChanged += Window1_SizeChanged;
                this.StateChanged += Window1_StateChanged;
                this.Closed += Window1_Closed;


            });

        }

        private void ShowNotice()
        {
            if (!_needShowNotic)
            {
                UserNoticeView userNoticeView = new UserNoticeView();
                userNoticeView.OnAction += UserNoticeView_OnAction;
                userNoticeView.Owner = this;
                userNoticeView.ShowDialog();
            }
        }

        private void UserNoticeView_OnAction(object arg1, bool isCheck, bool arg3)
        {
            if (isCheck)
            {
                this.WriteNoticeMemory(isCheck);
            }
            else
                this.Window1_Closed(null, null);
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
                int pluginKey = (int)temItem.Tag;
                if (pluginKey < 0)//TradeBookPlugin特殊处理
                    pluginKey = 22;
                IPlugin plugin = this._pluginManager.GetPlugin(pluginKey);
                if(plugin != null)
                {
                    if (pluginKey == 22)
                    {
                       sb.Append(string.Format("{0}-{1},", pluginKey, temItem.ToolTip));
                    }
                    else
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
                IniFileHelper.WriteValue(GetCacheKey(), SelectedTagKey, tabItem.Header as string);
            }

            IniFileHelper.WriteValue(GetCacheKey(), OpenPluginKey, selectPlugin);
        }

        private void WriteNoticeMemory(bool isNotice)
        {
            IniFileHelper.WriteValue(string.Format("{0}-{1}", Notice, LocalEnv.UserInfo.UserName), Notice, isNotice.ToString());
        }

        private string GetCacheKey()
        {
            return string.Format("{0}-{1}", OpenPlugin, LocalEnv.UserInfo.UserName);
        }

        private void ClosePopUpWindow()
        {
            
        }

        private void ReadHistory()
        {
            string historyTag = IniFileHelper.ReadValue(GetCacheKey(), OpenPluginKey) ?? string.Empty;
            string selectTag = IniFileHelper.ReadValue(GetCacheKey(), SelectedTagKey) ?? string.Empty;

            //string notice = IniFileHelper.ReadValue(string.Format("{0}-{1}", Notice, LocalEnv.UserInfo.UserName), Notice);
            //bool.TryParse(notice, out _needShowNotic);
            
            historyTag = historyTag.Replace("80-主页,", string.Empty);
            historyTag = historyTag.Replace("80-主页", string.Empty);
            historyTag = historyTag.Replace("89-内部看板,", string.Empty);
            historyTag = historyTag.Replace("89-内部看板", string.Empty);
            if (historyTag.Length > 0)
            {
                historyTag = "," + historyTag;
            }
            historyTag = "80 - 主页,89 - 内部看板" + historyTag;

            //Tbook权限
            if (!(LocalEnv.IsHaveTradeBookPermission || LocalEnv.ConfInfo.IsPAYL))
            {
                tcPlugin.ShowDefaultAddButton = false;
                historyTag = IniFileHelper.ReadValue(GetCacheKey(), OpenPluginKey);
                if (string.IsNullOrEmpty(historyTag))
                {
                    historyTag = System.Configuration.ConfigurationManager.AppSettings["DefaultPlugin"] ?? string.Empty;
                }
            }
            

            if (!string.IsNullOrEmpty(historyTag))
            {
                ////包含历史Tab时，重置默认打开插件
                //foreach (TabItem tab in Panel.Items)
                //{
                //    int pluginKey = (int)tab.Tag;
                //    IPlugin plugin = this._pluginManager.GetPlugin(pluginKey);
                //    if (plugin != null)
                //        _pluginManager.DisEnablePlugin(pluginKey);
                //}


                //this.Panel.Items.Clear();

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
                            if (plugin != null && !plugin.IsMain)// && CheckPermission(pluginKey))
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
                                    STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "OpenHistroyChromePlugin", Info1 = CommonUtils.SerializeObject(new List<string>() { plugin.PluginData.ToString() }), Info2 = "", Info3 = "" });
                                }
                            }
                        }
                    }
                }
            }

            //默认选择Tab页
            if (!(LocalEnv.IsHaveTradeBookPermission || LocalEnv.ConfInfo.IsPAYL))
            {
                if (string.IsNullOrEmpty(selectTag))
                {
                    selectTag = System.Configuration.ConfigurationManager.AppSettings["DefaultFocusPlugin"] ?? string.Empty;
                }
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
                            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                            {
                                AppUtils.Application.Panel.SelectedItem = tabItem;
                                AppUtils.Application.Panel.Focus();
                            }));

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
            bool result = true;

            if (pluginKey == 40)
            {
                STC.Common.Model.Admin.Module module = AdminHandler.Instance.GetModuleById("2001");
                if (module == null)
                {
                    //module = AdminHandler.Instance.GetModuleById("2002");
                    //if (module == null)
                        result = false;
                }
            }
            else if (pluginKey == 100)
            {
                STC.Common.Model.Admin.Module module = AdminHandler.Instance.GetModuleById("3010");
                if (module == null)
                {
                    //module = AdminHandler.Instance.GetModuleById("2002");
                    //if (module == null)
                    result = false;
                }
            }


            return result;
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowPos();
            AppUtils.Application.PluginManager.Application.BroadCast(Consts.Topic_RestartApp, this);
            //WindowManager.Instance.CloseAllWindow();
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
                WindowIconHelper.Instance.Stop();
            }
            //else
            //    window.WindowState = WindowState.Maximized;
        }

        private double currentWidth = 0;
       
        private void setServerTime()
        {
            //Action action=()=> lbServerTime.Text = String.Format("服务器时间：{0}",DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss"));
            //if (Thread.CurrentThread != lbServerTime.Dispatcher.Thread)
            //{
            //    lbServerTime.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, action);
            //}
            //else
            //{
            //    action();
            //}
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
                    (LoginWin as LoginGUI).SetProcess(80);

                    LoadPlugin();

                    this.Show();
                    //this.Cursor = Cursors.Arrow;
                    (LoginWin as LoginGUI).SetProcess(90);
                    (LoginWin as LoginGUI).Hide();
                    ReadHistory();
                });
            
            //this.Show();
        }

        //排除和历史重复的插件，防止重复加载
        private void CheckHistory()
        {
            string historyTag = IniFileHelper.ReadValue(GetCacheKey(), OpenPluginKey);
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
                                && plugin.PluginKey != (int)PluginKeyEnum.CustomPlugin && plugin.PluginKey != (int)PluginKeyEnum.XBondCommonPlugin
                                && plugin.PluginKey != (int)PluginKeyEnum.SettingPlugin)//3:STC.Sys.Plugin;1:BasePlugin
                                plugin.Enabled = false;
                        }
                    }
                    
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

                                // pluginKey是22，表示TradeBook插件， 可以多次打开
                                if (plugin.PluginKey == 22)
                                {
                                    plugin.Enabled = false;
                                }

                            }
                        }
                    }
                }
            }
        }


        #endregion



        #region IForm

        private IPluginManager _pluginManager;
        private PluginUtils _pluginUtils;
       
        public LogoButton MenuPanel { get { return btnLogo; } }

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
            get { return greadHeader; }
        }

        public Grid CustomePanel { get { return gridCustome; } }

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
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                Window window = (Window)sender;
                if (window != null && window.IsActive)
                    WindowIconHelper.Instance.Stop();

                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
            }));

            HotKeyHelper.ReregisterHostKey(m_Hwnd);
        }

        private void mainWindow_Deactivated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                this.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#606060"));
            }));

            HotKeyHelper.UnregisterHostKey(m_Hwnd);
        }

        public void SetCommonVisibility(Visibility visibility)
		{
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                bool isLite = AdminHandler.Instance.CheckPermission("5007", "500701");
                bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                if (!isAllow || isLite || isNotAllow)
                {
                    gridMain.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
                    gridMain.RowDefinitions[4].Height = new GridLength(0, GridUnitType.Star);
                }
                else
                {
                    if (visibility == Visibility.Collapsed)
                    {
                        gridCommon.Visibility = gridSplitter.Visibility = Visibility;
                        gridMain.RowDefinitions[3].Height = gridMain.RowDefinitions[4].Height = new GridLength(0);
                    }
                    else
                    {
                        gridCommon.Visibility = gridSplitter.Visibility = Visibility;
                        if (gridMain.RowDefinitions[3].Height.Value == 0)
                        {
                            gridMain.RowDefinitions[3].Height = new GridLength(8);
                        }

                        SetExecutionsHeight();
                    }
                }
            }));
        }

        private int _currentSelectedIndex = -1;

        private int _preFocusPluginKey = -1;
        private bool _isFirst = true;
        private void tcPlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInit)
                return;

            if (e != null)
            {
                if (_currentSelectedIndex == tcPlugin.SelectedIndex)
                    return;

                _currentSelectedIndex = tcPlugin.SelectedIndex;
            }

            bool isFirst = _isFirst;
            TabControl tc = sender as TabControl;
            TabItem tabItem = tc.SelectedItem as TabItem;
            if (tabItem != null && tabItem.Tag != null)
            {
                int pluginKey = (int)tabItem.Tag;
                IPlugin plugin = _pluginManager.GetPlugin(pluginKey);
                DesignHandler.Instance.SendTabChange(tabItem);
                if (!_isFirst && e != null)
                {
                    if (!(_preFocusPluginKey == -1 || (_preFocusPluginKey > 0 && _preFocusPluginKey != pluginKey)))
                    {
                        if (plugin != null && !plugin.IsFullScreen)
                        {
                            SetCommonVisibility(Visibility.Visible);
                        }
                        return;
                    }
                }

                _isFirst = false;

                _preFocusPluginKey = pluginKey;
                
                if (plugin != null)
                {
                    if (plugin.IsFullScreen)
                        SetCommonVisibility(Visibility.Collapsed);
                    else
                        SetCommonVisibility(Visibility.Visible);
                }
                else if (pluginKey < 0)//traderbook特殊处理
                {
                    SetCommonVisibility(Visibility.Visible);
                }

                //如果是 ChromePlugin,则显示刷新和弹出按钮
                btn_RefreshChrome.Visibility = btn_PopupWindow.Visibility = plugin.PluginKey == (int)PluginKeyEnum.ChromePlugin ? Visibility.Visible : Visibility.Collapsed;
                if (plugin.PluginKey == (int)PluginKeyEnum.GRBroker || plugin.PluginKey == (int)PluginKeyEnum.GRHistory || plugin.PluginKey == (int)PluginKeyEnum.GRTransaction)
                    btn_GrFileter.Visibility = btn_Grdelete.Visibility = Visibility.Visible;
                else
                    btn_GrFileter.Visibility = btn_GrBridge.Visibility = btn_Grdelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetCommonVisibility(Visibility.Collapsed);
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

       
        #region  锁屏
        private void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            try
            {
                long tick = GetLastInputTime() / 1000;

                if ((tick / ts > 0) && (tick % ts == 0))
                {
                    if (tick / ts != lastTick)
                    {
                        lastTick = tick / ts;
                        
                        if (LockScreen == "-1")
                        {
                            LogHandler.Info($"The system is idle for {LocalEnv.ConfInfo.LockScreenMinute} minutes, and the program automatically exit!");
                            Window1_Closed(null, null);
                        }
                        else if (LockScreen == "1")
                        {
                            LogHandler.Info($"The system is idle for {LocalEnv.ConfInfo.LockScreenMinute} minutes, and the screen is automatically locked!");
                            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
                            {
                                foreach (System.Windows.Forms.Screen scr in System.Windows.Forms.Screen.AllScreens)
                                {
                                    if (!scr.Primary)
                                    {
                                        this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                        {
                                            LockScreen ls2 = new LockScreen(false);
                                            ls2.Topmost = true;
                                            ls2.Owner = null;
                                            ls2.Left = scr.Bounds.Left;
                                            ls2.Top = scr.Bounds.Top;
                                            ls2.Width = scr.Bounds.Width;
                                            ls2.Height = scr.Bounds.Height;
                                            ls2.WindowState = WindowState.Maximized;
                                            ls2.ResizeMode = ResizeMode.NoResize;
                                            ls2.WindowStyle = WindowStyle.None;
                                            ls2.WindowState = WindowState.Normal;
                                            ls2.ShowInTaskbar = false;
                                            ls2.Show();
                                        }));
                                    }
                                }
                            }

                            foreach (System.Windows.Forms.Screen scr in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (scr.Primary)
                                {
                                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                    {
                                        LockScreen ls2 = new LockScreen();
                                        ls2.Topmost = true;
                                        ls2.Owner = null;
                                        ls2.Left = scr.Bounds.Left;
                                        ls2.Top = scr.Bounds.Top;
                                        ls2.Width = scr.Bounds.Width;
                                        ls2.Height = scr.Bounds.Height;
                                        ls2.WindowState = WindowState.Maximized;
                                        ls2.ResizeMode = ResizeMode.NoResize;
                                        ls2.WindowStyle = WindowStyle.None;
                                        ls2.WindowState = WindowState.Normal;
                                        ls2.ShowInTaskbar = false;
                                        ls2.Show();
                                    }));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info("{0}: {1}", "Application_Idle Error:", ex);
            }
        }

        // 创建结构体用于返回捕获时间
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            // 设置结构体块容量
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            // 捕获的时间
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        // 获取键盘和鼠标没有操作的时间
        private static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            // 捕获时间
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;
            else
                return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }
        #endregion


        #region 消息钩子预定义参数
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x30D;

        private IntPtr mNextClipBoardViewerHWnd;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool ChangeClipboardChain(IntPtr HWnd, IntPtr HWndNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        #endregion

        //剪贴板被占用（每次只能一个程序读取），则会报OpenClipboard HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))错误
        private String getClipboardText()
        {
            for (int i = 0; i < 200; i++)
            {
                try
                {
                    return System.Windows.Clipboard.GetText();
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);//这句加不加都没关系
                }
            }
            return String.Empty;
        }

        #region 保存和恢复窗口
        private void Window1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
                SaveWindowPos();
        }

        private void SaveWindowPos()
        {
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", WinHeight, this.ActualHeight + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", WinWidth, this.ActualWidth + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", PositionX, this.Left + "");
            IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", PositionY, this.Top + "");
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
                string height = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", WinHeight);
                string width = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", WinWidth);

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

                string positionX = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", PositionX);
                string positionY = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}", PositionY);
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


        #region 网页功能按钮
        /// <summary>
        /// 刷新网页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_RefreshChrome_Click(object sender, RoutedEventArgs e)
        {
            TabItem curtabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;
            if (curtabItem != null)
            {
                if (curtabItem.Content.GetType().ToString() ==  "STC.Chrome.Plugin.ChromeUserControl")
                {
                    System.Reflection.MethodInfo ChromeUpdAddress = curtabItem.Content.GetType().GetMethod("Reload");
                    ChromeUpdAddress?.Invoke(curtabItem.Content, new object[] { "" });
                }
            }
        }

        /// <summary>
        /// 网页弹出单独的窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_PopupWindow_Click(object sender, RoutedEventArgs e)
        {
            TabItem curtabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;
            if (curtabItem != null)
            {
                if (curtabItem.Content.GetType().ToString() == "STC.Chrome.Plugin.ChromeUserControl")
                {
                    PropertyInfo propertyInfo = curtabItem.Content.GetType().GetProperty("Key");

                    if (propertyInfo != null)
                    {
                        // Get the value of the property from the instance of the UserControl
                        object propertyValue = propertyInfo.GetValue(curtabItem.Content);
                        if (propertyValue != null)
                        {
                            string key = propertyValue.ToString();
                            //没有插件就是web网页
                            List<STC.Common.Model.Admin.Module> webModel = AdminHandler.Instance.GetWebModule();

                            if (webModel != null)
                            {
                                STC.Common.Model.Admin.Module m = webModel.FirstOrDefault(o => o.id == key);
                                if (m != null)
                                {
                                    AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebApp", m);
                                }
                                else
                                {
                                    //打开的其它页面
                                    m = new STC.Common.Model.Admin.Module();
                                    
                                    m.modulename = curtabItem.Header.ToString();

                                    PropertyInfo CurrentUrlInfo = curtabItem.Content.GetType().GetProperty("CurrentUrl");
                                    if (CurrentUrlInfo != null)
                                    {
                                        object CurrentUrlValue = CurrentUrlInfo.GetValue(curtabItem.Content);
                                        m.inf2 = CurrentUrlValue.ToString();
                                    }
                                    
                                    if (!string.IsNullOrEmpty(m.inf2))
                                        AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebApp", m);
                                }
                            }
                        }

                    }
                }

            }
        }


        /// <summary>
        /// 获取当前选择TabItem
        /// </summary>
        /// <returns></returns>
        private TabItem GetSelectedTabItem()
        {
            TabItem curtabItem = null;

            foreach (TabItem tabItem in this.Panel.Items)
            {
                if (tabItem.IsSelected)
                {
                    curtabItem = tabItem;
                    break;
                }
            }

            return curtabItem;
        }
        #endregion

        #region 上下分割窗口记忆
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

        private void SetExecutionsHeight()
        {
            Application.Current.Dispatcher.Invoke(() =>
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
            });
        }

        /// <summary>
        /// 保存窗口位置
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSaveWindowPos(object obj)
        {
            SaveWindowPos();
        }

        /// <summary>
        /// 窗口拖放
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnWindowDragMove(object obj)
        {
            this.DragMove();
        }

        #endregion

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var intPtr = new WindowInteropHelper(this).Handle;//获取当前窗口的句柄
            IntPtr sysMenuHandle = Win32Helper.GetSystemMenu(intPtr, false);
            Win32Helper.AppendMenu(sysMenuHandle, Win32Helper.MF_SEPARATOR, 0, string.Empty);//添加一个分隔线
            Win32Helper.AppendMenu(sysMenuHandle, Win32Helper.MF_STRING, Win32Helper.IDM_ABOUT, "恢复到主屏幕");

            Button defaultButton = tcPlugin.Template.FindName("DefaultAddButton", tcPlugin) as Button;
            defaultButton.Style = Application.Current.MainWindow.FindResource("AddItemButtonStyle") as Style;
        }

        #region 行情看板窗口改变大小
        private double normalWidth = 0;
        private double normalHeight = 0;
        private double normalLeft = 0;
        private double normalTop = 0;
        private void Instance_OnMax(object obj)
        {
            normalWidth = this.Width;
            normalHeight = this.Height;
            normalLeft = this.Left;
            normalTop = this.Top;

            OnMaxClick();
        }

        #endregion

        #region GR Broker
        private void btn_GrFileter_Click(object sender, RoutedEventArgs e)
        {
            //发布消息
            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TriggerGrCommand", Info1 = "DetailCommand" });
        }

        private void btn_GrBridge_Click(object sender, RoutedEventArgs e)
        {
            //发布消息
            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TriggerGrCommand", Info1 = "BridgeCommand" });
        }

        private void btn_Grdelete_Click(object sender, RoutedEventArgs e)
        {
            //发布消息
            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TriggerGrCommand", Info1 = "DeleteCommand" });
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
