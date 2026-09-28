using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class HomeGroupUpdateReplyPo : PResult
    {
        public HomeGroupUpdateInfo Datas;
    }

    public class HomeGroupUpdateInfo
    {
        public string closeby;
        public string createtime;
        public string groupname;
        public string id;
        public string ip;
        public int ordersn;
        public string terminal;
        public string updatetime;
        public string username;
    }
}
