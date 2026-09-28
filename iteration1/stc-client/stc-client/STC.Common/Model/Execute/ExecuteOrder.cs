using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Execute
{
    public class ExecuteOrder
    {
        public decimal AllocQty { get; set; }
        public decimal? AvgPx { get; set; }
        public string BrokerExecID { get; set; }
        public string PartyID { get; set; }
        public string PartyName { get; set; }
        public string PartySubID { get; set; }
        public string PartySubName { get; set; }
        public string CounterPartyName { get; set; }
        public string CounterPartySubName { get; set; }
        public decimal DeliveryType { get; set; }
        public string IndividualAllocID { get; set; }
        public string IndividualAllocID2 { get; set; }
        public string IndividualAllocID3 { get; set; }
        public decimal MarketIndicator { get; set; }
        public string MaturityDate { get; set; }
        public string OnBehalfOfCompID { get; set; }
        public string SecondaryTradeID { get; set; }
        public string SecurityID { get; set; }
        public decimal SecurityType { get; set; }
        public string SendTime { get; set; }
        public string SettlDate { get; set; }
        public string SettlType { get; set; }
        public decimal Side { get; set; }
        public string Symbol { get; set; }
        public string TradeDate { get; set; }
        public string TradeRequestID { get; set; }
        public string TradeID { get; set; }
        public string TradeStat { get; set; }
        public string AlloAccount { get; set; }
        public string TradeMatchTimestamp { get; set; }
        public decimal TransType { get; set; }
        public string TransactTime { get; set; }
        public string TrdMatchID { get; set; }
        public decimal TrdSubType { get; set; }
        public decimal TrdType { get; set; }
        public string UpdateFields { get; set; }
        public string UpdateTime { get; set; }

        public string CreateTime { get; set; }
        public string TradeType { get; set; }

        public string AllocText { get; set; }
        public string AllocText2 { get; set; }
        public string AllocText3{ get; set; }
        public decimal CleanPrice { get; set; }
        public decimal DirtyPrice { get; set; }
        public string Text { get; set; }
        public decimal? StipulationValue { get; set; }
        public decimal? Yield { get; set; }
        public string CounterPartyID { get; set; }
        public string CounterPartySubID { get; set; }
        public decimal? BridgePaymentFixedRate { get; set; }
        public decimal? BridgePaymentAmount { get; set; }
        public string BridgePaymentUnitOfMeasure { get; set; }
        public decimal? PaymentFixedRate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string PaymentUnitOfMeasure { get; set; }
        public decimal? BrokerPaymentRate { get; set; }
        public decimal? BrokerPaymentAmount { get; set; }
        public string BrokerPaymentUnitOfMeasure { get; set; }
        public string remark { get; set; }
        public string closeBy { get; set; }
        public string ordStatus { get; set; }
        public string PaymentType { get; set; }
        //成交价类型
        public string TransactionPriceType { get; set; }
        public string UserName { get; set; }
        public string ConfirmID { get; set; }
        //用于判断是否展示历史记录
        public string IsShow { get; set; }

    }
}
