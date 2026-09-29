using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteCreateItem</summary>
    public class QuoteCreateItem : QuoteInput
    {
        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

    }
}
