using System.Collections.Generic;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    /// <summary>
    /// 产品侧栏筛选面板的条件下推载荷（view.open / history.search 的 filter.conditions + 分组 Id）。
    /// 字段/运算符/值语义与后端过滤契约一致（同字段 OR、跨字段 AND）；
    /// 条件由 SidebarFilter 从面板控件态编译（远端同款），分组 Id 由服务层按激活集注入。
    /// </summary>
    public class ProductFilter
    {
        public List<FilterConditionPayload> Conditions { get; set; } = new List<FilterConditionPayload>();
    }
}
