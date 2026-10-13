using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public class RecommendationQueryData : ProtocolModel
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("scoringVersion")]
        public string ScoringVersion { get; set; }

        [JsonProperty("recommendations")]
        public List<RecommendationRecord> Recommendations { get; set; }

    }
}
