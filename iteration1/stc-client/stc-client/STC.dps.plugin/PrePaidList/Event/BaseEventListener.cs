using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.dps.plugin.Model.Fix.Message;
namespace STC.dps.plugin.PrePaidList.Event
{
    /*
      * 信号接口
      */
    public abstract class BaseEventListener : IEventListener
    {
        public BaseEventListener(ExecutionReport so)
        {
            this.strategyOrder = so;
        }
        protected ExecutionReport strategyOrder = null;

        public abstract string getOrderId();


        public abstract void onEvent(STC.Common.Handler.PrePaidList.Event ev);

    }
}