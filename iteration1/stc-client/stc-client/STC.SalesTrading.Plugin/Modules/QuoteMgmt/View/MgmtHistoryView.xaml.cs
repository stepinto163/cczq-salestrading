using System.Collections.Generic;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.View
{
    /// <summary>历史报价子页视图：工具栏（日期区间）+ 22 列宽表。DataContext 为 MgmtHistoryViewModel。</summary>
    public partial class MgmtHistoryView : UserControl
    {
        public MgmtHistoryView()
        {
            InitializeComponent();
            Shared.Components.BoardColumns.InsertValuationBlock(HistoryGrid, "报价时间",
                new[] { 78d, 72d, 88d, 78d, 78d, 88d }, "StcNumberCell", 12);
        }

        private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MgmtHistoryViewModel vm)
            {
                SyncSelection(vm.SelectedRow, e);
            }
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
