using STC.Common.Model.Fix.Message;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Frame.CustomUI.Component;
using STC.Frame.Utils;
using STC.Rfq.Plugin.Utils;
using STC.Rfq.Plugin.Utils.Data;
using STC.Rfq.Plugin.View.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace STC.Rfq.Plugin.View.HomePage
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
        }



        internal Grid _MainGrid;
        public double mainGridWidth;

        internal HomePageTabPanel HeaderPanel { get; private set; }

        private CustomScrollViewer _scrollViewerOverflow;

        private ButtonBase _buttonScrollLeft;

        private ButtonBase _buttonScrollRight;

        private Border _headerBorder;


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
        private async void AddGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            // 刷新缓存数据, 并添加新的标签页
            IOrderedEnumerable<RfqGroupInfo> sortedGroups = await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
            if (sortedGroups != null && sortedGroups.Count() == 20)
            {
                CustomMessageBox.Info("分组总数应不超过20个", "添加分组");
                return;
            }
            else
            {
                AddGroupWindow addGroupWindow = new AddGroupWindow();
                addGroupWindow.Owner = Application.Current.MainWindow;
                addGroupWindow.ShowDialog();
                if (addGroupWindow.Result == true)
                {
                    try
                    {
                        // 取缓存的最后一条数据，算顺序
                        int ordersn = 1;
                        if (sortedGroups != null && sortedGroups.Count() > 0)
                        {
                            ordersn = sortedGroups.Last().ordersn + 1;
                        }

                        string groupname = addGroupWindow.GroupName;
                        if (string.IsNullOrEmpty(groupname) == false)
                        {
                            RfqGroupReq request = new RfqGroupReq();
                            request.groupname = groupname;//组名
                            request.action = "1";//新增
                            request.ordersn = ordersn.ToString();//顺序
                            request.terminal = LocalEnv.ServerInfo.Version; // 终端
                            PResult pResult = await Utils.Data.RfqGroupHandler.Instance.AsyncUpdateGroups(request);
                            if (pResult != null && pResult.Code == "0000")
                            {
                                sortedGroups = await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
                                if (sortedGroups != null)
                                {
                                    RfqGroupInfo gi = sortedGroups.Last();
                                    Utils.Data.RfqGroupHandler.Instance.Notify(Utils.Data.RfqGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, groupname);
                                    AddGroupTabItem(gi, true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(ex);
                    }
                }
            }
        }

        public void AddGroupTabItem(RfqGroupInfo gi, bool isSelected)
        {
            if (this.CheckAccess())
            {
                Sub_AddGroupTabItem(gi, isSelected);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,(Action)(() =>
                {
                    Sub_AddGroupTabItem(gi, isSelected);
                }));
            }
        }

        private void Sub_AddGroupTabItem(RfqGroupInfo gi, bool isSelected)
        {
            foreach (HomePageTabItemWithClose subItme in this.Items)
            {
                if (subItme.Header.ToString() == gi.groupname)
                {
                    return;
                }
            }

            HomePageTabItemWithClose tabItem = new HomePageTabItemWithClose();
            tabItem.TabItemClosedFunc = TabItem_TabItemClosed;
            tabItem.TabItemUpdateAct = UpdateGroups;
            tabItem.TabItemOnHeaderChangeAct = OnHeaderChanged;
            tabItem.Tag = gi;
            tabItem.Header = gi.groupname;
            this.Items.Add(tabItem);
            
            if (isSelected == true)
            {
                tabItem.IsSelected = true;
            }
            
        }

        public void DelGroupTabItem(string groupname)
        {
            if (this.CheckAccess())
            {
                Sub_DelGroupTabItem(groupname);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, (Action)(() =>
                {
                    Sub_DelGroupTabItem(groupname);
                }));
            }
        }

        private void Sub_DelGroupTabItem(string groupname)
        {
            HomePageTabItemWithClose tabItem = null;
            foreach (HomePageTabItemWithClose subItme in this.Items)
            {
                if (subItme.Header.ToString() == groupname)
                {
                    tabItem = subItme;
                }
            }
            if (tabItem != null)
                this.Items.Remove(tabItem);
        }

        private async void UpdateGroupName(HomePageTabItemWithClose currentTabItem, string newGroupName)
        {

            if (currentTabItem != null && currentTabItem.Tag != null && currentTabItem.Tag is RfqGroupInfo)
            {
                RfqGroupInfo gi = currentTabItem.Tag as RfqGroupInfo;
                if (gi.groupname == newGroupName)
                {
                    return;
                }

                RfqGroupReq request = new RfqGroupReq();
                request.id = gi.id;
                request.groupname = newGroupName;//组名
                request.action = "2";//更新
                request.ordersn = gi.ordersn.ToString();//顺序
                PResult pResult = await Utils.Data.RfqGroupHandler.Instance.AsyncUpdateGroups(request);
                if (pResult != null && pResult.Code == "0000")
                {
                    gi.action = 2;
                    gi.groupname = newGroupName;
                    await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
                }
                else
                {
                    currentTabItem.Header = gi.groupname;
                }
            }
        }

        private async void UpdateGroups()
        {
            List<RfqGroupReq> tempList = new List<RfqGroupReq>();
            IList list = GetActualList();
            if (list != null)
            {
                foreach (var item in list)
                {
                    RfqGroupReq gi = ((HomePageTabItemWithClose)item).Tag as RfqGroupReq;
                    tempList.Add(gi);
                }
            }

            try
            {
                if (tempList != null)
                {
                    int index = 0;
                    foreach (RfqGroupReq gi in tempList)
                    {
                        RfqGroupReq request = new RfqGroupReq();
                        request.id = gi.id;
                        request.groupname = gi.groupname;//组名
                        request.action = "2";//更新
                        request.ordersn = index.ToString();//顺序
                        PResult pResult = await Utils.Data.RfqGroupHandler.Instance.AsyncUpdateGroups(request);
                        if (pResult != null && pResult.Code == "0000")
                        {
                            gi.action = "2";
                            gi.ordersn = index.ToString();
                            await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
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

        private async Task<bool> TabItem_TabItemClosed(HomePageTabItemWithClose sender)
        {
            if (sender != null)
            {
                if (sender.Tag != null && sender.Tag is RfqGroupInfo)
                {
                    RfqGroupInfo gi = sender.Tag as RfqGroupInfo;
                    IOrderedEnumerable<RfqGroupInfo> dic = await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();

                    if (string.IsNullOrEmpty(gi.id) == false)
                    {
                        lock (string.Intern(gi.id))
                        {
                            if (string.IsNullOrEmpty(gi.id) == false)
                            {
                                lock (string.Intern(gi.id))
                                {
                                    List<RfqstgViewModel> mmlist = Utils.Data.RfqGroupHandler.Instance.GetMMsByGroupID(gi.id);
                                    if (mmlist != null)
                                    {
                                        foreach (RfqstgViewModel mm in mmlist)
                                        {
                                            if ("1" == mm.enable)
                                            {
                                                CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再删除");
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                   

                    string title = "删除分组";
                    string text = "确定要删除分组" + gi.groupname + "吗? 组内做市策略也将会被删除";//"是否继续删除?";
                    //if (MMGroupInfo != null && MMGroupInfo.Datas.Count > 0)
                    //{
                    //     text = "该分组下有债券信息, 如果删除该分组\r\n将同步删除已维护债券信息, \r\n请问是否继续?";
                    //}
                    if (CustomMessageBox.Question(text, title) == false)
                    {
                        return false;
                    }
                    RfqGroupReq request = new RfqGroupReq();
                    request.id = gi.id;//组名
                    request.action = "3";//删除
                    PResult pResult = await Utils.Data.RfqGroupHandler.Instance.AsyncUpdateGroups(request);
                    if (pResult != null)
                    {
                        if (pResult.IsSuccess())
                        {
                            await Utils.Data.RfqGroupHandler.Instance.AsyncRequestGroups();
                            Utils.Data.RfqGroupHandler.Instance.Notify(Utils.Data.RfqGroupHandler.FavoriteTypeEnum.DelGroup, gi.id, gi.groupname);
                            return true;
                        }
                        else
                        {
                            CustomMessageBox.Warning(pResult.Msg);
                            return false;
                        }
                    }
                    
                    return true;
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
