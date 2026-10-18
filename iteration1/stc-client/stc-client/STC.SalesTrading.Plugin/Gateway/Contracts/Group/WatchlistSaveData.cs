using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{
    /// <summary>ficc.watchlist.save 应答 Data（H20）：Data.watchlist 为保存后的完整分组。</summary>
    public class WatchlistSaveData
    {
        [JsonProperty("watchlist")]
        public WatchlistDto Watchlist { get; set; }
    }
}
