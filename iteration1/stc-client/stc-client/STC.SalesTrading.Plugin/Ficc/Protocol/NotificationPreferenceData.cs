using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationPreferenceData</summary>
    public class NotificationPreferenceData : ProtocolModel
    {
        [JsonProperty("preference")]
        public NotificationPreference Preference { get; set; }

    }
}
