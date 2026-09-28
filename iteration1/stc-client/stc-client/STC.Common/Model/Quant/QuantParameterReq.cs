using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    public class QuantParameterReq
    {
        // 策略id
        public string quantid;
        //参数id
        public string id;
        // 参数key
        public string param_key;
        // 参数类型
        public string param_type;
        // 参数值
        public string param_value;
        // 说明
        public string param_desc;
        // 1:新增,2:更新,3:删除
        public string action;
        //stc用户
        public string username;
    }
}
