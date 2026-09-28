using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public interface IApiManager
    {
        void SetDefaultApi(string apiKey);

        IApi GetDefaultApi();

        void AddApi(IApi api);
        void RemoveApi(IApi api);
        IApi CreateNewApi(string apiKey, string conName);

        IApi GetApiByName(string conName);

        IApi GetApi(string apiKey);

    }
}
