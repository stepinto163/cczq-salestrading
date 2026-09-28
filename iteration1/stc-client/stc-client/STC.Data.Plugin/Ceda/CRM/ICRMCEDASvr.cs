

using STC.Data.Plugin.Model.CRM;

namespace STC.Data.Plugin.Ceda.CRM
{
    public interface ICRMCEDASvr
    {

        BrokerSearchReplyPo SearchBroker(CRMRequestParamPo requestParam);


        CrmInstReply SearchCRMinst(CRMRequestParamPo request);

        CrmContactReply SearchCRMcontact(CRMRequestParamPo request);
    }
}
