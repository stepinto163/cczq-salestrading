using com.gateway.connector.tcp;
using STC.Common.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static dps.plugin.GWPlugin;

namespace STC.dps.plugin.Handler
{
    public interface IBaseHandler
    {
        void Init(GateWayApiWraper gww);
        BrokerAccount IsContainBroker(string Venue, string SecurityType);
        void CacheBroker(List<BrokerAccount> list);
    }
}
