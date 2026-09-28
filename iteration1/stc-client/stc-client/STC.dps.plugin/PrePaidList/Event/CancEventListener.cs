using com.gateway.connector.connector;
using com.gateway.connector.proto;
using com.gateway.connector.utils;
using com.gateway.log;
using Newtonsoft.Json;
using STC.dps.plugin.Common;
using STC.dps.plugin.Model.Fix.Message;
using System.Text;

namespace STC.dps.plugin.PrePaidList.Event
{
    public class CancEventListener : BaseEventListener
    {
        public class RspCallBack : IRspCallBack
        {
            private Proto proto;
            public RspCallBack(Proto proto)
            {
                this.proto = proto;
            }
            public void onRspMessage(string rsp)
            {
                proto.Format = Format.REPLY;
                proto.Body = Encoding.UTF8.GetBytes(rsp);
            }
        }

        public CancEventListener(ExecutionReport so) : base(so)
        {
            so.setCancEventListener(this);
        }

        public override string getOrderId()
        {
            if (strategyOrder != null)
            {
                return strategyOrder.OrderID;
            }
            return "";
        }

        public override void onEvent(STC.Common.Handler.PrePaidList.Event ev)
        {
            if (strategyOrder != null)
            {// 预埋单未激活

                EventUtils.instance.removeEvent(strategyOrder.UserName, strategyOrder.CancEvent, this);
                onEventAfter(ev);
            }
        }

        public void onEventAfter(STC.Common.Handler.PrePaidList.Event events)
        {
            // 动作
            LogHelper.Info("CancEvent onEventAfter cancelStrategyOrder:{0}", strategyOrder);
            DataTimeSequence.Instance.Number = 1;
            ExecutionReport ex = new ExecutionReport();
            ex.ClOrdID = DataTimeSequence.Instance.GetId();
            ex.OrderID = strategyOrder.OrderID;
            ex.AlgoName = strategyOrder.AlgoName;
            string content = JsonConvert.SerializeObject(ex);
            EventManager.instance.GateWayApi.requestAsync(Constants.TBookSvr, Constants.Topic_stc_tbook_cancelOrder, content, Consts.WaitTime, new RspCallBack(EventManager.instance.msg));
        }
    }
}
