using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class ConfirmBridgeOrdersResponse: BaseReplyPo
    {
        public List<int> FailSeqList { get; set; }

    }
}
