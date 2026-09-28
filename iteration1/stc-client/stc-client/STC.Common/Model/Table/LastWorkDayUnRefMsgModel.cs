using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public  class LastWorkDayUnRefMsgModel
    {
        public string id { get; set; }

        public string type { get; set; }

        public string readflag { get; set; }

        public string content { get; set; }

        public string username { get; set; }

        public string createtime { get; set; }

        public string updatetime { get; set; }

        public string delflag { get; set; }

        public string detail { get; set; }
    }


    public class UnRefContentModel
    {
        public string content { get; set; }

        public string contentType { get; set; }

        public bool needSend { get; set; }

        public bool show { get; set; }

        public string title { get; set; }

        public string remark { get; set; }

        /// <summary>
        /// 类型：信用债--”CREDIT” 利率债--”RATE”
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 量
        /// </summary>
        public string orderTotal { get; set; }
    }

    public class UnRefMsgTypeModel
    {
        public string type { get; set; }

        public string orderTotal { get; set; }
    }
}
