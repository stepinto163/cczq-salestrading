using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class InstAssociatedRequest
    {
        /**
         * 操作类型 0:查询，1:新增，3:删除
         */
        public string action;
        /**
         * 机构ID
         */
        public string instId;
        /**
         * 机构名称
         */
        public string instName;
        /**
         * 关联机构ID
         */
        public string associatedInstId;
        /**
         * 关联机构名称
         */
        public string associatedInstName;
        /**
         * 备注
         */
        public string remark;
        /**
         * 创建人
         */
        public string createby;

        public List<string> deleteList;
    }
}
