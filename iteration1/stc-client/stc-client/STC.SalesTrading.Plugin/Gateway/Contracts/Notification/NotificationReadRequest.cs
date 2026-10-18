using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>ficc.notification.read 请求体（H30）。显式 ID 数组，避免"全部已读"误覆盖。</summary>
    public class NotificationReadRequest
    {
        [JsonProperty("notificationIds")]
        public List<string> NotificationIds { get; set; }

        /// <summary>true 写入 readTime，false 清空。</summary>
        [JsonProperty("read")]
        public bool Read { get; set; }
    }

}
