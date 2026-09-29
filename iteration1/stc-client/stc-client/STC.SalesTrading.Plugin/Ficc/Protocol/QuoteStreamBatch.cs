using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteStreamBatch</summary>
    public class QuoteStreamBatch : StreamMessage
    {
        [JsonProperty("quotes")]
        public List<QuoteRecord> Quotes { get; set; }

        [JsonProperty("reports")]
        public List<QuoteReport> Reports { get; set; }

        [JsonProperty("views")]
        public List<ViewDelta> Views { get; set; }

        [JsonProperty("reference")]
        public List<ReferenceRecord> Reference { get; set; }

    }
}
