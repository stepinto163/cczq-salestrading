using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class BatchCrossBridgeModel
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
        public string InstName { get; set;}

        /// <summary>
        /// 桥机构下交易员Id
        /// </summary>
        public string InstTraderId { get; set;}

        /// <summary>
        /// 桥机构下交易员名称
        /// </summary>
        public string InstTraderName { get; set; }

        /// <summary>
        /// 桥下订单数量上限
        /// </summary>
        public int BridgeOrderQuota { get; set; }

        /// <summary>
        /// 已使用额度
        /// </summary>
        public int UsedOrderQuota { get; set; }

        /// <summary>
        /// 当前批次桥下待确认订单数量
        /// </summary>
        public int ProcessOrderQuota { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// execid->orderid 订单Id
        /// </summary>
        public Dictionary<string, BridgeOrderModel> ProcessDetails { get; set; } = new Dictionary<string, BridgeOrderModel>();

        /// <summary>
        /// 桥下订单信息
        /// </summary>
        public List<CrossBridgeOrderInfo> BridgeOrderInfo { get; set; } = new List<CrossBridgeOrderInfo>();


        public BatchCrossBridgeConfirmModel ConvertToConfirmModel()
        {
            var model = new BatchCrossBridgeConfirmModel
            {
                BatchNumber = BatchNumber,
                InstId = InstId,
                InstName = InstName,
                InstTraderId = InstTraderId,
                InstTraderName = InstTraderName
            };

            if (ProcessDetails != null)
            {
                foreach (var item in ProcessDetails)
                {
                    model.ProcessDetails.Add(item.Value.ExecId);
                }
            }
            return model;
        }
    
    }
}
