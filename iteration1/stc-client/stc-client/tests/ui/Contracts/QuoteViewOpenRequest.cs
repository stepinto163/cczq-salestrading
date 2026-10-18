using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteViewOpenRequest</summary>
    public class QuoteViewOpenRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("ownerScope")]
        public string OwnerScope { get; set; }

        [JsonProperty("status")]
        public List<string> Status { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
