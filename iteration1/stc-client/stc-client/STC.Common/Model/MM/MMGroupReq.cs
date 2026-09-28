using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    /// <summary>
    /// 增删改关注组
    /// </summary>
    public class MMGroupReq
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
        //UNIFY:整组; NORMAL:单券
        public string inf1;
        // 整租参数
        public string inf2;
    }
}
