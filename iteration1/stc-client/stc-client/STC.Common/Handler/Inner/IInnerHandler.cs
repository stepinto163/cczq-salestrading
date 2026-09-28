using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Inner;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Inner
{
    public interface IInnerHandler
    {
        void Init(Action<ExecutionReport> onOrderCallBack);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribeOrder(Action<ExecutionReport> onOrderCallBack);

        List<ExecutionReport> QueryInnerOrderForInner(QueryOrderReq request);

        PResult TransactionOrder(InnerOrderReq request);

        PResult CancelOrder(InnerOrderReq request);

        PResult FakTransactionOrder(InnerOrderReq request);

        PResult FakCancelOrder(InnerOrderReq request);

        PResult DRPOrder(NewOrderSingle request);

        PResult DRPCancelOrder(Model.Fix.Message.OrderCancelRequest request);

        void ProcessInnerOrder(NetMessage netMsg);
    }
}
