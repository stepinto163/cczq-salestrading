namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>报价变更事件（推送通道契约，对应原型 SSE new_quote/edit_quote/revoke_quote）。</summary>
    public class QuoteEvent
    {
        /// <summary>事件类型。</summary>
        public QuoteEventKind Kind { get; set; }

        /// <summary>报价 Id。</summary>
        public string QuoteId { get; set; }

        /// <summary>状态修订号（stateRevision，缺口&gt;1 触发全量刷新）。</summary>
        public long StateRevision { get; set; }

        /// <summary>本批实际变化的报价数（冻结/隐藏态横幅按此累加；0 视为 1）。</summary>
        public int ChangedCount { get; set; }
    }
}
