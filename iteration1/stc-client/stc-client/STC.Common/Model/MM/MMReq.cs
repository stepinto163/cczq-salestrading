using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class MMReq
    {
        public string Action;//操作：1-新增 2-修改 3-删除 4-批量导入
        public List<MM> mmList;
    }
}
