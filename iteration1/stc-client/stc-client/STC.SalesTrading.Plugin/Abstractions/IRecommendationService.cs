using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    public interface IRecommendationService
    {
        Task<IReadOnlyList<RecommendationRow>> QueryRecommendationsAsync(string securityId, DateTime start, DateTime end, CancellationToken cancellation);
        Task<string> SubmitFeedbackAsync(string securityId, DateTime start, DateTime end, string recommendationId, string feedback);
    }
}
