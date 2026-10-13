using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public class RecommendationQueryRequest : Request
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

    }
}
