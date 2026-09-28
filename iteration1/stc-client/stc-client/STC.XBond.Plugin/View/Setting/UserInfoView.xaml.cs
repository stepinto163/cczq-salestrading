using STC.Common.Command;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.XBond.Plugin.ViewModel.Setting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
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

namespace STC.XBond.Plugin.View.Setting
{
    /// <summary>
    /// UserInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfoView : UserControl, ISetting
    {
        private UserInfoViewModel _userInfoViewModel = null;
        ContextMenu contextMenu;
        public UserInfoView()
        {
            InitializeComponent();

            this.Loaded += UserInfoView_Loaded;

            _userInfoViewModel = new UserInfoViewModel();
            this.DataContext = _userInfoViewModel;
        }

        public void OnSave()
        {
        }

        public string GetName()
        {
            return "STC账号";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        private void UserInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserInfoView_Loaded;
            if (_userInfoViewModel != null)
                _userInfoViewModel.Init();
        }

        private UpdateUserPassUI lastUI = null;
        private UpdateUserPwdView _updateUserPwdView = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LocalEnv.ConfInfo.ChangePassMode == "0")
            {
                DeleteBoxView deleteBoxView = new DeleteBoxView();
                deleteBoxView.Title = "提示";
                deleteBoxView.setWarnMsg("该操作请联系管理员");
                deleteBoxView.Show();
            }
            else
            {
                if (_userInfoViewModel.IsSafeModel)
                {

                    if (_updateUserPwdView == null)
                    {
                        _updateUserPwdView = new UpdateUserPwdView();
                        _updateUserPwdView.Closed += _updateUserPwdView_Closed;

                        _updateUserPwdView.SetDefaultUpdateDay(_userInfoViewModel.UpdatePwdDay, _userInfoViewModel.IspowerPwd, false);

                        _updateUserPwdView.Show();
                    }
                    else
                        _updateUserPwdView.Activate();
                }
                else
                {
                    if (lastUI == null)
                    {
                        lastUI = new UpdateUserPassUI();
                        lastUI.Closed += lastUIClosed;

                        lastUI.Show();
                    }
                    else
                        lastUI.Activate();
                }
            }
            
            
        }

        private void _updateUserPwdView_Closed(object sender, EventArgs e)
        {
            _updateUserPwdView.Closed -= _updateUserPwdView_Closed;

            _updateUserPwdView = null;
        }

        private void lastUIClosed(object sender, EventArgs e)
        {
            lastUI.Closed -= lastUIClosed;
            lastUI = null;
        }

        #region Demo菜单
        private void SetContextMenu()
        {
            //bool result = GetIdcTraderPermission();
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("OrderContextMenu");
            }
        }

        private void dgUser_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string isShowPlotDemoMenu = ConfigurationManager.AppSettings["IsShowPlotDemoMenu"];
            isShowPlotDemoMenu = string.IsNullOrEmpty(isShowPlotDemoMenu) ? "0" : isShowPlotDemoMenu;
            //if (LocalEnv.UserInfo.BdbBboPermissions == "1" || isShowPlotDemoMenu == "1")
            {
                bool isHaveChart = false;
                FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
                foreach (FileInfo info2 in files)
                {
                    if (info2.Name.Equals("STC.Chart.Plugin.dll") || info2.Name.Equals("STC.Chrome.Plugin.dll"))
                    {
                        isHaveChart = true;
                    }
                }

                if (isHaveChart)
                {
                    SetContextMenu();
                    if (contextMenu != null)
                    {
                        contextMenu.Tag = "Demo";
                        contextMenu.IsOpen = true;
                    }
                }
            }
        }

        public object LinePageInstance = null;
        public object CandleLineInstance = null;
        public Type ChartType = null;
        public Type ChartModel = null;
        public Assembly ChartAsm = null;
        public Assembly ChromeAsm = null;
        public MethodInfo ChromeUpdAddress = null;
        public MethodInfo ChartUpdateChart = null;
        private void miSample1_Click(object sender, RoutedEventArgs e)
        {
            OpenDemo("TenorDemo");
        }

        private void miSample2_Click(object sender, RoutedEventArgs e)
        {
            OpenDemo("CandleLine");
        }

        /// <summary>
        /// 直线散点图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miLine_Click(object sender, RoutedEventArgs e)
        {
            OpenDemo("ScatterSeriesDemo");
        }

        private void miChrome_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://klinecharts.com/zh-CN";
            //string url = "https://www.google.com.hk";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() {Url  = url, IsTopMost = false, IsShowAddressBar = true };
            cw.Key = "klinecharts";
            cw.Title = "独立窗口演示";
            cw.ForceTargetSelf = true;
            //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
            //cw.Proxy = "socks5://127.0.0.1:1080";
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
            // OpenChrome(url);
        }

        

        /// <summary>
        /// 打开非本地网站演示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miAddTradeBook_Click(object sender, RoutedEventArgs e)
        {
            //string url = $"http://172.16.138.77:19990/#/login?username={LocalEnv.UserInfo.UserName}&sid={LocalEnv.UserInfo.SessionID}";
            string url = $"http://172.16.138.77:19990/#/login";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url,  IsShowAddressBar = false , IsShowStatusBar = false };
            cw.Key = "InsertTabDemo";
            cw.Title = "非本地";
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
            //AddTagPage("http://www.taobao.com/");
        }

        /// <summary>
        /// c# js回调测试,打开本地网页,并注册回调类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miJsCallBackTest_Click(object sender, RoutedEventArgs e)
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/index.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() {IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
            cw.Key = "JsC#Demo";
            cw.Title = "互相调用演示";
            cw.UseChromePluginObject = true;
            //cw.CallBackObjectInstance.Add("cefJS", "CefJsEventDemo");
            cw.CallBackObjectInstance.Add("cef|", "CefSharpJsHandler");
            //下面采用异步方式，前端Js也是异步调用的，因而在key后面加上| 告诉插件要异步注册到chrome中
            //cw.CallBackObjectInstance.Add("csharpTest|", "JavascriptCallCharpTest");
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }


        private void miTopo_Click(object sender, RoutedEventArgs e)
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin\\HTML\\Topo\\monitorView.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { Url = url, IsTopMost = false, IsShowAddressBar = false, IsOpenInMainTabPage = true  };
            cw.Key = "TopoMonitor";
            cw.Title = "服务监控";
            cw.ForceTargetSelf = true;
            cw.UseChromePluginObject = true;
            cw.CallBackObjectInstance.Add("cefJS|", "TopoViewModel");
            //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
            //cw.Proxy = "socks5://127.0.0.1:1080";
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
            // OpenChrome(url);
        }

        private void miTopoEditor_Click(object sender, RoutedEventArgs e)
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin\\HTML\\Topo\\blockEditor.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { Url = url, IsTopMost = false, IsShowAddressBar = true };
            cw.Key = "blockEditor";
            cw.Title = "监控图编辑器";
            cw.ForceTargetSelf = true;
            cw.UseChromePluginObject = true;
            cw.CallBackObjectInstance.Add("cefJS|", "TopoViewModel");
            //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
            //cw.Proxy = "socks5://127.0.0.1:1080";
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }
        /// <summary>
        /// 获取独立窗口对象并导航到百度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void miGetObject_Click(object sender, RoutedEventArgs e)
        {
            string key = "klinecharts";
            //先判断是否有该插件
            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.ChromePlugin);
            if (plugin != null)
            {
               
                object chromectl = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.ChromePlugin}_ChromeUserControl_{key}");
                if (chromectl != null)
                {
                    string url = "https://www.baidu.com";
                    MethodInfo ChromeUpdAddress = chromectl.GetType().GetMethod("SetAddress");
                    ChromeUpdAddress?.Invoke(chromectl, new object[] { url });

                    await Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(6000);
                        MethodInfo ShowDevTools = chromectl.GetType().GetMethod("ShowDevTools");
                        ShowDevTools?.Invoke(chromectl, new object[] { });
                    });
                }
                else
                {
                    //MessageBox.Show("请先打开[Chrome独立窗口演示]", "提示", MessageBoxButton.OK, MessageBoxImage.Hand);
                }

                //demo 发送清零信号
                Common.Handler.Base.BaseHandler.Instance.Reset();

            }
        }

        #region Oxyplot  Demo
        public void OpenDemo(string name)
        {
            NoBorderWin myTradingPopupWin = new NoBorderWin();
            myTradingPopupWin.MaxHeight = 1080;
            myTradingPopupWin.MaxWidth = 1920;
            myTradingPopupWin.Closed += MyTradingPopupWin_Closed;
            
            FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(System.Environment.CurrentDirectory, "plugin")).GetFiles("*.dll");
            foreach (FileInfo info2 in files)
            {
                if (info2.Name.Equals("STC.Chart.Plugin.dll"))
                {
                    ChartAsm = Assembly.LoadFile(info2.FullName);
                }
                if (ChartAsm != null)
                {
                    try
                    {
                        if (name == "TenorDemo")
                        {
                            CandleLineInstance = ChartAsm.CreateInstance("STC.Chart.Plugin.OxyChart.TenorDemo");
                            ChartType = ChartAsm.GetType("STC.Chart.Plugin.OxyChart.TenorDemo");
                            myTradingPopupWin.Title = "TenorDemo";
                        }
                        else if (name == "CandleLine")
                        {
                            CandleLineInstance = ChartAsm.CreateInstance("STC.Chart.Plugin.View.Chart.CandleLine");
                            ChartType = ChartAsm.GetType("STC.Chart.Plugin.View.Chart.CandleLine");
                            myTradingPopupWin.Title = "CandleLine";
                        }
                        else if (name == "ScatterSeriesDemo")
                        {
                            CandleLineInstance = ChartAsm.CreateInstance("STC.Chart.Plugin.Demo.ScatterSeriesDemo");
                            ChartType = ChartAsm.GetType("STC.Chart.Plugin.Demo.ScatterSeriesDemo");
                            myTradingPopupWin.Title = "ScatterSeriesDemo";
                        }
                        else
                        {
                            break;
                        }
                        
                        ChartUpdateChart = ChartType.GetMethod("SetMyWindow");
                        myTradingPopupWin.SetMainControl(CandleLineInstance);

                        ChartUpdateChart?.Invoke(CandleLineInstance, new object[] { myTradingPopupWin });
                        myTradingPopupWin.Width = 800;
                        myTradingPopupWin.Height = 600;
                        WindowManager.Instance.OpenOnlyOneWindow(myTradingPopupWin.Title, myTradingPopupWin, null);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(ex.Message);
                    }

                    break;
                }
            }
        }
        #endregion

        #region 暂时不用，当需要完整控制Chrome时参考
        #region Chrome 打开网页测试
        public void OpenChrome(string url)
        {
            NoBorderWin myTradingPopupWin = new NoBorderWin();
            myTradingPopupWin.MaxHeight = 1080;
            myTradingPopupWin.MaxWidth = 1920;
            myTradingPopupWin.Topmost = false;
            myTradingPopupWin.SecurityID = "ChromeDemo";
            myTradingPopupWin.Closed += MyTradingPopupWin_Closed;

            try
            {
                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.ChromePlugin);
                if (plugin != null)
                {
                    CandleLineInstance = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.ChromePlugin}_ChromeDemo");
                    ChartUpdateChart = CandleLineInstance.GetType().GetMethod("SetTopWindow");
                    myTradingPopupWin.SetMainControl(CandleLineInstance);
                    ChartUpdateChart?.Invoke(CandleLineInstance, new object[] { myTradingPopupWin });

                    ChromeUpdAddress = CandleLineInstance.GetType().GetMethod("SetAddress");
                    ChromeUpdAddress?.Invoke(CandleLineInstance, new object[] { url }); ;


                    myTradingPopupWin.Width = 900;
                    myTradingPopupWin.Height = 460;
                    WindowManager.Instance.OpenOnlyOneWindow($"ChromeDemo", myTradingPopupWin, null);
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }
        }

        #endregion

        #region 打开Tabpage添加到首页
        public void AddTagPage(string url)
        {
            string tagHeader = "ChromeDemo";

            if (TabPageHelper.IsExistTabItem(tagHeader))
            {
                return;
            }
    
            try
            {
                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.ChromePlugin);
                if (plugin != null)
                {
                    CandleLineInstance = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.ChromePlugin}_ChromeUserControl");
                    int PluginKey = (int)PluginKeyEnum.ChromePlugin;

                    TabPageHelper.OpenTabPage(tagHeader, CandleLineInstance, PluginKey);

                    ChromeUpdAddress = CandleLineInstance.GetType().GetMethod("SetAddress");
                    ChromeUpdAddress?.Invoke(CandleLineInstance, new object[] { url }); ;
                }

            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }

        }

        #endregion
        #endregion

        /// <summary>
        /// '我的交易'弹出窗口关闭事件
        /// </summary>
        /// <param name="sender">'我的交易'弹出窗口实例</param>
        /// <param name="e">表示包含事件数据的类的基类，并提供要用于不包含事件数据的事件的值。</param>
        private void MyTradingPopupWin_Closed(object sender, EventArgs e)
        {
            PopupWindow popupWindow = sender as PopupWindow;
        }





        #endregion


        #region KLine
        private void MiKLineJsCall_Click(object sender, RoutedEventArgs e)
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            //string url = $"/STC.Chrome.Plugin;component/Html/KLine/KLineView.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = true, Url = url, IsShowAddressBar = true, IsShowStatusBar = false };
            cw.Key = "JsC#KLineDemo";
            cw.Title = "K线分析";
            cw.UseChromePluginObject = true;
            cw.SecurityId = "220210.IB";
            cw.CallBackObjectInstance.Add("cefJS", "KLineViewModel");
            //下面采用异步方式，前端Js也是异步调用的，因而在key后面加上| 告诉插件要异步注册到chrome中
            //cw.CallBackObjectInstance.Add("csharpTest|", "JavascriptCallCharpTest");
            AppUtils.Application.PluginManager.Application.BroadCast("OpenKLine", cw);
        }
        #endregion


    }
}
