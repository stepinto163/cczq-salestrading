using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 查询当前用户的设置的特殊审批黑名单 接口请求参数
    /// </summary>
    public class QueryRisFilterReq
    {
        public string username { get; set; }
    }
}
