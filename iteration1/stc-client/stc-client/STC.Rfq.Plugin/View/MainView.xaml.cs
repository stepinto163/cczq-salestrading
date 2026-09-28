using com.gateway.log;
using STC.Common.ColumnSetting;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Rfq.Plugin.Utils.Data;
using STC.Rfq.Plugin.View.HomePage;
using STC.Rfq.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace STC.Rfq.Plugin.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl, IDisposable
    {
        private MainViewModel _MainViewModel;

        public static volatile bool IsInited = false;

        private string _WinID = Guid.NewGuid().ToString();
        public string WinID
        {
            get
            {
                return _WinID;
            }
        }
        public MainView()
        {
            Utils.Data.RfqGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            InitializeComponent();
        }


        private void HomePageDataHandler_OnFavoritesChanged(RfqGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupid, string groupname)
        {
            if (favoriteTypeEnum == Utils.Data.RfqGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                RfqGroupInfo gi = RfqGroupHandler.Instance.GetGroupInfoByGroupID(groupid);
                if (_MainViewModel != null)
                {
                    _MainViewModel.SelectGroup = gi;
                }
                this.GroupTabControl.AddGroupTabItem(gi, !string.IsNullOrEmpty(groupname));
            }
            else if (favoriteTypeEnum == Utils.Data.RfqGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                this.GroupTabControl.DelGroupTabItem(groupname);
            }
            //else if (favoriteTypeEnum == Utils.Data.MMGroupHandler.FavoriteTypeEnum.ChangeColumns)
            //{
            //    foreach (HomePageTabItemWithClose item in this.GroupTabControl.Items)
            //    {
            //        if (item.Content != null && item.Content is MMListBoxView)
            //        {
            //            MMListBoxView mmListBoxView = item.Content as MMListBoxView;
            //            mmListBoxView.InitColumnsInfo();
            //        }
            //    }
            //}
            else if (favoriteTypeEnum == Utils.Data.RfqGroupHandler.FavoriteTypeEnum.UpdateGroup)
            {
                Task.Run(async () => {
                    await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        int index = 0;
                        foreach (RfqGroupInfo groupInfo in RfqGroupHandler.Instance.CacheSortedGroups)
                        {
                            TabItem findTabItem = FindTabItem(groupInfo.id);
                            if (findTabItem != null)
                            {
                                int oldIndex = this.GroupTabControl.Items.IndexOf(findTabItem);
                                if (index != oldIndex)
                                {
                                    this.GroupTabControl.Items.Remove(findTabItem);
                                    findTabItem.Tag = groupInfo;
                                    if (this.GroupTabControl.Items.Count > index)
                                    {
                                        this.GroupTabControl.Items.Insert(index, findTabItem);
                                    }
                                    else
                                    {
                                        this.GroupTabControl.Items.Add(findTabItem);
                                    }
                                }
                            }
                            index++;
                        }
                    });
                });
            }
        }
        private TabItem FindTabItem(string groupId)
        {
            foreach (TabItem tabItem in this.GroupTabControl.Items)
            {
                if (tabItem.Tag != null && tabItem.Tag is RfqGroupInfo)
                {
                    RfqGroupInfo mmgroupInfo = tabItem.Tag as RfqGroupInfo;

                    if (mmgroupInfo.id == groupId)
                    {
                        return tabItem;
                    }

                }
            }
            return null;
        }


        public void Load()
        {
           // (this.DataContext as MainViewModel).InitView();
        }

        public void Dispose()
        {
            LogHelper.Debug("MM dispose");

            ////if (_MainViewModel != null)
            ////{
            ////    _MainViewModel?.SetActiveMMListBox(_WinID, string.Empty);
            ////}

            Utils.Data.RfqGroupHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            if (Utils.Data.RfqGroupHandler.Instance.FavoritesChangedCount < 2)
            {
                if (_MainViewModel != null)
                {
                    this.DataContext = null;
                    _MainViewModel.Dispose();
                    _MainViewModel = null;
                }
            }
        }

        private async void MM_MainView_Initialized(object sender, EventArgs e)
        {
            try
            {
                IOrderedEnumerable<RfqGroupInfo> sortDic = await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();

                if (sortDic != null && sortDic.Count() > 0)
                {
                    foreach (RfqGroupInfo gi in sortDic)
                    {
                        if (gi.action != 3)
                        {
                            Utils.Data.RfqGroupHandler.Instance.Notify(Utils.Data.RfqGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, string.Empty);
                            //this.GroupTabControl.AddGroupTabItem(gi, false);
                        }
                    }
                    this.GroupTabControl.SelectedIndex = 0;
                }

                IsInited = true;
            }
            catch (Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
        }

        private void MM_MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is MainViewModel)
            {
                _MainViewModel = e.NewValue as MainViewModel;

                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;
                if (selectedTagItem != null && selectedTagItem.Content == null)
                {
                    RfqListBoxView mmlist = new RfqListBoxView();
                    RfqGroupInfo gi = selectedTagItem.Tag as RfqGroupInfo;
                    //_MainViewModel.SelectGroup = gi;
                    //_MainViewModel?.SetActiveMMListBox(_WinID, gi.id);
                    mmlist.DataContext = _MainViewModel?.GetMMListBoxViewModelByGroupID(gi.id);
                    selectedTagItem.Content = mmlist;
                }
            }
        }
       
        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_MainViewModel != null
               && this.GroupTabControl.SelectedItem != null
                && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            {
                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

                RfqGroupInfo gi = selectedTagItem.Tag as RfqGroupInfo;
                LogHandler.Info($"--------click tab1 {gi.groupname}:{gi.id}:{selectedTagItem.IsVisible}");
                _MainViewModel.SelectGroup = gi;
                if (selectedTagItem.Content == null)
                {
                    if (selectedTagItem.Tag != null && selectedTagItem.Tag is RfqGroupInfo)
                    {
                        LogHandler.Info($"--------{DateTime.Now}--:GroupTabControl_SelectionChanged");
                        LogHandler.Info($"--------click tab {gi.groupname}");

                        RfqListBoxView favoriteBondsLB = new RfqListBoxView();
                        favoriteBondsLB.DataContext = _MainViewModel?.GetMMListBoxViewModelByGroupID(gi.id);
                        selectedTagItem.Content = favoriteBondsLB;
                    }
                }

                _MainViewModel?.SetActiveMMListBox(_WinID, gi.id);

                ////string securityID = string.Empty;
                ////if (selectedTagItem.Content != null && selectedTagItem.Content is RfqListBoxView)
                ////{
                ////    DataGrid mmlist = (selectedTagItem.Content as RfqListBoxView).MMListView;
                ////    if (mmlist != null && mmlist.SelectedItem is DataRowView)
                ////    {
                ////        securityID = (mmlist.SelectedItem as DataRowView)["securityID"].ToString();
                ////    }
                ////}
                //his.OrderControl.securityID = securityID;
            }
        }
    }
}
