using CefSharp;
using CefSharp.Wpf;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Chrome.Plugin.Data;
using STC.Chrome.Plugin.Demo;
using STC.Chrome.Plugin.Handler;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Chrome;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Drp.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace STC.Chrome.Plugin
{
    public class ChromePlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 0;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.ChromePlugin; }
        }


        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = false;
        public override bool IsAlone { get; set; }


        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = true;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "Chrome"; }
        }

        public override string Description
        {
            get { return "Chrome"; }
        }

        public override string Author
        {
            get { return "陈彦畅"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        //先使用 chart的，后面修改 
        // to do...
        private readonly string _Code = "508";

        
        /// <summary>
        /// ChromeUserControl字典
        /// </summary>
        private Dictionary<string, ChromeUserControl> DicChromeUserControl = new Dictionary<string, ChromeUserControl>();

        private bool _isFirst = true;

        protected override void load()
        {
            //第一次菜单加载
            if (_isFirst)
            {
                //Add Custom assembly resolver
                //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                //bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
                //if (!isPermission)
                //    return;

                //读取配置文件中的代理信息
                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        ProxyInfo proxyInfoQb = ProxyUtils.ReadQBProxyInfo();
                        ProxyUtils.ChromeProxyinfo = ProxyUtils.GetChromeProxyInfo(proxyInfoQb);
                        break;
                    default:
                        ProxyInfo proxyInfostc = ProxyUtils.ReadProxyInfo();
                        ProxyUtils.ChromeProxyinfo = ProxyUtils.GetChromeProxyInfo(proxyInfostc);
                        break;
                }

                InitCef();

                AppUtils.Application.PluginManager.Application.Subscribe("OpenChrome", this);
                AppUtils.Application.PluginManager.Application.Subscribe("PopUpWebApp", this);
                AppUtils.Application.PluginManager.Application.Subscribe("PopUpWebFQ", this);
                AppUtils.Application.PluginManager.Application.Subscribe("PopUpQBAssistant", this); 
                AppUtils.Application.PluginManager.Application.Subscribe("ConvertSendFinQFriendReqToWebFinQ", this);
                AppUtils.Application.PluginManager.Application.Subscribe("ConvertSendFinQFriendReq_BestToWebFinQ", this); 
                AppUtils.Application.PluginManager.Application.Subscribe("GateWayNetMessage", this);
                //订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                _isFirst = false;
                if (notifyCloseTabItem == null)
                {
                    notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                    notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
                }

                CefHandler.Instance.Initialize();

                //注册清空复位订阅处理等事件
                if ((LocalEnv.UserInfo.QeubeeFingPermissions == "1" || LocalEnv.ConfInfo.IsUseQBPrdTestFQNotOpenQB == "1") && LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                {
                    //FinQ行情处理初始化
                    FinQMdHander.Instance.Initialize();
                }

                if ((LocalEnv.UserInfo.QeubeeFingPermissions == "1" || LocalEnv.ConfInfo.IsUseQBPrdTestFQNotOpenQB == "1") && LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                {
                    //打开FQ的ChromeWinModel
                    STC.Common.Model.Chrome.ChromeWinModel cw = new STC.Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = false, Url = LocalEnv.ConfInfo.WebFQAddress };
                    cw.Key = "WebFQ";
                    cw.Title = "报价助手";
                    cw.IsShowAddressBar = false;
                    cw.IsShowStatusBar = false;
                    cw.IsEnableMaxMin = false;
                    cw.IsShowAddressViewSourceDevTools = false;
                    cw.UseChromePluginObject = true;
                    OpenFQChromeInWindow(cw);
                }

                List<STC.Common.Model.Admin.Module> webModel = AdminHandler.Instance.GetWebModule().OrderBy(o => o.inf4).ToList();
                if (webModel != null)
                {
                    foreach (STC.Common.Model.Admin.Module m in webModel)
                    {
                        if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                        {
                            AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "15");
                        }
                        else
                        {
                            AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_xiazaizhongxin.png", m.modulename, int.Parse(m.id), "15");
                        }
                    }
                }


                base.load();
            }
        }

        public void InitCef()
        {
            try
            {
                CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

                CefSettings settings = new CefSettings();
                //如果是 Avalonia.CefGlue，还需要allow-insecure-localhost="1"
                settings.CefCommandLineArgs.Add("disable-features", "BlockInsecurePrivateNetworkRequests");
                //settings.CefCommandLineArgs.Add("allow-insecure-localhost", "1");
                // 1️⃣ 禁用不必要的特性
                //settings.CefCommandLineArgs.Add("disable-site-isolation-trials", "1"); // 降低进程数量
                //settings.CefCommandLineArgs.Add("disable-features", "TranslateUI,AutofillServerCommunication,OptimizationHints"); // 禁用不必要的功能
                settings.CefCommandLineArgs.Add("--ignore-urlfetcher-cert-requests", "1");
                settings.CefCommandLineArgs.Add("--ignore-certificate-errors", "1");
                //settings.CefCommandLineArgs.Add("--disable-web-security", "1");
                //settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";
                settings.CefCommandLineArgs.Add("enable-media-stream", "1");  //开启摄像头
                settings.CefCommandLineArgs.Add("enable-system-flash", "1");  //flash
                settings.CefCommandLineArgs.Add("enable-speech-input", "1");  //语音输入

                settings.CefCommandLineArgs.Add("disable-component-update", "1"); // 禁用组件更新
                settings.CefCommandLineArgs.Add("disable-widevine-cdm", "1");    // 禁用 Widevine
                // 使用命令行参数设置高 DPI 支持和硬件加速
                // settings.CefCommandLineArgs.Add("force-device-scale-factor", "1"); // 调整缩放因子
                settings.CefCommandLineArgs.Add("high-dpi-support", "1"); // 支持高 DPI

                //settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream"); //允许媒体权限请求（如摄像头/麦克风）时自动授权，无需用户手动点击“允许”。
                //settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing"); //允许屏幕共享

                settings.CefCommandLineArgs.Add("request-timeout", "20000"); // 超时时间，单位为毫秒
                
                //设置 wcf
                settings.CefCommandLineArgs.Add("enable-wcf", "1");
                //settings.CefCommandLineArgs.Add("renderer-process-limit", "4");
                //设置 ConcurrentTaskExecution 属性
                //settings.CefCommandLineArgs.Add("enable-concurrent-task-execution", "100");

                // 3️⃣ 网络性能提升
                //settings.CefCommandLineArgs.Add("enable-aggressive-domstorage-flushing", "1"); // 更快写入本地存储
                settings.CefCommandLineArgs.Add("enable-tcp-fastopen", "1");                   // TCP连接加速

                //有些网页使用了 HTML5 Drag & Drop API，CEF 可能默认禁用了部分特性。
                settings.CefCommandLineArgs.Add("allow-file-access-from-files", "1");
                settings.CefCommandLineArgs.Add("enable-blink-features", "HTMLDragAndDrop");
                //settings.CefCommandLineArgs.Add("disable-pinch", "1");  // 禁用缩放（可选）
                settings.CefCommandLineArgs.Add("enable-system-drag-drop", "1"); // 启用拖放


                //在 CefSettings 中增加 JavaScript 超时保护
                settings.CefCommandLineArgs.Add("disable-hang-monitor", "1"); // 禁用崩溃监控，避免因 JavaScript 超时被杀死，避免 CEF 进程被误判为“无响应”
                settings.CefCommandLineArgs.Add("disable-gpu-process-crash-limit", "1"); // 避免 GPU 进程崩溃影响主进程

                settings.CefCommandLineArgs.Add("minimum-font-size", "0"); //允许网页自己写 0px 字体

                //CEF 强制使用 CPU 进行渲染，避免 GPU 兼容性问题，可能会导致 WPF 进程卡死
                if (BaseMachineHelper.IsVirtualMachine())
                {
                    settings.CefCommandLineArgs.Add("disable-gpu", "1"); // 完全禁用GPU（如在服务器/低端设备上运行）
                    settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1"); // 禁用GPU合成，某些机器反而更快
                    settings.CefCommandLineArgs.Add("disable-software-rasterizer", "1");
                    // 多线程合成器支持
                    settings.CefCommandLineArgs.Add("enable-threaded-compositing", "1");
                    // 高性能帧调度
                    settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
                }
                else
                {
                    settings.CefCommandLineArgs.Remove("disable-gpu");
                    settings.CefCommandLineArgs.Remove("disable-gpu-compositing");
                    settings.CefCommandLineArgs.Remove("disable-software-rasterizer");
                    if (LocalEnv.ConfInfo.EnableChromeGPU == "1")
                    {
                        settings.CefCommandLineArgs.Add("enable-gpu", "1");
                        settings.CefCommandLineArgs.Add("enable-gpu-compositing", "1");//开启GPU合成（GPU Compositing），让GPU负责图层的合成，减轻CPU负担
                                                                                       //使用 GPU 加速 Canvas / WebGL 强制开启 WebGL & WebGL2
                        settings.CefCommandLineArgs.Add("enable-webgl", "1");
                        settings.CefCommandLineArgs["enable-accelerated-video-decode"] = "1";
                        settings.CefCommandLineArgs["enable-accelerated-2d-canvas"] = "1";
                        settings.CefCommandLineArgs.Add("ignore-gpu-blocklist", "1"); // 忽略黑名单设备，强制使用GPU
                        settings.CefCommandLineArgs.Add("enable-zero-copy", "1");
                    }
                    else
                    {
                        settings.CefCommandLineArgs.Add("disable-gpu", "1"); // 完全禁用GPU（如在服务器/低端设备上运行）
                        settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1"); // 禁用GPU合成，某些机器反而更快
                        settings.CefCommandLineArgs.Add("disable-software-rasterizer", "1");
                    }
                    // 高性能帧调度
                    settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
                    settings.CefCommandLineArgs["enable-features"] = "UseSkiaRenderer";
                }

                // 3. 禁止 GPU Shader 磁盘缓存（可选，避免 GPUCache 产生）
                settings.CefCommandLineArgs.Add("disable-gpu-shader-disk-cache", "1");
                settings.CefCommandLineArgs.Add("disable-visited-links", "1");
                // 6️⃣ 允许磁盘缓存，加速页面加载
                string cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "bin", "cache");
                // 2. 用户数据目录（Profile、GPUCache、WidevineCdm 等）
                settings.CefCommandLineArgs.Add("user-data-dir", Path.Combine(cachePath, "UserData"));

                //wpf必须有此设置,禁用CEF 的沙盒机制，如果设为0可能导致子进程启动失败或白屏
                if (LocalEnv.ConfInfo.NoSandbox == "1")
                    settings.CefCommandLineArgs.Add("no-sandbox", "1");

                settings.CachePath = cachePath;
                // CEF 运行在 UI 线程，这样 CEF 运行在独立的消息循环，不会阻塞 WPF UI 线程
                settings.MultiThreadedMessageLoop = true;
                settings.WindowlessRenderingEnabled = true; // 启用无窗口渲染（WPF 需要）
                settings.PersistSessionCookies = true;
                settings.PersistUserPreferences = true;

                //如果不使用代理则不设置代理
                if (ProxyUtils.ProxyInfo != null && ProxyUtils.ProxyInfo.AccessType == "1")
                {
                    //1：不使用代理
                    settings.CefCommandLineArgs.Add("no-proxy-server", "1");// 禁用代理，所有网络请求都直连
                }

                // 5️⃣ 禁用日志和调试（减少I/O）
                if (LocalEnv.ConfInfo.EnableChromeDebug == "0")
                {
                    settings.LogSeverity = LogSeverity.Disable;
                }
                else
                {
                    settings.RemoteDebuggingPort = 9222;
                }
                settings.Locale = "zh-CN";
                settings.AcceptLanguageList = "zh-CN";
                settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "bin", "Browser.exe");
                //settings.CefCommandLineArgs.Add("enable-blink-features", "MessageChannel");
                //CefSharpSettings.ConcurrentTaskExecution = true;
                CefSharpSettings.ConcurrentTaskExecution = true;

                if (!Cef.IsInitialized)
                {
                    Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
                }

            }
            catch(Exception ex)
            {
                LogHelper.Error($"CefSharp 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 释放消息处理
        /// </summary>
        /// <param name="tabItemHashCode"></param>
        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Tag.ToString() == ((int)PluginKeyEnum.ChromePlugin).ToString())
                    {
                        LogHelper.Info("ChromeUserControl:" + tabItem.Header ?? "" + " UserControl_Unloaded ");
                        ChromeUserControl mainView = tabItem.Content as ChromeUserControl;
                        if (DicChromeUserControl.ContainsKey(mainView.Key.ToString()))
                        {
                            DicChromeUserControl.Remove(mainView.Key.ToString());
                            //移除注册便于扩展方式调用
                            remove($"ChromeUserControl_{mainView.Key.ToString()}");
                        }

                        mainView.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 接受的消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals("OpenChrome"))
                {
                    OpenChrome((ChromeWinModel)o);
                }
                else if (name.Equals("PopUpWebApp"))
                {
                    if (o.GetType() == typeof(STC.Common.Model.Admin.Module))
                    {
                        PopUpWebApp((STC.Common.Model.Admin.Module)o);
                    }
                }
                else if (name.Equals("PopUpWebFQ"))
                {
                    //打开FQ的ChromeWinModel
                    OpenFQChromeInWindow((ChromeWinModel)o);
                }
                else if (name.Equals("PopUpQBAssistant"))
                {
                    //打开在线助手
                    OpenQBAssistantInWindow(CefHandler.Instance.GetQBAssistantModel());
                }
                else if (name.Equals("ConvertSendFinQFriendReqToWebFinQ"))
                {
                    //转换FinQ单家最优为WebFinQ请求
                    FinQMdHander.Instance.ConvertSendFinQFriendReqToWebFinQ((SendFinQFriendReq)o);
                }
                else if (name.Equals("ConvertSendFinQFriendReq_BestToWebFinQ"))
                {
                    //转换FinQ综合最优为WebFinQ请求
                    FinQMdHander.Instance.ConvertSendFinQFriendReq_BestToWebFinQ((SendFinQFriendReq_Best)o);
                }
                else if (name.Equals("GateWayNetMessage"))
                {
                    //处理网关消息
                    CefHandler.Instance.add(o);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    // 接收到清空复位消息
                    if (name.Equals(Consts.Topic_Clear))
                    {

                    }
                }

               
            }
        }

        private void PopUpWebApp(STC.Common.Model.Admin.Module urlmodel)
        {
            string url = urlmodel.inf2;
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Sid))
            {
                url = $"{url}?username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.Sid}";
            }
            else
            {
                if (!url.Contains("?"))
                {
                    url = $"{url}?username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.SessionID}";
                }
                else
                {
                    url = $"{url}&username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.SessionID}";
                }
            }

            //url = "https://www.baidu.com";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = false, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
            if (!string.IsNullOrEmpty(urlmodel.id))
                cw.Key = urlmodel.id.ToString();
            cw.Title = urlmodel.modulename;
            cw.ForceTargetSelf = true;
            cw.UseChromePluginObject = true;
            cw.IsShowMenuHandler = false;
            cw.IsShowAddressViewSourceDevTools = true;
            cw.Height = 600;
            cw.Width = 1024;
            cw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (!string.IsNullOrEmpty(urlmodel.inf5))
            {
                //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
                cw.Proxy = urlmodel.inf5;
            }

            OpenChrome(cw);
        }

        public void OpenChrome(ChromeWinModel cw)
        {
            //处理原始参数，如果key为空则取标题
            if (string.IsNullOrEmpty(cw.Key))
            {
                if (!string.IsNullOrEmpty(cw.Title))
                {
                    cw.Key = cw.Title;
                }
                else
                {
                    cw.Key = "ChromeDemo";
                }
            }
            if (!cw.IsShowAddressBar)
                cw.IsShowAddressBar = cw.IsShowStatusBar = cw.IsShowMenuHandler = LocalEnv.ConfInfo.ChromeShowToolBar == "1" ? true : false;
            if (LocalEnv.ConfInfo.ChromeShowToolBar == "1")
            {
                cw.IsShowAddressViewSourceDevTools = true;
            }
            
            if (cw.IsOpenInMainTabPage)
            {
                OpenChromeInTabpage(cw);
            }
            else
            {
                OpenChromeInWindow(cw);
            }
        }

        /// <summary>
        /// 打开Tabpage添加到首页
        /// </summary>
        /// <param name="cw"></param>
        public void OpenChromeInTabpage(ChromeWinModel cw)
        {
            string tagHeader = cw.Title;
            if (TabPageHelper.IsExistTabItem(tagHeader))
            {
                return;
            }

            try
            {
                ChromeUserControl chrom = new ChromeUserControl(cw);

                chrom.SetTopBarVisible(cw.IsShowAddressBar);
                chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                chrom.SetHideAddressViewSourceDevTools(cw.IsShowAddressViewSourceDevTools);
                
                if (!DicChromeUserControl.ContainsKey(cw.Key))
                {
                    DicChromeUserControl.Add(cw.Key, chrom);
                    //注册便于扩展方式调用
                    registerObject($"ChromeUserControl_{cw.Key}", chrom);
                }
                chrom.BrowserTitleChanged += Chrom_BrowserTitleChanged;

                TabPageHelper.OpenTabPage(tagHeader, chrom, PluginKey);
                //chrom.SetAddress(cw.Url);
                chrom.Browser.WpfKeyboardHandler = new CefSharp.Wpf.Experimental.WpfImeKeyboardHandler(chrom.Browser); // 在初始化 ChromiumWebBrowser 时设置 WpfKeyboardHandler 为 WpfImeKeyboardHandler
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }
        }

        private void Chrom_BrowserTitleChanged(object obj)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    string title = (obj as ChromeUserControl).Browser.Title;
                    if (((obj as ChromeUserControl).Parent as System.Windows.Controls.TabItem).Header == null)
                    {
                        ((obj as ChromeUserControl).Parent as System.Windows.Controls.TabItem).Header = title;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(((obj as ChromeUserControl).Parent as System.Windows.Controls.TabItem).Header.ToString()))
                        {
                            ((obj as ChromeUserControl).Parent as System.Windows.Controls.TabItem).Header = title;
                        }
                    }
                }
                catch { }
                
            }));
        }

        /// <summary>
        /// 打开独立窗口
        /// </summary>
        /// <param name="cw"></param>
        public void OpenChromeInWindow(ChromeWinModel cw)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                BorderWin myTradingPopupWin = new BorderWin();
                myTradingPopupWin.WindowState = cw.WindowState;
                myTradingPopupWin.Width = cw.Width;
                myTradingPopupWin.Height = cw.Height;
                
                myTradingPopupWin.SetTitleAndKey(cw.Key, cw.Title);
                myTradingPopupWin.Title = cw.Title;
                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                myTradingPopupWin.Closed += MyTradingPopupWin_Closed;
                if (!cw.IsEnableMaxMin)
                {
                    myTradingPopupWin.SetMinVisible(false);
                    myTradingPopupWin.SetMaxVisible(false);
                }

                try
                {
                    ChromeUserControl chrom = new ChromeUserControl(cw);
                    chrom.SetParentWindow(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(chrom);

                    chrom.SetTopBarVisible(cw.IsShowAddressBar);
                    chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                    chrom.SetHideAddressViewSourceDevTools(cw.IsShowAddressViewSourceDevTools);
                    //chrom.SetAddress(cw.Url);

                    Task.Factory.StartNew(() =>
                    {
                        if (!DicChromeUserControl.ContainsKey(cw.Key))
                        {
                            DicChromeUserControl.Add(cw.Key, chrom);
                            try
                            {
                                //注册便于扩展方式调用
                                registerObject($"ChromeUserControl_{cw.Key}", chrom);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error($"registerObject Error:{ex.Message}");
                            }
                        }
                    });

                    myTradingPopupWin.Width = cw.Width;
                    myTradingPopupWin.Height = cw.Height;

                    if (cw.WindowState != System.Windows.WindowState.Maximized)
                    {
                        if ((cw.MaxHeight != 0) && (cw.MaxWidth != 0))
                        {
                            myTradingPopupWin.MaxHeight = cw.MaxHeight;
                            myTradingPopupWin.MaxWidth = cw.MaxWidth;
                        }
                    }

                    myTradingPopupWin.Owner = null;
                    if ((cw.PositionX != 0) && (cw.PositionY != 0))
                    {
                        myTradingPopupWin.Top = cw.PositionX;
                        myTradingPopupWin.Left = cw.PositionY;
                    }

                    WindowManager.Instance.OpenOnlyOneWindow($"{cw.Key}", myTradingPopupWin, null);
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex.Message);
                }
            }));
        }

        #region 打开FQ独立窗口
        /// <summary>
        /// 打开FQ独立窗口
        /// </summary>
        /// <param name="cw"></param>
        public void OpenFQChromeInWindow(ChromeWinModel cw)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                object win = CefHandler.Instance.GetExistWin(cw.Key);
                if (win != null)
                {
                    //FinQMdHander.Instance.FQSetActionData = cw.ActionData;
                    //FinQMdHander.Instance.FQChangeBondData = cw.ChangeBondData;
                    //if (cw.ActionData != null)
                    //{
                    //    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
                    //}
                    if ((win as BdbBorderWin).Owner == null)
                        (win as BdbBorderWin).Owner = System.Windows.Application.Current.MainWindow;
                    try
                    {
                        (win as BdbBorderWin).MinWidth = 682;
                        (win as BdbBorderWin).MinHeight = 578;
                        (win as BdbBorderWin).ShowInTaskbar = true;
                        (win as BdbBorderWin).Opacity = 100;
                        (win as BdbBorderWin).Show();
                        (win as BdbBorderWin).Activate();
                        return;
                    }
                    catch
                    { 

                    }
                }

                //如果允许，强制使用chrome插件对象
                if (cw.UseChromePluginObject)
                    cw.CallBackObjectInstance.Add("cef|", "CefSharpJsHandler");

                BdbBorderWin myTradingPopupWin = new BdbBorderWin();
                myTradingPopupWin.WindowState = cw.WindowState;
                //myTradingPopupWin.Width = cw.Width;
                //myTradingPopupWin.Height = cw.Height;
                myTradingPopupWin.SetTitleAndKey(cw.Key, cw.Title);
                myTradingPopupWin.Title = cw.Title;
                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (!cw.IsShowAddressBar)
                    cw.IsShowAddressBar = cw.IsShowAddressViewSourceDevTools = cw.IsShowStatusBar = cw.IsShowMenuHandler = LocalEnv.ConfInfo.ChromeShowToolBar == "1" ? true : false;
                //myTradingPopupWin.Closed += MyTradingPopupWin_Closed;

                try
                {
                    ChromeUserControl chrom = new ChromeUserControl(cw);
                    chrom.SetParentWindow(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(chrom);

                    chrom.SetTopBarVisible(cw.IsShowAddressBar);
                    chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                    chrom.SetHideAddressViewSourceDevTools(cw.IsShowAddressViewSourceDevTools);
                    chrom.SetAddress(cw.Url);
                    chrom.Browser.WpfKeyboardHandler = new CefSharp.Wpf.Experimental.WpfImeKeyboardHandler(chrom.Browser); // 在初始化 ChromiumWebBrowser 时设置 WpfKeyboardHandler 为 WpfImeKeyboardHandler
                    myTradingPopupWin.Width = myTradingPopupWin.Width;
                    myTradingPopupWin.Height = myTradingPopupWin.Height;

                    if (cw.WindowState != System.Windows.WindowState.Maximized)
                    {
                        if ((cw.MaxHeight != 0) && (cw.MaxWidth != 0))
                        {
                            myTradingPopupWin.MaxHeight = cw.MaxHeight;
                            myTradingPopupWin.MaxWidth = cw.MaxWidth;
                        }
                    }

                    myTradingPopupWin.Owner = null;
                    if ((cw.PositionX != 0) && (cw.PositionY != 0))
                    {
                        myTradingPopupWin.Top = cw.PositionX;
                        myTradingPopupWin.Left = cw.PositionY;
                    }

                    //FinQMdHander.Instance.FQSetActionData = cw.ActionData;
                    //FinQMdHander.Instance.FQChangeBondData = cw.ChangeBondData;
                    //初始化时设置保存隐藏窗口
                    if (cw.ActionData == "{}" && cw.ChangeBondData == "{}")
                    {
                        myTradingPopupWin.Width = 16;
                        myTradingPopupWin.Height = 16;
                        myTradingPopupWin.ShowInTaskbar = false;
                        myTradingPopupWin.Opacity = 0;

                        CefHandler.Instance.AddWindow($"{cw.Key}", myTradingPopupWin, null);
                        myTradingPopupWin.Show();
                        return;
                    }

                    if (win == null)
                    {
                        CefHandler.Instance.OpenOnlyOneWindow($"{cw.Key}", myTradingPopupWin, null);
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex.Message);
                }
            }));
        }
        #endregion

        /// <summary>
        /// '我的交易'弹出窗口关闭事件
        /// </summary>
        /// <param name="sender">'我的交易'弹出窗口实例</param>
        /// <param name="e">表示包含事件数据的类的基类，并提供要用于不包含事件数据的事件的值。</param>
        private void MyTradingPopupWin_Closed(object sender, EventArgs e)
        {
            string key = (sender as BorderWin).SecurityID;
            if (DicChromeUserControl.ContainsKey(key))
            {
                //移除注册便于扩展方式调用
                try
                {
                    remove($"ChromeUserControl_{key}");
                    DicChromeUserControl.Remove(key);
                }
                catch { }
            }
        }


        public object GetObject(string name)
        {
            return Common.Utils.AppUtils.Application.GetObject($"{(int)PluginKeyEnum.ChromePlugin}_{name}");
        }

        protected override void unload()
        {
            base.unload();
        }

        public override void BeforeTerminating()
        {
            try
            {
                //停止http服务
                CefHandler.Instance.StopHttpServer();

                // 1. 确保所有 ChromiumWebBrowser 已经释放
                DisposeAllBrowsers();
                // 6️⃣ 允许磁盘缓存，加速页面加载
                string cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "bin", "cache");
               
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    // 2. 正常关闭 CEF
                    Cef.Shutdown();
                });

                // 3. 可选：删除缓存目录
                //if (Directory.Exists(cachePath))
                //{
                //    CleanupCefCache(cachePath);
                //}
            }
            catch (Exception ex)
            {
                LogHandler.Error("CleanupCefCache," + ex.Message);
            }

            base.BeforeTerminating();
        }

        #region 清空

        private void DisposeAllBrowsers()
        {
            Dispatcher.CurrentDispatcher.Invoke(()=>
            { 
                foreach (TabItem item in AppUtils.Application.Panel.Items)
                {
                    if (item.Content is ChromeUserControl)
                    {
                        (item.Content as ChromeUserControl).Browser?.Dispose();
                    }
                }

                object win = CefHandler.Instance.GetExistWin("WebFQ");
                if (win != null)
                {
                    if (win.GetType() == typeof(BdbBorderWin))
                    {
                        if ((win as BdbBorderWin).GetMainControl() is ChromeUserControl)
                        {
                            ((win as BdbBorderWin).GetMainControl() as ChromeUserControl).Browser?.Dispose();
                        }
                    }
                }
            });

        }

        private void CleanupCefCache(string cachePath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(cachePath);
                if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // ignore
            }
            catch (IOException)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(cachePath);
                    if (dirInfo.Exists)
                    {
                        dirInfo.Delete(true);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // ignore
                }
                catch (IOException)
                {
                    // ignore
                }
            }
        }
        #endregion

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp") || args.Name.Contains("STC.Chrome.Plugin") || args.Name.Contains("Browser.exe"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "bin", assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }
            return null;
        }



        /// <summary>
        /// 接收客户端发送的pubJS消息，切换TabPage， 执行网页pubJS方法  msgPo.ContentType.ToUpper() == "CUST_MD")
        /// </summary>
        /// <param name="msg">消息内容</param>
        /// <example>
        /// Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = "市场最优",Info2 = key, Info3 = value });
        /// </example>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "pubJS" && !string.IsNullOrEmpty(obj.Info1) && !string.IsNullOrEmpty(obj.Info2))
            {
                try
                {
                    string tagHeader = obj.Info1;
                    string key = obj.Info2 ?? "";
                    string value = obj.Info3 ?? "";
                    //收到最优待处理CUST_MD消息
                    if (obj.Info2 == "STCTagGroupDeal")
                    {
                        STC.Chrome.Plugin.Data.CefHandler.Instance.DictTagGroupDeal.AddOrUpdate(value, "1", (k, v) => "1");
                        CefHandler.Instance.PublishTagGroupDeal();
                    }
                    else if (obj.Info2 == "TagGroupSelection")
                    {
                        LogHelper.Info($"Receive PublishAppMsg pubJs Message! {CommonUtils.SerializeObject(obj)}");


                        LogHelper.Info($"Receive PublishAppMsg pubJs Message! {CommonUtils.SerializeObject(obj)}");
                        Dictionary<string, string> kk = new Dictionary<string, string>();
                        kk.Add(obj.Info2, obj.Info3 ?? "");
                        CefHandler.Instance.PublishPubJsMessage("TagGroupSelection", CommonUtils.SerializeObject(kk));

                        //bool isExist = false;
                        //Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(async () =>
                        //{
                        //    //恢复窗口当最小化时
                        //    //DRPTools.RestoreWindowWhenMinimized(System.Windows.Application.Current.MainWindow);

                        //    isExist = TabPageHelper.IsExistTabItem(tagHeader);

                        //    if (isExist)
                        //    {
                        //        System.Windows.Controls.TabItem selTabItem = AppUtils.Application.Panel.SelectedItem as System.Windows.Controls.TabItem;
                        //        if (selTabItem.Content is ChromeUserControl)
                        //        {
                        //            JavascriptResponse response = await ExecuteJavascript((selTabItem.Content as ChromeUserControl).Browser, $"pubJS('{key}','{value}');");
                        //            if (response.Success ? true : false)
                        //            {
                        //                LogHelper.Info($"Call pubJs Successed! {response.Message ?? string.Empty}");
                        //            }
                        //            else
                        //            {
                        //                LogHelper.Error(response.Message);
                        //            }
                        //        }
                        //    }
                        //}));
                    }
                    else if (obj.Info2 == "subscribeTagGroup")
                    {
                        //LogHelper.Info($"Receive PublishAppMsg pubJs Message! {CommonUtils.SerializeObject(obj)}");
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
                        {
                            //恢复窗口当最小化时
                            //DRPTools.RestoreWindowWhenMinimized(System.Windows.Application.Current.MainWindow);

                            List<string> tagGroupList = new List<string>();
                            //如果是*则发送到所有订阅TagGroup
                            if (tagHeader == "*")
                            {
                                tagGroupList = STC.Chrome.Plugin.Data.CefHandler.Instance.DictSubscribeTagGroup.Keys.ToList();
                            }
                            else
                            {
                                //如果为其他则是仅仅发送给指定的窗口
                                tagGroupList = tagHeader.Split(',').Where(p => !string.IsNullOrEmpty(p)).ToList();
                            }

                            if (tagGroupList.Count > 0)
                            {
                                //遍历所有订阅的TagGroup
                                foreach (string targetHeader in tagGroupList)
                                {
                                    if (obj.Info2 == "subscribeTagGroup")
                                    {
                                        //查找网页是否存在，如果存在则切换并执行pubJS方法
                                        TabItem selTabItem = TabPageHelper.GetExistTabItem(targetHeader);

                                        if (selTabItem != null)
                                        {
                                            if (selTabItem.Content is ChromeUserControl)
                                            {
                                                JavascriptResponse response = await ExecuteJavascript((selTabItem.Content as ChromeUserControl).Browser, $"pubJS('{key}','{value}');");
                                                if (response.Success ? true : false)
                                                {
                                                    LogHelper.Info($"Call pubJs Successed! {response.Message ?? string.Empty}");
                                                }
                                                else
                                                {
                                                    LogHelper.Error(response.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }));
                    }
                    else if (obj.Info2 == "refreshWebBondGroup")
                    {
                        LogHelper.Info($"Receive PublishAppMsg pubJs Message! {CommonUtils.SerializeObject(obj)}");

                        bool isExist = false;
                        Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                        {
                            //恢复窗口当最小化时
                            //DRPTools.RestoreWindowWhenMinimized(System.Windows.Application.Current.MainWindow);

                            isExist = TabPageHelper.IsExistTabItem(tagHeader);

                            if (isExist)
                            {
                                System.Windows.Controls.TabItem selTabItem = AppUtils.Application.Panel.SelectedItem as System.Windows.Controls.TabItem;

                                Task.Run(async () =>
                                {

                                    if (selTabItem.Content is ChromeUserControl)
                                    {
                                        JavascriptResponse response = await ExecuteJavascript((selTabItem.Content as ChromeUserControl).Browser, $"pubJS('{key}','{value}');");
                                        if (response.Success ? true : false)
                                        {
                                            LogHelper.Info($"Call pubJs Successed! {response.Message ?? string.Empty}");
                                        }
                                        else
                                        {
                                            LogHelper.Error(response.Message);
                                        }
                                    }
                                });

                            }
                        }));
                    }
                    else if (obj.Info2 == "OpenIssueRecogizationWin")
                    {
                        LogHelper.Info($"Receive PublishAppMsg OpenIssueRecogizationWin Message! {obj.Info3}");
                        OpenIssueRecogizationWin(obj.Info3);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            }
            else if (obj.Code == "OpenHistroyChromePlugin" && !string.IsNullOrEmpty(obj.Info1))
            {
                //打开历史插件
                List<string> keylist = CommonUtils.DeserializeObject<List<string>>(obj.Info1).Select(s => s.Replace("7-", "")).ToList();
                //没有插件就是web网页
                List<STC.Common.Model.Admin.Module> webModel = AdminHandler.Instance.GetWebModule().Where(p => keylist.Contains(p.modulename)).ToList();
                if (webModel != null)
                {
                    foreach (string key in keylist)
                    {
                        STC.Common.Model.Admin.Module m = webModel.FirstOrDefault(o => o.modulename == key);
                        if (m != null)
                        {
                            OpenWebUrl(m);
                        }
                    }
                }
            }
            else if (obj.Code == "PushChrome" && !string.IsNullOrEmpty(obj.Info1) && !string.IsNullOrEmpty(obj.Info2))
            {
                try
                {
                    string tagHeader = obj.Info1;
                    string key = obj.Info2 ?? "";
                    string value = obj.Info3 ?? "";
                    if (obj.Info2 == "pushJS")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
                        {
                            //if (LocalEnv.ConfInfo.EnableChromeDebug == "1")
                            LogHelper.Info($"Publish subscribe Message To Chrome [{tagHeader}]:{CommonUtils.SerializeObject(obj)}");
                            //恢复窗口当最小化时
                            //DRPTools.RestoreWindowWhenMinimized(System.Windows.Application.Current.MainWindow);
                            foreach (TabItem item in AppUtils.Application.Panel.Items)
                            {
                                if (tagHeader == "*")
                                {
                                    if (item.Content is ChromeUserControl)
                                    {
                                        CefSharpJsHandler.Instance.OnPushJS(value, tagHeader);
                                    }
                                }
                                else if (item.Header.ToString().Equals(tagHeader))
                                {
                                    if (item.Content is ChromeUserControl)
                                    {
                                        CefSharpJsHandler.Instance.OnPushJS(value, tagHeader);
                                        break;
                                    }
                                }
                            }

                            object win = CefHandler.Instance.GetExistWin(tagHeader);
                            if (win != null)
                            {
                                if (win.GetType() == typeof(BdbBorderWin))
                                {
                                    if ((win as BdbBorderWin).GetMainControl() is ChromeUserControl)
                                    {
                                        CefSharpJsHandler.Instance.OnPushJS(value, tagHeader);
                                    }
                                }
                            }
                        }));
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            }
            else if (obj.Code == "getFQ" && !string.IsNullOrEmpty(obj.Info1) && !string.IsNullOrEmpty(obj.Info2))
            {   
                //获取Toaken
                //todo...
                try
                {
                    string winHeader = obj.Info1;
                    string key = obj.Info2 ?? "";
                    string value = obj.Info3 ?? "";
                    if (key == "GetToken")
                    {
                        //获取Token
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(async () =>
                        {
                            if (!CefHandler.Instance.IsValidServerToken)
                            {
                                CefHandler.Instance.GetToken();
                            }
                            else
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "SendToken" });
                            }
                        }));
                    }
                    else if (key == "SendToken")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(async () =>
                        {
                            NetMessage resmsg = new NetMessage();
                            //发送Token
                            if (!string.IsNullOrWhiteSpace(CefHandler.Instance.FQClientToken?.token?.clientToken) && !string.IsNullOrWhiteSpace(CefHandler.Instance.FqServerToken?.token?.serverToken))
                            {
                                SendTokenResponse res = new SendTokenResponse() { Code = "0000", Msg = "Success", token = CefHandler.Instance.FqServerToken?.token?.serverToken + CefHandler.Instance.FQClientToken?.token?.clientToken ?? "", ExpiresTime = CefHandler.Instance.TokenExpirationTime, expiresIn = CefHandler.Instance.expiresIn, createTs = CefHandler.Instance.createTs };
                                resmsg.Topic = "fq_token_bind_callback";
                                resmsg.Data = CommonUtils.SerializeObject(res);
                                resmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                //先判断servertoken
                                if (string.IsNullOrWhiteSpace(CefHandler.Instance.FqServerToken?.token?.serverToken))
                                {
                                    //获取Token失败
                                    SendTokenResponse res = new SendTokenResponse() { Code = $"{CefHandler.Instance.FqServerToken?.errCode}", Msg = $"{CefHandler.Instance.FqServerToken.errMsg}", token = "" };
                                    resmsg.Topic = "fq_token_bind_callback";
                                    resmsg.Data = CommonUtils.SerializeObject(res); ;
                                    resmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                }
                                else
                                {
                                    //获取Token失败
                                    SendTokenResponse res = new SendTokenResponse() { Code = $"{CefHandler.Instance.FQClientToken?.errCode}", Msg = $"{CefHandler.Instance.FQClientToken.errMsg}", token = "" };
                                    resmsg.Topic = "fq_token_bind_callback";
                                    resmsg.Data = CommonUtils.SerializeObject(res); ;
                                    resmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                }
                            }

                            CefHandler.Instance.add(resmsg);
                            //发送数据
                            LogHelper.Info("SendToken:" + resmsg.Data);
                        }));
                    }
                    else if (key == "HideWebFQ")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(async () =>
                        {
                            //隐藏窗体
                            object win = CefHandler.Instance.GetExistWin("WebFQ");
                            if (win != null)
                            {
                                if (win.GetType() == typeof(BdbBorderWin))
                                {
                                    if ((win as BdbBorderWin).GetMainControl() is ChromeUserControl)
                                    {

                                        ChromiumWebBrowser browser = ((win as BdbBorderWin).GetMainControl() as ChromeUserControl).ChromiumWebBrowser;
                                        if (string.IsNullOrWhiteSpace(browser.Title))
                                        {
                                            ((BdbBorderWin)win).Close();
                                            CefHandler.Instance.RemoveWindow("WebFQ");
                                        }
                                        else
                                        {
                                            ((BdbBorderWin)win).Hide();
                                            LogHelper.Info("HideWebFQ.");
                                        }
                                    }
                                }
                            }
                        }));
                    }
                    else if (key == "FQRefresh")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(async () =>
                        {
                            //隐藏窗体
                            object win = CefHandler.Instance.GetExistWin("WebFQ");
                            if (win != null)
                            {
                                if (win.GetType() == typeof(BdbBorderWin))
                                {
                                    var mainControl = (win as BdbBorderWin).GetMainControl();
                                    System.Reflection.MethodInfo ChromeReload = mainControl.GetType().GetMethod("Reload");
                                    ChromeReload?.Invoke(mainControl, new object[] { "" });
                                }
                            }
                        }));
                    }
                    else if (key == "FQSetAction")
                    {
                        if (FinQMdHander.Instance.FQSetActionData != null)
                        {
                            if (CefHandler.Instance.IsFinQWebReady == false)
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQRefresh", Info3 = "" });
                                return;
                            }

                            NetMessage setresmsg = new NetMessage();
                            setresmsg.Topic = "setActionData";
                            setresmsg.Data = FinQMdHander.Instance.FQSetActionData.ToString();
                            setresmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            CefHandler.Instance.add(setresmsg);
                        }
                    }
                    else if (key == "FQChangeBond")
                    {
                        if (FinQMdHander.Instance.FQChangeBondData != null)
                        {
                            if (CefHandler.Instance.IsFinQWebReady == false)
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQRefresh", Info3 = "" });
                                return;
                            }

                            NetMessage setreschgmsg = new NetMessage();
                            setreschgmsg.Topic = "changeBond";
                            setreschgmsg.Data = FinQMdHander.Instance.FQChangeBondData.ToString();
                            setreschgmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            CefHandler.Instance.add(setreschgmsg);
                        }
                    }
                    else if (key == "FQTokenFanout")
                    {
                        NetMessage setreschgmsg = new NetMessage();
                        setreschgmsg.Topic = "fq_token_fanout";
                        setreschgmsg.Data = string.IsNullOrWhiteSpace(value) ? CefHandler.Instance.FQTokenFanout : value; //如果是自己的账号，则除了透传外，还要发给报价窗口的界面，处理本地的UI,如果是其它账户，则仅仅透传
                        setreschgmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        CefHandler.Instance.add(setreschgmsg);
                    }
                    else if (key == "GetRemainTime")
                    {
                        if (!TokenCountdownHandler.Instance.IsTickSubscribed)
                        {
                            TokenCountdownHandler.Instance.Tick += (s, remain) =>
                            {
                                NetMessage setresmsg = new NetMessage();
                                setresmsg.Topic = "fq_getRemainTime";
                                setresmsg.Data = "{\"RemainTime\":" + $"\"{TokenCountdownHandler.Instance.SecondsToTimeString(remain)}" + "\"}"; ;
                                setresmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                CefHandler.Instance.add(setresmsg);
                            };

                            TokenCountdownHandler.Instance.IsTickSubscribed = true;
                        }
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            }
        }




        private async Task<JavascriptResponse> ExecuteJavascript(ChromiumWebBrowser _Browser, string script)
        {
            JavascriptResponse response = new JavascriptResponse();
            if (_Browser != null && _Browser.IsBrowserInitialized)
            {
                try
                {
                    response = await _Browser.GetBrowser().MainFrame.EvaluateScriptAsync(script);
                    return response;
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.Message = $"执行 JavaScript 发生异常: {ex.Message}";
                }
            }
            else
            {
                response.Success = false;
                response.Message = "浏览器尚未初始化完成。";
            }

            return response;
        }


        /// <summary>
        /// 打开网页
        /// </summary>
        /// <param name="urlmodel"></param>
        private void OpenWebUrl(STC.Common.Model.Admin.Module urlmodel)
        {
            string url = urlmodel.inf2;
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.Sid))
            {
                url = $"{url}?username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.Sid}";
            }
            else
            {
                if (!url.Contains("?"))
                {
                    url = $"{url}?username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.SessionID}";
                }
                else
                {
                    url = $"{url}&username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.SessionID}";
                }
            }

            LogHandler.Debug("OpenWebUrl:" + url);

            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = false, IsShowStatusBar = false };
            cw.Key = urlmodel.id.ToString();
            cw.Title = urlmodel.modulename;
            cw.UseChromePluginObject = true;
            cw.CallBackObjectInstance.Add("cef|", "CefSharpJsHandler");
            cw.IsShowMenuHandler = false;
            cw.IsShowAddressViewSourceDevTools = false;
            cw.ForceTargetSelf = false;
            cw.ForceTargetOtherPage = true;

            //本地地址 //string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/index.html?timestamp={new Random(10000).NextDouble()}";
            if (!cw.Url.StartsWith("http"))
                cw.Url = AppDomain.CurrentDomain.BaseDirectory + $"{url}&timestamp={new Random(10000).NextDouble()}";

            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }
        #endregion

        #region 打开发行人识别
        public void OpenIssueRecogizationWin(string copytext)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                IssueRecogizationWin issueRecogizationWin = new IssueRecogizationWin(true);
                issueRecogizationWin.RefreshSelectedAction = RefreshSelectedAction;
                WindowManager.Instance.OpenOnlyOneWindow("IssueRecogizationWin", issueRecogizationWin, System.Windows.Application.Current.MainWindow);
            }));
        }

        /// <summary>
        /// 发行人识别回调
        /// </summary>
        /// <param name="items"></param>
        private void RefreshSelectedAction(List<Node> items)
        {
            if (items?.Count>0)
            {
                NetMessage setreschgmsg = new NetMessage();
                setreschgmsg.Topic = "IssueRecogizationCallBack";
                setreschgmsg.Data = CommonUtils.SerializeObject(items);
                setreschgmsg.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                CefHandler.Instance.add(setreschgmsg);
            }
        }
        #endregion

        #region QB在线小助手
        /// <summary>
        /// 打开QB在线小助手
        /// </summary>
        /// <param name="cw"></param>
        public void OpenQBAssistantInWindow(ChromeWinModel cw)
        {
            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                QBAssistantWin myTradingPopupWin = new QBAssistantWin();
                myTradingPopupWin.WindowState = cw.WindowState;
                myTradingPopupWin.Width = cw.Width;
                myTradingPopupWin.Height = cw.Height;
                myTradingPopupWin.SetTitleAndKey(cw.Key, cw.Title);
                myTradingPopupWin.Title = cw.Title;
                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                myTradingPopupWin.OnRefreshEvent += QBAssistantWin_OnRefreshEvent;
                try
                {
                    if (!cw.IsShowAddressBar)
                        cw.IsShowAddressBar = cw.IsShowStatusBar = cw.IsShowMenuHandler = LocalEnv.ConfInfo.ChromeShowToolBar == "1" ? true : false;

                    if (LocalEnv.ConfInfo.ChromeShowToolBar == "1")
                    {
                        cw.IsShowAddressViewSourceDevTools = true;
                    }

                    ChromeUserControl chrom = new ChromeUserControl(cw);
                    chrom.CurrentUrl = cw.Url;
                    chrom.SetParentWindow(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(chrom);
                    chrom.SetTopBarVisible(cw.IsShowAddressBar);
                    chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                    chrom.SetHideAddressViewSourceDevTools(cw.IsShowAddressViewSourceDevTools);
                    chrom.SetAddress(cw.Url);
                    chrom.Browser.WpfKeyboardHandler = new CefSharp.Wpf.Experimental.WpfImeKeyboardHandler(chrom.Browser); // 在初始化 ChromiumWebBrowser 时设置 WpfKeyboardHandler 为 WpfImeKeyboardHandler
                    
                    myTradingPopupWin.Width = cw.Width;
                    myTradingPopupWin.Height = cw.Height;

                    WindowManager.Instance.OpenOnlyOneWindow($"{cw.Key}", myTradingPopupWin, null);
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex.Message);
                }
            }));
        }

        /// <summary>
        /// 在线小助手刷新
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void QBAssistantWin_OnRefreshEvent(object arg1, RoutedEventArgs arg2)
        {
            if (arg1.GetType() == typeof(QBAssistantWin))
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                {
                    var mainControl = (arg1 as QBAssistantWin).GetMainControl();
                    System.Reflection.MethodInfo ChromeReload = mainControl.GetType().GetMethod("Reload");
                    ChromeReload?.Invoke(mainControl, new object[] { "" });
                }));
            }
        }

        #endregion
    }
}
