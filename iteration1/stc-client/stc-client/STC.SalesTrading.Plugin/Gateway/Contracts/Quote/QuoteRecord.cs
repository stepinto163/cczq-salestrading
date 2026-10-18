using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Gateway.Contracts.Quote
{

    /// <summary>
    /// 后端统一报价模型（H08 应答、H16/H17 视图页、S01 推送共用同一形状）。
    /// price/quantity 等十进制值以字符串传输；编码转换（side/quoteType/yieldType/settlType）
    /// 集中在 GatewayModelMapper。
    /// </summary>
    public class QuoteRecord
    {
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        /// <summary>克隆来源报价 Id（H07 克隆产生；暂无 UI 消费，容错保留）。</summary>
        [JsonProperty("clonedFromQuoteId")]
        public string ClonedFromQuoteId { get; set; }

        /// <summary>来源消息 Id，报价确认页分组键。</summary>
        [JsonProperty("sourceMessageId")]
        public string SourceMessageId { get; set; }

        [JsonProperty("source")]
        public QuoteSource Source { get; set; }

        [JsonProperty("ownerUserId")]
        public string OwnerUserId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        /// <summary>十进制字符串版本，乐观锁。</summary>
        [JsonProperty("revision")]
        public string Revision { get; set; }

        /// <summary>PENDING_CONFIRM / ACTIVE / REFERRED / DELETED。</summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>"1"=Bid / "2"=Ofr。</summary>
        [JsonProperty("side")]
        public string Side { get; set; }

        /// <summary>"1"=收益率 "2"=净价 "3"=全价。</summary>
        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        /// <summary>"1"=到期 "2"=行权。</summary>
        [JsonProperty("yieldType")]
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
        public bool Urgent { get; set; }

        [JsonProperty("recommended")]
        public bool Recommended { get; set; }

        [JsonProperty("whole")]
        public bool Whole { get; set; }

        [JsonProperty("oco")]
        public bool Oco { get; set; }

        [JsonProperty("packing")]
        public bool Packing { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("confirmedBy")]
        public string ConfirmedBy { get; set; }

        /// <summary>ISO 8601 带时区。</summary>
        [JsonProperty("createTime")]
        public DateTime? CreateTime { get; set; }

        /// <summary>列表排序键（updateTime DESC, quoteId ASC）。</summary>
        [JsonProperty("updateTime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>报价发布时间（确认前 null）——UI"报价时间"列。</summary>
        [JsonProperty("quoteTime")]
        public DateTime? QuoteTime { get; set; }

        [JsonProperty("confirmTime")]
        public DateTime? ConfirmTime { get; set; }
    }

}
