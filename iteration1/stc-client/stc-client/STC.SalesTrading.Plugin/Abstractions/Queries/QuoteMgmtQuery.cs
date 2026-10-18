using System;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    /// <summary>
    /// 报价管理查询语义。字段只表达 UI 筛选意图，不绑定后端传输 DTO。
    /// <para>无对应后端能力的字段已移除：来源（QT/ideal）、长城交易员筛选——
    /// 后端 QuoteView 目前不接受这两类条件，强映射只会得到服务端报错。</para>
    /// </summary>
    public sealed class QuoteMgmtQuery
    {
        public bool Today { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string BondSearchText { get; set; }
        public string InstitutionSearchText { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 200;
        public string SortField { get; set; }
        public bool SortDescending { get; set; }

        /// <summary>
        /// 历史查询续取游标（上次应答 nextCursor）。游标绑定权限/条件/资料快照：
        /// 条件或日期变更必须置 null 重查（1007 过期由实现抛出，UI 提示重查）。
        /// </summary>
        public string Cursor { get; set; }
    }
}
