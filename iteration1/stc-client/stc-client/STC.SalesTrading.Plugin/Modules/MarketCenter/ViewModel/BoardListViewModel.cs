using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Shared.Converters;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using MarketView = STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// "全部"看板 VM（信用/利率复用）。20 列宽表 + 工具栏（Copy/Refer/导出/Bid/Ofr 方向开关/
    /// 我的报价/紧急/推荐/整量/未报价债券）+ 状态栏统计。
    /// </summary>
    public partial class BoardListViewModel : ObservableObject, IDisposable, IQueryLoadContext, IBoardLifecycle    {
        private readonly IMarketCenterService _marketCenterService;
        private readonly IQuoteService _quoteService;
        private readonly IQuoteMgmtService _mgmtService;
        private readonly LatestRequestCoordinator _queryCoordinator = new LatestRequestCoordinator();
        private readonly DebouncedRequestScheduler _searchScheduler = new DebouncedRequestScheduler();
        private static readonly TimeSpan SearchDebounce = TimeSpan.FromMilliseconds(300);

        public BoardListViewModel(IMarketCenterService marketCenterService, BondMarket market,
            IQuoteService quoteService, IQuoteMgmtService mgmtService)
        {
            _marketCenterService = marketCenterService;
            _quoteService = quoteService;
            _mgmtService = mgmtService;
            Market = market;

            _status.Add("total", "报价总数: 0")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6");

            View = CollectionViewSource.GetDefaultView(Rows);

            // 侧栏分组 chip 激活/取消 → 重查（批次 A：关注分组闭环）
            MarketGroupSelectionService.ActiveGroupsChanged += OnActiveGroupsChanged;
            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
            _marketCenterService.SubscribeQuoteEvents(OnQuoteEvent);
            _ = QueryAsync();
        }

        public BondMarket Market { get; }

        public string Title
        {
            get { return Market == BondMarket.Credit ? "信用全部" : "利率全部"; }
        }

        public ObservableCollection<TapeRow> Rows { get; } = new ObservableCollection<TapeRow>();

        public ICollectionView View { get; }

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        #region 工具栏开关

                /// <summary>当前选中行（驱动右侧债券详情面板，行类型随子表而异）。</summary>
        [ObservableProperty]
        private object _selectedRow;

        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            _queryCoordinator.CancelCurrent();
            _searchScheduler.Schedule(SearchDebounce, token => { ResetAndQuery(); return System.Threading.Tasks.Task.CompletedTask; });
        }

[ObservableProperty]
        private bool _showBid = true;

        [ObservableProperty]
        private bool _showOfr = true;

        [ObservableProperty]
        private bool _showUrgent;

        [ObservableProperty]
        private bool _showRecommend;

        [ObservableProperty]
        private bool _showWhole;

        partial void OnShowBidChanged(bool value) { RefreshFilter(); }
        partial void OnShowOfrChanged(bool value) { RefreshFilter(); }
        partial void OnShowUrgentChanged(bool value) { RefreshFilter(); }
        partial void OnShowRecommendChanged(bool value) { RefreshFilter(); }
        partial void OnShowWholeChanged(bool value) { RefreshFilter(); }


        private void RefreshFilter()
        {
            _searchScheduler.Cancel();
            ResetAndQuery();
        }

        /// <summary>分组 chip 激活集变化 → 回第 1 页重查看板（服务端按组过滤）。</summary>
        private void OnActiveGroupsChanged()
        {
            RefreshFilter();
        }

        private void RecomputeStats()
        {
            _status.Set("total", string.Format("报价总数: {0}", Rows.Count));
            _status.Set("bid", "Bid: " + Rows.Where(r => r.Side == QuoteDirection.Bid).Count());
            _status.Set("ofr", "Ofr: " + Rows.Where(r => r.Side == QuoteDirection.Ofr).Count());
        }

        /// <summary>失败路径清空（QueryFlow 调用）：清行并复位结果字段。</summary>
        public void ResetForFailure()
        {
            Rows.Clear();
            TotalCount = 0;
            QueryId = null;
            IsPartial = false;
        }

        /// <summary>成功/失败路径末尾的计数回写（QueryFlow 调用）。</summary>
        public void AfterQuery()
        {
            RecomputeStats();
        }

        #endregion

        #region 数据加载

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private QueryLoadState _loadState = QueryLoadState.Loading;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _queryId;

        [ObservableProperty]
        private bool _isPartial;

        public bool IsLoading { get { return LoadState == QueryLoadState.Loading; } }

        public bool IsEmpty { get { return LoadState == QueryLoadState.Empty; } }

        partial void OnLoadStateChanged(QueryLoadState value)
        {
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>侧栏面板条件变化（含空条件=清除筛选）：取消当前查询并按新条件重查。</summary>
        public void ApplyProductFilter(Abstractions.Queries.ProductFilter filter)
        {
            productFilter = filter;
            ResetAndQuery();
        }

        /// <summary>侧栏面板条件（null=未启用）。</summary>
        private Abstractions.Queries.ProductFilter productFilter;

        /// <summary>当前页（1-based；PaginationBar TwoWay）。</summary>
        [ObservableProperty]
        private int _page = 1;

        /// <summary>总页数（服务端 TotalCount / 页大小）。</summary>
        [ObservableProperty]
        private int _totalPages = 1;

        /// <summary>页大小。</summary>
        public int PageSize { get; } = 50;

        partial void OnPageChanged(int value)
        {
            _ = QueryAsync();
        }

        /// <summary>筛选/条件变化 → 回第 1 页重查（Page 变化经 OnPageChanged 触发重查）。</summary>
        private void ResetAndQuery()
        {
            if (Page != 1) Page = 1;
            else _ = QueryAsync();
        }

                        private async System.Threading.Tasks.Task QueryAsync(bool silent = false)
        {
            await QueryFlow.RunAsync(_queryCoordinator, this, async lease =>
            {
                var query = new MarketBoardQuery
                {
                    Market = Market,
                    IncludeBid = ShowBid,
                    IncludeOfr = ShowOfr,
                    RequiredFlags = (ShowUrgent ? QuoteFlag.Urgent : QuoteFlag.None)
                        | (ShowRecommend ? QuoteFlag.Recommend : QuoteFlag.None)
                        | (ShowWhole ? QuoteFlag.Whole : QuoteFlag.None),
                    SearchText = SearchText,
                    // 侧栏面板条件 + 激活分组 Id 由网关并入 view.open filter（服务端过滤）
                    ProductFilter = productFilter,
                    Page = Page,
                    PageSize = PageSize,
                };
                var result = await _marketCenterService.QueryBoardRowsAsync(query, lease.Token);
                if (!lease.IsCurrent) return null;

                if (silent)
                {
                    // 推送触发的静默刷新：增量应用（插入/移动/替换/移除），新报价与改价行回传闪烁
                    var flashKeys = new HashSet<string>(
                        _marketCenterService.SnapshotTouchedQuoteIds(), StringComparer.Ordinal);
                    var flashed = Shared.ViewModel.RowsDiff.Apply(Rows, result.Items, r => r.QuoteId, flashKeys);
                    RaiseRowsFlashed(flashed);
                }
                else
                {
                    Rows.Clear();
                    foreach (var row in result.Items)
                    {
                        Rows.Add(row);
                    }
                }
                TotalCount = result.TotalCount;
                TotalPages = (int)System.Math.Ceiling(result.TotalCount / (double)PageSize);
                QueryId = result.QueryId;
                IsPartial = result.IsPartial;
                return Rows.Count > 0;
            }, Title);
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            _ = QueryAsync();
        }

        // ---- 子页激活态（IBoardLifecycle）：KeepAlive 常驻，不可见时推送置脏不查询 ----

        private bool _boardActive;
        private bool _dirty;

        public void SetBoardActive(bool active)
        {
            var was = _boardActive;
            _boardActive = active;
            if (active && !was && _dirty)
            {
                _dirty = false;
                // 冻结态（选中行/非第一页）保持横幅由用户点击刷新；常规态补静默刷（保留闪烁）
                if (SelectedRow == null && Page == 1)
                {
                    PendingNewQuoteCount = 0;
                    _ = QueryAsync(true);
                }
            }
        }

        /// <summary>本轮增量应用中新增/改价的行（View 订阅后播闪烁动画）。</summary>
        public event Action<IReadOnlyList<object>> RowsFlashed;

        private void RaiseRowsFlashed(List<TapeRow> rows)
        {
            var handler = RowsFlashed;
            if (handler != null && rows.Count > 0) handler(rows.Cast<object>().ToList());
        }

        private void OnQuoteEvent(QuoteEvent message)
        {
            // 不可见看板只置脏（激活时补静默刷新），不参与实时查询；
            // 横幅按本批实际变化报价数累加（事件数在同帧合并后=1，不是变化条数）
            if (!_boardActive)
            {
                _dirty = true;
                PendingNewQuoteCount += Math.Max(1, message.ChangedCount);
                return;
            }

            // 原型 0910：选中行或非第一页（手动态）不打断列表，只累计新报价并显示横幅
            if (SelectedRow != null || Page != 1)
            {
                PendingNewQuoteCount += Math.Max(1, message.ChangedCount);
                return;
            }

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = QueryAsync(true)));
                return;
            }
            _ = QueryAsync(true);
        }

        #endregion

        #region 工具栏命令

        /// <summary>
        /// 原型 0910：手动选中行后列表冻结，新到报价不再打断当前视图，只累计条数并在表格上方
        /// 显示「有 N 条新报价，点击刷新」横幅（0 表示不显示）。
        /// </summary>
        [ObservableProperty]
        private int _pendingNewQuoteCount;

        /// <summary>横幅点击：清空计数并重查（对应原型 refreshFrozenBoardProjection）。</summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task RefreshPendingQuotes()
        {
            PendingNewQuoteCount = 0;
            if (Page != 1) Page = 1; // 回第一页重查（经 OnPageChanged）
            else await QueryAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Refresh()
        {
            await QueryAsync();
            ToastService.Show(Title + " 已刷新（模拟）", ToastKind.Success);
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Retry()
        {
            await QueryAsync();
        }

        [RelayCommand]
        private void Copy()
        {
            ToastService.Show("Copy 报价文本待接 /api/bond-copy-enrich 富化");
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Refer()
        {
            var revision = BoardRowQuoteResolver.ResolveRevision(SelectedRow);
            var quoteId = BoardRowQuoteResolver.ResolveQuoteId(SelectedRow);
            if (string.IsNullOrEmpty(quoteId) || string.IsNullOrEmpty(revision))
            {
                ToastService.Show("请选中「全部」页的单笔报价行（本行无法定位报价版本）");
                return;
            }
            try
            {
                await _mgmtService.ReferQuotesAsync(
                    new[] { new QuoteRow { QuoteId = quoteId, Revision = revision } },
                    System.Threading.CancellationToken.None);
                ToastService.Show("已 Refer " + BoardRowQuoteResolver.ResolveLabel(SelectedRow) + "，行情更新以推送为准", ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("Refer 失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private void Export()
        {
            ToastService.Show("导出 xlsx 待接（原型 SheetJS）");
        }

        /// <summary>复制报价文本（原型 Copy 语义：bid/ofr 代码 价格 报量）。</summary>
        [RelayCommand]
        private void CopyQuoteText(object row)
        {
            if (!(row is TapeRow r)) return;
            var text = string.Format("{0} {1} {2} {3}",
                r.Side == QuoteDirection.Bid ? "bid" : "ofr",
                r.SecurityId, r.Price?.ToString("0.####"), r.Quantity);
            ClipboardHelper.CopyWithToast(text);
        }

        #region 行右键菜单（原型 #ctxMenu）

        /// <summary>右键菜单：新报价（原型 ctxNewQuote → 新报价表单）。</summary>
        [RelayCommand]
        private void NewQuoteForRow(object row)
        {
            OpenQuoteForm(row, MarketView.QuoteFormMode.New);
        }

        /// <summary>修改类操作的门禁：仅本人报价（提醒等无归属字段的行不禁，后端兜底）。</summary>
        private static bool IsMineRow(object row)
        {
            if (row is Abstractions.Models.TapeRow t) return t.IsMine;
            if (row is Abstractions.Models.BestRow b) return b.IsMine;
            return true;
        }

        /// <summary>右键菜单：编辑报价（原型 ctxEdit → 编辑报价表单）。</summary>
        [RelayCommand]
        private void OpenQuoteEntryForRow(object row)
        {
            if (!IsMineRow(row)) { ToastService.Show("仅本人报价可操作", ToastKind.Warning); return; }
            OpenQuoteForm(row, MarketView.QuoteFormMode.Edit);
        }

        private static void OpenQuoteForm(object row, MarketView.QuoteFormMode mode)
        {
            var owner = System.Windows.Application.Current?.MainWindow;
            if (row is TapeRow quoteRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, quoteRow, mode); return; }
            if (row is BestRow bestRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, bestRow, mode); return; }
            if (row is AlertRow alertRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, alertRow, mode); return; }

            ToastService.Show("请选择一行行情后再打开报价表单", ToastKind.Warning);
        }

        /// <summary>右键菜单：报价日志（ficc.quote.history.query，H10）。异步拉取——同步等网络会冻结 UI 最坏到请求超时。</summary>
        [RelayCommand]
        private void QuoteLog(object row)
        {
            var quoteId = BoardRowQuoteResolver.ResolveQuoteId(row);
            if (string.IsNullOrEmpty(quoteId))
            {
                ToastService.Show("该行未关联具体报价，无法查看日志");
                return;
            }
            try
            {
                MarketView.QuoteLogWindow.Show(System.Windows.Application.Current?.MainWindow, quoteId,
                    BoardRowQuoteResolver.ResolveLabel(row), _quoteService);
            }
            catch (Exception ex)
            {
                ToastService.Show("报价日志拉取失败：" + ex.Message, ToastKind.Error);
            }
        }

        private static string RowPrefix(object row)
        {
            if (row is TapeRow quoteRow) return (quoteRow.SecurityId ?? string.Empty) + " ";
            if (row is BestRow bestRow) return (bestRow.SecurityId ?? string.Empty) + " ";
            if (row is AlertRow alertRow) return (alertRow.SecurityId ?? string.Empty) + " ";
            return string.Empty;
        }

        #endregion

        /// <summary>Refer 行（ficc.quote.refer；仅"全部"页行携带乐观锁版本，聚合/提醒行引导跳转）。</summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task ReferRow(object row)
        {
            if (!IsMineRow(row)) { ToastService.Show("仅本人报价可操作", ToastKind.Warning); return; }
            var revision = BoardRowQuoteResolver.ResolveRevision(row);
            var quoteId = BoardRowQuoteResolver.ResolveQuoteId(row);
            if (string.IsNullOrEmpty(quoteId) || string.IsNullOrEmpty(revision))
            {
                ToastService.Show("请到「全部」页对单笔报价操作（本行无法定位报价版本）");
                return;
            }
            try
            {
                await _mgmtService.ReferQuotesAsync(
                    new[] { new QuoteRow { QuoteId = quoteId, Revision = revision } },
                    System.Threading.CancellationToken.None);
                ToastService.Show("已 Refer " + BoardRowQuoteResolver.ResolveLabel(row) + "，行情更新以推送为准", ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("Refer 失败：" + ex.Message, ToastKind.Error);
            }
        }

        /// <summary>右键菜单：打包/OCO 标签更新（ficc.quote.update；互斥，一开一关显式提交）。</summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task PackRow(object row)
        {
            if (!IsMineRow(row)) { ToastService.Show("仅本人报价可操作", ToastKind.Warning); return; }
            await ToggleExclusiveFlag(row, packing: true, oco: false, "打包");
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task OcoRow(object row)
        {
            if (!IsMineRow(row)) { ToastService.Show("仅本人报价可操作", ToastKind.Warning); return; }
            await ToggleExclusiveFlag(row, packing: false, oco: true, "OCO");
        }

        private async System.Threading.Tasks.Task ToggleExclusiveFlag(object row, bool packing, bool oco, string label)
        {
            var revision = BoardRowQuoteResolver.ResolveRevision(row);
            var quoteId = BoardRowQuoteResolver.ResolveQuoteId(row);
            if (string.IsNullOrEmpty(quoteId) || string.IsNullOrEmpty(revision))
            {
                ToastService.Show("请到「全部」页对单笔报价操作（本行无法定位报价版本）");
                return;
            }
            try
            {
                await _quoteService.UpdateQuotesAsync(
                    new[] { new QuoteRow { QuoteId = quoteId, Revision = revision, Packing = packing, Oco = oco } },
                    System.Threading.CancellationToken.None);
                ToastService.Show(label + " 已更新，行情更新以推送为准", ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show(label + " 更新失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private void OpenQuoteEntry()
        {
            // 顶栏「报价录入」：发布消息，由 Shell 打开报价录入模块浮窗（原型 quote-entry-overlay 承载报价录入整模块）
            EventAggregator.Publish(new OpenQuoteEntryOverlayMessage());
        }

        #endregion

        public void Dispose()
        {
            MarketGroupSelectionService.ActiveGroupsChanged -= OnActiveGroupsChanged;
            _marketCenterService.UnsubscribeQuoteEvents(OnQuoteEvent);
            _searchScheduler.Dispose();
            _queryCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
        }
    }
}
