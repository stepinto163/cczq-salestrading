using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Fix.Message
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
        public string msg;
        public string reason;

        public long ServerTime;
        public string accountTermIds;
        public string prods;

        //BDB
        public string IDCPermissions;
        public string BondTradePermissions;
        public string IdcTradePermissions;

        public string IdcExcelPermissions;
        public string IdcPythonPermissions;
        public string IdcQTRSPermissions;

        public string roleid;
        public string rolename;
        public string venueusersid;

        public string groupids;
        public string groupnames;

        public string clientAutoExitTime;
        public string passwordExpireMsg;
        public string BrokerSort;
        public string IdcBestfeedPermissions;
        public string IdcClientPermissions;
        public string ERPermissions;
        // 是否又QF权限
        public string QeubeeFinqPermissions;
    }
}
