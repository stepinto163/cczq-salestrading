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
    public class ExecutionAcknowledgement : FixMessage
    {
        public ExecutionAcknowledgement()
        {
            StandardHeader.MsgType = "BN";
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
        public string ClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
            set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
        }
        public string ExecAckStatus
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecAckStatus)); }
            set { this.Put(MessageDictionary.Fields.ExecAckStatus, value); }
        }
        public string ExecID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExecID)); }
            set { this.Put(MessageDictionary.Fields.ExecID, value); }
        }
        public ExecutionAcknowledgement Clone()
        {
            ExecutionAcknowledgement t = new ExecutionAcknowledgement();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.ClOrdID = ClOrdID;
            t.ExecAckStatus = ExecAckStatus;
            t.ExecID = ExecID;
            return t;
        }
    }
}

