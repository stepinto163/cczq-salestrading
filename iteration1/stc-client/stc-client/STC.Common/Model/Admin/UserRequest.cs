using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 用户增删改接口请求参数
    /// </summary>
    public class UserRequest
    {
        public string username { get; set; }
        public string roleid { get; set; }
        public string rolename { get; set; }

        //机构
        public string partyCode6 { get; set; }

        //CFESS账号
        public string partysubid { get; set; }
        
        //是否有效 1-有效；0-无效
        public string enable { get; set; }

    }
}
