using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>SourceMessageData</summary>
    public class SourceMessageData : PageData
    {
        [JsonProperty("sourceMessages")]
        public List<SourceMessage> SourceMessages { get; set; }

    }
}
