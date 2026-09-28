using netapi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BDB.Common.Model.OHLC;
using STC.Common.Model.MDHist;
using STC.Common.Api;

namespace BDB.Common.Handler.MDHist
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
            TKLineReply result = ApiManager.Instance.GetDefaultApi().Request<TKLineReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_KLine_Search, request);

            // 缓存kline 1m 数据
            if (result != null && !string.IsNullOrEmpty(result.Data))
            {
                List<OhlcMsg> klineDataList = JsonConvert.DeserializeObject<List<OhlcMsg>>(result.Data);
                foreach (OhlcMsg klineItem in klineDataList)
                {
                    if (klineItem.text == "1M")
                    {
                        Kline1MCache.Instance.AddOrUpdate(klineItem);
                    }
                }
            }
            return result;
        }

        public MDHistDReply MDHistD(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D, request, waitTime);
        }

        public MDHistDReply MDHistD2(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D2, request, waitTime);
        }

        public MDHistDReply MDHistD3(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D3, request, waitTime);
        }

        public MDHistDReply MDHistD4(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D4, request, waitTime);
        }

        public MDHistDReply MDHistD5(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D5, request, waitTime);
        }

        public MDHistDReply MDHistD6(MDHistDReq request, int waitTime)
        {
            return ApiManager.Instance.GetDefaultApi().Request<MDHistDReply>(ApiConsts.MDHISTSVR, ApiConsts.Topic_MDHist_D6, request, waitTime);
        }
    }
}
