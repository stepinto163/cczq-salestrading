using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MarketExec
{
    public class ExecComparer : IComparer<ExecutionReport>
    {
        public int Compare(ExecutionReport x, ExecutionReport y)
        {
            if (x == null || y == null)
                return 0;
            else
                return x.TransactTime.CompareTo(y.TransactTime);
        }
    }
}
