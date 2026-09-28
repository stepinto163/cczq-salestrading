using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
   public  class QueryFeeRequestPo: BaseReplyPo
    {

        /// <summary>
        /// 机构id
        /// </summary>
        public string proxyInstId { get; set; }
    }
}
