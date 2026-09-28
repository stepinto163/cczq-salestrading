using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{//角色查询
    public class TRolesQuery
    {
        public string Code { get; set; }
        public List<Roles> Datas = new List<Roles>();
        // public List<Roles> moduleaction = new List<Roles>();
    }
    public class Roles
    {
        public string createtime { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
        public string remark { get; set; }
        public string rolename { get; set; }
        public string terminal { get; set; }
    }
}
