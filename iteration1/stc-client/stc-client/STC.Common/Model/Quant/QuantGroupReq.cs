using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    /// <summary>
    /// 增删改关注组
    /// </summary>
    public class QuantGroupReq
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
    }
}
