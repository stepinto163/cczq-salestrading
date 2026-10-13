namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 智能推荐行（行情中心右侧详情面板 rec-panel 的 7 列表：
    /// 选择 / 推荐度 / 机构·交易员 / 代码·简称 / 报价次数 / 最近报价·报量(万) / 长城交易员）。
    /// 正式报价证据投影；敏感身份字段由服务端按证据归属脱敏。
    /// </summary>
    public class RecommendationRow : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        /// <summary>稳定推荐条目标识，用于一次性反馈。</summary>
        public string RecommendationId { get; set; }

        /// <summary>是否模拟数据（表内显示"模拟"角标）。</summary>
        public bool IsMock { get; set; }

        /// <summary>所属方向（Bid/Ofr），用于左右方向切换过滤。</summary>
        public QuoteDirection Side { get; set; }

        /// <summary>推荐路径（本券/相似券/相似机构）。"全部"态不过滤。</summary>
        public RecommendationScope Scope { get; set; }

        /// <summary>推荐度（0-100）。</summary>
        public double Score { get; set; }

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

        /// <summary>最近报价数值；口径由 PriceTypeName 标明。</summary>
        public decimal? LatestPrice { get; set; }

        /// <summary>最近报量（万）。</summary>
        public decimal? LatestVolume { get; set; }

        /// <summary>长城（我方）交易员。</summary>
        public string OwnerTrader { get; set; }

        /// <summary>是否勾选（"发送报价"用，Mock 阶段仅统计不落库）。</summary>
        public bool IsSelected { get; set; }

        public string PriceTypeName { get; set; }
        public System.Collections.Generic.IReadOnlyList<RecommendationEvidenceRow> Evidence { get; set; }
        private string feedback;
        public string Feedback
        {
            get { return feedback; }
            set
            {
                if (SetProperty(ref feedback, value))
                {
                    OnPropertyChanged(nameof(CanFeedback));
                    OnPropertyChanged(nameof(FeedbackText));
                }
            }
        }
        public bool CanFeedback => string.IsNullOrEmpty(Feedback);
        public string FeedbackText => Feedback == "USEFUL" ? "已反馈：有用"
            : Feedback == "NOT_USEFUL" ? "已反馈：无用"
            : Feedback == "NOT_APPLICABLE" ? "已反馈：不适用" : "未反馈";
    }

    public class RecommendationEvidenceRow
    {
        public string QuoteId { get; set; }
        public string Time { get; set; }
        public string Price { get; set; }
        public string PriceTypeName { get; set; }
        public string Quantity { get; set; }
        public string Institution { get; set; }
        public string Trader { get; set; }
        public string OwnerTrader { get; set; }
    }
}