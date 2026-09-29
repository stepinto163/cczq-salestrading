using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ViewDelta</summary>
    public class ViewDelta : ProtocolModel
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("upsertIds")]
        public List<string> UpsertIds { get; set; }

        [JsonProperty("removeIds")]
        public List<string> RemoveIds { get; set; }

    }
}
