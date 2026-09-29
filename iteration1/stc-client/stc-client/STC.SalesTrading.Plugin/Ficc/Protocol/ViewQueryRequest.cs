using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ViewQueryRequest</summary>
    public class ViewQueryRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageNumber")]
        public int? PageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

    }
}
