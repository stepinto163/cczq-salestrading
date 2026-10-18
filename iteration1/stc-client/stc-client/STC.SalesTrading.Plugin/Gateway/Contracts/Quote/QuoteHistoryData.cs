using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.history.search 应答 Data（H11）。nextCursor=null 表示取完。</summary>
    public class QuoteHistoryData
    {
        [JsonProperty("rows")]
        public List<QuoteHistoryRecord> Rows { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        /// <summary>债券资料（H11 自带，含估值——不依赖决策 #9 的 view 侧补齐）。</summary>
        [JsonProperty("reference")]
        public Dictionary<string, QuoteReference> Reference { get; set; }
    }

}
