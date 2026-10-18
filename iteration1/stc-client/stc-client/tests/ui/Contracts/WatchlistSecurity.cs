using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>WatchlistSecurity</summary>
    public class WatchlistSecurity : ProtocolModel
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }

    }
}
