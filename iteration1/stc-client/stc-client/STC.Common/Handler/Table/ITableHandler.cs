using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Table
{
    public interface ITableHandler
    {
        RecognizeIntentionQuoteReplyPo PublishIntention(NewOrderSingle request);
        RecognizeIntentionQuoteReplyPo RecognizeIntentionQuote(RecognizeIntentionQuoteRequest request);

        IntentionQuoteReplyPo QueryIntentionQuote(IntentionQuoteRequest request);

        IntentionQuoteReplyPo QueryOriginRequest(IntentionQuoteRequest request);

        ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(ConfirmIntentionQuoteRequest request);

        PResult InsertIntentionQuote(InsertIntentionQuoteReq request);

        PResult DeleteIntentionQuote(DeleteIntentionQuoteReq request);

        PResult RelateIntention(RelateIntentionReq request);

        PResult UpdateIntentionQuote(UpdateIntentionQuoteRequest request);

        void SubscribeIntention(string[] topics, Action<IntentionQuoteInfo> onIntentionallback);
        void SubscribeRecognization(string[] topics, Action<IntentionQuoteInfo> onRecognitionCallback);

        void SubscribeOrigin(string[] topics, Action<IntentionQuoteInfo> onOriginCallback);
        IntentionOrderReplyPo QueryIntentionOrder(IntentionOrderRequest request);

        void SubscribeIntentionOrder(Action<IntentionOrderInfo> onIntentionOrderCallback);

        IntentionExecReplyPo QueryIntentionExec(IntentionExecRequest request);

        void SubscribeIntentionExec(Action<IntentionExecInfo> onIntentionExecCallback);

        QueryEmsOrderReplyPo QueryEmsOrder(QueryEmsOrderReq request);

        QueryRolesReplyPo QueryRoles();

        void ProcessOrigin(NetMessage netMsg);
        void ProcessIntention(NetMessage netMsg);
        void ProcessRecognition(NetMessage netMsg);
        void ProcessIntentionOrder(NetMessage netMsg);
        void ProcessIntentionExec(NetMessage netMsg);

        QueryHistoryIntentionOrderReplyPo QueryEmsOrderHist(QueryHistoryIntentionOrderReq request);

        PResult UnRefHistoryIntentionOrder(UnRefHistoryIntentionOrderReq request);

        QueryUnrefParamReplyPo QueryUnrefParam(QueryUnrefParamReq request);

        PResult UpdateUnrefParam(UpdateUnrefParamReq request);

        BrokerQuoteRecordReply queryBrokerHis(BrokerReq req);

        BrokerWhiteListReplyPo brokerWhiteList(BrokerWhiteList request);
        BrokerWhiteReplyPo brokerWhite(BrokerWhiteList request);
    }
}
