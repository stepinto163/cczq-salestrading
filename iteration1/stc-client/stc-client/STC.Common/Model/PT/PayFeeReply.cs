using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PayFeeReply : BaseReplyPo
    {
        /// <summary>
        /// 代付方机构id
        /// </summary>
        public string issueProxyInstId { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string issueProxyFee { get; set; }

        public string Datas { get; set; }
    }


    public class PayFeeModel {
        /// <summary>
        /// 代付方机构id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 代付费用
        /// </summary>
        public string instProxyFee { get; set; }
    }
}
