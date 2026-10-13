using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Converters;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using STC.SalesTrading.Plugin.Ficc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 报价确认页 VM：分批分组折叠主从表 + 批量工具栏 + 状态栏统计。
    /// 加载待确认行 → 按原始消息批次分组 → 行内编辑 → 确认/删除入库。
    /// </summary>
    public partial class QuoteConfirmViewModel : ObservableObject, IDisposable
    {
        public ICrmDirectory CrmDirectory => _quoteService as ICrmDirectory;
        public CrmBatchEditor CrmBatch { get; }

        private readonly IQuoteService _quoteService;
        private readonly HashSet<QuoteConfirmRowViewModel> _hookedRows = new HashSet<QuoteConfirmRowViewModel>();
        private readonly HashSet<QuoteConfirmBatchViewModel> _hookedBatches = new HashSet<QuoteConfirmBatchViewModel>();
        private readonly LatestRequestCoordinator _loadCoordinator = new LatestRequestCoordinator();
        private bool _isUpdatingSelectionState;

        public QuoteConfirmViewModel(IQuoteService quoteService)
        {
            _quoteService = quoteService;
            CrmBatch = new CrmBatchEditor(() => SelectedRows);
            if (quoteService is ISalesTradingUpdates updates)
            {
                updates.QuotesChanged += OnBackendChanged;
            }

            _status.Add("group", "共 0 组 / 0 条报价")
                   .Add("source", "IDEAL: 0组 | QT: 0组")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("sync", "模拟数据");

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
            _ = InitializeAsync();
        }

        #region 集合与统计

        public ObservableCollection<QuoteConfirmBatchViewModel> Batches { get; } = new ObservableCollection<QuoteConfirmBatchViewModel>();

        /// <summary>已选行数。</summary>
        [ObservableProperty]
        private int _selectedCount;

        /// <summary>状态栏：共 N 组 / M 条报价。</summary>
        [ObservableProperty]
        private string _groupStats = "共 0 组 / 0 条报价";

        /// <summary>状态栏：来源统计。</summary>
        [ObservableProperty]
        private string _sourceStats = "IDEAL: 0组 | QT: 0组";

        /// <summary>状态栏：同步状态。</summary>
        [ObservableProperty]
        private string _syncState = "模拟数据";

        /// <summary>全选三态（表头复选框）。</summary>
        [ObservableProperty]
        private bool? _isSelectAllChecked = false;

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        private IEnumerable<QuoteConfirmRowViewModel> AllRows
        {
            get { return Batches.SelectMany(b => b.Rows); }
        }

        private IEnumerable<QuoteConfirmRowViewModel> SelectedRows
        {
            get { return AllRows.Where(r => r.IsSelected); }
        }

        /// <summary>待确认报价总条数（子页角标与导航角标共用同一数据源）。</summary>
        public int TotalRowCount
        {
            get { return Batches.Sum(b => b.Rows.Count); }
        }

        #endregion

        #region 加载与分组

        /// <summary>按服务端原始消息批次分组，缺少批次时保持独立报价。</summary>
        private async Task InitializeAsync()
        {
            var lease = _loadCoordinator.Begin();
            try
            {
                IReadOnlyList<QuoteConfirmRow> rows;
                try
                {
                    rows = await _quoteService.LoadPendingRowsAsync(lease.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception error)
                {
                    com.gateway.log.LogHelper.Error(error);
                    SyncState = "加载失败：" + error.Message;
                    ToastService.Show(SyncState, ToastKind.Error);
                    return;
                }

                if (!lease.IsCurrent) return;

                var selectedRows = Batches.SelectMany(batch => batch.Rows)
                    .Where(row => row.IsSelected)
                    .ToDictionary(row => row.RecognitionId);
                var expandedGroups = Batches.ToDictionary(batch => batch.BatchId, batch => batch.IsExpanded);
                UnhookAllRows();
                Batches.Clear();

                var groups = new Dictionary<string, List<QuoteConfirmRow>>();
                foreach (var row in rows)
                {
                    var key = !string.IsNullOrWhiteSpace(row.BatchId)
                        ? row.BatchId
                        : row.RecognitionId ?? row.ClientQuoteId;
                    List<QuoteConfirmRow> list;
                    if (!groups.TryGetValue(key, out list))
                    {
                        list = new List<QuoteConfirmRow>();
                        groups[key] = list;
                    }
                    list.Add(row);
                }

                foreach (var kv in groups)
                {
                    var first = kv.Value[0];
                    var batch = new QuoteConfirmBatchViewModel(new QuoteConfirmBatch
                    {
                        BatchId = kv.Key,
                        SourceType = first.SourceType,
                        Institution = first.BatchInstitution ?? first.Institution,
                        Trader = first.BatchTrader ?? first.Trader,
                        ReceivedAt = first.ReceivedAt,
                        Status = first.BatchStatus,
                        RowCount = kv.Value.Count,
                    });
                    bool expanded;
                    if (expandedGroups.TryGetValue(kv.Key, out expanded))
                    {
                        batch.IsExpanded = expanded;
                    }
                    foreach (var row in kv.Value)
                    {
                        // 推送仍须移除已确认/删除的报价；仅复用版本未变的选中行，保留未提交编辑。
                        QuoteConfirmRowViewModel rvm;
                        if (!selectedRows.TryGetValue(row.RecognitionId, out rvm)
                            || rvm.Row.Revision != row.Revision)
                        {
                            rvm = new QuoteConfirmRowViewModel(row);
                        }
                        HookRow(rvm);
                        batch.Rows.Add(rvm);
                    }
                    HookBatch(batch);
                    var captured = batch;
                    batch.Rows.CollectionChanged += (s, e) => OnBatchRowsChanged(captured, e);
                    Batches.Add(batch);
                    UpdateBatchChecked(batch);
                }

                RecomputeStats();
                PublishPendingBadge();
            }
            finally
            {
                lease.Dispose();
            }
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            _ = InitializeAsync();
        }

        #endregion

        #region 行钩子与统计

        private void HookRow(QuoteConfirmRowViewModel row)
        {
            if (_hookedRows.Add(row))
            {
                row.PropertyChanged += OnRowPropertyChanged;
            }
        }

        private void UnhookRow(QuoteConfirmRowViewModel row)
        {
            if (_hookedRows.Remove(row))
            {
                row.PropertyChanged -= OnRowPropertyChanged;
            }
        }

        private void HookBatch(QuoteConfirmBatchViewModel batch)
        {
            if (_hookedBatches.Add(batch)) batch.PropertyChanged += OnBatchPropertyChanged;
        }

        private void UnhookBatch(QuoteConfirmBatchViewModel batch)
        {
            if (_hookedBatches.Remove(batch)) batch.PropertyChanged -= OnBatchPropertyChanged;
        }

        private void UnhookAllRows()
        {
            foreach (var row in _hookedRows.ToList())
            {
                row.PropertyChanged -= OnRowPropertyChanged;
            }
            _hookedRows.Clear();
            foreach (var batch in _hookedBatches.ToList()) batch.PropertyChanged -= OnBatchPropertyChanged;
            _hookedBatches.Clear();
        }

        private void OnBatchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isUpdatingSelectionState || e.PropertyName != nameof(QuoteConfirmBatchViewModel.IsGroupChecked)) return;
            var batch = sender as QuoteConfirmBatchViewModel;
            if (batch == null || !batch.IsGroupChecked.HasValue) return;
            _isUpdatingSelectionState = true;
            try
            {
                foreach (var row in batch.Rows) row.IsSelected = batch.IsGroupChecked.Value;
            }
            finally { _isUpdatingSelectionState = false; }
            ResetToolbarSelection();
            RecomputeStats();
        }

        private void OnBatchRowsChanged(QuoteConfirmBatchViewModel batch, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (QuoteConfirmRowViewModel old in e.OldItems) UnhookRow(old);
            }
            if (e.NewItems != null)
            {
                foreach (QuoteConfirmRowViewModel added in e.NewItems) HookRow(added);
            }

            batch.NotifySummaryChanged();
            UpdateBatchChecked(batch);
            RecomputeStats();
        }

        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshToolbarFlags();
            if (_isUpdatingSelectionState || e.PropertyName != nameof(QuoteConfirmRowViewModel.IsSelected)) return;
            ResetToolbarSelection();

            // 行选择变化 → 刷新所属批次的组级三态与整体统计
            foreach (var batch in Batches)
            {
                if (batch.Rows.Contains((QuoteConfirmRowViewModel)sender))
                {
                    UpdateBatchChecked(batch);
                    break;
                }
            }
            RecomputeStats();
        }

        private void UpdateBatchChecked(QuoteConfirmBatchViewModel batch)
        {
            _isUpdatingSelectionState = true;
            try
            {
                if (batch.Rows.Count == 0) { batch.IsGroupChecked = false; return; }
                var selected = batch.Rows.Count(r => r.IsSelected);
                batch.IsGroupChecked = selected == 0 ? false : selected == batch.Rows.Count ? (bool?)true : null;
            }
            finally { _isUpdatingSelectionState = false; }
        }

        private void RecomputeStats()
        {
            var rows = AllRows.ToList();
            var selected = rows.Count(r => r.IsSelected);
            SelectedCount = selected;

            var ideal = Batches.Count(b => b.SourceType == "ideal");
            var qt = Batches.Count(b => b.SourceType == "qt");
            var ofr = rows.Count(r => r.Direction == "Ofr");
            var bid = rows.Count(r => r.Direction == "Bid");

            GroupStats = string.Format("共 {0} 组 / {1} 条报价", Batches.Count, rows.Count);
            SourceStats = string.Format("IDEAL: {0}组 | QT: {1}组", ideal, qt);
            SyncState = _quoteService is DpsSalesTradingServices ? "DPS实时订阅" : "模拟数据";

            _status.Set("group", GroupStats);
            _status.Set("source", SourceStats);
            _status.Set("ofr", "Ofr: " + ofr);
            _status.Set("bid", "Bid: " + bid);
            _status.Set("sync", SyncState);
            var allSelected = rows.Count > 0 && rows.All(r => r.IsSelected);
            IsSelectAllChecked = allSelected ? (bool?)true : (rows.Any(r => r.IsSelected) ? null : false);
            RefreshToolbarFlags();
        }

        private void PublishPendingBadge()
        {
            EventAggregator.Publish(new PendingQuotesChangedMessage(TotalRowCount));
        }

        partial void OnIsSelectAllCheckedChanged(bool? value)
        {
            if (value == null) return;
            foreach (var row in AllRows) row.IsSelected = value.Value;
        }

        #endregion

        #region 工具栏命令

        [RelayCommand]
        private void CompleteQuote()
        {
            var selected = SelectedRows.ToArray();
            if (!(_quoteService is DpsSalesTradingServices services) || selected.Length != 1)
            {
                ToastService.Show("请勾选一条报价，以补全CRM机构、联系人并确认。", ToastKind.Warning);
                return;
            }
            new STC.SalesTrading.Plugin.View.QuoteOperationWindow(services.Api, selected[0].RecognitionId)
            {
                Owner = System.Windows.Application.Current.MainWindow
            }.ShowDialog();
            _ = InitializeAsync();
        }

        [RelayCommand]
        private async Task ConfirmSelected()
        {
            var selected = SelectedRows.ToList();
            if (selected.Count == 0) { ToastService.Show("请先勾选要确认的报价", ToastKind.Warning); return; }

            try
            {
                foreach (var row in selected)
                {
                    row.Commit();
                }
                if (_quoteService is DpsSalesTradingServices services)
                {
                    await services.ConfirmRowsAsync(selected.Select(row => row.Row).ToArray(), CancellationToken.None);
                }
                else
                {
                    await _quoteService.ConfirmQuotesAsync(selected.Select(row => row.RecognitionId).ToList(), CancellationToken.None);
                }
                RemoveRows(selected);
                ToastService.Show(string.Format("已确认 {0} 条报价", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("确认报价失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteSelected()
        {
            var selected = SelectedRows.ToList();
            if (selected.Count == 0) { ToastService.Show("请先勾选要删除的报价", ToastKind.Warning); return; }

            try
            {
                if (_quoteService is DpsSalesTradingServices backend)
                {
                    await backend.DeletePendingAsync(selected.Select(row => row.Row), CancellationToken.None);
                }
                else
                {
                    await _quoteService.DeleteQuotesAsync(
                        selected.Select(row => row.RecognitionId).ToList(), CancellationToken.None);
                }
                RemoveRows(selected);
                ToastService.Show(string.Format("已删除 {0} 条报价", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("删除报价失败：" + ex.Message, ToastKind.Error);
            }
        }

        private void RemoveRows(IEnumerable<QuoteConfirmRowViewModel> rows)
        {
            var set = new HashSet<QuoteConfirmRowViewModel>(rows);
            foreach (var batch in Batches.ToList())
            {
                foreach (var row in batch.Rows.Where(set.Contains).ToList())
                {
                    batch.Rows.Remove(row);   // 触发 OnBatchRowsChanged → 统计刷新
                }
                if (batch.Rows.Count == 0)
                {
                    UnhookBatch(batch);
                    Batches.Remove(batch);
                }
            }
            RecomputeStats();
            PublishPendingBadge();
        }

        [RelayCommand]
        private void SetDirectionBid()
        {
            foreach (var row in RowsForBatchEdit()) row.Direction = "Bid";
        }

        [RelayCommand]
        private void SetDirectionOfr()
        {
            foreach (var row in RowsForBatchEdit()) row.Direction = "Ofr";
        }

        [ObservableProperty]
        private string _bpInput = string.Empty;

        [ObservableProperty]
        private string _amountInput = string.Empty;



        private int lastBpSign;
        private int lastAmountSign;
        private bool updatingAdjustmentInput;

        partial void OnBpInputChanged(string value)
        {
            if (!updatingAdjustmentInput) lastBpSign = 0;
        }

        partial void OnAmountInputChanged(string value)
        {
            if (!updatingAdjustmentInput) lastAmountSign = 0;
        }

        [RelayCommand]
        private void AdjustBpUp()
        {
            ApplyAdjustment(true, 1);
        }

        [RelayCommand]
        private void AdjustBpDown()
        {
            ApplyAdjustment(true, -1);
        }

        [RelayCommand]
        private void AdjustAmountUp()
        {
            ApplyAdjustment(false, 1);
        }

        [RelayCommand]
        private void AdjustAmountDown()
        {
            ApplyAdjustment(false, -1);
        }

        [RelayCommand]
        private void ApplyBpInput()
        {
            ApplyAdjustment(true, 0);
        }

        [RelayCommand]
        private void ApplyAmountInput()
        {
            ApplyAdjustment(false, 0);
        }

        private List<QuoteConfirmRowViewModel> RowsForBatchEdit()
        {
            var selected = SelectedRows.ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选要批量修改的报价", ToastKind.Warning);
            }
            return selected;
        }

        /// <summary>按钮固定调整1BP/1000万；输入框按 Enter 应用有符号增量，与产品原型一致。</summary>
        private void ApplyAdjustment(bool priceAdjustment, int buttonSign)
        {
            var selected = RowsForBatchEdit();
            if (selected.Count == 0) return;
            string input = priceAdjustment ? BpInput : AmountInput;
            decimal delta;
            decimal step = priceAdjustment ? 1m : 1000m;
            if (buttonSign == 0)
            {
                if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out delta) || delta == 0)
                {
                    ToastService.Show("请输入非零数字调整值，然后按 Enter", ToastKind.Warning);
                    return;
                }
            }
            else
            {
                delta = step * buttonSign;
                int lastSign = priceAdjustment ? lastBpSign : lastAmountSign;
                decimal previous;
                decimal display = lastSign == buttonSign && decimal.TryParse(input, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out previous) ? Math.Abs(previous) + step : step;
                updatingAdjustmentInput = true;
                try
                {
                    if (priceAdjustment)
                    {
                        BpInput = (display * buttonSign).ToString(CultureInfo.InvariantCulture);
                        lastBpSign = buttonSign;
                    }
                    else
                    {
                        AmountInput = (display * buttonSign).ToString(CultureInfo.InvariantCulture);
                        lastAmountSign = buttonSign;
                    }
                }
                finally
                {
                    updatingAdjustmentInput = false;
                }
            }
            int skipped = 0;
            foreach (var row in selected)
            {
                decimal current;
                if (!decimal.TryParse(priceAdjustment ? row.Price : row.Volume, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out current))
                {
                    skipped++;
                    continue;
                }
                // 收益率以百分数展示，1BP=0.01；净价/全价按原型使用价格单位增量。
                decimal unit = priceAdjustment && row.PriceType != "净价" && row.PriceType != "全价" ? 0.01m : 1m;
                string adjusted = Math.Max(0, current + delta * unit).ToString("0.####", CultureInfo.InvariantCulture);
                if (priceAdjustment) row.Price = adjusted;
                else row.Volume = adjusted;
            }
            if (skipped > 0)
            {
                ToastService.Show(string.Format("有 {0} 条报价数值无效，已跳过", skipped), ToastKind.Warning);
            }
        }

        private void ResetToolbarSelection()
        {
            // 新一组勾选行仍能应用上次选择的类型/清算，避免默认值无 SelectionChanged 事件。
            ToolbarPriceType = null;
            ToolbarSettlement = null;
            RefreshToolbarFlags();
        }

        /// <summary>工具栏价格类型下拉（变化即批量应用到选中行，原型 batchSetPriceType）。</summary>
        [ObservableProperty]
        private string _toolbarPriceType;

        partial void OnToolbarPriceTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var row in RowsForBatchEdit()) row.PriceType = value;
        }

        /// <summary>工具栏清算速度下拉（变化即批量应用，原型 batchSetSettlement）。</summary>
        [ObservableProperty]
        private string _toolbarSettlement;

        partial void OnToolbarSettlementChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var row in RowsForBatchEdit()) row.Settlement = value;
        }



        public bool ToolbarUrgent => AllSelectedFlag("urgent");
        public bool ToolbarRecommended => AllSelectedFlag("rec");
        public bool ToolbarWhole => AllSelectedFlag("whole");
        public bool ToolbarOco => AllSelectedFlag("oco");
        public bool ToolbarPacking => AllSelectedFlag("packing");

        private bool AllSelectedFlag(string flag)
        {
            var selected = SelectedRows.ToList();
            return selected.Count > 0 && selected.All(row => FlagValue(row, flag));
        }

        private static bool FlagValue(QuoteConfirmRowViewModel row, string flag)
        {
            switch (flag)
            {
                case "urgent": return row.IsUrgent;
                case "rec": return row.IsRec;
                case "whole": return row.IsWhole;
                case "oco": return row.IsOco;
                case "packing": return row.IsPacking;
                default: throw new ArgumentException("Unknown quote flag: " + flag);
            }
        }

        private void RefreshToolbarFlags()
        {
            OnPropertyChanged(nameof(ToolbarUrgent));
            OnPropertyChanged(nameof(ToolbarRecommended));
            OnPropertyChanged(nameof(ToolbarWhole));
            OnPropertyChanged(nameof(ToolbarOco));
            OnPropertyChanged(nameof(ToolbarPacking));
        }

        /// <summary>混合状态统一打开；全选中时统一关闭；OCO与打包互斥。</summary>
        [RelayCommand]
        private void ToggleFlag(string flag)
        {
            var selected = RowsForBatchEdit();
            bool enabled = selected.Any(row => !FlagValue(row, flag));
            foreach (var row in selected)
            {
                switch (flag)
                {
                    case "oco":
                        row.IsOco = enabled;
                        if (enabled) row.IsPacking = false;
                        break;
                    case "packing":
                        row.IsPacking = enabled;
                        if (enabled) row.IsOco = false;
                        break;
                    case "urgent": row.IsUrgent = enabled; break;
                    case "rec": row.IsRec = enabled; break;
                    case "whole": row.IsWhole = enabled; break;
                }
            }
            RefreshToolbarFlags();
        }

        #endregion

        #region 释放

        private void OnBackendChanged()
        {
            _ = InitializeAsync();
        }

        public void Dispose()
        {
            if (_quoteService is ISalesTradingUpdates updates)
            {
                updates.QuotesChanged -= OnBackendChanged;
            }
            _loadCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
            UnhookAllRows();
            // 批次行集合的 CollectionChanged 用闭包订阅，随 Batches 一起被 GC，无需显式退订
        }

        #endregion
    }
}
