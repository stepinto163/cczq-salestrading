using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteGetRequest</summary>
    public class QuoteGetRequest : Request
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
