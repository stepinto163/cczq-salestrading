using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Ans
{
    public interface IAnsHandler
    {
        void UnSubscribeAll(bool isSubscribe);
        void UnSubscribeAnsSummary(Action<ANSSummary> onMsgCallBack);

        void SubscribeAnsSummary(Action<ANSSummary> onMsgCallBack);

        void UnSubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack);

        void SubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack, List<string> userNameList);

        QueryAnsMonitorReplyPo QueryAnsMonitorInfo(string usename, string securitys);
        QueryANSDetailReplyPo QueryANSDetail(QueryANSDetailReq request);
        QueryANSOutlineReplyPo QueryANSOutline(QueryANSOutlineReq request);
        PResult IgnoreBreak(IgnoreBreakReq request);
    }
}
