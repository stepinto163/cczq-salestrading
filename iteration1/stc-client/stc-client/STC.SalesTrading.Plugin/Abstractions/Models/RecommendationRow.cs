using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 智能推荐行（行情中心右侧详情面板 rec-panel 的 7 列表：
    /// 选择 / 推荐度 / 机构·交易员 / 代码·简称 / 报价次数 / 最近报价·报量(万) / 长城交易员）。
    /// 对应原型 app.js recommendationMockRows 的行形态。
    /// </summary>
    public class RecommendationRow
    {
        /// <summary>推荐 id（Mock 阶段生成）。</summary>
        public string RecommendationId { get; set; }

        /// <summary>是否模拟数据（表内显示"模拟"角标）。</summary>
        public bool IsMock { get; set; }

        /// <summary>所属方向（Bid/Ofr），用于左右方向切换过滤。</summary>
        public QuoteDirection Side { get; set; }

        /// <summary>推荐路径（本券/相似券/相似机构）。"全部"态不过滤。</summary>
        public RecommendationScope Scope { get; set; }

        /// <summary>推荐度（0-100）。</summary>
        public int Score { get; set; }

        /// <summary>推荐档位（A/B/C），由 Score 派生。</summary>
        public string Grade { get; set; }

        /// <summary>机构名。</summary>
        public string InstitutionName { get; set; }

        /// <summary>机构侧交易员名。</summary>
        public string TraderName { get; set; }

        /// <summary>相关债券代码。</summary>
        public string RelatedCode { get; set; }

        /// <summary>相关债券简称。</summary>
        public string RelatedName { get; set; }

        /// <summary>报价次数。</summary>
        public int QuoteCount { get; set; }

        /// <summary>最近报价价（收益率口径）。</summary>
        public decimal? LatestPrice { get; set; }

        /// <summary>最近报量（万）。</summary>
        public decimal? LatestVolume { get; set; }

        /// <summary>长城（我方）交易员。</summary>
        public string OwnerTrader { get; set; }

        /// <summary>最近报价的价格口径（净价/全价/行权收益率/到期收益率）。</summary>
        public string PriceTypeName { get; set; }

        /// <summary>反馈状态：USEFUL / NOT_USEFUL / NOT_APPLICABLE；null=未反馈（可反馈）。</summary>
        public string Feedback { get; set; }

        /// <summary>还可反馈（一次性：已有反馈状态后不可再点）。</summary>
        public bool CanFeedback
        {
            get { return string.IsNullOrEmpty(Feedback); }
        }

        /// <summary>证据明细（正式报价，RowDetails 子表）。</summary>
        public List<RecommendationEvidenceRow> Evidence { get; set; }

        /// <summary>是否勾选（"发送报价"用，Mock 阶段仅统计不落库）。</summary>
        public bool IsSelected { get; set; }
    }
}