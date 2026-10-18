using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>Watchlist</summary>
    public class Watchlist : ProtocolModel
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("securities")]
        public List<WatchlistSecurity> Securities { get; set; }

    }
}
