using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    /// <summary>行情看板查询语义。字段只表达 UI 筛选意图，不绑定后端传输 DTO。</summary>
    public sealed class MarketBoardQuery
    {
        public BondMarket Market { get; set; }
        public bool IncludeBid { get; set; } = true;
        public bool IncludeOfr { get; set; } = true;
        public QuoteFlag RequiredFlags { get; set; }
        public string SearchText { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 200;
        public string SortField { get; set; }

        /// <summary>侧栏筛选面板条件下推载荷（面板条件 + 分组 Id 由服务层注入）。</summary>
        public ProductFilter ProductFilter { get; set; }
        public bool SortDescending { get; set; }
    }
}
