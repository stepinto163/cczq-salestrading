using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Execute;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.Admin;
using STC.Common.Dispatcher;

namespace STC.Common.Handler.Ptr
{
    public class PtrHandler : IPtrHandler
    {
        private PtrHandler() { }
        private static IPtrHandler ptrHandler = new PtrHandler();

        private bool isSubscribeForInnerOrder = false;
        public static IPtrHandler Instance { get { return ptrHandler; } }

        private event Action<ExecuteOrder> OnOrder;
        private readonly string _ModelId = "5000";
        private readonly string _Code = "5001";

        public void Init(Action<ExecuteOrder> onOrderCallBack, string user)
        {
            if (onOrderCallBack != null && (OnOrder == null || !OnOrder.GetInvocationList().Contains(onOrderCallBack)))
                OnOrder += onOrderCallBack;

            if (!isSubscribeForInnerOrder)
            {
                bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
                if (isPermission)
                    ApiManager.Instance.GetDefaultApi().SubScribe(DataBus.Instance.Instanse_OnMessage, new string[] { ApiConsts.Topic_ptr_reltime_deal_all }, ApiConsts.PTRSvr, "");
                else
                    ApiManager.Instance.GetDefaultApi().SubScribe(DataBus.Instance.Instanse_OnMessage, new string[] { string.Format("{0}{1}", ApiConsts.Topic_ptr_reltime_deal_prefix, user) }, ApiConsts.PTRSvr, "");
                isSubscribeForInnerOrder = true;
            }
        }

        private void Instance_OnOrder(object order)
        {
            DataBus.Instance.add(order);
        }

        public void ProcessPtrOrder(NetMessage netMsg)
        {
            ExecuteOrder data = BaseHelper.ConvertStcMessage<ExecuteOrder>(new string[] { ApiConsts.Topic_ptr_reltime_deal_all }, netMsg);
            if (data != null)
                OnOrder?.Invoke(data);
        }

        public ExeOrderSearchResp SearchExeOrder(ExecuteOrderCondition executeOrderCondition)
        {
            return ApiManager.Instance.GetDefaultApi().Request<ExeOrderSearchResp>(ApiConsts.PTRSvr, ApiConsts.Topic_ptr_search, executeOrderCondition);
        }

        public void UnSubscribeOrder(Action<ExecuteOrder> onOrderCallBack)
        {
            if (onOrderCallBack != null && (OnOrder == null || !OnOrder.GetInvocationList().Contains(onOrderCallBack)))
                OnOrder -= onOrderCallBack;
        }

        public PResult updateTradeRemark(UpdateRemarkRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTRSvr, ApiConsts.Topic_ptr_updateTradeRemark, req);
        }
        //
        public ExeOrderSearchResp SearchHistoryExeOrder(HistoryExecuteOrderCondition executeOrderCondition)
        {
            return ApiManager.Instance.GetDefaultApi().Request<ExeOrderSearchResp>(ApiConsts.PTRSvr, ApiConsts.Topic_ptr_Historysearch, executeOrderCondition);
        }

        public PResult updateTradeConfirm(ExecuteConfirmRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTRSvr, ApiConsts.Topic_ptr_deal_ack, req);
        }

    }
}
