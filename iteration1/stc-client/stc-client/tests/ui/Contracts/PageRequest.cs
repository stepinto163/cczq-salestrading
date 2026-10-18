using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>PageRequest</summary>
    public class PageRequest : Request
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

    }
}
