using STC.Common.Model.ODM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Risk;
using netapi.Api;
using STC.Common.Model.Quant;

namespace STC.Common.Handler.TBook
{
    public interface ITBookHandler
    {
        void OnClear();
        void Init(Action<ExecutionReport> onOrderCallBack, Action<List<Bond>> onBondCallBack);

        void InitQuant(string UserName,string quantid, Action<ExecutionReport> onOrderQuantCallBack);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribeOrder(Action<ExecutionReport> onOrderCallBack);

        void UnSubscribeQuantIdOrder(TTbookQuantStrategys quantid);

        void UnSubscribeBond(Action<List<Bond>> onBondCallBack);

        List<ExecutionReport> GetOrderBySymbol(string symbol);

        TBookCancelReplyPo CancelOrder(Model.TBook.OrderCancelRequest cancelPo);

        PResult PlaceOrder(NewOrderSingle newOrder);

        PResult OpenCloseBond(List<BondOperationPo> request);

        TBookOrder QueryOrderForTBook(QueryOrderReq req);
        TBookOrder QueryQuantOrderForTBook(QueryOrderReq req);

        void SubscribeConStatus(Action<List<SessionStatus>> onConStatusCallback);

        void SubscribeSystemParam(Action<UserSetting> onSystemParamCallback);

        PResult UpdatesystemParas(UserSetting request);

        PResult XBondMDSub(List<MdSecurityIdReq> req);

        MdSecurityidsReplyPo queryMDSecurityId(MdSecurityIdReq req);

        RiskParaReplyPo queryRiskPara(RiskParaReq req);

        RiskParaReplyPo queryRiskPara(QueryRiskParameter req);
        PResult updateRiskPara(RiskParaReq req);

        PResult updateOrder(NewOrderSingle req);

        void ProcessOrder(NetMessage netMsg);

        void ProcessQuantOrder(NetMessage netMsg);

        void ProcessConStatus(NetMessage netMsg);

        void ProcessSystemParam(NetMessage netMsg);

		OrderMassCancelReplyPo CancelOrderBatch(OrderMassCancelRequest cancelRequest);
    }
}
