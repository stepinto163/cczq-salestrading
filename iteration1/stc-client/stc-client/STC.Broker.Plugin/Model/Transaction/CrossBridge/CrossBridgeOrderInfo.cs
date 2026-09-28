using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class CrossBridgeOrderInfo
    {
        /// <summary>
        /// 主订单Id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 执行订单Id
        /// </summary>
        public string ExecuteOrderId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int OrderSeq { get; set; }

        /// <summary>
        /// 第一个 Borker 名字
        /// </summary>
        public string BrokerAName { get; set; }

        /// <summary>
        /// 第二个 Borker 名字
        /// </summary>
        public string BrokerBName { get; set; }

        /// <summary>
        /// 订单文本
        /// </summary>
        public string OrderText { get; set; }

    }
}
