using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>QuoteHistoryRecord</summary>
    public class QuoteHistoryRecord : ProtocolModel
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("operatorUserId")]
        public string OperatorUserId { get; set; }

        [JsonProperty("operationTime")]
        public string OperationTime { get; set; }

        [JsonProperty("before")]
        public QuoteRecord Before { get; set; }

        [JsonProperty("after")]
        public QuoteRecord After { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

    }
}
