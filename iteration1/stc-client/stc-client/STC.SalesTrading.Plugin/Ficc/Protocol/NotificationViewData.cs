using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationViewData</summary>
    public class NotificationViewData : ProtocolModel
    {
        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("view")]
        public ViewPage View { get; set; }

        [JsonProperty("notifications")]
        public List<NotificationRecord> Notifications { get; set; }

        [JsonProperty("preference")]
        public NotificationPreference Preference { get; set; }

    }
}
