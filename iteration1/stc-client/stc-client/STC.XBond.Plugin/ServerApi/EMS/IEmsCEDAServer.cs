using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ServerApi.EMS
{
    public interface IEmsCEDAServer
    {
        //event Action<List<SessionStatus>> OnEMSConStatus;
        //event Action<ExecutionReport> OnEMSExecutionReport;
        //event Action<OrderCancelReject> OnEMSCancelReject;

        event Action<ExecutionReport> OnExecuteOrder;
        event Action<ExecutionReport> OnOwnExecuteOrder;

        string Topic_stc_ems_placeorder { get; }
        string Topic_stc_ems_cancelorder { get; }
        string Topic_stc_ems_executionreport { get; }
        string Topic_stc_ems_cancelreject { get; }
        string Topic_stc_ems_constatus { get; }

        void Init(string sessionID);
        void UnsubscribAll();
        PResult PlaceOrderForEMS(NewOrderSingle order);
        PResult CancelOrderForEMS(OrderCancelRequest orderCancelRequset);
        void SubscribeForEMSExecutionReport(Action<string, ExecutionReport> msgCallback);
        void SubscribeForEMSCancelReject(Action<string, OrderCancelReject> msgCallback);
        void SubscribeForEMSConStatus(Action<string, SessionStatus> onEMSConStatus);

        /// <summary>
        /// 市场成交
        /// </summary>
        void SubscribeEMSExceptionReport();

        void SubscribeEMSOwnExecuteOrder();
    }
}
