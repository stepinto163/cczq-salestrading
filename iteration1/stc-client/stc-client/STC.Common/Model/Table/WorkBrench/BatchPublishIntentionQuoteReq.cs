using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table.WorkBrench
{
    public class BatchPublishIntentionQuoteReq
    {
        public String userName;

        public string RiskManualCk;

        public List<NewOrderSingle> quoteList;
    }
}
