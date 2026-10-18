using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>ficc.quote.history.query 请求体（H10）：单条报价变更历史（区别于跨日期历史搜索 H11）。</summary>
    public class QuoteLogRequest
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
