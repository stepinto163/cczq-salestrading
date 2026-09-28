using STC.Common.Model.MDHist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MDHist
{
    public interface IMDHistHandler
    {
        TKLineReply SearchKLine(KLineSearchParam request);
        MDHistDReply MDHistD(MDHistDReq request);

        MDHistDReply MDHistD2(MDHistDReq request);

        MDHistDReply MDHistD3(MDHistDReq request);

        MDHistDReply MDHistD4(MDHistDReq request);
        MDHistDReply MDHistD5(MDHistDReq request);
        MDHistDReply MDHistD6(MDHistDReq request);
    }
}
