using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class InsertIntentionQuoteReq
    {
        public string quoteType;
        public string userName;
        public List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> quoteList;
        public string riskReTryFlag;//风控试算
        public string riskManualCk;//跳过风控校验
        /// <summary>
        /// 0：投资经理录入
		/// 1：投资经理后端录入
		/// 2：交易员前端录入
		/// 3：交易员后端录入
		/// 4：成交前端录入
		/// 5：成交后端录入
        /// </summary>
        public string actionFlag;
        public string relatedUsers;//对谁可见
    }
}
