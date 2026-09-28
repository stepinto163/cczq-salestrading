using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Ans
{
    public class QueryANSOutlineReq
    {
        public string beginDate;//开始时间	yyyy-MM-dd
        public string endDate;//结束时间	yyyy-MM-dd 
        public int pageNum; //当前页数
        public int pageSize; //每页条数
    }
}
