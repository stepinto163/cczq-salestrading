using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryEmsOrderReq
    {
        public string dealTimeBegin;

        public string userName;

        public int pageSize;//一页多少条
        public int pageNum = 0;//第几页，从0开始

        public Dictionary<string, string> reqParams;
    }
}
