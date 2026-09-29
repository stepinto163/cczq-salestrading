using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>DictionaryRequest</summary>
    public class DictionaryRequest : Request
    {
        [JsonProperty("names")]
        public List<string> Names { get; set; }

    }
}
