using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{//角色模块动作设置
    public class RolesModulesActionUpdateReq
    {
        public string roleid;
        public List<ModulesInfo> modulesInfo = new List<ModulesInfo>();
    }

    public class ModulesInfo
    {
        public string moduleid;
        public string modulesStatus;//0删除，1增加
        public List<ModulesAction> moduleactionInfo = new List<ModulesAction>();
    }
}
