using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>BaseQueryRequest</summary>
    public class BaseQueryRequest : PageRequest
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }

    }
}
