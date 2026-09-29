using STC.SalesTrading.Plugin.Ficc.Protocol;
using System;
using System.IO;
using System.Linq;
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

internal static class MergedSalesTradingCheck
{
    private static bool submitted;
    private static bool running;
    private static readonly StreamWriter report = new StreamWriter("merged-salestrading-result.txt", false) { AutoFlush = true };

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
        timer.Tick += async (sender, args) =>
        {
            var login = app.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault();
            if (login != null && login.IsLoaded && !submitted)
            {
                submitted = true;
                ((TextBox)login.FindName("tbUsername")).Text = "trader1";
                ((PasswordBox)login.FindName("pbPwd")).Password = "trader1";
                ((Button)login.FindName("btnLogin")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            var shell = app.Windows.OfType<STC.Frame.Window1>().FirstOrDefault();
            var view = shell?.Panel.Items.OfType<TabItem>().Select(item => item.Content).OfType<MainView>().FirstOrDefault();
            if (view?.DataContext is MainViewModel model && !running)
            {
                running = true;
                timer.Stop();
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
        QuoteOperationWindow editor = null;
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
            editor = new QuoteOperationWindow(model.Backend.Api) { Owner = shell };
            editor.Show();
            await Until(() => editor.IsReady, "CRM-aware editor initialized");
            editor.Editor.Set(new JObject
            {
                ["securityId"] = "250001.IB", ["side"] = "1", ["quoteType"] = "1", ["yieldType"] = "1",
                ["price"] = "2.5000", ["quantity"] = "1", ["settlType"] = "2",
                ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["remark"] = "MERGED-UI-FUNCTIONAL"
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
            JObject history = await model.Backend.Api.RequestProjectionAsync(SalesTradingApi.QuoteHistoryQuery, ProtocolCodec.Decode<QuoteHistoryRequest>(new JObject { ["quoteId"] = quoteId, ["limit"] = 20 }));
            Check(((JArray)history["records"]).Count >= 4, "continuous PostgreSQL lifecycle history");
            editor.Close();
            editor = null;
            shell.WindowState = WindowState.Normal;
            shell.Width = 1440;
            shell.Height = 1000;
            model.SelectedNavItem = model.NavItems.First(item => item.Key == "mgmt");
            shell.UpdateLayout();
            await Task.Delay(200);
            Capture(view, "merged-management.png");
            model.SelectedNavItem = model.NavItems.First(item => item.Key == "market");
            shell.UpdateLayout();
            await Task.Delay(200);
            Capture(view, "merged-market.png");
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
        }
        report.WriteLine(result == 0 ? "RESULT PASS" : "RESULT FAIL");
        return result;
    }

    private static async Task Until(Func<bool> condition, string name)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(8);
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
