using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.get 应答 Data（H08）。</summary>
    public class QuoteGetData
    {
        [JsonProperty("quote")]
        public QuoteRecord Quote { get; set; }
    }

}
