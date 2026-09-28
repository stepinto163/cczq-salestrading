using System.Collections.Generic;
using System.Windows.Controls;
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
                SyncSelection(vm.SelectedRow);
            }
        }

        private void SyncSelection(HashSet<MgmtQuoteRow> target)
        {
            target.Clear();
            if (TodayGrid.SelectedItems != null)
            {
                foreach (var item in TodayGrid.SelectedItems)
                {
                    if (item is MgmtQuoteRow r) target.Add(r);
                }
            }
        }
    }
}
