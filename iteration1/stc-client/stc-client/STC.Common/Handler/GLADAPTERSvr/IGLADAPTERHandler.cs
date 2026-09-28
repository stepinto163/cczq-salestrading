using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Gladapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.GLADAPTERSvr
{
   public interface IGLADAPTERHandler
    {
        PResult syncPosAndTrd(SyncPosAndTrd request);
        void SubscribeTtrdOtcPool(Action<TtrdOtcPoolReplyPo> onConStatusCallback);
    }
}
