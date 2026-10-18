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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel
{
    /// <summary>
    /// 今日报价 VM。23 列表格 + 工具栏（Refer/Unrefer/删除/Copy/导出 + 报价方式/状态两下拉 +
    /// 代码·名称/报价机构/长城交易员三联想搜索）+ 状态栏统计。
    /// </summary>
    public partial class MgmtTodayViewModel : ObservableObject, IDisposable, IQueryLoadContext    {
        private readonly IQuoteMgmtService _mgmtService;
        private readonly LatestRequestCoordinator _queryCoordinator = new LatestRequestCoordinator();
        private readonly DebouncedRequestScheduler _textQueryScheduler = new DebouncedRequestScheduler();
        private static readonly TimeSpan TextQueryDebounce = TimeSpan.FromMilliseconds(300);

        public MgmtTodayViewModel(IQuoteMgmtService mgmtService)
        {
            _mgmtService = mgmtService;

            StatusOptions = new List<string> { "全部", "有效", "撤销" };

            _status.Add("total", "共 0 条")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6");

            View = CollectionViewSource.GetDefaultView(Rows);

            // 推送刷新（方案 §3.3.1）：视图变化由服务端推、客户端按需重查
            _mgmtService.QuotesChanged += OnQuotesChanged;

            // 数据源复位（断线/重登）：清态重查（Dispatcher 调度见 DispatchRefresh/ReloadImmediate）
            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);

            _ = QueryAsync();
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(ReloadImmediate));
                return;
            }
            ReloadImmediate();
        }

        public string Title { get; } = "今日报价";

        public ObservableCollection<QuoteRow> Rows { get; } = new ObservableCollection<QuoteRow>();

        public ICollectionView View { get; }

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }


        public IReadOnlyList<string> StatusOptions { get; }

        #region 过滤条件

        /// <summary>报价方式下拉选中值（全部/QT/ideal/手工录入）。</summary>

        /// <summary>报价状态下拉选中值（全部/有效/撤销）。</summary>
        [ObservableProperty]
        private string _selectedStatus = "全部";

        /// <summary>债券代码/名称联想搜索文本。</summary>
        [ObservableProperty]
        private string _bondSearchText = string.Empty;

        /// <summary>报价机构联想搜索文本。</summary>
        [ObservableProperty]
        private string _institutionSearchText = string.Empty;

        /// <summary>债券联想候选（从数据提取，接网关后由服务提供）。</summary>
        public ObservableCollection<string> BondSuggestions { get; } = new ObservableCollection<string>();

        /// <summary>报价机构联想候选。</summary>
        public ObservableCollection<string> InstitutionSuggestions { get; } = new ObservableCollection<string>();

        /// <summary>长城交易员联想候选。</summary>

        /// <summary>当前页（1-based；PaginationBar TwoWay）。</summary>
        [ObservableProperty]
        private int _page = 1;

        /// <summary>总页数（TotalCount / 50）。</summary>
        [ObservableProperty]
        private int _totalPages = 1;

        partial void OnPageChanged(int value)
        {
            _ = QueryAsync();
        }

        partial void OnSelectedStatusChanged(string value) { Page = 1; ReloadImmediate(); }
        partial void OnBondSearchTextChanged(string value) { Page = 1; ReloadText(); }
        partial void OnInstitutionSearchTextChanged(string value) { Page = 1; ReloadText(); }

        private void RecomputeStats()
        {
            _status.Set("total", string.Format("共 {0} 条", TotalCount));
            _status.Set("bid", "Bid: " + Rows.Count(r => r.Side == QuoteDirection.Bid));
            _status.Set("ofr", "Ofr: " + Rows.Count(r => r.Side == QuoteDirection.Ofr));
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

        private ProductFilter productFilter;
        private string searchText;

        public void ApplyFilters(ProductFilter filter, string text)
        {
            productFilter = filter;
            searchText = text;
            SelectedRow.Clear();
            ReloadImmediate();
        }

        private void ReloadImmediate()
        {
            _textQueryScheduler.Cancel();
            _queryCoordinator.CancelCurrent();
            _ = QueryAsync();
        }

        private void ReloadText()
        {
            _queryCoordinator.CancelCurrent();
            _textQueryScheduler.Schedule(TextQueryDebounce, token => QueryAsync());
        }

        private async System.Threading.Tasks.Task QueryAsync(bool silent = false, bool forceRefresh = false)
        {
            await QueryFlow.RunAsync(_queryCoordinator, this, async lease =>
            {
                var result = await _mgmtService.QueryTodayRowsAsync(new QuoteMgmtQuery
                {
                    Today = true,
                    ForceRefresh = forceRefresh,
                    Status = SelectedStatus,
                    ProductFilter = productFilter,
                    SearchText = searchText,
                    BondSearchText = BondSearchText,
                    InstitutionSearchText = InstitutionSearchText,
                    Page = Page,
                    PageSize = 50,
                }, lease.Token);
                if (!lease.IsCurrent) return null;

                Rows.Clear();
                foreach (var row in result.Items) Rows.Add(row);
                TotalCount = result.TotalCount;
                TotalPages = (int)Math.Ceiling(result.TotalCount / 50.0);
                QueryId = result.QueryId;
                IsPartial = result.IsPartial;
                BuildSuggestions();
                return Rows.Count > 0;
            }, Title);
        }

        public async System.Threading.Tasks.Task RefreshAfterEditAsync()
        {
            SelectedRow.Clear();
            _pendingRefresh = false;
            await QueryAsync(forceRefresh: true);
        }

        private void BuildSuggestions()
        {
            BondSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.SecurityId).Concat(Rows.Select(r => r.SecurityName)).Where(x => !string.IsNullOrEmpty(x)).Distinct()) BondSuggestions.Add(s);
            InstitutionSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.InstitutionName).Where(x => !string.IsNullOrEmpty(x)).Distinct()) InstitutionSuggestions.Add(s);
        }

        #endregion

        #region 工具栏命令

        [RelayCommand]
        private async System.Threading.Tasks.Task Refer()
        {
            var actionable = SelectedActionable(QuoteStatus.Active, "仅有效（ACTIVE）报价可 Refer");
            if (actionable == null) return;
            try
            {
                await _mgmtService.ReferQuotesAsync(actionable, CancellationToken.None);
                await QueryAsync();
                ToastService.Show(string.Format("已 Refer {0} 条报价", actionable.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("Refer 失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Unrefer()
        {
            var actionable = SelectedActionable(QuoteStatus.Referred, "仅已撤销（REFERRED）报价可 Unrefer");
            if (actionable == null) return;
            try
            {
                await _mgmtService.UnreferQuotesAsync(actionable, CancellationToken.None);
                await QueryAsync();
                ToastService.Show(string.Format("已 Unrefer {0} 条报价", actionable.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("Unrefer 失败：" + ex.Message, ToastKind.Error);
            }
        }

        /// <summary>取勾选行并校验生命周期状态；含不满足状态的行时提示并剔除，全部不满足返回 null。</summary>
        private List<QuoteRow> SelectedActionable(QuoteStatus required, string mismatchHint)
        {
            var selected = Rows.Where(r => SelectedRow?.Contains(r) == true).ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选报价行");
                return null;
            }
            var actionable = selected.Where(r => r.Status == required).ToList();
            if (actionable.Count == 0)
            {
                ToastService.Show(mismatchHint, ToastKind.Warning);
                return null;
            }
            if (actionable.Count < selected.Count)
            {
                ToastService.Show(mismatchHint + "，已跳过 " + (selected.Count - actionable.Count) + " 条", ToastKind.Warning);
            }
            return actionable;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Delete()
        {
            var selected = Rows.Where(r => SelectedRow?.Contains(r) == true).ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选要删除的报价");
                return;
            }
            try
            {
                await _mgmtService.DeleteQuotesAsync(selected, false, CancellationToken.None);
                SelectedRow.Clear();
                await QueryAsync();
                ToastService.Show(string.Format("已删除 {0} 条报价", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("删除报价失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private void Copy()
        {
            var selected = Rows.Where(r => SelectedRow?.Contains(r) == true).ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选要复制的报价");
                return;
            }
            var text = string.Join(Environment.NewLine, selected.Select(r =>
                $"{r.SourceTypeText} {r.SecurityId} {r.Side} {r.Price?.ToString("0.####")} {r.Quantity}"));
            ClipboardHelper.CopyWithToast(text);
        }

        [RelayCommand]
        private void Export()
        {
            ToastService.Show("导出 xlsx 待接（原型 SheetJS）");
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Retry()
        {
            await QueryAsync();
        }

        #endregion

        #region 行选择（多选）

        /// <summary>当前选中报价 Id 集合（DataGrid SelectedItems 由 View 代码同步）。</summary>
        public HashSet<QuoteRow> SelectedRow { get; } = new HashSet<QuoteRow>();

        /// <summary>View 同步选中集合后调用：释放选择时补一次被推送推迟的刷新。</summary>
        internal void OnSelectionSynced()
        {
            if (!_pendingRefresh || SelectedRow.Count > 0) return;
            _pendingRefresh = false;
            DispatchRefresh();
        }

        #endregion

        #region 推送刷新

        private bool _disposed;
        private bool _pendingRefresh;

        /// <summary>
        /// 服务端报价集合变化：无选中直接重查；有选中（用户正在查看/操作）先置脏，
        /// 待释放选择后统一刷新（与看板"手动态不打断"语义一致）。
        /// </summary>
        private void OnQuotesChanged()
        {
            if (_disposed) return;
            if (SelectedRow.Count > 0)
            {
                _pendingRefresh = true;
                return;
            }
            DispatchRefresh();
        }

        private void DispatchRefresh()
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = QueryAsync(true)));
                return;
            }
            _ = QueryAsync(true);
        }

        #endregion

        public void Dispose()
        {
            _disposed = true;
            EventAggregator.Unsubscribe(this);
            _mgmtService.QuotesChanged -= OnQuotesChanged;
            _textQueryScheduler.Dispose();
            _queryCoordinator.Dispose();
            SelectedRow.Clear();
        }
    }
}
