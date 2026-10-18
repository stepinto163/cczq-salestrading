using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>批量写入应答 Data（H01—H07）。</summary>
    public class QuoteWriteData
    {
        [JsonProperty("results")]
        public List<QuoteOperationResult> Results { get; set; }
    }

}
