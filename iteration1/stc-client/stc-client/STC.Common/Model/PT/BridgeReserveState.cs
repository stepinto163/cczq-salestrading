using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 桥预约状态
    /// </summary>
    public class BridgeReserveState
    {
        public String borderId;//桥订单号
        public String execId;//订单号
        public int bridgeState;//桥状态
        public EBridgeState bridgeStateEnum
        {
            get { return (EBridgeState)bridgeState; }
            set { bridgeState = (int)value; }
        }

        /// <summary>
        /// ofr桥状态
        /// </summary>
        public int ofrState { get; set; }

        /// <summary>
        /// bid桥状态
        /// </summary>
        public int bidState { get; set; }

        /// <summary>
        /// ofr桥状态
        /// </summary>
        public BridgeState OfrbridgeStateEnum
        {
            get { return (BridgeState)ofrState; }
            set { ofrState = (int)value; }
        }
        /// <summary>
        /// bid桥状态
        /// </summary>
        public BridgeState BidbridgeStateEnum
        {
            get { return (BridgeState)bidState; }
            set { bidState = (int)value; }
        }

        /// <summary>
        /// 状态类型：1-桥状态；2-发单机构状态
        /// </summary>
        public int stateType { get; set; }

        /// <summary>
        /// 发单机构id
        /// </summary>
        public string issueInstId { get; set; }

        /// <summary>
        /// 发单机构名称
        /// </summary>
        public string issueInstName { get; set; }

        /// <summary>
        /// 本方方向：1-卖方；2-买方
        /// </summary>
        public int sid { get; set; }

        /// <summary>
        /// 发单机构状态
        /// </summary>
        public int issueInstState { get; set; }

        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string rejectReason { get; set; }
    }
}
