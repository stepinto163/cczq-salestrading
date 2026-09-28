using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class BridgeConfigInfo
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public string InstId { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string InstName { get; set; }

        /// <summary>
        /// 交易员Id
        /// </summary>
        public string InstTraderId { get; set; }

        /// <summary>
        /// 交易员名称
        /// </summary>
        public string InstTraderName { get; set; }

        /// <summary>
        /// 限制数量
        /// </summary>
        public int BridgeOrderQuota { get; set; }

        /// <summary>
        /// 机构黑名单
        /// </summary>
        public List<BlackListInstitution> InstBlackListSettingList { get; set; }
    }

    public class BlackListInstitution
    {
        /// <summary>
        /// 桥机构Id
        /// </summary>
        public string BridgeInstId { get; set; }

        /// <summary>
        /// 桥机构交易员Id
        /// </summary>
        public string BridgeInstTraderId {get;set;}

        /// <summary>
        /// 黑名单机构名称
        /// </summary>
        public string BlackListInstName { get; set; }

        /// <summary>
        /// 黑名单机构Id
        /// </summary>
        public string BlackListInstId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Seq { get; set; }
    }
}
