using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>CrmLegacyRecord</summary>
    public class CrmLegacyRecord : ProtocolModel
    {
        [JsonProperty("idbkey")]
        public string Idbkey { get; set; }

        [JsonProperty("crmid")]
        public string Crmid { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("shortnamecn")]
        public string Shortnamecn { get; set; }

        [JsonProperty("fullnamecn")]
        public string Fullnamecn { get; set; }

        [JsonProperty("pinyin")]
        public string Pinyin { get; set; }

        [JsonProperty("pinyinfull")]
        public string Pinyinfull { get; set; }

        [JsonProperty("instcode")]
        public string Instcode { get; set; }

        [JsonProperty("insttypevalue")]
        public string Insttypevalue { get; set; }

        [JsonProperty("taglist")]
        public string Taglist { get; set; }

        [JsonProperty("contactcount")]
        public int? Contactcount { get; set; }

        [JsonProperty("namecn")]
        public string Namecn { get; set; }

        [JsonProperty("instid")]
        public string Instid { get; set; }

        [JsonProperty("instname")]
        public string Instname { get; set; }

        [JsonProperty("instfullnamecn")]
        public string Instfullnamecn { get; set; }

        [JsonProperty("ctctposition")]
        public string Ctctposition { get; set; }

        [JsonProperty("biztype")]
        public string Biztype { get; set; }

        [JsonProperty("commonuse")]
        public string Commonuse { get; set; }

        [JsonProperty("isonboard")]
        public string Isonboard { get; set; }

    }
}
