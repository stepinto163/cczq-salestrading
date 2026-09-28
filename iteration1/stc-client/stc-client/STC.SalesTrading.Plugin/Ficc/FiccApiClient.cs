using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.Ficc
{
    public sealed class FiccApiClient
    {
        private readonly IFiccTransport transport;

        public FiccApiClient(IFiccTransport transport)
        {
            this.transport = transport;
        }

        public static string ServiceFor(string topic)
        {
            return topic.StartsWith("ficc.notification.", StringComparison.Ordinal)
                ? "NotificationSvr" : "QuoteSvr";
        }

        public async Task<JObject> RequestAsync(string topic, JObject request)
        {
            JObject body = (JObject)request.DeepClone();
            if (body["requestId"] == null)
            {
                body["requestId"] = "STC-" + Guid.NewGuid().ToString("N");
            }
            JObject reply = await transport.RequestAsync(ServiceFor(topic), topic, body);
            if ((string)reply["Code"] != "0000")
            {
                throw new InvalidOperationException((string)reply["Code"] + ": " + (string)reply["Msg"]);
            }
            if ((string)reply["requestId"] != (string)body["requestId"])
            {
                throw new InvalidOperationException("请求与应答的 requestId 不一致。");
            }
            return reply["Data"] as JObject ?? new JObject();
        }

        public Task<JObject> CreateAsync(IEnumerable<JObject> quotes)
        {
            JArray requests = new JArray();
            foreach (JObject quote in quotes)
            {
                JObject item = (JObject)quote.DeepClone();
                if (string.IsNullOrWhiteSpace((string)item["clientQuoteId"]))
                {
                    item["clientQuoteId"] = Guid.NewGuid().ToString("N");
                }
                requests.Add(item);
            }
            return RequestAsync("ficc.quote.create", new JObject { ["quotes"] = requests });
        }

        public async Task<JObject> GetQuoteAsync(string quoteId)
        {
            JObject data = await RequestAsync("ficc.quote.get", new JObject { ["quoteId"] = quoteId });
            return data["quote"] as JObject;
        }

        public Task<JObject> OperateAsync(string operation, IEnumerable<JObject> displayedQuotes, JObject changes = null)
        {
            string[] operations = { "confirm", "update", "refer", "unrefer", "delete" };
            if (!operations.Contains(operation))
            {
                throw new ArgumentException("不支持的报价操作。", nameof(operation));
            }
            JArray quotes = new JArray();
            foreach (JObject displayedQuote in displayedQuotes)
            {
                if (displayedQuote["quoteId"] == null || displayedQuote["revision"] == null)
                {
                    throw new InvalidOperationException("必须选择已读取的报价及其 revision。");
                }
                JObject item = changes == null ? new JObject() : (JObject)changes.DeepClone();
                item["quoteId"] = displayedQuote["quoteId"].DeepClone();
                item["expectedRevision"] = displayedQuote["revision"].DeepClone();
                quotes.Add(item);
            }
            if (quotes.Count == 0)
            {
                throw new InvalidOperationException("请先选择报价。");
            }
            return RequestAsync("ficc.quote." + operation, new JObject { ["quotes"] = quotes });
        }

        public static JArray RequireQuoteResults(JObject response)
        {
            JArray results = response["results"] as JArray;
            if (results == null || results.Count == 0)
            {
                throw new InvalidOperationException("服务未返回逐条报价操作结果。");
            }
            string[] errors = results.OfType<JObject>()
                .Where(item => (string)item["Code"] != "0000")
                .Select(item => ((string)item["quoteId"] ?? (string)item["clientQuoteId"]) + ": " + (string)item["Msg"])
                .ToArray();
            if (errors.Length > 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            }
            return results;
        }
    }
}
