using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>ViewCloseRequest</summary>
    public class ViewCloseRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

    }
}
