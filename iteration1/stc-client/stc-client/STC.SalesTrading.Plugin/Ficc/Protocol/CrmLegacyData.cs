using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>CrmLegacyData</summary>
    public class CrmLegacyData : ProtocolModel
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("totalSize")]
        public int? TotalSize { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        [JsonProperty("datas")]
        public List<CrmLegacyRecord> Datas { get; set; }

    }
}
