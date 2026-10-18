using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Recommendation;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 智能推荐网关实现（ficc.recommendation.query / feedback）。
    /// side "1"=Bid；价格口径：quoteType 2=净价、3=全价，否则 yieldType 2=行权收益率、其余到期收益率。
    /// ownerUserId 经组合根注入的用户名映射转显示名。
    /// </summary>
    internal sealed class GatewayRecommendationService : IRecommendationService
    {
        private readonly GatewayRequester requester;
        private readonly Func<string, string> ownerName;

        public GatewayRecommendationService(GatewayRequester requester, Func<string, string> ownerName)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.ownerName = ownerName ?? (id => id);
        }

        public async Task<IReadOnlyList<RecommendationRow>> QueryAsync(string securityId, DateTime start, DateTime end, CancellationToken ct)
        {
            var data = await requester.RequestAsync<RecommendationQueryRequest, RecommendationQueryData>(
                GatewayTopics.RecommendationQuery,
                new RecommendationQueryRequest
                {
                    SecurityId = securityId,
                    StartDate = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                }, ct).ConfigureAwait(false);

            var records = data != null ? data.Recommendations : null;
            return (records ?? new List<RecommendationRecord>()).Where(item => item != null).Select(item => new RecommendationRow
            {
                RecommendationId = item.RecommendationId,
                IsMock = false,
                Side = item.Side == "1" ? QuoteDirection.Bid : QuoteDirection.Ofr,
                Scope = RecommendationScope.ExactBond,
                Score = (int)(Number(item.Score) ?? 0m),
                InstitutionName = item.InstitutionName,
                TraderName = item.ContactName,
                RelatedCode = item.SecurityId,
                RelatedName = item.SecurityName,
                QuoteCount = (int)(Number(item.QuoteCount) ?? 0),
                LatestPrice = Number(item.LatestPrice),
                LatestVolume = Number(item.LatestQuantity),
                PriceTypeName = PriceType(item.QuoteType, item.YieldType),
                OwnerTrader = ownerName(item.OwnerUserId),
                Feedback = item.Feedback,
                Evidence = (item.Evidence ?? new List<RecommendationEvidence>()).Where(e => e != null).Select(e => new RecommendationEvidenceRow
                {
                    QuoteId = e.QuoteId,
                    Side = e.Side == "1" ? QuoteDirection.Bid : QuoteDirection.Ofr,
                    Price = Number(e.Price),
                    Quantity = Number(e.Quantity),
                    PriceTypeName = PriceType(e.QuoteType, e.YieldType),
                    Institution = e.InstitutionName,
                    Trader = e.ContactName,
                    OwnerTrader = ownerName(e.OwnerUserId),
                    Time = e.UpdateTime,
                }).ToList(),
            }).ToList();
        }

        public async Task<string> SubmitFeedbackAsync(string securityId, DateTime start, DateTime end, string recommendationId, string feedback, CancellationToken ct)
        {
            var result = await requester.RequestAsync<RecommendationFeedbackRequest, RecommendationFeedbackData>(
                GatewayTopics.RecommendationFeedback,
                new RecommendationFeedbackRequest
                {
                    RecommendationId = recommendationId,
                    Feedback = feedback,
                }, ct).ConfigureAwait(false);
            return result != null ? result.Feedback : feedback;
        }

        private static decimal? Number(string value)
        {
            decimal result;
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ? result : (decimal?)null;
        }

        private static string PriceType(string quoteType, string yieldType)
        {
            return quoteType == "2" ? "净价" : quoteType == "3" ? "全价" : yieldType == "2" ? "行权收益率" : "到期收益率";
        }
    }
}
