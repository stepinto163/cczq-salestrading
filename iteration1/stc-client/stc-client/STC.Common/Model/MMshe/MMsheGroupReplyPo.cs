using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MMshe
{
    /// <summary>
    /// 关注组查询接口请求结果
    /// </summary>
    public class MMsheGroupReplyPo : BaseReplyPo
    {
        public List<MMsheGroupInfo> Datas;
        public string SRL;
    }

    public class MMsheGroupInfo
    {
        public int action;//操作：1-新增 2-修改 3-删除
        public string closeby;//操作人
        public string createtime;//创建时间
        public string groupname;//分组名称
        public string id;//修改、删除时必填
        public string ip;//服务器ip
        public int ordersn;//顺序
        public string terminal;//终端
        public string updatetime;//更新时间
        public string username;//stc用户
        public string remark;//remark
        public string venues;// 交易执行渠道
        public string venuetypegw;//交易平台
        //UNIFY:整组; NORMAL:单券
        public string inf1;
        // 整租参数
        public string inf2;
    }
}
