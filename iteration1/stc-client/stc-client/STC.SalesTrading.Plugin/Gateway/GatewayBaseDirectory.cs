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

            // CONTACT 应答只保证机构 ID；一次批量补齐本页机构，避免逐联系人查询。
            if (kind == "CONTACT")
            {
                var contacts = (data.Records ?? new List<BaseRecord>()).Where(record => record != null).ToList();
                var institutionIds = contacts.SelectMany(contact => ContactInstitutions(contact))
                    .Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                if (institutionIds.Count > 0)
                {
                    var byId = new Dictionary<string, BaseRecord>(StringComparer.Ordinal);
                    for (var index = 0; index < institutionIds.Count; index += 100)
                    {
                        var institutions = await requester.RequestAsync<BaseGetRequest, BaseQueryData>(GatewayTopics.BaseGet,
                            new BaseGetRequest { Kind = "INSTITUTION", Ids = institutionIds.Skip(index).Take(100).ToList() }, ct).ConfigureAwait(false);
                        foreach (var institution in institutions.Records ?? new List<BaseRecord>())
                        {
                            if (institution?.InstitutionId != null) byId[institution.InstitutionId] = institution;
                        }
                    }
                    var expanded = new List<BaseRecord>();
                    foreach (var contact in contacts)
                    {
                        var matches = ContactInstitutions(contact);
                        foreach (var id in matches.Where(id => string.IsNullOrEmpty(institutionId) || id == institutionId))
                        {
                            BaseRecord institution;
                            if (id == null || !byId.TryGetValue(id, out institution))
                            {
                                throw new InvalidOperationException("联系人所属机构资料缺失：" + contact.ContactId);
                            }
                            expanded.Add(new BaseRecord
                            {
                                ContactId = contact.ContactId,
                                ContactName = contact.ContactName,
                                InstitutionId = id,
                                InstitutionIds = matches,
                                InstitutionName = institution.InstitutionName
                            });
                        }
                    }
                    data.Records = expanded;
                }
            }
            var items = (data.Records ?? new List<BaseRecord>())
                .Where(r => r != null && (kind == "CONTACT" ? r.ContactId : r.InstitutionId) != null)
                .Select(r => kind == "CONTACT"
                    ? new LazySearchItem
                    {
                        Id = r.ContactId,
                        Name = r.ContactName,
                        SubText = r.InstitutionName,
                        InstitutionId = r.InstitutionId,
                        InstitutionName = r.InstitutionName,
                        InstitutionIds = r.InstitutionIds
                    }
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

        private static List<string> ContactInstitutions(BaseRecord contact)
        {
            return (contact.InstitutionIds ?? new List<string>()).Concat(new[] { contact.InstitutionId })
                .Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
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
