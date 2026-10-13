using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>业务发布回执；ACKNOWLEDGED 才表示 QTrade 全部受理。</summary>
    public sealed class QTradePublishData : ProtocolModel
    {
        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("applicationCode")]
        public string ApplicationCode { get; set; }

        [JsonProperty("fixSequence")]
        public int? FixSequence { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("recipients")]
        public List<QTradePublishRecipientResult> Recipients { get; set; }

    }
}
