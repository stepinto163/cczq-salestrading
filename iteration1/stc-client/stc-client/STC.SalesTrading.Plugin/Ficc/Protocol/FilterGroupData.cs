using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>FilterGroupData</summary>
    public class FilterGroupData : ProtocolModel
    {
        [JsonProperty("filterGroup")]
        public FilterGroup FilterGroup { get; set; }

        [JsonProperty("filterGroups")]
        public List<FilterGroup> FilterGroups { get; set; }

        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("deleted")]
        public bool? Deleted { get; set; }

    }
}
