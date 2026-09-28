using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    /// <summary>
    /// 查询质押券持仓
    /// </summary>
    public class QueryPledgePositionReq
    {
        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string tradeAccount { get; set; }

        /// <summary>
        /// 组合代码
        /// </summary>
        public string portfolioNo { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityId { get; set; }
    }
}
