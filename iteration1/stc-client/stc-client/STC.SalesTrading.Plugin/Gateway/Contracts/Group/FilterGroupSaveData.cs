using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>ficc.filterGroup.save 应答 Data（H23）：Data.filterGroup 为保存后的完整分组。</summary>
    public class FilterGroupSaveData
    {
        [JsonProperty("filterGroup")]
        public FilterGroupDto FilterGroup { get; set; }
    }
}
