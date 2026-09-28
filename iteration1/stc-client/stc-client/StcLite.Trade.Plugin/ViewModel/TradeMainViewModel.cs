using com.gateway.log;
using Dragablz;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View;
using BDB.Trade.Plugin.View.Trade;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STC.Common.Model.Admin;
using STC.Common.Model;
using STC.Common.View.Component;
using STC.Common.Model.Fix.Message;

namespace BDB.Trade.Plugin.ViewModel
{
    public class TradeMainViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;

        private ObservableCollection<GroupItems> _groupsList = new ObservableCollection<GroupItems>();

        private string preItemName;

        public void SetPreItemName(string tabItem)
        {
            preItemName = tabItem;
        }

        #region 关联属性

        public ICommand AddGroupCommand { get { return new RelayCommand(AddGroupClick); } }

        public ICommand AddBondCommand { get { return new RelayCommand(AddBondClick); } }

        private TabItem _selectTab = null;
        public TabItem SelectTab { get { return _selectTab; } set { Console.WriteLine("------------tabName:" + (value != null ? value.Name : string.Empty));  _selectTab = value; } }

        #endregion

        private void AddGroupClick()
        {
            // 添加组
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result)
            {
                if (addGroupWindow.Result == true)
                {
                    HomePageDataHandler.Instance.AddGroup(addGroupWindow.GroupName);
                }
            }
        }

        private void AddBondClick()
        {
            AddBondWindow addBondWind = new AddBondWindow();
            if (_selectTab != null)
            {
                GroupItems groupItem = _selectTab.Tag as GroupItems;
                addBondWind.SetGroupId(groupItem.GroupId);
            }

            addBondWind.Owner = Application.Current.MainWindow;
            addBondWind.ShowDialog();
        }



        public void Init(FrameworkElement ownUI)
        {
            _ownUI = ownUI;

            HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;

            LoadGroup();
        }

        

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum actionEnum, string groupID, string securityID, string bondId)
        {
            if (actionEnum == FavoriteTypeEnum.AddGroup)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    // 刷新缓存数据, 并添加新的标签页
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = HomePageDataHandler.Instance.RequestGroups(false);
                    if (sortedGroups != null)
                    {
                        HomeGroupInfo gi = sortedGroups.Last().Value;
                        GroupItems groupItem = new GroupItems();
                        groupItem.IsChecked = false;
                        groupItem.GroupId = gi.id;
                        groupItem.GroupName = gi.groupname;
                        _groupsList.Add(groupItem);

                        CreateInnerTab(groupItem);



                        QuoteConfigPo quoteConfigPo = new QuoteConfigPo();

                        quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;

                        quoteConfigPo.Pages = new List<QcPagePo>();
                        quoteConfigPo.GroupId = gi.id;
                        quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
                        quoteConfigPo.Rows = LayoutSetttingCache.Instance.layoutSettingPo.Row;
                        quoteConfigPo.Cols = LayoutSetttingCache.Instance.layoutSettingPo.Col;

                        QcPagePo pagePo = new QcPagePo();
                        pagePo.GroupId = gi.id;
                        pagePo.Index = 1;
                        pagePo.Panels = new List<QcPanelPo>();
                        quoteConfigPo.Pages.Add(pagePo);

                        int pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;

                        for (int i = 0; i < pageCount; i++)
                        {
                            QcPanelPo panelPo = new QcPanelPo();

                            panelPo.BondCode = string.Empty;
                            panelPo.Id = string.Empty;

                            pagePo.Panels.Add(panelPo);
                        }



                        QuoteConfigHandler.Instance.SaveConfig(quoteConfigPo, gi.id);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            (_ownUI as TradeMainView).UserControl_SizeChanged(_ownUI, null);
                        }));
                    }
                });
            }
            else if (actionEnum == FavoriteTypeEnum.DelGroup)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    TabControl tabControl = this._ownUI.FindName("tcPlugin") as TabControl;
                    TabItem removeItem = null;
                    foreach (TabItem tabItem in tabControl.Items)
                    {
                        GroupItems groupItem = tabItem.Tag as GroupItems;

                        if (groupItem != null && groupItem.GroupId == groupID)
                        {
                            removeItem = tabItem;

                            break;
                        }
                    }

                    if (removeItem != null)
                    {
                        GroupItems groupItems = removeItem.Tag as GroupItems;
                        if (groupItems != null)
                            UnScribeHeadChange(groupItems.GroupId);

                        tabControl.Items.Remove(removeItem);
                        StackPanel sp = this._ownUI.FindName("spSearch") as StackPanel;
                        if (sp != null)
                            sp.Margin = new Thickness(sp.Margin.Left - 106, sp.Margin.Top, 0, 0);


                        //删除分组
                        QuoteConfigHandler.Instance.DeleteConfigByGroupId(groupItems.GroupId, false);
                        //PageCardHandler.Instance.ClearUI();
                        //removeItem.Content = null;
                        //removeItem = null;
                    }

                    SetAddButtonStatus(tabControl);

                    ResetHeaderSize();
                }
                ));
            }
            
        }


        private void ResetHeaderSize()
        {
            TradeMainView tradeMainView = this._ownUI as TradeMainView;
            if (tradeMainView != null)
            {
                tradeMainView.UserControl_SizeChanged(tradeMainView, null);
            }
        }

        private void SetAddButtonStatus(TabControl tabControl)
        {
            Button btnAdd = this._ownUI.FindName("btnCustomAdd") as Button;
            if (tabControl.Items.Count >= 50)
                btnAdd.IsEnabled = false;
            else
                btnAdd.IsEnabled = true;
        }

        private void LoadGroup()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroupList = HomePageDataHandler.Instance.AsyncRequestGroups(true);
                    LogHelper.Info("LoadGroup end");
                    if (sortedGroupList != null && sortedGroupList.Count() > 0)
                    {
                        //foreach (KeyValuePair<string, HomeGroupInfo> pair in sortedGroupList)
                        //{
                        //    HomeGroupInfo homeGroupInfo = pair.Value;
                        //    GroupItems groupItem = new GroupItems();
                        //    groupItem.IsChecked = false;
                        //    groupItem.GroupId = homeGroupInfo.id;
                        //    groupItem.GroupName = homeGroupInfo.groupname;
                        //    _groupsList.Add(groupItem);

                        //    //RememberHelper.Instance.ReadCustomSort(groupItem.GroupId);
                        //    //RememberHelper.Instance.ReadCustomSettle(groupItem.GroupId);

                        //    CreateInnerTab(groupItem);
                        //}


                        CreateTab(sortedGroupList);
                    }
                    else
                    {
                        HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.NoGroup, string.Empty, string.Empty, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void CreateTab(IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroupList)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TabControl tabControl = this._ownUI.FindName("tcPlugin") as TabControl;
                tabControl.SelectedIndex = -1;
                TabItem selectItem = null;
                foreach (KeyValuePair<string, HomeGroupInfo> pair in sortedGroupList)
                {
                    HomeGroupInfo homeGroupInfo = pair.Value;
                    GroupItems group = new GroupItems();
                    group.IsChecked = false;
                    group.GroupId = homeGroupInfo.id;
                    group.GroupName = homeGroupInfo.groupname;
                    _groupsList.Add(group);

                    //RememberHelper.Instance.ReadCustomSort(groupItem.GroupId);
                    //RememberHelper.Instance.ReadCustomSettle(groupItem.GroupId);

                    TabItem tabItem = new TabItem();
                    tabItem.Header = group.GroupName;
                    tabItem.Tag = group;
                    tabItem.ToolTip = group.GroupName;

                    
                    tabControl.Items.Add(tabItem);
                    tabItem.Content = new TableAndCardView(group.GroupId, LayoutSetttingCache.Instance.layoutSettingPo.ShowModel == 1 ? Model.DisplayMode.List : Model.DisplayMode.Card);//new PriceTableView(group.GroupId);

                    if (!string.IsNullOrEmpty(preItemName) && group.GroupName.Equals(preItemName))
                    {
                        //tabControl.SelectedItem = tabItem;
                        selectItem = tabItem;
                    }
                    //else if (tabControl.SelectedIndex < 0)
                    //{
                    //    tabControl.SelectedIndex = 0;
                    //}

                    StackPanel sp = this._ownUI.FindName("spSearch") as StackPanel;
                    if (sp != null)
                        sp.Margin = new Thickness(sp.Margin.Left + 106, sp.Margin.Top, 0, 0);

                    LogHelper.Warn("------tabControl selectIndex:" + tabControl.SelectedIndex);


                    SetAddButtonStatus(tabControl);

                    TabablzControl tabablzControl = this._ownUI.FindName("tcPlugin") as TabablzControl;
                    if (tabablzControl != null)
                    {
                        IEnumerable<DragablzItem> dragablzItems = tabablzControl.GetOrderedHeaders();
                        if (dragablzItems != null)
                        {
                            foreach (DragablzItem item in dragablzItems)
                            {
                                item.CanDoubleClick = true;
                                TabItem innerItem = item.DataContext as TabItem;
                                if (innerItem != null)
                                {
                                    GroupItems groupItems = innerItem.Tag as GroupItems;
                                    if (groupItems != null && groupItems.GroupId == group.GroupId)
                                        item.OnHeadChange += Item_OnHeadChange;
                                }

                            }
                        }
                    }
                }

                if (selectItem != null)
                {
                    tabControl.SelectedItem = selectItem;
                }
                else if (tabControl.SelectedIndex < 0)
                {
                    tabControl.SelectedIndex = 0;
                }

                this.ResetHeaderSize();
                LogHelper.Warn("----111--tabControl selectIndex:" + tabControl.SelectedIndex);
            }));
        }


        private void UnScribeHeadChange(string groupId)
        {
            TabablzControl tabablzControl = this._ownUI.FindName("tcPlugin") as TabablzControl;
            IEnumerable<DragablzItem> dragablzItems = tabablzControl.GetOrderedHeaders();

            DragablzItem removeItem = null;
            foreach (DragablzItem item in dragablzItems)
            {
                TabItem tabItem = item.DataContext as TabItem;
                if (tabItem != null)
                {
                    GroupItems groupItems = tabItem.Tag as GroupItems;
                    if (groupItems != null && groupItems.GroupId == groupId)
                    {
                        removeItem = item;
                        break;
                    }
                }
            }

            if (removeItem != null)
                removeItem.OnHeadChange -= Item_OnHeadChange;
        }

        private void Item_OnHeadChange(string oldHead, string newHead)
        {
            TabItem tabItem = null;

            TabControl tabControl = this._ownUI.FindName("tcPlugin") as TabControl;
            bool flag = false;
            int index = -1;
            foreach (TabItem item in tabControl.Items)
            {
                index++;
                if (item.Header.ToString() == oldHead)
                {
                    tabItem = item;
                }

                if (item.Header.ToString() == newHead)
                    flag = true;
            }

            if (flag)
            {
                CustomMessageBox.Warning(string.Format("分组名称({0})已存在!", newHead));
                return;
            }

            GroupItems groupItems = tabItem.Tag as GroupItems;
            if (groupItems != null)
            {
                HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                request.id = groupItems.GroupId;
                request.groupname = newHead;//组名
                request.action = "2";//更新
                request.ordersn = index.ToString();//顺序

                ThreadPool.QueueUserWorkItem(o =>
                {

                    try
                    {

                        PResult pResult = HomePageDataHandler.Instance.UpdateGroups(request);
                        if (pResult != null && pResult.Code == "0000")
                        {
                            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupItems.GroupId);
                            homeGroupInfo.groupname = newHead;

                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                tabItem.Header = newHead;
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }

            
        }

        private void CreateInnerTab(GroupItems group)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = group.GroupName;
                tabItem.Tag = group;
                tabItem.ToolTip = group.GroupName;
                
                TabControl tabControl = this._ownUI.FindName("tcPlugin") as TabControl;
                tabControl.Items.Add(tabItem);
                tabItem.Content = new TableAndCardView(group.GroupId, LayoutSetttingCache.Instance.layoutSettingPo.ShowModel==1?Model.DisplayMode.List:Model.DisplayMode.Card);//new PriceTableView(group.GroupId);

                //if (!string.IsNullOrEmpty(preItemName) && group.GroupName.Equals(preItemName))
                //{
                //    tabControl.SelectedItem = tabItem;
                //}
                //else if (tabControl.SelectedIndex < 0)
                //{
                //    tabControl.SelectedIndex = 0;
                //}
                tabControl.SelectedItem = null;
                tabControl.SelectedItem = tabItem;

                StackPanel sp = this._ownUI.FindName("spSearch") as StackPanel;
                if (sp != null)
                    sp.Margin = new Thickness(sp.Margin.Left + 106, sp.Margin.Top, 0, 0);


                

                SetAddButtonStatus(tabControl);

                TabablzControl tabablzControl = this._ownUI.FindName("tcPlugin") as TabablzControl;
                if (tabablzControl != null)
                {
                    IEnumerable<DragablzItem> dragablzItems = tabablzControl.GetOrderedHeaders();

                    foreach (DragablzItem item in dragablzItems)
                    {
                        item.CanDoubleClick = true;
                        TabItem innerItem = item.DataContext as TabItem;
                        if (innerItem != null)
                        {
                            GroupItems groupItems = innerItem.Tag as GroupItems;
                            if (groupItems != null && groupItems.GroupId == group.GroupId)
                                item.OnHeadChange += Item_OnHeadChange;
                        }

                    }
                }


                ResetHeaderSize();
            }));
        }
        
    }
}
