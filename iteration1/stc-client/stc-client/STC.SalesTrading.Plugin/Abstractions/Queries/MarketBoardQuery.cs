using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    public sealed class MarketBoardQuery
    {
        public BondMarket Market { get; set; }
        public bool IncludeBid { get; set; } = true;
        public bool IncludeOfr { get; set; } = true;
        public bool MineOnly { get; set; }
        public QuoteFlag RequiredFlags { get; set; }
        public bool UnquotedOnly { get; set; }
        public string SearchText { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 200;
        public string SortField { get; set; }
        public bool SortDescending { get; set; }
    }
}
