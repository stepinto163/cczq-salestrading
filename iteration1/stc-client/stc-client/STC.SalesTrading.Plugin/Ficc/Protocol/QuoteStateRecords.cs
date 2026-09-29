using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QuoteStateRecords</summary>
    public class QuoteStateRecords : ProtocolModel
    {
        [JsonProperty("quotes")]
        public List<QuoteRecord> Quotes { get; set; }

        [JsonProperty("watchlists")]
        public List<Watchlist> Watchlists { get; set; }

        [JsonProperty("filterGroups")]
        public List<FilterGroup> FilterGroups { get; set; }

    }
}
