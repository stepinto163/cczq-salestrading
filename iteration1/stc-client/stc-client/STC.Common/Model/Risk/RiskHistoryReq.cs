using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 查询风控检查记录 接口请求
    /// </summary>
    public class RiskHistoryReq
    {
        public string type;
        public string operationResult;
        public string venues;
        public string firmflag;
        public string securityid;
        public string userName;
        public string orderid;
        public string dealTimeBegin;
        public string dealTimeEnd;
        public string pageNum;
        public string pageSize;
        public string id = "1";//用做后台识别，1-交易端；2-管理端
        public string rule;
    }
}
