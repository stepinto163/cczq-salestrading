using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.get 请求体（H08）。</summary>
    public class QuoteGetRequest
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }
    }

}
