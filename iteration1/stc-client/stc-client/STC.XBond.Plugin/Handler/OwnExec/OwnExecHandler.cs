using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Handler.OwnExec
{
    /// <summary>
    /// 我的成交
    /// </summary>
    public class OwnExecHandler : IOwnExecHandler
    {
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();

        private OwnExecHandler() { }
        private static IOwnExecHandler server = new OwnExecHandler();


        public static IOwnExecHandler Instance { get { return server; } }


        public event Action<ExecutionReport> OnOrder;
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            ClientApi.Instance.OnOwnExecuteOrder += Instance_OnExecuteOrder;
        }

        private void Instance_OnExecuteOrder(ExecutionReport execOrder)
        {
            dic.AddOrUpdate(execOrder.ExecID, execOrder, (key, value) => execOrder);

            OnOrder?.Invoke(execOrder);
        }
    }
}
