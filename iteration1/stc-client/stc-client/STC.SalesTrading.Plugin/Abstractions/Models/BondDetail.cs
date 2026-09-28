using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 债券详情（行情中心右侧详情面板 7 区块的聚合模型：
    /// detail-header / bond-header / inst-filter-bar / best-quote-bar / market-tape-stack / 估值网格 / rec-panel）。
    /// </summary>
    public class BondDetail
    {
        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>最新成交价。</summary>
        public decimal? LastPrice { get; set; }

        /// <summary>昨收。</summary>
        public decimal? PrevClose { get; set; }

        /// <summary>最高。</summary>
        public decimal? High { get; set; }

        /// <summary>开盘。</summary>
        public decimal? Open { get; set; }

        /// <summary>最低。</summary>
        public decimal? Low { get; set; }

        /// <summary>久期。</summary>
        public decimal? Duration { get; set; }

        /// <summary>报价时间。</summary>
        public DateTime? QuoteTime { get; set; }

        /// <summary>中债净价。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中债 YTM（%）。</summary>
        public decimal? NzYtm { get; set; }

        /// <summary>中债偏离。</summary>
        public decimal? NzDeviation { get; set; }

        /// <summary>主体评级。</summary>
        public string IssuerRating { get; set; }

        /// <summary>剩余期限。</summary>
        public string Maturity { get; set; }

        /// <summary>债券库。</summary>
        public string BondLibrary { get; set; }

        /// <summary>最优报价条：Bid。</summary>
        public decimal? BidPrice { get; set; }

        /// <summary>最优报价条：Ofr。</summary>
        public decimal? OfrPrice { get; set; }

        /// <summary>多档行情。</summary>
        public IReadOnlyList<InstQuoteRow> InstQuotes { get; set; }

        /// <summary>成交行情。</summary>
        public IReadOnlyList<MarketTradeRow> MarketTrades { get; set; }

        /// <summary>智能推荐（rec-panel；含 Bid/Ofr 双侧 + 三种路径的行，VM 本地过滤）。</summary>
        public IReadOnlyList<RecommendationRow> Recommendations { get; set; }
    }
}
