using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TBatchMsgReq
    {
        public string delflag;
        public string readflag;
        public List<string> id;
        public string userName = LocalEnv.UserInfo.UserName;
    }
}
