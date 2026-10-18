using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>CRM 联想目录（ficc.base.query，H43 CRM 分支：text+offset 分页，不返回 totalCount）。</summary>
    internal sealed class GatewayBaseDirectory : IBaseDirectory
    {
        private readonly GatewayRequester requester;

        public GatewayBaseDirectory(GatewayRequester requester)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
        }

        /// <summary>CRM 分页搜索（懒加载下拉）：offset/limit 直传后端，hasMore 透出。</summary>
        public async Task<LazySearchResult> SearchPageAsync(
            string kind, string text, string institutionId, int offset, int limit, CancellationToken ct)
        {
            if (kind != "INSTITUTION" && kind != "CONTACT")
            {
                throw new ArgumentException("kind 仅支持 INSTITUTION / CONTACT", "kind");
            }
            var request = new BaseQueryRequest
            {
                Kind = kind,
                Text = text ?? string.Empty,
                InstitutionId = kind == "CONTACT" ? institutionId : null,
                Offset = Math.Max(0, offset),
                Limit = Math.Max(1, limit),
            };
            var data = await requester.RequestAsync<BaseQueryRequest, BaseQueryData>(
                GatewayTopics.BaseQuery, request, ct).ConfigureAwait(false);

            var items = (data.Records ?? new List<BaseRecord>())
                .Where(r => r != null && (kind == "CONTACT" ? r.ContactId : r.InstitutionId) != null)
                .Select(r => kind == "CONTACT"
                    ? new LazySearchItem { Id = r.ContactId, Name = r.ContactName, SubText = r.PositionName }
                    : new LazySearchItem
                    {
                        Id = r.InstitutionId,
                        Name = r.InstitutionName,
                        // 机构框以简称为主显示与回填文本，缺失时兜底全称
                        SubText = string.IsNullOrEmpty(r.InstitutionShortName) ? r.InstitutionName : r.InstitutionShortName,
                    })
                .ToList();
            return new LazySearchResult
            {
                Items = items,
                HasMore = data.HasMore == true,
            };
        }

        public async Task<IReadOnlyList<CrmDirectoryEntry>> SearchAsync(string kind, string text, string institutionId, CancellationToken ct)
        {
            if (kind != "INSTITUTION" && kind != "CONTACT")
            {
                throw new ArgumentException("kind 仅支持 INSTITUTION / CONTACT", "kind");
            }
            var request = new BaseQueryRequest
            {
                Kind = kind,
                Text = text ?? string.Empty,
                InstitutionId = kind == "CONTACT" ? institutionId : null,
                Offset = 0,
                Limit = 50,
            };
            var data = await requester.RequestAsync<BaseQueryRequest, BaseQueryData>(
                GatewayTopics.BaseQuery, request, ct).ConfigureAwait(false);

            return (data.Records ?? new List<BaseRecord>())
                .Where(r => r != null)
                .Select(r => kind == "CONTACT"
                    ? new CrmDirectoryEntry
                    {
                        Id = r.ContactId,
                        Name = r.ContactName,
                        InstitutionIds = r.InstitutionIds,
                        InstitutionId = r.InstitutionId,
                        PositionName = r.PositionName,
                    }
                    : new CrmDirectoryEntry
                    {
                        Id = r.InstitutionId,
                        Name = r.InstitutionName,
                        ShortName = r.InstitutionShortName,
                    })
                .Where(e => e.Id != null)
                .ToList();
        }
    }
}
