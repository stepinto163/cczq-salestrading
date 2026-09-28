using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 债券组操作接口请求参数
    /// </summary>
    public class HomeGroupSecurityidsUpdateReq
    {
        public string id;
        public string groupid;
        public string securityid;
        public string symbol;
        public string action;
        public string remark;
        public string terminal;
        public string type;
        public string username;
        public string ordersn;
        public string remainingTime;
    }
}
