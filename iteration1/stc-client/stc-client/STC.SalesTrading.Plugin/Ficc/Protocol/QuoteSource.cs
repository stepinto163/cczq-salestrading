using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteSource</summary>
    public class QuoteSource : ProtocolModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("originService")]
        public string OriginService { get; set; }

        [JsonProperty("sourceUserId")]
        public string SourceUserId { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("externalMessageId")]
        public string ExternalMessageId { get; set; }

        [JsonProperty("originalMessageId")]
        public string OriginalMessageId { get; set; }

        // Sender's QTrade intention ID; -10000 means unavailable on a receiver message.
        [JsonProperty("externalQuoteId")]
        public string ExternalQuoteId { get; set; }

        // Bilateral conversation ID. One intention can have multiple conversations.
        [JsonProperty("negotiationId")]
        public string NegotiationId { get; set; }

        [JsonProperty("counterpartyId")]
        public string CounterpartyId { get; set; }

        [JsonProperty("chatType")]
        public string ChatType { get; set; }

        [JsonProperty("chatGroupId")]
        public string ChatGroupId { get; set; }

        [JsonProperty("externalRevision")]
        public string ExternalRevision { get; set; }

        [JsonProperty("sourceTime")]
        public string SourceTime { get; set; }

        [JsonProperty("transType")]
        public string TransType { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

    }
}
