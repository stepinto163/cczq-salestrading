using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>ViewCloseRequest</summary>
    public class ViewCloseRequest : Request
    {
        [JsonProperty("viewId")]
        public string ViewId { get; set; }

    }
}
