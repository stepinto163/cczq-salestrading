using BDB.Common.Common;
using BDB.Common.Handler.Admin;
using BDB.Common.Plugin;
using BDB.Common.Utils;
using BDB.Frame.Model;
using BDB.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

namespace BDB.Frame.CustomUI
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

            //tbVersion.Text = string.Format("V{0}", LocalEnv.ServerInfo.Version);
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

        private void InitMenu()
        {
            List<MenuPo> menus = new List<MenuPo>();

            MenuPo menuBond = new MenuPo();
            //menuBond.ShowImageSource = "pack://application:,,,/BDB.Frame;component/Images/stc_image_menulist_jiaoyibao.png";
            menuBond.Txt = "债券行情";
            menuBond.PluginKey = (int)PluginKeyEnum.TradingPlugin;
            
            //menus.Add(menuBond);

            
            MenuPo menuHistory = new MenuPo();
            //menuHistory.ShowImageSource = "pack://application:,,,/BDB.Frame;component/Images/stc_image_menulist_lishichaxun.png";
            menuHistory.Txt = "历史需求";
            menuHistory.PluginKey = (int)PluginKeyEnum.HistoryPlugin; ;
            
            MenuPo menuVenues = new MenuPo();
            //menuVenues.ShowImageSource = "pack://application:,,,/BDB.Frame;component/Images/stc_image_menulist_qudaozhixing.png";
            menuVenues.Txt = "历史达成";
            menuVenues.PluginKey = (int)PluginKeyEnum.HistoryDealPlugin; ;

            MenuPo menuRisk = new MenuPo();
            //menuRisk.ShowImageSource = "pack://application:,,,/BDB.Frame;component/Images/stc_image_menulist_fengkongshezhi.png";
            menuRisk.Txt = "历史消息";
            menuRisk.PluginKey = (int)PluginKeyEnum.HistoryMsgPlugin; ;

            MenuPo menuBroker = new MenuPo();
            //menuRisk.ShowImageSource = "pack://application:,,,/BDB.Frame;component/Images/stc_image_menulist_fengkongshezhi.png";
            menuBroker.Txt = "经纪商看板";
            menuBroker.PluginKey = (int)PluginKeyEnum.BrokerKanbanPlugin; ;
            bool qtrsresult = !string.IsNullOrEmpty(LocalEnv.UserInfo.IdcQTRSPermissions) && LocalEnv.UserInfo.IdcQTRSPermissions == "1";

            //bool result = !string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1";

            //if (result)
            //{
            //    menus.Add(menuHistory);

            //    menus.Add(menuVenues);

            //    menus.Add(menuRisk);
            //}

            //if (qtrsresult)
            //{
            //    menus.Add(menuBroker);
            //}


            MenuPo menuDownload = new MenuPo();
            menuDownload.ShowImageSource = "pack://application:,,,/BDB.Common;component/Images/stc_image_menulist_xiazaizhongxin.png";
            menuDownload.Txt = "下载中心";
            menuDownload.PluginKey = (int)PluginKeyEnum.DownloadCenterPlugin; ;


            //menus.Add(menuDownload);

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
                    if (plugin.PluginKey == 22)
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
            }

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

        private Color _blue = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00D198");
        private Color _white = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBC8");

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            //if (tb != null)
            //    tb.Foreground = new SolidColorBrush(_blue);
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            //if (tb != null)
            //    tb.Foreground = new SolidColorBrush(_white);
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rt = sender as Rectangle;
            //rt.Fill = new SolidColorBrush(_blue);
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rt = sender as Rectangle;
            //rt.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            Grid grid = border.Child as Grid;
            if (grid.Children != null && grid.Children.Count > 1)
            {
                Rectangle rt = grid.Children[0] as Rectangle;
                TextBlock tb = grid.Children[1] as TextBlock;
                if (rt != null)
                    rt.Fill = new SolidColorBrush(_blue);
                if (tb != null)
                    tb.Foreground = new SolidColorBrush(_blue);
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            Grid grid = border.Child as Grid;
            if (grid.Children != null && grid.Children.Count > 1)
            {
                Rectangle rt = grid.Children[0] as Rectangle;
                TextBlock tb = grid.Children[1] as TextBlock;
                if (rt != null)
                    rt.Fill = new SolidColorBrush(Colors.Transparent);
                if (tb != null)
                    tb.Foreground = new SolidColorBrush(_white);
            }
        }
    }
}
