using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>NotificationRecord</summary>
    public class NotificationTrigger : ProtocolModel
    {
        [JsonProperty("alertKey")]
        public string AlertKey { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }
    }

    public class NotificationRecord : ProtocolModel
    {
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("contentToken")]
        public string ContentToken { get; set; }

        [JsonProperty("alertKey")]
        public string AlertKey { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("readTime")]
        public string ReadTime { get; set; }

        [JsonProperty("reasons")]
        public List<NotificationReason> Reasons { get; set; }

        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }

        [JsonProperty("triggers")]
        public List<NotificationTrigger> Triggers { get; set; }
    }
}
