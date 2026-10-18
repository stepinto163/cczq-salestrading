using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation
{
    /// <summary>智能推荐查询请求（ficc.recommendation.query）：窗口起止，yyyy-MM-dd。</summary>
    public class RecommendationQueryRequest
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }
    }
}
