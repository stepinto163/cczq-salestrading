using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    /// <summary>
    /// 增删改关注组
    /// </summary>
    public class RfqGroupReq
    {
        // 组编号
        public string id;
        //组名
        public string groupname;

        // 0:查询,1:插入,2:更新,3:删除,4:导入,5:启动,6:停止
        public string action;
        // 顺序
        public string ordersn;
        // 备注
        public string remark;
        // 终端
        public string terminal;
    }
}
