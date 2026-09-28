using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Cache
{
    public interface ISortCache
    {
        void AddOrUpdate(string groupId, string id, int sort);

        int GetSettleById(string groupId, string id);

        ConcurrentDictionary<string, int> GetSettleByGroupId(string groupId);

        void RemoveById(string groupId, string id);

        void RemoveByGroupId(string groupId);

        bool ContainGroup(string groupId);
    }
}
