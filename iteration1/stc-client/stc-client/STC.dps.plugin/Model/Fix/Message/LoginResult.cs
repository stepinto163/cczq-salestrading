using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Model.Fix.Message
{
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
    }
}
