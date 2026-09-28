using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class QueryRolesReplyPo : BaseReplyPo
    {
        public List<RoleInfo> Datas;
    }

    public class RoleInfo
    {
        public string id;
        public string rolename;
    }
}
