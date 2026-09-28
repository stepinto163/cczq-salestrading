using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    /// <summary>
    /// 策略列表接口 分组策略查询
    /// </summary>
    public class RfqStgActModelReq
    {
        /// <summary>
        /// 策略ID
        /// </summary>
        public string strategyId;

        /// <summary>
        /// 分组ID
        /// </summary>
        public string groupid;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid;

        /// <summary>
        /// 债券名称
        /// </summary>
        public string symbol;

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settltype;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string username;

        /// <summary>
        /// 类型
        /// </summary>
        public string type;

        /// <summary>
        /// 开关标识
        /// </summary>
        public string enable;

        /// <summary>
        /// 创建时间开始
        /// </summary>
        public string createtimeStart;

        /// <summary>
        /// 创建时间结束
        /// </summary>
        public string createtimeEnd;
    }
}
