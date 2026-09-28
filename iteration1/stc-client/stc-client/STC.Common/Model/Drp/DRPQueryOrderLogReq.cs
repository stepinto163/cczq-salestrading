using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    public class DRPQueryOrderLogReq
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string orderid;

        /// <summary>
        ///  查询类型 (1-报单方，2-询价、点价方)
        /// </summary>
        public string queryType;
    }
}
