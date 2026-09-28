using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
   public class QueryRelevanceInstReply: BaseReplyPo
    {
        public List<RelevanceInstDetail> datas { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int total { get; set; }
    }

    /// <summary>
    /// 关联机构信息
    /// </summary>
    public class RelevanceInstDetail
    {

        /// <summary>
        /// 记录唯一编号
        /// </summary>
        public string recordId { get; set; }
        /// <summary>
        /// 机构id
        /// </summary>
        public string instId { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string instName { get; set; }
        /// <summary>
        /// 关联机构id
        /// </summary>
        public string associatedInstId { get; set; }
        /// <summary>
        /// 关联机构名称
        /// </summary>
        public string associatedInstName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string createby { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string updateby { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }
    }
}
