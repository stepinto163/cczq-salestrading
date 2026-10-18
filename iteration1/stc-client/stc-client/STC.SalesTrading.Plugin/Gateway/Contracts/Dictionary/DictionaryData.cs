using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Dictionary
{
    /// <summary>ficc.quote.dictionary.query 应答 Data（H14）。</summary>
    public class DictionaryData
    {
        /// <summary>业务字典（name → 候选值）。</summary>
        [JsonProperty("dictionaries")]
        public Dictionary<string, List<DictionaryOption>> Dictionaries { get; set; }

        [JsonProperty("filterFields")]
        public List<FilterFieldDto> FilterFields { get; set; }
    }
}
