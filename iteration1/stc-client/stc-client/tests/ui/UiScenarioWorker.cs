using SalesTrading.UiTests.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;

namespace SalesTrading.UiTests
{
    // Hosts the production STC application and its real controls, not replacement screens.
    // User operations go through UI button handlers. APIs assert results and clean up fixtures;
    // the negotiation case also seeds incoming QTrade proposals through the real intake endpoint.
    internal sealed partial class UiScenarioWorker
    {
        private static string reportDirectory;
        private static CaseDefinition definition;
        private static string recoveryManifest;
        private static readonly List<JObject> events = new List<JObject>();
        private readonly HashSet<string> ownedQuotes = new HashSet<string>();
        private readonly List<QuoteConfirmRowViewModel> submittedPreview = new List<QuoteConfirmRowViewModel>();
        private readonly TestApiClient api = new TestApiClient(new TestGatewayTransport());
        private MainView view;
        private MainViewModel model;
        private Window shell;
        private STC.SalesTrading.Plugin.Modules.MarketCenter.View.QuoteEntryDialog editDialog;
        private string quoteId;
        private int stepNumber;
        private bool cleaning;
        private Exception noticeAutomationFailure;
        private DemoCaptionWindow captions;
        private static ControlHighlight highlight;
        private readonly Stopwatch elapsed = Stopwatch.StartNew();

        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3) return 2;
            recoveryManifest = args.Length == 3 ? Path.GetFullPath(args[2]) : null;
            reportDirectory = Path.GetFullPath(args[1]);
            Directory.CreateDirectory(reportDirectory);
            definition = CaseDefinition.Read(args[0]);
            STC.App application = new STC.App();
            application.InitializeComponent();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            UiScenarioWorker worker = new UiScenarioWorker();
            DispatcherTimer noticeTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            noticeTimer.Tick += (sender, eventArgs) =>
            {
                try
                {
                    UserNoticeAutomation.DismissVisible(detail => Emit("用户使用须知", "DISMISSED", detail));
                }
                catch (Exception error)
                {
                    noticeTimer.Stop();
                    worker.noticeAutomationFailure = error;
                    Emit("用户使用须知", "FAILED", error.ToString());
                }
            };
            noticeTimer.Start();
            application.DispatcherUnhandledException += (sender, error) =>
            {
                // Unhandled UI exceptions fail the case; never turn them into a passing result.
                Emit("application", "FAILED", error.Exception.ToString());
            };
            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                int result = await worker.RunAsync();
                noticeTimer.Stop();
                application.Shutdown(result);
                Environment.Exit(result);
            }), DispatcherPriority.ApplicationIdle);
            return application.Run();
        }

        private static void Emit(string step, string state, string detail, long? waitMillis = null)
        {
            string secret = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
            if (!string.IsNullOrEmpty(secret)) detail = (detail ?? "").Replace(secret, "[REDACTED]");
            JObject entry = new JObject
            {
                ["time"] = DateTime.Now.ToString("O"), ["caseId"] = definition.Id,
                ["step"] = step, ["waitMillis"] = waitMillis, ["state"] = state, ["detail"] = detail
            };
            events.Add(entry);
            File.AppendAllText(Path.Combine(reportDirectory, "events.jsonl"), entry.ToString(Formatting.None) + Environment.NewLine);
        }

        private async Task<int> RunAsync()
        {
            string outcome = "PASSED";
            bool cleanupPassed = true;
            try
            {
                captions = new DemoCaptionWindow();
                highlight = new ControlHighlight();
                await ShowCaptionAsync("正在执行", "登录 STC 并打开销售交易");
                Emit("登录", "RUNNING", "启动正式 STC 登录窗口，连接真实服务");
                await LoginAsync();
                Emit("登录", "PASSED", "已登录并加载报价、行情及提醒页面");
                if (recoveryManifest != null)
                {
                    Require(Path.GetFileName(recoveryManifest) == "owned-quotes.json",
                        "恢复仅接受自动化生成的 owned-quotes.json");
                    var sourceCase = CaseDefinition.Read(Path.Combine(Path.GetDirectoryName(recoveryManifest), "case.json"));
                    Require(sourceCase.Id == definition.Id, "恢复清单与原用例不匹配");
                    foreach (string id in JArray.Parse(File.ReadAllText(recoveryManifest)).Values<string>())
                    {
                        JObject record = await api.GetQuoteAsync(id);
                        Require((string)record["ownerUserId"] == STC.Common.Utils.LocalEnv.UserInfo.UserId,
                            "恢复清单包含其他账号的报价：" + id);
                        Track(id);
                    }
                    Emit("恢复范围", "PASSED", "仅清理原报告列出的本账号报价；不宣称恢复丢失的分组或偏好快照");
                }
                else
                {
                    foreach (TestStep step in definition.Steps)
                    {
                        ThrowIfCancelled();
                        stepNumber++;
                        highlight.SetSuspended(step.Action == "filter.latency");
                        if (step.Action == "filter.latency")
                        {
                            captions.Suspend();
                        }
                        else
                        {
                            await ShowCaptionAsync("正在执行", step.Name);
                        }
                        if (editDialog != null && step.Action != "management.edit" && step.Action != "management.save"
                            && step.Action != "management.reject" && step.Action != "quote.assert") await CloseEditorAsync();
                        Emit(step.Name, "RUNNING", step.Action);
                        Stopwatch duration = Stopwatch.StartNew();
                        await ExecuteAsync(step);
                        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                        Capture(stepNumber.ToString("D2") + "-passed");
                        duration.Stop();
                        Emit(step.Name, "PASSED", duration.ElapsedMilliseconds + " ms");
                        await ShowCaptionAsync("已完成", step.Name);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                outcome = "CANCELLED";
                Emit("停止", outcome, "收到停止请求，开始清理本例数据");
            }
            catch (Exception error)
            {
                outcome = "FAILED";
                Emit(stepNumber == 0 ? "登录" : definition.Steps[stepNumber - 1].Name, "FAILED", error.ToString());
                Capture(stepNumber.ToString("D2") + "-failed");
            }
            finally
            {
                cleaning = true;
                highlight?.SetSuspended(true);
                try
                {
                    await ShowCaptionAsync(outcome == "PASSED" ? "收尾" : outcome == "CANCELLED" ? "已停止" : "测试失败",
                        "清理本例测试数据");
                }
                catch (Exception captionError)
                {
                    // 字幕故障不能阻断测试数据清理，但仍须如实标记本例失败。
                    outcome = "FAILED";
                    Emit("演示字幕", "FAILED", captionError.ToString());
                }
                try
                {
                    await CleanupAsync();
                    Emit("清理", "PASSED", "本例报价已删除；提醒夹具及偏好已恢复（如使用）；没有清空数据库或其他用户数据");
                }
                catch (Exception error)
                {
                    cleanupPassed = false;
                    Emit("清理", "FAILED", error.ToString());
                }
                if (!cleanupPassed && outcome == "PASSED") outcome = "FAILED";
                JObject result = new JObject
                {
                    ["caseId"] = definition.Id, ["name"] = definition.Name, ["status"] = outcome,
                    ["elapsedMillis"] = elapsed.ElapsedMilliseconds, ["cleanupPassed"] = cleanupPassed,
                    ["quoteIds"] = new JArray(ownedQuotes), ["events"] = new JArray(events)
                };
                File.WriteAllText(Path.Combine(reportDirectory, "result.json"), result.ToString());
                Emit("结果", outcome, "耗时 " + elapsed.ElapsedMilliseconds + " ms");
                captions?.Close();
                highlight?.Dispose();
            }
            return outcome == "PASSED" ? 0 : 1;
        }

        private async Task ShowCaptionAsync(string phase, string action)
        {
            captions?.Present(shell, definition.Name, stepNumber, definition.Steps.Count, phase, action);
            // 在操作计时前完成字幕渲染；不占用输入焦点，不附加固定睡眠。
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }

        private async Task LoginAsync()
        {
            string user = Environment.GetEnvironmentVariable("STC_UI_USER");
            string password = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
            Require(!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password), "请填写测试账号和密码");
            STC.Frame.UI.LoginGUI login = null;
            await Until(() => (login = Application.Current.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault()) != null && login.IsLoaded, "登录窗口", 45);
            await SetTextAsync((TextBox)login.FindName("tbUsername"), user);
            var passwordBox = (PasswordBox)login.FindName("pbPwd");
            await BeforeUiActionAsync(passwordBox, "输入密码");
            passwordBox.Password = password;
            await ClickAsync((Button)login.FindName("btnLogin"));
            STC.Frame.Window1 frame = null;
            await Until(() => (frame = Application.Current.Windows.OfType<STC.Frame.Window1>().FirstOrDefault()) != null
                && frame.IsVisible && frame.PluginManager != null, "STC 主窗口", 60);
            shell = frame;
            if (!frame.Panel.Items.OfType<TabItem>().Any(item => item.Content is MainView))
            {
                FrameworkElement logo = (FrameworkElement)frame.FindName("btnLogo");
                await ClickAsync((Button)logo.FindName("btnMenu"));
                DataGrid menu = (DataGrid)logo.FindName("dgMenu");
                await Until(() => menu.Items.Cast<object>().Any(item => Convert.ToString(item.GetType().GetProperty("PluginKey")?.GetValue(item)) == "230"), "销售交易菜单");
                object salesTrading = menu.Items.Cast<object>().Single(item => Convert.ToString(item.GetType().GetProperty("PluginKey")?.GetValue(item)) == "230");
                await SelectItemAsync(menu, salesTrading);
                menu.CurrentItem = salesTrading;
                await BeforeUiActionAsync(menu, "打开销售交易");
                menu.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
                {
                    RoutedEvent = System.Windows.Input.Mouse.MouseUpEvent
                });
            }
            await Until(() =>
            {
                view = frame?.Panel.Items.OfType<TabItem>().Select(item => item.Content).OfType<MainView>().FirstOrDefault();
                model = view?.DataContext as MainViewModel;
                shell = frame;
                return model != null;
            }, "登录完成及销售交易模块加载", 60);
            shell.WindowState = WindowState.Normal;
            shell.Width = 1440;
            shell.Height = 950;
            await SelectItemAsync(frame.Panel, frame.Panel.Items.OfType<TabItem>().Single(item => ReferenceEquals(item.Content, view)));
            shell.UpdateLayout();
            await Until(() => view.IsLoaded && view.IsVisible && model.NavItems.Count > 0, "销售交易界面完成渲染");
            await Until(() => !model.QuoteMgmtPage.Today.IsLoading && !model.MarketCenterPage.RateAll.IsLoading
                && !model.MarketCenterPage.Alert.IsLoading, "报价、行情及提醒初始化", 45);
            Require(string.IsNullOrEmpty(model.QuoteMgmtPage.Today.ErrorMessage), model.QuoteMgmtPage.Today.ErrorMessage);
            Require(string.IsNullOrEmpty(model.MarketCenterPage.RateAll.ErrorMessage), model.MarketCenterPage.RateAll.ErrorMessage);
            Require(string.IsNullOrEmpty(model.MarketCenterPage.Alert.ErrorMessage), model.MarketCenterPage.Alert.ErrorMessage);
        }

        private string Parameter(TestStep step, string name)
        {
            return ((string)step.Parameters[name] ?? "").Replace("${securityId}", Environment.GetEnvironmentVariable("STC_UI_SECURITY") ?? "240210.IB");
        }

        private async Task ExecuteAsync(TestStep step)
        {
            // 每个断言负责定位自己的目标；不得在验证结束后覆盖成整个页面。
            highlight?.Clear();
            try
            {
                bool serverOnly = new[] { "quote.assert", "history.assert", "quote.fields" }.Contains(step.Action);
                if (serverOnly)
                {
                    await BeginVerificationAsync("服务端校验（无对应界面控件） · " + step.Name);
                }
                await ExecuteActionAsync(step);
                if (serverOnly)
                {
                    await HoldVerificationAsync(null, "服务端校验 · " + step.Name + " · 结果见报告 JSON");
                }
            }
            finally
            {
                if (highlight != null)
                {
                    highlight.VerificationPinned = false;
                }
                verificationStepGranted = false;
            }
        }

        private async Task ExecuteActionAsync(TestStep step)
        {
            switch (step.Action)
            {
                case "quote.assert":
                    Require(quoteId != null, "本例尚未创建报价");
                    JObject quote = await api.GetQuoteAsync(quoteId);
                    Require((string)quote["status"] == Parameter(step, "status"), "服务端状态不匹配：" + quote["status"]);
                    Require((decimal)quote["price"] == decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture), "服务端价格不匹配");
                    File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-quote.json"), quote.ToString());
                    break;
                case "pending.assert":
                    await CloseEditorAsync();
                    await NavigateAsync("entry");
                    await Until(() => model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).Any(row => row.QuoteId == quoteId), "报价确认页接收本例报价");
                    var pendingPage = Descendants<QuoteConfirmView>(view).Single(control => control.IsVisible);
                    var pendingRow = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).Single(row => row.QuoteId == quoteId);
                    // 分组表保持虚拟化；先滚动并展开本例行，再判断实际控件是否呈现。
                    await SelectPendingRowAsync(pendingPage, pendingRow, false);
                    await VerifyAsync(FindVerificationRow(pendingRow), "报价确认 · 报价 " + quoteId + " 已显示",
                        () => Require(pendingRow.QuoteId == quoteId, "待确认报价标识不匹配"));
                    await Until(() => Descendants<DataGrid>(view).Any(grid => grid.Items.OfType<QuoteConfirmRowViewModel>()
                        .Any(row => row.QuoteId == quoteId)), "报价确认表呈现本例报价");
                    break;
                case "management.assert":
                    await CloseEditorAsync();
                    await NavigateAsync("mgmt");
                    await Until(() => model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == quoteId), "报价管理接收本例报价");
                    await VerifyAsync(FindVerificationRow(model.QuoteMgmtPage.Today.Rows.First(row => row.QuoteId == quoteId)),
                        "报价管理 · 报价 " + quoteId + " 已显示", () => Require(model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == quoteId), "报价已离开当前列表"));
                    Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>().Any(item => (string)item.GetType().GetProperty("QuoteId")?.GetValue(item) == quoteId)), "报价管理表未显示本例报价");
                    break;
                case "market.assert":
                    await CloseEditorAsync();
                    await NavigateAsync("market");
                    object rateTab = model.MarketCenterPage.SubTabs.Single(tab => tab.Key == "rateAll");
                    await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(rateTab)), rateTab);
                    await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    view.UpdateLayout();
                    await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == quoteId), "行情中心接收本例报价");
                    await VerifyAsync(FindVerificationRow(model.MarketCenterPage.RateAll.Rows.First(row => row.QuoteId == quoteId)),
                        "行情中心 · 报价 " + quoteId + " 已显示", () => Require(model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == quoteId), "报价已离开当前列表"));
                    Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>().Any(item => (string)item.GetType().GetProperty("QuoteId")?.GetValue(item) == quoteId)), "行情表未显示本例报价");
                    break;
                case "history.assert":
                    JObject history = await api.RequestProjectionAsync(SalesTradingApi.QuoteHistoryQuery, ProtocolCodec.Decode<QuoteHistoryRequest>(new JObject { ["quoteId"] = quoteId, ["limit"] = 100 }));
                    Require(((JArray)history["records"]).Count >= int.Parse(Parameter(step, "minimumCount")), "持久化变更历史条数不足");
                    File.WriteAllText(Path.Combine(reportDirectory, "history.json"), history.ToString());
                    break;
                case "parse.keyboard":
                case "parse":
                    await CloseEditorAsync();
                    await NavigateAsync("entry");
                    await SelectTabAsync("quote-precise");
                    await Until(() => Descendants<PreciseQuoteView>(view).Any(control => control.IsVisible), "精确报价页面");
                    PreciseQuoteView precise = Descendants<PreciseQuoteView>(view).First(control => control.IsVisible);
                    TextBox inputBox = (TextBox)precise.FindName("InputBox");
                    if (step.Action == "parse.keyboard")
                    {
                        string[] lines = Parameter(step, "text").Split('\n');
                        Require(lines.Length == 2, "键盘用例要求两行输入");
                        inputBox.Focus();
                        await SetTextAsync(inputBox, lines[0]);
                        inputBox.CaretIndex = inputBox.Text.Length;
                        var keyboard = System.Windows.Input.Keyboard.PrimaryDevice;
                        var previewKey = new System.Windows.Input.KeyEventArgs(keyboard, PresentationSource.FromVisual(inputBox),
                            Environment.TickCount, System.Windows.Input.Key.Return)
                            { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent };
                        await BeforeUiActionAsync(inputBox, "按 Enter 换行");
                        inputBox.RaiseEvent(previewKey);
                        Require(!previewKey.Handled && !model.QuotePage.Precise.IsParsing, "Enter 被识别命令拦截");
                        inputBox.RaiseEvent(new System.Windows.Input.KeyEventArgs(keyboard, PresentationSource.FromVisual(inputBox),
                            Environment.TickCount, System.Windows.Input.Key.Return)
                            { RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent });
                        Require(inputBox.Text.Contains("\n"), "Enter 没有插入换行");
                        await BeforeUiActionAsync(inputBox, "输入第二行文本");
                        inputBox.SelectedText = lines[1];
                        Require(inputBox.Text.Replace("\r\n", "\n") == Parameter(step, "text"), "键盘输入未保留两行原文");
                    }
                    else
                    {
                        await SetTextAsync(inputBox, Parameter(step, "text"));
                    }
                    await ClickAsync(Button(precise, "识别报价"));
                    await AwaitCommand(model.QuotePage.Precise.IdentifyCommand.ExecutionTask);
                    File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-parse.json"), new JObject
                    {
                        ["input"] = Parameter(step, "text"),
                        ["status"] = model.QuotePage.Precise.StatusMessage,
                        ["rows"] = JArray.FromObject(model.QuotePage.Precise.Rows.Select(row => new
                        {
                            row.Code, row.Direction, row.Price, row.Volume, row.PriceType,
                            row.TradeDate, row.Settlement, row.Institution, row.Trader
                        }))
                    }.ToString());
                    Require(model.QuotePage.Precise.Rows.Count == int.Parse(Parameter(step, "expectedRows")), "识别数量不匹配：" + model.QuotePage.Precise.StatusMessage);
                    var previewClientIds = new HashSet<string>(model.QuotePage.Precise.Rows.Select(row => row.Row.ClientQuoteId));
                    await VerifyPreviewRemainsLocalAsync(previewClientIds);
                    break;
                case "parse.assert":
                    QuoteConfirmRowViewModel parsed = model.QuotePage.Precise.Rows.Single();
                    await VerifyCellAsync(parsed, "代码", Parameter(step, "securityId"), () => parsed.Code);
                    await VerifyCellAsync(parsed, "报价机构", Parameter(step, "institution"), () => parsed.Institution);
                    await VerifyCellAsync(parsed, "报价交易员", Parameter(step, "trader"), () => parsed.Trader);
                    await VerifyCellAsync(parsed, "报价", decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture),
                        () => decimal.Parse(parsed.Price, CultureInfo.InvariantCulture));
                    await VerifyAsync(null, "模型校验 · CRM 机构与联系人 ID 不为空", () =>
                        Require(!string.IsNullOrEmpty(parsed.Row.InstitutionId) && !string.IsNullOrEmpty(parsed.Row.ContactId), "CRM 名称未解析为真实 ID"));
                    break;
                case "parse.select":
                    QuoteConfirmRowViewModel selected = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    DataGrid preview = Descendants<DataGrid>(view).First(grid => grid.IsVisible && grid.Items.Contains(selected));
                    preview.ScrollIntoView(selected);
                    preview.UpdateLayout();
                    CheckBox checkbox = Descendants<CheckBox>(preview).First(control => ReferenceEquals(control.DataContext, selected));
                    bool expected = bool.Parse(Parameter(step, "selected"));
                    if (checkbox.IsChecked != expected) await ClickAsync(checkbox);
                    Require(selected.IsSelected == expected && model.QuotePage.Precise.SelectedCount == int.Parse(Parameter(step, "expectedCount")), "勾选状态与已选数量不一致");
                    break;
                case "parse.confirm":
                    var selectedPreview = model.QuotePage.Precise.Rows.Where(row => row.IsSelected).ToArray();
                    submittedPreview.AddRange(selectedPreview);
                    Require(selectedPreview.Length == int.Parse(Parameter(step, "expectedCount")), "确认前已选数量不匹配");
                    await ClickAsync(Button(Descendants<PreciseQuoteView>(view).First(control => control.IsVisible), "确认报价"));
                    await AwaitCommand(model.QuotePage.Precise.ConfirmSelectedCommand.ExecutionTask);
                    CollectCreatedQuotes();
                    foreach (QuoteConfirmRowViewModel submitted in selectedPreview)
                    {
                        Require(!string.IsNullOrEmpty(submitted.Row.Revision) && !model.QuotePage.Precise.Rows.Contains(submitted), "确认未完成；查看 Toast 及 DPS 日志");
                        JObject stored = await api.GetQuoteAsync(submitted.QuoteId);
                        Require((string)stored["status"] == "ACTIVE", "识别报价未持久化为 ACTIVE");
                    }
                    await Until(() => !model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                        .Any(row => selectedPreview.Any(submitted => submitted.QuoteId == row.QuoteId)),
                        "已确认识别报价从报价确认页移除");
                    break;
                default:
                    await ExecuteRegressionAsync(step);
                    break;
            }
        }

        private async Task NavigateAsync(string key)
        {
            // 页面操作不能隔着模态报价窗口执行，复合动作也遵守同一规则。
            await CloseEditorAsync();
            // Select the actual ListBox item so navigation bindings execute normally.
            object item = model.NavItems.FirstOrDefault(candidate => candidate.Key == key)
                ?? (key == "entry" ? model.NavItems.First() : null);
            Require(item != null, "页面不存在：" + key);
            ListBox navigation = Descendants<ListBox>(view).First(list => list.Items.Contains(item));
            await SelectItemAsync(navigation, item);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            view.UpdateLayout();
        }

        private async Task SelectTabAsync(string key)
        {
            object item = model.QuotePage.SubTabs.SingleOrDefault(tab => tab.Key == key);
            Require(item != null, "集成客户端缺少要求的报价录入页签：" + key);
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(item)), item);
        }

        private async Task SelectMarketTabAsync(string key)
        {
            object item = model.MarketCenterPage.SubTabs.SingleOrDefault(tab => tab.Key == key);
            Require(item != null, "集成客户端缺少要求的行情页签：" + key);
            await SelectItemAsync(Descendants<ListBox>(view).First(list => list.Items.Contains(item)), item);
        }

        private void Track(string id)
        {
            Require(!string.IsNullOrWhiteSpace(id), "没有返回报价 ID");
            quoteId = id;
            ownedQuotes.Add(id);
            File.WriteAllText(Path.Combine(reportDirectory, "owned-quotes.json"), new JArray(ownedQuotes).ToString());
        }

        private void CollectCreatedQuotes()
        {
            foreach (QuoteConfirmRowViewModel row in submittedPreview.Where(row => !string.IsNullOrEmpty(row.Row.Revision))) Track(row.QuoteId);
        }

        private async Task CleanupAsync()
        {
            var failures = new List<string>();
            try
            {
                await CleanupQuotesAsync();
            }
            catch (Exception error)
            {
                failures.Add(error.ToString());
            }
            try
            {
                await CleanupNotificationFixtureAsync();
            }
            catch (Exception error)
            {
                failures.Add(error.ToString());
            }
            try
            {
                await CleanupGroupFixturesAsync();
            }
            catch (Exception error)
            {
                failures.Add(error.ToString());
            }
            Require(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private async Task CleanupQuotesAsync()
        {
            CollectCreatedQuotes();
            await CloseEditorAsync();
            await CleanupLatencyQuotesAsync();
            if (ownedQuotes.Count == 0) return;
            Require(model != null, "无法连接服务执行清理");
            List<string> failures = new List<string>();
            foreach (string id in ownedQuotes)
            {
                try
                {
                    JObject quote = await api.GetQuoteAsync(id);
                    if ((string)quote["status"] == "DELETED") continue;
                    if ((string)quote["status"] == "ACTIVE")
                    {
                        TestApiClient.RequireQuoteResults(await api.OperateAsync("refer", new[] { quote }));
                        quote = await api.GetQuoteAsync(id);
                    }
                    TestApiClient.RequireQuoteResults(await api.OperateAsync("delete", new[] { quote }));
                    Require((string)(await api.GetQuoteAsync(id))["status"] == "DELETED", "清理后仍未删除");
                }
                catch (Exception error) { failures.Add(id + ": " + error.Message); }
            }
            try
            {
                await CleanupGroupFixturesAsync();
            }
            catch (Exception error)
            {
                failures.Add(error.ToString());
            }
            Require(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private async Task CloseEditorAsync()
        {
            if (editDialog != null && editDialog.IsVisible)
            {
                await CloseWindowAsync(editDialog);
            }
            editDialog = null;
        }

        private void ThrowIfCancelled()
        {
            string parentPid = Environment.GetEnvironmentVariable("STC_UI_PARENT_PID");
            if (!cleaning && int.TryParse(parentPid, out int parent))
            {
                try
                {
                    using (Process parentProcess = Process.GetProcessById(parent))
                    {
                        if (parentProcess.HasExited)
                        {
                            throw new OperationCanceledException("测试管理器已退出");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    throw new OperationCanceledException("测试管理器已退出");
                }
            }
            if (!cleaning && noticeAutomationFailure != null)
            {
                throw new InvalidOperationException("自动处理用户使用须知失败。", noticeAutomationFailure);
            }
            if (!cleaning && File.Exists(Path.Combine(reportDirectory, "cancel"))) throw new OperationCanceledException();
        }

        private async Task AwaitCommand(Task task)
        {
            Require(task != null, "UI 命令未执行");
            await Until(() => task.IsCompleted, "UI 命令完成");
            await task;
        }

        private async Task Until(Func<bool> predicate, string description, int seconds = 20)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
            while (!predicate())
            {
                ThrowIfCancelled();
                if (DateTime.UtcNow >= deadline) throw new TimeoutException(description);
                await Task.Delay(100);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static Button Button(DependencyObject root, string caption)
        {
            var target = Descendants<Button>(root).FirstOrDefault(button => button.IsVisible && button.Content as string == caption);
            Require(target != null, "界面缺少可见按钮：" + caption + "，页面：" + root.GetType().Name);
            return target;
        }

        // 每次真实输入前等待，而不是在包含多个输入的业务步骤之间等待。
        // 等待期间显示当前目标红框，保持消息循环、取消和实时调速可用。
        private async Task BeforeUiActionAsync(FrameworkElement target, string action)
        {
            if (cleaning || (stepNumber > 0 && definition.Steps[stepNumber - 1].Action == "filter.latency"))
            {
                return;
            }
            ThrowIfCancelled();
            string step = stepNumber == 0 ? "登录" : definition.Steps[stepNumber - 1].Name;
            string label = (target as ContentControl)?.Content as string;
            if (string.IsNullOrWhiteSpace(label))
            {
                label = string.IsNullOrWhiteSpace(target.Name) ? target.GetType().Name : target.Name;
            }
            string detail = action + "：" + label;
            target.BringIntoView();
            highlight?.Show(target);
            await ShowCaptionAsync("正在执行", step + " · " + detail);
            await new OperationInterval(Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROLE") == "observer"
                ? Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROOT") : reportDirectory).WaitAsync(ThrowIfCancelled, (waiting, milliseconds) =>
                Emit(step, waiting ? "INTERVAL_WAIT" : "INTERVAL_DONE", detail, milliseconds),
                paused => ReportPlaybackPause(paused, detail));
            ThrowIfCancelled();
            Emit(step, "UI_ACTION", detail);
        }

        private bool verificationStepGranted;
        private string PlaybackDirectory => Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROOT") ?? reportDirectory;

        private void ReportPlaybackPause(bool paused, string detail)
        {
            Emit("播放控制", paused ? "PLAYBACK_PAUSED" : "PLAYBACK_RELEASED", detail);
            captions?.Present(shell, definition.Name, stepNumber, definition.Steps.Count,
                paused ? "已暂停" : "正在执行", detail);
        }

        private async Task BeginVerificationAsync(string detail)
        {
            if (highlight != null)
            {
                highlight.VerificationPinned = true;
            }
            await ShowCaptionAsync("正在验证", detail);
            verificationStepGranted = await new OperationInterval(PlaybackDirectory).WaitAsync(ThrowIfCancelled,
                (waiting, milliseconds) => { }, paused => ReportPlaybackPause(paused, detail), gateOnly: true);
        }

        // 先检测真实更新，再留出人工观察时间；结束后才允许操作方进入下一阶段。
        private async Task HoldVerificationAsync(FrameworkElement target, string detail)
        {
            ThrowIfCancelled();
            target?.BringIntoView();
            if (target != null)
            {
                Window.GetWindow(target)?.Activate();
            }
            highlight?.Show(target);
            await ShowCaptionAsync("验证通过，正在展示", detail);
            string directory = Environment.GetEnvironmentVariable("STC_UI_REALTIME_ROOT") ?? reportDirectory;
            try
            {
                await new OperationInterval(directory).WaitAsync(ThrowIfCancelled, (waiting, milliseconds) =>
                    Emit("验证结果展示", waiting ? "INTERVAL_WAIT" : "INTERVAL_DONE", detail, milliseconds),
                    paused => ReportPlaybackPause(paused, detail), singleStepGranted: verificationStepGranted);
            }
            finally
            {
                verificationStepGranted = false;
            }
            ThrowIfCancelled();
        }

        private async Task OpenModalAsync(Button control)
        {
            await BeforeUiActionAsync(control, "打开窗口");
            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => ClickImmediately(control)));
        }

        private async Task CloseWindowAsync(Window window)
        {
            await BeforeUiActionAsync(window, "关闭窗口");
            window.Close();
        }

        private async Task ClickAsync(ButtonBase control)
        {
            await BeforeUiActionAsync(control, "点击");
            ClickImmediately(control);
        }

        private static void ClickImmediately(ButtonBase control)
        {
            Require(control.IsEnabled && control.IsVisible, "按钮不可用：" + control.Content);
            control.Focus();
            highlight?.Show(control);
            control.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, null);
        }

        private void HighlightRow(object row)
        {
            highlight?.Show(FindVerificationRow(row));
        }

        private DataGridRow FindVerificationRow(object row)
        {
            DataGrid grid = Descendants<DataGrid>(view).SingleOrDefault(control => control.IsVisible && control.Items.Contains(row));
            Require(grid != null, "验证目标缺失：未找到包含目标报价的可见表格");
            grid.ScrollIntoView(row);
            grid.UpdateLayout();
            var container = grid.ItemContainerGenerator.ContainerFromItem(row) as DataGridRow;
            Require(container != null, "验证目标缺失：报价行尚未呈现；不回退到整个表格");
            return container;
        }

        // 使用列身份定位单元格，允许水平/垂直滚动，保留行列虚拟化。
        private static DataGridCell FindVerificationCell(DataGrid grid, object row, string header)
        {
            var column = grid.Columns.SingleOrDefault(item => Convert.ToString(item.Header) == header);
            Require(column != null, "验证目标缺失：列 " + header);
            grid.ScrollIntoView(row, column);
            grid.UpdateLayout();
            var container = grid.ItemContainerGenerator.ContainerFromItem(row) as DataGridRow;
            Require(container != null, "验证目标缺失：报价行尚未呈现");
            var cell = Descendants<DataGridCell>(container).SingleOrDefault(item => item.Column == column);
            Require(cell != null && cell.IsVisible, "验证目标缺失：单元格 " + header);
            return cell;
        }

        private async Task VerifyCellAsync<T>(object row, string header, T expected, Func<T> actual)
        {
            var grid = Descendants<DataGrid>(view).SingleOrDefault(control => control.IsVisible && control.Items.Contains(row));
            Require(grid != null, "验证目标缺失：未找到报价表格");
            var cell = FindVerificationCell(grid, row, header);
            string identity = Convert.ToString(row.GetType().GetProperty("Code")?.GetValue(row));
            string detail = identity + " · " + header + " · 预期 " + expected + "，实际 " + actual();
            await VerifyAsync(cell, detail, () =>
            {
                Require(ReferenceEquals(cell.DataContext, row), "验证目标已变化：报价单元格被回收或替换");
                Require(Equals(expected, actual()), identity + " · " + header + " 不匹配：预期 " + expected + "，实际 " + actual());
            });
        }

        private async Task VerifyAsync(FrameworkElement target, string detail, Action assertion)
        {
            try
            {
                if (highlight != null)
                {
                    highlight.VerificationPinned = true;
                }
                target?.BringIntoView();
                // 虚拟化刚生成的单元格需先完成 Loaded，再绘制红框和进入暂停点。
                await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
                highlight?.Show(target);
                await BeginVerificationAsync(detail);
                Require(target == null || (target.IsLoaded && target.IsVisible), "验证目标已隐藏或卸载：" + detail);
                Require(target == null || highlight == null || ReferenceEquals(highlight.Target, target),
                    "验证目标没有可用的红框装饰层：" + detail);
                Emit("验证目标", "VERIFICATION_TARGET", (target == null ? "无界面目标" : target.GetType().Name) + " · " + detail);
                assertion();
                await HoldVerificationAsync(target, detail);
            }
            finally
            {
                if (highlight != null)
                {
                    highlight.VerificationPinned = false;
                }
                verificationStepGranted = false;
            }
        }

        private async Task SelectItemAsync(Selector control, object item)
        {
            await BeforeUiActionAsync(control, "选择");
            control.SelectedItem = item;
            control.UpdateLayout();
            // 折叠下拉框标记整个输入控件；页签和表格标记实际已呈现的条目。
            FrameworkElement target = control is ComboBox ? control
                : control.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement ?? control;
            highlight?.Show(target);
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, index);
                if (child is T target) yield return target;
                foreach (T descendant in Descendants<T>(child)) yield return descendant;
            }
        }

        private void Capture(string name, Window target = null)
        {
            try
            {
                Window window = target ?? editDialog ?? shell;
                if (window == null || window.ActualWidth <= 0) return;
                window.UpdateLayout();
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (FileStream file = File.Create(Path.Combine(reportDirectory, name + ".png"))) encoder.Save(file);
            }
            catch (Exception error) { Emit("截图", "WARNING", error.Message); }
        }
    }
}
