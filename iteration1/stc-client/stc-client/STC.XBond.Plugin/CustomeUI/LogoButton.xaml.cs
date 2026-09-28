using STC.Common.Model;
using System;
using System.Collections.Generic;
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

namespace STC.XBond.Plugin.CustomUI
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class LogoButton : UserControl
    {
        private IForm Application;

        public LogoButton()
        {
            InitializeComponent();

            InitMenu();

            //SetAlignment();
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

        public void SetApplication(IForm app)
        {
            this.Application = app;
        }

        private void InitMenu()
        {
            List<MenuPo> menus = new List<MenuPo>();
            MenuPo menuMain = new MenuPo();
            menuMain.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_zhuye.png";
            menuMain.Txt = "主页";
            menuMain.PluginKey = 80;
            MenuPo menuDouble = new MenuPo();
            menuDouble.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_shuangbianzuoshi.png";
            menuDouble.Txt = "双边做市";
            menuDouble.PluginKey = 30;
            MenuPo menuXbond = new MenuPo();
            menuXbond.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_xbondzuoshi.png";
            menuXbond.Txt = "X-Bond做市";
            menuXbond.PluginKey = 20;
            MenuPo menuTradebook = new MenuPo();
            menuTradebook.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_tradebook.png";
            menuTradebook.Txt = "Tradebook";
            MenuPo menuHistory = new MenuPo();
            menuHistory.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_lishichaxun.png";
            menuHistory.Txt = "历史查询";
            MenuPo menuSetting = new MenuPo();
            menuSetting.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_fengkongshezhi.png";
            menuSetting.Txt = "风控设置";
            MenuPo menuVenues = new MenuPo();
            menuVenues.ShowImageSource = "pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_qudaozhixing.png";
            menuVenues.Txt = "渠道执行";
            menuVenues.PluginKey = 40;

            menus.Add(menuMain);
            menus.Add(menuDouble);
            menus.Add(menuXbond);
            menus.Add(menuTradebook);
            menus.Add(menuHistory);
            menus.Add(menuSetting);
            menus.Add(menuVenues);


            dgMenu.DataContext = menus;
            
        }

        private void dgMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.popMenu.IsOpen = false;

            MenuPo menuPo = dgMenu.CurrentItem as MenuPo;
            TabItem selectItem = Application.Panel.SelectedItem as TabItem;
            bool isExit = false;
            if (menuPo != null && selectItem != null)
            {

                if (!selectItem.Header.Equals(menuPo.Txt))
                {
                    foreach (TabItem tabItem in Application.Panel.Items)
                    {
                        if (tabItem.Header.ToString().Equals(menuPo.Txt))
                        {
                            Application.Panel.SelectedItem = tabItem;
                            isExit = true;
                            break;
                        }
                    }
                }
            }

            if (!isExit)
            {
                IPlugin plugin = Application.PluginManager.GetPlugin(menuPo.PluginKey);
                if (plugin != null)
                {
                    Application.PluginManager.EnablePlugin(menuPo.PluginKey);

                    if (Application.Panel.Items.Count > 0)
                    {
                        foreach (TabItem tabItem in Application.Panel.Items)
                        {
                            if (tabItem.Header.ToString().Equals(menuPo.Txt))
                            {
                                Application.Panel.SelectedItem = tabItem;
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
    }
}
