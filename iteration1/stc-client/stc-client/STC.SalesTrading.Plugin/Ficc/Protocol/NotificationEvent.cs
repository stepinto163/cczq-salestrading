using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationEvent</summary>
    public class NotificationEvent : ProtocolModel
    {
        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("record")]
        public NotificationEventRecord Record { get; set; }

    }
}
