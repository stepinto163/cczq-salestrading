using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QTrade 返回的逐目标发布结果。</summary>
    public sealed class QTradePublishRecipientResult : ProtocolModel
    {
        [JsonProperty("routingType")]
        public int? RoutingType { get; set; }

        [JsonProperty("routingId")]
        public string RoutingId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("errorType")]
        public int? ErrorType { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

    }
}
