using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Notification
{

    /// <summary>个人提醒开关（ficc.notification.preference.get/save，H31/H32）。save 携带 expectedRevision。</summary>
    public sealed class NotificationPreferenceData
    {
        [JsonProperty("preference")]
        public NotificationPreference Preference { get; set; }
    }

    public class NotificationPreference
    {
        [JsonProperty("watchlistEnabled")]
        public bool? WatchlistEnabled { get; set; }

        [JsonProperty("filterGroupEnabled")]
        public bool? FilterGroupEnabled { get; set; }

        [JsonProperty("counterQuoteEnabled")]
        public bool? CounterQuoteEnabled { get; set; }

        [JsonProperty("myBestEnabled")]
        public bool? MyBestEnabled { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }
    }

}
