using STC.dps.plugin.Model.Fix.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix.Message
{
	/// <summary>
	/// Message
	/// </summary>
	/// <remarks>
	/// 2021.07.14: 创建. 模板插件(V1.3.7781.22279(build:20210421)) 模板版本(v1.0.1.1)<br/>
	/// </remarks>
	public class ExecutionReport : FixMessage
	{
		public ExecutionReport()
		{
			StandardHeader.MsgType = "8";
		}
		public string VenueTypeGW
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenueTypeGW)); }
			set { this.Put(MessageDictionary.Fields.VenueTypeGW, value); }
		}
		public string Venues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Venues)); }
			set { this.Put(MessageDictionary.Fields.Venues, value); }
		}
		public string AlgoName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AlgoName)); }
			set { this.Put(MessageDictionary.Fields.AlgoName, value); }
		}
		public string StrategyOrderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.StrategyOrderId)); }
			set { this.Put(MessageDictionary.Fields.StrategyOrderId, value); }
		}
		public string StrategyOrderVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.StrategyOrderVenues)); }
			set { this.Put(MessageDictionary.Fields.StrategyOrderVenues, value); }
		}
		public string MarketIndicator
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
			set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
		}
		public string Terminal
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Terminal)); }
			set { this.Put(MessageDictionary.Fields.Terminal, value); }
		}
		public string OrdStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdStatus)); }
			set { this.Put(MessageDictionary.Fields.OrdStatus, value); }
		}
		public string ClOrdID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
			set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
		}
		public string OrderID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderID)); }
			set { this.Put(MessageDictionary.Fields.OrderID, value); }
		}
		public string QuoteID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteID)); }
			set { this.Put(MessageDictionary.Fields.QuoteID, value); }
		}
		public string QuantID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuantID)); }
			set { this.Put(MessageDictionary.Fields.QuantID, value); }
		}
		public string DcsexecID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DcsexecID)); }
			set { this.Put(MessageDictionary.Fields.DcsexecID, value); }
		}
		public string QuoteType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteType)); }
			set { this.Put(MessageDictionary.Fields.QuoteType, value); }
		}
		public string MDReqID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
			set { this.Put(MessageDictionary.Fields.MDReqID, value); }
		}
		public string QuoteTransType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteTransType)); }
			set { this.Put(MessageDictionary.Fields.QuoteTransType, value); }
		}
		public string QuoteStatusDesc
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteStatusDesc)); }
			set { this.Put(MessageDictionary.Fields.QuoteStatusDesc, value); }
		}
		public string OrdStatusReqID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdStatusReqID)); }
			set { this.Put(MessageDictionary.Fields.OrdStatusReqID, value); }
		}
		public string ExecAckStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecAckStatus)); }
			set { this.Put(MessageDictionary.Fields.ExecAckStatus, value); }
		}
		public string OrdRejReason
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdRejReason)); }
			set { this.Put(MessageDictionary.Fields.OrdRejReason, value); }
		}
		public string RejectText
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RejectText)); }
			set { this.Put(MessageDictionary.Fields.RejectText, value); }
		}
		public string TransactTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TransactTime)); }
			set { this.Put(MessageDictionary.Fields.TransactTime, value); }
		}
		public string SecurityID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SecurityID)); }
			set { this.Put(MessageDictionary.Fields.SecurityID, value); }
		}
		public string Symbol
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Symbol)); }
			set { this.Put(MessageDictionary.Fields.Symbol, value); }
		}
		public string ValidUntilTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ValidUntilTime)); }
			set { this.Put(MessageDictionary.Fields.ValidUntilTime, value); }
		}
		public string FirmToNonTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FirmToNonTime)); }
			set { this.Put(MessageDictionary.Fields.FirmToNonTime, value); }
		}
		public string OrdType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdType)); }
			set { this.Put(MessageDictionary.Fields.OrdType, value); }
		}
		public string TimeInForce
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TimeInForce)); }
			set { this.Put(MessageDictionary.Fields.TimeInForce, value); }
		}
		public string Side
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Side)); }
			set { this.Put(MessageDictionary.Fields.Side, value); }
		}
		public string Price
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Price)); }
			set { this.Put(MessageDictionary.Fields.Price, value); }
		}
		public string Price2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Price2)); }
			set { this.Put(MessageDictionary.Fields.Price2, value); }
		}
		public string NetPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetPrice)); }
			set { this.Put(MessageDictionary.Fields.NetPrice, value); }
		}
		public string NetPrice2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetPrice2)); }
			set { this.Put(MessageDictionary.Fields.NetPrice2, value); }
		}
		public string Yield
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Yield)); }
			set { this.Put(MessageDictionary.Fields.Yield, value); }
		}
		public string Yield2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Yield2)); }
			set { this.Put(MessageDictionary.Fields.Yield2, value); }
		}
		public string OrderQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderQty)); }
			set { this.Put(MessageDictionary.Fields.OrderQty, value); }
		}
		public string OrderQty2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderQty2)); }
			set { this.Put(MessageDictionary.Fields.OrderQty2, value); }
		}
		public string LeavesQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LeavesQty)); }
			set { this.Put(MessageDictionary.Fields.LeavesQty, value); }
		}
		public string LeavesQty2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LeavesQty2)); }
			set { this.Put(MessageDictionary.Fields.LeavesQty2, value); }
		}
		public string CumQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CumQty)); }
			set { this.Put(MessageDictionary.Fields.CumQty, value); }
		}
		public string TradeDate
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeDate)); }
			set { this.Put(MessageDictionary.Fields.TradeDate, value); }
		}
		public string TradeTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeTime)); }
			set { this.Put(MessageDictionary.Fields.TradeTime, value); }
		}
		public string SettlDate
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlDate)); }
			set { this.Put(MessageDictionary.Fields.SettlDate, value); }
		}
		public string SettlType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlType)); }
			set { this.Put(MessageDictionary.Fields.SettlType, value); }
		}
		public string ClearingMethod
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClearingMethod)); }
			set { this.Put(MessageDictionary.Fields.ClearingMethod, value); }
		}
		public string DeliveryType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DeliveryType)); }
			set { this.Put(MessageDictionary.Fields.DeliveryType, value); }
		}
		public string ExecID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecID)); }
			set { this.Put(MessageDictionary.Fields.ExecID, value); }
		}
		public string ExecType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecType)); }
			set { this.Put(MessageDictionary.Fields.ExecType, value); }
		}
		public string LastPx
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastPx)); }
			set { this.Put(MessageDictionary.Fields.LastPx, value); }
		}
		public string LastQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastQty)); }
			set { this.Put(MessageDictionary.Fields.LastQty, value); }
		}
		public string PartyID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyID)); }
			set { this.Put(MessageDictionary.Fields.PartyID, value); }
		}
		public string PartyName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyName)); }
			set { this.Put(MessageDictionary.Fields.PartyName, value); }
		}
		public string PartySubID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubID)); }
			set { this.Put(MessageDictionary.Fields.PartySubID, value); }
		}
		public string PartySubName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubName)); }
			set { this.Put(MessageDictionary.Fields.PartySubName, value); }
		}
		public string CounterPartyID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyID)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyID, value); }
		}
		public string CounterPartyName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyName, value); }
		}
		public string CounterPartyFullName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyFullName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyFullName, value); }
		}
		public string CounterPartySubID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartySubID)); }
			set { this.Put(MessageDictionary.Fields.CounterPartySubID, value); }
		}
		public string CounterPartySubName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartySubName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartySubName, value); }
		}
		public string CounterPartyOrdID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyOrdID)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyOrdID, value); }
		}
		public string DirtyPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DirtyPrice)); }
			set { this.Put(MessageDictionary.Fields.DirtyPrice, value); }
		}
		public string OpenQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OpenQty)); }
			set { this.Put(MessageDictionary.Fields.OpenQty, value); }
		}
		public string UserID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UserID)); }
			set { this.Put(MessageDictionary.Fields.UserID, value); }
		}
		public string UserName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UserName)); }
			set { this.Put(MessageDictionary.Fields.UserName, value); }
		}
		public string BizType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BizType)); }
			set { this.Put(MessageDictionary.Fields.BizType, value); }
		}
		public string BizTypeID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BizTypeID)); }
			set { this.Put(MessageDictionary.Fields.BizTypeID, value); }
		}
		public string AvgPx
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AvgPx)); }
			set { this.Put(MessageDictionary.Fields.AvgPx, value); }
		}
		public string RefOrderID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RefOrderID)); }
			set { this.Put(MessageDictionary.Fields.RefOrderID, value); }
		}
		public string FirmFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FirmFlag)); }
			set { this.Put(MessageDictionary.Fields.FirmFlag, value); }
		}
		public string WholeFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.WholeFlag)); }
			set { this.Put(MessageDictionary.Fields.WholeFlag, value); }
		}
		public string VenuesSelectF
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenuesSelectF)); }
			set { this.Put(MessageDictionary.Fields.VenuesSelectF, value); }
		}
		public string VenuesRdmNum
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenuesRdmNum)); }
			set { this.Put(MessageDictionary.Fields.VenuesRdmNum, value); }
		}
		public string SelfSelectVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SelfSelectVenues)); }
			set { this.Put(MessageDictionary.Fields.SelfSelectVenues, value); }
		}
		public string BlockNum
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BlockNum)); }
			set { this.Put(MessageDictionary.Fields.BlockNum, value); }
		}
		public string DealType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DealType)); }
			set { this.Put(MessageDictionary.Fields.DealType, value); }
		}
		public string CrossVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CrossVenues)); }
			set { this.Put(MessageDictionary.Fields.CrossVenues, value); }
		}
		public string AllocID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AllocID)); }
			set { this.Put(MessageDictionary.Fields.AllocID, value); }
		}
		public string UpdateTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpdateTime)); }
			set { this.Put(MessageDictionary.Fields.UpdateTime, value); }
		}
		public string UpdateUser
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpdateUser)); }
			set { this.Put(MessageDictionary.Fields.UpdateUser, value); }
		}
		public string SynchFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SynchFlag)); }
			set { this.Put(MessageDictionary.Fields.SynchFlag, value); }
		}
		public string OCOFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OCOFlag)); }
			set { this.Put(MessageDictionary.Fields.OCOFlag, value); }
		}
		public string InstitutionID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstitutionID)); }
			set { this.Put(MessageDictionary.Fields.InstitutionID, value); }
		}
		public string InstitutionName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstitutionName)); }
			set { this.Put(MessageDictionary.Fields.InstitutionName, value); }
		}
		public string TraderID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TraderID)); }
			set { this.Put(MessageDictionary.Fields.TraderID, value); }
		}
		public string TraderName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TraderName)); }
			set { this.Put(MessageDictionary.Fields.TraderName, value); }
		}
		public string ShowTrader
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ShowTrader)); }
			set { this.Put(MessageDictionary.Fields.ShowTrader, value); }
		}
		public string RoleName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RoleName)); }
			set { this.Put(MessageDictionary.Fields.RoleName, value); }
		}
		public string RoleID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RoleID)); }
			set { this.Put(MessageDictionary.Fields.RoleID, value); }
		}
		public string SalesAgentFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SalesAgentFlag)); }
			set { this.Put(MessageDictionary.Fields.SalesAgentFlag, value); }
		}
		public string CounterpartySalesAgentFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterpartySalesAgentFlag)); }
			set { this.Put(MessageDictionary.Fields.CounterpartySalesAgentFlag, value); }
		}
		public string OrigOrdPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrigOrdPrice)); }
			set { this.Put(MessageDictionary.Fields.OrigOrdPrice, value); }
		}
		public string MacID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MacID)); }
			set { this.Put(MessageDictionary.Fields.MacID, value); }
		}
		public string PublishIP
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PublishIP)); }
			set { this.Put(MessageDictionary.Fields.PublishIP, value); }
		}
		public string InsideIP
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InsideIP)); }
			set { this.Put(MessageDictionary.Fields.InsideIP, value); }
		}
		public string HardDiskID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HardDiskID)); }
			set { this.Put(MessageDictionary.Fields.HardDiskID, value); }
		}
		public string BrokerExecID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BrokerExecID)); }
			set { this.Put(MessageDictionary.Fields.BrokerExecID, value); }
		}
		public string AppointedID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AppointedID)); }
			set { this.Put(MessageDictionary.Fields.AppointedID, value); }
		}
		public string InitiatorID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InitiatorID)); }
			set { this.Put(MessageDictionary.Fields.InitiatorID, value); }
		}
		public string InitiatorSubID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InitiatorSubID)); }
			set { this.Put(MessageDictionary.Fields.InitiatorSubID, value); }
		}
		public string BridgecounterPartyFullName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgecounterPartyFullName)); }
			set { this.Put(MessageDictionary.Fields.BridgecounterPartyFullName, value); }
		}
		public string UpdateFiled
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpdateFiled)); }
			set { this.Put(MessageDictionary.Fields.UpdateFiled, value); }
		}
		public string TransType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TransType)); }
			set { this.Put(MessageDictionary.Fields.TransType, value); }
		}
		public string RoleType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RoleType)); }
			set { this.Put(MessageDictionary.Fields.RoleType, value); }
		}
		public string PartyOrderSource
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyOrderSource)); }
			set { this.Put(MessageDictionary.Fields.PartyOrderSource, value); }
		}
		public string CounterPartyOrderSource
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyOrderSource)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyOrderSource, value); }
		}
		public string RecordNum
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RecordNum)); }
			set { this.Put(MessageDictionary.Fields.RecordNum, value); }
		}
		public string SendingTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendingTime)); }
			set { this.Put(MessageDictionary.Fields.SendingTime, value); }
		}
		public string IocTimeOut
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IocTimeOut)); }
			set { this.Put(MessageDictionary.Fields.IocTimeOut, value); }
		}
		public string IocPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IocPrice)); }
			set { this.Put(MessageDictionary.Fields.IocPrice, value); }
		}
		public string HandlInst
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HandlInst)); }
			set { this.Put(MessageDictionary.Fields.HandlInst, value); }
		}
		public string IocVenueTypeGW
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IocVenueTypeGW)); }
			set { this.Put(MessageDictionary.Fields.IocVenueTypeGW, value); }
		}
		public string IocVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IocVenues)); }
			set { this.Put(MessageDictionary.Fields.IocVenues, value); }
		}
		public string CounterPartyVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyVenues)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyVenues, value); }
		}
		public string ParentFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ParentFlag)); }
			set { this.Put(MessageDictionary.Fields.ParentFlag, value); }
		}
		public string ParentFlagQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ParentFlagQty)); }
			set { this.Put(MessageDictionary.Fields.ParentFlagQty, value); }
		}
		public string ActionFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ActionFlag)); }
			set { this.Put(MessageDictionary.Fields.ActionFlag, value); }
		}
		public string ExecTypeReason
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecTypeReason)); }
			set { this.Put(MessageDictionary.Fields.ExecTypeReason, value); }
		}
		public string TradeMethod
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeMethod)); }
			set { this.Put(MessageDictionary.Fields.TradeMethod, value); }
		}
		public string SecurityType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SecurityType)); }
			set { this.Put(MessageDictionary.Fields.SecurityType, value); }
		}
		public string A1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.A1)); }
			set { this.Put(MessageDictionary.Fields.A1, value); }
		}
		public string B1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.B1)); }
			set { this.Put(MessageDictionary.Fields.B1, value); }
		}
		public string A0
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.A0)); }
			set { this.Put(MessageDictionary.Fields.A0, value); }
		}
		public string B2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.B2)); }
			set { this.Put(MessageDictionary.Fields.B2, value); }
		}
		public string M5b1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.M5b1)); }
			set { this.Put(MessageDictionary.Fields.M5b1, value); }
		}
		public string M5a1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.M5a1)); }
			set { this.Put(MessageDictionary.Fields.M5a1, value); }
		}
		public string A2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.A2)); }
			set { this.Put(MessageDictionary.Fields.A2, value); }
		}
		public string B0
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.B0)); }
			set { this.Put(MessageDictionary.Fields.B0, value); }
		}
		public string Vs
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Vs)); }
			set { this.Put(MessageDictionary.Fields.Vs, value); }
		}
		public string Info1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info1)); }
			set { this.Put(MessageDictionary.Fields.Info1, value); }
		}
		public string Info2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info2)); }
			set { this.Put(MessageDictionary.Fields.Info2, value); }
		}
		public string Info3
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info3)); }
			set { this.Put(MessageDictionary.Fields.Info3, value); }
		}
		public string Info4
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info4)); }
			set { this.Put(MessageDictionary.Fields.Info4, value); }
		}
		public string Info5
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info5)); }
			set { this.Put(MessageDictionary.Fields.Info5, value); }
		}
		public string Info6
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info6)); }
			set { this.Put(MessageDictionary.Fields.Info6, value); }
		}
		public string Info7
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info7)); }
			set { this.Put(MessageDictionary.Fields.Info7, value); }
		}
		public string Info8
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info8)); }
			set { this.Put(MessageDictionary.Fields.Info8, value); }
		}
		public string Info9
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info9)); }
			set { this.Put(MessageDictionary.Fields.Info9, value); }
		}
		public string Info10
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info10)); }
			set { this.Put(MessageDictionary.Fields.Info10, value); }
		}
		public string TrigEvent
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TrigEvent)); }
			set { this.Put(MessageDictionary.Fields.TrigEvent, value); }
		}
		public string CancEvent
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CancEvent)); }
			set { this.Put(MessageDictionary.Fields.CancEvent, value); }
		}
		public string InValidFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InValidFlag)); }
			set { this.Put(MessageDictionary.Fields.InValidFlag, value); }
		}
		public string EREventName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.EREventName)); }
			set { this.Put(MessageDictionary.Fields.EREventName, value); }
		}
		public string CancEventName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CancEventName)); }
			set { this.Put(MessageDictionary.Fields.CancEventName, value); }
		}

		private object trigEventListener;
		public object getTrigEventListener()
		{
			return this.trigEventListener;
		}
		public void setTrigEventListener(object trigEventListener)
		{
			this.trigEventListener = trigEventListener;
		}

		private object cancEventListener;
		public object getCancEventListener()
		{
			return this.cancEventListener;
		}
		public void setCancEventListener(object cancEventListener)
		{
			this.cancEventListener = cancEventListener;
		}

		public ExecutionReport Clone()
		{
			ExecutionReport t = new ExecutionReport();
			t.StandardHeader = StandardHeader;
			t.StandardTrailer = StandardTrailer;
			t.VenueTypeGW = VenueTypeGW;
			t.Venues = Venues;
			t.AlgoName = AlgoName;
			t.StrategyOrderId = StrategyOrderId;
			t.StrategyOrderVenues = StrategyOrderVenues;
			t.MarketIndicator = MarketIndicator;
			t.Terminal = Terminal;
			t.OrdStatus = OrdStatus;
			t.ClOrdID = ClOrdID;
			t.OrderID = OrderID;
			t.QuoteID = QuoteID;
			t.QuantID = QuantID;
			t.DcsexecID = DcsexecID;
			t.QuoteType = QuoteType;
			t.MDReqID = MDReqID;
			t.QuoteTransType = QuoteTransType;
			t.QuoteStatusDesc = QuoteStatusDesc;
			t.OrdStatusReqID = OrdStatusReqID;
			t.ExecAckStatus = ExecAckStatus;
			t.OrdRejReason = OrdRejReason;
			t.RejectText = RejectText;
			t.TransactTime = TransactTime;
			t.SecurityID = SecurityID;
			t.Symbol = Symbol;
			t.ValidUntilTime = ValidUntilTime;
			t.FirmToNonTime = FirmToNonTime;
			t.OrdType = OrdType;
			t.TimeInForce = TimeInForce;
			t.Side = Side;
			t.Price = Price;
			t.Price2 = Price2;
			t.NetPrice = NetPrice;
			t.NetPrice2 = NetPrice2;
			t.Yield = Yield;
			t.Yield2 = Yield2;
			t.OrderQty = OrderQty;
			t.OrderQty2 = OrderQty2;
			t.LeavesQty = LeavesQty;
			t.LeavesQty2 = LeavesQty2;
			t.CumQty = CumQty;
			t.TradeDate = TradeDate;
			t.TradeTime = TradeTime;
			t.SettlDate = SettlDate;
			t.SettlType = SettlType;
			t.ClearingMethod = ClearingMethod;
			t.DeliveryType = DeliveryType;
			t.ExecID = ExecID;
			t.ExecType = ExecType;
			t.LastPx = LastPx;
			t.LastQty = LastQty;
			t.PartyID = PartyID;
			t.PartyName = PartyName;
			t.PartySubID = PartySubID;
			t.PartySubName = PartySubName;
			t.CounterPartyID = CounterPartyID;
			t.CounterPartyName = CounterPartyName;
			t.CounterPartyFullName = CounterPartyFullName;
			t.CounterPartySubID = CounterPartySubID;
			t.CounterPartySubName = CounterPartySubName;
			t.CounterPartyOrdID = CounterPartyOrdID;
			t.DirtyPrice = DirtyPrice;
			t.OpenQty = OpenQty;
			t.UserID = UserID;
			t.UserName = UserName;
			t.BizType = BizType;
			t.BizTypeID = BizTypeID;
			t.AvgPx = AvgPx;
			t.RefOrderID = RefOrderID;
			t.FirmFlag = FirmFlag;
			t.WholeFlag = WholeFlag;
			t.VenuesSelectF = VenuesSelectF;
			t.VenuesRdmNum = VenuesRdmNum;
			t.SelfSelectVenues = SelfSelectVenues;
			t.BlockNum = BlockNum;
			t.DealType = DealType;
			t.CrossVenues = CrossVenues;
			t.AllocID = AllocID;
			t.UpdateTime = UpdateTime;
			t.UpdateUser = UpdateUser;
			t.SynchFlag = SynchFlag;
			t.OCOFlag = OCOFlag;
			t.InstitutionID = InstitutionID;
			t.InstitutionName = InstitutionName;
			t.TraderID = TraderID;
			t.TraderName = TraderName;
			t.ShowTrader = ShowTrader;
			t.RoleName = RoleName;
			t.RoleID = RoleID;
			t.SalesAgentFlag = SalesAgentFlag;
			t.CounterpartySalesAgentFlag = CounterpartySalesAgentFlag;
			t.OrigOrdPrice = OrigOrdPrice;
			t.MacID = MacID;
			t.PublishIP = PublishIP;
			t.InsideIP = InsideIP;
			t.HardDiskID = HardDiskID;
			t.BrokerExecID = BrokerExecID;
			t.AppointedID = AppointedID;
			t.InitiatorID = InitiatorID;
			t.InitiatorSubID = InitiatorSubID;
			t.BridgecounterPartyFullName = BridgecounterPartyFullName;
			t.UpdateFiled = UpdateFiled;
			t.TransType = TransType;
			t.RoleType = RoleType;
			t.PartyOrderSource = PartyOrderSource;
			t.CounterPartyOrderSource = CounterPartyOrderSource;
			t.RecordNum = RecordNum;
			t.SendingTime = SendingTime;
			t.IocTimeOut = IocTimeOut;
			t.IocPrice = IocPrice;
			t.HandlInst = HandlInst;
			t.IocVenueTypeGW = IocVenueTypeGW;
			t.IocVenues = IocVenues;
			t.CounterPartyVenues = CounterPartyVenues;
			t.ParentFlag = ParentFlag;
			t.ParentFlagQty = ParentFlagQty;
			t.ActionFlag = ActionFlag;
			t.ExecTypeReason = ExecTypeReason;
			t.TradeMethod = TradeMethod;
			t.SecurityType = SecurityType;
			t.A1 = A1;
			t.B1 = B1;
			t.A0 = A0;
			t.B2 = B2;
			t.M5b1 = M5b1;
			t.M5a1 = M5a1;
			t.A2 = A2;
			t.B0 = B0;
			t.Vs = Vs;
			t.Info1 = Info1;
			t.Info2 = Info2;
			t.Info3 = Info3;
			t.Info4 = Info4;
			t.Info5 = Info5;
			t.Info6 = Info6;
			t.Info7 = Info7;
			t.Info8 = Info8;
			t.Info9 = Info9;
			t.Info10 = Info10;
			t.TrigEvent = TrigEvent;
			t.CancEvent = CancEvent;
			t.InValidFlag = InValidFlag;
			t.EREventName = EREventName;
			t.CancEventName = CancEventName;
			
			t.trigEventListener = trigEventListener;
			t.cancEventListener = cancEventListener;
			return t;
		}
	}
}
