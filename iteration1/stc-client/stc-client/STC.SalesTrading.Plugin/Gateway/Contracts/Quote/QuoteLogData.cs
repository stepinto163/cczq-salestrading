using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>ficc.quote.history.query 应答 Data（H10）。按页返回，snapshotRevision 固定本次浏览的版本边界。</summary>
    public class QuoteLogData
    {
        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 20;

        [JsonProperty("snapshotRevision")]
        public string SnapshotRevision { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("records")]
        public List<QuoteLogRecord> Records { get; set; }

    }
}
