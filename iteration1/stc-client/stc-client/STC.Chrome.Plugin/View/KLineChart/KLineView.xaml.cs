using CefSharp;
using CefSharp.Wpf;
using com.gateway.log;
using STC.Chrome.Plugin.ViewModel.KLineChartModel;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static STC.Common.Utils.HomePageDataHandler;

namespace STC.Chrome.Plugin.View.KLineChart
{
    /// <summary>
    /// KLineView.xaml 的交互逻辑
    /// </summary>
    public partial class KLineView : UserControl
    {
        public Window myWindow { get; set; }
        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }


        private KLineViewModel klineViewModel = null;

        public double TopBarHeight = 30;

        // Using a DependencyProperty as the backing store for BondCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddressProperty =
        DependencyProperty.Register("Address", typeof(string), typeof(KLineView), new PropertyMetadata(string.Empty, (o, e) =>
        {
            var chromeUserControl = o as KLineView;
            chromeUserControl.Address = e.NewValue?.ToString();
            chromeUserControl.SetAddress(e.NewValue?.ToString());
        }));


        public KLineView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// key
        /// </summary>
        public string Key = "ChromeKLine";

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

        public KLineView(ChromeWinModel cwm)
        {
            Key = cwm.Key;
            this.ChromeWinModel = cwm;

            klineViewModel = new KLineViewModel(this);

            try
            {
                InitializeComponent();
                //Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
                //Loaded += KLineView_Loaded;
                ChromiumWebBrowser = Browser;

                Browser.LoadingStateChanged += WebBrowser_LoadingStateChanged;
                Browser.FrameLoadEnd += Browser_FrameLoadEnd;
                //CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                CefSharpSettings.ConcurrentTaskExecution = true;
                CefSharpSettings.WcfEnabled = true;
                CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
                CefSharpSettings.ConcurrentTaskExecution = true;
                Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
                Unloaded += ChromeUserControl_Unloaded;
                //Browser.DownloadHandler = new DownloadHandler();

                //////注册JS对象cefAsyncJS，_Event中方法为JS实际调用方法。示例：cefAsyncJS.login("");
                ////Browser.JavascriptObjectRepository.Register("cefJS", new CefJsEventDemo(this), isAsync: false, options: BindingOptions.DefaultBinder);
                //////注入c#方法到前端页面 
                ////Browser.JavascriptObjectRepository.Register("csharpTest", new JavascriptCallCharpTest(), true, BindingOptions.DefaultBinder);
                if (cwm.UseChromePluginObject)
                {
                    foreach (var x in cwm.CallBackObjectInstance)
                    {
                        switch (x.Value.ToString())
                        {
                            case "KLineViewModel":
                                //直接
                                Browser.JavascriptObjectRepository.Register(x.Key.Split('|')[0], klineViewModel, isAsync: x.Key.ToString().Contains("|"), options: BindingOptions.DefaultBinder);
                                break;
                            //case "JavascriptCallCharpTest":
                            //    //异步调用
                            //    Browser.JavascriptObjectRepository.Register(x.Key.Split('|')[0], new JavascriptCallCharpTest(this), isAsync: x.Key.ToString().Contains("|"), options: BindingOptions.DefaultBinder);
                            //    break;
                        }
                    }
                }
                else
                {
                    foreach (var x in cwm.CallBackObjectInstance)
                    {
                        Browser.JavascriptObjectRepository.Register(x.Key.Split('|')[0], x.Value, isAsync: x.Key.ToString().Contains("|"), options: BindingOptions.DefaultBinder);
                    }
                }

                //关闭鼠标右键菜单
                Browser.MenuHandler = null;


            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex);
                MessageBox.Show(ex.ToString());
            }

        }

        private void KLineView_Unloaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Browser.IsBrowserInitialized)
            {
                IBrowser = Browser.GetBrowser();
                this.SetProxy(this.ChromeWinModel.Proxy);

                KLineView_Loaded(null, null);
            }
            else if (Browser.IsDisposed)
            {

            }
        }

        /// <summary>
        /// 设置代理 http:"127.0.0.1:8118" sock5: "socks5://127.0.0.1:1080"
        /// </summary>
        /// <param name="proxy"></param>
        public void SetProxy(string proxy)
        {
            //http代理
            //SetProxyNew(Browser, "127.0.0.1:8118");
            //sock5代理
            //SetProxyNew(Browser, "socks5://127.0.0.1:1080");
            //是否开代理
            SetProxyNew(Browser, proxy);
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="cwb"></param>
        /// <param name="Address"> http:"127.0.0.1:8118" sock5: "socks5://127.0.0.1:1080"</param>
        async private void SetProxyNew(ChromiumWebBrowser cwb, string Address)
        {
            await Cef.UIThreadTaskFactory.StartNew(delegate
            {
                var rc = cwb.GetBrowser().GetHost().RequestContext;
                Dictionary<string, string> v = new Dictionary<string, string>();
                v["mode"] = "fixed_servers";
                v["server"] = Address;
                string error;
                bool success = rc.SetPreference("proxy", v, out error);
            });
        }

        private void WebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {

            }
        }

        /// <summary>
        /// 在网页中注册对象 84以后的版本注册后想要在js中使用callbackObjectForJs类中的方法，需要先在js中调用，因为 CefSharpSettings.LegacyJavascriptBindingEnabled = true; 已经取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //隐藏滚动条
            ////e.Browser.MainFrame.ExecuteJavaScriptAsync("document.body.style.overflow = 'hidden'");
            //在网页中注册对象
            ////StringBuilder sbt = new StringBuilder();
            ////sbt.Append("(function(){")
            ////    .Append("CefSharp.BindObjectAsync('cefJS');")
            ////    .Append("CefSharp.BindObjectAsync('csharpTest');")
            ////    .Append("})();");
            ////Browser.GetFocusedFrame().EvaluateScriptAsync(sbt.ToString());


            //在网页中注册对象
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
            }

            //关闭演示按钮
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                //如果不是例子代码则关闭演示按钮
                if (Browser != null)
                {
                    //string address = Browser.Address.ToString();
                    //if (!address.ToLower().Contains("/client/plugin/html/index.html"))
                    //{
                    //    btn_CallJS.Visibility = Visibility.Collapsed;
                    //}
                    //修改窗口标题

                    if (this.myWindow != null)
                    {
                        MethodInfo ChromeUpdAddress = this.myWindow.GetType().GetMethod("SetWebPageTitle");
                        ChromeUpdAddress?.Invoke(this.myWindow, new object[] { Browser.Title });
                    }
                }
            }));

        }


        private void KLineView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= KLineView_Loaded;

            
            this.DataContext = klineViewModel;

            klineViewModel.Init();

            if (!string.IsNullOrEmpty(this.ChromeWinModel.Url))
            {
                Browser.Address = this.ChromeWinModel.Url;

                lbCode.Visibility = Visibility.Visible;
                seBond.Visibility = Visibility.Hidden;
                tbBondId.Text = ChromeWinModel.SecurityId;
                //tbBondName.Text = obj.Short_Name;

                string isFavorite = HomePageDataHandler.IsFavorite(ChromeWinModel.SecurityId);
                
                klineViewModel.SetFavoriteSrouce(isFavorite);
                klineViewModel.LoadKLineData(ChromeWinModel.SecurityId, "1M", string.Empty);
            }
            
        }

        


        public void SetAddress(string url)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (Browser != null)
                {
                    Browser.Address = url;
                }
            }));
        }

        /// <summary>
        /// 设置父级窗口
        /// </summary>
        /// <param name="mywin"></param>
        public void SetParentWindow(object mywin)
        {
            this.myWindow = (Window)mywin;
        }

        private void ChromeUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ChromeUserControl_Unloaded;

            // Cef.GetGlobalCookieManager().DeleteCookiesAsync("", "");

            if (!Browser.IsDisposed)
            {
                Browser.Dispose();
                Browser = null;
            }

            GC.Collect(0);
        }



        #region c#端 调用 JS Demo
        /// <summary>
        /// c# 调用 JS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("document.getElementById('btnCallc').click();");
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
            string path = AppDomain.CurrentDomain.BaseDirectory + "plugin\\cache\\Temp\\"; //文件名称
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

        private void ShowDevTools_Click(object sender, RoutedEventArgs e)
        {
            ShowDevTool();
        }

        private void ShowDevTool()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (Browser != null)
                {
                    Browser.ShowDevTools();
                }
            }));
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            Browser.GetBrowser().Reload(true);
        }

        

        private void seBond_AfterSearch(STC.Common.Model.Base.SimpleBondsInfoPo obj)
        {
            lbCode.Visibility = Visibility.Visible;
            seBond.Visibility = Visibility.Hidden;
            tbBondId.Text = obj.Bond_ID;
            tbBondName.Text = obj.Short_Name;

            string isFavorite = HomePageDataHandler.IsFavorite(obj.Bond_ID);

            igFavorite.Visibility = (!string.IsNullOrEmpty(isFavorite) && isFavorite == "1")?Visibility.Visible:Visibility.Collapsed;

            this.klineViewModel.setSecurityID(obj.Bond_ID);
            
        }

        public void ResetDataBySecurityId(string securityId)
        {
            SimpleBondsInfoPo bondsInfoPo = new SimpleBondsInfoPo();
            bondsInfoPo.Bond_ID = securityId;
            bondsInfoPo.Short_Name = string.Empty;

            this.seBond_AfterSearch(bondsInfoPo);
        }


        private void lbCode_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void lbCode_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void lbCode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lbCode.Visibility = Visibility.Hidden;
            seBond.Visibility = Visibility.Visible;

            //if (this.Parent != null && this.Parent is TabItem)
            //{
            //    seBond.SearchText = ((TabItem)this.Parent).Header.ToString();
            //}


            seBond.SelectAll();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Edit_Btn_Click(object sender, RoutedEventArgs e)
        {
            lbCode.Visibility = Visibility.Hidden;
            seBond.Visibility = Visibility.Visible;
            seBond.SelectAll();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            klineViewModel.OnSizeChange();
        }
    }
}
