using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.View
{
    /// <summary>今日报价子页视图：工具栏 + 23 列宽表。DataContext 为 MgmtTodayViewModel。</summary>
    public partial class MgmtTodayView : UserControl
    {
        public MgmtTodayView()
        {
            InitializeComponent();
            Shared.Components.BoardColumns.InsertValuationBlock(TodayGrid, "报价时间",
                new[] { 78d, 72d, 88d, 78d, 78d, 88d }, "StcNumberCell", 12);
        }

        private void TodayGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MgmtTodayViewModel vm)
            {
                SyncSelection(vm.SelectedRow, e);
                vm.OnSelectionSynced();
            }
        }

        /// <summary>双击行 → 编辑报价（行模型即完整 Quote，含 Revision 直接回填；方案 §3.1.4 入口矩阵）。</summary>
        private void TodayGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext is MgmtTodayViewModel vm)) return;
            if (!(TodayGrid.SelectedItem is QuoteRow row)) return;
            if (string.IsNullOrEmpty(row.QuoteId)) return;
            MarketCenter.View.QuoteEntryDialog.ShowForQuote(
                System.Windows.Application.Current?.MainWindow, row, MarketCenter.View.QuoteFormMode.Edit);
        }

        private static void SyncSelection(HashSet<QuoteRow> target, SelectionChangedEventArgs e)
        {
            target.Clear();
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (item is QuoteRow r) target.Add(r);
                }
            }
        }
    }
}
