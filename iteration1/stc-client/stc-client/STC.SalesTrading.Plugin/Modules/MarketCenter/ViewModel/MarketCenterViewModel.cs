using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.MarketCenter.Model;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using STC.SalesTrading.Plugin.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 行情中心容器 VM：左筛选侧栏 + 6 子页 + 右债券详情。
    /// 选中行联动：各看板 VM 的 SelectedRow 变化 → 映射基础字段 → 服务异步回填多档/成交。
    /// </summary>
    public partial class MarketCenterViewModel : ObservableObject, IDisposable, IProductFilterHost
    {
        private readonly IMarketCenterService _marketCenterService;

        /// <summary>关注债券/筛选分组弹窗的识别回填取数入口（面板 code-behind 经此传入弹窗）。</summary>
        public System.Threading.Tasks.Task<STC.SalesTrading.Plugin.Ficc.Protocol.DictionaryData> GetFilterDictionaryAsync()
        {
            if (_marketCenterService is IProductFilterCatalog catalog) return catalog.GetFilterDictionaryAsync();
            throw new InvalidOperationException("当前行情服务未提供筛选字典。");
        }

        public void ApplyProductFilter(string scope, STC.SalesTrading.Plugin.Ficc.Protocol.Filter filter)
        {
            if (scope == "Credit")
            {
                CreditAll.ApplyProductFilter(filter);
                CreditBest.ApplyProductFilter(filter);
            }
            else if (scope == "Rate")
            {
                RateAll.ApplyProductFilter(filter);
                RateBest.ApplyProductFilter(filter);
            }
            else Alert.ApplyProductFilter(filter);
        }

        public IMarketCenterService MarketCenterService => _marketCenterService;

        public MarketCenterViewModel(IMarketCenterService marketCenterService)
        {
            _marketCenterService = marketCenterService;

            CreditAll = new BoardListViewModel(marketCenterService, BondMarket.Credit);
            CreditBest = new BoardBestViewModel(marketCenterService, BondMarket.Credit);
            RateAll = new BoardListViewModel(marketCenterService, BondMarket.Rate);
            RateBest = new BoardBestViewModel(marketCenterService, BondMarket.Rate);
            Intent = new BoardIntentViewModel(marketCenterService);
            Alert = new BoardAlertViewModel(marketCenterService);
            Detail = BondDetailViewModel.Empty;

            SubTabs = new ObservableCollection<SubTabItem>
            {
                new SubTabItem("信用全部", CreditAll, "creditAll"),
                new SubTabItem("信用最优", CreditBest, "creditBest"),
                new SubTabItem("利率全部", RateAll, "rateAll"),
                new SubTabItem("利率最优", RateBest, "rateBest"),
                new SubTabItem("模糊报价", Intent, "intent"),
                new SubTabItem("报价提醒", Alert, "alert"),
            };

            // 选中行 → 详情面板联动
            HookSelectedRow(CreditAll);
            HookSelectedRow(CreditBest);
            HookSelectedRow(RateAll);
            HookSelectedRow(RateBest);
            HookSelectedRow(Alert);

            // 子页角标：报价提醒 = 未读提醒数（原型 .subtab .cnt），与导航角标同源
            EventAggregator.Subscribe<AlertCountChangedMessage>(this, OnAlertCountChanged);

            // 最优页同档浮层点击本人报价 → 跳到对应市场「全部」并选中（原型 openBestQuoteInAll）
            EventAggregator.Subscribe<LocateQuoteInAllMessage>(this, OnLocateQuoteInAll);

            // 首次加载可能与订阅同帧完成，这里补一次初值，避免角标缺失
            var alertTab = SubTabs.FirstOrDefault(t => t.Key == "alert");
            if (alertTab != null)
            {
                alertTab.BadgeCount = Alert.Rows.Count;
            }

            _selectedSubTab = SubTabs[0];
            _selectedSubPage = CreditAll;
            _filterPanel = MarketFilterPanelType.Credit;

            // 侧栏分组 chip（原型 _updateSidebarFavBonds/_updateSidebarFilterGroups）：
            // 数据变化（管理弹窗保存/右键建组）重建，激活集变化回填选中态
            RebuildSidebarChips();
            FilterProfileService.DataChanged += OnGroupDataChanged;
            MarketGroupSelectionService.ActiveGroupsChanged += OnActiveGroupsChanged;

            // 宿主行情订阅（PriceHandler/LastExecution 通道）：登录态可用时选中行即切订阅，
            // 快照/成交/主数据回调灌入右侧详情面板；无会话时 IsLive=false 自动走 Mock 兜底
            _quoteFeed = new HostQuoteFeed();
            if (_quoteFeed.IsLive)
            {
                _quoteFeed.Snapshot += OnFeedSnapshot;
                _quoteFeed.LastExecution += OnFeedLastExec;
                _quoteFeed.BondInfo += OnFeedBondInfo;
            }
        }

        // ============ 侧栏分组 chip（批次 A：关注分组闭环） ============

        /// <summary>信用面板 - 关注债券分组 chip。</summary>
        public ObservableCollection<MarketGroupChip> CreditFavChips { get; } = new ObservableCollection<MarketGroupChip>();

        /// <summary>信用面板 - 筛选分组 chip。</summary>
        public ObservableCollection<MarketGroupChip> CreditFilterChips { get; } = new ObservableCollection<MarketGroupChip>();

        /// <summary>利率面板 - 关注债券分组 chip。</summary>
        public ObservableCollection<MarketGroupChip> RateFavChips { get; } = new ObservableCollection<MarketGroupChip>();

        /// <summary>利率面板 - 筛选分组 chip。</summary>
        public ObservableCollection<MarketGroupChip> RateFilterChips { get; } = new ObservableCollection<MarketGroupChip>();

        /// <summary>提醒合并面板 - 关注债券分组 chip（信用+利率，原型 merged 侧栏口径）。</summary>
        public ObservableCollection<MarketGroupChip> MergedFavChips { get; } = new ObservableCollection<MarketGroupChip>();

        /// <summary>提醒合并面板 - 筛选分组 chip（信用+利率）。</summary>
        public ObservableCollection<MarketGroupChip> MergedFilterChips { get; } = new ObservableCollection<MarketGroupChip>();

        private void OnGroupDataChanged()
        {
            RebuildSidebarChips();
        }

        private void OnActiveGroupsChanged()
        {
            RefreshChipActiveStates();
        }

        private void RebuildSidebarChips()
        {
            RebuildChips(CreditFavChips, CreditFilterChips, FilterProfileService.MarketKind.Credit);
            RebuildChips(RateFavChips, RateFilterChips, FilterProfileService.MarketKind.Rate);

            MergedFavChips.Clear();
            MergedFilterChips.Clear();
            foreach (var chip in CreditFavChips) MergedFavChips.Add(chip);
            foreach (var chip in RateFavChips) MergedFavChips.Add(chip);
            foreach (var chip in CreditFilterChips) MergedFilterChips.Add(chip);
            foreach (var chip in RateFilterChips) MergedFilterChips.Add(chip);
            RefreshChipActiveStates();
        }

        private static void RebuildChips(ObservableCollection<MarketGroupChip> favTarget, ObservableCollection<MarketGroupChip> filterTarget, FilterProfileService.MarketKind market)
        {
            favTarget.Clear();
            foreach (var group in FilterProfileService.GetFavGroups(market))
            {
                // 原型 renderMarketGroupTags 过滤「默认」空分组
                if (group.Name == "默认" && group.Bonds.Count == 0) continue;
                favTarget.Add(new MarketGroupChip(group));
            }

            filterTarget.Clear();
            foreach (var group in FilterProfileService.GetFilterGroups(market))
            {
                if (group.Name == "默认" && group.ConditionCount == 0) continue;
                filterTarget.Add(new MarketGroupChip(group));
            }
        }

        private void RefreshChipActiveStates()
        {
            RefreshActive(CreditFavChips);
            RefreshActive(CreditFilterChips);
            RefreshActive(RateFavChips);
            RefreshActive(RateFilterChips);
            RefreshActive(MergedFavChips);
            RefreshActive(MergedFilterChips);
        }

        private static void RefreshActive(IEnumerable<MarketGroupChip> chips)
        {
            foreach (var chip in chips)
            {
                chip.RefreshActive();
            }
        }

        /// <summary>报价提醒子页角标（按 Key 定位，避免依赖子页顺序）。</summary>
        private void OnAlertCountChanged(AlertCountChangedMessage message)
        {
            var tab = SubTabs.FirstOrDefault(t => t.Key == "alert");
            if (tab != null)
            {
                tab.BadgeCount = message.UnreadCount;
            }
        }

        /// <summary>最优页同档浮层点击本人报价 → 切到对应市场「全部」子页并选中该行（原型 openBestQuoteInAll）。</summary>
        private void OnLocateQuoteInAll(LocateQuoteInAllMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.QuoteId))
            {
                return;
            }

            var isCredit = message.Market == BondMarket.Credit;
            var tab = SubTabs.FirstOrDefault(t => t.Key == (isCredit ? "creditAll" : "rateAll"));
            if (tab != null)
            {
                SelectedSubTab = tab;
            }

            var board = isCredit ? (BoardListViewModel)CreditAll : RateAll;
            var row = board.Rows.FirstOrDefault(r => r.QuoteId == message.QuoteId);
            if (row != null)
            {
                board.SelectedRow = row;
            }
        }

        public BoardListViewModel CreditAll { get; }

        public BoardBestViewModel CreditBest { get; }

        public BoardListViewModel RateAll { get; }

        public BoardBestViewModel RateBest { get; }

        public BoardIntentViewModel Intent { get; }

        public BoardAlertViewModel Alert { get; }

        /// <summary>宿主行情订阅通道（登录态下为真实行情；未登录为哑通道）。</summary>
        private HostQuoteFeed _quoteFeed;

        /// <summary>右侧债券详情（选中行驱动）。</summary>
        [ObservableProperty]
        private BondDetailViewModel _detail;

        /// <summary>左侧筛选面板（按子页切换，原型 filters-credit/rate/intent/merged）。</summary>
        [ObservableProperty]
        private MarketFilterPanelType _filterPanel;

        /// <summary>搜索框占位文案（随子页切换：意向页专用「搜索报价内容、识别结果、机构、交易员」）。</summary>
        public string SearchPlaceholder
        {
            get
            {
                switch (FilterPanel)
                {
                    case MarketFilterPanelType.Intent: return "搜索报价内容、识别结果、机构、交易员";
                    default: return "搜索债券、机构、交易员、发行人";
                }
            }
        }

        partial void OnFilterPanelChanged(MarketFilterPanelType value)
        {
            OnPropertyChanged(nameof(SearchPlaceholder));
        }

        /// <summary>左侧筛选面板的统一文本查询条件，最终由行情服务端解释。</summary>
        [ObservableProperty]
        private string _filterSearchText = string.Empty;

        partial void OnFilterSearchTextChanged(string value)
        {
            CreditAll.SearchText = value ?? string.Empty;
            CreditBest.SearchText = value ?? string.Empty;
            RateAll.SearchText = value ?? string.Empty;
            RateBest.SearchText = value ?? string.Empty;
            Intent.SearchText = value ?? string.Empty;
        }

        /// <summary>清除过滤（原型 clearBondFilterScope / clearIntentFilters）：除搜索词外，
        /// 还要退掉当前子页市场范围内的激活分组（意向页不清分组），看板与 chip 经
        /// ActiveGroupsChanged 自动回填与重查。面板内 tags/输入框的视觉重置由
        /// MarketFilterPanel.OnClearFilters 同步完成。</summary>
        [RelayCommand]
        private void ClearFilters()
        {
            FilterSearchText = string.Empty;
            switch (FilterPanel)
            {
                case MarketFilterPanelType.Credit:
                    MarketGroupSelectionService.ClearActive(FilterProfileService.MarketKind.Credit);
                    break;
                case MarketFilterPanelType.Rate:
                    MarketGroupSelectionService.ClearActive(FilterProfileService.MarketKind.Rate);
                    break;
                case MarketFilterPanelType.Alert:
                    // 提醒面板 = 原型 merged 口径，信用 + 利率一起退
                    MarketGroupSelectionService.ClearActive(FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate);
                    break;
                    // 意向页（原型 clearIntentFilters）只清搜索/标签，不动分组激活集
            }
        }

        #region 左右面板折叠（原型 filter-sidebar / board-detail 384↔40px 手柄）

        private const double PanelExpandedWidth = 384;
        private const double PanelCollapsedWidth = 40;

        [ObservableProperty]
        private bool _isFilterPanelExpanded = true;

        [ObservableProperty]
        private double _filterPanelWidth = PanelExpandedWidth;

        partial void OnIsFilterPanelExpandedChanged(bool value)
        {
            FilterPanelWidth = value ? PanelExpandedWidth : PanelCollapsedWidth;
        }

        [ObservableProperty]
        private bool _isDetailPanelExpanded = true;

        [ObservableProperty]
        private double _detailPanelWidth = PanelExpandedWidth;

        partial void OnIsDetailPanelExpandedChanged(bool value)
        {
            DetailPanelWidth = value ? PanelExpandedWidth : PanelCollapsedWidth;
        }

        /// <summary>折叠/展开左侧筛选面板（按钮位于面板内部）。</summary>
        [RelayCommand]
        private void ToggleFilterPanel()
        {
            IsFilterPanelExpanded = !IsFilterPanelExpanded;
        }

        /// <summary>折叠/展开右侧债券详情面板（按钮位于面板内部）。</summary>
        [RelayCommand]
        private void ToggleDetailPanel()
        {
            IsDetailPanelExpanded = !IsDetailPanelExpanded;
        }

        #endregion

        public ObservableCollection<SubTabItem> SubTabs { get; }

        [ObservableProperty]
        private SubTabItem _selectedSubTab;

        [ObservableProperty]
        private object _selectedSubPage;

        partial void OnSelectedSubTabChanged(SubTabItem value)
        {
            if (value == null) return;
            SelectedSubPage = value.Target;

            // 联动左侧筛选面板（原型：credit*→filters-credit，rate*→filters-rate，intent→filters-intent，alert→filters-merged）
            switch (value.Key)
            {
                case "intent": FilterPanel = MarketFilterPanelType.Intent; break;
                case "alert": FilterPanel = MarketFilterPanelType.Alert; break;
                case "rateAll":
                case "rateBest": FilterPanel = MarketFilterPanelType.Rate; break;
                default: FilterPanel = MarketFilterPanelType.Credit; break;
            }
        }

        [RelayCommand]
        private void SwitchSubTab(SubTabItem item)
        {
            if (item != null) SelectedSubTab = item;
        }

        #region 选中行 → 详情

        private void HookSelectedRow(ObservableObject board)
        {
            board.PropertyChanged += OnBoardPropertyChanged;
        }

        private void OnBoardPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BoardListViewModel.SelectedRow)) return;

            object row = null;
            var list = sender as BoardListViewModel;
            var best = sender as BoardBestViewModel;
            var alert = sender as BoardAlertViewModel;
            if (list != null) row = list.SelectedRow;
            else if (best != null) row = best.SelectedRow;
            else if (alert != null) row = alert.SelectedRow;

            UpdateDetailAsync(row);
        }

        private void OnFeedSnapshot(QuoteFeedLevel[] levels)
        {
            Detail?.ApplyFeedLevels(levels);
        }

        private void OnFeedLastExec(QuoteFeedLastExec exec)
        {
            Detail?.ApplyFeedLastExec(exec);
        }

        private void OnFeedBondInfo(QuoteFeedBondInfo info)
        {
            Detail?.ApplyFeedBondInfo(info);
        }

        private void UpdateDetailAsync(object row)
        {
            Detail?.StopRecommendations();
            if (row == null)
            {
                Detail = BondDetailViewModel.Empty;
                if (_quoteFeed != null) _quoteFeed.Switch(null);
                return;
            }

            var detail = new BondDetailViewModel();

            if (row is BoardQuoteRow q)
            {
                MapIdentity(detail, q.Code, q.Name, q.Maturity, q.BondLibrary, q.IssuerRating, q.NzYtm, q.NzNetPrice);
                // 最优条不再用选中行报价预填（看板行是单边报价，冒充市场最优会误导）；
                // Bid/Ofr/价差由宿主快照（ApplyFeedLevels）填充，无快照时显示 —
            }
            else if (row is BoardBestRow b)
            {
                MapIdentity(detail, b.Code, b.Name, b.Maturity, b.BondLibrary, b.IssuerRating, b.NzYtm, b.NzNetPrice);
            }
            else if (row is BoardAlertRow a)
            {
                MapIdentity(detail, a.Code, a.Name, a.Maturity, a.BondLibrary, a.IssuerRating, a.NzYtm, a.NzNetPrice);
            }
            else
            {
                Detail = BondDetailViewModel.Empty;
                if (_quoteFeed != null) _quoteFeed.Switch(null);
                return;
            }

            detail.HasDetail = true;
            Detail = detail;
            var dps = _marketCenterService as STC.SalesTrading.Plugin.Ficc.DpsSalesTradingServices;
            if (dps != null)
            {
                var names = STC.SalesTrading.Plugin.Ficc.DpsModelMapper.UserNames(
                    STC.Common.Handler.Admin.AdminHandler.Instance.GetCachestcUserQuery()?.Datas,
                    STC.Common.Utils.LocalEnv.UserInfo.UserId, STC.Common.Utils.LocalEnv.UserInfo.FullName,
                    STC.Common.Utils.LocalEnv.UserInfo.UserName);
                detail.AttachRecommendations(new STC.SalesTrading.Plugin.Ficc.DpsRecommendationService(dps.Api,
                    id => id != null && names.TryGetValue(id, out string name) ? name : id));
            }
            // 行情区块（多档/最优/最新成交/估值）由宿主订阅通道（_quoteFeed）真实数据灌入，不再走 Mock 回填；
            // 无行情债券保持行内已有字段 + 空档表
            if (_quoteFeed != null && _quoteFeed.IsLive) _quoteFeed.Switch(detail.Code);
        }

        private static void MapIdentity(BondDetailViewModel detail, string code, string name, string maturity,
            string bondLibrary, string issuerRating, decimal? nzYtm, decimal? nzNetPrice)
        {
            detail.Code = code ?? string.Empty;
            detail.Name = name ?? string.Empty;
            detail.Maturity = maturity ?? string.Empty;
            detail.BondLibrary = bondLibrary ?? string.Empty;
            detail.IssuerRating = issuerRating ?? string.Empty;
            detail.NzDisplay = string.Format("{0} / {1}",
                nzYtm?.ToString("0.0000") ?? "—",
                nzNetPrice?.ToString("0.0000") ?? "—");
        }

        #endregion

        public void Dispose()
        {
            Detail?.StopRecommendations();
            EventAggregator.Unsubscribe(this);
            CreditAll.PropertyChanged -= OnBoardPropertyChanged;
            CreditBest.PropertyChanged -= OnBoardPropertyChanged;
            RateAll.PropertyChanged -= OnBoardPropertyChanged;
            RateBest.PropertyChanged -= OnBoardPropertyChanged;
            Alert.PropertyChanged -= OnBoardPropertyChanged;

            CreditAll.Dispose();
            CreditBest.Dispose();
            RateAll.Dispose();
            RateBest.Dispose();
            Intent.Dispose();
            Alert.Dispose();
        }
    }
}
