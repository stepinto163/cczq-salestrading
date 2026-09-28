using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TSafeMangerReq
    {
        public string powerPwd;
        public string updatePwdTime;
        public string stopPwdTimes;
        public string action;
    }

    public class TSafeManger : PResult
    {
        public TSystemSafeParam Datas;
    }

    public class TSystemSafeParam
    {
        public string clientAutoExitTime;
        public List<TTbookSystemParameter> tbookSystemParameterList;
        public string overPwdValidTime;
    }

    public class TSafeMangerPo
    {
        public string Code;
        public string Msg;
        public string powerPwd;
        public string updatePwdTime;
        public string stopPwdTimes;
        public string overPwdValidTime;
    }

    public class TTbookSystemParameter
    {
        public string param_key = "";//参数key		

        public string username = "";//用户名		

        public string param_value;//参数值		

        public string param_desc;//描述		

        public string createtime;

        public string updatetime;
    }
}
