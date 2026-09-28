using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class PtExecSendReplyPo : BaseReplyPo
    {
        public List<string> sendOrder;
        public List<string> unSendOrder;

        public Dictionary<string,List<string>> Datas;
    }
}
