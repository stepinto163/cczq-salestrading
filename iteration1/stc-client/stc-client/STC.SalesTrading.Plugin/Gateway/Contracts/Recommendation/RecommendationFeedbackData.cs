using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation
{
    /// <summary>反馈应答 Data（回显最终状态）。</summary>
    public class RecommendationFeedbackData
    {
        [JsonProperty("recommendationId")]
        public string RecommendationId { get; set; }

        [JsonProperty("feedback")]
        public string Feedback { get; set; }
    }
}
