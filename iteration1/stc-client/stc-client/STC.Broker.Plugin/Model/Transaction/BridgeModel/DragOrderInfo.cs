using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.BridgeModel
{
    [Serializable]
    public class DragOrderInfo
    {
        public string ExecId { get; set; }


        public string Seq { get; set; }
    }
}
