using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteViewData</summary>
    public class QuoteViewData : ProtocolModel
    {
        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("view")]
        public ViewPage View { get; set; }

        [JsonProperty("quotes")]
        public List<QuoteRecord> Quotes { get; set; }

        [JsonProperty("reference")]
        public List<ReferenceRecord> Reference { get; set; }

    }
}
