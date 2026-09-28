using netapi.Api;
using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.CDH;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.PT
{
    public interface IPTHandler
    {
        PtExecordersReplyPo ExecOrderQuery(PtExecOrderQueryReq request);
        void SubscribeExec(Action<List<TbookPtExecorders>> onMsgCallBack, string UserName);
        void ProcessPTExec(NetMessage netMsg);
        PtExecSendReplyPo ExecOrderSend(PtExecOrderSaveReq req);
        PResult ExecOrderSave(PtExecOrderSaveReq req);
        PResult ExecOrderBatchSave(List<PtExecOrderSaveReq> req);
        PtExecUpdateReplyPo ExecOrderUpdate(PtExecOrderSaveReq req);
        PResult ExecOrderDelete(PtExecOrderSaveReq req);
        void OnSendPT(DataRowView obj);
        void SubscribeSendPT(Action<DataRowView> callData);
        PtExecUpdateReplyPo ExecOrderBatchUpdate(List<PtExecOrderSaveReq> req);
        PtExecordersReplyPo ExecOrderUnSendQuery(PTExecOrderUnSendReq req);

        PtExecordersReplyPo ExecOrderApprovalHistQuery (List<string> request);
        PtExecordersReplyPo ExecOrderMvtQuery(PtExecOrderQueryReq request);


        PResult ExecOrderRiskCheck(PtExecOrderQueryReq request);

    }
    
}
