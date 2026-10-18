using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
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
