using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>智能推荐独立适配器；真实 DPS 应答映射到视图，不使用 Mock 回填。</summary>
    public sealed class DpsRecommendationService : IRecommendationService
    {
        private readonly FiccApiClient api;
        private readonly Func<string, string> ownerName;
        public DpsRecommendationService(FiccApiClient api, Func<string, string> ownerName)
        {
            this.api = api;
            this.ownerName = ownerName;
        }

        public async Task<IReadOnlyList<RecommendationRow>> QueryRecommendationsAsync(string securityId, DateTime start, DateTime end, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var data = await api.RequestAsync(SalesTradingApi.RecommendationQuery, new RecommendationQueryRequest
            {
                SecurityId = securityId, StartDate = start.ToString("yyyy-MM-dd"), EndDate = end.ToString("yyyy-MM-dd")
            });
            cancellation.ThrowIfCancellationRequested();
            return (data.Recommendations ?? new List<RecommendationRecord>()).Select(item => new RecommendationRow
            {
                RecommendationId = item.RecommendationId, Side = item.Side == "1" ? QuoteDirection.Bid : QuoteDirection.Ofr,
                Scope = RecommendationScope.ExactBond, Score = item.Score ?? 0,
                InstitutionName = item.InstitutionName, TraderName = item.ContactName,
                RelatedCode = item.SecurityId, RelatedName = item.SecurityName, QuoteCount = item.QuoteCount ?? 0,
                LatestPrice = Number(item.LatestPrice), LatestVolume = Number(item.LatestQuantity),
                PriceTypeName = PriceType(item.QuoteType, item.YieldType),
                OwnerTrader = ownerName(item.OwnerUserId), Feedback = item.Feedback,
                Evidence = (item.Evidence ?? new List<RecommendationEvidence>()).Select(detail => new RecommendationEvidenceRow
                {
                    QuoteId = detail.QuoteId, Time = detail.UpdateTime, Price = detail.Price, Quantity = detail.Quantity,
                    PriceTypeName = PriceType(detail.QuoteType, detail.YieldType),
                    Institution = detail.InstitutionName, Trader = detail.ContactName, OwnerTrader = ownerName(detail.OwnerUserId)
                }).ToArray()
            }).ToArray();
        }

        private static decimal? Number(string value)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result) ? result : (decimal?)null;
        }

        private static string PriceType(string quoteType, string yieldType)
        {
            return quoteType == "2" ? "净价" : quoteType == "3" ? "全价" : yieldType == "2" ? "行权收益率" : "到期收益率";
        }

        public async Task<string> SubmitFeedbackAsync(string securityId, DateTime start, DateTime end, string recommendationId, string feedback)
        {
            var result = await api.RequestAsync(SalesTradingApi.RecommendationFeedback, new RecommendationFeedbackRequest
            {
                SecurityId = securityId, StartDate = start.ToString("yyyy-MM-dd"), EndDate = end.ToString("yyyy-MM-dd"),
                RecommendationId = recommendationId, Feedback = feedback
            });
            return result.Feedback;
        }
    }
}
