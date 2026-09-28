using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.BookManagement
{
    public class BlockNumQueryReq
    {
        public String blocknum;// 包编号
        public int pageNum;// 从0开始
        public int pageSize;// = 5000
        public String flag;
    }
}
