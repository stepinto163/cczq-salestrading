using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class QueryProcessingRecordHistReq
    {
        /// <summary>
        /// 查询执行时间 yyyy-MM-dd
        /// </summary>
        public string startDate { get; set; }

        /// <summary>
        /// 查询结束时间 yyyy-MM-dd
        /// </summary>
        public string endDate { get; set; }
    }
}
