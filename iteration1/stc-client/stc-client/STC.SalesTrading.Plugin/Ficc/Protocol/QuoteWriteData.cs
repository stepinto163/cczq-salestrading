using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteWriteData</summary>
    public class QuoteWriteData : ProtocolModel
    {
        [JsonProperty("results")]
        public List<QuoteOperationResult> Results { get; set; }

    }
}
