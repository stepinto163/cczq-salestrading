using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>Request</summary>
    public class Request : ProtocolModel
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

    }
}
