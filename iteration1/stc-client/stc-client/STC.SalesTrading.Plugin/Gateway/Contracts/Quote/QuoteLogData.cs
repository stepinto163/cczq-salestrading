using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{
    /// <summary>ficc.quote.history.query 应答 Data（H10）。nextCursor 非空可续取。</summary>
    public class QuoteLogData
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("records")]
        public List<QuoteLogRecord> Records { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }
    }
}
