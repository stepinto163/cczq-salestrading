using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>EventQueryRequest</summary>
    public class EventQueryRequest : Request
    {
        [JsonProperty("epoch")]
        public string Epoch { get; set; }

        [JsonProperty("afterSequence")]
        public string AfterSequence { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

    }
}
