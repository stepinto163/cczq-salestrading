using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationPreference</summary>
    public class NotificationPreference : ProtocolModel
    {
        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("watchlistEnabled")]
        public bool? WatchlistEnabled { get; set; }

        [JsonProperty("filterGroupEnabled")]
        public bool? FilterGroupEnabled { get; set; }

        [JsonProperty("counterQuoteEnabled")]
        public bool? CounterQuoteEnabled { get; set; }

        [JsonProperty("myBestEnabled")]
        public bool? MyBestEnabled { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

    }
}
