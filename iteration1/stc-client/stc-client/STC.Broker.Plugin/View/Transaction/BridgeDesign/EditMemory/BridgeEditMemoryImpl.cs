using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign.EditMemory
{
    /// <summary>
    /// 缓存桥的编辑状态
    /// </summary>
    public class BridgeEditMemoryImpl : BridgeEditMemory
    {
        private static BridgeEditMemory bridgeEditMemory = new BridgeEditMemoryImpl();
        private BridgeEditMemoryImpl() { }
        public static BridgeEditMemory Instance { get { return bridgeEditMemory; } }

        private Dictionary<string, bool> editDic = new Dictionary<string, bool>();

        public void Add(string key, bool isEdit)
        {
            if (!editDic.ContainsKey(key))
                editDic.Add(key, isEdit);
            else
                editDic[key] = isEdit;
        }

        public void Clear()
        {
            editDic.Clear();
        }

        public void DeleteByKey(string key)
        {
            if (editDic.ContainsKey(key))
                editDic.Remove(key);
        }

        public bool CheckEditStatus(string key)
        {
            if (editDic.ContainsKey(key))
                return editDic[key];

            return false;
        }
    }
}
