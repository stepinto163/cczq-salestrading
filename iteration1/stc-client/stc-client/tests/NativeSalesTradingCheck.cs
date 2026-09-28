using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.View;

internal static class NativeSalesTradingCheck
{
    private static readonly List<string> created = new List<string>();
    private static bool submitted;
    private static bool testing;
    private static int ticks;
    private static StreamWriter report;

    [STAThread]
    public static int Main()
    {
        report = new StreamWriter("native-salestrading-result.txt", false) { AutoFlush = true };
        STC.App app = new STC.App();
        app.InitializeComponent();
        app.DispatcherUnhandledException += (sender, args) =>
        {
            report.WriteLine("FAIL " + args.Exception);
            Environment.Exit(1);
        };
        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        timer.Tick += async (sender, args) =>
        {
            ticks++;
            var login = app.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault();
            if (login != null && login.IsLoaded && !submitted)
            {
                submitted = true;
                ((TextBox)login.FindName("tbUsername")).Text = "trader1";
                ((PasswordBox)login.FindName("pbPwd")).Password = "trader1";
                ((Button)login.FindName("btnLogin")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            var shell = app.Windows.OfType<STC.Frame.Window1>().FirstOrDefault();
            var view = shell?.Panel.Items.OfType<TabItem>().Select(item => item.Content).OfType<SalesTradingView>().FirstOrDefault();
            if (view != null && view.IsConnected && !testing)
            {
                testing = true;
                timer.Stop();
                int exitCode = await Exercise(view);
                app.Shutdown(exitCode);
            }
            else if (ticks > 80)
            {
                report.WriteLine("FAIL native sales-trading startup timeout.");
                timer.Stop();
                app.Shutdown(1);
            }
        };
        timer.Start();
        int result = app.Run();
        report.Dispose();
        Environment.Exit(result);
        return result;
    }

    private static async Task<int> Exercise(SalesTradingView view)
    {
        int exitCode = 0;
        try
        {
            Check(view.Quotes.Topic != view.Prices.Topic && view.Prices.Topic != view.Notifications.Topic, "separate domain subscriptions");
            view.ShowSection("行情中心");
            view.UpdateLayout();
            await Until(() => !view.Busy, "market navigation is ready");
            ProductFilterPanel filterPanel = Descendants(view).OfType<ProductFilterPanel>().Single();
            Check(!Descendants(filterPanel).OfType<TextBlock>().Any(control => control.Text == "报价数量" || control.Text == "债券代码"),
                "product sidebar does not expose raw API-field forms");
            CheckBox ownership = Descendants(filterPanel).OfType<CheckBox>().First(control => AutomationProperties.GetAutomationId(control) == "filter.ownershipType.YQ");
            ownership.IsChecked = true;
            TextBox lower = Descendants(filterPanel).OfType<TextBox>().First(control => AutomationProperties.GetAutomationId(control) == "filter.bidPrice.min");
            TextBox upper = Descendants(filterPanel).OfType<TextBox>().First(control => AutomationProperties.GetAutomationId(control) == "filter.bidPrice.max");
            lower.Text = "1.5";
            upper.Text = "3.5";
            JArray conditions = (JArray)filterPanel.Read()["conditions"];
            Check(conditions.OfType<JObject>().Any(condition => (string)condition["field"] == "ownershipType" && (string)condition["value"][0] == "YQ"),
                "Chinese ownership selection sends STC dictionary code");
            Check(conditions.OfType<JObject>().Any(condition => (string)condition["field"] == "bidPrice" && (string)condition["operator"] == "BETWEEN"),
                "price range produces one BETWEEN condition");
            CheckBox shortTenor = Descendants(filterPanel).OfType<CheckBox>().First(control => (string)control.Content == "<3M");
            shortTenor.IsChecked = true;
            Check(((JArray)filterPanel.Read()["conditions"]).OfType<JObject>().Any(condition =>
                (string)condition["field"] == "remainingYears" && (string)condition["operator"] == "LT"), "tenor preset uses strict LT boundary");
            shortTenor.IsChecked = false;
            CheckBox cleanPrice = Descendants(filterPanel).OfType<CheckBox>().First(control => AutomationProperties.GetAutomationId(control) == "filter.quoteType.2");
            TextBox spread = Descendants(filterPanel).OfType<TextBox>().First(control => AutomationProperties.GetAutomationId(control) == "filter.bidValuationSpread.min");
            cleanPrice.IsChecked = true;
            spread.Text = "0.1";
            Check(((JArray)filterPanel.Read()["conditions"]).OfType<JObject>().Any(condition =>
                (string)condition["field"] == "bidCleanPriceSpread"), "clean-price selection uses clean-price spread field");
            cleanPrice.IsChecked = false;
            spread.Clear();
            await ClickVisible(view, "筛选");
            Check(view.Prices.IsReady, "native sidebar filter accepted by real QuoteSvr");
            await ClickVisible(view, "清除过滤");
            await ClickVisible(view, "信用全部");
            Check(view.Prices.PriceMode == "FULL" && view.Prices.IsReady, "credit full-price mode connects through price view");
            await ClickVisible(view, "全部债券");
            Check(view.Prices.PriceMode == "BEST" && view.Prices.IsReady, "restore all-bond best-price view");
            JObject book = view.Prices.Rows.FirstOrDefault(row => (string)row["securityId"] == "250001.IB");
            JArray entries = book?["marketData"]?["NoMDEntries"] as JArray ?? new JArray();
            decimal bestBid = entries.OfType<JObject>().Where(row => (string)row["MDEntryType"] == "0")
                .Select(row => decimal.Parse((string)row["MDEntryPx"], CultureInfo.InvariantCulture)).DefaultIfEmpty(2.5m).Min();
            string testPrice = (bestBid - 0.0100m).ToString("F4", CultureInfo.InvariantCulture);
            string updatePrice = (bestBid - 0.0200m).ToString("F4", CultureInfo.InvariantCulture);
            JObject draft = new JObject
            {
                ["securityId"] = "250001.IB", ["side"] = "1", ["quoteType"] = "1", ["yieldType"] = "1",
                ["price"] = testPrice, ["quantity"] = "1000", ["settlType"] = "2", ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["remark"] = "NATIVE-FUNCTIONAL-CHECK"
            };
            view.Editor.Set(draft);
            await Click(view, "保存待确认");
            string firstId = (string)view.EditingQuote["quoteId"];
            created.Add(firstId);
            Check((string)view.EditingQuote["status"] == "PENDING_CONFIRM", "create remains pending confirmation");
            await Until(() => view.PendingQuotes.Rows.Any(row => (string)row["quoteId"] == firstId), "pending view receives create push");
            await Click(view, "确认报价");
            Check((string)view.EditingQuote["status"] == "ACTIVE", "confirm activates quote");
            await Until(() => !view.PendingQuotes.Rows.Any(row => (string)row["quoteId"] == firstId), "confirm removes row from pending view");
            await Until(() => view.Prices.Rows.Any(book => book["quoteRefs"]?.ToString().Contains(firstId) == true), "price stream contains confirmed quote");

            await Click(view, "新报价");
            view.Editor.Set(draft);
            await Click(view, "保存待确认");
            string secondId = (string)view.EditingQuote["quoteId"];
            created.Add(secondId);
            await Until(() => (string)view.PendingQuotes.Rows.FirstOrDefault()?["quoteId"] == secondId, "new pending quote sorts to top in confirmation list");
            Check(!view.Quotes.Rows.Any(row => (string)row["quoteId"] == secondId), "management excludes pending quotes");
            await Click(view, "确认报价");
            await Until(() => (string)view.Quotes.Rows.FirstOrDefault()?["quoteId"] == secondId, "confirmed quote appears at top of management");
            view.Quotes.BeginBrowsing();
            await view.LoadQuoteAsync(firstId);
            JObject edited = (JObject)view.EditingQuote.DeepClone();
            edited["price"] = updatePrice;
            view.Editor.Set(edited);
            await Click(view, "保存修改");
            Check((string)view.EditingQuote["price"] == updatePrice, "edit uses displayed revision");
            await Until(() => view.Quotes.PendingUpdateCount > 0, "browsing collects update count");
            Check((string)view.Quotes.Rows.First()["quoteId"] == secondId, "browsing preserves row order after update");
            await view.Quotes.ResumeLatestAsync();
            await Until(() => (string)view.Quotes.Rows.FirstOrDefault()?["quoteId"] == firstId, "resume latest restores update-time order");
            await Click(view, "撤销 Refer");
            Check((string)view.EditingQuote["status"] == "REFERRED", "refer changes state");
            await Until(() => !view.Prices.Rows.Any(book => book["quoteRefs"]?.ToString().Contains(firstId) == true), "refer removes price entry");
            await Click(view, "恢复 Unrefer");
            Check((string)view.EditingQuote["status"] == "ACTIVE" && (string)view.EditingQuote["quoteId"] == firstId, "unrefer keeps quote ID");
            JObject history = await view.Api.RequestAsync("ficc.quote.history.query", new JObject { ["quoteId"] = firstId, ["limit"] = 100 });
            Check(((JArray)history["records"]).Count >= 5, "continuous lifecycle audit history");
            JObject search = await view.Api.RequestAsync("ficc.quote.history.search", new JObject
            {
                ["fromDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["toDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                ["limit"] = 30, ["filter"] = new JObject { ["conditions"] = new JArray(new JObject
                { ["field"] = "securityId", ["operator"] = "EQ", ["value"] = "250001.IB" }) }
            });
            Check(search["rows"] is JArray, "historical search returns rows collection");
            await Click(view, "撤销 Refer");
            await Click(view, "删除报价");
            Check((string)view.EditingQuote["status"] == "DELETED", "delete through native button");
            string batchMarker = "NATIVE-BATCH-" + Guid.NewGuid().ToString("N");
            draft["remark"] = batchMarker;
            await Click(view, "新报价");
            view.Editor.Set(draft);
            await Click(view, "加入批量草稿");
            await Click(view, "加入批量草稿");
            await Click(view, "保存批量草稿");
            await Until(() => view.PendingQuotes.Rows.Count(row => (string)row["remark"] == batchMarker) == 2,
                "batch creates two pending quotes through native buttons and shared push topic");
            created.AddRange(view.PendingQuotes.Rows.Where(row => (string)row["remark"] == batchMarker).Select(row => (string)row["quoteId"]));
            Check(view.Quotes.IsReady && view.Prices.IsReady && view.Notifications.IsReady, "streams remain continuous");
        }
        catch (Exception error)
        {
            report.WriteLine("FAIL " + error);
            exitCode = 1;
        }
        finally
        {
            foreach (string id in created)
            {
                try
                {
                    JObject quote = await view.Api.GetQuoteAsync(id);
                    if ((string)quote["status"] != "DELETED")
                    {
                        if ((string)quote["status"] == "ACTIVE")
                        {
                            var referred = await view.Api.OperateAsync("refer", new[] { quote });
                            STC.SalesTrading.Plugin.Ficc.FiccApiClient.RequireQuoteResults(referred);
                            quote = await view.Api.GetQuoteAsync(id);
                        }
                        var response = await view.Api.OperateAsync("delete", new[] { quote });
                        STC.SalesTrading.Plugin.Ficc.FiccApiClient.RequireQuoteResults(response);
                    }
                    report.WriteLine("CLEANED " + id);
                }
                catch (Exception error)
                {
                    report.WriteLine("CLEANUP_FAILED " + id + " " + error.Message);
                    exitCode = 1;
                }
            }
            view.Dispose();
        }
        report.WriteLine("RESULT " + (exitCode == 0 ? "PASS" : "FAIL"));
        return exitCode;
    }

    private static async Task Click(SalesTradingView view, string title)
    {
        bool precise = new[] { "新报价", "保存待确认", "加入批量草稿", "保存批量草稿" }.Contains(title);
        view.ShowEntryPage(!precise);
        view.UpdateLayout();
        await ClickVisible(view, title);
    }

    private static async Task ClickVisible(SalesTradingView view, string title)
    {
        await Until(() => !view.Busy, "previous UI request finished");
        Button button = Descendants(view).OfType<Button>().First(item => Equals(item.Content, title));
        if (!button.IsEnabled)
        {
            throw new Exception("Button disabled: " + title);
        }
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await Until(() => !view.Busy, "button completed: " + title);
        if (view.LastError != null)
        {
            throw new Exception(view.LastError);
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static async Task Until(Func<bool> predicate, string name)
    {
        for (int attempt = 0; attempt < 60 && !predicate(); attempt++)
        {
            await Task.Delay(50);
        }
        Check(predicate(), name);
    }

    private static void Check(bool success, string name)
    {
        if (!success)
        {
            throw new Exception(name);
        }
        report.WriteLine("PASS " + name);
    }
}
