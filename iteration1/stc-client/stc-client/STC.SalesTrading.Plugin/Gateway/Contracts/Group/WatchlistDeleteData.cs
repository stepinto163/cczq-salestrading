using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>ficc.watchlist.delete 应答 Data（H21）。</summary>
    public class WatchlistDeleteData
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }
}
