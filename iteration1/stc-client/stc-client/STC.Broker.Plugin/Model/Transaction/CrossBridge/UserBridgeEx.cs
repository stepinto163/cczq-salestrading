using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class UserBridgeEx : UserBridge
    {
        public string InstTraderName
        {
            get
            {
                return string.Format("{0}({1})", bankInstitutionname, bankTradername);
            }
        }
    }
}
