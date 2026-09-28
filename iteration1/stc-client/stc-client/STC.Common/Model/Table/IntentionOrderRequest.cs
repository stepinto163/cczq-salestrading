using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionOrderRequest
    {
        // stc用户名
        public string userName;
        // 意向单编号
        public string intentionId;
        // 意向订单状态，多个逗号分割
        public string status;
        // 日期，格式yyyy-MM-dd，不传代表所有
        public string date;
    }
}
