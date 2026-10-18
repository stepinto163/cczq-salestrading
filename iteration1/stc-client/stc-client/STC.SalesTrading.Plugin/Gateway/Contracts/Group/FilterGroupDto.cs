using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>筛选分组（H22 应答、H23/H24 保存删除）。保存为完整替换 + expectedRevision。</summary>
    public class FilterGroupDto
    {
        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }

        [JsonProperty("subgroups")]
        public List<FilterSubgroupDto> Subgroups { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }
    }

}
