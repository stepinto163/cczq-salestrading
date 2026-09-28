using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    /*
     * 报文：
     * 	    "borderID": "b727105896987000832",
			"bridgeSource": 1,
			"bridgeState": 0,
			"bridgeType": 0,
			"brokerId": "420095879",
			"createBy": "420095879",
			"createTime": "2020-06-29 10:19:14",
			"execId": "e727104206007209985",
			"instId": "2a4622f4346a647001346a7c14f800a2",
			"instName": "恒生银行",
			"needMvt": false,
			"operateType": 2,
			"state": 1,
			"updateBy": "420095879",
			"updateTime": "2020-06-29 10:19:14"
         */


    /// <summary>
    ///  加桥订单
    /// </summary>
    public class bridgeOrder
    {
        /// <summary>
        /// 桥订单号
        /// </summary>
        public string borderID { get; set; }
        /// <summary>
        /// 桥来源：1-添加、2-预约、3-换桥
        /// </summary>
        public int bridgeSource { get; set; }
        /// <summary>
        /// 桥状态: 0-待发送，1-待回复，2-已确认，3-已拒绝
        /// 20200807 王兴鹏 注意 
        /// 保留int类型，用于与后台接口对接，序列化整形数据
        /// 前端使用枚举类型包装过的属性，这样字段的每个数值的业务含义就清晰了。
        /// </summary>
        public int bridgeState { get; set; }

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
        /// 桥类型: 0-单桥、1-Bid桥、2-Ofr桥
        /// </summary>
        public int bridgeType { get; set; }
        public string brokerId { get; set; }
        public string createBy { get; set; }
        public string createTime { get; set; }
        /// <summary>
        /// 执行订单id
        /// </summary>
        public string execId { get; set; }
        /// <summary>
        /// 联系人 机构id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 联系人 机构名称
        /// </summary>
        public string instName { get; set; }
        /// <summary>
        /// 联系人 交易员id
        /// </summary>
        public string instTradeId { get; set; }
        /// <summary>
        /// 联系人 交易员名称
        /// </summary>
        public string instTradeName { get; set; }
        public string reserveBrokerId { get; set; }
        public int state { get; set; }
        public string updateBy { get; set; }
        public string updateTime { get; set; }
        /// <summary>
        /// 合单备注
        /// </summary>
        public string combinedOrderRemark { get; set; }
        //*********V2.8 添加**************
        /// <summary>
        /// 计费（机构id）
        /// </summary>
        public string feeInstId { get; set; }
        /// <summary>
        /// 计费（机构id）
        /// </summary>
        public string feeInstName { get; set; }
        /// <summary>
        /// 计费（交易员id）
        /// </summary>
        public string feeInstTradeId { get; set; }
        /// <summary>
        /// 计费（交易员名称）
        /// </summary>
        public string feeInstTradeName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string contactType { get; set; }
        //*********V2.11 添加**************
        /// <summary>
        /// ofr 约定号
        /// </summary>
        public string ofrAgreementNo { get; set; }
        /// <summary>
        /// bid 约定号
        /// </summary>
        public string bidAgreementNo { get; set; }
        /// <summary>
        /// ofr方向1-桥方发起，2-对方发起
        /// </summary>
        public int ofrDir { get; set; }

        /// <summary>
        /// bid方向 1-桥方发起，2-对方发起
        /// </summary>
        public int bidDir { get; set; }

        /// <summary>
        /// bid方向备注
        /// </summary>
        public string bidOriginalBridgeRemark { get; set; }

        /// <summary>
        /// ofr方向备注
        /// </summary>
        public string ofrOriginalBridgeRemark { get; set; }


        /// <summary>
        /// ofr特殊佣金
        /// </summary>
        public string OfrSpecialBridgeFee { get; set; }

        /// <summary>
        /// bid特殊佣金
        /// </summary>
        public string BidSpecialBridgeFee { get; set; }

        /// <summary>
        /// 和桥具有绑定关系的brokers
        /// </summary>
        public List<string> CrmBindBrokers { get; set; }

        /// <summary>
        /// ofr拒绝原因
        /// </summary>
        public string ofrRejectReason { get; set; }
        /// <summary>
        /// bid拒绝原因
        /// </summary>
        public string bidRejectReason { get; set; }
    }
}
