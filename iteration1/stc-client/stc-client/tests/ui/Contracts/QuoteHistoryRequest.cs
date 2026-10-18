using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteHistoryRequest</summary>
    public class QuoteHistoryRequest : PageRequest
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
