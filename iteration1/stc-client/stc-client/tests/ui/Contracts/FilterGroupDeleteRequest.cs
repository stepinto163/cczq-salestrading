using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>FilterGroupDeleteRequest</summary>
    public class FilterGroupDeleteRequest : Request
    {
        [JsonProperty("filterGroupId")]
        public string FilterGroupId { get; set; }

        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

    }
}
