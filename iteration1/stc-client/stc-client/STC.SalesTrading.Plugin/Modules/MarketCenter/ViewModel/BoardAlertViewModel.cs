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
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 报价提醒 VM：23 列提醒表 + 4 开关（关注债券/筛选分组/对价提醒/我的最优报价）+ 右下角弹窗推送（待接）。
    /// </summary>
    public partial class BoardAlertViewModel : ObservableObject, IDisposable, IQueryLoadContext, IBoardLifecycle    {
        private readonly IMarketCenterService _marketCenterService;
        private readonly IQuoteService _quoteService;
        private readonly IQuoteMgmtService _mgmtService;
        private readonly LatestRequestCoordinator _loadCoordinator = new LatestRequestCoordinator();

        public BoardAlertViewModel(IMarketCenterService marketCenterService, IQuoteService quoteService, IQuoteMgmtService mgmtService)
        {
            _marketCenterService = marketCenterService;
            _quoteService = quoteService;
            _mgmtService = mgmtService;

            _status.Add("total", "报价总数: 0")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6")
                   .Add("pair", "对价: 0", "BidDeepBrush", "#D29922");

            View = CollectionViewSource.GetDefaultView(Rows);
            View.Filter = FilterRow;

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
            EventAggregator.Subscribe<Shared.Messages.AlertsChangedMessage>(this, OnAlertsChanged);
            _ = LoadAsync();
        }

        public string Title { get; } = "报价提醒";

        public ObservableCollection<AlertRow> Rows { get; } = new ObservableCollection<AlertRow>();

        public ICollectionView View { get; }

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

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

                /// <summary>当前选中行（驱动右侧债券详情面板，行类型随子表而异）。</summary>
        [ObservableProperty]
        private object _selectedRow;

[ObservableProperty]
        private bool _showFavorite;

        [ObservableProperty]
        private bool _showFilterGroup;

        [ObservableProperty]
        private bool _showPriceMatch;

        [ObservableProperty]
        private bool _showBestQuote;

        partial void OnShowFavoriteChanged(bool value) { View.Refresh(); }
        partial void OnShowFilterGroupChanged(bool value) { View.Refresh(); }
        partial void OnShowPriceMatchChanged(bool value) { View.Refresh(); }
        partial void OnShowBestQuoteChanged(bool value) { View.Refresh(); }

        private bool FilterRow(object o)
        {
            var r = (AlertRow)o;
            var anyFilter = ShowFavorite || ShowFilterGroup || ShowPriceMatch || ShowBestQuote;
            if (!anyFilter) return true;
            if (ShowFavorite && r.Types.Contains(AlertType.FavoriteBond)) return true;
            if (ShowFilterGroup && r.Types.Contains(AlertType.FilterGroup)) return true;
            if (ShowPriceMatch && r.Types.Contains(AlertType.PriceMatch)) return true;
            if (ShowBestQuote && r.Types.Contains(AlertType.MyBestQuote)) return true;
            return false;
        }

        private void RecomputeStats()
        {
            _status.Set("total", string.Format("报价总数: {0}", Rows.Count));
            _status.Set("bid", "Bid: " + Rows.Where(r => r.Bid.HasValue).Count());
            _status.Set("ofr", "Ofr: " + Rows.Where(r => r.Ofr.HasValue).Count());
            _status.Set("pair", "对价: " + Rows.Where(r => r.Bid.HasValue && r.Ofr.HasValue && r.Bid.Value >= r.Ofr.Value).Count());
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

        /// <summary>侧栏面板条件（Combined）变化：回第 1 页重查（通知域无分组过滤）。</summary>
        public void ApplyProductFilter(Abstractions.Queries.ProductFilter filter)
        {
            productFilter = filter;
            if (Page != 1) Page = 1;
            else _ = LoadAsync();
        }

        /// <summary>横幅点击：清零计数并回第 1 页重查。</summary>
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void RefreshPendingQuotes()
        {
            PendingNewQuoteCount = 0;
            if (Page != 1) Page = 1;
            else _ = LoadAsync();
        }

        private Abstractions.Queries.ProductFilter productFilter;

        /// <summary>当前页（1-based；PaginationBar TwoWay）。</summary>
        [ObservableProperty]
        private int _page = 1;

        /// <summary>总页数（服务端 TotalCount / 页大小）。</summary>
        [ObservableProperty]
        private int _totalPages = 1;

        /// <summary>页大小。</summary>
        public int PageSize { get; } = 50;

        /// <summary>「有 N 条新提醒」横幅计数（推送订阅二期接入；现仅分页预留）。</summary>
        [ObservableProperty]
        private int _pendingNewQuoteCount;

        partial void OnPageChanged(int value)
        {
            _ = LoadAsync();
        }

        /// <summary>按独立报价版本实例建立基线并去重，不使用合并行 ID。</summary>
        private readonly AlertTriggerTracker triggerTracker = new AlertTriggerTracker();

        private async System.Threading.Tasks.Task LoadAsync(bool silent = false)
        {
            List<KeyValuePair<AlertRow, AlertTrigger>> freshTriggers = null;
            await QueryFlow.RunAsync(_loadCoordinator, this, async lease =>
            {
                var rows = await _marketCenterService.LoadAlertRowsAsync(productFilter, Page, PageSize, lease.Token);
                if (!lease.IsCurrent) return null;

                if (silent)
                {
                    // 推送触发的静默刷新：增量应用（移除/移动/原位替换/插入），保滚动位置；
                    // 整表 Clear+重建会在滚动/交互途中造成布局风暴（提醒表滚动卡顿成因之一）
                    Shared.ViewModel.RowsDiff.Apply(Rows, rows, r => r.NotificationId, null);
                }
                else
                {
                    Rows.Clear();
                    foreach (var row in rows) Rows.Add(row);
                }
                TotalCount = Rows.Count;
                TotalPages = (int)System.Math.Ceiling(TotalCount / (double)PageSize);
                QueryId = null;
                IsPartial = false;

                freshTriggers = triggerTracker.Observe(rows, silent);
                return Rows.Count > 0;
            }, Title, silent);
            // 弹窗在 QueryFlow 结束后执行：静默轮只弹新增，用户操作轮只记入不弹
            PushAlertPopups(freshTriggers);
        }

        /// <summary>
        /// 加载后推送右下角提醒弹窗 + nav 角标（原型 0910 alert-popup 语义）：
        /// 同类型提醒并入同一个弹窗分组（头部显示类型 + 新增数，正文最多 3 条）。
        /// </summary>
        private void PushAlertPopups(List<KeyValuePair<AlertRow, AlertTrigger>> freshOnly)
        {
            var popupSource = freshOnly ?? new List<KeyValuePair<AlertRow, AlertTrigger>>();
            foreach (var source in popupSource)
            {
                var a = source.Key;
                var type = source.Value.Type;
                var item = new STC.SalesTrading.Plugin.Shared.ViewModel.AlertPopupItem(
                    source.Value.AlertKey, TypeLabel(type), type,
                    a.SecurityId ?? string.Empty, a.SecurityName ?? string.Empty,
                    a.UpdateTime?.ToString("HH:mm") ?? string.Empty,
                    () =>
                    {
                        // 弹窗"查看"即已读上报（ficc.notification.read，方案 §3.2.4）
                        ToastService.Show("已标记提醒已读：" + (a.SecurityId ?? string.Empty), ToastKind.Success);
                        _ = _marketCenterService.MarkAlertReadAsync(a.NotificationId, System.Threading.CancellationToken.None);
                    })
                {
                    BidTrader = source.Value.TraderBid,
                    BidVolume = source.Value.VolBid?.ToString("0"),
                    BidPrice = source.Value.Bid?.ToString("0.0000") ?? "—",
                    OfrPrice = source.Value.Ofr?.ToString("0.0000") ?? "—",
                    OfrVolume = source.Value.VolOfr?.ToString("0"),
                    OfrTrader = source.Value.TraderOfr,
                    PriceTypeMark = source.Value.PriceTypeMark,
                };
                STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Show(item);
            }
            EventAggregator.Publish(new AlertCountChangedMessage(Rows.Count(row => !row.IsRead)));
        }

        private static string TypeLabel(AlertType type)
        {
            switch (type)
            {
                case AlertType.FavoriteBond: return "关注债券";
                case AlertType.FilterGroup: return "筛选分组";
                case AlertType.PriceMatch: return "对价提醒";
                case AlertType.MyBestQuote: return "我的最优";
                default: return "提醒";
            }
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            // 复位广播可能来自网关回调线程，同样分流
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = LoadAsync()));
                return;
            }
            _ = LoadAsync();
        }

        /// <summary>提醒域推送到达（notification 流 views.changed）：静默刷新列表，新到达的提醒弹横幅。
        /// 推送在 netapi 回调线程——CollectionView 拒绝跨线程改源集合，必须分流到 UI 线程。
        /// 提醒在其他页签也持续接收，避免切回时才补弹。</summary>
        private void OnAlertsChanged(Shared.Messages.AlertsChangedMessage message)
        {
            // 提醒是账户级事件；停留其他页时也应接收新提醒，而不是切回本页才补弹。
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = LoadAsync(silent: true)));
                return;
            }
            _ = LoadAsync(silent: true);
        }

        public void SetBoardActive(bool active)
        {
            // 账户级提醒持续处理；页签切换不重放已经见过的实例。
        }

        /// <summary>复制提醒报价文本。</summary>
        [RelayCommand]
        private void CopyQuoteText(object row)
        {
            if (!(row is AlertRow r)) return;
            var text = string.Format("{0} Bid {1} / Ofr {2} {3}", r.SecurityId, r.Bid?.ToString("0.####"), r.Ofr?.ToString("0.####"), r.VolOfr);
            ClipboardHelper.CopyWithToast(text);
        }

        /// <summary>Refer 单行：引导到「全部」页操作。</summary>
        [RelayCommand]
        private void ReferRow(object row)
        {
            var r = row as AlertRow;
            ToastService.Show("请到「全部」页对单笔报价操作（提醒行不携带报价版本）");
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
            if (row is TapeRow quoteRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, quoteRow, mode); return; }
            if (row is BestRow bestRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, bestRow, mode); return; }
            if (row is AlertRow alertRow) { MarketView.QuoteEntryDialog.ShowForRow(owner, alertRow, mode); return; }

            ToastService.Show("请选择一行行情后再打开报价表单", ToastKind.Warning);
        }

        /// <summary>右键菜单：打包 —— 提醒行无法定位报价版本，引导到「全部」页操作。</summary>
        [RelayCommand]
        private void PackRow(object row)
        {
            ToastService.Show("请到「全部」页对单笔报价操作（提醒行不携带报价版本）");
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

        [RelayCommand]
        private async System.Threading.Tasks.Task MarkRead(System.Collections.IList selected)
        {
            var rows = selected?.Cast<AlertRow>().ToList() ?? new List<AlertRow>();
            if (rows.Count == 0)
            {
                ToastService.Show("请先选择提醒", ToastKind.Warning);
                return;
            }
            try
            {
                foreach (var notificationId in rows.Select(row => row.NotificationId).Distinct())
                {
                    await _marketCenterService.MarkAlertReadAsync(notificationId, CancellationToken.None);
                    foreach (var row in Rows.Where(item => item.NotificationId == notificationId)) row.ReadTime = DateTime.Now;
                }
            }
            catch (Exception error)
            {
                ToastService.Show("标记已读失败：" + error.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Refresh()
        {
            await LoadAsync();
            ToastService.Show("报价提醒已刷新", ToastKind.Success);
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Retry()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private void OpenQuoteEntry()
        {
            // 顶栏「报价录入」：发布消息，由 Shell 打开报价录入模块浮窗（原型 quote-entry-overlay 承载报价录入整模块）
            EventAggregator.Publish(new OpenQuoteEntryOverlayMessage());
        }

        public void Dispose()
        {
            _loadCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
        }
    }
}
