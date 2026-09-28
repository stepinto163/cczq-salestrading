using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 债券组查询接口请求结果
    /// </summary>
    public class HomeGroupSecurityidsReplyPo
    {
        public string Code;
        public Dictionary<string, HomeGroupInfo> Datas;
    }

    public class HomeGroupInfo
    {
        public int action { get; set; }
        public string closeby{ get; set; }
        public string createtime{ get; set; }
        public string groupname{ get; set; }
        public string id{ get; set; }
        public string ip{ get; set; }
        public int ordersn{ get; set; }
        public string terminal{ get; set; }
        public string updatetime{ get; set; }
        public string username{ get; set; }
        public string inf1 { get; set; }
        public string inf2 { get; set; }
        public string inf3 { get; set; }
        public string inf4 { get; set; }
        public string inf5 { get; set; }
        public List<HomeGroupSecurityIDsInfo> securityids{ get; set; }
        public Dictionary<string, string> tagDic;
        public GroupMsgPo groupMsgPo;

    }

    public class HomeGroupSecurityIDsInfo
    {
        public int action{ get; set; }
        public string closeby{ get; set; }
        public string createtime{ get; set; }
        public string groupId{ get; set; }
        public string id{ get; set; }
        public string ip{ get; set; }
        public string remark{ get; set; }
        public string securityid{ get; set; }
        public string symbol{ get; set; }
        public string terminal{ get; set; }
        public string updatetime{ get; set; }
        public string username{ get; set; }
        public string settletype{ get; set; }
        public int sort{ get; set; }
        public string inf1 { get; set; }
        public string inf2 { get; set; }
        public string inf3 { get; set; }
        public string inf4 { get; set; }
        public string inf5 { get; set; }
    }
}
