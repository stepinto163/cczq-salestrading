using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>分组删除请求体（H21 关注组 / H24 筛选组，二者字段名不同故分两类）。</summary>
    public class WatchlistDeleteRequest
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }
    }
}
