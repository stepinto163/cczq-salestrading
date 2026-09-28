using STC.Common.Api;
using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.OwnExec
{
    /// <summary>
    /// 我的成交
    /// </summary>
    public class OwnExecHandler : IOwnExecHandler
    {
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();
        private bool isSubscribeForEMS = false;

        private Dictionary<string, object> _topicDic = new Dictionary<string, object>();

        private OwnExecHandler() { }
        private static IOwnExecHandler server = new OwnExecHandler();


        public static IOwnExecHandler Instance { get { return server; } }


        private event Action<ExecutionReport> OnOrder;
        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnOrder != null)
                OnOrder = null;

            isSubscribeForEMS = isSubscribe;
        }


        public void UnSubscribe(Action<ExecutionReport> onOrderCallBack)
        {
            if (OnOrder != null && onOrderCallBack != null)
                OnOrder -= onOrderCallBack;
        }

        public void Init(Action<ExecutionReport> onOrderCallBack)
        {
            if (onOrderCallBack != null && (OnOrder == null || !OnOrder.GetInvocationList().Contains(onOrderCallBack)))
                OnOrder += onOrderCallBack;

            lock (this)
            {
                if (!isSubscribeForEMS)
                {
                    if (Utils.LocalEnv.IsHaveTradeBookPermission || Utils.LocalEnv.ConfInfo.IsPAYL)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnExecuteOrder, new string[] { ApiConsts.Topic_stc_ems_executionreport_cdh, ApiConsts.Topic_stc_ems_executionreport_cmds, ApiConsts.Topic_stc_ems_executionreport_cmds_qdmesp,
                            ApiConsts.Topic_stc_ems_executionreport_sse, 
                            ApiConsts.Topic_stc_ems_executionreport_ssf, 
                            ApiConsts.Topic_stc_ems_executionreport_sze,
                            ApiConsts.Topic_stc_ems_executionreport_szf,
                            ApiConsts.Topic_stc_ems_all },
                        ApiConsts.EMSSVR, "");
                    }

                    //ClientApi.Instance.subscribeForEMSExecutionReport(Instance_OnExecuteOrder);
                    isSubscribeForEMS = true;
                }
            }
        }

        private void Instance_OnExecuteOrder(object execOrder)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { ApiConsts.Topic_stc_ems_executionreport_cdh, ApiConsts.Topic_stc_ems_executionreport_cmds, ApiConsts.Topic_stc_ems_executionreport_cmds_qdmesp,
                ApiConsts.Topic_stc_ems_executionreport_sse,
                ApiConsts.Topic_stc_ems_executionreport_ssf,
                ApiConsts.Topic_stc_ems_executionreport_sze,
                ApiConsts.Topic_stc_ems_executionreport_szf,
                ApiConsts.Topic_stc_ems_all }, execOrder);
            if (data != null)
                Instance_OnExecuteOrder(data);
        }

        private void Instance_OnExecuteOrder(ExecutionReport execOrder)
        {
            if (execOrder != null && execOrder.VenueTypeGW != null && !execOrder.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()))
            //&& LoginHandler.Instance.IsMySelf(execOrder.user)
            {
                dic.AddOrUpdate(execOrder.OrderID, execOrder, (key, value) => execOrder);

                OnOrder?.Invoke(execOrder);
            }
        }

        public TBookOrder QueryOrderForTBook(QueryOrderReq req)
        {
            TBookOrder result = new TBookOrder();
            if (!(Utils.LocalEnv.IsHaveTradeBookPermission || Utils.LocalEnv.ConfInfo.IsPAYL))
            {
                return result;
            }

            TBookOrder bookOrders = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_execOrderQuery, req); //ClientApi.Instance.queryExecOrderForEMS(req);
            result.datas = bookOrders.datas.Where(delegate (ExecutionReport execOrder)
            {
                if (execOrder.VenueTypeGW != null && !execOrder.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()))
                    return true;
                else
                    return false;
            }).ToList();

            foreach (ExecutionReport er in result.datas)
                dic.AddOrUpdate(er.OrderID, er, (key, value) => er);

            result.totalSize = result.datas.Count;

            return result;
        }
    }
}
