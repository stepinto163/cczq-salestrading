using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Fix.Message
{
   public class InstitutionReport:FixMessage
    {
        /// <summary>
        ///@ requirement 1.4.2.:JR02
        ///@author sz:lihuan
        ///@data 2021/07/09 10:02
        /// </summary>
        public InstitutionReport()
        {
            StandardHeader.MsgType = "8";
        }
        /// <summary>
        /// 全称
        /// </summary>
        public string FullName
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FullName)); }
            set { this.Put(MessageDictionary.Fields.FullName, value); }
        }
        /// <summary>
        /// 客户id
        /// </summary>
        public string InstId
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstId)); }
            set { this.Put(MessageDictionary.Fields.InstId, value); }
        }
        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PinYin)); }
            set { this.Put(MessageDictionary.Fields.PinYin, value); }
        }
        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ShortName)); }
            set { this.Put(MessageDictionary.Fields.ShortName, value); }
        } 
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpdateInsTime)); }
            set { this.Put(MessageDictionary.Fields.UpdateInsTime, value); }
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
        public InstitutionReport Clone()
        {
            InstitutionReport t = new InstitutionReport();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.FullName = FullName;
            t.InstId = InstId;
            t.PinYin = PinYin;
            t.ShortName = ShortName;
            t.updatetime = updatetime;
            t.Signal = Signal;
            return t;
        }
    }
}
