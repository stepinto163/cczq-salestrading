using System;
using STC.SalesTrading.Plugin.Ficc;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 看板/管理列表统一的异步查询骨架（原 6 个 VM 各 ~55 行的逐字重复）：
    /// Begin → Loading → 载入 → 过期丢弃 / 空态 / 失败态（清行 + 错误信息 + toast）→ Dispose。
    /// 派生差异只留两处：loadAsync（构造查询、调服务、回填行，返回是否有数据；返回 null = 已过期放弃）、
    /// context（LoadState/ErrorMessage 写入 + 行计数刷新钩子）。
    /// </summary>
    internal static class QueryFlow
    {
        /// <summary>执行一次查询流程。</summary>
        /// <param name="coordinator">最新请求协调器（Begin/IsCurrent/Dispose）。</param>
        /// <param name="context">状态写入与刷新钩子（失败清行 / 成功失败后的 RecomputeStats）。</param>
        /// <param name="loadAsync">
        /// 载入体：await 服务调用；过期（lease.IsCurrent == false）时直接返回 null 放弃；
        /// 否则回填行并返回 Rows 是否非空。
        /// </param>
        /// <param name="failToastTitle">失败 toast 前缀（各 VM 的 Title）。</param>
        public static async Task RunAsync(
            LatestRequestCoordinator coordinator,
            IQueryLoadContext context,
            Func<LatestRequestCoordinator.Lease, Task<bool?>> loadAsync,
            string failToastTitle)
        {
            var lease = coordinator.Begin();
            context.LoadState = QueryLoadState.Loading;
            context.ErrorMessage = null;
            try
            {
                var hasRows = await loadAsync(lease);
                if (hasRows == null || !lease.IsCurrent) return;
                long stateStarted = ClientPerformanceTrace.Start();
                context.LoadState = hasRows.Value ? QueryLoadState.Loaded : QueryLoadState.Empty;
                ClientPerformanceTrace.End("query.state:" + context.GetType().Name, stateStarted);
                long afterStarted = ClientPerformanceTrace.Start();
                context.AfterQuery();
                ClientPerformanceTrace.End("query.after:" + context.GetType().Name, afterStarted);
            }
            catch (OperationCanceledException)
            {
                if (lease.IsCurrent) context.LoadState = QueryLoadState.Stale;
            }
            catch (Exception ex)
            {
                if (!(ex is NotSupportedException))
                {
                    com.gateway.log.LogHelper.Error(ex);
                }
                if (lease.IsCurrent)
                {
                    context.ResetForFailure();
                    context.ErrorMessage = ex.Message;
                    context.LoadState = QueryLoadState.Error;
                    context.AfterQuery();
                    ToastService.Show(failToastTitle + " 查询失败：" + ex.Message, ToastKind.Error);
                }
            }
            finally
            {
                lease.Dispose();
            }
        }
    }

    /// <summary>QueryFlow 的状态写入钩子（由各查询 VM 实现）。</summary>
    internal interface IQueryLoadContext
    {
        /// <summary>查询状态（Loading/Loaded/Empty/Error/Stale）。</summary>
        QueryLoadState LoadState { get; set; }

        /// <summary>失败信息。</summary>
        string ErrorMessage { get; set; }

        /// <summary>失败路径的清空（Rows.Clear + TotalCount/QueryId/IsPartial 复位）。</summary>
        void ResetForFailure();

        /// <summary>成功与失败路径末尾的刷新（RecomputeStats 等计数回写）。</summary>
        void AfterQuery();
    }
}
