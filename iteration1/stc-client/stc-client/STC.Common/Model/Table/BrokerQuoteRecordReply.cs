using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class BrokerQuoteRecordReply : BaseReplyPo
    {
        public int totalSize { get; set; }
        public List<BrokerQuoteRecord> Datas;
    }
}