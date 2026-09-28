using STC.SalesTrading.Plugin.Shared.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>报价提醒视图（23 列）：列拖拽/宽度持久化 + 行右键菜单。</summary>
    public partial class BoardAlertView : UserControl
    {
        private readonly DispatcherTimer _saveTimer;

        public BoardAlertView()
        {
            InitializeComponent();
            Shared.Components.BoardColumns.InsertValuationBlock(Grid, "报价时间");
            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _saveTimer.Tick += (s, e) => { _saveTimer.Stop(); ColumnLayoutManager.Save(Grid, "board.alert"); };
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ColumnLayoutManager.Apply(Grid, "board.alert");
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
            // BoardAlertRow 没有市场字段：按原型 merged 侧栏口径合并列出两个市场的分组
            var alert = row.DataContext as Abstractions.Models.BoardAlertRow;
            if (alert != null)
            {
                BoardRowGroupMenu.Populate(menu, alert.Code, alert.Name, Window.GetWindow(this),
                    FilterProfileService.MarketKind.Credit, FilterProfileService.MarketKind.Rate);
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
