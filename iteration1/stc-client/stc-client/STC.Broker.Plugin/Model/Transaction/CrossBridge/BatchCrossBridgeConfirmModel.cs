using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class BatchCrossBridgeConfirmModel
    {
        /// <summary>
        /// 运行批次
        /// </summary>
        public string BatchNumber { get; set; }

        /// <summary>
        /// 桥机构Id
        /// </summary>
        public string InstId { get; set; }

        /// <summary>
        /// 桥机构简称
        /// </summary>
        public string InstName { get; set; }

        /// <summary>
        /// 桥机构下交易员Id
        /// </summary>
        public string InstTraderId { get; set; }

        /// <summary>
        /// 桥机构下交易员名称
        /// </summary>
        public string InstTraderName { get; set; }

        /// <summary>
        /// 执行订单id
        /// </summary>
        public List<string> ProcessDetails { get; set; } = new List<string>();
    }
}
