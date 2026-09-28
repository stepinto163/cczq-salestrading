using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.View
{
    /// <summary>报价确认页：分批分组折叠主从表 + 批量工具栏。</summary>
    public partial class QuoteConfirmView : UserControl
    {
        public QuoteConfirmView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 主表加载后，按各批次的 IsExpanded 同步每行明细展开态。
        /// （RowDetailsVisibilityMode=Collapsed 时 DataGrid 不管理明细显隐，需代码精确控制。）
        /// </summary>
        private void MasterGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            SyncAllRowDetails();
        }

        private void SyncAllRowDetails()
        {
            if (MasterGrid == null) return;
            foreach (var item in MasterGrid.Items)
            {
                var row = (DataGridRow)MasterGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (row == null) continue;
                var batch = row.DataContext as QuoteConfirmBatchViewModel;
                row.DetailsVisibility = batch != null && batch.IsExpanded
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 点击组头行折叠/展开明细（原型 toggleGroup 语义）。
        /// 点击复选框/标签等交互控件时不触发折叠。
        /// </summary>
        private void MasterGrid_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            // 交互控件上点击 → 不折叠
            var hit = source;
            while (hit != null && !(hit is CheckBox) && !(hit is ToggleButton) && !(hit is Button))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }
            if (hit != null) return;

            // 定位点击的组头行
            var current = source;
            while (current != null && !(current is DataGridRow) && !(current is DataGrid))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            var row = current as DataGridRow;
            if (row == null) return;
            if (row.IsMouseOver)
            {
                var batch = row.DataContext as QuoteConfirmBatchViewModel;
                if (batch != null)
                {
                    batch.IsExpanded = !batch.IsExpanded;
                    row.DetailsVisibility = batch.IsExpanded ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

    }
}
