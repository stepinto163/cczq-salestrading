using STC.Broker.Plugin.Handler;
using STC.Broker.Plugin.Model.Card;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Frame.Utils;
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

namespace STC.Broker.Plugin.View.HomePage
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
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }
        private void HomePageView_Initialized(object sender, EventArgs e)
        {
            ThreadPoolHelper.InvokeThreadPool((P) =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                    {
                        //从服务器中获取
                        List<QuotePageModel> list = CardInfoHandler.Instance.QueryGrUserFavoritesSettings();
                        if (list != null)
                        {
                            if (list.Count == 0)
                            {
                                //如果服务器没有配置，则从本地获取    
                                list = CardInfoHandler.Instance.GetAllConfigByCondtion();
                            }
                        }
                        else
                        {
                            list = new List<QuotePageModel>();
                        }

                        if (list.Count > 0)
                        {
                            foreach (QuotePageModel qpm in list)
                            {
                                this.GroupTabControl.Sub_AddGroupTabItem(qpm, false, this);
                            }

                            this.GroupTabControl.SelectedIndex = 0;
                        }
                        else
                        {
                            var sortDic = HomePageDataHandler.RequestGroups(false);
                            if (sortDic != null && sortDic.Count() > 0)
                            {
                                foreach (KeyValuePair<string, HomeGroupInfo> gi in sortDic)
                                {
                                    if (gi.Value.action != 3)
                                    {
                                        this.GroupTabControl.AddGroupTabItem(gi.Value, false, this);
                                    }
                                }

                                this.GroupTabControl.SelectedIndex = 0;
                            }
                        }

                        
                        IsInited = true;
                    }));

                }
                catch (Exception ex)
                {
                    com.gateway.log.LogHelper.Error(ex);
                }
            });
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "TabControlNeedSelectionChanged" && !string.IsNullOrEmpty(obj.Info1))
            {
                if (obj.Info1.ToString() == "PricingPanel")
                {
                    //改变组
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                        {
                            if (GroupTabControl.Items.Count > 1)
                            {
                                if (GroupTabControl.SelectedIndex < GroupTabControl.Items.Count - 1)
                                {
                                    GroupTabControl.SelectedIndex = GroupTabControl.SelectedIndex + 1;
                                }
                                else if (GroupTabControl.SelectedIndex == GroupTabControl.Items.Count - 1)
                                {
                                    GroupTabControl.SelectedIndex = 0;
                                }
                            }
                        }));
                    }
                    catch
                    { }
                }
            }

        }
        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.GroupTabControl.SelectedItem != null
            //     && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            //{
            //    HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

            //    ////MMGroupInfo gi1 = selectedTagItem.Tag as MMGroupInfo;
            //    ////LogHandler.Info($"--------click tab1 {gi1.groupname}:{gi1.id}:{selectedTagItem.IsVisible}");
            //    ////if (selectedTagItem.Content == null)
            //    ////{
            //    ////    if (selectedTagItem.Tag != null && selectedTagItem.Tag is MMGroupInfo)
            //    ////    {
            //    ////        LogHandler.Info($"--------{DateTime.Now}--:GroupTabControl_SelectionChanged");
            //    ////        MMGroupInfo gi = selectedTagItem.Tag as MMGroupInfo;
            //    ////        LogHandler.Info($"--------click tab {gi.groupname}");
            //    ////        MMListBoxView mmlist = new MMListBoxView(gi.id);
            //    ////        selectedTagItem.Content = mmlist;
            //    ////    }
            //    ////}
            //}
        }

        private void GroupTabControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                e.Handled = true; // 阻止左右切换
            }
        }
    }
}
