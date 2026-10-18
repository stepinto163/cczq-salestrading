using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>ficc.quote.create 请求体（H01）。1—100 条，单条也是数组。</summary>
    public class QuoteCreateRequest
    {
        [JsonProperty("quotes")]
        public List<QuoteInput> Quotes { get; set; }
    }

}
