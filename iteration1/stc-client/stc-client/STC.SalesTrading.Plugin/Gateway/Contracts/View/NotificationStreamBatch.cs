using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>ficc.notification.report 推送批次（S02）。preference 变化发送完整对象。</summary>
    public class NotificationStreamBatch : StreamMessage
    {
        [JsonProperty("notifications")]
        public List<Notification.NotificationRecord> Notifications { get; set; }

        [JsonProperty("preference")]
        public Notification.NotificationPreference Preference { get; set; }
    }

}
