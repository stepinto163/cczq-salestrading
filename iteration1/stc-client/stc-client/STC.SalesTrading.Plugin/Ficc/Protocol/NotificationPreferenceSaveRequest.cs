using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationPreferenceSaveRequest</summary>
    public class NotificationPreferenceSaveRequest : Request
    {
        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("watchlistEnabled")]
        public bool? WatchlistEnabled { get; set; }

        [JsonProperty("filterGroupEnabled")]
        public bool? FilterGroupEnabled { get; set; }

        [JsonProperty("counterQuoteEnabled")]
        public bool? CounterQuoteEnabled { get; set; }

        [JsonProperty("myBestEnabled")]
        public bool? MyBestEnabled { get; set; }

    }
}
