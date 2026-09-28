using STC.Common.Model.Ans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Ansshe
{
    public interface IAnssheHandler
    {
        void UnSubscribeAll(bool isSubscribe);
        void UnSubscribeAnsSummary(Action<ANSSummary> onMsgCallBack);

        void SubscribeAnsSummary(Action<ANSSummary> onMsgCallBack);

        void UnSubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack);

        void SubscribeAnsMonitorInfo(Action<ANSMonitorInfo> onMsgCallBack, List<string> userNameList);

        QueryAnsMonitorReplyPo QueryAnsMonitorInfo(string usename, string securitys);
        QueryANSDetailReplyPo QueryANSDetail(QueryANSDetailReq request);
        QueryANSOutlineReplyPo QueryANSOutline(QueryANSOutlineReq request);
    }
}
