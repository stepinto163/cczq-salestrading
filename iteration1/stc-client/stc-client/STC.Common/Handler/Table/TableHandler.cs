using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Table
{
    public class TableHandler : ITableHandler
    {
        private static TableHandler tableHandler = new TableHandler();
        private bool isSubscribeForIO = false;
        private event Action<IntentionQuoteInfo> OnOriginInfo;

        private bool isSubscribeForTBookIQ = false;
        private event Action<IntentionQuoteInfo> OnIntentionQuoteInfo;

        private bool isSubscribeForTBookIR = false;
        private event Action<IntentionQuoteInfo> OnRecognition;

        private bool isSubscribeForIntentionOrder = false;
        private event Action<IntentionOrderInfo> OnIntentionOrder;

        private bool isSubscribeForIntentionExec = false;
        private event Action<IntentionExecInfo> OnIntentionExec;


        public static ITableHandler Instance { get { return tableHandler; } }

        private TableHandler()
        {

        }



        public STC.Common.Model.Table.RecognizeIntentionQuoteReplyPo PublishIntention(NewOrderSingle request)
        {
            request.BookID = Utils.LocalEnv.ManualOrderDefaultBookId ?? null;
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.RecognizeIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_publishIntentionQuote, request);
        }

        public RecognizeIntentionQuoteReplyPo RecognizeIntentionQuote(RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RecognizeIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_RecognizeIntentionQuote, request);
        }

        public IntentionQuoteReplyPo QueryOriginRequest(IntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<IntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_QueryOriginRequest, request);
        }

        public IntentionQuoteReplyPo QueryIntentionQuote(IntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<IntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_QueryIntentionQuote, request);
        }

        public ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(ConfirmIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<ConfirmIntentionQuoteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_ConfirmIntentionQuote, request);
        }

        public PResult UpdateIntentionQuote(UpdateIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_UpdateIntentionQuote, request);
        }

        public PResult InsertIntentionQuote(InsertIntentionQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_InsertIntentionQuote, request);
        }

        public PResult DeleteIntentionQuote(DeleteIntentionQuoteReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_DeleteIntentionQuote, request);
        }

        public PResult RelateIntention(RelateIntentionReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_relate, request);
        }

        public QueryEmsOrderReplyPo QueryEmsOrder(QueryEmsOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryEmsOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryEmsOrder, request);
        }

        public QueryRolesReplyPo QueryRoles()
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryRolesReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryRoles, "{}");
        }

        /// <summary>
        /// 原始请求订阅
        /// </summary>
        /// <param name="onOriginCallback"></param>
        public void SubscribeOrigin(string[] topics, Action<IntentionQuoteInfo> onOriginCallback)
        {
            if (onOriginCallback != null && (OnOriginInfo == null || !OnOriginInfo.GetInvocationList().Contains(onOriginCallback)))
            {
                OnOriginInfo += onOriginCallback;
            }

            lock (this)
            {
                if (!isSubscribeForIO)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         topics, ApiConsts.TableSvr, "");
                    isSubscribeForIO = true;
                }
            }
        }

        /// <summary>
        /// Tbook意向报价回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnOrigin(object info)
        {
            DataBus.Instance.add(info);
        }

        public void ProcessOrigin(NetMessage netMsg)
        {
            IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_stc_table_io}**" }, netMsg);
            if (data != null)
                OnOriginInfo?.Invoke(data);
        }

        /// <summary>
        /// 意向报价状态订阅
        /// </summary>
        /// <param name="onIntentionallback"></param>
        public void SubscribeIntention(string[] topics, Action<IntentionQuoteInfo> onIntentionallback)
        {
            if (onIntentionallback != null && (OnIntentionQuoteInfo == null || !OnIntentionQuoteInfo.GetInvocationList().Contains(onIntentionallback)))
            {
                OnIntentionQuoteInfo += onIntentionallback;
            }
               
            lock(this)
            {
                if (!isSubscribeForTBookIQ)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         topics, ApiConsts.TableSvr, "");
                    isSubscribeForTBookIQ = true;
                }
            }
        }

        /// <summary>
        /// Tbook意向报价回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnIntention(object info)
        {
            DataBus.Instance.add(info);
        }

        public void ProcessIntention(NetMessage netMsg)
        {
            IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_stc_table_iq}{LocalEnv.UserInfo.UserName}" }, netMsg);
            if (data != null)
                OnIntentionQuoteInfo?.Invoke(data);
        }

        /// <summary>
        /// 意向报价识别订阅
        /// </summary>
        /// <param name="onIntentionallback"></param>
        public void SubscribeRecognization(string[] topics, Action<IntentionQuoteInfo> onRecognitionCallback)
        {
            if (onRecognitionCallback != null && (OnRecognition == null || !OnRecognition.GetInvocationList().Contains(onRecognitionCallback)))
                OnRecognition += onRecognitionCallback;

            lock(this)
            {
                if (!isSubscribeForTBookIR)
                {
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         topics, ApiConsts.TableSvr, "");
                    isSubscribeForTBookIR = true;
                }
            }
        }

        /// <summary>
        /// Tbook意向报价识别回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnRecognition(object info)
        {
            DataBus.Instance.add(info);
        }

        public void ProcessRecognition(NetMessage netMsg)
        {
            IntentionQuoteInfo data = BaseHelper.ConvertStcMessage<IntentionQuoteInfo>(new string[] { $"{ApiConsts.Topic_stc_table_ir}{LocalEnv.UserInfo.UserName}" }, netMsg);
            if (data != null)
                OnRecognition?.Invoke(data);
        }

        /// <summary>
        /// 查询意向订单/需求工作台作废区查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IntentionOrderReplyPo QueryIntentionOrder(IntentionOrderRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<IntentionOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_QueryIntentionOrder, request);
        }

        /// <summary>
        /// 意向订单订阅
        /// </summary>
        /// <param name="onIntentionOrderCallback"></param>
        public void SubscribeIntentionOrder(Action<IntentionOrderInfo> onIntentionOrderCallback)
        {
            if (onIntentionOrderCallback != null && (OnIntentionOrder == null || !OnIntentionOrder.GetInvocationList().Contains(onIntentionOrderCallback)))
                OnIntentionOrder += onIntentionOrderCallback;

            lock(this)
            {
                if (!isSubscribeForIntentionOrder)
                {
                    isSubscribeForIntentionOrder = true;
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         new string[] { $"{ApiConsts.Topic_IntentionOrder}{LocalEnv.UserInfo.UserName}" }, ApiConsts.TableSvr, "");
                }
            }
        }

        /// <summary>
        /// Tbook意向订单回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnIntentionOrder(object info)
        {
            DataBus.Instance.add(info);
        }

        public void ProcessIntentionOrder(NetMessage netMsg)
        {
            IntentionOrderInfo data = BaseHelper.ConvertStcMessage<IntentionOrderInfo>(new string[] { $"{ApiConsts.Topic_IntentionOrder}{LocalEnv.UserInfo.UserName}" }, netMsg);
            if (data != null)
                OnIntentionOrder?.Invoke(data);
        }

        /// <summary>
        /// 查询意向成交
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IntentionExecReplyPo QueryIntentionExec(IntentionExecRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<IntentionExecReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_QueryIntentionExec, request);
        }

        /// <summary>
        /// 意向成交订阅
        /// </summary>
        /// <param name="onIntentionExecCallback"></param>
        public void SubscribeIntentionExec(Action<IntentionExecInfo> onIntentionExecCallback)
        {
            if (onIntentionExecCallback != null && (OnIntentionExec == null || !OnIntentionExec.GetInvocationList().Contains(onIntentionExecCallback)))
                OnIntentionExec += onIntentionExecCallback;

            lock(this)
            {
                if (!isSubscribeForIntentionExec)
                {
                    isSubscribeForIntentionExec = true;
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage,
                         new string[] { $"{ApiConsts.Topic_IntentionExec}{LocalEnv.UserInfo.UserName}" }, ApiConsts.TableSvr, "");
                }
            }
        }

        /// <summary>
        /// Tbook意向成交回调函数
        /// </summary>
        /// <param name="us"></param>
        private void Instance_OnIntentionExec(object info)
        {
            DataBus.Instance.add(info);
        }

        public void ProcessIntentionExec(NetMessage netMsg)
        {
            IntentionExecInfo data = BaseHelper.ConvertStcMessage<IntentionExecInfo>(new string[] { $"{ApiConsts.Topic_IntentionExec}{LocalEnv.UserInfo.UserName}" }, netMsg);
            if (data != null)
                OnIntentionExec?.Invoke(data);
        }


        ///// <summary>
        ///// 需求工作台作废区查询
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public QueryHistoryIntentionOrderReplyPo QueryHistoryIntentionOrder(QueryHistoryIntentionOrderReq request)
        //{
        //    return ApiManager.Instance.GetDefaultApi().Request<QueryHistoryIntentionOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_Old_QueryIntentionQuote, request);
        //}

        public QueryHistoryIntentionOrderReplyPo QueryEmsOrderHist(QueryHistoryIntentionOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryHistoryIntentionOrderReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_table_queryEmsOrderHist, request);
        }

        /// <summary>
        /// 历史查询作废区UnRef  信用债--”CREDIT” 利率债--”RATE”
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult UnRefHistoryIntentionOrder(UnRefHistoryIntentionOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_stc_TableUnrefOrder, request);
        }

        /// <summary>
        /// 日初订单恢复提醒个人系统设置查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public QueryUnrefParamReplyPo QueryUnrefParam(QueryUnrefParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryUnrefParamReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_QueryUnrefParam, request);
        }

        /// <summary>
        /// 日初订单恢复提醒个人系统设置更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult UpdateUnrefParam(UpdateUnrefParamReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.TableSvr, ApiConsts.Topic_stc_UpdateUnrefParam, request);
        }

        public BrokerQuoteRecordReply queryBrokerHis(BrokerReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BrokerQuoteRecordReply>(ApiConsts.TableSvr, ApiConsts.Topic_stc_brokerQuery, req); //ClientApi.Instance.queryRiskHistory(req);
        }

        public BrokerWhiteListReplyPo brokerWhiteList(BrokerWhiteList request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BrokerWhiteListReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_brokerWhiteList, request);
        }

        public BrokerWhiteReplyPo brokerWhite(BrokerWhiteList request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BrokerWhiteReplyPo>(ApiConsts.TableSvr, ApiConsts.Topic_stc_brokerWhiteList, request);
        }

    }
}
