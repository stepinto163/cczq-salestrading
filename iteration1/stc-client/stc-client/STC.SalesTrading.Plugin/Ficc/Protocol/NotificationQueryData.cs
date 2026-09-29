using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationQueryData</summary>
    public class NotificationQueryData : PageData
    {
        [JsonProperty("notifications")]
        public List<NotificationRecord> Notifications { get; set; }

    }
}
