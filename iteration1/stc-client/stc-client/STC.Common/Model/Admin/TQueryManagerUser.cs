using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TQueryManagerUser : PResult
    {
        public List<ManagerUser> Datas;
    }

    public class ManagerUser
    {
        public string cfp_userid;
        public string cfp_username;
        public string closeby;
        public string cp_userid;
        public string cp_username;
        public string createtime;
        public string ib_userid;
        public string ib_username;
        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;
        public string ir_userid;
        public string ir_username;
        public string managertype;
        public string mangerid;
        public string mangername;
        public string parentnode;
        public string remark;
        public string status;
        public string updatetime;
    }
}
