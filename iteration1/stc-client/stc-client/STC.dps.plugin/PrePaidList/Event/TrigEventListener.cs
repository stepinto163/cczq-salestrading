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
	public class TrigEventListener : BaseEventListener
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
		public TrigEventListener(ExecutionReport so) : base(so)
		{
			so.setTrigEventListener(this);
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
			if (strategyOrder != null && !EventUtils.isDelete(strategyOrder) && "1".Equals(strategyOrder.InValidFlag))
			{// 预埋单未激活

				EventUtils.instance.removeEvent(strategyOrder.UserName, strategyOrder.TrigEvent, this);
				strategyOrder.InValidFlag = "0";
				// 替换交易参数			
				//EventUtils.replaceVars(  event , strategyOrder);
				onEventAfter(ev);
			}
		}
		public void onEventAfter(STC.Common.Handler.PrePaidList.Event ev)
		{
			// 动作
			LogHelper.Info("TrigEvent onEventAfter matchOrder:{0}", strategyOrder.OrderID);
			// StrategyFacade.matchOrder(strategyOrder);//重新匹配
			strategyOrder.StrategyOrderId = strategyOrder.OrderID;

			ExecutionReport ex = new ExecutionReport();
			ex.StrategyOrderId = strategyOrder.StrategyOrderId;
			ex.InValidFlag = strategyOrder.InValidFlag;
			ex.AlgoName = strategyOrder.AlgoName;
			string content = JsonConvert.SerializeObject(ex);
			EventManager.instance.GateWayApi.requestAsync(Constants.TBookSvr, Constants.Topic_stc_tbook_replaceOrder, content, Consts.WaitTime, new RspCallBack(EventManager.instance.msg));
		}
	}

}

    

