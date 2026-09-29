using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>DictionaryData</summary>
    public class DictionaryData : ProtocolModel
    {
        [JsonProperty("dictionaries")]
        public Dictionary<string,List<DictionaryOption>> Dictionaries { get; set; }

        [JsonProperty("filterFields")]
        public List<FilterField> FilterFields { get; set; }

    }
}
