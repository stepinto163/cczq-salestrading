using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MMshe
{
    public class MMshe
    {
        public string groupid;//债卷分组id
        public string groupname;//债卷分组
        public string securityid;//产品编号
        public string symbol;//产品名称
        public string settltype;//清算速度
        public string enable;//开关标识  1开关，0关闭
        public string remark;// 备注
        public string terminal;// 终端
        public string ip;//服务器ip
        public MMsheOrderParamReq Params;//参数
        public string Action;//操作：1-新增 2-修改 3-删除
        public string closeby;//操作人
        public string createtime;//创建时间
        public string inf1;//扩展字段1
        public string inf2;//扩展字段2
        public string inf3;//扩展字段3
        public string inf4;//扩展字段4
        public string inf5;//扩展字段5
        public string key;//唯一编码
        public string updatetime;//更新时间
        public string username;// stc用户
        public string venues;// 交易平台
        public string venuetypegw;//交易执行渠道
        public bool manual;

        public string getKey()
        {
            return string.Format("{0}-{1}",securityid,venuetypegw);
        }
    }

    public class Inf2
    {
        public bool bid;
        public bool ofr;
    }
}
