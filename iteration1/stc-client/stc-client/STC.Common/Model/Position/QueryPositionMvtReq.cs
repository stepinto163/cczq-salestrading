using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    /// 历史持仓查询
    /// </summary>
    public class QueryPositionMvtReq
    {
        /// <summary>
        /// 用,隔开
        /// </summary>
        public string bookid { get; set; }

        public string securityid { get; set; }

        public string startTime { get; set; }

        public string endTime { get; set; }

        public int pageNum { get; set; }

        public int pageSize { get; set; }
    }
}
