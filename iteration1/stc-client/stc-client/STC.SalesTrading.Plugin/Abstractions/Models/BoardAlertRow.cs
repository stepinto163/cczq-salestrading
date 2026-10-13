using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>报价提醒行（原型 board-alert 投影，23 列，结构同最优表 + 提醒类型）。</summary>
    public class BoardAlertRow
    {
        public bool IsRead { get; set; }

        /// <summary>提醒类型：关注债券(绿)/筛选分组(蓝)/对价提醒(金)/我的最优(紫)。</summary>
        public AlertType AlertType { get; set; }

        /// <summary>提醒键（alertKey，用于已读状态）。</summary>
        public string AlertKey { get; set; }

        /// <summary>剩余期限（m）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>Bid 报价机构（instBid）。</summary>
        public string InstBid { get; set; }

        /// <summary>Bid 长城交易员（trBid）。</summary>
        public string TraderBid { get; set; }

        /// <summary>Bid 报量，单位万（bvBid）。</summary>
        public decimal? VolBid { get; set; }

        /// <summary>Bid（bp）。</summary>
        public decimal? Bid { get; set; }

        /// <summary>Ofr（op）。</summary>
        public decimal? Ofr { get; set; }

        /// <summary>Ofr 报量，单位万（bvOfr）。</summary>
        public decimal? VolOfr { get; set; }

        /// <summary>Ofr 长城交易员（trOfr）。</summary>
        public string TraderOfr { get; set; }

        /// <summary>Ofr 报价机构（instOfr）。</summary>
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

        /// <summary>
        /// 是否已置「打包」标签。提醒行是提醒视图，Mock 阶段不携带单笔标签，恒为 false，
        /// 右键菜单 打包 项据此显示未置标签状态。
        /// </summary>
        public bool IsPacking
        {
            get { return false; }
        }

        /// <summary>是否已置「OCO」标签（同上，提醒行不携带单笔标签）。</summary>
        public bool IsOco
        {
            get { return false; }
        }
    }
}
