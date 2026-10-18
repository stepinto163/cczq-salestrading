using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 行情中心"最优"看板行（后端 mode=BEST）。
    /// <para>一行 = 一只债券的价格簿，Bid/Ofr 两列取各侧第一价格档；同价档的全部并列报价
    /// 在 <see cref="BidQuotes"/> / <see cref="OfrQuotes"/> 中展开（供"同档浮层"）。</para>
    /// </summary>
    public class BestRow
    {
        /// <summary>
        /// 所属市场。后端按 filter.market 查询域区分、行上无此字段，
        /// 由服务实现按查询填入（UI 右键分组菜单需要行级市场语义）。
        /// </summary>
        public BondMarket Market { get; set; }

        /// <summary>债券代码，带市场后缀。</summary>
        public string SecurityId { get; set; }

        /// <summary>债券简称。</summary>
        public string SecurityName { get; set; }

        /// <summary>该券价格簿最近更新时间，列表排序依据。</summary>
        public DateTime? UpdateTime { get; set; }

        #region 买方

        /// <summary>买方向最优价格（收益率比较口径）。无报价为 null。</summary>
        public decimal? Bid { get; set; }

        /// <summary>买方最优档报量合计，单位万。</summary>
        public decimal? BidVolume { get; set; }

        /// <summary>买方最优档并列笔数（&gt;1 时 UI 显示笔数按钮与浮层）。</summary>
        public int BidQuotesCount { get; set; }

        /// <summary>是否本人报价（Bid/Ofr 任一侧最优为本人；右键修改类菜单的门禁）。</summary>
        public bool IsMine { get; set; }

        /// <summary>买方最优档全部并列报价。</summary>
        public IReadOnlyList<BestQuoteDetail> BidQuotes { get; set; }

        #endregion

        #region 卖方

        /// <summary>卖方向最优价格。</summary>
        public decimal? Ofr { get; set; }

        /// <summary>卖方最优档报量合计，单位万。</summary>
        public decimal? OfrVolume { get; set; }

        /// <summary>卖方最优档并列笔数。</summary>
        public int OfrQuotesCount { get; set; }

        /// <summary>卖方最优档全部并列报价。</summary>
        public IReadOnlyList<BestQuoteDetail> OfrQuotes { get; set; }

        #endregion

        #region reference（债券资料；决策 #9/#10）

        /// <summary>剩余期限。</summary>
        public string Maturity { get; set; }

        /// <summary>债券库。</summary>
        public string BondLibrary { get; set; }

        /// <summary>主体评级。</summary>
        public string IssuerRating { get; set; }

        /// <summary>债项评级。</summary>
        public string BondRating { get; set; }

        /// <summary>中债估值 YTM，%。</summary>
        public decimal? NzYtm { get; set; }

        /// <summary>中债净价。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中证估值 YTM，%。</summary>
        public decimal? CsYtm { get; set; }

        /// <summary>中证净价。</summary>
        public decimal? CsNetPrice { get; set; }

        /// <summary>中证全价（看板列工厂统一插列；计算源暂缺，恒空占位）。</summary>
        public decimal? CsFullPrice { get; set; }

        /// <summary>中债偏离，BP。</summary>
        public decimal? NzDeviation { get; set; }

        #endregion

        /// <summary>该券全部关联报价 Id（quoteRefs 全集），用于定位与编辑。</summary>
        public string[] SourceQuoteIds { get; set; }
    }
}
