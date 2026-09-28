using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Limit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Limit
{
    public class LimitHandler : ILimitHandler
    {
        private static LimitHandler limitHandler = new LimitHandler();

        public static ILimitHandler Instance { get { return limitHandler; } }


        private LimitHandler()
        {

        }


        // 额度汇总查询
        public QuerySummaryReply QuerySummary(QuerySummaryReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QuerySummaryReply>(ApiConsts.LimitSvr, ApiConsts.Topic_stc_limit_summary, request);
        }

        public QueryDetailsReply QueryDetailSummary(QueryDetailsReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryDetailsReply>(ApiConsts.LimitSvr, ApiConsts.Topic_stc_limit_details, request);
        }
    }
}
