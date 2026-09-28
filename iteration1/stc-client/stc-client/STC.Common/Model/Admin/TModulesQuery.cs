using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{//模块查询

    public class TModulesQuery
    {
        public string Code;
        public List<Modules> Msg=new List<Modules>();
    }

    public class Modules
    {
        public string closeby{get;set;}
        public string createtime { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
        public string modulename { get; set; }
        public string parentid { get; set; }
        public string terminal { get; set; }
        public string remark { get; set; }
        public string updatetime { get; set; }
    }
}
