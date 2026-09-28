using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Fix.Message;
using System.Collections.Concurrent;
using STC.Common.Model.TBook;
using STC.Common.Model.Inner;
using STC.Common.Api;
using netapi.Api;
using STC.Common.Dispatcher;

namespace STC.Common.Handler.Inner
{
    public class InnerHandler : IInnerHandler
    {
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();
        private bool isSubscribeForInnerOrder = false;

        private InnerHandler() { }
        private static IInnerHandler server = new InnerHandler();


        public static IInnerHandler Instance { get { return server; } }

        private event Action<ExecutionReport> OnOrder;

        public void Init(Action<ExecutionReport> onOrderCallBack)
        {
            if (onOrderCallBack != null && (OnOrder == null || !OnOrder.GetInvocationList().Contains(onOrderCallBack)))
                OnOrder += onOrderCallBack;

            if (!isSubscribeForInnerOrder)
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { ApiConsts.Topic_stc_inner_order }, ApiConsts.InnerSvr, "");
                //ClientApi.Instance.subscribeForInner(Instance_OnOrder);
                isSubscribeForInnerOrder = true;
            }
        }

        private void Instance_OnOrder(object order)
        {
            DataBus.Instance.add(order);
        }

        public void ProcessInnerOrder(NetMessage netMsg)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { ApiConsts.Topic_stc_inner_order }, netMsg);
            if (data != null)
                Instance_OnOrder(data);
        }

        private void Instance_OnOrder(ExecutionReport order)
        {
            lock (string.Intern(order.OrderID))
            {
                if (order.OrdStatus.Equals("0"))
                {
                    dic.AddOrUpdate(order.OrderID, order, (key, value) => order);
                }
                else
                {
                    ExecutionReport tem = null;
                    dic.TryRemove(order.OrderID, out tem);
                }

                OnOrder?.Invoke(order);
            }
        }

        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnOrder != null)
                OnOrder = null;
            dic.Clear();
            isSubscribeForInnerOrder = isSubscribe;
        }

        public void UnSubscribeOrder(Action<ExecutionReport> onOrderCallBack)
        {
            if (onOrderCallBack != null && OnOrder != null)
                OnOrder -= onOrderCallBack;
        }

        public List<ExecutionReport> QueryInnerOrderForInner(QueryOrderReq request)
        {
            TBookOrder result = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_queryInnerOrder, request); //ClientApi.Instance.queryInnerOrderForInner(request);

            if (result != null && result.datas != null && result.datas.Count > 0)
            {
                foreach (ExecutionReport er in result.datas)
                    dic.AddOrUpdate(er.OrderID, er, (key, value) => er);
            }

            return result.datas;
        }

        public PResult TransactionOrder(InnerOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_transaction, request); //ClientApi.Instance.transactionForInner(request);
        }

        public PResult CancelOrder(InnerOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_cancelOrder, request); //ClientApi.Instance.cancelOrderForInner(request);
        }

        public PResult FakTransactionOrder(InnerOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_fakTransaction, request); //ClientApi.Instance.fakTransactionForInner(request);
        }

        public PResult FakCancelOrder(InnerOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_fakCancel, request); //ClientApi.Instance.fakCancelForInner(request);
        }

        /// <summary>
        /// 定向回价（确认）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult DRPOrder(NewOrderSingle request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_PlaceOrder, request);
        }

        /// <summary>
        /// 定向回价（议价、拒绝）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult DRPCancelOrder(Model.Fix.Message.OrderCancelRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.InnerSvr, ApiConsts.Topic_stc_inner_DRPOrder, request);
        }

    }
}
