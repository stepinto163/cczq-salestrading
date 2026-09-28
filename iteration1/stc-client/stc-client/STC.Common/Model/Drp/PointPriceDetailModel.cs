using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    /// <summary>
    /// 点价详情数据结构
    /// </summary>
    public class PointPriceDetailModel
    {
        public DRPValue order;
        public List<PriceValue> Prices = new List<PriceValue>();
        public DRPValue iocOrder;
    }
}
