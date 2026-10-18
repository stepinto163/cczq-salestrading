using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>筛选条件（FilterSubgroupDto.conditions[]）。</summary>
    public class FilterGroupCondition
    {
        [JsonProperty("conditionId")]
        public string ConditionId { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        /// <summary>字符串或字符串数组（构造侧强类型赋值）。</summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }
    }

}
