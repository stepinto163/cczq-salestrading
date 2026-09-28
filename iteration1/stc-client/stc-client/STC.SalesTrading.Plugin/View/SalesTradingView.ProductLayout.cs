using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    public sealed partial class SalesTradingView
    {
        private readonly TabControl entryTabs = new TabControl();
        private readonly List<ProductFilterPanel> filterPanels = new List<ProductFilterPanel>();
        private readonly DataGrid draftGrid = UiElements.Grid("债券代码|securityId", "方向|side", "价格|price", "数量（万）|quantity",
            "清算|settlType", "机构|institutionName", "联系人|contactName", "备注|remark");
        private ProductFilterPanel marketFilters;
        private ProductFilterPanel managementFilters;
        private string market;
        private JObject marketFilter = EmptyFilter();
        private readonly TabControl marketTabs = new TabControl();

        private void BuildProductEntry()
        {
            DockPanel page = new DockPanel();
            StackPanel editor = new StackPanel();
            editor.Children.Add(editingLabel);
            editor.Children.Add(Editor);
            WrapPanel editingCommands = new WrapPanel();
            AddCommand(editingCommands, "update", "保存修改", () => OperateAsync("update"));
            AddCommand(editingCommands, "refer", "撤销 Refer", () => OperateAsync("refer"));
            AddCommand(editingCommands, "unrefer", "恢复 Unrefer", () => OperateAsync("unrefer"));
            AddCommand(editingCommands, "history", "变更历史", ShowQuoteHistoryAsync);
            editor.Children.Add(editingCommands);
            Expander editDetails = new Expander
            {
                Header = "报价信息 · 机构 / 联系人匹配",
                Content = editor,
                IsExpanded = true,
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34))
            };
            DockPanel.SetDock(editDetails, Dock.Bottom);
            page.Children.Add(editDetails);

            DockPanel confirmation = new DockPanel();
            WrapPanel confirmCommands = new WrapPanel();
            AddCommand(confirmCommands, "confirm", "确认报价", () => OperateAsync("confirm"));
            AddCommand(confirmCommands, "delete", "删除报价", () => OperateAsync("delete"));
            confirmCommands.Children.Add(UiElements.Button("精确报价录入", () => ShowEntryPage(false)));
            confirmCommands.Children.Add(new TextBlock
            {
                Text = "选择报价后核对下方信息；已填写的机构、联系人须匹配 CRM 后确认。",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            });
            DockPanel.SetDock(confirmCommands, Dock.Top);
            confirmation.Children.Add(confirmCommands);
            confirmation.Children.Add(BuildPage(PendingQuotes, true));
            TabItem pending = new TabItem { Header = "报价确认", Content = confirmation };
            PendingQuotes.Changed += () => pending.Header = "报价确认  " + PendingQuotes.TotalCount;
            entryTabs.Items.Add(pending);

            DockPanel precise = new DockPanel();
            WrapPanel preciseCommands = new WrapPanel();
            AddCommand(preciseCommands, "new", "新报价", () =>
            {
                EditingQuote = null;
                UpdateButtons();
                return Task.CompletedTask;
            });
            AddCommand(preciseCommands, "create", "保存待确认", CreateAsync);
            AddCommand(preciseCommands, "addDraft", "加入批量草稿", () =>
            {
                if (drafts.Count >= 100)
                {
                    throw new InvalidOperationException("每批最多 100 条。");
                }
                drafts.Add(Editor.Read());
                RefreshDrafts();
                return Task.CompletedTask;
            });
            AddCommand(preciseCommands, "createBatch", "保存批量草稿", CreateBatchAsync);
            preciseCommands.Children.Add(UiElements.Button("移除选中草稿", () =>
            {
                if (draftGrid.SelectedItem is JObject row)
                {
                    drafts.Remove(row);
                    RefreshDrafts();
                }
            }));
            preciseCommands.Children.Add(UiElements.Button("更新选中草稿", async () => await ExecuteAsync(() =>
            {
                if (!(draftGrid.SelectedItem is JObject draft))
                {
                    throw new InvalidOperationException("请先选择需要修改的草稿。");
                }
                JObject replacement = Editor.Read();
                if (draft["clientQuoteId"] != null)
                {
                    replacement["clientQuoteId"] = draft["clientQuoteId"].DeepClone();
                }
                drafts[drafts.IndexOf(draft)] = replacement;
                RefreshDrafts();
                return Task.CompletedTask;
            })));
            DockPanel.SetDock(preciseCommands, Dock.Top);
            precise.Children.Add(preciseCommands);
            TextBlock hint = new TextBlock
            {
                Text = "在下方录入精确报价。单条保存或批量保存后，进入“报价确认”核对并生效。双击草稿可载入编辑。",
                Margin = new Thickness(8),
                TextWrapping = TextWrapping.Wrap
            };
            DockPanel.SetDock(hint, Dock.Top);
            precise.Children.Add(hint);
            draftGrid.ItemsSource = drafts;
            draftGrid.MouseDoubleClick += (sender, args) =>
            {
                if (draftGrid.SelectedItem is JObject draft)
                {
                    EditingQuote = null;
                    Editor.Set(draft);
                    UpdateButtons();
                }
            };
            precise.Children.Add(draftGrid);
            entryTabs.Items.Add(new TabItem { Header = "精确报价", Content = precise });
            page.Children.Add(entryTabs);
            tabs.Items.Add(new TabItem { Header = "报价录入", Content = page });
        }

        private void RefreshDrafts()
        {
            draftGrid.Items.Refresh();
            status.Text = "批量草稿 " + drafts.Count + " 条（尚未发送）";
        }

        public void ShowEntryPage(bool confirmation)
        {
            tabs.SelectedIndex = 0;
            entryTabs.SelectedIndex = confirmation ? 0 : 1;
        }

        public void ShowSection(string title)
        {
            TabItem section = tabs.Items.OfType<TabItem>().First(item => (string)item.Header == title);
            tabs.SelectedItem = section;
        }

        private FrameworkElement BuildProductMarket()
        {
            DockPanel marketPage = new DockPanel();
            WrapPanel modes = new WrapPanel();
            foreach (string option in new[] { "全部债券||BEST", "信用全部|CREDIT|FULL", "信用最优|CREDIT|BEST", "利率全部|RATE|FULL", "利率最优|RATE|BEST" })
            {
                string[] parts = option.Split('|');
                modes.Children.Add(UiElements.Button(parts[0], async () => await ExecuteAsync(async () =>
                {
                    market = parts[1];
                    Prices.PriceMode = parts[2];
                    await ApplyMarketFilterAsync(marketFilters.Read());
                })));
            }
            modes.Children.Add(UiElements.Button("报价录入", () => ShowEntryPage(false)));
            DockPanel.SetDock(modes, Dock.Top);
            marketPage.Children.Add(modes);
            marketPage.Children.Add(BuildPage(Prices, false));
            marketTabs.Items.Add(new TabItem { Header = "债券行情", Content = marketPage });
            marketTabs.Items.Add(new TabItem { Header = "报价提醒", Content = BuildPage(Notifications, false) });
            marketTabs.SelectionChanged += async (sender, args) =>
            {
                if (args.Source == marketTabs && IsConnected && !Busy)
                {
                    await ExecuteAsync(() => ApplyMarketFilterAsync(marketFilters.Read()));
                }
            };
            marketFilters = new ProductFilterPanel(Api, ExecuteAsync, ApplyMarketFilterAsync);
            filterPanels.Add(marketFilters);
            return WithSidebar(marketFilters, marketTabs);
        }

        private Task ApplyMarketFilterAsync(JObject filter)
        {
            marketFilter = (JObject)filter.DeepClone();
            if (!string.IsNullOrEmpty(market))
            {
                marketFilter["market"] = market;
            }
            string description = string.IsNullOrEmpty(market) ? "全部债券" : market == "CREDIT" ? "信用债" : "利率债";
            status.Text = description + " · " + (Prices.PriceMode == "BEST" ? "最优报价" : "全部价格档位");
            return marketTabs.SelectedIndex == 1
                ? Notifications.OpenAsync(marketFilter) : Prices.OpenAsync(marketFilter);
        }

        private FrameworkElement BuildProductManagement()
        {
            TabControl management = new TabControl();
            management.Items.Add(new TabItem { Header = "今日报价", Content = BuildCurrentQuoteManagement(),
                ToolTip = "当前有效及已撤销报价；交易日归档由服务端管理" });
            management.Items.Add(new TabItem { Header = "历史报价", Content = BuildHistory() });
            managementFilters = new ProductFilterPanel(Api, ExecuteAsync, async filter =>
            {
                if (management.SelectedIndex == 0)
                {
                    await Quotes.OpenAsync(ReadManagementFilter());
                }
                else
                {
                    await searchHistory(false);
                }
            });
            filterPanels.Add(managementFilters);
            return WithSidebar(managementFilters, management);
        }

        private static Grid WithSidebar(FrameworkElement sidebar, FrameworkElement content)
        {
            Grid layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(384), MinWidth = 384, MaxWidth = 384 });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });
            layout.ColumnDefinitions.Add(new ColumnDefinition());
            Border splitter = new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromRgb(48, 54, 61)) };
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(content, 2);
            layout.Children.Add(sidebar);
            layout.Children.Add(splitter);
            layout.Children.Add(content);
            return layout;
        }

        private Func<bool, Task> searchHistory;

        private FrameworkElement BuildMarketDetails(DataGrid marketGrid)
        {
            Grid layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition());
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(290), MinWidth = 220 });
            DockPanel detail = new DockPanel { Margin = new Thickness(6), Background = Background };
            TextBlock title = new TextBlock { Text = "报价明细", FontSize = 15, Margin = new Thickness(6) };
            DockPanel.SetDock(title, Dock.Top);
            detail.Children.Add(title);
            DataGrid entries = UiElements.Grid("方向|bookSide", "收益率|MDEntryPx", "数量（万）|MDEntrySize", "清算|SettlType");
            Button read = UiElements.Button("读取选中报价", async () => await ExecuteAsync(async () =>
            {
                if (!(entries.SelectedItem is JObject row) || row["quoteId"] == null)
                {
                    throw new InvalidOperationException("请先在明细中选择报价。");
                }
                await LoadQuoteAsync((string)row["quoteId"]);
                ShowEntryPage(true);
            }));
            DockPanel.SetDock(read, Dock.Bottom);
            detail.Children.Add(read);
            detail.Children.Add(entries);
            string selectedSecurity = null;
            DataGrid allQuotes = UiElements.Grid("代码|securityId", "简称|securityName", "方向|bookSide", "报价|MDEntryPx",
                "报量（万）|MDEntrySize", "报价机构ID|InstitutionID", "交易员ID|TraderID", "清算|SettlType", "报价时间|MDEntryTime");
            Grid main = new Grid();
            main.Children.Add(marketGrid);
            main.Children.Add(allQuotes);
            allQuotes.Visibility = Visibility.Collapsed;
            Action refresh = () =>
            {
                bool full = Prices.PriceMode == "FULL";
                allQuotes.Visibility = full ? Visibility.Visible : Visibility.Collapsed;
                marketGrid.Visibility = full ? Visibility.Collapsed : Visibility.Visible;
                if (full)
                {
                    JArray allRows = new JArray();
                    foreach (JObject sourceBook in Prices.Rows)
                    {
                        foreach (JObject sourceEntry in (JArray)sourceBook["marketData"]["NoMDEntries"])
                        {
                            JObject row = (JObject)sourceEntry.DeepClone();
                            row["securityId"] = sourceBook["securityId"].DeepClone();
                            row["securityName"] = sourceBook["marketData"]["Symbol"]?.DeepClone();
                            row["bookSide"] = (string)sourceEntry["MDEntryType"] == "0" ? "Bid" : "Ofr";
                            allRows.Add(row);
                        }
                    }
                    allQuotes.ItemsSource = allRows;
                }
                JObject book = Prices.Rows.FirstOrDefault(item => (string)item["securityId"] == selectedSecurity);
                title.Text = selectedSecurity == null ? "报价明细 · 请选择债券" : selectedSecurity + " · 报价明细";
                JArray rows = new JArray();
                if (book != null)
                {
                    foreach (JObject entry in (JArray)book["marketData"]["NoMDEntries"])
                    {
                        JObject row = (JObject)entry.DeepClone();
                        row["bookSide"] = (string)entry["MDEntryType"] == "0" ? "Bid" : "Ofr";
                        row["quoteId"] = book["quoteRefs"]?[(string)entry["MDEntryID"]]?["quoteId"]?.DeepClone();
                        rows.Add(row);
                    }
                }
                entries.ItemsSource = rows;
            };
            marketGrid.SelectionChanged += (sender, args) =>
            {
                if (marketGrid.SelectedItem is JObject book)
                {
                    selectedSecurity = (string)book["securityId"];
                    refresh();
                }
            };
            allQuotes.SelectionChanged += (sender, args) =>
            {
                if (allQuotes.SelectedItem is JObject row && selectedSecurity != (string)row["securityId"])
                {
                    selectedSecurity = (string)row["securityId"];
                    refresh();
                }
            };
            Prices.Changed += refresh;
            Grid.SetColumn(detail, 1);
            layout.Children.Add(main);
            layout.Children.Add(detail);
            return layout;
        }
    }
}
