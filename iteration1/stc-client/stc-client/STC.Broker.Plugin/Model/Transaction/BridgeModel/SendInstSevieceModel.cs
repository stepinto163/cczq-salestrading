using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.BridgeModel
{
   public class SendInstSevieceModel
    {
        /// <summary>
        /// 发单机构id
        /// </summary>
        public string issueInstId { get; set; }
        /// <summary>
        /// 发单机构名称
        /// </summary>
        public string issueInstName { get; set; }
        /// <summary>
        /// 发单量
        /// </summary>
        public decimal qty { get; set; }
        /// <summary>
        /// //创建人
        /// </summary>
        public string createby { get; set; }

        /// <summary>
        /// 发单机构状态
        /// </summary>
        public int IssueInstState { get; set; } = 2;

        /// <summary>
        /// 原因
        /// </summary>
        public string RejectReason { get; set; }

        /// <summary>
        /// 桥状态
        /// </summary>
        public BridgeState BridgeState
        {
            get
            {
                return (BridgeState)IssueInstState ;
            }
        }

        /// <summary>
        /// 发单机构状态展示
        /// </summary>
        public string BridgeStateShow { get; set; }
       
    }
}
