using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.View
{
    internal static class UiElements
    {
        internal static Button Button(string title, Action action)
        {
            Button button = new Button { Content = title, Margin = new Thickness(4), Padding = new Thickness(12, 5, 12, 5) };
            button.Click += (sender, args) => action();
            return button;
        }

        internal static DataGrid Grid(params string[] columns)
        {
            DataGrid grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                CanUserAddRows = false,
                CanUserSortColumns = false,
                EnableRowVirtualization = true,
                EnableColumnVirtualization = true,
                SelectionMode = DataGridSelectionMode.Single,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                RowHeight = 26,
                Background = new SolidColorBrush(Color.FromRgb(23, 28, 36)),
                Foreground = Brushes.Gainsboro,
                RowBackground = new SolidColorBrush(Color.FromRgb(23, 28, 36)),
                AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(30, 36, 45)),
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HorizontalGridLinesBrush = new SolidColorBrush(Color.FromRgb(48, 55, 65)),
                Margin = new Thickness(4)
            };
            Style header = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            header.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(42, 49, 60))));
            header.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.Gainsboro));
            header.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(6)));
            grid.ColumnHeaderStyle = header;
            VirtualizingPanel.SetIsVirtualizing(grid, true);
            VirtualizingPanel.SetVirtualizationMode(grid, VirtualizationMode.Recycling);
            foreach (string column in columns)
            {
                string[] parts = column.Split('|');
                grid.Columns.Add(new DataGridTextColumn
                {
                    Header = parts[0],
                    Binding = new Binding("[" + parts[1] + "]") { Converter = new DisplayValueConverter(), ConverterParameter = parts[1] },
                    Width = new DataGridLength(parts[1].EndsWith("Time") ? 185 : parts[1] == "quoteId" ? 220 : 110)
                });
            }
            return grid;
        }
    }
}
