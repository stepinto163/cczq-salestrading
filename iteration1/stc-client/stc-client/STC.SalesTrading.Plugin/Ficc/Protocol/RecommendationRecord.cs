using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public class RecommendationRecord : ProtocolModel
    {
        [JsonProperty("recommendationId")]
        public string RecommendationId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("score")]
        public double? Score { get; set; }

        [JsonProperty("quoteCount")]
        public int? QuoteCount { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("latestPrice")]
        public string LatestPrice { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        [JsonProperty("latestQuantity")]
        public string LatestQuantity { get; set; }

        [JsonProperty("latestTime")]
        public string LatestTime { get; set; }

        [JsonProperty("feedback")]
        public string Feedback { get; set; }

        [JsonProperty("evidence")]
        public List<RecommendationEvidence> Evidence { get; set; }

    }
}
