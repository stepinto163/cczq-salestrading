using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>报价来源（QuoteRecord.source）。</summary>
    public class QuoteSource
    {
        /// <summary>MANUAL / QT / IDEAL。</summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

}
