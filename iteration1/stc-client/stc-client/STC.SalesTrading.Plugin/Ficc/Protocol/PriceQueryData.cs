using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceQueryData</summary>
    public class PriceQueryData : PageData
    {
        [JsonProperty("books")]
        public List<PriceBook> Books { get; set; }

    }
}
