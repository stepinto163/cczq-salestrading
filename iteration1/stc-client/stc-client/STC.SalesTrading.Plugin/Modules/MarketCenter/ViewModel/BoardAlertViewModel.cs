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
using STC.SalesTrading.Plugin.Ficc;
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
    public partial class BoardAlertViewModel : ObservableObject, IDisposable, IQueryLoadContext    {
        private STC.SalesTrading.Plugin.Ficc.Protocol.Filter productFilter;

        public void ApplyProductFilter(STC.SalesTrading.Plugin.Ficc.Protocol.Filter filter)
        {
            productFilter = filter;
            _loadCoordinator.CancelCurrent();
            _ = LoadAsync();
        }

        private readonly IMarketCenterService _marketCenterService;
        private readonly LatestRequestCoordinator _loadCoordinator = new LatestRequestCoordinator();

        public BoardAlertViewModel(IMarketCenterService marketCenterService)
        {
            _marketCenterService = marketCenterService;
            if (marketCenterService is ISalesTradingUpdates updates)
            {
                updates.NotificationsChanged += OnBackendChanged;
            }

            _status.Add("total", "报价总数: 0")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6")
                   .Add("pair", "对价: 0", "BidDeepBrush", "#D29922");

            View = CollectionViewSource.GetDefaultView(Rows);
            View.Filter = FilterRow;

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
            _ = LoadAsync();
        }

        public string Title { get; } = "报价提醒";

        public ObservableCollection<BoardAlertRow> Rows { get; } = new ObservableCollection<BoardAlertRow>();

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
            var r = (BoardAlertRow)o;
            var anyFilter = ShowFavorite || ShowFilterGroup || ShowPriceMatch || ShowBestQuote;
            if (!anyFilter) return true;
            if (ShowFavorite && r.AlertType == AlertType.FavoriteBond) return true;
            if (ShowFilterGroup && r.AlertType == AlertType.FilterGroup) return true;
            if (ShowPriceMatch && r.AlertType == AlertType.PriceMatch) return true;
            if (ShowBestQuote && r.AlertType == AlertType.MyBestQuote) return true;
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

        /// <summary>成功/失败路径末尾的计数回写（QueryFlow 调用）；成功路径追加提醒弹窗检查。</summary>
        public void AfterQuery()
        {
            RecomputeStats();
            if (LoadState != QueryLoadState.Error) PushAlertPopups();
        }

                        private async System.Threading.Tasks.Task LoadAsync()
        {
            await QueryFlow.RunAsync(_loadCoordinator, this, async lease =>
            {
                var rows = _marketCenterService is DpsSalesTradingServices dps
                    ? await dps.LoadAlertRowsAsync(productFilter, lease.Token)
                    : await _marketCenterService.LoadAlertRowsAsync(lease.Token);
                if (!lease.IsCurrent) return null;

                Rows.Clear();
                foreach (var row in rows) Rows.Add(row);
                TotalCount = Rows.Count;
                QueryId = null;
                IsPartial = false;
                return Rows.Count > 0;
            }, Title);
        }

        /// <summary>
        /// 加载后推送右下角提醒弹窗 + nav 角标（原型 0910 alert-popup 语义）：
        /// 同类型提醒并入同一个弹窗分组（头部显示类型 + 新增数，正文最多 3 条）。
        /// </summary>
        private readonly HashSet<string> displayedAlerts = new HashSet<string>();

        private void PushAlertPopups()
        {
            foreach (var a in Rows.Where(row => !row.IsRead && displayedAlerts.Add(row.AlertKey + ":" + row.AlertType)).Take(2))
            {
                var item = new STC.SalesTrading.Plugin.Shared.ViewModel.AlertPopupItem(
                    a.AlertKey, TypeLabel(a.AlertType), a.AlertType,
                    a.Code ?? string.Empty, a.Name ?? string.Empty,
                    a.QuoteTime?.ToString("HH:mm") ?? string.Empty,
                    () => ToastService.Show("查看提醒：定位提醒页（待接）"))
                {
                    BidTrader = a.TraderBid,
                    BidVolume = a.VolBid?.ToString("0"),
                    BidPrice = a.Bid?.ToString("0.0000") ?? "—",
                    OfrPrice = a.Ofr?.ToString("0.0000") ?? "—",
                    OfrVolume = a.VolOfr?.ToString("0"),
                    OfrTrader = a.TraderOfr,
                };
                STC.SalesTrading.Plugin.Shared.Services.AlertPopupService.Show(item);
            }
            EventAggregator.Publish(new AlertCountChangedMessage(Rows.Where(row => !row.IsRead).Select(row => row.AlertKey).Distinct().Count()));
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
            _ = LoadAsync();
        }

        /// <summary>复制提醒报价文本。</summary>
        [RelayCommand]
        private void CopyQuoteText(object row)
        {
            if (!(row is BoardAlertRow r)) return;
            var text = string.Format("{0} Bid {1} / Ofr {2} {3}", r.Code, r.Bid?.ToString("0.####"), r.Ofr?.ToString("0.####"), r.VolOfr);
            ClipboardHelper.CopyWithToast(text);
        }

        /// <summary>Refer 单行（待接）。</summary>
        [RelayCommand]
        private void ReferRow(object row)
        {
            var r = row as BoardAlertRow;
            ToastService.Show(string.Format("Refer 待接：{0} {1}", r?.Code, r?.Name));
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

        [RelayCommand]
        private async System.Threading.Tasks.Task Refresh()
        {
            await LoadAsync();
            ToastService.Show("报价提醒已刷新（模拟）", ToastKind.Success);
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

        private void OnBackendChanged()
        {
            _ = LoadAsync();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task MarkRead()
        {
            try
            {
                if (!(_marketCenterService is DpsSalesTradingServices services) || !(SelectedRow is BoardAlertRow selected))
                {
                    throw new InvalidOperationException("请先选择一条提醒。");
                }
                await services.MarkNotificationReadAsync(selected.AlertKey);
                await LoadAsync();
                ToastService.Show("已标记为已读。", ToastKind.Success);
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
                ToastService.Show(error.Message, ToastKind.Error);
            }
        }

        public void Dispose()
        {
            if (_marketCenterService is ISalesTradingUpdates updates)
            {
                updates.NotificationsChanged -= OnBackendChanged;
            }
            _loadCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
        }
    }
}
