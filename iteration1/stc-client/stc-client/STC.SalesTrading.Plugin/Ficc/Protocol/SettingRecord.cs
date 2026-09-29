using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>SettingRecord</summary>
    public class SettingRecord : FilterGroup
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("securities")]
        public List<WatchlistSecurity> Securities { get; set; }

    }
}
