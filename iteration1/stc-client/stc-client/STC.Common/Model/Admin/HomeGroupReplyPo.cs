using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 关注组查询接口请求结果
    /// </summary>
    public class HomeGroupReplyPo
    {
        public string Code;
        public Dictionary<string, GroupInfo> Msg;
    }

    public class GroupInfo
    {
        public int action;
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
