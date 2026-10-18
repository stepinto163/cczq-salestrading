using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.history.search 请求体（H11）。日期范围用 fromDate/toDate，不建订阅；续取只传 cursor。</summary>
    public class QuoteHistorySearchRequest
    {
        [JsonProperty("filter")]
        public View.QuoteFilter Filter { get; set; }

        /// <summary>yyyy-MM-dd。</summary>
        [JsonProperty("fromDate")]
        public string FromDate { get; set; }

        [JsonProperty("toDate")]
        public string ToDate { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

}
