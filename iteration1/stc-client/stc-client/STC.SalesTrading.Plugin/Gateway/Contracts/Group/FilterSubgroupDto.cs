using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>筛选子组——仅为编辑器组织结构，查询前拍平（规范 3.4）。</summary>
    public class FilterSubgroupDto
    {
        [JsonProperty("subgroupId")]
        public string SubgroupId { get; set; }

        [JsonProperty("conditions")]
        public List<FilterGroupCondition> Conditions { get; set; }
    }

}
