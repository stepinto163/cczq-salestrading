using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>看板行 → 关联报价身份解析（右键 Refer/打包/OCO/日志共用）。</summary>
    internal static class BoardRowQuoteResolver
    {
        /// <summary>行关联的报价 Id；聚合/提醒行取首个关联，无关联返回 null。</summary>
        public static string ResolveQuoteId(object row)
        {
            if (row is TapeRow tape) return tape.QuoteId;
            if (row is BestRow best) return best.SourceQuoteIds != null && best.SourceQuoteIds.Length > 0 ? best.SourceQuoteIds[0] : null;
            if (row is AlertRow alert) return alert.QuoteIds != null && alert.QuoteIds.Count > 0 ? alert.QuoteIds[0] : null;
            return null;
        }

        /// <summary>行关联报价的版本（乐观锁）；仅"全部"页行携带，其余返回 null。</summary>
        public static string ResolveRevision(object row)
        {
            return row is TapeRow tape ? tape.Revision : null;
        }

        /// <summary>行展示前缀（代码）。</summary>
        public static string ResolveLabel(object row)
        {
            if (row is TapeRow tape) return tape.SecurityId;
            if (row is BestRow best) return best.SecurityId;
            if (row is AlertRow alert) return alert.SecurityId;
            return null;
        }
    }
}
