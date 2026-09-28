using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class MMUpdateEnableReq
    {
        public string securityid;//债券代码
        public string settltype;//清算速度
        public string enable;//启用标记1:启用,0关闭
        public string macID;
        public string hardDiskID;
        public string insideIP;
    }
}
