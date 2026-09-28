using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.PT
{
    public class GRHandler : IGRHandler
    {
        private static GRHandler grHandler = new GRHandler();
        public static IGRHandler Instance { get { return grHandler; } }

        private string UserName = ApiManager.Instance.GetDefaultApi().UserName;
        private string UserID = LocalEnv.UserInfo.UserId;

        private event Action<List<ExecutionReport>> On_Exec;
        /// <summary>
        /// 桥查看更新推送
        /// </summary>
        private event Action<BridgeReserveReviews> OnBridgeReviewChange;
        /// <summary>
        /// 桥状态更新推送
        /// </summary>
        private event Action<BridgeReserveState> OnBridgeStateChange;

        private bool isSubscribeForExec = false;

        private bool isSubscribeBridgeReview = false;
        private bool isSubscribeBridgeStateChange = false;
        /// <summary>
        /// 其他人桥权限标识
        /// </summary>
        private bool isSubscribeOtherBridgerForExec = false;
        /// <summary>
        /// 订阅成交订单
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        public void SubscribeGrExec(Action<List<ExecutionReport>> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_Exec == null || !On_Exec.GetInvocationList().Contains(onMsgCallBack)))
                On_Exec += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeForExec)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_GrExecOrderSubscribe}{LocalEnv.UserInfo.UserId}" };

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.PTSvr, string.Empty);

                    isSubscribeForExec = true;
                }
            }
        }


        public void SubscribeOtherGrExec(Action<List<ExecutionReport>> onMsgCallBack)
        {
            if (onMsgCallBack != null && (On_Exec == null || !On_Exec.GetInvocationList().Contains(onMsgCallBack)))
                On_Exec += onMsgCallBack;

            lock (this)
            {
                if (!isSubscribeOtherBridgerForExec)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_GrExecOtherOrderSubscribe}{LocalEnv.UserInfo.UserId}" };

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.PTSvr, string.Empty);

                    isSubscribeOtherBridgerForExec = true;
                }
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="netMsg"></param>
        public void ProcessGrExec(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_GrExecOrderSubscribe))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<ExecutionReport> data = BaseHelper.ConvertStcMessage<List<ExecutionReport>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            On_Exec?.Invoke(data);
                        }
                    }
                    else
                    {
                        //todo ...  这里暂时跳过，接收所有人的信息
                        if (!string.IsNullOrWhiteSpace(this.UserID) && !"*".Equals(this.UserID))
                        {
                            string[] str = netMsg.Topic.Replace(ApiConsts.Topic_GrExecOrderSubscribe, "").Split('.');
                            if (!str[0].Equals(this.UserID))
                                return;
                        }

                        List<ExecutionReport> data2 = new List<ExecutionReport>();
                        ExecutionReport mm2 = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { netMsg.Topic }, netMsg);
                        data2.Add(mm2);
                        On_Exec?.Invoke(data2);
                    }

                }
                else if (netMsg.Topic.StartsWith(ApiConsts.Topic_GrExecOtherOrderSubscribe))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<ExecutionReport> data = BaseHelper.ConvertStcMessage<List<ExecutionReport>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            On_Exec?.Invoke(data);
                        }
                    }
                    else
                    {
                        //todo ...  这里暂时跳过，接收所有人的信息
                        if (!string.IsNullOrWhiteSpace(this.UserID) && !"*".Equals(this.UserID))
                        {
                            string[] str = netMsg.Topic.Replace(ApiConsts.Topic_GrExecOtherOrderSubscribe, "").Split('.');
                            if (!str[0].Equals(this.UserID))
                                return;
                        }

                        List<ExecutionReport> data2 = new List<ExecutionReport>();
                        ExecutionReport mm2 = BaseHelper.ConvertStcMessage<ExecutionReport>(new string[] { netMsg.Topic }, netMsg);
                        data2.Add(mm2);
                        On_Exec?.Invoke(data2);
                    }
                }
            }
        }

        public void SubscribeBridgeReview(Action<BridgeReserveReviews> onMsgCallBack)
        {
            if (onMsgCallBack != null && (OnBridgeReviewChange == null || !OnBridgeReviewChange.GetInvocationList().Contains(onMsgCallBack)))
                OnBridgeReviewChange += onMsgCallBack;
            lock (this)
            {
                if (!isSubscribeBridgeReview)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_BridgeReview}{LocalEnv.UserInfo.UserId}" };

                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.PTSvr, string.Empty);

                    isSubscribeBridgeReview = true;
                }
            }
        }

        public void ProcessGrBridgeReview(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_BridgeReview))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<BridgeReserveReviews> data = BaseHelper.ConvertStcMessage<List<BridgeReserveReviews>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (BridgeReserveReviews po in data)
                            {
                                OnBridgeReviewChange?.Invoke(po);
                            }
                        }
                    }
                    else
                    {
                        //todo ...  这里暂时跳过，接收所有人的信息
                        if (!string.IsNullOrWhiteSpace(this.UserID) && !"*".Equals(this.UserID))
                        {
                            string[] str = netMsg.Topic.Replace(ApiConsts.Topic_BridgeReview, "").Split('.');
                            if (!str[0].Equals(this.UserID))
                                return;
                        }
                        BridgeReserveReviews mm2 = BaseHelper.ConvertStcMessage<BridgeReserveReviews>(new string[] { netMsg.Topic }, netMsg);
                        OnBridgeReviewChange?.Invoke(mm2);
                    }

                }
            }
        }

        public void SubscribeBridgeStateChange(Action<BridgeReserveState> onMsgCallBack)
        {
            if (onMsgCallBack != null && (OnBridgeStateChange == null || !OnBridgeStateChange.GetInvocationList().Contains(onMsgCallBack)))
                OnBridgeStateChange += onMsgCallBack;
            lock (this)
            {
                if (!isSubscribeBridgeStateChange)
                {
                    string[] topic = new string[] { $"{ApiConsts.Topic_BridgeState}{LocalEnv.UserInfo.UserId}" };
                    ApiManager.Instance.GetDefaultApi().SubScribeWithImage(DataBus.Instance.Instanse_OnMessage, topic, ApiConsts.PTSvr, string.Empty);
                    isSubscribeBridgeStateChange = true;
                }
            }
        }

        public void ProcessGrBridgeStateChange(NetMessage netMsg)
        {
            if (netMsg != null)
            {
                if (netMsg.Topic.StartsWith(ApiConsts.Topic_BridgeState))
                {
                    if (netMsg.Data.StartsWith(ApiConsts.BRACKET))
                    {
                        List<BridgeReserveState> data = BaseHelper.ConvertStcMessage<List<BridgeReserveState>>(new string[] { netMsg.Topic }, netMsg);
                        if (data != null && data.Count > 0)
                        {
                            foreach (BridgeReserveState po in data)
                            {
                                OnBridgeStateChange?.Invoke(po);
                            }
                        }
                    }
                    else
                    {
                        //todo ...  这里暂时跳过，接收所有人的信息
                        if (!string.IsNullOrWhiteSpace(this.UserID) && !"*".Equals(this.UserID))
                        {
                            string[] str = netMsg.Topic.Replace(ApiConsts.Topic_BridgeState, "").Split('.');
                            if (!str[0].Equals(this.UserID))
                                return;
                        }
                        BridgeReserveState mm2 = BaseHelper.ConvertStcMessage<BridgeReserveState>(new string[] { netMsg.Topic }, netMsg);
                        OnBridgeStateChange?.Invoke(mm2);
                    }

                }
            }
        }


        /// <summary>
        /// 成交订单查询接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GrExecOrdersReplyPo ExecOrderQuery(GrExecOrderQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GrExecOrdersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOrderQuery, req);
        }


        public GrExecOrdersReplyPo ExecOrderHistQuery(GrExecOrderQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GrExecOrdersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOrderHistQuery, req);
        }

        public GrExecOrdersReplyPo ExecOrderHistRecordQuery(GrExecOrderQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GrExecOrdersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOrderHistRecordQuery, req);
        }
        /// <summary>
        /// 查询删除订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public GrExecOrdersReplyPo ExecOrderDelQuery(GrExecOrderQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GrExecOrdersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOrderDelQuery, req);
        }

        /// <summary>
        /// 交易处理更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderUpdate(ExecutionReport req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOrderUpdate, req);
        }

        /// <summary>
        /// 交易处理保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderSave(ExecutionReport req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_GrexecOrderSave, req);
        }


        /// <summary>
        /// 桥操作接口 bridgeChangeType 0:添加桥 1:修改桥 2:删除桥 3:查询桥
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public UserBridgeChangeReplyPo UserBridgeExec(UserBridgeChangeRequestPo req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<UserBridgeChangeReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrUserBridge, req);
        }

        /// <summary>
        /// 发单机构管理信息查询接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseReplyPo UpdateIssueGroupSetting(IssueGroupSettingRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BaseReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrInstGroups, request);
        }

        public QueryIssueGroupReply QueryIssueGroupSetting(QueryIssueGroupRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryIssueGroupReply>(ApiConsts.PTSvr, ApiConsts.Topic_GrinstInfo, request);
        }

        public OperateIssueInstReply OperateIssueInstInfos(OperateIssueInstRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<OperateIssueInstReply>(ApiConsts.PTSvr, ApiConsts.Topic_GrinstInfo, request);
        }

        /// <summary>
        /// 代付机构列表 增删改查
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PaymentInstReplyPo OperateInstProxySetting(PaymentInstRequestPo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PaymentInstReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_InstProxySetting, request);
        }

        /// <summary>
        /// 代付机构费用设置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FeeSettingReply OperateInstProxyFeeSetting(FeeSettingRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<FeeSettingReply>(ApiConsts.PTSvr, ApiConsts.Topic_InstProxyFeeSetting, request);
        }

        /// <summary>
        /// 关联机构接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InstAssociatedReply OperateInstAssociated(InstAssociatedRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<InstAssociatedReply>(ApiConsts.PTSvr, ApiConsts.Topic_InstInstAssociated, request);
        }


        public UserRoleSettingReply OperateUserRoleSetting(UserRoleSettingRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<UserRoleSettingReply>(ApiConsts.PTSvr, ApiConsts.Topic_UserRoleSetting, request);
        }

        /// <summary>
        /// 克隆订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecOrderClone(CloneRequestPo req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_OnExecOrderClone, req);
        }

        /// <summary>
        /// 克隆订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult ExecBatchOrderUpdate(List<ExecutionReport> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_OnBatchExecOrderClone, req);
        }

        /// <summary>
        /// 推市场成交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult ExecPushOrderUpdate(ExecutionReport req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_GrexecPushOrderSave, req);
        }

        public PResult sendQQMsg(SendQQMsgReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PTSvr, ApiConsts.Topic_SendQQMsg, req);
        }

        /// <summary>
        /// 代付费查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PayFeeReply ExecPayFeeInfoQuery(PayFeeRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PayFeeReply>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecIssueProxyInfoQuery, req);
        }

        /// <summary>
        /// 清算是否修改查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SettlResult ExecSettInfoQuery(SettlRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<SettlResult>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecSettInfoQuery, req);
        }


        /// <summary>
        /// 查询用户权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SystemPowerReplay ExecSystemPowerQuery(SystemPowerRequest req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<SystemPowerReplay>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecSystemPowerQuery, req);
        }

        public GrExecOrdersReplyPo ExecOtherOrderQuery(GrExecOrderQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GrExecOrdersReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_GrExecOtherOrderQuery, req);
        }

        /// <summary>
        /// 查询桥历史单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public BridgeHistReplyPo BridgeMvtQuery(BridgeHistRequestPo req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BridgeHistReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_BridgeMvtQuery, req);
        }


        /// <summary>
        /// 查询桥查看——某个桥
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BridgeReviewsReplyPo BridgeReviewsQuery()
        {
            return ApiManager.Instance.GetDefaultApi().Request<BridgeReviewsReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_BridgeReviewsQuery, null);
        }
        /// <summary>
        /// 点击桥查看
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseReplyPo BridgeReviewState(CheckBridgeRequestPo req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BaseReplyPo>(ApiConsts.PTSvr, ApiConsts.Topic_BridgeReviewState, req);
        }
    }
}

