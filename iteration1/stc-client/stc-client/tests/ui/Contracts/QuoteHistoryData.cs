using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteHistoryData</summary>
    public class QuoteHistoryData : PageData
    {
        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 20;

        [JsonProperty("snapshotRevision")]
        public string SnapshotRevision { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("records")]
        public List<QuoteHistoryRecord> Records { get; set; }

    }
}
