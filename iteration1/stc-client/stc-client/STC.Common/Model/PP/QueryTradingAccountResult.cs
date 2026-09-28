using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class QueryTradingAccountResult : BaseReplyPo
    {
        public List<StcTradingAccountModel> Datas { get; set; }
    }
}
