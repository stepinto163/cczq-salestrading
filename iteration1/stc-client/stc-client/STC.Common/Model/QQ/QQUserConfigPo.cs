using Ats.Foundation.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.QQ
{
    public class QQUserConfigPo
    {
        public int Version { get; set; }
        public string UserNmae { get; set; }
        public string ExecId { get; set; }
        public string OrdStatus { get; set; }
        /// <summary>
        /// T-交易员/Trader; B-经纪人/Broker;
        /// </summary>
        public string MessageType { get; set; }
        public string UpdateTime { get; set; }
        public string ValidUntilTime { get; set; }
        public string Venues { get; set; }
        public string Text { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 是否是点价意向
        /// 0-需求;1-意向
        /// </summary>
        public string Ispoint { get; set; }
        public string IocTimeOut { get; set; }
    }
}