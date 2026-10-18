using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>ficc.notification.read 应答 Data（H30）。</summary>
    public class NotificationReadData
    {
        [JsonProperty("results")]
        public List<NotificationReadResult> Results { get; set; }
    }

}
