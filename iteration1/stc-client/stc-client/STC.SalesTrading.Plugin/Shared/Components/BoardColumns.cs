using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 看板/管理列表 DataGrid 的公共列工厂（WPF 的 DataGrid.Columns 不能用 UserControl 复用，
    /// 逐列 XAML 又在 5 个视图逐字重复，故以代码工厂统一生成）。
    /// </summary>
    internal static class BoardColumns
    {
        /// <summary>
        /// 在锚点列之前插入「中债净价/中债偏离/中债YTM/中证净价/中证全价/中证YTM」6 列
        /// （原 5 个视图各 30 行的逐字重复；看板列宽 105 均一，管理列表为 78/72/88…）。
        /// 在视图 code-behind 的 InitializeComponent 之后调用一次。
        /// </summary>
        /// <param name="grid">目标表格。</param>
        /// <param name="anchorHeader">插入位置锚点列的 Header（如「报价时间」）；找不到则追加到末尾。</param>
        /// <param name="widths">6 列宽；缺省 105 均一。</param>
        /// <param name="styleKey">数值单元格 ElementStyle 基样式（看板 StcNumberCellCenter / 管理 StcNumberCell）。</param>
        /// <param name="fontSize">单元格字号（管理列表为 12，看板默认跟随样式）。</param>
        public static void InsertValuationBlock(DataGrid grid, string anchorHeader,
            double[] widths = null, string styleKey = "StcNumberCellCenter", double? fontSize = null)
        {
            if (grid == null) return;
            if (widths == null || widths.Length != 6)
            {
                widths = new[] { 105d, 105d, 105d, 105d, 105d, 105d };
            }

            var baseStyle = grid.TryFindResource(styleKey) as Style;
            var headers = new[]
            {
                new Tuple<string, string, string>("中债净价", "NzNetPrice", "0.0000"),
                new Tuple<string, string, string>("中债偏离", "NzDeviation", "0.00"),
                new Tuple<string, string, string>("中债YTM(%)", "NzYtm", "0.0000"),
                new Tuple<string, string, string>("中证净价", "CsNetPrice", "0.0000"),
                new Tuple<string, string, string>("中证全价", "CsFullPrice", "0.0000"),
                new Tuple<string, string, string>("中证YTM(%)", "CsYtm", "0.0000"),
            };

            var columns = new List<DataGridColumn>(headers.Length);
            for (var i = 0; i < headers.Length; i++)
            {
                var style = baseStyle;
                if (fontSize.HasValue && style != null)
                {
                    var derived = new Style { TargetType = typeof(TextBlock), BasedOn = style };
                    derived.Setters.Add(new Setter(TextBlock.FontSizeProperty, fontSize.Value));
                    style = derived;
                }
                var column = new DataGridTextColumn
                {
                    Header = headers[i].Item1,
                    Binding = new System.Windows.Data.Binding(headers[i].Item2)
                    {
                        StringFormat = headers[i].Item3,
                    },
                    Width = new DataGridLength(widths[i]),
                };
                if (style != null) column.ElementStyle = style;
                columns.Add(column);
            }

            var anchor = FindColumn(grid, anchorHeader);
            // 倒序插入保证顺序与锚点相对位置稳定
            for (var i = columns.Count - 1; i >= 0; i--)
            {
                if (anchor >= 0)
                {
                    grid.Columns.Insert(anchor, columns[i]);
                }
                else
                {
                    grid.Columns.Add(columns[i]);
                }
            }
        }

        private static int FindColumn(DataGrid grid, string header)
        {
            for (var i = 0; i < grid.Columns.Count; i++)
            {
                if (string.Equals(grid.Columns[i].Header as string, header, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
