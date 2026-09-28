using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 发单设置， 分组查询返回参数
    /// </summary>
    public class IssueGroup
    {
        /// <summary>
        /// 分组ID   operateType为2、3、4必填
        /// </summary>
        public string groupId { get; set; }

        /// <summary>
        /// 分组名称 operateType为1、3必填
        /// </summary>
        public string groupName { get; set; }

        /// <summary>
        /// 新的优先级 operateType为1、4必填
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// 原优先级 operateType为2、4必填
        /// </summary>
        public int oldPriority { get; set; }

        /// <summary>
        /// “默认请求”勾选项：1-是；2-否; operateType为3必填
        /// </summary>
        public string defaultRequest { get; set; }

        /// <summary>
        /// 机构列表
        /// </summary>
        public List<IssueInst> instInfos;

        public string remark { get; set; }//备注
        public string createby { get; set; }//创建人
        public string createtime { get; set; }//创建时间
        public string updateby { get; set; }//更新人
        public string updatetime { get; set; }//更新时间
    }
}
