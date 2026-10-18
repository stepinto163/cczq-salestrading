using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>quote 域 view.open / view.query 应答 Data（H16/H17）。</summary>
    public class QuoteViewData
    {
        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("view")]
        public ViewPage View { get; set; }

        [JsonProperty("quotes")]
        public List<Quote.QuoteRecord> Quotes { get; set; }

        /// <summary>reference 补齐（决策 #9）：securityId → 债券资料。</summary>
        [JsonProperty("reference")]
        public Dictionary<string, QuoteReference> Reference { get; set; }
    }

}
