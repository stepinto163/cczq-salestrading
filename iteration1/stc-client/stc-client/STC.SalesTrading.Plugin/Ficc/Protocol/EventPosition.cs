using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>EventPosition</summary>
    public class EventPosition : ProtocolModel
    {
        [JsonProperty("epoch")]
        public string Epoch { get; set; }

        [JsonProperty("sequence")]
        public string Sequence { get; set; }

    }
}
