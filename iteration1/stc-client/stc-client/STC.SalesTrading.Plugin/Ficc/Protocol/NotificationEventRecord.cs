using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationEventRecord</summary>
    public class NotificationEventRecord : NotificationRecord
    {
        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("subgroups")]
        public List<FilterSubgroup> Subgroups { get; set; }

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
