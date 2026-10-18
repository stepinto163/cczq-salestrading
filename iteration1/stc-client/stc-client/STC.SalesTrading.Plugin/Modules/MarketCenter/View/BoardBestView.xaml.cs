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
    /// <summary>"最优"看板视图（信用/利率复用）：列拖拽/宽度持久化 + 双击行 → 报价录入弹窗。</summary>
    public partial class BoardBestView : UserControl
    {
        private readonly DispatcherTimer _saveTimer;

        public BoardBestView()
        {
            InitializeComponent();
            Shared.Components.BoardColumns.InsertValuationBlock(Grid, "报价时间");
            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _saveTimer.Tick += (s, e) => { _saveTimer.Stop(); ColumnLayoutManager.Save(Grid, LayoutKey); };
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += (s, e) =>
            {
                var vm = e.NewValue as BoardBestViewModel;
                if (vm != null) vm.RowsFlashed += OnVmRowsFlashed;
                var old = e.OldValue as BoardBestViewModel;
                if (old != null) old.RowsFlashed -= OnVmRowsFlashed;
            };
        }

        private string LayoutKey
        {
            get
            {
                var vm = DataContext as BoardBestViewModel;
                return vm != null && vm.Market == BondMarket.Rate ? "board.rate.best" : "board.credit.best";
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

        /// <summary>双击行 → 报价录入弹窗。表头/空白区双击不触发。</summary>
        private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject) == null) return;
            if ((sender as DataGrid)?.SelectedItem is BestRow row
                && DataContext is BoardBestViewModel vm)
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
            var best = row.DataContext as Abstractions.Models.BestRow;
            if (best != null)
            {
                var market = best.Market == BondMarket.Rate
                    ? FilterProfileService.MarketKind.Rate
                    : FilterProfileService.MarketKind.Credit;
                BoardRowGroupMenu.Populate(menu, best.SecurityId, best.SecurityName, Window.GetWindow(this), market);
            }
        }


        #region 新报价/改价行闪烁（原型推送高亮语义）

        /// <summary>订阅 VM 的增量应用事件：新报价/改价行播背景高亮渐隐动画（公共实现 BoardRowFlash，
        /// 内含 Recycling 行容器复用下的动画终止保护）。</summary>
        private void OnVmRowsFlashed(System.Collections.Generic.IReadOnlyList<object> rows)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() =>
                    Shared.Components.BoardRowFlash.Flash(Grid, rows)));
                return;
            }
            Shared.Components.BoardRowFlash.Flash(Grid, rows);
        }

        #endregion

        private static T FindAncestor<T>(DependencyObject node) where T : DependencyObject
        {
            while (node != null)
            {
                if (node is T match) return match;
                node = System.Windows.Media.VisualTreeHelper.GetParent(node);
            }
            return null;
        }

        #region 同档最优报价浮层（原型 0910 best-quote-count / best-quote-layer）

        /// <summary>悬停「同档笔数」按钮 → 展开 7 列浮层（原型为 hover + 延迟隐藏，此处简化为进入即开）。</summary>
        private void OnBestCountMouseEnter(object sender, MouseEventArgs e)
        {
            var toggle = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (toggle != null)
            {
                toggle.IsChecked = true;
            }
        }

        /// <summary>鼠标离开浮层内容 → 收起。<c>sender</c> 是 Popup 的直接子元素，故其逻辑父级即 Popup。</summary>
        private void OnBestLayerMouseLeave(object sender, MouseEventArgs e)
        {
            var popup = (sender as FrameworkElement)?.Parent as System.Windows.Controls.Primitives.Popup;
            var toggle = popup?.PlacementTarget as System.Windows.Controls.Primitives.ToggleButton;
            if (toggle != null)
            {
                toggle.IsChecked = false;
            }
        }

        #endregion
    }
}
