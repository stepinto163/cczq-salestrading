using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    public class RfqChangeAcceptOrderReq
    {
        /// <summary>
        /// 请求ID，报价编号  Y
        /// </summary>
        public string QuoteReqID { get; set; }

        /// <summary>
        /// 报价编号,首次回价不传，修改回价必传  有则必传
        /// </summary>
        public string QuoteID { get; set; }

        /// <summary>
        /// 报价类型  N
        /// </summary>
        public string QuoteType { get; set; }


        /// <summary>
        /// 债券状态  Y   撤40，拒38
        /// </summary>
        public string QuoteStatus { get; set; }

        /// <summary>
        /// 请求类型  N
        /// </summary>
        public string QuoteRequestType { get; set; }

        /// <summary>
        /// 业务时间yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string TransactTime { get; set; }

        /// <summary>
        /// 债券编号 Y
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 债券名称 N
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 订单类型   N
        /// </summary>
        public string OrdType { get; set; }


        /// <summary>
        /// 报价量 Y
        /// </summary>
        public string OrderQty { get; set; }

        /// <summary>
        /// 报价价格 Y
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice { get; set; }

        /// <summary>
        /// 报价过期时间
        /// </summary>
        public string ValidUntilTime { get; set; }

        /// <summary>
        /// 更新时间yyyy-MM-dd HH:mm:ss  N
        /// </summary>
        public string UpdateTime { get; set; }

        /// <summary>
        /// 回价被拒原因   N
        /// </summary>
        public string QuoteRejectReason { get; set; }

        /// <summary>
        /// 客户编号  N
        /// </summary>
        public string ClOrdID { get; set; }


        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 时长记忆
        /// </summary>
        public string Info1 { get; set; }


        /// <summary>
        /// 回复有效时间
        /// </summary>
        public string Info2 { get; set; }

        /// <summary>
        /// 收益率 Y
        /// </summary>
        public string Yield { get; set; }

        /// <summary>
        /// 是否风控校验，Y/N
        /// </summary>
        public string RiskManualCk { get; set; }
        public string MacID { get; set; }

        public string PublishIP { get; set; }

        public string InsideIP { get; set; }

        public string HardDiskID { get; set; }

        /// <summary>
        /// BookID
        /// </summary>
        public string BookID { get; set; }

        /// <summary>
        /// 手动回价 Manual
        /// </summary>
        public string Info5 { get; set; }

    }
}
