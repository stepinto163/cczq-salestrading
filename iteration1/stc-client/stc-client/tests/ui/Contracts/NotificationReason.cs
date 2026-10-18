using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>NotificationReason</summary>
    public class NotificationReason : ProtocolModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("groupIds")]
        public List<string> GroupIds { get; set; }

        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }

        [JsonProperty("ownQuoteId")]
        public string OwnQuoteId { get; set; }

        [JsonProperty("otherQuoteId")]
        public string OtherQuoteId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

    }
}
