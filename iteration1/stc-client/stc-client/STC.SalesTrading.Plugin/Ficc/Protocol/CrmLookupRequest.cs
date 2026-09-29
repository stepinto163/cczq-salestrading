using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>CrmLookupRequest</summary>
    public class CrmLookupRequest : Request
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("match")]
        public string Match { get; set; }

        [JsonProperty("dealerStr")]
        public string DealerStr { get; set; }

        [JsonProperty("PageSize")]
        public int? PageSize { get; set; }

    }
}
