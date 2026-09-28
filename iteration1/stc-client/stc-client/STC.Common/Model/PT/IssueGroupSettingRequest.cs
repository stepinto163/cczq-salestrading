using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class IssueGroupSettingRequest
    {
        /// <summary>
        /// 操作类型：1：新增；
        ///           2：删除；
        ///           3：修改组信息（包含组名、“默认请求”勾选项）；
        ///           4：修改优先级

        /// </summary>
        public int action { get; set; }

        /// <summary>
        /// 分组信息列表 
        /// </summary>
        public List<IssueGroup> groupInfos { get; set; }

        public string userName { get; set; }
    }
}
