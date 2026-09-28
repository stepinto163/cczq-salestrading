using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 当前用户所属角色信息
    /// </summary>
    public class TUsersQuery
    {
        public string Code;
        public User Datas =new User();
    }

    public class User
    {
        public string createtime;
        public string enable;
        public string name;
        public string password;
        public string updatetime;
        public string userid;
        public string username;
        public string roleid;
        public string rolename;
        public string inf3;

        public List<TbookVenueUsers> tbookVenueUsers;
    }
}
