using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 债券资料过渡补取（决策 #10）：行缺 reference 时按 securityId 批量
    /// ficc.base.get {kind:SECURITY}（H42）回填。后端在 view/推送补齐 reference（决策 #9）后，
    /// 删除调用点的那一行即可，本类可整体移除。
    /// </summary>
    internal sealed class ReferenceDataCache
    {
        internal readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
        internal readonly Dictionary<string, BaseRecord> Records = new Dictionary<string, BaseRecord>(StringComparer.Ordinal);
        internal DateTime Date = DateTime.Today;
    }

    internal static class GatewayReferenceFill
    {
        /// <summary>判断行是否缺关键资料：无简称或无中债估值。</summary>
        public static async Task EnsureAsync(GatewayRequester requester, IReadOnlyList<QuoteRow> rows, CancellationToken ct)
        {
            var records = await FetchAsync(requester, rows == null ? null : rows.Select(r => r.SecurityId).ToList(), ct).ConfigureAwait(false);
            if (records == null || rows == null) return;
            foreach (var row in rows)
            {
                BaseRecord rec;
                if (row.SecurityId == null || !records.TryGetValue(row.SecurityId, out rec)) continue;
                Apply(row, rec);
            }
        }

        /// <summary>行情"全部"行补取（缺简称或估值时）。</summary>
        public static async Task EnsureTapeAsync(GatewayRequester requester, IReadOnlyList<TapeRow> rows, CancellationToken ct, ReferenceDataCache cache = null)
        {
            var records = await FetchAsync(requester, rows == null ? null : rows.Select(r => r.SecurityId).ToList(), ct, cache).ConfigureAwait(false);
            if (records == null || rows == null) return;
            foreach (var row in rows)
            {
                BaseRecord rec;
                if (row.SecurityId == null || !records.TryGetValue(row.SecurityId, out rec)) continue;
                if (row.SecurityName == null) row.SecurityName = rec.SecurityName;
                if (row.Maturity == null) row.Maturity = rec.RemainingYears;
                if (row.BondLibrary == null) row.BondLibrary = rec.BondSubtype;
                if (row.IssuerRating == null) row.IssuerRating = rec.IssuerRating;
                if (row.BondRating == null) row.BondRating = rec.BondRating;
                row.NzYtm = row.NzYtm ?? ParseDecimal(rec.ValuationYield);
                row.NzNetPrice = row.NzNetPrice ?? ParseDecimal(rec.ValuationCleanPrice);
                row.CsYtm = row.CsYtm ?? ParseDecimal(rec.CsiValuationYield);
                row.CsNetPrice = row.CsNetPrice ?? ParseDecimal(rec.CsiValuationCleanPrice);
            }
        }

        /// <summary>行情"最优"行补取（券级资料一次回填）。</summary>
        public static async Task EnsureBestAsync(GatewayRequester requester, IReadOnlyList<BestRow> rows, CancellationToken ct, ReferenceDataCache cache = null)
        {
            var records = await FetchAsync(requester, rows == null ? null : rows.Select(r => r.SecurityId).ToList(), ct, cache).ConfigureAwait(false);
            if (records == null || rows == null) return;
            foreach (var row in rows)
            {
                BaseRecord rec;
                if (row.SecurityId == null || !records.TryGetValue(row.SecurityId, out rec)) continue;
                if (row.SecurityName == null) row.SecurityName = rec.SecurityName;
                if (row.Maturity == null) row.Maturity = rec.RemainingYears;
                if (row.BondLibrary == null) row.BondLibrary = rec.BondSubtype;
                if (row.IssuerRating == null) row.IssuerRating = rec.IssuerRating;
                if (row.BondRating == null) row.BondRating = rec.BondRating;
                row.NzYtm = row.NzYtm ?? ParseDecimal(rec.ValuationYield);
                row.NzNetPrice = row.NzNetPrice ?? ParseDecimal(rec.ValuationCleanPrice);
                row.CsYtm = row.CsYtm ?? ParseDecimal(rec.CsiValuationYield);
                row.CsNetPrice = row.CsNetPrice ?? ParseDecimal(rec.CsiValuationCleanPrice);
            }
        }

        /// <summary>批量 ficc.base.get SECURITY（单请求最多 100 Id）；无缺失返回 null 跳过请求。</summary>
        private static async Task<Dictionary<string, BaseRecord>> FetchAsync(GatewayRequester requester,
            List<string> securityIds, CancellationToken ct, ReferenceDataCache cache = null)
        {
            var ids = (securityIds ?? new List<string>()).Where(id => !string.IsNullOrEmpty(id))
                .Distinct(StringComparer.Ordinal).ToList();
            if (ids.Count == 0) return null;
            var state = cache ?? new ReferenceDataCache();
            await state.Gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (state.Date != DateTime.Today)
                {
                    state.Records.Clear();
                    state.Date = DateTime.Today;
                }
                var missing = ids.Where(id => !state.Records.ContainsKey(id)).ToList();
                for (var offset = 0; offset < missing.Count; offset += 100)
                {
                    var data = await requester.RequestAsync<BaseGetRequest, BaseQueryData>(
                        GatewayTopics.BaseGet, new BaseGetRequest
                        {
                            Kind = "SECURITY",
                            Ids = missing.Skip(offset).Take(100).ToList()
                        }, ct).ConfigureAwait(false);
                    foreach (var record in data.Records ?? new List<BaseRecord>())
                    {
                        if (!string.IsNullOrEmpty(record?.SecurityId)) state.Records[record.SecurityId] = record;
                    }
                }
                return ids.Where(id => state.Records.ContainsKey(id))
                    .ToDictionary(id => id, id => state.Records[id], StringComparer.Ordinal);
            }
            finally
            {
                state.Gate.Release();
            }
        }

        private static void Apply(QuoteRow row, BaseRecord rec)
        {
            if (row.SecurityName == null) row.SecurityName = rec.SecurityName;
            if (row.Maturity == null) row.Maturity = rec.RemainingYears;
            if (row.BondLibrary == null) row.BondLibrary = rec.BondSubtype;
            if (row.IssuerRating == null) row.IssuerRating = rec.IssuerRating;
            if (row.BondRating == null) row.BondRating = rec.BondRating;
            row.NzYtm = row.NzYtm ?? ParseDecimal(rec.ValuationYield);
            row.NzNetPrice = row.NzNetPrice ?? ParseDecimal(rec.ValuationCleanPrice);
            row.CsYtm = row.CsYtm ?? ParseDecimal(rec.CsiValuationYield);
            row.CsNetPrice = row.CsNetPrice ?? ParseDecimal(rec.CsiValuationCleanPrice);
        }

        private static decimal? ParseDecimal(string text)
        {
            decimal value;
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                ? value : (decimal?)null;
        }
    }
}
