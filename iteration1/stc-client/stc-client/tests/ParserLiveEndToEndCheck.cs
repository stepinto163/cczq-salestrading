using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.View;

internal static class ParserLiveEndToEndCheck
{
    private static bool submitted;
    private static bool running;
    private static bool openedMenu;
    private static readonly StreamWriter report = new StreamWriter("parser-e2e-result.txt", false) { AutoFlush = true };

    [STAThread]
    public static int Main()
    {
        STC.App app = new STC.App();
        app.InitializeComponent();
        app.DispatcherUnhandledException += (sender, args) =>
        {
            report.WriteLine("FAIL " + args.Exception);
            Environment.Exit(1);
        };
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        int ticks = 0;
        timer.Tick += async (sender, args) =>
        {
            if (++ticks % 25 == 0)
            {
                report.WriteLine("WAIT windows: " + string.Join(",", app.Windows.Cast<Window>().Select(window => window.GetType().FullName)));
                foreach (var warning in app.Windows.OfType<STC.Frame.UI.SysWarnGUI>())
                    report.WriteLine("WAIT warning: " + warning.tbMsg.Text);
            }
            var login = app.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault();
            if (login != null && login.IsLoaded && !submitted)
            {
                submitted = true;
                ((TextBox)login.FindName("tbUsername")).Text = Environment.GetEnvironmentVariable("PARSER_TEST_USER");
                ((PasswordBox)login.FindName("pbPwd")).Password = Environment.GetEnvironmentVariable("PARSER_TEST_PASSWORD");
                ((Button)login.FindName("btnLogin")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            var shell = app.Windows.OfType<STC.Frame.Window1>().FirstOrDefault();
            var view = shell?.Panel.Items.OfType<TabItem>().Select(item => item.Content).OfType<MainView>().FirstOrDefault();
            if (ticks % 25 == 0 && shell != null)
                report.WriteLine("WAIT visible=" + shell.IsVisible + " tabs: " + string.Join(",", shell.Panel.Items.OfType<TabItem>().Select(item => item.Content?.GetType().FullName)));
            if (view == null && shell != null && shell.IsVisible && shell.IsLoaded && shell.PluginManager != null)
            {
                FrameworkElement logo = (FrameworkElement)shell.FindName("btnLogo");
                if (!openedMenu)
                {
                    openedMenu = true;
                    ((Button)logo.FindName("btnMenu")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                DataGrid menu = (DataGrid)logo.FindName("dgMenu");
                object sales = menu.Items.Cast<object>().FirstOrDefault(item =>
                    Convert.ToString(item.GetType().GetProperty("PluginKey")?.GetValue(item)) == "230");
                if (sales != null)
                {
                    menu.SelectedItem = sales;
                    menu.CurrentItem = sales;
                    menu.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice,
                        0, System.Windows.Input.MouseButton.Left) { RoutedEvent = System.Windows.Input.Mouse.MouseUpEvent });
                }
            }
            if (shell != null && shell.IsVisible && view?.DataContext is MainViewModel model && !running)
            {
                running = true;
                timer.Stop();
                shell.WindowState = WindowState.Normal;
                shell.Width = 1440;
                shell.Height = 1000;
                shell.Panel.SelectedItem = shell.Panel.Items.OfType<TabItem>().Single(item => ReferenceEquals(item.Content, view));
                shell.UpdateLayout();
                int result = await Exercise(shell, view, model);
                app.Shutdown(result);
            }
        };
        timer.Start();
        int exit = app.Run();
        report.Dispose();
        Environment.Exit(exit);
        return exit;
    }

    private static async Task<int> Exercise(Window shell, MainView view, MainViewModel model)
    {
        string quoteId = null;
        var parsedRows = new List<STC.SalesTrading.Plugin.Abstractions.Models.QuoteConfirmRow>();
        QuoteOperationWindow editor = null;
        JObject originalPreference = null;
        int result = 0;
        try
        {
            await Until(() => !model.QuoteMgmtPage.Today.IsLoading && !model.MarketCenterPage.RateAll.IsLoading
                && !model.MarketCenterPage.Alert.IsLoading && !model.QuoteMgmtPage.History.IsLoading, "product pages initialized");
            Check(model.QuoteMgmtPage.History.ErrorMessage == null, "history query: " + model.QuoteMgmtPage.History.ErrorMessage);
            Check(model.Backend != null, "product shell uses DPS services");
            Check(model.QuoteMgmtPage.Today.ErrorMessage == null, "management query: " + model.QuoteMgmtPage.Today.ErrorMessage);
            Check(model.MarketCenterPage.RateAll.ErrorMessage == null, "price query: " + model.MarketCenterPage.RateAll.ErrorMessage);
            Check(model.MarketCenterPage.Alert.ErrorMessage == null, "notification query: " + model.MarketCenterPage.Alert.ErrorMessage);
            model.SelectedNavItem = model.NavItems.First(item => item.Key == "quote");
            model.QuotePage.SelectedSubTab = model.QuotePage.SubTabs.First(item => item.Key == "quote-precise");
            var precise = model.QuotePage.Precise;
            precise.InputText = "长城证券 田老哥\n240210 2.5/2.4 1000;240215 2.3 2000 bid";
            await precise.IdentifyCommand.ExecuteAsync(null);
            Check(precise.Rows.Count == 3, "real ParserSvr returns both sides and both semicolon-separated bonds: " + precise.StatusMessage);
            Check(precise.Rows[0].Row.Price == 2.5m && precise.Rows[1].Row.Price == 2.4m,
                "native price concatenation defect is corrected in Java reply");
            Check(precise.Rows[1].Row.Volume == 1000m && precise.Rows[2].Row.Volume == 2000m,
                "quantities remain independent from prices");
            Check(precise.Rows.All(row => !string.IsNullOrWhiteSpace(row.Row.InstitutionId)
                && !string.IsNullOrWhiteSpace(row.Row.ContactId)), "live CRMSvr resolves institution and trader for every parsed row");
            parsedRows.AddRange(precise.Rows.Select(row => row.Row));
            shell.UpdateLayout();
            Capture(view, "parser-e2e-recognition.png");
            foreach (var row in precise.Rows) row.IsSelected = true;
            await precise.ConfirmSelectedCommand.ExecuteAsync(null);
            Check(precise.Rows.Count == 0 && parsedRows.All(row => row.Revision > 0),
                "precise page confirmation submits all selected parsed quotes");
            foreach (var row in parsedRows)
            {
                JObject saved = await model.Backend.Api.GetQuoteAsync(row.RecognitionId);
                Check((string)saved["status"] == "ACTIVE" && (decimal)saved["price"] == row.Price,
                    "recognized quote persisted ACTIVE: " + row.Code + " " + row.Direction);
            }
            await Until(() => parsedRows.All(parsed => model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == parsed.RecognitionId)),
                "all parsed quotes reach management through real-time push");
            // Best-price views may show a better existing quote; use a non-competitive price expectation.
            await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.Code == "240210.IB"),
                "market receives parsed security");
            precise.InputText = "bid\n240210 2.55 三千万 T+0\n240215 2.45 2000";
            await precise.IdentifyCommand.ExecuteAsync(null);
            Check(precise.Rows.Count == 2 && precise.Rows[0].Row.Volume == 3000m,
                "multiline heading and Chinese quantity through real client command");
            Check(precise.Rows.All(row => row.Row.Direction == STC.SalesTrading.Plugin.Abstractions.Models.QuoteDirection.Bid),
                "heading direction applies to both rows");
            foreach (var row in precise.Rows) row.IsSelected = true;
            precise.DeleteSelectedCommand.Execute(null);
            Check(precise.Rows.Count == 0, "unsubmitted preview rows can be removed");
            originalPreference = (JObject)(await model.Backend.Api.RequestAsync("ficc.notification.preference.get", new JObject()))["preference"];
            await model.Backend.Api.RequestAsync("ficc.notification.preference.save", new JObject
            {
                ["expectedRevision"] = originalPreference["revision"].DeepClone(),
                ["watchlistEnabled"] = false,
                ["filterGroupEnabled"] = false,
                ["counterQuoteEnabled"] = false,
                ["myBestEnabled"] = true
            });
            editor = new QuoteOperationWindow(model.Backend.Api) { Owner = shell };
            editor.Show();
            await Until(() => editor.IsReady, "CRM-aware editor initialized");
            editor.Editor.Set(new JObject
            {
                ["securityId"] = "240210.IB", ["side"] = "1", ["quoteType"] = "1", ["yieldType"] = "1",
                ["price"] = "2.5000", ["quantity"] = "1", ["settlType"] = "2",
                ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["remark"] = "LOCAL-STC-E2E"
            });
            await editor.OperateAsync("create");
            quoteId = (string)editor.CurrentQuote["quoteId"];
            Check((string)editor.CurrentQuote["status"] == "PENDING_CONFIRM", "create persists pending quote");
            await Until(() => model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows).Any(row => row.RecognitionId == quoteId),
                "colleague confirmation page receives real quote push");
            var pendingRow = model.QuotePage.Confirm.Batches.SelectMany(batch => batch.Rows)
                .Single(row => row.RecognitionId == quoteId);
            pendingRow.IsSelected = true;
            pendingRow.Price = "2.5100";
            await model.QuotePage.Confirm.ConfirmSelectedCommand.ExecuteAsync(null);
            JObject confirmed = await model.Backend.Api.GetQuoteAsync(quoteId);
            Check((string)confirmed["status"] == "ACTIVE" && (decimal)confirmed["price"] == 2.51m,
                "product confirmation submits edited fields and revision");
            await Until(() => model.MarketCenterPage.RateAll.Rows.Any(row => row.Code == "240210.IB" && row.QuoteId == quoteId),
                "market price view receives confirmed quote through real-time push");
            await Until(() => model.MarketCenterPage.Alert.Rows.Any(row => row.Code == "240210.IB"),
                "my-best notification reaches the native reminder view");
            editor.Close();
            editor = new QuoteOperationWindow(model.Backend.Api, quoteId) { Owner = shell };
            editor.Show();
            await Until(() => editor.IsReady, "edit reloads current revision");
            JObject changedQuote = (JObject)editor.CurrentQuote.DeepClone();
            changedQuote["price"] = "2.5200";
            editor.Editor.Set(changedQuote);
            await editor.OperateAsync("update");
            Check((decimal)editor.CurrentQuote["price"] == 2.52m, "editor update persists");
            await Until(() => model.QuoteMgmtPage.Today.Rows.Any(row => row.QuoteId == quoteId), "management receives confirmed quote");
            var today = model.QuoteMgmtPage.Today;
            await Until(() => today.Rows.Any(row => row.QuoteId == quoteId
                && row.Revision == (long)editor.CurrentQuote["revision"]), "management receives revised quote");
            today.SelectedRow.Clear();
            today.SelectedRow.Add(today.Rows.First(row => row.QuoteId == quoteId));
            await today.ReferCommand.ExecuteAsync(null);
            Check((string)(await model.Backend.Api.GetQuoteAsync(quoteId))["status"] == "REFERRED", "product Refer command writes backend");
            today.SelectedRow.Clear();
            today.SelectedRow.Add(today.Rows.First(row => row.QuoteId == quoteId));
            await today.UnreferCommand.ExecuteAsync(null);
            Check((string)(await model.Backend.Api.GetQuoteAsync(quoteId))["status"] == "ACTIVE", "product Unrefer preserves quote ID");
            JObject history = await model.Backend.Api.RequestAsync("ficc.quote.history.query", new JObject { ["quoteId"] = quoteId, ["limit"] = 20 });
            Check(((JArray)history["records"]).Count >= 4, "continuous PostgreSQL lifecycle history");
            editor.Close();
            editor = null;
            shell.WindowState = WindowState.Normal;
            shell.Width = 1440;
            shell.Height = 1000;
            model.SelectedNavItem = model.NavItems.First(item => item.Key == "mgmt");
            shell.UpdateLayout();
            await Task.Delay(200);
            Capture(view, "parser-e2e-management.png");
            model.SelectedNavItem = model.NavItems.First(item => item.Key == "market");
            shell.UpdateLayout();
            await Task.Delay(200);
            Capture(view, "parser-e2e-market.png");
            report.WriteLine("PASS colleague product XAML rendered");
        }
        catch (Exception error)
        {
            report.WriteLine("FAIL " + error);
            result = 1;
        }
        finally
        {
            editor?.Close();
            foreach (var parsed in parsedRows.Where(row => row.Revision > 0))
            {
                try
                {
                    JObject saved = await model.Backend.Api.GetQuoteAsync(parsed.RecognitionId);
                    if ((string)saved["status"] == "ACTIVE")
                    {
                        FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("refer", new[] { saved }));
                        saved = await model.Backend.Api.GetQuoteAsync(parsed.RecognitionId);
                    }
                    FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("delete", new[] { saved }));
                    report.WriteLine("CLEANED parser quote " + parsed.RecognitionId);
                }
                catch (Exception error)
                {
                    report.WriteLine("CLEANUP_FAILED parser quote " + error);
                    result = 1;
                }
            }
            if (quoteId != null)
            {
                try
                {
                    JObject quote = await model.Backend.Api.GetQuoteAsync(quoteId);
                    if ((string)quote["status"] == "ACTIVE")
                    {
                        FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("refer", new[] { quote }));
                        quote = await model.Backend.Api.GetQuoteAsync(quoteId);
                    }
                    FiccApiClient.RequireQuoteResults(await model.Backend.Api.OperateAsync("delete", new[] { quote }));
                    report.WriteLine("CLEANED " + quoteId);
                }
                catch (Exception error)
                {
                    report.WriteLine("CLEANUP_FAILED " + error);
                    result = 1;
                }
            }
            if (originalPreference != null)
            {
                try
                {
                    JObject currentPreference = (JObject)(await model.Backend.Api.RequestAsync("ficc.notification.preference.get", new JObject()))["preference"];
                    JObject restore = new JObject { ["expectedRevision"] = currentPreference["revision"].DeepClone() };
                    foreach (string key in new[] { "watchlistEnabled", "filterGroupEnabled", "counterQuoteEnabled", "myBestEnabled" })
                    {
                        restore[key] = originalPreference[key].DeepClone();
                    }
                    await model.Backend.Api.RequestAsync("ficc.notification.preference.save", restore);
                    report.WriteLine("CLEANED notification preferences restored");
                }
                catch (Exception error)
                {
                    report.WriteLine("CLEANUP_FAILED " + error);
                    result = 1;
                }
            }
        }
        report.WriteLine(result == 0 ? "RESULT PASS" : "RESULT FAIL");
        return result;
    }

    private static async Task Until(Func<bool> condition, string name)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50);
        }
        Check(condition(), name);
    }

    private static void Check(bool success, string name)
    {
        if (!success)
        {
            throw new InvalidOperationException(name);
        }
        report.WriteLine("PASS " + name);
    }

    private static void Capture(FrameworkElement view, string path)
    {
        DrawingVisual drawing = new DrawingVisual();
        using (DrawingContext context = drawing.RenderOpen())
        {
            context.DrawRectangle(new VisualBrush(view), null, new Rect(0, 0, view.ActualWidth, view.ActualHeight));
        }
        RenderTargetBitmap image = new RenderTargetBitmap((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        image.Render(drawing);
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using (FileStream file = File.Create(path))
        {
            encoder.Save(file);
        }
    }
}
