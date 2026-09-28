using netapi.Api;
using STC.Common.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.CRM
{
    public interface ICRMHandler
    {
        CrmContactReply QueryContact(CRMRequestParamPo request);

        CrmInstReply QueryInst(CRMRequestParamPo request);

        BrokerSearchReplyPo QueryBroker(CRMRequestParamPo request);

        void SubscribeCrmContact(Action<CrmContactReplyModel> klineCallBack);
        void SubscribeCrmInstMsg(Action<CrmInstReplyModel> klineCallBack);
        void SubscribeOnCrmAccountMsg(Action<CrmAccountReplyModel> klineCallBack);

        void ProcessCrmContactReplyModel(NetMessage netMsg);

    }
}
