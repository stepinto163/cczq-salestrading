using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    public sealed partial class SalesTradingView
    {
        private readonly ObservableCollection<JObject> managementRows = new ObservableCollection<JObject>();
        private readonly Dictionary<string, JObject> securityReference = new Dictionary<string, JObject>();
        private bool loadingReference;
        private bool loadingMoreQuotes;
        private bool loadingEarlierQuotes;
        private int managementGeneration = -1;
        private int displayedThroughPage;
        private int firstLoadedPage = 1;
        private TextBox managementBondSearch;
        private TextBox managementInstitutionSearch;
        private DataGrid managementGrid;
        private Button managementUpdates;
        private TextBlock managementCount;

        private FrameworkElement BuildCurrentQuoteManagement()
        {
            DockPanel panel = new DockPanel { Background = (Brush)new BrushConverter().ConvertFromString("#0D1117") };
            WrapPanel toolbar = new WrapPanel { Margin = new Thickness(10, 8, 10, 6) };
            managementGrid = CreateManagementGrid();
            managementGrid.ItemsSource = managementRows;
            foreach (string operation in new[] { "refer|Refer", "unrefer|Unrefer", "delete|删除" })
            {
                string[] parts = operation.Split('|');
                toolbar.Children.Add(UiElements.Button(parts[1], async () => await ExecuteAsync(async () =>
                {
                    JObject[] selected = managementGrid.SelectedItems.OfType<JObject>().ToArray();
                    FiccApiClient.RequireQuoteResults(await Api.OperateAsync(parts[0], selected));
                })));
            }
            toolbar.Children.Add(UiElements.Button("Copy", () => CopyManagementRows(managementGrid)));
            toolbar.Children.Add(UiElements.Button("导出", () => ExportManagementRows(managementGrid)));
            managementBondSearch = ManagementSearchBox("代码/名称...", true);
            managementInstitutionSearch = ManagementSearchBox("报价机构...", true);
            toolbar.Children.Add(managementBondSearch);
            toolbar.Children.Add(managementInstitutionSearch);
            // No owner-name or source filter exists in the current service contract. Do not fake a current-page search.
            toolbar.Children.Add(ManagementSearchBox("长城交易员...", false));
            toolbar.Children.Add(new TextBlock { Text = "报价方式:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) });
            toolbar.Children.Add(new ComboBox { ItemsSource = new[] { "全部", "QT", "ideal", "手工录入" }, SelectedIndex = 0,
                Width = 88, Height = 26, IsEnabled = false, ToolTip = "当前服务尚未提供报价来源筛选" });
            toolbar.Children.Add(new TextBlock { Text = "报价状态:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) });
            ComboBox state = new ComboBox { ItemsSource = new[] { "全部", "有效", "撤销" }, SelectedIndex = 0, Width = 76, Height = 26 };
            state.SelectionChanged += async (sender, args) =>
            {
                Quotes.QuoteStatuses = state.SelectedIndex == 1 ? new JArray("ACTIVE")
                    : state.SelectedIndex == 2 ? new JArray("REFERRED") : new JArray("ACTIVE", "REFERRED");
                if (IsConnected)
                {
                    await ExecuteAsync(() => Quotes.OpenAsync(ReadManagementFilter()));
                }
            };
            toolbar.Children.Add(state);
            foreach (Button button in toolbar.Children.OfType<Button>())
            {
                button.FontSize = 14;
                button.Height = 26;
                button.Padding = new Thickness(10, 2, 10, 2);
                button.Margin = new Thickness(4, 0, 4, 8);
            }
            foreach (ComboBox select in toolbar.Children.OfType<ComboBox>())
            {
                select.Style = (Style)FindResource("ProductFilterSelect");
                select.Margin = new Thickness(4, 0, 4, 8);
            }
            DockPanel.SetDock(toolbar, Dock.Top);
            panel.Children.Add(toolbar);
            managementUpdates = UiElements.Button("有 0 条更新，点击刷新", async () => await ExecuteAsync(async () =>
            {
                await Quotes.ResumeLatestAsync();
                if (managementRows.Count > 0)
                {
                    managementGrid.ScrollIntoView(managementRows[0]);
                }
            }));
            managementUpdates.Visibility = Visibility.Collapsed;
            managementUpdates.Background = (Brush)new BrushConverter().ConvertFromString("#2D2100");
            managementUpdates.Foreground = (Brush)new BrushConverter().ConvertFromString("#FBBF24");
            managementUpdates.Margin = new Thickness(0);
            DockPanel.SetDock(managementUpdates, Dock.Top);
            panel.Children.Add(managementUpdates);
            managementCount = new TextBlock { Margin = new Thickness(12, 8, 12, 8), FontSize = 11, Foreground = Brushes.SlateGray };
            DockPanel.SetDock(managementCount, Dock.Bottom);
            panel.Children.Add(managementCount);
            panel.Children.Add(managementGrid);
            managementGrid.MouseDoubleClick += async (sender, args) =>
            {
                if (managementGrid.SelectedItem is JObject quote)
                {
                    await ExecuteAsync(async () =>
                    {
                        await LoadQuoteAsync((string)quote["quoteId"]);
                        ShowEntryPage(true);
                    });
                }
            };
            managementGrid.PreviewMouseWheel += async (sender, args) =>
            {
                if (Busy || !Quotes.IsReady)
                {
                    return;
                }
                Quotes.BeginBrowsing();
                ScrollViewer scroll = FindVisualChild<ScrollViewer>(managementGrid);
                if (scroll != null && args.Delta < 0 && scroll.VerticalOffset >= scroll.ScrollableHeight - 3)
                {
                    await LoadMoreManagementAsync();
                }
                else if (scroll != null && args.Delta > 0 && scroll.VerticalOffset <= 1)
                {
                    await LoadEarlierManagementAsync();
                }
            };
            managementGrid.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(async (sender, args) =>
            {
                if (args.VerticalChange > 0 && args.ExtentHeightChange == 0 && !Busy && Quotes.IsReady)
                {
                    Quotes.BeginBrowsing();
                    if (args.VerticalOffset + args.ViewportHeight >= args.ExtentHeight - 3)
                    {
                        await LoadMoreManagementAsync();
                    }
                }
            }));
            Quotes.Changed += RefreshManagementRows;
            Quotes.Removed += ids =>
            {
                foreach (JObject row in managementRows.Where(row => ids.Contains((string)row["quoteId"])).ToArray())
                {
                    managementRows.Remove(row);
                }
            };
            return panel;
        }

        private TextBox ManagementSearchBox(string hint, bool enabled)
        {
            TextBox input = new TextBox { Tag = hint, Width = 120, Height = 26, Margin = new Thickness(4, 0, 4, 8),
                Style = (Style)FindResource("ProductFilterInput"), IsEnabled = enabled,
                ToolTip = enabled ? "输入后按 Enter 筛选" : "当前仅查询本人报价；服务尚未提供按交易员名称筛选" };
            input.KeyDown += async (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    await ExecuteAsync(() => Quotes.OpenAsync(ReadManagementFilter()));
                }
            };
            return input;
        }

        private JObject ReadManagementFilter()
        {
            JObject filter = managementFilters.Read();
            JArray conditions = (JArray)filter["conditions"];
            if (!string.IsNullOrWhiteSpace(managementBondSearch?.Text))
            {
                string value = managementBondSearch.Text.Trim();
                conditions.Add(new JObject { ["field"] = value.Contains(".") ? "securityId" : "securityName",
                    ["operator"] = value.Contains(".") ? "EQ" : "CONTAINS", ["value"] = value });
            }
            if (!string.IsNullOrWhiteSpace(managementInstitutionSearch?.Text))
            {
                conditions.Add(new JObject { ["field"] = "searchText", ["operator"] = "CONTAINS", ["value"] = managementInstitutionSearch.Text.Trim() });
            }
            return filter;
        }

        private async Task LoadMoreManagementAsync()
        {
            if (loadingMoreQuotes || loadingEarlierQuotes || displayedThroughPage * Quotes.PageSize >= Quotes.TotalCount)
            {
                return;
            }
            loadingMoreQuotes = true;
            try
            {
                await ExecuteAsync(() => Quotes.QueryAsync(displayedThroughPage + 1));
            }
            finally
            {
                loadingMoreQuotes = false;
            }
        }

        private async Task LoadEarlierManagementAsync()
        {
            if (loadingMoreQuotes || loadingEarlierQuotes || firstLoadedPage <= 1)
            {
                return;
            }
            loadingEarlierQuotes = true;
            try
            {
                await ExecuteAsync(() => Quotes.QueryAsync(firstLoadedPage - 1));
            }
            finally
            {
                loadingEarlierQuotes = false;
            }
        }

        private void RefreshManagementRows()
        {
            var selectedIds = new HashSet<string>(managementGrid.SelectedItems.OfType<JObject>().Select(row => (string)row["quoteId"]));
            if (managementGeneration != Quotes.Generation || !Quotes.IsBrowsing)
            {
                managementRows.Clear();
                displayedThroughPage = 0;
                firstLoadedPage = 1;
                managementGeneration = Quotes.Generation;
            }
            bool append = Quotes.PageNumber > displayedThroughPage;
            bool prepend = loadingEarlierQuotes && Quotes.PageNumber < firstLoadedPage;
            int inserted = 0;
            foreach (JObject quote in Quotes.Rows)
            {
                int index = managementRows.ToList().FindIndex(existing => (string)existing["quoteId"] == (string)quote["quoteId"]);
                JObject row = ManagementRow(quote);
                if (index >= 0)
                {
                    managementRows[index] = row;
                }
                else if (prepend)
                {
                    managementRows.Insert(inserted++, row);
                }
                else if (append || !Quotes.IsBrowsing)
                {
                    managementRows.Add(row);
                }
            }
            displayedThroughPage = Math.Max(displayedThroughPage, Quotes.PageNumber);
            if (prepend)
            {
                firstLoadedPage = Quotes.PageNumber;
            }
            ScrollViewer scroll = FindVisualChild<ScrollViewer>(managementGrid);
            double oldOffset = scroll?.VerticalOffset ?? 0;
            int removed = 0;
            while (managementRows.Count > 300)
            {
                managementRows.RemoveAt(prepend ? managementRows.Count - 1 : 0);
                removed++;
            }
            if (removed > 0)
            {
                if (prepend)
                {
                    displayedThroughPage--;
                }
                else
                {
                    firstLoadedPage++;
                }
            }
            if (scroll != null && (inserted > 0 || removed > 0))
            {
                scroll.ScrollToVerticalOffset(prepend ? oldOffset + inserted : Math.Max(0, oldOffset - removed));
            }
            managementCount.Text = "共" + Quotes.TotalCount + "条  |  已加载 " + managementRows.Count + "条"
                + (Quotes.IsBrowsing ? "  |  浏览中，列表位置保持稳定" : "");
            managementUpdates.Visibility = Quotes.IsBrowsing ? Visibility.Visible : Visibility.Collapsed;
            managementUpdates.Content = "有 " + Quotes.PendingUpdateCount + " 条更新，点击刷新";
            foreach (JObject row in managementRows.Where(row => selectedIds.Contains((string)row["quoteId"])))
            {
                if (!managementGrid.SelectedItems.Contains(row))
                {
                    managementGrid.SelectedItems.Add(row);
                }
            }
            LoadManagementReferenceAsync();
        }

        private async void LoadManagementReferenceAsync()
        {
            if (loadingReference || disposed)
            {
                return;
            }
            string[] ids = managementRows.Select(row => (string)row["securityId"]).Where(id => !securityReference.ContainsKey(id)).Distinct().Take(100).ToArray();
            if (ids.Length == 0)
            {
                return;
            }
            loadingReference = true;
            try
            {
                JObject response = await Api.RequestAsync("ficc.base.get", new JObject { ["kind"] = "SECURITY", ["ids"] = new JArray(ids) });
                foreach (JObject security in (JArray)response["records"])
                {
                    securityReference[(string)security["securityId"]] = security;
                }
                for (int index = 0; index < managementRows.Count; index++)
                {
                    managementRows[index] = ManagementRow(managementRows[index]);
                }
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
                ShowError("债券显示资料加载失败：" + error.Message);
            }
            finally
            {
                loadingReference = false;
            }
        }

        private JObject ManagementRow(JObject quote)
        {
            JObject row = (JObject)quote.DeepClone();
            if (securityReference.TryGetValue((string)quote["securityId"], out JObject security))
            {
                foreach (JProperty property in security.Properties())
                {
                    if (row[property.Name] == null)
                    {
                        row[property.Name] = property.Value.DeepClone();
                    }
                }
            }
            string sourceType = (string)quote["source"]?["type"];
            row["sourceLabel"] = sourceType == "QT" ? "QT" : sourceType == "IDEAL" ? "ideal" : sourceType == "MANUAL" ? "手工录入" : "—";
            row["flags"] = string.Join(" ", new[] { "urgent|急", "recommended|荐", "whole|整", "oco|OCO", "packing|打包" }
                .Where(flag => (bool?)quote[flag.Split('|')[0]] == true).Select(flag => flag.Split('|')[1]));
            return row;
        }

        private static DataGrid CreateManagementGrid()
        {
            DataGrid grid = UiElements.Grid("报价方式|sourceLabel", "报价状态|status", "剩余期限|remainingYears", "代码|securityId", "简称|securityName",
                "方向|side", "报价|price", "报量(万)|quantity", "操作|flags", "报价机构|institutionName", "报价交易员|contactName", "长城交易员|ownerUserName",
                "最新成交价|lastTradePrice", "债券库|bondSubtype", "主体评级|issuerRating", "债券评级|bondRating", "中债净价|valuationCleanPrice",
                "中债偏离|bidValuationSpread", "中债YTM(%)|valuationYield", "中证净价|csiValuationCleanPrice", "中证全价|csiValuationFullPrice",
                "中证YTM(%)|csiValuationYield", "报价时间|updateTime");
            grid.Margin = new Thickness(0);
            grid.SelectionMode = DataGridSelectionMode.Extended;
            grid.Background = (Brush)new BrushConverter().ConvertFromString("#0D1117");
            grid.RowBackground = grid.Background;
            grid.AlternatingRowBackground = grid.Background;
            grid.BorderThickness = new Thickness(0);
            grid.FrozenColumnCount = 5;
            grid.FontSize = 13;
            grid.RowHeight = 32;
            grid.HorizontalGridLinesBrush = (Brush)new BrushConverter().ConvertFromString("#21262D");
            Style header = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader), grid.ColumnHeaderStyle);
            header.Setters.Add(new Setter(Control.BackgroundProperty, (Brush)new BrushConverter().ConvertFromString("#161B22")));
            header.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)new BrushConverter().ConvertFromString("#8B949E")));
            header.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 8, 10, 8)));
            grid.ColumnHeaderStyle = header;
            Style cell = new Style(typeof(DataGridCell));
            cell.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 0, 10, 0)));
            cell.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            grid.CellStyle = cell;
            foreach (DataGridColumn column in grid.Columns)
            {
                column.Width = new DataGridLength((string)column.Header == "代码" ? 115 : (string)column.Header == "报价时间" ? 170 : 90);
            }
            return grid;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, index);
                if (child is T found)
                {
                    return found;
                }
                T nested = FindVisualChild<T>(child);
                if (nested != null)
                {
                    return nested;
                }
            }
            return null;
        }

        private static string ManagementText(DataGrid grid, bool selectedOnly)
        {
            var rows = selectedOnly ? grid.SelectedItems.OfType<JObject>() : grid.Items.OfType<JObject>();
            var columns = grid.Columns.OfType<DataGridTextColumn>().ToArray();
            string Header() => string.Join("\t", columns.Select(column => column.Header));
            string Row(JObject row) => string.Join("\t", columns.Select(column =>
                (row[((Binding)column.Binding).Path.Path.Trim('[', ']')]?.ToString() ?? "").Replace("\t", " ").Replace("\r", " ").Replace("\n", " ")));
            return Header() + Environment.NewLine + string.Join(Environment.NewLine, rows.Select(Row));
        }

        private void CopyManagementRows(DataGrid grid)
        {
            Clipboard.SetText(ManagementText(grid, true));
            status.Text = "已复制选中报价。";
        }

        private void ExportManagementRows(DataGrid grid)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "报价-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".tsv",
                Filter = "制表符文本 (*.tsv)|*.tsv",
                Title = "导出当前已加载报价"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                File.WriteAllText(dialog.FileName, ManagementText(grid, false), new UTF8Encoding(true));
                status.Text = "已导出当前已加载报价。";
            }
        }
    }
}
