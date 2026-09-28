using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class BatchCheckRiskReplyPo
    {
        public string Code;
        public Dictionary<string, RiskResult> Msg;
        public bool IsSuccess()
        {
            return "0000".Equals(Code);
        }
    }

    public class RiskResult
    {
        public List<TPlaceOrderResult> FailMsg;
        public StcBondPositionModel position;
    }
}
