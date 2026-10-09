using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>查询 CRM，并将联系人与所属机构解析成可明确选择的组合。</summary>
    public sealed class DpsCrmDirectory : ICrmDirectory
    {
        private readonly FiccApiClient api;

        public DpsCrmDirectory(FiccApiClient api)
        {
            this.api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task<IReadOnlyList<CrmChoice>> SearchAsync(string kind, string text, string institutionId)
        {
            string scope = kind == "CONTACT" && !string.IsNullOrWhiteSpace(institutionId)
                ? institutionId : null;
            var result = await api.RequestAsync(SalesTradingApi.BaseQuery, new BaseQueryRequest
            {
                Kind = kind,
                Text = text,
                InstitutionId = scope,
                Limit = 30
            });
            var records = result.Records ?? new List<ReferenceRecord>();
            if (kind != "CONTACT")
            {
                return records.Select(record => new CrmChoice
                {
                    InstitutionId = record.InstitutionId,
                    InstitutionName = record.InstitutionName
                }).ToList();
            }

            // CONTACT 返回机构 ID 关联，而非机构名称；批量补齐，避免每个联系人各发一次请求。
            var ids = records.SelectMany(record => InstitutionIds(record, scope)).Distinct().ToList();
            var institutions = new Dictionary<string, string>();
            for (int offset = 0; offset < ids.Count; offset += 200)
            {
                var resolved = await api.RequestAsync(SalesTradingApi.BaseGet, new BaseGetRequest
                {
                    Kind = "INSTITUTION",
                    Ids = ids.Skip(offset).Take(200).ToList()
                });
                foreach (var institution in resolved.Records ?? new List<ReferenceRecord>())
                {
                    if (!string.IsNullOrWhiteSpace(institution.InstitutionId))
                    {
                        institutions[institution.InstitutionId] = institution.InstitutionName;
                    }
                }
            }

            var choices = new List<CrmChoice>();
            foreach (var contact in records)
            {
                var affiliations = InstitutionIds(contact, scope).ToList();
                if (affiliations.Count == 0 && scope == null)
                {
                    throw new InvalidOperationException("CRM 联系人未关联机构：" + contact.ContactName);
                }
                foreach (string id in affiliations)
                {
                    if (!institutions.TryGetValue(id, out string name) || string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidOperationException("CRM 联系人所属机构未找到：" + contact.ContactName + " / " + id);
                    }
                    // 多机构联系人按机构分别展示，不能默选第一个机构。
                    choices.Add(new CrmChoice
                    {
                        ContactId = contact.ContactId,
                        ContactName = contact.ContactName,
                        InstitutionId = id,
                        InstitutionName = name
                    });
                }
            }
            return choices;
        }

        private static IEnumerable<string> InstitutionIds(ReferenceRecord contact, string scope)
        {
            return (contact.InstitutionIds ?? new List<string>())
                .Concat(new[] { contact.InstitutionId })
                .Where(id => !string.IsNullOrWhiteSpace(id) && (scope == null || id == scope))
                .Distinct();
        }
    }
}
