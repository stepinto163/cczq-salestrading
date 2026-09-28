using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 机构交易员信息
    /// </summary>
    public class IssueInstTrader
    {
        /// <summary>
        /// 机构id
        /// </summary>
        public string instId { get; set; }

        /// <summary>
        /// 机构交易员id (联系人id)
        /// </summary>
        public string instTraderId { get; set; }

        /// <summary>
        /// 机构交易员名称 (联系人)
        /// </summary>
        public string instTraderName { get; set; }

        /// <summary>
        /// 发单人id
        /// </summary>
        public string instIssueId { get; set; }

        /// <summary>
        /// 发单人名称
        /// </summary>
        public string instIssueName { get; set; }

        /// <summary>
        /// 交易所联系人id
        /// </summary>
        public string instExchangeContactsId { get; set; }

        /// <summary>
        /// 交易所联系人名称
        /// </summary>
        public string instExchangeContactsName { get; set; }

        /// <summary>
        /// 交易所联系人QQ
        /// </summary>
        public string instExchangeContactsQQ { get; set; }

        //备注
        public string remark;
    }
}
