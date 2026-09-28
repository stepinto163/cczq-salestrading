using STC.Common.Model.Limit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Limit
{
    public interface ILimitHandler
    {
        QuerySummaryReply QuerySummary(QuerySummaryReq request);

        QueryDetailsReply QueryDetailSummary(QueryDetailsReq request);
    }
}
