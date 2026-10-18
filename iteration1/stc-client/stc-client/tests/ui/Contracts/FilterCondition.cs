using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>筛选条件；value 按 operator/字段字典为标量或数组。</summary>
    public class FilterCondition : ProtocolModel
    {
        [JsonProperty("conditionId")]
        public string ConditionId { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }

    }
}
