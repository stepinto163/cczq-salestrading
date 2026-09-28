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
    public class MarketDataRequestReject : FixMessage
    {
        public MarketDataRequestReject()
        {
            StandardHeader.MsgType = "Y";
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
        public string MDReqID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
            set { this.Put(MessageDictionary.Fields.MDReqID, value); }
        }
        public string ClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
            set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
        }
        public string MDBookType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDBookType)); }
            set { this.Put(MessageDictionary.Fields.MDBookType, value); }
        }
        public string MDReqRejReason
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqRejReason)); }
            set { this.Put(MessageDictionary.Fields.MDReqRejReason, value); }
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
        public MarketDataRequestReject Clone()
        {
            MarketDataRequestReject t = new MarketDataRequestReject();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.MDReqID = MDReqID;
            t.ClOrdID = ClOrdID;
            t.MDBookType = MDBookType;
            t.MDReqRejReason = MDReqRejReason;
            t.RejectText = RejectText;
            t.TransactTime = TransactTime;
            t.PartyID = PartyID;
            t.PartyName = PartyName;
            t.PartySubID = PartySubID;
            t.PartySubName = PartySubName;
            t.CounterPartyID = CounterPartyID;
            t.CounterPartyName = CounterPartyName;
            t.CounterPartySubID = CounterPartySubID;
            t.CounterPartySubName = CounterPartySubName;
            t.Info1 = Info1;
            t.Info2 = Info2;
            t.Info3 = Info3;
            t.Info4 = Info4;
            t.Info5 = Info5;
            return t;
        }
    }
}

