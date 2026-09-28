using STC.Common.Api;
using netapi.Api;

using STC.Common.Model.EMS;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.EMS
{
    public class EMSHandler : IEMSHandler
    {
        private EMSHandler() { }
        private static IEMSHandler server = new EMSHandler();


        public static IEMSHandler Instance { get { return server; } }

        public PResult updateOrder(UpdateOrderReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_ems_updateExecOrder, request); //ClientApi.Instance.updateOrderForEMS(request);
        }
    }
}
