using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    public class BaseGetRequest : Request
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("ids")]
        public List<string> Ids { get; set; }
    }

    public class BaseGetData : ProtocolModel
    {
        [JsonProperty("records")]
        public List<Newtonsoft.Json.Linq.JObject> Records { get; set; }
    }
}
