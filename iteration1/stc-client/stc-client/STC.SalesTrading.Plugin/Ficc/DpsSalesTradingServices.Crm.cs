using System.Collections.Generic;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;

namespace STC.SalesTrading.Plugin.Ficc
{
    public sealed partial class DpsSalesTradingServices : ICrmDirectory
    {
        private ICrmDirectory crmDirectory;

        public Task<IReadOnlyList<CrmChoice>> SearchAsync(string kind, string text, string institutionId)
        {
            if (crmDirectory == null) crmDirectory = new DpsCrmDirectory(Api);
            return crmDirectory.SearchAsync(kind, text, institutionId);
        }
    }
}
