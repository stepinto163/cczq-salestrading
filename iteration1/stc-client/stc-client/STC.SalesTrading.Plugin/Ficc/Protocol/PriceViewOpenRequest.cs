using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PriceViewOpenRequest</summary>
    public class PriceViewOpenRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("depth")]
        public int? Depth { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
