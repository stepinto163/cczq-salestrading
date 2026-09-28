using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PaymentInstReplyPo : BaseReplyPo
    {

        public List<PaymentInstDetail> Datas;

        /// <summary>
        /// 总条数
        /// </summary>
        public int total { get; set; }
    }
}
