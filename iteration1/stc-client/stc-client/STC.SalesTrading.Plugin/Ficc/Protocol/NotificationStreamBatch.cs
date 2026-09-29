using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationStreamBatch</summary>
    public class NotificationStreamBatch : StreamMessage
    {
        [JsonProperty("notifications")]
        public List<NotificationRecord> Notifications { get; set; }

        [JsonProperty("views")]
        public List<ViewDelta> Views { get; set; }

        [JsonProperty("preference")]
        public NotificationPreference Preference { get; set; }

    }
}
