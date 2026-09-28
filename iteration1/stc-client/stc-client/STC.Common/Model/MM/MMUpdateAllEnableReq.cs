using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class MMUpdateAllEnableReq
    {
        public string enable; //开关标记  1开关，0关闭
        public string groupid;//组id， 不传表示所有
        public string macID;
        public string hardDiskID;
        public string insideIP;
    }
}
