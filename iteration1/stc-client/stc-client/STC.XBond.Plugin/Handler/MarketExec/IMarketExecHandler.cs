using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Handler.MarketExec
{
    public interface IMarketExecHandler
    {
        event Action<ExecutionReport> OnOrder;

        void Init();

        void Clear();
    }
}
