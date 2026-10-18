using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>意向发布请求；与 Java QTradePublishRequest 对应。</summary>
    public sealed class QTradePublishRequest : Request
    {
        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("senderQtAccount")]
        public string SenderQtAccount { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("preText")]
        public string PreText { get; set; }

        [JsonProperty("postText")]
        public string PostText { get; set; }

        [JsonProperty("recipients")]
        public List<QTradePublishRecipient> Recipients { get; set; }

        [JsonProperty("quotes")]
        public List<QTradePublishQuote> Quotes { get; set; }

    }
}
