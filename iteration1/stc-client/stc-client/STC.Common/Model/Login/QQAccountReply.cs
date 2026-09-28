using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Login
{
    public class QQAccountReply : PResult
    {
        public List<QQItem> Datas;
    }

    public class QQItem
    {
        public string astChildTp;
        public string astParentTp;
        public int brokerContactId;
        public string orgId;
        public object qq;
        public string userId;
        public string userNm;
    }
}
