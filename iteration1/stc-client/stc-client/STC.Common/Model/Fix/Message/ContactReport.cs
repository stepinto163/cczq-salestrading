using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Fix.Message
{
   public class ContactReport:FixMessage
    {
        public ContactReport()
        {
            StandardHeader.MsgType = "8";
        }
        public string ContactId
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ContactId)); }
            set { this.Put(MessageDictionary.Fields.ContactId, value); }
        }
        /// <summary>
        /// 客户id
        /// </summary>
        public string Instid
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstId)); }
            set { this.Put(MessageDictionary.Fields.Instid, value); }
        }
        /// <summary>
        /// 拼音
        /// </summary>
        public string Name
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Name)); }
            set { this.Put(MessageDictionary.Fields.Name, value); }
        }
        /// <summary>
        /// 简称
        /// </summary>
        public string QQ
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QQ)); }
            set { this.Put(MessageDictionary.Fields.QQ, value); }
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string Updatetime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpdateInsTime)); }
            set { this.Put(MessageDictionary.Fields.Updatetime, value); }
        }
        public string Signal
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Signal)); }
            set { this.Put(MessageDictionary.Fields.Signal, value); }
        }
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public ContactReport Clone()
        {
            ContactReport t = new ContactReport();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.ContactId = ContactId;
            t.Instid = Instid;
            t.Name = Name;
            t.QQ = QQ;
            t.Updatetime = Updatetime;
            t.Signal = Signal;
            return t;
        }
    }
}
