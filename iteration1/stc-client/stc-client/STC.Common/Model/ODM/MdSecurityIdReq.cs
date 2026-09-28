using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.ODM
{
    /// <summary>
    /// 查询ODM债券订阅列表接口请求参数
    /// </summary>
    public class MdSecurityIdReq
    {
        public string securityID;
        public string symbol;
        public string venueTypeGW;
        public string marketIndicator;
        public string subscriptionRequestType;
        public string settlType;
    }
}
