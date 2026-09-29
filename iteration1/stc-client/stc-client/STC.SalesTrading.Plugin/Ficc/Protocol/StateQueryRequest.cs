using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>StateQueryRequest</summary>
    public class StateQueryRequest : PageRequest
    {
        [JsonProperty("collections")]
        public List<string> Collections { get; set; }

    }
}
