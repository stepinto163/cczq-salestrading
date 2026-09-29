using STC.SalesTrading.Plugin.Ficc.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using netapi.Api;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    public sealed partial class SalesTradingView : UserControl, IDisposable
    {
        private readonly StcFiccTransport transport;
        public FiccApiClient Api { get; }
        public LivePage Quotes { get; }
        public LivePage PendingQuotes { get; }
        public LivePage Prices { get; }
        public LivePage Notifications { get; }
        public QuoteEditor Editor { get; }
        public JObject EditingQuote { get; private set; }
        public string LastError { get; private set; }
        public bool Busy { get; private set; }
        public bool IsConnected { get; private set; }
        private bool disposed;
        private readonly TextBlock status = new TextBlock { Margin = new Thickness(8), TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock editingLabel = new TextBlock { Margin = new Thickness(8) };
        private readonly TabControl tabs = new TabControl { TabStripPlacement = Dock.Top };
        private readonly List<JObject> drafts = new List<JObject>();
        private readonly Dictionary<string, Button> commands = new Dictionary<string, Button>();
        private readonly TextBox log = new TextBox { IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        private readonly Queue<string> logLines = new Queue<string>();
        private readonly DispatcherTimer logTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        private bool logDirty;

        public SalesTradingView()
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/STC.SalesTrading.Plugin;component/View/ProductTheme.xaml", UriKind.Relative)
            });
            Brush background = new SolidColorBrush(Color.FromRgb(23, 28, 36));
            Background = background;
            Foreground = Brushes.Gainsboro;
            tabs.Background = background;
            Style labels = new Style(typeof(TextBlock));
            labels.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gainsboro));
            Resources[typeof(TextBlock)] = labels;
            transport = new StcFiccTransport(ApiManager.Instance.GetDefaultApi(),
                SynchronizationContext.Current ?? new DispatcherSynchronizationContext(Dispatcher));
            Api = new FiccApiClient(transport);
            Quotes = new LivePage(Api, transport, "quote");
            PendingQuotes = new LivePage(Api, transport, "quote");
            Prices = new LivePage(Api, transport, "price");
            Notifications = new LivePage(Api, transport, "notification");
            foreach (LivePage page in Pages())
            {
                page.Failed += ShowError;
            }
            Editor = new QuoteEditor(Api, ShowError);
            DockPanel root = new DockPanel();
            DockPanel.SetDock(status, Dock.Bottom);
            root.Children.Add(status);
            root.Children.Add(tabs);
            Content = root;
            BuildProductEntry();
            tabs.Items.Add(new TabItem { Header = "行情中心", Content = BuildProductMarket() });
            tabs.Items.Add(new TabItem { Header = "报价管理", Content = BuildProductManagement() });
            BuildLog();
            transport.MessageLogged += AppendLog;
            logTimer.Tick += (sender, args) =>
            {
                if (logDirty && log.IsVisible)
                {
                    log.Text = string.Join(Environment.NewLine + "────────────────────────" + Environment.NewLine, logLines);
                    log.ScrollToEnd();
                    logDirty = false;
                }
            };
            logTimer.Start();
            Loaded += async (sender, args) =>
            {
                if (!IsConnected && !Busy && !disposed)
                {
                    await ExecuteAsync(InitializeAsync);
                }
            };
            UpdateButtons();
        }

        private IEnumerable<LivePage> Pages()
        {
            return new[] { PendingQuotes, Quotes, Prices, Notifications };
        }

        public async Task InitializeAsync()
        {
            Stopwatch timer = Stopwatch.StartNew();
            LogStartup("STARTING", timer.ElapsedMilliseconds);
            try
            {
                JObject dictionary = await Api.RequestProjectionAsync(SalesTradingApi.QuoteDictionaryQuery, ProtocolCodec.Decode<DictionaryRequest>(new JObject()));
                Editor.SetDictionary(dictionary);
                JObject watchlists = await Api.RequestProjectionAsync(SalesTradingApi.WatchlistQuery, ProtocolCodec.Decode<WatchlistQueryRequest>(new JObject()));
                JObject filterGroups = await Api.RequestProjectionAsync(SalesTradingApi.FilterGroupQuery, ProtocolCodec.Decode<FilterGroupQueryRequest>(new JObject()));
                foreach (ProductFilterPanel panel in filterPanels)
                {
                    panel.SetDictionary(dictionary);
                    panel.SetSavedGroups((JArray)watchlists["watchlists"], (JArray)filterGroups["filterGroups"]);
                }
                await PendingQuotes.OpenAsync(EmptyFilter(), true);
                await Quotes.OpenAsync(EmptyFilter());
                await Prices.OpenAsync(EmptyFilter());
                await Notifications.OpenAsync(EmptyFilter());
                IsConnected = true;
                LogStartup("READY", timer.ElapsedMilliseconds);
                status.Text = "报价、行情、提醒已连接；当前页实时更新。";
            }
            catch (Exception error)
            {
                LogStartup("FAILED", timer.ElapsedMilliseconds, error);
                throw;
            }
        }

        private static void LogStartup(string state, long elapsed, Exception error = null)
        {
            string message = "COMPONENT " + new JObject
            {
                ["service"] = "STCClient", ["component"] = "SALES_TRADING", ["state"] = state,
                ["mode"] = LocalIntegrationProfile.Current.Enabled ? "local" : "stc",
                ["implementation"] = nameof(SalesTradingView), ["target"] = "QuoteSvr/NotificationSvr via authenticated GW",
                ["elapsedMillis"] = elapsed
            }.ToString(Formatting.None);
            Console.WriteLine(message);
            if (error == null)
            {
                com.gateway.log.LogHelper.Info(message);
            }
            else
            {
                com.gateway.log.LogHelper.Error(message, error);
            }
        }

        private void AddCommand(Panel panel, string command, string title, Func<Task> action)
        {
            Button button = UiElements.Button(title, async () => await ExecuteAsync(action));
            commands.Add(command, button);
            panel.Children.Add(button);
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            if (Busy || disposed)
            {
                return;
            }
            Busy = true;
            LastError = null;
            status.Foreground = Brushes.MediumSeaGreen;
            UpdateButtons();
            try
            {
                await action();
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
                ShowError(error.Message);
            }
            finally
            {
                Busy = false;
                UpdateButtons();
            }
        }

        public async Task CreateAsync()
        {
            JObject response = await Api.CreateAsync(new[] { Editor.Read() });
            JObject result = (JObject)FiccApiClient.RequireQuoteResults(response)[0];
            await LoadQuoteAsync((string)result["quoteId"]);
            ShowEntryPage(true);
            status.Text = "已保存待确认报价。";
        }

        private async Task CreateBatchAsync()
        {
            if (drafts.Count == 0)
            {
                throw new InvalidOperationException("请先加入批量草稿。");
            }
            // Retain failed rows; never repeat successful rows in the next batch.
            foreach (JObject draft in drafts)
            {
                if (draft["clientQuoteId"] == null)
                {
                    draft["clientQuoteId"] = Guid.NewGuid().ToString("N");
                }
            }
            JObject response = await Api.CreateAsync(drafts);
            JArray results = (JArray)response["results"];
            var successful = new HashSet<string>(results.OfType<JObject>().Where(row => (string)row["Code"] == "0000")
                .Select(row => (string)row["clientQuoteId"]));
            drafts.RemoveAll(row => successful.Contains((string)row["clientQuoteId"]));
            RefreshDrafts();
            FiccApiClient.RequireQuoteResults(response);
            status.Text = "批量创建成功 " + successful.Count + " 条；请在待确认列表确认。";
        }

        public async Task LoadQuoteAsync(string quoteId)
        {
            EditingQuote = await Api.GetQuoteAsync(quoteId);
            if (EditingQuote == null)
            {
                throw new InvalidOperationException("报价不存在或无权查看。");
            }
            Editor.Set(EditingQuote);
            UpdateButtons();
        }

        public async Task OperateAsync(string operation)
        {
            if (EditingQuote == null)
            {
                throw new InvalidOperationException("请先选择并读取报价。");
            }
            JObject changes = operation == "confirm" || operation == "update" ? Editor.Read() : null;
            JObject response = await Api.OperateAsync(operation, new[] { EditingQuote }, changes);
            FiccApiClient.RequireQuoteResults(response);
            await LoadQuoteAsync((string)EditingQuote["quoteId"]);
            status.Text = "操作成功：" + operation;
        }

        private void UpdateButtons()
        {
            string state = (string)EditingQuote?["status"];
            foreach (var command in commands)
            {
                bool allowed = true;
                switch (command.Key)
                {
                    case "create":
                        allowed = EditingQuote == null;
                        break;
                    case "confirm":
                        allowed = state == "PENDING_CONFIRM";
                        break;
                    case "update":
                        allowed = state == "PENDING_CONFIRM" || state == "ACTIVE";
                        break;
                    case "refer":
                        allowed = state == "ACTIVE";
                        break;
                    case "unrefer":
                        allowed = state == "REFERRED";
                        break;
                    case "delete":
                        allowed = state == "PENDING_CONFIRM" || state == "REFERRED";
                        break;
                    case "history":
                        allowed = EditingQuote != null;
                        break;
                }
                command.Value.IsEnabled = allowed && !Busy;
            }
            foreach (ProductFilterPanel panel in filterPanels)
            {
                panel.IsEnabled = !Busy;
            }
            editingLabel.Text = EditingQuote == null ? "正在录入新报价" : "编辑报价 " + EditingQuote["quoteId"]
                + " · " + state + " · revision " + EditingQuote["revision"] + "（实时列表更新不覆盖正在编辑的版本）";
        }

        private FrameworkElement BuildPage(LivePage page, bool pendingOnly)
        {
            DockPanel panel = new DockPanel();
            WrapPanel tools = new WrapPanel();
            TextBox security = new TextBox { Width = 145, Margin = new Thickness(4), ToolTip = "债券代码，留空查询全部" };
            TextBlock count = new TextBlock { Margin = new Thickness(8) };
            if (pendingOnly)
            {
                tools.Children.Add(new TextBlock { Text = "待确认报价", Margin = new Thickness(8) });
                tools.Children.Add(security);
            }
            tools.Children.Add(UiElements.Button("刷新", async () => await ExecuteAsync(async () =>
            {
                if (page == Prices || page == Notifications)
                {
                    await ApplyMarketFilterAsync(marketFilters.Read());
                }
                else
                {
                    await page.OpenAsync(pendingOnly ? Filter(security.Text) : managementFilters.Read(), pendingOnly);
                }
            })));
            tools.Children.Add(UiElements.Button("上一页", async () => await ExecuteAsync(() => page.QueryAsync(Math.Max(1, page.PageNumber - 1)))));
            tools.Children.Add(UiElements.Button("下一页", async () => await ExecuteAsync(async () =>
            {
                if (page.PageNumber * page.PageSize < page.TotalCount)
                {
                    await page.QueryAsync(page.PageNumber + 1);
                }
            })));
            tools.Children.Add(UiElements.Button("返回最新", async () => await ExecuteAsync(() => page.QueryAsync(1))));
            tools.Children.Add(count);
            DockPanel.SetDock(tools, Dock.Top);
            panel.Children.Add(tools);
            bool isQuote = page == Quotes || page == PendingQuotes;
            DataGrid grid = isQuote ? UiElements.Grid("债券|securityId", "状态|status", "方向|side", "价格|price", "数量(万)|quantity",
                "机构|institutionName", "联系人|contactName", "更新时间|updateTime", "报价ID|quoteId")
                : page == Prices ? UiElements.Grid("债券|securityId", "更新时间|updateTime")
                : UiElements.Grid("债券|securityId", "提醒内容|content", "已读时间|readTime", "更新时间|updateTime", "通知ID|notificationId");
            grid.ItemsSource = page.Rows;
            if (page == Prices)
            {
                foreach (string side in new[] { "0", "1" })
                {
                    grid.Columns.Insert(side == "0" ? 1 : 2, new DataGridTextColumn
                    {
                        Header = side == "0" ? "Bid 收益率" : "Ofr 收益率",
                        Binding = new Binding { Converter = new BestPriceConverter(), ConverterParameter = side },
                        Width = new DataGridLength(130)
                    });
                }
                tools.Children.Add(UiElements.Button("查看报价明细", () =>
                {
                    if (grid.SelectedItem is JObject book)
                    {
                        ShowBook(book);
                    }
                }));
            }
            if (isQuote)
            {
                if (pendingOnly)
                {
                    grid.SelectionChanged += async (sender, args) =>
                    {
                        if (!Busy && grid.SelectedItem is JObject row && (string)row["quoteId"] != (string)EditingQuote?["quoteId"])
                        {
                            await ExecuteAsync(() => LoadQuoteAsync((string)row["quoteId"]));
                        }
                    };
                }
                if (page == Quotes)
                {
                    foreach (string operation in new[] { "refer|Refer", "unrefer|Unrefer", "delete|删除" })
                    {
                        string[] parts = operation.Split('|');
                        tools.Children.Add(UiElements.Button(parts[1], async () => await ExecuteAsync(async () =>
                        {
                            if (!(grid.SelectedItem is JObject selected))
                            {
                                throw new InvalidOperationException("请先选择报价。");
                            }
                            JObject result = await Api.OperateAsync(parts[0], new[] { selected });
                            FiccApiClient.RequireQuoteResults(result);
                        })));
                    }
                }
                tools.Children.Add(UiElements.Button("读取 / 操作选中报价", async () => await ExecuteAsync(async () =>
                {
                    if (!(grid.SelectedItem is JObject quote))
                    {
                        throw new InvalidOperationException("请先选择报价。");
                    }
                    await LoadQuoteAsync((string)quote["quoteId"]);
                    ShowEntryPage(true);
                })));
                grid.MouseDoubleClick += async (sender, args) =>
                {
                    if (grid.SelectedItem is JObject quote)
                    {
                        await ExecuteAsync(async () =>
                        {
                            await LoadQuoteAsync((string)quote["quoteId"]);
                            ShowEntryPage(true);
                        });
                    }
                };
            }
            else if (page == Notifications)
            {
                tools.Children.Add(UiElements.Button("标为已读", async () => await ExecuteAsync(async () =>
                {
                    if (grid.SelectedItem is JObject notification)
                    {
                        await Api.RequestProjectionAsync(SalesTradingApi.NotificationRead, ProtocolCodec.Decode<NotificationReadRequest>(new JObject
                        {
                            ["notificationIds"] = new JArray(notification["notificationId"].DeepClone()), ["read"] = true
                        }));
                    }
                })));
            }
            page.Changed += () => count.Text = "第 " + page.PageNumber + " 页 / 共 " + page.TotalCount + " 条";
            panel.Children.Add(page == Prices ? BuildMarketDetails(grid) : (UIElement)grid);
            return panel;
        }

        private FrameworkElement BuildHistory()
        {
            DockPanel panel = new DockPanel();
            WrapPanel tools = new WrapPanel();
            DatePicker from = new DatePicker { SelectedDate = DateTime.Today, Margin = new Thickness(4) };
            DatePicker to = new DatePicker { SelectedDate = DateTime.Today, Margin = new Thickness(4) };
            DataGrid grid = CreateManagementGrid();
            string cursor = null;
            JObject search = null;
            Func<bool, Task> query = async next =>
            {
                if (!next)
                {
                    search = new JObject { ["fromDate"] = from.SelectedDate?.ToString("yyyy-MM-dd"),
                        ["toDate"] = to.SelectedDate?.ToString("yyyy-MM-dd"), ["ownerScope"] = "MINE", ["limit"] = 30,
                        ["filter"] = managementFilters.Read() };
                    cursor = null;
                }
                else if (cursor == null)
                {
                    return;
                }
                JObject request = (JObject)search.DeepClone();
                if (cursor != null)
                {
                    request["cursor"] = cursor;
                }
                JObject response = await Api.RequestProjectionAsync(SalesTradingApi.QuoteHistorySearch, ProtocolCodec.Decode<QuoteHistorySearchRequest>(request));
                grid.ItemsSource = ((JArray)response["rows"]).OfType<JObject>().Select(row =>
                {
                    JObject quote = (JObject)row["quote"];
                    if (row["reference"] is JObject reference)
                    {
                        securityReference[(string)quote["securityId"]] = reference;
                    }
                    return ManagementRow(quote);
                }).ToArray();
                cursor = (string)response["nextCursor"];
                status.Text = cursor == null ? "历史查询完成" : "还有历史报价，可查询下一页。";
            };
            searchHistory = query;
            tools.Children.Add(from);
            tools.Children.Add(to);
            tools.Children.Add(UiElements.Button("查询历史", async () => await ExecuteAsync(() => query(false))));
            tools.Children.Add(UiElements.Button("下一页", async () => await ExecuteAsync(() => query(true))));
            DockPanel.SetDock(tools, Dock.Top);
            panel.Children.Add(tools);
            panel.Children.Add(grid);
            return panel;
        }

        private void ShowBook(JObject book)
        {
            DataGrid grid = UiElements.Grid("方向(0=Bid)|MDEntryType", "收益率|MDEntryPx", "数量(万)|MDEntrySize",
                "清算|SettlType", "报价ID|quoteId");
            JArray rows = new JArray();
            foreach (JObject entry in (JArray)book["marketData"]["NoMDEntries"])
            {
                JObject row = (JObject)entry.DeepClone();
                row["quoteId"] = book["quoteRefs"]?[(string)entry["MDEntryID"]]?["quoteId"]?.DeepClone();
                rows.Add(row);
            }
            grid.ItemsSource = rows;
            DockPanel content = new DockPanel();
            Window window = new Window { Title = "最优价格档内报价 · " + book["securityId"], Width = 800, Height = 450,
                Owner = Window.GetWindow(this), Content = content, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            Button edit = UiElements.Button("读取选中报价", async () =>
            {
                if (grid.SelectedItem is JObject row && row["quoteId"] != null)
                {
                    window.Close();
                    await ExecuteAsync(async () =>
                    {
                        await LoadQuoteAsync((string)row["quoteId"]);
                        tabs.SelectedIndex = 0;
                    });
                }
            });
            DockPanel.SetDock(edit, Dock.Bottom);
            content.Children.Add(edit);
            content.Children.Add(grid);
            window.ShowDialog();
        }

        private async Task ShowQuoteHistoryAsync()
        {
            JObject response = await Api.RequestProjectionAsync(SalesTradingApi.QuoteHistoryQuery, ProtocolCodec.Decode<QuoteHistoryRequest>(new JObject
            {
                ["quoteId"] = EditingQuote["quoteId"].DeepClone(), ["limit"] = 100
            }));
            DataGrid grid = UiElements.Grid("动作|action", "操作时间|operationTime", "操作人|operatorUserId", "版本|revision");
            grid.ItemsSource = (JArray)(response["history"] ?? response["records"]);
            new Window { Title = "报价变更历史", Content = grid, Width = 760, Height = 430,
                Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog();
        }

        private void BuildLog()
        {
            DockPanel panel = new DockPanel();
            WrapPanel tools = new WrapPanel();
            tools.Children.Add(UiElements.Button("清空日志", () =>
            {
                logLines.Clear();
                log.Clear();
            }));
            CheckBox heartbeat = new CheckBox { Content = "显示 HEARTBEAT", Margin = new Thickness(8) };
            heartbeat.Checked += (sender, args) => transport.LogHeartbeat = true;
            heartbeat.Unchecked += (sender, args) => transport.LogHeartbeat = false;
            tools.Children.Add(heartbeat);
            DockPanel.SetDock(tools, Dock.Top);
            panel.Children.Add(tools);
            panel.Children.Add(log);
            Expander diagnostics = new Expander { Header = "消息日志", Content = panel, IsExpanded = false };
            panel.Height = 180;
            DockPanel root = (DockPanel)Content;
            DockPanel.SetDock(diagnostics, Dock.Bottom);
            root.Children.Insert(0, diagnostics);
        }

        private void AppendLog(string message)
        {
            if (disposed)
            {
                return;
            }
            logLines.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
            while (logLines.Count > 100)
            {
                logLines.Dequeue();
            }
            logDirty = true;
        }

        private void ShowError(string message)
        {
            LastError = message;
            status.Foreground = Brushes.IndianRed;
            status.Text = message;
        }

        private static JObject EmptyFilter()
        {
            return new JObject { ["conditions"] = new JArray() };
        }

        private static JObject Filter(string securityId)
        {
            JObject filter = EmptyFilter();
            if (!string.IsNullOrWhiteSpace(securityId))
            {
                ((JArray)filter["conditions"]).Add(new JObject { ["field"] = "securityId", ["operator"] = "EQ", ["value"] = securityId.Trim() });
            }
            return filter;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            logTimer.Stop();
            CloseViewsAsync();
        }

        private async void CloseViewsAsync()
        {
            var closing = new List<Task>();
            foreach (LivePage page in Pages())
            {
                closing.Add(page.CloseAsync());
                page.Dispose();
            }
            try
            {
                await Task.WhenAll(closing);
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
            }
            finally
            {
                transport.MessageLogged -= AppendLog;
                transport.Dispose();
            }
        }
    }
}
