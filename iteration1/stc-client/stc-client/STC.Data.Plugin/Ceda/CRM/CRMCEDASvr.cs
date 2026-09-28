
using STC.Frame.Utils;
using Ats.Foundation.Message;
using STC.Data.Plugin.Model.CRM;

namespace STC.Data.Plugin.Ceda.CRM
{
    public class CRMCEDASvr : BaseCEDAServer, ICRMCEDASvr
    {
        private static ICRMCEDASvr instance = new CRMCEDASvr();
        public static ICRMCEDASvr Instance { get { return instance; } }

        private string SvrId { get { return "CRMSvr"; } }
        private const string topic_crm_inst = "STC.broker.crm.inst.query";
        private const string topic_crm_Contact = "STC.broker.crm.contact.query";
        private const string topic_Broker_Search = "STC.broker.crm.account.query";
        public string Topic_Broker_Search { get { return topic_Broker_Search; } }


        public BrokerSearchReplyPo SearchBroker(CRMRequestParamPo request)
        {
            string content = MsgHelper.Serializer(request);
            AtsMsg<BrokerSearchReplyPo> msg = RequestMessage<BrokerSearchReplyPo>(SvrId, topic_Broker_Search, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        public CrmInstReply SearchCRMinst(CRMRequestParamPo request)
        {
            string content = MsgHelper.Serializer(request);
            AtsMsg<CrmInstReply> msg = RequestMessage<CrmInstReply>(SvrId, topic_crm_inst, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }

        public  CrmContactReply SearchCRMcontact(CRMRequestParamPo request)
        {
            string content = MsgHelper.Serializer(request);
            AtsMsg<CrmContactReply> msg = RequestMessage<CrmContactReply>(SvrId, topic_crm_Contact, content, "JSON");
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    return msg.Body;
                }
            }
            return null;
        }
    }
}
