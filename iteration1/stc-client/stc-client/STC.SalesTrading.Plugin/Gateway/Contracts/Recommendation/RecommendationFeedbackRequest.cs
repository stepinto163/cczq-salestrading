using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation
{
    /// <summary>推荐反馈提交（ficc.recommendation.feedback）：USEFUL / NOT_USEFUL / NOT_APPLICABLE。</summary>
    public class RecommendationFeedbackRequest
    {
        [JsonProperty("recommendationId")]
        public string RecommendationId { get; set; }

        [JsonProperty("feedback")]
        public string Feedback { get; set; }
    }
}
