using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteReference</summary>
    public class QuoteReference : ProtocolModel
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("quoteRevision")]
        public string QuoteRevision { get; set; }

    }
}
