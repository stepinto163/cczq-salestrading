using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class UnRefHistoryIntentionOrderReq
    {
        /// <summary>
        /// 用户名 username_key
        /// </summary>
        public string username;

        /// <summary>
        /// 类型：信用债--”CREDIT” 利率债--”RATE”
        /// </summary>
        public string type;
    }

    public enum UnRefTypeEnum
    {
        /// <summary>
        /// 信用债
        /// </summary>
        CREDIT,

        /// <summary>
        /// 利率债
        /// </summary>
        RATE,
    }
}
