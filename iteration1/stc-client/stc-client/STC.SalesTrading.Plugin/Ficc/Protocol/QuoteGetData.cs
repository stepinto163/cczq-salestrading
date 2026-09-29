using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteGetData</summary>
    public class QuoteGetData : ProtocolModel
    {
        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

    }
}
