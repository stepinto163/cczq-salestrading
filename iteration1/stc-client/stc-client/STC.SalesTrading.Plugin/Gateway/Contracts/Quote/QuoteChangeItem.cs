using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>
    /// 确认（ficc.quote.confirm）/修改（ficc.quote.update）条目。quoteId + expectedRevision 必填，
    /// 其余仅提交要修改的字段（未传 = 保留）。remark / yieldType 允许显式清空（序列化 Include）。
    /// </summary>
    public class QuoteChangeItem
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        /// <summary>十进制字符串版本，乐观锁。</summary>
        [JsonProperty("expectedRevision")]
        public string ExpectedRevision { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        /// <summary>改为净价/全价时须显式提交 null 清除，故 Include。</summary>
        [JsonProperty("yieldType", NullValueHandling = NullValueHandling.Include)]
        public string YieldType { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("quantityUnit")]
        public string QuantityUnit { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        [JsonProperty("settlType")]
        public string SettlType { get; set; }

        [JsonProperty("urgent")]
        public bool? Urgent { get; set; }

        [JsonProperty("recommended")]
        public bool? Recommended { get; set; }

        [JsonProperty("whole")]
        public bool? Whole { get; set; }

        [JsonProperty("oco")]
        public bool? Oco { get; set; }

        [JsonProperty("packing")]
        public bool? Packing { get; set; }

        /// <summary>显式提交 null 以清空备注，故 Include。</summary>
        [JsonProperty("remark", NullValueHandling = NullValueHandling.Include)]
        public string Remark { get; set; }

        /// <summary>refer/unrefer 的原因说明（H04/H05，可空；update/confirm/delete 不发）。</summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

}
