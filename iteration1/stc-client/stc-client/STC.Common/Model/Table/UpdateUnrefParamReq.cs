using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class UpdateUnrefParamReq
    {
        /// <summary>
        /// Key 传值”STC_InnerBoard_Order_UnRef”
        /// </summary>
        public string param_key;

        /// <summary>
        /// 用户名
        /// </summary>
        public string username;

        /// <summary>
        /// 参数值 格式：{"CREDIT":[{"Name":"8:00提醒恢复","IsChecked":true}],"RATE":[{"Name":"8:00提醒恢复","IsChecked":true}]}
        /// </summary>
        public string param_value;


        /// <summary>
        /// 描述:”日初订单恢复提醒”
        /// </summary>
        public string param_desc;
    }
}
