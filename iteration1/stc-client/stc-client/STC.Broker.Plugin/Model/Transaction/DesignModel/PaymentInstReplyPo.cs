using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
    public class PaymentInstReplyPo : BaseReplyPo
    {

        public List<PaymentInstDetail> Data;

        /// <summary>
        /// 总条数
        /// </summary>
        public int total { get; set; }
    }
}
