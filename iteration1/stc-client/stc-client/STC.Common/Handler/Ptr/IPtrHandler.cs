using netapi.Api;
using STC.Common.Model.Execute;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Ptr
{
    public interface IPtrHandler
    {
        ExeOrderSearchResp SearchExeOrder(ExecuteOrderCondition executeOrderCondition);

        void Init(Action<ExecuteOrder> onOrderCallBack, string user);

        void UnSubscribeOrder(Action<ExecuteOrder> onOrderCallBack);
        PResult updateTradeRemark(UpdateRemarkRequest req);
        ExeOrderSearchResp SearchHistoryExeOrder(HistoryExecuteOrderCondition executeOrderCondition);
        PResult updateTradeConfirm(ExecuteConfirmRequest req);

        void ProcessPtrOrder(NetMessage netMsg);
    }
}


