using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Quant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Quant
{
    public interface IQuantHandler
    {
        PResult quantGroupUpdate(QuantGroupReq id);
        TbookQuantGroupsReplyPo quantGroupQuery();
        //TTbookQuantStrategysReplyPo quantQuantStrategysQuery();

        void UnSubscribeMM(Action<List<TTbookQuantStrategys>> onMsgCallBack);

        void SubscribeMM(Action<List<TTbookQuantStrategys>> onMsgCallBack, string UserName);

        PResult quantStrategyUpdate(QuantStrategyReq req);
        TTbookQuantParameterReplyAlter quantParasUpdate(TTbookQuantParameterRequest req);
        PResult quantUpdateEnable(QuantStrategyReq req);
        TbookQuantLogReplyPo QuantLogQuery(QuantLogSelReq req);
        TbookQuantTempletReplyPo QueryTemplet();
        TbookQuantIndicatorReplyPo QueryIndicator(TbookQuantIndicatorReq req);
        TTbookQuantStrategysReplyPo QuantAllStrategys();
        PResult disableStrategys(List<TTbookQuantStrategys> list);

        void ProcessMM(NetMessage netMsg);
        void ProcessParas(NetMessage netMsg);
        void ProcessQuantLog(NetMessage netMsg);
        void ProcessIndicator(NetMessage netMsg);
    }
}
