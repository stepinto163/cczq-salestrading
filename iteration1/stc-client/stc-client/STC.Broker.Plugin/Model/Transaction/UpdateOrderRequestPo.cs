using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class UpdateOrderRequestPo
    {
        public string id { get; set; }

        public List<UpdateExecOrderPo> execOrders { get; set; }
    }
}
