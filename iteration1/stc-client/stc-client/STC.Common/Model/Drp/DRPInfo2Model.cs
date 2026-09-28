using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    public class DRPInfo2Model
    {
        /// <summary>
        /// 1-议价，2-成交，3-拒绝
        /// </summary>
        public string actionType { get; set; }
        public string orderId { get; set; }
        public string algoName { get; set; }
        public string bargainQty { get; set; }
        public string bargainYield { get; set; }

        public string msgCode { get; set; }
    }
}
