using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteHistoryRow</summary>
    public class QuoteHistoryRow : ProtocolModel
    {
        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

        [JsonProperty("reference")]
        public ReferenceRecord Reference { get; set; }

    }
}
