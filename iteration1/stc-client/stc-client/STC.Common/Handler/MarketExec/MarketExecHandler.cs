using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Handler.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MarketExec
{
    /// <summary>
    /// 包含市场和我的成交
    /// </summary>
    public class MarketExecHandler : IMarketExecHandler
    {
        
        //private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();
        //private ConcurrentDictionary<string, ExecutionReport> dicOrder = new ConcurrentDictionary<string, ExecutionReport>();
        private bool isSubscribeForEMS = false;
        private bool isSubscribeOrderForEMS = false;

        private MarketExecHandler() { }
        private static IMarketExecHandler server = new MarketExecHandler();


        public static IMarketExecHandler Instance { get { return server; } }

        
        private event Action<ExecutionReport> OnExecOrder;// 市场成交
        private event Action<ExecutionReport> OnOrder;// 分笔成交

        public void OnClear()
        {
            //dic.Clear();
            //dicOrder.Clear();
        }


        #region 市场数据加载完成信号（无用）
        private bool _loadData = false;
        public bool IsLoad()
        {
            return _loadData;
        }

        private event Action<bool> OnDataComplete;
        public void SetLoadDataStatus(bool loadData)
        {
            _loadData = loadData;

            OnDataComplete?.Invoke(loadData);
        }

        public bool LoadDataStatus() { return _loadData; }

        public void SubscribeLoadDataStatus(Action<bool> onLoadData)
        {
            if (onLoadData != null)
                OnDataComplete += onLoadData;
        }
        public void UnSubscribeLoadDataStatus(Action<bool> onLoadData)
        {
            if (onLoadData != null)
                OnDataComplete -= onLoadData;
        }
        #endregion

        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnExecOrder != null)
                OnExecOrder = null;

            if (OnOrder != null)
                OnOrder = null;

            //dic.Clear();
            //dicOrder.Clear();

            isSubscribeOrderForEMS = isSubscribeForEMS = isSubscribe;
        }

        public void UnSubscribe(Action<ExecutionReport> onOrderCallBack)
        {
            if (OnExecOrder != null)
            {
                OnExecOrder -= onOrderCallBack;
            }
        }

        /// <summary>
        /// 注册成交数据（包括本方所有ems成交数据和cdh数据）
        /// </summary>
        /// <param name="onOrderCallBack"></param>
        public void Init(Action<ExecutionReport> onOrderCallBack)
        {
            if (onOrderCallBack != null)
            {
                if (OnExecOrder == null)
                {
                    OnExecOrder += onOrderCallBack;
                }
                else
                {
                    int index = Array.IndexOf(OnExecOrder.GetInvocationList(), onOrderCallBack);
                    if (index < 0)
                    {
                        OnExecOrder += onOrderCallBack;
                    }
                }
            }

            lock (this)
            {
                if (!isSubscribeForEMS)
                {
                    if (Utils.LocalEnv.IsHaveTradeBookPermission || Utils.LocalEnv.ConfInfo.IsPAYL)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { ApiConsts.Topic_stc_ems_executionreport_cdh, ApiConsts.Topic_stc_ems_executionreport_cmds, ApiConsts.Topic_stc_ems_executionreport_cmds_qdmesp,
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
            DataBus.Instance.add(execOrder);
        }

        public void ProcessExecuteOrder(NetMessage netMsg)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { ApiConsts.Topic_stc_ems_executionreport_cdh, ApiConsts.Topic_stc_ems_executionreport_cmds, ApiConsts.Topic_stc_ems_executionreport_cmds_qdmesp,
                ApiConsts.Topic_stc_ems_executionreport_sse,
                ApiConsts.Topic_stc_ems_executionreport_ssf, 
                ApiConsts.Topic_stc_ems_executionreport_sze,
                ApiConsts.Topic_stc_ems_executionreport_szf,
                ApiConsts.Topic_stc_ems_executionreport_c_sdn,
                ApiConsts.Topic_stc_ems_all }, netMsg);
            if (data != null)
                Instance_OnExecuteOrder(data);
        }

        /// <summary>
        /// 历史市场成交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public TBookOrder QueryExecOrderForEMS(QueryOrderReq req)
        {
            if (!(Utils.LocalEnv.IsHaveTradeBookPermission || Utils.LocalEnv.ConfInfo.IsPAYL))
            {
                TBookOrder result = new TBookOrder();
                return result;
            }

            TBookOrder bookOrders = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_execOrderQuery, req); //ClientApi.Instance.queryExecOrderForEMS(req);

            if (bookOrders != null && bookOrders.datas != null && bookOrders.datas.Count > 0)
            {
                foreach (ExecutionReport er in bookOrders.datas)
                {
                    //dic.AddOrUpdate(er.OrderID, er, (key, value) => er);
                    //BDB
                    switch (Utils.LocalEnv.ServerInfo.AppName.ToLower())
                    {
                        case "bdb":
                        case "idcclient":
                            MarketExecCache.Instance.AddOrUpdate(er);
                            break;
                    }
                }
            }

            return bookOrders;
        }
        
      
        public void InitByBroker(Action<ExecutionReport> onOrderCallBack, string user, List<string> brokerList)
        {
            if (brokerList != null && brokerList.Count > 0)
            {
                if (onOrderCallBack != null)
                {
                    if (OnExecOrder == null)
                    {
                        OnExecOrder += onOrderCallBack;
                    }
                    else
                    {
                        int index = Array.IndexOf(OnExecOrder.GetInvocationList(), onOrderCallBack);
                        if (index < 0)
                        {
                            OnExecOrder += onOrderCallBack;
                        }
                    }
                }

                lock (this)
                {
                    if (!isSubscribeForEMS)
                    {
                        List<string> topicList = new List<string>();
                        //foreach (string broker in brokerList)
                        //{
                        //    topicList.Add(string.Format("{0}{1}.**", ApiConsts.Topic_stc_ems_executionreport_cdh_prefix, broker.ToLower()));
                        //}

                        //topicList.Add(string.Format("{0}*.{1}.**", ApiConsts.Topic_stc_ems_executionreport_prefix, user));

                        topicList.Add(ApiConsts.Topic_stc_ems_executionreport_sdn);//SDN国债期货(old)
                        topicList.Add(ApiConsts.Topic_stc_ems_executionreport_c_sdn);//SDN国债期货(new)
                        topicList.Add(ApiConsts.Topic_stc_ems_executionreport_cdh);//CDH债券

                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnExecuteOrder, topicList.ToArray(),
                            ApiConsts.EMSSVR, "");
                        //ClientApi.Instance.subscribeForEMSExecutionReport(Instance_OnExecuteOrder);
                        isSubscribeForEMS = true;
                    }
                }
            }
        }

        private void Instance_OnExecuteOrder(ExecutionReport execOrder)
        {
            if (execOrder != null)
            {
                 execOrder.Vs = execOrder.Vs == null || execOrder.Vs == "Y" || execOrder.Vs == "" ? "Y" : "N";

                //dic.AddOrUpdate(execOrder.OrderID, execOrder, (key, value) => execOrder);
                //BDB
                switch (Utils.LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        MarketExecCache.Instance.AddOrUpdate(execOrder);
                        break;
                }

                OnExecOrder?.Invoke(execOrder);
            }
        }


        #region EMS order

        public TBookOrder QueryOrderForEMS(QueryOrderReq req)
        {
            bool isAllow = Common.Handler.Admin.AdminHandler.Instance.CheckPermission("1003", null);
            if (isAllow)
            {
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                if (isNotAllow)
                {
                    return null;
                }
                TBookOrder bookOrders = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_orderQuery, req); //CClientApi.Instance.queryOrderForEMS(req);

                return bookOrders;
            }

            return null;
            //if (bookOrders != null && bookOrders.datas.Count > 0)
            //{
            //    foreach (ExecutionReport er in bookOrders.datas)
            //        dicOrder.AddOrUpdate(er.OrderID, er, (key, value) => er);
            //}

        }


        /// <summary>
        /// 只是为了存储注册过的SecurityID
        /// key: securityid 
        /// </summary>
        private ConcurrentDictionary<string, Action<ExecutionReport>> securitysdic = new ConcurrentDictionary<string, Action<ExecutionReport>>();
        //private event Action<ExecutionReport> OnEMSOrder;


        /// <summary>
        /// 根据securtityID订阅EMS Order 信息
        /// </summary>
        /// <param name="securitys"></param>
        /// <param name="onOrderCallBack"></param>
        public void SubscribeOrderBySecurtityIDs(List<string> securitys, Action<ExecutionReport> onOrderCallBack)
        {
            /*if (onOrderCallBack != null)
            {
                //if (OnOrder == null)
                //{
                //    OnOrder += onOrderCallBack;
                //}
                //else
                //{
                //    int index = Array.IndexOf(OnOrder.GetInvocationList(), onOrderCallBack);
                //    if (index < 0)
                //    {
                //        OnOrder += onOrderCallBack;
                //    }
                //}
                foreach(string securityID in securitys)
                {
                    if (securitysdic.ContainsKey(securityID) == false)
                    {
                        securitysdic[securityID] = onOrderCallBack;
                    }
                    else
                    {
                        int index = Array.IndexOf(securitysdic[securityID].GetInvocationList(), (Action<ExecutionReport>)onOrderCallBack);
                        if (index < 0)
                        {
                            securitysdic[securityID] += onOrderCallBack;
                        }
                    }
                }
            }

            lock (this)
            {
                if (Utils.LocalEnv.IsHaveTradeBookPermission)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { ApiConsts.Topic_stc_ems_order },
                            ApiConsts.EMSSVR, "");
                }
                //ClientApi.Instance.subscribeForEMSOrderReport(securitys, Instance_OnOrder);
            }*/
        }

        private void Instance_OnOrder(object order)
        {
            DataBus.Instance.add(order);
        }

        public void ProcessEmsOrder(NetMessage netMsg)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { $"{ApiConsts.Topic_stc_ems_order_prefix}**" }, netMsg);
            if (data != null)
            {
                data.Vs = data.Vs == null || data.Vs == "Y" || data.Vs == "" ? "Y" : "N";

                Instance_OnOrder(data);
            }
        }

        /// <summary>
        /// 根据security注销EMS Order消息
        /// </summary>
        /// <param name="securitys"></param>
        public void UnsubscribeOrderBySecurtityIDs(List<string> securitys)
        {
            /*foreach(string securtityID in securitys)
            {
                if( securitysdic.ContainsKey(securtityID))
                {
                    Action<ExecutionReport> action;
                    securitysdic.TryRemove(securtityID, out action);
                }
            }

            ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.EMSSVR, securitys.ToArray());// ClientApi.Instance.unsubscribeForEMSOrderReport(securitys);
*/        }

        public void SubscribeOrder(Action<ExecutionReport> onOrderCallBack)
        {
            /*if (onOrderCallBack != null)
            {
                if (OnOrder == null)
                {
                    OnOrder += onOrderCallBack;
                }
                else
                {
                    int index = Array.IndexOf(OnOrder.GetInvocationList(), onOrderCallBack);
                    if (index < 0)
                    {
                        OnOrder += onOrderCallBack;
                    }
                }
            }

            lock (this)
            {
                if (!isSubscribeOrderForEMS)
                {
                    if (Utils.LocalEnv.IsHaveTradeBookPermission)
                    {
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, new string[] { ApiConsts.Topic_stc_ems_order },
                        ApiConsts.EMSSVR, "");
                    }
                    //ClientApi.Instance.subscribeForEMSOrderReport(Instance_OnOrder);
                    isSubscribeOrderForEMS = true;
                }
            }*/
        }


        private void Instance_OnOrder(ExecutionReport order)
        {
            if (order != null)
            {
                //dicOrder.AddOrUpdate(order.OrderID, order, (key, value) => order);

                if (securitysdic.ContainsKey(order.SecurityID))
                {
                    securitysdic[order.SecurityID]?.Invoke(order);
                }

                OnOrder?.Invoke(order);
            }
        }

        public void UnsubscribeOrder(Action<ExecutionReport> onOrderCallBack)
        {
            if (OnOrder != null)
            {
                OnOrder -= onOrderCallBack;
            }
        }

        #endregion
    }
}
