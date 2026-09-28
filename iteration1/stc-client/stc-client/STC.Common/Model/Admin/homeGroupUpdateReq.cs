using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 关注组操作接口请求参数
    /// </summary>
    public class HomeGroupUpdateReq
    {
        // 组编号
        public string id;
        //组名
        public string groupname;
        // 1:新增,2:更新,3:删除
        public string action;
        // 顺序
        public string ordersn;
        // 备注
        public string remark;
        // 终端
        public string terminal;

        public string type;
        public string username;
        public string inf2;
        public string closeby;
    }
}
