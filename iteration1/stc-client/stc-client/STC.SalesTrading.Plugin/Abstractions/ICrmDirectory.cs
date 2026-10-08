using System.Collections.Generic;
using System.Threading.Tasks;

namespace STC.SalesTrading.Plugin.Abstractions
{
    public interface ICrmDirectory
    {
        Task<IReadOnlyList<CrmChoice>> SearchAsync(string kind, string text, string institutionId);
    }

    public sealed class CrmChoice
    {
        public string InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string DisplayName => string.IsNullOrEmpty(ContactId)
            ? InstitutionName
            : ContactName + " — " + InstitutionName;
    }
}
