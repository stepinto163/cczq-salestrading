namespace STC.SalesTrading.Plugin.Shared.Messages
{
    /// <summary>未读提醒数变化（行情中心 tab 角标）。由行情中心模块发布，Shell 订阅。</summary>
    public class AlertCountChangedMessage
    {
        public AlertCountChangedMessage(int unreadCount)
        {
            UnreadCount = unreadCount;
        }

        /// <summary>当前未读提醒条数。</summary>
        public int UnreadCount { get; }
    }
}
