using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix.Message
{
    /// <summary>
    /// Message
    /// </summary>
    /// <remarks>
    /// 2020.09.08: 创建. 模板插件(V1.3.7488.16761(build:20200702)) 模板版本(v1.0.1.1)<br/>
    /// </remarks>
    public class OrderCancelRequest : FixMessage
    {
        public OrderCancelRequest()
        {
            StandardHeader.MsgType = "F";
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
        public string MarketIndicator
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
            set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
        }
        public string ClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
            set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
        }
        public string OrigClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrigClOrdID)); }
            set { this.Put(MessageDictionary.Fields.OrigClOrdID, value); }
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
        public string QuoteType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteType)); }
            set { this.Put(MessageDictionary.Fields.QuoteType, value); }
        }
        public string AlgoName
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.AlgoName)); }
            set { this.Put(MessageDictionary.Fields.AlgoName, value); }
        }
        public string ValidUntilTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ValidUntilTime)); }
            set { this.Put(MessageDictionary.Fields.ValidUntilTime, value); }
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
        public OrderCancelRequest Clone()
        {
            OrderCancelRequest t = new OrderCancelRequest();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.MarketIndicator = MarketIndicator;
            t.ClOrdID = ClOrdID;
            t.OrigClOrdID = OrigClOrdID;
            t.OrderID = OrderID;
            t.QuoteID = QuoteID;
            t.QuoteType = QuoteType;
            t.AlgoName = AlgoName;
            t.ValidUntilTime = ValidUntilTime;
            t.TransactTime = TransactTime;
            t.SecurityID = SecurityID;
            t.Symbol = Symbol;
            t.Side = Side;
            t.Price = Price;
            t.Price2 = Price2;
            t.NetPrice = NetPrice;
            t.NetPrice2 = NetPrice2;
            t.Yield = Yield;
            t.Yield2 = Yield2;
            t.OrderQty = OrderQty;
            t.OrderQty2 = OrderQty2;
            t.PartyID = PartyID;
            t.PartyName = PartyName;
            t.PartySubID = PartySubID;
            t.PartySubName = PartySubName;
            t.UserID = UserID;
            t.UserName = UserName;
            t.BizType = BizType;
            t.Info1 = Info1;
            t.Info2 = Info2;
            t.Info3 = Info3;
            t.Info4 = Info4;
            t.Info5 = Info5;
            return t;
        }
    }
}
