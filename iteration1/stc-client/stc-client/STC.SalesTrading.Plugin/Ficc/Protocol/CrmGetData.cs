using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>CrmGetData</summary>
    public class CrmGetData : ProtocolModel
    {
        [JsonProperty("record")]
        public ReferenceRecord Record { get; set; }

    }
}
