using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteHistoryData</summary>
    public class QuoteHistoryData : PageData
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("records")]
        public List<QuoteHistoryRecord> Records { get; set; }

    }
}
