using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>BaseData</summary>
    public class BaseData : ProtocolModel
    {
        [JsonProperty("records")]
        public List<ReferenceRecord> Records { get; set; }

        [JsonProperty("hasMore")]
        public bool? HasMore { get; set; }

        [JsonProperty("nextOffset")]
        public int? NextOffset { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

    }
}
