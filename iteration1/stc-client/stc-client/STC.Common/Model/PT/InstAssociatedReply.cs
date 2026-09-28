using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class InstAssociatedReply : BaseReplyPo
    {
        public List<InstAssociatedInfo> Datas;
    }

    public class InstAssociatedInfo
    {
        public string id;//主键ID
        public string instId;//机构ID
        public string isntName;//机构名称
        public string associatedInstId;//关联机构ID
        public string associatedInstName;//关联机构名称
        public string remark;//备注
        public string createby;//创建人
        public string createtime;//创建时间
        public string updateby;//更新人
        public string updatetime;//更新时间
    }
}
