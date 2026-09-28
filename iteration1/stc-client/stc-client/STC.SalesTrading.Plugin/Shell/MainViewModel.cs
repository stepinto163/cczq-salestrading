using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Mock;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace STC.SalesTrading.Plugin.Shell
{
    /// <summary>
    /// 应用壳 VM：顶部导航/页面切换/徽标/用户下拉。
    /// 同时是组合根：在此注入三大模块 VM 与服务实现（开发期 Mock，接网关时仅替换实现）。
    /// 跨模块联动统一走 STC.Common.Utils.Events.EventAggregator（消息契约见 Shared/Messages）；
    /// 弹窗展示经 IOverlayService 接缝，本类不直接引用任何 Window/View 类型（组合根装配除外）。
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable,
        IRecipient<PendingQuotesChangedMessage>, IRecipient<AlertCountChangedMessage>, IRecipient<OpenQuoteEntryOverlayMessage>
    {
        #region 构造

        private readonly IQuoteService _quoteService;
        private readonly IOverlayService _overlayService;

        /// <summary>默认复用宿主已登录的 DPS 连接；测试可通过重载构造器注入服务。</summary>
        public MainViewModel()
            : this(new DpsSalesTradingServices())
        {
        }

        private readonly IDisposable ownedServices;
        public DpsSalesTradingServices Backend { get; }

        private MainViewModel(DpsSalesTradingServices services)
            : this(services, services, services, new OverlayService())
        {
            ownedServices = services;
            Backend = services;
            UserName = services.UserName;
        }

        public MainViewModel(IQuoteService quoteService, IMarketCenterService marketCenterService,
            IQuoteMgmtService mgmtService, IOverlayService overlayService)
        {
            _quoteService = quoteService;
            _overlayService = overlayService;
            QuotePage = new QuoteViewModel(quoteService);
            MarketCenterPage = new MarketCenterViewModel(marketCenterService);
            QuoteMgmtPage = new QuoteMgmtViewModel(mgmtService);

            NavItems = new ObservableCollection<NavItem>
            {
                new NavItem("报价录入", QuotePage, "quote"),
                new NavItem("行情中心", MarketCenterPage, "market"),
                new NavItem("报价管理", QuoteMgmtPage, "mgmt"),
            };

            // 原型默认进入行情中心
            _selectedNavItem = NavItems[1];

            // 经 EventAggregator 注册 IRecipient 接口，接收跨模块徽标消息（弱引用，Dispose 时显式退订）
            EventAggregator.RegisterAll(this);
        }

        #endregion

        #region 页面与导航

        public ObservableCollection<NavItem> NavItems { get; }

        [ObservableProperty]
        private NavItem _selectedNavItem;

        public QuoteViewModel QuotePage { get; }

        public MarketCenterViewModel MarketCenterPage { get; }

        public QuoteMgmtViewModel QuoteMgmtPage { get; }

        [RelayCommand]
        private void SwitchPage(NavItem item)
        {
            if (item != null) SelectedNavItem = item;
        }

        #endregion

        #region 徽标

        /// <summary>按 Key 定位导航项（角标唯一数据源为 NavItem.BadgeCount），避免按下标耦合顺序。</summary>
        private NavItem NavByKey(string key) => NavItems.FirstOrDefault(n => n.Key == key) ?? NavItems[0];

        public void Receive(PendingQuotesChangedMessage message)
        {
            NavByKey("quote").BadgeCount = message.PendingCount;
        }

        public void Receive(AlertCountChangedMessage message)
        {
            NavByKey("market").BadgeCount = message.UnreadCount;
        }

        /// <summary>行情中心顶栏「报价录入」→ 打开报价录入模块浮窗（独立新建的报价录入实例）。</summary>
        public void Receive(OpenQuoteEntryOverlayMessage message)
        {
            _overlayService.ShowQuoteEntryOverlay(new QuoteViewModel(_quoteService));
        }

        #endregion

        #region 用户区

        [RelayCommand]
        private void ManualQuote()
        {
            if (Backend == null)
            {
                ToastService.Show("当前数据源不支持真实报价操作。", ToastKind.Warning);
                return;
            }
            new STC.SalesTrading.Plugin.View.QuoteOperationWindow(Backend.Api)
            {
                Owner = System.Windows.Application.Current.MainWindow
            }.ShowDialog();
        }

        /// <summary>顶栏显示的当前用户名。</summary>
        [ObservableProperty]
        private string _userName = "admin";

        /// <summary>当前登录用户资料（开发期 Mock，接网关后由鉴权上下文填充）。</summary>
        public UserProfile CurrentUser { get; } = new UserProfile();

        [RelayCommand]
        private void ShowProfile()
        {
            _overlayService.ShowUserCenter(CurrentUser);
        }

        [RelayCommand]
        private void ShowSettings()
        {
            _overlayService.ShowSettings();
        }

        [RelayCommand]
        private void Logout()
        {
            ToastService.Show("已退出登录（Mock，接入网关后跳转登录页）", ToastKind.Info);
        }

        #endregion

        #region 释放

        public void Dispose()
        {
            EventAggregator.Unsubscribe(this);
            QuotePage.Dispose();
            MarketCenterPage.Dispose();
            QuoteMgmtPage.Dispose();
            ownedServices?.Dispose();
        }

        #endregion
    }
}
