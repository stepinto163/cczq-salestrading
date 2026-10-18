using System;
using System.Collections.Generic;
using System.Linq;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 报价提醒行（后端 NotificationRecord）。
    /// <para>一行 = 一条通知。同券的多种非对价原因后端已合并为一条、对价独立成条，
    /// 因此不必再按 reason 拆行；一条含多个类型时 UI 显示首类并在 tooltip 给出全量。</para>
    /// </summary>
    public sealed class AlertTrigger
    {
        public string AlertKey { get; set; }
        public AlertType Type { get; set; }
        public IReadOnlyList<string> QuoteIds { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ofr { get; set; }
        public decimal? VolBid { get; set; }
        public decimal? VolOfr { get; set; }
        public string TraderBid { get; set; }
        public string TraderOfr { get; set; }
        public string PriceTypeMark { get; set; }
    }

    public class AlertRow
    {
        /// <summary>通知 Id，标记已读的操作主键（notificationId）。</summary>
        public string NotificationId { get; set; }

        /// <summary>独立触发实例；合并行成员变化不会改变未变报价的实例键。</summary>
        public IReadOnlyList<AlertTrigger> Triggers { get; set; }

        /// <summary>本通知包含的提醒类型集合（reasons[].type 去重）。</summary>
        public IReadOnlyList<AlertType> Types { get; set; }

        /// <summary>类型显示文本（原型 alertType 单元格：多类型 "/" 连接全量展示，不上色）。</summary>
        public string TypesText
        {
            get
            {
                if (Types == null || Types.Count == 0) return "提醒";
                return string.Join("/", Types.Select(AlertTypeText));
            }
        }

        /// <summary>全量类型文本（tooltip 用，顿号分隔）。</summary>
        public string TypesFullText
        {
            get
            {
                return Types == null || Types.Count == 0 ? "提醒" : string.Join("、", Types.Select(AlertTypeText));
            }
        }

        private static string AlertTypeText(AlertType type)
        {
            switch (type)
            {
                case AlertType.FavoriteBond: return "关注债券";
                case AlertType.FilterGroup: return "筛选分组";
                case AlertType.PriceMatch: return "对价提醒";
                case AlertType.MyBestQuote: return "我的最优";
                default: return "提醒";
            }
        }

        /// <summary>债券代码（securityId）。</summary>
        public string SecurityId { get; set; }

        /// <summary>债券简称（reference 补充）。</summary>
        public string SecurityName { get; set; }

        /// <summary>服务端生成的提醒文案（content）。tooltip 展示。</summary>
        public string Content { get; set; }

        /// <summary>关联报价 Id 集合（quoteIds）。</summary>
        public IReadOnlyList<string> QuoteIds { get; set; }

        /// <summary>已读时间；null 表示未读（readTime）。</summary>
        public DateTime? ReadTime { get; set; }

        /// <summary>最近修改时间，列表排序依据（updateTime）。</summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>摘要报价的发布时间；与通知更新时间分开，标记已读不会改变报价时间。</summary>
        public DateTime? QuoteTime { get; set; }

        #region 触发报价摘要（每方向最近变更的快照）

        /// <summary>买方触发报价的原始价格。</summary>
        public decimal? Bid { get; set; }

        /// <summary>卖方触发报价的原始价格。</summary>
        public decimal? Ofr { get; set; }

        /// <summary>买方报量，单位万。</summary>
        public decimal? VolBid { get; set; }

        /// <summary>卖方报量，单位万。</summary>
        public decimal? VolOfr { get; set; }

        /// <summary>报价机构名称。</summary>
        public string InstitutionName { get; set; }

        /// <summary>报价联系人名称。</summary>
        public string ContactName { get; set; }

        /// <summary>报价归属用户显示名。为空时回退显示 OwnerUserId。</summary>
        public string OwnerUserName { get; set; }

        // 每方向的价、量、机构、联系人和归属人来自同一笔触发报价，不拼接其他报价字段。
        public string ContactBid { get; set; }

        public string ContactOfr { get; set; }

        public string InstBid { get; set; }

        public string TraderBid { get; set; }

        public string InstOfr { get; set; }

        public string TraderOfr { get; set; }

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

        /// <summary>中债偏离，BP。</summary>
        public decimal? NzDeviation { get; set; }

        // 中证估值由基础资料补齐；数据源未提供的全价保持空，不能用净价冒充。
        public decimal? CsNetPrice { get; set; }

        public decimal? CsFullPrice { get; set; }

        public decimal? CsYtm { get; set; }

        #endregion

        /// <summary>是否已读。</summary>
        public bool IsRead
        {
            get { return ReadTime.HasValue; }
        }
    }
}
