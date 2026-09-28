namespace STC.SalesTrading.Plugin.Shared.Messages
{
    /// <summary>
    /// 待确认报价数变化（报价录入 tab 角标）。由报价录入模块发布，Shell 订阅。
    /// 消息契约统一放 Shared/Messages，跨模块通信只经 EventAggregator，模块间不直接引用。
    /// </summary>
    public class PendingQuotesChangedMessage
    {
        public PendingQuotesChangedMessage(int pendingCount)
        {
            PendingCount = pendingCount;
        }

        /// <summary>当前待确认报价条数。</summary>
        public int PendingCount { get; }
    }
}
