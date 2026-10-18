using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>WatchlistSaveRequest</summary>
    public class WatchlistSaveRequest : Request
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("securities")]
        public List<WatchlistSecurity> Securities { get; set; }

    }
}
