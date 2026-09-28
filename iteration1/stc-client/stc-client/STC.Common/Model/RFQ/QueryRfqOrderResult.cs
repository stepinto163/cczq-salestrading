using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    public class QueryRfqOrderResult : BaseReplyPo
    {
        public List<ExecutionReport> Datas { get; set; }
    }
}
