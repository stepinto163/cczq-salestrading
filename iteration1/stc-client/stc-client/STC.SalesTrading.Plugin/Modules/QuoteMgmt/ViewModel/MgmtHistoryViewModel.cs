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
    /// 历史报价 VM。按日终分组的 22 列表格（无报价状态列）+ 工具栏（Unrefer/删除/Copy/导出 +
    /// 代码·名称/报价机构/长城交易员三联想 + 日期区间）。
    /// </summary>
    public partial class MgmtHistoryViewModel : ObservableObject, IDisposable, IQueryLoadContext    {
        private readonly IQuoteMgmtService _mgmtService;
        private readonly LatestRequestCoordinator _queryCoordinator = new LatestRequestCoordinator();
        private readonly DebouncedRequestScheduler _textQueryScheduler = new DebouncedRequestScheduler();
        private static readonly TimeSpan TextQueryDebounce = TimeSpan.FromMilliseconds(300);

        public MgmtHistoryViewModel(IQuoteMgmtService mgmtService)
        {
            _mgmtService = mgmtService;

            _status.Add("total", "共 0 条")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6");

            View = CollectionViewSource.GetDefaultView(Rows);

            // 默认上一工作日（周日回退到上周五）
            var d = DateTime.Today.AddDays(-1);
            if (d.DayOfWeek == DayOfWeek.Sunday) d = d.AddDays(-2);
            else if (d.DayOfWeek == DayOfWeek.Saturday) d = d.AddDays(-1);
            StartDate = d.AddDays(-7);
            EndDate = d;

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

        public string Title { get; } = "历史报价";

        public ObservableCollection<QuoteRow> Rows { get; } = new ObservableCollection<QuoteRow>();

        public ICollectionView View { get; }

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        #region 过滤条件

        /// <summary>开始日期。</summary>
        [ObservableProperty]
        private DateTime _startDate;

        /// <summary>结束日期。</summary>
        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private string _bondSearchText = string.Empty;

        [ObservableProperty]
        private string _institutionSearchText = string.Empty;

        public ObservableCollection<string> BondSuggestions { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> InstitutionSuggestions { get; } = new ObservableCollection<string>();


        partial void OnStartDateChanged(DateTime value) => ReloadImmediate();
        partial void OnEndDateChanged(DateTime value) => ReloadImmediate();
        partial void OnBondSearchTextChanged(string value) => ReloadText();
        partial void OnInstitutionSearchTextChanged(string value) => ReloadText();

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

                        private async System.Threading.Tasks.Task QueryAsync(bool silent = false)
        {
            await QueryFlow.RunAsync(_queryCoordinator, this, async lease =>
            {
                var result = await _mgmtService.QueryHistoryRowsAsync(new QuoteMgmtQuery
                {
                    Today = false,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    BondSearchText = BondSearchText,
                    InstitutionSearchText = InstitutionSearchText,
                    // 原型历史无分页控件：一次取足（后端游标协议保留，二期滚动加载复用）
                    PageSize = 500,
                }, lease.Token);
                if (!lease.IsCurrent) return null;

                Rows.Clear();
                foreach (var row in result.Items) Rows.Add(row);
                TotalCount = result.TotalCount;
                QueryId = result.QueryId;
                IsPartial = result.IsPartial;
                BuildSuggestions();
                return Rows.Count > 0;
            }, Title);
        }

        private void Reload()
        {
            ReloadImmediate();
        }

        private void ReloadImmediate()
        {
            _textQueryScheduler.Cancel();
            _queryCoordinator.CancelCurrent();
            _ = QueryAsync(true);
        }

        private void ReloadText()
        {
            _queryCoordinator.CancelCurrent();
            _textQueryScheduler.Schedule(TextQueryDebounce, token => QueryAsync());
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Retry()
        {
            await QueryAsync();
        }

        #endregion

        #region 工具栏命令


        [RelayCommand]
        private void Copy()
        {
            var selected = Rows.Where(r => SelectedRow.Contains(r)).ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选要复制的报价");
                return;
            }
            var text = string.Join(Environment.NewLine, selected.Select(r =>
                $"{r.SecurityId} {r.Side} {r.Price?.ToString("0.####")} {r.Quantity}"));
            ClipboardHelper.CopyWithToast(text);
        }

        [RelayCommand]
        private void Export()
        {
            ToastService.Show("导出 xlsx 待接（原型 SheetJS）");
        }

        private void BuildSuggestions()
        {
            BondSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.SecurityId).Concat(Rows.Select(r => r.SecurityName)).Where(x => !string.IsNullOrEmpty(x)).Distinct()) BondSuggestions.Add(s);
            InstitutionSuggestions.Clear();
            foreach (var s in Rows.Select(r => r.InstitutionName).Where(x => !string.IsNullOrEmpty(x)).Distinct()) InstitutionSuggestions.Add(s);
        }

        #endregion

        public HashSet<QuoteRow> SelectedRow { get; } = new HashSet<QuoteRow>();

        public void Dispose()
        {
            EventAggregator.Unsubscribe(this);
            _textQueryScheduler.Dispose();
            _queryCoordinator.Dispose();
            SelectedRow.Clear();
        }
    }
}
