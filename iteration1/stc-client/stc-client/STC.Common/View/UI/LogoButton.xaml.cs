using STC.Common.Handler.Admin;
using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class LogoButton : UserControl
    {
        public LogoButton()
        {
            InitializeComponent();

            InitMenu();

            //SetAlignment();

            tbVersion.Text = string.Format("{0}", LocalEnv.ServerInfo.Version);
            tbEnv.Text = LocalEnv.ConfInfo.IsTest ? $"[{LocalEnv.ConfInfo.LoginEnv}]" : "[生产]";

            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.EnvironmentFlag))
            {
                if (LocalEnv.ConfInfo.EnvironmentFlag.Equals("3"))
                    tbEnv.Text = "[生产Pro]";
                else if (LocalEnv.ConfInfo.EnvironmentFlag.Equals("4"))
                    tbEnv.Text = "[生产Se]";
            }

            LogoMenuIcon lmi = new LogoMenuIcon();

            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "dmart":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/dmart-logo.png";
                    lmi.Stretch = "None";
                    lmi.Width = "200";
                    lmi.Opacity = "1";
                    lmi.Margin = "10,0,0,0";
                    break;
                case "gtja":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/stc_gtja_mainlogo.png";
                    lmi.Stretch = "Uniform";
                    lmi.Width = "164";
                    lmi.Opacity = "0.7";
                    lmi.Margin = "4,0,10,0";
                    tbVersion.Foreground = tbEnv.Foreground = new SolidColorBrush(Colors.LightGray);
                    break;
                case "idbpro":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/idc_logo_mainlogo.png";
                    lmi.Stretch = "Uniform";
                    lmi.Width = "200";
                    lmi.Opacity = "1";
                    lmi.Margin = "6,-6,0,0";
                    break;
                case "bdb":
                case "idcclient":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/bdb_logo_mainlogo.png";
                    lmi.Stretch = "Uniform";
                    lmi.Width = "190";
                    lmi.Opacity = "1";
                    lmi.Margin = "2,-6,0,0";
                    break;
                default:
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/stclite-logo-2x2.png";
                    lmi.Stretch = "Uniform";
                    lmi.Width = "200";
                    lmi.Opacity = "1";
                    lmi.Margin = "0,0,0,0";
                    break;
            }

            btnMenu.DataContext = lmi;
        }

        public void SetAlignment()
        {
            //获取系统是以Left-handed（true）还是Right-handed（false）
            var ifLeft = SystemParameters.MenuDropAlignment;

            if (ifLeft)
            {
                // change to false
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, false);

                ifLeft = SystemParameters.MenuDropAlignment;
            }
        }

        public void AddMenu(string iconSource, string name, int pluginKey, string showSort)
        {
            List<MenuPo> menus = dgMenu.DataContext as List<MenuPo>;

            List<MenuPo> selectMenus = menus.Where(o => { return o.PluginKey == pluginKey; }).ToList();
            if (selectMenus != null && selectMenus.Count > 0)
                return;

            MenuPo menuPo = new MenuPo();
            menuPo.ShowImageSource = iconSource;
            menuPo.PluginKey = pluginKey;
            menuPo.Txt = name;
            menuPo.ShowSort = showSort;

            
           
            menus.Add(menuPo);

            menus.Sort(new MenuSort());

            dgMenu.DataContext = menus;
        }

        private void InitMenu()
        {
            List<MenuPo> menus = new List<MenuPo>();
            //MenuPo menuMain = new MenuPo();
            //menuMain.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_zhuye.png";
            //menuMain.Txt = "主页";
            //menuMain.PluginKey = 80;
           
            //MenuPo menuTradebook = new MenuPo();
            //menuTradebook.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_tradebook.png";
            //menuTradebook.Txt = "Tradebook";
            //menuTradebook.PluginKey = 22;
            //MenuPo menuHistory = new MenuPo();
            //menuHistory.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_lishichaxun.png";
            //menuHistory.Txt = "历史查询";
            //menuHistory.PluginKey = 50;
            
            //MenuPo menuVenues = new MenuPo();
            //menuVenues.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_qudaozhixing.png";
            //menuVenues.Txt = "渠道执行";
            //menuVenues.PluginKey = 40;

            //MenuPo menuRisk = new MenuPo();
            //menuRisk.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_fengkongshezhi.png";
            //menuRisk.Txt = "风控指标";
            //menuRisk.PluginKey = 90;

            //MenuPo menuBookManagement = new MenuPo();
            //menuBookManagement.ShowImageSource = "pack://application:,,,/STC.Common;component/Images/stc_image_menulist_jiaoyibao.png";
            //menuBookManagement.Txt = "交易包管理";
            //menuBookManagement.PluginKey = 100;

            //menus.Add(menuMain);
            //menus.Add(menuTradebook);
            //menus.Add(menuHistory);

            //STC.Common.Model.Admin.Module module = AdminHandler.Instance.GetModuleById("2001");
            //if (module != null)
            //    menus.Add(menuVenues);
            
            //menus.Add(menuRisk);


            //STC.Common.Model.Admin.Module menuBookManagerModel = AdminHandler.Instance.GetModuleById("3010");
            //if (menuBookManagerModel != null)
            //    menus.Add(menuBookManagement);

            dgMenu.DataContext = menus;
            
        }

        private void dgMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.popMenu.IsOpen = false;

            MenuPo menuPo = dgMenu.CurrentItem as MenuPo;
            TabItem selectItem = AppUtils.Application.Panel.SelectedItem as TabItem;
            bool isExit = false;
            if (menuPo != null && selectItem != null)
            {

                if (!selectItem.Header.Equals(menuPo.Txt))
                {
                    foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                    {
                        if (tabItem.Header.ToString().Equals(menuPo.Txt))
                        {
                            AppUtils.Application.Panel.SelectedItem = tabItem;
                            isExit = true;
                            break;
                        }
                    }
                }
            }

            if (!isExit)
            {
                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(menuPo.PluginKey);
                if (plugin != null)
                {
                    AppUtils.Application.PluginManager.EnablePlugin(menuPo.PluginKey);
                    // pluginKey是22，表示TradeBook插件， 可以多次打开
                    if (!plugin.IsAlone)
                    {
                        plugin.Enabled = false;
                    }

                    if (AppUtils.Application.Panel.Items.Count > 0)
                    {
                        foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                        {
                            if (tabItem.Header.ToString().Equals(menuPo.Txt))
                            {
                                AppUtils.Application.Panel.SelectedItem = tabItem;
                                isExit = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //没有插件就是web网页
                    List<STC.Common.Model.Admin.Module> webModel = AdminHandler.Instance.GetWebModule();
                    if (webModel != null)
                    {
                        STC.Common.Model.Admin.Module m = webModel.FirstOrDefault(o => o.id == menuPo.PluginKey.ToString());
                        if (m != null)
                        {
                            OpenWebUrl(m);
                        }
                    }
                }
            }

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

            if (!string.IsNullOrEmpty(urlmodel.inf5))
            {
                //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
                cw.Proxy = urlmodel.inf5;
            }

            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e) 
        {
            this.popMenu.IsOpen = true;
        }

        private void popMenu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (!ifLeft)
                popMenu.HorizontalOffset = 0;
        }
    }

    public class LogoMenuIcon
    { 
        public string LogoImage { get; set; }

        public string Stretch { get; set; }

        public string Margin { get; set; }

        public string Width { get; set; }

        public string Opacity { get; set; }

        public string BackImage { get; set; }
    }
}
