using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>ficc.filterGroup.delete 请求体（H24）。</summary>
    public class FilterGroupDeleteRequest
    {
        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }
    }
}
