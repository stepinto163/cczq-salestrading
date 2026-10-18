using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>创建报价输入（ficc.quote.create，H01）。clientQuoteId 必填；id/revision/source/owner 由服务端生成。</summary>
    public class QuoteInput
    {
        /// <summary>客户端行标识，创建应答与推送经它回绑。</summary>
        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        /// <summary>债券代码，带市场后缀。</summary>
        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        /// <summary>可先只录名称，确认时再从 CRM 选定 Id（H43）。</summary>
        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        /// <summary>"1"=Bid / "2"=Ofr。</summary>
        [JsonProperty("side")]
        public string Side { get; set; }

        /// <summary>"1"=收益率 "2"=净价 "3"=全价。</summary>
        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        /// <summary>收益率类型："1"=到期 "2"=行权；净价/全价必须为 null。</summary>
        [JsonProperty("yieldType")]
        public string YieldType { get; set; }

        /// <summary>价格/收益率，十进制字符串；收益率百分数不超过 4 位小数。</summary>
        [JsonProperty("price")]
        public string Price { get; set; }

        /// <summary>报量，单位 WAN，须大于 0。</summary>
        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        /// <summary>固定 WAN。</summary>
        [JsonProperty("quantityUnit")]
        public string QuantityUnit { get; set; }

        /// <summary>固定 CNY。</summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>交易日期 yyyy-MM-dd。</summary>
        [JsonProperty("tradeDate")]
        public string TradeDate { get; set; }

        /// <summary>清算速度 "1"/"2"/"3"/"4" = T+0..T+3。</summary>
        [JsonProperty("settlType")]
        public string SettlType { get; set; }

        [JsonProperty("urgent")]
        public bool? Urgent { get; set; }

        [JsonProperty("recommended")]
        public bool? Recommended { get; set; }

        [JsonProperty("whole")]
        public bool? Whole { get; set; }

        /// <summary>与 Packing 互斥。</summary>
        [JsonProperty("oco")]
        public bool? Oco { get; set; }

        [JsonProperty("packing")]
        public bool? Packing { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

}
