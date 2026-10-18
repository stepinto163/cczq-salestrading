using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>NotificationPreferenceData</summary>
    public class NotificationPreferenceData : ProtocolModel
    {
        [JsonProperty("preference")]
        public NotificationPreference Preference { get; set; }

    }
}
