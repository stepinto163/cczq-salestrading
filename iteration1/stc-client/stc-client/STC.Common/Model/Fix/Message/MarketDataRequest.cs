using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix.Message
{
    /// <summary>
    /// Message
    /// </summary>
    /// <remarks>
    /// 2020.09.08: 创建. 模板插件(V1.3.7488.16761(build:20200702)) 模板版本(v1.0.1.1)<br/>
    /// </remarks>
    public class MarketDataRequest : FixMessage
    {
        public MarketDataRequest()
        {
            StandardHeader.MsgType = "V";
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
        public string MDBookType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDBookType)); }
            set { this.Put(MessageDictionary.Fields.MDBookType, value); }
        }
        public string SubscriptionRequestType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SubscriptionRequestType)); }
            set { this.Put(MessageDictionary.Fields.SubscriptionRequestType, value); }
        }
        public string ClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
            set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
        }
        public string MDReqID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
            set { this.Put(MessageDictionary.Fields.MDReqID, value); }
        }
        public string MarketIndicator
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
            set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
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
        public string QuoteStatus
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteStatus)); }
            set { this.Put(MessageDictionary.Fields.QuoteStatus, value); }
        }
        public string SettlType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlType)); }
            set { this.Put(MessageDictionary.Fields.SettlType, value); }
        }
        public string Side
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Side)); }
            set { this.Put(MessageDictionary.Fields.Side, value); }
        }
        public string OrderQty
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderQty)); }
            set { this.Put(MessageDictionary.Fields.OrderQty, value); }
        }
        public string DeliveryType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DeliveryType)); }
            set { this.Put(MessageDictionary.Fields.DeliveryType, value); }
        }
        public string ClearingMethod
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClearingMethod)); }
            set { this.Put(MessageDictionary.Fields.ClearingMethod, value); }
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
        public string PartyID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyID)); }
            set { this.Put(MessageDictionary.Fields.PartyID, value); }
        }
        public string PartySubID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubID)); }
            set { this.Put(MessageDictionary.Fields.PartySubID, value); }
        }
        public string MarkerID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarkerID)); }
            set { this.Put(MessageDictionary.Fields.MarkerID, value); }
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
        public MarketDataRequest Clone()
        {
            MarketDataRequest t = new MarketDataRequest();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.MDBookType = MDBookType;
            t.SubscriptionRequestType = SubscriptionRequestType;
            t.ClOrdID = ClOrdID;
            t.MDReqID = MDReqID;
            t.MarketIndicator = MarketIndicator;
            t.SecurityID = SecurityID;
            t.Symbol = Symbol;
            t.QuoteStatus = QuoteStatus;
            t.SettlType = SettlType;
            t.Side = Side;
            t.OrderQty = OrderQty;
            t.DeliveryType = DeliveryType;
            t.ClearingMethod = ClearingMethod;
            t.ValidUntilTime = ValidUntilTime;
            t.TransactTime = TransactTime;
            t.PartyID = PartyID;
            t.PartySubID = PartySubID;
            t.MarkerID = MarkerID;
            t.Info1 = Info1;
            t.Info2 = Info2;
            t.Info3 = Info3;
            t.Info4 = Info4;
            t.Info5 = Info5;
            return t;
        }
    }
}

