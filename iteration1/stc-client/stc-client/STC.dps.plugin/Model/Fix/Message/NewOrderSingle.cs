using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix.Message
{
    /// <summary>
    /// Message
    /// </summary>
    /// <remarks>
    /// 2020.09.11: 创建. 模板插件(V1.3.7488.16761(build:20200702)) 模板版本(v1.0.1.1)<br/>
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
            t.Yield = Yield;
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
            t.PartySubName = PartySubName;
            t.CounterPartyID = CounterPartyID;
            t.CounterPartyName = CounterPartyName;
            t.CounterPartySubID = CounterPartySubID;
            t.CounterPartySubName = CounterPartySubName;
            t.UserID = UserID;
            t.UserName = UserName;
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
            return t;
        }
    }
}

