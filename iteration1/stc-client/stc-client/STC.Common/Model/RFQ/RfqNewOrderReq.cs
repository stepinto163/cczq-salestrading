using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    public class RfqNewOrderReq
    {
        /// <summary>
        /// 市场 N
        /// </summary>
        public string MarketIndicator { get; set; }

        /// <summary>
        /// 消息类型，请求报价（R）,撤销请求（U32） Y
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 请求ID，报价编号  Y
        /// </summary>
        public string QuoteReqID { get; set; }

        /// <summary>
        /// 请求类别（默认112）
        /// </summary>
        public string QuoteType { get; set; }


        /// <summary>
        /// 客户编号  N
        /// </summary>
        public string ClOrdID { get; set; }

        /// <summary>
        /// BookID
        /// </summary>
        public string BookID { get; set; }

        /// <summary>
        /// 债券编号 Y
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 债券名称 Y
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 债券类型 N
        /// </summary>
        public string SecurityType { get; set; }


        /// <summary>
        /// Taker机构id N
        /// </summary>
        public string PartyID { get; set; }


        /// <summary>
        /// Taker机构名称 N
        /// </summary>
        public string PartyName { get; set; }

        /// <summary>
        /// Taker交易员ID N
        /// </summary>
        public string PartySubID { get; set; }

        /// <summary>
        /// Taker交易员名称 N
        /// </summary>
        public string PartySubName { get; set; }



        /// <summary>
        /// Maker机构ID N
        /// </summary>
        public string CounterPartyID { get; set; }


        /// <summary>
        /// Maker机构名称 N
        /// </summary>
        public string CounterPartyName { get; set; }

        /// <summary>
        /// Maker交易员名称 N
        /// </summary>
        public string CounterPartySubID { get; set; }

        /// <summary>
        /// Maker交易员名称 N
        /// </summary>
        public string CounterPartySubName { get; set; }

        /// <summary>
        /// 订单类型 N
        /// </summary>
        public string OrdType { get; set; }

        /// <summary>
        /// 请求方向 Y
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// 报价价格 Y
        /// </summary>
        public string Price { get; set; }


        /// <summary>
        /// 报价量 Y
        /// </summary>
        public string OrderQty { get; set; }


        /// <summary>
        /// 交易日 N
        /// </summary>
        public string TradeDate { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public string TradeTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 报价编号,首次回价不传，修改回价必传  有则必传
        /// </summary>

        public string TimeInForce { get; set; }


        /// <summary>
        /// 时长记忆
        /// </summary>
        public string Info1 { get; set; }


        /// <summary>
        /// 回复有效时间
        /// </summary>
        public string Info2 { get; set; }

        public string Info3 { get; set; }

        public string Info4 { get; set; }

        public string Info5 { get; set; }

        /// <summary>
        /// 更新时间yyyy-MM-dd HH:mm:ss  N
        /// </summary>
        public string UpdateTime { get; set; }


        /// <summary>
        /// 请求持续时间，格式YYYYMMDD-HH:MM:SS.sss
        /// </summary>
        public string ValidUntilTime { get; set; }


        /// <summary>
        ///  N
        /// </summary>
        public string SettlType { get; set; }

        /// <summary>
        ///  N
        /// </summary>
        public string SettlDate { get; set; }

        /// <summary>
        ///  N
        /// </summary>
        public string TransactTime { get; set; }

        /// <summary>
        /// 交易子渠道（inner交易为空，默认QDM） N
        /// </summary>
        public string VenueTypeGW { get; set; }

        /// <summary>
        /// 交易渠道（inner交易为空，默认QDM） N
        /// </summary>
        public string Venues { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// 清算速度 N
        /// </summary>
        public string ClearingMethod { get; set; }

        /// <summary>
        /// 结算方式 N
        /// </summary>
        public string DeliveryType { get; set; }

        /// <summary>
        /// 是否风控校验，Y/N
        /// </summary>
        public string RiskManualCk { get; set; }

        public string MacID { get; set; }

        public string PublishIP { get; set; }

        public string InsideIP { get; set; }

        public string HardDiskID { get; set; }
    }
}
