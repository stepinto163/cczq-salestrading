using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>
    /// ficc.filterGroup.save 请求体（H23）：字段平铺，一次保存一组（整组替换并校验字段及操作）。
    /// 新建不传 FilterGroupId/ExpectedRevision；更新两者必填。
    /// </summary>
    public class FilterGroupSaveRequest
    {
        /// <summary>服务端组 Id；新建为 null。</summary>
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

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }
    }
}
