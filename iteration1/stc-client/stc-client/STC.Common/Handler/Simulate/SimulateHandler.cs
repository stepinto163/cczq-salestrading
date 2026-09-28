using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Fix.Message;

namespace STC.Common.Handler.Simulate
{
    public class SimulateHandler : ISimulateHandler
    {

        private SimulateHandler() { }
        private static ISimulateHandler _simulateHandler = new SimulateHandler();


        public static ISimulateHandler Instance { get { return _simulateHandler; } }


        private event Action<NewOrderSingle> OnNewOrder;


        public void Subscribe(Action<NewOrderSingle> callBack)
        {
            lock (this)
            {
                if (callBack != null && (OnNewOrder == null || !OnNewOrder.GetInvocationList().Contains(callBack)))
                    OnNewOrder += callBack;

                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnNewOrderSingle, new string[] { ApiConsts.DPS_TOPIC_SIMULATE_ORDER },
                        ApiConsts.STCLITESVR, "");
            }
        }

        public void UnSubscribe(Action<NewOrderSingle> callBack)
        {
            lock (this)
            {
                if (callBack != null && OnNewOrder.GetInvocationList().Contains(callBack))
                    OnNewOrder -= callBack;
            }
        }

        private void Instance_OnNewOrderSingle(object clearPo)
        {
            NewOrderSingle data = BaseHelper.ConvertStcMessage<NewOrderSingle>(new string[] { ApiConsts.DPS_TOPIC_SIMULATE_ORDER }, clearPo);
            if (data != null)
                OnNewOrder(data);
        }
    }
}
