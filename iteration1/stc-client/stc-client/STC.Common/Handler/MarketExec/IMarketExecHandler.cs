using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MarketExec
{
    public interface IMarketExecHandler
    {
        void OnClear();

        void Init(Action<ExecutionReport> onOrderCallBack);

        void SubscribeOrder(Action<ExecutionReport> onOrderCallBack);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribe(Action<ExecutionReport> onOrderCallBack);

        TBookOrder QueryExecOrderForEMS(QueryOrderReq req);

        void UnsubscribeOrder(Action<ExecutionReport> onOrderCallBack);

        TBookOrder QueryOrderForEMS(QueryOrderReq req);

        void SubscribeOrderBySecurtityIDs(List<string> securitys, Action<ExecutionReport> onOrderCallBack);

        void UnsubscribeOrderBySecurtityIDs(List<string> securitys);

        void ProcessEmsOrder(NetMessage netMsg);

        void ProcessExecuteOrder(NetMessage netMsg);

        void InitByBroker(Action<ExecutionReport> onOrderCallBack, string user, List<string> brokerList);

        void SetLoadDataStatus(bool loadData);
    }
}
