using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>行情条目关联的报价身份（books[].quoteRefs 值）。</summary>
    public class QuoteRef
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("quoteRevision")]
        public string QuoteRevision { get; set; }
    }

}
