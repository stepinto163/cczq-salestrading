using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>QtIntakeStatusData</summary>
    public class QtIntakeStatusData : ProtocolModel
    {
        [JsonProperty("fixConnected")]
        public bool? FixConnected { get; set; }

        [JsonProperty("storageFailed")]
        public bool? StorageFailed { get; set; }

        [JsonProperty("quoteSvrOnline")]
        public bool? QuoteSvrOnline { get; set; }

    }
}
