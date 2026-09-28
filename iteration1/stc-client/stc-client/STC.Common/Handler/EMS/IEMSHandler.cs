using STC.Common.Model.EMS;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.EMS
{
    public interface IEMSHandler
    {
        PResult updateOrder(UpdateOrderReq request);
    }
}
