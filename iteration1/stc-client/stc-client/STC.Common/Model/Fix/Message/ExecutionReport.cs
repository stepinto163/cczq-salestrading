using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix.Message
{
	/// <summary>
	/// Message
	/// </summary>
	/// <remarks>
	/// 2024.05.09: 创建. 模板插件(V1.3.7781.22279(build:20210421)) 模板版本(v1.0.1.1)<br/>
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
		public string BookID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BookID)); }
			set { this.Put(MessageDictionary.Fields.BookID, value); }
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
		public string Rejecter
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Rejecter)); }
			set { this.Put(MessageDictionary.Fields.Rejecter, value); }
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
		public string NetPrice3
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetPrice3)); }
			set { this.Put(MessageDictionary.Fields.NetPrice3, value); }
		}
		public string Yield
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Yield)); }
			set { this.Put(MessageDictionary.Fields.Yield, value); }
		}
		public string YieldType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.YieldType)); }
			set { this.Put(MessageDictionary.Fields.YieldType, value); }
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
		public string CumQty2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CumQty2)); }
			set { this.Put(MessageDictionary.Fields.CumQty2, value); }
		}
		public string OrdStatus2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdStatus2)); }
			set { this.Put(MessageDictionary.Fields.OrdStatus2, value); }
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
		public string ExecType2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecType2)); }
			set { this.Put(MessageDictionary.Fields.ExecType2, value); }
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
		public string LastPx2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastPx2)); }
			set { this.Put(MessageDictionary.Fields.LastPx2, value); }
		}
		public string LastQty2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastQty2)); }
			set { this.Put(MessageDictionary.Fields.LastQty2, value); }
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
		public string PartySubID2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubID2)); }
			set { this.Put(MessageDictionary.Fields.PartySubID2, value); }
		}
		public string PartySubName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubName)); }
			set { this.Put(MessageDictionary.Fields.PartySubName, value); }
		}
		public string PartyCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyCode)); }
			set { this.Put(MessageDictionary.Fields.PartyCode, value); }
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
		public string CounterPartyCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyCode)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyCode, value); }
		}
		public string DirtyPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DirtyPrice)); }
			set { this.Put(MessageDictionary.Fields.DirtyPrice, value); }
		}
		public string DirtyPrice2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DirtyPrice2)); }
			set { this.Put(MessageDictionary.Fields.DirtyPrice2, value); }
		}
		public string DirtyPrice3
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DirtyPrice3)); }
			set { this.Put(MessageDictionary.Fields.DirtyPrice3, value); }
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
		public string Name
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Name)); }
			set { this.Put(MessageDictionary.Fields.Name, value); }
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
		public string AvgPx2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AvgPx2)); }
			set { this.Put(MessageDictionary.Fields.AvgPx2, value); }
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
		public string MaxFloor
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MaxFloor)); }
			set { this.Put(MessageDictionary.Fields.MaxFloor, value); }
		}
		public string IntentionID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IntentionID)); }
			set { this.Put(MessageDictionary.Fields.IntentionID, value); }
		}
		public string OffsetFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OffsetFlag)); }
			set { this.Put(MessageDictionary.Fields.OffsetFlag, value); }
		}
		public string HedgeFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HedgeFlag)); }
			set { this.Put(MessageDictionary.Fields.HedgeFlag, value); }
		}
		public string ExerciseFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExerciseFlag)); }
			set { this.Put(MessageDictionary.Fields.ExerciseFlag, value); }
		}
		public string PriceType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PriceType)); }
			set { this.Put(MessageDictionary.Fields.PriceType, value); }
		}
		public string SSDetect
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SSDetect)); }
			set { this.Put(MessageDictionary.Fields.SSDetect, value); }
		}
		public string QuoteReqID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteReqID)); }
			set { this.Put(MessageDictionary.Fields.QuoteReqID, value); }
		}
		public string QuoteStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteStatus)); }
			set { this.Put(MessageDictionary.Fields.QuoteStatus, value); }
		}
		public string OrigClOrdID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrigClOrdID)); }
			set { this.Put(MessageDictionary.Fields.OrigClOrdID, value); }
		}
		public string AccountID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AccountID)); }
			set { this.Put(MessageDictionary.Fields.AccountID, value); }
		}
		public string AccountName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AccountName)); }
			set { this.Put(MessageDictionary.Fields.AccountName, value); }
		}
		public string BBVol
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BBVol)); }
			set { this.Put(MessageDictionary.Fields.BBVol, value); }
		}
		public string BBPx
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BBPx)); }
			set { this.Put(MessageDictionary.Fields.BBPx, value); }
		}
		public string BBTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BBTime)); }
			set { this.Put(MessageDictionary.Fields.BBTime, value); }
		}
		public string BOVol
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BOVol)); }
			set { this.Put(MessageDictionary.Fields.BOVol, value); }
		}
		public string BOPx
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BOPx)); }
			set { this.Put(MessageDictionary.Fields.BOPx, value); }
		}
		public string BOTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BOTime)); }
			set { this.Put(MessageDictionary.Fields.BOTime, value); }
		}
		public string Tag1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Tag1)); }
			set { this.Put(MessageDictionary.Fields.Tag1, value); }
		}
		public string PriceCondition
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PriceCondition)); }
			set { this.Put(MessageDictionary.Fields.PriceCondition, value); }
		}
		public string DqOrders
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DqOrders)); }
			set { this.Put(MessageDictionary.Fields.DqOrders, value); }
		}
		public string CounterAccountID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterAccountID)); }
			set { this.Put(MessageDictionary.Fields.CounterAccountID, value); }
		}
		public string CounterAccountName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterAccountName)); }
			set { this.Put(MessageDictionary.Fields.CounterAccountName, value); }
		}
		public string Version
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Version)); }
			set { this.Put(MessageDictionary.Fields.Version, value); }
		}
		public string DqInfo
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DqInfo)); }
			set { this.Put(MessageDictionary.Fields.DqInfo, value); }
		}
		public string MatchStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MatchStatus)); }
			set { this.Put(MessageDictionary.Fields.MatchStatus, value); }
		}
		public string RelateStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RelateStatus)); }
			set { this.Put(MessageDictionary.Fields.RelateStatus, value); }
		}
		public string QuoteMethod
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteMethod)); }
			set { this.Put(MessageDictionary.Fields.QuoteMethod, value); }
		}
		public string EntryType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.EntryType)); }
			set { this.Put(MessageDictionary.Fields.EntryType, value); }
		}
		public string Option
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Option)); }
			set { this.Put(MessageDictionary.Fields.Option, value); }
		}
		public string CreateBy
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CreateBy)); }
			set { this.Put(MessageDictionary.Fields.CreateBy, value); }
		}
		public string MacaulayDuration
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MacaulayDuration)); }
			set { this.Put(MessageDictionary.Fields.MacaulayDuration, value); }
		}
		public string ModifiedDuration
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ModifiedDuration)); }
			set { this.Put(MessageDictionary.Fields.ModifiedDuration, value); }
		}
		public string Pvbp
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Pvbp)); }
			set { this.Put(MessageDictionary.Fields.Pvbp, value); }
		}
		public string Convexity
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Convexity)); }
			set { this.Put(MessageDictionary.Fields.Convexity, value); }
		}
		public string RepoMethod
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RepoMethod)); }
			set { this.Put(MessageDictionary.Fields.RepoMethod, value); }
		}
		public string TradeLimitDays
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeLimitDays)); }
			set { this.Put(MessageDictionary.Fields.TradeLimitDays, value); }
		}
		public string DeliveryType2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DeliveryType2)); }
			set { this.Put(MessageDictionary.Fields.DeliveryType2, value); }
		}
		public string SettlCurrency
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlCurrency)); }
			set { this.Put(MessageDictionary.Fields.SettlCurrency, value); }
		}
		public string SettlCurrFxRate
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlCurrFxRate)); }
			set { this.Put(MessageDictionary.Fields.SettlCurrFxRate, value); }
		}
		public string PledgeSubmissionCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PledgeSubmissionCode)); }
			set { this.Put(MessageDictionary.Fields.PledgeSubmissionCode, value); }
		}
		public string BenchmarkInterestRate
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BenchmarkInterestRate)); }
			set { this.Put(MessageDictionary.Fields.BenchmarkInterestRate, value); }
		}
		public string QuoteRespType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteRespType)); }
			set { this.Put(MessageDictionary.Fields.QuoteRespType, value); }
		}
		public string Spread
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Spread)); }
			set { this.Put(MessageDictionary.Fields.Spread, value); }
		}
		public string ContingencyIndicator
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ContingencyIndicator)); }
			set { this.Put(MessageDictionary.Fields.ContingencyIndicator, value); }
		}
		public string SeatNumber
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SeatNumber)); }
			set { this.Put(MessageDictionary.Fields.SeatNumber, value); }
		}
		public string Channel
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Channel)); }
			set { this.Put(MessageDictionary.Fields.Channel, value); }
		}
		public string OnLine
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OnLine)); }
			set { this.Put(MessageDictionary.Fields.OnLine, value); }
		}
		public string FanDian
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FanDian)); }
			set { this.Put(MessageDictionary.Fields.FanDian, value); }
		}
		public string FanDianFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FanDianFlag)); }
			set { this.Put(MessageDictionary.Fields.FanDianFlag, value); }
		}
		public string PartDealFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartDealFlag)); }
			set { this.Put(MessageDictionary.Fields.PartDealFlag, value); }
		}
		public string ExchangeSignFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExchangeSignFlag)); }
			set { this.Put(MessageDictionary.Fields.ExchangeSignFlag, value); }
		}
		public string BargainFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BargainFlag)); }
			set { this.Put(MessageDictionary.Fields.BargainFlag, value); }
		}
		public string VipCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VipCode)); }
			set { this.Put(MessageDictionary.Fields.VipCode, value); }
		}
		public string Text
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Text)); }
			set { this.Put(MessageDictionary.Fields.Text, value); }
		}
		public string CommandType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CommandType)); }
			set { this.Put(MessageDictionary.Fields.CommandType, value); }
		}
		public string OrderSN
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderSN)); }
			set { this.Put(MessageDictionary.Fields.OrderSN, value); }
		}
		public string CounterPartyTraderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyTraderId)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyTraderId, value); }
		}
		public string CounterPartyTraderName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyTraderName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyTraderName, value); }
		}
		public string CounterPartyInstitutionId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyInstitutionId)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyInstitutionId, value); }
		}
		public string CounterPartyInstitutionName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyInstitutionName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyInstitutionName, value); }
		}
		public string CompanyId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CompanyId)); }
			set { this.Put(MessageDictionary.Fields.CompanyId, value); }
		}
		public string MatchQTY
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MatchQTY)); }
			set { this.Put(MessageDictionary.Fields.MatchQTY, value); }
		}
		public string QuoteInternalFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteInternalFlag)); }
			set { this.Put(MessageDictionary.Fields.QuoteInternalFlag, value); }
		}
		public string TimeToMaturity
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TimeToMaturity)); }
			set { this.Put(MessageDictionary.Fields.TimeToMaturity, value); }
		}
		public string BrokerBConfirmQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BrokerBConfirmQty)); }
			set { this.Put(MessageDictionary.Fields.BrokerBConfirmQty, value); }
		}
		public string SendQQMsgFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendQQMsgFlag)); }
			set { this.Put(MessageDictionary.Fields.SendQQMsgFlag, value); }
		}
		public string SendQQMsgTem
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendQQMsgTem)); }
			set { this.Put(MessageDictionary.Fields.SendQQMsgTem, value); }
		}
		public string Sequence
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Sequence)); }
			set { this.Put(MessageDictionary.Fields.Sequence, value); }
		}
		public string BridgeFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeFlag)); }
			set { this.Put(MessageDictionary.Fields.BridgeFlag, value); }
		}
		public string SendTo
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendTo)); }
			set { this.Put(MessageDictionary.Fields.SendTo, value); }
		}
		public string SendTo2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendTo2)); }
			set { this.Put(MessageDictionary.Fields.SendTo2, value); }
		}
		public string CloseBy
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CloseBy)); }
			set { this.Put(MessageDictionary.Fields.CloseBy, value); }
		}
		public string BrokerUnread
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BrokerUnread)); }
			set { this.Put(MessageDictionary.Fields.BrokerUnread, value); }
		}
		public string CloseById
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CloseById)); }
			set { this.Put(MessageDictionary.Fields.CloseById, value); }
		}
		public string DCSFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DCSFlag)); }
			set { this.Put(MessageDictionary.Fields.DCSFlag, value); }
		}
		public string QueryFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QueryFlag)); }
			set { this.Put(MessageDictionary.Fields.QueryFlag, value); }
		}
		public string CloseById2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CloseById2)); }
			set { this.Put(MessageDictionary.Fields.CloseById2, value); }
		}
		public string InternalFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InternalFlag)); }
			set { this.Put(MessageDictionary.Fields.InternalFlag, value); }
		}
		public string SettlType2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlType2)); }
			set { this.Put(MessageDictionary.Fields.SettlType2, value); }
		}
		public string TradeDate2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeDate2)); }
			set { this.Put(MessageDictionary.Fields.TradeDate2, value); }
		}
		public string SettlDate2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlDate2)); }
			set { this.Put(MessageDictionary.Fields.SettlDate2, value); }
		}
		public string BridgeInstitutionName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionName)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionName, value); }
		}
		public string BridgeInstitutionId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionId)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionId, value); }
		}
		public string BridgeTraderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeTraderId)); }
			set { this.Put(MessageDictionary.Fields.BridgeTraderId, value); }
		}
		public string BridgeTraderName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeTraderName)); }
			set { this.Put(MessageDictionary.Fields.BridgeTraderName, value); }
		}
		public string BridgeChannel
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeChannel)); }
			set { this.Put(MessageDictionary.Fields.BridgeChannel, value); }
		}
		public string BridgeSendTo
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeSendTo)); }
			set { this.Put(MessageDictionary.Fields.BridgeSendTo, value); }
		}
		public string BridgeFee
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeFee)); }
			set { this.Put(MessageDictionary.Fields.BridgeFee, value); }
		}
		public string BridgeRemark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeRemark)); }
			set { this.Put(MessageDictionary.Fields.BridgeRemark, value); }
		}
		public string BridgeType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeType)); }
			set { this.Put(MessageDictionary.Fields.BridgeType, value); }
		}
		public string BridgeInstitutionName2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionName2)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionName2, value); }
		}
		public string BridgeInstitutionId2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionId2)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionId2, value); }
		}
		public string BridgeTraderId2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeTraderId2)); }
			set { this.Put(MessageDictionary.Fields.BridgeTraderId2, value); }
		}
		public string BridgeTraderName2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeTraderName2)); }
			set { this.Put(MessageDictionary.Fields.BridgeTraderName2, value); }
		}
		public string BridgeChannel2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeChannel2)); }
			set { this.Put(MessageDictionary.Fields.BridgeChannel2, value); }
		}
		public string BridgeSendTo2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeSendTo2)); }
			set { this.Put(MessageDictionary.Fields.BridgeSendTo2, value); }
		}
		public string BridgeFee2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeFee2)); }
			set { this.Put(MessageDictionary.Fields.BridgeFee2, value); }
		}
		public string BridgeRemark2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeRemark2)); }
			set { this.Put(MessageDictionary.Fields.BridgeRemark2, value); }
		}
		public string BridgeType2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeType2)); }
			set { this.Put(MessageDictionary.Fields.BridgeType2, value); }
		}
		public string BridgeStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeStatus)); }
			set { this.Put(MessageDictionary.Fields.BridgeStatus, value); }
		}
		public string BridgeCloseById
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeCloseById)); }
			set { this.Put(MessageDictionary.Fields.BridgeCloseById, value); }
		}
		public string BridgeCreateTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeCreateTime)); }
			set { this.Put(MessageDictionary.Fields.BridgeCreateTime, value); }
		}
		public string BridgeUpdateTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeUpdateTime)); }
			set { this.Put(MessageDictionary.Fields.BridgeUpdateTime, value); }
		}
		public string BridgeCreateTime2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeCreateTime2)); }
			set { this.Put(MessageDictionary.Fields.BridgeCreateTime2, value); }
		}
		public string BridgeUpdateTime2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeUpdateTime2)); }
			set { this.Put(MessageDictionary.Fields.BridgeUpdateTime2, value); }
		}
		public string BridgeRemarkShow
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeRemarkShow)); }
			set { this.Put(MessageDictionary.Fields.BridgeRemarkShow, value); }
		}
		public string BridgeRemarkShow2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeRemarkShow2)); }
			set { this.Put(MessageDictionary.Fields.BridgeRemarkShow2, value); }
		}
		public string BorderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BorderId)); }
			set { this.Put(MessageDictionary.Fields.BorderId, value); }
		}
		public string BorderId2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BorderId2)); }
			set { this.Put(MessageDictionary.Fields.BorderId2, value); }
		}
		public string SyncDCSFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SyncDCSFlag)); }
			set { this.Put(MessageDictionary.Fields.SyncDCSFlag, value); }
		}
		public string SyncDCSTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SyncDCSTime)); }
			set { this.Put(MessageDictionary.Fields.SyncDCSTime, value); }
		}
		public string DoubleBridgeChannel
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeChannel)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeChannel, value); }
		}
		public string DoubleBridgeSendTo
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeSendTo)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeSendTo, value); }
		}
		public string DoubleBridgeFee
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeFee)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeFee, value); }
		}
		public string DoubleBridgeRemark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeRemark)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeRemark, value); }
		}
		public string DoubleBridgeOffSettltype
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeOffSettltype)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeOffSettltype, value); }
		}
		public string DoubleBridgeBidSettltype
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeBidSettltype)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeBidSettltype, value); }
		}
		public string BondSubtype
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BondSubtype)); }
			set { this.Put(MessageDictionary.Fields.BondSubtype, value); }
		}
		public string OnlineA
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OnlineA)); }
			set { this.Put(MessageDictionary.Fields.OnlineA, value); }
		}
		public string OnlineB
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OnlineB)); }
			set { this.Put(MessageDictionary.Fields.OnlineB, value); }
		}
		public string AutoConfirmA
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AutoConfirmA)); }
			set { this.Put(MessageDictionary.Fields.AutoConfirmA, value); }
		}
		public string AutoConfirmB
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AutoConfirmB)); }
			set { this.Put(MessageDictionary.Fields.AutoConfirmB, value); }
		}
		public string BrokerageA
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BrokerageA)); }
			set { this.Put(MessageDictionary.Fields.BrokerageA, value); }
		}
		public string BrokerageB
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BrokerageB)); }
			set { this.Put(MessageDictionary.Fields.BrokerageB, value); }
		}
		public string Remark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Remark)); }
			set { this.Put(MessageDictionary.Fields.Remark, value); }
		}
		public string SyncQmFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SyncQmFlag)); }
			set { this.Put(MessageDictionary.Fields.SyncQmFlag, value); }
		}
		public string SyncQmTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SyncQmTime)); }
			set { this.Put(MessageDictionary.Fields.SyncQmTime, value); }
		}
		public string DoubleBridgeSettleSide
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DoubleBridgeSettleSide)); }
			set { this.Put(MessageDictionary.Fields.DoubleBridgeSettleSide, value); }
		}
		public string AgreementNo
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AgreementNo)); }
			set { this.Put(MessageDictionary.Fields.AgreementNo, value); }
		}
		public string ForBidRefreshFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ForBidRefreshFlag)); }
			set { this.Put(MessageDictionary.Fields.ForBidRefreshFlag, value); }
		}
		public string SendSorFlagA
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendSorFlagA)); }
			set { this.Put(MessageDictionary.Fields.SendSorFlagA, value); }
		}
		public string SendSorFlagB
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendSorFlagB)); }
			set { this.Put(MessageDictionary.Fields.SendSorFlagB, value); }
		}
		public string IssueState
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueState)); }
			set { this.Put(MessageDictionary.Fields.IssueState, value); }
		}
		public string IssueDir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueDir)); }
			set { this.Put(MessageDictionary.Fields.IssueDir, value); }
		}
		public string IssueChannel
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueChannel)); }
			set { this.Put(MessageDictionary.Fields.IssueChannel, value); }
		}
		public string IssueInstId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueInstId)); }
			set { this.Put(MessageDictionary.Fields.IssueInstId, value); }
		}
		public string IssueInstName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueInstName)); }
			set { this.Put(MessageDictionary.Fields.IssueInstName, value); }
		}
		public string IssueInstNameShort
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueInstNameShort)); }
			set { this.Put(MessageDictionary.Fields.IssueInstNameShort, value); }
		}
		public string IssueTraderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueTraderId)); }
			set { this.Put(MessageDictionary.Fields.IssueTraderId, value); }
		}
		public string IssueTraderName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueTraderName)); }
			set { this.Put(MessageDictionary.Fields.IssueTraderName, value); }
		}
		public string IssueRemark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueRemark)); }
			set { this.Put(MessageDictionary.Fields.IssueRemark, value); }
		}
		public string ProxyInstId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ProxyInstId)); }
			set { this.Put(MessageDictionary.Fields.ProxyInstId, value); }
		}
		public string ProxyFee
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ProxyFee)); }
			set { this.Put(MessageDictionary.Fields.ProxyFee, value); }
		}
		public string ProxyFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ProxyFlag)); }
			set { this.Put(MessageDictionary.Fields.ProxyFlag, value); }
		}
		public string SingleQqmsgSendFlagA
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SingleQqmsgSendFlagA)); }
			set { this.Put(MessageDictionary.Fields.SingleQqmsgSendFlagA, value); }
		}
		public string SingleQqmsgSendFlagB
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SingleQqmsgSendFlagB)); }
			set { this.Put(MessageDictionary.Fields.SingleQqmsgSendFlagB, value); }
		}
		public string OfrBridgeDir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OfrBridgeDir)); }
			set { this.Put(MessageDictionary.Fields.OfrBridgeDir, value); }
		}
		public string BidBridgeDir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BidBridgeDir)); }
			set { this.Put(MessageDictionary.Fields.BidBridgeDir, value); }
		}
		public string DetailMark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DetailMark)); }
			set { this.Put(MessageDictionary.Fields.DetailMark, value); }
		}
		public string AssociatedFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AssociatedFlag)); }
			set { this.Put(MessageDictionary.Fields.AssociatedFlag, value); }
		}
		public string MakerOrderQty
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MakerOrderQty)); }
			set { this.Put(MessageDictionary.Fields.MakerOrderQty, value); }
		}
		public string OFRSpecialBridgeFee
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OFRSpecialBridgeFee)); }
			set { this.Put(MessageDictionary.Fields.OFRSpecialBridgeFee, value); }
		}
		public string BIDSpecialBridgeFee
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BIDSpecialBridgeFee)); }
			set { this.Put(MessageDictionary.Fields.BIDSpecialBridgeFee, value); }
		}
		public string BridgeInstitutionFullName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionFullName)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionFullName, value); }
		}
		public string BridgeInstitutionFullName2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionFullName2)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionFullName2, value); }
		}
		public string ReadFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ReadFlag)); }
			set { this.Put(MessageDictionary.Fields.ReadFlag, value); }
		}
		public string SendMsgFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendMsgFlag)); }
			set { this.Put(MessageDictionary.Fields.SendMsgFlag, value); }
		}
		public string ID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ID)); }
			set { this.Put(MessageDictionary.Fields.ID, value); }
		}
		public string InstitutionShowName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstitutionShowName)); }
			set { this.Put(MessageDictionary.Fields.InstitutionShowName, value); }
		}
		public string CounterPartyInstitutionShowName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyInstitutionShowName)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyInstitutionShowName, value); }
		}
		public string BridgeInstitutionShowName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionShowName)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionShowName, value); }
		}
		public string BridgeInstitutionShowName2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeInstitutionShowName2)); }
			set { this.Put(MessageDictionary.Fields.BridgeInstitutionShowName2, value); }
		}
		public string AddBridgeWay
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AddBridgeWay)); }
			set { this.Put(MessageDictionary.Fields.AddBridgeWay, value); }
		}
		public string BridgeStatus2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeStatus2)); }
			set { this.Put(MessageDictionary.Fields.BridgeStatus2, value); }
		}
		public string InstExecOrderReqListExt
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstExecOrderReqListExt)); }
			set { this.Put(MessageDictionary.Fields.InstExecOrderReqListExt, value); }
		}
		public string OfrDir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OfrDir)); }
			set { this.Put(MessageDictionary.Fields.OfrDir, value); }
		}
		public string BidDir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BidDir)); }
			set { this.Put(MessageDictionary.Fields.BidDir, value); }
		}
		public string OfrDir2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OfrDir2)); }
			set { this.Put(MessageDictionary.Fields.OfrDir2, value); }
		}
		public string BidDir2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BidDir2)); }
			set { this.Put(MessageDictionary.Fields.BidDir2, value); }
		}
		public string DelFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DelFlag)); }
			set { this.Put(MessageDictionary.Fields.DelFlag, value); }
		}
		public string ParamCloseById
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ParamCloseById)); }
			set { this.Put(MessageDictionary.Fields.ParamCloseById, value); }
		}
		public string BridgeOrder
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeOrder)); }
			set { this.Put(MessageDictionary.Fields.BridgeOrder, value); }
		}
		public string BridgeOrder2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BridgeOrder2)); }
			set { this.Put(MessageDictionary.Fields.BridgeOrder2, value); }
		}
		public string IssueDesc
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueDesc)); }
			set { this.Put(MessageDictionary.Fields.IssueDesc, value); }
		}
		public string IssueShowType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IssueShowType)); }
			set { this.Put(MessageDictionary.Fields.IssueShowType, value); }
		}
		public string FeeTraderid
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeTraderid)); }
			set { this.Put(MessageDictionary.Fields.FeeTraderid, value); }
		}
		public string FeeTraderid2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeTraderid2)); }
			set { this.Put(MessageDictionary.Fields.FeeTraderid2, value); }
		}
		public string FeeTradername
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeTradername)); }
			set { this.Put(MessageDictionary.Fields.FeeTradername, value); }
		}
		public string FeeTradername2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeTradername2)); }
			set { this.Put(MessageDictionary.Fields.FeeTradername2, value); }
		}
		public string FeeInstitutionid
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeInstitutionid)); }
			set { this.Put(MessageDictionary.Fields.FeeInstitutionid, value); }
		}
		public string FeeInstitutionid2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeInstitutionid2)); }
			set { this.Put(MessageDictionary.Fields.FeeInstitutionid2, value); }
		}
		public string FeeInstitutionname
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeInstitutionname)); }
			set { this.Put(MessageDictionary.Fields.FeeInstitutionname, value); }
		}
		public string FeeInstitutionname2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FeeInstitutionname2)); }
			set { this.Put(MessageDictionary.Fields.FeeInstitutionname2, value); }
		}
		public string Contacttype
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Contacttype)); }
			set { this.Put(MessageDictionary.Fields.Contacttype, value); }
		}
		public string Contacttype2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Contacttype2)); }
			set { this.Put(MessageDictionary.Fields.Contacttype2, value); }
		}
		public string OperateBorderID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OperateBorderID)); }
			set { this.Put(MessageDictionary.Fields.OperateBorderID, value); }
		}
		public string ListedMarket
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ListedMarket)); }
			set { this.Put(MessageDictionary.Fields.ListedMarket, value); }
		}
		public string CombinedOrderRemark
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CombinedOrderRemark)); }
			set { this.Put(MessageDictionary.Fields.CombinedOrderRemark, value); }
		}
		public string CombinedOrderRemark2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CombinedOrderRemark2)); }
			set { this.Put(MessageDictionary.Fields.CombinedOrderRemark2, value); }
		}
		public string FidSettlAmount
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FidSettlAmount)); }
			set { this.Put(MessageDictionary.Fields.FidSettlAmount, value); }
		}
		public string FidSettlAmount2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FidSettlAmount2)); }
			set { this.Put(MessageDictionary.Fields.FidSettlAmount2, value); }
		}
		public string Doublebridgedir
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Doublebridgedir)); }
			set { this.Put(MessageDictionary.Fields.Doublebridgedir, value); }
		}
		public string MarkDetailUsers
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarkDetailUsers)); }
			set { this.Put(MessageDictionary.Fields.MarkDetailUsers, value); }
		}
		public string DayOffStr
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DayOffStr)); }
			set { this.Put(MessageDictionary.Fields.DayOffStr, value); }
		}
		public string ValuationNetPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ValuationNetPrice)); }
			set { this.Put(MessageDictionary.Fields.ValuationNetPrice, value); }
		}
		public string ValuationNetPrice2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ValuationNetPrice2)); }
			set { this.Put(MessageDictionary.Fields.ValuationNetPrice2, value); }
		}
		public string IsConfirmOperation
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IsConfirmOperation)); }
			set { this.Put(MessageDictionary.Fields.IsConfirmOperation, value); }
		}
		public string BusinessType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BusinessType)); }
			set { this.Put(MessageDictionary.Fields.BusinessType, value); }
		}
		public string YieldToExecution
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.YieldToExecution)); }
			set { this.Put(MessageDictionary.Fields.YieldToExecution, value); }
		}
		public string YieldToExecution2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.YieldToExecution2)); }
			set { this.Put(MessageDictionary.Fields.YieldToExecution2, value); }
		}
		public string Divided
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Divided)); }
			set { this.Put(MessageDictionary.Fields.Divided, value); }
		}

		private readonly string GROUP_NoUnderlyings = MessageDictionary.Fields.NoUnderlyings;
		public List<GNoUnderlyings> NoUnderlyings
		{
			get { return this.Get(GROUP_NoUnderlyings) as List<GNoUnderlyings>; }
			set { this.Put(GROUP_NoUnderlyings, value); }
		}
		public class GNoUnderlyings : BaseMessage
		{
			public string UnderlyingSymbol
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnderlyingSymbol)); }
				set { this.Put(MessageDictionary.Fields.UnderlyingSymbol, value); }
			}
			public string UnderlyingSecurityID
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnderlyingSecurityID)); }
				set { this.Put(MessageDictionary.Fields.UnderlyingSecurityID, value); }
			}
			public string UnderlyingQty
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnderlyingQty)); }
				set { this.Put(MessageDictionary.Fields.UnderlyingQty, value); }
			}

			private readonly string GROUP_NoUnderlyingStips = MessageDictionary.Fields.NoUnderlyingStips;
			public List<GNoUnderlyingStips> NoUnderlyingStips
			{
				get { return this.Get(GROUP_NoUnderlyingStips) as List<GNoUnderlyingStips>; }
				set { this.Put(GROUP_NoUnderlyingStips, value); }
			}
			public class GNoUnderlyingStips : BaseMessage
			{
				public string UnderlyingStipType
				{
					get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnderlyingStipType)); }
					set { this.Put(MessageDictionary.Fields.UnderlyingStipType, value); }
				}
				public string UnderlyingStipValue
				{
					get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnderlyingStipValue)); }
					set { this.Put(MessageDictionary.Fields.UnderlyingStipValue, value); }
				}
				public GNoUnderlyingStips Clone()
				{
					GNoUnderlyingStips t = new GNoUnderlyingStips();
					t.UnderlyingStipType = UnderlyingStipType;
					t.UnderlyingStipValue = UnderlyingStipValue;
					return t;
				}
			}

			public GNoUnderlyings Clone()
			{
				GNoUnderlyings t = new GNoUnderlyings();
				t.UnderlyingSymbol = UnderlyingSymbol;
				t.UnderlyingSecurityID = UnderlyingSecurityID;
				t.UnderlyingQty = UnderlyingQty;
				t.NoUnderlyingStips = new List<GNoUnderlyingStips>();
				if (NoUnderlyingStips != null)
				{
					foreach (var g in NoUnderlyingStips)
					{
						GNoUnderlyingStips gNoUnderlyingStips = g.Clone();
						t.NoUnderlyingStips.Add(gNoUnderlyingStips);
					}
				}
				return t;
			}
		}


		private readonly string GROUP_NoFloorDetails = MessageDictionary.Fields.NoFloorDetails;
		public List<GNoFloorDetails> NoFloorDetails
		{
			get { return this.Get(GROUP_NoFloorDetails) as List<GNoFloorDetails>; }
			set { this.Put(GROUP_NoFloorDetails, value); }
		}
		public class GNoFloorDetails : BaseMessage
		{
			public string FloorDetailID
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FloorDetailID)); }
				set { this.Put(MessageDictionary.Fields.FloorDetailID, value); }
			}
			public string FloorDetailPrice
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FloorDetailPrice)); }
				set { this.Put(MessageDictionary.Fields.FloorDetailPrice, value); }
			}

			private readonly string GROUP_NoFloorDetailSubIDs = MessageDictionary.Fields.NoFloorDetailSubIDs;
			public List<GNoFloorDetailSubIDs> NoFloorDetailSubIDs
			{
				get { return this.Get(GROUP_NoFloorDetailSubIDs) as List<GNoFloorDetailSubIDs>; }
				set { this.Put(GROUP_NoFloorDetailSubIDs, value); }
			}
			public class GNoFloorDetailSubIDs : BaseMessage
			{
				public string FloorDetailSubID
				{
					get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FloorDetailSubID)); }
					set { this.Put(MessageDictionary.Fields.FloorDetailSubID, value); }
				}
				public string FloorDetailSubIDType
				{
					get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FloorDetailSubIDType)); }
					set { this.Put(MessageDictionary.Fields.FloorDetailSubIDType, value); }
				}
				public GNoFloorDetailSubIDs Clone()
				{
					GNoFloorDetailSubIDs t = new GNoFloorDetailSubIDs();
					t.FloorDetailSubID = FloorDetailSubID;
					t.FloorDetailSubIDType = FloorDetailSubIDType;
					return t;
				}
			}

			public GNoFloorDetails Clone()
			{
				GNoFloorDetails t = new GNoFloorDetails();
				t.FloorDetailID = FloorDetailID;
				t.FloorDetailPrice = FloorDetailPrice;
				t.NoFloorDetailSubIDs = new List<GNoFloorDetailSubIDs>();
				if (NoFloorDetailSubIDs != null)
				{
					foreach (var g in NoFloorDetailSubIDs)
					{
						GNoFloorDetailSubIDs gNoFloorDetailSubIDs = g.Clone();
						t.NoFloorDetailSubIDs.Add(gNoFloorDetailSubIDs);
					}
				}
				return t;
			}
		}


		private readonly string GROUP_NoCrossCustodians = MessageDictionary.Fields.NoCrossCustodians;
		public List<GNoCrossCustodians> NoCrossCustodians
		{
			get { return this.Get(GROUP_NoCrossCustodians) as List<GNoCrossCustodians>; }
			set { this.Put(GROUP_NoCrossCustodians, value); }
		}
		public class GNoCrossCustodians : BaseMessage
		{
			public string CrossCustodianInstitutionName
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CrossCustodianInstitutionName)); }
				set { this.Put(MessageDictionary.Fields.CrossCustodianInstitutionName, value); }
			}
			public string CrossCustodianTradeAmt
			{
				get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CrossCustodianTradeAmt)); }
				set { this.Put(MessageDictionary.Fields.CrossCustodianTradeAmt, value); }
			}
			public GNoCrossCustodians Clone()
			{
				GNoCrossCustodians t = new GNoCrossCustodians();
				t.CrossCustodianInstitutionName = CrossCustodianInstitutionName;
				t.CrossCustodianTradeAmt = CrossCustodianTradeAmt;
				return t;
			}
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
			t.BookID = BookID;
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
			t.Rejecter = Rejecter;
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
			t.NetPrice3 = NetPrice3;
			t.Yield = Yield;
			t.YieldType = YieldType;
			t.Yield2 = Yield2;
			t.OrderQty = OrderQty;
			t.OrderQty2 = OrderQty2;
			t.LeavesQty = LeavesQty;
			t.LeavesQty2 = LeavesQty2;
			t.CumQty = CumQty;
			t.CumQty2 = CumQty2;
			t.OrdStatus2 = OrdStatus2;
			t.TradeDate = TradeDate;
			t.TradeTime = TradeTime;
			t.SettlDate = SettlDate;
			t.SettlType = SettlType;
			t.ClearingMethod = ClearingMethod;
			t.DeliveryType = DeliveryType;
			t.ExecID = ExecID;
			t.ExecType = ExecType;
			t.ExecType2 = ExecType2;
			t.LastPx = LastPx;
			t.LastQty = LastQty;
			t.LastPx2 = LastPx2;
			t.LastQty2 = LastQty2;
			t.PartyID = PartyID;
			t.PartyName = PartyName;
			t.PartySubID = PartySubID;
			t.PartySubID2 = PartySubID2;
			t.PartySubName = PartySubName;
			t.PartyCode = PartyCode;
			t.CounterPartyID = CounterPartyID;
			t.CounterPartyName = CounterPartyName;
			t.CounterPartyFullName = CounterPartyFullName;
			t.CounterPartySubID = CounterPartySubID;
			t.CounterPartySubName = CounterPartySubName;
			t.CounterPartyOrdID = CounterPartyOrdID;
			t.CounterPartyCode = CounterPartyCode;
			t.DirtyPrice = DirtyPrice;
			t.DirtyPrice2 = DirtyPrice2;
			t.DirtyPrice3 = DirtyPrice3;
			t.OpenQty = OpenQty;
			t.UserID = UserID;
			t.UserName = UserName;
			t.Name = Name;
			t.BizType = BizType;
			t.BizTypeID = BizTypeID;
			t.AvgPx = AvgPx;
			t.AvgPx2 = AvgPx2;
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
			t.MaxFloor = MaxFloor;
			t.IntentionID = IntentionID;
			t.OffsetFlag = OffsetFlag;
			t.HedgeFlag = HedgeFlag;
			t.ExerciseFlag = ExerciseFlag;
			t.PriceType = PriceType;
			t.SSDetect = SSDetect;
			t.QuoteReqID = QuoteReqID;
			t.QuoteStatus = QuoteStatus;
			t.OrigClOrdID = OrigClOrdID;
			t.AccountID = AccountID;
			t.AccountName = AccountName;
			t.BBVol = BBVol;
			t.BBPx = BBPx;
			t.BBTime = BBTime;
			t.BOVol = BOVol;
			t.BOPx = BOPx;
			t.BOTime = BOTime;
			t.Tag1 = Tag1;
			t.PriceCondition = PriceCondition;
			t.DqOrders = DqOrders;
			t.CounterAccountID = CounterAccountID;
			t.CounterAccountName = CounterAccountName;
			t.Version = Version;
			t.DqInfo = DqInfo;
			t.MatchStatus = MatchStatus;
			t.RelateStatus = RelateStatus;
			t.QuoteMethod = QuoteMethod;
			t.EntryType = EntryType;
			t.Option = Option;
			t.CreateBy = CreateBy;
			t.MacaulayDuration = MacaulayDuration;
			t.ModifiedDuration = ModifiedDuration;
			t.Pvbp = Pvbp;
			t.Convexity = Convexity;
			t.RepoMethod = RepoMethod;
			t.TradeLimitDays = TradeLimitDays;
			t.DeliveryType2 = DeliveryType2;
			t.SettlCurrency = SettlCurrency;
			t.SettlCurrFxRate = SettlCurrFxRate;
			t.PledgeSubmissionCode = PledgeSubmissionCode;
			t.BenchmarkInterestRate = BenchmarkInterestRate;
			t.QuoteRespType = QuoteRespType;
			t.Spread = Spread;
			t.ContingencyIndicator = ContingencyIndicator;
			t.SeatNumber = SeatNumber;
			t.Channel = Channel;
			t.OnLine = OnLine;
			t.FanDian = FanDian;
			t.FanDianFlag = FanDianFlag;
			t.PartDealFlag = PartDealFlag;
			t.ExchangeSignFlag = ExchangeSignFlag;
			t.BargainFlag = BargainFlag;
			t.VipCode = VipCode;
			t.Text = Text;
			t.CommandType = CommandType;
			t.OrderSN = OrderSN;
			t.CounterPartyTraderId = CounterPartyTraderId;
			t.CounterPartyTraderName = CounterPartyTraderName;
			t.CounterPartyInstitutionId = CounterPartyInstitutionId;
			t.CounterPartyInstitutionName = CounterPartyInstitutionName;
			t.CompanyId = CompanyId;
			t.MatchQTY = MatchQTY;
			t.QuoteInternalFlag = QuoteInternalFlag;
			t.TimeToMaturity = TimeToMaturity;
			t.BrokerBConfirmQty = BrokerBConfirmQty;
			t.SendQQMsgFlag = SendQQMsgFlag;
			t.SendQQMsgTem = SendQQMsgTem;
			t.Sequence = Sequence;
			t.BridgeFlag = BridgeFlag;
			t.SendTo = SendTo;
			t.SendTo2 = SendTo2;
			t.CloseBy = CloseBy;
			t.BrokerUnread = BrokerUnread;
			t.CloseById = CloseById;
			t.DCSFlag = DCSFlag;
			t.QueryFlag = QueryFlag;
			t.CloseById2 = CloseById2;
			t.InternalFlag = InternalFlag;
			t.SettlType2 = SettlType2;
			t.TradeDate2 = TradeDate2;
			t.SettlDate2 = SettlDate2;
			t.BridgeInstitutionName = BridgeInstitutionName;
			t.BridgeInstitutionId = BridgeInstitutionId;
			t.BridgeTraderId = BridgeTraderId;
			t.BridgeTraderName = BridgeTraderName;
			t.BridgeChannel = BridgeChannel;
			t.BridgeSendTo = BridgeSendTo;
			t.BridgeFee = BridgeFee;
			t.BridgeRemark = BridgeRemark;
			t.BridgeType = BridgeType;
			t.BridgeInstitutionName2 = BridgeInstitutionName2;
			t.BridgeInstitutionId2 = BridgeInstitutionId2;
			t.BridgeTraderId2 = BridgeTraderId2;
			t.BridgeTraderName2 = BridgeTraderName2;
			t.BridgeChannel2 = BridgeChannel2;
			t.BridgeSendTo2 = BridgeSendTo2;
			t.BridgeFee2 = BridgeFee2;
			t.BridgeRemark2 = BridgeRemark2;
			t.BridgeType2 = BridgeType2;
			t.BridgeStatus = BridgeStatus;
			t.BridgeCloseById = BridgeCloseById;
			t.BridgeCreateTime = BridgeCreateTime;
			t.BridgeUpdateTime = BridgeUpdateTime;
			t.BridgeCreateTime2 = BridgeCreateTime2;
			t.BridgeUpdateTime2 = BridgeUpdateTime2;
			t.BridgeRemarkShow = BridgeRemarkShow;
			t.BridgeRemarkShow2 = BridgeRemarkShow2;
			t.BorderId = BorderId;
			t.BorderId2 = BorderId2;
			t.SyncDCSFlag = SyncDCSFlag;
			t.SyncDCSTime = SyncDCSTime;
			t.DoubleBridgeChannel = DoubleBridgeChannel;
			t.DoubleBridgeSendTo = DoubleBridgeSendTo;
			t.DoubleBridgeFee = DoubleBridgeFee;
			t.DoubleBridgeRemark = DoubleBridgeRemark;
			t.DoubleBridgeOffSettltype = DoubleBridgeOffSettltype;
			t.DoubleBridgeBidSettltype = DoubleBridgeBidSettltype;
			t.BondSubtype = BondSubtype;
			t.OnlineA = OnlineA;
			t.OnlineB = OnlineB;
			t.AutoConfirmA = AutoConfirmA;
			t.AutoConfirmB = AutoConfirmB;
			t.BrokerageA = BrokerageA;
			t.BrokerageB = BrokerageB;
			t.Remark = Remark;
			t.SyncQmFlag = SyncQmFlag;
			t.SyncQmTime = SyncQmTime;
			t.DoubleBridgeSettleSide = DoubleBridgeSettleSide;
			t.AgreementNo = AgreementNo;
			t.ForBidRefreshFlag = ForBidRefreshFlag;
			t.SendSorFlagA = SendSorFlagA;
			t.SendSorFlagB = SendSorFlagB;
			t.IssueState = IssueState;
			t.IssueDir = IssueDir;
			t.IssueChannel = IssueChannel;
			t.IssueInstId = IssueInstId;
			t.IssueInstName = IssueInstName;
			t.IssueInstNameShort = IssueInstNameShort;
			t.IssueTraderId = IssueTraderId;
			t.IssueTraderName = IssueTraderName;
			t.IssueRemark = IssueRemark;
			t.ProxyInstId = ProxyInstId;
			t.ProxyFee = ProxyFee;
			t.ProxyFlag = ProxyFlag;
			t.SingleQqmsgSendFlagA = SingleQqmsgSendFlagA;
			t.SingleQqmsgSendFlagB = SingleQqmsgSendFlagB;
			t.OfrBridgeDir = OfrBridgeDir;
			t.BidBridgeDir = BidBridgeDir;
			t.DetailMark = DetailMark;
			t.AssociatedFlag = AssociatedFlag;
			t.MakerOrderQty = MakerOrderQty;
			t.OFRSpecialBridgeFee = OFRSpecialBridgeFee;
			t.BIDSpecialBridgeFee = BIDSpecialBridgeFee;
			t.BridgeInstitutionFullName = BridgeInstitutionFullName;
			t.BridgeInstitutionFullName2 = BridgeInstitutionFullName2;
			t.ReadFlag = ReadFlag;
			t.SendMsgFlag = SendMsgFlag;
			t.ID = ID;
			t.InstitutionShowName = InstitutionShowName;
			t.CounterPartyInstitutionShowName = CounterPartyInstitutionShowName;
			t.BridgeInstitutionShowName = BridgeInstitutionShowName;
			t.BridgeInstitutionShowName2 = BridgeInstitutionShowName2;
			t.AddBridgeWay = AddBridgeWay;
			t.BridgeStatus2 = BridgeStatus2;
			t.InstExecOrderReqListExt = InstExecOrderReqListExt;
			t.OfrDir = OfrDir;
			t.BidDir = BidDir;
			t.OfrDir2 = OfrDir2;
			t.BidDir2 = BidDir2;
			t.DelFlag = DelFlag;
			t.ParamCloseById = ParamCloseById;
			t.BridgeOrder = BridgeOrder;
			t.BridgeOrder2 = BridgeOrder2;
			t.IssueDesc = IssueDesc;
			t.IssueShowType = IssueShowType;
			t.FeeTraderid = FeeTraderid;
			t.FeeTraderid2 = FeeTraderid2;
			t.FeeTradername = FeeTradername;
			t.FeeTradername2 = FeeTradername2;
			t.FeeInstitutionid = FeeInstitutionid;
			t.FeeInstitutionid2 = FeeInstitutionid2;
			t.FeeInstitutionname = FeeInstitutionname;
			t.FeeInstitutionname2 = FeeInstitutionname2;
			t.Contacttype = Contacttype;
			t.Contacttype2 = Contacttype2;
			t.OperateBorderID = OperateBorderID;
			t.ListedMarket = ListedMarket;
			t.CombinedOrderRemark = CombinedOrderRemark;
			t.CombinedOrderRemark2 = CombinedOrderRemark2;
			t.FidSettlAmount = FidSettlAmount;
			t.FidSettlAmount2 = FidSettlAmount2;
			t.Doublebridgedir = Doublebridgedir;
			t.MarkDetailUsers = MarkDetailUsers;
			t.DayOffStr = DayOffStr;
			t.ValuationNetPrice = ValuationNetPrice;
			t.ValuationNetPrice2 = ValuationNetPrice2;
			t.IsConfirmOperation = IsConfirmOperation;
			t.BusinessType = BusinessType;
			t.YieldToExecution = YieldToExecution;
			t.YieldToExecution2 = YieldToExecution2;
			t.Divided = Divided;
			t.NoUnderlyings = new List<GNoUnderlyings>();
			if (NoUnderlyings != null)
			{
				foreach (var g in NoUnderlyings)
				{
					GNoUnderlyings gNoUnderlyings = g.Clone();
					t.NoUnderlyings.Add(gNoUnderlyings);
				}
			}
			t.NoFloorDetails = new List<GNoFloorDetails>();
			if (NoFloorDetails != null)
			{
				foreach (var g in NoFloorDetails)
				{
					GNoFloorDetails gNoFloorDetails = g.Clone();
					t.NoFloorDetails.Add(gNoFloorDetails);
				}
			}
			t.NoCrossCustodians = new List<GNoCrossCustodians>();
			if (NoCrossCustodians != null)
			{
				foreach (var g in NoCrossCustodians)
				{
					GNoCrossCustodians gNoCrossCustodians = g.Clone();
					t.NoCrossCustodians.Add(gNoCrossCustodians);
				}
			}
			return t;
		}
	}
}
