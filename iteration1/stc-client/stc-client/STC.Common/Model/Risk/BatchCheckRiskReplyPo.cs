using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    public class BatchCheckRiskReplyPo
    {
        public string Code;
        public Dictionary<string, List<TPlaceOrderResult>> Msg;
        public bool IsSuccess()
        {
            return "0000".Equals(Code);
        }
    }
}
