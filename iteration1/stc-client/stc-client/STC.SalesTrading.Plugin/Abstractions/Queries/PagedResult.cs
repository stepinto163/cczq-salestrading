using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public string QueryId { get; set; }
        public bool IsPartial { get; set; }
        public string WarningMessage { get; set; }
    }
}
