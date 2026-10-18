using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Group
{

    /// <summary>关注分组成员（WatchlistDto.securities[]）。</summary>
    public class WatchlistItem
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }
    }

}
