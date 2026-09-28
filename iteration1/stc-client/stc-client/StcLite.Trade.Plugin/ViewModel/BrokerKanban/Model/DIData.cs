using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StcLite.Trade.Plugin.ViewModel.BrokerKanban.Model
{
    /// <summary>
    /// 需求意向结构
    /// </summary>
    public class DIData
    {
        /// <summary>
        /// 用户报价方向  Bid：买入 Ofr：卖出
        /// </summary>
        public string side { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 债券简称
        /// </summary>
        public string symbol { get; set; }

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settltype { get; set; }

        /// <summary>
        /// 清算日期
        /// </summary>
        public string settlDate { get; set; }
        /// <summary>
        /// 意向  Firm"闪电标记" NonFirm 如果为nonfirm，标注初始点价时长
        /// </summary>
        public string firmflag { get; set; }

        /// <summary>
        /// 收益率（%）
        /// </summary>
        public decimal EntryPx { get; set; }


        public decimal lastPx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal lastQty { get; set; }

        /// <summary>
        /// 数量（万元）
        /// </summary>
        public decimal orderQty { get; set; }


        public decimal cumQty { get; set; }

        public decimal leavesQty { get; set; }


        //经纪商
        public string venues { get; set; }

        public string ordtype { get; set; }

        /// <summary>
        /// 报价类型
        /// </summary>
        public string quoteType { get; set; }

        /// <summary>
        /// 债券类型/产品类型
        /// </summary>
        public string symboltype { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string ordStatus { get; set; }


        /// <summary>
        /// 执行状态
        /// </summary>
        public string execType { get; set; }

        /// <summary>
        /// 当ordStatus=4时，必传
        /// </summary>
        public string execTypeReason { get; set; }

        /// <summary>
        /// 当OrdStatus=8时，描述订单拒绝理由
        /// </summary>
        public string ordRejReason { get; set; }

        /// <summary>
        /// 当OrdRejReason=99时，表示错误原因的文字说明
        /// </summary>
        public string rejectText { get; set; }


        /// <summary>
        /// qtr订单编号
        /// </summary>
        public string orderid { get; set; }

        /// <summary>
        /// 客户订单编号
        /// </summary>
        public string clordid { get; set; }

        /// <summary>
        /// 备注 
        /// </summary>
        public string comment { get; set; }


        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string updatetime { get; set; }

        /// <summary>
        /// 本方交易员
        /// </summary>
        public string tradername { get; set; }
    }
}
