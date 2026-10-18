using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Gateway;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway;
using STC.SalesTrading.Plugin.Gateway.Contracts.Quote;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;

namespace SalesTrading.IntegrationTests
{
    // 使用实际插件程序集，替换的只有 DPS 传输；不访问生产/QA 数据。
    internal sealed class ControlledGateway : IGatewayClient
    {
        internal Func<GatewayRequest, Task<string>> Respond;
        internal readonly List<GatewayRequest> Requests = new List<GatewayRequest>();
        internal readonly List<Action<GatewayMessage>> Handlers = new List<Action<GatewayMessage>>();
        internal int Subscriptions;
        internal int Disposals;
        public string CurrentUserName => "test2";
        public GatewayResponse<T> Request<T>(GatewayRequest request) => RequestAsync<T>(request, CancellationToken.None).GetAwaiter().GetResult();
        public async Task<GatewayResponse<T>> RequestAsync<T>(GatewayRequest request, CancellationToken ct)
        {
            Requests.Add(request);
            var json = Respond == null ? Program.Reply(new StreamOpenData { StreamId = "s", PushTopic = "push" }) : await Respond(request);
            return GatewayResponse<T>.Ok((T)(object)json);
        }
        public IDisposable Subscribe(GatewaySubscription request, Action<GatewayMessage> handler)
        {
            Handlers.Add(handler);
            Subscriptions++;
            return new Release(() => { Disposals++; Handlers.Remove(handler); });
        }
        public IDisposable SubscribeConnectionState(Action<GatewayConnectionChanged> handler) => new Release(() => { });
        internal void Push(object data)
        {
            foreach (var handler in Handlers.ToArray()) handler(new GatewayMessage { Payload = JsonConvert.SerializeObject(data) });
        }
        private sealed class Release : IDisposable
        {
            private Action action;
            internal Release(Action action) { this.action = action; }
            public void Dispose() { Interlocked.Exchange(ref action, null)?.Invoke(); }
        }
    }

    internal static class Program
    {
        private static readonly CancellationToken Token = CancellationToken.None;
        private static int passed;
        private static int failed;
        internal static string Reply(object data) => JsonConvert.SerializeObject(new { Code = "0000", Data = data });
        private static void Equal<T>(T expected, T actual, string detail)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception(detail + ": expected=" + expected + ", actual=" + actual);
        }
        private static async Task Run(string name, Func<Task> test)
        {
            try { await test(); passed++; Console.WriteLine("PASS " + name); }
            catch (Exception error) { failed++; Console.WriteLine("FAIL " + name + " " + error); }
        }
        private static async Task Throws<T>(Func<Task> operation) where T : Exception
        {
            try { await operation(); }
            catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
        private static QuoteRecord Quote(string revision) => new QuoteRecord { QuoteId = "q1", Revision = revision, UpdateTime = DateTime.Now };
        private static QuoteViewData Snapshot(string sequence) => new QuoteViewData { StreamId = "s", Sequence = sequence, Quotes = new List<QuoteRecord> { Quote(sequence) } };
        private static QuoteStreamBatch Batch(string viewId, string sequence, bool remove = false) => new QuoteStreamBatch
        {
            StreamId = "s", Sequence = sequence, Kind = "UPDATE", Quotes = new List<QuoteRecord> { Quote(sequence) },
            Views = new List<ViewDelta> { new ViewDelta { ViewId = viewId, UpsertIds = remove ? null : new List<string> { "q1" }, RemoveIds = remove ? new List<string> { "q1" } : null } }
        };
        private static ControlledGateway ForOpen(TaskCompletionSource<string> response) => new ControlledGateway
        {
            Respond = request => request.Topic.EndsWith("view.open") || request.Topic.EndsWith("view.query")
                ? response.Task : Task.FromResult(Reply(new StreamOpenData { StreamId = "s", PushTopic = "push" }))
        };
        private static async Task QuoteRace(bool opening, bool remove)
        {
            var response = new TaskCompletionSource<string>();
            var gateway = ForOpen(response);
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var view = new QuoteLiveView(requester, stream))
            {
                var operation = view.OpenAsync(new QuoteFilter(), null, "VISIBLE", 200, Token);
                if (!opening)
                {
                    response.SetResult(Reply(Snapshot("1")));
                    await operation;
                    response = new TaskCompletionSource<string>();
                    gateway.Respond = request => request.Topic.EndsWith("view.query") ? response.Task : Task.FromResult(Reply(new { }));
                    operation = view.QueryPageAsync(1, Token);
                }
                gateway.Push(Batch(view.ViewId, "2", remove));
                response.SetResult(Reply(Snapshot("1")));
                await operation;
                Equal(remove ? 0 : 1, view.Rows.Count, "row count after replay");
                if (!remove) Equal("2", view.Rows.Single().Revision, "latest revision");
            }
        }
        private static async Task OtherDomainRace(bool price)
        {
            var response = new TaskCompletionSource<string>();
            var gateway = ForOpen(response);
            var requester = new GatewayRequester(gateway);
            if (price)
            {
                using (var stream = new GatewayReportStream<PriceStreamBatch>(requester, GatewayTopics.PriceStreamOpen))
                using (var view = new PriceLiveView(requester, stream))
                {
                    var opening = view.OpenAsync(new QuoteFilter(), "FULL", 200, Token);
                    gateway.Push(new PriceStreamBatch { StreamId = "s", Sequence = "2", Books = new List<PriceBookRow> { new PriceBookRow { SecurityId = "b", BookRevision = "2", ViewId = view.ViewId } }, Views = new List<ViewDelta> { new ViewDelta { ViewId = view.ViewId, UpsertIds = new List<string> { "b" } } } });
                    response.SetResult(Reply(new PriceViewData { StreamId = "s", Sequence = "1", Books = new List<PriceBookRow> { new PriceBookRow { SecurityId = "b", BookRevision = "1" } } }));
                    await opening;
                    Equal("2", view.Rows.Single().BookRevision, "price revision");
                }
            }
            else
            {
                using (var stream = new GatewayReportStream<NotificationStreamBatch>(requester, GatewayTopics.NotificationStreamOpen))
                using (var view = new NotificationLiveView(requester, stream))
                {
                    var opening = view.OpenAsync(new QuoteFilter(), "ALL", 200, Token);
                    gateway.Push(new NotificationStreamBatch { StreamId = "s", Sequence = "2", Views = new List<ViewDelta> { new ViewDelta { ViewId = view.ViewId, RemoveIds = new List<string> { "n" } } } });
                    response.SetResult(Reply(new NotificationViewData { StreamId = "s", Sequence = "1", Notifications = new List<STC.SalesTrading.Plugin.Gateway.Contracts.Notification.NotificationRecord> { new STC.SalesTrading.Plugin.Gateway.Contracts.Notification.NotificationRecord { NotificationId = "n" } } }));
                    await opening;
                    Equal(0, view.Rows.Count, "notification removed");
                }
            }
        }
        private static QuoteRow Input(string code) => new QuoteRow { SecurityId = code, Price = 2m, Quantity = 1000m, Side = QuoteDirection.Bid, TradeDate = DateTime.Today };
        private static string WriteReply(GatewayRequest request, Func<int, bool> fails = null)
        {
            var input = JObject.Parse((string)request.Payload);
            var results = ((JArray)input["quotes"]).Select((row, index) => new
            {
                Code = fails != null && fails(index) ? "1001" : "0000", Msg = "test result",
                clientQuoteId = (string)row["clientQuoteId"], quoteId = (string)row["quoteId"] ?? "q-" + (string)row["securityId"],
                revision = request.Topic == GatewayTopics.QuoteCreate ? "1" : "2"
            }).ToList();
            return Reply(new { results });
        }
        private static async Task Submission(string mode)
        {
            var gateway = new ControlledGateway();
            int creates = 0;
            int confirms = 0;
            string unknownPayload = null;
            gateway.Respond = request =>
            {
                if (request.Topic == GatewayTopics.QuoteCreate)
                {
                    creates++;
                    if (mode == "create-timeout" && creates == 1)
                    {
                        unknownPayload = (string)request.Payload;
                        throw new TimeoutException("Response lost after commit");
                    }
                    if (mode == "create-timeout") Equal(unknownPayload, (string)request.Payload, "same create request replay");
                    return Task.FromResult(WriteReply(request, mode == "partial-create" && creates == 1 ? new Func<int, bool>(index => index == 1) : null));
                }
                if (request.Topic == GatewayTopics.QuoteConfirm)
                {
                    confirms++;
                    if (mode == "confirm-timeout" && confirms == 1)
                    {
                        unknownPayload = (string)request.Payload;
                        throw new TimeoutException("Response lost after confirm");
                    }
                    if (mode == "confirm-timeout") Equal(unknownPayload, (string)request.Payload, "same confirm request replay");
                    return Task.FromResult(WriteReply(request, mode == "partial-confirm" && confirms == 1 ? new Func<int, bool>(index => index == 1) : null));
                }
                return Task.FromResult(Reply(new { }));
            };
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var service = new GatewayQuoteService(requester, stream))
            {
                var rows = new[] { Input("a"), Input("b") };
                if (mode == "unresolved") rows[0].InstitutionName = "unselected text";
                Exception error = null;
                try { await service.SubmitQuotesAsync(rows, Token); } catch (Exception caught) { error = caught; }
                if (mode == "blank")
                {
                    Equal(null, error, "blank counterparties permitted");
                    Equal(QuoteStatus.Active, rows[0].Status, "blank row confirmed");
                }
                else if (mode == "unresolved")
                {
                    Equal(true, error is GatewayApiException, "unresolved rejected");
                    Equal(null, rows[0].QuoteId, "unresolved never created");
                    Equal(QuoteStatus.Active, rows[1].Status, "valid row succeeds");
                }
                else
                {
                    Equal(true, error != null, "failure surfaced");
                    if (mode.StartsWith("partial")) Equal(QuoteStatus.Active, rows[0].Status, "successful row retained");
                    await service.SubmitQuotesAsync(rows, Token);
                    Equal(true, rows.All(row => row.Status == QuoteStatus.Active), "retry completes remaining rows");
                    Equal(mode == "partial-create" || mode == "create-timeout" ? 2 : 1, creates, "no recreation after confirmation failure");
                }
            }
        }
        private static async Task ReferenceFill()
        {
            var gateway = new ControlledGateway();
            gateway.Respond = request =>
            {
                var ids = (JArray)JObject.Parse((string)request.Payload)["ids"];
                return Task.FromResult(Reply(new { records = ids.Select(id => new BaseRecord { SecurityId = (string)id, ValuationYield = "2.2" }).ToList() }));
            };
            var requester = new GatewayRequester(gateway);
            var cache = new ReferenceDataCache();
            var rows = Enumerable.Range(0, 105).Select(index => new TapeRow { SecurityId = "b" + index }).ToList();
            rows.Add(new TapeRow { SecurityId = "b0" });
            await GatewayReferenceFill.EnsureTapeAsync(requester, rows, Token, cache);
            Equal(true, rows.All(row => row.NzYtm == 2.2m), "all rows including duplicates and >100 ids");
            Equal(2, gateway.Requests.Count, "batch size contract");
            var best = new List<BestRow> { new BestRow { SecurityId = "b0" } };
            await GatewayReferenceFill.EnsureBestAsync(requester, best, Token, cache);
            Equal(2.2m, best[0].NzYtm.Value, "best row receives cached data");
            Equal(2, gateway.Requests.Count, "cache avoids extra network call");
        }
        [STAThread]
        private static int Main()
        {
            Execute().GetAwaiter().GetResult();
            Console.WriteLine("RESULT passed=" + passed + " failed=" + failed);
            return failed == 0 ? 0 : 1;
        }
        private static async Task Execute()
        {
            await Run("quote-open-update-race", () => QuoteRace(true, false));
            await Run("quote-open-remove-race", () => QuoteRace(true, true));
            await Run("quote-query-update-race", () => QuoteRace(false, false));
            await Run("quote-query-remove-race", () => QuoteRace(false, true));
            await Run("price-open-update-race", () => OtherDomainRace(true));
            await Run("notification-open-remove-race", () => OtherDomainRace(false));
            await Run("close-during-view-open", async () =>
            {
                var response = new TaskCompletionSource<string>();
                var gateway = ForOpen(response);
                var requester = new GatewayRequester(gateway);
                using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
                using (var view = new QuoteLiveView(requester, stream))
                {
                    var opening = view.OpenAsync(new QuoteFilter(), null, "VISIBLE", 200, Token);
                    await view.CloseAsync();
                    response.SetResult(Reply(Snapshot("1")));
                    await Throws<OperationCanceledException>(() => opening);
                    Equal(0, view.Rows.Count, "closed view cannot be repopulated");
                }
            });
            await Run("older-query-response-rejected", async () =>
            {
                var response = new TaskCompletionSource<string>();
                var gateway = ForOpen(response);
                var requester = new GatewayRequester(gateway);
                using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
                using (var view = new QuoteLiveView(requester, stream))
                {
                    var opening = view.OpenAsync(new QuoteFilter(), null, "VISIBLE", 200, Token);
                    response.SetResult(Reply(Snapshot("1")));
                    await opening;
                    gateway.Push(Batch(view.ViewId, "3"));
                    await Throws<InvalidOperationException>(() => view.QueryPageAsync(1, Token));
                    Equal(0, view.Rows.Count, "stale snapshot never displayed");
                }
            });
            foreach (var partial in new[] { false, true })
            {
                await Run("preview-command-partial=" + partial, async () =>
                {
                    var gateway = new ControlledGateway
                    {
                        Respond = request => Task.FromResult(request.Topic == GatewayTopics.QuoteCreate
                            ? WriteReply(request, partial ? new Func<int, bool>(index => index == 1) : null)
                            : request.Topic == GatewayTopics.QuoteConfirm ? WriteReply(request) : Reply(new { }))
                    };
                    var requester = new GatewayRequester(gateway);
                    using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
                    using (var service = new GatewayQuoteService(requester, stream))
                    using (var model = new PreciseQuoteViewModel(service))
                    {
                        var accepted = new QuoteConfirmRowViewModel(Input("a")) { IsSelected = true };
                        var other = new QuoteConfirmRowViewModel(Input("b")) { IsSelected = true };
                        model.Rows.Add(accepted);
                        model.Rows.Add(other);
                        await model.ConfirmSelectedCommand.ExecuteAsync(null);
                        Equal(partial ? 1 : 0, model.Rows.Count, "only failed preview remains");
                        Equal(QuoteStatus.Active, accepted.Row.Status, "confirmed by server");
                        if (partial) Equal(other, model.Rows.Single(), "correct failure retained");
                    }
                });
            }
            await Run("buffer-replay-on-register", async () =>
            {
                var gateway = new ControlledGateway();
                using (var stream = new GatewayReportStream<QuoteStreamBatch>(new GatewayRequester(gateway), GatewayTopics.QuoteStreamOpen))
                {
                    await stream.EnsureSubscribedAsync(Token);
                    gateway.Push(Batch("v", "1"));
                    int count = 0;
                    stream.Register(batch => count++);
                    Equal(1, count, "buffered delivery");
                    gateway.Push(Batch("v", "1"));
                    Equal(1, count, "duplicate ignored");
                }
            });
            await Run("dispose-during-subscription", async () =>
            {
                var response = new TaskCompletionSource<string>();
                var gateway = new ControlledGateway { Respond = request => response.Task };
                var stream = new GatewayReportStream<QuoteStreamBatch>(new GatewayRequester(gateway), GatewayTopics.QuoteStreamOpen);
                var opening = stream.EnsureSubscribedAsync(Token);
                stream.Dispose();
                response.SetResult(Reply(new StreamOpenData { StreamId = "s", PushTopic = "push" }));
                await Throws<OperationCanceledException>(() => opening);
                Equal(0, gateway.Subscriptions - gateway.Disposals, "no surviving subscription");
            });
            await Run("sequence-gap-reopens-stream", async () =>
            {
                var gateway = new ControlledGateway();
                using (var stream = new GatewayReportStream<QuoteStreamBatch>(new GatewayRequester(gateway), GatewayTopics.QuoteStreamOpen))
                {
                    int failures = 0;
                    stream.Failed += reason => failures++;
                    await stream.EnsureSubscribedAsync(Token);
                    gateway.Push(Batch("v", "1"));
                    gateway.Push(Batch("v", "3"));
                    Equal(1, failures, "gap visible");
                    await stream.EnsureSubscribedAsync(Token);
                    Equal(2, gateway.Subscriptions, "new subscription");
                }
            });
            foreach (var mode in new[] { "blank", "unresolved", "partial-create", "partial-confirm", "create-timeout", "confirm-timeout" })
                await Run("submission-" + mode, () => Submission(mode));
            await Run("reference-fill-duplicates-refresh-batching", ReferenceFill);
            await Run("blank-crm-commit-normalizes-null", () =>
            {
                var row = new QuoteConfirmRowViewModel(Input("a"));
                row.Commit();
                Equal(null, row.Row.InstitutionName, "institution");
                Equal(null, row.Row.ContactName, "contact");
                return Task.CompletedTask;
            });
            await Run("best-details-latest-time-counterparty", () =>
            {
                var entries = new List<MarketDataEntry>
                {
                    new MarketDataEntry { MDEntryID="old", MDEntryType="0", MDEntryPx="2", MDEntrySize="2000", MDEntryDate="20261018", MDEntryTime="09:00:00.000", InstitutionName="old institution", TraderName="old trader" },
                    new MarketDataEntry { MDEntryID="new", MDEntryType="0", MDEntryPx="2", MDEntrySize="1000", MDEntryDate="20261018", MDEntryTime="10:00:00.000", InstitutionName="new institution", TraderName="new trader" }
                };
                var book = new PriceBookRow { SecurityId = "b", MarketData = new MarketDataSnapshot { NoMDEntries = entries } };
                var method = typeof(GatewayMarketCenterService).GetMethod("AggregateBest", BindingFlags.Static | BindingFlags.NonPublic);
                var row = (BestRow)method.Invoke(null, new object[] { book, default(BondMarket), null, new Dictionary<string,string>() });
                Equal("new", row.BidQuotes[0].QuoteId, "newest first rather than largest volume");
                Equal("new institution", row.BidQuotes[0].Institution, "institution preserved");
                Equal("new trader", row.BidQuotes[0].Trader, "trader preserved");
                Equal("09:00:00", row.BidQuotes[1].Time, "own timestamp");
                return Task.CompletedTask;
            });
        }
    }
}
