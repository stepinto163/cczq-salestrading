using System.Collections.Generic;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 产品侧栏筛选面板的宿主接缝：面板（SidebarFilter）经 DataContext 取筛选字典（Configure 填充候选）、
    /// 把编译好的条件按市场范围（Credit/Rate）下发给容器 VM 转发各看板重查（远端 IProductFilterHost 同构）。
    /// </summary>
    public interface IProductFilterHost
    {
        /// <summary>按 scope（Credit/Rate）应用筛选条件（看板取消当前查询并重查）。</summary>
        void ApplyProductFilter(string scope, Queries.ProductFilter filter);

        /// <summary>筛选字段字典（ficc.quote.dictionary.query，filterFields；候选兜底已含业务字典）。</summary>
        Task<IReadOnlyList<FilterFieldEntry>> GetFilterDictionaryAsync();
    }
}
