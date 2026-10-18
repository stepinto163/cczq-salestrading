using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
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

        /// <summary>
        /// 组合根入口：装配 FICC 网关数据源（批次 8 起 Mock 已移除，全链路真数据源）。
        /// </summary>
        public MainViewModel()
            : this(new ServiceSet())
        {
        }

        /// <summary>服务集装配（四个域服务 + 两个目录共享一个网关连接）。</summary>
        private readonly ServiceSet _services;

        private MainViewModel(ServiceSet services)
            : this(services.Quote, services.Market, services.Mgmt, services.Groups, new OverlayService())
        {
            _services = services;
            // 顶栏/个人中心显示名取网关登录身份
            if (!string.IsNullOrEmpty(services.UserName))
            {
                UserName = services.UserName;
                CurrentUser.Name = services.UserName;
            }
            // 报价录入弹窗的提交服务（组合根装配例外，见 MainViewModel 类注释）
            Modules.MarketCenter.View.QuoteEntryDialog.QuoteService = services.Quote;
            // CRM 联想组件的目录服务（同上）
            Shared.Components.CrmSearchBox.Directory = services.Directory;
            Shared.Services.CrmLazySearch.Directory = services.Directory;
            // 分组管理弹窗的目录服务（同上）
            Modules.MarketCenter.ViewModel.FavBondManagerViewModel.Directory = services.Groups;
            Modules.MarketCenter.ViewModel.FilterGroupManagerViewModel.Directory = services.Groups;
            // 详情面板智能推荐（ownerUserId → 显示名，AdminHandler 用户缓存 + LocalEnv 本人兜底）
            var recUserNames = Gateway.UserNameMap.Build();
            Modules.MarketCenter.ViewModel.BondDetailViewModel.RecommendationService =
                new Gateway.GatewayRecommendationService(services.Requester,
                    id => id != null && recUserNames.TryGetValue(id, out var n) ? n : id);
        }

        private sealed class ServiceSet
        {
            public readonly IQuoteService Quote;
            public readonly IMarketCenterService Market;
            public readonly IQuoteMgmtService Mgmt;
            public readonly IBaseDirectory Directory;
            public readonly IGroupDirectory Groups;
            public readonly Gateway.GatewayRequester Requester;
            public readonly string UserName;

            /// <summary>quote 域共享推送流（Quote/Mgmt 共用；tab 关闭时退订宿主推送）。</summary>
            public readonly Gateway.GatewayReportStream<Gateway.Contracts.View.QuoteStreamBatch> Stream;

            public void Dispose()
            {
                // 三个域服务各持有视图/推送订阅，必须逐一释放（此前只退订了 quote 流，
                // 行情/提醒域的订阅与服务端视图随每次关 tab 泄漏一套）。
                // 报价窗是跨 tab 全局单例且静态引用本服务集：归属本 tab 时随 tab 关闭，
                // 否则窗口会在服务已 Dispose 后继续提交（半死状态）。
                SafeDispose(Stream);
                SafeDispose(Quote);
                SafeDispose(Market);
                SafeDispose(Mgmt);
                Modules.MarketCenter.View.QuoteEntryDialog.DetachServices(Quote);
                Modules.MarketCenter.View.QuoteEntryOverlayWindow.CloseActive();
            }

            private static void SafeDispose(object disposable)
            {
                try { (disposable as IDisposable)?.Dispose(); }
                catch { /* 单个失败不断释放链 */ }
            }

            public ServiceSet()
            {
                // 批次 8：全链路真数据源（Mock 已移除；如需临时恢复，见 git 历史 6ac747569 之前版本）
                var requester = new Gateway.GatewayRequester(new Gateway.ApiManagerGatewayClient());
                Stream = new Gateway.GatewayReportStream<Gateway.Contracts.View.QuoteStreamBatch>(
                    requester, Gateway.GatewayTopics.QuoteStreamOpen);
                var stream = Stream;
                Quote = new Gateway.GatewayQuoteService(requester, stream);
                Market = new Gateway.GatewayMarketCenterService(requester);
                Mgmt = new Gateway.GatewayQuoteMgmtService(requester, stream);
                Directory = new Gateway.GatewayBaseDirectory(requester);
                Groups = new Gateway.GatewayGroupDirectory(requester);
                Requester = requester;
                UserName = requester.CurrentUserName;
            }
        }

        public MainViewModel(IQuoteService quoteService, IMarketCenterService marketCenterService,
            IQuoteMgmtService mgmtService, IGroupDirectory groupDirectory, IOverlayService overlayService)
        {
            _quoteService = quoteService;
            _overlayService = overlayService;
            QuotePage = new QuoteViewModel(quoteService);
            MarketCenterPage = new MarketCenterViewModel(marketCenterService, quoteService, mgmtService, groupDirectory);
            QuoteMgmtPage = new QuoteMgmtViewModel(mgmtService, groupDirectory);

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
            _services.Dispose();            // 先断流源（宿主推送订阅/域流）
            EventAggregator.Unsubscribe(this);
            QuotePage.Dispose();
            MarketCenterPage.Dispose();
            QuoteMgmtPage.Dispose();
        }

        #endregion
    }
}
