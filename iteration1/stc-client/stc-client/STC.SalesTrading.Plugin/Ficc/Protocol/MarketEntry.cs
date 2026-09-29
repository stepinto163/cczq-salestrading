using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>MarketEntry</summary>
    public class MarketEntry : ProtocolModel
    {
        [JsonProperty("MDEntryID")]
        public string MDEntryID { get; set; }

        [JsonProperty("MDEntryType")]
        public string MDEntryType { get; set; }

        [JsonProperty("MDEntryPx")]
        public string MDEntryPx { get; set; }

        [JsonProperty("MDEntrySize")]
        public string MDEntrySize { get; set; }

        [JsonProperty("SettlType")]
        public string SettlType { get; set; }

        [JsonProperty("MDEntryPositionNo")]
        public string MDEntryPositionNo { get; set; }

        [JsonProperty("MDEntryDate")]
        public string MDEntryDate { get; set; }

        [JsonProperty("MDEntryTime")]
        public string MDEntryTime { get; set; }

        [JsonProperty("Yield")]
        public string Yield { get; set; }

        [JsonProperty("YieldToExecution")]
        public string YieldToExecution { get; set; }

        [JsonProperty("InstitutionID")]
        public string InstitutionID { get; set; }

        [JsonProperty("TraderID")]
        public string TraderID { get; set; }

    }
}
