using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.OwnExec
{
    public interface IOwnExecHandler
    {

        void Init(Action<ExecutionReport> onOrderCallBack);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribe(Action<ExecutionReport> onOrderCallBack);

        TBookOrder QueryOrderForTBook(QueryOrderReq req);
    }
}
