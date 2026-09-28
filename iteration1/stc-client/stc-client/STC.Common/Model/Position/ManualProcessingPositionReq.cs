using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class ManualProcessingPositionReq
    {
        /// <summary>
        /// 跑批起始日期 yyyy-MM-dd
        /// </summary>
        public string startDate { get; set; }

        /// <summary>
        /// 不必传
        /// </summary>
        public string actionUser { get; set; }

        /// <summary>
        /// 不必传
        /// </summary>
        public string actionUserName { get; set; }
    }
}
