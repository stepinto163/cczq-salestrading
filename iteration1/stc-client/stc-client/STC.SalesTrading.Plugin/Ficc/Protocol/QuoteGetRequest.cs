using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteGetRequest</summary>
    public class QuoteGetRequest : Request
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

    }
}
