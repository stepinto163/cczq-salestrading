using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.CrossBridge
{
    /// <summary>
    /// 查询指定桥下的未查看记录
    /// </summary>
    public class BridgeReviewsRequestPo
    {
        public String inst_id;//桥机构ID
        public String inst_trade_id;//桥机构交易员ID
    }
}
