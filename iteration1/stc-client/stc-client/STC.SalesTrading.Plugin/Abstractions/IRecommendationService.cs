using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 智能推荐服务（详情面板 rec-panel 数据源，ficc.recommendation.query / feedback）。
    /// 查询按债券 + 时间窗口；反馈一次性（USEFUL / NOT_USEFUL / NOT_APPLICABLE）。
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>查询某债券在窗口内的推荐行（不含未反馈过滤，前端按 Feedback 呈现）。</summary>
        Task<IReadOnlyList<RecommendationRow>> QueryAsync(string securityId, DateTime start, DateTime end, CancellationToken ct);

        /// <summary>提交推荐反馈，返回服务端确认的最终状态。</summary>
        Task<string> SubmitFeedbackAsync(string securityId, DateTime start, DateTime end, string recommendationId, string feedback, CancellationToken ct);
    }
}
