
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class HisRecordPo
    {
        public static string Flag = "@";
        public string OperateTime { get; set; }
        public string OperatePerson { get; set; }
        public string OperateType { get; set; }
        public string TimeToMaturity { get; set; }
        public string GoodsCode { get; set; }
        public string GoodsShortName { get; set; }
        public string Price { get; set; }
        public string PriceDescription { get; set; }
        public string Qty { get; set; }
        public string Settle { get; set; }
        public string InstA { get; set; }
        public string InstB { get; set; }
        public string BrokerA { get; set; }
        public string BrokerB { get; set; }
        public string SendTo { get; set; }
        public string BridgeSatus { get; set; }
        public string OfrSendTo { get; set; }
        public string BidSendTo { get; set; }
        public string DoubleBridgeSettle { get; set; }
        public string DoubleBridgeInfo { get; set; }
        //是否免佣 
        public string BrokerageA { get; set; }
        //是否免佣 
        public string BrokerageB { get; set; }

        #region 3.0.0版本

        /// <summary>
        /// 发给方向 1：ofr发bid  2：bid发ofr
        /// </summary>
        public string issueDir { get; set; }

        /// <summary>
        /// 发给方向 1：ofr发bid  2：bid发ofr
        /// </summary>
        public string IssueDirShow { get; set; }

        /// <summary>
        /// 发给渠道
        /// </summary>
        public string issueChannel { get; set; }

        /// <summary>
        /// 发给渠道
        /// </summary>
        public string IssueChannelShow { get; set; }
        /// <summary>
        /// 发单机构id
        /// </summary>
        public string issueInstId { get; set; }
        /// <summary>
        /// 发单机构名称
        /// </summary>
        public string issueInstName { get; set; }
        /// <summary>
        /// 发单人id
        /// </summary>
        public string issueTraderId { get; set; }
        /// <summary>
        /// 发单人名称
        /// </summary>
        public string issueTraderName { get; set; }

        /// <summary>
        /// 发单人展示
        /// </summary>
        public string IssueTraderShow
        {
            get; set;
        }

        /// <summary>
        /// 发单人名称
        /// </summary>
        public string issueTraderNameShow { get; set; }

        /// <summary>
        /// 发给备注
        /// </summary>
        public string issueRemark { get; set; }
        /// <summary>
        /// 发给动作描述
        /// </summary>
        public string issueDesc { get; set; }
        /// <summary>
        /// 代付费机构id
        /// </summary>
        public string issueProxyInstId { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string issueProxyFee { get; set; }
        /// <summary>
        /// 代付费标记0：未开启代付费  1：开启代付费
        /// </summary>
        public string IssueProxyFlag { get; set; }

        /// <summary>
        /// 发给标准化开启状态 0 未开启 1 开启
        /// </summary>
        public string issueState { get; set; }
        /// <summary>
        /// 1 展示全部  2 展示渠道和备注
        /// </summary>
        public string issueShowType { get; set; }
        #endregion

        /// <summary>
        /// 发给方向 1：GVN  2：TKN 3：TRD
        /// </summary>
        public string ForbidRefreshFlag { get; set; }

    }
}
