using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    /// 历史成交查询请求，含BookId
    /// </summary>
    public class QueryExternalReq
    {
        public string securityid { get; set; }

        public int pageNum { get; set; }

        public int pageSize { get; set; }

        public string dealTimeBegin { get; set; }

        public string dealTimeEnd { get; set; }

        public string bookid { get; set; }
    }
}
