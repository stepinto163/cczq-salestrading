using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class BrokerSendQQReq
    {
        public String SendMsgType;
        public String BrokerAidbkey;
        public String BrokerAName;
        public String BrokerBidbkey;
        public String BrokerBName;
        public String ContactBidbkey;
        public String ContactBName;
        public TextMsg Text;
        public String ExecID;
        public List<ContentMsg> Content;
    }
}
