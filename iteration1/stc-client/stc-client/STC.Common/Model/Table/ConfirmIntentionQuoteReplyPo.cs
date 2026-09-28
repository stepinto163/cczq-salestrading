using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class ConfirmIntentionQuoteReplyPo : BaseReplyPo
    {
        //报价唯一编号
        public string intentionId;
        //订单唯一编号
        public string orderId;
        //债券编号
        public string securityId;
        // 订单状态,0=待处理,1=已处理,2=已成交,3=已忽略
        public string status;
    }
}
