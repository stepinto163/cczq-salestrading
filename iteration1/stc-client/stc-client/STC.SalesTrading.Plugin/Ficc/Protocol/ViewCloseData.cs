using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ViewCloseData</summary>
    public class ViewCloseData : ProtocolModel
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("closed")]
        public bool? Closed { get; set; }

    }
}
