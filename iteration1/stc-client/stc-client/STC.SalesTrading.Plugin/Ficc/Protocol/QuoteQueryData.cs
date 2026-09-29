using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteQueryData</summary>
    public class QuoteQueryData : PageData
    {
        [JsonProperty("quotes")]
        public List<QuoteRecord> Quotes { get; set; }

        [JsonProperty("reference")]
        public List<ReferenceRecord> Reference { get; set; }

    }
}
