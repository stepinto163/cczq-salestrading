using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.View
{

    /// <summary>
    /// 债券资料补充（决策 #9：期望后端在 quote 域 view/推送附带；字段集对齐 base.get SECURITY 记录）。
    /// 估值口径可能为字符串，转换在 Mapper 层做 TryParse。
    /// </summary>
    public class QuoteReference
    {
        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        /// <summary>剩余期限，如 "7.73Y"。</summary>
        [JsonProperty("remainingYears")]
        public string RemainingYears { get; set; }

        /// <summary>债券库（bondSubtype）。</summary>
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

        [JsonProperty("referenceVersion")]
        public string ReferenceVersion { get; set; }
    }

}
