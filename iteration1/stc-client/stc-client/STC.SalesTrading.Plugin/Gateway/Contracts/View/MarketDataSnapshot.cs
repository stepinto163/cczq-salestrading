using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>STC 兼容行情快照（S03 / H51 books[].marketData）。字段为 STC 风格命名，逐字段显式对齐。</summary>
    public class MarketDataSnapshot
    {
        [JsonProperty("SecurityID")]
        public string SecurityID { get; set; }

        [JsonProperty("Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("MarketIndicator")]
        public string MarketIndicator { get; set; }

        [JsonProperty("VenueTypeGW")]
        public string VenueTypeGW { get; set; }

        [JsonProperty("Venues")]
        public string Venues { get; set; }

        /// <summary>所选范围内完整条目数组；空簿为空数组。</summary>
        [JsonProperty("NoMDEntries")]
        public List<MarketDataEntry> NoMDEntries { get; set; }
    }

}
