using Newtonsoft.Json;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation
{
    /// <summary>推荐条目：机构维度推荐（含证据与反馈状态）。price/quantity 为字符串容错承载。</summary>
    public class RecommendationRecord
    {
        [JsonProperty("recommendationId")]
        public string RecommendationId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        /// <summary>"1"=Bid，其余 Ofr。</summary>
        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("score")]
        public string Score { get; set; }

        [JsonProperty("quoteCount")]
        public string QuoteCount { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("latestPrice")]
        public string LatestPrice { get; set; }

        [JsonProperty("latestQuantity")]
        public string LatestQuantity { get; set; }

        /// <summary>2=净价 / 3=全价；否则按 yieldType（2=行权收益率）。</summary>
        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        [JsonProperty("latestTime")]
        public string LatestTime { get; set; }

        /// <summary>USEFUL / NOT_USEFUL / NOT_APPLICABLE；null=未反馈。</summary>
        [JsonProperty("feedback")]
        public string Feedback { get; set; }

        [JsonProperty("evidence")]
        public List<RecommendationEvidence> Evidence { get; set; }
    }
}
