using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.FinQ
{
    public class SendFQGroupMessageReq
    {
        public string bid;//业务id,见接口说明
        public string sender;//发送方QQ号
        public string receiver;//群内被@的人
        public string message;//发送的文本，限制3000个字符
        public string groupOpenID;//群openID 与群号二选一提供
        public string groupcode;//群号 与群openID二选一提供
        public string Username;
    }
}
