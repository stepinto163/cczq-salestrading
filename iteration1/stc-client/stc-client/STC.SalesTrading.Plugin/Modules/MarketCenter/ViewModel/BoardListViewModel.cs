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
    public partial class BoardListViewModel : ObservableObject, IDisposable, IQueryLoadContext    {
        private readonly IMarketCenterService _marketCenterService;
        private bool _suppressFilter;
        private readonly LatestRequestCoordinator _queryCoordinator = new LatestRequestCoordinator();
        private readonly DebouncedRequestScheduler _searchScheduler = new DebouncedRequestScheduler();
        private static readonly TimeSpan SearchDebounce = TimeSpan.FromMilliseconds(300);

        public BoardListViewModel(IMarketCenterService marketCenterService, BondMarket market)
        {
            _marketCenterService = marketCenterService;
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

        public ObservableCollection<BoardQuoteRow> Rows { get; } = new ObservableCollection<BoardQuoteRow>();

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
            _searchScheduler.Schedule(SearchDebounce, token => QueryAsync());
        }

[ObservableProperty]
        private bool _showBid = true;

        [ObservableProperty]
        private bool _showOfr = true;

        [ObservableProperty]
        private bool _showMine;

        [ObservableProperty]
        private bool _showUrgent;

        [ObservableProperty]
        private bool _showRecommend;

        [ObservableProperty]
        private bool _showWhole;

        [ObservableProperty]
        private bool _showUnquoted;

        partial void OnShowBidChanged(bool value) { RefreshFilter(); }
        partial void OnShowOfrChanged(bool value) { RefreshFilter(); }
        partial void OnShowMineChanged(bool value) { RefreshFilter(); }
        partial void OnShowUrgentChanged(bool value) { RefreshFilter(); }
        partial void OnShowRecommendChanged(bool value) { RefreshFilter(); }
        partial void OnShowWholeChanged(bool value) { RefreshFilter(); }

        partial void OnShowUnquotedChanged(bool value)
        {
            if (value)
            {
                // 原型 syncBoardToolbarControls：未报价债券勾选时清空其余开关
                _suppressFilter = true;
                ShowMine = false;
                ShowUrgent = false;
                ShowRecommend = false;
                ShowWhole = false;
                _suppressFilter = false;
                ToastService.Show("未报价债券列表待接 bond-filter/unquoted 服务", ToastKind.Warning);
            }
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            if (_suppressFilter) return;
            _searchScheduler.Cancel();
            _ = QueryAsync();
        }

        /// <summary>分组 chip 激活集变化 → 重查看板（过滤在 QueryAsync 结果上做）。</summary>
        private void OnActiveGroupsChanged()
        {
            RefreshFilter();
        }

        private void RecomputeStats()
        {
            _status.Set("total", string.Format("报价总数: {0}", Rows.Count));
            _status.Set("bid", "Bid: " + Rows.Where(r => r.Direction == QuoteDirection.Bid).Count());
            _status.Set("ofr", "Ofr: " + Rows.Where(r => r.Direction == QuoteDirection.Ofr).Count());
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

                        private async System.Threading.Tasks.Task QueryAsync()
        {
            await QueryFlow.RunAsync(_queryCoordinator, this, async lease =>
            {
                var query = new MarketBoardQuery
                {
                    Market = Market,
                    IncludeBid = ShowBid,
                    IncludeOfr = ShowOfr,
                    MineOnly = ShowMine,
                    RequiredFlags = (ShowUrgent ? QuoteFlag.Urgent : QuoteFlag.None)
                        | (ShowRecommend ? QuoteFlag.Recommend : QuoteFlag.None)
                        | (ShowWhole ? QuoteFlag.Whole : QuoteFlag.None),
                    UnquotedOnly = ShowUnquoted,
                    SearchText = SearchText,
                };
                var result = await _marketCenterService.QueryBoardRowsAsync(query, lease.Token);
                if (!lease.IsCurrent) return null;

                Rows.Clear();
                foreach (var row in result.Items)
                {
                    // 侧栏分组 chip 激活时的客户端过滤（原型 include_security_ids 行过滤的本地近似）
                    if (MarketGroupSelectionService.Matches(row)) Rows.Add(row);
                }
                TotalCount = result.TotalCount;
                QueryId = result.QueryId;
                IsPartial = result.IsPartial;
                return Rows.Count > 0;
            }, Title);
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            _ = QueryAsync();
        }

        private void OnQuoteEvent(QuoteEvent message)
        {
            // 原型 0910：已选中行时（手动态）不打断列表，只累计新报价并显示横幅
            if (SelectedRow != null)
            {
                PendingNewQuoteCount++;
                return;
            }

            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = QueryAsync()));
                return;
            }
            _ = QueryAsync();
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
            await QueryAsync();
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
        private void Refer()
        {
            ToastService.Show("Refer 待接 quote-refer 服务");
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
            if (!(row is BoardQuoteRow r)) return;
            var text = string.Format("{0} {1} {2} {3}",
                r.Direction == QuoteDirection.Bid ? "bid" : "ofr",
                r.Code, r.Price?.ToString("0.####"), r.Volume);
            ClipboardHelper.CopyWithToast(text);
        }

        #region 行右键菜单（原型 #ctxMenu）

        /// <summary>右键菜单：新报价（原型 ctxNewQuote → 新报价表单）。</summary>
        [RelayCommand]
        private void NewQuoteForRow(object row)
        {
            OpenQuoteForm(row, MarketView.QuoteFormMode.New);
        }

        /// <summary>右键菜单：编辑报价（原型 ctxEdit → 编辑报价表单）。</summary>
        [RelayCommand]
        private void OpenQuoteEntryForRow(object row)
        {
            OpenQuoteForm(row, MarketView.QuoteFormMode.Edit);
        }

        private static void OpenQuoteForm(object row, MarketView.QuoteFormMode mode)
        {
            var owner = System.Windows.Application.Current?.MainWindow;
            if (row is BoardQuoteRow quoteRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, quoteRow, mode); return; }
            if (row is BoardBestRow bestRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, bestRow, mode); return; }
            if (row is BoardAlertRow alertRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, alertRow, mode); return; }

            ToastService.Show("请选择一行行情后再打开报价表单", ToastKind.Warning);
        }

        /// <summary>右键菜单：打包 —— 原型直接更新该报价标签（待接报价编辑接口）。</summary>
        [RelayCommand]
        private void PackRow(object row)
        {
            ToastService.Show(string.Format("{0}打包 标签待接（原型直接更新该报价）", RowPrefix(row)));
        }

        /// <summary>右键菜单：OCO —— 同上。</summary>
        [RelayCommand]
        private void OcoRow(object row)
        {
            ToastService.Show(string.Format("{0}OCO 标签待接（原型直接更新该报价）", RowPrefix(row)));
        }

        /// <summary>右键菜单：报价日志（原型 ctxLog → 报价日志弹窗 quote-log-modal，插件暂无该弹窗）。</summary>
        [RelayCommand]
        private void QuoteLog(object row)
        {
            ToastService.Show(string.Format("{0}报价日志待接（原型 quote-log-modal）", RowPrefix(row)));
        }

        private static string RowPrefix(object row)
        {
            if (row is BoardQuoteRow quoteRow) return (quoteRow.Code ?? string.Empty) + " ";
            if (row is BoardBestRow bestRow) return (bestRow.Code ?? string.Empty) + " ";
            if (row is BoardAlertRow alertRow) return (alertRow.Code ?? string.Empty) + " ";
            return string.Empty;
        }

        #endregion

        /// <summary>Refer 单行（待接 quote-refer 服务）。</summary>
        [RelayCommand]
        private void ReferRow(object row)
        {
            var r = row as BoardQuoteRow;
            ToastService.Show(string.Format("Refer 待接：{0} {1} {2}", r?.Code, r?.Direction, r?.Price?.ToString("0.####")));
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
