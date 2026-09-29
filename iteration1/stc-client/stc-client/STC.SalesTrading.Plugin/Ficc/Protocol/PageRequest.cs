using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>PageRequest</summary>
    public class PageRequest : Request
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

    }
}
