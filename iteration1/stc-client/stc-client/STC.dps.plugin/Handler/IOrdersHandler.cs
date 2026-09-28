using com.gateway.connector.tcp;
using STC.Common.Model.Fix.Message;
using STC.dps.plugin.QM.po;

namespace STC.dps.plugin.Handler
{
    public interface IOrdersHandler
    {
        void Init(GateWayApi GateWayApi, string userName);
        void OnMessage(ExecutionReport EX);
        QMPo IsContain(string key, NewOrderSingle order);
        ExecutionReport IsContainID(string Id);
        int getVersionByVenue(string venue);
        void Clear();


        QMPo IsContainEMS(string key, ExecutionReport order);
    }
}
