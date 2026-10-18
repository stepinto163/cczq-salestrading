using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 报价确认页的分组头（按 sourceMessageId 分组，无该键时回退 机构|来源）。
    /// 这是 UI 分组概念，不是后端模型。
    /// </summary>
    public class QuoteConfirmBatch
    {
        /// <summary>分组键：来源消息 Id，或 机构|来源 回退键。</summary>
        public string BatchId { get; set; }

        /// <summary>来源渠道：IDEAL/QT/MANUAL。</summary>
        public string SourceType { get; set; }

        /// <summary>报价机构。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员。</summary>
        public string Trader { get; set; }

        /// <summary>首条报价的创建时间（组头展示"到报时间"）。</summary>
        public DateTime? ReceivedAt { get; set; }

        /// <summary>批内条数。</summary>
        public int RowCount { get; set; }

        /// <summary>分组状态：待确认/有缺失项。</summary>
        public BatchStatus Status { get; set; }
    }
}
