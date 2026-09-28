using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    public class BridgeConfigInfoResponse: BaseReplyPo
    {
        public List<BridgeConfigInfo> datas;

        /// <summary>
        /// 总条数
        /// </summary>
        public int total { get; set; }
    }
}
