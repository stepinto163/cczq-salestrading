using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Cache
{
    public class SettleCache : ISettleCache
    {
        private SettleCache() { }
        private static ISettleCache server = new SettleCache();


        public static ISettleCache Instance { get { return server; } }
        
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _settleDic = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public void AddOrUpdate(string groupId, string id, string settle)
        {
            if (_settleDic.ContainsKey(groupId))
            {
                ConcurrentDictionary<string, string> securityDic = _settleDic[groupId];
                securityDic.AddOrUpdate(id, settle, (key, value) => settle);
            }
            else
            {
                ConcurrentDictionary<string, string> securityDic = new ConcurrentDictionary<string, string>();
                securityDic.AddOrUpdate(id, settle, (key, value) => settle);

                _settleDic.AddOrUpdate(groupId, securityDic, (key, value) => securityDic);
            }
        }

        public string GetSettleById(string groupId, string id)
        {
            string result = string.Empty;

            return result;

            if (!string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId))
            {
                ConcurrentDictionary<string, string> securityDic = _settleDic[groupId];
                if (!string.IsNullOrEmpty(id) && securityDic.ContainsKey(id))
                    result = securityDic[id];
            }

            return result;
        }

        public ConcurrentDictionary<string, string> GetSettleByGroupId(string groupId)
        {
            ConcurrentDictionary<string, string> result = null;
            if (_settleDic.ContainsKey(groupId))
            {
                result = _settleDic[groupId];
            }

            return result;
        }

        public void RemoveByGroupId(string groupId)
        {
            if (_settleDic.ContainsKey(groupId))
            {
                if (_settleDic.ContainsKey(groupId))
                {
                    ConcurrentDictionary<string, string> securityDic = null;
                    _settleDic.TryRemove(groupId, out securityDic);
                }
            }
        }

        public void RemoveById(string groupId, string id)
        {
            if (_settleDic.ContainsKey(groupId))
            {
                ConcurrentDictionary<string, string> securityDic = _settleDic[groupId];
                if (securityDic.ContainsKey(id))
                {
                    string tem = string.Empty;
                    securityDic.TryRemove(id, out tem);
                }
            }
        }

        public bool IsContain(string groupId)
        {
            return !string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId);
        }
    }
}
