using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationStateRecords</summary>
    public class NotificationStateRecords : ProtocolModel
    {
        [JsonProperty("notifications")]
        public List<NotificationRecord> Notifications { get; set; }

        [JsonProperty("preferences")]
        public List<NotificationPreference> Preferences { get; set; }

        [JsonProperty("filterGroups")]
        public List<FilterGroup> FilterGroups { get; set; }

    }
}
