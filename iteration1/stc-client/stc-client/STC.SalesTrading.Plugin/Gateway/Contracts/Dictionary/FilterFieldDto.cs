using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Dictionary
{
    /// <summary>筛选字段（H14 filterFields[]：字段目录唯一事实源，来自共享 FilterEngine）。</summary>
    public class FilterFieldDto
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>值类型（enum/classification/text/number/date…）。</summary>
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

        /// <summary>分组（报价/债券/流动性/信用/条款/区域）。</summary>
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }

        /// <summary>是否模拟数据字段（R6 模拟期标记）。</summary>
        [JsonProperty("simulated")]
        public bool? Simulated { get; set; }
    }
}
