using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Cal;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
using STC.Common.Model;

namespace STC.Common.Handler.Cal
{
    public class CalHandler : ICalHandler
    {
        private CalHandler() { }
        private static ICalHandler calHandler = new CalHandler();


        public static ICalHandler Instance { get { return calHandler; } }

        public CalResult CalBond(CalReq requst)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CalResult>(ApiConsts.CalSvr, ApiConsts.Topic_stc_calbond, requst); //ClientApi.Instance.calBondForCal(requst);
        }

        public CalWeekDateResult calSettlementDate(CalWeekDateReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CalWeekDateResult>(ApiConsts.EMSSVR, ApiConsts.Topic_stc_calSettlementDate, request); //ClientApi.Instance.calSettlementDateForCal(request);
        }

        public CalCurveResult CalCurve(CalCurveReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CalCurveResult>(ApiConsts.CalSvr, ApiConsts.Topic_stc_calCurve, req);
        }
    }
}
