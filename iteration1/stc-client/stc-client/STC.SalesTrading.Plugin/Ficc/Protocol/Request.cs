using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>Request</summary>
    public class Request : ProtocolModel
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

    }
}
