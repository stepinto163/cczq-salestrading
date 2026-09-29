using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceQueryRequest</summary>
    public class PriceQueryRequest : PageRequest
    {
        [JsonProperty("securityIds")]
        public List<string> SecurityIds { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("depth")]
        public int? Depth { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
