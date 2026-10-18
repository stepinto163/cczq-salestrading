using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>推荐证据行（RowDetails 子表：支撑该推荐的正式报价明细）。</summary>
    public class RecommendationEvidenceRow
    {
        public string QuoteId { get; set; }

        public QuoteDirection Side { get; set; }

        public decimal? Price { get; set; }

        public decimal? Quantity { get; set; }

        /// <summary>价格口径（净价/全价/行权收益率/到期收益率）。</summary>
        public string PriceTypeName { get; set; }

        public string Institution { get; set; }

        public string Trader { get; set; }

        /// <summary>长城（我方）交易员。</summary>
        public string OwnerTrader { get; set; }

        public string Time { get; set; }
    }
}
