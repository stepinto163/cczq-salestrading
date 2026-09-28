using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class BridgeOrderModel
    {
        /// <summary>
        /// 执行订单id
        /// </summary>
        public string ExecId { get; set; }

        /// <summary>
        /// 主订单Id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 废弃不用了
        /// </summary>
        public string ProxyFee { get; set; }

        /// <summary>
        /// bid方订单桥费用
        /// </summary>
        public string BidProxyFee { get; set; }

        /// <summary>
        /// ofr方订单桥费用
        /// </summary>
        public string OfrProxyFee { get; set; }
    }
}
