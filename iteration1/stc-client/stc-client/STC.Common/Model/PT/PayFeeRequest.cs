using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
   public class PayFeeRequest
    {
        /// <summary>
        /// 机构a id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 机构b id
        /// </summary>
        public string instId2 { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string action { get; set; } = "0";
    }
}
