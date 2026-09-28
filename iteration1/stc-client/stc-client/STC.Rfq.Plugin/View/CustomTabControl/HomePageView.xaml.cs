using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Frame.Utils;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.View.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace STC.Mm.Plugin.View.HomePage
{
    /// <summary>
    /// HomePageView.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageView : UserControl
    {
        public static volatile bool IsInited = false;

        public HomePageView()
        {
            InitializeComponent();
        }
        private void HomePageView_Initialized(object sender, EventArgs e)
        {
            try
            {
                IOrderedEnumerable<MMGroupInfo> sortDic = MMGroupHandler.RequestGroups(false);

                if (sortDic != null && sortDic.Count() > 0)
                {
                    foreach (MMGroupInfo gi in sortDic)
                    {
                        if (gi.action != 3)
                        {
                            this.GroupTabControl.AddGroupTabItem(gi, false);
                        }
                    }
                    this.GroupTabControl.SelectedIndex = 0;
                }

                IsInited = true;
            }
            catch(Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
            
        }

        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.GroupTabControl.SelectedItem != null
                 && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            {
                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

                MMGroupInfo gi1 = selectedTagItem.Tag as MMGroupInfo;
                LogHandler.Info($"--------click tab1 {gi1.groupname}:{gi1.id}:{selectedTagItem.IsVisible}");
                if (selectedTagItem.Content == null)
                {
                    if (selectedTagItem.Tag != null && selectedTagItem.Tag is MMGroupInfo)
                    {
                        LogHandler.Info($"--------{DateTime.Now}--:GroupTabControl_SelectionChanged");
                        MMGroupInfo gi = selectedTagItem.Tag as MMGroupInfo;
                        LogHandler.Info($"--------click tab {gi.groupname}");
                        MMListBoxView mmlist = new MMListBoxView(gi.id);
                        selectedTagItem.Content = mmlist;
                    }
                }
            }
        }
    }
}
