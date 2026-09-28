using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Limit
{
    public class QuerySummaryReply : PResult
    {

        public List<BondSummaryInfo> datas;
    }

    public class BondSummaryInfo
    {
        public string id;
        public string leaseMaturityDate;
        public decimal mxLimitAmount;
        public string qtpAccount;
        public decimal sTotalOpenLimit;
        public decimal sTotalUsedLimit;
        public string securityId;
        public string settleDate;
        public string settleType;
        public string status;
        public string symbol;
        public decimal totalOpenLimit;
        public decimal totalUsedLimit;
        public string tradeDate;
        public string agencyName;
        public decimal t0AvailableLimit;
        public decimal t0UsedLimit;
        public decimal t1AvailableLimit;
        public decimal t1UsedLimit;
    }
}
