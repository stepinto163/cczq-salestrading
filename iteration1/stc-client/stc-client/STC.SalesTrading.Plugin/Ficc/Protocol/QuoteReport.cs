using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteReport</summary>
    public class QuoteReport : ProtocolModel
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }

        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [JsonProperty("operatorUserId")]
        public string OperatorUserId { get; set; }

        [JsonProperty("operationTime")]
        public string OperationTime { get; set; }

    }
}
