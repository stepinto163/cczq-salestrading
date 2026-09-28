using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.EMS
{
    /// <summary>
    /// 更新订单请求参数
    /// </summary>
    public class UpdateOrderReq
    {
        public string OrderId;
        public string Price;
        public string Counterpartyid;
        public string Counterpartyname;
        public string Counterpartysubid;
        public string Counterpartysubname;
        public string Synchflag;

        public string InstitutionID;
        public string InstitutionName;
        public string TraderID;
        public string TraderName;
        public string Info1;
        public string Info2;
        public string Info5;

        public string TradeMethod;
        public string Info6;
        public string IntentionId;//意向单id
        public string tag1;// 标签
    }
}
