using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 行情中心"最优"看板行（原型 board-credit-best/rate-best，22 列）。
    /// 按 代码|价格类型|清算速度 聚合双向最优：收益率口径 Bid 越小越优、Ofr 越大越优；
    /// 净价/全价口径相反。
    /// 注：原型 0910 把聚合键改为「仅债券代码」并统一折算收益率后比价，
    /// 该业务口径待后端 Query 契约确认，此处保持原分组语义，仅补充同档多笔展示字段。
    /// </summary>
    public class BoardBestRow
    {
        /// <summary>行情所属市场：信用/利率。</summary>
        public BondMarket Market { get; set; }

        /// <summary>剩余期限（m）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        public PriceType PriceType { get; set; }

        public SettlementType Settlement { get; set; }

        /// <summary>最优 Bid 报价机构（instBid）。</summary>
        public string InstBid { get; set; }

        /// <summary>最优 Bid 长城交易员（trBid）。</summary>
        public string TraderBid { get; set; }

        /// <summary>最优 Bid 报量，单位万（bvBid）。</summary>
        public decimal? VolBid { get; set; }

        /// <summary>最优 Bid（bp）。</summary>
        public decimal? Bid { get; set; }

        /// <summary>最优 Ofr（op）。</summary>
        public decimal? Ofr { get; set; }

        /// <summary>最优 Ofr 报量，单位万（bvOfr）。</summary>
        public decimal? VolOfr { get; set; }

        /// <summary>最优 Ofr 长城交易员（trOfr）。</summary>
        public string TraderOfr { get; set; }

        /// <summary>最优 Ofr 报价机构（instOfr）。</summary>
        public string InstOfr { get; set; }

        /// <summary>最新成交价（lastPrice）。</summary>
        public decimal? LastPrice { get; set; }

        /// <summary>债券库（bondLib）。</summary>
        public string BondLibrary { get; set; }

        /// <summary>主体评级（r）。</summary>
        public string IssuerRating { get; set; }

        /// <summary>债券评级（br）。</summary>
        public string BondRating { get; set; }

        /// <summary>中债净价（nzNet）。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中债偏离（nzDev）。</summary>
        public decimal? NzDeviation { get; set; }

        /// <summary>中债 YTM（nzYTM，%）。</summary>
        public decimal? NzYtm { get; set; }

        /// <summary>中证净价（csNet）。</summary>
        public decimal? CsNetPrice { get; set; }

        /// <summary>中证全价（csFull）。</summary>
        public decimal? CsFullPrice { get; set; }

        /// <summary>中证 YTM（csYTM，%）。</summary>
        public decimal? CsYtm { get; set; }

        public string CsiValuationDate { get; set; }
        public string CsiValuationStatus { get; set; }
        public string CsiValuationDescription => "中证到期估值；日期：" + (CsiValuationDate ?? "未知")
            + "；状态：" + (CsiValuationStatus ?? "未提供");

        /// <summary>报价时间（t）。</summary>
        public DateTime? QuoteTime { get; set; }

        /// <summary>聚合来源报价 Id 集合（sourceQuoteId）。</summary>
        public string[] SourceQuoteIds { get; set; }

        /// <summary>
        /// 同档最优 Bid 笔数（原型 0910：折算收益率同一档位可能有多笔，&gt;1 时单元格前显示笔数按钮）。
        /// </summary>
        public int BidBestCount { get; set; }

        /// <summary>同档最优 Ofr 笔数。</summary>
        public int OfrBestCount { get; set; }

        /// <summary>同档最优 Bid 明细（悬停浮层 7 列，仅本人行可点击）。</summary>
        public System.Collections.Generic.IReadOnlyList<BestQuoteDetail> BidBestQuotes { get; set; }

        /// <summary>同档最优 Ofr 明细（悬停浮层 7 列，仅本人行可点击）。</summary>
        public System.Collections.Generic.IReadOnlyList<BestQuoteDetail> OfrBestQuotes { get; set; }

        /// <summary>
        /// 是否已置「打包」标签。最优行是聚合视图，Mock 阶段不携带单笔标签，恒为 false，
        /// 右键菜单 打包 项据此显示未置标签状态。
        /// </summary>
        public bool IsPacking
        {
            get { return false; }
        }

        /// <summary>是否已置「OCO」标签（同上，最优行不携带单笔标签）。</summary>
        public bool IsOco
        {
            get { return false; }
        }
    }
}
