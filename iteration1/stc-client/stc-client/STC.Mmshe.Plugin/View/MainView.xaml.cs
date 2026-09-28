using Ats.Foundation.Utils;
using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Common;
using STC.Frame.CustomUI.Component;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.Utils.Data;
using STC.Mmshe.Plugin.View.HomePage;
using STC.Mmshe.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// UserControl2.xaml 的交互逻辑
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

        private string _groupId;
        public string BreakAlarmGroupID
        {
            get
            {
                return _groupId;
            }
            set
            {
                if (value != null)
                {
                    _groupId = value;
                }
            }
        }
        public MainView()
        {
            Utils.Data.MMsheGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            InitializeComponent();

            this.InterestRateDebtListBox.SelectionChanged += this.InterestRateDebtListBox_SelectionChanged;
            this.CreditBondListBox.SelectionChanged += this.CreditBondListBox_SelectionChanged;
            this.PolicyFinancialBondListBox.SelectionChanged += PolicyFinancialBondListBox_SelectionChanged;
            this.RemaingingTimeFilterListBox.SelectionChanged += this.RemaingingTimeFilterListBox_SelectionChanged;
            this.NewBondSignFilterListBox.SelectionChanged += this.NewBondSignFilterListBox_SelectionChanged;
            this.Rating_CurrentFilterListBox.SelectionChanged += this.Rating_CurrentFilterListBox_SelectionChanged;
            //this.enableListBox.SelectionChanged += EnableListBox_SelectionChanged;
            this.SettlTypeFilterListBox.SelectionChanged += SettlTypeFilterListBox_SelectionChanged;
            //this.monitorStatusListBox.SelectionChanged += MonitorStatusListBox_SelectionChanged;
            /*if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            {
                WidenQuoteBtn.Visibility = Visibility.Visible;
            }
            else
            {
                WidenQuoteBtn.Visibility = Visibility.Hidden;
            }*/
            initComboBoxData();
        }

        private void initComboBoxData()
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    SetComboBoxItemsSource(this.StrategyStatusMscb, "StrategyStauts");
                    SetComboBoxItemsSource(this.MMStatusMscb, "MMStatus");
                }));
            }
        }
        private void SetComboBoxItemsSource(MultiSelectComboBox cb, string key)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (key == "StrategyStauts")
            {
                dic["0001"] = "开启";
                dic["0010"] = "关闭";
            }
            else if (key == "MMStatus")
            {
                dic["0010"] = "已达标";
                dic["0001"] = "可达标";
                dic["0011"] = "危险";
                dic["0101"] = "失败";
            }
            cb.ItemsSource = dic;
        }
        private void HomePageDataHandler_OnFavoritesChanged(MMsheGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupid, string groupname)
        {
            if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                MMsheGroupInfo gi = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(groupid);
                if (_MainViewModel != null)
                {
                    _MainViewModel.SelectGroup = gi;
                }
                this.GroupTabControl.AddGroupTabItem(gi, !string.IsNullOrEmpty(groupname));
            }
            else if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                this.GroupTabControl.DelGroupTabItem(groupname);
            }
            //else if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.ChangeColumns)
            //{
            //    foreach (HomePageTabItemWithClose item in this.GroupTabControl.Items)
            //    {
            //        if (item.Content != null && item.Content is MMsheListBoxView)
            //        {
            //            MMsheListBoxView MMsheListBoxView = item.Content as MMsheListBoxView;
            //            MMsheListBoxView.InitColumnsInfo();
            //        }
            //    }
            //}
            else if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.UpdateGroup)
            {
                Task.Run(async () => {
                    await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (Action)(() =>
                    {
                        int index = 0;
                        if (MMsheGroupHandler.Instance.CacheSortedGroups != null)
                        {
                            foreach (MMsheGroupInfo groupInfo in MMsheGroupHandler.Instance.CacheSortedGroups)
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
                            if (_MainViewModel != null)
                            {
                                this.GroupTabControl.refreshWarnNum(_MainViewModel.cacheAlarmModel);
                            }
                        }
                    }));
                });
            }
        }

        public void OpenTab(string groupId, string securityId)
        {
            TabItem findTabItem = FindTabItem(groupId);
            if (findTabItem != null)
            {
                GroupTabControl.SelectedItem = findTabItem;
                //int oldIndex = this.GroupTabControl.Items.IndexOf(findTabItem);
                //Application.Current.Dispatcher.BeginInvoke
                //((Action)delegate { GroupTabControl.SelectedIndex = oldIndex; }, DispatcherPriority.Render, null);

            }
        }

        private TabItem FindTabItem(string groupId)
        {
            foreach (TabItem tabItem in this.GroupTabControl.Items)
            {
                if (tabItem.Tag != null && tabItem.Tag is MMsheGroupInfo)
                {
                    MMsheGroupInfo MMsheGroupInfo = tabItem.Tag as MMsheGroupInfo;

                    if (MMsheGroupInfo.id == groupId)
                    {
                        return tabItem;
                    }

                }
            }
            return null;
        }

        public void Dispose()
        {
            LogHelper.Debug("MM dispose");

            if (_MainViewModel != null)
            {
                _MainViewModel?.SetActiveMMListBox(_WinID, string.Empty);
            }

            Utils.Data.MMsheGroupHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            if (Utils.Data.MMsheGroupHandler.Instance.FavoritesChangedCount < 2)
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
                IOrderedEnumerable<MMsheGroupInfo> sortDic = await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();

                if (sortDic != null && sortDic.Count() > 0)
                {
                    foreach (MMsheGroupInfo gi in sortDic)
                    {
                        if (gi.action != 3)
                        {
                            Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, string.Empty);
                            // this.GroupTabControl.AddGroupTabItem(gi, false);
                        }
                    }
                    this.GroupTabControl.SelectedIndex = 0;
                    if (_MainViewModel != null)
                    {
                        this.GroupTabControl.refreshWarnNum(_MainViewModel.cacheAlarmModel);
                    }
                }
                if (!string.IsNullOrEmpty(BreakAlarmGroupID))
                {
                    OpenTab(BreakAlarmGroupID, "");
                    BreakAlarmGroupID = "";
                }
                IsInited = true;
            }
            catch (Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
        }


        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_MainViewModel != null
                && this.GroupTabControl.SelectedItem != null
                 && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            {
                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

                MMsheGroupInfo gi = selectedTagItem.Tag as MMsheGroupInfo;

                if (gi.inf1 == MMsheConstants.MM_GROPE_TYPE_UNIFY)
                {
                    this.OrderControl.IsUnify = true;
                }
                else
                {
                    this.OrderControl.IsUnify = false;
                }

                LogHandler.Info($"--------click tab1 {gi.groupname}:{gi.id}:{selectedTagItem.IsVisible}");
                _MainViewModel.SelectGroup = gi;
                if (selectedTagItem.Content == null)
                {
                    if (selectedTagItem.Tag != null && selectedTagItem.Tag is MMsheGroupInfo)
                    {
                        LogHandler.Info($"--------{DateTime.Now}--:GroupTabControl_SelectionChanged");
                        LogHandler.Info($"--------click tab {gi.groupname}");

                        MMsheListBoxView favoriteBondsLB = new MMsheListBoxView();
                        favoriteBondsLB.DataContext = _MainViewModel?.GetMmsheListBoxViewModelByGroupID(gi.id);
                        favoriteBondsLB.MMListView.SelectionChanged += MMListView_SelectionChanged;
                        selectedTagItem.Content = favoriteBondsLB;
                    }
                }

                _MainViewModel?.SetActiveMMListBox(_WinID, gi.id);

                string securityID = string.Empty;
                if (selectedTagItem.Content != null && selectedTagItem.Content is MMsheListBoxView)
                {
                    DataGrid mmlist = (selectedTagItem.Content as MMsheListBoxView).MMListView;
                    if (mmlist != null && mmlist.SelectedItem is DataRowView)
                    {
                        securityID = (mmlist.SelectedItem as DataRowView)["securityID"].ToString();
                    }
                }
                this.OrderControl.securityID = securityID;
                this.OrderControl.Venue = gi.venues;
            }
        }


        private void MMListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DataRowView drv = mmlist.MMListView.SelectedItem as DataRowView;
            //if (!drv.DataView.Table.Columns.Contains("dgPriceType"))
            //{

            //    drv.DataView.Table.Columns.Add("dgPriceType",typeof(OrderControlPriceType));
            //}

            //OrderControlPriceType priceType = new OrderControlPriceType();
            //drv.BeginEdit();
            //drv.Row["dgPriceType"] = priceType; //默认为全市场
            //drv.EndEdit();

            if (sender != null && sender is DataGrid)
            {
                DataGrid mmlist = sender as DataGrid;


                string securityID = string.Empty;
                string groudID = (mmlist.DataContext as MmsheListBoxViewModel).groupID;

                foreach (HomePageTabItemWithClose item in this.GroupTabControl.Items)
                {
                    if (item.Tag is MMsheGroupInfo)
                    {
                        MMsheGroupInfo gi = item.Tag as MMsheGroupInfo;
                        if (gi.id == groudID && item.IsSelected == true)
                        {
                            if (mmlist != null && mmlist.SelectedItem is DataRowView)
                            {
                                securityID = (mmlist.SelectedItem as DataRowView)["securityID"].ToString();
                            }
                            this.OrderControl.securityID = securityID;
                            this.OrderControl.Venue = gi.venues;
                            break;
                        }
                    }

                }



            }
        }


        private void OrderControl_CurrentOrderParamChanged(object sender, RoutedEventArgs e)
        {
            //if (this.OrderControl.CurrentOrderParam != null & _MMsheListBoxViewModel != null)
            {
                //TODO:参数改变，需要发送给后端
            }
        }

        private void MM_Warn_bd_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWarnPopup();
        }

        private void MM_Warn_bd_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!MM_Warn_bd.IsMouseOver && !this.AnsAttention.IsMouseOver)
            {
                CollapsedWarnPopup();
            }
        }

        private void Ans_status_tb_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWarnPopup();
        }

        private void Ans_status_tb_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapsedWarnPopup();
        }

        private void ShowWarnPopup()
        {
            if (_MainViewModel.CurrentANSSummary != null && !string.IsNullOrEmpty(_MainViewModel.CurrentANSSummary.status))
            {
                MM_Duty_pop.Visibility = Visibility.Visible;
                MM_Duty_pop.IsOpen = true;
                AnsAttention.DataContext = _MainViewModel.CurrentANSSummary;
                AnsAttention.newBond_dg.SelectedIndex = -1;
                AnsAttention.otherBond_dg.SelectedIndex = -1;
                AnsAttention.bondType_dg.SelectedIndex = -1;
            }
            //else
            //{
            //    CollapsedWarnPopup();
            //}
        }

        private void CollapsedWarnPopup()
        {
            AnsAttention.DataContext = null;
            MM_Duty_pop.IsOpen = false;
            MM_Duty_pop.Visibility = Visibility.Collapsed;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = true;
        }

        private const string ALL = "全部";

        private void ItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (e.AddedItems.Count > 0)
                {
                    foreach (object obj in e.AddedItems)
                    {
                        ListBoxItem item = obj as ListBoxItem;
                        if (item != null)
                        {
                            if (ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (lbi.IsSelected && !ALL.Equals(lbi.Content))
                                        lbi.IsSelected = false;
                                }
                            }
                            else
                            {
                                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                if (tmp.IsSelected)
                                    tmp.IsSelected = false;

                            }
                        }
                    }
                }

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (listBox.SelectedItems.Count == 0)
                            listBox.SelectedItem = item;
                    }
                }
            }
        }

        private void InterestRateDebtListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterBondType(InterestRateDebtListBox.SelectedItems);
            }
        }

        private void CreditBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            AdjustShowAll_ToggleButtonCheck();

            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterCreditBondType();
            }
        }

        private void EnableListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterEnableStatus();
            }
        }

        private void PolicyFinancialBondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterPolicyFinancialBond();
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterRemainingTime();
            }
        }

        private void NewBondSignFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterNewBondSign();
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterRating();
            }
        }

        private void SettlTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterSettlType();
            }
        }

        private void MonitorStatusListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._MainViewModel != null)
            {
                this._MainViewModel.FilterMonitorStatus();
            }
        }

        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private void Rating_CurrentFilterListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var item10 = this.Rating_CurrentFilterListBox.Items[0];
            if (item10 != null && item10 is ListBoxItem)
            {
                ListBoxItem lbi = item10 as ListBoxItem;
                var reg = lbi.Template.FindName("Sep", lbi);
                if (reg != null && reg is Rectangle)
                {
                    ((Rectangle)reg).Visibility = Visibility.Hidden;
                }
            }
        }

        private bool isNonmanualCheckShowAll = false;
        //private bool isCheckShowAll = false;
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }

            //isCheckShowAll = true;
            if (this.InterestRateDebtListBox != null)
            {
                this.InterestRateDebtListBox.SelectedIndex = -1;
            }

            if (this.CreditBondListBox != null)
            {
                this.CreditBondListBox.SelectedIndex = -1;
            }

            //isCheckShowAll = false;
        }

        private void ShowAll_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;

            if (InterestRateDebtListBox.SelectedItems.Count > 0
                 || this.CreditBondListBox.SelectedItems.Count > 0)
            {
                isChecked = false;
            }

            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked;
            isNonmanualCheckShowAll = false;
        }

        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.InterestRateDebtListBox.SelectedIndex = -1;
            this.CreditBondListBox.SelectedIndex = -1;
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            int mmStatisticsPluginKey = (int)PluginKeyEnum.MmStatisticsPlugin;

            //MmStatistics view = new MmStatistics();
            //XBond.Plugin.Utils.TabPageHelper.OpenTabPage("做市统计", view, mmStatisticsPluginKey);

            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(mmStatisticsPluginKey);

            if (plugin != null)
            {
                bool isExist = TabPageHelper.IsExistTabItem(plugin.Description);

                if (!isExist)
                {
                    AppUtils.Application.PluginManager.EnablePlugin(mmStatisticsPluginKey);
                }
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Common.View.Component.CustomMessageBox.Info("目前处于一键拉宽状态，请先恢复正常报价后再操作", "提醒");
        }

        private void MM_MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is MainViewModel)
            {
                _MainViewModel = e.NewValue as MainViewModel;

                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;
                if (selectedTagItem != null && selectedTagItem.Content == null)
                {
                    MMsheListBoxView mmlist = new MMsheListBoxView();
                    MMsheGroupInfo gi = selectedTagItem.Tag as MMsheGroupInfo;
                    _MainViewModel.SelectGroup = gi;
                    _MainViewModel?.SetActiveMMListBox(_WinID, gi.id);
                    mmlist.DataContext = _MainViewModel?.GetMmsheListBoxViewModelByGroupID(gi.id);
                    mmlist.MMListView.SelectionChanged += MMListView_SelectionChanged;
                    selectedTagItem.Content = mmlist;
                }
            }
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null && string.IsNullOrEmpty(obj.Bond_ID) == false)
            {
                //STC.Common.Model.MMshe.MMshe mm = MMsheGroupHandler.Instance.GetMMBySecurityID(obj.Bond_ID);
                //if (mm != null)
                //{
                //    foreach (HomePageTabItemWithClose item in this.GroupTabControl.Items)
                //    {
                //        if (item.Tag is MMsheGroupInfo)
                //        {
                //            MMsheGroupInfo gi = item.Tag as MMsheGroupInfo;
                //            if (gi.id == mm.groupid)
                //            {
                //                item.IsSelected = true;
                //                break;
                //            }
                //        }

                //    }
                //}

            }

            //this.OrderControl.dgPrice.Init(string.Empty);
            //this.OrderControl.dgPriceQdmesp.Init(string.Empty);
            //this.OrderControl.Tab_DgPrice.SelectedIndex = 0;
        }

        private void FilterPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterPopup.IsOpen = false;
        }

        private void isBreakBondCB_Checked(object sender, RoutedEventArgs e)
        {
            _MainViewModel.FilterPauseBond(true);
        }

        private void isBreakBondCB_Unchecked(object sender, RoutedEventArgs e)
        {
            _MainViewModel.FilterPauseBond(false);
        }

        private void StrategyStatusMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();
            if (!string.IsNullOrEmpty(ccb.SelectedValues))
            {
                string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                _MainViewModel.FilterStrategyStatus(orgArr);
            }
            else
            {
                _MainViewModel.FilterStrategyStatus(null);
            }
        }

        private void MMStatusMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();
            if (!string.IsNullOrEmpty(ccb.SelectedValues))
            {
                string[] orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                _MainViewModel.FilterMMStatus(orgArr);
            }
            else
            {
                _MainViewModel.FilterMMStatus(null);
            }
        }
    }
}
