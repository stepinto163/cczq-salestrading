using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>懒加载搜索下拉的候选行（Name=显示名，SubText=简称/职位等辅助文本）。</summary>
    public class LazySearchItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubText { get; set; }
    }

    /// <summary>分页搜索结果（后端 CRM 查询无 totalCount，仅 hasMore 页码步进）。</summary>
    public class LazySearchResult
    {
        public IReadOnlyList<LazySearchItem> Items { get; set; } = new List<LazySearchItem>();
        public bool HasMore { get; set; }
    }
}
