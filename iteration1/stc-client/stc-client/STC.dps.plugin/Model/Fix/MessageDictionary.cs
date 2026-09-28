using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix
{
    /// <summary>
    /// MessageDictionary
    /// </summary>
    /// <remarks>
    /// 2020.09.28: 创建. 模板插件(V1.3.7488.16761(build:20200702)) 模板版本(v1.0.1.0)<br/>
    /// </remarks>
    public class MessageDictionary
    {

        public static class FieldsEnum
        {

            public static class ExecAckStatus
            {
                public static readonly string RECEIVED = "0";
                public static readonly string ACCEPTED = "1";
                public static readonly string REJECTED = "2";
            }

            public static class TradSesStatus
            {
                public static readonly string START = "7";
                public static readonly string OPEN = "2";
                public static readonly string BREAK = "8";
                public static readonly string CLOSED = "3";
                public static readonly string ENDED = "9";
                public static readonly string CQSTIME = "18";
                public static readonly string CQETIME = "22";
                public static readonly string CMSTIME = "19";
            }

            public static class VenueTypeGW
            {
                public static readonly string CDH = "CDH";
                public static readonly string SOR = "SOR";
                public static readonly string IDB = "IDB";
                public static readonly string ODM = "ODM";
                public static readonly string QDMESP = "QDMESP";
                public static readonly string NDM = "NDM";
                public static readonly string QDM = "QDM";
                public static readonly string INNER = "INNER";
                public static readonly string HTQDM = "HTQDM";
                public static readonly string TBOOK = "TBOOK";
                public static readonly string ManualBroker = "ManualBroker";
                public static readonly string OFFLINE = "OFFLINE";
            }

            public static class Venues
            {
                public static readonly string TPSC = "TPSC";
                public static readonly string CNEX = "CNEX";
                public static readonly string CCTB = "CCTB";
                public static readonly string PTCN = "PTCN";
                public static readonly string CTTJ = "CTTJ";
                public static readonly string UEDA = "UEDA";
                public static readonly string IDB = "IDB";
                public static readonly string ODM = "ODM";
                public static readonly string QDMESP = "QDMESP";
                public static readonly string NDM = "NDM";
                public static readonly string QDM = "QDM";
                public static readonly string QUANT = "QUANT";
                public static readonly string MANUAL = "MANUAL";
                public static readonly string HTQDM = "HTQDM";
                public static readonly string TBOOK = "TBOOK";
                public static readonly string OFFLINE = "OFFLINE";
            }

            public static class MDBookType
            {
                public static readonly string QDMESP = "123";
                public static readonly string QDMESPZ = "124";
                public static readonly string QDM = "201";
                public static readonly string ODM = "202";
            }

            public static class SubscriptionRequestType
            {
                public static readonly string Subscribe = "1";
                public static readonly string Unsubscribe = "2";
            }

            public static class MarketIndicator
            {
                public static readonly string Bond = "4";
            }

            public static class SecurityType
            {
                public static readonly string Interest_Rate = "BNC";
                public static readonly string Credit_Bond = "BOC";
            }

            public static class SettlType
            {
                public static readonly string T0 = "1";
                public static readonly string T1 = "2";
                public static readonly string Tom0 = "3";
                public static readonly string Tom1 = "4";
            }

            public static class Side
            {
                public static readonly string Bid = "1";
                public static readonly string Ofr = "2";
            }

            public static class QuoteTransType
            {
                public static readonly string New = "N";
                public static readonly string Update = "R";
                public static readonly string Canceled = "C";
                public static readonly string Confirmed = "1";
                public static readonly string Refuse = "2";
                public static readonly string Refuse_Ack = "3";
            }

            public static class OrdType
            {
                public static readonly string Market = "1";
                public static readonly string Limit = "2";
            }

            public static class OrdStatus
            {
                public static readonly string Pending_New = "N";
                public static readonly string New = "0";
                public static readonly string Partially_filled = "2";
                public static readonly string Canceled = "4";
                public static readonly string Freezed = "I";
                public static readonly string Rejected = "5";
                public static readonly string Invalid = "8";
                public static readonly string Expired = "21";
            }

            public static class ExecType
            {
                public static readonly string Trade = "F";
                public static readonly string Rejected = "5";
                public static readonly string Canceled = "4";
            }

            public static class TimeInForce
            {
                public static readonly string Day = "0";
                public static readonly string IOC = "3";
                public static readonly string GFT = "A";
            }

            public static class InternalFlag
            {
                public static readonly string External = "1";
                public static readonly string Internal = "2";
            }

            public static class BargainFlag
            {
                public static readonly string Zero_Star = "0";
                public static readonly string One_Star = "1";
                public static readonly string Two_Star = "2";
            }

            public static class OCOFlag
            {
                public static readonly string OCO = "1";
                public static readonly string Bale = "2";
            }

            public static class Terminal
            {
                public static readonly string Terminals = "0";
                public static readonly string API = "2";
            }

            public static class WholeFlag
            {
                public static readonly string Whole = "1";
                public static readonly string None = "0";
            }

            public static class VenuesSelectF
            {
                public static readonly string None = "1";
                public static readonly string Default = "2";
                public static readonly string SelfSelection = "3";
                public static readonly string RandomVenues = "4";
            }

            public static class DealType
            {
                public static readonly string InterDealFlag = "1";
                public static readonly string ExternalDealFlag = "2";
            }

            public static class CrossVenues
            {
                public static readonly string All = "1";
                public static readonly string Inner = "2";
                public static readonly string InnerXBond = "3";
            }

            public static class RiskManualCk
            {
                public static readonly string Y = "Y";
                public static readonly string N = "N";
            }

            public static class ShowTrader
            {
                public static readonly string Y = "Y";
                public static readonly string N = "N";
            }

            public static class SynchFlag
            {
                public static readonly string Accomplish = "Y";
                public static readonly string PendingParty = "S";
                public static readonly string PendingCounterParty = "C";
            }

            public static class PartyOrderSource
            {
                public static readonly string TBOOK = "TBOOK";
                public static readonly string IDB = "IDB";
                public static readonly string QUANT = "QUANT";
            }

            public static class CounterPartyOrderSource
            {
                public static readonly string TBOOK = "TBOOK";
                public static readonly string IDB = "IDB";
                public static readonly string QUANT = "QUANT";
                public static readonly string ODM = "ODM";
                public static readonly string QDMESP = "QDMESP";
                public static readonly string TPSC = "TPSC";
                public static readonly string CTTJ = "CTTJ";
                public static readonly string CNEX = "CNEX";
                public static readonly string CCTB = "CCTB";
                public static readonly string PTCN = "PTCN";
                public static readonly string UEDA = "UEDA";
                public static readonly string MARKET = "MARKET";
            }

            public static class RoleType
            {
                public static readonly string SalesAgentFlag = "SalesAgentFlag";
                public static readonly string SelfDealsFlag = "SelfDealsFlag";
            }

            public static class TransType
            {
                public static readonly string Add = "A";
                public static readonly string Update = "U";
                public static readonly string Resend = "S";
                public static readonly string Delete = "D";
            }

            public static class HandlInst
            {
                public static readonly string Hide = "1";
                public static readonly string Show = "2";
            }

            public static class CounterPartyVenues
            {
                public static readonly string IDB = "IDB";
                public static readonly string MANUAL = "MANUAL";
                public static readonly string MARKET = "MARKET";
                public static readonly string SALESAGENT = "SALESAGENT";
                public static readonly string SELFDEALS = "SELFDEALS";
                public static readonly string TPSC = "TPSC";
                public static readonly string CTTJ = "CTTJ";
                public static readonly string CNEX = "CNEX";
                public static readonly string CCTB = "CCTB";
                public static readonly string PTCN = "PTCN";
                public static readonly string UEDA = "UEDA";
                public static readonly string ODM = "ODM";
                public static readonly string QDMESP = "QDMESP";
                public static readonly string QUANT = "QUANT";
            }

            public static class ParentFlag
            {
                public static readonly string Y = "Y";
                public static readonly string N = "N";
            }
        }


        public static class Fields
        {
            public static readonly string ExecAckStatus = "ExecAckStatus";
            public static readonly string TradSesStatus = "TradSesStatus";
            public static readonly string MsgType = "MsgType";
            public static readonly string TargetSessionId = "TargetSessionId";
            public static readonly string NetStatus = "NetStatus";
            public static readonly string VenueTypeGW = "VenueTypeGW";
            public static readonly string Venues = "Venues";
            public static readonly string MDBookType = "MDBookType";
            public static readonly string SubscriptionRequestType = "SubscriptionRequestType";
            public static readonly string MDReqID = "MDReqID";
            public static readonly string MarketIndicator = "MarketIndicator";
            public static readonly string SecurityID = "SecurityID";
            public static readonly string SecurityType = "SecurityType";
            public static readonly string SettlType = "SettlType";
            public static readonly string Side = "Side";
            public static readonly string OrderQty = "OrderQty";
            public static readonly string OrderQty2 = "OrderQty2";
            public static readonly string ClearingMethod = "ClearingMethod";
            public static readonly string ValidUntilTime = "ValidUntilTime";
            public static readonly string PartyID = "PartyID";
            public static readonly string PartySubID = "PartySubID";
            public static readonly string MarkerID = "MarkerID";
            public static readonly string Info1 = "Info1";
            public static readonly string Info2 = "Info2";
            public static readonly string Info3 = "Info3";
            public static readonly string Info4 = "Info4";
            public static readonly string Info5 = "Info5";
            public static readonly string MDReqRejReason = "MDReqRejReason";
            public static readonly string RejectText = "RejectText";
            public static readonly string Text = "Text";
            public static readonly string NoMDEntries = "NoMDEntries";
            public static readonly string QuoteTransType = "QuoteTransType";
            public static readonly string QuoteStatus = "QuoteStatus";
            public static readonly string MDEntryType = "MDEntryType";
            public static readonly string MDPriceLevel = "MDPriceLevel";
            public static readonly string MDEntryID = "MDEntryID";
            public static readonly string MDEntryDate = "MDEntryDate";
            public static readonly string MDEntryTime = "MDEntryTime";
            public static readonly string MDEntryPx = "MDEntryPx";
            public static readonly string Yield = "Yield";
            public static readonly string DirtyPrice = "DirtyPrice";
            public static readonly string UnMatchQty = "UnMatchQty";
            public static readonly string MDEntrySize = "MDEntrySize";
            public static readonly string DeliveryType = "DeliveryType";
            public static readonly string PartyName = "PartyName";
            public static readonly string PartySubName = "PartySubName";
            public static readonly string TransactTime = "TransactTime";
            public static readonly string OrdType = "OrdType";
            public static readonly string ClOrdID = "ClOrdID";
            public static readonly string Price = "Price";
            public static readonly string OrigClOrdID = "OrigClOrdID";
            public static readonly string Price2 = "Price2";
            public static readonly string OrderID = "OrderID";
            public static readonly string CxlRejReason = "CxlRejReason";
            public static readonly string OrdStatus = "OrdStatus";
            public static readonly string OrdStatusReqID = "OrdStatusReqID";
            public static readonly string OrdRejReason = "OrdRejReason";
            public static readonly string YieldType = "YieldType";
            public static readonly string LeavesQty = "LeavesQty";
            public static readonly string SettlDate = "SettlDate";
            public static readonly string TradeDate = "TradeDate";
            public static readonly string ExecID = "ExecID";
            public static readonly string ExecType = "ExecType";
            public static readonly string PriceType = "PriceType";
            public static readonly string LastPx = "LastPx";
            public static readonly string LastQty = "LastQty";
            public static readonly string TimeInForce = "TimeInForce";
            public static readonly string Symbol = "Symbol";
            public static readonly string AvgPx = "AvgPx";
            public static readonly string CumQty = "CumQty";
            public static readonly string RefOrderID = "RefOrderID";
            public static readonly string QuoteID = "QuoteID";
            public static readonly string QuoteType = "QuoteType";
            public static readonly string QuoteStatusDesc = "QuoteStatusDesc";
            public static readonly string TradSesStartTime = "TradSesStartTime";
            public static readonly string TradSesOpenTime = "TradSesOpenTime";
            public static readonly string TradSesCloseTime = "TradSesCloseTime";
            public static readonly string TradSesEndTime = "TradSesEndTime";
            public static readonly string SettlCurrency = "SettlCurrency";
            public static readonly string SettlCurrFxRate = "SettlCurrFxRate";
            public static readonly string AccruedInterestAmt = "AccruedInterestAmt";
            public static readonly string TradeVolume = "TradeVolume";
            public static readonly string AllocID = "AllocID";
            public static readonly string CounterPartyID = "CounterPartyID";
            public static readonly string CounterPartyName = "CounterPartyName";
            public static readonly string CounterPartySubID = "CounterPartySubID";
            public static readonly string CounterPartySubName = "CounterPartySubName";
            public static readonly string AlgoName = "AlgoName";
            public static readonly string InternalFlag = "InternalFlag";
            public static readonly string BargainFlag = "BargainFlag";
            public static readonly string OCOFlag = "OCOFlag";
            public static readonly string Terminal = "Terminal";
            public static readonly string UserName = "UserName";
            public static readonly string Yield2 = "Yield2";
            public static readonly string LeavesQty2 = "LeavesQty2";
            public static readonly string NetPrice = "NetPrice";
            public static readonly string NetPrice2 = "NetPrice2";
            public static readonly string StrategyOrderId = "StrategyOrderId";
            public static readonly string BizType = "BizType";
            public static readonly string CalculateRequestID = "CalculateRequestID";
            public static readonly string CalculateRequestType = "CalculateRequestType";
            public static readonly string CalculateReportID = "CalculateReportID";
            public static readonly string OpenQty = "OpenQty";
            public static readonly string FirmFlag = "FirmFlag";
            public static readonly string UserID = "UserID";
            public static readonly string WholeFlag = "WholeFlag";
            public static readonly string QuantID = "QuantID";
            public static readonly string FirmToNonTime = "FirmToNonTime";
            public static readonly string VenuesRdmNum = "VenuesRdmNum";
            public static readonly string SelfSelectVenues = "SelfSelectVenues";
            public static readonly string VenuesSelectF = "VenuesSelectF";
            public static readonly string BlockNum = "BlockNum";
            public static readonly string CounterPartyOrdID = "CounterPartyOrdID";
            public static readonly string DealType = "DealType";
            public static readonly string CCDCValuation = "CCDCValuation";
            public static readonly string CrossVenues = "CrossVenues";
            public static readonly string RiskManualCk = "RiskManualCk";
            public static readonly string InstitutionID = "InstitutionID";
            public static readonly string InstitutionName = "InstitutionName";
            public static readonly string TraderID = "TraderID";
            public static readonly string TraderName = "TraderName";
            public static readonly string ShowTrader = "ShowTrader";
            public static readonly string RoleName = "RoleName";
            public static readonly string UpdateTime = "UpdateTime";
            public static readonly string UpdateUser = "UpdateUser";
            public static readonly string SynchFlag = "SynchFlag";
            public static readonly string BizTypeID = "BizTypeID";
            public static readonly string SalesAgentFlag = "SalesAgentFlag";
            public static readonly string RoleID = "RoleID";
            public static readonly string OrigOrdPrice = "OrigOrdPrice";
            public static readonly string MacID = "MacID";
            public static readonly string PublishIP = "PublishIP";
            public static readonly string InsideIP = "InsideIP";
            public static readonly string HardDiskID = "HardDiskID";
            public static readonly string CounterpartySalesAgentFlag = "CounterpartySalesAgentFlag";
            public static readonly string BrokerExecID = "BrokerExecID";
            public static readonly string AppointedID = "AppointedID";
            public static readonly string InitiatorID = "InitiatorID";
            public static readonly string InitiatorSubID = "InitiatorSubID";
            public static readonly string CounterPartyFullName = "CounterPartyFullName";
            public static readonly string OrigSettlType = "OrigSettlType";
            public static readonly string IocTimeOut = "IocTimeOut";
            public static readonly string IocPrice = "IocPrice";
            public static readonly string IocVenueTypeGW = "IocVenueTypeGW";
            public static readonly string IocVenues = "IocVenues";
            public static readonly string PartyOrderSource = "PartyOrderSource";
            public static readonly string CounterPartyOrderSource = "CounterPartyOrderSource";
            public static readonly string RoleType = "RoleType";
            public static readonly string TransType = "TransType";
            public static readonly string UpdateFiled = "UpdateFiled";
            public static readonly string BridgecounterPartyFullName = "BridgecounterPartyFullName";
            public static readonly string HandlInst = "HandlInst";
            public static readonly string RecordNum = "RecordNum";
            public static readonly string SendingTime = "SendingTime";
            public static readonly string LocalTime = "LocalTime";
            public static readonly string StrategyOrderVenues = "StrategyOrderVenues";
            public static readonly string CounterPartyVenues = "CounterPartyVenues";
            public static readonly string ParentFlag = "ParentFlag";
            public static readonly string DcsexecID = "DcsexecID";
            public static readonly string ParentFlagQty = "ParentFlagQty";
            public static readonly string ActionFlag = "ActionFlag";
            public static readonly string ExecTypeReason = "ExecTypeReason";
            public static readonly string TradeTime = "TradeTime";
            public static readonly string TradeMethod = "TradeMethod";
            public static readonly string Info6 = "Info6";
            public static readonly string Info7 = "Info7";
            public static readonly string Info8 = "Info8";
            public static readonly string Info9 = "Info9";
            public static readonly string Info10 = "Info10";
            public static readonly string B1 = "B1";
            public static readonly string A1 = "A1";
            public static readonly string B0 = "B0";
            public static readonly string B2 = "B2";
            public static readonly string M5b1 = "M5b1";
            public static readonly string M5a1 = "M5a1";
            public static readonly string A2 = "A2";
            public static readonly string A0 = "A0";
            public static readonly string Vs = "Vs";
            public static readonly string TrigEvent = "TrigEvent";
            public static readonly string CancEvent = "CancEvent";
            public static readonly string InValidFlag = "InValidFlag";
            public static readonly string EREventName = "EREventName";
            public static readonly string CancEventName = "CancEventName";
            public static readonly string StandardHeader = "StandardHeader";
            public static readonly string StandardTrailer = "StandardTrailer";
        }
    }
}
