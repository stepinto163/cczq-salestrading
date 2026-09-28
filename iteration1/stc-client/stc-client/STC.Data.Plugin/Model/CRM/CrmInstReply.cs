
using STC.Broker.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Model.CRM
{
    public class CrmInstReply : BaseReplyPo
    {
        public int totalSize;
        public List<CrmInst> Datas { get; set; }
    }
}
