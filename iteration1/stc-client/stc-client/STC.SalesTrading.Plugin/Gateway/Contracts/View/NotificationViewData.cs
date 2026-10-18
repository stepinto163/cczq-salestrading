using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>notification 域 view.open / view.query 应答 Data（H26/H27）。</summary>
    public class NotificationViewData
    {
        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("view")]
        public ViewPage View { get; set; }

        [JsonProperty("notifications")]
        public List<Notification.NotificationRecord> Notifications { get; set; }
    }

}
