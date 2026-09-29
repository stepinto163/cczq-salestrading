using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteViewOpenRequest</summary>
    public class QuoteViewOpenRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("ownerScope")]
        public string OwnerScope { get; set; }

        [JsonProperty("status")]
        public List<string> Status { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
