using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class QueryIssueGroupRequest
    {
        /// <summary>
        /// 操作类型：1：新增；
        ///           2：删除；
        ///           3：修改组信息（包含组名、“默认请求”勾选项）；
        ///           4：修改优先级

        /// </summary>
        public int action { get; set; }

        /// <summary>
        /// 组id， 未生效
        /// </summary>
        public string groupId;

        /// <summary>
        /// 机构id， 未生效
        /// </summary>
        public string instId;
    }
}
