using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>
    /// Quote/Price/Notification 三域共用筛选（ficc.*.view.open 与 quote.query / history.search）。
    /// 同字段 OR、跨字段 AND；空选中列表表示未启用该类限制。
    /// </summary>
    public class QuoteFilter
    {
        /// <summary>RATE / CREDIT；通知域可空。</summary>
        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("conditions")]
        public List<FilterCondition> Conditions { get; set; }

        [JsonProperty("watchlistIds")]
        public List<string> WatchlistIds { get; set; }

        [JsonProperty("filterGroupIds")]
        public List<string> FilterGroupIds { get; set; }
    }

}
