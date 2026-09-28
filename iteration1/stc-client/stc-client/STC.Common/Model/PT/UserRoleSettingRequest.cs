using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class UserRoleSettingRequest
    {
        public string id;//
        public string roleName;//角色名称
        public string roleType;//角色类型
        public string userName;//用户名
        public string userId;//用户ID
        public string userDisplayName;//用户中文名称
        public List<UserRoleSettingDetail> relatedUsers;//关联用户
        public string remark;//备注

        /**
         * 操作类型 0:查询 1:新增 2:修改 3:删除
         */
        public string action;
    }
}
