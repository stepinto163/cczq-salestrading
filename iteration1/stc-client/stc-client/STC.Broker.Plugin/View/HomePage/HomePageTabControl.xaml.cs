using STC.Broker.Plugin.Handler;
using STC.Broker.Plugin.Model.Card;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.Price;
using STC.Broker.Plugin.ViewModel.Price;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace STC.Broker.Plugin.View.HomePage
{
    /// <summary>
    [TemplatePart(Name = HeaderPanelKey, Type = typeof(HomePageTabPanel))]
    [TemplatePart(Name = OverflowScrollviewer, Type = typeof(CustomScrollViewer))]
    [TemplatePart(Name = ScrollButtonLeft, Type = typeof(RepeatButton))]
    [TemplatePart(Name = ScrollButtonRight, Type = typeof(RepeatButton))]
    [TemplatePart(Name = HeaderBorder, Type = typeof(Border))]
    public partial class HomePageTabControl : TabControl
    {
        private const string MainGridKey = "mainGrid";

        private const string HeaderPanelKey = "PART_HeaderPanel";

        private const string OverflowScrollviewer = "PART_OverflowScrollviewer";

        private const string ScrollButtonLeft = "PART_ScrollButtonLeft";

        private const string ScrollButtonRight = "PART_ScrollButtonRight";

        private const string HeaderBorder = "PART_HeaderBorder";

        public HomePageTabControl()
        {
            InitializeComponent();
            this.Unloaded += HomePageTabControl_Unloaded;
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
        }

        private void HomePageTabControl_Unloaded(object sender, RoutedEventArgs e)
        {
            HomePageDataHandler.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;
        }

        

        internal Grid _MainGrid;
        public double mainGridWidth;

        internal HomePageTabPanel HeaderPanel { get; private set; }

        private CustomScrollViewer _scrollViewerOverflow;

        private ButtonBase _buttonScrollLeft;

        private ButtonBase _buttonScrollRight;

        private Border _headerBorder;

        private HomePageView homepageview;

        /// <summary>
        ///     是否为内部操作
        /// </summary>
        internal bool IsInternalAction;

        /// <summary>
        ///     是否启用动画
        /// </summary>
        public static readonly DependencyProperty IsAnimationEnabledProperty = DependencyProperty.Register(
            "IsAnimationEnabled", typeof(bool), typeof(HomePageTabControl), new PropertyMetadata(false));

        /// <summary>
        ///     是否启用动画
        /// </summary>
        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }

        /// <summary>
        ///     是否可以拖动
        /// </summary>
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register(
            "IsDraggable", typeof(bool), typeof(HomePageTabControl), new PropertyMetadata(false));

        /// <summary>
        ///     是否可以拖动
        /// </summary>
        public bool IsDraggable
        {
            get { return (bool)GetValue(IsDraggableProperty); }
            set { SetValue(IsDraggableProperty, value); }
        }

        /// <summary>
        ///     是否显示关闭按钮
        /// </summary>
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.RegisterAttached(
            "ShowCloseButton", typeof(bool), typeof(HomePageTabControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetShowCloseButton(DependencyObject element, bool value)
            => element.SetValue(ShowCloseButtonProperty, value);

        public static bool GetShowCloseButton(DependencyObject element)
            => (bool)element.GetValue(ShowCloseButtonProperty);

        /// <summary>
        ///     是否显示关闭按钮
        /// </summary>
        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, value); }
        }

        /// <summary>
        ///     是否将标签填充
        /// </summary>
        public static readonly DependencyProperty IsTabFillEnabledProperty = DependencyProperty.Register(
            "IsTabFillEnabled", typeof(bool), typeof(HomePageTabControl), new PropertyMetadata(false));

        /// <summary>
        ///     是否将标签填充
        /// </summary>
        public bool IsTabFillEnabled
        {
            get { return (bool)GetValue(IsTabFillEnabledProperty); }
            set { SetValue(IsTabFillEnabledProperty, value); }
        }

        /// <summary>
        ///     标签宽度
        /// </summary>
        public static readonly DependencyProperty TabItemWidthProperty = DependencyProperty.Register(
            "TabItemWidth", typeof(double), typeof(HomePageTabControl), new PropertyMetadata(100.0));

        /// <summary>
        ///     标签宽度
        /// </summary>
        public double TabItemWidth
        {
            get { return (double)GetValue(TabItemWidthProperty); }
            set { SetValue(TabItemWidthProperty, value); }
        }

        /// <summary>
        ///     标签高度
        /// </summary>
        public static readonly DependencyProperty TabItemHeightProperty = DependencyProperty.Register(
            "TabItemHeight", typeof(double), typeof(HomePageTabControl), new PropertyMetadata(24.0));

        /// <summary>
        ///     标签高度
        /// </summary>
        public double TabItemHeight
        {
            get { return (double)GetValue(TabItemHeightProperty); }
            set { SetValue(TabItemHeightProperty, value); }
        }

        /// <summary>
        ///     是否可以滚动
        /// </summary>
        public static readonly DependencyProperty IsScrollableProperty = DependencyProperty.Register(
            "IsScrollable", typeof(bool), typeof(HomePageTabControl), new PropertyMetadata(true));

        /// <summary>
        ///     是否可以滚动
        /// </summary>
        public bool IsScrollable
        {
            get { return (bool)GetValue(IsScrollableProperty); }
            set { SetValue(IsScrollableProperty, value); }
        }


        /// <summary>
        ///     是否显示滚动按钮
        /// </summary>
        public static readonly DependencyProperty ShowScrollButtonProperty = DependencyProperty.Register(
            "ShowScrollButton", typeof(bool), typeof(HomePageTabControl), new PropertyMetadata(false));

        /// <summary>
        ///     是否显示滚动按钮
        /// </summary>
        public bool ShowScrollButton
        {
            get { return (bool)GetValue(ShowScrollButtonProperty); }
            set { SetValue(ShowScrollButtonProperty, value); }
        }

        /// <summary>
        ///     可见的标签数量
        /// </summary>
        private int _itemShowCount;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.CalculateScrollViewerWidth();

            if (HeaderPanel == null)
            {
                IsInternalAction = false;
                return;
            }

            //UpdateOverflowButton();

            if (IsInternalAction)
            {
                IsInternalAction = false;
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (!(ItemContainerGenerator.ContainerFromIndex(i) is HomePageTabItemWithClose)) return;
                    HomePageTabItemWithClose item = ItemContainerGenerator.ContainerFromIndex(i) as HomePageTabItemWithClose;
                    item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    item.TabPanel = HeaderPanel;
                }
            }

            _headerBorder?.InvalidateMeasure();
            IsInternalAction = false;
        }

        public override void OnApplyTemplate()
        {
            //if (_buttonOverflow != null)
            //{
            //    if (_buttonOverflow.Menu != null)
            //    {
            //        _buttonOverflow.Menu.Closed -= Menu_Closed;
            //        _buttonOverflow.Menu = null;
            //    }

            //    _buttonOverflow.Click -= ButtonOverflow_Click;
            //}
            //DefaultStyleKey = typeof(TabControl);
            if (_buttonScrollLeft != null) _buttonScrollLeft.Click -= ButtonScrollLeft_Click;
            if (_buttonScrollRight != null) _buttonScrollRight.Click -= ButtonScrollRight_Click;

            base.OnApplyTemplate();
            HeaderPanel = GetTemplateChild(HeaderPanelKey) as HomePageTabPanel;
            _MainGrid = GetTemplateChild(MainGridKey) as Grid;

            if (IsTabFillEnabled) return;

            //_buttonOverflow = GetTemplateChild(OverflowButtonKey) as ContextMenuToggleButton;
            _scrollViewerOverflow = GetTemplateChild(OverflowScrollviewer) as CustomScrollViewer;
            _buttonScrollLeft = GetTemplateChild(ScrollButtonLeft) as ButtonBase;
            _buttonScrollRight = GetTemplateChild(ScrollButtonRight) as ButtonBase;
            _headerBorder = GetTemplateChild(HeaderBorder) as Border;

            if (_buttonScrollLeft != null) _buttonScrollLeft.Click += ButtonScrollLeft_Click;
            if (_buttonScrollRight != null) _buttonScrollRight.Click += ButtonScrollRight_Click;

            //if (_buttonOverflow != null)
            //{
            //    var menu = new ContextMenu
            //    {
            //        Placement = PlacementMode.Bottom,
            //        PlacementTarget = _buttonOverflow
            //    };
            //    menu.Closed += Menu_Closed;
            //    _buttonOverflow.Menu = menu;
            //    _buttonOverflow.Click += ButtonOverflow_Click;
            //}
        }

        private void ButtonScrollRight_Click(object sender, RoutedEventArgs e)
        {

            _scrollViewerOverflow.ScrollToHorizontalOffsetInternal(Math.Min(
                _scrollViewerOverflow.CurrentHorizontalOffset + TabItemWidth, _scrollViewerOverflow.ScrollableWidth));
        }

        private void ButtonScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            _scrollViewerOverflow.ScrollToHorizontalOffsetInternal(Math.Max(
                _scrollViewerOverflow.CurrentHorizontalOffset - TabItemWidth, 0));
        }

        internal double GetHorizontalOffset() => _scrollViewerOverflow?.CurrentHorizontalOffset ?? 0;

        internal void UpdateScroll() => _scrollViewerOverflow?.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 0)
        {
            RoutedEvent = MouseWheelEvent
        });

        internal IList GetActualList()
        {
            IList list;
            if (ItemsSource != null)
            {
                list = ItemsSource as IList;
            }
            else
            {
                list = Items;
            }

            return list;
        }

        protected override bool IsItemItsOwnContainerOverride(object item) => item is HomePageTabItemWithClose;

        protected override DependencyObject GetContainerForItemOverride() => new HomePageTabItemWithClose();


        /// <summary>
        /// 增加组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result == true)
            {
                HomePageDataHandler.AddGroup(addGroupWindow.GroupName);
            }
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securtityID)
        {
            if (favoriteTypeEnum == HomePageDataHandler.FavoriteTypeEnum.AddGroup)
            {
                // 刷新缓存数据, 并添加新的标签页
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = HomePageDataHandler.RequestGroups(false);
                if (sortedGroups != null)
                {
                    HomeGroupInfo gi = sortedGroups.Last().Value;
                    AddGroupTabItem(gi, true, homepageview);
                }
            }
            else if (favoriteTypeEnum == HomePageDataHandler.FavoriteTypeEnum.DelGroup)
            {
                //如果删除收藏夹
                foreach (HomePageTabItemWithClose tabItem in this.Items)
                {
                    if (tabItem.Tag.ToString() == groupID)
                    {
                        this.Items.Remove(tabItem);
                        break;
                    }
                }

            }
        }

        public void AddGroupTabItem(HomeGroupInfo gi, bool isSelected, HomePageView hpv)
        {
            this.homepageview = hpv;
            Sub_AddGroupTabItem(gi, isSelected, hpv);
        }

        public void Sub_AddGroupTabItem(HomeGroupInfo gi, bool isSelected, HomePageView hpv)
        {
            this.homepageview = hpv;
            HomePageTabItemWithClose tabItem = new HomePageTabItemWithClose();
            tabItem.TabItemClosedFunc = TabItem_TabItemClosed;
            tabItem.TabItemUpdateAct = UpdateGroups;
            tabItem.TabItemOnHeaderChangeAct = OnHeaderChanged;
            tabItem.Tag = gi.id;
            tabItem.Header = gi.groupname;
            this.Items.Add(tabItem);
            if (isSelected == true)
            {
                tabItem.IsSelected = true;
            }

            ThreadPoolHelper.InvokeThreadPool((P) =>
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (Action)(() =>
                {
                    PricingPanel listView = new PricingPanel();
                    //首先查询配置
                    QuotePageModel CurQuoteConfigPo = CardInfoHandler.Instance.QueryQuotePageConfig(gi.id);

                    if (CurQuoteConfigPo == null)
                    {
                        CurQuoteConfigPo = CardInfoHandler.Instance.ResetQuote(gi.id, gi.groupname, gi.securityids);
                    }

                    List<int> PageList = CurQuoteConfigPo.Panels.Select(p => p.Page).Distinct().ToList();


                    for (int p = 0; p < PageList.Count; p++)
                    {
                        ListViewControl lst = new ListViewControl();
                        TabItem Ti = new TabItem();
                        Ti.Header = p;
                        Ti.Height = 0;
                        Ti.Content = lst;
                        Ti.Tag = p.ToString();
                        listView.Tab_DgPanel.Items.Add(Ti);
                    }

                    tabItem.Content = listView;

                    listView.Init(gi, hpv);


                    hpv.positionloadMask.Visibility = Visibility.Collapsed;
                }));
            });
        }

        public void Sub_AddGroupTabItem(QuotePageModel CurQuoteConfigPo, bool isSelected, HomePageView hpv)
        {
            this.homepageview = hpv;
            HomePageTabItemWithClose tabItem = new HomePageTabItemWithClose();
            tabItem.TabItemClosedFunc = TabItem_TabItemClosed;
            tabItem.TabItemUpdateAct = UpdateGroups;
            tabItem.TabItemOnHeaderChangeAct = OnHeaderChanged;
            tabItem.Tag = CurQuoteConfigPo.GroupId;
            tabItem.Header = CurQuoteConfigPo.GroupName;
            this.Items.Add(tabItem);
            if (isSelected == true)
            {
                tabItem.IsSelected = true;
            }

            ThreadPoolHelper.InvokeThreadPool((P) =>
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (Action)(() =>
                {
                    PricingPanel listView = new PricingPanel();
                    //首先查询配置
                    List<int> PageList = CurQuoteConfigPo.Panels.Select(p => p.Page).Distinct().ToList();

                    for (int p = 0; p < PageList.Count; p++)
                    {
                        ListViewControl lst = new ListViewControl();
                        TabItem Ti = new TabItem();
                        Ti.Header = p;
                        Ti.Height = 0;
                        Ti.Content = lst;
                        Ti.Tag = p.ToString();
                        listView.Tab_DgPanel.Items.Add(Ti);
                    }

                    tabItem.Content = listView;
                    List<HomeGroupSecurityIDsInfo> hgsi = new List<HomeGroupSecurityIDsInfo>();

                    foreach (QuoteGroupConfigModel qgcm in CurQuoteConfigPo.Panels)
                    {
                        hgsi.Add(new HomeGroupSecurityIDsInfo() { groupId = CurQuoteConfigPo.GroupId, securityid = qgcm.SecurityID, symbol = qgcm.SymbolName, settletype = qgcm.SettleIndex, id = qgcm.ParentId });
                    }

                    listView.Init(new HomeGroupInfo() { id = CurQuoteConfigPo.GroupId, groupname= CurQuoteConfigPo.GroupName, username= CurQuoteConfigPo.UserName, securityids = hgsi}, hpv);

                    hpv.positionloadMask.Visibility = Visibility.Collapsed;
                }));
            });
        }

        private async void UpdateGroupName(HomePageTabItemWithClose currentTabItem, string newGroupName)
        {

            if (currentTabItem != null && currentTabItem.Tag != null)
            {
                IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.RequestGroups(false);
                HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, currentTabItem.Tag.ToString());
                if (homeGroupInfo == null)
                {
                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, currentTabItem.Tag.ToString(), currentTabItem.Header.ToString());
                    return;
                }

                if (homeGroupInfo.groupname == newGroupName)
                    return;

                HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                request.id = currentTabItem.Tag.ToString();
                request.groupname = newGroupName;//组名
                request.action = "2";//更新
                request.ordersn = homeGroupInfo.ordersn.ToString();
                request.username = homeGroupInfo.username;
                request.closeby = LocalEnv.UserInfo.UserName;
                PResult pResult = await HomePageDataHandler.AsyncUpdateGroups(request);
                if (pResult != null && pResult.Code == "0000")
                {
                   
                }

                HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.UpdateGroup, currentTabItem.Tag.ToString(), currentTabItem.Header.ToString());
            }
        }

        private async void UpdateGroups()
        {
            List<HomeGroupInfo> tempList = new List<HomeGroupInfo>();
            IList list = GetActualList();
            if (list != null)
            {
                foreach (var item in list)
                {
                    HomeGroupInfo gi = ((HomePageTabItemWithClose)item).Tag as HomeGroupInfo;
                    tempList.Add(gi);
                }
            }

            try
            {
                if (tempList != null)
                {
                    int index = 0;
                    foreach (HomeGroupInfo gi in tempList)
                    {
                        HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                        request.id = gi.id;
                        request.groupname = gi.groupname;//组名
                        request.action = "2";//更新
                        request.ordersn = index.ToString();//顺序
                        request.username = gi.username;
                        request.closeby = LocalEnv.UserInfo.UserName;
                        PResult pResult = await HomePageDataHandler.AsyncUpdateGroups(request);
                        if (pResult != null && pResult.Code == "0000")
                        {
                            gi.action = 2;
                            gi.ordersn = index;
                        }
                        index++;
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void OnHeaderChanged(HomePageTabItemWithClose sender, object newHeader)
        {
            if (sender != null && newHeader != null)
            {
                UpdateGroupName(sender, newHeader.ToString());
            }
        }

        private bool TabItem_TabItemClosed(HomePageTabItemWithClose sender)
        {
            if (sender != null)
            {
                if (sender.Tag != null)
                {
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.RequestGroups(false);
                    HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, sender.Tag.ToString());

                    string title = "删除分组";
                    string text = "确定要删除标签页" + (sender as HomePageTabItemWithClose).Header.ToString() + "吗?";//"是否继续删除?";
                    if (homeGroupInfo != null && homeGroupInfo.securityids.Count > 0)
                    {
                         text = "该分组下有债券信息, 如果删除该分组\r\n将同步删除已维护债券信息, \r\n请问是否继续?";
                    }
                    if (Common.View.Component.CustomMessageBox.Question(text, title) == false)
                    {
                        return false;
                    }
                    HomeGroupUpdateReq request = new HomeGroupUpdateReq();
                    request.id = sender.Tag.ToString();//组名
                    request.action = "3";//删除
                    request.username = homeGroupInfo.username;
                    PResult pResult = HomePageDataHandler.UpdateGroups(request);
                    if (pResult != null && pResult.Code == "0000")
                    {
                        HomePageDataHandler.RequestGroups(true);
                        HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.DelGroup, sender.Tag.ToString(), sender.Header.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScrollViewerWidth();
        }

        private void CalculateScrollViewerWidth()
        {
            if (_MainGrid != null)
            {
                double scrollViewerWidth = _MainGrid.ActualWidth - 32 - 68;// - (_scrollViewerOverflow.ScrollableWidth > 0 ? 11.2 * 2 : 0);
                double needItemsWidth = this.Items.Count * TabItemWidth;

                _scrollViewerOverflow.Width = Math.Min(scrollViewerWidth, needItemsWidth);
            }
        }
    }
}
