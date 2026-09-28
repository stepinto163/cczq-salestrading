using netapi.Api;
using STC.Common.Api;
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
   public class GLADAPTERHandler: IGLADAPTERHandler
    {

        private static GLADAPTERHandler gLADAPTERHandler = new GLADAPTERHandler();
   
        public static IGLADAPTERHandler Instance { get { return gLADAPTERHandler; } }

        private event Action<TtrdOtcPoolReplyPo> OnTtrdOtcPool;
        private bool isSubscribeForTtrdOtcPool = false;


        public PResult syncPosAndTrd(SyncPosAndTrd request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.GLADAPTERSvr, ApiConsts.Topic_stc_syncPosAndTrd, request);
        }

        public void SubscribeTtrdOtcPool(Action<TtrdOtcPoolReplyPo> onConStatusCallback)
        {
            if (onConStatusCallback != null)
                OnTtrdOtcPool += onConStatusCallback;

            if (!isSubscribeForTtrdOtcPool)
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnForTtrdOtcPool,
                     new string[] { ApiConsts.Topic_stc_blackWhiteBroadcast },
                        ApiConsts.GLADAPTERSvr, "");
                isSubscribeForTtrdOtcPool = true;
            }
        }

        private void Instance_OnForTtrdOtcPool(object sessionStatus)
        {
            TtrdOtcPoolReplyPo data = BaseHelper.ConvertStcMessage<TtrdOtcPoolReplyPo>(new string[] { ApiConsts.Topic_stc_blackWhiteBroadcast }, sessionStatus);
            if (data != null)
                OnTtrdOtcPool?.Invoke(data);
        }

    }
}
