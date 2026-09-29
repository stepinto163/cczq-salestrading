using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using netapi.Api;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.View;
using STC.SalesTrading.Plugin.Modules.QuoteMgmt.ViewModel;
using STC.SalesTrading.Plugin.Shared.Components;

internal static class SidebarFilterChecks
{
    private static int passed;
    private static readonly JArray filters = new JArray();

    [STAThread]
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        int result = 1;
        application.Dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await Run(args.Contains("--smoke"));
                if (!args.Contains("--smoke")) File.WriteAllText("sidebar-filters.json", filters.ToString());
                Console.WriteLine("RESULT passed=" + passed);
                result = 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); }
            finally { application.Shutdown(); }
        }));
        application.Run();
        return result;
    }

    private static async Task Run(bool smokeOnly)
    {
        var api = new GWApi();
        api.NewApi();
        api.SetGzip(true);
        try
        {
            string user = Environment.GetEnvironmentVariable("FICC_TEST_USER");
            string response = await Task.Run(() => ((com.gateway.connector.tcp.GateWayApi)api.GetApiConnect())
                .connect("127.0.0.1", 28600, user, Environment.GetEnvironmentVariable("FICC_TEST_PASSWORD"),
                    false, 1, new Dictionary<string, object> { ["isManage"] = "N" }));
            Check((string)JObject.Parse(response)["Code"] == "0001", "local STC login");
            api.UserName = user;
            api.Sid = (string)JObject.Parse(response)["sid"];
            ApiManager.Instance.AddApi(api);
            ApiManager.Instance.SetDefaultApi(api.ApiKey);
            using (var service = new DpsSalesTradingServices())
            {
                DictionaryData dictionary = await service.GetFilterDictionaryAsync();
                Check(dictionary.FilterFields.Count > 40, "live QuoteSvr dictionary");
                File.WriteAllText("sidebar-dictionary.json", dictionary.ToJson().ToString());
                var host = new RecordingHost(dictionary);
                foreach (FrameworkElement panel in smokeOnly ? new FrameworkElement[0] : new FrameworkElement[]
                    { new CreditFilterPanel(), new RateFilterPanel(), new AlertFilterPanel() })
                {
                    panel.DataContext = host;
                    Window window = Show(panel);
                    await Task.Delay(100);
                    SidebarFilter.Configure(panel, dictionary);
                    panel.UpdateLayout();
                    foreach (FrameworkElement field in SidebarFilter.Descendants<FrameworkElement>(panel)
                        .Where(item => SidebarFilter.GetField(item) != null).ToArray())
                    {
                        SidebarFilter.Reset(panel);
                        string name = SidebarFilter.GetField(field);
                        if (field is FilterRangeRow range)
                        {
                            if (range.IsDisabled) continue;
                            range.MinText = "1";
                        }
                        else if (field is FilterSearchRow)
                            SidebarFilter.Descendants<TextBox>(field).First().Text = "检索";
                        else if (field is ComboBox select) select.SelectedIndex = 1;
                        else if (field is FilterComboRow)
                            SidebarFilter.Descendants<ComboBox>(field).First().SelectedIndex = 1;
                        else SidebarFilter.Descendants<ToggleButton>(field).First().IsChecked = true;
                        Filter filter = SidebarFilter.Read(panel);
                        Check(filter.Conditions.Any(condition => condition.Field == name), panel.GetType().Name + ": " + name);
                        filters.Add(filter.ToJson());
                    }
                    SidebarFilter.Reset(panel);
                    Range(panel, "bidPrice").MinText = "2";
                    Range(panel, "bidPrice").MaxText = "3";
                    Check(SidebarFilter.Read(panel).Conditions.Single().Operator == "BETWEEN", "bounded range is AND, not same-field OR");
                    SidebarFilter.Reset(panel);
                    Toggle(panel, "remainingYears", "<3M").IsChecked = true;
                    Toggle(panel, "remainingYears", "3-5Y").IsChecked = true;
                    Check(SidebarFilter.Read(panel).Conditions.Count == 2, "disjoint maturity ranges remain OR alternatives");
                    SidebarFilter.Reset(panel);
                    Toggle(panel, "settlType", "T+1").IsChecked = true;
                    Toggle(panel, "settlType", "非远期").IsChecked = true;
                    Toggle(panel, "quoteType", "净价").IsChecked = true;
                    Range(panel, "bidValuationSpread").MinText = "-0.1";
                    await Task.Delay(400);
                    Check(host.Last.Conditions.Any(condition => condition.Field == "bidCleanPriceSpread"), "clean-price spread uses clean-price field");
                    Check(host.Last.ToJson()["conditions"].OfType<JObject>().Any(condition =>
                        (string)condition["field"] == "settlType" && JToken.DeepEquals(condition["value"], new JArray("2", "1"))),
                        "settlement choices form a union");
                    Range(panel, "bidValuationSpread").MaxText = "-1";
                    bool invalid = false;
                    try { SidebarFilter.Read(panel); } catch (InvalidOperationException) { invalid = true; }
                    Check(invalid, "reversed range is rejected");
                    SidebarFilter.Reset(panel);
                    Check(SidebarFilter.Read(panel).Conditions.Count == 0 && host.Last.Conditions.Count == 0, "reset clears controls and applied filter");
                    window.Close();
                }

                // Production ViewModels and adapter, real GW/QuoteSvr: no quote writes.
                using (var management = new QuoteMgmtViewModel(service))
                {
                    var panel = new MgmtFilterPanel { DataContext = management };
                    Window window = Show(panel);
                    await Until(() => !management.Today.IsLoading && !management.History.IsLoading);
                    await Task.Delay(100);
                    var combined = SidebarFilter.Descendants<AlertFilterPanel>(panel).Single();
                    SidebarFilter.Configure(combined, dictionary);
                    int original = management.Today.Rows.Count;
                    Check(management.Today.ErrorMessage == null && management.History.ErrorMessage == null, "today/history baseline queries");
                    Range(combined, "bidPrice").MinText = "999999999";
                    await Task.Delay(450);
                    await Until(() => !management.Today.IsLoading && !management.History.IsLoading);
                    Check(management.Today.ErrorMessage == null && management.Today.Rows.Count == 0, "today: impossible Bid price removes rows");
                    Check(management.History.ErrorMessage == null && management.History.Rows.Count == 0, "history: same sidebar condition reaches server");
                    SidebarFilter.Reset(combined);
                    await Until(() => !management.Today.IsLoading && !management.History.IsLoading);
                    Check(management.Today.Rows.Count == original, "today: reset restores baseline rows=" + original);
                    management.FilterSearchText = "SIDEBAR-NO-SUCH-BOND-" + Guid.NewGuid().ToString("N");
                    await Until(() => !management.Today.IsLoading && !management.History.IsLoading);
                    Check(management.Today.Rows.Count == 0 && management.History.Rows.Count == 0, "management sidebar text filters both tabs");
                    window.Close();
                }
                foreach (BondMarket market in new[] { BondMarket.Credit, BondMarket.Rate })
                {
                    using (var all = new BoardListViewModel(service, market))
                    using (var best = new BoardBestViewModel(service, market))
                    {
                        await Until(() => !all.IsLoading && !best.IsLoading);
                        int original = all.Rows.Count;
                        var filter = new Filter { Conditions = new List<FilterCondition>
                        { new FilterCondition { Field = "bidPrice", Operator = "GTE", Value = 999999999m } } };
                        all.ApplyProductFilter(filter);
                        best.ApplyProductFilter(filter);
                        await Until(() => !all.IsLoading && !best.IsLoading);
                        Check(all.ErrorMessage == null && best.ErrorMessage == null && all.Rows.Count == 0 && best.Rows.Count == 0,
                            market + ": full and best views filter on server");
                        all.ApplyProductFilter(new Filter { Conditions = new List<FilterCondition>() });
                        best.ApplyProductFilter(new Filter { Conditions = new List<FilterCondition>() });
                        await Until(() => !all.IsLoading && !best.IsLoading);
                        Check(all.Rows.Count == original, market + ": reset restores baseline rows=" + original);
                    }
                }
                using (var market = new MarketCenterViewModel(service))
                {
                    var panel = new MarketFilterPanel { DataContext = market };
                    Window window = Show(panel);
                    await Until(() => !market.CreditAll.IsLoading && !market.RateAll.IsLoading);
                    market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "rateAll");
                    window.UpdateLayout();
                    await Task.Delay(200);
                    var rate = (RateFilterPanel)panel.FindName("RateHost");
                    Toggle(rate, "settlType", "T+1").IsChecked = true;
                    await Task.Delay(450);
                    Check(SidebarFilter.Read(rate).Conditions.Single().Field == "settlType", "hidden rate panel initializes when selected");
                    await Until(() => !market.RateAll.IsLoading && !market.RateBest.IsLoading);
                    Check(market.RateAll.ErrorMessage == null && market.RateBest.ErrorMessage == null, "rate sidebar applies to both real tab queries");
                    market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "creditAll");
                    window.UpdateLayout();
                    var credit = (CreditFilterPanel)panel.FindName("CreditHost");
                    Check(SidebarFilter.Read(credit).Conditions.Count == 0, "credit conditions remain independent");
                    market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "rateBest");
                    window.UpdateLayout();
                    Check(SidebarFilter.Read(rate).Conditions.Count == 1, "rate condition survives all/best tab switch");
                    market.SelectedSubTab = market.SubTabs.Single(tab => tab.Key == "alert");
                    window.UpdateLayout();
                    await Task.Delay(200);
                    var alert = (AlertFilterPanel)panel.FindName("AlertHost");
                    Range(alert, "bidPrice").MinText = "999999999";
                    await Task.Delay(450);
                    await Until(() => !market.Alert.IsLoading);
                    Check(market.Alert.ErrorMessage == null && market.Alert.Rows.Count == 0, "notification sidebar reaches real NotificationSvr");
                    window.Close();
                }
            }
        }
        finally { api.DisConnect(); }
    }

    private static Window Show(FrameworkElement content)
    {
        var window = new Window { Content = content, Width = 430, Height = 800, ShowActivated = false,
            ShowInTaskbar = false, Left = -20000, Top = -20000 };
        window.Show();
        window.UpdateLayout();
        return window;
    }

    private static FilterRangeRow Range(FrameworkElement root, string name) =>
        SidebarFilter.Descendants<FilterRangeRow>(root).Single(row => SidebarFilter.GetField(row) == name);

    private static ToggleButton Toggle(FrameworkElement root, string field, string caption) =>
        SidebarFilter.Descendants<FrameworkElement>(root).Where(item => SidebarFilter.GetField(item) == field)
            .SelectMany(item => SidebarFilter.Descendants<ToggleButton>(item)).Single(toggle => toggle.Content?.ToString() == caption);

    private static async Task Until(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(15);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("query completion");
            await Task.Delay(50);
        }
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        passed++;
        Console.WriteLine("PASS " + description);
    }

    private sealed class RecordingHost : IProductFilterHost
    {
        private readonly DictionaryData dictionary;
        public Filter Last;
        public RecordingHost(DictionaryData dictionary) { this.dictionary = dictionary; }
        public Task<DictionaryData> GetFilterDictionaryAsync() => Task.FromResult(dictionary);
        public void ApplyProductFilter(string scope, Filter filter) { Last = filter; }
    }
}
