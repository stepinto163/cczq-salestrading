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
