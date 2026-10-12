using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;

internal static class ProtocolContractChecks
{
    private static int passed;

    public static int Main(string[] args)
    {
        try
        {
            Run(args[0]).GetAwaiter().GetResult();
            Console.WriteLine("PASS protocol contract: " + passed + " checks");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static async Task Run(string fixturePath)
    {
        CheckMarketParticipants();
        JObject partialBook = JObject.Parse("{\"securityId\":\"240210.IB\",\"marketData\":{\"NoMDEntries\":[]},\"calculationIssues\":[{\"quoteId\":\"Q-CAL\",\"code\":\"CALCULATOR_OFFLINE\"}]}");
        PriceBook decodedBook = ProtocolCodec.Decode<PriceBook>(partialBook);
        Check(decodedBook.CalculationIssues.Single().QuoteId == "Q-CAL", "typed per-quote calculation issue");
        Equal(partialBook, decodedBook.ToJson(), "calculation issue round trip");
        MethodInfo warningMethod = typeof(DpsSalesTradingServices).GetMethod("PriceCalculationWarning", BindingFlags.NonPublic | BindingFlags.Static);
        string warning = (string)warningMethod.Invoke(null, new object[] { new[] { partialBook, partialBook } });
        Check(warning.Contains("1 条") && warning.Contains("报价管理"), "partial warning counts unique quotes and explains original quote access");
        Check(warningMethod.Invoke(null, new object[] { new[] { JObject.Parse("{\"securityId\":\"240210.IB\"}") } }) == null,
            "healthy and legacy price replies have no warning");
        Dictionary<string, Endpoint> endpoints = typeof(SalesTradingApi).GetFields()
            .Select(field => (Endpoint)field.GetValue(null)).ToDictionary(endpoint => endpoint.Topic);
        foreach (JObject fixture in JArray.Parse(File.ReadAllText(fixturePath)))
        {
            if (fixture["message"] is JObject message)
            {
                string topic = (string)fixture["topic"];
                Type type = topic.StartsWith("ficc.quote.report.") ? typeof(QuoteStreamBatch)
                    : topic.StartsWith("ficc.price.report.") ? typeof(PriceStreamBatch)
                    : topic.StartsWith("ficc.notification.report.") ? typeof(NotificationStreamBatch)
                    : topic == "ficc.quote.events" ? typeof(QuoteEventBatch)
                    : topic == "ficc.notification.events" ? typeof(NotificationEventBatch)
                    : topic == "ficc.price.snapshots" ? typeof(PriceFeedBatch) : typeof(PriceBook);
                Equal(message, ProtocolCodec.Decode(message, type).ToJson(), topic + " push");
                continue;
            }
            Endpoint endpoint = endpoints[(string)fixture["topic"]];
            JObject request = (JObject)fixture["request"];
            Equal(request, ProtocolCodec.Decode(request, endpoint.RequestType).ToJson(), endpoint.Topic + " request");
            JObject reply = (JObject)fixture["reply"];
            Equal(reply, ProtocolCodec.Decode<Reply>(reply).ToJson(), endpoint.Topic + " envelope");
            if ((string)reply["Code"] == "0000" && reply["Data"] is JObject data)
            {
                Equal(data, ProtocolCodec.Decode(data, endpoint.DataType).ToJson(), endpoint.Topic + " Data");
            }
        }

        JObject patch = JObject.Parse("{\"quoteId\":\"Q\",\"expectedRevision\":\"9007199254740993\",\"contactId\":null,\"urgent\":false,\"price\":2.50}");
        QuoteChangeItem item = ProtocolCodec.Decode<QuoteChangeItem>(patch);
        Equal(patch, item.ToJson(), "null, false and numeric token");
        item.Price = "2.6";
        Check(item.ToJson()["contactId"].Type == JTokenType.Null, "patch keeps explicit null");
        Check(item.ToJson()["quantity"] == null, "patch omits absent quantity");

        QuoteChangeItem clear = new QuoteChangeItem { QuoteId = "Q", Urgent = false };
        clear.ClearField("contactId");
        Check(clear.ToJson()["contactId"].Type == JTokenType.Null, "new patch can clear contact");
        Check(clear.ToJson()["institutionId"] == null, "new patch omits unspecified field");

        QuoteViewData view = ProtocolCodec.Decode<QuoteViewData>(JObject.Parse("{\"quotes\":[{\"quoteId\":\"A\",\"futureField\":1},{\"quoteId\":\"B\",\"futureField\":2}]}"));
        view.Quotes.Reverse();
        Check((int)view.ToJson()["quotes"][0]["futureField"] == 2, "extensions follow reordered record");

        // 优化初始化快照后，嵌套编辑、未知字段、原文隔离和显式清空必须保持原语义。
        JObject originalView = JObject.Parse("{\"quotes\":[{\"quoteId\":\"A\",\"price\":2.50,\"urgent\":false,\"source\":{\"type\":\"QT\",\"negotiationId\":\"N1\",\"extension\":{\"keep\":1}}}],\"extension\":7}");
        QuoteViewData editableView = ProtocolCodec.Decode<QuoteViewData>(originalView);
        Equal(originalView, editableView.ToJson(), "nested initial encoding retains original numeric tokens");
        editableView.Quotes[0].Price = "2.6";
        editableView.Quotes[0].Source.NegotiationId = "N2";
        editableView.Quotes[0].ClearField("contactId");
        JObject editedView = editableView.ToJson();
        Check((string)editedView["quotes"][0]["price"] == "2.6", "nested numeric-to-string edit retained");
        Check((string)editedView["quotes"][0]["source"]["negotiationId"] == "N2", "nested source edit retained");
        Check((int)editedView["quotes"][0]["source"]["extension"]["keep"] == 1
            && (int)editedView["extension"] == 7, "nested and outer extensions retained after edit");
        Check(editedView["quotes"][0]["contactId"].Type == JTokenType.Null, "nested explicit clear retained");
        originalView["extension"] = 99;
        editedView["quotes"][0]["source"]["extension"]["keep"] = 99;
        Check((int)editableView.ToJson()["extension"] == 7
            && (int)editableView.ToJson()["quotes"][0]["source"]["extension"]["keep"] == 1,
            "input and returned JSON cannot mutate saved originals");
        await Task.WhenAll(Enumerable.Range(0, 8).Select(index => Task.Run(() =>
        {
            QuoteViewData concurrent = ProtocolCodec.Decode<QuoteViewData>(originalView);
            if (!JToken.DeepEquals(originalView, concurrent.ToJson()))
                throw new InvalidOperationException("Concurrent protocol round trip changed content");
        })));
        Check(true, "metadata cache supports concurrent independent serializer operations");

        var transport = new RecordingTransport();
        var client = new FiccApiClient(transport);
        QuoteWriteData result = await client.CreateQuotesAsync(new[] { new QuoteCreateItem { SecurityId = "240210.IB", Price = "2.5000" } });
        Check(transport.Topic == SalesTradingApi.QuoteCreate.Topic, "typed create endpoint");
        Check((string)transport.Body["quotes"][0]["price"] == "2.5000", "decimal precision retained");
        Check(!string.IsNullOrEmpty((string)transport.Body["quotes"][0]["clientQuoteId"]), "client quote identity assigned");
        Check(result.Results[0].Revision == "9007199254740993", "large revision retained");
        transport.PartialFailure = true;
        result = await client.CreateQuotesAsync(new[] { new QuoteCreateItem { SecurityId = "240210.IB" } });
        bool rejected = false;
        try { FiccApiClient.RequireQuoteResults(result); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "batch item failure is not mistaken for success");

        transport.WrongCorrelation = true;
        rejected = false;
        try { await client.RequestAsync(SalesTradingApi.QuoteStreamOpen, new Request()); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "request correlation mismatch rejected");

        JObject legacy = JObject.Parse("{\"Code\":\"0000\",\"datas\":[{\"taglist\":\"A,B\",\"instcode\":\"G001\"}]}");
        Equal(legacy, ProtocolCodec.Decode<CrmLegacyData>(legacy).ToJson(), "legacy CRM flat reply");
        JObject parser = JObject.Parse("{\"Code\":\"0000\",\"Datas\":[{\"price\":2.5,\"qty\":1000,\"institutionId\":null}]}");
        Equal(parser, ProtocolCodec.Decode<ParserReply>(parser).ToJson(), "ParserSvr Datas reply");

        QuoteSource firstConversation = ProtocolCodec.Decode<QuoteSource>(JObject.Parse(
            "{\"type\":\"QT\",\"externalQuoteId\":\"INTENTION-1\",\"negotiationId\":\"A-B\",\"sourceUserId\":\"QT-A\"}"));
        QuoteSource secondConversation = ProtocolCodec.Decode<QuoteSource>(JObject.Parse(
            "{\"type\":\"QT\",\"externalQuoteId\":\"INTENTION-1\",\"negotiationId\":\"A-C\",\"sourceUserId\":\"QT-A\"}"));
        Check(firstConversation.ExternalQuoteId == secondConversation.ExternalQuoteId
            && firstConversation.NegotiationId != secondConversation.NegotiationId,
            "one intention retains distinct bilateral conversations in typed client models");
        Check((string)firstConversation.ToJson()["negotiationId"] == "A-B",
            "conversation identity survives client serialization");

        JArray captured = JArray.Parse(File.ReadAllText(fixturePath));
        if (captured.OfType<JObject>().Any(row => (string)row["request"]?["viewId"] == "quote-A"))
        {
            await CheckNegotiationPages(captured);
        }
    }

    private static void CheckMarketParticipants()
    {
        Type mapper = typeof(DpsSalesTradingServices).Assembly.GetType("STC.SalesTrading.Plugin.Ficc.DpsModelMapper");
        var users = new[]
        {
            new STC.Common.Model.Admin.StcTbookUsers { Userid = "A", Username = "alice", Name = "本方甲" },
            new STC.Common.Model.Admin.StcTbookUsers { Userid = "B", Username = "bob", Name = "本方乙" }
        };
        var names = (Dictionary<string, string>)mapper.GetMethod("UserNames").Invoke(null,
            new object[] { users, "A", "本方甲", "alice" });
        Check(names["A"] == "本方甲" && names["B"] == "本方乙", "STC display names indexed by userid, not login name");
        JObject book = JObject.Parse("{\"securityId\":\"240210.IB\",\"updateTime\":\"2026-10-12T10:00:00+08:00\",\"quoteRefs\":{\"FICC:Q\":{\"quoteId\":\"Q\",\"quoteRevision\":\"3\"}}}");
        JObject entry = JObject.Parse("{\"MDEntryID\":\"FICC:Q\",\"MDEntryType\":\"0\",\"MDEntryPx\":\"2.5\",\"MDEntrySize\":\"1000\",\"SettlType\":\"2\",\"UserID\":\"A\",\"InstitutionID\":\"I\",\"InstitutionName\":\"对手机构\",\"TraderID\":\"C\",\"TraderName\":\"对手丙\",\"ExerciseFlag\":\"0\"}");
        MarketEntry typedEntry = ProtocolCodec.Decode<MarketEntry>(entry);
        Check(typedEntry.UserID == "A" && typedEntry.InstitutionName == "对手机构" && typedEntry.TraderName == "对手丙",
            "typed market protocol carries participant names and owner id");
        Equal(entry, typedEntry.ToJson(), "participant protocol round trip");
        Func<JObject, STC.SalesTrading.Plugin.Abstractions.Models.BoardQuoteRow> mapRow = item =>
        {
            JObject quote = (JObject)mapper.GetMethod("MarketQuote").Invoke(null, new object[] { book, item });
            mapper.GetMethod("ApplyOwnerDisplay").Invoke(null, new object[] { quote, names, "A" });
            JObject display = (JObject)mapper.GetMethod("Display").Invoke(null, new object[] { quote });
            Check((string)display["InstitutionId"] == (string)item["InstitutionID"]
                && (string)display["ContactId"] == (string)item["TraderID"], "participant ids survive projection");
            return display.ToObject<STC.SalesTrading.Plugin.Abstractions.Models.BoardQuoteRow>();
        };
        var row = mapRow(entry);
        Check(row.Institution == "对手机构" && row.Trader == "对手丙" && row.WallTrader == "本方甲",
            "ALL grid preserves counterparty institution, trader and STC owner separately");
        Check(row.IsMine && row.OwnerAccountId == "A" && row.QuoteId == "Q" && row.Revision == 3,
            "owner id and quote reference retained");
        Check(row.PriceType == STC.SalesTrading.Plugin.Abstractions.Models.PriceType.YieldToExercise,
            "market entry retains exercise yield basis");
        var detail = (STC.SalesTrading.Plugin.Abstractions.Models.BestQuoteDetail)typeof(DpsSalesTradingServices)
            .GetMethod("BestDetail", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { row });
        Check(detail.Institution == row.Institution && detail.Trader == row.Trader
            && detail.OwnerTrader == row.WallTrader && detail.IsMine, "BEST popup preserves all participant columns");
        entry["UserID"] = "B";
        foreach (string field in new[] { "InstitutionID", "InstitutionName", "TraderID", "TraderName" })
        {
            entry.Remove(field);
        }
        row = mapRow(entry);
        Check(row.WallTrader == "本方乙" && !row.IsMine && row.Institution == null && row.Trader == null,
            "another owner's public quote shows its actual owner without reconstructing hidden counterparty");
        entry["UserID"] = "UNKNOWN";
        row = mapRow(entry);
        Check(row.WallTrader == "UNKNOWN" && !row.IsMine, "unknown owner stays identifiable, never replaced with current user");
        entry.Remove("UserID");
        row = mapRow(entry);
        Check(string.IsNullOrEmpty(row.WallTrader) && !row.IsMine, "older payload without owner does not invent one");
        var ownOnly = (Dictionary<string, string>)mapper.GetMethod("UserNames").Invoke(null,
            new object[] { null, "A", "本方甲", "alice" });
        Check(ownOnly["A"] == "本方甲", "login name data works without AdminSvr cache");
    }

    // Run the real client page/update code against captured Java responses and pushes.
    // This deliberately does not claim to test DPS transport or rendered desktop controls.
    private static async Task CheckNegotiationPages(JArray fixtures)
    {
        var pages = new Dictionary<string, LivePage>();
        var transports = new Dictionary<string, CapturedViewTransport>();
        try
        {
            foreach (string user in new[] { "A", "B", "C" })
            foreach (string domain in new[] { "quote", "price" })
            {
                string key = domain + "-" + user;
                var transport = new CapturedViewTransport(fixtures, user, domain);
                var page = new LivePage(new FiccApiClient(transport), transport, domain)
                {
                    PriceMode = "FULL", QuoteStatuses = new JArray("ACTIVE")
                };
                page.Failed += reason => { throw new InvalidOperationException(reason); };
                pages.Add(key, page);
                transports.Add(key, transport);
                await page.OpenAsync(new JObject());
            }
            foreach (JObject fixture in fixtures.OfType<JObject>())
            {
                JObject message = fixture["message"] as JObject;
                if (message == null) continue;
                // The subsequent REFER/confirm pair is covered by Java's replacement checks.
                // Stop before it: this replay needs no synthetic response to automatic page refills.
                if ((message["events"] as JArray)?.OfType<JObject>()
                    .Any(item => (string)item["action"] == "REFER") == true) break;
                foreach (CapturedViewTransport transport in transports.Values)
                    transport.Deliver((string)fixture["topic"], message);
                foreach (JObject quote in (message["quotes"] as JArray ?? new JArray()).OfType<JObject>())
                {
                    if ((string)quote["status"] != "PENDING_CONFIRM") continue;
                    string user = (string)quote["ownerUserId"];
                    string id = (string)quote["quoteId"];
                    Check(pages["quote-" + user].Rows.All(row => (string)row["quoteId"] != id),
                        "pending report never inserts into client's confirmed quote page");
                    Check(pages["price-" + user].Rows.All(row =>
                        ((JObject)row["quoteRefs"]).Properties().All(reference =>
                            (string)reference.Value["quoteId"] != id)),
                        "pending report never inserts into client's price page");
                }
            }
            foreach (string user in new[] { "A", "B", "C" })
            {
                LivePage quotes = pages["quote-" + user];
                LivePage prices = pages["price-" + user];
                Check(quotes.IsReady && prices.IsReady, user + " client streams remain synchronized");
                Check(quotes.Rows.Count == (user == "A" ? 2 : 1)
                    && quotes.Rows.All(row => (string)row["ownerUserId"] == user),
                    user + " client confirmed rows contain only own conversations");
                var quoteIds = new HashSet<string>(quotes.Rows.Select(row => (string)row["quoteId"]));
                var priceIds = new HashSet<string>(prices.Rows.SelectMany(row =>
                    ((JObject)row["quoteRefs"]).Properties().Select(reference => (string)reference.Value["quoteId"])));
                Check(quoteIds.SetEquals(priceIds), user + " client price rows match own confirmed quotes");
            }
        }
        finally
        {
            foreach (LivePage page in pages.Values) page.Dispose();
        }
    }

    private sealed class CapturedViewTransport : IFiccTransport, IDisposable
    {
        private readonly JArray fixtures;
        private readonly string user;
        private readonly string domain;
        private string viewId;
        private string topic;
        private Action<JObject> receive;
        public event Action ConnectionChanged { add { } remove { } }

        public CapturedViewTransport(JArray fixtures, string user, string domain)
        {
            this.fixtures = fixtures;
            this.user = user;
            this.domain = domain;
        }

        public Task<JObject> RequestAsync(string service, string requestedTopic, JObject request)
        {
            JObject fixture = fixtures.OfType<JObject>().First(row => (string)row["topic"] == requestedTopic
                && (requestedTopic.EndsWith("stream.open")
                    ? (string)row["reply"]?["Data"]?["sessionId"] == "SESSION-" + user
                    : (string)row["request"]?["viewId"] == domain + "-" + user));
            JObject reply = (JObject)fixture["reply"].DeepClone();
            reply["requestId"] = request["requestId"].DeepClone();
            if (requestedTopic.EndsWith("view.open"))
            {
                viewId = (string)request["viewId"];
                Remap(reply);
            }
            return Task.FromResult(reply);
        }

        public IDisposable Subscribe(string service, string subscribedTopic, Action<JObject> callback, bool withImage = false)
        {
            topic = subscribedTopic;
            receive = callback;
            return this;
        }

        public void Deliver(string incomingTopic, JObject original)
        {
            if (incomingTopic != topic) return;
            JObject copy = (JObject)original.DeepClone();
            Remap(copy);
            receive(copy);
        }

        private void Remap(JObject message)
        {
            foreach (JObject item in message.DescendantsAndSelf().OfType<JObject>())
                if ((string)item["viewId"] == domain + "-" + user) item["viewId"] = viewId;
        }

        public void Dispose() { receive = null; }
    }

    private static void Equal(JToken expected, JToken actual, string name)
    {
        if (!JToken.DeepEquals(expected, actual))
        {
            throw new Exception(name + " mismatch\nexpected=" + expected + "\nactual=" + actual);
        }
        passed++;
    }

    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        passed++;
    }

    private sealed class RecordingTransport : IFiccTransport
    {
        public string Topic;
        public JObject Body;
        public bool PartialFailure;
        public bool WrongCorrelation;
        public event Action ConnectionChanged { add { } remove { } }

        public Task<JObject> RequestAsync(string service, string topic, JObject request)
        {
            Topic = topic;
            Body = request;
            return Task.FromResult(new JObject
            {
                ["Code"] = "0000",
                ["requestId"] = WrongCorrelation ? "wrong" : (string)request["requestId"],
                ["Data"] = new JObject { ["results"] = new JArray(new JObject
                {
                    ["Code"] = PartialFailure ? "1001" : "0000",
                    ["Msg"] = "test",
                    ["quoteId"] = "Q",
                    ["revision"] = "9007199254740993"
                }) }
            });
        }

        public IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false)
        {
            throw new NotSupportedException();
        }
    }
}
