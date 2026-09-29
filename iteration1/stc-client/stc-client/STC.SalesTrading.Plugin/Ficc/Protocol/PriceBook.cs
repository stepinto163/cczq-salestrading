using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceBook</summary>
    public class PriceBook : ProtocolModel
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("bookRevision")]
        public string BookRevision { get; set; }

        [JsonProperty("marketData")]
        public MarketData MarketData { get; set; }

        [JsonProperty("quoteRefs")]
        public Dictionary<string,QuoteReference> QuoteRefs { get; set; }

    }
}
