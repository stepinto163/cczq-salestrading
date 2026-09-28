using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class UserRoleSettingReply : BaseReplyPo
    {
        public List<UserRoleSettingDetail> Datas;
    }

    public class UserRoleSettingDetail
    {
        public string id;//
        public string roleName;//角色名称
        public string roleType;//角色类型
        public string userName;//用户名
        public string userId;//用户ID
        public string userDisplayName;//用户中文名称
        public List<UserRoleSettingDetail> relatedUsers;//关联用户
        public string remark;//备注
    }

    public enum UserRoleEnum
    {
        [Description("管理员")]
        ADMIN = 0,
        [Description("桥专员")]
        BRIDGE_ATTACHE = 1,
        [Description("开闭市设置")]
        OPEN_CLOSE_MARKET = 2,
        [Description("查看他人过桥成交")]
        VIEW_OTHER_BRIDGE_TRADE = 3,
        [Description("查看指定人成交")]
        VIEW_APPOINTED_TRADE = 4,
        [Description("助理")]
        ASSISTANT = 5,
        [Description("不可查看他人历史成交")]
        NOT_VIEW_OTHER_HIST_TRADE = 6,
    }
}
