using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>WatchlistData</summary>
    public class WatchlistData : ProtocolModel
    {
        [JsonProperty("watchlist")]
        public Watchlist Watchlist { get; set; }

        [JsonProperty("watchlists")]
        public List<Watchlist> Watchlists { get; set; }

        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("deleted")]
        public bool? Deleted { get; set; }

    }
}
