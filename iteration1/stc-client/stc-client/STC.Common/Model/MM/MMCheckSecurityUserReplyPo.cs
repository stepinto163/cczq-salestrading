using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class MMCheckSecurityUserReplyPo : BaseReplyPo
    {
        public List<CheckSecurityFeedback> Datas;
    }

    public class CheckSecurityFeedback
    {
        public string securityId;
        public string feedback;
    }
}
