using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.Table.WorkBrench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Table
{
    public interface ITableOldHandler
    {
        STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RequestIntention(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request);
        STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RecognizeIntentionQuote(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request);

        STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo QueryIntentionQuote(STC.Common.Model.Table.WorkBrench.IntentionQuoteRequest request);

        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo ConfirmIntentionQuote(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request);

        Model.Table.WorkBrench.InsertIntentionQuoteReplyPo InsertIntentionQuote(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request);

        PResult DeleteIntentionQuote(STC.Common.Model.Table.WorkBrench.DeleteIntentionQuoteReq request);

        PResult UpdateIntentionQuote(STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request);

        void SubscribeIntention(string[] topics, Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> onIntentionallback);
        void SubscribeRecognization(string[] topics, Action<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> onRecognitionCallback);
        void SubscribeQor(Action<STC.Common.Model.Table.EmsOrderReport> onRecognitionCallback, bool isPermission = true);
        void SubscribeQor_Irs(Action<STC.Common.Model.Table.EmsOrderReport> onRecognitionCallback, bool isPermission = true);

        QueryEmsOrderReplyPo QueryEmsOrder(QueryEmsOrderReq request);
        QueryEmsOrderReplyPo QueryEmsOrder_Irs(QueryEmsOrderReq request);

        QueryRolesReplyPo QueryRoles();

        QueryQuoteTagGroupReplyPo QueryQuoteTagGroup(QueryQuoteTagGroupRequest req);

        QueryQuoteTagReplyPo QueryQuoteTag();

        void ProcessRecognition(NetMessage netMsg);
        void ProcessIntention(NetMessage netMsg);
        void ProcessQor(NetMessage netMsg);

        void ProcessQor_Irs(NetMessage netMsg);

        QueryTagsCountReplyPo QueryTagsCount(QueryTagsCountRequest req);

        QueryTagsLoadReplyPo QueryTagsLoad(QueryTagsLoadRequest req);
        void SetLoadDataStatus_Issuer(bool loadData);
        void SubscribeLoadData_Issuer(Action<bool> onLoadData);

        void UnSubscribeLoadData_Issuer(Action<bool> onLoadData);

        void SetLoadDataStatus_Guarantor(bool loadData);

        void SubscribeLoadData_Guarantor(Action<bool> onLoadData);

        void UnSubscribeLoadData_Guarantor(Action<bool> onLoadData);

        IntentionInquiryPo IntentionInquiry(IntentionInquiryRequest request);

        void SubscribeInquiry(string[] topics, Action<TableIntentionInquiry> onCallback);

        void ProcessInquiry(NetMessage netMsg);

        PResult BatchPublishIntention(Model.Table.WorkBrench.BatchPublishIntentionQuoteReq req);


        STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo GetBuyerIntentionLogs(string intentionid);

        IntentionExecutionAssociateReplyPo ProcessIntentionExecutionAssociate(IntentionExecutionAssociateReq req);

        void CefSubScribeWithImage(string[] topic);

        InsertIntentionQuoteReplyPo InsertIntentionQuoteNew(STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq request);
    }
}
