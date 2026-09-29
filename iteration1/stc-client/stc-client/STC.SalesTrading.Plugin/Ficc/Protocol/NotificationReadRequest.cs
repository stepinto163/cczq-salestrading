using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationReadRequest</summary>
    public class NotificationReadRequest : Request
    {
        [JsonProperty("notificationIds")]
        public List<string> NotificationIds { get; set; }

        [JsonProperty("read")]
        public bool? Read { get; set; }

    }
}
