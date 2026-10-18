using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using STC.SalesTrading.Plugin.Shell;
using System;
using STC.SalesTrading.Plugin.Shared.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel
{
    /// <summary>报价管理容器 VM：2 子页（今日报价/历史报价）切换 + 左侧筛选栏折叠（复用 filters-merged）。</summary>
    public partial class QuoteMgmtViewModel : ObservableObject, IDisposable, IProductFilterHost
    {
        public QuoteMgmtViewModel(IQuoteMgmtService mgmtService, IGroupDirectory groupDirectory)
        {
            this.groupDirectory = groupDirectory;
            Today = new MgmtTodayViewModel(mgmtService);
            History = new MgmtHistoryViewModel(mgmtService);

            SubTabs = new ObservableCollection<SubTabItem>
            {
                new SubTabItem("今日报价", Today),
                new SubTabItem("历史报价", History),
            };

            _selectedSubTab = SubTabs[0];
            _selectedSubPage = Today;
            RebuildGroupChips();
            FilterProfileService.DataChanged += RebuildGroupChips;
            MarketGroupSelectionService.ActiveGroupsChanged += OnActiveGroupsChanged;
        }

        public ObservableCollection<MarketGroupChip> MergedFavChips { get; } = new ObservableCollection<MarketGroupChip>();
        public ObservableCollection<MarketGroupChip> MergedFilterChips { get; } = new ObservableCollection<MarketGroupChip>();

        private void RebuildGroupChips()
        {
            MergedFavChips.Clear();
            MergedFilterChips.Clear();
            foreach (var market in new[] { FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate })
            {
                foreach (var group in FilterProfileService.GetFavGroups(market))
                {
                    MergedFavChips.Add(new MarketGroupChip(group));
                }
                foreach (var group in FilterProfileService.GetFilterGroups(market))
                {
                    MergedFilterChips.Add(new MarketGroupChip(group));
                }
            }
            OnActiveGroupsChanged();
        }

        private void OnActiveGroupsChanged()
        {
            foreach (var chip in MergedFavChips)
            {
                chip.RefreshActive();
            }
            foreach (var chip in MergedFilterChips)
            {
                chip.RefreshActive();
            }
            ApplyFilters();
        }

        private readonly IGroupDirectory groupDirectory;
        private ProductFilter productFilter;

        public Task<IReadOnlyList<FilterFieldEntry>> GetFilterDictionaryAsync()
        {
            return Task.FromResult(groupDirectory.GetFilterFields(FilterProfileService.MarketKind.Rate));
        }

        public void ApplyProductFilter(string scope, ProductFilter filter)
        {
            productFilter = filter;
            ApplyFilters();
        }

        [ObservableProperty]
        private string _filterSearchText = string.Empty;

        partial void OnFilterSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            Today.ApplyFilters(productFilter, FilterSearchText);
            History.ApplyFilters(productFilter, FilterSearchText);
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
            if (ReferenceEquals(SelectedSubPage, History)) History.Activate();
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
            FilterProfileService.DataChanged -= RebuildGroupChips;
            MarketGroupSelectionService.ActiveGroupsChanged -= OnActiveGroupsChanged;
            Today.Dispose();
            History.Dispose();
        }
    }
}
