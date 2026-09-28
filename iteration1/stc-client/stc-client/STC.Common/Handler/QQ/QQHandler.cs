using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.QQ
{
    public class QQHandler : IQQHandler
    {
        private static QQHandler instance = new QQHandler();
        public static QQHandler Instance { get { return instance; } }

        public QQHandler()
        {
            //Base.BaseHandler.Instance.Init(Instance_OnClear);
        }

        public PResult SendQQ(SendFinQFriendReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QQSvr, ApiConsts.sendFinQFriend_Topic, request);
        }

        public PResult SendQQ_Best(SendFinQFriendReq_Best request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.QQSvr, ApiConsts.sendFinQFriend_Topic, request);
        }
    }
}
