using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class QueryPledgePositionResult : BaseReplyPo
    {
        public int totalSize { get; set; }
        public List<StcPledgePositionModel> Datas { get; set; }
    }
}
