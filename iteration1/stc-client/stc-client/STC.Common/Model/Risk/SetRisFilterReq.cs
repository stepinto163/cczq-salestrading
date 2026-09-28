using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 新增/修改 特殊审批黑名单 接口请求参数
    /// </summary>
    public class SetRisFilterReq : BaseReplyPo
    {
        /// <summary>
        /// ID,新增时不填，修改时填写
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 券号
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 黑名单类别
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 审批信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 是否启用 1,0
        /// </summary>
        public string enable { get; set; }

        /// <summary>
        /// 服务器创建时间
        /// </summary>
        public string createtime { get; set; }

        /// <summary>
        /// 增加或修改时填写该时间（服务器时间）
        /// </summary>
        public string updatetime { get; set; }
    }
}
