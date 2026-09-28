using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.QQ
{
    public interface IQQHandler
    {
        PResult SendQQ(SendFinQFriendReq request);

        PResult SendQQ_Best(SendFinQFriendReq_Best request);
    }
}
