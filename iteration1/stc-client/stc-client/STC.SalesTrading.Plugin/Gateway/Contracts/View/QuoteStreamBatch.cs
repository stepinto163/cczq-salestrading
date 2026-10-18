using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>ficc.quote.report 推送批次（S01）。quotes 是本批完整报价对象并集，每个 quoteId 只出现一次。</summary>
    public class QuoteStreamBatch : StreamMessage
    {
        [JsonProperty("quotes")]
        public List<Quote.QuoteRecord> Quotes { get; set; }

        [JsonProperty("reports")]
        public List<StreamReportItem> Reports { get; set; }

        /// <summary>reference 补齐（决策 #9 已落地）：securityId → 债券资料，随推送附带。</summary>
        [JsonProperty("reference")]
        public Dictionary<string, QuoteReference> Reference { get; set; }
    }

}
