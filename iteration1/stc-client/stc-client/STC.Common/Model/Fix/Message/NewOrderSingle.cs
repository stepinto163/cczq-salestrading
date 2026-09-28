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
	public class NewOrderSingle : FixMessage
	{
		public NewOrderSingle()
		{
			StandardHeader.MsgType = "D";
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
		public string StrategyOrderVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.StrategyOrderVenues)); }
			set { this.Put(MessageDictionary.Fields.StrategyOrderVenues, value); }
		}
		public string StrategyOrderId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.StrategyOrderId)); }
			set { this.Put(MessageDictionary.Fields.StrategyOrderId, value); }
		}
		public string MarketIndicator
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
			set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
		}
		public string QuoteID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteID)); }
			set { this.Put(MessageDictionary.Fields.QuoteID, value); }
		}
		public string MDReqID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
			set { this.Put(MessageDictionary.Fields.MDReqID, value); }
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
		public string Terminal
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Terminal)); }
			set { this.Put(MessageDictionary.Fields.Terminal, value); }
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
		public string TransactTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TransactTime)); }
			set { this.Put(MessageDictionary.Fields.TransactTime, value); }
		}
		public string OrdType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdType)); }
			set { this.Put(MessageDictionary.Fields.OrdType, value); }
		}
		public string QuoteType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteType)); }
			set { this.Put(MessageDictionary.Fields.QuoteType, value); }
		}
		public string QuoteTransType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteTransType)); }
			set { this.Put(MessageDictionary.Fields.QuoteTransType, value); }
		}
		public string OrdStatus
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrdStatus)); }
			set { this.Put(MessageDictionary.Fields.OrdStatus, value); }
		}
		public string TimeInForce
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TimeInForce)); }
			set { this.Put(MessageDictionary.Fields.TimeInForce, value); }
		}
		public string ClOrdID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
			set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
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
		public string SecurityType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SecurityType)); }
			set { this.Put(MessageDictionary.Fields.SecurityType, value); }
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
        public string FullPrice
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FullPrice)); }
            set { this.Put(MessageDictionary.Fields.FullPrice, value); }
        }
        public string FullPrice2
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FullPrice2)); }
            set { this.Put(MessageDictionary.Fields.FullPrice2, value); }
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
		public string ClearingMethod
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClearingMethod)); }
			set { this.Put(MessageDictionary.Fields.ClearingMethod, value); }
		}
		public string SettlType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlType)); }
			set { this.Put(MessageDictionary.Fields.SettlType, value); }
		}
		public string DeliveryType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DeliveryType)); }
			set { this.Put(MessageDictionary.Fields.DeliveryType, value); }
		}
		public string AlgoName
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AlgoName)); }
			set { this.Put(MessageDictionary.Fields.AlgoName, value); }
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
		public string PartyCode6
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyCode6)); }
			set { this.Put(MessageDictionary.Fields.PartyCode6, value); }
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
		public string CounterPartyCode6
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyCode6)); }
			set { this.Put(MessageDictionary.Fields.CounterPartyCode6, value); }
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
		public string CrossVenues
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CrossVenues)); }
			set { this.Put(MessageDictionary.Fields.CrossVenues, value); }
		}
		public string RiskManualCk
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RiskManualCk)); }
			set { this.Put(MessageDictionary.Fields.RiskManualCk, value); }
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
		public string ShowTrader
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ShowTrader)); }
			set { this.Put(MessageDictionary.Fields.ShowTrader, value); }
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
		public string HandlInst
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HandlInst)); }
			set { this.Put(MessageDictionary.Fields.HandlInst, value); }
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
		public string PriceCondition
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PriceCondition)); }
			set { this.Put(MessageDictionary.Fields.PriceCondition, value); }
		}
		public string PendingCancelTime
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PendingCancelTime)); }
			set { this.Put(MessageDictionary.Fields.PendingCancelTime, value); }
		}
		public string Text
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Text)); }
			set { this.Put(MessageDictionary.Fields.Text, value); }
		}
		public string OrgType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrgType)); }
			set { this.Put(MessageDictionary.Fields.OrgType, value); }
		}
		public string VipCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VipCode)); }
			set { this.Put(MessageDictionary.Fields.VipCode, value); }
		}
		public string IfSubBond
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.IfSubBond)); }
			set { this.Put(MessageDictionary.Fields.IfSubBond, value); }
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
		public string CompanyId
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CompanyId)); }
			set { this.Put(MessageDictionary.Fields.CompanyId, value); }
		}
		public string RefOrderID
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RefOrderID)); }
			set { this.Put(MessageDictionary.Fields.RefOrderID, value); }
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

		public string ActionFlag
        {
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ActionFlag)); }
			set { this.Put(MessageDictionary.Fields.ActionFlag, value); }
		}

		public string RiskReTryFlag
        {
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RiskReTryFlag)); }
			set { this.Put(MessageDictionary.Fields.RiskReTryFlag, value); }
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

		public NewOrderSingle Clone()
		{
			NewOrderSingle t = new NewOrderSingle();
			t.StandardHeader = StandardHeader;
			t.StandardTrailer = StandardTrailer;
			t.VenueTypeGW = VenueTypeGW;
			t.Venues = Venues;
			t.StrategyOrderVenues = StrategyOrderVenues;
			t.StrategyOrderId = StrategyOrderId;
			t.MarketIndicator = MarketIndicator;
			t.QuoteID = QuoteID;
			t.MDReqID = MDReqID;
			t.BookID = BookID;
			t.QuantID = QuantID;
			t.Terminal = Terminal;
			t.ValidUntilTime = ValidUntilTime;
			t.FirmToNonTime = FirmToNonTime;
			t.TransactTime = TransactTime;
			t.OrdType = OrdType;
			t.QuoteType = QuoteType;
			t.QuoteTransType = QuoteTransType;
			t.OrdStatus = OrdStatus;
			t.TimeInForce = TimeInForce;
			t.ClOrdID = ClOrdID;
			t.SecurityID = SecurityID;
			t.Symbol = Symbol;
			t.SecurityType = SecurityType;
			t.Side = Side;
			t.Price = Price;
			t.Price2 = Price2;
			t.NetPrice = NetPrice;
			t.NetPrice2 = NetPrice2;
			t.FullPrice = FullPrice;
            t.FullPrice2 = FullPrice2;
            t.Yield = Yield;
			t.YieldType = YieldType;
			t.Yield2 = Yield2;
			t.OrderQty = OrderQty;
			t.OrderQty2 = OrderQty2;
			t.ClearingMethod = ClearingMethod;
			t.SettlType = SettlType;
			t.DeliveryType = DeliveryType;
			t.AlgoName = AlgoName;
			t.PartyID = PartyID;
			t.PartyName = PartyName;
			t.PartySubID = PartySubID;
			t.PartySubID2 = PartySubID2;
			t.PartySubName = PartySubName;
			t.PartyCode6 = PartyCode6;
			t.CounterPartyID = CounterPartyID;
			t.CounterPartyName = CounterPartyName;
			t.CounterPartySubID = CounterPartySubID;
			t.CounterPartySubName = CounterPartySubName;
			t.CounterPartyCode6 = CounterPartyCode6;
			t.UserID = UserID;
			t.UserName = UserName;
			t.Name = Name;
			t.BizType = BizType;
			t.BizTypeID = BizTypeID;
			t.VenuesSelectF = VenuesSelectF;
			t.VenuesRdmNum = VenuesRdmNum;
			t.SelfSelectVenues = SelfSelectVenues;
			t.BlockNum = BlockNum;
			t.FirmFlag = FirmFlag;
			t.WholeFlag = WholeFlag;
			t.CrossVenues = CrossVenues;
			t.RiskManualCk = RiskManualCk;
			t.OCOFlag = OCOFlag;
			t.InstitutionID = InstitutionID;
			t.InstitutionName = InstitutionName;
			t.TraderID = TraderID;
			t.TraderName = TraderName;
			t.RoleName = RoleName;
			t.RoleID = RoleID;
			t.ShowTrader = ShowTrader;
			t.SalesAgentFlag = SalesAgentFlag;
			t.CounterpartySalesAgentFlag = CounterpartySalesAgentFlag;
			t.OrigOrdPrice = OrigOrdPrice;
			t.MacID = MacID;
			t.PublishIP = PublishIP;
			t.InsideIP = InsideIP;
			t.HardDiskID = HardDiskID;
			t.IocTimeOut = IocTimeOut;
			t.IocPrice = IocPrice;
			t.IocVenueTypeGW = IocVenueTypeGW;
			t.IocVenues = IocVenues;
			t.HandlInst = HandlInst;
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
			t.AccountID = AccountID;
			t.AccountName = AccountName;
			t.PriceCondition = PriceCondition;
			t.PendingCancelTime = PendingCancelTime;
			t.Text = Text;
			t.OrgType = OrgType;
			t.VipCode = VipCode;
			t.IfSubBond = IfSubBond;
			t.ExerciseFlag = ExerciseFlag;
			t.PriceType = PriceType;
			t.CreateBy = CreateBy;
			t.MacaulayDuration = MacaulayDuration;
			t.ModifiedDuration = ModifiedDuration;
			t.Pvbp = Pvbp;
			t.Convexity = Convexity;
			t.RepoMethod = RepoMethod;
			t.TradeLimitDays = TradeLimitDays;
			t.DeliveryType2 = DeliveryType2;
			t.Channel = Channel;
			t.OnLine = OnLine;
			t.FanDian = FanDian;
			t.FanDianFlag = FanDianFlag;
			t.PartDealFlag = PartDealFlag;
			t.ExchangeSignFlag = ExchangeSignFlag;
			t.BargainFlag = BargainFlag;
			t.CompanyId = CompanyId;
			t.RefOrderID = RefOrderID;
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
