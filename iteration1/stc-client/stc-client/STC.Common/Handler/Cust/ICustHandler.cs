using STC.Common.Model.Cust;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Cust
{
    public interface ICustHandler
    {
        QueryCustMdReplyPo QueryCustMd(QueryCustMdReq request);
    }
}
