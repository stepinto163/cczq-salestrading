using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Cache
{
    public interface ISettleCache
    {
        void AddOrUpdate(string groupId, string id, string settle);

        string GetSettleById(string groupId, string id);

        ConcurrentDictionary<string, string> GetSettleByGroupId(string groupId);

        void RemoveById(string groupId, string id);

        void RemoveByGroupId(string groupId);

        bool IsContain(string groupId);
    }
}
