using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class QueryFundAccountResult : BaseReplyPo
    {
        public List<FundAccountModel> Datas { get; set; }
    }
}
