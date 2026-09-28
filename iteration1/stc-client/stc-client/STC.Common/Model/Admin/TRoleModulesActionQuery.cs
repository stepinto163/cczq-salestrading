using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{//模块动作查询
    public class TRoleModulesActionQuery
    { 
        public string Code;
        public List<Module> Datas = new List<Module>();   
    }

    public class Module
    {
        public string id;
        public string closeby;
        public string createtime;
        public string modulename;
        public string ip;
        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;
        public List<ModulesAction> moduleaction;
    }
}
