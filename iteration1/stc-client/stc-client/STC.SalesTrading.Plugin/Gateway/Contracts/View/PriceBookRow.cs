using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>价格簿行（H51/H52 books[]、S03 推送）。一行 = 一券的完整快照；updateTime 是面板排序键。</summary>
    public class PriceBookRow
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("bookRevision")]
        public string BookRevision { get; set; }

        [JsonProperty("marketData")]
        public MarketDataSnapshot MarketData { get; set; }

        /// <summary>MDEntryID → 报价身份。</summary>
        [JsonProperty("quoteRefs")]
        public Dictionary<string, QuoteRef> QuoteRefs { get; set; }

        /// <summary>该券有效报价最大 updateTime；空簿 null 并以 removeIds 移出。</summary>
        [JsonProperty("updateTime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>计算异常列表（R8：最优计算未就绪的字段级提示；元素 {quoteId, code}，暂无 UI 消费，容错保留）。</summary>
        [JsonProperty("calculationIssues")]
        public List<CalculationIssue> CalculationIssues { get; set; }
    }

}
