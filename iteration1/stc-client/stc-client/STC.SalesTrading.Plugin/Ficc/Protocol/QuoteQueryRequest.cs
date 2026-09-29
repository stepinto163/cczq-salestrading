using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteQueryRequest</summary>
    public class QuoteQueryRequest : PageRequest
    {
        [JsonProperty("ownerScope")]
        public string OwnerScope { get; set; }

        [JsonProperty("status")]
        public List<string> Status { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
