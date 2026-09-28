using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.FinQ
{
    public interface IFinQHandler
    {
        PResult SendFQMessage(SendFQMessageReq request);

        PResult SendDesktopFinQ(SendDesktopFinQReq request);

        GetUserConfigFinqReplyPo GetUserConfig(GetUserConfigFinqRequest request);

        PResult SendFQGroupMessage(SendFQGroupMessageReq request);

        void SendWebFQ();

        void SendWebFQ(FQDataModel fstm = null, FQUpdateDataModel updm = null);

        void SendWebFQ(FQUpdateDataModel fstm = null, FQUpdateDataModel updm = null);

        PResult GetFinQServerToken(GetFinQTokenReq request);
    }
}
