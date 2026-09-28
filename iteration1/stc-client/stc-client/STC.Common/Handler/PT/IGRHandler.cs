using netapi.Api;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.PT
{
    public interface IGRHandler
    {
        GrExecOrdersReplyPo ExecOrderQuery(GrExecOrderQueryReq req);
        GrExecOrdersReplyPo ExecOrderHistQuery(GrExecOrderQueryReq req);

        GrExecOrdersReplyPo ExecOrderHistRecordQuery(GrExecOrderQueryReq req);
        PResult ExecOrderUpdate(ExecutionReport req);
        PResult ExecOrderSave(ExecutionReport req);

        PResult ExecPushOrderUpdate(ExecutionReport req);

        PResult ExecBatchOrderUpdate(List<ExecutionReport> req);

        void SubscribeGrExec(Action<List<ExecutionReport>> onMsgCallBack);

        void SubscribeOtherGrExec(Action<List<ExecutionReport>> onMsgCallBack);

        void ProcessGrExec(NetMessage netMsg);
        UserBridgeChangeReplyPo UserBridgeExec(UserBridgeChangeRequestPo req);
        /// <summary>
        /// 删除定单查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        GrExecOrdersReplyPo ExecOrderDelQuery(GrExecOrderQueryReq req);

        BaseReplyPo UpdateIssueGroupSetting(IssueGroupSettingRequest request);

        QueryIssueGroupReply QueryIssueGroupSetting(QueryIssueGroupRequest request);

        OperateIssueInstReply OperateIssueInstInfos(OperateIssueInstRequest request);

        ///// <summary>
        ///// 代付机构列表 增删改查
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        PaymentInstReplyPo OperateInstProxySetting(PaymentInstRequestPo request);

        FeeSettingReply OperateInstProxyFeeSetting(FeeSettingRequest request);

        InstAssociatedReply OperateInstAssociated(InstAssociatedRequest request);

        UserRoleSettingReply OperateUserRoleSetting(UserRoleSettingRequest request);

        /// <summary>
        /// 克隆订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult ExecOrderClone(CloneRequestPo req);

        /// <summary>
        /// 发送QQ消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult sendQQMsg(SendQQMsgReq req);

        PayFeeReply ExecPayFeeInfoQuery(PayFeeRequest req);

        SettlResult ExecSettInfoQuery(SettlRequest req);

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        SystemPowerReplay ExecSystemPowerQuery(SystemPowerRequest req);
        /// <summary>
        /// 其他人过桥订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        GrExecOrdersReplyPo ExecOtherOrderQuery(GrExecOrderQueryReq req);

        BridgeHistReplyPo BridgeMvtQuery(BridgeHistRequestPo req);

        BridgeReviewsReplyPo BridgeReviewsQuery();

        BaseReplyPo BridgeReviewState(CheckBridgeRequestPo req);

        /// <summary>
        /// 桥查看更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        void SubscribeBridgeReview(Action<BridgeReserveReviews> onMsgCallBack);
        void ProcessGrBridgeReview(NetMessage netMsg);

        /// <summary>
        /// 桥状态更新
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        void SubscribeBridgeStateChange(Action<BridgeReserveState> onMsgCallBack);
        void ProcessGrBridgeStateChange(NetMessage netMsg);
    }
}