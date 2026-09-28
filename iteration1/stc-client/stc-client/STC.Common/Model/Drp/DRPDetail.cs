using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    /// <summary>
    /// 价格匹配详情数据结构
    /// </summary>
    public class DRPDetail
    {
        public DRPValue order;
        public List<DRPValue> iocOrderDqList = new List<DRPValue>();
        public DRPValue iocOrder;
    }
}
