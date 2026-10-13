using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>结构化意向明细；数量单位为元，不是平台报价记录。</summary>
    public sealed class QTradePublishQuote : ProtocolModel
    {
        [JsonProperty("clientQuoteId")]
        public string ClientQuoteId { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("bidPriceType")]
        public int? BidPriceType { get; set; }

        [JsonProperty("bidPrice")]
        public string BidPrice { get; set; }

        [JsonProperty("bidQuantityYuan")]
        public string BidQuantityYuan { get; set; }

        [JsonProperty("bidTradeDate")]
        public string BidTradeDate { get; set; }

        [JsonProperty("bidSettlementType")]
        public string BidSettlementType { get; set; }

        [JsonProperty("offerPriceType")]
        public int? OfferPriceType { get; set; }

        [JsonProperty("offerPrice")]
        public string OfferPrice { get; set; }

        [JsonProperty("offerQuantityYuan")]
        public string OfferQuantityYuan { get; set; }

        [JsonProperty("offerTradeDate")]
        public string OfferTradeDate { get; set; }

        [JsonProperty("offerSettlementType")]
        public string OfferSettlementType { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

    }
}
