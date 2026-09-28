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
    public class OrderCancelReject : FixMessage
    {
        public OrderCancelReject()
        {
            StandardHeader.MsgType = "9";
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
        public string OrderID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrderID)); }
            set { this.Put(MessageDictionary.Fields.OrderID, value); }
        }
        public string CxlRejReason
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CxlRejReason)); }
            set { this.Put(MessageDictionary.Fields.CxlRejReason, value); }
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
        public OrderCancelReject Clone()
        {
            OrderCancelReject t = new OrderCancelReject();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.MarketIndicator = MarketIndicator;
            t.ClOrdID = ClOrdID;
            t.OrderID = OrderID;
            t.CxlRejReason = CxlRejReason;
            t.RejectText = RejectText;
            t.TransactTime = TransactTime;
            t.Info1 = Info1;
            t.Info2 = Info2;
            t.Info3 = Info3;
            t.Info4 = Info4;
            t.Info5 = Info5;
            return t;
        }
    }

}

