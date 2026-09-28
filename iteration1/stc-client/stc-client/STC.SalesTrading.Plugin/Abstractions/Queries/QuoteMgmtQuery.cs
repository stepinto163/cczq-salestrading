using System;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    /// <summary>
    /// 报价管理查询语义。字段只表达 UI 筛选意图，不绑定后端传输 DTO。
    /// </summary>
    public sealed class QuoteMgmtQuery
    {
        public bool Today { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Mode { get; set; }
        public string Status { get; set; }
        public string BondSearchText { get; set; }
        public string InstitutionSearchText { get; set; }
        public string TraderSearchText { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 200;
        public string SortField { get; set; }
        public bool SortDescending { get; set; }
    }
}
