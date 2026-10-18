using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>明确的单聊或群聊接收目标。</summary>
    public sealed class QTradePublishRecipient : ProtocolModel
    {
        [JsonProperty("routingType")]
        public int? RoutingType { get; set; }

        [JsonProperty("routingId")]
        public string RoutingId { get; set; }

    }
}
