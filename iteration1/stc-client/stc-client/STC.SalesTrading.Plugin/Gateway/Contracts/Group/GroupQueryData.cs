using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>分组查询应答 Data（H19/H22）。watchlists / filterGroups 按 topic 二选一。</summary>
    public class GroupQueryData
    {
        [JsonProperty("watchlists")]
        public List<WatchlistDto> Watchlists { get; set; }

        [JsonProperty("filterGroups")]
        public List<FilterGroupDto> FilterGroups { get; set; }
    }

}
