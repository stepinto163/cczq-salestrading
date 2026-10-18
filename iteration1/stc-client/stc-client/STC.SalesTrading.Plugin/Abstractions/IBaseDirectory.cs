using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 基础资料 / CRM 联想目录（数据接缝）。后端 ficc.base.query（H43）；
    /// CrmSearchBox 组件经它取候选，网关实现见 Gateway/GatewayBaseDirectory。
    /// </summary>
    public interface IBaseDirectory
    {
        /// <summary>
        /// CRM 包含搜索（不区分大小写）。kind 取 INSTITUTION / CONTACT；
        /// CONTACT 可传 institutionId 限定当前关联机构（传 null 跨机构搜索）。
        /// </summary>
        Task<IReadOnlyList<CrmDirectoryEntry>> SearchAsync(string kind, string text, string institutionId, CancellationToken ct);

        /// <summary>CRM 分页搜索（懒加载下拉）：返回一页候选与 hasMore（后端无总数，页码步进）。</summary>
        Task<LazySearchResult> SearchPageAsync(
            string kind, string text, string institutionId, int offset, int limit, CancellationToken ct);
    }
}
