using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
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
            CheckParticipantColumns();
            CheckBestPriceParticipants();
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

    private static void CheckParticipantColumns()
    {
        Type mapper = typeof(DpsSalesTradingServices).Assembly.GetType("STC.SalesTrading.Plugin.Ficc.DpsModelMapper");
        var book = JObject.Parse("{\"securityId\":\"240210.IB\",\"quoteRefs\":{\"FICC:Q\":{\"quoteId\":\"Q\",\"quoteRevision\":\"1\"}}}");
        var entry = JObject.Parse("{\"MDEntryID\":\"FICC:Q\",\"MDEntryType\":\"0\",\"MDEntryPx\":\"2.5\",\"MDEntrySize\":\"1000\",\"SettlType\":\"2\",\"UserID\":\"A\",\"InstitutionName\":\"对手机构\",\"TraderName\":\"对手交易员\"}");
        var quote = (JObject)mapper.GetMethod("MarketQuote").Invoke(null, new object[] { book, entry });
        quote["csiValuationYield"] = "1.7050";
        quote["csiValuationCleanPrice"] = "104.6547";
        quote["csiValuationFullPrice"] = "105.1698";
        quote["csiValuationDate"] = "2026-07-24";
        quote["csiValuationStatus"] = "READY";
        quote["csiRecommendedBasis"] = "EXERCISE";
        quote["csiExerciseYield"] = "2.1000";
        mapper.GetMethod("ApplyOwnerDisplay").Invoke(null, new object[]
        {
            quote, new Dictionary<string, string> { ["A"] = "本方归属人" }, "A"
        });
        var display = (JObject)mapper.GetMethod("Display").Invoke(null, new object[] { quote });
        var row = display.ToObject<BoardQuoteRow>();
        Check(row.CsYtm == 1.7050m && row.CsNetPrice == 104.6547m
            && row.CsFullPrice == 105.1698m, "CSI maturity values preserve source units");
        Check(display.ToObject<QuoteConfirmRow>().CsPriceBasis == "行权", "CSI recommended basis is separate from maturity yield");
        Check(display.ToObject<BoardBestRow>().CsYtm == 1.7050m
            && display.ToObject<MgmtQuoteRow>().CsYtm == 1.7050m
            && display.ToObject<BoardAlertRow>().CsYtm == 1.7050m, "CSI fields map across all board models");
        var empty = (JObject)mapper.GetMethod("Display").Invoke(null, new object[] { new JObject() });
        Check(empty.ToObject<BoardQuoteRow>().CsYtm == null, "Missing CSI yield stays empty, not zero");
        var view = new BoardListView
        {
            DataContext = new
            {
                LoadState = STC.SalesTrading.Plugin.Abstractions.Queries.QueryLoadState.Loaded,
                PendingNewQuoteCount = 0
            }
        };
        view.Measure(new Size(1550, 400));
        view.Arrange(new Rect(0, 0, 1550, 400));
        view.UpdateLayout();
        var grid = Descendants(view).OfType<DataGrid>().First();
        grid.ItemsSource = new[] { row };
        view.Measure(new Size(1550, 400));
        view.Arrange(new Rect(0, 0, 1550, 400));
        view.UpdateLayout();
        Pump();
        foreach (var expected in new Dictionary<string, string>
        {
            ["报价机构"] = "对手机构", ["报价交易员"] = "对手交易员", ["长城交易员"] = "本方归属人",
            ["中证净价"] = "104.6547", ["中证全价"] = "105.1698", ["中证YTM(%)"] = "1.7050"
        })
        {
            var column = grid.Columns.OfType<DataGridTextColumn>().Single(item => (string)item.Header == expected.Key);
            grid.ScrollIntoView(row, column);
            grid.UpdateLayout();
            Pump();
            var cell = column.GetCellContent(row) as TextBlock;
            Check(cell != null && cell.Text == expected.Value, "rendered market column: " + expected.Key);
            if (expected.Key.StartsWith("中证"))
            {
                Check(((string)cell.ToolTip).Contains("2026-07-24"), "CSI rendered tooltip preserves valuation date");
            }
        }
        var bitmap = new RenderTargetBitmap(1550, 400, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var file = System.IO.File.Create(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "market-participants.png")))
        {
            encoder.Save(file);
        }
    }

    private static void CheckBestPriceParticipants()
    {
        Type mapper = typeof(DpsSalesTradingServices).Assembly.GetType("STC.SalesTrading.Plugin.Ficc.DpsModelMapper");
        MethodInfo populate = typeof(DpsSalesTradingServices).GetMethod("PopulateBestQuotes", BindingFlags.Static | BindingFlags.NonPublic);
        var book = new JObject { ["securityId"] = "260004.IB", ["updateTime"] = "2026-10-13T15:00:00+08:00", ["quoteRefs"] = new JObject() };
        var rows = new List<BoardQuoteRow>();
        // 与 BEST 协议相同：每侧同价报价按时间倒序；用不同身份验证不会混用旧报价字段。
        foreach (string side in new[] { "0", "1" })
        {
            foreach (string age in new[] { "new", "old" })
            {
                string id = side + age;
                book["quoteRefs"][id] = new JObject { ["quoteId"] = id, ["quoteRevision"] = "1" };
                var entry = new JObject
                {
                    ["MDEntryID"] = id, ["MDEntryType"] = side, ["MDEntryPx"] = "2.0",
                    ["MDEntrySize"] = age == "new" ? "2000" : "1000", ["SettlType"] = "2",
                    ["MDEntryDate"] = "20261013", ["MDEntryTime"] = age == "new" ? "09:07:02.200" : "09:07:01.100",
                    ["InstitutionName"] = "机构-" + id, ["TraderName"] = "对手-" + id, ["UserID"] = id
                };
                var quote = (JObject)mapper.GetMethod("MarketQuote").Invoke(null, new object[] { book, entry });
                mapper.GetMethod("ApplyOwnerDisplay").Invoke(null, new object[]
                {
                    quote, new Dictionary<string, string> { [id] = "本方-" + id }, id
                });
                var display = (JObject)mapper.GetMethod("Display").Invoke(null, new object[] { quote });
                rows.Add(display.ToObject<BoardQuoteRow>());
            }
        }
        var best = new BoardBestRow { Code = "260004.IB", Name = "最优报价测试" };
        populate.Invoke(null, new object[] { best, rows.ToArray() });
        Check(best.InstBid == "机构-0new" && best.TraderBid == "本方-0new", "latest Bid supplies institution and STC owner");
        Check(best.InstOfr == "机构-1new" && best.TraderOfr == "本方-1new", "latest Ofr supplies its own institution and STC owner");
        Check(best.VolBid == 3000m && best.VolOfr == 3000m && best.BidBestCount == 2 && best.OfrBestCount == 2,
            "equal-price quantities and counts remain aggregated");
        Check(best.BidBestQuotes[0].Trader == "对手-0new" && best.OfrBestQuotes[0].Trader == "对手-1new",
            "popup retains counterparty trader separately from owner");
        Check(best.BidBestQuotes[0].Time == "09:07:02" && best.BidBestQuotes[1].Time == "09:07:01",
            "popup uses each quote timestamp, not book update time");
        Check(rows[0].QuoteTime.Value.Millisecond == 200 && best.SourceQuoteIds.Length == 4,
            "individual milliseconds and all source IDs survive projection");

        var view = new BoardBestView { DataContext = new { LoadState = STC.SalesTrading.Plugin.Abstractions.Queries.QueryLoadState.Loaded } };
        view.Measure(new Size(1550, 300));
        view.Arrange(new Rect(0, 0, 1550, 300));
        view.UpdateLayout();
        var grid = Descendants(view).OfType<DataGrid>().First();
        grid.ItemsSource = new[] { best };
        foreach (var expected in new Dictionary<string, string>
        {
            ["报价机构.Bid"] = "机构-0new", ["长城交易员.Bid"] = "本方-0new",
            ["报价机构.Ofr"] = "机构-1new", ["长城交易员.ofr"] = "本方-1new"
        })
        {
            var column = grid.Columns.OfType<DataGridTextColumn>().Single(item => (string)item.Header == expected.Key);
            grid.ScrollIntoView(best, column);
            grid.UpdateLayout();
            Pump();
            var cell = column.GetCellContent(best) as TextBlock;
            Check(cell != null && cell.Text == expected.Value, "rendered best-price column: " + expected.Key);
        }
        var bitmap = new RenderTargetBitmap(1550, 300, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var file = System.IO.File.Create(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "best-price-participants.png")))
        {
            encoder.Save(file);
        }

        rows[0].Institution = null;
        rows[0].Trader = null;
        populate.Invoke(null, new object[] { best, rows.ToArray() });
        Check(best.InstBid == null && best.BidBestQuotes[0].Trader == null && best.TraderBid == "本方-0new",
            "missing latest counterparty never borrows older quote identity");
        populate.Invoke(null, new object[] { best, new[] { rows[1] } });
        Check(best.InstBid == "机构-0old" && best.TraderBid == "本方-0old" && best.VolBid == 1000m,
            "removing newest quote promotes remaining quote");
        Check(best.InstOfr == null && best.TraderOfr == null && best.Ofr == null && best.OfrBestCount == 0,
            "empty side clears stale representative");
        populate.Invoke(null, new object[] { best, Array.Empty<BoardQuoteRow>() });
        Check(best.InstBid == null && best.TraderBid == null && best.Bid == null && best.SourceQuoteIds.Length == 0,
            "empty book clears both sides");
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (DependencyObject nested in Descendants(child))
            {
                yield return nested;
            }
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
