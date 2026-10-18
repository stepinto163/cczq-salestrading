using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>FilterSubgroup</summary>
    public class FilterSubgroup : ProtocolModel
    {
        [JsonProperty("subgroupId")]
        public string SubgroupId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayOrder")]
        public int? DisplayOrder { get; set; }

        [JsonProperty("conditions")]
        public List<FilterCondition> Conditions { get; set; }

    }
}
