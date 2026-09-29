using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>IntakeQuoteItem</summary>
    public class IntakeQuoteItem : QuoteInput
    {
        [JsonProperty("externalQuoteId")]
        public string ExternalQuoteId { get; set; }

        [JsonProperty("externalRevision")]
        public string ExternalRevision { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

    }
}
