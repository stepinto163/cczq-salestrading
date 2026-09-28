using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    /// <summary>
    /// 明细交易员。发送明细消息的依据
    /// </summary>
    public class DetailTrader
    {
        public string Key { get; set; }
        public string InstName { get; set; }
        public string TraerName { get; set; }
        public string InstID { get; set; }
        public string TraderID { get; set; }
        public bool IsOfr { get; set; }

        /// <summary>
        /// 是否是其他人成交单
        /// </summary>
        public bool IsOtherOrder { get; set; }
    }
}

