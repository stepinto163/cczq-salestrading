using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 模糊报价 VM：7 列可编辑表（报价内容行内编辑）+ 工具栏（更新报价/Copy/Refer/Unrefer/导出 + 日期区间）+ 未同步红点。
    /// </summary>
    public partial class BoardIntentViewModel : ObservableObject, IDisposable, IQueryLoadContext    {
        private readonly IMarketCenterService _marketCenterService;
        private readonly LatestRequestCoordinator _loadCoordinator = new LatestRequestCoordinator();

        public BoardIntentViewModel(IMarketCenterService marketCenterService)
        {
            _marketCenterService = marketCenterService;

            _status.Add("count", "当前 0 条 / 区间总数 0 条");

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
            // 模糊报价本期不上（R9 后端未实现、tab 已隐藏）：不发起初始查询，避免重进时错误弹窗
        }

        public string Title { get; } = "模糊报价";

        public ObservableCollection<IntentRowViewModel> Rows { get; } = new ObservableCollection<IntentRowViewModel>();

        /// <summary>区间内全量行（Rows = 应用搜索后的可见子集）。</summary>
        private List<IntentRowViewModel> _allRows = new List<IntentRowViewModel>();

        /// <summary>搜索词（原型 intentSearchInput：匹配 报价内容/识别结果/机构/交易员）。</summary>
        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            ApplySearch();
        }

        /// <summary>按搜索词过滤 _allRows 回填 Rows（大小写不敏感 contains）。</summary>
        private void ApplySearch()
        {
            var keyword = (SearchText ?? string.Empty).Trim();
            IEnumerable<IntentRowViewModel> source = _allRows;
            if (keyword.Length > 0)
            {
                source = _allRows.Where(r =>
                    Contains(r.Content) || Contains(r.RecognizedConditions) || Contains(r.Institution) || Contains(r.Trader));
            }
            Rows.Clear();
            foreach (var sourceRow in source)
            {
                Rows.Add(sourceRow);
            }
            RecomputeStats();

            bool Contains(string value)
            {
                return !string.IsNullOrEmpty(value) && value.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }


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

        /// <summary>开始日期（MM-dd）。</summary>
        [ObservableProperty]
        private string _startDate = DateTime.Now.ToString("MM-dd");

        /// <summary>结束日期（MM-dd）。</summary>
        [ObservableProperty]
        private string _endDate = DateTime.Now.ToString("MM-dd");

        partial void OnStartDateChanged(string value) { _ = LoadAsync(); }
        partial void OnEndDateChanged(string value) { _ = LoadAsync(); }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            DateTime start, end;
            if (!DateTime.TryParse(StartDate, out start)) start = DateTime.Now.Date;
            if (!DateTime.TryParse(EndDate, out end)) end = DateTime.Now.Date;
            if (end < start) end = start;

            await QueryFlow.RunAsync(_loadCoordinator, this, async lease =>
            {
                var rows = await _marketCenterService.LoadIntentRowsAsync(start, end, lease.Token);
                if (!lease.IsCurrent) return null;

                _allRows = rows.Select(row => new IntentRowViewModel(row)).ToList();
                ApplySearch();
                TotalCount = _allRows.Count;
                QueryId = null;
                IsPartial = false;
                return Rows.Count > 0;
            }, Title);
        }

        private void RecomputeStats()
        {
            _status.Set("count", string.Format("当前 {0} 条 / 区间总数 {1} 条", Rows.Count, _allRows.Count));
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

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            // R9 未实现，静默（与构造一致不发查询）
        }

        #region 工具栏命令

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadAsync();
            ToastService.Show("模糊报价已刷新（模拟）", ToastKind.Success);
        }

        [RelayCommand]
        private async Task Retry()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private void Copy()
        {
            ToastService.Show("Copy 模糊报价文本待接");
        }

        [RelayCommand]
        private async Task Refer()
        {
            await ChangeSelectedStatusAsync(IntentStatus.Active);
        }

        [RelayCommand]
        private async Task Unrefer()
        {
            await ChangeSelectedStatusAsync(IntentStatus.Revoked);
        }

        private async Task ChangeSelectedStatusAsync(IntentStatus status)
        {
            var selected = Rows.Where(r => r.IsSelected).ToList();
            if (selected.Count == 0) { ToastService.Show("请先选择模糊报价", ToastKind.Warning); return; }
            try
            {
                await _marketCenterService.SetIntentStatusAsync(
                    selected.Select(row => row.Row.IntentId).ToList(),
                    status,
                    CancellationToken.None);
                await LoadAsync();
                ToastService.Show(string.Format("已{0} {1} 条模糊报价", status == IntentStatus.Active ? "Refer" : "Unrefer", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("更新模糊报价状态失败：" + ex.Message, ToastKind.Error);
            }
        }

        [RelayCommand]
        private void Export()
        {
            ToastService.Show("导出 xlsx 待接（原型 SheetJS）");
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
            _loadCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
        }
    }
}
