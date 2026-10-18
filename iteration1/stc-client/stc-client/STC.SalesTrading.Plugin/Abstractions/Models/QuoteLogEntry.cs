using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 报价日志条目（看板行右键"报价日志"弹窗行，ficc.quote.history.query H10）。
    /// 只带展示字段；before/after 全量快照不进 UI。
    /// </summary>
    public class QuoteLogEntry
    {
        /// <summary>操作时间。</summary>
        public DateTime? OperationTime { get; set; }

        /// <summary>动作 CREATE / UPDATE / CONFIRM / REFER / UNREFER / DELETE。</summary>
        public string Action { get; set; }

        /// <summary>操作后的版本。</summary>
        public string Revision { get; set; }

        /// <summary>操作人。</summary>
        public string OperatorUserId { get; set; }

        /// <summary>原因（refer/unrefer/delete 可携带）。</summary>
        public string Reason { get; set; }
    }
}
