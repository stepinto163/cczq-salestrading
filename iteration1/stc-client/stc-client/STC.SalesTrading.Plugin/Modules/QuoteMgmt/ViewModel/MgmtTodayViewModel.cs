using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Shared.Converters;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using STC.SalesTrading.Plugin.Ficc;
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
            if (mgmtService is ISalesTradingUpdates updates)
            {
                updates.QuotesChanged += OnBackendChanged;
            }

            ModeOptions = new List<string> { "全部", "QT", "ideal", "手工录入" };
            StatusOptions = new List<string> { "全部", "有效", "撤销" };

            _status.Add("total", "共 0 条")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6");

            View = CollectionViewSource.GetDefaultView(Rows);

            _ = QueryAsync();
        }

        public string Title { get; } = "今日报价";

        public ObservableCollection<MgmtQuoteRow> Rows { get; } = new ObservableCollection<MgmtQuoteRow>();

        public ICollectionView View { get; }

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        public IReadOnlyList<string> ModeOptions { get; }

        public IReadOnlyList<string> StatusOptions { get; }

        #region 过滤条件

        /// <summary>报价方式下拉选中值（全部/QT/ideal/手工录入）。</summary>
        [ObservableProperty]
        private string _selectedMode = "全部";

        /// <summary>报价状态下拉选中值（全部/有效/撤销）。</summary>
        [ObservableProperty]
        private string _selectedStatus = "全部";

        /// <summary>债券代码/名称联想搜索文本。</summary>
        [ObservableProperty]
        private string _bondSearchText = string.Empty;

        /// <summary>报价机构联想搜索文本。</summary>
        [ObservableProperty]
        private string _institutionSearchText = string.Empty;

        /// <summary>长城交易员联想搜索文本。</summary>
        [ObservableProperty]
        private string _traderSearchText = string.Empty;

        /// <summary>债券联想候选（从数据提取，接网关后由服务提供）。</summary>
        public ObservableCollection<string> BondSuggestions { get; } = new ObservableCollection<string>();

        /// <summary>报价机构联想候选。</summary>
        public ObservableCollection<string> InstitutionSuggestions { get; } = new ObservableCollection<string>();

        /// <summary>长城交易员联想候选。</summary>
        public ObservableCollection<string> TraderSuggestions { get; } = new ObservableCollection<string>();

        partial void OnSelectedModeChanged(string value) => ReloadImmediate();
        partial void OnSelectedStatusChanged(string value) => ReloadImmediate();
        partial void OnBondSearchTextChanged(string value) => ReloadText();
        partial void OnInstitutionSearchTextChanged(string value) => ReloadText();
        partial void OnTraderSearchTextChanged(string value) => ReloadText();

        private void RecomputeStats()
        {
            _status.Set("total", string.Format("共 {0} 条", TotalCount));
            _status.Set("bid", "Bid: " + Rows.Count(r => r.Direction == QuoteDirection.Bid));
            _status.Set("ofr", "Ofr: " + Rows.Count(r => r.Direction == QuoteDirection.Ofr));
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

        /// <summary>报价方式显示文本（供列渲染与联想）。</summary>
        public static string ModeLabel(QuoteMode mode)
        {
            switch (mode)
            {
                case QuoteMode.Qt: return "QT";
                case QuoteMode.Ideal: return "ideal";
                case QuoteMode.Manual: return "手工录入";
                default: return string.Empty;
            }
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

        private STC.SalesTrading.Plugin.Ficc.Protocol.Filter productFilter;

        public void ApplyProductFilter(STC.SalesTrading.Plugin.Ficc.Protocol.Filter filter)
        {
            productFilter = filter;
            _queryCoordinator.CancelCurrent();
            _ = QueryAsync();
        }

        private async System.Threading.Tasks.Task QueryAsync()
        {
            await QueryFlow.RunAsync(_queryCoordinator, this, async lease =>
            {
                var result = await _mgmtService.QueryTodayRowsAsync(new QuoteMgmtQuery
                {
                    ProductFilter = productFilter,
                    Today = true,
                    Mode = SelectedMode,
                    Status = SelectedStatus,
                    BondSearchText = BondSearchText,
                    InstitutionSearchText = InstitutionSearchText,
                    TraderSearchText = TraderSearchText,
                }, lease.Token);
                if (!lease.IsCurrent) return null;

                long collectionStarted = ClientPerformanceTrace.Start();
                Rows.Clear();
                foreach (var row in result.Items)
                {
                    Rows.Add(row);
                }
                ClientPerformanceTrace.End("today.collection", collectionStarted);
                long propertiesStarted = ClientPerformanceTrace.Start();
                TotalCount = result.TotalCount;
                QueryId = result.QueryId;
                IsPartial = result.IsPartial;
                ClientPerformanceTrace.End("today.properties", propertiesStarted);
                long suggestionsStarted = ClientPerformanceTrace.Start();
                BuildSuggestions();
                ClientPerformanceTrace.End("today.suggestions", suggestionsStarted);
                return Rows.Count > 0;
            }, Title);
        }

        private void BuildSuggestions()
        {
            BondSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.Code).Concat(Rows.Select(r => r.Name)).Where(x => !string.IsNullOrEmpty(x)).Distinct()) BondSuggestions.Add(s);
            InstitutionSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.Institution).Where(x => !string.IsNullOrEmpty(x)).Distinct()) InstitutionSuggestions.Add(s);
            TraderSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.WallTrader).Where(x => !string.IsNullOrEmpty(x)).Distinct()) TraderSuggestions.Add(s);
        }

        #endregion

        #region 工具栏命令

        [RelayCommand]
        private void EditQuote()
        {
            if (!(_mgmtService is DpsSalesTradingServices services) || SelectedRow.Count != 1)
            {
                ToastService.Show("请选择一条报价。", ToastKind.Warning);
                return;
            }
            new STC.SalesTrading.Plugin.View.QuoteOperationWindow(services.Api, SelectedRow.First().QuoteId)
            {
                Owner = System.Windows.Application.Current.MainWindow
            }.ShowDialog();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Refer()
        {
            await OperateSelectedAsync("refer");
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Unrefer()
        {
            await OperateSelectedAsync("unrefer");
        }

        private async System.Threading.Tasks.Task OperateSelectedAsync(string operation)
        {
            try
            {
                if (!(_mgmtService is DpsSalesTradingServices services))
                {
                    throw new InvalidOperationException("当前数据源不支持真实报价操作。");
                }
                await services.OperateAsync(operation, SelectedRow.ToArray(), CancellationToken.None);
                await QueryAsync();
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
                ToastService.Show(error.Message, ToastKind.Error);
            }
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
                if (_mgmtService is DpsSalesTradingServices services)
                {
                    await services.OperateAsync("delete", selected, CancellationToken.None);
                }
                else
                {
                    await _mgmtService.DeleteQuotesAsync(selected.Select(row => row.QuoteId).ToList(), false, CancellationToken.None);
                }
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
                $"{ModeLabel(r.Mode)} {r.Code} {r.Direction} {r.Price?.ToString("0.####")} {r.Volume}"));
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
        public HashSet<MgmtQuoteRow> SelectedRow { get; } = new HashSet<MgmtQuoteRow>();

        #endregion

        private void OnBackendChanged()
        {
            _ = QueryAsync();
        }

        public void Dispose()
        {
            if (_mgmtService is ISalesTradingUpdates updates)
            {
                updates.QuotesChanged -= OnBackendChanged;
            }
            _textQueryScheduler.Dispose();
            _queryCoordinator.Dispose();
            SelectedRow.Clear();
        }
    }
}
