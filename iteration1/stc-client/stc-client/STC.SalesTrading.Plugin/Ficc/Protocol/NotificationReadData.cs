using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>NotificationReadData</summary>
    public class NotificationReadData : ProtocolModel
    {
        [JsonProperty("results")]
        public List<NotificationReadResult> Results { get; set; }

    }
}
