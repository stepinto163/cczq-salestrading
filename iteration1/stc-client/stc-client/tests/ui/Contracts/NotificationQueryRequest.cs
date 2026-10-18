using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>NotificationQueryRequest</summary>
    public class NotificationQueryRequest : PageRequest
    {
        [JsonProperty("readState")]
        public string ReadState { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("filter")]
        public Filter Filter { get; set; }

    }
}
