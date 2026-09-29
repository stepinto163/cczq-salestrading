using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ParserRequest</summary>
    public class ParserRequest : ProtocolModel
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

    }
}
