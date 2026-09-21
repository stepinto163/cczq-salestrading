using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ficc.SalesTrading;

namespace Ficc.FrontendLearning
{
    // 阅读入口：业务请求只依赖 DPS，不依赖窗体控件。
    public sealed class QuoteExamples
    {
        private readonly DpsClient client;

        public QuoteExamples(DpsClient client)
        {
            this.client = client;
        }

        public Task<JObject> Request(string topic, JObject body)
        {
            // DPS 同步 SDK 放在线程池，避免阻塞 UI。
            return Task.Run(() => client.Data(topic, body));
        }

        public Task<JObject> Create(string securityId, string price)
        {
            return Request("ficc.quote.create", new JObject
            {
                ["quotes"] = new JArray(new JObject
                {
                    ["clientQuoteId"] = Guid.NewGuid().ToString("N"),
                    ["securityId"] = securityId,
                    ["side"] = "1",
                    ["quoteType"] = "1",
                    ["yieldType"] = "1",
                    ["price"] = price,
                    ["quantity"] = "1000",
                    ["quantityUnit"] = "WAN",
                    ["currency"] = "CNY",
                    ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
                    ["settlType"] = "2"
                })
            });
        }

        public async Task<JObject> Get(string quoteId)
        {
            var result = await Request("ficc.quote.get", new JObject { ["quoteId"] = quoteId });
            return (JObject)result["quote"];
        }

        // 编辑时先 Get，界面保留这次读取的 revision；不能提交前再取新 revision 掩盖冲突。
        public Task<JObject> Operate(string operation, JObject displayedQuote, JObject fields = null)
        {
            if (!new[] { "confirm", "update", "refer", "unrefer", "delete" }.Contains(operation))
                throw new ArgumentException("Unsupported quote operation");
            if (displayedQuote == null)
                throw new InvalidOperationException("请先读取报价，或创建一条新报价。不能使用空的 revision。");

            var quote = fields == null ? new JObject() : (JObject)fields.DeepClone();
            quote["quoteId"] = displayedQuote["quoteId"].DeepClone();
            quote["expectedRevision"] = displayedQuote["revision"].DeepClone();
            return Request("ficc.quote." + operation, new JObject { ["quotes"] = new JArray(quote) });
        }

        // 外层 Code=0000 只代表请求被处理，必须检查每条报价的结果。
        public static JObject RequireSuccess(JObject reply)
        {
            var result = (JObject)reply["results"]?[0];
            if (result == null || (string)result["Code"] != "0000")
                throw new InvalidOperationException(result?.ToString() ?? "Missing quote result");
            return result;
        }

        public Task<JObject> SearchInstitutions(string text)
        {
            return Request("ficc.base.query", new JObject
            {
                ["kind"] = "INSTITUTION",
                ["text"] = text,
                ["offset"] = 0,
                ["limit"] = 20
            });
        }

        public Task<JObject> SearchContacts(string institutionId, string text)
        {
            return Request("ficc.base.query", new JObject
            {
                ["kind"] = "CONTACT",
                ["institutionId"] = institutionId,
                ["text"] = text,
                ["offset"] = 0,
                ["limit"] = 20
            });
        }

        public Task<JObject> History(string quoteId)
        {
            return Request("ficc.quote.history.query", new JObject { ["quoteId"] = quoteId, ["limit"] = 20 });
        }

        public Task<JObject> MarkRead(string notificationId)
        {
            return Request("ficc.notification.read", new JObject
            {
                ["notificationIds"] = new JArray(notificationId),
                ["read"] = true
            });
        }

        public static JObject BondFilter(string securityId)
        {
            var conditions = new JArray();
            if (!string.IsNullOrWhiteSpace(securityId))
                conditions.Add(new JObject
                {
                    ["field"] = "securityId",
                    ["operator"] = "EQ",
                    ["value"] = securityId
                });
            return new JObject { ["conditions"] = conditions };
        }
    }
}
