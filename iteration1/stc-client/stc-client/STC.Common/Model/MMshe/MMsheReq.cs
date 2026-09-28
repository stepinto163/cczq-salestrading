using System.Collections.Generic;

namespace STC.Common.Model.MMshe
{
    public class MMsheReq
    {
        public string Action;//操作：1-新增 2-修改 3-删除 4-批量导入
        public List<MMshe> mmList;
    }
}
