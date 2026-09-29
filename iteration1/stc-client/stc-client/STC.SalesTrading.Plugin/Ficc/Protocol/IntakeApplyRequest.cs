using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>IntakeApplyRequest</summary>
    public class IntakeApplyRequest : Request
    {
        [JsonProperty("sourceType")]
        public string SourceType { get; set; }

        [JsonProperty("externalMessageId")]
        public string ExternalMessageId { get; set; }

        [JsonProperty("sourceUserId")]
        public string SourceUserId { get; set; }

        [JsonProperty("sourceTime")]
        public string SourceTime { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("quotes")]
        public List<IntakeQuoteItem> Quotes { get; set; }

    }
}
