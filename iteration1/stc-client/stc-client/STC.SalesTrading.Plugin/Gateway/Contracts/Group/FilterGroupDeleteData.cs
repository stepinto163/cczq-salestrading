using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>ficc.filterGroup.delete 应答 Data（H24）。</summary>
    public class FilterGroupDeleteData
    {
        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }
}
