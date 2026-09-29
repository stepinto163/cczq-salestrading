using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;

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

        private async Task<TData> SendAsync<TData>(string topic, JObject request) where TData : ProtocolModel
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
            ApiReply<TData> result = ProtocolCodec.Decode<ApiReply<TData>>(reply);
            return result.Data ?? ProtocolCodec.Decode<TData>(new JObject());
        }

        public async Task<TData> RequestAsync<TRequest, TData>(Endpoint<TRequest, TData> endpoint, TRequest request)
            where TRequest : Request where TData : ProtocolModel
        {
            return await SendAsync<TData>(endpoint.Topic, request.ToJson());
        }

        // Projection for existing dynamic grids/filter editors; callers still choose
        // a compile-time request/result contract, never an untyped topic string.
        public async Task<JObject> RequestProjectionAsync<TRequest, TData>(Endpoint<TRequest, TData> endpoint, TRequest request)
            where TRequest : Request where TData : ProtocolModel
        {
            return (await RequestAsync(endpoint, request)).ToJson();
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
            return CreateAndProjectAsync(requests.OfType<JObject>()
                .Select(ProtocolCodec.Decode<QuoteCreateItem>).ToList());
        }

        public Task<QuoteWriteData> CreateQuotesAsync(IEnumerable<QuoteCreateItem> quotes)
        {
            List<QuoteCreateItem> inputs = quotes.ToList();
            foreach (QuoteCreateItem input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input.ClientQuoteId))
                {
                    input.ClientQuoteId = Guid.NewGuid().ToString("N");
                }
            }
            return RequestAsync(SalesTradingApi.QuoteCreate, new QuoteCreateRequest { Quotes = inputs });
        }

        private async Task<JObject> CreateAndProjectAsync(List<QuoteCreateItem> quotes)
        {
            return (await CreateQuotesAsync(quotes)).ToJson();
        }

        public async Task<QuoteRecord> GetQuoteModelAsync(string quoteId)
        {
            QuoteGetData data = await RequestAsync(SalesTradingApi.QuoteGet, new QuoteGetRequest { QuoteId = quoteId });
            return data.Quote;
        }

        public async Task<JObject> GetQuoteAsync(string quoteId)
        {
            return (await GetQuoteModelAsync(quoteId))?.ToJson();
        }

        public Task<QuoteWriteData> ChangeQuotesAsync(Endpoint<QuoteChangeRequest, QuoteWriteData> endpoint,
            IEnumerable<QuoteChangeItem> quotes)
        {
            return RequestAsync(endpoint, new QuoteChangeRequest { Quotes = quotes.ToList() });
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
            Endpoint<QuoteChangeRequest, QuoteWriteData> endpoint;
            switch (operation)
            {
                case "confirm": endpoint = SalesTradingApi.QuoteConfirm; break;
                case "update": endpoint = SalesTradingApi.QuoteUpdate; break;
                case "refer": endpoint = SalesTradingApi.QuoteRefer; break;
                case "unrefer": endpoint = SalesTradingApi.QuoteUnrefer; break;
                case "delete": endpoint = SalesTradingApi.QuoteDelete; break;
                default: throw new ArgumentException("Unsupported quote operation: " + operation);
            }
            return ChangeAndProjectAsync(endpoint, quotes.OfType<JObject>()
                .Select(ProtocolCodec.Decode<QuoteChangeItem>));
        }

        private async Task<JObject> ChangeAndProjectAsync(Endpoint<QuoteChangeRequest, QuoteWriteData> endpoint,
            IEnumerable<QuoteChangeItem> quotes)
        {
            return (await ChangeQuotesAsync(endpoint, quotes)).ToJson();
        }

        public static IReadOnlyList<QuoteOperationResult> RequireQuoteResults(QuoteWriteData response)
        {
            if (response.Results == null || response.Results.Count == 0)
            {
                throw new InvalidOperationException("服务未返回逐条报价操作结果。");
            }
            string[] errors = response.Results.Where(item => item.Code != "0000")
                .Select(item => (item.QuoteId ?? item.ClientQuoteId) + ": " + item.Msg).ToArray();
            if (errors.Length > 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            }
            return response.Results;
        }

        public static JArray RequireQuoteResults(JObject response)
        {
            RequireQuoteResults(ProtocolCodec.Decode<QuoteWriteData>(response));
            return (JArray)response["results"];
        }
    }
}
