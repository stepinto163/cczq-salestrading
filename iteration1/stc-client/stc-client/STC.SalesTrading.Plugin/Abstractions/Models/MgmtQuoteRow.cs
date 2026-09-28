using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>报价管理行（原型 mgmtToday/mgmtHistory，23 列，与看板行语义不同故独立定义）。</summary>
    public class MgmtQuoteRow
    {
        public long Revision { get; set; }

        /// <summary>报价 Id（quoteId）。</summary>
        public string QuoteId { get; set; }

        /// <summary>报价方式（mode）：QT/ideal/手工录入。</summary>
        public QuoteMode Mode { get; set; }

        /// <summary>报价状态：有效/撤销。</summary>
        public QuoteStatus Status { get; set; }

        /// <summary>剩余期限（m）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>方向（d）。</summary>
        public QuoteDirection Direction { get; set; }

        /// <summary>报价（bp/op）。</summary>
        public decimal? Price { get; set; }

        /// <summary>报量，单位万（bv）。</summary>
        public decimal? Volume { get; set; }

        /// <summary>旗标组合。</summary>
        public QuoteFlag Flags { get; set; }

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

        /// <summary>报价方式显示文本（QT/ideal/手工录入）。</summary>
        public string ModeText
        {
            get
            {
                switch (Mode)
                {
                    case QuoteMode.Qt: return "QT";
                    case QuoteMode.Ideal: return "ideal";
                    case QuoteMode.Manual: return "手工录入";
                    default: return string.Empty;
                }
            }
        }

        /// <summary>旗标组合的显示文本（急/荐/整/OCO/打包，空格分隔；无则空）。</summary>
        public string FlagsText
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (Flags.HasFlag(QuoteFlag.Urgent)) parts.Add("急");
                if (Flags.HasFlag(QuoteFlag.Recommend)) parts.Add("荐");
                if (Flags.HasFlag(QuoteFlag.Whole)) parts.Add("整");
                if (Flags.HasFlag(QuoteFlag.Oco)) parts.Add("OCO");
                if (Flags.HasFlag(QuoteFlag.Packing)) parts.Add("打包");
                return string.Join(" ", parts);
            }
        }
    }
}
