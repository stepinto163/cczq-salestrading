using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceStreamBatch</summary>
    public class PriceStreamBatch : StreamMessage
    {
        [JsonProperty("books")]
        public List<PriceBook> Books { get; set; }

        [JsonProperty("views")]
        public List<ViewDelta> Views { get; set; }

    }
}
