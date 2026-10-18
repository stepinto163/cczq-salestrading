using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Base
{

    /// <summary>
    /// 基础资料/CRM 记录（H42/H43 records[]）。不同 kind 返回不同字段子集，全字段可空。
    /// 估值字段按字符串传输（口径在 Mapper 层 TryParse），避免后端数值/字符串形态变化打断反序列化。
    /// </summary>
    public class BaseRecord
    {
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        [JsonProperty("remainingYears")]
        public string RemainingYears { get; set; }

        /// <summary>债券库。</summary>
        [JsonProperty("bondSubtype")]
        public string BondSubtype { get; set; }

        [JsonProperty("issuerRating")]
        public string IssuerRating { get; set; }

        [JsonProperty("bondRating")]
        public string BondRating { get; set; }

        /// <summary>中债估值 YTM，%。</summary>
        [JsonProperty("valuationYield")]
        public string ValuationYield { get; set; }

        /// <summary>中债净价。</summary>
        [JsonProperty("valuationCleanPrice")]
        public string ValuationCleanPrice { get; set; }

        /// <summary>中证估值 YTM，%。</summary>
        [JsonProperty("csiValuationYield")]
        public string CsiValuationYield { get; set; }

        /// <summary>中证净价。</summary>
        [JsonProperty("csiValuationCleanPrice")]
        public string CsiValuationCleanPrice { get; set; }

        [JsonProperty("issuerId")]
        public string IssuerId { get; set; }

        [JsonProperty("issuerName")]
        public string IssuerName { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("institutionShortName")]
        public string InstitutionShortName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("positionName")]
        public string PositionName { get; set; }

        /// <summary>联系人可归属多个机构。</summary>
        [JsonProperty("institutionIds")]
        public List<string> InstitutionIds { get; set; }

        [JsonProperty("referenceVersion")]
        public string ReferenceVersion { get; set; }
    }

}
