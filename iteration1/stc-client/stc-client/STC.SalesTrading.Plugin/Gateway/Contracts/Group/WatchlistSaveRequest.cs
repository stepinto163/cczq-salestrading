using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>
    /// ficc.watchlist.save 请求体（H20）：字段平铺，一次保存一组（组成员完整替换）。
    /// 新建不传 WatchlistId/ExpectedRevision；更新两者必填。
    /// </summary>
    public class WatchlistSaveRequest
    {
        /// <summary>服务端组 Id；新建为 null。</summary>
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>RATE / CREDIT。</summary>
        [JsonProperty("market")]
        public string Market { get; set; }

        /// <summary>MARKET / HISTORY / NOTIFICATION。</summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }

        /// <summary>组成员全集（同券去重，服务端处理）。</summary>
        [JsonProperty("securities")]
        public List<WatchlistItem> Securities { get; set; }

        /// <summary>更新时的乐观锁；新建为 null。</summary>
        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }
    }
}
