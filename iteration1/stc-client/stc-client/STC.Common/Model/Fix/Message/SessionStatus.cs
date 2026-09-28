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
    public class SessionStatus : FixMessage
    {
        public SessionStatus()
        {
            StandardHeader.MsgType = "IC4";
        }
        public string TargetSessionId
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TargetSessionId)); }
            set { this.Put(MessageDictionary.Fields.TargetSessionId, value); }
        }
        public string NetStatus
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetStatus)); }
            set { this.Put(MessageDictionary.Fields.NetStatus, value); }
        }
        public string NetLatency
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetLatency)); }
            set { this.Put(MessageDictionary.Fields.NetLatency, value); }
        }
        public string NetLatencyStatus
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetLatencyStatus)); }
            set { this.Put(MessageDictionary.Fields.NetLatencyStatus, value); }
        }
        public string MarketIndicator
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
            set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
        }
        public string TradSesStatus
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradSesStatus)); }
            set { this.Put(MessageDictionary.Fields.TradSesStatus, value); }
        }
        public string TradSesStartTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradSesStartTime)); }
            set { this.Put(MessageDictionary.Fields.TradSesStartTime, value); }
        }
        public string TradSesCloseTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradSesCloseTime)); }
            set { this.Put(MessageDictionary.Fields.TradSesCloseTime, value); }
        }
        public string TradSesOpenTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradSesOpenTime)); }
            set { this.Put(MessageDictionary.Fields.TradSesOpenTime, value); }
        }
        public string TradSesEndTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradSesEndTime)); }
            set { this.Put(MessageDictionary.Fields.TradSesEndTime, value); }
        }
        public string VenueTypeGW
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenueTypeGW)); }
            set { this.Put(MessageDictionary.Fields.VenueTypeGW, value); }
        }
        public string Text
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Text)); }
            set { this.Put(MessageDictionary.Fields.Text, value); }
        }
        public SessionStatus Clone()
        {
            SessionStatus t = new SessionStatus();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.TargetSessionId = TargetSessionId;
            t.NetStatus = NetStatus;
            t.NetLatency = NetLatency;
            t.NetLatencyStatus = NetLatencyStatus;
            t.MarketIndicator = MarketIndicator;
            t.TradSesStatus = TradSesStatus;
            t.TradSesStartTime = TradSesStartTime;
            t.TradSesCloseTime = TradSesCloseTime;
            t.TradSesOpenTime = TradSesOpenTime;
            t.TradSesEndTime = TradSesEndTime;
            t.VenueTypeGW = VenueTypeGW;
            t.Text = Text;
            return t;
        }
    }
}

