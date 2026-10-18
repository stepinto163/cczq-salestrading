using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>ficc.quote.history.query 请求体（H10）：单条报价变更历史（区别于跨日期历史搜索 H11）。</summary>
    public class QuoteLogRequest
    {
        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 20;

        [JsonProperty("snapshotRevision")]
        public string SnapshotRevision { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
