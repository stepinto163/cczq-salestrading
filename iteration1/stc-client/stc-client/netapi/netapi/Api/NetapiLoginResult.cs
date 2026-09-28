using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.netapi.Api
{
    public class NetapiLoginResult
    {
        public string sid;
        public string usid;
        public string Code;
        public string Msg;
        public string ConnectPermissions;
        public string BonDPermissions;
        public string BondTradePermissions;
        public string IDCPermissions;
        public string ERPermissions;
        public string IdcClientPermissions;
        public string IdcTradePermissions;
        public string IdcBestfeedPermissions;
        public string BrokerSort;
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
        public string passwordExpireMsg;
        public string clientAutoExitTime;
        public string biztypeid;
        /// <summary>
        /// 当前客户端台子
        /// </summary>
        public string SubCompanyId { get; set; }

        // 是否又QF权限
        public string QeubeeFinqPermissions;

        // 内部看板权限
        public string BdbInnerPermissions;

        // 中债估值显示权限
        public string CDCPermissions;

        // 市场最优页面权限
        public string BdbBboPermissions;

        // 策略定价页面权限
        public string BdbStaratPermissions;

        // qq号
        public string inf3;
    }
}
