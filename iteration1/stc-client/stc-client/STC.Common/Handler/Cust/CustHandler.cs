using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.Cust;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Cust
{
    public class CustHandler : ICustHandler
    {
        private static ICustHandler server = new CustHandler();
        public static ICustHandler Instance { get { return server; } }

        private CustHandler() { }

        public QueryCustMdReplyPo QueryCustMd(QueryCustMdReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryCustMdReplyPo>(ApiConsts.CustSvr, ApiConsts.Topic_stc_cust_md_query, request);
        }
    }
}
