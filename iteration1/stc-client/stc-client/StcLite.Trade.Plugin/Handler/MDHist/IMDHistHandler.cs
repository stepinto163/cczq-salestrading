using STC.Common.Model.MDHist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Common.Handler.MDHist
{
    public interface IMDHistHandler
    {
        TKLineReply SearchKLine(KLineSearchParam request);
        MDHistDReply MDHistD(MDHistDReq request, int waitTime);

        MDHistDReply MDHistD2(MDHistDReq request, int waitTime);

        MDHistDReply MDHistD3(MDHistDReq request, int waitTime);

        MDHistDReply MDHistD4(MDHistDReq request, int waitTime);
        MDHistDReply MDHistD5(MDHistDReq request, int waitTime);
        MDHistDReply MDHistD6(MDHistDReq request, int waitTime);
    }
}
