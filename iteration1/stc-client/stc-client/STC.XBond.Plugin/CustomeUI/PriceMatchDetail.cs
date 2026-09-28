using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// 价格匹配详情数据结构
    /// </summary>
    public class PriceMatchDetail
    {
        public Order order;
        public List<PriceValue> Prices = new List<PriceValue>();
        public Order iocOrder;
    }
}
