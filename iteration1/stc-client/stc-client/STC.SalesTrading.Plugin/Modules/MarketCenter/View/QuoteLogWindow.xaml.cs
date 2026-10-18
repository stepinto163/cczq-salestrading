using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>报价日志弹窗：单条报价的变更历史（H10），看板行右键"报价日志"打开。</summary>
    public partial class QuoteLogWindow : Window
    {
        private QuoteLogWindow()
        {
            InitializeComponent();
        }

        /// <summary>展示某报价的变更历史（数据由调用方拉取后传入）。</summary>
        public static void Show(Window owner, string quoteId, string label, IReadOnlyList<QuoteLogEntry> entries)
        {
            var window = new QuoteLogWindow();
            var host = owner ?? Application.Current?.MainWindow;
            if (host != null) window.Owner = host;
            window.TitleText.Text = "报价日志 · " + (label ?? quoteId);
            window.LogGrid.ItemsSource = (entries ?? new List<QuoteLogEntry>())
                .Select(e => new
                {
                    OperationTimeText = e.OperationTime.HasValue
                        ? e.OperationTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                        : "--",
                    e.Action,
                    e.Revision,
                    e.OperatorUserId,
                    Reason = e.Reason ?? string.Empty,
                })
                .ToList();
            window.ShowDialog();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch { }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
