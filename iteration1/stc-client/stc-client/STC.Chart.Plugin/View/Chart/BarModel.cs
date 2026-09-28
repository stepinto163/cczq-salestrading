using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Chart.Plugin.Chart
{
   public class BarModel
    {
        /// <summary>
        /// 数据
        /// </summary>
        public double Data { get; set; }
        /// <summary>
        /// 颜色 true为红色 false为绿色
        /// </summary>
        public bool Type { get; set; }
    }
}
