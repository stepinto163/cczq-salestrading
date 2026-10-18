using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteCreateItem</summary>
    public class QuoteCreateItem : QuoteInput
    {
        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

    }
}
