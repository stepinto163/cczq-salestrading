using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 行情中心"全部"看板行（原型 board-credit-all/rate-all，20 列）。
    /// 纯 DTO；筛选/选中等 UI 状态由模块 ViewModel 层包装。
    /// </summary>
    public class BoardQuoteRow
    {
        public long Revision { get; set; }

        /// <summary>行情所属市场：信用/利率。</summary>
        public BondMarket Market { get; set; }

        /// <summary>报价 Id（quoteId）。</summary>
        public string QuoteId { get; set; }

        /// <summary>剩余期限（m）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>方向（d）。</summary>
        public QuoteDirection Direction { get; set; }

        /// <summary>价格类型（pt）。</summary>
        public PriceType PriceType { get; set; }

        /// <summary>清算速度，用于最优报价分组。</summary>
        public SettlementType Settlement { get; set; }

        /// <summary>报价（bp/op）。</summary>
        public decimal? Price { get; set; }

        /// <summary>报量，单位万（bv）。</summary>
        public decimal? Volume { get; set; }

        /// <summary>报价机构（inst）。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员（tr）。</summary>
        public string Trader { get; set; }

        /// <summary>长城交易员。</summary>
        public string WallTrader { get; set; }

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

        /// <summary>报价时间（t）。</summary>
        public DateTime? QuoteTime { get; set; }

        /// <summary>旗标组合（急/荐/整等）。</summary>
        public QuoteFlag Flags { get; set; }

        /// <summary>是否我的报价。</summary>
        public bool IsMine { get; set; }

        /// <summary>所属账户 Id（ownerAccountId）。</summary>
        public string OwnerAccountId { get; set; }

        /// <summary>是否已置「打包」标签（右键菜单 打包 项的高亮状态，原型 ctxPackItem.active）。</summary>
        public bool IsPacking
        {
            get { return (Flags & QuoteFlag.Packing) != 0; }
        }

        /// <summary>是否已置「OCO」标签（右键菜单 OCO 项的高亮状态，原型 ctxOcoItem.active）。</summary>
        public bool IsOco
        {
            get { return (Flags & QuoteFlag.Oco) != 0; }
        }
    }
}
