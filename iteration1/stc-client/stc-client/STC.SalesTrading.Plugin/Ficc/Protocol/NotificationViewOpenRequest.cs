using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationViewOpenRequest</summary>
    public class NotificationViewOpenRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("readState")]
        public string ReadState { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
