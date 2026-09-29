using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>FilterField</summary>
    public class FilterField : ProtocolModel
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("operators")]
        public List<string> Operators { get; set; }

        [JsonProperty("enumValues")]
        public List<DictionaryOption> EnumValues { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }

        [JsonProperty("simulated")]
        public bool? Simulated { get; set; }

    }
}
