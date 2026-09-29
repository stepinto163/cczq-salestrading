using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationReadResult</summary>
    public class NotificationReadResult : ProtocolModel
    {
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("readTime")]
        public string ReadTime { get; set; }

    }
}
