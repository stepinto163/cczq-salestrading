using CefSharp;
using CefSharp.Wpf;
using CefSharp.Wpf.Handler;
using com.gateway.log;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using STC.Chrome.Plugin.Data;
using STC.Chrome.Plugin.Demo;
using STC.Chrome.Plugin.Model;
using STC.Common.Model.Chrome;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace STC.Chrome.Plugin
{
    public partial class ChromeUserControl : UserControl
    {
        public Window myWindow { get; set; }
        
        /// <summary>
        /// CurrentUrl
        /// </summary>
        private string _cururl = "";
        public string CurrentUrl
        {
            get { return _cururl; }
            set
            {
                _cururl = value;
            }
        }

        /// <summary>
        /// key
        /// </summary>
        private string _key = "Chrome";
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
            }
        }

        /// <summary>
        /// ChromiumWebBrowser 对象
        /// </summary>
        public ChromiumWebBrowser ChromiumWebBrowser { get; set; }

        /// <summary>
        /// IBrowser 对象
        /// </summary>
        public IBrowser IBrowser { get; set; }

        /// <summary>
        /// 调用参数
        /// </summary>
        public ChromeWinModel ChromeWinModel;

        public ChromeUserControl(ChromeWinModel cwm)
        {
            Key = cwm.Key;
            this.ChromeWinModel = cwm;

            try
            {
                InitializeComponent();

                ChromiumWebBrowser = Browser;
                Loaded += WebHostPlugin_Loaded;
                Browser.FrameLoadStart += Browser_FrameLoadStart;
                Browser.FrameLoadEnd += Browser_FrameLoadEnd;
                Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
                Browser.PreviewMouseWheel += Browser_PreviewMouseWheel;
                Browser.PreviewKeyDown += Browser_PreviewKeyDown;
                Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
                Browser.TitleChanged += Browser_TitleChanged;
                Browser.DownloadHandler = new DownloadHandler();

                if (LocalEnv.ConfInfo.EnableChromeConsoleLog == "1")
                    Browser.ConsoleMessage += Browser_ConsoleMessage;

                Browser.LoadError += (sender, args) =>
                {
                    if (!args.Frame.IsMain) return;
                };

                if (cwm.ForceTargetSelf)
                {
                    Browser.LifeSpanHandler = new LifeSpanHandler();
                }

                if (cwm.ForceTargetOtherPage)
                {
                    Browser.LifeSpanHandler = new LifeSpanPageHandler();
                }

                if (LocalEnv.ConfInfo.ChromeShowToolBar == "0" || cwm.IsShowMenuHandler == false)
                {
                    //关闭鼠标右键菜单
                    Browser.MenuHandler = new MenuHandler();
                }
                else
                {
                    Browser.MenuHandler = new ContextMenuHandler(true);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        /// <summary>
        /// 窗口标题改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //修改窗口标题
            string title = Browser.Title;

            if (this.myWindow != null && (this.myWindow.GetType() != typeof(STC.Common.View.UI.BorderWin) || this.myWindow.GetType() != typeof(STC.Common.View.UI.BdbBorderWin)))
            {
                MethodInfo ChromeUpdAddress = this.myWindow.GetType().GetMethod("SetWebPageTitle");
                ChromeUpdAddress?.Invoke(this.myWindow, new object[] { title });
            }
            if (BrowserTitleChanged != null)
                BrowserTitleChanged?.Invoke(this);
        }

        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LocalEnv.ConfInfo.ChromeProxyServer))
            {
                string proxy = LocalEnv.ConfInfo.ChromeProxyServer;
                SetProxyNew(Browser, proxy);
                LogHelper.Info($"Browser Set Proxy! {proxy}");
                return;
            }

            if (ProxyUtils.ProxyInfo != null && ProxyUtils.ProxyInfo.AccessType != null)
            {
                if (ProxyUtils.ProxyInfo.AccessType == "3")
                    SetProxyNew(Browser, ProxyUtils.ChromeProxyinfo);
            }
        }

        #region Zoom In Or Out
        private void Browser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                Browser.Reload();
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (e.Key == System.Windows.Input.Key.D0 || e.Key == System.Windows.Input.Key.NumPad0))
            {
                Browser.SetZoomLevel(0);

                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    btn_SearchBtn.Visibility = Visibility.Collapsed;
                }));
            }
        }
        
        /// <summary>
        /// 重置为默认缩放级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowReset_Click(object sender, RoutedEventArgs e)
        {
            ShowResetZoomTools();
        }

        public void ShowResetZoomTools()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (Browser != null)
                {
                    Browser.SetZoomLevel(0);
                    btn_SearchBtn.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void Browser_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }
            try
            {
                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                else if (e.Delta < 0)
                {
                    ZoomOut();
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {

            }
        }
        private void ZoomIn()
        {
            if (Browser.ZoomLevel < 4)
            {
                Browser.ZoomInCommand.Execute(null);

                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    btn_SearchBtn.Visibility = Visibility.Visible;
                }));
            }
        }

        private void ZoomOut()
        {
            if (Browser.ZoomLevel > -4)
            {
                Browser.ZoomOutCommand.Execute(null);

                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    btn_SearchBtn.Visibility = Visibility.Visible;
                }));

            }
        }

        #endregion

        /// <summary>
        /// 在网页中注册对象 84以后的版本注册后想要在js中使用callbackObjectForJs类中的方法，需要先在js中调用，因为 CefSharpSettings.LegacyJavascriptBindingEnabled = true; 已经取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //隐藏滚动条
            ////e.Browser.MainFrame.ExecuteJavaScriptAsync("document.body.style.overflow = 'hidden'");
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                LogHelper.Info("Browser_FrameLoadEnd.");

                try
                {
                    //在网页中注册对象
                    if (this.ChromeWinModel.CallBackObjectInstance != null)
                    {
                        if (this.ChromeWinModel.CallBackObjectInstance.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("(function(){");
                            foreach (var x in ChromeWinModel.CallBackObjectInstance)
                            {
                                sb.Append($"CefSharp.BindObjectAsync('{x.Key.Split('|')[0]}');");
                            }
                            sb.Append("})();");
                            Browser.GetFocusedFrame().EvaluateScriptAsync(sb.ToString());

                            LogHelper.Info($"Browser_FrameLoadEnd ! EvaluateScriptAsync Successed!");
                        }
                    }
                    else
                    {
                        LogHelper.Info($"Browser_FrameLoadEnd !");
                    }

                    //检测到对象时转为 JSON 字符串打印到控制台
                    if (LocalEnv.ConfInfo.EnableChromeConsoleLog == "1")
                    {
                        Browser.GetMainFrame().ExecuteJavaScriptAsync(@"
                        (function() {
                            var oldLog = console.log;
                            console.log = function() {
                                var args = Array.from(arguments).map(a => {
                                    if (typeof a === 'object') {
                                        try {
                                            return JSON.stringify(a);
                                        } catch (e) {
                                            return '[Unserializable Object]';
                                        }
                                    }
                                    return a;
                                });
                                oldLog.apply(console, args);
                            };
                        })();
                        ");
                    }

                    ////关闭自动修改窗口标题功能
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        //如果不是例子代码则关闭演示按钮
                        if (Browser != null)
                        {
                            CurrentUrl = Browser.Address;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            });
        }

        #region Dispose
        /// <summary>
        /// 窗口标题变化
        /// </summary>
        public event Action<object> BrowserTitleChanged;


        // --- IDisposable 实现 ---
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                   // **重要：释放浏览器实例**
                    // 确保在UI线程上执行（如果Dispose可能从非UI线程调用）
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        if (Browser != null && !Browser.IsDisposed)
                        {
                            Browser.Dispose();
                        }
                    });
                }

                // 释放非托管资源 (CefSharp 主要在这里处理)
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(true);
            GC.SuppressFinalize(this); // 阻止垃圾回收器调用Finalize
        }

        // (可选) Finalizer，以防 Dispose 没有被显式调用
        ~ChromeUserControl()
        {
            Dispose(false);
        }
        #endregion

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Browser.IsBrowserInitialized)
            {
                LogHelper.Info($"Browser IsBrowserInitialized!");
                Browser.Address = this.ChromeWinModel.Url;
                Browser.GetMainFrame().LoadUrl(this.ChromeWinModel.Url);
                if (LocalEnv.ConfInfo.EnableChromeConsoleLog == "0")
                    LogHelper.Info($"Browser Set Address!");
                else
                    LogHelper.Info($"Browser Set Address! {this.ChromeWinModel.Url}");

                if (ChromeWinModel.CallBackObjectInstance != null)
                {
                    foreach (var x in ChromeWinModel.CallBackObjectInstance)
                    {
                        switch (x.Value.ToString())
                        {
                            case "CefSharpJsHandler":
                                //异步调用 网页发布消息到客户端 正式版
                                CefSharpJsHandler.Instance.SetBrowser(Browser);
                                Browser.JavascriptObjectRepository.Register(x.Key.Split('|')[0], CefSharpJsHandler.Instance, isAsync: true);
                                break;
                        }
                    }
                }

                
                Task.Run(async () =>
                {
                    
                    // 1. 初始等待 
                    await Task.Delay(CefHandler.Instance.GetBaseAddress(LocalEnv.ConfInfo.WebFQAddress) == CefHandler.Instance.GetBaseAddress(this.ChromeWinModel.Url) ? 5000 : 30000);

                    // 2. 异步获取内容 (不再阻塞线程)
                    string html = GetHtmlFromChromiumWebBrowser(1000);

                    if (string.IsNullOrWhiteSpace(html))
                    {
                        // 3. 仅在切换到 UI 线程执行刷新动作
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            // 假设 ForceNavigateAsync 是异步的
                            await ForceNavigateAsync(Browser, this.ChromeWinModel.Url);
                        });
                    }
                });
                
            }
            else if (Browser.IsDisposed)
            {

            }

        }


        private void WebHostPlugin_Loaded(object sender, RoutedEventArgs e)
        {
            LogHelper.Info($"WebHostPlugin_Loaded."); 
        }

        public void SetAddress(string url)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Reload();
                if (Browser != null)
                {
                    if (LocalEnv.ConfInfo.EnableChromeConsoleLog == "1")
                        LogHelper.Info($"SetAddress. {url}");
                    else
                        LogHelper.Info($"SetAddress.");

                    this.ChromeWinModel.Url = url;
                }

            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 设置父级窗口
        /// </summary>
        /// <param name="mywin"></param>
        public void SetParentWindow(object mywin)
        {
            this.myWindow = (Window)mywin;
        }

        /// <summary>
        /// 设置工具栏是否可见
        /// </summary>
        /// <param name="isvisible"></param>
        public void SetTopBarVisible(bool isvisible)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                TopBar.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        /// <summary>
        /// 设置状态栏是否可见
        /// </summary>
        /// <param name="isvisible"></param>
        public void SetStatusBarVisible(bool isvisible)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                StatusBar.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }


        /// <summary>
        /// 设置地址栏，查看原代码按钮，和开发着工具按钮是否可见
        /// </summary>
        /// <param name="isvisible"></param>
        public void SetHideAddressViewSourceDevTools(bool isvisible)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                txtBoxAddress.Visibility = btn_ViewCode.Visibility = btn_ShowDevTools.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        /// <summary>
        /// 显示开发工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDevTools_Click(object sender, RoutedEventArgs e)
        {
            ShowDevTools();
        }

        public void ShowDevTools()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (Browser != null)
                {
                    Browser.ShowDevTools();
                }
            }));
        }


        /// <summary>
        /// 强制重新加载忽略缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }


        /// <summary>
        /// 强制重新加载忽略缓存
        /// </summary>
        public void Reload(string url = "")
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(async () =>
            {
                //清理缓存
                string msg = $"Chrome Reload: [{Browser.Title}]";
                LogHelper.Info(msg);
                CefSharp.ICookieManager cookieManager = Cef.GetGlobalCookieManager();
                cookieManager.DeleteCookies("", "");
                await ForceNavigateAsync(Browser, Browser.Address);
            }));
        }


        public void StopLoad_Click(object sender, RoutedEventArgs e)
        {
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var cefBrowser = Browser.GetBrowser();
                if (cefBrowser != null)
                {
                    cefBrowser.StopLoad();                       // 发出停止请求
                }
            });
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="cwb"></param>
        /// <param name="Address">127.0.0.1:8118 sock5: "socks5://127.0.0.1:1080"</param>
        async private void SetProxyNew(ChromiumWebBrowser cwb, string Address)
        {
            await Cef.UIThreadTaskFactory.StartNew(delegate
            {
                LogHelper.Info($"SetProxy:{Address}");
                var rc = cwb.GetBrowser().GetHost().RequestContext;
                Dictionary<string, string> v = new Dictionary<string, string>();
                v["mode"] = "fixed_servers";
                v["server"] = Address;
                string error;
                bool success = rc.SetPreference("proxy", v, out error);
            });
        }


        #region c#端 调用 JS Demo
        /// <summary>
        /// c# 调用 JS, 测试时取消xaml <!--Button Style="{DynamicResource Button.CallJs}" x:Name="btn_CallJS" Click="Button_Click" Grid.Column="7" ToolTip="C#端调用网页端的Js演示"/--> 两侧的注释
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('loadModel').click();");
            //JavascriptResponse x = await Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loop();");
            //JavascriptResponse x = await Browser.GetBrowser().MainFrame.EvaluateScriptAsync("pubJS('tableGroupAlert','3年国债订阅组');");
            //if (x.Success ? true : false)
            //{
            //    JavascriptResponse res = await Browser.GetBrowser().MainFrame.EvaluateScriptAsync("document.getElementById('btnLogin').click();");

            //}

            string winkey = $"TestChrome";
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (WindowManager.Instance.IsExist(winkey) == false)
                {
                    BorderWin myTradingPopupWin = new BorderWin();
                    myTradingPopupWin.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0A0F0E"));
                    JsCSharpDemo tradeBookView = new JsCSharpDemo(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(tradeBookView);
                    myTradingPopupWin.SetMinVisible(false);
                    myTradingPopupWin.SetMaxVisible(false);
                    myTradingPopupWin.SetTitleAndKey("测试C#调用Js", winkey);
                    myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                    myTradingPopupWin.WindowStyle = WindowStyle.None;
                    myTradingPopupWin.WindowState = WindowState.Normal;
                    myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    myTradingPopupWin.Owner = null;
                    myTradingPopupWin.Width = 322;
                    myTradingPopupWin.Height = 280;
                    myTradingPopupWin.MinHeight = myTradingPopupWin.Height;
                    myTradingPopupWin.MinWidth = myTradingPopupWin.Width;
                    myTradingPopupWin.Topmost = true;
                    tradeBookView.MOK += TradeBookView_MOK;
                    WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                }
            }));
        }

        private void TradeBookView_MOK(object sender, RoutedEventArgs e)
        {
            try
            {
                string data = e.OriginalSource.ToString();
                string target = data.Split(':')[0] ?? "";
                string key = data.Split(':')[1] ?? "";
                string value = data.Split(':')[2] ?? "";

                //发布消息
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "pubJS", Info1 = target, Info2 = key, Info3 = value });
                LogHelper.Info($"PublishAppMsg pubJs Message Successed! {key}:{value}");

                //JavascriptResponse x = await Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"pubJS('{key}','{value}');");
                //if (x.Success ? true : false)
                //{
                //    LogHelper.Info($"Call pubJs Successed! {x.Message??string.Empty}");
                //}
                //else
                //{ 
                //    LogHelper.Error(x.Message);
                //}
            }
            catch (Exception ex)
            { 
                LogHelper.Error(ex.Message);
            }
            
        }
        #endregion

        #region 使用外置编辑器打开源代码 Developing
        [DllImport("shell32.dll")]
        public static extern int ShellExecute(IntPtr hwnd, string lpszOp, string lpszFile, string lpszParams, string lpszDir, int FsShowCmd);
        /// <summary>
        /// 查看源代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnViewCode_Click(object sender, RoutedEventArgs e)
        {
            string msg = GetHtmlFromChromiumWebBrowser();
            string path = AppDomain.CurrentDomain.BaseDirectory +"plugin\\cache\\Temp\\"; //文件名称
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filename = path + $"tmpHtml.html";
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            
            File.WriteAllText(filename, msg);

            string appName = AppDomain.CurrentDomain.BaseDirectory + "bin\\SciTE.exe"; //文件名称
            string ext = $"-open:{Path.GetFileName(filename)}";
            ShellExecute(IntPtr.Zero, "Open", appName, ext, path, 1);
        }

        private string GetHtmlFromChromiumWebBrowser()
        {
            Task<string> TaskHtml = Browser.GetBrowser().MainFrame.GetSourceAsync();
            string HtmlResponse = TaskHtml.Result;
            return HtmlResponse;
        }
        #endregion

        private async void OnAddressTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string url = (sender as TextBox).Text;

                string html = GetHtmlFromChromiumWebBrowser(1000);

                if (!string.IsNullOrWhiteSpace(html))
                    Browser.GetMainFrame().LoadUrl(url);
                else
                    await ForceNavigateAsync(Browser, url);
            }
        }

        /// <summary>
        /// 刷新机制：判断网页是否为空，如果为空则强制杀 Browser.exe 并重新加载
        /// </summary>
        public async Task ForceNavigateAsync(ChromiumWebBrowser browser, string url, bool isReload = true)
        {
            try
            {
                string html = GetHtmlFromChromiumWebBrowser(3000);
                if (string.IsNullOrWhiteSpace(html))
                {
                    // Step 1: 杀掉所有 Browser.exe
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            var procs = Process.GetProcessesByName("Browser");
                            if (procs.Length == 0) break;

                            foreach (var p in procs)
                            {
                                try
                                {
                                    p.Kill();
                                    p.WaitForExit(1);
                                }
                                catch { /* 忽略异常 */ }
                            }

                            // 0.5秒后再次检测
                            await Task.Delay(300);

                            // 如果已经没有进程了，立刻退出
                            if (Process.GetProcessesByName("Browser").Length > 0)
                                break;
                        }
                    });

                    // Step 2: 等待 0.1秒
                    await Task.Delay(100);

                    // Step 3: 确认 Cef 初始化好了
                    if (browser.IsInitialized)
                    {
                        browser.LoadUrl(url);
                    }
                    else
                    {
                        // 如果还没初始化，就等一等
                        for (int i = 0; i < 20; i++) // 最多等 2 秒
                        {
                            if (browser.IsInitialized)
                            {
                                browser.LoadUrl(url);
                                break;
                            }
                            await Task.Delay(100);
                        }
                    }
                }
                else
                {
                    // 网页正常，直接刷新
                    if (isReload)
                        browser.Reload();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"刷新时发生错误: {ex.Message}");
            }
        }

        public string GetHtmlFromChromiumWebBrowser(int timeoutMs = 2000)
        {
            try
            {
                var task = Browser.GetBrowser().MainFrame.GetSourceAsync();
                // 等待任务完成，超时则返回 string.Empty
                if (task.Wait(timeoutMs))
                {
                    return task.Result ?? string.Empty;
                }
                else
                {
                    // 超时
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                string msg = $"Chrome Console from [{(sender as ChromiumWebBrowser)?.Title}]:{e.Message}";
                switch (e.Level)
                {
                    case LogSeverity.Info:
                        LogHelper.Info(msg);
                        break;
                    case LogSeverity.Error:
                    case LogSeverity.Fatal:
                        LogHelper.Error(msg);
                        break;
                    case LogSeverity.Warning:
                        LogHelper.Debug(msg);
                        break;
                    default:
                        LogHelper.Info(msg);
                        break;

                }
            }));
        }
    }
}
