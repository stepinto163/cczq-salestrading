using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shared.Components;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.View
{
    /// <summary>报价管理左侧筛选面板宿主：顶部清除/搜索 + 合并筛选面板（复用 filters-merged）。</summary>
    public partial class MgmtFilterPanel : UserControl
    {
        public MgmtFilterPanel()
        {
            InitializeComponent();
        }

        /// <summary>同时清除侧栏控件和服务端筛选条件。</summary>
        private void OnClearFilters(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel.QuoteMgmtViewModel viewModel)
            {
                viewModel.FilterSearchText = string.Empty;
            }
            SidebarFilter.Reset(CombinedHost);
        }
    }
}
