using STC.SalesTrading.Plugin.Shared.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>模糊报价视图（7 列可编辑 + 未同步红点）：列拖拽/宽度持久化。</summary>
    public partial class BoardIntentView : UserControl
    {
        private readonly DispatcherTimer _saveTimer;

        public BoardIntentView()
        {
            InitializeComponent();
            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _saveTimer.Tick += (s, e) => { _saveTimer.Stop(); ColumnLayoutManager.Save(Grid, "board.intent"); };
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ColumnLayoutManager.Apply(Grid, "board.intent");
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
    }
}
