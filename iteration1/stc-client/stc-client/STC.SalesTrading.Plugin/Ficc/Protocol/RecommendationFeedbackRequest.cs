using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public class RecommendationFeedbackRequest : RecommendationQueryRequest
    {
        [JsonProperty("recommendationId")]
        public string RecommendationId { get; set; }

        [JsonProperty("feedback")]
        public string Feedback { get; set; }

    }
}
