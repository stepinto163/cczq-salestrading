using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class CalSettleDateReplyPo
    {
        public CalSettleDatePo Data{ get; set; }
    }

    public class CalSettleDatePo
    {
        public string dealDate { get; set; }
        public string settlementDate { get; set; }
        public string settlementType { get; set; }
    }
}
