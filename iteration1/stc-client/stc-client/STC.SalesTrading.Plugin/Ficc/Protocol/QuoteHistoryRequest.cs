using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteHistoryRequest</summary>
    public class QuoteHistoryRequest : PageRequest
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
