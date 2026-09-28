using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix
{
	/// <summary>
	/// MessageDictionary
	/// </summary>
	/// <remarks>
	/// 2024.05.09: 创建. 模板插件(V1.3.7781.22279(build:20210421)) 模板版本(v1.0.1.0)<br/>
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
				public static readonly string SSE = "SSE";
			}

			public static class Venues
			{
				public static readonly string TPSC = "TPSC";
				public static readonly string CNEX = "CNEX";
				public static readonly string CCTB = "CCTB";
				public static readonly string PTCN = "PTCN";
				public static readonly string CTTJ = "CTTJ";
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
				public static readonly string ODM = "ODM";
				public static readonly string QDMESP = "QDMESP";
				public static readonly string QUANT = "QUANT";
			}

			public static class ParentFlag
			{
				public static readonly string Y = "Y";
				public static readonly string N = "N";
			}

			public static class TradeMethod
			{
				public static readonly string SendNegotiate = "1";
				public static readonly string SendRFQ = "4";
				public static readonly string ReceivedNegotiate = "30";
				public static readonly string ReceivedRFQ = "50";
				public static readonly string Unknown = "60";
			}

			public static class ExecType2
			{
				public static readonly string Trade = "F";
				public static readonly string Rejected = "8";
				public static readonly string Canceled = "4";
			}

			public static class NetLatencyStatus
			{
				public static readonly string Normal = "1";
				public static readonly string Latency = "0";
			}

			public static class MassCancelRequestType
			{
				public static readonly string CANCEL_ORDERS_FOR_A_SECURITY = "1";
				public static readonly string CANCEL_ORDERS_FOR_AN_UNDERLYING_SECURITY = "2";
				public static readonly string CANCEL_ORDERS_FOR_A_PRODUCT = "3";
				public static readonly string CANCEL_ORDERS_FOR_A_CFICODE = "4";
				public static readonly string CANCEL_ORDERS_FOR_A_SECURITYTYPE = "5";
				public static readonly string CANCEL_ORDERS_FOR_A_TRADING_SESSION = "6";
				public static readonly string CANCEL_ALL_ORDERS = "7";
				public static readonly string CANCEL_STRATEGY_ORDERS = "8";
				public static readonly string CANCEL_MANUAL_ORDERS = "9";
			}

			public static class MassCancelResponse
			{
				public static readonly string CANCEL_REQUEST_REJECTED = "0";
				public static readonly string CANCEL_ORDERS_FOR_A_SECURITY = "1";
				public static readonly string CANCEL_ORDERS_FOR_AN_UNDERLYING_SECURITY = "2";
				public static readonly string CANCEL_ORDERS_FOR_A_PRODUCT = "3";
				public static readonly string CANCEL_ORDERS_FOR_A_CFICODE = "4";
				public static readonly string CANCEL_ORDERS_FOR_A_SECURITYTYPE = "5";
				public static readonly string CANCEL_ORDERS_FOR_A_TRADING_SESSION = "6";
				public static readonly string CANCEL_ALL_ORDERS = "7";
			}

			public static class MassCancelRejectReason
			{
				public static readonly string MASS_CANCEL_NOT_SUPPORTED = "0";
				public static readonly string INVALID_OR_UNKNOWN_SECURITY = "1";
				public static readonly string INVALID_OR_UNKNOWN_UNDERLYING = "2";
				public static readonly string INVALID_OR_UNKNOWN_PRODUCT = "3";
				public static readonly string INVALID_OR_UNKNOWN_CFICODE = "4";
				public static readonly string INVALID_OR_UNKNOWN_SECURITY_TYPE = "5";
				public static readonly string INVALID_OR_UNKNOWN_TRADING_SESSION = "6";
				public static readonly string OTHER = "99";
			}

			public static class QuoteRespType
			{
				public static readonly string Accept = "1";
				public static readonly string Reject = "7";
			}

			public static class QuoteMethod
			{
				public static readonly string REQUEST = "1";
				public static readonly string RESPONSE = "2";
			}

			public static class EntryType
			{
				public static readonly string AUTO = "0";
				public static readonly string MANUAL = "1";
			}

			public static class Option
			{
				public static readonly string ADD = "0";
				public static readonly string UPDATE = "1";
				public static readonly string DELETE = "2";
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
			public static readonly string Rejecter = "Rejecter";
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
			public static readonly string MaxFloor = "MaxFloor";
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
            public static readonly string FullPrice = "FullPrice";
            public static readonly string FullPrice2 = "FullPrice2";
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
			public static readonly string LastPx2 = "LastPx2";
			public static readonly string LastQty2 = "LastQty2";
			public static readonly string CumQty2 = "CumQty2";
			public static readonly string OrdStatus2 = "OrdStatus2";
			public static readonly string AvgPx2 = "AvgPx2";
			public static readonly string ExecType2 = "ExecType2";
			public static readonly string BookID = "BookID";
			public static readonly string NetLatency = "NetLatency";
			public static readonly string NetLatencyStatus = "NetLatencyStatus";
			public static readonly string ExerciseFlag = "ExerciseFlag";
			public static readonly string MassCancelRequestType = "MassCancelRequestType";
			public static readonly string MassCancelResponse = "MassCancelResponse";
			public static readonly string MassCancelRejectReason = "MassCancelRejectReason";
			public static readonly string TotalAffectedOrders = "TotalAffectedOrders";
			public static readonly string AffectedOrdGrp = "AffectedOrdGrp";
			public static readonly string ConfirmID = "ConfirmID";
			public static readonly string TradeReportID = "TradeReportID";
			public static readonly string TradeRequestID = "TradeRequestID";
			public static readonly string TradeID = "TradeID";
			public static readonly string SecondaryTradeID = "SecondaryTradeID";
			public static readonly string TradeRequestType = "TradeRequestType";
			public static readonly string TrdType = "TrdType";
			public static readonly string TrdSubType = "TrdSubType";
			public static readonly string TrdMatchID = "TrdMatchID";
			public static readonly string TradeRptStat = "TradeRptStat";
			public static readonly string MaturityDate = "MaturityDate";
			public static readonly string SettlCurrAmt = "SettlCurrAmt";
			public static readonly string TradeMatchTimestamp = "TradeMatchTimestamp";
			public static readonly string MatchStatus = "MatchStatus";
			public static readonly string NoPayments = "NoPayments";
			public static readonly string PaymentType = "PaymentType";
			public static readonly string PaymentFixedRate = "PaymentFixedRate";
			public static readonly string PaymentAmount = "PaymentAmount";
			public static readonly string PaymentUnitOfMeasure = "PaymentUnitOfMeasure";
			public static readonly string NoPartyIDs = "NoPartyIDs";
			public static readonly string PartyIDSource = "PartyIDSource";
			public static readonly string PartyRole = "PartyRole";
			public static readonly string NoPartySubIDs = "NoPartySubIDs";
			public static readonly string PartySubIDType = "PartySubIDType";
			public static readonly string NoStipulations = "NoStipulations";
			public static readonly string StipulationType = "StipulationType";
			public static readonly string StipulationValue = "StipulationValue";
			public static readonly string NoAllocs = "NoAllocs";
			public static readonly string AllocType = "AllocType";
			public static readonly string IndividualAllocID = "IndividualAllocID";
			public static readonly string AllocAccount = "AllocAccount";
			public static readonly string AllocText = "AllocText";
			public static readonly string CleanPrice = "CleanPrice";
			public static readonly string OnBehalfOfCompID = "OnBehalfOfCompID";
			public static readonly string TradeRequestResult = "TradeRequestResult";
			public static readonly string TradeRequestStatus = "TradeRequestStatus";
			public static readonly string PartyCode = "PartyCode";
			public static readonly string CounterPartyCode = "CounterPartyCode";
			public static readonly string SSDetect = "SSDetect";
			public static readonly string OpenPrice = "OpenPrice";
			public static readonly string HighPrice = "HighPrice";
			public static readonly string LowPrice = "LowPrice";
			public static readonly string PreClosePrice = "PreClosePrice";
			public static readonly string SettlePrice = "SettlePrice";
			public static readonly string PreSettlePrice = "PreSettlePrice";
			public static readonly string OpenInterest = "OpenInterest";
			public static readonly string PreOpenInterest = "PreOpenInterest";
			public static readonly string Turnover = "Turnover";
			public static readonly string Volume = "Volume";
			public static readonly string UpperLimitPrice = "UpperLimitPrice";
			public static readonly string LowerLimitPrice = "LowerLimitPrice";
			public static readonly string LastPrice = "LastPrice";
			public static readonly string TradeNumber = "TradeNumber";
			public static readonly string OrderSource = "OrderSource";
			public static readonly string ReferenceInfo1 = "ReferenceInfo1";
			public static readonly string ReferenceInfo2 = "ReferenceInfo2";
			public static readonly string ReferenceInfo3 = "ReferenceInfo3";
			public static readonly string ReferenceInfo4 = "ReferenceInfo4";
			public static readonly string ReferenceInfo5 = "ReferenceInfo5";
			public static readonly string Name = "Name";
			public static readonly string PartySubID2 = "PartySubID2";
			public static readonly string IntentionID = "IntentionID";
			public static readonly string OffsetFlag = "OffsetFlag";
			public static readonly string HedgeFlag = "HedgeFlag";
			public static readonly string TradingPhaseCode = "TradingPhaseCode";
			public static readonly string TradingType = "TradingType";
			public static readonly string CalFlag = "CalFlag";
			public static readonly string CCDCValuaDate = "CCDCValuaDate";
			public static readonly string CCDCValuaType = "CCDCValuaType";
			public static readonly string LastWorkDay = "LastWorkDay";
			public static readonly string QuoteReqID = "QuoteReqID";
			public static readonly string QuoteRespType = "QuoteRespType";
			public static readonly string QuoteRespID = "QuoteRespID";
			public static readonly string QuoteRequestType = "QuoteRequestType";
			public static readonly string QuoteStatusReqID = "QuoteStatusReqID";
			public static readonly string QuoteRejectReason = "QuoteRejectReason";
			public static readonly string Comments = "Comments";
			public static readonly string AccountID = "AccountID";
			public static readonly string AccountName = "AccountName";
			public static readonly string BBVol = "BBVol";
			public static readonly string BBPx = "BBPx";
			public static readonly string BOVol = "BOVol";
			public static readonly string BOPx = "BOPx";
			public static readonly string Tag1 = "Tag1";
			public static readonly string PriceCondition = "PriceCondition";
			public static readonly string DqOrders = "DqOrders";
			public static readonly string PendingCancelTime = "PendingCancelTime";
			public static readonly string CounterAccountID = "CounterAccountID";
			public static readonly string CounterAccountName = "CounterAccountName";
			public static readonly string BBTime = "BBTime";
			public static readonly string BOTime = "BOTime";
			public static readonly string OrgType = "OrgType";
			public static readonly string VipCode = "VipCode";
			public static readonly string IfSubBond = "IfSubBond";
			public static readonly string Version = "Version";
			public static readonly string DqInfo = "DqInfo";
			public static readonly string RelateStatus = "RelateStatus";
			public static readonly string QuoteMethod = "QuoteMethod";
			public static readonly string EntryType = "EntryType";
			public static readonly string Option = "Option";
			public static readonly string DqUpdate = "DqUpdate";
			public static readonly string CreateBy = "CreateBy";
			public static readonly string MakerPrice = "MakerPrice";
			public static readonly string MakerOrderQty = "MakerOrderQty";
			public static readonly string MacaulayDuration = "MacaulayDuration";
			public static readonly string ModifiedDuration = "ModifiedDuration";
			public static readonly string Pvbp = "Pvbp";
			public static readonly string Convexity = "Convexity";
			public static readonly string Sign = "Sign";
			public static readonly string Sign1 = "Sign1";
			public static readonly string Sign2 = "Sign2";
			public static readonly string Sign3 = "Sign3";
			public static readonly string NoUnderlyings = "NoUnderlyings";
			public static readonly string UnderlyingSymbol = "UnderlyingSymbol";
			public static readonly string UnderlyingSecurityID = "UnderlyingSecurityID";
			public static readonly string UnderlyingQty = "UnderlyingQty";
			public static readonly string NoUnderlyingStips = "NoUnderlyingStips";
			public static readonly string UnderlyingStipType = "UnderlyingStipType";
			public static readonly string UnderlyingStipValue = "UnderlyingStipValue";
			public static readonly string NoFloorDetails = "NoFloorDetails";
			public static readonly string FloorDetailID = "FloorDetailID";
			public static readonly string FloorDetailPrice = "FloorDetailPrice";
			public static readonly string NoFloorDetailSubIDs = "NoFloorDetailSubIDs";
			public static readonly string FloorDetailSubID = "FloorDetailSubID";
			public static readonly string FloorDetailSubIDType = "FloorDetailSubIDType";
			public static readonly string RepoMethod = "RepoMethod";
			public static readonly string TradeRecordID = "TradeRecordID";
			public static readonly string TradeLimitDays = "TradeLimitDays";
			public static readonly string NoCrossCustodians = "NoCrossCustodians";
			public static readonly string CrossCustodianInstitutionName = "CrossCustodianInstitutionName";
			public static readonly string CrossCustodianTradeAmt = "CrossCustodianTradeAmt";
			public static readonly string DeliveryType2 = "DeliveryType2";
			public static readonly string Channel = "Channel";
			public static readonly string QuoteTime = "QuoteTime";
			public static readonly string Remark = "Remark";
			public static readonly string UserReference1 = "UserReference1";
			public static readonly string UserReference2 = "UserReference2";
			public static readonly string UserReference3 = "UserReference3";
			public static readonly string UserReference4 = "UserReference4";
			public static readonly string UserReference5 = "UserReference5";
			public static readonly string UserReference6 = "UserReference6";
			public static readonly string NegotiationCount = "NegotiationCount";
			public static readonly string TradeCashAmt = "TradeCashAmt";
			public static readonly string QueryRequestID = "QueryRequestID";
			public static readonly string PledgeSubmissionCode = "PledgeSubmissionCode";
			public static readonly string BenchmarkInterestRate = "BenchmarkInterestRate";
			public static readonly string Spread = "Spread";
			public static readonly string ContingencyIndicator = "ContingencyIndicator";
			public static readonly string SeatNumber = "SeatNumber";
			public static readonly string PartyCode6 = "PartyCode6";
			public static readonly string CounterPartyCode6 = "CounterPartyCode6";
			public static readonly string CommandType = "CommandType";
			public static readonly string TradeReportTransType = "TradeReportTransType";
			public static readonly string NoSides = "NoSides";
			public static readonly string OnLine = "OnLine";
			public static readonly string FanDian = "FanDian";
			public static readonly string FanDianFlag = "FanDianFlag";
			public static readonly string PartDealFlag = "PartDealFlag";
			public static readonly string ExchangeSignFlag = "ExchangeSignFlag";
			public static readonly string OrderSN = "OrderSN";
			public static readonly string CounterPartyTraderId = "CounterPartyTraderId";
			public static readonly string CounterPartyTraderName = "CounterPartyTraderName";
			public static readonly string CounterPartyInstitutionId = "CounterPartyInstitutionId";
			public static readonly string CounterPartyInstitutionName = "CounterPartyInstitutionName";
			public static readonly string CompanyId = "CompanyId";
			public static readonly string MatchQTY = "MatchQTY";
			public static readonly string QuoteInternalFlag = "QuoteInternalFlag";
			public static readonly string TimeToMaturity = "TimeToMaturity";
			public static readonly string BrokerBConfirmQty = "BrokerBConfirmQty";
			public static readonly string SendQQMsgFlag = "SendQQMsgFlag";
			public static readonly string SendQQMsgTem = "SendQQMsgTem";
			public static readonly string Sequence = "Sequence";
			public static readonly string BridgeFlag = "BridgeFlag";
			public static readonly string SendTo = "SendTo";
			public static readonly string SendTo2 = "SendTo2";
			public static readonly string CloseBy = "CloseBy";
			public static readonly string BrokerUnread = "BrokerUnread";
			public static readonly string CloseById = "CloseById";
			public static readonly string DCSFlag = "DCSFlag";
			public static readonly string QueryFlag = "QueryFlag";
			public static readonly string CloseById2 = "CloseById2";
			public static readonly string SettlType2 = "SettlType2";
			public static readonly string TradeDate2 = "TradeDate2";
			public static readonly string SettlDate2 = "SettlDate2";
			public static readonly string BridgeInstitutionName = "BridgeInstitutionName";
			public static readonly string BridgeInstitutionId = "BridgeInstitutionId";
			public static readonly string BridgeTraderId = "BridgeTraderId";
			public static readonly string BridgeTraderName = "BridgeTraderName";
			public static readonly string BridgeChannel = "BridgeChannel";
			public static readonly string BridgeSendTo = "BridgeSendTo";
			public static readonly string BridgeFee = "BridgeFee";
			public static readonly string BridgeRemark = "BridgeRemark";
			public static readonly string BridgeType = "BridgeType";
			public static readonly string BridgeInstitutionName2 = "BridgeInstitutionName2";
			public static readonly string BridgeInstitutionId2 = "BridgeInstitutionId2";
			public static readonly string BridgeTraderId2 = "BridgeTraderId2";
			public static readonly string BridgeTraderName2 = "BridgeTraderName2";
			public static readonly string BridgeChannel2 = "BridgeChannel2";
			public static readonly string BridgeSendTo2 = "BridgeSendTo2";
			public static readonly string BridgeFee2 = "BridgeFee2";
			public static readonly string BridgeRemark2 = "BridgeRemark2";
			public static readonly string BridgeType2 = "BridgeType2";
			public static readonly string BridgeStatus = "BridgeStatus";
			public static readonly string BridgeCloseById = "BridgeCloseById";
			public static readonly string BridgeCreateTime = "BridgeCreateTime";
			public static readonly string BridgeUpdateTime = "BridgeUpdateTime";
			public static readonly string BridgeCreateTime2 = "BridgeCreateTime2";
			public static readonly string BridgeUpdateTime2 = "BridgeUpdateTime2";
			public static readonly string BridgeRemarkShow = "BridgeRemarkShow";
			public static readonly string BridgeRemarkShow2 = "BridgeRemarkShow2";
			public static readonly string BorderId = "BorderId";
			public static readonly string BorderId2 = "BorderId2";
			public static readonly string SyncDCSFlag = "SyncDCSFlag";
			public static readonly string SyncDCSTime = "SyncDCSTime";
			public static readonly string DoubleBridgeChannel = "DoubleBridgeChannel";
			public static readonly string DoubleBridgeSendTo = "DoubleBridgeSendTo";
			public static readonly string DoubleBridgeFee = "DoubleBridgeFee";
			public static readonly string DoubleBridgeRemark = "DoubleBridgeRemark";
			public static readonly string DoubleBridgeOffSettltype = "DoubleBridgeOffSettltype";
			public static readonly string DoubleBridgeBidSettltype = "DoubleBridgeBidSettltype";
			public static readonly string BondSubtype = "BondSubtype";
			public static readonly string OnlineA = "OnlineA";
			public static readonly string OnlineB = "OnlineB";
			public static readonly string AutoConfirmA = "AutoConfirmA";
			public static readonly string AutoConfirmB = "AutoConfirmB";
			public static readonly string BrokerageA = "BrokerageA";
			public static readonly string BrokerageB = "BrokerageB";
			public static readonly string SyncQmFlag = "SyncQmFlag";
			public static readonly string SyncQmTime = "SyncQmTime";
			public static readonly string DoubleBridgeSettleSide = "DoubleBridgeSettleSide";
			public static readonly string AgreementNo = "AgreementNo";
			public static readonly string ForBidRefreshFlag = "ForBidRefreshFlag";
			public static readonly string SendSorFlagA = "SendSorFlagA";
			public static readonly string SendSorFlagB = "SendSorFlagB";
			public static readonly string IssueState = "IssueState";
			public static readonly string IssueDir = "IssueDir";
			public static readonly string IssueChannel = "IssueChannel";
			public static readonly string IssueInstId = "IssueInstId";
			public static readonly string IssueInstName = "IssueInstName";
			public static readonly string IssueInstNameShort = "IssueInstNameShort";
			public static readonly string IssueTraderId = "IssueTraderId";
			public static readonly string IssueTraderName = "IssueTraderName";
			public static readonly string IssueRemark = "IssueRemark";
			public static readonly string ProxyInstId = "ProxyInstId";
			public static readonly string ProxyFee = "ProxyFee";
			public static readonly string ProxyFlag = "ProxyFlag";
			public static readonly string SingleQqmsgSendFlagA = "SingleQqmsgSendFlagA";
			public static readonly string SingleQqmsgSendFlagB = "SingleQqmsgSendFlagB";
			public static readonly string OfrBridgeDir = "OfrBridgeDir";
			public static readonly string BidBridgeDir = "BidBridgeDir";
			public static readonly string DetailMark = "DetailMark";
			public static readonly string AssociatedFlag = "AssociatedFlag";
			public static readonly string OFRSpecialBridgeFee = "OFRSpecialBridgeFee";
			public static readonly string BIDSpecialBridgeFee = "BIDSpecialBridgeFee";
			public static readonly string BridgeInstitutionFullName = "BridgeInstitutionFullName";
			public static readonly string BridgeInstitutionFullName2 = "BridgeInstitutionFullName2";
			public static readonly string ReadFlag = "ReadFlag";
			public static readonly string SendMsgFlag = "SendMsgFlag";
			public static readonly string ID = "ID";
			public static readonly string InstitutionShowName = "InstitutionShowName";
			public static readonly string CounterPartyInstitutionShowName = "CounterPartyInstitutionShowName";
			public static readonly string BridgeInstitutionShowName = "BridgeInstitutionShowName";
			public static readonly string BridgeInstitutionShowName2 = "BridgeInstitutionShowName2";
			public static readonly string AddBridgeWay = "AddBridgeWay";
			public static readonly string BridgeStatus2 = "BridgeStatus2";
			public static readonly string InstExecOrderReqListExt = "InstExecOrderReqListExt";
			public static readonly string OfrDir = "OfrDir";
			public static readonly string BidDir = "BidDir";
			public static readonly string OfrDir2 = "OfrDir2";
			public static readonly string BidDir2 = "BidDir2";
			public static readonly string DelFlag = "DelFlag";
			public static readonly string ParamCloseById = "ParamCloseById";
			public static readonly string BridgeOrder = "BridgeOrder";
			public static readonly string BridgeOrder2 = "BridgeOrder2";
			public static readonly string IssueDesc = "IssueDesc";
			public static readonly string IssueShowType = "IssueShowType";
			public static readonly string FeeTraderid = "FeeTraderid";
			public static readonly string FeeTraderid2 = "FeeTraderid2";
			public static readonly string FeeTradername = "FeeTradername";
			public static readonly string FeeTradername2 = "FeeTradername2";
			public static readonly string FeeInstitutionid = "FeeInstitutionid";
			public static readonly string FeeInstitutionid2 = "FeeInstitutionid2";
			public static readonly string FeeInstitutionname = "FeeInstitutionname";
			public static readonly string FeeInstitutionname2 = "FeeInstitutionname2";
			public static readonly string Contacttype = "Contacttype";
			public static readonly string Contacttype2 = "Contacttype2";
			public static readonly string OperateBorderID = "OperateBorderID";
			public static readonly string ListedMarket = "ListedMarket";
			public static readonly string CombinedOrderRemark = "CombinedOrderRemark";
			public static readonly string CombinedOrderRemark2 = "CombinedOrderRemark2";
			public static readonly string DirtyPrice2 = "DirtyPrice2";
			public static readonly string DirtyPrice3 = "DirtyPrice3";
			public static readonly string NetPrice3 = "NetPrice3";
			public static readonly string FidSettlAmount = "FidSettlAmount";
			public static readonly string FidSettlAmount2 = "FidSettlAmount2";
			public static readonly string Doublebridgedir = "Doublebridgedir";
			public static readonly string DayOffStr = "DayOffStr";
			public static readonly string ValuationNetPrice = "ValuationNetPrice";
			public static readonly string ValuationNetPrice2 = "ValuationNetPrice2";
			public static readonly string CCDCValuation2 = "CCDCValuation2";
			public static readonly string MarkDetailUsers = "MarkDetailUsers";
			public static readonly string IsConfirmOperation = "IsConfirmOperation";
			public static readonly string YieldToExecution = "YieldToExecution";
			public static readonly string YieldToExecution2 = "YieldToExecution2";
			public static readonly string BusinessType = "BusinessType";
			public static readonly string Divided = "Divided";
			public static readonly string StandardHeader = "StandardHeader";
			public static readonly string StandardTrailer = "StandardTrailer";
			public static readonly string Parties = "Parties";
			public static readonly string Payments = "Payments";
			public static readonly string Stipulations = "Stipulations";
			public static readonly string Allocs = "Allocs";
			public static readonly string Underlyings = "Underlyings";
			public static readonly string UnderlyingStips = "UnderlyingStips";
			public static readonly string FloorDetails = "FloorDetails";
			public static readonly string FloorDetailSubIDs = "FloorDetailSubIDs";
			public static readonly string RiskReTryFlag = "RiskReTryFlag";
			public static readonly string BondType = "BondType";
		}
	}
}


