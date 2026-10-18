using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>结构化筛选条件。Value 按契约是字符串或字符串数组（构造侧强类型赋值；应答侧不读）。</summary>
    public class FilterCondition
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>EQ / NE / IN / CONTAINS 等（见规范第 3 节过滤库）。</summary>
        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }

}
