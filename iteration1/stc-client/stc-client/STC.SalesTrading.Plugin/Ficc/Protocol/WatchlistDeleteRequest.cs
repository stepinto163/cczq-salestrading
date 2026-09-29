using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>WatchlistDeleteRequest</summary>
    public class WatchlistDeleteRequest : Request
    {
        [JsonProperty("watchlistId")]
        public string WatchlistId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

    }
}
