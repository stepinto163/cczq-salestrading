using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    //private String borderId;//桥订单号
    //private int operateType;//操作类型
    //private String reviewId;//查看单号
    //private int bridgeState;//桥状态
    //private String createBy; //操作人
    //private String createTime;//操作时间
    //private String instId;//桥机构ID
    //private String instTradeId;//桥机构交易员ID
    //private String reserveBrokerId;//预约人账号-经纪人id
    // 桥预约查看
    /// </summary>
    public class BridgeReserveReviews
    {
        public String borderId;//桥订单号
        public String execId;//订单号
        public int operateType;//操作类型
        public EOperateType operateTypeEnum
        {
            get { return (EOperateType)operateType; }
            set { operateType = (int)value; }
        }
        public String reviewId;//查看单号
        public int bridgeState;//桥状态
        public EBridgeState bridgeStateEnum
        {
            get { return (EBridgeState)bridgeState; }
            set { bridgeState = (int)value; }
        }
        //桥状态
        public String createBy; //操作人
        public String createTime;//操作时间
        public String instId;//桥机构ID
        public String instTradeId;//桥机构交易员ID

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
    }

   
}
