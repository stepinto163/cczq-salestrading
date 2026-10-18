using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>ficc.notification.read 单项结果（H30）。</summary>
    public class NotificationReadResult
    {
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("readTime")]
        public DateTime? ReadTime { get; set; }
    }

}
