using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Login
{
    /// <summary>
    /// 登录信息
    /// </summary>
   public class LoginResult
    {
        public string sid;
        public string Code;
        public string Msg;
        public string ConnectPermissions;
        public string BonDPermissions;
        public string companyId;
        public string companyName;
        public string userId;
        public string userName;
        public string Name;

        public string method;
        public string type;

        public long ServerTime;
        public string accountTermIds;
        public string prods;

        public string roleid;
        public string rolename;
        public string venueusersid;

        public string groupids;
        public string groupnames;
    }
}
