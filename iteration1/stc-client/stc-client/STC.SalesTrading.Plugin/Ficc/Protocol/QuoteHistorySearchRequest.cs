using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteHistorySearchRequest</summary>
    public class QuoteHistorySearchRequest : PageRequest
    {
        [JsonProperty("fromDate")]
        public string FromDate { get; set; }

        [JsonProperty("toDate")]
        public string ToDate { get; set; }

        [JsonProperty("ownerScope")]
        public string OwnerScope { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
