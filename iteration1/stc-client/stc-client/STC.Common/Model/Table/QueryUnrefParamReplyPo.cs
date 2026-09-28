using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryUnrefParamReplyPo : BaseReplyPo
    {
        public int totalSize { get; set; }
        public TTbookSystemParameter Datas;
    }
}
