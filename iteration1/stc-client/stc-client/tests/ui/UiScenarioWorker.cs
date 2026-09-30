using STC.SalesTrading.Plugin.Ficc.Protocol;
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
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.View;

namespace SalesTrading.UiTests
{
    // Hosts the production STC application and its real controls, not replacement screens.
    // Business writes go through UI button handlers. APIs are used for assertions and cleanup only.
    internal sealed partial class UiScenarioWorker
    {
        private static string reportDirectory;
        private static CaseDefinition definition;
        private static readonly List<JObject> events = new List<JObject>();
        private readonly HashSet<string> ownedQuotes = new HashSet<string>();
        private readonly List<QuoteConfirmRowViewModel> submittedPreview = new List<QuoteConfirmRowViewModel>();
        private MainView view;
        private MainViewModel model;
        private Window shell;
        private QuoteOperationWindow editor;
        private string quoteId;
        private int stepNumber;
        private bool cleaning;
        private readonly Stopwatch elapsed = Stopwatch.StartNew();

        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length != 2) return 2;
            reportDirectory = Path.GetFullPath(args[1]);
            Directory.CreateDirectory(reportDirectory);
            definition = CaseDefinition.Read(args[0]);
            STC.App application = new STC.App();
            application.InitializeComponent();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            UiScenarioWorker worker = new UiScenarioWorker();
            application.DispatcherUnhandledException += (sender, error) =>
            {
                // Unhandled UI exceptions fail the case; never turn them into a passing result.
                Emit("application", "FAILED", error.Exception.ToString());
            };
            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                int result = await worker.RunAsync();
                application.Shutdown(result);
                Environment.Exit(result);
            }), DispatcherPriority.ApplicationIdle);
            return application.Run();
        }

        private static void Emit(string step, string state, string detail)
        {
            string secret = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
            if (!string.IsNullOrEmpty(secret)) detail = (detail ?? "").Replace(secret, "[REDACTED]");
            JObject entry = new JObject
            {
                ["time"] = DateTime.Now.ToString("O"), ["caseId"] = definition.Id,
                ["step"] = step, ["state"] = state, ["detail"] = detail
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
                Emit("登录", "RUNNING", "启动正式 STC 登录窗口，连接真实服务");
                await LoginAsync();
                Emit("登录", "PASSED", "已登录并加载报价、行情及提醒页面");
                foreach (TestStep step in definition.Steps)
                {
                    stepNumber++;
                    ThrowIfCancelled();
                    Emit(step.Name, "RUNNING", step.Action);
                    Stopwatch duration = Stopwatch.StartNew();
                    await ExecuteAsync(step);
                    await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    Capture(stepNumber.ToString("D2") + "-passed");
                    Emit(step.Name, "PASSED", duration.ElapsedMilliseconds + " ms");
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
            }
            return outcome == "PASSED" ? 0 : 1;
        }

        private async Task LoginAsync()
        {
            string user = Environment.GetEnvironmentVariable("STC_UI_USER");
            string password = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
            Require(!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password), "请填写测试账号和密码");
            STC.Frame.UI.LoginGUI login = null;
            await Until(() => (login = Application.Current.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault()) != null && login.IsLoaded, "登录窗口", 45);
            ((TextBox)login.FindName("tbUsername")).Text = user;
            ((PasswordBox)login.FindName("pbPwd")).Password = password;
            Click((Button)login.FindName("btnLogin"));
            STC.Frame.Window1 frame = null;
            await Until(() => (frame = Application.Current.Windows.OfType<STC.Frame.Window1>().FirstOrDefault()) != null
                && frame.IsVisible && frame.PluginManager != null, "STC 主窗口", 60);
            shell = frame;
            if (!frame.Panel.Items.OfType<TabItem>().Any(item => item.Content is MainView))
            {
                FrameworkElement logo = (FrameworkElement)frame.FindName("btnLogo");
                Click((Button)logo.FindName("btnMenu"));
                DataGrid menu = (DataGrid)logo.FindName("dgMenu");
                await Until(() => menu.Items.Cast<object>().Any(item => Convert.ToString(item.GetType().GetProperty("PluginKey")?.GetValue(item)) == "230"), "销售交易菜单");
                object salesTrading = menu.Items.Cast<object>().Single(item => Convert.ToString(item.GetType().GetProperty("PluginKey")?.GetValue(item)) == "230");
                menu.SelectedItem = salesTrading;
                menu.CurrentItem = salesTrading;
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
                return model?.Backend != null;
            }, "登录完成及销售交易模块加载", 60);
            shell.WindowState = WindowState.Normal;
            shell.Width = 1440;
            shell.Height = 950;
            frame.Panel.SelectedItem = frame.Panel.Items.OfType<TabItem>().Single(item => ReferenceEquals(item.Content, view));
            shell.UpdateLayout();
            await Until(() => view.IsLoaded && view.IsVisible && Descendants<Button>(view).Any(button => (string)(button.Content as string) == "手工报价"), "销售交易界面完成渲染");
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
            switch (step.Action)
            {
                case "manual.open":
                    // ShowDialog runs a nested dispatcher loop; queue the click so the test can
                    // continue driving the dialog while the product handler waits for it to close.
                    _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => Click(Button(view, "手工报价"))));
                    await Until(() => (editor = Application.Current.Windows.OfType<QuoteOperationWindow>().FirstOrDefault()) != null && editor.IsReady, "手工报价初始化");
                    break;
                case "manual.fill":
                    Require(editor != null && editor.IsReady, "请先打开手工报价");
                    foreach (string field in Actions.Fields[step.Action])
                    {
                        FrameworkElement input = Descendants<FrameworkElement>(editor.Editor)
                            .Single(control => System.Windows.Automation.AutomationProperties.GetAutomationId(control) == "QuoteEditor." + field);
                        if (input is TextBox text) text.Text = Parameter(step, field);
                        else if (input is ComboBox choice) choice.SelectedValue = Parameter(step, field);
                    }
                    break;
                case "manual.click":
                    Require(editor != null && editor.IsReady, "报价操作窗口未就绪");
                    long revision = (long?)editor.CurrentQuote?["revision"] ?? 0;
                    Click(Button(editor, Parameter(step, "button")));
                    await Until(() => editor.IsReady && (long?)editor.CurrentQuote?["revision"] > revision,
                        "报价操作成功；检查操作窗口错误及 DPS 日志");
                    Track((string)editor.CurrentQuote["quoteId"]);
                    break;
                case "quote.assert":
                    Require(quoteId != null, "本例尚未创建报价");
                    JObject quote = await model.Backend.Api.GetQuoteAsync(quoteId);
                    Require((string)quote["status"] == Parameter(step, "status"), "服务端状态不匹配：" + quote["status"]);
                    Require((decimal)quote["price"] == decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture), "服务端价格不匹配");
                    File.WriteAllText(Path.Combine(reportDirectory, stepNumber.ToString("D2") + "-quote.json"), quote.ToString());
                    break;
                case "pending.assert":
                    CloseEditor();
                    await NavigateAsync("entry");
                    await Until(() => model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).Any(row => row.RecognitionId == quoteId), "报价确认页接收本例报价");
                    Require(Descendants<DataGrid>(view).Any(grid => grid.Items.OfType<QuoteConfirmRowViewModel>().Any(row => row.RecognitionId == quoteId)), "报价确认表未显示本例报价");
                    break;
                case "management.assert":
                    CloseEditor();
                    await NavigateAsync("mgmt");
                    await Until(() => model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == quoteId), "报价管理接收本例报价");
                    Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>().Any(item => (string)item.GetType().GetProperty("QuoteId")?.GetValue(item) == quoteId)), "报价管理表未显示本例报价");
                    break;
                case "market.assert":
                    CloseEditor();
                    await NavigateAsync("market");
                    object rateTab = model.MarketCenterPage.SubTabs.Single(tab => tab.Key == "rateAll");
                    Descendants<ListBox>(view).First(list => list.Items.Contains(rateTab)).SelectedItem = rateTab;
                    await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    view.UpdateLayout();
                    await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.QuoteId == quoteId), "行情中心接收本例报价");
                    Require(Descendants<DataGrid>(view).Any(grid => grid.IsVisible && grid.Items.Cast<object>().Any(item => (string)item.GetType().GetProperty("QuoteId")?.GetValue(item) == quoteId)), "行情表未显示本例报价");
                    break;
                case "history.assert":
                    JObject history = await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.QuoteHistoryQuery, ProtocolCodec.Decode<QuoteHistoryRequest>(new JObject { ["quoteId"] = quoteId, ["limit"] = 100 }));
                    Require(((JArray)history["records"]).Count >= int.Parse(Parameter(step, "minimumCount")), "持久化变更历史条数不足");
                    File.WriteAllText(Path.Combine(reportDirectory, "history.json"), history.ToString());
                    break;
                case "parse.keyboard":
                case "parse":
                    CloseEditor();
                    await NavigateAsync("entry");
                    SelectTab("quote-precise");
                    await Until(() => Descendants<PreciseQuoteView>(view).Any(control => control.IsVisible), "精确报价页面");
                    PreciseQuoteView precise = Descendants<PreciseQuoteView>(view).First(control => control.IsVisible);
                    TextBox inputBox = (TextBox)precise.FindName("InputBox");
                    if (step.Action == "parse.keyboard")
                    {
                        string[] lines = Parameter(step, "text").Split('\n');
                        Require(lines.Length == 2, "键盘用例要求两行输入");
                        inputBox.Focus();
                        inputBox.Text = lines[0];
                        inputBox.CaretIndex = inputBox.Text.Length;
                        var keyboard = System.Windows.Input.Keyboard.PrimaryDevice;
                        var previewKey = new System.Windows.Input.KeyEventArgs(keyboard, PresentationSource.FromVisual(inputBox),
                            Environment.TickCount, System.Windows.Input.Key.Return)
                            { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent };
                        inputBox.RaiseEvent(previewKey);
                        Require(!previewKey.Handled && !model.QuotePage.Precise.IsParsing, "Enter 被识别命令拦截");
                        inputBox.RaiseEvent(new System.Windows.Input.KeyEventArgs(keyboard, PresentationSource.FromVisual(inputBox),
                            Environment.TickCount, System.Windows.Input.Key.Return)
                            { RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent });
                        Require(inputBox.Text.Contains("\n"), "Enter 没有插入换行");
                        inputBox.SelectedText = lines[1];
                        Require(inputBox.Text.Replace("\r\n", "\n") == Parameter(step, "text"), "键盘输入未保留两行原文");
                    }
                    else
                    {
                        inputBox.Text = Parameter(step, "text");
                    }
                    Click(Button(precise, "识别报价"));
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
                    break;
                case "parse.assert":
                    QuoteConfirmRowViewModel parsed = model.QuotePage.Precise.Rows.Single();
                    Require(parsed.Code == Parameter(step, "securityId") && parsed.Institution == Parameter(step, "institution")
                        && parsed.Trader == Parameter(step, "trader") && decimal.Parse(parsed.Price, CultureInfo.InvariantCulture) == decimal.Parse(Parameter(step, "price"), CultureInfo.InvariantCulture), "识别结果字段不匹配");
                    Require(!string.IsNullOrEmpty(parsed.Row.InstitutionId) && !string.IsNullOrEmpty(parsed.Row.ContactId), "CRM 名称未解析为真实 ID");
                    break;
                case "parse.select":
                    QuoteConfirmRowViewModel selected = model.QuotePage.Precise.Rows[int.Parse(Parameter(step, "row")) - 1];
                    DataGrid preview = Descendants<DataGrid>(view).First(grid => grid.IsVisible && grid.Items.Contains(selected));
                    preview.ScrollIntoView(selected);
                    preview.UpdateLayout();
                    CheckBox checkbox = Descendants<CheckBox>(preview).First(control => ReferenceEquals(control.DataContext, selected));
                    bool expected = bool.Parse(Parameter(step, "selected"));
                    if (checkbox.IsChecked != expected) Click(checkbox);
                    Require(selected.IsSelected == expected && model.QuotePage.Precise.SelectedCount == int.Parse(Parameter(step, "expectedCount")), "勾选状态与已选数量不一致");
                    break;
                case "parse.confirm":
                    var selectedPreview = model.QuotePage.Precise.Rows.Where(row => row.IsSelected).ToArray();
                    submittedPreview.AddRange(selectedPreview);
                    Require(selectedPreview.Length == int.Parse(Parameter(step, "expectedCount")), "确认前已选数量不匹配");
                    Click(Button(Descendants<PreciseQuoteView>(view).First(control => control.IsVisible), "确认报价"));
                    await AwaitCommand(model.QuotePage.Precise.ConfirmSelectedCommand.ExecutionTask);
                    CollectCreatedQuotes();
                    foreach (QuoteConfirmRowViewModel submitted in selectedPreview)
                    {
                        Require(submitted.Row.Revision > 0 && !model.QuotePage.Precise.Rows.Contains(submitted), "确认未完成；查看 Toast 及 DPS 日志");
                        JObject stored = await model.Backend.Api.GetQuoteAsync(submitted.RecognitionId);
                        Require((string)stored["status"] == "ACTIVE", "识别报价未持久化为 ACTIVE");
                    }
                    break;
                default:
                    await ExecuteRegressionAsync(step);
                    break;
            }
        }

        private async Task NavigateAsync(string key)
        {
            // Select the actual ListBox item so navigation bindings execute normally.
            object item = model.NavItems.FirstOrDefault(candidate => candidate.Key == key)
                ?? (key == "entry" ? model.NavItems.First() : null);
            Require(item != null, "页面不存在：" + key);
            ListBox navigation = Descendants<ListBox>(view).First(list => list.Items.Contains(item));
            navigation.SelectedItem = item;
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            view.UpdateLayout();
        }

        private void SelectTab(string key)
        {
            object item = model.QuotePage.SubTabs.Single(tab => tab.Key == key);
            Descendants<ListBox>(view).First(list => list.Items.Contains(item)).SelectedItem = item;
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
            if (editor?.CurrentQuote != null) Track((string)editor.CurrentQuote["quoteId"]);
            foreach (QuoteConfirmRowViewModel row in submittedPreview.Where(row => row.Row.Revision > 0)) Track(row.RecognitionId);
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
            Require(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private async Task CleanupQuotesAsync()
        {
            CollectCreatedQuotes();
            CloseEditor();
            if (ownedQuotes.Count == 0) return;
            Require(model?.Backend != null, "无法连接服务执行清理");
            List<string> failures = new List<string>();
            foreach (string id in ownedQuotes)
            {
                try
                {
                    JObject quote = await model.Backend.Api.GetQuoteAsync(id);
                    if ((string)quote["status"] == "DELETED") continue;
                    if ((string)quote["status"] == "ACTIVE")
                    {
                        FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("refer", new[] { quote }));
                        quote = await model.Backend.Api.GetQuoteAsync(id);
                    }
                    FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("delete", new[] { quote }));
                    Require((string)(await model.Backend.Api.GetQuoteAsync(id))["status"] == "DELETED", "清理后仍未删除");
                }
                catch (Exception error) { failures.Add(id + ": " + error.Message); }
            }
            Require(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private void CloseEditor()
        {
            editor?.Close();
            editor = null;
        }

        private void ThrowIfCancelled()
        {
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
            return Descendants<Button>(root).First(button => button.IsVisible && (string)(button.Content as string) == caption);
        }

        private static void Click(ButtonBase control)
        {
            Require(control.IsEnabled && control.IsVisible, "按钮不可用：" + control.Content);
            control.Focus();
            control.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, null);
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

        private void Capture(string name)
        {
            try
            {
                Window window = editor ?? shell;
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
