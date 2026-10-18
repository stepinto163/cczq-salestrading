using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 报价日志条目（看板行右键"报价日志"弹窗行，ficc.quote.history.query H10）。
    /// 只带展示字段；before/after 全量快照不进 UI。
    /// </summary>
    /// <summary>单条报价的变更历史分页；版本边界由服务端返回并在翻页时保留。</summary>
    public sealed class QuoteLogPage
    {
        public System.Collections.Generic.IReadOnlyList<QuoteLogEntry> Records { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string SnapshotRevision { get; set; }
    }

    public class QuoteLogEntry
    {
        /// <summary>操作时间。</summary>
        public DateTime? OperationTime { get; set; }

        /// <summary>动作 CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE。</summary>
        public string Action { get; set; }

        /// <summary>操作后的版本。</summary>
        public string Revision { get; set; }

        /// <summary>审计中的操作人主键，保留原值用于追溯。</summary>
        public string OperatorUserId { get; set; }

        /// <summary>操作人的 STC 登录名；无法解析时明确显示未知用户。</summary>
        public string OperatorUserName { get; set; }

        /// <summary>原因（refer/unrefer/delete 可携带）。</summary>
        public string Reason { get; set; }
    }
}
