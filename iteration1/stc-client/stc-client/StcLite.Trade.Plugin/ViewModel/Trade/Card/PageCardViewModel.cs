using com.gateway.log;
using STC.Common.Model;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.View.Trade.Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using STC.Common.Model.Admin;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class PageCardViewModel : BaseViewModel
    {
        string key = string.Empty;

        private FrameworkElement ownUI;
        private bool isInit = false;
        private ObservableCollection<string> pageButtonList = new ObservableCollection<string>();

        public ObservableCollection<string> PageButtonList
        {
            get
            {
                return pageButtonList;
            }
            //set
            //{
            //    pageButtonList = value;
            //    this.OnPropertyChanged();
            //}
        }


        private int pageSelectIndex = -1;
        public int PageSelectIndex
        {
            get { return pageSelectIndex; }
            set
            {
                pageSelectIndex = value;

                //TabControl tcGroupPage = parentUI.FindName("tcGroupPage") as TabControl;

                //tcGroupPage.SelectedIndex = PageSelectIndex;

                this.OnPropertyChanged();

                if (value >= 0 && !isInit)
                {
                    OnPageChange(DataType.SQLite);

                }

            }
        }



        public void SetPageIndexAndResetPage(int index, DataType dataType)
        {
            pageSelectIndex = index;
            this.OnPropertyChanged("PageSelectIndex");

            OnPageChange(dataType);
        }

        private string currentGroupId = string.Empty;
        
        private List<PageCardItemView> pageCardItemViewList = new List<PageCardItemView>();

        public string CurrentGroupId
        {
            get { return currentGroupId; }
        }

        public void SetCurrentGroupId(string groupId)
        {
            currentGroupId = groupId;
        }


        public void OnPageChange(DataType dataType)
        {
            TabControl tcGroupPage = GetTabControl();
            if (tcGroupPage != null && tcGroupPage.Items.Count > 0)
            {
                TabItem tabItem = tcGroupPage.Items[0] as TabItem;

                PageCardHandler.Instance.ResetData(tabItem, CurrentGroupId, PageSelectIndex + 1, key);
                //PageCardHandler.Instance.ResetPage(tabItem, currentGroupId, PageSelectIndex + 1, dataType);
            }
        }


        public Button CreatePageButton(string content)
        {
            Style style = ownUI.FindResource("PageButton") as Style;
            Button button = new Button();
            //button.Style = style;
            button.Click += PageButtonClick;
            button.Content = content;


            return button;
        }

        private void PageButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string pageNumStr = button.Content as string;
            int pageNum = 0;
            if (int.TryParse(pageNumStr, out pageNum))
            {
                TabControl tcGroupPage = ownUI.FindName("tcGroupPage") as TabControl;
                tcGroupPage.SelectedIndex = pageNum - 1;
            }
        }

        public TabControl GetTabControl()
        {
            if (ownUI != null)
            {
                TabControl tcGroupPage = ownUI.FindName("tcGroupPage") as TabControl;

                return tcGroupPage;
            }

            return null;
        }

        private bool isSub = false;
        public void InitSub()
        {
            if (!isSub)
            {
                QuoteConfigHandler.Instance.OnGroupTableChange += Instance_OnGroupTableChange;

                isSub = true;
            }
        }

        public void Init(FrameworkElement frameworkElement, string groupId, bool refresh)
        {
            long start = DateTime.Now.Ticks;

            if (ownUI == null)
            {
                ownUI = frameworkElement;

                key = PageCardHandler.GetWindowHashKey(ownUI);

                HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;

                //QuoteConfigHandler.Instance.OnGroupTableChange += Instance_OnGroupTableChange;
            }

            TabControl tcGroupPage = GetTabControl();

            if (tcGroupPage == null)
                return;

            

            if (string.IsNullOrEmpty(currentGroupId) || refresh)
            {
                isInit = true;
                foreach (TabItem tab in tcGroupPage.Items)
                {
                    tab.Content = null;
                }


                currentGroupId = groupId;
                

                if (refresh)
                {
                    tcGroupPage.Items.Clear();

                    PageButtonList.Clear();

                }

                TabItem tabItem = null;
                if (tcGroupPage.Items.Count == 0)
                {
                    tabItem = new TabItem();
                    tabItem.Visibility = Visibility.Collapsed;
                    tcGroupPage.Items.Add(tabItem);

                    tcGroupPage.SelectedIndex = 0;
                }
                else
                {
                    tabItem = tcGroupPage.Items[0] as TabItem;
                }


                int pageIndex = 0;
                int pageCount = 0;
                int pageNum = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;
                QuoteConfigPo cacheConfig = QuoteConfigHandler.Instance.GetConfigByGroupId(groupId);


                bool isResetPage = false;
                bool isCreate = PageCardHandler.Instance.IsCreateCardView(key);//CardView已创建后只刷新数据
                //if (cacheConfig != null && !resetCard)
                if (cacheConfig != null && isCreate)
                {
                    pageCount = cacheConfig.Pages.Count;

                    PageButtonList.Clear();

                    for (int j = 0; j < pageCount; j++)
                    {
                        PageButtonList.Add((j + 1).ToString());
                    }



                    QcPagePo lastPage = null;
                    foreach (QcPagePo pagePo in cacheConfig.Pages)
                    {
                        if (pagePo.IsSeleted)
                        {
                            pageIndex = pagePo.Index;
                        }

                        if (lastPage == null || pagePo.Index > lastPage.Index)
                        {
                            lastPage = pagePo;
                        }
                    }

                    pageSelectIndex = pageIndex - 1 >= 0 ? pageIndex - 1 : 0;
                    this.OnPropertyChanged("PageSelectIndex");

                    PageCardHandler.Instance.ResetData(tabItem, groupId, PageSelectIndex + 1, key);
                    
                    //最后一页判断是否显示add按钮
                    bool isShowAddBtn = false;
                    if (lastPage != null)
                    {
                        foreach (QcPanelPo panelPo in lastPage.Panels)
                        {
                            if (!string.IsNullOrEmpty(panelPo.BondCode))
                            {
                                isShowAddBtn = true;
                                break;
                            }
                        }
                    }
                    Button btAdd = ownUI.FindName("btAdd") as Button;
                    btAdd.Visibility = isShowAddBtn ? Visibility.Visible : Visibility.Collapsed;

                }
                else
                {
                    CommonHandler.Instance.ResetNum();
                    List<HomeGroupSecurityIDsInfo> securityIDsInfosList = null;
                    if (cacheConfig == null)
                    {
                        securityIDsInfosList = QuoteConfigHandler.Instance.GetPreCardListByGroupId(groupId);
                        if (securityIDsInfosList == null)
                        {
                            securityIDsInfosList = PageCardHandler.Instance.GetPageSecurityIDList(groupId, 1, DataType.DB);
                        }
                        int maxCount = securityIDsInfosList.Count;
                        pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;

                        pageNum = maxCount % pageCount == 0 ? (maxCount / pageCount) : (maxCount / pageCount + 1);



                        QuoteConfigPo quoteConfigPo = new QuoteConfigPo();

                        quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;

                        quoteConfigPo.Pages = new List<QcPagePo>();
                        quoteConfigPo.GroupId = currentGroupId;
                        quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
                        quoteConfigPo.Rows = LayoutSetttingCache.Instance.layoutSettingPo.Row;
                        quoteConfigPo.Cols = LayoutSetttingCache.Instance.layoutSettingPo.Col;

                        if (pageNum > 0)
                        {
                            PageButtonList.Clear();
                            for (int j = 0; j < pageNum; j++)
                            {
                                int index = j * pageCount;
                                PageButtonList.Add((j + 1).ToString());

                                QcPagePo pagePo = new QcPagePo();
                                if (pageIndex == index)
                                    pagePo.IsSeleted = true;

                                pagePo.GroupId = currentGroupId;
                                pagePo.Index = j + 1;
                                pagePo.Panels = new List<QcPanelPo>();
                                quoteConfigPo.Pages.Add(pagePo);

                                for (int i = 0; i < pageCount; i++)
                                {
                                    QcPanelPo panelPo = new QcPanelPo();
                                    if (index < securityIDsInfosList.Count)
                                    {
                                        HomeGroupSecurityIDsInfo securityIDsInfo = securityIDsInfosList[index];
                                        if (securityIDsInfo != null && !string.IsNullOrEmpty(securityIDsInfo.securityid))
                                        {
                                            panelPo.BondCode = securityIDsInfo.securityid;
                                            panelPo.Id = securityIDsInfo.id;


                                        }
                                    }
                                    panelPo.Row = (int)(i / quoteConfigPo.Cols) + 1;
                                    panelPo.Col = (i % quoteConfigPo.Cols == 0) ? quoteConfigPo.Cols : (i % quoteConfigPo.Cols + 1);
                                    pagePo.Panels.Add(panelPo);

                                    index++;
                                }

                            }
                        }
                        else
                        {
                            PageButtonList.Add(1.ToString());

                            QcPagePo pagePo = new QcPagePo();
                            pagePo.GroupId = currentGroupId;
                            pagePo.Index = 1;
                            pagePo.Panels = new List<QcPanelPo>();
                            quoteConfigPo.Pages.Add(pagePo);
                            for (int i = 0; i < pageCount; i++)
                            {
                                QcPanelPo panelPo = new QcPanelPo();

                                panelPo.BondCode = string.Empty;
                                panelPo.Id = string.Empty;

                                panelPo.Row = (int)(i / quoteConfigPo.Cols) + 1;
                                panelPo.Col = (i % quoteConfigPo.Cols == 0) ? quoteConfigPo.Cols : (i % quoteConfigPo.Cols + 1);

                                pagePo.Panels.Add(panelPo);
                            }

                        }

                        QuoteConfigHandler.Instance.SaveConfig(quoteConfigPo, groupId);
                    }
                    else
                    {

                        if (!isCreate)
                        {
                            pageNum = cacheConfig.Pages.Count;

                            PageButtonList.Clear();

                            for (int j = 0; j < pageNum; j++)
                            {
                                PageButtonList.Add((j + 1).ToString());
                            }

                            QcPagePo lastPage = null;
                            foreach (QcPagePo pagePo in cacheConfig.Pages)
                            {
                                if (pagePo.IsSeleted)
                                {
                                    pageIndex = pagePo.Index;
                                }

                                if (lastPage == null || pagePo.Index > lastPage.Index)
                                {
                                    lastPage = pagePo;
                                }
                            }

                            pageIndex = pageIndex - 1 >= 0 ? pageIndex - 1 : 0;
                            
                            //最后一页判断是否显示add按钮
                            bool isShowAddBtn = false;
                            if (lastPage != null)
                            {
                                foreach (QcPanelPo panelPo in lastPage.Panels)
                                {
                                    if (!string.IsNullOrEmpty(panelPo.BondCode))
                                    {
                                        isShowAddBtn = true;
                                        break;
                                    }
                                }
                            }
                            Button btAdd = ownUI.FindName("btAdd") as Button;
                            btAdd.Visibility = isShowAddBtn ? Visibility.Visible : Visibility.Collapsed;

                        }
                        else
                        {
                            securityIDsInfosList = PageCardHandler.Instance.GetPageSecurityIDList(groupId, 1, DataType.DB);
                            int maxCount = securityIDsInfosList.Count;
                            pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;
                            pageNum = maxCount % pageCount == 0 ? (maxCount / pageCount) : (maxCount / pageCount + 1);
                            PageButtonList.Clear();
                            if (pageNum > 0)
                            {
                                for (int j = 0; j < pageNum; j++)
                                {
                                    PageButtonList.Add((j + 1).ToString());
                                }
                            }
                            else
                            {
                                PageButtonList.Add(1.ToString());
                            }
                        }
                    }

                    pageSelectIndex = pageIndex;
                    tcGroupPage.SelectedIndex = PageSelectIndex;
                    this.OnPropertyChanged("PageSelectIndex");


                    if (cacheConfig == null)
                    {
                        List<HomeGroupSecurityIDsInfo> temList = QuoteConfigHandler.Instance.GetPreCardListByGroupId(groupId);
                        if (temList == null)
                        {
                            PageCardHandler.Instance.ResetPage(tabItem, groupId, PageSelectIndex + 1, DataType.DB, key);
                        }
                        else
                        {
                            PageCardHandler.Instance.ResetPage(tabItem, groupId, PageSelectIndex + 1, DataType.SQLite, key);
                        }
                    }
                    else
                    {
                        PageCardHandler.Instance.ResetPage(tabItem, groupId, PageSelectIndex + 1, DataType.SQLite, key);
                    }
                    isResetPage = true;
                    if (refresh)
                    {
                        if (isResetPage)
                        {
                            GC.Collect();
                        }
                    }
                }

                RefreshWidth();
                isInit = false;
                //if (refresh)
                //{
                //    RefreshWidth();

                //    //long startGC = DateTime.Now.Ticks;
                //    if (isResetPage)
                //    {
                //        GC.Collect();
                //    }
                //    //long endGC = DateTime.Now.Ticks;

                //    //Console.WriteLine("执行一次GC耗时:{0} 秒" + (endGC - startGC) / 10000000.0);
                //    //LogHelper.Warn("执行一次GC耗时:{0} 秒" + (endGC - startGC) / 10000000.0);
                //}

                //long end = DateTime.Now.Ticks;
                //Console.WriteLine("执行一次ResetData耗时:{0} 秒" + (end - start) / 10000000.0);
                //LogHelper.Warn("执行一次ResetData-1耗时:{0} 秒" + (end - start) / 10000000.0);
                //if (refresh)
                //{
                //    Instance_OnFavoritesChanged(FavoriteTypeEnum.AddBond, currentGroupId, string.Empty, string.Empty);

                //    GC.Collect();
                //}

                //long end = DateTime.Now.Ticks;

                //Console.WriteLine("执行一次Init耗时:{0} 秒" + (end - start) / 10000000.0);

                //LogHelper.Info("-------------执行一次Init耗时:{0} 秒" + (end - start) / 10000000.0);

            }
            else
            {
                //this.SetPageIndexAndResetPage(PageSelectIndex, DataType.SQLite);

                //tcGroupPage.SelectedIndex = PageSelectIndex;
            }
        }

        private void RefreshWidth()
        {
            StackPanel stackPanel = ownUI?.FindName("spItemsHost") as StackPanel;
            if (stackPanel != null)
            {
                stackPanel.Width = 66 + PageButtonList.Count * 19;

            }

            ListBox listBox = ownUI?.FindName("lbButton") as ListBox;
            if (listBox != null)
            {
                listBox.Width = 4 + PageButtonList.Count * 19;

            }
        }

        private void Instance_OnGroupTableChange(string groupId, string id, string securityId, FavoriteTypeEnum favoriteTypeEnum)
        {
            if (currentGroupId == groupId)
            {
                //列表删除、增加，卡片重置
                QuoteConfigHandler.Instance.UpdateConfigByGroupId(groupId, id, securityId, favoriteTypeEnum);
                //QuoteConfigHandler.Instance.DeleteConfigByGroupId(groupId, true);
                //this.Init(ownUI, groupId, true);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    RefreshWidth();
                }));
            }
        }

        public void AddButton(string content)
        {
            PageButtonList.Add(content);

            RefreshWidth();
        }

        public void DeleteButton(string content)
        {
            pageButtonList.Remove(content);

            int index = 1;
            for (int i = 0; i < pageButtonList.Count; i++)
            {
                pageButtonList[i] = (index++).ToString();
            }

            RefreshWidth();
        }


        private void Instance_OnFavoritesChanged(FavoriteTypeEnum typeEnum, string groupID, string securityID, string bondId)
        {
            return;

            if (currentGroupId == groupID && typeEnum == FavoriteTypeEnum.DelBond)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    TabControl tabControl = GetTabControl();
                    if (tabControl != null)
                    {
                        QuoteConfigPo quoteConfigPo = new QuoteConfigPo();
                        quoteConfigPo.Pages = new List<QcPagePo>();
                        quoteConfigPo.GroupId = groupID;
                        quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
                        quoteConfigPo.Rows = LayoutSetttingCache.Instance.layoutSettingPo.Row;
                        quoteConfigPo.Cols = LayoutSetttingCache.Instance.layoutSettingPo.Col;
                        int index = 0;
                        foreach (TabItem tabItem in tabControl.Items)
                        {
                            PageCardItemView pageCardItem = tabItem.Content as PageCardItemView;
                            PageCardItemViewModel pageCardItemViewModel = pageCardItem.DataContext as PageCardItemViewModel;

                            List<PriceAndOrderViewModel> viewModelList = pageCardItemViewModel.GetPriceAndOrderViewModelList();
                            QcPagePo pagePo = new QcPagePo();
                            pagePo.GroupId = groupID;
                            pagePo.Panels = new List<QcPanelPo>();

                            quoteConfigPo.Pages.Add(pagePo);
                            pagePo.Index = index;
                            if (index == tabControl.SelectedIndex)
                            {
                                pagePo.IsSeleted = true;

                            }
                            index++;

                            foreach (PriceAndOrderViewModel priceAndOrderViewModel in viewModelList)
                            {
                                QcPanelPo panelPo = new QcPanelPo();
                                if (typeEnum == FavoriteTypeEnum.DelBond && priceAndOrderViewModel.GroupId == groupID && priceAndOrderViewModel.PriceModel.Security == securityID)
                                {
                                    panelPo.BondCode = panelPo.Id = string.Empty;
                                }
                                else
                                {
                                    panelPo.BondCode = priceAndOrderViewModel.PriceModel.Security;
                                    panelPo.Id = priceAndOrderViewModel.Id;
                                }
                                pagePo.Panels.Add(panelPo);
                            }

                        }

                        QuoteConfigHandler.Instance.SaveConfig(quoteConfigPo, groupID);
                    }
                }));
            }
        }

    }
}
