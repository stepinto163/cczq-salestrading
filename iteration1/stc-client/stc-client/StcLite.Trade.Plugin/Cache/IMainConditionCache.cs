using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Cache
{
    public interface IMainConditionCache
    {
        void AddOrUpdate(string groupId, object condition);

        object GetConditionById(string groupId);

        void RemoveById(string groupId);

        bool IsContain(string groupId);
    }
}
