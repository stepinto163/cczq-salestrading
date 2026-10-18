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
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
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
        private static Task QuoteHistoryWindowPaging()
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                System.Windows.Window window = null;
                try
                {
                    var gateway = new ControlledGateway
                    {
                        Respond = request =>
                        {
                            var body = JObject.Parse((string)request.Payload);
                            int number = (int)body["pageNumber"];
                            return Task.FromResult(Reply(new QuoteLogData
                            {
                                QuoteId = "q1", PageNumber = number, PageSize = 20,
                                SnapshotRevision = "41", TotalCount = 41, HasMore = number < 3,
                                Records = Enumerable.Range(1, 41).Reverse().Skip((number - 1) * 20).Take(20)
                                    .Select(revision => new QuoteLogRecord { Revision = revision.ToString(), Action = "UPDATE" }).ToList()
                            }));
                        }
                    };
                    var requester = new GatewayRequester(gateway);
                    using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
                    using (var service = new GatewayQuoteService(requester, stream))
                    using (var model = new STC.SalesTrading.Plugin.Modules.MarketCenter.View.QuoteLogViewModel(service, "q1"))
                    {
                        window = (System.Windows.Window)Activator.CreateInstance(
                            typeof(STC.SalesTrading.Plugin.Modules.MarketCenter.View.QuoteLogWindow), true);
                        window.DataContext = model;
                        window.Show();
                        model.LoadPageAsync(1, true).GetAwaiter().GetResult();
                        Action render = () =>
                        {
                            window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() => { }));
                            window.UpdateLayout();
                        };
                        render();
                        var pager = FindVisual<STC.SalesTrading.Plugin.Shared.Components.PaginationBar>(window);
                        var next = (System.Windows.Controls.Button)pager.FindName("NextButton");
                        var previous = (System.Windows.Controls.Button)pager.FindName("PrevButton");
                        var grid = (System.Windows.Controls.DataGrid)window.FindName("LogGrid");
                        Equal(false, previous.IsEnabled, "first page disables previous");
                        Equal(20, grid.Items.Count, "first page rendered row count");
                        next.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        render();
                        Equal(2, model.Page, "real next button loads page two");
                        Equal("21", ((QuoteLogEntry)grid.Items[0]).Revision, "page two grid data");
                        next.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        render();
                        Equal(1, grid.Items.Count, "last page renders single remaining record");
                        Equal(false, next.IsEnabled, "last page disables next");
                        previous.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        render();
                        Equal(2, model.Page, "real previous button restores page two");
                        var image = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth,
                            (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                        image.Render(window);
                        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                        using (var output = System.IO.File.Create("quote-history-pagination.png")) encoder.Save(output);
                    }
                }
                catch (Exception error) { failure = error; }
                finally { window?.Close(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) throw new Exception("WPF history paging", failure);
            return Task.CompletedTask;
        }

        private static T FindVisual<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
        {
            if (root is T match) return match;
            for (int index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
            {
                var found = FindVisual<T>(System.Windows.Media.VisualTreeHelper.GetChild(root, index));
                if (found != null) return found;
            }
            return null;
        }

        private static Task NotificationTriggerIdentity()
        {
            var tracker = new AlertTriggerTracker();
            Func<string, string[], AlertRow> row = (id, keys) => new AlertRow
            {
                NotificationId = id,
                Triggers = keys.Select(key => new AlertTrigger { AlertKey = key, Type = AlertType.FavoriteBond }).ToList()
            };
            Equal(0, tracker.Observe(new[] { row("merged-A", new[] { "A1" }) }, true).Count, "first snapshot never pops even if push races login");
            Equal(1, tracker.Observe(new[] { row("merged-AB", new[] { "A1", "B1" }) }, true).Count, "adding B pops only B");
            Equal(0, tracker.Observe(new[] { row("merged-AB", new[] { "A1", "B1" }) }, true).Count, "duplicate update does not pop");
            Equal(0, tracker.Observe(new[] { row("merged-B", new[] { "B1" }) }, true).Count, "removing A must not re-pop unchanged B");
            var changed = tracker.Observe(new[] { row("merged-B2", new[] { "B2" }) }, true);
            Equal("B2", changed.Single().Value.AlertKey, "new version pops exactly once");
            Equal(0, tracker.Observe(new[] { row("refreshed", new[] { "B3" }) }, false).Count, "manual refresh establishes baseline only");
            Equal(0, tracker.Observe(new[] { row("refreshed", new[] { "B3" }) }, true).Count, "refresh baseline prevents delayed popup");
            var read = row("read", new[] { "B4" });
            read.ReadTime = DateTime.UtcNow;
            Equal(0, tracker.Observe(new[] { read }, true).Count, "read notification does not re-pop");
            Equal(0, tracker.Observe(new[] { new AlertRow { NotificationId = "old-server" } }, true).Count, "missing trigger metadata does not guess from aggregate identity");
            return Task.CompletedTask;
        }

        private static AlertRow TriggerRow(params string[] keys)
        {
            return new AlertRow { NotificationId = Guid.NewGuid().ToString(), Triggers = keys
                .Select(key => new AlertTrigger { AlertKey = key, Type = AlertType.FavoriteBond }).ToList() };
        }

        private static Task NotificationCombinedReasons()
        {
            var tracker = new AlertTriggerTracker();
            tracker.Observe(new AlertRow[0], false);
            Equal(4, tracker.Observe(new[] { TriggerRow("watch:A1", "filter:A1", "best:A1"),
                TriggerRow("counter:A1:B1") }, true).Count, "four independent reasons remain distinct");
            Equal(0, tracker.Observe(new[] { TriggerRow("watch:A1", "best:A1"),
                TriggerRow("counter:A1:B1") }, true).Count, "removing filter reason does not re-pop other reasons");
            Equal(0, tracker.Observe(new[] { TriggerRow("watch:A1", "filter:A1", "best:A1") }, true).Count,
                "re-enabled unchanged reason remains deduplicated in this session");
            var changed = tracker.Observe(new[] { TriggerRow("watch:A2", "filter:A2", "best:A2"),
                TriggerRow("counter:A2:B1") }, true);
            Equal(4, changed.Count, "source edit triggers all matching new-version reasons");
            return Task.CompletedTask;
        }

        private static Task NotificationSnapshotsAndPaging()
        {
            var tracker = new AlertTriggerTracker();
            tracker.Observe(new[] { TriggerRow("A1") }, false);
            Equal(0, tracker.Observe(new[] { TriggerRow("B1") }, false).Count, "page navigation establishes baseline");
            Equal(0, tracker.Observe(new[] { TriggerRow("A1", "B1") }, true).Count, "seen rows moving between pages do not pop");
            Equal(1, tracker.Observe(new[] { TriggerRow("B2") }, true).Count, "loaded page new version pops");
            Equal(0, tracker.Observe(new[] { TriggerRow("B2") }, false).Count, "reconnect resnapshot has no popup");
            var restarted = new AlertTriggerTracker();
            Equal(0, restarted.Observe(new[] { TriggerRow("B2") }, true).Count, "new client snapshot never replays old popups");
            Equal(1, restarted.Observe(new[] { TriggerRow("B3") }, true).Count, "new events after restart are not suppressed");
            return Task.CompletedTask;
        }

        private static Task NotificationRapidUpdatesAndUsers()
        {
            var firstUser = new AlertTriggerTracker();
            var secondUser = new AlertTriggerTracker();
            firstUser.Observe(new AlertRow[0], false);
            secondUser.Observe(new AlertRow[0], false);
            foreach (string key in new[] { "A1", "A2", "A3" })
            {
                Equal(1, firstUser.Observe(new[] { TriggerRow(key) }, true).Count, "ordered new version seen once");
                Equal(0, firstUser.Observe(new[] { TriggerRow(key) }, true).Count, "duplicate rapid version suppressed");
            }
            Equal(1, secondUser.Observe(new[] { TriggerRow("A3") }, true).Count, "one user's tracker cannot consume another's reminder");
            var read = TriggerRow("A3");
            read.ReadTime = DateTime.UtcNow;
            Equal(0, firstUser.Observe(new[] { read }, true).Count, "marking read does not pop");
            read.ReadTime = null;
            Equal(0, firstUser.Observe(new[] { read }, true).Count, "marking unread does not create a quote event");
            Equal(1, firstUser.Observe(new[] { TriggerRow("A4") }, true).Count, "next quote version still pops");
            return Task.CompletedTask;
        }

        private static Task QuoteHistoryOperatorNames()
        {
            var users = new[]
            {
                new STC.Common.Model.Admin.StcTbookUsers { Userid = "101", Username = "adrian", Name = "龚逸" },
                new STC.Common.Model.Admin.StcTbookUsers { Userid = "202", Username = "lijc", Name = "李继存" }
            };
            var names = UserNameMap.BuildLoginNames(users, "303", "test2");
            Equal("adrian", UserNameMap.ResolveLoginName("101", names), "operator uses login name rather than display name");
            Equal("lijc", UserNameMap.ResolveLoginName("202", names), "other operator is not relabeled as current viewer");
            Equal("test2", UserNameMap.ResolveLoginName("303", names), "current session supplements absent cache entry");
            Equal("未知用户", UserNameMap.ResolveLoginName("404", names), "unknown numeric identity is not shown as a username");
            Equal("未知用户", UserNameMap.ResolveLoginName(null, names), "missing identity is explicit");
            return Task.CompletedTask;
        }

        private static async Task QuoteHistoryPaging()
        {
            bool fail = false;
            int latest = 105;
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    Equal(GatewayTopics.QuoteHistoryQuery, request.Topic, "history topic");
                    var body = JObject.Parse((string)request.Payload);
                    Equal(null, body["limit"], "no legacy limit");
                    Equal(null, body["cursor"], "no legacy cursor");
                    Equal("q1", (string)body["quoteId"], "quote identity retained");
                    if (fail) throw new TimeoutException("history unavailable");
                    int number = (int)body["pageNumber"];
                    int size = (int)body["pageSize"];
                    int boundary = body["snapshotRevision"] == null ? latest : int.Parse((string)body["snapshotRevision"]);
                    var records = Enumerable.Range(1, boundary).Reverse().Skip((number - 1) * size).Take(size)
                        .Select(revision => new QuoteLogRecord { Revision = revision.ToString(), Action = "UPDATE" }).ToList();
                    return Task.FromResult(Reply(new QuoteLogData { QuoteId = "q1", PageNumber = number,
                        PageSize = size, SnapshotRevision = boundary.ToString(), TotalCount = boundary,
                        HasMore = number * size < boundary, Records = records }));
                }
            };
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var service = new GatewayQuoteService(requester, stream))
            using (var model = new STC.SalesTrading.Plugin.Modules.MarketCenter.View.QuoteLogViewModel(service, "q1"))
            {
                await model.LoadPageAsync(1, true);
                Equal(105, model.TotalCount, "all audit entries counted");
                Equal(6, model.TotalPages, "six pages rather than truncated first 100");
                Equal("105", model.Records[0].Revision, "latest first");
                latest = 106;
                await model.LoadPageAsync(6);
                Equal(6, model.Page, "last page selected");
                Equal(5, model.Records.Count, "last page contains five entries");
                Equal("1", model.Records.Last().Revision, "oldest entry reachable");
                Equal(105, model.TotalCount, "new event does not shift current snapshot");
                fail = true;
                await model.LoadPageAsync(5);
                Equal(6, model.Page, "failed navigation preserves displayed page");
                Equal(5, model.Records.Count, "failed navigation preserves records");
                Equal(true, model.RetryCommand.CanExecute(null), "failed page can be retried");
                fail = false;
                await model.RetryCommand.ExecuteAsync(null);
                Equal(5, model.Page, "retry requests failed target page");
                Equal(null, model.ErrorMessage, "retry clears error");
                await model.RefreshCommand.ExecuteAsync(null);
                Equal(1, model.Page, "refresh resets to first page");
                Equal(106, model.TotalCount, "refresh fetches latest boundary");
                Equal("106", model.Records[0].Revision, "refresh displays newest event");
                int beforeClose = gateway.Requests.Count;
                model.Dispose();
                await model.LoadPageAsync(2);
                Equal(beforeClose, gateway.Requests.Count, "closed dialog sends no further requests");
            }
        }

        private static async Task LifecyclePayload()
        {
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    var item = (JObject)JObject.Parse((string)request.Payload)["quotes"][0];
                    var allowed = new[] { "quoteId", "expectedRevision", "reason" };
                    Equal(true, item.Properties().All(property => allowed.Contains(property.Name)),
                        "lifecycle cannot contain edit fields, including explicit nulls");
                    Equal("q1", (string)item["quoteId"], "lifecycle identity");
                    Equal("7", (string)item["expectedRevision"], "optimistic revision");
                    return Task.FromResult(WriteReply(request));
                }
            };
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var management = new GatewayQuoteMgmtService(requester, stream))
            using (var quotes = new GatewayQuoteService(requester, stream))
            {
                var rows = new[] { new QuoteRow { QuoteId = "q1", Revision = "7" } };
                await management.ReferQuotesAsync(rows, Token);
                await management.UnreferQuotesAsync(rows, Token);
                await management.DeleteQuotesAsync(rows, false, Token);
                await quotes.DeleteQuotesAsync(rows, Token);
                Equal(4, gateway.Requests.Count, "all lifecycle paths exercised");
            }
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
        private static async Task CrmInstitutionMetadata()
        {
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    var payload = JObject.Parse((string)request.Payload);
                    if (request.Topic == GatewayTopics.BaseQuery)
                    {
                        return Task.FromResult(Reply(new { records = new[]
                        {
                            new { contactId = "c1", contactName = "同名联系人", institutionIds = new[] { "i1", "i2" } },
                            new { contactId = "c2", contactName = "同名联系人", institutionIds = new[] { "i1" } }
                        }}));
                    }
                    Equal(GatewayTopics.BaseGet, request.Topic, "batched institution lookup");
                    Equal(2, ((JArray)payload["ids"]).Count, "deduplicated institutions");
                    return Task.FromResult(Reply(new { records = new[]
                    {
                        new { institutionId = "i1", institutionName = "机构甲" },
                        new { institutionId = "i2", institutionName = "机构乙" }
                    }}));
                }
            };
            var directory = new GatewayBaseDirectory(new GatewayRequester(gateway));
            var result = await directory.SearchPageAsync("CONTACT", "同名", null, 0, 50, Token);
            Equal(3, result.Items.Count, "one choice per contact/institution relationship");
            Equal(true, result.Items.All(item => !string.IsNullOrEmpty(item.InstitutionName)), "visible institution names");
            Equal(2, gateway.Requests.Count, "no per-contact request loop");
            var scoped = await directory.SearchPageAsync("CONTACT", "同名", "i2", 0, 50, Token);
            Equal(1, scoped.Items.Count, "institution scope");
            Equal("i2", scoped.Items.Single().InstitutionId, "chosen relationship");
        }

        private static Task GroupRoundTrip(bool watchlist)
        {
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    var payload = JObject.Parse((string)request.Payload);
                    var market = (string)payload["market"];
                    if (request.Topic == GatewayTopics.QuoteDictionaryQuery)
                        return Task.FromResult(Reply(new { filterFields = new[] { new { field = "price" }, new { field = "urgent" } } }));
                    if (request.Topic == GatewayTopics.WatchlistQuery)
                        return Task.FromResult(Reply(new { watchlists = new[] { new { watchlistId = market, name = market, revision = "1" } } }));
                    if (request.Topic == GatewayTopics.FilterGroupQuery)
                        return Task.FromResult(Reply(new { filterGroups = new[] { new
                        {
                            filterGroupId = market, name = market, revision = "1",
                            subgroups = new[] { new { conditions = new object[]
                            {
                                new { field = "price", @operator = "BETWEEN", value = new[] { 1.1m, 2.3m } },
                                new { field = "urgent", @operator = "EQ", value = true }
                            } } }
                        } } }));
                    if (request.Topic == GatewayTopics.FilterGroupSave)
                    {
                        var conditions = (JArray)payload["subgroups"][0]["conditions"];
                        Equal("BETWEEN", (string)conditions[0]["operator"], "range operator preserved");
                        Equal(JTokenType.Array, conditions[0]["value"].Type, "range is not converted to text");
                        Equal(true, (bool)conditions[1]["value"], "boolean type preserved");
                        return Task.FromResult(Reply(new { filterGroup = new { filterGroupId = "RATE", revision = "2" } }));
                    }
                    if (request.Topic == GatewayTopics.WatchlistSave)
                        return Task.FromResult(Reply(new { watchlist = new { watchlistId = "RATE", revision = "2" } }));
                    return Task.FromResult(Reply(new { deleted = true }));
                }
            };
            var directory = new GatewayGroupDirectory(new GatewayRequester(gateway));
            if (watchlist)
            {
                directory.GetFavGroups(FilterProfileService.MarketKind.Credit);
                var rate = directory.GetFavGroups(FilterProfileService.MarketKind.Rate);
                directory.SaveFavGroups(FilterProfileService.MarketKind.Rate, rate);
                Equal(0, gateway.Requests.Count(request => request.Topic == GatewayTopics.WatchlistDelete), "other market retained");
                directory.SaveFavGroups(FilterProfileService.MarketKind.Rate, new FavBondGroup[0]);
                var deleted = gateway.Requests.Single(request => request.Topic == GatewayTopics.WatchlistDelete);
                Equal("RATE", (string)JObject.Parse((string)deleted.Payload)["watchlistId"], "delete only this market");
            }
            else
            {
                directory.GetFilterGroups(FilterProfileService.MarketKind.Credit);
                var rate = directory.GetFilterGroups(FilterProfileService.MarketKind.Rate);
                directory.SaveFilterGroups(FilterProfileService.MarketKind.Rate, rate);
                Equal(0, gateway.Requests.Count(request => request.Topic == GatewayTopics.FilterGroupDelete), "other market retained");
                directory.SaveFilterGroups(FilterProfileService.MarketKind.Rate, new FilterNamedGroup[0]);
                var deleted = gateway.Requests.Single(request => request.Topic == GatewayTopics.FilterGroupDelete);
                Equal("RATE", (string)JObject.Parse((string)deleted.Payload)["filterGroupId"], "delete only this market");
            }
            return Task.CompletedTask;
        }

        private static Task PendingToolbarSelection()
        {
            var gateway = new ControlledGateway();
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var service = new GatewayQuoteService(requester, stream))
            using (var model = new QuoteConfirmViewModel(service))
            {
                var batch = new QuoteConfirmBatchViewModel(new QuoteConfirmBatch { BatchId = "test" });
                var selected = new QuoteConfirmRowViewModel(Input("a")) { IsSelected = true };
                var untouched = new QuoteConfirmRowViewModel(Input("b"));
                batch.Rows.Add(selected);
                batch.Rows.Add(untouched);
                model.Batches.Add(batch);
                model.AmountInput = "300";
                Equal("300", selected.Volume, "quantity text replaces selected quantity");
                model.AdjustAmountUpCommand.Execute(null);
                Equal("1300", selected.Volume, "+1k is fixed 1000, not input multiplier");
                Equal("1000", untouched.Volume, "unchecked quantity unchanged");
                model.SelectContactCommand.Execute(new LazySearchItem { Id = "c1", Name = "联系人", InstitutionId = "i1", InstitutionName = "机构甲" });
                Equal("i1", selected.InstitutionId, "batch contact fills institution");
                Equal(null, untouched.ContactId, "unchecked contact unchanged");
                model.TraderSelectedId = null;
                model.TraderInput = "仅输入文本";
                Equal(null, selected.ContactId, "typed text does not resolve contact");
                model.InstitutionSelectedId = "i2";
                Equal(string.Empty, selected.Trader, "new institution clears incompatible contact");
                Equal(string.Empty, model.TraderInput, "toolbar clears incompatible contact text too");
                Equal(null, model.TraderSelectedId, "toolbar clears incompatible contact ID too");
            }
            return Task.CompletedTask;
        }

        private static async Task ManagementPageAndRefresh()
        {
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    var payload = JObject.Parse((string)request.Payload);
                    if (request.Topic == GatewayTopics.QuoteViewOpen)
                    {
                        Equal("VISIBLE", (string)payload["ownerScope"], "confirmed quotes shared within permissions");
                        return Task.FromResult(Reply(new { streamId = "s", sequence = "1", view = new { totalCount = 2 }, quotes = new[] { Quote("1") } }));
                    }
                    if (request.Topic == GatewayTopics.QuoteViewQuery)
                    {
                        var page = (int)payload["pageNumber"];
                        var quote = Quote("2");
                        if (page == 2) quote.QuoteId = "q2";
                        return Task.FromResult(Reply(new { streamId = "s", sequence = "2", view = new { totalCount = 2 }, quotes = new[] { quote } }));
                    }
                    return Task.FromResult(Reply(new StreamOpenData { StreamId = "s", PushTopic = "push" }));
                }
            };
            var requester = new GatewayRequester(gateway);
            using (var stream = new GatewayReportStream<QuoteStreamBatch>(requester, GatewayTopics.QuoteStreamOpen))
            using (var service = new GatewayQuoteMgmtService(requester, stream))
            {
                var query = new STC.SalesTrading.Plugin.Abstractions.Queries.QuoteMgmtQuery { PageSize = 1 };
                var first = await service.QueryTodayRowsAsync(query, Token);
                Equal("1", first.Items.Single().Revision, "initial revision");
                query.ForceRefresh = true;
                var refreshed = await service.QueryTodayRowsAsync(query, Token);
                Equal("2", refreshed.Items.Single().Revision, "own edit must fetch latest revision");
                query.Page = 2;
                var second = await service.QueryTodayRowsAsync(query, Token);
                Equal("q2", second.Items.Single().QuoteId, "second page not skipped twice");
            }
        }

        private static async Task NotificationReadFailure()
        {
            var gateway = new ControlledGateway
            {
                Respond = request =>
                {
                    if (request.Topic == GatewayTopics.NotificationViewOpen)
                        return Task.FromResult(Reply(new { streamId = "s", sequence = "1", notifications = new[] { new { notificationId = "n1", quoteIds = new[] { "q1" } } } }));
                    if (request.Topic == GatewayTopics.NotificationRead)
                        return Task.FromResult(Reply(new { results = new[] { new { notificationId = "n1", Code = "1001", Msg = "rejected" } } }));
                    return Task.FromResult(Reply(new StreamOpenData { StreamId = "s", PushTopic = "push" }));
                }
            };
            using (var service = new GatewayMarketCenterService(new GatewayRequester(gateway)))
            {
                await service.LoadAlertRowsAsync(null, 1, 50, Token);
                await Throws<GatewayApiException>(() => service.MarkAlertReadAsync("n1", Token));
                var rows = await service.LoadAlertRowsAsync(null, 1, 50, Token);
                Equal(false, rows.Single().IsRead, "server rejection cannot appear read locally");
            }
        }

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
            await Run("pending-toolbar-selected-rows-only", PendingToolbarSelection);
            await Run("management-scope-refresh-pagination", ManagementPageAndRefresh);
            await Run("notification-read-rejection-keeps-unread", NotificationReadFailure);
            await Run("crm-batched-institution-metadata", CrmInstitutionMetadata);
            await Run("group-stale-editor-preserves-original-revision", async () =>
            {
                var gateway = new ControlledGateway
                {
                    Respond = request =>
                    {
                        if (request.Topic == GatewayTopics.WatchlistQuery)
                            return Task.FromResult(Reply(new { watchlists = new[] { new { watchlistId = "w1", name = "old", revision = "1" } } }));
                        var payload = JObject.Parse((string)request.Payload);
                        Equal("1", (string)payload["expectedRevision"], "editor's original revision, not another save's revision");
                        if ((string)payload["name"] == "stale")
                            return Task.FromResult("{\"Code\":\"1004\",\"Msg\":\"Group changed\"}");
                        return Task.FromResult(Reply(new { watchlist = new { watchlistId = "w1", revision = "2" } }));
                    }
                };
                var directory = new GatewayGroupDirectory(new GatewayRequester(gateway));
                var first = directory.GetFavGroups(FilterProfileService.MarketKind.Rate);
                var second = directory.GetFavGroups(FilterProfileService.MarketKind.Rate);
                first.Single().Name = "first";
                directory.SaveFavGroups(FilterProfileService.MarketKind.Rate, first);
                second.Single().Name = "stale";
                await Throws<GatewayApiException>(() =>
                {
                    directory.SaveFavGroups(FilterProfileService.MarketKind.Rate, second);
                    return Task.CompletedTask;
                });
                Equal("first", FilterProfileService.GetFavGroups(FilterProfileService.MarketKind.Rate).Single().Name, "failed save cannot replace local state");
            });
            await Run("watchlist-delete-is-market-scoped", () => GroupRoundTrip(true));
            await Run("filter-group-types-and-market-isolation", () => GroupRoundTrip(false));
            await Run("crm-row-selection-fills-and-invalidates-institution", () =>
            {
                var row = new QuoteConfirmRowViewModel(Input("a"));
                row.SelectContactCommand.Execute(new LazySearchItem { Id = "c1", Name = "联系人", InstitutionId = "i1", InstitutionName = "机构甲" });
                Equal("i1", row.Row.InstitutionId, "selection fills institution ID");
                Equal("机构甲", row.Institution, "selection fills displayed institution");
                Equal("c1", row.Row.ContactId, "selected contact ID");
                row.InstitutionId = "i2";
                Equal(null, row.Row.ContactId, "different institution invalidates contact");
                Equal(string.Empty, row.Trader, "stale displayed contact cleared");
                return Task.CompletedTask;
            });
            await Run("reference-fill-duplicates-refresh-batching", ReferenceFill);
            await Run("notification-trigger-identity-and-snapshot", NotificationTriggerIdentity);
            await Run("notification-combined-reasons", NotificationCombinedReasons);
            await Run("notification-snapshots-paging", NotificationSnapshotsAndPaging);
            await Run("notification-rapid-updates-users", NotificationRapidUpdatesAndUsers);
            await Run("quote-history-operator-login-names", QuoteHistoryOperatorNames);
            await Run("quote-history-numbered-paging-refresh-and-retry", QuoteHistoryPaging);
            await Run("quote-history-wpf-pagination-controls", QuoteHistoryWindowPaging);
            await Run("lifecycle-payload-excludes-edit-fields", LifecyclePayload);
            await Run("preview-direction-does-not-require-persisted-id", () =>
            {
                foreach (var side in new[] { QuoteDirection.Bid, QuoteDirection.Ofr })
                {
                    var row = new STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel.QuoteConfirmRowViewModel(
                        new QuoteRow { Side = side, QuoteId = null, Revision = null });
                    Equal(side.ToString(), row.Direction, "解析预览必须显示已识别方向");
                    row.Commit();
                    Equal(side, row.Row.Side, "确认不能改变解析方向");
                }
                return Task.CompletedTask;
            });
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
