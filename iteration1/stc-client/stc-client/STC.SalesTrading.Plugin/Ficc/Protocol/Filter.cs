using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>共享筛选 DSL；字段与枚举见 product-filter-contract.zh-CN。</summary>
    public class Filter : ProtocolModel
    {
        [JsonProperty("conditions")]
        public List<FilterCondition> Conditions { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("watchlistIds")]
        public List<string> WatchlistIds { get; set; }

        [JsonProperty("filterGroupIds")]
        public List<string> FilterGroupIds { get; set; }

    }
}
