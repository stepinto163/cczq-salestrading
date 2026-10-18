using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>推送中的 view 变化（S01/S02/S03 的 views[]）。upsertIds 与 removeIds 不重叠。</summary>
    public class ViewDelta
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("removeIds")]
        public List<string> RemoveIds { get; set; }

        [JsonProperty("upsertIds")]
        public List<string> UpsertIds { get; set; }
    }

}
