using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public class RecommendationEvidence : ProtocolModel
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

    }
}
