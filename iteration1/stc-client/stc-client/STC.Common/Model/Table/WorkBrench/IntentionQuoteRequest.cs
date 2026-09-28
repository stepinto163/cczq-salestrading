using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class IntentionQuoteRequest
    {
        // stc用户名
        public string userName;
        // 订单状态 ,0=待处理,1=已处理,2=已成交, 3=已忽略
        public string status;
        // 日期，格式yyyy-MM-dd，不传代表所有
        public string date;
        // 部门
        public string biztypeid;

        public string startdate;

        public string enddate;

        /// <summary>
        /// 一。买方机构
        ///   个需：query 传quoteType传BUYER_INTENTION
        ///   私域：query 传quoteType传DEMAND
        /// 二。 卖方机构
        ///   个需：query 传quoteType传INTENTION
        ///   私域：query 传quoteType传DEMAND
        /// </summary>
        public string quoteType;
        public string securityId;
        public string side;
    }
}
