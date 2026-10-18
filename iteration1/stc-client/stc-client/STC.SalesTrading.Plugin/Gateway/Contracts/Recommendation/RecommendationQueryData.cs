using Newtonsoft.Json;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation
{
    /// <summary>智能推荐查询应答 Data。</summary>
    public class RecommendationQueryData
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
