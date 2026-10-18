using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>WatchlistQueryRequest</summary>
    public class WatchlistQueryRequest : Request
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

    }
}
