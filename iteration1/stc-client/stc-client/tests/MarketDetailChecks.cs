using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;
using STC.SalesTrading.Plugin.Shared.Converters;

internal static class MarketDetailChecks
{
    private static int passed;
    private static readonly Type FeedType = typeof(DpsSalesTradingServices).Assembly
        .GetType("STC.SalesTrading.Plugin.Shared.Services.HostQuoteFeed", true);

    [STAThread]
    public static int Main()
    {
        try
        {
            new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            CheckDetail();
            CheckFeedReplay();
            Console.WriteLine("PASS market detail: " + passed + " checks");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void CheckDetail()
    {
        var detail = new BondDetailViewModel { HasDetail = true, Code = "240210.IB" };
        Check(detail.BidPrice == null && detail.OfrPrice == null, "no invented initial best prices");
        detail.ApplyFeedBondInfo(new QuoteFeedBondInfo { Code = detail.Code, ValuationShow = "1.7500" });
        string valuation = detail.NzDisplay;
        detail.ApplyFeedLevels(new[]
        {
            new QuoteFeedLevel { Side = QuoteFeedSide.Bid, Price = 100m, Volume = 200m, VenueCode = "ODM", Yield = 2m },
            new QuoteFeedLevel { Side = QuoteFeedSide.Bid, Price = 99m, Volume = 300m, VenueCode = "TPSC" },
            new QuoteFeedLevel { Side = QuoteFeedSide.Ofr, Price = 101m, Volume = 400m, VenueCode = "TPSC" }
        });
        Check(detail.BidPrice == 100m && detail.OfrPrice == 101m, "host snapshot supplies both best sides");
        Check(detail.InstQuotes[0].IsBestBid && detail.InstQuotes[0].IsBestOfr, "best side highlights");
        Check(!detail.InstQuotes[1].IsBestBid && !detail.InstQuotes[1].IsBestOfr, "other levels not highlighted");
        Check(detail.NzDisplay == valuation, "market quotes do not overwrite ChinaBond valuation");
        detail.ToggleInst("TPSC");
        Check(detail.BidPrice == 99m && detail.InstQuotes.Count == 1, "channel filter changes best and depth");
        detail.ApplyFeedLevels(Array.Empty<QuoteFeedLevel>());
        Check(detail.InstQuotes.Count == 0 && detail.BidPrice == null && detail.OfrPrice == null,
            "empty snapshot clears obsolete book");
        detail.ApplyFeedLastExec(new QuoteFeedLastExec { Yield = 2.1m });
        Check(detail.LastYield == 2.1m && detail.LastExecDisplay.EndsWith("%"), "yield-only execution displayed");
        detail.ApplyFeedLastExec(new QuoteFeedLastExec { Price = 102m });
        Check(detail.LastYield == null && !detail.LastExecDisplay.EndsWith("%"), "price fallback clears old yield");
        detail.PrevClose = 100m;
        detail.ApplyFeedLastExec(new QuoteFeedLastExec { Price = 102m, Yield = 2.1m });
        Check(detail.IsUp, "compare price with previous price, not yield with price");
        detail.ApplyFeedBondInfo(new QuoteFeedBondInfo { Code = "199385.IB", ShortName = "wrong", ValuationShow = "9" });
        Check(detail.Name != "wrong" && detail.NzDisplay == valuation, "other bond metadata ignored");
        var bond = new BondsInfoPo { CCDCValua = new CCDCValuaModel { CCDCValuation = "1.75|2.00" } };
        var extract = FeedType.GetMethod("ExtractValuationYtm", BindingFlags.NonPublic | BindingFlags.Static);
        Check((string)extract.Invoke(null, new object[] { bond }) == "1.75", "raw valuation uses first yield segment");
        var converter = new VenueBadgeBrushConverter();
        Check(converter.Convert("ODM", null, null, CultureInfo.InvariantCulture).ToString() == "#FF229B88",
            "X-Bond source badge colour");
        var panel = new BondDetailPanel { DataContext = detail };
        panel.Measure(new Size(420, 900));
        panel.Arrange(new Rect(0, 0, 420, 900));
        panel.UpdateLayout();
        Check(panel.ActualWidth == 420, "merged detail XAML and resources load and lay out");
    }

    private static void CheckFeedReplay()
    {
        var feed = (IHostQuoteFeed)Activator.CreateInstance(FeedType, true);
        Select(feed, "240210.IB", 1);
        int snapshots = 0;
        int executions = 0;
        feed.Snapshot += levels => snapshots++;
        feed.LastExecution += execution => executions++;
        Invoke(feed, "OnCtPrice", new MarketDataSnapshotFullRefresh { SecurityID = "240210.IB" });
        Invoke(feed, "OnLastExecution", new ExecutionReport { SecurityID = "240210.IB", LastPx = "101", Yield = "2.1" });
        Pump();
        Check(snapshots == 1 && executions == 1, "live callbacks reach current selection");
        Invoke(feed, "ReplayCurrent");
        Pump();
        Check(snapshots == 2 && executions == 2, "same-bond replay restores snapshot and execution");
        Invoke(feed, "OnLastExecution", new ExecutionReport { SecurityID = "240210.IB", LastPx = "999" });
        Select(feed, "199385.IB", 2);
        Pump();
        Check(executions == 2, "queued previous-bond event discarded");
        Check(((QuoteFeedLastExec)Get(feed, "_lastExec")).Price == 101m, "stale event cannot replace cache");
        Select(feed, "240210.IB", 3);
        Invoke(feed, "OnLastExecution", new ExecutionReport { SecurityID = "240210.IB", LastPx = "998" });
        Select(feed, "199385.IB", 4);
        Select(feed, "240210.IB", 5);
        Pump();
        Check(executions == 2, "A-B-A selection rejects older generation");
        Invoke(feed, "ReplayCurrent");
        Select(feed, null, 6);
        Pump();
        Check(snapshots == 2 && executions == 2, "queued replay discarded after deselection");
        feed.Dispose();
        Check(Get(feed, "_lastExec") == null && Get(feed, "_lastLevels") == null, "dispose clears cached data");
    }

    private static void Select(object feed, string code, long version)
    {
        FeedType.GetField("_current", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(feed, code);
        FeedType.GetField("_selectionVersion", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(feed, version);
    }

    private static object Get(object feed, string field)
    {
        return FeedType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(feed);
    }

    private static void Invoke(object feed, string method, params object[] arguments)
    {
        FeedType.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(feed, arguments);
    }

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        passed++;
    }
}
