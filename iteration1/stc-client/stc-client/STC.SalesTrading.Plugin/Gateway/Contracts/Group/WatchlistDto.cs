using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>关注分组（H19 应答、H20/H21 保存删除）。保存为完整替换。</summary>
    public class WatchlistDto
    {
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

        [JsonProperty("securities")]
        public List<WatchlistItem> Securities { get; set; }

        /// <summary>字符串版本，save 作 expectedRevision。</summary>
        [JsonProperty("revision")]
        public string Revision { get; set; }
    }

}
