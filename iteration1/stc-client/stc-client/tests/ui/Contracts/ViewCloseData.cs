using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>ViewCloseData</summary>
    public class ViewCloseData : ProtocolModel
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

        [JsonProperty("closed")]
        public bool? Closed { get; set; }

    }
}
