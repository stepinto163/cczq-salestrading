using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Cache
{
    public class SortCache : ISortCache
    {
        private SortCache() { }
        private static ISortCache server = new SortCache();


        public static ISortCache Instance { get { return server; } }

        /// <summary>
        /// key: groupid, subkey:id (securityid的唯一标识) subvalue:sort
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _settleDic = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();


        public bool ContainGroup(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                if (_settleDic.ContainsKey(groupId))
                {
                    return true;
                }
            }

            return false;
        }


        public void AddOrUpdate(string groupId, string id, int sort)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                if (_settleDic.ContainsKey(groupId))
                {
                    ConcurrentDictionary<string, int> securityDic = _settleDic[groupId];
                    securityDic.AddOrUpdate(id, sort, (key, value) => sort);
                }
                else
                {
                    ConcurrentDictionary<string, int> securityDic = new ConcurrentDictionary<string, int>();
                    securityDic.AddOrUpdate(id, sort, (key, value) => sort);

                    _settleDic.AddOrUpdate(groupId, securityDic, (key, value) => securityDic);
                }
            }
        }

        public int GetSettleById(string groupId, string id)
        {
            int result = -1;

            if (!string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId))
            {
                ConcurrentDictionary<string, int> securityDic = _settleDic[groupId];
                if (securityDic.ContainsKey(id))
                    result = securityDic[id];
            }

            return result;
        }

        public ConcurrentDictionary<string, int> GetSettleByGroupId(string groupId)
        {
            ConcurrentDictionary<string, int> result = null;
            if (!string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId))
            {
                result = _settleDic[groupId];
            }

            return result;
        }

        public void RemoveById(string groupId, string id)
        {
            if (!string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId))
            {
                ConcurrentDictionary<string, int> securityDic = _settleDic[groupId];
                if (securityDic.ContainsKey(id))
                {
                    int tem = 0;
                    securityDic.TryRemove(id, out tem);
                }
            }
        }

        public void RemoveByGroupId(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId) && _settleDic.ContainsKey(groupId))
            {
                if (_settleDic.ContainsKey(groupId))
                {
                    ConcurrentDictionary<string, int> securityDic = null;
                    _settleDic.TryRemove(groupId, out securityDic);
                }
            }
        }

    }
}
