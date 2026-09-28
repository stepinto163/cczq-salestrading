using common.netapi.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class ApiManager : IApiManager
    {
        private Dictionary<string, IApi> _dicApi = new Dictionary<string, IApi>();
        private string _defaultApiKey = "1";
        public static string _pluginSign = "netapi.dll"; //所有的插件都以"netapi.dll"结尾
        private static IApiManager clientApi = new ApiManager();


        public static IApiManager Instance { get { return clientApi; } }

        private ApiManager()
        {
            LoadAllPlugins(AppDomain.CurrentDomain.BaseDirectory, true, _pluginSign);

            if (_dicApi.Count > 0)
            {
                foreach (KeyValuePair<string, IApi> kv in _dicApi)
                    kv.Value.Init();
            }
        }

        public void SetDefaultApi(string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
                _defaultApiKey = apiKey;
        }


        private void LoadAllPlugins(string pluginFolderPath, bool searchChildFolder, string pluginSign)
        {
            var config = new TypeLoadConfig(false, false, pluginSign);
            IList<Type> pluginTypeList = ReflectionHelper.LoadDerivedType(typeof(IApi), pluginFolderPath, searchChildFolder, config);
            for (int i = 0; i < pluginTypeList.Count; i++)
            {
                var plugin = (IApi)Activator.CreateInstance(pluginTypeList[i]);
                this.AddApi(plugin);
            }
        }


        public void AddApi(IApi api)
        {
            if (!_dicApi.ContainsKey(api.ApiKey))
            {
                _dicApi.Add(api.ApiKey, api);
            }
            else
            {
                _dicApi.Remove(api.ApiKey);
                _dicApi.Add(api.ApiKey, api);
            }
        }

        public IApi GetDefaultApi()
        {
            return GetApi(_defaultApiKey);
        }

        public IApi GetApi(string apiKey)
        {
            if (_dicApi.ContainsKey(apiKey))
            {
                return _dicApi[apiKey];
            }

            return null;
        }

        public IApi GetApiByName(string conName)
        {
            return GetApi(conName);
        }


        public IApi CreateNewApi(string apiKey, string conName)
        {
            IApi api = GetApi(apiKey);

            IApi newApi = (IApi)Activator.CreateInstance(api.GetType());
            newApi.NewApi();
            newApi.ApiKey = conName;
            if (!_dicApi.ContainsKey(conName))
                _dicApi.Add(conName, newApi);

            return newApi;
        }


        public void RemoveApi(IApi api)
        {
            if (api == null || api.ApiKey == null)
                return;
            if (_dicApi.ContainsKey(api.ApiKey))
            {
                _dicApi.Remove(api.ApiKey);
            }
        }
    }
}
