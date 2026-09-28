using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    /// <summary>
    /// 质押券持仓
    /// </summary>
    public class StcPledgePositionModel
    {
        /// <summary>
        /// 组合代码
        /// </summary>
        public string portfolioNo { get; set; }

        /// <summary>
        /// 组合名称
        /// </summary>
        public string portfolioName { get; set; }

        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string tradeAccount { get; set; }
        
        /// <summary>
        /// 交易账户名称
        /// </summary>
        public string tradeName { get; set; }

        /// <summary>
        /// 类型：1:质押, 2:STC在途意向,3:STC在途成交
        /// </summary>
        public string tradeType { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        public string side { get; set; }

        /// <summary>
        /// 量
        /// </summary>
        public decimal qty { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>
        public string maturityDate { get; set; }

        /// <summary>
        /// 对手方
        /// </summary>
        public string counterparty { get; set; }

    }
}
