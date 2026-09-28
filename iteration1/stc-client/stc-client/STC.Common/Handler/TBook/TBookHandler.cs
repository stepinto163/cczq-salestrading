using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Api;
using STC.Common.Model.TBook;
using STC.Common.Model.ODM;
using STC.Common.Model.Risk;
using netapi.Api;
using STC.Common.Dispatcher;
using System.Threading;
using STC.Common.Model.Quant;

namespace STC.Common.Handler.TBook
{
    /// <summary>
    /// 我的订单
    /// </summary>
    public class TBookHandler : ITBookHandler
    {
        private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();
        private ConcurrentDictionary<string, ExecutionReport> dicQuant = new ConcurrentDictionary<string, ExecutionReport>();
        private ConcurrentDictionary<string, Bond> bondDic = new ConcurrentDictionary<string, Bond>();
        private object _orderLock = new object();
        private object _quantorderLock = new object();
        private object _bondLock = new object();
        private bool isSubscribeForTBookOrder = false;
        private bool isSubscribeForTBookBond = false;
        private bool isSubscribeForTBookConStatus = false;
        private bool isSubscribeForTbookSysParam = false;


        private TBookHandler() { }
        private static ITBookHandler server = new TBookHandler();


        public static ITBookHandler Instance { get { return server; } }

        private event Action<ExecutionReport> OnOrder;
        private event Action<ExecutionReport> OnQuantOrder;
        private event Action<List<Bond>> OnBondChange;
        private event Action<List<SessionStatus>> OnConStatus;
        private event Action<UserSetting> OnSystemParam;

        public void OnClear()
        {
            dic.Clear();
            bondDic.Clear();
        }


        /// <summary>
        /// 取消订阅我的订单和债券列表
        /// </summary>
        /// <param name="isSubscribe"></param>
        public void UnSubscribeAll(bool isSubscribe)
        {
            if (OnOrder != null)
                OnOrder = null;
            if (OnQuantOrder != null)
                OnQuantOrder = null;
            if (OnBondChange != null)
                OnBondChange = null;

            dic.Clear();
            bondDic.Clear();

            isSubscribeForTBookBond = isSubscribe;
            isSubscribeForTBookOrder = isSubscribe;
        }

        /// <summary>
        /// 取消订阅我的订单
        /// </summary>
        /// <param name="onOrderCallBack"></param>
        public void UnSubscribeOrder(Action<ExecutionReport> onOrderCallBack)
        {
            if (onOrderCallBack != null && OnOrder != null && OnOrder.GetInvocationList().Contains(onOrderCallBack))
                OnOrder -= onOrderCallBack;
        }

        /// <summary>
        /// 取消订阅策略订单
        /// </summary>
        /// <param name="onOrderCallBack"></param>
        public void UnSubscribeQuantOrder(Action<ExecutionReport> onQuantOrderCallBack)
        {
            if (onQuantOrderCallBack != null && OnQuantOrder != null && OnQuantOrder.GetInvocationList().Contains(onQuantOrderCallBack))
                OnQuantOrder -= onQuantOrderCallBack;
        }

        public void UnSubscribeQuantIdOrder(TTbookQuantStrategys obj)
        {
            ApiManager.Instance.GetDefaultApi().UnSubScribe(ApiConsts.TBookSvr, new string[] { $"{ApiConsts.Topic_stc_tbook_prefix}{obj.Username}.{obj.Quantid}.**" });
        }

        /// <summary>
        /// 取消订阅债券列表
        /// </summary>
        /// <param name="onBondCallBack"></param>
        public void UnSubscribeBond(Action<List<Bond>> onBondCallBack)
        {
            if (OnBondChange != null && onBondCallBack != null)
                OnBondChange -= onBondCallBack;
        }

        /// <summary>
        /// 订阅我的订单和债券列表
        /// </summary>
        /// <param name="onOrderCallBack"></param>
        /// <param name="onBondCallBack"></param>
        public void Init(Action<ExecutionReport> onOrderCallBack, Action<List<Bond>> onBondCallBack)
        {
            if (onOrderCallBack != null)
            {
                lock (_orderLock)
                {
                    if (OnOrder == null || !OnOrder.GetInvocationList().Contains(onOrderCallBack))
                    OnOrder += onOrderCallBack;
                
                    if (!isSubscribeForTBookOrder)
                    {
                        isSubscribeForTBookOrder = true;
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         new string[] { $"{ApiConsts.Topic_stc_tbook_prefix}{ApiManager.Instance.GetDefaultApi().UserName}.**" },
                            ApiConsts.TBookSvr, "");
                        //ClientApi.Instance.subscribeForTBook(Instance_OnOrder);
                    }
                }
                
                if (isSubscribeForTBookOrder)
                {
                    List<ExecutionReport> orders = dic.Values.ToList();
                    foreach (ExecutionReport er in orders)
                        Instance_OnOrder(er);
                }
            }

            if (onBondCallBack != null)
            {
                lock(_bondLock)
                {
                    if (OnBondChange == null || !OnBondChange.GetInvocationList().Contains(onBondCallBack))
                        OnBondChange += onBondCallBack;

                    if (!isSubscribeForTBookBond)
                    {
                        isSubscribeForTBookBond = true;
                        ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnBond,
                         new string[] { ApiConsts.Topic_stc_tbook_xbond },
                            ApiConsts.TBookSvr, "");
                        //ClientApi.Instance.subscribeBondForTBook(Instance_OnBond);
                       
                    }
                }
                
                if (isSubscribeForTBookBond)
                {
                    Instance_OnBond(bondDic.Values.ToList());
                }
            }
        }

        private void Instance_OnBond(object bonds)
        {
            // 弃用
            Instance_OnBond(bonds as List<Bond>);
        }

        private void Instance_OnOrder(object order)
        {
            DataBus.Instance.add(order);
        }

        public void ProcessOrder(NetMessage netMsg)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { $"{ApiConsts.Topic_stc_tbook_prefix}{ApiManager.Instance.GetDefaultApi().UserName}.**", ApiConsts.Topic_stc_tbook_xbond }, netMsg);
            if (data != null)
                Instance_OnOrder(data);
        }

        public void ProcessQuantOrder(NetMessage netMsg)
        {
            ExecutionReport data = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { netMsg.Topic }, netMsg);
            if (data != null)
                Instance_OnOrderQuant(data);
        }

        /// <summary>
        /// 订阅我的策略订单或者所有策略订单
        /// </summary>
        /// <param name="onOrderQuantCallBack"></param>

        public void InitQuant(string UserName,string quantid, Action<ExecutionReport> onOrderQuantCallBack)
        {
            if (onOrderQuantCallBack != null)
            {
                lock (_quantorderLock)
                {
                    if (OnQuantOrder == null || !OnQuantOrder.GetInvocationList().Contains(onOrderQuantCallBack))
                        OnQuantOrder += onOrderQuantCallBack;


                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                     new string[] { $"{ApiConsts.Topic_stc_tbook_prefix}{UserName}.{quantid}.**" },
                        ApiConsts.TBookSvr, "");
                    //ClientApi.Instance.subscribeForTBook(Instance_OnOrder);

                }

                //List<ExecutionReport> orders = dicQuant.Values.ToList();
                //foreach (ExecutionReport er in orders)
                //    Instance_OnOrderQuant(er);
            }
        }


        /// <summary>
        /// 订阅服务器状态信息
        /// </summary>
        /// <param name="onConStatusCallback"></param>
        public void SubscribeConStatus(Action<List<SessionStatus>> onConStatusCallback)
        {
            if (onConStatusCallback != null)
                OnConStatus += onConStatusCallback;

            if (!isSubscribeForTBookConStatus)
            {

                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                     new string[] { ApiConsts.Topic_stc_tbook_conStatus },
                        ApiConsts.TBookSvr, "");

                //ClientApi.Instance.subscribeForTBookConStatus(Instance_OnConStatus);
                isSubscribeForTBookConStatus = true;
            }
        }

        private void Instance_OnConStatus(object sessionStatus)
        {
            DataBus.Instance.add(sessionStatus);
        }

        public void ProcessConStatus(NetMessage netMsg)
        {
            List<SessionStatus> data = BaseHelper.ConvertStcMessage<List<SessionStatus>>(new string[] { ApiConsts.Topic_stc_tbook_conStatus }, netMsg);
            if (data != null)
                Instance_OnConStatus(data);
        }

        /// <summary>
        /// 服务器状态回调方法
        /// </summary>
        /// <param name="sessionStatus"></param>
        private void Instance_OnConStatus(List<SessionStatus> sessionStatus)
        {
            OnConStatus?.Invoke(sessionStatus);
        }

        /// <summary>
        /// 债券列表数据回调方法
        /// </summary>
        /// <param name="bonds"></param>
        private void Instance_OnBond(List<Bond> bonds)
        {
            foreach (Bond bond in bonds)
            {
                string primaryKey = string.Format("{0}#{1}#{2}#{3}", bond.securityid, bond.settltype, bond.venues, bond.venuetypegw);
                bondDic.AddOrUpdate(primaryKey, bond, (key, value) => bond);
            }

            OnBondChange?.Invoke(bonds);
        }

        /// <summary>
        /// 我的订单数据回调方法
        /// </summary>
        /// <param name="order"></param>
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

        /// <summary>
        /// 策略订单数据回调方法
        /// </summary>
        /// <param name="order"></param>
        private void Instance_OnOrderQuant(ExecutionReport order)
        {
            lock (string.Intern(order.OrderID))
            {
                //if (order.OrdStatus.Equals("0"))
                //{
                //    dicQuant.AddOrUpdate(order.OrderID, order, (key, value) => order);
                //}
                //else
                //{
                //    ExecutionReport tem = null;
                //    dicQuant.TryRemove(order.OrderID, out tem);
                //}
                OnQuantOrder?.Invoke(order);
            }
        }

        /// <summary>
        /// 获取该债券代码订单
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public List<ExecutionReport> GetOrderBySymbol(string symbol)
        {
            List<ExecutionReport> result = dic.Values.Where(delegate (ExecutionReport order)
            {
                if (order.Symbol.Equals(symbol))
                    return true;
                else
                    return false;
            }).ToList();


            return result;
        }

        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="cancelPo"></param>
        /// <returns></returns>
        public TBookCancelReplyPo CancelOrder(Model.TBook.OrderCancelRequest cancelPo)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TBookCancelReplyPo>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_cancel, cancelPo); //ClientApi.Instance.cancelOrderForTBook(cancelPo);
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        public PResult PlaceOrder(NewOrderSingle newOrder)
        {
            newOrder.BookID = Utils.LocalEnv.ManualOrderDefaultBookId ?? null;
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_placeOrder, newOrder); //ClientApi.Instance.placeOrderForTBook(newOrder);
        }

        /// <summary>
        /// 开关债券
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult OpenCloseBond(List<BondOperationPo> request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_xbond, request); //ClientApi.Instance.OpenCloseBondForTBook(request);
        }

        /// <summary>
        /// 请求我的列表历史数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public TBookOrder QueryOrderForTBook(QueryOrderReq req)
        {
            TBookOrder bookOrder = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_orderQuery, req); //ClientApi.Instance.queryOrderForTBook(req);

            if (bookOrder != null && bookOrder.datas != null && bookOrder.datas.Count > 0)
            {
                foreach (ExecutionReport order in bookOrder.datas)
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
                    }
                }
            }

            return bookOrder;
        }
        public TBookOrder QueryQuantOrderForTBook(QueryOrderReq req)
        {
            TBookOrder bookOrder = ApiManager.Instance.GetDefaultApi().Request<TBookOrder>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_orderQuery, req); 
            return bookOrder;
        }
        /// <summary>
        /// 订阅Tbook系统参数
        /// </summary>
        /// <param name="onSystemParamCallback"></param>
        public void SubscribeSystemParam(Action<UserSetting> onSystemParamCallback)
        {
            if (onSystemParamCallback != null)
                OnSystemParam += onSystemParamCallback;

            if (!isSubscribeForTbookSysParam)
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                     new string[] { ApiConsts.Topic_stc_tbook_system_param },
                        ApiConsts.TBookSvr, "");
                //ClientApi.Instance.subscribeForTBookSystemParam(Instance_OnSysParam);
                isSubscribeForTbookSysParam = true;
            }
        }

        private void Instance_OnSysParam(object us)
        {
            DataBus.Instance.add(us);
        }

        public void ProcessSystemParam(NetMessage netMsg)
        {
            UserSetting data = BaseHelper.ConvertStcMessage<UserSetting>(new string[] { ApiConsts.Topic_stc_tbook_system_param }, netMsg);
            if (data != null)
                Instance_OnSysParam(data);
        }

        /// <summary>
        /// Tbook系统参数回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnSysParam(UserSetting us)
        {
            OnSystemParam?.Invoke(us);
        }

        /// <summary>
        /// 更新Tbook系统参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult UpdatesystemParas(UserSetting request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TBookSvr, ApiConsts.Topic_systemParasUpdate, request); //CClientApi.Instance.updatesystemParas(request);
        }

        /// <summary>
        /// 请求查询XBond列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult XBondMDSub(List<MdSecurityIdReq> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_md_prefix_n, req); //ClientApi.Instance.XBondMDSub(req);
        }

        /// <summary>
        /// 请求查询ODM债券订阅列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public MdSecurityidsReplyPo queryMDSecurityId(MdSecurityIdReq req)
        {
            MdSecurityidsReplyPo mdReplyPo = ApiManager.Instance.GetDefaultApi().Request<MdSecurityidsReplyPo>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_md_securityidQuery, req); //ClientApi.Instance.queryMDSecurityId(req);
            return mdReplyPo;
        }

        /// <summary>
        /// 请求查询风控指标
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public RiskParaReplyPo queryRiskPara(RiskParaReq req)
        {
            RiskParaReplyPo po = ApiManager.Instance.GetDefaultApi().Request<RiskParaReplyPo>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_riskParaQuery, req); //ClientApi.Instance.queryRiskPara(req);
            return po;
        }

        public RiskParaReplyPo queryRiskPara(QueryRiskParameter req)
        {
            RiskParaReplyPo po = ApiManager.Instance.GetDefaultApi().Request<RiskParaReplyPo>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_riskParaQuery, req); //ClientApi.Instance.queryRiskPara(req);
            return po;
        }
        /// <summary>
        /// 更新风控指标
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult updateRiskPara(RiskParaReq req)
        {
            PResult po = ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RiskSvr, ApiConsts.Topic_stc_riskParaUpdate, req); //ClientApi.Instance.updateRiskPara(req);
            return po;
        }

        /// <summary>
        /// TBook修改渠道管理/xbond净价
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult updateOrder(NewOrderSingle req)
        {
            PResult po = ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_update, req); //ClientApi.Instance.repalceOrderForTBook(req);
            return po;
        }

        /// <summary>
        /// 批量撤单 - 撤销全部单/撤销策略单/撤销手工单/撤销当前价位所有买(卖)单
        /// </summary>
        /// <param name="cancelRequest"></param>
        /// <returns></returns>
        public OrderMassCancelReplyPo CancelOrderBatch(OrderMassCancelRequest cancelRequest)
        {
            return ApiManager.Instance.GetDefaultApi().Request<OrderMassCancelReplyPo>(ApiConsts.TBookSvr, ApiConsts.Topic_stc_tbook_cancel_batch, cancelRequest); 
        }
    }
}
