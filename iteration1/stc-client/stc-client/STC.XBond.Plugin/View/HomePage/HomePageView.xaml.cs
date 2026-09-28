using STC.Common.Data;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.HomePage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.XBond.Plugin.View.HomePage
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

            DesignHandler.Instance.OnKeyDown += Instance_OnKeyDown;

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "TriggerHomePageListCommand")
            {
                SectionSecurityIDChange(obj.Info1 ?? null);
            }
        }

        private async Task SectionSecurityIDChange(object obj)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    string securityObj = (obj ?? null) as string;
                    dgPrice.InitTableModel(securityObj, "");
                }));
            });
        }


        private void Instance_OnKeyDown(KeyEventArgs keyEventArgs)
        {
            object obj = AppUtils.Application.Panel.SelectedItem;
            if (obj != null)
            {
                TabItem tabItem = obj as TabItem;

                string tag = ((TabItem)tabItem).Tag.ToString() ?? "80";
                //如果是网页关闭快捷搜索
                if (tag == ((int)Frame.Common.PluginKeyEnum.ChromePlugin).ToString())
                {
                    return;
                }

                if (tabItem != null && tabItem.Tag.ToString() == "80")
                {
                    HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;
                    FavoriteBondsListBox favoriteBondsLB = selectedTagItem.Content as FavoriteBondsListBox;
                    if (favoriteBondsLB != null)
                    {
                        favoriteBondsLB.OnKeyDown();
                    }
                }
            }

        }

        private void HomePageView_Initialized(object sender, EventArgs e)
        {
            var sortDic = HomePageDataHandler.RequestGroups(false);

            if (sortDic != null && sortDic.Count() > 0)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> gi in sortDic)
                {
                    if (gi.Value.action != 3)
                    {
                        this.GroupTabControl.AddGroupTabItem(gi.Value, false);
                    }
                }
                this.GroupTabControl.SelectedIndex = 0;
            }

            IsInited = true;
        }

        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.GroupTabControl.SelectedItem != null && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            {
                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

                HomeGroupInfo gi1 = selectedTagItem.Tag as HomeGroupInfo;
                if (selectedTagItem.Content == null)
                {
                    if (selectedTagItem.Tag != null && selectedTagItem.Tag is HomeGroupInfo)
                    {
                        HomeGroupInfo gi = selectedTagItem.Tag as HomeGroupInfo;
                        FavoriteBondsListBox favoriteBondsLB = new FavoriteBondsListBox();
                        //favoriteBondsLB.OnSectionChange += FavoriteBondsLB_OnSectionChange;
                        selectedTagItem.Content = favoriteBondsLB;
                        favoriteBondsLB.Init(gi);
                    }
                }
            }
        }

        //private async Task GetIsPwdFavorite(string securityObj)
        //{
        //    await Task.Run(() =>
        //    {
        //        dgPrice.Init(securityObj);
        //    });
        //}
    }
}
