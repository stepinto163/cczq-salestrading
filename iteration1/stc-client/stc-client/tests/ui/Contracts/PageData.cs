using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>PageData</summary>
    public class PageData : ProtocolModel
    {
        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        [JsonProperty("hasMore")]
        public bool? HasMore { get; set; }

        [JsonProperty("totalCount")]
        public int? TotalCount { get; set; }

    }
}
