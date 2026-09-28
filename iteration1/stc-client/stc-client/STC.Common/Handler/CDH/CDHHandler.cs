using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.CDH;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.CDH
{
    public class CDHHandler : AsyncBase<object>, ICDHHandler
    {

        private bool isSubscribeForANS = false;

      
        private bool isSubscribeForANSSummary = false;

      

        private static CDHHandler ansHandler = new CDHHandler();
        public static ICDHHandler Instance { get { return ansHandler; } }

        private CDHHandler()
        {
           // this.start();
        }

       
      
        /// <summary>
        /// 行情录入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PResult EnterMarketRequest(QmDataInfo request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.CDHSvr, ApiConsts.Topic_qm_EnterMarket, request);
        }

       
    }
}
