using STC.Common.Model;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Cal
{
    public interface ICalHandler
    {

        CalResult CalBond(CalReq requst);

        CalWeekDateResult calSettlementDate(CalWeekDateReq request);

        CalCurveResult CalCurve(CalCurveReq req);
    }
}
