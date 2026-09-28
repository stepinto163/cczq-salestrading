using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.MDHist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MDHist
{
    public class MDHistHandler : IMDHistHandler
    {
        private static MDHistHandler mdHistHandler = new MDHistHandler();

        public static MDHistHandler Instance { get { return mdHistHandler; } }

        private MDHistHandler()
        {

        }


        public TKLineReply SearchKLine(KLineSearchParam request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<TKLineReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_KLine_Search, request);
        }

        public MDHistDReply MDHistD(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D, request);
        }

        public MDHistDReply MDHistD2(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D2, request);
        }

        public MDHistDReply MDHistD3(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D3, request);
        }

        public MDHistDReply MDHistD4(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D4, request);
        }

        public MDHistDReply MDHistD5(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D5, request);
        }

        public MDHistDReply MDHistD6(MDHistDReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D6, request);
        }
    }
}
