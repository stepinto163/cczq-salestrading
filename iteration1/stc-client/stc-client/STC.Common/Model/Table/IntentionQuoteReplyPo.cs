using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionQuoteReplyPo : BaseReplyPo
    {
        public List<IntentionQuoteInfo> Datas;
        public int Total;
        public int TotalBid;
        public int TotalOfr;
    }
}
