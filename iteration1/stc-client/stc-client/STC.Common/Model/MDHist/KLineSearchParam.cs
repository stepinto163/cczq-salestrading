using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MDHist
{
    public class KLineSearchParam
    {
        public string SecurityID { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string Start { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string End { get; set; }

        public string Type { get; set; }
        /// <summary>
        /// 1D、1M、5M、30M、4H
        /// </summary>
        public string Text { get; set; }

        public string QueryType { get; set; }
        /// <summary>
        /// high、open、low、close、fmtTime
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// 查询笔数（时间和笔数二选一）
        /// </summary>
        public string Count { get; set; }
    }
}
