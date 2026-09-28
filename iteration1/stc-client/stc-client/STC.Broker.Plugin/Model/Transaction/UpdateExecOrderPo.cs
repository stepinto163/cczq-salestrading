
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class UpdateExecOrderPo
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public int commandType = 0;
        /// <summary>
        /// 成交单ID
        /// </summary>
        public string execId { get; set; }
        /// <summary>
        /// bid
        /// 结算方式
        /// </summary>
        public string settlementType { get; set; }
        /// <summary>
        /// ofr
        /// 结算方式
        /// </summary>
        public string settlementType2 { get; set; }
        /// <summary>
        /// 券码
        /// </summary>
        public string goodscode { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal? price { get; set; }
        /// <summary>
        /// 量
        /// </summary>
        public decimal? qty { get; set; }
        /// <summary>
        /// brokerA ID
        /// </summary>
        public string brokerida { get; set; }
        /// <summary>
        /// brokerB ID
        /// </summary>
        public string brokeridb { get; set; }
        /// <summary>
        /// 交易员A ID
        /// </summary>
        public string bankagentida { get; set; }
        /// <summary>
        /// 交易员B ID
        /// </summary>
        public string bankagentidb { get; set; }
        /// <summary>
        /// 桥状态：1不需要加桥 2需要加桥
        /// </summary>
        public string bridgeflag { get; set; }

        /// <summary>
        /// 主发给 bid发给
        /// </summary>
        public string sendto { get; set; }

        /// <summary>
        /// 主发给 ofr发给
        /// </summary>
        public string sendto2 { get; set; }
        /// <summary>
        /// broker查看标记
        /// </summary>
        public string brokerreadflag { get; set; }
        /// <summary>
        /// 债券简称
        /// </summary>
        public string goodsShortName { get; set; }
        /// <summary>
        /// 到期期限
        /// </summary>
        public string timeToMaturity { get; set; }

        /// <summary>
        ///  1:删除
        /// </summary>
        public string delflag { get; set; }

        /// <summary>
        /// ofr桥的机构简称
        /// </summary>
        public string bridgebankcodeshort { get; set; }
        /// <summary>
        /// ofr桥的机构名称
        /// </summary>
        public string bridgebankcode { get; set; }
        /// <summary>
        /// ofr桥的机构idbkey
        /// </summary>
        public string bridgebankid { get; set; }
        /// <summary>
        /// ofr桥的交易员idbkey
        /// </summary>
        public string bridgebankagentid { get; set; }
        /// <summary>
        /// ofr桥的交易员名称
        /// </summary>
        public string bridgebankagentname { get; set; }
        /// <summary>
        /// ofr桥联系方式
        /// </summary>
        public string contactType { get; set; }
        /// <summary>
        /// ofr桥的机构名称计费
        /// </summary>
        public string feeInstName { get; set; }
        /// <summary>
        /// ofr桥的机构idbkey计费
        /// </summary>
        public string feeInstId { get; set; }
        /// <summary>
        /// ofr桥的交易员idbkey计费
        /// </summary>
        public string feeInstTradeId { get; set; }
        /// <summary>
        /// ofr桥的交易员名称计费
        /// </summary>
        public string feeInstTradeName { get; set; }
        /// <summary>
        /// ofr桥的渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string bridgechannel { get; set; }
        /// <summary>
        /// ofr桥的发给
        /// </summary>
        public string bridgesendto { get; set; }
        /// <summary>
        /// ofr桥的费用
        /// </summary>
        public string bridgefee { get; set; }
        /// <summary>
        /// ofr桥的特殊佣金
        /// </summary>
        public string OfrSpecialBridgeFee
        {
            get;
            set;
        }
        /// <summary>
        /// ofr桥的备注
        /// </summary>
        public string bridgeremark { get; set; }
        /// <summary>
        /// ofr桥桥备注是否在明细中显示
        /// </summary>
        public string bridgeremarkshow { get; set; }
        /// <summary>
        /// ofr桥的桥类型
        /// 默认方式:  bid,ofr共有桥: B,O    bid的桥:B     offer的桥:O
        /// </summary>
        public string bridgeType { get; set; }
        /// <summary>
        /// Bid桥的机构简称
        /// </summary>
        public string bridgebankcodeshort2 { get; set; }
        /// <summary>
        /// Bid桥的机构名称
        /// </summary>
        public string bridgebankcode2 { get; set; }
        /// <summary>
        /// Bid桥的机构idbkey
        /// </summary>
        public string bridgebankid2 { get; set; }
        /// <summary>
        /// Bid桥的交易员idbkey
        /// </summary>
        public string bridgebankagentid2 { get; set; }
        /// <summary>
        /// Bid桥的交易员名称
        /// </summary>
        public string bridgebankagentname2 { get; set; }
        /// <summary>
        /// bid桥联系方式
        /// </summary>
        public string contactType2 { get; set; }
        /// <summary>
        /// Bid桥的机构简称计费
        /// </summary>
        public string feeBridgebankcodeshort2 { get; set; }
        /// <summary>
        /// Bid桥的机构名称计费
        /// </summary>
        public string feeInstName2 { get; set; }
        /// <summary>
        /// Bid桥的机构idbkey计费
        /// </summary>
        public string feeInstId2 { get; set; }
        /// <summary>
        /// Bid桥的交易员idbkey计费
        /// </summary>
        public string feeInstTradeId2 { get; set; }
        /// <summary>
        /// Bid桥的交易员名称计费
        /// </summary>
        public string feeInstTradeName2 { get; set; }
        /// <summary>
        /// Bid桥的渠道：1:对话，2:请求,3:双边,4:xbond,5:ideal
        /// </summary>
        public string bridgechannel2 { get; set; }
        /// <summary>
        /// Bid桥的发给
        /// </summary>
        public string bridgesendto2 { get; set; }
        /// <summary>
        /// Bid桥 桥备注是否在明细中显示
        /// </summary>
        public string bridgeremarkshow2 { get; set; }
        /// <summary>
        /// Bid桥的费用
        /// </summary>
        public string bridgefee2 { get; set; }
        /// <summary>
        /// bid桥的特殊佣金
        /// </summary>
        public string BidSpecialBridgeFee
        {
            get;
            set;
        }
        /// <summary>
        /// Bid桥的备注
        /// </summary>
        public string bridgeremark2 { get; set; }
        /// <summary>
        /// Bid桥的桥类型：默认方式:bid,ofr共有桥:B,O bid的桥:B offer的桥:O
        /// </summary>
        public string bridgeType2 { get; set; }
        /// <summary>
        /// 桥的状态 B按下和O按下
		/// QQQQQQQQQQQQQQQQQQQQQQQQQQ可能的值
        /// </summary>
        public string bridgeStatus { get; set; }
        /// <summary>
        /// 双桥渠道  
        /// </summary>
        public string doublebridgechannel { get; set; }
        /// <summary>
        /// 双桥发给  
        /// </summary>
        public string doublebridgesendto { get; set; }
        /// <summary>
        /// 双桥费用
        /// </summary>
        public string doublebridgefee { get; set; }
        /// <summary>
        /// 双桥备注
        /// </summary>
        public string doublebridgeremark { get; set; }
        /// <summary>
        /// 双桥offer清算类型
        /// </summary>
        public string doublebridgeoffersettlementtype { get; set; }
        /// <summary>
        /// 双桥bid清算类型 
        /// </summary>
        public string doublebridgebidsettlementtype { get; set; }
        /// <summary>
        /// 返点
        /// </summary>
        public string fandian { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 是否免佣
        /// Y：免佣 N：不免用 
        /// </summary>
        public string brokeragea { get; set; }
        /// <summary>
        /// 是否免佣
        /// Y：免佣 N：不免用 
        /// </summary>
        public string brokerageb { get; set; }
        /// <summary>
        /// 删除，修改，换桥的时候传给后台。
        /// </summary>
        public string operateBorderID { get; set; }
        /// <summary>
        /// Bid桥 合单备注
        /// </summary>
        public string combinedOrderRemark { get; set; }
        /// <summary>
        /// Bid桥 合单备注
        /// </summary>
        public string combinedOrderRemark2 { get; set; }
        /// <summary>
        /// 双桥错期选择，默认是Ofr ；且只有订单结算类型是错期且添加双桥，此字段才有意义。
        /// O：Ofr方错期
        /// B：Bid方错期
        /// </summary>
        public string doubleBridgeSettleSide { get; set; }
        /// <summary>
        /// ofr方向
        /// </summary>
        public int ofrDir { get; set; }
        /// <summary>
        /// bid方向
        /// </summary>
        public int bidDir { get; set; }
        /// <summary>
        /// bid2方向
        /// </summary>
        public int bidDir2 { get; set; }
        /// <summary>
        /// 桥之间方向
        /// </summary>
        public int doublebridgedir { get; set; }


        /// <summary>
        /// 成交不刷标记 0:默认未标记 1:标记表示成交不刷
        /// </summary>
        public string forbidRefreshFlag { get; set; }

        /// <summary>
        /// 手动发送TraderA成交
        /// </summary>
        public string sendSorFlaga { get; set; }


        /// <summary>
        /// 手动发送TraderB成交
        /// </summary>
        public string sendSorFlagb { get; set; }


        #region  20220214 -----2.17.0版本
        /// <summary>
        /// ofr方发单机构集合
        /// </summary>
        public List<SendInstSevieceModel> ofrIssueInfoReqList { get; set; }
        /// <summary>
        /// bid方发单机构集合
        /// </summary>
        public List<SendInstSevieceModel> bidIssueInfoReqList { get; set; }
        #endregion


        #region 3.0.0版本

        /// <summary>
        /// 发给方向 1：ofr发bid  2：bid发ofr
        /// </summary>
        public string issueDir { get; set; }
        /// <summary>
        /// 发给渠道
        /// </summary>
        public string issueChannel { get; set; }
        /// <summary>
        /// 发单机构id
        /// </summary>
        public string issueTraderId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string issueRemark { get; set; }
        /// <summary>
        /// 代付费机构id
        /// </summary>
        public string proxyInstId { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string proxyFee { get; set; }
        /// <summary>
        /// 代付费标记0：未开启代付费  1：开启代付费
        /// </summary>
        public string proxyFlag { get; set; }
        /// <summary>
        /// 发给动作描述
        /// </summary>
        public string issueDesc { get; set; }
        /// <summary>
        /// 开启发给标准化
        /// </summary>
        public string issueState { get; set; }

        /// <summary>
        /// 明细标记  0 未标记 1  已标记
        /// </summary>
        public string detailMark { get; set; }
        #endregion

        #region  20230207 -----3.3.0版本
        /// <summary>
        /// ofr方发单机构集合
        /// </summary>
        public string manualDealSide { get; set; }
        /// <summary>
        /// 手动发送单条明细qq消息标记a  0 待发送 1 已发送
        /// </summary>
        public string singleQQMsgSendFlaga { get; set; }
        /// <summary>
        /// 手动发送单条明细qq消息标记b  0 待发送 1 已发送
        /// </summary>
        public string singleQQMsgSendFlagb { get; set; }
        #endregion

    }
}
