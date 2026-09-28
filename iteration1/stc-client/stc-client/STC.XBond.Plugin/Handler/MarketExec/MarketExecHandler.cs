using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;

namespace STC.XBond.Plugin.Handler.MarketExec
{
    /// <summary>
    /// 市场成交
    /// </summary>
    public class MarketExecHandler : IMarketExecHandler
    {
        
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();

        private MarketExecHandler() { }
        private static IMarketExecHandler server = new MarketExecHandler();


        public static IMarketExecHandler Instance { get { return server; } }

        
        public event Action<ExecutionReport> OnOrder;
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            ClientApi.Instance.OnExecuteOrder += Instance_OnExecuteOrder;
            ClientApi.Instance.SubscribeEMSOrder();
        }

        private void Instance_OnExecuteOrder(ExecutionReport execOrder)
        {
            dic.AddOrUpdate(execOrder.ExecID, execOrder, (key, value) => execOrder);

            OnOrder?.Invoke(execOrder);
        }
    }
}
