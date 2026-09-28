using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign.EditMemory
{
    public interface BridgeEditMemory
    {
        void Add(string key, bool isEdit);

        void Clear();

        void DeleteByKey(string key);

        bool CheckEditStatus(string key);
    }
}
