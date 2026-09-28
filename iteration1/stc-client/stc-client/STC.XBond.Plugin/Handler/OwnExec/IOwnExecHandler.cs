using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Handler.OwnExec
{
    public interface IOwnExecHandler
    {
        event Action<ExecutionReport> OnOrder;

        void Init();

        void Clear();
    }
}
