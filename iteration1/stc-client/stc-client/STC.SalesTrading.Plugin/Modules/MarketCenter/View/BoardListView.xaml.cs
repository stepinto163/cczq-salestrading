using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Shared.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>"全部"看板视图（信用/利率复用）：列拖拽/宽度持久化 + 双击行 → 报价录入弹窗。</summary>
    public partial class BoardListView : UserControl
    {
        private readonly DispatcherTimer _saveTimer;

        public BoardListView()
        {
            InitializeComponent();
            Shared.Components.BoardColumns.InsertValuationBlock(Grid, "报价时间");
            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _saveTimer.Tick += (s, e) => { _saveTimer.Stop(); ColumnLayoutManager.Save(Grid, LayoutKey); };
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private string LayoutKey
        {
            get
            {
                var vm = DataContext as BoardListViewModel;
                return vm != null && vm.Market == BondMarket.Rate ? "board.rate.all" : "board.credit.all";
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ColumnLayoutManager.Apply(Grid, LayoutKey);
            Grid.ColumnDisplayIndexChanged += OnColumnChanged;
            foreach (var column in Grid.Columns)
            {
                DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn))
                    .AddValueChanged(column, OnColumnChanged);
            }
            _saveTimer.Stop();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Grid.ColumnDisplayIndexChanged -= OnColumnChanged;
            foreach (var column in Grid.Columns)
            {
                DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn))
                    .RemoveValueChanged(column, OnColumnChanged);
            }
        }

        private void OnColumnChanged(object sender, EventArgs e)
        {
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        /// <summary>双击行 → 报价录入弹窗（原型双击/键盘 Enter 打开报价编辑）。表头/空白区双击不触发。</summary>
        private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject) == null) return;
            if ((sender as DataGrid)?.SelectedItem is BoardQuoteRow row
                && DataContext is BoardListViewModel vm)
            {
                vm.OpenQuoteEntryForRowCommand.Execute(row);
            }
        }

        /// <summary>右键菜单仅限行：表头/空白区取消弹出；并把行对象注入菜单 DataContext 供 CommandParameter 使用。</summary>
        private void Grid_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null)
            {
                e.Handled = true;
                return;
            }
            var menu = (sender as DataGrid).ContextMenu;
            menu.DataContext = row.DataContext;
            // 添加到分组：按行市场动态列出关注分组（原型 contextFavoriteSubmenuHtml）
            var quote = row.DataContext as Abstractions.Models.BoardQuoteRow;
            if (quote != null)
            {
                var market = quote.Market == BondMarket.Rate
                    ? FilterProfileService.MarketKind.Rate
                    : FilterProfileService.MarketKind.Credit;
                BoardRowGroupMenu.Populate(menu, quote.Code, quote.Name, Window.GetWindow(this), market);
            }
        }

        private static T FindAncestor<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null)
            {
                if (node is T match) return match;
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            return null;
        }
    }
}
