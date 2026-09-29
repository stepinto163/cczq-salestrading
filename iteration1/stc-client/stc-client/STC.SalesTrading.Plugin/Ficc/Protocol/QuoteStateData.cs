using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteStateData</summary>
    public class QuoteStateData : ProtocolModel
    {
        [JsonProperty("epoch")]
        public string Epoch { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("records")]
        public QuoteStateRecords Records { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        [JsonProperty("done")]
        public bool? Done { get; set; }

    }
}
