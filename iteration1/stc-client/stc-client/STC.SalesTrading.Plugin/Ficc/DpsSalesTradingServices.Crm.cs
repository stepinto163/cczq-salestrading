using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Ficc
{
    public sealed partial class DpsSalesTradingServices : ICrmDirectory
    {
        public async Task<IReadOnlyList<CrmChoice>> SearchAsync(string kind, string text, string institutionId)
        {
            var result = await Api.RequestAsync(SalesTradingApi.BaseQuery, new BaseQueryRequest
            {
                Kind = kind,
                Text = text,
                InstitutionId = kind == "CONTACT" && !string.IsNullOrWhiteSpace(institutionId)
                    ? institutionId : null,
                Limit = 30
            });
            return (result.Records ?? new List<ReferenceRecord>()).Select(record => new CrmChoice
            {
                InstitutionId = record.InstitutionId,
                InstitutionName = record.InstitutionName,
                ContactId = record.ContactId,
                ContactName = record.ContactName
            }).ToList();
        }
    }
}
