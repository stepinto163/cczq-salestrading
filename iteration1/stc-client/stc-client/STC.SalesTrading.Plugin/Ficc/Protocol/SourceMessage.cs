using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>SourceMessage</summary>
    public class SourceMessage : QuoteSource
    {
        [JsonProperty("sourceMessageId")]
        public string SourceMessageId { get; set; }

        [JsonProperty("sourceType")]
        public string SourceType { get; set; }

        [JsonProperty("quoteIds")]
        public List<string> QuoteIds { get; set; }

    }
}
