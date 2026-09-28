using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Chart
{
    public class ChartModel
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Volumn { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
    }
}
