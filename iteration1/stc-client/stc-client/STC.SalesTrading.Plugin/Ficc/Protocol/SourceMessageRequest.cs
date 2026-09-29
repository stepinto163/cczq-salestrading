using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>SourceMessageRequest</summary>
    public class SourceMessageRequest : PageRequest
    {
        [JsonProperty("sourceMessageId")]
        public string SourceMessageId { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
