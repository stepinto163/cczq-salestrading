using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>MarketData</summary>
    public class MarketData : ProtocolModel
    {
        [JsonProperty("SecurityID")]
        public string SecurityID { get; set; }

        [JsonProperty("Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("MarketIndicator")]
        public string MarketIndicator { get; set; }

        [JsonProperty("VenueTypeGW")]
        public string VenueTypeGW { get; set; }

        [JsonProperty("Venues")]
        public string Venues { get; set; }

        [JsonProperty("NoMDEntries")]
        public List<MarketEntry> NoMDEntries { get; set; }

    }
}
