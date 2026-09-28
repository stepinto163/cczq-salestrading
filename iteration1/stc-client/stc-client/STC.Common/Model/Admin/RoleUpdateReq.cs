using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 角色增改删请求参数
    /// </summary>
    public class RoleUpdateReq
    {
        public string id;//当action=1时不需要此属性 
        public string rolename;//当action=3时不需要此属性 
        public string action;//"1:新增,2:更新,3:删除", 
        public string terminal;//当action=3时不需要此属性
        public string remark;//当action=3时不需要此属性 
        public string copyRolePermissions;
    }
}
