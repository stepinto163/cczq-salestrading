using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>BaseGetRequest</summary>
    public class BaseGetRequest : Request
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("ids")]
        public List<string> Ids { get; set; }

    }
}
