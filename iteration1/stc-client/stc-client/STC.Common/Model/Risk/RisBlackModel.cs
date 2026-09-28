using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 下载查询所有黑名单 返回结构
    /// </summary>
    public class RisBlackModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 黑名单类别
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 服务器创建时间
        /// </summary>
        public string createtime { get; set; }

        /// <summary>
        /// 是否启用 1,0
        /// </summary>
        public string enable { get; set; }

        /// <summary>
        /// 审批信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 券号
        /// </summary>
        public string securityid { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public string username { get; set; }

    }
}
