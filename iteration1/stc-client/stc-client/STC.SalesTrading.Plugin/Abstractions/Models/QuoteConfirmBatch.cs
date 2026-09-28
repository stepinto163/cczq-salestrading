using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 待确认报价批次（报价确认页的分组头，按 批次|机构|来源 分组）。
    /// </summary>
    public class QuoteConfirmBatch
    {
        /// <summary>批次 Id（batchId）。</summary>
        public string BatchId { get; set; }

        /// <summary>来源渠道：IDEAL/QT。</summary>
        public string SourceType { get; set; }

        /// <summary>报价机构。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员。</summary>
        public string Trader { get; set; }

        /// <summary>到报时间。</summary>
        public DateTime? ReceivedAt { get; set; }

        /// <summary>批内条数。</summary>
        public int RowCount { get; set; }

        /// <summary>批次状态：待确认/部分识别失败/识别失败/部分确认。</summary>
        public BatchStatus Status { get; set; }

        /// <summary>批内识别结果主键集合。</summary>
        public IReadOnlyList<string> RecognitionIds { get; set; }
    }
}
