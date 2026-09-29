using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using STC.SalesTrading.Plugin.Shell;
using System;
using System.Collections.ObjectModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel
{
    /// <summary>报价管理容器 VM：2 子页（今日报价/历史报价）切换 + 左侧筛选栏折叠（复用 filters-merged）。</summary>
    public partial class QuoteMgmtViewModel : ObservableObject, IDisposable, IProductFilterHost
    {
        private readonly IQuoteMgmtService service;
        private STC.SalesTrading.Plugin.Ficc.Protocol.Filter sidebarFilter;

        [ObservableProperty]
        private string _filterSearchText = string.Empty;

        partial void OnFilterSearchTextChanged(string value)
        {
            ApplyProductFilter("Combined", sidebarFilter);
        }

        public System.Threading.Tasks.Task<STC.SalesTrading.Plugin.Ficc.Protocol.DictionaryData> GetFilterDictionaryAsync()
        {
            if (service is IProductFilterCatalog catalog) return catalog.GetFilterDictionaryAsync();
            throw new InvalidOperationException("当前报价服务未提供筛选字典。");
        }

        public void ApplyProductFilter(string scope, STC.SalesTrading.Plugin.Ficc.Protocol.Filter filter)
        {
            sidebarFilter = filter;
            var combined = new STC.SalesTrading.Plugin.Ficc.Protocol.Filter
            {
                Conditions = new System.Collections.Generic.List<STC.SalesTrading.Plugin.Ficc.Protocol.FilterCondition>(
                    filter?.Conditions ?? new System.Collections.Generic.List<STC.SalesTrading.Plugin.Ficc.Protocol.FilterCondition>())
            };
            if (!string.IsNullOrWhiteSpace(FilterSearchText))
                combined.Conditions.Add(new STC.SalesTrading.Plugin.Ficc.Protocol.FilterCondition
                { Field = "searchText", Operator = "CONTAINS", Value = FilterSearchText.Trim() });
            Today.ApplyProductFilter(combined);
            History.ApplyProductFilter(combined);
        }

        public QuoteMgmtViewModel(IQuoteMgmtService mgmtService)
        {
            service = mgmtService;
            Today = new MgmtTodayViewModel(mgmtService);
            History = new MgmtHistoryViewModel(mgmtService);

            SubTabs = new ObservableCollection<SubTabItem>
            {
                new SubTabItem("今日报价", Today),
                new SubTabItem("历史报价", History),
            };

            _selectedSubTab = SubTabs[0];
            _selectedSubPage = Today;
        }

        public MgmtTodayViewModel Today { get; }

        public MgmtHistoryViewModel History { get; }

        public ObservableCollection<SubTabItem> SubTabs { get; }

        [ObservableProperty]
        private SubTabItem _selectedSubTab;

        [ObservableProperty]
        private object _selectedSubPage;

        partial void OnSelectedSubTabChanged(SubTabItem value)
        {
            if (value != null) SelectedSubPage = value.Target;
        }

        [RelayCommand]
        private void SwitchSubTab(SubTabItem item)
        {
            if (item != null) SelectedSubTab = item;
        }

        #region 左侧筛选栏折叠（对齐行情中心 384↔40px）

        private const double FilterExpandedWidth = 384;
        private const double FilterCollapsedWidth = 40;

        [ObservableProperty]
        private bool _isFilterExpanded = true;

        [ObservableProperty]
        private double _filterPanelWidth = FilterExpandedWidth;

        partial void OnIsFilterExpandedChanged(bool value)
        {
            FilterPanelWidth = value ? FilterExpandedWidth : FilterCollapsedWidth;
        }

        [RelayCommand]
        private void ToggleFilterPanel()
        {
            IsFilterExpanded = !IsFilterExpanded;
        }

        #endregion

        public void Dispose()
        {
            Today.Dispose();
            History.Dispose();
        }
    }
}
