using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>IntakeApplyData</summary>
    public class IntakeApplyData : ProtocolModel
    {
        [JsonProperty("sourceMessageId")]
        public string SourceMessageId { get; set; }

        [JsonProperty("results")]
        public List<QuoteOperationResult> Results { get; set; }

    }
}
